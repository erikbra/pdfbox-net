/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/test/java/org/apache/fontbox/ttf/GraphicsStateTest.java
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
/// Tests for GraphicsState defaults, deep copy and per-glyph reset: a shallow copy would let a
/// glyph write through to the saved post-prep template, and resetting too much or too little
/// would discard or retain state the TrueType spec is specific about.
/// </summary>
public class GraphicsStateTest
{
    [Fact]
    public void TestDefaults()
    {
        GraphicsState gs = new GraphicsState();
        Equal(UnitVector.XAxis(), gs.GetProjectionVector());
        Equal(UnitVector.XAxis(), gs.GetFreedomVector());
        Equal(GraphicsState.ROUND_TO_GRID, gs.GetRoundState());
        Equal(1, gs.GetLoop());
        Equal(Fixed.ONE, gs.GetMinimumDistance());
        Equal(17 * Fixed.ONE / 16, gs.GetControlValueCutIn());
        Equal(9, gs.GetDeltaBase());
        Equal(3, gs.GetDeltaShift());
        Equal(1, gs.GetZp0());
        Equal(0, gs.GetRp0());
        Assert.True(gs.IsAutoFlip());
    }

    [Fact]
    public void TestDeepCopyIsIndependent()
    {
        GraphicsState original = new GraphicsState();
        GraphicsState clone = original.Copy();

        // the vector objects must not be shared (bug #2 guard)
        Assert.NotSame(original.GetFreedomVector(), clone.GetFreedomVector());

        // mutating the clone's vector in place must not touch the original
        clone.GetFreedomVector().Set(0, Fixed.ONE_F2DOT14);
        clone.SetRp0(5);
        clone.SetRoundState(GraphicsState.ROUND_OFF);

        Equal(UnitVector.XAxis(), original.GetFreedomVector());
        Equal(0, original.GetRp0());
        Equal(GraphicsState.ROUND_TO_GRID, original.GetRoundState());
    }

    [Fact]
    public void TestResetForGlyphResetsOnlySpecMandatedFields()
    {
        GraphicsState gs = new GraphicsState();

        // simulate state left behind by prep / a previous glyph
        gs.GetFreedomVector().Set(0, Fixed.ONE_F2DOT14);
        gs.GetProjectionVector().Set(0, Fixed.ONE_F2DOT14);
        gs.SetRp0(3);
        gs.SetRp1(4);
        gs.SetRp2(5);
        gs.SetZp0(0);
        gs.SetLoop(7);
        // prep-configured fields that must survive a per-glyph reset
        gs.SetRoundState(GraphicsState.ROUND_OFF);
        gs.SetControlValueCutIn(999);
        gs.SetMinimumDistance(123);
        gs.SetDeltaBase(42);

        gs.ResetForGlyph();

        // reset to defaults
        Equal(UnitVector.XAxis(), gs.GetFreedomVector());
        Equal(UnitVector.XAxis(), gs.GetProjectionVector());
        Equal(0, gs.GetRp0());
        Equal(0, gs.GetRp1());
        Equal(0, gs.GetRp2());
        Equal(1, gs.GetZp0());
        Equal(1, gs.GetLoop());

        // preserved from prep
        Equal(GraphicsState.ROUND_OFF, gs.GetRoundState());
        Equal(999, gs.GetControlValueCutIn());
        Equal(123, gs.GetMinimumDistance());
        Equal(42, gs.GetDeltaBase());
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
