using PdfBox.Net.PDModel;
using PdfBox.Net.Tools;
using PdfBoxToolsVersion = PdfBox.Net.Tools.Version;

namespace PdfBox.Net.Tests;

public class ToolsModuleParityCloseoutTest
{
    [Fact]
    public void Version_GetVersion_DoesNotThrow()
    {
        string? version = PdfBoxToolsVersion.GetVersion();
        if (version is not null)
        {
            Assert.NotEmpty(version);
        }
    }

    [Fact]
    public void PDFBox_Run_VersionCommand_ReturnsZero()
    {
        StringWriter output = new();
        int code = PDFBox.Run(["version"], output, new StringWriter());

        Assert.Equal(0, code);
        Assert.NotEmpty(output.ToString().Trim());
    }

    [Fact]
    public void PDFText2HTML_ConvertText_EncodesHtml()
    {
        string html = PDFText2HTML.ConvertText("<hello>");
        Assert.Contains("&lt;hello&gt;", html);
    }

    [Fact]
    public void PDFMerger_And_PDFSplit_ProcessFixtureDocuments()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "pdfbox-net-tools-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        string sourceA = Path.Combine(tempDir, "source-a.pdf");
        string sourceB = Path.Combine(tempDir, "source-b.pdf");
        CreateSinglePagePdf(sourceA);
        CreateSinglePagePdf(sourceB);

        string mergedPath = Path.Combine(tempDir, "merged.pdf");
        PDFMerger.Merge(mergedPath, sourceA, sourceB);

        using (PDDocument merged = Loader.LoadPDF(mergedPath))
        {
            Assert.Equal(2, merged.GetNumberOfPages());
        }

        IReadOnlyList<string> splitFiles = PDFSplit.Split(mergedPath, tempDir, splitAtPage: 1);
        Assert.Equal(2, splitFiles.Count);
        Assert.All(splitFiles, path => Assert.True(File.Exists(path)));
    }

    [Fact]
    public void Decrypt_Run_ThrowsNotSupported()
    {
        Assert.Throws<NotSupportedException>(() => Decrypt.Run());
    }

    private static void CreateSinglePagePdf(string filePath)
    {
        using PDDocument document = new();
        document.AddPage(new PDPage());
        document.Save(filePath);
    }

}
