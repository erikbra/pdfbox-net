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
    public void Convert_SemanticContinuousFlow_UsesFixedLayoutFallbackForSpatialPages()
    {
        using PDDocument columnsDocument = Loader.LoadPDF(FixturePath("4PP-Highlighting.pdf"));
        PdfLayoutDocument columnsLayout = PdfLayoutExtractor.Extract(columnsDocument);
        PdfHtmlDocument columnsHtml = PdfHtmlConverter.Convert(columnsLayout, new PdfHtmlOptions
        {
            TextMode = PdfHtmlTextMode.Semantic,
            SemanticPageMode = PdfHtmlSemanticPageMode.ContinuousFlow
        });
        XDocument columnsDom = ParseHtml(columnsHtml.Html);
        XElement columnsGrid = Assert.Single(ElementsByClass(columnsDom, "pdf-semantic-line-grid"));
        XElement[] gridRows = columnsGrid.Descendants()
            .Where(element => HasClass(element, "pdf-semantic-line-grid-row"))
            .ToArray();
        Assert.Equal(84, gridRows.Length);
        Assert.All(gridRows, row => Assert.Equal(2, row.Descendants()
            .Count(element => HasClass(element, "pdf-semantic-line-grid-cell"))));
        Assert.Equal(16, columnsGrid.Descendants()
            .Count(element => HasClass(element, "pdf-semantic-line-grid-highlight")));
        Assert.Empty(ElementsByClass(columnsDom, "pdf-semantic-layout-fallback-page"));
        Assert.Contains(".pdf-semantic-line-grid", columnsHtml.Css, StringComparison.Ordinal);

        using PDDocument staggeredColumnsDocument = CreateTextDocument("""
            BT
            /F1 10 Tf
            72 700 Td
            (left 01) Tj
            0 -14 Td (left 02) Tj
            0 -14 Td (left 03) Tj
            0 -14 Td (left 04) Tj
            0 -14 Td (left 05) Tj
            0 -14 Td (left 06) Tj
            0 -14 Td (left 07) Tj
            0 -14 Td (left 08) Tj
            0 -14 Td (left 09) Tj
            0 -14 Td (left 10) Tj
            0 -14 Td (left 11) Tj
            0 -14 Td (left 12) Tj
            ET
            BT
            /F1 10 Tf
            330 665 Td
            (right 01) Tj
            0 -14 Td (right 02) Tj
            0 -14 Td (right 03) Tj
            0 -14 Td (right 04) Tj
            0 -14 Td (right 05) Tj
            0 -14 Td (right 06) Tj
            0 -14 Td (right 07) Tj
            0 -14 Td (right 08) Tj
            0 -14 Td (right 09) Tj
            0 -14 Td (right 10) Tj
            0 -14 Td (right 11) Tj
            0 -14 Td (right 12) Tj
            ET
            """);
        PdfLayoutDocument staggeredColumnsLayout = PdfLayoutExtractor.Extract(staggeredColumnsDocument);
        PdfHtmlDocument staggeredColumnsHtml = PdfHtmlConverter.Convert(staggeredColumnsLayout, new PdfHtmlOptions
        {
            TextMode = PdfHtmlTextMode.Semantic,
            SemanticPageMode = PdfHtmlSemanticPageMode.ContinuousFlow
        });
        XDocument staggeredColumnsDom = ParseHtml(staggeredColumnsHtml.Html);
        Assert.Equal(2, ElementsByClass(staggeredColumnsDom, "pdf-semantic-column").Count());
        Assert.Equal(24, ElementsByClass(staggeredColumnsDom, "pdf-semantic-column-run").Count());
        Assert.Empty(ElementsByClass(staggeredColumnsDom, "pdf-semantic-layout-fallback-page"));
        Assert.Contains(".pdf-semantic-columns", staggeredColumnsHtml.Css, StringComparison.Ordinal);

        using PDDocument formDocument = Loader.LoadPDF(FixturePath("Acroform-PDFBOX-2333.pdf"));
        PdfLayoutDocument formLayout = PdfLayoutExtractor.Extract(formDocument, new PdfLayoutOptions
        {
            IncludeImageAssets = true
        });
        PdfHtmlDocument formHtml = PdfHtmlConverter.Convert(formLayout, new PdfHtmlOptions
        {
            TextMode = PdfHtmlTextMode.Semantic,
            SemanticPageMode = PdfHtmlSemanticPageMode.ContinuousFlow
        });
        XDocument formDom = ParseHtml(formHtml.Html);
        XElement formFallback = Assert.Single(ElementsByClass(formDom, "pdf-semantic-layout-fallback-page"));
        Assert.Equal(formLayout.Pages[0].Images.Count, formFallback.Descendants()
            .Count(element => HasClass(element, "pdf-image")));

        using PDDocument sparseDocument = CreateTextDocument("""
            BT
            /F1 12 Tf
            72 700 Td
            (Title) Tj
            ET
            BT
            /F1 12 Tf
            285 650 Td
            (Placed expression) Tj
            ET
            """);
        PdfLayoutDocument sparseLayout = PdfLayoutExtractor.Extract(sparseDocument);
        PdfHtmlDocument sparseHtml = PdfHtmlConverter.Convert(sparseLayout, new PdfHtmlOptions
        {
            TextMode = PdfHtmlTextMode.Semantic,
            SemanticPageMode = PdfHtmlSemanticPageMode.ContinuousFlow
        });
        XDocument sparseDom = ParseHtml(sparseHtml.Html);
        XElement sparseFallback = Assert.Single(ElementsByClass(sparseDom, "pdf-semantic-layout-fallback-page"));
        Assert.Equal(sparseLayout.Pages[0].Runs.Count, sparseFallback.Descendants()
            .Count(element => HasClass(element, "pdf-text-run")));
    }

    [Fact]
    public void Convert_SemanticContinuousFlow_UsesFixedLayoutForFullPageVectorBackdrops()
    {
        using PDDocument document = CreateTextDocument("""
            q
            0.95 0.82 0.25 rg
            0 0 612 792 re
            f
            Q
            BT
            /F1 10 Tf
            72 750 Td
            (Backdrop layout line 01) Tj
            0 -20 Td (Backdrop layout line 02) Tj
            0 -20 Td (Backdrop layout line 03) Tj
            0 -20 Td (Backdrop layout line 04) Tj
            0 -20 Td (Backdrop layout line 05) Tj
            0 -20 Td (Backdrop layout line 06) Tj
            0 -20 Td (Backdrop layout line 07) Tj
            0 -20 Td (Backdrop layout line 08) Tj
            0 -20 Td (Backdrop layout line 09) Tj
            0 -20 Td (Backdrop layout line 10) Tj
            0 -20 Td (Backdrop layout line 11) Tj
            0 -20 Td (Backdrop layout line 12) Tj
            0 -20 Td (Backdrop layout line 13) Tj
            0 -20 Td (Backdrop layout line 14) Tj
            0 -20 Td (Backdrop layout line 15) Tj
            0 -20 Td (Backdrop layout line 16) Tj
            0 -20 Td (Backdrop layout line 17) Tj
            0 -20 Td (Backdrop layout line 18) Tj
            0 -20 Td (Backdrop layout line 19) Tj
            0 -20 Td (Backdrop layout line 20) Tj
            ET
            """);
        PdfHtmlDocument html = PdfHtmlConverter.Convert(PdfLayoutExtractor.Extract(document), new PdfHtmlOptions
        {
            TextMode = PdfHtmlTextMode.Semantic,
            SemanticPageMode = PdfHtmlSemanticPageMode.ContinuousFlow
        });
        XDocument dom = ParseHtml(html.Html);

        Assert.Single(ElementsByClass(dom, "pdf-semantic-layout-fallback-page"));
    }

    [Fact]
    public void Convert_SemanticContinuousFlow_PreservesAnnotationAndAutomaticTextLinks()
    {
        using PDDocument annotationDocument = CreateLinkedTextDocument(textY: 760);
        PdfHtmlDocument annotationHtml = PdfHtmlConverter.Convert(PdfLayoutExtractor.Extract(annotationDocument), new PdfHtmlOptions
        {
            TextMode = PdfHtmlTextMode.Semantic,
            SemanticPageMode = PdfHtmlSemanticPageMode.ContinuousFlow
        });
        XDocument annotationDom = ParseHtml(annotationHtml.Html);

        XElement annotationLink = Assert.Single(ElementsByClass(annotationDom, "pdf-semantic-link"));
        Assert.Equal("https://example.com/pdfbox", annotationLink.Attribute("href")?.Value);
        Assert.Equal("uri", annotationLink.Attribute("data-link-kind")?.Value);
        Assert.Empty(ElementsByClass(annotationDom, "pdf-link-overlay"));

        using PDDocument automaticDocument = CreateTextDocument("""
            BT
            /F1 12 Tf
            72 760 Td
            (Contact hello@example.com or https://example.com/pdfbox.) Tj
            ET
            """);
        PdfHtmlDocument automaticHtml = PdfHtmlConverter.Convert(PdfLayoutExtractor.Extract(automaticDocument), new PdfHtmlOptions
        {
            TextMode = PdfHtmlTextMode.Semantic,
            SemanticPageMode = PdfHtmlSemanticPageMode.ContinuousFlow
        });
        XDocument automaticDom = ParseHtml(automaticHtml.Html);

        Assert.Contains(automaticDom.Descendants("a"), link =>
            link.Attribute("href")?.Value == "mailto:hello@example.com");
        Assert.Contains(automaticDom.Descendants("a"), link =>
            link.Attribute("href")?.Value == "https://example.com/pdfbox");
    }

    [Fact]
    public void Convert_SemanticContinuousFlow_EmitsBulletLinesAsListItems()
    {
        using PDDocument document = CreateTextDocument("""
            BT
            /F1 12 Tf
            72 700 Td
            (\225 First member) Tj
            0 -18 Td (\225 Second member) Tj
            0 -18 Td (\225 Third member) Tj
            0 -18 Td (\225 Fourth member) Tj
            0 -18 Td (\225 Fifth member) Tj
            0 -18 Td (\225 Sixth member) Tj
            0 -18 Td (\225 Seventh member) Tj
            0 -18 Td (\225 Eighth member) Tj
            0 -18 Td (\225 Ninth member) Tj
            ET
            """);
        PdfHtmlDocument html = PdfHtmlConverter.Convert(PdfLayoutExtractor.Extract(document), new PdfHtmlOptions
        {
            TextMode = PdfHtmlTextMode.Semantic,
            SemanticPageMode = PdfHtmlSemanticPageMode.ContinuousFlow
        });
        XDocument dom = ParseHtml(html.Html);

        XElement list = Assert.Single(dom.Descendants("ul"));
        Assert.Equal(new[]
            {
                "First member", "Second member", "Third member", "Fourth member", "Fifth member",
                "Sixth member", "Seventh member", "Eighth member", "Ninth member"
            },
            list.Elements("li").Select(item => item.Value.Trim()).ToArray());
    }

    [Fact]
    public async Task Convert_SemanticContinuousFlow_RendersDetectedGridWithSourceGeometry()
    {
        using PDDocument document = Loader.LoadPDF(FixturePath("4PP-Highlighting.pdf"));
        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document);
        PdfHtmlDocument html = PdfHtmlConverter.Convert(layout, new PdfHtmlOptions
        {
            TextMode = PdfHtmlTextMode.Semantic,
            SemanticPageMode = PdfHtmlSemanticPageMode.ContinuousFlow
        });

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

        GridRenderMetrics metrics = await page.EvaluateAsync<GridRenderMetrics>(
            """
            () => {
              const grid = document.querySelector(".pdf-semantic-line-grid");
              const rows = Array.from(grid.querySelectorAll(".pdf-semantic-line-grid-row"));
              const firstRow = rows[0].getBoundingClientRect();
              const secondRow = rows[1].getBoundingClientRect();
              const firstCells = Array.from(rows[0].querySelectorAll(".pdf-semantic-line-grid-cell"));
              const firstCell = firstCells[0].getBoundingClientRect();
              const secondCell = firstCells[1].getBoundingClientRect();
              const gridBox = grid.getBoundingClientRect();
              const firstHighlight = grid.querySelector(".pdf-semantic-line-grid-highlight").getBoundingClientRect();
              return {
                rowCount: rows.length,
                highlightCount: grid.querySelectorAll(".pdf-semantic-line-grid-highlight").length,
                firstCellLeft: firstCell.left - gridBox.left,
                secondCellLeft: secondCell.left - gridBox.left,
                firstCellTop: firstCell.top - gridBox.top,
                firstRowStep: secondRow.top - firstRow.top,
                firstHighlightWidth: firstHighlight.width
              };
            }
            """);

        const float cssPixelsPerPoint = 96f / 72f;
        Assert.Equal(84, metrics.RowCount);
        Assert.Equal(16, metrics.HighlightCount);
        AssertWithin(1f, 36f * cssPixelsPerPoint, (float)metrics.FirstCellLeft);
        AssertWithin(1f, 306f * cssPixelsPerPoint, (float)metrics.SecondCellLeft);
        AssertWithin(1f, 44.579f * cssPixelsPerPoint, (float)metrics.FirstCellTop);
        AssertWithin(1f, (52.679f - 44.579f) * cssPixelsPerPoint, (float)metrics.FirstRowStep);
        AssertWithin(1f, 16.141f * cssPixelsPerPoint, (float)metrics.FirstHighlightWidth);
    }


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
        Assert.Contains(".pdf-font-", html.Css, StringComparison.Ordinal);
        Assert.Contains(".pdf-color-", html.Css, StringComparison.Ordinal);
        Assert.Contains(".pdf-semantic-justified", html.Css, StringComparison.Ordinal);
        Assert.Contains(".pdf-semantic-measured-width", html.Css, StringComparison.Ordinal);
        Assert.Contains(".pdf-semantic-align-center", html.Css, StringComparison.Ordinal);

        XElement verticalHeader = Assert.Single(dom.Descendants("header"), header =>
            header.Value.Contains("arXiv:1706.03762v7", StringComparison.Ordinal));
        Assert.Contains("pdf-semantic-positioned", verticalHeader.Attribute("class")?.Value);
        Assert.Contains("pdf-semantic-vertical", verticalHeader.Attribute("class")?.Value);
        Dictionary<string, string> verticalStyle = ParseStyle(verticalHeader.Attribute("style")?.Value ?? "");
        Assert.InRange(ParsePoints(verticalStyle["top"]), 550f, 590f);

        XElement permissionHeader = Assert.Single(dom.Descendants("header"), header =>
            header.Value.Contains("Provided proper attribution", StringComparison.Ordinal));
        Assert.DoesNotContain("pdf-semantic-positioned", permissionHeader.Attribute("class")?.Value ?? "");
        Assert.Contains("reproduce the tables and figures in this paper solely for use in journalistic or", permissionHeader.Value, StringComparison.Ordinal);
        Assert.Contains("scholarly works.", permissionHeader.Value, StringComparison.Ordinal);

        XElement title = Assert.Single(dom.Descendants("h1"), element =>
            element.Value.Contains("Attention Is All You Need", StringComparison.Ordinal));
        Assert.Null(title.Attribute("data-semantic-kind"));
        Assert.Null(title.Attribute("style"));
        Assert.Contains("pdf-semantic-title", title.Attribute("class")?.Value);
        Assert.Contains("border-top: 4pt solid currentColor", html.Css, StringComparison.Ordinal);
        Assert.Contains("border-bottom: 1pt solid currentColor", html.Css, StringComparison.Ordinal);

        XElement[] authors = ElementsByClass(dom, "pdf-semantic-author-block").ToArray();
        Assert.Equal(8, authors.Length);
        Assert.All(authors, author => Assert.Equal("address", author.Name.LocalName));
        XElement[] authorRows = ElementsByClass(dom, "pdf-semantic-author-row").ToArray();
        Assert.Equal(new[] { 4, 3, 1 }, authorRows
            .Select(row => row.Descendants().Count(element => HasClass(element, "pdf-semantic-author-block")))
            .ToArray());
        Assert.Contains(authors, author =>
            author.Value.Contains("Ashish Vaswani", StringComparison.Ordinal) &&
            author.Value.Contains("avaswani@google.com", StringComparison.Ordinal));
        Assert.Contains(authors, author =>
            author.Value.Contains("Illia Polosukhin", StringComparison.Ordinal) &&
            author.Value.Contains("illia.polosukhin@gmail.com", StringComparison.Ordinal));
        XElement ashish = Assert.Single(authors, author => author.Value.Contains("Ashish Vaswani", StringComparison.Ordinal));
        XElement[] ashishLines = ashish.Descendants().Where(element => HasClass(element, "pdf-semantic-line")).ToArray();
        Assert.True(ashishLines.Length >= 3);
        Assert.NotEqual(ashishLines[0].Attribute("class")?.Value, ashishLines[^1].Attribute("class")?.Value);
        Assert.Contains(dom.Descendants("a"), link =>
            link.Attribute("href")?.Value == "#page-1-fn-asterisk" &&
            link.Value == "∗");

        XElement abstractHeading = Assert.Single(dom.Descendants("h2"), element => element.Value == "Abstract");
        Assert.Null(abstractHeading.Attribute("data-semantic-kind"));
        Assert.Contains("pdf-semantic-align-center", abstractHeading.Attribute("class")?.Value);
        XElement abstractParagraph = Assert.Single(ElementsByClass(dom, "pdf-semantic-paragraph"), paragraph =>
            paragraph.Value.StartsWith("The dominant sequence transduction models", StringComparison.Ordinal) &&
            paragraph.Value.Contains("large and limited training data.", StringComparison.Ordinal));
        Assert.Contains("pdf-semantic-justified", abstractParagraph.Attribute("class")?.Value);
        Assert.Contains("pdf-semantic-measured-width", abstractParagraph.Attribute("class")?.Value);
        Dictionary<string, string> abstractStyle = ParseStyle(abstractParagraph.Attribute("style")?.Value ?? "");
        Assert.InRange(ParsePercent(abstractStyle["--pdf-semantic-width"]), 80f, 84f);
        Assert.Equal("center", abstractStyle["--pdf-semantic-align-self"]);

        XElement introductionHeading = Assert.Single(dom.Descendants("h1"), heading => heading.Value == "1 Introduction");
        Assert.DoesNotContain("pdf-semantic-align-center", introductionHeading.Attribute("class")?.Value ?? "");
        XElement pageNumberFooter = Assert.Single(ElementsByClass(dom, "pdf-semantic-footer"), footer => footer.Value == "2");
        Assert.Contains("pdf-semantic-align-center", pageNumberFooter.Attribute("class")?.Value);
        XElement pageEndParagraph = Assert.Single(ElementsByClass(dom, "pdf-semantic-paragraph"), paragraph =>
            paragraph.Value.StartsWith("Most competitive neural sequence transduction models", StringComparison.Ordinal));
        Assert.DoesNotContain("pdf-semantic-measured-width", pageEndParagraph.Attribute("class")?.Value ?? "");
        XElement[] footnotes = ElementsByClass(dom, "pdf-semantic-footnote").ToArray();
        Assert.True(footnotes.Length >= 4);
        Assert.All(footnotes, footnote => Assert.Equal("p", footnote.Name.LocalName));
        Assert.Contains(footnotes, footnote => footnote.Attribute("id")?.Value == "page-1-fn-asterisk");
        Assert.Contains(footnotes, footnote => footnote.Attribute("id")?.Value == "page-4-fn-4");
        Assert.Contains(ElementsByClass(dom, "pdf-semantic-footer"), footer =>
            footer.Value.Contains("31st Conference", StringComparison.Ordinal));
        Assert.Contains(".pdf-semantic-flow > footer.pdf-semantic-footer", html.Css, StringComparison.Ordinal);
        Assert.Contains(".pdf-semantic-footnotes::before", html.Css, StringComparison.Ordinal);
    }

    [Fact]
    public void Convert_SemanticTextMode_EmitsArxivVariationTableSpans()
    {
        using PDDocument document = Loader.LoadPDF(Path.Combine(AppContext.BaseDirectory, "Fixtures", "arxiv-sample.pdf"));
        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document, new PdfLayoutOptions
        {
            IncludeImages = false,
            IncludeLinks = false,
            IncludePaths = true
        });

        PdfHtmlDocument html = PdfHtmlConverter.Convert(layout, new PdfHtmlOptions
        {
            TextMode = PdfHtmlTextMode.Semantic
        });
        XDocument dom = ParseHtml(html.Html);

        XElement variationTable = Assert.Single(dom.Descendants("table"), table =>
            table.Value.Contains("Pdrop", StringComparison.Ordinal) &&
            table.Value.Contains("positional embedding instead of sinusoids", StringComparison.Ordinal));
        XElement groupA = Assert.Single(variationTable.Descendants("th"), cell => cell.Value.Trim() == "(A)");
        Assert.Equal("rowgroup", groupA.Attribute("scope")?.Value);
        Assert.Equal("4", groupA.Attribute("rowspan")?.Value);
        Assert.Contains("pdf-semantic-table-row-group-header", groupA.Attribute("class")?.Value);

        XElement groupB = Assert.Single(variationTable.Descendants("th"), cell => cell.Value.Trim() == "(B)");
        Assert.Equal("2", groupB.Attribute("rowspan")?.Value);
        XElement groupC = Assert.Single(variationTable.Descendants("th"), cell => cell.Value.Trim() == "(C)");
        Assert.Equal("7", groupC.Attribute("rowspan")?.Value);
        XElement groupD = Assert.Single(variationTable.Descendants("th"), cell => cell.Value.Trim() == "(D)");
        Assert.Equal("4", groupD.Attribute("rowspan")?.Value);

        XElement descriptorCell = Assert.Single(variationTable.Descendants("td"), cell =>
            cell.Value.Contains("positional embedding instead of sinusoids", StringComparison.Ordinal));
        Assert.Equal("9", descriptorCell.Attribute("colspan")?.Value);
        Assert.DoesNotContain(variationTable.Descendants("tr"), row =>
            row.Elements().Count() == 1 &&
            row.Value.Trim() is "(A)" or "(B)" or "(D)");
        Assert.Contains(".pdf-semantic-table td[colspan]", html.Css, StringComparison.Ordinal);
        Assert.Contains(".pdf-semantic-table-row-group-header", html.Css, StringComparison.Ordinal);
    }

    [Fact]
    public void Convert_SemanticTextMode_ReservesGraphicRegionsAndPromotesFlowRulesToCss()
    {
        using PDDocument document = Loader.LoadPDF(Path.Combine(AppContext.BaseDirectory, "Fixtures", "arxiv-sample.pdf"));
        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document, new PdfLayoutOptions
        {
            IncludeImageAssets = true,
            IncludeLinks = false
        });

        PdfHtmlDocument html = PdfHtmlConverter.Convert(layout, new PdfHtmlOptions
        {
            TextMode = PdfHtmlTextMode.Semantic
        });
        XDocument dom = ParseHtml(html.Html);

        XElement firstPage = Assert.Single(dom.Descendants("section"), section =>
            section.Attribute("id")?.Value == "page-1");
        Assert.DoesNotContain(firstPage.Descendants(), element => HasClass(element, "pdf-vector-layer"));
        Assert.Contains("border-top: 4pt solid currentColor", html.Css, StringComparison.Ordinal);
        Assert.Contains("border-bottom: 1pt solid currentColor", html.Css, StringComparison.Ordinal);
        Assert.Contains(".pdf-semantic-footnotes::before", html.Css, StringComparison.Ordinal);
        XElement footnoteSection = Assert.Single(firstPage.Descendants(), element => HasClass(element, "pdf-semantic-footnotes"));
        Dictionary<string, string> footnoteRuleStyle = ParseStyle(footnoteSection.Attribute("style")?.Value ?? "");
        Assert.InRange(ParsePoints(footnoteRuleStyle["--pdf-footnote-rule-width"]), 140f, 146f);
        Assert.InRange(ParsePoints(footnoteRuleStyle["--pdf-footnote-rule-thickness"]), 0.3f, 0.5f);
        Assert.Equal("#000000", footnoteRuleStyle["--pdf-footnote-rule-color"]);
        Assert.DoesNotContain(dom.Descendants("h1").Where(heading =>
                heading.Value.Contains("Input-Input Layer5", StringComparison.Ordinal)),
            heading => HasClass(heading, "pdf-semantic-title"));

        XElement[] figureSpaces = ElementsByClass(dom, "pdf-semantic-figure-space").ToArray();
        Assert.True(figureSpaces.Length >= 2);
        Assert.All(figureSpaces, figure =>
        {
            Dictionary<string, string> style = ParseStyle(figure.Attribute("style")?.Value ?? "");
            Assert.True(ParsePoints(style["height"]) >= 30f);
        });

        XElement figure1Caption = Assert.Single(ElementsByClass(dom, "pdf-semantic-caption"), paragraph =>
            paragraph.Value == "Figure 1: The Transformer - model architecture.");
        Assert.Contains("pdf-semantic-align-center", figure1Caption.Attribute("class")?.Value);
        XElement figure4Caption = Assert.Single(ElementsByClass(dom, "pdf-semantic-caption"), paragraph =>
            paragraph.Value.StartsWith("Figure 4: Two attention heads", StringComparison.Ordinal));
        Assert.DoesNotContain("pdf-semantic-align-center", figure4Caption.Attribute("class")?.Value ?? "");

        XElement fourthPage = Assert.Single(dom.Descendants("section"), section =>
            section.Attribute("id")?.Value == "page-4");
        XElement flow = Assert.Single(fourthPage.Elements("article"), article => HasClass(article, "pdf-semantic-flow"));
        XElement[] flowChildren = flow.Elements().ToArray();
        XElement attentionLabels = Assert.Single(flowChildren, element =>
            HasClass(element, "pdf-semantic-line-row") &&
            element.Value.Contains("Scaled Dot-Product Attention", StringComparison.Ordinal) &&
            element.Value.Contains("Multi-Head Attention", StringComparison.Ordinal));
        XElement[] labelLines = attentionLabels.Elements("span").ToArray();
        Assert.Equal(new[] { "Scaled Dot-Product Attention", "Multi-Head Attention" }, labelLines.Select(static line => line.Value).ToArray());
        Assert.Contains("pdf-semantic-align-center", attentionLabels.Attribute("class")?.Value);
        Dictionary<string, string> attentionLabelStyle = ParseStyle(attentionLabels.Attribute("style")?.Value ?? "");
        Assert.Equal("2", attentionLabelStyle["--pdf-semantic-line-count"]);
        int labelIndex = Array.IndexOf(flowChildren, attentionLabels);
        int figureSpaceIndex = Array.FindIndex(flowChildren, element => HasClass(element, "pdf-semantic-figure-space"));
        Assert.True(labelIndex >= 0 && figureSpaceIndex > labelIndex);
    }

    [Fact]
    public void Convert_TransparencyGroups_EmitNestedSvgOpacity()
    {
        using PDDocument document = Loader.LoadPDF(Path.Combine(AppContext.BaseDirectory, "Fixtures", "arxiv-sample.pdf"));
        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document);

        PdfLayoutPage attentionVisualizationPage = layout.Pages[12];
        Assert.Contains(attentionVisualizationPage.VectorGroups, group => group.Opacity < 0.1f);

        PdfHtmlDocument html = PdfHtmlConverter.Convert(layout, new PdfHtmlOptions
        {
            TextMode = PdfHtmlTextMode.Semantic,
            SemanticPageMode = PdfHtmlSemanticPageMode.ContinuousFlow
        });

        Assert.Contains("class=\"pdf-vector-group\"", html.Html, StringComparison.Ordinal);
        Assert.Contains("Attention Visualizations", html.Html, StringComparison.Ordinal);
        Assert.DoesNotContain("Input-Input Layer5", html.Html, StringComparison.Ordinal);
        float[] groupOpacities = Regex.Matches(html.Html, "<g class=\"pdf-vector-group\"[^>]* opacity=\"(?<opacity>[^\"]+)\"")
            .Select(match => float.Parse(match.Groups["opacity"].Value, CultureInfo.InvariantCulture))
            .ToArray();
        Assert.Contains(groupOpacities, opacity => opacity < 0.1f);
        XDocument dom = ParseHtml(html.Html);
        XElement attentionVisualization = Assert.Single(ElementsByClass(dom, "pdf-semantic-figure"), figure =>
            figure.Attribute("data-source-page")?.Value == "13");
        Assert.Contains(attentionVisualization.Descendants(), element =>
            HasClass(element, "pdf-vector-group") &&
            element.Attribute("opacity")?.Value == "0" &&
            element.Descendants().Any(path =>
                path.Name.LocalName == "path" &&
                path.Attribute("fill")?.Value == "#D3D3D3"));
        Assert.Contains(attentionVisualization.Descendants(), element =>
            HasClass(element, "pdf-vector-group") &&
            element.Attribute("opacity")?.Value == "0.533" &&
            element.Descendants().Any(path =>
                path.Name.LocalName == "path" &&
                path.Attribute("fill")?.Value == "#E377C2"));
        Assert.Contains(attentionVisualization.Descendants(), element =>
            HasClass(element, "pdf-vector-group") &&
            element.Attribute("clip-path")?.Value.StartsWith("url(#pdf-vector-figure-13-", StringComparison.Ordinal) == true);
        Assert.Contains(attentionVisualization.Descendants(), element =>
            element.Name.LocalName == "clipPath" &&
            element.Descendants().Any(rectangle =>
                rectangle.Name.LocalName == "rect" &&
                rectangle.Attribute("x")?.Value == "108" &&
                rectangle.Attribute("y")?.Value.StartsWith("100.787", StringComparison.Ordinal) == true));
    }

    [Fact]
    public void Convert_SemanticContinuousFlow_EmitsSoftPageMarkersInsteadOfFixedPages()
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
            TextMode = PdfHtmlTextMode.Semantic,
            SemanticPageMode = PdfHtmlSemanticPageMode.ContinuousFlow
        });
        XDocument dom = ParseHtml(html.Html);

        Assert.Empty(ElementsByClass(dom, "pdf-page"));
        Assert.Empty(ElementsByClass(dom, "pdf-semantic-page"));
        Assert.Contains("pdf-document-continuous", dom.Descendants("body").Single().Attribute("class")?.Value);
        Assert.Contains(".pdf-semantic-page-break", html.Css, StringComparison.Ordinal);
        Assert.Contains(".pdf-semantic-continuous-flow .pdf-semantic-page-break::after", html.Css, StringComparison.Ordinal);
        Assert.Contains("--pdf-page-width: min(612pt, calc(100vw - 48pt))", html.Css, StringComparison.Ordinal);
        Assert.Contains("--pdf-page-corner-shadow: 22pt", html.Css, StringComparison.Ordinal);
        Assert.Contains("background: var(--pdf-page-surround)", html.Css, StringComparison.Ordinal);
        Assert.Contains("radial-gradient(ellipse at right top", html.Css, StringComparison.Ordinal);
        Assert.Contains("calc(100% - var(--pdf-page-shadow-mask) - var(--pdf-page-shadow-mask) - var(--pdf-page-corner-shadow) - var(--pdf-page-corner-shadow))", html.Css, StringComparison.Ordinal);
        Assert.Contains("text-align-last: center", html.Css, StringComparison.Ordinal);
        Assert.Contains("break-before: page", html.Css, StringComparison.Ordinal);

        XElement documentFlow = Assert.Single(ElementsByClass(dom, "pdf-semantic-document-flow"));
        XElement verticalHeader = Assert.Single(dom.Descendants("header"), header =>
            header.Value.Contains("arXiv:1706.03762v7", StringComparison.Ordinal));
        Assert.Contains("pdf-semantic-positioned", verticalHeader.Attribute("class")?.Value);
        Assert.Contains("pdf-semantic-vertical", verticalHeader.Attribute("class")?.Value);
        Dictionary<string, string> verticalStyle = ParseStyle(verticalHeader.Attribute("style")?.Value ?? "");
        Assert.InRange(ParsePoints(verticalStyle["left"]), 18f, 24f);
        Assert.InRange(ParsePoints(verticalStyle["top"]), 550f, 590f);

        XElement article = Assert.Single(documentFlow.Elements("article"), element =>
            HasClass(element, "pdf-semantic-flow") &&
            HasClass(element, "pdf-semantic-continuous-flow"));
        XElement[] pageBreaks = ElementsByClass(dom, "pdf-semantic-page-break").ToArray();
        Assert.Equal(layout.Pages.Count, pageBreaks.Length);
        Assert.Equal("page-1", pageBreaks[0].Attribute("id")?.Value);
        Assert.Contains("pdf-semantic-page-start", pageBreaks[0].Attribute("class")?.Value);
        Assert.Equal("1", pageBreaks[0].Attribute("data-page-number")?.Value);
        Assert.Equal("page-2", pageBreaks[1].Attribute("id")?.Value);
        Assert.Equal("2", pageBreaks[1].Attribute("data-page-number")?.Value);

        XElement[] articleChildren = article.Elements().ToArray();
        int pageTwoBreakIndex = Array.IndexOf(articleChildren, pageBreaks[1]);
        int introductionIndex = Array.FindIndex(articleChildren, element =>
            element.Name.LocalName == "h1" &&
            element.Value == "1 Introduction");
        Assert.True(pageTwoBreakIndex >= 0 && introductionIndex > pageTwoBreakIndex);

        XElement abstractHeading = Assert.Single(article.Descendants("h2"), element => element.Value == "Abstract");
        Assert.Contains("pdf-semantic-align-center", abstractHeading.Attribute("class")?.Value);
        Assert.Contains(ElementsByClass(dom, "pdf-semantic-footer"), footer =>
            footer.Value.Contains("31st Conference", StringComparison.Ordinal));
        Assert.Contains(ElementsByClass(dom, "pdf-semantic-footer"), footer => footer.Value == "2");
    }

    [Fact]
    public void Convert_SemanticContinuousFlow_PreservesFiguresAndCrossPageParagraphContinuations()
    {
        using PDDocument document = Loader.LoadPDF(Path.Combine(AppContext.BaseDirectory, "Fixtures", "arxiv-sample.pdf"));
        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document, new PdfLayoutOptions
        {
            IncludeImageAssets = true,
            IncludeLinks = false
        });

        PdfHtmlDocument html = PdfHtmlConverter.Convert(layout, new PdfHtmlOptions
        {
            TextMode = PdfHtmlTextMode.Semantic,
            SemanticPageMode = PdfHtmlSemanticPageMode.ContinuousFlow
        });
        XDocument dom = ParseHtml(html.Html);

        XElement[] figures = ElementsByClass(dom, "pdf-semantic-figure").ToArray();
        Assert.True(figures.Length >= 2);
        Assert.Contains(figures, figure => figure.Attribute("data-source-page")?.Value == "3");
        Assert.Contains(figures, figure => figure.Attribute("data-source-page")?.Value == "4");
        Assert.Contains(figures, figure => figure.Attribute("data-source-page")?.Value == "13");
        Assert.True(ElementsByClass(dom, "pdf-semantic-figure-svg").Count() >= 2);
        XElement[] svgImages = dom.Descendants().Where(static element => element.Name.LocalName == "image").ToArray();
        Assert.Contains(svgImages, image => image.Attribute("href")?.Value == "assets/images/page-4-image-0.png");
        Assert.Contains(svgImages, image => image.Attribute("href")?.Value == "assets/images/page-4-image-1.png");
        XElement attentionVisualization = Assert.Single(figures, figure => figure.Attribute("data-source-page")?.Value == "13");
        XElement[] attentionVisualizationLabels = attentionVisualization.Descendants()
            .Where(static element => element.Name.LocalName == "text" && HasClass(element, "pdf-semantic-figure-text"))
            .ToArray();
        Assert.Contains(attentionVisualizationLabels, label => label.Value == "making");
        Assert.Contains(attentionVisualizationLabels, label => label.Value == "registration");
        Assert.Contains(attentionVisualizationLabels, label =>
            label.Attribute("transform")?.Value.Contains("rotate", StringComparison.Ordinal) == true);
        Assert.DoesNotContain(ElementsByClass(dom, "pdf-semantic-paragraph"), paragraph =>
            paragraph.Value.Contains("In A tt p en u", StringComparison.Ordinal));
        XElement figureThreeCaption = Assert.Single(ElementsByClass(dom, "pdf-semantic-caption"), caption =>
            caption.Value.StartsWith("Figure 3:", StringComparison.Ordinal));
        Assert.Equal("figcaption", figureThreeCaption.Name.LocalName);
        Assert.Contains("Best viewed in color.", figureThreeCaption.Value, StringComparison.Ordinal);
        Assert.DoesNotContain("registration registration", figureThreeCaption.Value, StringComparison.Ordinal);
        XElement figureFiveCaption = Assert.Single(ElementsByClass(dom, "pdf-semantic-caption"), caption =>
            caption.Value.StartsWith("Figure 5:", StringComparison.Ordinal));
        Assert.Equal("figcaption", figureFiveCaption.Name.LocalName);
        Assert.Contains("figcaption.pdf-semantic-caption", html.Css, StringComparison.Ordinal);
        Assert.DoesNotContain("Input-Input Layer5", dom.Root?.Value ?? "", StringComparison.Ordinal);
        Assert.Empty(ElementsByClass(dom, "pdf-semantic-figure-space"));
        Assert.Contains(".pdf-semantic-formula", html.Css, StringComparison.Ordinal);
        Assert.Contains(".pdf-semantic-formula-run", html.Css, StringComparison.Ordinal);
        Assert.Contains(".pdf-semantic-formula-vector-layer", html.Css, StringComparison.Ordinal);
        Assert.Contains(".pdf-semantic-inline-run", html.Css, StringComparison.Ordinal);
        Assert.Contains(".pdf-semantic-inline-fraction", html.Css, StringComparison.Ordinal);
        Assert.Contains(".pdf-semantic-math", html.Css, StringComparison.Ordinal);
        Assert.Contains(".pdf-semantic-italic", html.Css, StringComparison.Ordinal);
        Assert.Contains(".pdf-semantic-inline-footnotes", html.Css, StringComparison.Ordinal);
        Assert.Contains(".pdf-semantic-formula-radical", html.Css, StringComparison.Ordinal);
        Assert.Contains(".pdf-semantic-formula-attached-suffix", html.Css, StringComparison.Ordinal);
        Assert.Contains(".pdf-semantic-table", html.Css, StringComparison.Ordinal);
        Assert.Contains(".pdf-semantic-inline-summation", html.Css, StringComparison.Ordinal);

        XElement continuedParagraph = Assert.Single(ElementsByClass(dom, "pdf-semantic-page-spanning"), paragraph =>
            paragraph.Value.StartsWith("An attention function can be described", StringComparison.Ordinal));
        Assert.Equal("p", continuedParagraph.Name.LocalName);
        Assert.Contains("The output is computed as a weighted sum", continuedParagraph.Value, StringComparison.Ordinal);
        Assert.Contains("of the values, where the weight assigned to each value", continuedParagraph.Value, StringComparison.Ordinal);
        Assert.Contains("corresponding key.", continuedParagraph.Value, StringComparison.Ordinal);
        XElement pageFourBreak = Assert.Single(continuedParagraph.Elements(), element =>
            HasClass(element, "pdf-semantic-inline-page-break") &&
            element.Attribute("data-page-number")?.Value == "4");
        XElement pageThreeFooter = Assert.Single(continuedParagraph.Elements(), element =>
            HasClass(element, "pdf-semantic-footer") &&
            element.Value == "3");
        Assert.Contains("pdf-semantic-inline-flow-element", pageThreeFooter.Attribute("class")?.Value);
        Assert.Contains("pdf-semantic-align-center", pageThreeFooter.Attribute("class")?.Value);
        XElement[] continuedParagraphChildren = continuedParagraph.Elements().ToArray();
        Assert.True(
            Array.IndexOf(continuedParagraphChildren, pageThreeFooter) <
            Array.IndexOf(continuedParagraphChildren, pageFourBreak));
        Assert.DoesNotContain(ElementsByClass(dom, "pdf-semantic-footer"), footer =>
            footer.Value == "3" &&
            footer.Parent != continuedParagraph);
        Assert.DoesNotContain(ElementsByClass(dom, "pdf-semantic-paragraph"), paragraph =>
            !HasClass(paragraph, "pdf-semantic-page-spanning") &&
            paragraph.Value.StartsWith("of the values, where the weight assigned", StringComparison.Ordinal));

        XElement pageFourFootnote = Assert.Single(ElementsByClass(dom, "pdf-semantic-footnote"), footnote =>
            footnote.Attribute("id")?.Value == "page-4-fn-4");
        Assert.Contains("To illustrate why the dot products get large", pageFourFootnote.Value, StringComparison.Ordinal);
        Assert.Contains("∑", pageFourFootnote.Value, StringComparison.Ordinal);
        Assert.Contains(pageFourFootnote.Descendants("sub"), subscript =>
            subscript.Value == "i" &&
            HasClass(subscript, "pdf-semantic-math"));
        Assert.Contains(pageFourFootnote.Descendants("sub"), subscript =>
            subscript.Value == "k" &&
            HasClass(subscript, "pdf-semantic-math"));
        XElement summation = Assert.Single(pageFourFootnote.Descendants(), element =>
            HasClass(element, "pdf-semantic-inline-summation"));
        Assert.Contains("∑", summation.Value, StringComparison.Ordinal);
        Assert.Contains("i=1", summation.Value, StringComparison.Ordinal);
        Assert.Contains(summation.Descendants("sub"), subscript => subscript.Value == "k");
        Assert.True(
            html.Html.IndexOf("id=\"page-4-fn-4\"", StringComparison.Ordinal) <
            html.Html.IndexOf("id=\"page-5\"", StringComparison.Ordinal));
        Assert.Contains(dom.Descendants("a"), link =>
            link.Attribute("href")?.Value == "#page-4-fn-4" &&
            link.Value == "4");
        XElement pageFourFooter = Assert.Single(ElementsByClass(dom, "pdf-semantic-footer"), footer =>
            footer.Value == "4");
        Assert.Empty(pageFourFooter.Descendants("a"));

        XElement multiHeadIntro = Assert.Single(ElementsByClass(dom, "pdf-semantic-paragraph"), paragraph =>
            paragraph.Value.StartsWith("Instead of performing a single attention function", StringComparison.Ordinal));
        Assert.DoesNotContain("pdf-semantic-measured-width", multiHeadIntro.Attribute("class")?.Value ?? "");

        XElement formula = Assert.Single(ElementsByClass(dom, "pdf-semantic-formula"), element =>
            element.Attribute("aria-label")?.Value.Contains("MultiHead", StringComparison.Ordinal) == true &&
            element.Attribute("aria-label")?.Value.Contains("Where the projections", StringComparison.Ordinal) == true);
        Assert.Equal("div", formula.Name.LocalName);
        Assert.Equal("math", formula.Attribute("role")?.Value);
        Dictionary<string, string> formulaStyle = ParseStyle(formula.Attribute("style")?.Value ?? "");
        Assert.True(ParsePoints(formulaStyle["--pdf-semantic-formula-width"]) > 300f);
        Assert.True(ParsePoints(formulaStyle["--pdf-semantic-formula-height"]) > 60f);
        XElement[] formulaRuns = formula.Elements().Where(static element =>
            HasClass(element, "pdf-semantic-formula-run")).ToArray();
        Assert.True(formulaRuns.Length > 100);
        Assert.Contains("MultiHead", string.Concat(formulaRuns.Select(static run => run.Value)), StringComparison.Ordinal);
        Assert.Contains("Where", string.Concat(formulaRuns.Select(static run => run.Value)), StringComparison.Ordinal);
        Assert.Contains(formulaRuns, run =>
            run.Value == "1" && HasClass(run, "pdf-semantic-formula-attached-suffix"));
        Assert.Contains(formulaRuns, run =>
            run.Value == "i" && HasClass(run, "pdf-semantic-formula-attached-suffix"));
        Assert.DoesNotContain("In this work", formula.Value, StringComparison.Ordinal);

        XElement attentionFormula = Assert.Single(ElementsByClass(dom, "pdf-semantic-formula"), element =>
            element.Attribute("aria-label")?.Value.Contains("Attention(Q, K, V)", StringComparison.Ordinal) == true);
        Assert.Contains(attentionFormula.Descendants(), element =>
            HasClass(element, "pdf-semantic-formula-vector-layer"));
        Assert.Contains(attentionFormula.Descendants(), element =>
            HasClass(element, "pdf-semantic-formula-radical") &&
            element.Value == "√");
        Assert.Contains(attentionFormula.Descendants(), element =>
            element.Name.LocalName == "path");
        Assert.Contains("√", attentionFormula.Value, StringComparison.Ordinal);
        Assert.Contains("dk", attentionFormula.Value, StringComparison.Ordinal);

        XElement attentionCostParagraph = Assert.Single(ElementsByClass(dom, "pdf-semantic-paragraph"), paragraph =>
            paragraph.Value.StartsWith("In this work we employ h = 8", StringComparison.Ordinal));
        Assert.DoesNotContain("pdf-semantic-formula", attentionCostParagraph.Attribute("class")?.Value ?? "");
        Assert.DoesNotContain("pdf-semantic-measured-width", attentionCostParagraph.Attribute("class")?.Value ?? "");
        Assert.Contains("single-head attention with full dimensionality.", attentionCostParagraph.Value, StringComparison.Ordinal);

        XElement sequenceParagraph = Assert.Single(ElementsByClass(dom, "pdf-semantic-paragraph"), paragraph =>
            paragraph.Value.Contains("symbol representations", StringComparison.Ordinal) &&
            paragraph.Value.Contains("continuous representations", StringComparison.Ordinal));
        Assert.Contains(sequenceParagraph.Descendants("sub"), subscript =>
            subscript.Value == "1" &&
            HasClass(subscript, "pdf-semantic-math"));
        Assert.Contains(sequenceParagraph.Descendants("sub"), subscript =>
            subscript.Value == "n" &&
            HasClass(subscript, "pdf-semantic-math"));

        XElement selfAttentionComparisonParagraph = Assert.Single(ElementsByClass(dom, "pdf-semantic-paragraph"), paragraph =>
            paragraph.Value.Contains("In this section we compare various aspects of self-attention", StringComparison.Ordinal));
        Assert.Contains("such as a hidden layer in a typical sequence transduction encoder", selfAttentionComparisonParagraph.Value, StringComparison.Ordinal);

        XElement encoderParagraph = Assert.Single(ElementsByClass(dom, "pdf-semantic-paragraph"), paragraph =>
            paragraph.Value.Contains("The encoder is composed of a stack of N = 6 identical layers.", StringComparison.Ordinal));
        Assert.Contains(encoderParagraph.Descendants(), element =>
            element.Value == "N" &&
            HasClass(element, "pdf-semantic-italic") &&
            HasClass(element, "pdf-semantic-math"));
        Assert.Contains(encoderParagraph.Descendants("sub"), subscript =>
            subscript.Value == "model");

        XElement positionalEncodingParagraph = Assert.Single(ElementsByClass(dom, "pdf-semantic-paragraph"), paragraph =>
            paragraph.Value.Contains("relative positions", StringComparison.Ordinal) &&
            paragraph.Value.Contains("linear function", StringComparison.Ordinal));
        Assert.Contains(positionalEncodingParagraph.Descendants("sub"), subscript =>
            subscript.Value == "pos+k");
        Assert.Contains(positionalEncodingParagraph.Descendants("sub"), subscript =>
            subscript.Value == "pos");
        Assert.DoesNotContain(ElementsByClass(dom, "pdf-semantic-paragraph"), paragraph =>
            paragraph.Value.StartsWith("pos+k can be represented", StringComparison.Ordinal));
        Assert.DoesNotContain(ElementsByClass(dom, "pdf-semantic-paragraph"), paragraph =>
            paragraph.Value == "PE pos");

        XElement scaledDotProductParagraph = Assert.Single(ElementsByClass(dom, "pdf-semantic-paragraph"), paragraph =>
            paragraph.Value.StartsWith("We call our particular attention", StringComparison.Ordinal));
        Assert.Contains("and values of dimension", scaledDotProductParagraph.Value, StringComparison.Ordinal);
        Assert.Contains("divide each by √dk", scaledDotProductParagraph.Value, StringComparison.Ordinal);
        Assert.DoesNotContain("a nd values", scaledDotProductParagraph.Value, StringComparison.Ordinal);
        Assert.DoesNotContain("√ nd", scaledDotProductParagraph.Value, StringComparison.Ordinal);
        Assert.Contains(scaledDotProductParagraph.Descendants("sub"), subscript =>
            subscript.Value == "k" &&
            HasClass(subscript, "pdf-semantic-math"));

        XElement scalingParagraph = Assert.Single(ElementsByClass(dom, "pdf-semantic-paragraph"), paragraph =>
            paragraph.Value.Contains("we scale the dot products by", StringComparison.Ordinal));
        Assert.Contains("for large values of dk", scalingParagraph.Value, StringComparison.Ordinal);
        XElement[] inverseSquareRootFractions = ElementsByClass(dom, "pdf-semantic-inline-fraction").ToArray();
        Assert.True(inverseSquareRootFractions.Length >= 2);
        Assert.All(inverseSquareRootFractions.Take(2), fraction =>
        {
            Assert.Contains("pdf-semantic-math", fraction.Attribute("class")?.Value);
            Assert.Contains(fraction.Descendants(), element =>
                HasClass(element, "pdf-semantic-inline-fraction-numerator") &&
                element.Value == "1");
            Assert.Contains(fraction.Descendants(), element =>
                HasClass(element, "pdf-semantic-inline-fraction-denominator") &&
                element.Value.Contains("√d", StringComparison.Ordinal));
            Assert.Contains(fraction.Descendants("sub"), subscript =>
                subscript.Value == "k");
        });
        Assert.DoesNotContain("√1", html.Html, StringComparison.Ordinal);

        XElement learningRateFormula = Assert.Single(ElementsByClass(dom, "pdf-semantic-formula"), element =>
            element.Attribute("aria-label")?.Value.Contains("lrate", StringComparison.Ordinal) == true &&
            element.Attribute("aria-label")?.Value.Contains("warmup_steps", StringComparison.Ordinal) == true);
        Assert.Equal("math", learningRateFormula.Attribute("role")?.Value);
        Assert.Contains("(3)", learningRateFormula.Value, StringComparison.Ordinal);
        Assert.Contains("warmup", learningRateFormula.Value, StringComparison.Ordinal);
        Assert.Single(ElementsByClass(dom, "pdf-semantic-formula"), element =>
            element.Attribute("aria-label")?.Value.Contains("lrate", StringComparison.Ordinal) == true);

        XElement regularizationParagraph = Assert.Single(ElementsByClass(dom, "pdf-semantic-paragraph"), paragraph =>
            paragraph.Value.Contains("For the base model, we use a rate of", StringComparison.Ordinal));
        Assert.Contains(regularizationParagraph.Descendants(), element =>
            element.Value == "P" &&
            HasClass(element, "pdf-semantic-math"));
        Assert.Contains(regularizationParagraph.Descendants("sub"), subscript =>
            subscript.Value == "drop");

        XElement[] semanticTables = ElementsByClass(dom, "pdf-semantic-table")
            .Where(static element => element.Name.LocalName == "table")
            .ToArray();
        Assert.True(semanticTables.Length >= 2);
        XElement complexityTable = Assert.Single(semanticTables, table =>
            table.Value.Contains("Complexity per Layer", StringComparison.Ordinal) &&
            table.Value.Contains("Self-Attention", StringComparison.Ordinal));
        Assert.Contains(complexityTable.Elements("thead").Descendants("th"), header =>
            header.Value.Contains("Sequential", StringComparison.Ordinal) &&
            header.Value.Contains("Operations", StringComparison.Ordinal));
        XElement selfAttentionComplexity = Assert.Single(complexityTable.Elements("tbody").Descendants("td"), cell =>
            cell.Value.Contains("O(n2", StringComparison.Ordinal));
        Assert.Contains(selfAttentionComplexity.Descendants("sup"), superscript =>
            superscript.Value == "2" &&
            HasClass(superscript, "pdf-semantic-math"));
        Assert.Contains(complexityTable.Descendants("sub"), subscript =>
            subscript.Value == "k" &&
            HasClass(subscript, "pdf-semantic-math"));
        Assert.Contains(complexityTable.Descendants(), cell =>
            HasClass(cell, "pdf-semantic-table-cell-border-bottom") ||
            HasClass(cell, "pdf-semantic-table-cell-border-top"));
        XElement selfAttentionLabel = Assert.Single(complexityTable.Elements("tbody").Descendants("td"), cell =>
            cell.Value == "Self-Attention");
        Assert.Contains("pdf-semantic-table-cell-align-left", selfAttentionLabel.Attribute("class")?.Value);
        Assert.Contains("pdf-semantic-table-cell-align-center", selfAttentionComplexity.Attribute("class")?.Value);
        Assert.Contains(".pdf-semantic-table .pdf-semantic-table-cell-border-top", html.Css, StringComparison.Ordinal);
        Assert.Contains("border-top: 0.45pt solid currentColor", html.Css, StringComparison.Ordinal);
        Assert.Contains(".pdf-semantic-table .pdf-semantic-table-cell-border-bottom", html.Css, StringComparison.Ordinal);
        Assert.DoesNotContain(ElementsByClass(dom, "pdf-semantic-paragraph"), paragraph =>
            paragraph.Value.StartsWith("Layer Type", StringComparison.Ordinal));

        XElement bleuTable = Assert.Single(semanticTables, table =>
            table.Value.Contains("Transformer (big)", StringComparison.Ordinal) &&
            table.Value.Contains("28.4", StringComparison.Ordinal));
        XElement[] bleuHeaderRows = bleuTable.Elements("thead").Elements("tr").ToArray();
        Assert.Equal(2, bleuHeaderRows.Length);
        Assert.Equal(new[] { "Model", "BLEU", "Training Cost (FLOPs)" },
            bleuHeaderRows[0].Elements("th").Select(static header => header.Value).ToArray());
        Assert.Equal(new[] { "EN-DE", "EN-FR", "EN-DE", "EN-FR" },
            bleuHeaderRows[1].Elements("th").Select(static header => header.Value).ToArray());
        XElement[] bleuGroupHeaders = bleuHeaderRows[0].Elements("th").ToArray();
        Assert.Equal("2", bleuGroupHeaders[0].Attribute("rowspan")?.Value);
        Assert.Equal("2", bleuGroupHeaders[1].Attribute("colspan")?.Value);
        Assert.Equal("2", bleuGroupHeaders[2].Attribute("colspan")?.Value);
        Assert.All(bleuGroupHeaders, header =>
            Assert.True(HasClass(header, "pdf-semantic-table-cell-border-top")));
        Assert.False(HasClass(bleuGroupHeaders[0], "pdf-semantic-table-cell-border-bottom"));
        Assert.True(HasClass(bleuGroupHeaders[1], "pdf-semantic-table-cell-border-bottom"));
        Assert.True(HasClass(bleuGroupHeaders[2], "pdf-semantic-table-cell-border-bottom"));
        Assert.Contains(bleuTable.Elements("tbody").Descendants("td"), cell =>
            cell.Value == "ByteNet [18]");
        Assert.Contains(bleuTable.Elements("tbody").Descendants("td"), cell =>
            cell.Value == "23.75");
        Assert.Contains(bleuTable.Elements("tbody").Descendants("td"), cell =>
            cell.Value == "Transformer (big)");
        Assert.Contains(bleuTable.Descendants(), cell =>
            HasClass(cell, "pdf-semantic-table-cell-border-bottom"));
        XElement convS2SEnsembleBleu = Assert.Single(bleuTable.Elements("tbody").Descendants("td"), cell =>
            cell.Value == "41.29");
        Assert.True(HasClass(convS2SEnsembleBleu, "pdf-semantic-bold") ||
            convS2SEnsembleBleu.Descendants().Any(value => HasClass(value, "pdf-semantic-bold")));
        XElement transformerBase = Assert.Single(bleuTable.Elements("tbody").Elements("tr"), row =>
            row.Elements("td").First().Value == "Transformer (base model)");
        XElement transformerBaseCost = transformerBase.Elements("td").Last();
        Assert.True(HasClass(transformerBaseCost, "pdf-semantic-bold") ||
            transformerBaseCost.Descendants().Any(value => HasClass(value, "pdf-semantic-bold")));
        XElement transformerBig = Assert.Single(bleuTable.Elements("tbody").Elements("tr"), row =>
            row.Elements("td").First().Value == "Transformer (big)");
        Assert.All(transformerBig.Elements("td"), cell =>
            Assert.True(HasClass(cell, "pdf-semantic-table-cell-border-bottom")));
        Assert.True(HasClass(transformerBig.Elements("td").ElementAt(1), "pdf-semantic-bold") ||
            transformerBig.Elements("td").ElementAt(1).Descendants().Any(value => HasClass(value, "pdf-semantic-bold")));
        Assert.True(HasClass(transformerBig.Elements("td").ElementAt(2), "pdf-semantic-bold") ||
            transformerBig.Elements("td").ElementAt(2).Descendants().Any(value => HasClass(value, "pdf-semantic-bold")));
        Assert.DoesNotContain("border-bottom: 0.35pt solid #d1d5db", html.Css, StringComparison.Ordinal);

        XElement residualDropoutParagraph = Assert.Single(ElementsByClass(dom, "pdf-semantic-paragraph"), paragraph =>
            paragraph.Value.Contains("Residual Dropout", StringComparison.Ordinal) &&
            paragraph.Value.Contains("For the base model", StringComparison.Ordinal));
        Assert.Contains(residualDropoutParagraph.Descendants("strong"), strong =>
            strong.Value == "Residual" &&
            HasClass(strong, "pdf-semantic-bold"));
        Assert.Contains(residualDropoutParagraph.Descendants("strong"), strong =>
            strong.Value == "Dropout" &&
            HasClass(strong, "pdf-semantic-bold"));

        XElement labelSmoothingParagraph = Assert.Single(ElementsByClass(dom, "pdf-semantic-paragraph"), paragraph =>
            paragraph.Value.Contains("Label Smoothing", StringComparison.Ordinal) &&
            paragraph.Value.Contains("label smoothing of value", StringComparison.Ordinal));
        Assert.Contains(labelSmoothingParagraph.Descendants("strong"), strong =>
            strong.Value == "Label" &&
            HasClass(strong, "pdf-semantic-bold"));
        Assert.Contains(labelSmoothingParagraph.Descendants("strong"), strong =>
            strong.Value == "Smoothing" &&
            HasClass(strong, "pdf-semantic-bold"));

        XElement variationTable = Assert.Single(semanticTables, table =>
            table.Value.Contains("Pdrop", StringComparison.Ordinal) &&
            table.Value.Contains("base", StringComparison.Ordinal) &&
            table.Value.Contains("big", StringComparison.Ordinal));
        XElement[] variationHeaderRows = variationTable.Elements("thead").Elements("tr").ToArray();
        Assert.Equal(2, variationHeaderRows.Length);
        Assert.Contains(variationHeaderRows[0].Elements("th"), header => header.Value == "train");
        Assert.Contains(variationHeaderRows[1].Elements("th"), header => header.Value == "steps");
        Assert.Contains(variationHeaderRows[0].Elements("th"), header => header.Value == "params");
        XElement parameterScaleHeader = Assert.Single(variationHeaderRows[1].Elements("th"), header =>
            header.Value.Contains("×106", StringComparison.Ordinal));
        Assert.Contains("×106", parameterScaleHeader.Value, StringComparison.Ordinal);
        Assert.Contains(parameterScaleHeader.Descendants("sup"), superscript => superscript.Value == "6");
        Assert.Contains(variationTable.Elements("tbody").Elements("tr"), row =>
            row.Elements("td").First().Value == "big" &&
            row.Elements("td").Last().Value == "213");
        Assert.Contains(variationTable.Descendants(), cell =>
            HasClass(cell, "pdf-semantic-table-cell-border-right"));

        XElement parserTable = Assert.Single(semanticTables, table =>
            table.Value.Contains("Parser", StringComparison.Ordinal) &&
            table.Value.Contains("WSJ 23 F1", StringComparison.Ordinal));
        Assert.Contains("pdf-semantic-measured-width", parserTable.Attribute("class")?.Value);
        Assert.DoesNotContain("pdf-semantic-table-centered-cells", parserTable.Attribute("class")?.Value ?? "");
        Dictionary<string, string> parserTableStyle = ParseStyle(parserTable.Attribute("style")?.Value ?? "");
        Assert.InRange(ParsePercent(parserTableStyle["--pdf-semantic-width"]), 80f, 97f);
        Assert.Equal("center", parserTableStyle["--pdf-semantic-align-self"]);
        XElement parserHeaderRow = Assert.Single(parserTable.Elements("thead").Elements("tr"));
        Assert.Equal(new[] { "Parser", "Training", "WSJ 23 F1" },
            parserHeaderRow.Elements("th").Select(static header => header.Value).ToArray());
        Assert.Contains(parserTable.Elements("tbody").Elements("tr"), row =>
            row.Elements("td").First().Value.StartsWith("Vinyals & Kaiser", StringComparison.Ordinal) &&
            row.Elements("td").Last().Value == "88.3");
        Assert.Contains(parserTable.Descendants(), cell =>
            HasClass(cell, "pdf-semantic-table-cell-border-right"));
        Assert.Contains(parserTable.Descendants(), cell =>
            HasClass(cell, "pdf-semantic-table-cell-align-center"));
    }

    [Fact]
    public async Task Convert_SemanticContinuousFlow_RendersReadableFlowInBrowser()
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
            TextMode = PdfHtmlTextMode.Semantic,
            SemanticPageMode = PdfHtmlSemanticPageMode.ContinuousFlow
        });

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

        ContinuousFlowMetrics metrics = await page.EvaluateAsync<ContinuousFlowMetrics>(
            """
            () => {
              const documentFlow = document.querySelector(".pdf-semantic-document-flow");
              const flow = document.querySelector(".pdf-semantic-continuous-flow");
              const markers = Array.from(document.querySelectorAll(".pdf-semantic-page-break"));
              const introduction = Array.from(document.querySelectorAll("h1"))
                .find(element => element.textContent === "1 Introduction");
              const pageThreeFooter = Array.from(document.querySelectorAll(".pdf-semantic-page-spanning > .pdf-semantic-footer"))
                .find(element => element.textContent.trim() === "3");
              const pageSixFooter = Array.from(document.querySelectorAll(".pdf-semantic-page-spanning > .pdf-semantic-footer"))
                .find(element => element.textContent.trim() === "6");
              const pageFourMarker = document.querySelector("#page-4");
              const documentBox = documentFlow.getBoundingClientRect();
              const flowBox = flow.getBoundingClientRect();
              const flowCenter = flowBox.left + flowBox.width / 2;
              const textCenterOffset = element => {
                const range = document.createRange();
                range.selectNodeContents(element);
                const textBox = range.getBoundingClientRect();
                range.detach();
                return Math.abs((textBox.left + textBox.width / 2) - flowCenter);
              };
              const childRightOverflow = Math.max(0, ...Array.from(flow.children)
                .filter(child => !child.classList.contains("pdf-semantic-page-break"))
                .map(child => child.getBoundingClientRect().right - documentBox.right));

              return {
                fixedPageCount: document.querySelectorAll(".pdf-page").length,
                markerCount: markers.length,
                documentWidth: documentBox.width,
                flowWidth: flowBox.width,
                firstMarkerTop: markers[0].getBoundingClientRect().top,
                secondMarkerTop: markers[1].getBoundingClientRect().top,
                introductionTop: introduction.getBoundingClientRect().top,
                pageThreeFooterBottom: pageThreeFooter.getBoundingClientRect().bottom,
                pageFourMarkerTop: pageFourMarker.getBoundingClientRect().top,
                pageThreeFooterCenterOffset: textCenterOffset(pageThreeFooter),
                pageSixFooterCenterOffset: textCenterOffset(pageSixFooter),
                childRightOverflow
              };
            }
            """);

        Assert.Equal(0, metrics.FixedPageCount);
        Assert.Equal(layout.Pages.Count, metrics.MarkerCount);
        Assert.InRange(metrics.DocumentWidth, 780, 840);
        Assert.InRange(metrics.FlowWidth, 500, 540);
        Assert.True(metrics.SecondMarkerTop > metrics.FirstMarkerTop);
        Assert.True(metrics.IntroductionTop > metrics.SecondMarkerTop);
        Assert.True(
            metrics.PageThreeFooterBottom <= metrics.PageFourMarkerTop + 1.0,
            $"Page 3 footer renders below the page 4 marker by {metrics.PageThreeFooterBottom - metrics.PageFourMarkerTop:0.###} CSS pixels.");
        Assert.True(
            metrics.PageThreeFooterCenterOffset <= 1.0,
            $"Page 3 footer text is {metrics.PageThreeFooterCenterOffset:0.###} CSS pixels away from center.");
        Assert.True(
            metrics.PageSixFooterCenterOffset <= 1.0,
            $"Page 6 footer text is {metrics.PageSixFooterCenterOffset:0.###} CSS pixels away from center.");
        Assert.True(
            metrics.ChildRightOverflow <= 1.0,
            $"Continuous semantic flow extends {metrics.ChildRightOverflow:0.###} CSS pixels outside the document column.");
    }

    [Fact]
    public async Task Convert_SemanticTextMode_DoesNotClipArxivPageFlow()
    {
        using PDDocument document = Loader.LoadPDF(Path.Combine(AppContext.BaseDirectory, "Fixtures", "arxiv-sample.pdf"));
        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document, new PdfLayoutOptions
        {
            IncludeImageAssets = true,
            IncludeLinks = false
        });
        PdfHtmlDocument html = PdfHtmlConverter.Convert(layout, new PdfHtmlOptions
        {
            TextMode = PdfHtmlTextMode.Semantic
        });

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
                Width = 1200,
                Height = 1400
            }
        });
        await page.GotoAsync(new Uri(Path.Combine(tempDirectory.Path, "index.html")).AbsoluteUri);

        double[] overflows = await page.EvaluateAsync<double[]>(
            """
            () => Array.from(document.querySelectorAll(".pdf-semantic-page"))
              .slice(0, 7)
              .map(page => {
                const flow = page.querySelector(".pdf-semantic-flow");
                if (!flow) {
                  return 0;
                }

                const pageBottom = page.getBoundingClientRect().bottom;
                const children = Array.from(flow.children);
                return Math.max(0, ...children.map(child => child.getBoundingClientRect().bottom - pageBottom));
              })
            """);

        string[] overflowDetails = await page.EvaluateAsync<string[]>(
            """
            () => Array.from(document.querySelectorAll(".pdf-semantic-page"))
              .slice(0, 7)
              .map((page, index) => {
                const flow = page.querySelector(".pdf-semantic-flow");
                if (!flow) {
                  return `page ${index + 1}: no semantic flow`;
                }

                const pageBottom = page.getBoundingClientRect().bottom;
                const child = Array.from(flow.children)
                  .map(child => ({ child, box: child.getBoundingClientRect() }))
                  .sort((left, right) => right.box.bottom - left.box.bottom)[0];
                if (!child) {
                  return `page ${index + 1}: no flow children`;
                }

                const classes = child.child.getAttribute("class") || child.child.tagName.toLowerCase();
                const text = (child.child.textContent || "").trim().replace(/\s+/g, " ").slice(0, 120);
                return `page ${index + 1}: ${classes}; bottom=${child.box.bottom.toFixed(3)}; pageBottom=${pageBottom.toFixed(3)}; text=${text}`;
              })
            """);

        Assert.True(overflows.Length >= 7);
        Assert.Equal(overflows.Length, overflowDetails.Length);
        for (int index = 0; index < overflows.Length; index++)
        {
            Assert.True(
                overflows[index] <= 1.0,
                $"Semantic flow on page {index + 1} extends {overflows[index]:0.###} CSS pixels below the page. {overflowDetails[index]}");
        }
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
    public void Convert_4ppHighlightingFixtureUsesFittedTextForCompressedGlyphBoxes()
    {
        using PDDocument document = Loader.LoadPDF(FixturePath("4PP-Highlighting.pdf"));
        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document, new PdfLayoutOptions
        {
            IncludeImageAssets = true
        });

        PdfHtmlDocument html = PdfHtmlConverter.Convert(layout);
        XDocument dom = ParseHtml(html.Html);
        XElement[] fittedRuns = ElementsByClass(dom, "pdf-text-run-svg").ToArray();
        XElement[] textRuns = ElementsByClass(dom, "pdf-text-run").ToArray();

        Assert.NotEmpty(fittedRuns);
        Assert.Equal(layout.Pages[0].Runs.Count, textRuns.Length);
        Assert.True(TextCoverage(layout.Text, dom.Root?.Value ?? "") >= 0.99);
        foreach (XElement textRun in textRuns.Take(8))
        {
            Dictionary<string, string> style = ParseStyle(textRun.Attribute("style")?.Value ?? "");
            Assert.True(ParsePoints(style["font-size"]) < 6);
        }
    }

    [Fact]
    public void Convert_UsesMeasuredSvgTextForUnknownBrowserFontMetrics()
    {
        PdfLayoutColor black = new(0f, 0f, 0f, 1f, "DeviceRGB");
        PdfLayoutRectangle pageBounds = new(0f, 0f, 612f, 792f);
        PdfLayoutRectangle textBounds = new(72f, 80f, 180f, 16f);
        PdfTextGlyph glyph = new("Custom display title", "SubsetDisplayFont", 20f, 0f, textBounds, black);
        PdfTextRun run = new("Custom display title", "SubsetDisplayFont", 20f, 0f, textBounds, black, [glyph]);
        PdfTextLine line = new(run.Text, textBounds, [run]);
        PdfLayoutPage page = new(
            1,
            pageBounds,
            pageBounds,
            pageBounds.Width,
            pageBounds.Height,
            0,
            [glyph],
            [run],
            [line],
            [new PdfTextBlock(run.Text, textBounds, [line])],
            [],
            [],
            [],
            [],
            []);
        PdfHtmlDocument html = PdfHtmlConverter.Convert(new PdfLayoutDocument([page], []));
        XDocument dom = ParseHtml(html.Html);

        XElement fittedRun = Assert.Single(ElementsByClass(dom, "pdf-text-run"));
        XElement fittedText = Assert.Single(fittedRun.Descendants(), element => HasClass(element, "pdf-text-run-svg"))
            .Descendants()
            .Single(element => element.Name.LocalName == "text");
        Assert.Equal("180", fittedText.Attribute("textLength")?.Value);
        Assert.Equal("spacingAndGlyphs", fittedText.Attribute("lengthAdjust")?.Value);
        Dictionary<string, string> style = ParseStyle(fittedRun.Attribute("style")?.Value ?? "");
        Assert.Equal(20f, ParsePoints(style["font-size"]));
    }

    [Fact]
    public void Convert_AcroFormFixtureEmitsWidgetAppearanceImageOverlays()
    {
        using PDDocument document = Loader.LoadPDF(FixturePath("Acroform-PDFBOX-2333.pdf"));
        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document, new PdfLayoutOptions
        {
            IncludeImageAssets = true
        });

        PdfLayoutImage[] appearanceImages = layout.Pages[0].Images
            .Where(image => image.Kind == PdfLayoutImageKind.AnnotationAppearance)
            .ToArray();
        PdfHtmlDocument html = PdfHtmlConverter.Convert(layout);
        XDocument dom = ParseHtml(html.Html);
        XElement[] imageElements = ElementsByClass(dom, "pdf-image")
            .Where(element => (element.Attribute("data-asset-id")?.Value ?? string.Empty).Contains("-annotation-", StringComparison.Ordinal))
            .ToArray();

        Assert.True(appearanceImages.Length >= 10);
        Assert.Equal(appearanceImages.Length, imageElements.Length);
        Assert.Equal(appearanceImages.Length, html.Assets.Count(asset => asset.ContentType == "image/png"));
        Assert.All(appearanceImages, image =>
        {
            Assert.True(image.Bounds.Width > 0);
            Assert.True(image.Bounds.Height > 0);
            Assert.True(image.IntrinsicWidth > 0);
            Assert.True(image.IntrinsicHeight > 0);
        });
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
    public void Convert_PaintsContainingVectorBackdropsBeforeImages()
    {
        using PDDocument document = CreateImageWithVectorBackdropDocument();
        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document, new PdfLayoutOptions
        {
            IncludeImageAssets = true
        });

        PdfHtmlDocument html = PdfHtmlConverter.Convert(layout);
        XDocument dom = ParseHtml(html.Html);
        XElement page = Assert.Single(ElementsByClass(dom, "pdf-page"));
        XElement backdropLayer = Assert.Single(ElementsByClass(dom, "pdf-vector-background-layer"));
        XElement image = Assert.Single(ElementsByClass(dom, "pdf-image"));
        XElement foregroundLayer = Assert.Single(page.Elements(), element =>
            HasClass(element, "pdf-vector-layer") &&
            !HasClass(element, "pdf-vector-background-layer"));

        Assert.Equal("background", backdropLayer.Attribute("data-vector-layer")?.Value);
        Assert.Equal("foreground", foregroundLayer.Attribute("data-vector-layer")?.Value);
        Assert.Equal("0", Assert.Single(backdropLayer.Descendants(),
                element => HasClass(element, "pdf-vector-path"))
            .Attribute("data-path-index")?.Value);
        Assert.Equal("1", Assert.Single(foregroundLayer.Descendants(),
                element => HasClass(element, "pdf-vector-path"))
            .Attribute("data-path-index")?.Value);

        XElement[] children = page.Elements().ToArray();
        Assert.True(Array.IndexOf(children, backdropLayer) < Array.IndexOf(children, image));
        Assert.True(Array.IndexOf(children, image) < Array.IndexOf(children, foregroundLayer));
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

    private static string FixturePath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName);
    }

    private static (int Width, int Height) PngDimensions(byte[] png)
    {
        Assert.True(png.Length >= 24);
        Assert.True(BinaryPrimitives.ReadUInt32BigEndian(png.AsSpan(0, 4)) == 0x89504E47u);
        return (
            BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16, 4)),
            BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20, 4)));
    }

    private sealed class ContinuousFlowMetrics
    {
        public int FixedPageCount { get; set; }

        public int MarkerCount { get; set; }

        public double DocumentWidth { get; set; }

        public double FlowWidth { get; set; }

        public double FirstMarkerTop { get; set; }

        public double SecondMarkerTop { get; set; }

        public double IntroductionTop { get; set; }

        public double PageThreeFooterBottom { get; set; }

        public double PageFourMarkerTop { get; set; }

        public double PageThreeFooterCenterOffset { get; set; }

        public double PageSixFooterCenterOffset { get; set; }

        public double ChildRightOverflow { get; set; }
    }

    private sealed class GridRenderMetrics
    {
        public int RowCount { get; set; }

        public int HighlightCount { get; set; }

        public double FirstCellLeft { get; set; }

        public double SecondCellLeft { get; set; }

        public double FirstCellTop { get; set; }

        public double FirstRowStep { get; set; }

        public double FirstHighlightWidth { get; set; }
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
            .Where(element => HasClass(element, className));
    }

    private static bool HasClass(XElement element, string className)
    {
        return (element.Attribute("class")?.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [])
            .Contains(className, StringComparer.Ordinal);
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

    private static float ParsePercent(string value)
    {
        Assert.EndsWith("%", value);
        return float.Parse(value[..^1], CultureInfo.InvariantCulture);
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

    private static PDDocument CreateLinkedTextDocument(float textY = 700)
    {
        PDDocument document = CreateTextDocument($"""
            BT
            /F1 12 Tf
            72 {textY.ToString(CultureInfo.InvariantCulture)} Td
            (Linked text) Tj
            ET
            """);
        PDAnnotationLink link = new();
        link.SetRectangle(new PDRectangle(72, textY - 20, 120, 24));
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

    private static PDDocument CreateImageWithVectorBackdropDocument()
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
            content.SetNonStrokingColor(1f, 1f, 1f);
            content.AddRect(60, 590, 144, 80);
            content.Fill();
            content.DrawImage(image, 72, 600, 120, 60);
            content.SetNonStrokingColor(1f, 0f, 0f);
            content.AddRect(210, 600, 12, 12);
            content.Fill();
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
