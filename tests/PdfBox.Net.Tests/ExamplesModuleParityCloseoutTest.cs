using System.Text;
using PdfBox.Net.COS;
using PdfBox.Net.MultiPdf;
using PdfBox.Net.PDModel;
using PdfBox.Net.Text;

namespace PdfBox.Net.Tests;

public class ExamplesModuleParityCloseoutTest
{
    [Fact]
    public void CreateBlankPdf_Flow_CreatesAndLoadsSinglePageDocument()
    {
        string path = GetTempFilePath("blank-flow.pdf");

        using (PDDocument created = new())
        {
            created.AddPage(new PDPage());
            created.Save(path);
        }

        using PDDocument loaded = Loader.LoadPDF(path);
        Assert.Equal(1, loaded.GetNumberOfPages());
    }

    [Fact]
    public void PdfMergerExample_Flow_MergesTwoSinglePageDocuments()
    {
        string inputA = GetTempFilePath("merge-a.pdf");
        string inputB = GetTempFilePath("merge-b.pdf");
        string merged = GetTempFilePath("merged.pdf");

        CreateSinglePagePdf(inputA);
        CreateSinglePagePdf(inputB);

        PDFMergerUtility merger = new()
        {
            DestinationFileName = merged,
        };
        merger.AddSource(inputA);
        merger.AddSource(inputB);
        merger.MergeDocuments();

        using PDDocument loaded = Loader.LoadPDF(merged);
        Assert.Equal(2, loaded.GetNumberOfPages());
    }

    [Fact]
    public void ExtractTextSimple_Flow_ExtractsExpectedText()
    {
        using PDDocument document = CreateSimpleTextFixtureDocument("(Hello from examples)");
        string extracted = new PDFTextStripper().GetText(document);

        string normalized = extracted.ReplaceLineEndings("\n");
        Assert.False(string.IsNullOrWhiteSpace(normalized));
        Assert.Contains("H", normalized);
        Assert.Contains("e", normalized);
    }

    private static void CreateSinglePagePdf(string path)
    {
        using PDDocument document = new();
        document.AddPage(new PDPage());
        document.Save(path);
    }

    private static PDDocument CreateSimpleTextFixtureDocument(string text)
    {
        PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);

        COSDictionary pageDict = (COSDictionary)page.GetCOSObject();
        pageDict.SetItem(COSName.RESOURCES, CreateDefaultResourcesDictionary());

        COSStream stream = new();
        using (Stream output = stream.CreateOutputStream())
        {
            string content = $"BT\n/F1 12 Tf\n72 720 Td\n{text} Tj\nET\n";
            byte[] bytes = Encoding.Latin1.GetBytes(content);
            output.Write(bytes, 0, bytes.Length);
        }

        pageDict.SetItem(COSName.CONTENTS, stream);
        return document;
    }

    private static COSDictionary CreateDefaultResourcesDictionary()
    {
        COSDictionary fontDictionary = new();
        fontDictionary.SetItem(COSName.TYPE, COSName.GetPDFName("Font"));
        fontDictionary.SetItem(COSName.GetPDFName("Subtype"), COSName.GetPDFName("Type1"));
        fontDictionary.SetItem(COSName.GetPDFName("BaseFont"), COSName.GetPDFName("Helvetica"));

        COSDictionary fonts = new();
        fonts.SetItem(COSName.GetPDFName("F1"), fontDictionary);

        COSDictionary resources = new();
        resources.SetItem(COSName.GetPDFName("Font"), fonts);
        return resources;
    }

    private static string GetTempFilePath(string fileName)
    {
        string dir = Path.Combine(Path.GetTempPath(), "pdfbox-net-examples-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, fileName);
    }
}
