using System.Text;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using BenchmarkLoadAndSave = PdfBox.Net.Benchmark.LoadAndSave;
using BenchmarkRendering = PdfBox.Net.Benchmark.Rendering;
using BenchmarkTextExtraction = PdfBox.Net.Benchmark.TextExtraction;

namespace PdfBox.Net.Tests;

public class BenchmarkModuleParityCloseoutTest
{
    [Fact]
    public void LoadAndSave_Flow_LoadsAndWritesFixtureDocument()
    {
        string sourcePath = GetTempFilePath("benchmark-source.pdf");
        CreateSinglePagePdf(sourcePath);

        BenchmarkLoadAndSave.LoadFile(sourcePath);
        BenchmarkLoadAndSave.SaveFile(sourcePath);
    }

    [Fact]
    public void Rendering_Flow_RendersFixtureDocument()
    {
        string sourcePath = GetTempFilePath("benchmark-render.pdf");
        CreateSinglePagePdf(sourcePath);

        BenchmarkRendering.RenderNoOutput(sourcePath, 72f);
        BenchmarkRendering.RenderToPngFiles(sourcePath, 72f, "benchmark-test-render");

        string outputPath = Path.Combine(BenchmarkRendering.RenderOutputDir, "benchmark-test-render-0.png");
        Assert.True(File.Exists(outputPath));
    }

    [Fact]
    public void TextExtraction_Flow_ExtractsFixtureText()
    {
        string sourcePath = GetTempFilePath("benchmark-text.pdf");
        using (PDDocument document = CreateSimpleTextFixtureDocument("(benchmark text extraction)"))
        {
            document.Save(sourcePath);
        }

        string unsorted = BenchmarkTextExtraction.Extract(sourcePath, sortByPosition: false).ReplaceLineEndings("\n");
        string sorted = BenchmarkTextExtraction.Extract(sourcePath, sortByPosition: true).ReplaceLineEndings("\n");

        Assert.Contains("benchmark", unsorted, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("benchmark", sorted, StringComparison.OrdinalIgnoreCase);
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
        string dir = Path.Combine(Path.GetTempPath(), "pdfbox-net-benchmark-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, fileName);
    }
}
