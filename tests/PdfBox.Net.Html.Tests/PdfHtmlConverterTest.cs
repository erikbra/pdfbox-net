using System.Buffers.Binary;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Playwright;
using PdfBox.Net.COS;
using PdfBox.Net.Html;
using PdfBox.Net.Layout;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.PDModel.Interactive.Action;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.Rendering;

namespace PdfBox.Net.Html.Tests;

public class PdfHtmlConverterTest
{
    [Fact]
    public void Convert_EmitsPageContainersMatchingLayoutDimensions()
    {
        using PDDocument document = CreateTextDocument("""
            BT
            /F1 12 Tf
            72 700 Td
            (Hello HTML) Tj
            ET
            """);
        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document);

        PdfHtmlDocument html = PdfHtmlConverter.Convert(layout);
        XDocument dom = ParseHtml(html.Html);

        XElement page = Assert.Single(ElementsByClass(dom, "pdf-page"));
        PdfLayoutPage layoutPage = Assert.Single(layout.Pages);
        Assert.Equal("1", page.Attribute("data-page-number")?.Value);
        Dictionary<string, string> style = ParseStyle(page.Attribute("style")?.Value ?? "");
        AssertClose(layoutPage.Width, ParsePoints(style["width"]));
        AssertClose(layoutPage.Height, ParsePoints(style["height"]));
    }

    [Fact]
    public void Convert_EmitsSelectablePositionedTextWithHighCoverage()
    {
        using PDDocument document = CreateTextDocument("""
            BT
            /F1 12 Tf
            72 700 Td
            (First line) Tj
            0 -24 Td
            (Second line) Tj
            ET
            """);
        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document);

        PdfHtmlDocument html = PdfHtmlConverter.Convert(layout);
        XDocument dom = ParseHtml(html.Html);
        XElement[] spans = ElementsByClass(dom, "pdf-text-run").ToArray();

        Assert.Equal(layout.Pages[0].Runs.Count, spans.Length);
        Assert.True(TextCoverage(layout.Text, dom.Root?.Value ?? "") >= 0.99);
        Assert.All(spans, span =>
        {
            Dictionary<string, string> style = ParseStyle(span.Attribute("style")?.Value ?? "");
            Assert.Equal("absolute", style["position"]);
            Assert.True(ParsePoints(style["left"]) >= 0);
            Assert.True(ParsePoints(style["top"]) >= 0);
            Assert.True(ParsePoints(style["font-size"]) > 0);
        });
    }

    [Fact]
    public void Convert_SemanticTextMode_EmitsGroupedArxivElements()
    {
        using PDDocument document = Loader.LoadPDF(Path.Combine(AppContext.BaseDirectory, "Fixtures", "arxiv-sample.pdf"));
        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document, new PdfLayoutOptions
        {
            IncludeImages = false,
            IncludeLinks = false,
            IncludePaths = false
        });

        PdfHtmlDocument html = PdfHtmlConverter.Convert(layout, new PdfHtmlOptions
        {
            TextMode = PdfHtmlTextMode.Semantic
        });
        XDocument dom = ParseHtml(html.Html);

        Assert.Empty(ElementsByClass(dom, "pdf-text-run"));
        XElement title = Assert.Single(dom.Descendants("h1"), element =>
            element.Value.Contains("Attention Is All You Need", StringComparison.Ordinal));
        Assert.Equal("heading", title.Attribute("data-semantic-kind")?.Value);

        XElement[] authors = ElementsByClass(dom, "pdf-semantic-author-block").ToArray();
        Assert.Equal(8, authors.Length);
        Assert.All(authors, author => Assert.Equal("address", author.Name.LocalName));
        Assert.Contains(authors, author =>
            author.Value.Contains("Ashish Vaswani", StringComparison.Ordinal) &&
            author.Value.Contains("avaswani@google.com", StringComparison.Ordinal));
        Assert.Contains(authors, author =>
            author.Value.Contains("Illia Polosukhin", StringComparison.Ordinal) &&
            author.Value.Contains("illia.polosukhin@gmail.com", StringComparison.Ordinal));

        XElement abstractHeading = Assert.Single(dom.Descendants("h2"), element => element.Value == "Abstract");
        Assert.Equal("heading", abstractHeading.Attribute("data-semantic-kind")?.Value);
        Assert.Contains(ElementsByClass(dom, "pdf-semantic-paragraph"), paragraph =>
            paragraph.Value.StartsWith("The dominant sequence transduction models", StringComparison.Ordinal) &&
            paragraph.Value.Contains("large and limited training data.", StringComparison.Ordinal));

        Assert.Contains(dom.Descendants("h1"), heading => heading.Value == "1 Introduction");
        XElement[] footnotes = ElementsByClass(dom, "pdf-semantic-footnote").ToArray();
        Assert.Equal(3, footnotes.Length);
        Assert.All(footnotes, footnote => Assert.Equal("aside", footnote.Name.LocalName));
        Assert.Contains(ElementsByClass(dom, "pdf-semantic-footer"), footer =>
            footer.Value.Contains("31st Conference", StringComparison.Ordinal));
    }

    [Fact]
    public void Convert_ForegroundBoxMaskMatchesLayoutRuns()
    {
        using PDDocument document = CreateTextDocument("""
            BT
            /F1 12 Tf
            72 700 Td
            (Box mask) Tj
            0 -24 Td
            (Second) Tj
            ET
            """);
        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document);

        PdfHtmlDocument html = PdfHtmlConverter.Convert(layout);
        XDocument dom = ParseHtml(html.Html);
        PdfLayoutRectangle[] expected = layout.Pages[0].Runs.Select(run => run.Bounds).ToArray();
        PdfLayoutRectangle[] actual = ElementsByClass(dom, "pdf-text-run")
            .Select(span => RectangleFromStyle(ParseStyle(span.Attribute("style")?.Value ?? "")))
            .ToArray();

        Assert.Equal(expected.Length, actual.Length);
        Assert.True(ForegroundIntersectionOverUnion(expected, actual) >= 0.995);
    }

    [Fact]
    public async Task Convert_RenderedInHeadlessBrowserMatchesLayoutGeometry()
    {
        using PDDocument document = CreateTextDocument("""
            BT
            /F1 12 Tf
            72 700 Td
            (Browser geometry) Tj
            0 -24 Td
            (Second browser line) Tj
            ET
            """);
        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document);
        PdfHtmlDocument html = PdfHtmlConverter.Convert(layout);

        using TempDirectory tempDirectory = new();
        html.WriteToDirectory(tempDirectory.Path);

        using IPlaywright playwright = await Playwright.CreateAsync();
        await using IBrowser browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        IPage page = await browser.NewPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize
            {
                Width = 1000,
                Height = 1200
            }
        });
        await page.GotoAsync(new Uri(Path.Combine(tempDirectory.Path, "index.html")).AbsoluteUri);

        string artifactDirectory = ArtifactDirectory(nameof(Convert_RenderedInHeadlessBrowserMatchesLayoutGeometry));
        BrowserRenderComparison comparison = await CompareRenderedGeometryAsync(document, layout, page);
        if (comparison.Mismatches.Count > 0)
        {
            await WriteBrowserMismatchArtifactsAsync(html, page, artifactDirectory, comparison);
        }

        Assert.True(
            comparison.Mismatches.Count == 0,
            string.Join(Environment.NewLine, comparison.Mismatches) + Environment.NewLine + $"Artifacts: {artifactDirectory}");
    }

    [Fact]
    public void Convert_EmitsLinkOverlayWithUriAndBounds()
    {
        using PDDocument document = CreateLinkedTextDocument();
        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document);

        PdfHtmlDocument html = PdfHtmlConverter.Convert(layout);
        XDocument dom = ParseHtml(html.Html);

        XElement link = Assert.Single(ElementsByClass(dom, "pdf-link-overlay"));
        Assert.Equal("https://example.com/pdfbox", link.Attribute("href")?.Value);
        Assert.Equal("uri", link.Attribute("data-link-kind")?.Value);
        Assert.Equal("https://example.com/pdfbox", link.Attribute("data-uri")?.Value);
        Assert.Equal("https://example.com/pdfbox", link.Attribute("aria-label")?.Value);
        Dictionary<string, string> style = ParseStyle(link.Attribute("style")?.Value ?? "");
        Assert.Equal("absolute", style["position"]);
        AssertClose(72, ParsePoints(style["left"]));
        AssertClose(88, ParsePoints(style["top"]));
        AssertClose(120, ParsePoints(style["width"]));
        AssertClose(24, ParsePoints(style["height"]));
    }

    [Fact]
    public void Convert_EmitsImageElementWithExportedAssetAndBounds()
    {
        using PDDocument document = CreateImageDocument();
        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document, new PdfLayoutOptions
        {
            IncludeImageAssets = true
        });

        PdfHtmlDocument html = PdfHtmlConverter.Convert(layout);
        XDocument dom = ParseHtml(html.Html);

        Assert.Empty(layout.Diagnostics);
        PdfHtmlAsset asset = Assert.Single(html.Assets);
        Assert.Equal("assets/images/page-1-image-0.png", asset.RelativePath);
        Assert.Equal("image/png", asset.ContentType);
        Assert.Equal((2, 2), PngDimensions(asset.Data));
        XElement image = Assert.Single(ElementsByClass(dom, "pdf-image"));
        Assert.Equal(asset.RelativePath, image.Attribute("src")?.Value);
        Assert.Equal("page-1-image-0", image.Attribute("data-asset-id")?.Value);
        Assert.Equal("Im0", image.Attribute("data-source-name")?.Value);
        Dictionary<string, string> style = ParseStyle(image.Attribute("style")?.Value ?? "");
        Assert.Equal("absolute", style["position"]);
        AssertClose(72, ParsePoints(style["left"]));
        AssertClose(132, ParsePoints(style["top"]));
        AssertClose(120, ParsePoints(style["width"]));
        AssertClose(60, ParsePoints(style["height"]));
    }

    [Fact]
    public void Convert_EmitsInlineImageElementWithExportedAsset()
    {
        using PDDocument document = CreateInlineImageDocument();
        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document, new PdfLayoutOptions
        {
            IncludeImageAssets = true
        });

        PdfHtmlDocument html = PdfHtmlConverter.Convert(layout);
        XDocument dom = ParseHtml(html.Html);

        PdfLayoutImage layoutImage = Assert.Single(Assert.Single(layout.Pages).Images);
        Assert.Equal(PdfLayoutImageKind.InlineImage, layoutImage.Kind);
        Assert.Empty(layout.Diagnostics);
        PdfHtmlAsset asset = Assert.Single(html.Assets);
        Assert.Equal("assets/images/page-1-image-0.png", asset.RelativePath);
        Assert.Equal((2, 2), PngDimensions(asset.Data));
        XElement image = Assert.Single(ElementsByClass(dom, "pdf-image"));
        Assert.Equal(asset.RelativePath, image.Attribute("src")?.Value);
        Assert.Equal("page-1-image-0", image.Attribute("data-asset-id")?.Value);
        Assert.Null(image.Attribute("data-source-name"));
    }

    [Fact]
    public async Task Convert_RenderedImageInHeadlessBrowserMatchesLayoutGeometry()
    {
        using PDDocument document = CreateImageDocument();
        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document, new PdfLayoutOptions
        {
            IncludeImageAssets = true
        });
        PdfHtmlDocument html = PdfHtmlConverter.Convert(layout);

        using TempDirectory tempDirectory = new();
        html.WriteToDirectory(tempDirectory.Path);

        using IPlaywright playwright = await Playwright.CreateAsync();
        await using IBrowser browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        IPage page = await browser.NewPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize
            {
                Width = 1000,
                Height = 1200
            }
        });
        await page.GotoAsync(new Uri(Path.Combine(tempDirectory.Path, "index.html")).AbsoluteUri);

        const float cssPixelsPerPoint = 96f / 72f;
        const float tolerancePx = 1.0f;
        PdfLayoutImage layoutImage = Assert.Single(Assert.Single(layout.Pages).Images);
        ILocator pageLocator = page.Locator(".pdf-page");
        ILocator imageLocator = page.Locator(".pdf-image");
        await imageLocator.WaitForAsync();
        LocatorBoundingBoxResult pageBox = await pageLocator.BoundingBoxAsync()
            ?? throw new InvalidOperationException("Page did not render a bounding box.");
        LocatorBoundingBoxResult imageBox = await imageLocator.BoundingBoxAsync()
            ?? throw new InvalidOperationException("Image did not render a bounding box.");

        AssertWithin(tolerancePx, layoutImage.Bounds.X * cssPixelsPerPoint, (float)(imageBox.X - pageBox.X));
        AssertWithin(tolerancePx, layoutImage.Bounds.Y * cssPixelsPerPoint, (float)(imageBox.Y - pageBox.Y));
        AssertWithin(tolerancePx, layoutImage.Bounds.Width * cssPixelsPerPoint, (float)imageBox.Width);
        AssertWithin(tolerancePx, layoutImage.Bounds.Height * cssPixelsPerPoint, (float)imageBox.Height);
    }

    [Fact]
    public void Convert_EmitsVectorSvgOverlayWithPathStyle()
    {
        using PDDocument document = CreateTextDocument("""
            q
            2 w
            1 0 0 RG
            0.1 0.6 0.2 rg
            72 600 120 60 re
            B
            Q
            """);
        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document);

        PdfHtmlDocument html = PdfHtmlConverter.Convert(layout);
        XDocument dom = ParseHtml(html.Html);

        XElement svg = Assert.Single(ElementsByClass(dom, "pdf-vector-layer"));
        Assert.Equal("1", svg.Attribute("data-path-count")?.Value);
        Assert.Equal("0 0 612 792", svg.Attribute("viewBox")?.Value);
        XElement path = Assert.Single(ElementsByClass(dom, "pdf-vector-path"));
        Assert.Equal("0", path.Attribute("data-path-index")?.Value);
        Assert.Equal("M 72 192 L 192 192 L 192 132 L 72 132 Z", path.Attribute("d")?.Value);
        Assert.Equal("#1A9933", path.Attribute("fill")?.Value);
        Assert.Equal("1", path.Attribute("fill-opacity")?.Value);
        Assert.Equal("nonzero", path.Attribute("fill-rule")?.Value);
        Assert.Equal("#FF0000", path.Attribute("stroke")?.Value);
        Assert.Equal("2", path.Attribute("stroke-width")?.Value);
        Assert.Equal("butt", path.Attribute("stroke-linecap")?.Value);
        Assert.Equal("miter", path.Attribute("stroke-linejoin")?.Value);
    }

    [Fact]
    public async Task Convert_RenderedVectorPathInHeadlessBrowserMatchesLayoutGeometry()
    {
        using PDDocument document = CreateTextDocument("""
            0.1 0.6 0.2 rg
            72 600 120 60 re
            f
            """);
        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document);
        PdfHtmlDocument html = PdfHtmlConverter.Convert(layout);

        using TempDirectory tempDirectory = new();
        html.WriteToDirectory(tempDirectory.Path);

        using IPlaywright playwright = await Playwright.CreateAsync();
        await using IBrowser browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        IPage page = await browser.NewPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize
            {
                Width = 1000,
                Height = 1200
            }
        });
        await page.GotoAsync(new Uri(Path.Combine(tempDirectory.Path, "index.html")).AbsoluteUri);

        const float cssPixelsPerPoint = 96f / 72f;
        const float tolerancePx = 1.0f;
        PdfLayoutPath layoutPath = Assert.Single(Assert.Single(layout.Pages).Paths);
        ILocator pageLocator = page.Locator(".pdf-page");
        ILocator pathLocator = page.Locator(".pdf-vector-path");
        await pathLocator.WaitForAsync();
        LocatorBoundingBoxResult pageBox = await pageLocator.BoundingBoxAsync()
            ?? throw new InvalidOperationException("Page did not render a bounding box.");
        LocatorBoundingBoxResult pathBox = await pathLocator.BoundingBoxAsync()
            ?? throw new InvalidOperationException("Vector path did not render a bounding box.");

        List<string> mismatches = [];
        AddMismatchIfOutsideTolerance(mismatches, "vector path x", layoutPath.Bounds.X * cssPixelsPerPoint, (float)(pathBox.X - pageBox.X), tolerancePx);
        AddMismatchIfOutsideTolerance(mismatches, "vector path y", layoutPath.Bounds.Y * cssPixelsPerPoint, (float)(pathBox.Y - pageBox.Y), tolerancePx);
        AddMismatchIfOutsideTolerance(mismatches, "vector path width", layoutPath.Bounds.Width * cssPixelsPerPoint, (float)pathBox.Width, tolerancePx);
        AddMismatchIfOutsideTolerance(mismatches, "vector path height", layoutPath.Bounds.Height * cssPixelsPerPoint, (float)pathBox.Height, tolerancePx);

        if (mismatches.Count > 0)
        {
            string artifactDirectory = ArtifactDirectory(nameof(Convert_RenderedVectorPathInHeadlessBrowserMatchesLayoutGeometry));
            await WriteGeometryMismatchArtifactsAsync(html, page, artifactDirectory, mismatches);
        }

        Assert.True(
            mismatches.Count == 0,
            string.Join(Environment.NewLine, mismatches) + Environment.NewLine +
            $"Artifacts: {ArtifactDirectory(nameof(Convert_RenderedVectorPathInHeadlessBrowserMatchesLayoutGeometry))}");
    }

    [Fact]
    public void WriteToDirectory_EmitsStableFilesWithNoBrokenLocalReferences()
    {
        using PDDocument document = CreateTextDocument("""
            BT
            /F1 12 Tf
            72 700 Td
            (Assets) Tj
            ET
            """);
        PdfHtmlDocument html = PdfHtmlConverter.Convert(PdfLayoutExtractor.Extract(document));

        using TempDirectory tempDirectory = new();
        html.WriteToDirectory(tempDirectory.Path);

        string indexPath = Path.Combine(tempDirectory.Path, "index.html");
        string cssPath = Path.Combine(tempDirectory.Path, "assets", "pdfbox-net-fixed.css");
        Assert.True(File.Exists(indexPath));
        Assert.True(File.Exists(cssPath));
        Assert.Empty(BrokenLocalReferences(indexPath));
    }

    [Fact]
    public void WriteToDirectory_LinkOverlayDoesNotCreateBrokenLocalReference()
    {
        using PDDocument document = CreateLinkedTextDocument();
        PdfHtmlDocument html = PdfHtmlConverter.Convert(PdfLayoutExtractor.Extract(document));

        using TempDirectory tempDirectory = new();
        html.WriteToDirectory(tempDirectory.Path);

        Assert.Empty(BrokenLocalReferences(Path.Combine(tempDirectory.Path, "index.html")));
    }

    [Fact]
    public void WriteToDirectory_ImageAssetDoesNotCreateBrokenLocalReference()
    {
        using PDDocument document = CreateImageDocument();
        PdfHtmlDocument html = PdfHtmlConverter.Convert(PdfLayoutExtractor.Extract(document, new PdfLayoutOptions
        {
            IncludeImageAssets = true
        }));

        using TempDirectory tempDirectory = new();
        html.WriteToDirectory(tempDirectory.Path);

        string indexPath = Path.Combine(tempDirectory.Path, "index.html");
        Assert.Empty(BrokenLocalReferences(indexPath));
        PdfHtmlAsset asset = Assert.Single(html.Assets);
        string assetPath = Path.Combine(tempDirectory.Path, asset.RelativePath.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(assetPath));
        Assert.Equal(asset.Data, File.ReadAllBytes(assetPath));
    }

    [Fact]
    public void Convert_OutputIsDeterministic()
    {
        using PDDocument document = CreateTextDocument("""
            BT
            /F1 12 Tf
            72 700 Td
            (Stable) Tj
            ET
            """);
        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document);

        PdfHtmlDocument first = PdfHtmlConverter.Convert(layout);
        PdfHtmlDocument second = PdfHtmlConverter.Convert(layout);

        Assert.Equal(first.Html, second.Html);
        Assert.Equal(first.Css, second.Css);
        Assert.Equal(first.CssPath, second.CssPath);
    }

    private static async Task<BrowserRenderComparison> CompareRenderedGeometryAsync(
        PDDocument document,
        PdfLayoutDocument layout,
        IPage page)
    {
        const float cssPixelsPerPoint = 96f / 72f;
        const float geometryTolerancePx = 1.0f;

        List<string> mismatches = new();
        PdfLayoutPage layoutPage = Assert.Single(layout.Pages);
        ILocator pageLocator = page.Locator(".pdf-page");
        LocatorBoundingBoxResult? pageBox = await pageLocator.BoundingBoxAsync();
        if (pageBox == null)
        {
            return new BrowserRenderComparison(["No .pdf-page element was rendered."], [], []);
        }

        float expectedPageWidth = layoutPage.Width * cssPixelsPerPoint;
        float expectedPageHeight = layoutPage.Height * cssPixelsPerPoint;
        AddMismatchIfOutsideTolerance(mismatches, "page width", expectedPageWidth, (float)pageBox.Width, geometryTolerancePx);
        AddMismatchIfOutsideTolerance(mismatches, "page height", expectedPageHeight, (float)pageBox.Height, geometryTolerancePx);

        byte[] pageScreenshot = await pageLocator.ScreenshotAsync();
        (int screenshotWidth, int screenshotHeight) = PngDimensions(pageScreenshot);
        AddMismatchIfOutsideTolerance(mismatches, "page screenshot width", expectedPageWidth, screenshotWidth, 1.0f);
        AddMismatchIfOutsideTolerance(mismatches, "page screenshot height", expectedPageHeight, screenshotHeight, 1.0f);

        using BufferedImage browserPage = DecodePng(pageScreenshot);
        using BufferedImage pdfPage = new PDFRenderer(document).RenderImageWithDPI(0, 96f, ImageType.RGB);
        byte[] pdfPagePng = RenderingBackend.Current.ImageCodec.Encode(pdfPage, EncodedImageFormat.Png, 100);
        AddMismatchIfOutsideTolerance(mismatches, "PDF render width", pdfPage.Width, browserPage.Width, 0);
        AddMismatchIfOutsideTolerance(mismatches, "PDF render height", pdfPage.Height, browserPage.Height, 0);
        if (pdfPage.Width == browserPage.Width && pdfPage.Height == browserPage.Height)
        {
            AddForegroundMaskMismatches(mismatches, pdfPage, browserPage);
        }

        ILocator textRuns = page.Locator(".pdf-text-run");
        int renderedRunCount = await textRuns.CountAsync();
        if (renderedRunCount != layoutPage.Runs.Count)
        {
            mismatches.Add($"text run count expected {layoutPage.Runs.Count}, actual {renderedRunCount}");
        }

        for (int i = 0; i < Math.Min(renderedRunCount, layoutPage.Runs.Count); i++)
        {
            PdfTextRun run = layoutPage.Runs[i];
            LocatorBoundingBoxResult? runBox = await textRuns.Nth(i).BoundingBoxAsync();
            if (runBox == null)
            {
                mismatches.Add($"text run {i} did not render a bounding box");
                continue;
            }

            float relativeX = (float)(runBox.X - pageBox.X);
            float relativeY = (float)(runBox.Y - pageBox.Y);
            AddMismatchIfOutsideTolerance(mismatches, $"text run {i} left", run.Bounds.X * cssPixelsPerPoint, relativeX, geometryTolerancePx);
            AddMismatchIfOutsideTolerance(mismatches, $"text run {i} top", run.Bounds.Y * cssPixelsPerPoint, relativeY, geometryTolerancePx);
            AddMismatchIfOutsideTolerance(mismatches, $"text run {i} width", run.Bounds.Width * cssPixelsPerPoint, (float)runBox.Width, geometryTolerancePx);
            AddMismatchIfOutsideTolerance(mismatches, $"text run {i} height", run.Bounds.Height * cssPixelsPerPoint, (float)runBox.Height, geometryTolerancePx);
        }

        string renderedText = await pageLocator.InnerTextAsync();
        if (TextCoverage(layout.Text, renderedText) < 0.99f)
        {
            mismatches.Add($"rendered text coverage below 0.99: {TextCoverage(layout.Text, renderedText):0.###}");
        }

        return new BrowserRenderComparison(mismatches, pageScreenshot, pdfPagePng);
    }

    private static BufferedImage DecodePng(byte[] png)
    {
        return RenderingBackend.Current.ImageCodec.Decode(png)
            ?? throw new InvalidOperationException("Unable to decode browser page screenshot PNG.");
    }

    private static void AddForegroundMaskMismatches(
        List<string> mismatches,
        BufferedImage pdfPage,
        BufferedImage browserPage)
    {
        ForegroundShapeStats? stats = ForegroundShapeStats.Create(
            pdfPage,
            browserPage,
            luminanceThreshold: 245,
            dilationRadius: 3);
        if (stats == null)
        {
            mismatches.Add("foreground mask comparison did not find any foreground pixels");
            return;
        }

        if (stats.ForegroundDeltaRatio > 0.45f)
        {
            mismatches.Add($"foreground mask pixel-count delta expected <= 0.45, actual {stats.ForegroundDeltaRatio:0.###}");
        }

        if (stats.PdfMissRatio > 0.25f)
        {
            mismatches.Add($"foreground mask PDF miss ratio expected <= 0.25, actual {stats.PdfMissRatio:0.###}");
        }

        if (stats.BrowserMissRatio > 0.25f)
        {
            mismatches.Add($"foreground mask browser miss ratio expected <= 0.25, actual {stats.BrowserMissRatio:0.###}");
        }
    }

    private static void AddMismatchIfOutsideTolerance(
        List<string> mismatches,
        string name,
        float expected,
        float actual,
        float tolerance)
    {
        if (MathF.Abs(expected - actual) > tolerance)
        {
            mismatches.Add($"{name} expected {expected:0.###}, actual {actual:0.###}, tolerance {tolerance:0.###}");
        }
    }

    private static async Task WriteBrowserMismatchArtifactsAsync(
        PdfHtmlDocument html,
        IPage page,
        string artifactDirectory,
        BrowserRenderComparison comparison)
    {
        if (Directory.Exists(artifactDirectory))
        {
            Directory.Delete(artifactDirectory, recursive: true);
        }

        Directory.CreateDirectory(artifactDirectory);
        html.WriteToDirectory(artifactDirectory);
        if (comparison.PdfPagePng.Length > 0)
        {
            await File.WriteAllBytesAsync(Path.Combine(artifactDirectory, "pdf-page.png"), comparison.PdfPagePng);
        }

        if (comparison.BrowserPagePng.Length > 0)
        {
            await File.WriteAllBytesAsync(Path.Combine(artifactDirectory, "browser-page.png"), comparison.BrowserPagePng);
        }

        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            FullPage = true,
            Path = Path.Combine(artifactDirectory, "viewport.png")
        });
        File.WriteAllLines(Path.Combine(artifactDirectory, "mismatches.txt"), comparison.Mismatches);
        await File.WriteAllTextAsync(Path.Combine(artifactDirectory, "visual-report.html"), VisualReportHtml(comparison.Mismatches));
    }

    private static async Task WriteGeometryMismatchArtifactsAsync(
        PdfHtmlDocument html,
        IPage page,
        string artifactDirectory,
        IReadOnlyList<string> mismatches)
    {
        if (Directory.Exists(artifactDirectory))
        {
            Directory.Delete(artifactDirectory, recursive: true);
        }

        Directory.CreateDirectory(artifactDirectory);
        html.WriteToDirectory(artifactDirectory);
        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            FullPage = true,
            Path = Path.Combine(artifactDirectory, "viewport.png")
        });
        await File.WriteAllLinesAsync(Path.Combine(artifactDirectory, "mismatches.txt"), mismatches);
    }

    private static string VisualReportHtml(IReadOnlyList<string> mismatches)
    {
        StringBuilder report = new();
        report.AppendLine("<!doctype html>");
        report.AppendLine("<html lang=\"en\">");
        report.AppendLine("<head>");
        report.AppendLine("  <meta charset=\"utf-8\" />");
        report.AppendLine("  <title>HTML render mismatch</title>");
        report.AppendLine("  <style>");
        report.AppendLine("    body{font-family:Arial,sans-serif;margin:24px;color:#111827;background:#f9fafb}");
        report.AppendLine("    .images{display:flex;gap:24px;align-items:flex-start;flex-wrap:wrap}");
        report.AppendLine("    figure{margin:0;padding:12px;background:white;border:1px solid #d1d5db}");
        report.AppendLine("    img{max-width:48vw;height:auto;border:1px solid #e5e7eb}");
        report.AppendLine("    code{white-space:pre-wrap}");
        report.AppendLine("  </style>");
        report.AppendLine("</head>");
        report.AppendLine("<body>");
        report.AppendLine("  <h1>HTML render mismatch</h1>");
        report.AppendLine("  <div class=\"images\">");
        report.AppendLine("    <figure><figcaption>PDF render</figcaption><img src=\"pdf-page.png\" alt=\"PDF render\" /></figure>");
        report.AppendLine("    <figure><figcaption>Browser render</figcaption><img src=\"browser-page.png\" alt=\"Browser render\" /></figure>");
        report.AppendLine("  </div>");
        report.AppendLine("  <h2>Mismatches</h2>");
        report.AppendLine("  <code>");
        report.Append(WebUtility.HtmlEncode(string.Join(Environment.NewLine, mismatches)));
        report.AppendLine("</code>");
        report.AppendLine("</body>");
        report.AppendLine("</html>");
        return report.ToString();
    }

    private static string ArtifactDirectory(string testName)
    {
        return Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../artifacts/html-render-tests",
            testName));
    }

    private static (int Width, int Height) PngDimensions(byte[] png)
    {
        Assert.True(png.Length >= 24);
        Assert.True(BinaryPrimitives.ReadUInt32BigEndian(png.AsSpan(0, 4)) == 0x89504E47u);
        return (
            BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16, 4)),
            BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20, 4)));
    }

    private sealed class BrowserRenderComparison
    {
        public BrowserRenderComparison(List<string> mismatches, byte[] browserPagePng, byte[] pdfPagePng)
        {
            Mismatches = mismatches;
            BrowserPagePng = browserPagePng;
            PdfPagePng = pdfPagePng;
        }

        public List<string> Mismatches { get; }

        public byte[] BrowserPagePng { get; }

        public byte[] PdfPagePng { get; }
    }

    private sealed class ForegroundShapeStats
    {
        private ForegroundShapeStats(float foregroundDeltaRatio, float pdfMissRatio, float browserMissRatio)
        {
            ForegroundDeltaRatio = foregroundDeltaRatio;
            PdfMissRatio = pdfMissRatio;
            BrowserMissRatio = browserMissRatio;
        }

        public float ForegroundDeltaRatio { get; }

        public float PdfMissRatio { get; }

        public float BrowserMissRatio { get; }

        public static ForegroundShapeStats? Create(
            BufferedImage pdfPage,
            BufferedImage browserPage,
            int luminanceThreshold,
            int dilationRadius)
        {
            bool[] pdfMask = ForegroundMask(pdfPage, luminanceThreshold);
            bool[] browserMask = ForegroundMask(browserPage, luminanceThreshold);
            int pdfForeground = pdfMask.Count(static foreground => foreground);
            int browserForeground = browserMask.Count(static foreground => foreground);
            int maxForeground = Math.Max(pdfForeground, browserForeground);
            if (maxForeground == 0)
            {
                return null;
            }

            bool[] dilatedPdfMask = DilateMask(pdfMask, pdfPage.Width, pdfPage.Height, dilationRadius);
            bool[] dilatedBrowserMask = DilateMask(browserMask, browserPage.Width, browserPage.Height, dilationRadius);
            int pdfMisses = CountMisses(pdfMask, dilatedBrowserMask);
            int browserMisses = CountMisses(browserMask, dilatedPdfMask);
            return new ForegroundShapeStats(
                MathF.Abs(pdfForeground - browserForeground) / (float)maxForeground,
                pdfMisses / (float)maxForeground,
                browserMisses / (float)maxForeground);
        }

        private static bool[] ForegroundMask(BufferedImage image, int luminanceThreshold)
        {
            bool[] mask = new bool[image.Width * image.Height];
            int index = 0;
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    int argb = image.GetRgb(x, y);
                    int alpha = (argb >> 24) & 0xFF;
                    if (alpha == 0)
                    {
                        index++;
                        continue;
                    }

                    int red = CompositeOnWhite((argb >> 16) & 0xFF, alpha);
                    int green = CompositeOnWhite((argb >> 8) & 0xFF, alpha);
                    int blue = CompositeOnWhite(argb & 0xFF, alpha);
                    int luminance = ((red * 299) + (green * 587) + (blue * 114)) / 1000;
                    mask[index++] = luminance < luminanceThreshold;
                }
            }

            return mask;
        }

        private static int CompositeOnWhite(int channel, int alpha)
        {
            return alpha >= 255 ? channel : ((channel * alpha) + (255 * (255 - alpha))) / 255;
        }

        private static bool[] DilateMask(bool[] mask, int width, int height, int radius)
        {
            if (radius <= 0)
            {
                return (bool[])mask.Clone();
            }

            bool[] dilated = new bool[mask.Length];
            for (int y = 0; y < height; y++)
            {
                int rowOffset = y * width;
                for (int x = 0; x < width; x++)
                {
                    if (!mask[rowOffset + x])
                    {
                        continue;
                    }

                    int minX = Math.Max(0, x - radius);
                    int maxX = Math.Min(width - 1, x + radius);
                    int minY = Math.Max(0, y - radius);
                    int maxY = Math.Min(height - 1, y + radius);
                    for (int yy = minY; yy <= maxY; yy++)
                    {
                        int offset = yy * width;
                        for (int xx = minX; xx <= maxX; xx++)
                        {
                            dilated[offset + xx] = true;
                        }
                    }
                }
            }

            return dilated;
        }

        private static int CountMisses(bool[] source, bool[] target)
        {
            int misses = 0;
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] && !target[i])
                {
                    misses++;
                }
            }

            return misses;
        }
    }

    private static XDocument ParseHtml(string html)
    {
        string xml = Regex.Replace(html, "<!doctype html>\\s*", "", RegexOptions.IgnoreCase);
        return XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
    }

    private static IEnumerable<XElement> ElementsByClass(XDocument document, string className)
    {
        return document.Descendants()
            .Where(element => (element.Attribute("class")?.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [])
                .Contains(className, StringComparer.Ordinal));
    }

    private static Dictionary<string, string> ParseStyle(string style)
    {
        return style.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Split(':', 2))
            .Where(parts => parts.Length == 2)
            .ToDictionary(
                parts => parts[0].Trim(),
                parts => parts[1].Trim(),
                StringComparer.OrdinalIgnoreCase);
    }

    private static PdfLayoutRectangle RectangleFromStyle(Dictionary<string, string> style)
    {
        return new PdfLayoutRectangle(
            ParsePoints(style["left"]),
            ParsePoints(style["top"]),
            ParsePoints(style["width"]),
            ParsePoints(style["height"]));
    }

    private static float ParsePoints(string value)
    {
        Assert.EndsWith("pt", value);
        return float.Parse(value[..^2], CultureInfo.InvariantCulture);
    }

    private static void AssertClose(float expected, float actual)
    {
        Assert.InRange(actual, expected - 0.01f, expected + 0.01f);
    }

    private static void AssertWithin(float tolerance, float expected, float actual)
    {
        Assert.InRange(actual, expected - tolerance, expected + tolerance);
    }

    private static float TextCoverage(string expected, string actual)
    {
        Dictionary<string, int> expectedCounts = TokenCounts(expected);
        Dictionary<string, int> actualCounts = TokenCounts(actual);
        int total = expectedCounts.Values.Sum();
        int matched = expectedCounts.Sum(pair => Math.Min(pair.Value, actualCounts.GetValueOrDefault(pair.Key)));
        return total == 0 ? 1 : matched / (float)total;
    }

    private static Dictionary<string, int> TokenCounts(string value)
    {
        Dictionary<string, int> counts = new(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in Regex.Matches(value, "\\w+|[^\\w\\s]"))
        {
            counts[match.Value] = counts.GetValueOrDefault(match.Value) + 1;
        }

        return counts;
    }

    private static float ForegroundIntersectionOverUnion(
        IReadOnlyList<PdfLayoutRectangle> expected,
        IReadOnlyList<PdfLayoutRectangle> actual)
    {
        if (expected.Count == 0 && actual.Count == 0)
        {
            return 1;
        }

        float intersection = 0;
        for (int i = 0; i < Math.Min(expected.Count, actual.Count); i++)
        {
            intersection += IntersectionArea(expected[i], actual[i]);
        }

        float expectedArea = expected.Sum(Area);
        float actualArea = actual.Sum(Area);
        float union = expectedArea + actualArea - intersection;
        return union <= 0 ? 0 : intersection / union;
    }

    private static float IntersectionArea(PdfLayoutRectangle first, PdfLayoutRectangle second)
    {
        float left = MathF.Max(first.X, second.X);
        float top = MathF.Max(first.Y, second.Y);
        float right = MathF.Min(first.Right, second.Right);
        float bottom = MathF.Min(first.Bottom, second.Bottom);
        return MathF.Max(0, right - left) * MathF.Max(0, bottom - top);
    }

    private static float Area(PdfLayoutRectangle rectangle)
    {
        return MathF.Max(0, rectangle.Width) * MathF.Max(0, rectangle.Height);
    }

    private static IEnumerable<string> BrokenLocalReferences(string htmlPath)
    {
        string html = File.ReadAllText(htmlPath);
        foreach (Match match in Regex.Matches(html, "(?:href|src)=\"(?<reference>[^\"]+)\""))
        {
            string reference = match.Groups["reference"].Value;
            if (reference.Contains(":", StringComparison.Ordinal) || reference.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            string resolved = Path.Combine(Path.GetDirectoryName(htmlPath)!, reference.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(resolved))
            {
                yield return reference;
            }
        }
    }

    private static PDDocument CreateTextDocument(string contentStream)
    {
        PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);

        COSDictionary pageDictionary = (COSDictionary)page.GetCOSObject();
        pageDictionary.SetItem(COSName.RESOURCES, CreateDefaultResourcesDictionary());
        pageDictionary.SetItem(COSName.CONTENTS, CreateContentStream(contentStream));
        return document;
    }

    private static PDDocument CreateLinkedTextDocument()
    {
        PDDocument document = CreateTextDocument("""
            BT
            /F1 12 Tf
            72 700 Td
            (Linked text) Tj
            ET
            """);
        PDAnnotationLink link = new();
        link.SetRectangle(new PDRectangle(72, 680, 120, 24));
        PDActionURI action = new();
        action.SetURI("https://example.com/pdfbox");
        link.SetAction(action);
        document.GetPage(0).SetAnnotations([link]);
        return document;
    }

    private static PDDocument CreateImageDocument()
    {
        PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);
        byte[] rgb =
        [
            255, 0, 0,
            0, 255, 0,
            0, 0, 255,
            255, 255, 255
        ];
        PDImageXObject image = LosslessFactory.CreateFromRawData(document, rgb, 2, 2, 8, 3);
        using (PDPageContentStream content = new(document, page))
        {
            content.DrawImage(image, 72, 600, 120, 60);
        }

        return document;
    }

    private static PDDocument CreateInlineImageDocument()
    {
        PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);

        COSDictionary pageDictionary = (COSDictionary)page.GetCOSObject();
        pageDictionary.SetItem(COSName.CONTENTS, CreateInlineImageContentStream());
        return document;
    }

    private static COSStream CreateInlineImageContentStream()
    {
        COSStream stream = new();
        using Stream output = stream.CreateOutputStream();
        WriteLatin1(output, "q\n120 0 0 60 72 600 cm\nBI\n/W 2 /H 2 /BPC 8 /CS /RGB\nID\n");
        output.Write([
            255, 0, 0,
            0, 255, 0,
            0, 0, 255,
            255, 255, 255
        ]);
        WriteLatin1(output, "\nEI\nQ\n");
        return stream;
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

    private static void WriteLatin1(Stream stream, string value)
    {
        byte[] bytes = Encoding.Latin1.GetBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "pdfbox-net-html-" + Guid.NewGuid().ToString("N"));
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
