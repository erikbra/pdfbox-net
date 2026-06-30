using PdfBox.Net.FontBox.Encoding;
using PdfBox.Net.FontBox.CFF;
using PdfBox.Net.FontBox.Pfb;
using PdfBox.Net.FontBox.Type1;

namespace PdfBox.Net.FontBox.Tests;

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

    [Fact]
    public void TestEmpty()
    {
        Assert.Throws<IOException>(() => Type1Font.CreateWithPFB([]));
    }

    [Fact]
#pragma warning disable CS0618
    public void Type1FontUtilHexAndEncryptionRoundTrip()
    {
        byte[] value = [0x00, 0x0f, 0xa5, 0xff];

        Assert.Equal("000FA5FF", Type1FontUtil.HexEncode(value));
        Assert.Equal(value, Type1FontUtil.HexDecode("000fa5ff"));

        byte[] plain = [1, 2, 3, 4, 5, 6];
        Assert.Equal(plain, Type1FontUtil.EexecDecrypt(Type1FontUtil.EexecEncrypt(plain)));
        Assert.Equal(plain, Type1FontUtil.CharstringDecrypt(Type1FontUtil.CharstringEncrypt(plain, 4), 4));
    }
#pragma warning restore CS0618
}
