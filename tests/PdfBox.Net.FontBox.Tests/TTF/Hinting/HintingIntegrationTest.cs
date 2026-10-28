/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/test/java/org/apache/fontbox/ttf/HintingIntegrationTest.java
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

using PdfBox.Net.FontBox.TTF;
using PdfBox.Net.IO;
using ExecutionContext = PdfBox.Net.FontBox.TTF.ExecutionContext;

namespace PdfBox.Net.FontBox.Tests.TTF.Hinting;

/// <summary>
/// End-to-end tests: drive hinting through TrueTypeFont#getHintedPath(int, int) at the
/// GeneralPath level. LiberationSans carries a full bytecode program; Lohit-Bengali has a cvt
/// but no fpgm/prep, so it exercises the no-bytecode fallback.
/// </summary>
public class HintingIntegrationTest : HintingTestSupport
{


    private static int Gid(TrueTypeFont font, int codePoint)
    {
        return font.GetUnicodeCmapLookup()!.GetGlyphId(codePoint);
    }

    /// <summary>
    /// Flattens a path to a coordinate list so two paths can be compared point-for-point.
    /// </summary>


    [Fact]
    public void TestGaspGate()
    {
        using TrueTypeFont font = Parse("/ttf/LiberationSans-Regular.ttf");
        int h = Gid(font, 'H');
        // LiberationSans gasp: grid-fitting is off at <=8 ppem, on above it
        IsNull(font.GetHintedPath(h, 8), "no hinting expected at 8ppem (gasp)");
        NotNull(font.GetHintedPath(h, 16), "hinting expected at 16ppem");
    }

    /// <summary>
    /// head.lowestRecPPEM is the vendor's smallest readable outline size. CJK fonts that ship
    /// bitmap strikes for small sizes set it to 20-28 (MS Mincho/Gothic, MingLiU: 25; Ricoh HG*: 28)
    /// and their instructions, run under grayscale at those sizes, make the text heavy; text fonts sit
    /// at 6-9. No test font declares a value above its gasp threshold, so the value is raised on
    /// Liberation Sans (normally 8) before the hinter first reads it.
    /// </summary>
    [Fact]
    public void TestLowestRecPpemGate()
    {
        using TrueTypeFont font = Parse("/ttf/LiberationSans-Regular.ttf");
        font.GetHeader()!.SetLowestRecPPEM(20);
        int h = Gid(font, 'H');
        IsNull(font.GetHintedPath(h, 16), "no hinting below lowestRecPPEM");
        NotNull(font.GetHintedPath(h, 20), "hinting expected from lowestRecPPEM up");
    }

    [Fact]
    public void TestHintedDiffersFromRawOutline()
    {
        using TrueTypeFont font = Parse("/ttf/LiberationSans-Regular.ttf");
        int h = Gid(font, 'H');
        GeneralPath? hinted = font.GetHintedPath(h, 16);
        NotNull(hinted);
        GeneralPath raw = font.GetGlyph()!.GetGlyph(h)!.GetPath();
        // grid-fitting must actually change the outline (proves the pipeline is wired in)
        Assert.False(Enumerable.SequenceEqual(Flatten(hinted), Flatten(raw)),
                "hinted path should differ from the raw outline");
    }

    [Fact]
    public void TestDeterministic()
    {
        using TrueTypeFont font = Parse("/ttf/LiberationSans-Regular.ttf");
        int h = Gid(font, 'H');
        double[] first = Flatten(font.GetHintedPath(h, 16));
        double[] second = Flatten(font.GetHintedPath(h, 16));
        // re-hinting the same glyph at the same ppem is deterministic (no leaked state)
        Assert.True(Enumerable.SequenceEqual(first, second));
    }

    /// <summary>
    /// Hinting one glyph must not change the next. The storage area and twilight zone are deliberately
    /// shared across the glyphs hinted at one size (that is how prep seeds them), so this pins
    /// down that nothing &lt;em&gt;else&lt;/em&gt; leaks between glyphs - graphics state, zone contents, the cached
    /// ppem. A composite is in the run because it re-enters the hinter for each component.
    ///
    /// &lt;p&gt;This is a property of a well-behaved font rather than a universal law: a glyph program may
    /// legally write storage, and FreeType would carry that into the next glyph too. LiberationSans does
    /// not, so any difference here is a bug on our side.
    /// </summary>
    [Fact]
    public void TestHintingIsIndependentOfGlyphOrder()
    {
        using TrueTypeFont font = Parse("/ttf/LiberationSans-Regular.ttf");
        int h = Gid(font, 'H');
        int eacute = Gid(font, 0x00E9);
        Assert.True(font.GetGlyph()!.GetGlyph(eacute)!.NumberOfContours < 0,
                "expected e-acute to be a composite glyph");

        double[] before = Flatten(font.GetHintedPath(h, 16));
        foreach (char c in "oxn8")
        {
            font.GetHintedPath(Gid(font, c), 16);
        }
        font.GetHintedPath(eacute, 16);
        double[] after = Flatten(font.GetHintedPath(h, 16));

        Assert.True(Enumerable.SequenceEqual(before, after),
                "hinting other glyphs must not change the result for 'H'");
    }

    /// <summary>
    /// Returning to a ppem must reproduce the earlier result. A ppem change re-runs the control value
    /// program, which clears the storage area and twilight zone, so this covers the round trip output of a
    /// size and back into it.
    /// </summary>
    [Fact]
    public void TestHintingIsIndependentOfPpemOrder()
    {
        using TrueTypeFont font = Parse("/ttf/LiberationSans-Regular.ttf");
        int h = Gid(font, 'H');

        double[] before = Flatten(font.GetHintedPath(h, 16));
        font.GetHintedPath(h, 11);
        font.GetHintedPath(h, 24);
        double[] after = Flatten(font.GetHintedPath(h, 16));

        Assert.True(Enumerable.SequenceEqual(before, after),
                "hinting at other ppems must not change the result at 16ppem");
    }

    /// <summary>
    /// A ppem change rescales the CVT and clears storage before prep runs, so a prep
    /// that fails at one size leaves the interpreter holding that size's state. Returning to the earlier
    /// size must re-run prep rather than trust the cached ppem. Liberation Sans's own
    /// prep is prefixed with MPPEM 11 EQ IF &lt;invalid opcode&gt; EIF so it fails at 11ppem
    /// only.
    /// </summary>
    [Fact]
    public void TestFailedPrepDoesNotLeaveStaleState()
    {
        using TrueTypeFont font = WithPrepPrefix(0x4B, 0xB0, 11, 0x54, 0x58, 0x28, 0x59);
        int h = Gid(font, 'H');
        double[] before = Flatten(font.GetHintedPath(h, 16));
        IsNull(font.GetHintedPath(h, 11), "prep fails at 11ppem, so no hinting there");
        double[] after = Flatten(font.GetHintedPath(h, 16));

        Assert.True(Enumerable.SequenceEqual(before, after),
                "a prep failure at another size must not change the result at 16ppem");
    }

    /// <summary>
    /// A component with ROUND_XY_TO_GRID has its y offset rounded to the grid, but under the v40
    /// interpreter not its x offset (FreeType's TT_Process_Composite_Component). In Liberation Sans the
    /// 'Â¼' glyph (no instructions of its own) places four.sups at (952, -561) font units with that flag;
    /// at 11ppem that scales to (327.25, -192.84)/64 px, and FreeType offsets the component's points by exactly
    /// (+327, -192).
    /// </summary>
    [Fact]
    public void TestRoundXyToGridRoundsOnlyTheComponentYOffset()
    {
        using TrueTypeFont font = Parse("/ttf/LiberationSans-Regular.ttf");
        Equal(2048, font.GetUnitsPerEm());
        GlyphHinter hinter = new GlyphHinter(font);
        GlyfCompositeDescript quarter =
                Assert.IsType<GlyfCompositeDescript>(font.GetGlyph()!.GetGlyph(Gid(font, 0x00BC))!.Description);
        quarter.Resolve();
        GlyfCompositeComp four = quarter.GetComponents()[2];
        Equal(952, four.XTranslate);
        Equal(-561, four.YTranslate);
        Assert.NotEqual(0, four.Flags & GlyfCompositeComp.RoundXyToGrid);

        // four.sups has no instructions, so it enters the composite as its plain scaled outline
        GlyphDescription component = Assert.IsAssignableFrom<GlyphDescription>(font.GetGlyph()!.GetGlyph(four.GlyphIndex)!.Description);
        IsNull(hinter.GetHintedPointsF26Dot6(four.GlyphIndex, 11));

        int[][] composite = Assert.IsType<int[][]>(hinter.GetHintedPointsF26Dot6(Gid(font, 0x00BC), 11));
        for (int k = 0; k < component.GetPointCount(); k++)
        {
            int x = Fixed.Scale(component.GetXCoordinate(k), 11, 2048);
            int y = Fixed.Scale(component.GetYCoordinate(k), 11, 2048);
            Equal(327, composite[0][four.FirstIndex + k] - x, "x of point " + k);
            Equal(-192, composite[1][four.FirstIndex + k] - y, "y of point " + k);
        }
    }

    /// <summary>
    /// INSTCTRL(1,1) in prep switches hinting off at that size: FreeType renders the raw outline.
    /// </summary>
    [Fact]
    public void TestInstctrlInPrepCanDisableHinting()
    {
        using TrueTypeFont font = WithPrepPrefix(0xB1, 1, 1, 0x8E);
        IsNull(font.GetHintedPath(Gid(font, 'H'), 16));
    }

    /// <summary>
    /// INSTCTRL(4,3) in prep is the native-ClearType waiver: glyph programs run without the v40
    /// backward-compatibility rules, so x-direction moves are no longer suppressed and the result
    /// differs from the default.
    /// </summary>
    [Fact]
    public void TestInstctrlInPrepCanWaiveBackwardCompatibility()
    {
        using TrueTypeFont plain = Parse("/ttf/LiberationSans-Regular.ttf");
        using TrueTypeFont waived = WithPrepPrefix(0xB1, 4, 3, 0x8E);
        int h = Gid(plain, 'H');
        Assert.False(Enumerable.SequenceEqual(Flatten(plain.GetHintedPath(h, 16)),
                Flatten(waived.GetHintedPath(h, 16))));
    }

    /// <summary>
    /// Loads Liberation Sans with the given bytecode prepended to its prep, before the hinter
    /// first reads the table.
    /// </summary>
    private static TrueTypeFont WithPrepPrefix(params int[] prefix)
    {
        TrueTypeFont font = Parse("/ttf/LiberationSans-Regular.ttf");
        byte[] prep = font.GetControlValueProgram()!.GetProgram();
        byte[] program = new byte[prefix.Length + prep.Length];
        for (int i = 0; i < prefix.Length; i++)
        {
            program[i] = (byte) prefix[i];
        }
        Array.Copy(prep, 0, program, prefix.Length, prep.Length);
        ControlValueProgramTable? table = new ControlValueProgramTable();
        table.SetTag(ControlValueProgramTable.TAG);
        table.SetLength((uint)program.Length);
        table.Read(font, new RandomAccessReadDataStream(new RandomAccessReadBuffer(program)));
        font.AddTable(table);
        return font;
    }

    /// <summary>
    /// The full-control probe compares two outlines of a glyph by the largest displacement of any
    /// point on either axis - so a component moved inside the glyph counts, not just its extents.
    /// </summary>
    [Fact]
    public void TestPointDeltaIsTheLargestPointDisplacement()
    {
        int[][] a = { new int[] { 0, 640, 320 }, new int[] { 0, 0, 640 } };
        Equal(0, GlyphHinter.PointDelta(a, a));
        // an interior point moves 5px in x (bounding box unchanged), another 1px in y
        int[][] b = { new int[] { 0, 640, 640 }, new int[] { 0, 64, 640 } };
        Equal(320, GlyphHinter.PointDelta(a, b));
    }

    /// <summary>
    /// A glyph counts towards full bytecode control only if backward compatibility changes it a lot
    /// &lt;em&gt;and&lt;/em&gt; it is far from its raw outline even with backward compatibility on. A glyph whose
    /// full-control result alone is wild - broken x instructions that backward compatibility hides,
    /// as in Giovanni Book - must not count, or the font would be wrecked.
    /// </summary>
    [Fact]
    public void TestBuildsGeometryNeedsBothSignals()
    {
        int[][] raw = { new int[] { 0, 640 }, new int[] { 0, 640 } };
        int[][] nearRaw = { new int[] { 0, 640 }, new int[] { 0, 704 } };      // 1px from raw: ordinary hinting
        int[][] built = { new int[] { 640, 640 }, new int[] { 0, 640 } };      // 10px from raw: components moved
        int[][] wild = { new int[] { 6400, 640 }, new int[] { 0, 640 } };      // 100px: broken x instructions
        // tricky font: far from raw with backward compatibility on, and different again without
        Assert.True(GlyphHinter.BuildsGeometry(raw, built, wild));
        // broken x hinting: fine with backward compatibility on, wild without
        Assert.False(GlyphHinter.BuildsGeometry(raw, nearRaw, wild));
        // ordinary font: neither
        Assert.False(GlyphHinter.BuildsGeometry(raw, nearRaw, nearRaw));
        // far from raw but backward compatibility makes no difference: full control would not help
        Assert.False(GlyphHinter.BuildsGeometry(raw, built, built));
    }

    /// <summary>
    /// A font whose bytecode builds its glyphs is assembled at its lowestRecPPEM when drawn smaller,
    /// and the result scaled down; at and above that size, and for every other font, glyphs are hinted
    /// at the size they are drawn at.
    /// </summary>
    [Fact]
    public void TestAssemblyPpem()
    {
        // MingLiU-like font (lowestRecPPEM 25): below 25 assemble at 25, from 25 up unchanged
        Equal(25, GlyphHinter.AssemblyPpem(13, true, 25));
        Equal(25, GlyphHinter.AssemblyPpem(24, true, 25));
        Equal(25, GlyphHinter.AssemblyPpem(25, true, 25));
        Equal(107, GlyphHinter.AssemblyPpem(107, true, 25));
        // ordinary fonts are never resized, whatever lowestRecPPEM says
        Equal(13, GlyphHinter.AssemblyPpem(13, false, 25));
        // an absurd lowestRecPPEM is capped
        Equal(64, GlyphHinter.AssemblyPpem(13, true, 255));
        // nothing to do for a font that declares a small lowestRecPPEM, or a non-positive size
        Equal(13, GlyphHinter.AssemblyPpem(13, true, 9));
        Equal(0, GlyphHinter.AssemblyPpem(0, true, 25));
    }

    [Fact]
    public void TestFontWithoutBytecodeFallsBack()
    {
        // Lohit-Bengali has a cvt but no fpgm and no prep: there is nothing to execute
        using TrueTypeFont font = Parse("/ttf/Lohit-Bengali.ttf");
        int gid = font.GetUnicodeCmapLookup()!.GetGlyphId(0x0985); // Bengali letter A
        IsNull(font.GetHintedPath(gid, 16));
    }

    [Fact]
    public void TestEmptyGlyphFallsBack()
    {
        using TrueTypeFont font = Parse("/ttf/LiberationSans-Regular.ttf");
        // the space glyph has no contours; hinting must fall back rather than fail
        IsNull(font.GetHintedPath(Gid(font, ' '), 16));
    }

    [Fact]
    public void TestCommonGlyphsHintWithoutFallingBack()
    {
        // a real-font smoke test: every one of these simple glyphs carries instructions and must
        // grid-fit without throwing (which would silently fall back to null). This is what caught the
        // MIRP stack-order bug.
        using TrueTypeFont font = Parse("/ttf/LiberationSans-Regular.ttf");
        string sample = "HILEToxn0123456789";
        foreach (int ppem in new int[] { 11, 13, 16, 24 })
        {
            for (int i = 0; i < sample.Length; i++)
            {
                char c = sample[i];
                NotNull(font.GetHintedPath(Gid(font, c), ppem),
                        "expected '" + c + "' to hint at " + ppem + "ppem");
            }
        }
    }
    [Fact]
    public void CompositeScaleUsesJavaHalfRounding()
    {
        // ARG_1_AND_2_ARE_WORDS | ARGS_ARE_XY_VALUES | WE_HAVE_A_SCALE, with scale 0.5.
        using MemoryTTFDataStream data = new(new byte[] { 0, 11, 0, 0, 0, 0, 0, 0, 0x20, 0 });
        var component = new GlyfCompositeComp(data);
        Equal(1, component.ScaleX(1, 0));
        Equal(0, component.ScaleX(-1, 0));
        Equal(1, component.ScaleY(0, 1));
        Equal(0, component.ScaleY(0, -1));
        Equal(16777215, component.ScaleX(33554430, 0));
    }

}
