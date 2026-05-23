using PdfBox.Net.FontBox.CFF;
using PdfBox.Net.IO;

namespace PdfBox.Net.Tests;

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
        Assert.NotNull(font.GetPath("space"));
    }
}
