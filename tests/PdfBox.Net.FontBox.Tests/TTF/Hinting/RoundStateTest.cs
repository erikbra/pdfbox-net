/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/test/java/org/apache/fontbox/ttf/RoundStateTest.java
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
/// Table-driven tests of the GraphicsState#round(int) state machine across all round states.
/// </summary>
public class RoundStateTest
{
    private static int Round(int state, int distance)
    {
        GraphicsState gs = new GraphicsState();
        gs.SetRoundState(state);
        return gs.Round(distance);
    }

    [Fact]
    public void TestRoundToGrid()
    {
        Equal(128, Round(GraphicsState.ROUND_TO_GRID, 100)); // 1.56px -> 2px
        Equal(64, Round(GraphicsState.ROUND_TO_GRID, 95));   // 1.48px -> 1px
        Equal(128, Round(GraphicsState.ROUND_TO_GRID, 96));  // 1.5px rounds up
        Equal(-128, Round(GraphicsState.ROUND_TO_GRID, -100));
    }

    [Fact]
    public void TestRoundDownAndUp()
    {
        Equal(64, Round(GraphicsState.ROUND_DOWN_TO_GRID, 100));
        Equal(64, Round(GraphicsState.ROUND_DOWN_TO_GRID, 127));
        Equal(128, Round(GraphicsState.ROUND_UP_TO_GRID, 65));
        Equal(64, Round(GraphicsState.ROUND_UP_TO_GRID, 64));
    }

    [Fact]
    public void TestRoundOff()
    {
        Equal(100, Round(GraphicsState.ROUND_OFF, 100));
        Equal(-37, Round(GraphicsState.ROUND_OFF, -37));
    }

    [Fact]
    public void TestRoundToDoubleGrid()
    {
        // double grid snaps to multiples of half a pixel (32)
        Equal(96, Round(GraphicsState.ROUND_TO_DOUBLE_GRID, 80));  // 1.25px -> 1.5px
        Equal(64, Round(GraphicsState.ROUND_TO_DOUBLE_GRID, 70));  // 1.09px -> 1.0px
    }

    [Fact]
    public void TestRoundToHalfGrid()
    {
        // half grid snaps to (n + 0.5) pixels, i.e. 32, 96, 160, ...
        Equal(96, Round(GraphicsState.ROUND_TO_HALF_GRID, 100)); // -> 1.5px
        Equal(32, Round(GraphicsState.ROUND_TO_HALF_GRID, 50));  // -> 0.5px
    }

    [Fact]
    public void TestSuperRoundReducesToGrid()
    {
        // SROUND with period=1px, phase=0, threshold=half is equivalent to round-to-grid
        GraphicsState gs = new GraphicsState();
        gs.SetSuperRound(0x4000, 0x48); // 01 period=1.0, 00 phase=0, 1000 threshold=4*p/8=half
        Equal(128, gs.Round(100));
        Equal(64, gs.Round(95));
    }
    /// <summary>
    /// Every mode mirrors about zero the way FreeType's Round_* functions do: a negative distance is
    /// rounded as -round(-d), and a result that would cross zero is clamped to zero (or to
    /// the phase for half-grid). Stems measured right-to-left produce negative distances, so this is
    /// not an edge case.
    /// </summary>
    [Fact]
    public void TestNegativeDistancesMirrorFreeType()
    {
        Equal(-64, Round(GraphicsState.ROUND_TO_GRID, -95));
        Equal(-128, Round(GraphicsState.ROUND_TO_GRID, -96));
        Equal(0, Round(GraphicsState.ROUND_TO_GRID, -31));
        Equal(-64, Round(GraphicsState.ROUND_DOWN_TO_GRID, -127)); // toward zero
        Equal(0, Round(GraphicsState.ROUND_DOWN_TO_GRID, -63));
        Equal(-128, Round(GraphicsState.ROUND_UP_TO_GRID, -65));  // away from zero
        Equal(-64, Round(GraphicsState.ROUND_UP_TO_GRID, -1));
        Equal(-96, Round(GraphicsState.ROUND_TO_DOUBLE_GRID, -80));
        Equal(-64, Round(GraphicsState.ROUND_TO_DOUBLE_GRID, -70));
        Equal(-32, Round(GraphicsState.ROUND_TO_DOUBLE_GRID, -20));
        Equal(-96, Round(GraphicsState.ROUND_TO_HALF_GRID, -100));
        Equal(-32, Round(GraphicsState.ROUND_TO_HALF_GRID, -50));
        Equal(-32, Round(GraphicsState.ROUND_TO_HALF_GRID, -1));  // floor(1)+32, negated
    }

    /// <summary>
    /// Zero and exact grid values are fixed points of every mode.
    /// </summary>
    [Fact]
    public void TestGridValuesAreFixedPoints()
    {
        foreach (int state in new int[] { GraphicsState.ROUND_TO_GRID, GraphicsState.ROUND_DOWN_TO_GRID,
                GraphicsState.ROUND_UP_TO_GRID, GraphicsState.ROUND_TO_DOUBLE_GRID })
        {
            Equal(0, Round(state, 0), "state " + state);
            Equal(128, Round(state, 128), "state " + state);
            Equal(-128, Round(state, -128), "state " + state);
        }
        Equal(32, Round(GraphicsState.ROUND_TO_HALF_GRID, 0));
        Equal(96, Round(GraphicsState.ROUND_TO_HALF_GRID, 64));
    }

    /// <summary>
    /// S45ROUND parameters are derived in F2Dot14 and shifted, as FreeType does: with the diagonal
    /// period 0x2D41 the "period - 1" threshold is (0x2D41 - 1) &gt;&gt; 8 = 45, not (45 - 1) = 44.
    /// </summary>
    [Fact]
    public void TestSuperRound45Parameters()
    {
        GraphicsState gs = new GraphicsState();
        gs.SetSuperRound(0x2D41, 0x40); // period = diagonal, phase 0, threshold = period - 1
        // period 45, threshold 45: any positive distance rounds up to the next multiple of 45
        Equal(45, gs.Round(1));
        Equal(90, gs.Round(46));
        Equal(-45, gs.Round(-1));

        gs.SetSuperRound(0x2D41, 0x58); // period = diagonal, phase = period/4 = 11, threshold = half
        Equal(11, gs.Round(0));
        Equal(56, gs.Round(40)); // 45 + 11
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
