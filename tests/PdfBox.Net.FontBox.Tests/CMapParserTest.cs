using System.Text;
using PdfBox.Net.FontBox.CMap;
using FontBoxCMap = PdfBox.Net.FontBox.CMap.CMap;
using PdfBox.Net.IO;

namespace PdfBox.Net.FontBox.Tests;

public class CMapParserTest
{
    [Theory]
    [InlineData("")]
    [InlineData("../Identity-H")]
    [InlineData("resource/Identity-H")]
    [InlineData("resource\\Identity-H")]
    [InlineData(".Identity-H")]
    public void ParsePredefined_RejectsNamesOutsidePredefinedResources(string name)
    {
        IOException exception = Assert.Throws<IOException>(() => new CMapParser().ParsePredefined(name));
        Assert.Equal("Error: Invalid CMap name " + name, exception.Message);
    }

    [Theory]
    [InlineData("UniCNS-UTF16-H", "Adobe-CNS1-UCS2")]
    [InlineData("UniGB-UTF16-H", "Adobe-GB1-UCS2")]
    [InlineData("UniJIS-UTF16-H", "Adobe-Japan1-UCS2")]
    [InlineData("UniKS-UTF16-H", "Adobe-Korea1-UCS2")]
    public void PredefinedCjkEncodingResolvesUnicodeThroughCharacterCollection(string encodingName, string ucs2Name)
    {
        CMapParser parser = new();
        FontBoxCMap encoding = parser.ParsePredefined(encodingName);
        FontBoxCMap unicode = parser.ParsePredefined(ucs2Name);
        int cid = encoding.ToCID([0, 0x41]);
        Assert.NotEqual(0, cid);
        Assert.Equal("A", unicode.ToUnicode(cid));
    }

    [Fact]
    public void EveryBundledPredefinedCMapCanBeParsed()
    {
        const string prefix = "PdfBox.Net.FontBox.CMap.";
        string[] names = typeof(CMapParser).Assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(prefix, StringComparison.Ordinal)).ToArray();
        Assert.Equal(92, names.Length);
        foreach (string name in names)
        {
            FontBoxCMap cmap = new CMapParser().ParsePredefined(name[prefix.Length..]);
            Assert.False(string.IsNullOrEmpty(cmap.Name), name);
        }
    }

    [Fact]
    public void ParseRepresentativeCMapStreamAndResolveMappings()
    {
        byte[] cmapBytes = System.Text.Encoding.ASCII.GetBytes(
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

        FontBoxCMap cmap = parser.Parse(randomAccessRead);

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
