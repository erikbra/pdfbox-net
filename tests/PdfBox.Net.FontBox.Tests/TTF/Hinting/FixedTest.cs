/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/test/java/org/apache/fontbox/ttf/FixedTest.java
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
/// Unit tests for the Fixed fixed-point math used by the interpreter.
/// </summary>
public class FixedTest
{
    [Fact]
    public void TestIntRoundTrip()
    {
        for (int n = -1000; n <= 1000; n++)
        {
            Equal(n, Fixed.ToInt(Fixed.FromInt(n)), "round-trip " + n);
        }
        Equal(64, Fixed.FromInt(1));
        Equal(-128, Fixed.FromInt(-2));
    }

    [Fact]
    public void TestFloorCeilRound()
    {
        Equal(64, Fixed.Floor(100));   // 1.5625px -> 1px
        Equal(128, Fixed.Ceil(100));   // -> 2px
        Equal(64, Fixed.Round(70));    // just above 1px rounds to 1px
        Equal(128, Fixed.Round(96));   // 1.5px rounds up to 2px
        Equal(0, Fixed.Round(31));     // just below half a pixel rounds to 0
        Equal(64, Fixed.Round(32));    // exactly half rounds up
    }

    /// <summary>
    /// Floor and ceiling of negative values: &amp; ~63 rounds towards negative infinity in two's
    /// complement, so (v + 63) &amp; ~63 is a true ceiling for every sign, exactly FreeType's
    /// FT_PIX_CEIL_LONG (used by the CEILING opcode).
    /// </summary>
    [Fact]
    public void TestFloorCeilNegative()
    {
        Equal(0, Fixed.Ceil(-1));
        Equal(0, Fixed.Ceil(-63));
        Equal(-64, Fixed.Ceil(-64));
        Equal(-64, Fixed.Ceil(-65));
        Equal(-64, Fixed.Ceil(-127));
        Equal(-128, Fixed.Ceil(-128));
        Equal(-64, Fixed.Floor(-1));
        Equal(-64, Fixed.Floor(-64));
        Equal(-128, Fixed.Floor(-65));
    }

    [Fact]
    public void TestMulDiv()
    {
        // 2.0 * 3.0 == 6.0
        Equal(Fixed.FromInt(6), Fixed.Mul(Fixed.FromInt(2), Fixed.FromInt(3)));
        // 6.0 / 2.0 == 3.0
        Equal(Fixed.FromInt(3), Fixed.Div(Fixed.FromInt(6), Fixed.FromInt(2)));
        // division by zero is defined as zero
        Equal(0, Fixed.Div(Fixed.FromInt(5), 0));
        // signed rounding
        Equal(-Fixed.FromInt(6), Fixed.Mul(Fixed.FromInt(-2), Fixed.FromInt(3)));
    }

    [Fact]
    public void TestMul14()
    {
        // multiplying by the F2Dot14 unit (1.0) is the identity
        Equal(Fixed.FromInt(5), Fixed.Mul14(Fixed.FromInt(5), Fixed.ONE_F2DOT14));
        // multiplying by 0.5 in F2Dot14 halves the value
        Equal(Fixed.FromInt(5) / 2, Fixed.Mul14(Fixed.FromInt(5), Fixed.ONE_F2DOT14 / 2));
    }

    [Fact]
    public void TestScale()
    {
        // 1000 font units at 16 ppem with unitsPerEm 2048 == 500 subpixel units == 7.8125px
        Equal(500, Fixed.Scale(1000, 16, 2048));
        // a flat *64 would be wrong: it must depend on unitsPerEm
        Equal(Fixed.FromInt(16), Fixed.Scale(2048, 16, 2048)); // one em == ppem pixels
        Equal(0, Fixed.Scale(1234, 16, 0)); // guard against zero unitsPerEm
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
