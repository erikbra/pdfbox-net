using System.Text;
using System.Text.Json;
using PdfBox.Net.COS;
using PdfBox.Net.ConversionQuality;
using PdfBox.Net.PDModel;

namespace PdfBox.Net.Html.Tests;

public sealed class HtmlReviewArtifactGeneratorTest
{
    [Fact]
    public void Generate_CopiesSourcePdfBesideGeneratedHtmlAndComparisonPage()
    {
        using TempDirectory tempDirectory = new();
        string sourcePdf = Path.Combine(tempDirectory.Path, "source-input.pdf");
        using (PDDocument document = CreateTextDocument())
        {
            document.Save(sourcePdf);
        }

        string manifestPath = Path.Combine(tempDirectory.Path, "manifest.json");
        string outputDirectory = Path.Combine(tempDirectory.Path, "html-review");
        File.WriteAllText(
            manifestPath,
            JsonSerializer.Serialize(
                new
                {
                    schema = 1,
                    examples = new[]
                    {
                        new
                        {
                            id = "review-artifact-sample",
                            title = "Review artifact sample",
                            sourcePdf,
                            notes = "Synthetic test manifest."
                        }
                    }
                },
                new JsonSerializerOptions { WriteIndented = true }));

        HtmlReviewArtifactResult result = HtmlReviewArtifactGenerator.Generate(manifestPath, outputDirectory);

        HtmlReviewExampleResult example = Assert.Single(result.Examples);
        Assert.Equal("review-artifact-sample", example.Id);
        string exampleDirectory = Path.Combine(outputDirectory, "review-artifact-sample");
        string copiedSource = Path.Combine(exampleDirectory, "source.pdf");
        string convertedHtml = Path.Combine(exampleDirectory, "index.html");
        string css = Path.Combine(exampleDirectory, "assets", "pdfbox-net-fixed.css");
        string compare = Path.Combine(exampleDirectory, "compare.html");

        Assert.True(File.Exists(Path.Combine(outputDirectory, "index.html")));
        Assert.True(File.Exists(copiedSource));
        Assert.True(File.Exists(convertedHtml));
        Assert.True(File.Exists(css));
        Assert.True(File.Exists(compare));
        Assert.Equal(File.ReadAllBytes(sourcePdf), File.ReadAllBytes(copiedSource));
        Assert.Contains("source.pdf", File.ReadAllText(compare));
        Assert.Contains("index.html", File.ReadAllText(compare));
        Assert.Contains("review-artifact-sample/compare.html", File.ReadAllText(Path.Combine(outputDirectory, "index.html")));
    }

    private static PDDocument CreateTextDocument()
    {
        PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);

        COSDictionary pageDictionary = (COSDictionary)page.GetCOSObject();
        pageDictionary.SetItem(COSName.RESOURCES, CreateDefaultResourcesDictionary());
        pageDictionary.SetItem(COSName.CONTENTS, CreateContentStream("""
            BT
            /F1 12 Tf
            72 700 Td
            (Review artifact sample) Tj
            ET
            """));
        return document;
    }

    private static COSStream CreateContentStream(string contentStream)
    {
        COSStream stream = new();
        using Stream output = stream.CreateOutputStream();
        byte[] bytes = Encoding.Latin1.GetBytes(contentStream);
        output.Write(bytes, 0, bytes.Length);
        return stream;
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

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "pdfbox-net-html-review-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
