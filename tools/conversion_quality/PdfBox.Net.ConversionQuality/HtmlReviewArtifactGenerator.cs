using System.Net;
using System.Text;
using System.Text.Json;
using PdfBox.Net.Html;
using PdfBox.Net.Layout;
using PdfBox.Net.PDModel;
using PdfBox.Net.Rendering;

namespace PdfBox.Net.ConversionQuality;

public static class HtmlReviewArtifactGenerator
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public static HtmlReviewArtifactResult Generate(string manifestPath, string outputDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(manifestPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);

        string fullManifestPath = Path.GetFullPath(manifestPath);
        string fullOutputDirectory = Path.GetFullPath(outputDirectory);
        string manifestDirectory = Path.GetDirectoryName(fullManifestPath)
            ?? throw new ArgumentException("Manifest path must include a directory.", nameof(manifestPath));

        HtmlReviewManifest manifest = LoadManifest(fullManifestPath);
        if (manifest.Schema != 1)
        {
            throw new InvalidOperationException($"Unsupported HTML review manifest schema {manifest.Schema}.");
        }

        if (manifest.Examples.Count == 0)
        {
            throw new InvalidOperationException("HTML review manifest must contain at least one example.");
        }

        SkiaRenderingBackend.Register();
        RecreateDirectory(fullOutputDirectory);

        List<HtmlReviewExampleResult> results = [];
        foreach (HtmlReviewManifestExample example in manifest.Examples)
        {
            results.Add(GenerateExample(example, manifestDirectory, fullOutputDirectory));
        }

        WriteText(Path.Combine(fullOutputDirectory, "index.html"), RenderIndex(manifest, results));
        WriteText(
            Path.Combine(fullOutputDirectory, "manifest.json"),
            JsonSerializer.Serialize(manifest, JsonOptions) + Environment.NewLine);

        return new HtmlReviewArtifactResult(fullOutputDirectory, results);
    }

    private static HtmlReviewManifest LoadManifest(string manifestPath)
    {
        using FileStream stream = File.OpenRead(manifestPath);
        return JsonSerializer.Deserialize<HtmlReviewManifest>(stream, JsonOptions)
            ?? throw new InvalidOperationException($"{manifestPath} did not contain a valid manifest.");
    }

    private static HtmlReviewExampleResult GenerateExample(
        HtmlReviewManifestExample example,
        string manifestDirectory,
        string outputDirectory)
    {
        if (string.IsNullOrWhiteSpace(example.Id))
        {
            throw new InvalidOperationException("HTML review example is missing id.");
        }

        if (string.IsNullOrWhiteSpace(example.SourcePdf))
        {
            throw new InvalidOperationException($"HTML review example '{example.Id}' is missing sourcePdf.");
        }

        string sourcePdf = ResolvePath(example.SourcePdf, manifestDirectory);
        if (!File.Exists(sourcePdf))
        {
            throw new FileNotFoundException($"HTML review source PDF was not found: {sourcePdf}", sourcePdf);
        }

        string directoryName = SafeDirectoryName(example.Id);
        string exampleDirectory = Path.Combine(outputDirectory, directoryName);
        RecreateDirectory(exampleDirectory);

        PdfLayoutDocument layout;
        PdfHtmlDocument html;
        string capturedConversionWarnings;
        TextWriter originalError = Console.Error;
        using (StringWriter conversionWarnings = new())
        {
            try
            {
                Console.SetError(conversionWarnings);
                using PDDocument document = Loader.LoadPDF(sourcePdf);
                layout = PdfLayoutExtractor.Extract(document, new PdfLayoutOptions
                {
                    IncludeImageAssets = true
                });
                html = PdfHtmlConverter.Convert(layout);
            }
            finally
            {
                Console.SetError(originalError);
            }

            capturedConversionWarnings = conversionWarnings.ToString();
        }

        html.WriteToDirectory(exampleDirectory);
        string copiedSourcePdf = Path.Combine(exampleDirectory, "source.pdf");
        File.Copy(sourcePdf, copiedSourcePdf, overwrite: true);
        PdfHtmlQualityReport qualityReport = new PdfHtmlQualityProbe()
            .AnalyzeAsync(new PdfHtmlQualityProbeOptions(
                copiedSourcePdf,
                exampleDirectory,
                layout,
                Path.Combine(exampleDirectory, "quality"),
                example.QualityPages ?? 2))
            .GetAwaiter()
            .GetResult();

        HtmlReviewExampleResult result = new(
            example.Id,
            example.Title ?? example.Id,
            example.Notes ?? "",
            exampleDirectory,
            PageCount: layout.Pages.Count,
            TextRuns: layout.Pages.Sum(page => page.Runs.Count),
            TextLines: layout.Pages.Sum(page => page.Lines.Count),
            ImagePlacements: layout.Pages.Sum(page => page.Images.Count),
            VectorPaths: layout.Pages.Sum(page => page.Paths.Count),
            ExportedAssets: html.Assets.Count,
            Links: layout.Pages.Sum(page => page.Links.Count),
            Diagnostics: layout.Diagnostics.Count + layout.Pages.Sum(page => page.Diagnostics.Count) +
                CountNonEmptyLines(capturedConversionWarnings),
            QualityStatus: qualityReport.Status,
            QualityChecksNeedingReview: qualityReport.Checks.Count(static check => check.Status == "needs-review"));

        WriteText(Path.Combine(exampleDirectory, "summary.md"), RenderExampleSummary(result, layout));
        WriteText(Path.Combine(exampleDirectory, "diagnostics.txt"), RenderDiagnostics(layout, capturedConversionWarnings));
        WriteText(Path.Combine(exampleDirectory, "compare.html"), RenderComparePage(result));
        return result;
    }

    private static string ResolvePath(string value, string baseDirectory)
    {
        if (Path.IsPathRooted(value))
        {
            return Path.GetFullPath(value);
        }

        return Path.GetFullPath(Path.Combine(baseDirectory, value));
    }

    private static string SafeDirectoryName(string value)
    {
        StringBuilder builder = new(value.Length);
        foreach (char character in value)
        {
            bool allowed = character is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or
                (>= '0' and <= '9') or '-' or '_';
            builder.Append(allowed ? character : '-');
        }

        string safe = builder.ToString().Trim('-');
        if (safe.Length == 0)
        {
            throw new InvalidOperationException($"HTML review example id '{value}' cannot be used as a directory name.");
        }

        return safe;
    }

    private static void RecreateDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }

        Directory.CreateDirectory(path);
    }

    private static void WriteText(string path, string text)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text, Utf8NoBom);
    }

    private static string RenderIndex(HtmlReviewManifest manifest, IReadOnlyList<HtmlReviewExampleResult> results)
    {
        StringBuilder html = new();
        html.AppendLine("<!doctype html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("  <meta charset=\"utf-8\" />");
        html.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
        html.AppendLine("  <title>PDF conversion HTML review artifacts</title>");
        html.AppendLine("  <style>");
        html.AppendLine("    body{font-family:Arial,sans-serif;margin:24px;color:#111827;background:#f9fafb}");
        html.AppendLine("    table{border-collapse:collapse;width:100%;background:#fff;border:1px solid #d1d5db}");
        html.AppendLine("    th,td{border-bottom:1px solid #e5e7eb;padding:8px 10px;text-align:left;vertical-align:top}");
        html.AppendLine("    th{background:#f3f4f6;font-size:13px}");
        html.AppendLine("    a{color:#0f5ea8}");
        html.AppendLine("    .note{max-width:960px}");
        html.AppendLine("  </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine("  <h1>PDF conversion HTML review artifacts</h1>");
        if (!string.IsNullOrWhiteSpace(manifest.Description))
        {
            html.Append("  <p class=\"note\">");
            html.Append(WebUtility.HtmlEncode(manifest.Description));
            html.AppendLine("</p>");
        }

        html.AppendLine("  <table>");
        html.AppendLine("    <thead><tr><th>Example</th><th>Links</th><th>Pages</th><th>Text runs</th><th>Images</th><th>Paths</th><th>Diagnostics</th><th>Quality</th></tr></thead>");
        html.AppendLine("    <tbody>");
        foreach (HtmlReviewExampleResult result in results)
        {
            string directoryName = SafeDirectoryName(result.Id);
            html.Append("      <tr><td>");
            html.Append(WebUtility.HtmlEncode(result.Title));
            html.Append("</td><td>");
            html.Append($"<a href=\"{directoryName}/compare.html\">compare</a> ");
            html.Append($"<a href=\"{directoryName}/source.pdf\">source PDF</a> ");
            html.Append($"<a href=\"{directoryName}/index.html\">HTML</a> ");
            html.Append($"<a href=\"{directoryName}/summary.md\">summary</a>");
            html.Append("</td><td>");
            html.Append(result.PageCount.ToString());
            html.Append("</td><td>");
            html.Append(result.TextRuns.ToString());
            html.Append("</td><td>");
            html.Append(result.ImagePlacements.ToString());
            html.Append("</td><td>");
            html.Append(result.VectorPaths.ToString());
            html.Append("</td><td>");
            html.Append(result.Diagnostics.ToString());
            html.Append("</td><td>");
            html.Append(WebUtility.HtmlEncode(result.QualityStatus));
            if (result.QualityChecksNeedingReview > 0)
            {
                html.Append(" (");
                html.Append(result.QualityChecksNeedingReview.ToString());
                html.Append(")");
            }

            html.Append($" <a href=\"{directoryName}/quality/quality-report.md\">report</a>");
            html.AppendLine("</td></tr>");
        }

        html.AppendLine("    </tbody>");
        html.AppendLine("  </table>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        return html.ToString();
    }

    private static string RenderExampleSummary(HtmlReviewExampleResult result, PdfLayoutDocument layout)
    {
        string preview = NormalizeWhitespace(layout.Text);
        if (preview.Length > 700)
        {
            preview = preview[..700] + "...";
        }

        StringBuilder summary = new();
        summary.AppendLine($"# {result.Title}");
        summary.AppendLine();
        if (result.Notes.Length > 0)
        {
            summary.AppendLine(result.Notes);
            summary.AppendLine();
        }

        summary.AppendLine("- Source PDF: [source.pdf](source.pdf)");
        summary.AppendLine("- Converted HTML: [index.html](index.html)");
        summary.AppendLine("- Side-by-side comparison: [compare.html](compare.html)");
        summary.AppendLine("- Quality probe: [quality/quality-report.md](quality/quality-report.md)");
        summary.AppendLine($"- Pages: {result.PageCount}");
        summary.AppendLine($"- Text runs: {result.TextRuns}");
        summary.AppendLine($"- Text lines: {result.TextLines}");
        summary.AppendLine($"- Image placements: {result.ImagePlacements}");
        summary.AppendLine($"- Vector paths: {result.VectorPaths}");
        summary.AppendLine($"- Exported assets: {result.ExportedAssets}");
        summary.AppendLine($"- Links: {result.Links}");
        summary.AppendLine($"- Diagnostics: {result.Diagnostics}");
        summary.AppendLine($"- Quality status: {result.QualityStatus}");
        summary.AppendLine($"- Quality checks needing review: {result.QualityChecksNeedingReview}");
        summary.AppendLine();
        summary.AppendLine("## Text Preview");
        summary.AppendLine();
        summary.AppendLine(preview.Length == 0 ? "_No extracted text._" : preview);
        return summary.ToString();
    }

    private static string RenderDiagnostics(PdfLayoutDocument layout, string capturedConversionWarnings)
    {
        List<PdfLayoutDiagnostic> diagnostics =
        [
            .. layout.Diagnostics,
            .. layout.Pages.SelectMany(page => page.Diagnostics)
        ];

        if (diagnostics.Count == 0 && CountNonEmptyLines(capturedConversionWarnings) == 0)
        {
            return "No diagnostics." + Environment.NewLine;
        }

        StringBuilder text = new();
        if (CountNonEmptyLines(capturedConversionWarnings) > 0)
        {
            text.AppendLine("Conversion warnings:");
            text.AppendLine(capturedConversionWarnings.TrimEnd());
            text.AppendLine();
        }

        foreach (PdfLayoutDiagnostic diagnostic in diagnostics)
        {
            if (diagnostic.PageNumber is int pageNumber)
            {
                text.Append($"page {pageNumber}: ");
            }

            text.Append(diagnostic.Severity);
            text.Append(' ');
            text.Append(diagnostic.Code);
            text.Append(": ");
            text.AppendLine(diagnostic.Message);
        }

        return text.ToString();
    }

    private static int CountNonEmptyLines(string value)
    {
        return value
            .Split(["\r\n", "\n", "\r"], StringSplitOptions.None)
            .Count(line => line.Trim().Length > 0);
    }

    private static string RenderComparePage(HtmlReviewExampleResult result)
    {
        StringBuilder html = new();
        html.AppendLine("<!doctype html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("  <meta charset=\"utf-8\" />");
        html.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
        html.Append("  <title>");
        html.Append(WebUtility.HtmlEncode(result.Title));
        html.AppendLine(" comparison</title>");
        html.AppendLine("  <style>");
        html.AppendLine("    *{box-sizing:border-box}");
        html.AppendLine("    body{margin:0;font-family:Arial,sans-serif;color:#111827;background:#f3f4f6}");
        html.AppendLine("    header{height:48px;display:flex;align-items:center;gap:16px;padding:0 16px;background:#fff;border-bottom:1px solid #d1d5db}");
        html.AppendLine("    h1{font-size:16px;margin:0;flex:1;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}");
        html.AppendLine("    a{color:#0f5ea8}");
        html.AppendLine("    main{display:grid;grid-template-columns:1fr 1fr;height:calc(100vh - 48px)}");
        html.AppendLine("    section{min-width:0;border-right:1px solid #d1d5db;display:grid;grid-template-rows:32px 1fr}");
        html.AppendLine("    section:last-child{border-right:0}");
        html.AppendLine("    h2{font-size:13px;margin:0;padding:8px 12px;background:#e5e7eb}");
        html.AppendLine("    iframe{border:0;width:100%;height:100%;background:#fff}");
        html.AppendLine("    @media (max-width:900px){main{grid-template-columns:1fr;height:auto}section{height:80vh;border-right:0;border-bottom:1px solid #d1d5db}}");
        html.AppendLine("  </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.Append("  <header><h1>");
        html.Append(WebUtility.HtmlEncode(result.Title));
        html.AppendLine("</h1><a href=\"source.pdf\">source PDF</a><a href=\"index.html\">generated HTML</a><a href=\"quality/quality-report.md\">quality report</a><a href=\"summary.md\">summary</a></header>");
        html.AppendLine("  <main>");
        html.AppendLine("    <section><h2>Source PDF</h2><iframe title=\"Source PDF\" src=\"source.pdf\"></iframe></section>");
        html.AppendLine("    <section><h2>Generated HTML</h2><iframe title=\"Generated HTML\" src=\"index.html\"></iframe></section>");
        html.AppendLine("  </main>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        return html.ToString();
    }

    private static string NormalizeWhitespace(string value)
    {
        return string.Join(" ", value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}

public sealed record HtmlReviewArtifactResult(
    string OutputDirectory,
    IReadOnlyList<HtmlReviewExampleResult> Examples);

public sealed record HtmlReviewExampleResult(
    string Id,
    string Title,
    string Notes,
    string Directory,
    int PageCount,
    int TextRuns,
    int TextLines,
    int ImagePlacements,
    int VectorPaths,
    int ExportedAssets,
    int Links,
    int Diagnostics,
    string QualityStatus,
    int QualityChecksNeedingReview);

public sealed class HtmlReviewManifest
{
    public int Schema { get; set; }

    public string? Description { get; set; }

    public List<HtmlReviewManifestExample> Examples { get; set; } = [];
}

public sealed class HtmlReviewManifestExample
{
    public string Id { get; set; } = "";

    public string? Title { get; set; }

    public string SourcePdf { get; set; } = "";

    public string? Notes { get; set; }

    public int? QualityPages { get; set; }
}
