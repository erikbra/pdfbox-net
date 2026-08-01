using PdfBox.Net.COS;
using PdfBox.Net.PdfParser;

namespace PdfBox.Net.Tests;

public class PDFXrefStreamParserTest
{
    [Fact]
    public void ConstructorShouldRejectZeroWidthXrefEntries()
    {
        using COSStream stream = CreateXrefStream(0, 0, 0);

        IOException exception = Assert.Throws<IOException>(() => new PDFXrefStreamParser(stream));

        Assert.Contains("Incorrect /W array", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ConstructorShouldAcceptValidXrefEntryWidths()
    {
        using COSStream stream = CreateXrefStream(1, 4, 2);

        PDFXrefStreamParser parser = new(stream);

        Assert.Same(stream, parser.Parse().GetStream());
    }

    private static COSStream CreateXrefStream(params int[] widths)
    {
        COSStream stream = new();
        COSArray widthArray = new();
        foreach (int width in widths)
        {
            widthArray.Add(COSInteger.Get(width));
        }
        stream.SetItem(COSName.GetPDFName("W"), widthArray);
        return stream;
    }
}
