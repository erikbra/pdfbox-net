using PdfBox.Net.FontBox.TTF;
using PdfBox.Net.IO;

namespace PdfBox.Net.FontBox.Tests;

public class TTFParserTest
{
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
