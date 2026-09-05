using System.Text;
using PdfBox.Net.Examples.PDModel;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common.FileSpecification;

namespace PdfBox.Net.Examples.Tests.PDModel;

public class TestExtractEmbeddedFiles
{
    [Fact]
    public void ExtractionUsesPdfDirectoryAndRejectsParentTraversal()
    {
        string root = Path.Combine(Path.GetTempPath(), "pdfbox-extraction-" + Guid.NewGuid().ToString("N"));
        string directory = Path.Combine(root, "document");
        Directory.CreateDirectory(directory);
        try
        {
            string pdf = Path.Combine(directory, "embedded.pdf");
            CreatePdf(pdf, ["safe/note.txt", "../outside.txt"]);

            ExtractEmbeddedFiles.Main([pdf]);

            Assert.Equal("embedded content", File.ReadAllText(Path.Combine(directory, "safe/note.txt")));
            Assert.False(File.Exists(Path.Combine(root, "outside.txt")));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ExtractionRejectsFinalFileAndDirectorySymlinksOutsidePdfDirectory()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip("Creating symbolic links on Windows requires a separately enabled privilege.");
        }

        string root = Path.Combine(Path.GetTempPath(), "pdfbox-extraction-" + Guid.NewGuid().ToString("N"));
        string directory = Path.Combine(root, "document");
        Directory.CreateDirectory(directory);
        string outside = Path.Combine(root, "outside");
        Directory.CreateDirectory(outside);
        string protectedFile = Path.Combine(outside, "protected.txt");
        File.WriteAllText(protectedFile, "unchanged");
        string fileLink = Path.Combine(directory, "file-link.txt");
        string directoryLink = Path.Combine(directory, "directory-link");
        try
        {
            File.CreateSymbolicLink(fileLink, protectedFile);
            Directory.CreateSymbolicLink(directoryLink, outside);
            string pdf = Path.Combine(directory, "embedded.pdf");
            CreatePdf(pdf, ["file-link.txt", "directory-link/new.txt"]);

            ExtractEmbeddedFiles.Main([pdf]);

            Assert.Equal("unchanged", File.ReadAllText(protectedFile));
            Assert.False(File.Exists(Path.Combine(outside, "new.txt")));
        }
        finally
        {
            File.Delete(fileLink);
            if (Directory.Exists(directoryLink))
            {
                Directory.Delete(directoryLink);
            }
            Directory.Delete(root, recursive: true);
        }
    }

    private static void CreatePdf(string filename, string[] embeddedNames)
    {
        using PDDocument document = new();
        document.AddPage(new PDPage());
        Dictionary<string, PDComplexFileSpecification> entries = [];
        foreach (string name in embeddedNames)
        {
            using MemoryStream contents = new(Encoding.UTF8.GetBytes("embedded content"));
            PDComplexFileSpecification file = new();
            file.SetFile(name);
            file.SetEmbeddedFile(new PDEmbeddedFile(document, contents));
            entries.Add(name, file);
        }
        PDEmbeddedFilesNameTreeNode tree = new();
        tree.SetNames(entries);
        new PDDocumentNameDictionary(document.GetDocumentCatalog()).SetEmbeddedFiles(tree);
        document.Save(filename);
    }
}
