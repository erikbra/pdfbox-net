using PdfBox.Net.PDModel;
using PdfBox.Net.Tools;

namespace PdfBox.Net.Tests;

public class ToolsModuleParityCloseoutTest
{
    [Fact]
    public void Version_GetVersion_DoesNotThrow()
    {
        string? version = Version.GetVersion();
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
        string fixture = GetFixturePath("minimal-document-fixture.pdf");
        string tempDir = Path.Combine(Path.GetTempPath(), "pdfbox-net-tools-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        string mergedPath = Path.Combine(tempDir, "merged.pdf");
        PDFMerger.Merge(mergedPath, fixture, fixture);

        using (PDDocument merged = Loader.LoadPDF(mergedPath))
        {
            Assert.Equal(2, merged.GetNumberOfPages());
        }

        IReadOnlyList<string> splitFiles = PDFSplit.Split(mergedPath, tempDir, splitAtPage: 1);
        Assert.Equal(2, splitFiles.Count);
        Assert.All(splitFiles, File.Exists);
    }

    [Fact]
    public void Decrypt_Run_ThrowsNotSupported()
    {
        Assert.Throws<NotSupportedException>(() => Decrypt.Run());
    }

    private static string GetFixturePath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName);
    }
}
