using PdfBox.Net.FontBox.TTF;
using PdfBox.Net.IO;

namespace PdfBox.Net.Tests;

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
        Assert.Equal(2, font.GetNumberOfGlyphs());
        Assert.Equal(1, font.GetGlyphId('A'));
        Assert.Equal(600, font.GetAdvanceWidth(1));
        Assert.Equal(50, font.GetLeftSideBearing(1));
        Assert.Equal("A", font.GetName(1));

        HeaderTable header = Assert.IsType<HeaderTable>(font.GetTable("head"));
        Assert.Equal((short)500, header.XMax);
        Assert.Equal((short)700, header.YMax);
        Assert.Equal((short)0, header.IndexToLocFormat);

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
    }

    [Fact]
    public void TestMinimalTrueTypeParsesGlyphData()
    {
        TrueTypeFont font = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());

        GlyphData glyph = Assert.IsType<GlyphData>(font.GetGlyph(1));
        Assert.Equal((short)1, glyph.NumberOfContours);
        Assert.Equal((short)50, glyph.XMinimum);
        Assert.Equal((short)500, glyph.XMaximum);
        Assert.Equal((short)700, glyph.YMaximum);
        Assert.Equal(4, glyph.Description.GetPointCount());
        Assert.Equal((short)50, glyph.Description.GetXCoordinate(0));
        Assert.Equal((short)500, glyph.Description.GetXCoordinate(1));
        Assert.Equal((short)700, glyph.Description.GetYCoordinate(2));
        Assert.Equal(700f, glyph.BoundingBox.GetHeight());
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
