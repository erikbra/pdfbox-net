using PdfBox.Net.FontBox.CMap;
using PdfBox.Net.IO;

namespace PdfBox.Net.Tests;

public class CMapParserTest
{
    [Fact]
    public void TestMinimalCMapParsesAndMapsCodes()
    {
        byte[] bytes = FontBoxTestFixtures.CreateMinimalCMap();
        using RandomAccessReadBuffer input = new(bytes);
        CMapParser parser = new();
        CMap cmap = parser.Parse(input);

        Assert.Equal("Test-CMap", cmap.Name);
        Assert.Equal("Adobe", cmap.Registry);
        Assert.Equal("Identity", cmap.Ordering);
        Assert.Equal(0, cmap.Supplement);

        Assert.True(cmap.HasCIDMappings());
        Assert.Equal(100, cmap.ToCID(new byte[] { 0x30 }));
        Assert.Equal(101, cmap.ToCID(new byte[] { 0x31 }));
        Assert.Equal(102, cmap.ToCID(new byte[] { 0x32 }));
        Assert.Equal(200, cmap.ToCID(new byte[] { 0x40 }));

        Assert.Equal(" ", cmap.ToUnicode(new byte[] { 0x20 }));
        Assert.Equal(0x20, cmap.SpaceMapping);

        using MemoryStream contentStream = new([0x31]);
        Assert.Equal(0x31, cmap.ReadCode(contentStream));
    }
}
