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

        Assert.Throws<NotSupportedException>(() => BenchmarkLoadAndSave.SaveIncrementalFile(sourcePath));
        Assert.Throws<NotSupportedException>(() => BenchmarkLoadAndSave.SaveFileNoCompression(sourcePath));
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
        string sourcePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "minimal-document-fixture.pdf");

        string unsorted = BenchmarkTextExtraction.Extract(sourcePath, sortByPosition: false).ReplaceLineEndings("\n");
        string sorted = BenchmarkTextExtraction.Extract(sourcePath, sortByPosition: true).ReplaceLineEndings("\n");

        Assert.NotNull(unsorted);
        Assert.NotNull(sorted);
    }

    private static void CreateSinglePagePdf(string path)
    {
        using PDDocument document = new();
        document.AddPage(new PDPage());
        document.Save(path);
    }

    private static string GetTempFilePath(string fileName)
    {
        string dir = Path.Combine(Path.GetTempPath(), "pdfbox-net-benchmark-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, fileName);
    }
}
