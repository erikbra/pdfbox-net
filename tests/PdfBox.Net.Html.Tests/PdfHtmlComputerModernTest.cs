using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Playwright;
using PdfBox.Net.Html;
using PdfBox.Net.Layout;
using PdfBox.Net.PDModel;

namespace PdfBox.Net.Html.Tests;

public class PdfHtmlComputerModernTest
{
    [Theory]
    [InlineData("CMR10")]
    [InlineData("ABCDEF+CMR8")]
    [InlineData("CMBX12")]
    [InlineData("CMTI10")]
    [InlineData("CMSS10")]
    public void HasMathFont_ComputerModernProseFamilies_ReturnFalse(string fontName)
    {
        Assert.False(PdfHtmlConverter.HasMathFont(fontName));
    }

    [Theory]
    [InlineData("CMMI10")]
    [InlineData("ABCDEF+CMMIB10")]
    [InlineData("CMSY8")]
    [InlineData("CMBSY10")]
    [InlineData("CMEX10")]
    [InlineData("MSAM10")]
    [InlineData("MSBM10")]
    [InlineData("AMSA")]
    [InlineData("AMSB")]
    public void HasMathFont_MathAndAmsSymbolFamilies_ReturnTrue(string fontName)
    {
        Assert.True(PdfHtmlConverter.HasMathFont(fontName));
    }

    [Fact]
    public void Convert_SemanticContinuousFlow_PreservesSvtPageTwoDisplayPrograms()
    {
        using PDDocument document = Loader.LoadPDF(FixturePath("arxiv-svt-pages-1-2.pdf"));
        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document, new PdfLayoutOptions
        {
            IncludeImages = false,
            IncludeLinks = false
        });

        PdfHtmlDocument html = PdfHtmlConverter.Convert(layout, new PdfHtmlOptions
        {
            TextMode = PdfHtmlTextMode.Semantic,
            SemanticPageMode = PdfHtmlSemanticPageMode.ContinuousFlow
        });
        XDocument dom = ParseHtml(html.Html);

        XElement[] formulaElements = ElementsByClass(dom, "pdf-semantic-formula").ToArray();
        Assert.DoesNotContain(formulaElements, static formula => formula.Name.LocalName == "p");
        Assert.DoesNotContain(formulaElements, formula =>
            FormulaLabel(formula).Trim().Equals("{∈}", StringComparison.Ordinal));

        XElement[] formulas = formulaElements
            .Where(static formula => formula.Name.LocalName == "div")
            .ToArray();
        XElement programOne = FormulaWithNumber(formulas, "(1.1)");
        XElement bound = FormulaWithNumber(formulas, "(1.2)");
        XElement programThree = FormulaWithNumber(formulas, "(1.3)");

        Assert.NotSame(programOne, bound);
        Assert.NotSame(bound, programThree);
        Assert.NotSame(programOne, programThree);
        Assert.Equal(3, formulas.Count(formula =>
            FormulaLabel(formula).Contains("(1.", StringComparison.Ordinal)));
        Assert.Equal("div", programOne.Name.LocalName);
        Assert.Equal("div", bound.Name.LocalName);
        Assert.Equal("div", programThree.Name.LocalName);
        Assert.Contains("minimize", FormulaLabel(programOne), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("subject to", FormulaLabel(programOne), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("6/5", FormulaLabel(bound), StringComparison.Ordinal);
        Assert.Contains("rank", FormulaLabel(programThree), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("subject to", FormulaLabel(programThree), StringComparison.OrdinalIgnoreCase);

        XElement[] prose = dom.Descendants("p")
            .Where(element => HasClass(element, "pdf-semantic-paragraph"))
            .Where(element => !element.Ancestors().Any(ancestor => HasClass(ancestor, "pdf-semantic-footnotes")))
            .ToArray();
        XElement beforePrograms = ParagraphContaining(prose, "one has available m sampled entries");
        XElement betweenOneAndTwo = ParagraphContaining(prose, "provided that the number of samples obeys");
        XElement betweenTwoAndThree = ParagraphContaining(prose, "is the nuclear norm");

        Assert.Contains("solving the optimization problem", beforePrograms.Value, StringComparison.Ordinal);
        Assert.DoesNotContain(beforePrograms.DescendantsAndSelf(), element =>
            HasClass(element, "pdf-semantic-formula"));
        Assert.NotSame(beforePrograms, betweenOneAndTwo);
        Assert.NotSame(betweenOneAndTwo, betweenTwoAndThree);
        AssertPrecedes(beforePrograms, programOne);
        AssertPrecedes(programOne, betweenOneAndTwo);
        AssertPrecedes(betweenOneAndTwo, bound);
        AssertPrecedes(bound, betweenTwoAndThree);
        AssertPrecedes(betweenTwoAndThree, programThree);
        Assert.DoesNotContain(prose, paragraph =>
            paragraph.Value.Trim() is "(1.1)" or "(1.2)" or "(1.3)");

        XElement footnotes = Assert.Single(ElementsByClass(dom, "pdf-semantic-footnotes"));
        Assert.Equal("section", footnotes.Name.LocalName);
        XElement footnote = Assert.Single(footnotes.Descendants("p"));
        Assert.Contains("Note that an", footnote.Value, StringComparison.Ordinal);
        Assert.Contains("degrees of freedom", footnote.Value, StringComparison.Ordinal);
        AssertPrecedes(programThree, footnotes);

        XElement pageTwoContinuation = Assert.Single(
            ElementsByClass(dom, "pdf-semantic-page-continuation"),
            continuation => continuation.Value.StartsWith("applied science", StringComparison.Ordinal));
        Assert.DoesNotContain(pageTwoContinuation.Descendants(), element =>
            HasClass(element, "pdf-semantic-math") &&
            FontClasses(element).Any(static className =>
                className.Equals("pdf-font-cmr10", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task Convert_SemanticContinuousFlow_PreservesSvtTitleTypography()
    {
        using PDDocument document = Loader.LoadPDF(FixturePath("arxiv-svt-pages-1-2.pdf"));
        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document, new PdfLayoutOptions
        {
            IncludeImages = false,
            IncludeLinks = false
        });
        PdfSemanticPage sourcePage = PdfSemanticExtractor.Extract(layout).Pages[0];
        PdfSemanticElement sourceTitle = Assert.Single(sourcePage.Elements, static element =>
            element.Kind == PdfSemanticElementKind.Heading &&
            element.HeadingLevel == 1 &&
            element.Lines.Count == 1 &&
            element.Lines[0].DominantFontSize >= 16f);
        PdfSemanticLine sourceLine = Assert.Single(sourceTitle.Lines);
        Assert.StartsWith("CMR", sourceLine.DominantFontName, StringComparison.Ordinal);

        PdfHtmlDocument html = PdfHtmlConverter.Convert(layout, new PdfHtmlOptions
        {
            TextMode = PdfHtmlTextMode.Semantic,
            SemanticPageMode = PdfHtmlSemanticPageMode.ContinuousFlow
        });
        XDocument dom = ParseHtml(html.Html);
        XElement title = Assert.Single(dom.Descendants("h1"), element =>
            HasClass(element, "pdf-semantic-title"));

        Assert.Equal(sourceTitle.Text, title.Value);
        Assert.True(HasClass(title, "pdf-semantic-title-regular"));
        Assert.DoesNotContain(title.DescendantsAndSelf(), element =>
            HasClass(element, "pdf-semantic-bold"));

        Match widthMatch = Regex.Match(
            title.Attribute("style")?.Value ?? "",
            "(?:^|;)--pdf-semantic-title-width:(?<width>[0-9.]+)pt(?:;|$)",
            RegexOptions.CultureInvariant);
        Assert.True(widthMatch.Success, title.ToString(SaveOptions.DisableFormatting));
        float measuredWidth = float.Parse(widthMatch.Groups["width"].Value, CultureInfo.InvariantCulture);
        Assert.InRange(measuredWidth, sourceLine.Bounds.Width - 0.001f, sourceLine.Bounds.Width + 0.001f);

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
                Height = 1400
            }
        });
        await page.GotoAsync(new Uri(Path.Combine(tempDirectory.Path, "index.html")).AbsoluteUri);

        TitleRenderMetrics metrics = await GetTitleRenderMetrics(page);

        const float cssPixelsPerPoint = 96f / 72f;
        Assert.Equal("400", metrics.FontWeight);
        Assert.Equal(1, metrics.LineCount);
        Assert.InRange(
            (float)metrics.Width,
            measuredWidth * cssPixelsPerPoint - 1f,
            measuredWidth * cssPixelsPerPoint + 1f);
        Assert.True(
            metrics.TextWidth <= metrics.Width + 1,
            $"Rendered title text width {metrics.TextWidth:0.###}px exceeded its {metrics.Width:0.###}px box.");

        await page.SetViewportSizeAsync(375, 1400);
        TitleRenderMetrics narrowMetrics = await GetTitleRenderMetrics(page);

        Assert.Equal("normal", narrowMetrics.WhiteSpace);
        Assert.True(narrowMetrics.LineCount > 1, "Expected the title to wrap naturally at the narrow viewport.");
        Assert.True(narrowMetrics.Left >= -1, $"Title began outside the viewport at {narrowMetrics.Left:0.###}px.");
        Assert.True(
            narrowMetrics.Right <= narrowMetrics.ViewportWidth + 1,
            $"Title ended at {narrowMetrics.Right:0.###}px outside the {narrowMetrics.ViewportWidth:0.###}px viewport.");
        Assert.True(
            narrowMetrics.TitleScrollWidth <= narrowMetrics.TitleClientWidth + 1,
            $"Title overflowed horizontally: {narrowMetrics.TitleScrollWidth:0.###}px > {narrowMetrics.TitleClientWidth:0.###}px.");
    }

    private static Task<TitleRenderMetrics> GetTitleRenderMetrics(IPage page)
    {
        return page.EvaluateAsync<TitleRenderMetrics>(
            """
            () => {
              const title = document.querySelector("h1.pdf-semantic-title");
              const titleBox = title.getBoundingClientRect();
              const range = document.createRange();
              range.selectNodeContents(title);
              const textRects = Array.from(range.getClientRects())
                .filter(rect => rect.width > 0 && rect.height > 0);
              const lineTops = [];
              for (const rect of textRects) {
                if (!lineTops.some(top => Math.abs(top - rect.top) < 1)) {
                  lineTops.push(rect.top);
                }
              }

              const style = getComputedStyle(title);
              return {
                fontWeight: style.fontWeight,
                whiteSpace: style.whiteSpace,
                lineCount: lineTops.length,
                left: titleBox.left,
                right: titleBox.right,
                width: titleBox.width,
                textWidth: range.getBoundingClientRect().width,
                titleClientWidth: title.clientWidth,
                titleScrollWidth: title.scrollWidth,
                viewportWidth: window.innerWidth
              };
            }
            """);
    }

    private static XElement FormulaWithNumber(IEnumerable<XElement> formulas, string equationNumber)
    {
        return Assert.Single(formulas, formula =>
            FormulaLabel(formula).Contains(equationNumber, StringComparison.Ordinal));
    }

    private static XElement ParagraphContaining(IEnumerable<XElement> paragraphs, string text)
    {
        return Assert.Single(paragraphs, paragraph =>
            paragraph.Value.Contains(text, StringComparison.Ordinal));
    }

    private static void AssertPrecedes(XElement first, XElement second)
    {
        Assert.True(XNode.CompareDocumentOrder(first, second) < 0);
    }

    private static string FormulaLabel(XElement formula)
    {
        return formula.Attribute("aria-label")?.Value ?? formula.Value;
    }

    private static string FixturePath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName);
    }

    private static XDocument ParseHtml(string html)
    {
        string xml = Regex.Replace(html, "<!doctype html>\\s*", "", RegexOptions.IgnoreCase);
        xml = string.Concat(xml.Where(XmlConvert.IsXmlChar));
        return XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
    }

    private static IEnumerable<XElement> ElementsByClass(XDocument document, string className)
    {
        return document.Descendants().Where(element => HasClass(element, className));
    }

    private static bool HasClass(XElement element, string className)
    {
        return FontClasses(element).Contains(className, StringComparer.Ordinal);
    }

    private static IEnumerable<string> FontClasses(XElement element)
    {
        return element.Attribute("class")?.Value
            .Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];
    }

    private sealed class TitleRenderMetrics
    {
        public string FontWeight { get; set; } = "";

        public string WhiteSpace { get; set; } = "";

        public int LineCount { get; set; }

        public double Left { get; set; }

        public double Right { get; set; }

        public double Width { get; set; }

        public double TextWidth { get; set; }

        public double TitleClientWidth { get; set; }

        public double TitleScrollWidth { get; set; }

        public double ViewportWidth { get; set; }
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "pdfbox-net-html-title-" + Guid.NewGuid().ToString("N"));
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
