using PdfBox.Net.FontBox.AFM;

namespace PdfBox.Net.Tests;

public class AFMParserTest
{
    private static Stream OpenFixture()
    {
        string path = Path.Combine("Fixtures", "AFM", "test-font.afm");
        return File.OpenRead(path);
    }

    [Fact]
    public void TestParseFontLevelMetadata()
    {
        using Stream stream = OpenFixture();
        AFMParser parser = new(stream);
        FontMetrics metrics = parser.Parse();

        Assert.Equal("TestFont", metrics.FontName);
        Assert.Equal("Test Font", metrics.FullName);
        Assert.Equal("Test Family", metrics.FamilyName);
        Assert.Equal("Regular", metrics.Weight);
        Assert.Equal(4.1f, metrics.AfmVersion, 2);
        Assert.Equal(683f, metrics.Ascender, 1);
        Assert.Equal(-217f, metrics.Descender, 1);
        Assert.Equal(662f, metrics.CapHeight, 1);
        Assert.Equal(450f, metrics.XHeight, 1);
        Assert.Equal(-100f, metrics.UnderlinePosition, 1);
        Assert.Equal(50f, metrics.UnderlineThickness, 1);
        Assert.Equal(0f, metrics.ItalicAngle, 1);
        Assert.False(metrics.IsFixedPitch);
        Assert.Equal("AdobeStandardEncoding", metrics.EncodingScheme);
        Assert.Equal("1.0", metrics.FontVersion);
        Assert.Equal("Copyright Test Notice", metrics.Notice);
        Assert.Single(metrics.Comments);
    }

    [Fact]
    public void TestParseFontBBox()
    {
        using Stream stream = OpenFixture();
        AFMParser parser = new(stream);
        FontMetrics metrics = parser.Parse();

        Assert.NotNull(metrics.FontBBox);
        Assert.Equal(-168f, metrics.FontBBox!.GetLowerLeftX(), 1);
        Assert.Equal(-218f, metrics.FontBBox.GetLowerLeftY(), 1);
        Assert.Equal(1000f, metrics.FontBBox.GetUpperRightX(), 1);
        Assert.Equal(898f, metrics.FontBBox.GetUpperRightY(), 1);
    }

    [Fact]
    public void TestParseCharMetrics()
    {
        using Stream stream = OpenFixture();
        AFMParser parser = new(stream);
        FontMetrics metrics = parser.Parse();

        Assert.Equal(4, metrics.CharMetrics.Count);

        CharMetric space = metrics.CharMetrics.First(m => m.Name == "space");
        Assert.Equal(32, space.CharacterCode);
        Assert.Equal(250f, space.Wx, 1);
        Assert.NotNull(space.BoundingBox);
        Assert.Equal(0f, space.BoundingBox!.GetLowerLeftX(), 1);

        CharMetric charA = metrics.CharMetrics.First(m => m.Name == "A");
        Assert.Equal(65, charA.CharacterCode);
        Assert.Equal(722f, charA.Wx, 1);
        Assert.NotNull(charA.BoundingBox);
        Assert.Equal(15f, charA.BoundingBox!.GetLowerLeftX(), 1);
        Assert.Equal(0f, charA.BoundingBox.GetLowerLeftY(), 1);
        Assert.Equal(706f, charA.BoundingBox.GetUpperRightX(), 1);
        Assert.Equal(683f, charA.BoundingBox.GetUpperRightY(), 1);
    }

    [Fact]
    public void TestParseLigatures()
    {
        using Stream stream = OpenFixture();
        AFMParser parser = new(stream);
        FontMetrics metrics = parser.Parse();

        CharMetric charA = metrics.CharMetrics.First(m => m.Name == "A");
        Assert.Equal(2, charA.Ligatures.Count);
        Assert.Equal("fi", charA.Ligatures[0].Successor);
        Assert.Equal("fi", charA.Ligatures[0].LigatureValue);
        Assert.Equal("fl", charA.Ligatures[1].Successor);
        Assert.Equal("fl", charA.Ligatures[1].LigatureValue);
    }

    [Fact]
    public void TestParseKernPairs()
    {
        using Stream stream = OpenFixture();
        AFMParser parser = new(stream);
        FontMetrics metrics = parser.Parse();

        Assert.Single(metrics.KernPairs);
        KernPair kp = metrics.KernPairs[0];
        Assert.Equal("A", kp.FirstGlyph);
        Assert.Equal("V", kp.SecondGlyph);
        Assert.Equal(-80f, kp.DeltaX, 1);
        Assert.Equal(0f, kp.DeltaY, 1);
    }

    [Fact]
    public void TestParseTrackKern()
    {
        using Stream stream = OpenFixture();
        AFMParser parser = new(stream);
        FontMetrics metrics = parser.Parse();

        Assert.Single(metrics.TrackKerns);
        TrackKern tk = metrics.TrackKerns[0];
        Assert.Equal(-1, tk.Degree);
        Assert.Equal(12f, tk.MinPtSize, 1);
        Assert.Equal(-1f, tk.MinKern, 1);
        Assert.Equal(24f, tk.MaxPtSize, 1);
        Assert.Equal(0f, tk.MaxKern, 1);
    }

    [Fact]
    public void TestParseComposites()
    {
        using Stream stream = OpenFixture();
        AFMParser parser = new(stream);
        FontMetrics metrics = parser.Parse();

        Assert.Single(metrics.Composites);
        Composite comp = metrics.Composites[0];
        Assert.Equal("Aacute", comp.Name);
        Assert.Equal(2, comp.Parts.Count);

        CompositePart part0 = comp.Parts[0];
        Assert.Equal("A", part0.Name);
        Assert.Equal(0, part0.DisplacementX);
        Assert.Equal(0, part0.DisplacementY);

        CompositePart part1 = comp.Parts[1];
        Assert.Equal("acutecomb", part1.Name);
        Assert.Equal(195, part1.DisplacementX);
        Assert.Equal(195, part1.DisplacementY);
    }

    [Fact]
    public void TestAverageFontWidth()
    {
        using Stream stream = OpenFixture();
        AFMParser parser = new(stream);
        FontMetrics metrics = parser.Parse();

        // All 4 characters have Wx > 0: space=250, A=722, V=722, .notdef=500 => (250+722+722+500)/4 = 548.5
        float avg = metrics.GetAverageFontWidth();
        Assert.True(avg > 0);
        Assert.Equal((250f + 722f + 722f + 500f) / 4f, avg, 1);
    }

    [Fact]
    public void TestGetCharWidth()
    {
        using Stream stream = OpenFixture();
        AFMParser parser = new(stream);
        FontMetrics metrics = parser.Parse();

        Assert.Equal(250f, metrics.GetCharWidth(32), 1);
        Assert.Equal(722f, metrics.GetCharWidth(65), 1);
        Assert.Equal(0f, metrics.GetCharWidth(999), 1);
    }

    [Fact]
    public void TestReducedDataOnly()
    {
        using Stream stream = OpenFixture();
        AFMParser parser = new(stream);
        FontMetrics metrics = parser.Parse(reducedDataOnly: true);

        Assert.Equal("TestFont", metrics.FontName);
        Assert.Empty(metrics.CharMetrics);
        Assert.Empty(metrics.KernPairs);
        Assert.Empty(metrics.Composites);
    }

    [Fact]
    public void TestInvalidAfmThrows()
    {
        byte[] data = "Not a valid AFM file"u8.ToArray();
        using MemoryStream ms = new(data);
        AFMParser parser = new(ms);
        Assert.Throws<IOException>(() => parser.Parse());
    }
}
