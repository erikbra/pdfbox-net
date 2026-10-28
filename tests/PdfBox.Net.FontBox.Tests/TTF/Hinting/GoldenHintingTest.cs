/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/test/java/org/apache/fontbox/ttf/GoldenHintingTest.java
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

public class GoldenHintingTest : HintingTestSupport
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HintedPointsMatchFreeTypeExactly(bool composite)
    {
        using TrueTypeFont font = Parse("LiberationSans-Regular.ttf");
        GlyphHinter hinter = new(font);
        int compared = 0;
        foreach (int ppem in new[] { 11, 13, 16, 24 })
        {
            foreach (var glyph in ReadGolden(ppem))
            {
                var data = Assert.IsType<GlyphData>(font.GetGlyph()!.GetGlyph(glyph.gid));
                if ((data.NumberOfContours < 0) != composite) continue;
                if (composite)
                {
                    var description = Assert.IsType<GlyfCompositeDescript>(data.Description);
                    Assert.NotEmpty(description.GetInstructions());
                }
                int[][] points = Assert.IsType<int[][]>(hinter.GetHintedPointsF26Dot6(glyph.gid, ppem));
                Assert.Equal(glyph.x.Length, points[0].Length);
                Assert.Equal(glyph.x, points[0]);
                Assert.Equal(glyph.y, points[1]);
                compared += glyph.x.Length;
            }
        }
        Assert.True(compared > 0);
        Assert.Equal(0, hinter.GetFailureCount());
    }

    private static IEnumerable<(int gid, int[] x, int[] y)> ReadGolden(int ppem)
    {
        int gid = -1;
        int[] x = [];
        foreach (string line in File.ReadLines(Fixture($"hinting/LiberationSans-Regular-{ppem}.txt")))
        {
            if (line.StartsWith("glyph ", StringComparison.Ordinal)) gid = int.Parse(line.Split(' ')[1], System.Globalization.CultureInfo.InvariantCulture);
            else if (line.StartsWith("x ", StringComparison.Ordinal)) x = ParseInts(line[2..]);
            else if (line.StartsWith("y ", StringComparison.Ordinal)) yield return (gid, x, ParseInts(line[2..]));
        }
    }

    private static int[] ParseInts(string text) => text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .Select(value => int.Parse(value, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
}
