/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/test/java/org/apache/fontbox/ttf/HintingSmokeTest.java
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

public class HintingSmokeTest : HintingTestSupport
{
    [Theory]
    [InlineData("LiberationSans-Regular.ttf")]
    [InlineData("JosefinSans-Italic.ttf")]
    [InlineData("Lohit-Bengali.ttf")]
    [InlineData("Lohit-Devanagari.ttf")]
    [InlineData("Lohit-Gujarati.ttf")]
    [InlineData("Lohit-Tamil.ttf")]
    public void EveryGlyphHintsWithoutFailure(string fixture)
    {
        CheckFont(Fixture(fixture));
    }

    [Theory]
    [InlineData("DejaVuSansMono.ttf")]
    [InlineData("Keyboard.ttf")]
    [InlineData("NotoEmoji-Regular.ttf")]
    [InlineData("NotoMono-Regular.ttf")]
    [InlineData("ipag00303/ipag.ttf")]
    public void DownloadedUpstreamFontsHintWithoutFailure(string relativePath)
    {
        string? directory = Environment.GetEnvironmentVariable("PDFBOX_FONTBOX_TEST_FONTS");
        Assert.SkipWhen(directory == null, "Set PDFBOX_FONTBOX_TEST_FONTS to the upstream fontbox/target/fonts directory.");
        string path = Path.Combine(directory!, relativePath);
        Assert.True(File.Exists(path), $"Upstream smoke fixture missing: {path}");
        CheckFont(path);
    }

    private static void CheckFont(string path)
    {
        using TrueTypeFont font = new TTFParser(true).Parse(File.ReadAllBytes(path));
        GlyphHinter hinter = new(font);
        Assert.True(font.GetNumberOfGlyphs() > 0);
        foreach (int ppem in new[] { 9, 12, 16, 24, 48 })
        {
            for (int gid = 0; gid < font.GetNumberOfGlyphs(); gid++) hinter.GetHintedPointsF26Dot6(gid, ppem);
        }
        Assert.True(hinter.GetFailureCount() == 0, hinter.GetFirstFailure());
        Assert.False(hinter.IsFullControl());
    }
}
