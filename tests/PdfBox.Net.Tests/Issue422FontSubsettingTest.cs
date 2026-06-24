/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Focused coverage for issue #422 TrueType/CID subsetting wiring.
 *
 * PORT_MODE: native-test
 */

using PdfBox.Net.FontBox.TTF;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Font.Encoding;

namespace PdfBox.Net.Tests;

public class Issue422FontSubsettingTest
{
    [Fact]
    public void PDTrueTypeFontEmbedder_SubsetBuildsParseableSubset()
    {
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());
        PDTrueTypeFontEmbedder embedder = new(WinAnsiEncoding.INSTANCE, ttf);

        embedder.AddToSubset('A');
        embedder.Subset();

        byte[]? subsetBytes = embedder.GetSubsetBytes();
        Assert.NotNull(subsetBytes);
        TrueTypeFont subset = new TTFParser().Parse(subsetBytes);

        Assert.Equal(7, embedder.GetSubsetTag()!.Length);
        Assert.EndsWith("+", embedder.GetSubsetTag());
        Assert.True(subsetBytes.Length > 0);
        Assert.True(subset.GetNumberOfGlyphs() >= 2);
        Assert.Contains(embedder.GetGidToCidMap()!, entry => entry.Key == 0 && entry.Value == 0);
        Assert.Contains(embedder.GetGidToCidMap()!, entry => entry.Value == 1);
    }

    [Fact]
    public void PDCIDFontType2Embedder_SubsetBuildsParseableSubset()
    {
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());
        PDCIDFontType2Embedder embedder = new(ttf);

        embedder.AddToSubset('A');
        embedder.Subset();

        byte[]? subsetBytes = embedder.GetSubsetBytes();
        Assert.NotNull(subsetBytes);
        TrueTypeFont subset = new TTFParser().Parse(subsetBytes);

        Assert.Equal(embedder.GetSubsetTag(), embedder.GetTag(new Dictionary<int, int>(embedder.GetGidToCidMap()!)));
        Assert.True(subset.GetNumberOfGlyphs() >= 2);
        Assert.Contains(embedder.GetGidToCidMap()!, entry => entry.Value == 1);
    }

    [Fact]
    public void TrueTypeEmbedder_SubsetThrowsWhenDisabled()
    {
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());
        PDTrueTypeFontEmbedder embedder = new(WinAnsiEncoding.INSTANCE, ttf, embedSubset: false);

        embedder.AddToSubset('A');

        Assert.False(embedder.NeedsSubset());
        Assert.Throws<InvalidOperationException>(() => embedder.Subset());
    }
}
