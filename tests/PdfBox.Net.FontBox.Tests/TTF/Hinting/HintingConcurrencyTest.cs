/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/test/java/org/apache/fontbox/ttf/HintingConcurrencyTest.java
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

public class HintingConcurrencyTest : HintingTestSupport
{
    [Fact]
    public async Task ConcurrentHintingMatchesSingleThreadedResults()
    {
        const string glyphs = "HILEToxn";
        int[] ppems = [11, 13, 16, 24];
        using TrueTypeFont reference = Parse("LiberationSans-Regular.ttf");
        int[] gids = glyphs.Select(c => reference.GetUnicodeCmapLookup()!.GetGlyphId(c)).ToArray();
        Dictionary<(int glyph, int ppem), double[]> expected = [];
        for (int g = 0; g < gids.Length; g++)
        {
            foreach (int ppem in ppems) expected[(g, ppem)] = Flatten(reference.GetHintedPath(gids[g], ppem));
        }
        using TrueTypeFont shared = Parse("LiberationSans-Regular.ttf");
        using ManualResetEventSlim start = new(false);
        Task[] tasks = Enumerable.Range(0, 8).Select(offset => Task.Run(() =>
        {
            start.Wait(TestContext.Current.CancellationToken);
            for (int i = 0; i < 150; i++)
            {
                int g = (i + offset) % gids.Length;
                int ppem = ppems[(i + offset) % ppems.Length];
                Assert.Equal(expected[(g, ppem)], Flatten(shared.GetHintedPath(gids[g], ppem)));
            }
        }, TestContext.Current.CancellationToken)).ToArray();
        start.Set();
        await Task.WhenAll(tasks).WaitAsync(TimeSpan.FromSeconds(120), TestContext.Current.CancellationToken);
    }
}
