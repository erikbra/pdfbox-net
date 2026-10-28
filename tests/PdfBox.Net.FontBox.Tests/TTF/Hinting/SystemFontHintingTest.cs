/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/test/java/org/apache/fontbox/ttf/SystemFontHintingTest.java
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
using PdfBox.Net.FontBox.TTF;

namespace PdfBox.Net.FontBox.Tests.TTF.Hinting;

public class SystemFontHintingTest : HintingTestSupport
{
    private static string? ArialPath => new[] { "c:/windows/fonts/arial.ttf",
        "/usr/share/fonts/truetype/msttcorefonts/arial.ttf", "/usr/share/fonts/truetype/msttcorefonts/Arial.ttf",
        "/Library/Fonts/Arial.ttf", "/System/Library/Fonts/Supplemental/Arial.ttf" }.FirstOrDefault(File.Exists);
    public static bool ArialInstalled => ArialPath != null;

    [Fact(Skip = "Arial not installed", SkipUnless = nameof(ArialInstalled))]
    public void ArialNIsHintedWithoutFailure()
    {
        using TrueTypeFont font = new TTFParser().Parse(File.ReadAllBytes(ArialPath!));
        const int gid = 81, ppem = 13;
        Assert.Equal(gid, font.GetUnicodeCmapLookup()!.GetGlyphId('n'));
        Assert.NotNull(font.GetHintedPath(gid, ppem));
        GlyphHinter hinter = new(font);
        int[][] points = Assert.IsType<int[][]>(hinter.GetHintedPointsF26Dot6(gid, ppem));
        Assert.Equal(0, hinter.GetFailureCount());
        Assert.Equal(0, points[1].Min());
        Assert.Equal(0, points[1].Max() % 64);
        if (font.GetHeader()!.GetCheckSumAdjustment() == 0x7b86e499L)
        {
            Assert.Equal(new[] { 55, 55, 121, 121, 168, 258, 297, 362, 394, 401, 405, 405, 405, 332, 332, 332, 315, 272,
                242, 196, 128, 128, 128 }, points[0]);
            Assert.Equal(new[] { 0, 448, 448, 387, 448, 448, 448, 420, 373, 342, 321, 269, 0, 0, 267, 312, 357, 384,
                384, 384, 324, 240, 0 }, points[1]);
        }
    }
}
