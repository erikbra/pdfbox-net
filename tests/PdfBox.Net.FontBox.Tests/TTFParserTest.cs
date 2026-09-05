using PdfBox.Net.FontBox.TTF;
using PdfBox.Net.IO;

namespace PdfBox.Net.FontBox.Tests;

public class TTFParserTest
{
    [Fact]
    public void ParseTableHeaders_AgreesWithCompleteFontAndClosesSource()
    {
        byte[] bytes = FontBoxTestFixtures.CreateMinimalTrueType();
        using TrueTypeFont font = new TTFParser().Parse(bytes);
        using RandomAccessReadBuffer input = new(bytes);

        FontHeaders headers = new TTFParser().ParseTableHeaders(input);

        Assert.True(input.IsClosed());
        Assert.Null(headers.GetError());
        Assert.Equal(font.GetName(), headers.GetName());
        Assert.False(headers.IsOTFAndPostScript());
        Assert.Equal(font.GetNaming()!.GetFontFamily(), headers.GetFontFamily());
        Assert.Equal(font.GetNaming()!.GetFontSubFamily(), headers.GetFontSubFamily());
    }

    [Fact]
    public void ParseEmbedded_ClosesSourceAndKeepsFontReadable()
    {
        using MemoryStream input = new(FontBoxTestFixtures.CreateMinimalTrueType());
        using TrueTypeFont font = new TTFParser(true).ParseEmbedded(input);
        Assert.False(input.CanRead);
        Assert.Equal("MiniTTF", font.GetName());
        Assert.Equal(1000, font.GetUnitsPerEm());
    }

    [Fact]
    public void ParseEmbedded_ClosesSourceWhenFontIsMalformed()
    {
        using MemoryStream input = new([0, 1, 0, 0]);
        Assert.ThrowsAny<IOException>(() => new TTFParser(true).ParseEmbedded(input));
        Assert.False(input.CanRead);
    }

    [Fact]
    public void ParseRandomAccess_ClosesSourceWhenFontIsMalformed()
    {
        using RandomAccessReadBuffer input = new(new byte[] { 0, 1, 0, 0 });
        Assert.ThrowsAny<IOException>(() => new TTFParser().Parse(input));
        Assert.True(input.IsClosed());
    }

    [Fact]
    public void TestMinimalTrueTypeParsesCoreTables()
    {
        byte[] bytes = FontBoxTestFixtures.CreateMinimalTrueType();
        TTFParser parser = new();
        TrueTypeFont font = parser.Parse(bytes);

        Assert.Equal(9, font.NumberOfTables);
        Assert.Equal(1000, font.GetUnitsPerEm());
        Assert.Equal("MiniTTF", font.GetName());

        HeaderTable header = Assert.IsType<HeaderTable>(font.GetTable("head"));
        Assert.Equal((short)500, header.XMax);
        Assert.Equal((short)700, header.YMax);

        MaximumProfileTable maxp = Assert.IsType<MaximumProfileTable>(font.GetTable("maxp"));
        Assert.Equal((ushort)2, maxp.NumGlyphs);
    }

    [Fact]
    public void TestMinimalTrueTypeParsesFromRandomAccessRead()
    {
        byte[] bytes = FontBoxTestFixtures.CreateMinimalTrueType();
        using RandomAccessReadBuffer input = new(bytes);
        TTFParser parser = new();
        TrueTypeFont font = parser.Parse(input);

        Assert.Equal("MiniTTF", font.GetName());
        Assert.NotNull(font.GetTable("name"));
        Assert.True(input.IsClosed());
    }

    [Fact]
    public void TestTtfParserRejectsOpenTypeWhenNotEnabled()
    {
        byte[] bytes = FontBoxTestFixtures.CreateMinimalOpenTypeCff();
        TTFParser parser = new();

        Assert.Throws<IOException>(() => parser.Parse(bytes));
    }

    [Fact]
    public void TestOtfParserParsesOpenTypeWithCffTable()
    {
        byte[] bytes = FontBoxTestFixtures.CreateMinimalOpenTypeCff();
        OTFParser parser = new();
        OpenTypeFont font = Assert.IsType<OpenTypeFont>(parser.Parse(bytes));

        Assert.True(font.IsPostScript);
        Assert.NotNull(font.GetTable("CFF "));
    }
}
