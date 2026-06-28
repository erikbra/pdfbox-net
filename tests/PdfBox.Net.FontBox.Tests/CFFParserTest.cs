using PdfBox.Net.FontBox.CFF;
using PdfBox.Net.IO;

namespace PdfBox.Net.FontBox.Tests;

public class CFFParserTest
{
    [Fact]
    public void TestMinimalOpenTypeCffParses()
    {
        byte[] bytes = FontBoxTestFixtures.CreateMinimalOpenTypeCff();
        using RandomAccessReadBuffer input = new(bytes);
        CFFParser parser = new();
        List<CFFFont> fonts = parser.Parse(input);
        CFFType1Font font = Assert.IsType<CFFType1Font>(Assert.Single(fonts));
        Assert.Equal("MiniCFF", font.GetName());
        Assert.Equal(2, font.GetNumCharStrings());
        Assert.Equal(0, font.GetFontBBox().GetLowerLeftX());
        Assert.Equal(700, font.GetFontBBox().GetUpperRightY());
        Assert.Equal(".notdef", font.GetCharset().GetNameForGID(0));
        Assert.Equal("space", font.GetCharset().GetNameForGID(1));
        Assert.Equal(1, font.GetCharset().GetSIDForGID(1));
        Assert.IsType<CFFStandardEncoding>(font.GetEncoding());
        Assert.Single(font.GetPrivateDict());
        Assert.True(font.HasGlyph("space"));
        Assert.Equal(1, font.NameToGID("space"));
        Assert.Equal(0, font.NameToGID("missing"));
        Assert.Equal("space", font.GetType1CharString("space").GlyphName);
        Assert.NotNull(font.GetPath("space"));
    }

    [Fact]
    public void TestParserToStringTracksLastParsedFontName()
    {
        byte[] bytes = FontBoxTestFixtures.CreateMinimalOpenTypeCff();
        CFFParser parser = new();

        Assert.Equal("CFFParser[null]", parser.ToString());

        parser.Parse(bytes);

        Assert.Equal("CFFParser[MiniCFF]", parser.ToString());
    }

    [Fact]
    public void TestType1CffWithExpertCharsetAndEncodingParses()
    {
        byte[] bytes = FontBoxTestFixtures.CreateMinimalCffWithExpertCharsetEncoding();
        CFFParser parser = new();
        List<CFFFont> fonts = parser.Parse(bytes);
        CFFType1Font font = Assert.IsType<CFFType1Font>(Assert.Single(fonts));
        Assert.IsType<CFFExpertCharset>(font.GetCharset());
        Assert.IsType<CFFExpertEncoding>(font.GetEncoding());
    }

    [Fact]
    public void TestMinimalCidCffParsesEndToEnd()
    {
        byte[] bytes = FontBoxTestFixtures.CreateMinimalCidCff();
        CFFParser parser = new();
        List<CFFFont> fonts = parser.Parse(bytes);
        CFFCIDFont font = Assert.IsType<CFFCIDFont>(Assert.Single(fonts));

        Assert.Equal("Adobe", font.Registry);
        Assert.Equal("Identity", font.Ordering);
        Assert.Equal(0, font.Supplement);
        Assert.True(font.GetCharset().IsCIDFont());
        Assert.Equal(42, font.GetCharset().GetCIDForGID(1));
        Assert.Equal(1, font.GetCharset().GetGIDForCID(42));
        Assert.Equal(0, font.GetFDSelect()!.GetFDIndex(0));
        Assert.Equal(0, font.GetFDSelect()!.GetFDIndex(1));
        Assert.Single(font.GetFontDicts());
        Assert.Single(font.GetPrivDicts());
        Assert.True(font.HasGlyph("\\42"));

        CIDKeyedType2CharString charString = Assert.IsType<CIDKeyedType2CharString>(font.GetType2CharString(42));
        Assert.Equal(42, charString.CID);
    }
}
