/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/test/java/org/apache/fontbox/ttf/BackwardCompatibilityMoveTest.java
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
/// Pins the v40 "backward compatibility" movement rules in ExecutionContext#movePoint, which
/// are a transcription of FreeType's Direct_Move under TT_SUPPORT_SUBPIXEL_HINTING_MINIMAL:
/// &lt;ul&gt;
///   &lt;li&gt;with the flag off, movePoint is the plain spec move on both axes;&lt;/li&gt;
///   &lt;li&gt;with the flag on, x moves are suppressed (the point is still marked touched, so IUP treats it
///       as a reference) and y moves are allowed until IUP has run on both axes, after which the glyph
///       is frozen.&lt;/li&gt;
/// &lt;/ul&gt;
/// The flag is only ever set for the glyph program (GlyphHinter.runProgram); fpgm/prep run
/// with it off.
/// </summary>
public class BackwardCompatibilityMoveTest
{
    private const int MOVE = 100; // 1.56 px

    private static ExecutionContext Context(bool backwardCompatibility)
    {
        TrueTypeInterpreter interp = new TrueTypeInterpreter(32, 8, 0, 1000);
        ExecutionContext ctx = interp.NewContext(new GraphicsState());
        Zone zone = new Zone(2, 1);
        zone.GetContourEnds()[0] = 1;
        ctx.SetGlyphZone(zone);
        ctx.SetBackwardCompatibility(backwardCompatibility);
        return ctx;
    }

    private static void FreedomY(ExecutionContext ctx)
    {
        ctx.GetGraphicsState().GetFreedomVector().Set(0, Fixed.ONE_F2DOT14);
        ctx.GetGraphicsState().GetProjectionVector().Set(0, Fixed.ONE_F2DOT14);
    }

    [Fact]
    public void TestFlagOffIsThePlainMove()
    {
        ExecutionContext ctx = Context(false);
        Zone zone = ctx.GetGlyphZone();
        ctx.MovePoint(zone, 0, MOVE);
        Equal(MOVE, zone.GetCurrentX()[0]);
        Assert.True(zone.GetTouchedX()[0]);
        Assert.False(zone.GetTouchedY()[0]);

        FreedomY(ctx);
        ctx.MovePoint(zone, 1, MOVE);
        Equal(MOVE, zone.GetCurrentY()[1]);
        Assert.True(zone.GetTouchedY()[1]);

        // IUP on both axes does not freeze anything when the flag is off
        ctx.SetIupxCalled();
        ctx.SetIupyCalled();
        ctx.MovePoint(zone, 1, MOVE);
        Equal(2 * MOVE, zone.GetCurrentY()[1]);
    }

    [Fact]
    public void TestFlagOnSuppressesXButStillTouches()
    {
        ExecutionContext ctx = Context(true);
        Zone zone = ctx.GetGlyphZone();
        ctx.MovePoint(zone, 0, MOVE);
        Equal(0, zone.GetCurrentX()[0], "x must not be grid-fit under backward compatibility");
        Assert.True(zone.GetTouchedX()[0], "the point still counts as touched for IUP");
    }

    [Fact]
    public void TestFlagOnAllowsYUntilIupHasRunOnBothAxes()
    {
        ExecutionContext ctx = Context(true);
        Zone zone = ctx.GetGlyphZone();
        FreedomY(ctx);

        ctx.MovePoint(zone, 0, MOVE);
        Equal(MOVE, zone.GetCurrentY()[0], "y moves are allowed before IUP");

        ctx.SetIupxCalled(); // only one axis so far: still allowed
        ctx.MovePoint(zone, 0, MOVE);
        Equal(2 * MOVE, zone.GetCurrentY()[0]);

        ctx.SetIupyCalled(); // both axes: frozen
        ctx.MovePoint(zone, 0, MOVE);
        Equal(2 * MOVE, zone.GetCurrentY()[0], "y is frozen once IUP ran on both axes");
        Assert.True(zone.GetTouchedY()[0]);
    }

    /// <summary>
    /// A diagonal freedom vector is gated per axis: its x part is dropped, its y part applied.
    /// </summary>
    [Fact]
    public void TestDiagonalFreedomVectorIsGatedPerAxis()
    {
        ExecutionContext ctx = Context(true);
        Zone zone = ctx.GetGlyphZone();
        int d = (int) Math.Round(Fixed.ONE_F2DOT14 / Math.Sqrt(2));
        ctx.GetGraphicsState().GetFreedomVector().Set(d, d);
        ctx.GetGraphicsState().GetProjectionVector().Set(d, d);
        ctx.MovePoint(zone, 0, MOVE);
        Equal(0, zone.GetCurrentX()[0]);
        Assert.True(zone.GetCurrentY()[0] > 0);
        Assert.True(zone.GetTouchedX()[0]);
        Assert.True(zone.GetTouchedY()[0]);
    }
    /// <summary>
    /// SHPIX under backward compatibility is gated like DELTAP (FreeType Ins_SHPIX): a glyph-zone
    /// point moves in y only if it was already touched in y and IUP has not run; twilight points
    /// always move. With the flag off SHPIX is unconditional.
    /// </summary>
    [Fact]
    public void TestShpixIsGatedLikeDeltap()
    {
        TrueTypeInterpreter interp = new TrueTypeInterpreter(32, 8, 4, 1000);
        byte[] shpixPoint0 = { 0x00 /* SVTCA[0] y */, (byte) 0xB1, 0, 100, 0x38 /* SHPIX */ };

        // flag off: moves
        ExecutionContext ctx = interp.NewContext(new GraphicsState());
        Zone zone = new Zone(2, 1);
        zone.GetContourEnds()[0] = 1;
        ctx.SetGlyphZone(zone);
        interp.Run(ctx, new BytecodeStream(shpixPoint0));
        Equal(100, zone.GetCurrentY()[0]);

        // flag on, point untouched: blocked
        ctx = interp.NewContext(new GraphicsState());
        zone = new Zone(2, 1);
        zone.GetContourEnds()[0] = 1;
        ctx.SetGlyphZone(zone);
        ctx.SetBackwardCompatibility(true);
        interp.Run(ctx, new BytecodeStream(shpixPoint0));
        Equal(0, zone.GetCurrentY()[0], "untouched point must not be moved by SHPIX");

        // flag on, point touched in y: moves
        zone.GetTouchedY()[0] = true;
        interp.Run(ctx, new BytecodeStream(shpixPoint0));
        Equal(100, zone.GetCurrentY()[0]);

        // flag on, after IUP on both axes: blocked even though touched
        ctx.SetIupxCalled();
        ctx.SetIupyCalled();
        interp.Run(ctx, new BytecodeStream(shpixPoint0));
        Equal(100, zone.GetCurrentY()[0]);

        // flag on, twilight zone: always moves (SZP2 0 first)
        ctx = interp.NewContext(new GraphicsState());
        ctx.SetGlyphZone(zone);
        ctx.SetBackwardCompatibility(true);
        byte[] twilight = { 0x00, (byte) 0xB0, 0, 0x15 /* SZP2 */, (byte) 0xB1, 1, 100, 0x38 };
        interp.Run(ctx, new BytecodeStream(twilight));
        Equal(100, ctx.GetTwilightZone().GetCurrentY()[1]);
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
