using PdfBox.Net.FontBox.Encoding;
using PdfBox.Net.FontBox.Pfb;
using PdfBox.Net.FontBox.Type1;

namespace PdfBox.Net.Tests;

public class PfbAndType1FontTest
{
    [Fact]
    public void TestMinimalPfbFontParses()
    {
        Type1Font font = Type1Font.CreateWithPFB(FontBoxTestFixtures.CreateMinimalType1Pfb());
        Assert.Equal("1.0", font.GetVersion());
        Assert.Equal("TestFont", font.GetFontName());
        Assert.Equal("Test Font", font.GetFullName());
        Assert.Equal("Test Family", font.GetFamilyName());
        Assert.Equal("Test notice", font.GetNotice());
        Assert.False(font.IsFixedPitch());
        Assert.False(font.IsForceBold());
        Assert.Equal(0, font.GetItalicAngle());
        Assert.Equal("Regular", font.GetWeight());
        Assert.IsType<BuiltInEncoding>(font.GetEncoding());
        Assert.Equal("A", font.GetEncoding().GetName(65));
        Assert.Equal(2, font.GetCharStringsDict().Count);
        Assert.True(font.HasGlyph("A"));
        Assert.NotNull(font.GetPath("A"));
        Assert.Single(font.GetSubrsArray());
        Assert.Equal(500, font.GetFontBBox().GetUpperRightX());
    }

    [Fact]
    public void TestNegativeRecordSize()
    {
        byte[] crashInput =
        [
            0x80, 0x01,
            0x01, 0x00, 0x00, 0xFF,
            0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF,
            0x27, 0x05, 0xF8, 0xFF,
            0xD2, 0x40,
        ];
        Assert.Throws<IOException>(() => new PfbParser(crashInput));
    }
}
