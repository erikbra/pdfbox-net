using System.Text;
using PdfBox.Net.FontBox.CMap;
using PdfBox.Net.IO;

namespace PdfBox.Net.Tests;

public class CMapParserTest
{
    [Fact]
    public void ParseRepresentativeCMapStreamAndResolveMappings()
    {
        byte[] cmapBytes = Encoding.ASCII.GetBytes(
            "/CIDInit /ProcSet findresource begin\n" +
            "12 dict begin\n" +
            "begincmap\n" +
            "/CMapName /Test-CMap def\n" +
            "/CMapVersion 1.0 def\n" +
            "/CMapType 2 def\n" +
            "/Registry (Adobe) def\n" +
            "/Ordering (Identity) def\n" +
            "/Supplement 0 def\n" +
            "1 begincodespacerange\n" +
            "<00> <FF>\n" +
            "endcodespacerange\n" +
            "1 beginbfchar\n" +
            "<21> <0041>\n" +
            "endbfchar\n" +
            "1 begincidchar\n" +
            "<20> 100\n" +
            "endcidchar\n" +
            "1 begincidrange\n" +
            "<30> <32> 200\n" +
            "endcidrange\n" +
            "endcmap\n" +
            "CMapName currentdict /CMap defineresource pop\n" +
            "end\n" +
            "end\n");

        using RandomAccessReadBuffer randomAccessRead = new(cmapBytes);
        CMapParser parser = new();

        CMap cmap = parser.Parse(randomAccessRead);

        Assert.Equal("Test-CMap", cmap.Name);
        Assert.Equal("1", cmap.Version);
        Assert.Equal(2, cmap.Type);
        Assert.Equal("Adobe", cmap.Registry);
        Assert.Equal("Identity", cmap.Ordering);
        Assert.Equal(0, cmap.Supplement);

        Assert.Equal("A", cmap.ToUnicode(0x21));
        Assert.Equal(100, cmap.ToCID([0x20]));
        Assert.Equal(200, cmap.ToCID([0x30]));
        Assert.Equal(201, cmap.ToCID([0x31]));
        Assert.Equal(202, cmap.ToCID([0x32]));

        using MemoryStream stream = new([0x31]);
        Assert.Equal(0x31, cmap.ReadCode(stream));
    }
}
