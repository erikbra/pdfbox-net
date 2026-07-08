using System.Text;
using System.Text.Json;
using PdfBox.Net.COS;
using PdfBox.Net.ConversionQuality;
using PdfBox.Net.Layout;
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
        string semanticHtml = Path.Combine(exampleDirectory, "semantic", "index.html");
        string semanticCss = Path.Combine(exampleDirectory, "semantic", "assets", "pdfbox-net-semantic.css");
        string continuousSemanticHtml = Path.Combine(exampleDirectory, "semantic-continuous", "index.html");
        string continuousSemanticCss = Path.Combine(exampleDirectory, "semantic-continuous", "assets", "pdfbox-net-semantic-continuous.css");
        string compare = Path.Combine(exampleDirectory, "compare.html");
        string qualityReportJson = Path.Combine(exampleDirectory, "quality", "quality-report.json");
        string qualityReportMarkdown = Path.Combine(exampleDirectory, "quality", "quality-report.md");

        Assert.True(File.Exists(Path.Combine(outputDirectory, "index.html")));
        Assert.True(File.Exists(copiedSource));
        Assert.True(File.Exists(convertedHtml));
        Assert.True(File.Exists(css));
        Assert.True(File.Exists(semanticHtml));
        Assert.True(File.Exists(semanticCss));
        Assert.True(File.Exists(continuousSemanticHtml));
        Assert.True(File.Exists(continuousSemanticCss));
        Assert.True(File.Exists(compare));
        Assert.True(File.Exists(qualityReportJson));
        Assert.True(File.Exists(qualityReportMarkdown));
        Assert.Equal(File.ReadAllBytes(sourcePdf), File.ReadAllBytes(copiedSource));
        string comparisonHtml = File.ReadAllText(compare);
        Assert.Contains("source.pdf", comparisonHtml);
        Assert.Contains("semantic-continuous/index.html", comparisonHtml);
        Assert.Contains("quality/quality-report.md", comparisonHtml);
        Assert.DoesNotContain("Fixed-layout HTML", comparisonHtml);
        Assert.DoesNotContain("<h2>Semantic HTML</h2>", comparisonHtml);
        Assert.DoesNotContain("semantic/index.html", comparisonHtml);
        string artifactIndex = File.ReadAllText(Path.Combine(outputDirectory, "index.html"));
        Assert.Contains("review-artifact-sample/compare.html", artifactIndex);
        Assert.DoesNotContain("review-artifact-sample/semantic/index.html", artifactIndex);
        Assert.Contains("review-artifact-sample/semantic-continuous/index.html", artifactIndex);
        Assert.Contains("review-artifact-sample/quality/quality-report.md", artifactIndex);
        string summary = File.ReadAllText(Path.Combine(exampleDirectory, "summary.md"));
        Assert.Contains("semantic-continuous/index.html", summary);
        Assert.DoesNotContain("semantic/index.html", summary);

        using JsonDocument quality = JsonDocument.Parse(File.ReadAllText(qualityReportJson));
        Assert.Equal(1, quality.RootElement.GetProperty("Schema").GetInt32());
        Assert.True(quality.RootElement.GetProperty("IssueCategories").GetArrayLength() > 0);
        Assert.True(example.QualityStatus.Length > 0);
        string[] qualityArtifacts = quality.RootElement.GetProperty("Artifacts")
            .EnumerateArray()
            .Select(static artifact => artifact.GetString() ?? "")
            .ToArray();
        if (qualityArtifacts.Contains("page-1-diff.png", StringComparer.Ordinal))
        {
            Assert.Contains("review-artifact-sample/quality/page-1-diff.png", artifactIndex);
            Assert.Contains("quality/page-1-diff.png", File.ReadAllText(Path.Combine(exampleDirectory, "summary.md")));
        }
    }

    [Fact]
    public async Task AnalyzeAsync_ReportsWordBoundaryLossWithoutFailingProbe()
    {
        using TempDirectory tempDirectory = new();
        string sourcePdf = Path.Combine(tempDirectory.Path, "source.pdf");
        PdfLayoutDocument layout;
        using (PDDocument document = CreateTextDocument())
        {
            document.Save(sourcePdf);
            layout = PdfLayoutExtractor.Extract(document);
        }

        string htmlDirectory = Path.Combine(tempDirectory.Path, "html");
        Directory.CreateDirectory(htmlDirectory);
        File.WriteAllText(
            Path.Combine(htmlDirectory, "index.html"),
            """
            <!doctype html>
            <html lang="en">
            <head><meta charset="utf-8" /></head>
            <body style="margin:0;background:#f3f4f6">
              <section class="pdf-page" data-page-number="1" style="position:relative;width:612pt;height:792pt;background:white">
                <span class="pdf-text-run" style="position:absolute;left:72pt;top:80pt;font-size:12pt;font-family:Arial,sans-serif">Reviewartifactsample</span>
              </section>
            </body>
            </html>
            """);

        string outputDirectory = Path.Combine(tempDirectory.Path, "quality");
        PdfHtmlQualityReport report = await new PdfHtmlQualityProbe().AnalyzeAsync(new PdfHtmlQualityProbeOptions(
            sourcePdf,
            htmlDirectory,
            layout,
            outputDirectory,
            MaxPages: 1),
            TestContext.Current.CancellationToken);

        Assert.Equal("needs-review", report.Status);
        Assert.Contains(report.Checks, check => check.Id == "word-boundaries" && check.Status == "needs-review");
        Assert.True(File.Exists(Path.Combine(outputDirectory, "quality-report.json")));
        Assert.True(File.Exists(Path.Combine(outputDirectory, "quality-report.md")));
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
