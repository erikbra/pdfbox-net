using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Encryption;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.PDModel.Interactive.Form;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Rendering;
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
    public void PDFBox_Run_HelpAndMissingCommand_MatchAppDispatcherShape()
    {
        StringWriter help = new();
        int helpCode = PDFBox.Run(["help"], help, new StringWriter());

        Assert.Equal(0, helpCode);
        Assert.Contains("Usage: pdfbox [COMMAND] [OPTIONS]", help.ToString(), StringComparison.Ordinal);
        Assert.Contains("export:text", help.ToString(), StringComparison.Ordinal);

        StringWriter error = new();
        int missingCode = PDFBox.Run([], new StringWriter(), error);

        Assert.Equal(1, missingCode);
        Assert.Contains("Subcommand", error.ToString(), StringComparison.OrdinalIgnoreCase);
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

    [Fact]
    public void Decrypt_Run_DecryptsStandardEncryptedPdf()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "pdfbox-net-tools-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        string encryptedPath = Path.Combine(tempDir, "encrypted.pdf");
        string decryptedPath = Path.Combine(tempDir, "decrypted.pdf");

        using (PDDocument document = new())
        {
            document.AddPage(new PDPage());
            document.Protect(new StandardProtectionPolicy(
                "secret",
                "secret",
                AccessPermission.GetOwnerAccessPermission()));
            document.Save(encryptedPath);
        }

        StringWriter error = new();
        int exitCode = Decrypt.Run(
            ["-i", encryptedPath, "-o", decryptedPath, "-password", "secret"],
            error);

        Assert.Equal(0, exitCode);
        Assert.Equal(string.Empty, error.ToString());
        using PDDocument decrypted = Loader.LoadPDF(decryptedPath);
        Assert.False(decrypted.IsEncrypted());
        Assert.Equal(1, decrypted.GetNumberOfPages());
    }

    [Fact]
    public void Encrypt_Run_EncryptsStandardPasswordPdf()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "pdfbox-net-tools-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        string sourcePath = Path.Combine(tempDir, "plain.pdf");
        string encryptedPath = Path.Combine(tempDir, "encrypted.pdf");
        CreateSinglePagePdf(sourcePath);

        StringWriter error = new();
        int exitCode = Encrypt.Run(
            ["-i", sourcePath, "-o", encryptedPath, "-O", "owner-secret", "-U", "user-secret", "-keyLength", "128"],
            error);

        Assert.Equal(0, exitCode);
        Assert.Equal(string.Empty, error.ToString());
        Assert.Throws<InvalidPasswordException>(() => Loader.LoadPDF(encryptedPath));
        using PDDocument encrypted = Loader.LoadPDF(encryptedPath, "user-secret");
        Assert.True(encrypted.IsEncrypted());
        Assert.Equal(1, encrypted.GetNumberOfPages());
    }

    [Fact]
    public void ExtractXMP_Run_WritesDocumentMetadata()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "pdfbox-net-tools-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        string sourcePath = Path.Combine(tempDir, "metadata.pdf");
        string xmpPath = Path.Combine(tempDir, "metadata.xml");
        byte[] payload = System.Text.Encoding.UTF8.GetBytes("<x:xmpmeta xmlns:x=\"adobe:ns:meta/\"/>");

        using (PDDocument document = new())
        {
            document.AddPage(new PDPage());
            PDMetadata metadata = new(document);
            metadata.ImportXMPMetadata(payload);
            document.GetDocumentCatalog().SetMetadata(metadata);
            document.Save(sourcePath);
        }

        StringWriter error = new();
        int exitCode = ExtractXMP.Run(["-i", sourcePath, "-o", xmpPath], error: error);

        Assert.Equal(0, exitCode);
        Assert.Equal(string.Empty, error.ToString());
        Assert.Equal(payload, File.ReadAllBytes(xmpPath));
    }

    [Fact]
    public void TextToPDF_Run_CreatesExtractableTextPdf()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "pdfbox-net-tools-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        string textPath = Path.Combine(tempDir, "source.txt");
        string pdfPath = Path.Combine(tempDir, "text.pdf");
        File.WriteAllText(textPath, "Hello from TextToPDF");

        StringWriter error = new();
        int exitCode = TextToPDF.Run(["-i", textPath, "-o", pdfPath], error);

        Assert.Equal(0, exitCode);
        Assert.Equal(string.Empty, error.ToString());
        Assert.Contains("Hello from TextToPDF", ExtractText.GetText(pdfPath));
    }

    [Fact]
    public void ImageToPDF_Run_EmbedsImageXObject()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "pdfbox-net-tools-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        string imagePath = Path.Combine(tempDir, "source.png");
        string pdfPath = Path.Combine(tempDir, "image.pdf");
        CreatePng(imagePath);

        StringWriter error = new();
        int exitCode = ImageToPDF.Run(["-i", imagePath, "-o", pdfPath], error);

        Assert.Equal(0, exitCode);
        Assert.Equal(string.Empty, error.ToString());
        using PDDocument document = Loader.LoadPDF(pdfPath);
        PDResources resources = Assert.IsType<PDResources>(document.GetPage(0).GetResources());
        Assert.Contains(resources.GetXObjectNames(), resources.IsImageXObject);
    }

    [Fact]
    public void ExtractImages_Run_WritesImageXObjectAsPng()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "pdfbox-net-tools-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        string sourcePath = Path.Combine(tempDir, "image-source.pdf");
        string outputPrefix = Path.Combine(tempDir, "extracted");

        using (PDDocument document = new())
        {
            PDPage page = new();
            document.AddPage(page);
            using BufferedImage bitmap = new(2, 1, BufferedImage.TYPE_INT_RGB);
            bitmap.SetRgb(0, 0, unchecked((int)0xFFFF0000));
            bitmap.SetRgb(1, 0, unchecked((int)0xFF00FF00));
            PDImageXObject image = LosslessFactory.CreateFromImage(document, bitmap);
            using PDPageContentStream content = new(document, page);
            content.DrawImage(image, 20, 20, 2, 1);
            document.Save(sourcePath);
        }

        StringWriter error = new();
        int exitCode = ExtractImages.Run(["-i", sourcePath, "-o", outputPrefix], error);

        Assert.Equal(0, exitCode);
        Assert.Equal(string.Empty, error.ToString());
        Assert.True(File.Exists($"{outputPrefix}-1.png"));
    }

    [Fact]
    public void PrintPDF_Run_ReturnsFailureWhenNoPrintBackendIsRegistered()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "pdfbox-net-tools-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        string sourcePath = Path.Combine(tempDir, "plain.pdf");
        CreateSinglePagePdf(sourcePath);

        StringWriter error = new();
        int exitCode = PrintPDF.Run(["-i", sourcePath], error);

        Assert.Equal(4, exitCode);
        Assert.Contains("No PdfBox.Net print backend is registered", error.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ExportAndImportFDF_Run_RoundTripsTextFieldValue()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "pdfbox-net-tools-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        string sourcePath = Path.Combine(tempDir, "source-form.pdf");
        string fdfPath = Path.Combine(tempDir, "form-data.fdf");
        string targetPath = Path.Combine(tempDir, "target-form.pdf");
        string importedPath = Path.Combine(tempDir, "imported-form.pdf");
        CreateTextFieldPdf(sourcePath, "source-value");
        CreateTextFieldPdf(targetPath, string.Empty);

        StringWriter exportError = new();
        int exportExit = ExportFDF.Run(["-i", sourcePath, "-o", fdfPath], exportError);
        StringWriter importError = new();
        int importExit = ImportFDF.Run(["-i", targetPath, "--data", fdfPath, "-o", importedPath], importError);

        Assert.Equal(0, exportExit);
        Assert.Equal(string.Empty, exportError.ToString());
        Assert.Equal(0, importExit);
        Assert.Equal(string.Empty, importError.ToString());
        Assert.Equal("source-value", ReadTextFieldValue(importedPath));
    }

    [Fact]
    public void ExportAndImportXFDF_Run_RoundTripsTextFieldValue()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "pdfbox-net-tools-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        string sourcePath = Path.Combine(tempDir, "source-form.pdf");
        string xfdfPath = Path.Combine(tempDir, "form-data.xfdf");
        string targetPath = Path.Combine(tempDir, "target-form.pdf");
        string importedPath = Path.Combine(tempDir, "imported-form.pdf");
        CreateTextFieldPdf(sourcePath, "source-value");
        CreateTextFieldPdf(targetPath, string.Empty);

        StringWriter exportError = new();
        int exportExit = ExportXFDF.Run(["-i", sourcePath, "-o", xfdfPath], exportError);
        StringWriter importError = new();
        int importExit = ImportXFDF.Run(["-i", targetPath, "--data", xfdfPath, "-o", importedPath], importError);

        Assert.Equal(0, exportExit);
        Assert.Equal(string.Empty, exportError.ToString());
        Assert.Equal(0, importExit);
        Assert.Equal(string.Empty, importError.ToString());
        Assert.Equal("source-value", ReadTextFieldValue(importedPath));
    }

    [Fact]
    public void PDFBox_Run_DispatchesImplementedToolCommands()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "pdfbox-net-tools-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        string textPath = Path.Combine(tempDir, "source.txt");
        string pdfPath = Path.Combine(tempDir, "text.pdf");
        File.WriteAllText(textPath, "dispatcher text");

        StringWriter error = new();
        int exitCode = PDFBox.Run(["texttopdf", "-i", textPath, "-o", pdfPath], new StringWriter(), error);

        Assert.Equal(0, exitCode);
        Assert.Equal(string.Empty, error.ToString());
        Assert.True(File.Exists(pdfPath));
    }

    [Fact]
    public void PDFBox_Run_DispatchesPdfBox30ToolAliases()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "pdfbox-net-tools-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        string textPath = Path.Combine(tempDir, "source.txt");
        string pdfPath = Path.Combine(tempDir, "text.pdf");
        File.WriteAllText(textPath, "dispatcher alias text");

        StringWriter error = new();
        int exitCode = PDFBox.Run(["fromtext", "-i", textPath, "-o", pdfPath], new StringWriter(), error);

        Assert.Equal(0, exitCode);
        Assert.Equal(string.Empty, error.ToString());
        Assert.True(File.Exists(pdfPath));
        Assert.Contains("dispatcher alias text", ExtractText.GetText(pdfPath));
    }

    [Fact]
    public void PDFBox_Run_DispatchesPdfBox30CoreCliCommands()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "pdfbox-net-tools-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        string textPath = Path.Combine(tempDir, "source.txt");
        string pdfPath = Path.Combine(tempDir, "text.pdf");
        string decodedPath = Path.Combine(tempDir, "decoded.pdf");
        string secondPath = Path.Combine(tempDir, "second.pdf");
        string mergedPath = Path.Combine(tempDir, "merged.pdf");
        string splitPrefix = Path.Combine(tempDir, "part");
        File.WriteAllText(textPath, "apache 3 cli text");
        CreateSinglePagePdf(secondPath);

        StringWriter error = new();
        Assert.Equal(0, PDFBox.Run(["fromtext", "-i", textPath, "-o", pdfPath], new StringWriter(), error));
        Assert.Equal(string.Empty, error.ToString());

        StringWriter textOutput = new();
        error = new StringWriter();
        Assert.Equal(0, PDFBox.Run(["export:text", "-i", pdfPath, "-console"], textOutput, error));
        Assert.Contains("apache 3 cli text", textOutput.ToString(), StringComparison.Ordinal);
        Assert.Equal(string.Empty, error.ToString());

        error = new StringWriter();
        Assert.Equal(0, PDFBox.Run(["decode", pdfPath, decodedPath], new StringWriter(), error));
        Assert.True(File.Exists(decodedPath));
        Assert.Equal(string.Empty, error.ToString());

        error = new StringWriter();
        Assert.Equal(0, PDFBox.Run(["merge", "-i", pdfPath, secondPath, "-o", mergedPath], new StringWriter(), error));
        Assert.Equal(string.Empty, error.ToString());
        using (PDDocument merged = Loader.LoadPDF(mergedPath))
        {
            Assert.Equal(2, merged.GetNumberOfPages());
        }

        error = new StringWriter();
        Assert.Equal(0, PDFBox.Run(["split", "-i", mergedPath, "-outputPrefix", splitPrefix, "-split", "1"], new StringWriter(), error));
        Assert.Equal(string.Empty, error.ToString());
        Assert.True(File.Exists($"{splitPrefix}-1.pdf"));
        Assert.True(File.Exists($"{splitPrefix}-2.pdf"));
    }

    [Fact]
    public void PDFBox_Run_ReportsExplicitUnsupportedOrAdaptedCommands()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "pdfbox-net-tools-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        string sourcePath = Path.Combine(tempDir, "plain.pdf");
        CreateSinglePagePdf(sourcePath);

        StringWriter debugError = new();
        int debugExit = PDFBox.Run(["debug", sourcePath], new StringWriter(), debugError);

        Assert.Equal(1, debugExit);
        Assert.Contains("accepted as a debugger product adaptation", debugError.ToString(), StringComparison.Ordinal);

        StringWriter renderError = new();
        int renderExit = PDFBox.Run(["render", "-i", sourcePath, "-format", "tiff"], new StringWriter(), renderError);

        Assert.Equal(4, renderExit);
        Assert.Contains("PNG and JPEG output only", renderError.ToString(), StringComparison.Ordinal);
    }

    private static void CreateSinglePagePdf(string filePath)
    {
        using PDDocument document = new();
        document.AddPage(new PDPage());
        document.Save(filePath);
    }

    private static void CreatePng(string filePath)
    {
        using BufferedImage bitmap = new(2, 1, BufferedImage.TYPE_INT_RGB);
        bitmap.SetRgb(0, 0, unchecked((int)0xFFFF0000));
        bitmap.SetRgb(1, 0, unchecked((int)0xFF00FF00));
        PdfBox.Net.Tools.ImageIO.ImageIOUtil.WriteImage(bitmap, filePath, 96);
    }

    private static void CreateTextFieldPdf(string filePath, string value)
    {
        using PDDocument document = new();
        document.AddPage(new PDPage());
        PDAcroForm form = new(document);
        document.GetDocumentCatalog().SetAcroForm(form);
        PDTextField field = new(form);
        field.SetPartialName("Name");
        field.SetValue(value);
        form.SetFields([field]);
        document.Save(filePath);
    }

    private static string? ReadTextFieldValue(string filePath)
    {
        using PDDocument document = Loader.LoadPDF(filePath);
        PDField field = Assert.IsType<PDTextField>(document.GetDocumentCatalog().GetAcroForm()!.GetField("Name"));
        return field.GetValueAsString();
    }

}
