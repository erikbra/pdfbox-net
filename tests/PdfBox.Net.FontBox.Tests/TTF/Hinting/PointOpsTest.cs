/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/test/java/org/apache/fontbox/ttf/PointOpsTest.java
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
using ExecutionContext = PdfBox.Net.FontBox.TTF.ExecutionContext;

namespace PdfBox.Net.FontBox.Tests.TTF.Hinting;

/// <summary>
/// Unit tests for the point-moving opcodes: each builds a glyph zone, runs a short program
/// through the interpreter, and asserts the resulting coordinates. Coordinates are in F26Dot6 (64 per
/// pixel). Byte-exact agreement with FreeType is proven by GoldenHintingTest; these check the
/// per-opcode logic.
/// </summary>
public class PointOpsTest
{
    /// <summary>
    /// 16ppem at 1024 units per em is a scale of exactly 1 (16 * 64 / 1024), so the synthetic zones'
    /// unscaled coordinates can equal their device coordinates; MDRP and IP measure the former.
    /// </summary>
    private static TrueTypeInterpreter Interpreter()
    {
        return new TrueTypeInterpreter(256, 16, 16, 1024);
    }

    /// <summary>
    /// Builds a single-contour glyph zone with the given x coordinates (original == current).
    /// </summary>
    private static Zone LineZone(params int[] xs)
    {
        Zone zone = new Zone(xs.Length, 1);
        for (int i = 0; i < xs.Length; i++)
        {
            zone.GetOriginalX()[i] = xs[i];
            zone.GetCurrentX()[i] = xs[i];
            // these synthetic tests work directly in device units, so the unscaled originals (used by
            // IP for its interpolation ratio) mirror the scaled ones
            zone.GetUnscaledX()[i] = xs[i];
        }
        zone.GetContourEnds()[0] = xs.Length - 1;
        return zone;
    }

    private static ExecutionContext Context(TrueTypeInterpreter interp, Zone glyph)
    {
        ExecutionContext ctx = interp.NewContext(new GraphicsState());
        ctx.SetPpem(16);
        ctx.SetGlyphZone(glyph);
        return ctx;
    }

    [Fact]
    public void TestProjectionAndMove()
    {
        ExecutionContext ctx =
                new ExecutionContext(null, new GraphicsState(), 16, new int[0], null, new Zone(0, 0));
        Zone zone = LineZone(100, 0);
        // default projection/freedom is the x axis: project returns the x coordinate
        Equal(100, ctx.Project(zone.GetCurrentX()[0], zone.GetCurrentY()[0]));
        ctx.MovePoint(zone, 0, 28); // move +28 along x
        Equal(128, zone.GetCurrentX()[0]);
        Assert.True(zone.GetTouchedX()[0]);
        Assert.False(zone.GetTouchedY()[0]);
    }

    [Fact]
    public void TestMdapRoundsToGrid()
    {
        TrueTypeInterpreter interp = Interpreter();
        Zone zone = LineZone(100); // 1.5625px
        ExecutionContext ctx = Context(interp, zone);
        // PUSHB[0] 0 ; MDAP[1] (round)
        interp.Run(ctx, new BytecodeStream(new byte[] { (byte) 0xB0, 0, 0x2F }));
        Equal(128, zone.GetCurrentX()[0]); // rounded to 2px
        Assert.True(zone.GetTouchedX()[0]);
    }

    [Fact]
    public void TestMdrpRelativeToRp0()
    {
        TrueTypeInterpreter interp = Interpreter();
        Zone zone = LineZone(0, 100); // rp0 = point 0 at x=0, point 1 at 1.5625px
        ExecutionContext ctx = Context(interp, zone);
        // PUSHB[0] 1 ; MDRP[round] (0xC4, round bit = 0x04) - grid-rounded distance from rp0
        interp.Run(ctx, new BytecodeStream(new byte[] { (byte) 0xB0, 1, (byte) 0xC4 }));
        Equal(128, zone.GetCurrentX()[1]); // distance 100 rounds to 128
    }

    /// <summary>
    /// MDRP measures the original distance in font units and scales it, rather than projecting the
    /// individually rounded scaled originals (FreeType's Ins_MDRP, orus). From DejaVu Sans Bold '1' at
    /// 14ppem: points 72 units apart at 2048 upem are 31.5/64 px apart. Scaled on their own they land
    /// at 653 and 622, 31 apart, which rounds to 0; the scaled distance is 32, which rounds to a pixel.
    /// </summary>
    [Fact]
    public void TestMdrpScalesTheUnscaledOriginalDistance()
    {
        TrueTypeInterpreter interp = new TrueTypeInterpreter(256, 16, 16, 2048);
        interp.SetPpem(14, 14);
        Zone zone = new Zone(2, 1);
        int[] unscaled = { 1493, 1421 };
        for (int i = 0; i < 2; i++)
        {
            int scaled = Fixed.Scale(unscaled[i], 14, 2048);
            SetPoint(zone, i, scaled, 0);
            zone.GetUnscaledX()[i] = unscaled[i];
        }
        Equal(653, zone.GetCurrentX()[0]);
        Equal(622, zone.GetCurrentX()[1]);
        ExecutionContext ctx = interp.NewContext(new GraphicsState());
        ctx.SetGlyphZone(zone);
        // PUSHB[0] 1 ; MDRP[round] (0xC4) relative to rp0 = point 0
        interp.Run(ctx, new BytecodeStream(new byte[] { (byte) 0xB0, 1, (byte) 0xC4 }));
        Equal(653 - 64, zone.GetCurrentX()[1]);
    }

    [Fact]
    public void TestMdrpRoundAndMinimumDistanceFlagsAreDistinct()
    {
        // guards the flag encoding: round is bit 0x04, minimum-distance is bit 0x08 (they were once
        // swapped). A small original distance (30) below the minimum (64):
        TrueTypeInterpreter interp = Interpreter();
        // round only (0xC4): 30 -> round(30) = 0, no minimum clamp
        Zone roundZone = LineZone(0, 30);
        interp.Run(Context(interp, roundZone), new BytecodeStream(new byte[] { (byte) 0xB0, 1, (byte) 0xC4 }));
        Equal(0, roundZone.GetCurrentX()[1]);

        // minimum-distance only (0xC8): no rounding, but clamp the distance up to the minimum (64)
        Zone minZone = LineZone(0, 30);
        interp.Run(Context(interp, minZone), new BytecodeStream(new byte[] { (byte) 0xB0, 1, (byte) 0xC8 }));
        Equal(64, minZone.GetCurrentX()[1]);
    }

    [Fact]
    public void TestMirpUsesControlValue()
    {
        TrueTypeInterpreter interp = Interpreter();
        // raw cvt 128 at 16ppem / 1024 upem scales to 128 (2px)
        interp.SetControlValues(new int[] { 128 });
        interp.SetPpem(16, 16);
        Zone zone = LineZone(0, 100);
        ExecutionContext ctx = interp.NewContext(new GraphicsState());
        ctx.SetPpem(16);
        ctx.SetGlyphZone(zone);
        // PUSHB[1] 1 0 (point=1 pushed first, cvtIndex=0 on top) ; MIRP[round] (0xE4, round = 0x04)
        interp.Run(ctx, new BytecodeStream(new byte[] { (byte) 0xB1, 1, 0, (byte) 0xE4 }));
        Equal(128, zone.GetCurrentX()[1]);
    }

    [Fact]
    public void TestMsirpSetsExactDistance()
    {
        TrueTypeInterpreter interp = Interpreter();
        Zone zone = LineZone(0, 100);
        ExecutionContext ctx = Context(interp, zone);
        // PUSHB[1] 1 64 (point=1, distance=1px) ; MSIRP[0] (0x3A)
        interp.Run(ctx, new BytecodeStream(new byte[] { (byte) 0xB1, 1, 64, 0x3A }));
        Equal(64, zone.GetCurrentX()[1]); // exactly 1px from rp0 at x=0
    }

    [Fact]
    public void TestAlignRp()
    {
        TrueTypeInterpreter interp = Interpreter();
        Zone zone = LineZone(0, 100); // rp0 at 0
        ExecutionContext ctx = Context(interp, zone);
        // PUSHB[0] 1 ; ALIGNRP (0x3C) - align point 1 onto rp0
        interp.Run(ctx, new BytecodeStream(new byte[] { (byte) 0xB0, 1, 0x3C }));
        Equal(0, zone.GetCurrentX()[1]);
    }

    [Fact]
    public void TestIupInterpolatesUntouched()
    {
        TrueTypeInterpreter interp = Interpreter();
        // three points on one contour; the middle one is untouched
        Zone zone = LineZone(0, 50, 100);
        zone.GetTouchedX()[0] = true;
        zone.GetTouchedX()[2] = true;
        zone.GetCurrentX()[2] = 120; // the right anchor was hinted +20
        ExecutionContext ctx = Context(interp, zone);
        // IUP[1] (x axis)
        interp.Run(ctx, new BytecodeStream(new byte[] { 0x31 }));
        // p1 interpolates proportionally: 0 + 50*(120-0)/100 = 60
        Equal(60, zone.GetCurrentX()[1]);
        // touched anchors are never moved by IUP
        Equal(0, zone.GetCurrentX()[0]);
        Equal(120, zone.GetCurrentX()[2]);
    }

    [Fact]
    public void TestIupShiftsWhenSingleTouchedPoint()
    {
        TrueTypeInterpreter interp = Interpreter();
        Zone zone = LineZone(0, 50, 100);
        zone.GetTouchedX()[1] = true;
        zone.GetCurrentX()[1] = 70; // the only touched point moved +20
        ExecutionContext ctx = Context(interp, zone);
        interp.Run(ctx, new BytecodeStream(new byte[] { 0x31 })); // IUP[1]
        // with one touched point, every other point shifts by the same delta (+20)
        Equal(20, zone.GetCurrentX()[0]);
        Equal(120, zone.GetCurrentX()[2]);
    }

    [Fact]
    public void TestIpInterpolatesBetweenReferencePoints()
    {
        TrueTypeInterpreter interp = Interpreter();
        Zone zone = LineZone(0, 50, 100);
        // set rp1=0, rp2=2, move the anchors, then IP point 1
        zone.GetCurrentX()[2] = 120;
        GraphicsState gs = new GraphicsState();
        gs.SetRp1(0);
        gs.SetRp2(2);
        ExecutionContext ctx = interp.NewContext(gs);
        ctx.SetPpem(16);
        ctx.SetGlyphZone(zone);
        // PUSHB[0] 1 ; IP (0x39)
        interp.Run(ctx, new BytecodeStream(new byte[] { (byte) 0xB0, 1, 0x39 }));
        Equal(60, zone.GetCurrentX()[1]);
    }

    [Fact]
    public void TestIpInterpolatesTwilightPointsUsingScaledOriginals()
    {
        // Regression (PDFBOX-3293): twilight-zone points have no font-unit source, so their unscaled
        // coordinates are (0,0). Measuring IP's interpolation ratio against those zeros collapses every
        // interpolated point onto the reference point. A prep program that builds the x-height control
        // value this way then yields 0, flattening whole glyphs onto the baseline at small ppem (e.g.
        // lowercase 'm' in a gasp-less Arial subset at 7ppem). For twilight points IP must measure the
        // scaled originals instead, exactly as FreeType's Ins_IP does.
        TrueTypeInterpreter interp = Interpreter();
        GraphicsState gs = new GraphicsState();
        gs.SetZp0(0); // all three zone pointers reference the twilight zone
        gs.SetZp1(0);
        gs.SetZp2(0);
        gs.SetRp1(0);
        gs.SetRp2(2);
        ExecutionContext ctx = interp.NewContext(gs);
        ctx.SetPpem(16);
        Zone twilight = ctx.GetTwilightZone();
        // anchors at originals 0 and 100; p2's current is stretched to 120. The unscaled coordinates
        // stay (0,0) for every point, exactly as MIAP leaves freshly placed twilight points.
        int[] xs = { 0, 50, 100 };
        for (int p = 0; p < xs.Length; p++)
        {
            twilight.GetOriginalX()[p] = xs[p];
            twilight.GetCurrentX()[p] = xs[p];
        }
        twilight.GetCurrentX()[2] = 120;
        // PUSHB[0] 1 ; IP (0x39)
        interp.Run(ctx, new BytecodeStream(new byte[] { (byte) 0xB0, 1, 0x39 }));
        // interpolate by the scaled originals: 0 + 50*(120-0)/100 = 60 (the unscaled zeros would give 0)
        Equal(60, twilight.GetCurrentX()[1]);
    }

    [Fact]
    public void TestSvtcaSetsProjectionVector()
    {
        TrueTypeInterpreter interp = Interpreter();
        ExecutionContext ctx = interp.NewContext(new GraphicsState());
        // SVTCA[0] (y axis) ; GPV
        interp.Run(ctx, new BytecodeStream(new byte[] { 0x00, 0x0C }));
        Equal(Fixed.ONE_F2DOT14, ctx.Peek(0)); // pv.y
        Equal(0, ctx.Peek(1));                 // pv.x
    }

    /// <summary>
    /// SPVFS takes the low 16 bits of each operand, sign-extended, and normalizes them (FreeType's
    /// Ins_SPVFS); (0,0) leaves the vector as it was.
    /// </summary>
    [Fact]
    public void TestSpvfsNormalizesSignExtendedOperands()
    {
        TrueTypeInterpreter interp = Interpreter();
        ExecutionContext ctx = interp.NewContext(new GraphicsState());
        // PUSHW[1] 0xFFFF 0x0001 (x = -1 after sign extension, y = 1) ; SPVFS ; GPV
        interp.Run(ctx, new BytecodeStream(new byte[] { (byte) 0xB9, (byte) 0xFF, (byte) 0xFF, 0x00,
                0x01, 0x0A, 0x0C }));
        Equal(11585, ctx.Peek(0));  // pv.y
        Equal(-11585, ctx.Peek(1)); // pv.x
        // PUSHB[1] 0 0 ; SPVFS ; GPV - unchanged
        interp.Run(ctx, new BytecodeStream(new byte[] { (byte) 0xB1, 0, 0, 0x0A, 0x0C }));
        Equal(11585, ctx.Peek(0));
        Equal(-11585, ctx.Peek(1));
    }

    [Fact]
    public void TestRoundOpcode()
    {
        // PUSHB[0] 100 ; ROUND[0] (0x68) -> 128 under default round-to-grid
        ExecutionContext ctx = Interpreter().ExecuteProgram(new byte[] { (byte) 0xB0, 100, 0x68 }, 16);
        Equal(128, ctx.Peek(0));
    }

    [Fact]
    public void TestMdMeasuresSignedDistance()
    {
        // MD measures project(zp0[p1] - zp1[p2]); p1 is the deeper operand, p2 the top (FreeType sign).
        TrueTypeInterpreter interp = Interpreter();
        Zone zone = LineZone(0, 100); // point 0 at x=0, point 1 at x=100
        ExecutionContext ctx = Context(interp, zone);
        // PUSHB[1] 0 1 (p1=0 deeper, p2=1 top) ; MD[grid] (0x49)
        interp.Run(ctx, new BytecodeStream(new byte[] { (byte) 0xB1, 0, 1, 0x49 }));
        Equal(-100, ctx.Peek(0)); // x0 - x1 = -100
    }

    [Fact]
    public void TestGcReadsProjectedCoordinate()
    {
        TrueTypeInterpreter interp = Interpreter();
        Zone zone = LineZone(192); // 3px
        ExecutionContext ctx = Context(interp, zone);
        // PUSHB[0] 0 ; GC[0] (0x46) current coordinate
        interp.Run(ctx, new BytecodeStream(new byte[] { (byte) 0xB0, 0, 0x46 }));
        Equal(192, ctx.Peek(0));
    }

    [Fact]
    public void TestIsectMovesPointToLineIntersection()
    {
        TrueTypeInterpreter interp = Interpreter();
        // p0 = the point to move; line A = horizontal y=64 (p1,p2); line B = vertical x=64 (p3,p4)
        Zone zone = new Zone(5, 1);
        SetPoint(zone, 0, 0, 0);
        SetPoint(zone, 1, 0, 64);
        SetPoint(zone, 2, 128, 64);
        SetPoint(zone, 3, 64, 0);
        SetPoint(zone, 4, 64, 128);
        ExecutionContext ctx = Context(interp, zone);
        // push p0,a0,a1,b0,b1 = 0 1 2 3 4 ; ISECT (0x0F) pops b1,b0,a1,a0,point
        interp.Run(ctx, new BytecodeStream(
                new byte[] { (byte) 0xB4, 0, 1, 2, 3, 4, 0x0F }));
        Equal(64, zone.GetCurrentX()[0]);
        Equal(64, zone.GetCurrentY()[0]);
        Assert.True(zone.GetTouchedX()[0]);
        Assert.True(zone.GetTouchedY()[0]);
    }

    // --- oracle-free invariant checks ------------------------------------

    [Fact]
    public void TestIupNeverMovesATouchedPoint()
    {
        // invariant: IUP leaves every already-touched point exactly where it is
        TrueTypeInterpreter interp = Interpreter();
        Zone zone = LineZone(0, 50, 100);
        zone.GetTouchedX()[1] = true;
        zone.GetCurrentX()[1] = 77; // a touched point at a deliberately off-grid position
        ExecutionContext ctx = Context(interp, zone);
        interp.Run(ctx, new BytecodeStream(new byte[] { 0x31 })); // IUP[x]
        Equal(77, zone.GetCurrentX()[1]);
    }

    [Fact]
    public void TestTouchedPointsLandOnGridUnderRoundToGrid()
    {
        // invariant: under an integer round state, a rounded (touched) point lands on a grid line
        TrueTypeInterpreter interp = Interpreter();
        Zone zone = LineZone(100, 150, 77); // off-grid positions
        ExecutionContext ctx = Context(interp, zone); // default round state is round-to-grid
        // MDAP[1] each point (PUSHB[0] n ; MDAP[1])
        interp.Run(ctx, new BytecodeStream(new byte[] {
                (byte) 0xB0, 0, 0x2F, (byte) 0xB0, 1, 0x2F, (byte) 0xB0, 2, 0x2F }));
        for (int i = 0; i < 3; i++)
        {
            Assert.True(zone.GetTouchedX()[i], "point " + i + " should be touched");
            Equal(0, zone.GetCurrentX()[i] % Fixed.ONE, "point " + i + " off the grid");
        }
    }

    /// <summary>
    /// SPVTL takes the top point from zp2 and the one below it from zp1, and the line runs from the
    /// zp2 point to the zp1 point (FreeType's Ins_SxVTL). Here that is (64,0) to (0,0), so the
    /// projection vector is -x and GC of the point at x=64 reads -64.
    /// </summary>
    [Fact]
    public void TestSpvtlLineRunsFromTopPointToLowerPoint()
    {
        TrueTypeInterpreter interp = Interpreter();
        Zone zone = new Zone(2, 1);
        SetPoint(zone, 0, 64, 0);
        SetPoint(zone, 1, 0, 0);
        ExecutionContext ctx = Context(interp, zone);
        // PUSHB[1] 1 0 (zp1 point 1 below, zp2 point 0 on top) ; SPVTL[0] ; PUSHB[0] 0 ; GC[0]
        interp.Run(ctx, new BytecodeStream(new byte[] { (byte) 0xB1, 1, 0, 0x06, (byte) 0xB0, 0, 0x46 }));
        Equal(-Fixed.ONE_F2DOT14, ctx.GetGraphicsState().GetProjectionVector().GetX());
        Equal(-64, ctx.Peek(0));
    }

    /// <summary>
    /// SPVTL sets the dual projection vector equal to the projection vector; only SDPVTL derives it
    /// from the original outline (FreeType's Ins_SPVTL). The current line here is horizontal while the
    /// original line is vertical, so a dual vector taken from the originals would be the y axis.
    /// </summary>
    [Fact]
    public void TestSpvtlSetsDualVectorToProjectionVector()
    {
        TrueTypeInterpreter interp = Interpreter();
        Zone zone = new Zone(2, 1);
        SetPoint(zone, 0, 0, 0);
        SetPoint(zone, 1, 0, 64);
        zone.GetCurrentX()[1] = 64;
        zone.GetCurrentY()[1] = 0;
        ExecutionContext ctx = Context(interp, zone);
        // PUSHB[1] 1 0 ; SPVTL[0] ; PUSHB[0] 1 ; GC[1] (original, along the dual vector)
        interp.Run(ctx, new BytecodeStream(new byte[] { (byte) 0xB1, 1, 0, 0x06, (byte) 0xB0, 1, 0x47 }));
        UnitVector dual = ctx.GetGraphicsState().GetDualProjectionVector();
        Equal(Fixed.ONE_F2DOT14, dual.GetX());
        Equal(0, dual.GetY());
        Equal(0, ctx.Peek(0)); // original (0,64) projected on the x axis
    }

    /// <summary>
    /// A zero-length line gives the x axis even for the perpendicular form (FreeType's Ins_SxVTL).
    /// </summary>
    [Fact]
    public void TestSpvtlZeroLengthLineIsXAxisEvenWhenPerpendicular()
    {
        TrueTypeInterpreter interp = Interpreter();
        Zone zone = new Zone(1, 1);
        SetPoint(zone, 0, 64, 64);
        ExecutionContext ctx = Context(interp, zone);
        // PUSHB[1] 0 0 ; SPVTL[1] (perpendicular)
        interp.Run(ctx, new BytecodeStream(new byte[] { (byte) 0xB1, 0, 0, 0x07 }));
        UnitVector pv = ctx.GetGraphicsState().GetProjectionVector();
        Equal(Fixed.ONE_F2DOT14, pv.GetX());
        Equal(0, pv.GetY());
    }

    /// <summary>
    /// SDPVTL: when the original line has zero length FreeType drops the perpendicular flag, and the
    /// drop carries over to the current line computed after it, so the projection vector runs along
    /// the current line rather than across it.
    /// </summary>
    [Fact]
    public void TestSdpvtlZeroLengthOriginalLineDropsPerpendicularForCurrentLine()
    {
        TrueTypeInterpreter interp = Interpreter();
        Zone zone = new Zone(2, 1);
        SetPoint(zone, 0, 0, 0);
        SetPoint(zone, 1, 0, 0);
        zone.GetCurrentX()[1] = 64;
        ExecutionContext ctx = Context(interp, zone);
        // PUSHB[1] 1 0 ; SDPVTL[1] (perpendicular)
        interp.Run(ctx, new BytecodeStream(new byte[] { (byte) 0xB1, 1, 0, (byte) 0x87 }));
        UnitVector pv = ctx.GetGraphicsState().GetProjectionVector();
        UnitVector dual = ctx.GetGraphicsState().GetDualProjectionVector();
        Equal(Fixed.ONE_F2DOT14, pv.GetX());
        Equal(0, pv.GetY());
        Equal(Fixed.ONE_F2DOT14, dual.GetX());
        Equal(0, dual.GetY());
    }

    /// <summary>
    /// An interpreter whose cvt[0] scales to 128 (2px) at 16ppem, and a context on the given zone.
    /// </summary>
    private static ExecutionContext ContextWithCvt128(TrueTypeInterpreter interp, Zone glyph)
    {
        interp.SetControlValues(new int[] { 128 });
        interp.SetPpem(16, 16);
        return Context(interp, glyph);
    }

    /// <summary>
    /// MIRP to a twilight point first places it the control value away from rp0's original position
    /// along the freedom vector (undocumented MS rasterizer behaviour, FreeType's Ins_MIRP). Without
    /// that the point's stale origin at 0 would flip the control value to -128.
    /// </summary>
    [Fact]
    public void TestMirpPlacesTwilightPointFromRp0()
    {
        TrueTypeInterpreter interp = Interpreter();
        ExecutionContext ctx = ContextWithCvt128(interp, LineZone(64)); // rp0 = glyph point 0 at 1px
        // PUSHB[0] 0 ; SZP1 (twilight) ; PUSHB[1] 3 0 ; MIRP[0] (no round, no minimum)
        interp.Run(ctx, new BytecodeStream(new byte[] { (byte) 0xB0, 0, 0x14,
                (byte) 0xB1, 3, 0, (byte) 0xE0 }));
        Zone twilight = ctx.GetTwilightZone();
        Equal(192, twilight.GetOriginalX()[3]);
        Equal(192, twilight.GetCurrentX()[3]);
    }

    /// <summary>
    /// MSIRP to a twilight point first places its original position the distance away from rp0's
    /// original position (undocumented MS rasterizer behaviour, FreeType's Ins_MSIRP).
    /// </summary>
    [Fact]
    public void TestMsirpPlacesTwilightPointFromRp0()
    {
        TrueTypeInterpreter interp = Interpreter();
        ExecutionContext ctx = Context(interp, LineZone(64)); // rp0 = glyph point 0 at 1px
        // PUSHB[0] 0 ; SZP1 (twilight) ; PUSHB[1] 3 64 ; MSIRP[0]
        interp.Run(ctx, new BytecodeStream(new byte[] { (byte) 0xB0, 0, 0x14,
                (byte) 0xB1, 3, 64, 0x3A }));
        Zone twilight = ctx.GetTwilightZone();
        Equal(128, twilight.GetOriginalX()[3]);
        Equal(128, twilight.GetCurrentX()[3]);
    }

    /// <summary>
    /// MIAP on a twilight point places its origin at the control value along the freedom vector, not
    /// the projection vector (FreeType's Ins_MIAP). With a diagonal freedom vector and an x-axis
    /// projection the difference shows in the original y.
    /// </summary>
    [Fact]
    public void TestMiapPlacesTwilightPointAlongFreedomVector()
    {
        TrueTypeInterpreter interp = Interpreter();
        ExecutionContext ctx = ContextWithCvt128(interp, LineZone(0));
        int diagonal = 0x2D41; // 1/sqrt(2) in F2Dot14
        // PUSHB[0] 0 ; SZP0 (twilight) ; PUSHW[1] diagonal diagonal ; SFVFS ; PUSHB[1] 3 0 ; MIAP[0]
        interp.Run(ctx, new BytecodeStream(new byte[] { (byte) 0xB0, 0, 0x13,
                (byte) 0xB9, 0x2D, 0x41, 0x2D, 0x41, 0x0B, (byte) 0xB1, 3, 0, 0x3E }));
        Zone twilight = ctx.GetTwilightZone();
        Equal(Fixed.Mul14(128, diagonal), twilight.GetOriginalX()[3]);
        Equal(Fixed.Mul14(128, diagonal), twilight.GetOriginalY()[3]);
        Equal(128, twilight.GetCurrentX()[3]); // then moved along fv until its x projects to 128
    }

    /// <summary>
    /// ISECT on parallel lines (zero discriminant) cannot intersect them; like FreeType it moves the
    /// point to the average of the four line points.
    /// </summary>
    [Fact]
    public void TestIsectParallelLinesUseTheAverageOfTheFourPoints()
    {
        TrueTypeInterpreter interp = Interpreter();
        Zone zone = new Zone(5, 1);
        SetPoint(zone, 0, 0, 0);
        SetPoint(zone, 1, 0, 0);     // line A: y = 0
        SetPoint(zone, 2, 128, 0);
        SetPoint(zone, 3, 0, 64);    // line B: y = 64, parallel to A
        SetPoint(zone, 4, 128, 64);
        ExecutionContext ctx = Context(interp, zone);
        interp.Run(ctx, new BytecodeStream(new byte[] { (byte) 0xB4, 0, 1, 2, 3, 4, 0x0F })); // ISECT
        Equal(64, zone.GetCurrentX()[0]); // (0 + 128 + 0 + 128) / 4
        Equal(32, zone.GetCurrentY()[0]); // (0 + 0 + 64 + 64) / 4
    }

    /// <summary>
    /// ALIGNPTS moves each point by half their projected distance, truncated, towards the other
    /// (FreeType's Ins_ALIGNPTS): 127/64 px apart, each moves 63, leaving them 1/64 px apart.
    /// </summary>
    [Fact]
    public void TestAlignPtsMovesEachPointByHalfTheDistance()
    {
        TrueTypeInterpreter interp = Interpreter();
        Zone zone = LineZone(0, 127);
        ExecutionContext ctx = Context(interp, zone);
        interp.Run(ctx, new BytecodeStream(new byte[] { (byte) 0xB1, 0, 1, 0x27 })); // ALIGNPTS
        Equal(63, zone.GetCurrentX()[0]);
        Equal(64, zone.GetCurrentX()[1]);
    }

    /// <summary>
    /// UTP clears the touch flag only on the axes the freedom vector acts on.
    /// </summary>
    [Fact]
    public void TestUtpUntouchesOnlyTheFreedomAxes()
    {
        TrueTypeInterpreter interp = Interpreter();
        Zone zone = LineZone(0, 64);
        zone.GetTouchedX()[1] = true;
        zone.GetTouchedY()[1] = true;
        ExecutionContext ctx = Context(interp, zone);
        interp.Run(ctx, new BytecodeStream(new byte[] { (byte) 0xB0, 1, 0x29 })); // UTP (fv = x)
        Assert.False(zone.GetTouchedX()[1]);
        Assert.True(zone.GetTouchedY()[1]);
    }

    /// <summary>
    /// FLIPPT toggles the on-curve flag of each point.
    /// </summary>
    [Fact]
    public void TestFlipPtTogglesOnCurve()
    {
        TrueTypeInterpreter interp = Interpreter();
        Zone zone = LineZone(0, 64);
        zone.GetOnCurve()[1] = true;
        ExecutionContext ctx = Context(interp, zone);
        interp.Run(ctx, new BytecodeStream(new byte[] { (byte) 0xB0, 1, (byte) 0x80 })); // FLIPPT
        Assert.False(zone.GetOnCurve()[1]);
        interp.Run(ctx, new BytecodeStream(new byte[] { (byte) 0xB0, 1, (byte) 0x80 }));
        Assert.True(zone.GetOnCurve()[1]);
    }

    /// <summary>
    /// SHC with a contour the zone does not have is ignored, as in FreeType outside pedantic mode.
    /// </summary>
    [Fact]
    public void TestShcIgnoresAnInvalidContour()
    {
        TrueTypeInterpreter interp = Interpreter();
        Zone zone = LineZone(0, 64);
        zone.GetCurrentX()[0] = 32; // rp2 = point 0 moved by 32
        ExecutionContext ctx = Context(interp, zone);
        interp.Run(ctx, new BytecodeStream(new byte[] { (byte) 0xB0, 5, 0x34 })); // SHC[0] contour 5
        Equal(64, zone.GetCurrentX()[1]);
    }

    /// <summary>
    /// In the twilight zone SHC treats all points as one contour (FreeType's Ins_SHC): contour 0
    /// shifts every twilight point by the reference point's movement.
    /// </summary>
    [Fact]
    public void TestShcShiftsTheWholeTwilightZone()
    {
        TrueTypeInterpreter interp = Interpreter();
        Zone zone = LineZone(0, 64);
        zone.GetCurrentX()[0] = 64; // rp1 = glyph point 0, moved by 64
        ExecutionContext ctx = Context(interp, zone);
        // PUSHB[0] 0 ; SZP2 (twilight) ; PUSHB[0] 0 ; SHC[1] (rp1 in zp0 = glyph zone)
        interp.Run(ctx, new BytecodeStream(new byte[] { (byte) 0xB0, 0, 0x15, (byte) 0xB0, 0, 0x35 }));
        Zone twilight = ctx.GetTwilightZone();
        Equal(64, twilight.GetCurrentX()[0]);
        Equal(64, twilight.GetCurrentX()[15]);
    }

    /// <summary>
    /// Under backward compatibility, once IUP has run on both axes further IUPs do nothing (FreeType's
    /// Ins_IUP), so a late y move is not spread over the untouched points.
    /// </summary>
    [Fact]
    public void TestIupDoesNothingOnceBothAxesAreDoneUnderBackwardCompatibility()
    {
        TrueTypeInterpreter interp = Interpreter();
        Zone zone = LineZone(0, 64, 128);
        zone.GetCurrentY()[0] = 64;
        zone.GetTouchedY()[0] = true;
        ExecutionContext ctx = Context(interp, zone);
        ctx.SetBackwardCompatibility(true);
        interp.Run(ctx, new BytecodeStream(new byte[] { 0x30, 0x31 })); // IUP[y], IUP[x]
        Equal(64, zone.GetCurrentY()[1]); // shifted with the single touched point
        zone.GetCurrentY()[0] = 128;
        interp.Run(ctx, new BytecodeStream(new byte[] { 0x30 })); // IUP[y] again: ignored
        Equal(64, zone.GetCurrentY()[1]);
    }

    /// <summary>
    /// An output-of-range point number - an operand, or a reference point set with SRPn - is malformed
    /// font data: it must fail as a HintingException, which falls back to the raw glyph, not
    /// as an ArrayIndexOutOfBoundsException, which would escape to the renderer. The zone has two
    /// points (and the twilight zone 16), so point 99 exists in neither.
    /// </summary>
    [Theory]
    [MemberData(nameof(OutOfRangePointPrograms))]
    public void TestOutOfRangePointIsAHintingException(string name, byte[] program)
    {
        Assert.NotEmpty(name);
        TrueTypeInterpreter interp = Interpreter();
        ExecutionContext ctx = Context(interp, LineZone(0, 64));
        Assert.Throws<HintingException>( () => interp.Run(ctx, new BytecodeStream(program)));
    }

    public static IEnumerable<object[]> OutOfRangePointPrograms()
    {
        byte b1 = (byte) 0xB0; // PUSHB[0], one byte
        byte b2 = (byte) 0xB1; // PUSHB[1], two bytes
        byte b5 = (byte) 0xB4; // PUSHB[4], five bytes
        return new object[][] {
                new object[] {"MDAP", new byte[] { b1, 99, 0x2E } },
                new object[] {"MIAP", new byte[] { b2, 99, 0, 0x3E } },
                new object[] {"MSIRP", new byte[] { b2, 99, 64, 0x3A } },
                new object[] {"MDRP", new byte[] { b1, 99, (byte) 0xC0 } },
                new object[] {"MIRP", new byte[] { b2, 99, 0, (byte) 0xE0 } },
                new object[] {"ALIGNRP", new byte[] { b1, 99, 0x3C } },
                new object[] {"ALIGNPTS", new byte[] { b2, 0, 99, 0x27 } },
                new object[] {"UTP", new byte[] { b1, 99, 0x29 } },
                new object[] {"SHPIX", new byte[] { b2, 99, 64, 0x38 } },
                new object[] {"SHP", new byte[] { b1, 99, 0x32 } },
                new object[] {"IP", new byte[] { b1, 99, 0x39 } },
                new object[] {"GC", new byte[] { b1, 99, 0x46 } },
                new object[] {"SCFS", new byte[] { b2, 99, 0, 0x48 } },
                new object[] {"MD", new byte[] { b2, 0, 99, 0x49 } },
                new object[] {"ISECT", new byte[] { b5, 99, 0, 1, 0, 1, 0x0F } },
                new object[] {"SPVTL", new byte[] { b2, 0, 99, 0x06 } },
                new object[] {"FLIPPT", new byte[] { b1, 99, (byte) 0x80 } },
                new object[] {"SRP0 then MDRP", new byte[] { b1, 99, 0x10, b1, 1, (byte) 0xC0 } },
                new object[] {"SRP1 then IP", new byte[] { b1, 99, 0x11, b1, 1, 0x39 } },
                new object[] {"SRP2 then SHP", new byte[] { b1, 99, 0x12, b1, 1, 0x32 }} };
    }

    private static void SetPoint(Zone zone, int i, int x, int y)
    {
        zone.GetCurrentX()[i] = x;
        zone.GetCurrentY()[i] = y;
        zone.GetOriginalX()[i] = x;
        zone.GetOriginalY()[i] = y;
        zone.GetUnscaledX()[i] = x;
        zone.GetUnscaledY()[i] = y;
    }
    private static void Equal<T>(T expected, T actual, string? message = null)
    {
        Assert.Equal(expected, actual);
    }

    private static void AssertTimeoutPreemptively(TimeSpan timeout, Action action)
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();
        action();
        Assert.True(timer.Elapsed < timeout, $"Execution exceeded {timeout}.");
    }

}
