/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/GlyphHinter.java
 * PDFBOX_SOURCE_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc
 */

/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System.Numerics;
using System.Text;
using System.Globalization;
using PdfBox.Net.Util.Geometry;
using Microsoft.Extensions.Logging;
using PdfBox.Net.Logging;

namespace PdfBox.Net.FontBox.TTF;



/// <summary>
/// Applies TrueType bytecode hinting (grid-fitting) to a font's glyphs, producing grid-fitted paths.
/// &lt;p&gt;
/// One hinter is created per TrueTypeFont. It lazily builds a TrueTypeInterpreter from
/// the font's maxp/head/cvt/fpgm/prep tables, runs the font
/// program once, and re-runs the control value program whenever the ppem changes. For each glyph it
/// scales the outline into the pixel grid (appending the phantom points), runs the glyph's instructions
/// and scales the grid-fitted result back into font units, so the rest of the rendering pipeline - which
/// scales font units to device pixels at exactly this ppem - reproduces the grid-fitting.
/// &lt;p&gt;
/// Hinting is best-effort: anything malformed, unsupported, or not applicable (an empty glyph, a simple
/// glyph with no instructions, a ppem below head.lowestRecPPEM or excluded by the gasp
/// table, or a prep that switches hinting off with INSTCTRL) falls back to null, and the
/// caller renders the raw outline. Composite glyphs are hinted: each component with its own
/// instructions, then the composite's. One bad glyph never disables hinting for the rest of the font.
/// Whether to hint at all is the caller's decision (PDFRenderer.setHintingEnabled(boolean), off
/// by default); this class always grid-fits when asked.
/// &lt;p&gt;
/// The interpreter carries a great deal of mutable state - the storage area, the twilight zone, the
/// post-prep template, the active ppem - so every entry point here is synchronized and
/// one font hints one glyph at a time. That is correct but it does serialize: a font substituted from
/// the system is held in a process-wide cache, so several rendering threads can share one instance and
/// queue on this monitor. Embedded fonts are per-document and unaffected. If it ever measures as a
/// bottleneck the answer is a per-thread or pooled interpreter, not a weaker lock; until then the simple
/// thing is the right thing. HintingConcurrencyTest pins the current behavior.
/// 
/// @author Apache PDFBox
/// </summary>
internal class GlyphHinter
{
    private static ILogger<GlyphHinter> LOG => PdfBoxLogging.CreateLogger<GlyphHinter>();

    private TrueTypeFont font;

    private bool initialized;
    private bool available;
    private bool warned;
    private int failureCount;
    private string? firstFailure;
    private TrueTypeInterpreter interpreter = null!; // Set by Initialize before available becomes true.
    private GaspTable? gasp;
    private int lowestRecPpem;
    private int unitsPerEm;
    private int currentPpem = -1;
    // null until probed: whether the font's bytecode builds glyph geometry (see needsFullControl)
    private bool? fullControl;
    // while probing, overrides the backward-compatibility decision for glyph programs
    private bool? forcedBackwardCompatibility;

    /// <summary>
/// The ppem the full-control probe hints its sample glyphs at.
/// </summary>
    private const int PROBE_PPEM = 32;
    /// <summary>
/// At most this many glyphs with instructions are sampled by the probe.
/// </summary>
    private const int PROBE_SAMPLE = 32;
    /// <summary>
/// Glyphs examined while looking for the sample, to bound the cost on huge CJK fonts.
/// </summary>
    private const int PROBE_CANDIDATES = 512;
    /// <summary>
/// A sampled glyph "moves" if some point is displaced by more than this (5px at PROBE_PPEM).
/// </summary>
    private const int PROBE_TOLERANCE = 5 * 64;
    /// <summary>
/// The number of moving glyphs that marks a font as needing full control.
/// </summary>
    private const int PROBE_MIN_MOVING = 2;
    /// <summary>
/// Upper bound on the size a full-control font is assembled at (see assemblyPpem).
/// </summary>
    private const int MAX_ASSEMBLY_PPEM = 64;

    internal GlyphHinter(TrueTypeFont font)
    {
        this.font = font;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.Synchronized)]
    private void Initialize() 
    {
        if (initialized)
        {
            return;
        }
        initialized = true;
        available = false;

        MaximumProfileTable maxp = font.GetMaximumProfile();
        FontProgramTable fpgm = font.GetFontProgram();
        ControlValueProgramTable prep = font.GetControlValueProgram();
        ControlValueTable cvt = font.GetControlValues();

        // hinting is only meaningful if the font carries a bytecode program
        if (maxp == null || (fpgm == null && prep == null))
        {
            return;
        }

        unitsPerEm = font.GetUnitsPerEm();
        gasp = font.GetGasp();
        lowestRecPpem = font.GetHeader().GetLowestRecPPEM();

        interpreter = new TrueTypeInterpreter(maxp.GetMaxStackElements(), maxp.GetMaxStorage(),
                maxp.GetMaxTwilightPoints(), unitsPerEm);
        interpreter.SetFontProgram(fpgm != null ? fpgm.GetProgram() : null);
        interpreter.SetControlValueProgram(prep != null ? prep.GetProgram() : null);
        interpreter.SetControlValues(cvt != null ? cvt.GetValues() : null);
        interpreter.PrepareFontProgram();
        available = true;
    }

    /// <summary>
/// Returns the grid-fitted path of the glyph at the given ppem, or null if hinting does not
/// apply and the caller should render the raw outline.
/// &lt;p&gt;
/// One deliberate departure from FreeType: a font whose bytecode builds its glyphs (see
/// #needsFullControl()) is, below its head.lowestRecPPEM, hinted at that size and
/// the result scaled down (see #assemblyPpem). Such fonts - MingLiU and its relatives all
/// declare 25 - need the bytecode to put their glyphs together, but at small sizes the same
/// program also forces every stroke to a whole pixel in both directions. It was written for
/// black-and-white rendering, and Windows showed embedded bitmaps there instead; antialiased, those
/// whole-pixel strokes make text roughly 1.7 times as heavy as the design. Assembling at the
/// smallest size the vendor recommends for the outlines keeps the glyphs correct at their designed
/// weight. #getHintedPointsF26Dot6 is unaffected and still hints at the requested size.
/// 
/// @param gid the glyph id
/// @param requestedPpem the pixels-per-em the glyph is drawn at
/// @return the hinted path in font units, or null
/// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.Synchronized)]
    internal GeneralPath? GetPath(int gid, int requestedPpem)
    {
        int ppem = requestedPpem;
        try
        {
            Initialize();
            if (available)
            {
                ppem = AssemblyPpem(requestedPpem, NeedsFullControl(), lowestRecPpem);
            }
        }
        catch (IOException e)
        {
            // hint() below reports the failure and falls back
        }
        Hinted? hinted = Hint(gid, ppem);
        if (hinted == null)
        {
            return null;
        }
        // scale the grid-fitted coordinates back into font units (drop the phantom points)
        int[] hintedX = new int[hinted.pointCount];
        int[] hintedY = new int[hinted.pointCount];
        int[] curX = hinted.zone.GetCurrentX();
        int[] curY = hinted.zone.GetCurrentY();
        for (int i = 0; i < hinted.pointCount; i++)
        {
            hintedX[i] = ToFontUnits(curX[i], ppem);
            hintedY[i] = ToFontUnits(curY[i], ppem);
        }
        return new GlyphRenderer(hinted.gd, hintedX, hintedY).GetPath();
    }

    /// <summary>
/// Returns the grid-fitted glyph points in F26Dot6 device coordinates (the raw interpreter output,
/// before scaling back to font units, and excluding the phantom points), or null if hinting
/// does not apply. This is the form compared against a FreeType reference dump by the golden tests.
/// 
/// @param gid the glyph id
/// @param ppem the pixels-per-em to grid-fit to
/// @return a {x[], y[]} pair in F26Dot6, or null
/// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.Synchronized)]
    internal int[][]? GetHintedPointsF26Dot6(int gid, int ppem)
    {
        Hinted? hinted = Hint(gid, ppem);
        if (hinted == null)
        {
            return null;
        }
        int[] x = new int[hinted.pointCount];
        int[] y = new int[hinted.pointCount];
        Array.Copy(hinted.zone.GetCurrentX(), 0, x, 0, hinted.pointCount);
        Array.Copy(hinted.zone.GetCurrentY(), 0, y, 0, hinted.pointCount);
        return new int[][] { x, y };
    }

    /// <summary>
/// Runs the control value program untraced, then grid-fits the glyph with an execution tracer
/// attached, so the per-instruction trace can be diffed against FreeType's ttinterp trace.
/// For development/debugging only.
/// 
/// @param gid the glyph id
/// @param ppem the pixels-per-em
/// @param output where to write the trace
/// @param tracePoint a glyph point index to log per instruction, or -1
/// @throws IOException if the font could not be read
/// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.Synchronized)]
    internal void TraceGlyph(int gid, int ppem, TextWriter output, int tracePoint)
            
    {
        Initialize();
        if (!available)
        {
            return;
        }
        SetActivePpem(ppem);
        interpreter.SetTracer(new ExecutionTracer(output, tracePoint));
        try
        {
            Hint(gid, ppem, 0);
        }
        finally
        {
            interpreter.SetTracer(null);
        }
    }

    /// <summary>
/// Maximum composite nesting depth, to bound recursion on pathological fonts.
/// </summary>
    private const int MAX_COMPONENT_DEPTH = 8;

    /// <summary>
/// Runs all gating, then grid-fits the glyph, returning the executed zone or null on fallback.
/// </summary>
    private Hinted? Hint(int gid, int ppem)
    {
        if (ppem <= 0)
        {
            return null;
        }
        try
        {
            Initialize();
            if (!available)
            {
                return null;
            }
            // fonts whose bytecode builds the glyphs are hinted at every size: FreeType likewise
            // exempts its "tricky" fonts from both size gates
            bool full = NeedsFullControl();
            // head.lowestRecPPEM is the vendor's "smallest readable size in pixels" for the
            // outlines. Fonts that ship bitmap strikes for small sizes (MS Mincho/Gothic say 25) set
            // it above the sizes the bitmaps cover; their instructions were never meant to run there,
            // and doing so under grayscale forces every thin stroke to a full pixel. Text fonts
            // without strikes sit at 6-9, so this never fires for them.
            if (!full && ppem < lowestRecPpem)
            {
                return null;
            }
            // gasp gate: if a gasp table is present and does not request grid-fitting here, skip.
            // (GRIDFIT without DOGRAY is deliberately NOT treated as "do not hint": Arial and
            // Liberation flag 9-17ppem that way, so it would switch hinting off for body text.)
            if (!full && gasp != null && !gasp.IsGridFit(ppem))
            {
                return null;
            }
            SetActivePpem(ppem);
            // INSTCTRL bit 1: prep switched hinting off at this size (fonts do this above a ppem
            // threshold); render the raw outline, as FreeType does in tt_loader_init
            if ((interpreter.GetSavedState().GetInstructControl() & 1) != 0)
            {
                return null;
            }
            return Hint(gid, ppem, 0);
        }
        catch (Exception e) when (e is IOException or HintingException)
        {
            LogFailure(gid, ppem, e);
            return null;
        }
    }

    /// <summary>
/// Reports a glyph that could not be hinted. Only the first failure in a font is a warning carrying
/// the stack trace; the rest go to debug. Hinting is attempted per (glyph, ppem) pair, so a
/// font whose bytecode never runs - a malformed program, or one using something unimplemented - would
/// otherwise emit thousands of identical stack traces for a page of CJK text.
/// </summary>
    private void LogFailure(int gid, int ppem, Exception e)
    {
        failureCount++;
        if (firstFailure == null)
        {
            firstFailure = "glyph " + gid + " at " + ppem + "ppem: " + e;
        }
        if (warned)
        {
            LOG.LogDebug(e, "hinting failed for glyph {GlyphId} at {Ppem}ppem, using raw outline", gid, ppem);
            return;
        }
        warned = true;
        LOG.LogWarning(e, "hinting failed for glyph {GlyphId} at {Ppem}ppem in font {FontName}, using raw outline; further failures in this font are logged at debug level", gid, ppem, FontName());
    }

    /// <summary>
/// @return how many (glyph, ppem) hinting attempts failed and fell back to the raw outline, as
/// opposed to not applying (no instructions, gasp, lowestRecPPEM); for tests
/// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.Synchronized)]
    internal int GetFailureCount()
    {
        return failureCount;
    }

    /// <summary>
/// @return the first failure, "glyph &amp;lt;gid&amp;gt; at &amp;lt;ppem&amp;gt;ppem: &amp;lt;exception&amp;gt;", or null
/// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.Synchronized)]
    internal string? GetFirstFailure()
    {
        return firstFailure;
    }

    /// <summary>
/// The font's PostScript name for the warning above, best-effort - we are already handling a fault.
/// </summary>
    private string FontName()
    {
        try
        {
            return font.GetName();
        }
        catch (IOException e)
        {
            return "<unknown>";
        }
    }

    /// <summary>
/// The ppem to run a glyph program at when the glyph is drawn at ppem: the same, except that
/// a full-control font below its lowestRecPPEM is assembled at lowestRecPPEM
/// (capped at {@value #MAX_ASSEMBLY_PPEM}), and the result scaled down (see #getPath).
/// 
/// @param ppem the size the glyph is drawn at
/// @param fullControl whether the font's bytecode builds its glyphs
/// @param lowestRecPpem the font's head.lowestRecPPEM
/// @return the size to hint at
/// </summary>
    internal static int AssemblyPpem(int ppem, bool fullControl, int lowestRecPpem)
    {
        if (!fullControl || ppem <= 0 || ppem >= lowestRecPpem)
        {
            return ppem;
        }
        return Math.Max(ppem, Math.Min(lowestRecPpem, MAX_ASSEMBLY_PPEM));
    }

    /// <summary>
/// Whether the font's bytecode builds its glyph geometry rather than just fitting outlines to the
/// pixel grid. A small set of fonts - DynaLab CJK faces such as MingLiU and DFKai-SB, which FreeType
/// calls "tricky" - position and scale glyph components with their instructions, in x as well as y,
/// so they only render correctly with full bytecode control: no v40 backward compatibility (which
/// suppresses x moves) and no size gates.
/// &lt;p&gt;
/// Rather than recognise such fonts by name, this measures the behaviour itself, once per font: it
/// hints a spread of up to {@value #PROBE_SAMPLE} glyphs that carry instructions at
/// {@value #PROBE_PPEM} ppem with backward compatibility on and with it off (full control), and
/// compares them point by point with each other and with the raw outline. Ordinary hinting only
/// rounds edges to the grid, so with backward compatibility on no point moves more than a pixel or
/// two from the raw outline; a program that builds the glyphs moves components by many pixels even
/// then, and moves them differently again with it off. A glyph counts when both comparisons exceed
/// 5px (see #buildsGeometry), and {@value #PROBE_MIN_MOVING} such glyphs mark the font.
/// &lt;p&gt;
/// Measured at {@value #PROBE_PPEM} ppem on 267 fonts from CJK PDFs and system fonts, against
/// FreeType's own classification (second-largest glyph per font): fonts FreeType calls tricky are
/// at least 7.0px from raw with backward compatibility on and differ by at least 8.9px with it off;
/// ordinary fonts stay within 2.0px of raw - including Giovanni Book, whose broken x instructions
/// move points 90px with it off. No false positives; the only misses were two tiny subsets whose
/// hinted glyphs stay within 0.4px of the raw outline, so full control changes nothing for them.
/// 
/// @return true if glyph programs must run with full control
/// @throws IOException if the glyph table cannot be read
/// </summary>
    private bool NeedsFullControl() 
    {
        if (fullControl == null)
        {
            fullControl = ProbeFullControl();
            // the probe ran prep at its own size
            currentPpem = -1;
        }
        return fullControl.Value;
    }

    private bool ProbeFullControl() 
    {
        int moving = 0;
        foreach (int gid in ProbeSample())
        {
            try
            {
                int[][] compatible = ProbeHintedPoints(gid, true);
                int[][] full = ProbeHintedPoints(gid, false);
                if (compatible == null || full == null)
                {
                    continue;
                }
                int[][] raw = ProbeRawPoints(gid, full[0].Length);
                if (raw != null && BuildsGeometry(raw, compatible, full) && ++moving >= PROBE_MIN_MOVING)
                {
                    return true;
                }
            }
            catch (Exception e) when (e is IOException or HintingException)
            {
                // a glyph that cannot be hinted says nothing about the font
                LOG.LogDebug(e, "full-control probe could not hint glyph {GlyphId}", gid);
            }
        }
        return false;
    }

    /// <summary>
/// Up to PROBE_SAMPLE glyphs with instructions, spread evenly over the font's non-empty glyphs.
/// Non-empty glyphs are found from the loca offsets without parsing them - an embedded CID subset
/// keeps its full glyph count with only a handful of glyphs present - and at most
/// PROBE_CANDIDATES of them, spread evenly, are parsed.
/// </summary>
    private int[] ProbeSample() 
    {
        IndexToLocationTable loca = font.GetIndexToLocation();
        long[] offsets = loca != null ? loca.GetOffsets() : null;
        if (offsets == null)
        {
            return new int[0];
        }
        int[] present = new int[offsets.Length - 1];
        int presentCount = 0;
        for (int gid = 0; gid + 1 < offsets.Length; gid++)
        {
            if (offsets[gid + 1] > offsets[gid])
            {
                present[presentCount++] = gid;
            }
        }
        int candidates = Math.Min(presentCount, PROBE_CANDIDATES);
        int[] instructed = new int[candidates];
        int count = 0;
        for (int i = 0; i < candidates; i++)
        {
            int gid = present[(int) ((long) i * presentCount / candidates)];
            GlyphData glyph = font.GetGlyph().GetGlyph(gid);
            if (glyph == null || !(glyph.Description is GlyfDescript))
            {
                continue;
            }
            GlyfDescript gd = (GlyfDescript) glyph.Description;
            int[] instructions = gd.GetInstructions();
            if (gd.GetPointCount() > 0
                    && (gd.IsComposite() || instructions != null && instructions.Length > 0))
            {
                instructed[count++] = gid;
            }
        }
        // spread the sample over the whole font: the glyphs whose programs build geometry are the
        // ideographs, not the Latin and punctuation at the low glyph ids
        int size = Math.Min(count, PROBE_SAMPLE);
        int[] sample = new int[size];
        for (int i = 0; i < size; i++)
        {
            sample[i] = instructed[(int) ((long) i * count / size)];
        }
        return sample;
    }

    /// <summary>
/// Hints one glyph at PROBE_PPEM with backward compatibility forced, ignoring the size gates.
/// </summary>
    private int[][] ProbeHintedPoints(int gid, bool backwardCompatibility) 
    {
        SetActivePpem(PROBE_PPEM);
        forcedBackwardCompatibility = backwardCompatibility;
        try
        {
            Hinted? hinted = Hint(gid, PROBE_PPEM, 0);
            if (hinted == null)
            {
                return null;
            }
            int[] x = hinted.zone.GetCurrentX()[..hinted.pointCount];
            int[] y = hinted.zone.GetCurrentY()[..hinted.pointCount];
            return new int[][] { x, y };
        }
        finally
        {
            forcedBackwardCompatibility = null;
        }
    }

    /// <summary>
/// The glyph's scaled, unhinted outline at PROBE_PPEM, in the frame of the hinted points (origin
/// at x = 0, as the parsed description has it), or null if its point count differs.
/// </summary>
    private int[][] ProbeRawPoints(int gid, int pointCount) 
    {
        GlyphDescription gd = font.GetGlyph().GetGlyph(gid).Description;
        if (gd.IsComposite())
        {
            gd.Resolve();
        }
        if (gd.GetPointCount() != pointCount)
        {
            return null;
        }
        int[][] raw = new int[][] { new int[pointCount], new int[pointCount] };
        for (int i = 0; i < pointCount; i++)
        {
            raw[0][i] = Fixed.Scale(gd.GetXCoordinate(i), PROBE_PPEM, unitsPerEm);
            raw[1][i] = Fixed.Scale(gd.GetYCoordinate(i), PROBE_PPEM, unitsPerEm);
        }
        return raw;
    }

    /// <summary>
/// Whether a glyph's program builds its geometry rather than fitting it to the grid: backward
/// compatibility changes the result by more than PROBE_TOLERANCE, &lt;em&gt;and&lt;/em&gt; even with backward
/// compatibility on the result is that far from the raw outline. The second condition tells a font
/// that needs full control (MingLiU: at least 7px from raw with it on) from one whose x
/// instructions are merely broken and are hidden by backward compatibility (Giovanni Book: about
/// 1px from raw with it on, 90px with it off) - giving the latter full control would wreck it.
/// 
/// @param raw the scaled, unhinted outline
/// @param compatible the outline hinted with backward compatibility on
/// @param full the outline hinted with full control
/// @return true if the glyph counts towards needing full control
/// </summary>
    internal static bool BuildsGeometry(int[][] raw, int[][] compatible, int[][] full)
    {
        return PointDelta(compatible, full) > PROBE_TOLERANCE
                && PointDelta(raw, compatible) > PROBE_TOLERANCE;
    }

    /// <summary>
/// The largest displacement of any point between two outlines of the same glyph, on either axis.
/// </summary>
    internal static int PointDelta(int[][] a, int[][] b)
    {
        int delta = 0;
        for (int i = 0; i < a[0].Length; i++)
        {
            delta = Math.Max(delta, Math.Max(Math.Abs(a[0][i] - b[0][i]), Math.Abs(a[1][i] - b[1][i])));
        }
        return delta;
    }

    /// <summary>
/// @return whether the probe found that this font's bytecode builds its glyphs; for tests
/// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.Synchronized)]
    internal bool IsFullControl() 
    {
        Initialize();
        return available && NeedsFullControl();
    }

    /// <summary>
/// Re-runs the control value program if the ppem changed. The guard is load-bearing, not just an
/// optimization: setPpem clears the storage area and twilight zone before running
/// prep, so re-running it per glyph would wipe the values prep seeded for the glyph
/// programs to read.
/// </summary>
    private void SetActivePpem(int ppem)
    {
        if (ppem != currentPpem)
        {
            // setPpem rescales the CVT and clears storage before running prep; if prep throws, the
            // interpreter no longer holds currentPpem's state, so force the next call to re-run it
            currentPpem = -1;
            interpreter.SetPpem(ppem, ppem);
            currentPpem = ppem;
        }
    }

    /// <summary>
/// Grid-fits one glyph (simple or composite), recursing into components.
/// </summary>
    private Hinted? Hint(int gid, int ppem, int depth) 
    {
        if (depth > MAX_COMPONENT_DEPTH)
        {
            return null;
        }
        GlyphData glyph = font.GetGlyph().GetGlyph(gid);
        if (glyph == null)
        {
            return null;
        }
        GlyphDescription gd = glyph.Description;
        if (!(gd is GlyfDescript))
        {
            return null;
        }
        if (gd.IsComposite())
        {
            gd.Resolve();
            if (gd.GetPointCount() == 0)
            {
                return null;
            }
            return HintComposite(glyph, (GlyfCompositeDescript) gd, gid, ppem, depth);
        }
        if (gd.GetContourCount() == 0 || gd.GetPointCount() == 0)
        {
            // empty glyph (e.g. space, newline): nothing to hint
            return null;
        }
        int[] instructions = ((GlyfDescript) gd).GetInstructions();
        if (instructions == null || instructions.Length == 0)
        {
            return null;
        }
        int pointCount = gd.GetPointCount();
        Zone zone = BuildZone(glyph, gd, gid, ppem, pointCount, gd.GetContourCount());
        ExecutionContext ctx = RunProgram(zone, instructions, ppem, false);
        // move the origin to x = 0, as FreeType does after hinting (TT_Load_Glyph translates by
        // -pp1.x). Under backward compatibility FreeType keeps the unhinted phantom points
        // (TT_Hint_Glyph), otherwise the hinted ones
        int originX = ctx.IsBackwardCompatibility() ? zone.GetOriginalX()[pointCount]
                : zone.GetCurrentX()[pointCount];
        int[] curX = zone.GetCurrentX();
        for (int i = 0; i < pointCount + 4; i++)
        {
            curX[i] -= originX;
        }
        return new Hinted(gd, zone, pointCount);
    }

    /// <summary>
/// Grid-fits a composite glyph the way FreeType does: each component is hinted on its own, then
/// transformed and offset into the composite's coordinate space, the phantom points appended, and
/// finally the composite's own instructions (if any) run over the assembled outline.
/// </summary>
    private Hinted? HintComposite(GlyphData glyph, GlyfCompositeDescript composite, int gid, int ppem,
            int depth) 
    {
        int pointCount = composite.GetPointCount();
        int contourCount = composite.GetContourCount();
        Zone zone = new Zone(pointCount + 4, contourCount);

        foreach (GlyfCompositeComp comp in composite.GetComponents())
        {
            AssembleComponent(comp, ppem, depth, zone);
        }
        int[] ends = zone.GetContourEnds();
        for (int c = 0; c < contourCount; c++)
        {
            ends[c] = composite.GetEndPtOfContours(c);
        }
        AppendPhantomPoints(glyph, gid, ppem, pointCount, zone);
        // A composite's own program works on its components as already hinted and assembled, so its
        // unscaled coordinates are those assembled positions (the originals) at a scale of 1, as in
        // FreeType's TT_Hint_Glyph (see ExecutionContext.isComposite)
        Array.Copy(zone.GetOriginalX(), 0, zone.GetUnscaledX(), 0, pointCount + 4);
        Array.Copy(zone.GetOriginalY(), 0, zone.GetUnscaledY(), 0, pointCount + 4);

        int[] instructions = composite.GetInstructions();
        if (instructions != null && instructions.Length > 0)
        {
            RunProgram(zone, instructions, ppem, true);
        }
        return new Hinted(composite, zone, pointCount);
    }

    /// <summary>
/// Hints one component glyph and writes its transformed/offset points into the composite's zone
/// arrays. The component's grid-fitted outline goes to both the current and the original arrays,
/// as FreeType bakes hinted components into the composite before running its instructions.
/// </summary>
    private void AssembleComponent(GlyfCompositeComp comp, int ppem, int depth, Zone zone)
            
    {
        int componentGid = comp.GlyphIndex;
        int first = comp.FirstIndex;

        GlyphData componentGlyph = font.GetGlyph().GetGlyph(componentGid);
        GlyphDescription cgd = componentGlyph != null ? componentGlyph.Description : null;
        if (cgd == null)
        {
            return;
        }
        if (cgd.IsComposite())
        {
            cgd.Resolve();
        }
        int count = cgd.GetPointCount();
        bool[] onCurve = zone.GetOnCurve();

        // scaled-but-unhinted component points, used when the component is not hinted
        int[] cOrgX = new int[count];
        int[] cOrgY = new int[count];
        for (int k = 0; k < count; k++)
        {
            cOrgX[k] = Fixed.Scale(cgd.GetXCoordinate(k), ppem, unitsPerEm);
            cOrgY[k] = Fixed.Scale(cgd.GetYCoordinate(k), ppem, unitsPerEm);
            onCurve[first + k] = (cgd.GetFlags(k) & GlyfDescript.OnCurve) != 0;
        }

        // grid-fitted component points (its own instructions executed); fall back to unhinted
        int[] cCurX = cOrgX;
        int[] cCurY = cOrgY;
        Hinted? hintedComponent = Hint(componentGid, ppem, depth + 1);
        if (hintedComponent != null && hintedComponent.pointCount == count)
        {
            cCurX = hintedComponent.zone.GetCurrentX();
            cCurY = hintedComponent.zone.GetCurrentY();
        }

        // device-space offset. With ROUND_XY_TO_GRID FreeType's v40 interpreter rounds only the y
        // offset to the grid, leaving x unrounded to approximate ClearType's fine horizontal grid
        // (TT_Process_Composite_Component)
        int offsetX = Fixed.Scale(comp.XTranslate, ppem, unitsPerEm);
        int offsetY = Fixed.Scale(comp.YTranslate, ppem, unitsPerEm);
        if ((comp.Flags & GlyfCompositeComp.RoundXyToGrid) != 0)
        {
            offsetY = Fixed.Round(offsetY);
        }

        int[] curX = zone.GetCurrentX();
        int[] curY = zone.GetCurrentY();
        int[] orgX = zone.GetOriginalX();
        int[] orgY = zone.GetOriginalY();
        for (int k = 0; k < count; k++)
        {
            curX[first + k] = comp.ScaleX(cCurX[k], cCurY[k]) + offsetX;
            curY[first + k] = comp.ScaleY(cCurX[k], cCurY[k]) + offsetY;
            // FreeType bakes each hinted component into the composite and copies cur -> org before
            // running the composite program, so the original equals the assembled hinted position
            // (a SHC/MDRP in the composite then measures zero movement for an unmoved component point)
            orgX[first + k] = curX[first + k];
            orgY[first + k] = curY[first + k];
        }
    }

    /// <summary>
/// Clones the saved post-prep state, resets it for the glyph, and runs the program over the zone.
/// </summary>
    private ExecutionContext RunProgram(Zone zone, int[] instructions, int ppem, bool composite)
    {
        GraphicsState saved = interpreter.GetSavedState();
        // INSTCTRL bit 2: glyph programs start from the default graphics state rather than prep's
        GraphicsState gs = (saved.GetInstructControl() & 2) == 0 ? saved.Copy() : new GraphicsState();
        gs.ResetForGlyph();
        ExecutionContext ctx = interpreter.NewContext(gs);
        ctx.SetPpem(ppem);
        ctx.SetGlyphZone(zone);
        // v40 grayscale "backward compatibility" applies to the glyph program only, never fpgm/prep,
        // which build control values via twilight-zone x/y moves that must not be suppressed. A
        // native-ClearType font waives it from prep with INSTCTRL bit 4; read from gs, so a bit-2
        // reset to the default state also clears the waiver, as in FreeType's tt_loader_init.
        // A font whose bytecode builds the glyph geometry gets full control, as FreeType gives its
        // "tricky" fonts; the probe forces the mode while it measures.
        if (forcedBackwardCompatibility != null)
        {
            ctx.SetBackwardCompatibility(forcedBackwardCompatibility.Value);
        }
        else
        {
            ctx.SetBackwardCompatibility(!(fullControl == true)
                    && (gs.GetInstructControl() & 4) == 0);
        }
        ctx.SetComposite(composite);
        interpreter.Run(ctx, new BytecodeStream(ToByteArray(instructions)));
        return ctx;
    }

    /// <summary>
/// The result of grid-fitting one glyph: its description, the executed zone, and its point count
/// (without the appended phantom points).
/// </summary>
    private sealed class Hinted
    {
        internal readonly GlyphDescription gd;
        internal readonly Zone zone;
        internal readonly int pointCount;

        internal Hinted(GlyphDescription gd, Zone zone, int pointCount)
        {
            this.gd = gd;
            this.zone = zone;
            this.pointCount = pointCount;
        }
    }

    private Zone BuildZone(GlyphData glyph, GlyphDescription gd, int gid, int ppem, int pointCount,
            int contourCount) 
    {
        // four phantom points are appended after the glyph's own points
        Zone zone = new Zone(pointCount + 4, contourCount);
        int[] curX = zone.GetCurrentX();
        int[] curY = zone.GetCurrentY();
        int[] orgX = zone.GetOriginalX();
        int[] orgY = zone.GetOriginalY();
        int[] unsX = zone.GetUnscaledX();
        int[] unsY = zone.GetUnscaledY();
        bool[] onCurve = zone.GetOnCurve();
        // GlyphData shifts a simple glyph's x coordinates by lsb - xMin so its origin is at 0.
        // FreeType hints the unshifted outline, with the origin phantom point at xMin - lsb, and
        // moves the origin afterwards; undo the shift so rounding and phantom-point positions match
        HorizontalMetricsTable hmtx = font.GetHorizontalMetrics();
        int leftSideBearing = hmtx != null ? hmtx.GetLeftSideBearing(gid) : 0;
        int parseShift = (short) (leftSideBearing - glyph.XMinimum);
        for (int i = 0; i < pointCount; i++)
        {
            int fx = gd.GetXCoordinate(i) - parseShift;
            int fy = gd.GetYCoordinate(i);
            unsX[i] = fx;
            unsY[i] = fy;
            int x = Fixed.Scale(fx, ppem, unitsPerEm);
            int y = Fixed.Scale(fy, ppem, unitsPerEm);
            orgX[i] = x;
            orgY[i] = y;
            curX[i] = x;
            curY[i] = y;
            onCurve[i] = (gd.GetFlags(i) & GlyfDescript.OnCurve) != 0;
        }
        int[] ends = zone.GetContourEnds();
        for (int c = 0; c < contourCount; c++)
        {
            ends[c] = gd.GetEndPtOfContours(c);
        }
        AppendPhantomPoints(glyph, gid, ppem, pointCount, zone);
        return zone;
    }

    private void AppendPhantomPoints(GlyphData glyph, int gid, int ppem, int pointCount, Zone zone)
            
    {
        HorizontalMetricsTable hmtx = font.GetHorizontalMetrics();
        int advanceWidth = hmtx != null ? hmtx.GetAdvanceWidth(gid) : unitsPerEm;
        int leftSideBearing = hmtx != null ? hmtx.GetLeftSideBearing(gid) : 0;
        int originX = glyph.XMinimum - leftSideBearing;
        int yMax = glyph.YMaximum;

        // pp1 = origin, pp2 = origin + advance (horizontal); pp3/pp4 are the vertical pair
        int[] px = [ originX, originX + advanceWidth, 0, 0 ];
        int[] py = [ 0, 0, yMax, yMax - unitsPerEm ];
        for (int i = 0; i < 4; i++)
        {
            int index = pointCount + i;
            zone.GetUnscaledX()[index] = px[i];
            zone.GetUnscaledY()[index] = py[i];
            zone.GetOriginalX()[index] = Fixed.Scale(px[i], ppem, unitsPerEm);
            zone.GetOriginalY()[index] = Fixed.Scale(py[i], ppem, unitsPerEm);
            // FreeType rounds the phantom points to the grid before running the glyph program
            zone.GetCurrentX()[index] = Fixed.Round(zone.GetOriginalX()[index]);
            zone.GetCurrentY()[index] = Fixed.Round(zone.GetOriginalY()[index]);
        }
    }

    /// <summary>
/// Scales an F26Dot6 device coordinate back to font units.
/// </summary>
    private int ToFontUnits(int f26dot6, int ppem)
    {
        long numerator = (long) f26dot6 * unitsPerEm;
        long denominator = (long) ppem * Fixed.ONE;
        long half = denominator / 2;
        return (int) ((numerator >= 0 ? numerator + half : numerator - half) / denominator);
    }

    private static byte[] ToByteArray(int[] instructions)
    {
        byte[] bytes = new byte[instructions.Length];
        for (int i = 0; i < instructions.Length; i++)
        {
            bytes[i] = (byte) instructions[i];
        }
        return bytes;
    }
}
