/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/test/java/org/apache/fontbox/ttf/UnitVectorTest.java
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
/// UnitVector#normalize: the vectors steer every diagonal move, and a one-unit difference in
/// F2Dot14 shifts points by 1/64 px, so the results are checked against the unit vectors FreeType
/// produces for the same deltas.
/// </summary>
public class UnitVectorTest
{
    /// <summary>
    /// {dx, dy, x, y}: the expected values are the unit vectors FreeType 2.13.2 computes for these
    /// deltas (its output, measured offline). Nearly axis-aligned vectors show the truncation to
    /// F2Dot14: (1000, -7) gives (16383, -114), not the rounded (16384, -115).
    /// </summary>
    private static readonly int[][] FREETYPE = {
        new int[] { 3, 4, 9830, 13107 },
        new int[] { 1, 1, 11585, 11585 },
        new int[] { -64, 128, -7327, 14654 },
        new int[] { 1000, -7, 16383, -114 },
        new int[] { 7, -1000, 114, -16383 },
        new int[] { -5, -5, -11585, -11585 },
        new int[] { 30000, 1, 16384, 0 },
        new int[] { 1, 2, 7327, 14654 },
    };

    [Fact]
    public void TestNormalizeMatchesFreeType()
    {
        foreach (int[] c in FREETYPE)
        {
            UnitVector v = UnitVector.Normalize(c[0], c[1]);
            Equal(c[2], v.GetX(), "x of (" + c[0] + "," + c[1] + ")");
            Equal(c[3], v.GetY(), "y of (" + c[0] + "," + c[1] + ")");
        }
    }

    [Fact]
    public void TestNormalizeAxisAndZero()
    {
        Equal(Fixed.ONE_F2DOT14, UnitVector.Normalize(5, 0).GetX());
        Equal(-Fixed.ONE_F2DOT14, UnitVector.Normalize(0, -5).GetY());
        Equal(Fixed.ONE_F2DOT14, UnitVector.Normalize(0, 0).GetX());
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
