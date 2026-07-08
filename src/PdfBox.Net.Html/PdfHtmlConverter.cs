using System.Globalization;
using System.Net;
using System.Text;
using PdfBox.Net.Layout;

namespace PdfBox.Net.Html;

/// <summary>
/// Converts layout documents to fixed-layout HTML.
/// </summary>
public static class PdfHtmlConverter
{
    private const string Css = """
        .pdf-document {
          background: #f3f4f6;
          color: #111827;
          font-family: Arial, Helvetica, sans-serif;
          margin: 0;
          padding: 24pt;
        }

        .pdf-page {
          background: #fff;
          box-shadow: 0 1pt 4pt rgba(17, 24, 39, 0.16);
          margin: 0 auto 24pt;
          overflow: hidden;
          position: relative;
        }

        .pdf-text-run {
          color: #111827;
          display: block;
          line-height: 1;
          margin: 0;
          padding: 0;
          position: absolute;
          transform-origin: 0 0;
          white-space: pre;
          z-index: 1;
        }

        .pdf-semantic-element {
          box-sizing: border-box;
          color: #111827;
          margin: 0;
          overflow: visible;
          padding: 0;
          z-index: 1;
        }

        .pdf-page.pdf-semantic-page {
          box-sizing: border-box;
        }

        .pdf-semantic-document-flow {
          background: #fff;
          box-shadow: 0 1pt 4pt rgba(17, 24, 39, 0.16);
          box-sizing: border-box;
          margin: 0 auto 24pt;
          padding: 54pt 72pt 48pt;
          width: min(612pt, calc(100vw - 48pt));
        }

        .pdf-semantic-flow {
          box-sizing: border-box;
          display: flex;
          flex-direction: column;
          margin: 0 auto;
          min-height: 100%;
          padding: 54pt 0 36pt;
          width: min(396pt, calc(100% - 144pt));
        }

        .pdf-semantic-continuous-flow {
          min-height: 0;
          padding: 0;
          width: min(396pt, 100%);
        }

        .pdf-semantic-flow > * + * {
          margin-top: 0;
        }

        .pdf-semantic-page-break {
          border: 0;
          border-top: 1pt dashed #d1d5db;
          break-before: page;
          color: #6b7280;
          margin: 26pt 0 18pt;
          page-break-before: always;
          text-align: center;
        }

        .pdf-semantic-page-break::after {
          background: #fff;
          content: "Page " attr(data-page-number);
          font: 8pt Arial, Helvetica, sans-serif;
          padding: 0 6pt;
          position: relative;
          top: -0.65em;
        }

        .pdf-semantic-page-start {
          border-top: 0;
          break-before: auto;
          margin: 0 0 14pt;
          page-break-before: auto;
        }

        .pdf-semantic-page-artifacts {
          color: #6b7280;
          margin-bottom: 12pt;
        }

        .pdf-semantic-page-artifacts .pdf-semantic-element {
          line-height: 1.2;
          margin-bottom: 4pt;
        }

        .pdf-semantic-flow header {
          line-height: 1.25;
          margin-bottom: 26pt;
        }

        .pdf-semantic-flow h1,
        .pdf-semantic-flow h2,
        .pdf-semantic-flow h3,
        .pdf-semantic-flow h4,
        .pdf-semantic-flow h5,
        .pdf-semantic-flow h6 {
          font-weight: 600;
          line-height: 1.12;
          margin-bottom: 8pt;
        }

        .pdf-semantic-title {
          border-bottom: 1pt solid currentColor;
          border-top: 4pt solid currentColor;
          padding: 9pt 0 10pt;
        }

        .pdf-semantic-heading {
          font-weight: 600;
          line-height: 1.12;
        }

        .pdf-semantic-paragraph {
          line-height: 1.18;
          margin-bottom: 6pt;
        }

        .pdf-semantic-justified {
          text-align: justify;
          text-align-last: left;
        }

        .pdf-semantic-measured-width {
          align-self: var(--pdf-semantic-align-self, flex-start);
          width: min(100%, var(--pdf-semantic-width, 100%));
        }

        .pdf-semantic-align-center {
          text-align: center;
        }

        .pdf-semantic-align-right {
          text-align: right;
        }

        .pdf-semantic-line-row {
          column-gap: 18pt;
          display: grid;
          grid-template-columns: repeat(var(--pdf-semantic-line-count, 2), minmax(0, 1fr));
          margin-bottom: 6pt;
        }

        .pdf-semantic-line-row .pdf-semantic-line {
          min-width: 0;
        }

        .pdf-semantic-caption {
          line-height: 1.18;
        }

        .pdf-semantic-authors {
          display: flex;
          flex-direction: column;
          gap: 14pt;
          margin: 16pt 0 28pt;
        }

        .pdf-semantic-author-row {
          display: grid;
          gap: 18pt;
          justify-content: center;
        }

        .pdf-author-count-1 { grid-template-columns: minmax(78pt, 96pt); }
        .pdf-author-count-2 { grid-template-columns: repeat(2, minmax(78pt, 1fr)); }
        .pdf-author-count-3 { grid-template-columns: repeat(3, minmax(78pt, 1fr)); }
        .pdf-author-count-4 { grid-template-columns: repeat(4, minmax(72pt, 1fr)); }
        .pdf-author-count-5 { grid-template-columns: repeat(5, minmax(64pt, 1fr)); }
        .pdf-author-count-6 { grid-template-columns: repeat(6, minmax(58pt, 1fr)); }

        .pdf-semantic-authors address {
          font-style: normal;
          line-height: 1.15;
          text-align: center;
        }

        .pdf-semantic-line {
          display: block;
        }

        .pdf-semantic-author-block .pdf-semantic-line + .pdf-semantic-line {
          margin-top: 1pt;
        }

        .pdf-semantic-figure-space {
          display: block;
          flex: 0 0 auto;
          margin: 0 0 12pt;
          pointer-events: none;
        }

        .pdf-semantic-footnotes {
          margin-top: 18pt;
          padding-top: 0;
        }

        .pdf-semantic-footnotes::before {
          border-top: var(--pdf-footnote-rule-thickness, 0.5pt) solid var(--pdf-footnote-rule-color, #9ca3af);
          content: "";
          display: block;
          margin-bottom: 6pt;
          width: var(--pdf-footnote-rule-width, 144pt);
        }

        .pdf-semantic-footnotes p {
          line-height: 1.18;
          margin: 0 0 4pt;
        }

        .pdf-semantic-footnote-ref,
        .pdf-semantic-footnote-backref {
          color: inherit;
          text-decoration: none;
        }

        .pdf-semantic-footer,
        .pdf-semantic-header {
          line-height: 1.1;
        }

        .pdf-semantic-flow > footer.pdf-semantic-footer {
          margin-top: auto;
          padding-top: 16pt;
        }

        .pdf-semantic-positioned {
          position: absolute;
        }

        .pdf-semantic-vertical {
          transform: rotate(-90deg);
          transform-origin: left top;
          white-space: nowrap;
        }

        .pdf-link-overlay {
          background: transparent;
          display: block;
          outline: none;
          position: absolute;
          z-index: 3;
        }

        .pdf-image {
          display: block;
          object-fit: fill;
          position: absolute;
          z-index: 0;
        }

        .pdf-vector-layer {
          display: block;
          left: 0;
          overflow: visible;
          pointer-events: none;
          position: absolute;
          top: 0;
          z-index: 0;
        }
        """;

    /// <summary>
    /// Converts an extracted layout document to fixed-layout HTML.
    /// </summary>
    /// <param name="layout">The layout document.</param>
    /// <param name="options">HTML conversion options.</param>
    /// <returns>The generated HTML document.</returns>
    public static PdfHtmlDocument Convert(PdfLayoutDocument layout, PdfHtmlOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(layout);

        options ??= new PdfHtmlOptions();
        string cssPath = NormalizeCssPath(options.CssPath);
        Dictionary<string, PdfLayoutImageAsset> imageAssets = layout.ImageAssets
            .GroupBy(asset => asset.AssetId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        bool semanticText = options.TextMode == PdfHtmlTextMode.Semantic;
        bool continuousSemanticFlow = semanticText && options.SemanticPageMode == PdfHtmlSemanticPageMode.ContinuousFlow;
        PdfSemanticDocument? semantic = semanticText
            ? PdfSemanticExtractor.Extract(layout, options.SemanticExtractionOptions)
            : null;
        StringBuilder html = new();
        html.AppendLine("<!doctype html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("  <meta charset=\"utf-8\" />");
        html.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
        html.Append("  <title>").Append(Html(options.Title)).AppendLine("</title>");
        html.Append("  <link rel=\"stylesheet\" href=\"").Append(HtmlAttribute(cssPath)).AppendLine("\" />");
        html.AppendLine("</head>");
        html.Append("<body class=\"pdf-document");
        if (continuousSemanticFlow)
        {
            html.Append(" pdf-document-continuous");
        }

        html.AppendLine("\">");

        if (continuousSemanticFlow && semantic != null)
        {
            WriteSemanticContinuousDocument(html, layout, semantic, options.Scale);
        }
        else
        {
            for (int index = 0; index < layout.Pages.Count; index++)
            {
                WritePage(
                    html,
                    layout.Pages[index],
                    imageAssets,
                    options.Scale,
                    semantic?.Pages[index],
                    options.TextMode);
            }
        }

        html.AppendLine("</body>");
        html.AppendLine("</html>");
        PdfHtmlAsset[] assets = imageAssets.Values
            .Select(asset => new PdfHtmlAsset(asset.RelativePath, asset.ContentType, asset.Data))
            .ToArray();
        return new PdfHtmlDocument(html.ToString(), cssPath, BuildCss(semantic), assets);
    }

    private static string BuildCss(PdfSemanticDocument? semantic)
    {
        if (semantic == null)
        {
            return Css;
        }

        StringBuilder css = new(Css);
        css.AppendLine();
        foreach (string fontName in semantic.Elements
            .SelectMany(static element => element.Lines)
            .Select(static line => line.DominantFontName)
            .Where(static fontName => !string.IsNullOrWhiteSpace(fontName))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal))
        {
            css.Append('.')
                .Append(FontClass(fontName))
                .Append("{font-family:")
                .Append(CssFontFamily(fontName))
                .AppendLine("}");
        }

        foreach (float fontSize in semantic.Elements
            .SelectMany(static element => element.Lines)
            .Select(static line => MathF.Round(line.DominantFontSize * 2f) / 2f)
            .Distinct()
            .Order())
        {
            css.Append('.')
                .Append(FontSizeClass(fontSize))
                .Append("{font-size:")
                .Append(CssPoints(fontSize))
                .AppendLine("}");
        }

        foreach (PdfLayoutColor color in semantic.Elements
            .SelectMany(static element => element.Lines)
            .Select(static line => line.Color)
            .Distinct()
            .OrderBy(static color => color.Red)
            .ThenBy(static color => color.Green)
            .ThenBy(static color => color.Blue)
            .ThenBy(static color => color.Alpha))
        {
            css.Append('.')
                .Append(ColorClass(color))
                .Append("{color:")
                .Append(ColorHex(color));
            if (color.Alpha < 0.999f)
            {
                css.Append(";opacity:")
                    .Append(SvgNumber(color.Alpha));
            }

            css.AppendLine("}");
        }

        return css.ToString();
    }

    private static void WritePage(
        StringBuilder html,
        PdfLayoutPage page,
        IReadOnlyDictionary<string, PdfLayoutImageAsset> imageAssets,
        float scale,
        PdfSemanticPage? semanticPage,
        PdfHtmlTextMode textMode)
    {
        html.Append("  <section class=\"pdf-page");
        if (textMode == PdfHtmlTextMode.Semantic)
        {
            html.Append(" pdf-semantic-page");
        }

        html.Append("\" data-page-number=\"")
            .Append(page.PageNumber.ToString(CultureInfo.InvariantCulture))
            .Append("\" id=\"page-")
            .Append(page.PageNumber.ToString(CultureInfo.InvariantCulture))
            .Append("\" style=\"width:")
            .Append(CssPoints(page.Width * scale))
            .Append(";height:")
            .Append(CssPoints(page.Height * scale))
            .AppendLine("\">");

        foreach (PdfLayoutImage image in page.Images)
        {
            if (imageAssets.TryGetValue(image.AssetId, out PdfLayoutImageAsset? asset))
            {
                WriteImage(html, image, asset, scale);
            }
        }

        if (page.Paths.Count > 0)
        {
            WriteVectorLayer(
                html,
                page,
                scale,
                textMode == PdfHtmlTextMode.Semantic ? semanticPage : null);
        }

        if (textMode == PdfHtmlTextMode.Semantic && semanticPage != null)
        {
            WriteSemanticPage(html, page, semanticPage, scale);
        }
        else
        {
            foreach (PdfTextRun run in page.Runs)
            {
                WriteTextRun(html, run, scale);
            }
        }

        foreach (PdfLayoutLink link in page.Links)
        {
            WriteLink(html, link, scale);
        }

        html.AppendLine("  </section>");
    }

    private static void WriteImage(StringBuilder html, PdfLayoutImage image, PdfLayoutImageAsset asset, float scale)
    {
        html.Append("    <img class=\"pdf-image\" src=\"")
            .Append(HtmlAttribute(asset.RelativePath))
            .Append("\" alt=\"\" data-asset-id=\"")
            .Append(HtmlAttribute(image.AssetId));

        if (!string.IsNullOrEmpty(image.SourceName))
        {
            html.Append("\" data-source-name=\"")
                .Append(HtmlAttribute(image.SourceName));
        }

        html.Append("\" style=\"position:absolute;left:")
            .Append(CssPoints(image.Bounds.X * scale))
            .Append(";top:")
            .Append(CssPoints(image.Bounds.Y * scale))
            .Append(";width:")
            .Append(CssPoints(image.Bounds.Width * scale))
            .Append(";height:")
            .Append(CssPoints(image.Bounds.Height * scale))
            .AppendLine("\" />");
    }

    private static void WriteVectorLayer(
        StringBuilder html,
        PdfLayoutPage page,
        float scale,
        PdfSemanticPage? semanticPage)
    {
        PdfLayoutPath[] paths = page.Paths
            .Where(path => semanticPage == null || !IsSemanticFlowRulePath(page, semanticPage, path))
            .ToArray();
        if (paths.Length == 0)
        {
            return;
        }

        html.Append("    <svg class=\"pdf-vector-layer\" data-path-count=\"")
            .Append(paths.Length.ToString(CultureInfo.InvariantCulture))
            .Append("\" viewBox=\"0 0 ")
            .Append(SvgNumber(page.Width))
            .Append(' ')
            .Append(SvgNumber(page.Height))
            .Append("\" style=\"position:absolute;left:0;top:0;width:")
            .Append(CssPoints(page.Width * scale))
            .Append(";height:")
            .Append(CssPoints(page.Height * scale))
            .AppendLine("\" aria-hidden=\"true\">");

        foreach (PdfLayoutPath path in paths)
        {
            WriteVectorPath(html, path);
        }

        html.AppendLine("    </svg>");
    }

    private static void WriteVectorPath(StringBuilder html, PdfLayoutPath path)
    {
        html.Append("      <path class=\"pdf-vector-path\" data-path-index=\"")
            .Append(path.Index.ToString(CultureInfo.InvariantCulture))
            .Append("\" d=\"")
            .Append(HtmlAttribute(SvgPathData(path.Commands)))
            .Append("\"");

        if (path.FillColor is PdfLayoutColor fill)
        {
            html.Append(" fill=\"")
                .Append(ColorHex(fill))
                .Append("\" fill-opacity=\"")
                .Append(SvgNumber(fill.Alpha))
                .Append("\" fill-rule=\"")
                .Append(FillRule(path.FillRule))
                .Append("\"");
        }
        else
        {
            html.Append(" fill=\"none\"");
        }

        if (path.Stroke is PdfLayoutStrokeStyle stroke)
        {
            html.Append(" stroke=\"")
                .Append(ColorHex(stroke.Color))
                .Append("\" stroke-opacity=\"")
                .Append(SvgNumber(stroke.Color.Alpha))
                .Append("\" stroke-width=\"")
                .Append(SvgNumber(stroke.Width))
                .Append("\" stroke-linecap=\"")
                .Append(LineCap(stroke.LineCap))
                .Append("\" stroke-linejoin=\"")
                .Append(LineJoin(stroke.LineJoin))
                .Append("\" stroke-miterlimit=\"")
                .Append(SvgNumber(stroke.MiterLimit))
                .Append("\"");

            if (stroke.DashArray.Count > 0 && stroke.DashArray.Any(dash => dash > 0))
            {
                html.Append(" stroke-dasharray=\"")
                    .Append(string.Join(" ", stroke.DashArray.Select(SvgNumber)))
                    .Append("\" stroke-dashoffset=\"")
                    .Append(SvgNumber(stroke.DashPhase))
                    .Append("\"");
            }
        }
        else
        {
            html.Append(" stroke=\"none\"");
        }

        html.AppendLine(" />");
    }

    private static void WriteTextRun(StringBuilder html, PdfTextRun run, float scale)
    {
        html.Append("    <span class=\"pdf-text-run\" data-font=\"")
            .Append(HtmlAttribute(run.FontName))
            .Append("\" style=\"position:absolute;left:")
            .Append(CssPoints(run.Bounds.X * scale))
            .Append(";top:")
            .Append(CssPoints(run.Bounds.Y * scale))
            .Append(";width:")
            .Append(CssPoints(run.Bounds.Width * scale))
            .Append(";height:")
            .Append(CssPoints(run.Bounds.Height * scale))
            .Append(";font-size:")
            .Append(CssPoints(run.FontSize * scale))
            .Append(";font-family:")
            .Append(CssFontFamily(run.FontName))
            .Append(";color:")
            .Append(ColorHex(run.Color))
            .Append("\">")
            .Append(Html(run.Text))
            .AppendLine("</span>");
    }

    private static void WriteSemanticPage(
        StringBuilder html,
        PdfLayoutPage page,
        PdfSemanticPage semanticPage,
        float scale)
    {
        FootnoteContext footnotes = FootnoteContext.Create(page.PageNumber, semanticPage.Elements);
        PdfSemanticElement[] positioned = semanticPage.Elements
            .Where(IsPositionedSemanticElement)
            .ToArray();
        foreach (PdfSemanticElement element in positioned)
        {
            WritePositionedSemanticElement(html, page, element, footnotes, scale);
        }

        HashSet<PdfSemanticElement> positionedSet = positioned.ToHashSet();
        PdfSemanticElement[] flowElements = semanticPage.Elements
            .Where(element => !positionedSet.Contains(element))
            .ToArray();
        html.AppendLine("    <article class=\"pdf-semantic-flow\">");
        WriteSemanticFlowElements(
            html,
            page,
            semanticPage,
            flowElements,
            footnotes,
            scale,
            includeFigureSpaces: true,
            omitSimplePageNumberFooters: false);
        html.AppendLine("    </article>");
    }

    private static void WriteSemanticContinuousDocument(
        StringBuilder html,
        PdfLayoutDocument layout,
        PdfSemanticDocument semantic,
        float scale)
    {
        html.AppendLine("  <main class=\"pdf-semantic-document-flow\">");
        html.AppendLine("    <article class=\"pdf-semantic-flow pdf-semantic-continuous-flow\">");

        for (int index = 0; index < layout.Pages.Count; index++)
        {
            PdfLayoutPage page = layout.Pages[index];
            PdfSemanticPage semanticPage = semantic.Pages[index];
            WriteSemanticPageBreak(html, page.PageNumber, isFirstPage: index == 0);

            FootnoteContext footnotes = FootnoteContext.Create(page.PageNumber, semanticPage.Elements);
            PdfSemanticElement[] positioned = semanticPage.Elements
                .Where(IsPositionedSemanticElement)
                .ToArray();
            WriteContinuousPageArtifacts(html, page, positioned, footnotes);

            HashSet<PdfSemanticElement> positionedSet = positioned.ToHashSet();
            PdfSemanticElement[] flowElements = semanticPage.Elements
                .Where(element => !positionedSet.Contains(element))
                .ToArray();
            WriteSemanticFlowElements(
                html,
                page,
                semanticPage,
                flowElements,
                footnotes,
                scale,
                includeFigureSpaces: false,
                omitSimplePageNumberFooters: true);
        }

        html.AppendLine("    </article>");
        html.AppendLine("  </main>");
    }

    private static void WriteSemanticPageBreak(StringBuilder html, int pageNumber, bool isFirstPage)
    {
        string pageNumberText = pageNumber.ToString(CultureInfo.InvariantCulture);
        html.Append("      <div class=\"pdf-semantic-page-break");
        if (isFirstPage)
        {
            html.Append(" pdf-semantic-page-start");
        }

        html.Append("\" id=\"page-")
            .Append(pageNumberText)
            .Append("\" data-page-number=\"")
            .Append(pageNumberText)
            .Append("\" role=\"separator\" aria-label=\"Original PDF page ")
            .Append(pageNumberText)
            .AppendLine("\"></div>");
    }

    private static void WriteContinuousPageArtifacts(
        StringBuilder html,
        PdfLayoutPage page,
        IReadOnlyList<PdfSemanticElement> elements,
        FootnoteContext footnotes)
    {
        if (elements.Count == 0)
        {
            return;
        }

        html.Append("      <aside class=\"pdf-semantic-page-artifacts\" aria-label=\"Original page ")
            .Append(page.PageNumber.ToString(CultureInfo.InvariantCulture))
            .AppendLine(" artifacts\">");
        foreach (PdfSemanticElement element in elements)
        {
            WriteFlowSemanticElement(html, element, footnotes, page, allowMeasuredWidth: false);
        }

        html.AppendLine("      </aside>");
    }

    private static void WriteSemanticFlowElements(
        StringBuilder html,
        PdfLayoutPage page,
        PdfSemanticPage semanticPage,
        IReadOnlyList<PdfSemanticElement> flowElements,
        FootnoteContext footnotes,
        float scale,
        bool includeFigureSpaces,
        bool omitSimplePageNumberFooters)
    {
        PdfLayoutRectangle[] figureRegions = includeFigureSpaces
            ? SemanticFigureRegions(page, semanticPage).ToArray()
            : [];
        int nextFigureRegion = 0;
        for (int index = 0; index < flowElements.Count; index++)
        {
            PdfSemanticElement element = flowElements[index];
            if (omitSimplePageNumberFooters && IsSimplePageNumberFooter(element, page))
            {
                continue;
            }

            while (nextFigureRegion < figureRegions.Length &&
                ShouldInsertFigureSpaceBefore(element, figureRegions[nextFigureRegion]))
            {
                WriteFigureSpace(html, figureRegions[nextFigureRegion], scale);
                nextFigureRegion++;
            }

            if (element.Kind == PdfSemanticElementKind.AuthorBlock)
            {
                index = WriteAuthorSection(html, flowElements, index, footnotes);
                continue;
            }

            if (element.Kind == PdfSemanticElementKind.Footnote)
            {
                index = WriteFootnoteSection(
                    html,
                    flowElements,
                    index,
                    footnotes,
                    DecorativeFootnoteRulePath(page, semanticPage));
                continue;
            }

            WriteFlowSemanticElement(
                html,
                element,
                footnotes,
                page,
                allowMeasuredWidth: IsMeasuredWidthCandidate(flowElements, index));
        }
    }

    private static void WriteFigureSpace(StringBuilder html, PdfLayoutRectangle region, float scale)
    {
        html.Append("      <figure class=\"pdf-semantic-figure-space\" aria-hidden=\"true\" data-source-top=\"")
            .Append(HtmlAttribute(CssPoints(region.Y)))
            .Append("\" style=\"height:")
            .Append(CssPoints((region.Height + 18f) * scale))
            .AppendLine("\"></figure>");
    }

    private static IEnumerable<PdfLayoutRectangle> SemanticFigureRegions(
        PdfLayoutPage page,
        PdfSemanticPage semanticPage)
    {
        List<PdfLayoutRectangle> regions = [];
        regions.AddRange(page.Images
            .Select(static image => image.Bounds)
            .Where(bounds => IsSubstantialGraphic(page, bounds)));

        PdfLayoutPath[] candidatePaths = page.Paths
            .Where(path => !IsSemanticFlowRulePath(page, semanticPage, path))
            .Where(path => path.Bounds.Width > 2f && path.Bounds.Height > 2f)
            .ToArray();
        PdfLayoutRectangle[] largePathBounds = candidatePaths
            .Select(static path => path.Bounds)
            .Where(bounds => IsSubstantialGraphic(page, bounds))
            .ToArray();
        regions.AddRange(largePathBounds);

        if (candidatePaths.Length >= 8)
        {
            PdfLayoutRectangle union = UnionRectangles(candidatePaths.Select(static path => path.Bounds));
            if (IsSubstantialGraphic(page, union))
            {
                regions.Add(union);
            }
        }

        foreach (PdfLayoutRectangle region in MergeGraphicRegions(regions))
        {
            yield return region;
        }
    }

    private static bool IsSubstantialGraphic(PdfLayoutPage page, PdfLayoutRectangle bounds)
    {
        return bounds.Width >= page.Width * 0.18f &&
            bounds.Height >= MathF.Max(18f, page.Height * 0.035f);
    }

    private static IEnumerable<PdfLayoutRectangle> MergeGraphicRegions(IEnumerable<PdfLayoutRectangle> regions)
    {
        List<PdfLayoutRectangle> merged = [];
        foreach (PdfLayoutRectangle region in regions
            .OrderBy(static region => region.Y)
            .ThenBy(static region => region.X))
        {
            if (merged.Count == 0)
            {
                merged.Add(region);
                continue;
            }

            PdfLayoutRectangle last = merged[^1];
            bool closeVertically = region.Y <= last.Bottom + 18f;
            bool overlapsHorizontally = region.X <= last.Right + 18f && region.Right + 18f >= last.X;
            if (closeVertically && overlapsHorizontally)
            {
                merged[^1] = UnionRectangles([last, region]);
            }
            else
            {
                merged.Add(region);
            }
        }

        return merged;
    }

    private static PdfLayoutRectangle UnionRectangles(IEnumerable<PdfLayoutRectangle> rectangles)
    {
        using IEnumerator<PdfLayoutRectangle> enumerator = rectangles.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            return new PdfLayoutRectangle(0, 0, 0, 0);
        }

        PdfLayoutRectangle first = enumerator.Current;
        float left = first.X;
        float top = first.Y;
        float right = first.Right;
        float bottom = first.Bottom;
        while (enumerator.MoveNext())
        {
            PdfLayoutRectangle rectangle = enumerator.Current;
            left = MathF.Min(left, rectangle.X);
            top = MathF.Min(top, rectangle.Y);
            right = MathF.Max(right, rectangle.Right);
            bottom = MathF.Max(bottom, rectangle.Bottom);
        }

        return new PdfLayoutRectangle(left, top, right - left, bottom - top);
    }

    private static float HorizontalGap(PdfLayoutRectangle first, PdfLayoutRectangle second)
    {
        if (first.Right < second.X)
        {
            return second.X - first.Right;
        }

        if (second.Right < first.X)
        {
            return first.X - second.Right;
        }

        return 0f;
    }

    private static bool ShouldInsertFigureSpaceBefore(PdfSemanticElement element, PdfLayoutRectangle figureRegion)
    {
        return element.Bounds.Y >= figureRegion.Y - 2f;
    }

    private static bool IsMeasuredWidthCandidate(IReadOnlyList<PdfSemanticElement> elements, int index)
    {
        return index + 1 >= elements.Count ||
            elements[index + 1].Kind != PdfSemanticElementKind.Footer;
    }

    private static bool IsSimplePageNumberFooter(PdfSemanticElement element, PdfLayoutPage page)
    {
        return element.Kind == PdfSemanticElementKind.Footer &&
            string.Equals(
                element.Text.Trim(),
                page.PageNumber.ToString(CultureInfo.InvariantCulture),
                StringComparison.Ordinal);
    }

    private static int WriteAuthorSection(
        StringBuilder html,
        IReadOnlyList<PdfSemanticElement> elements,
        int startIndex,
        FootnoteContext footnotes)
    {
        int index = startIndex;
        List<PdfSemanticElement> authors = [];
        for (; index < elements.Count && elements[index].Kind == PdfSemanticElementKind.AuthorBlock; index++)
        {
            authors.Add(elements[index]);
        }

        html.AppendLine("      <section class=\"pdf-semantic-authors\" aria-label=\"Authors\">");
        foreach (IReadOnlyList<PdfSemanticElement> row in GroupAuthorRows(authors))
        {
            html.Append("        <div class=\"pdf-semantic-author-row ")
                .Append(AuthorCountClass(row.Count))
                .AppendLine("\">");
            foreach (PdfSemanticElement author in row)
            {
                WriteFlowSemanticElement(html, author, footnotes);
            }

            html.AppendLine("        </div>");
        }

        html.AppendLine("      </section>");
        return index - 1;
    }

    private static IEnumerable<IReadOnlyList<PdfSemanticElement>> GroupAuthorRows(IReadOnlyList<PdfSemanticElement> authors)
    {
        List<List<PdfSemanticElement>> rows = [];
        foreach (PdfSemanticElement author in authors
            .OrderBy(static author => AuthorCenterY(author))
            .ThenBy(static author => author.Bounds.X))
        {
            List<PdfSemanticElement>? row = rows.FirstOrDefault(existing =>
                MathF.Abs(AuthorCenterY(existing[0]) - AuthorCenterY(author)) <= AuthorRowTolerance(existing[0], author));
            if (row == null)
            {
                rows.Add([author]);
            }
            else
            {
                row.Add(author);
            }
        }

        return rows
            .OrderBy(static row => row.Average(static author => AuthorCenterY(author)))
            .Select(static row => row.OrderBy(static author => author.Bounds.X).ToArray());
    }

    private static float AuthorCenterY(PdfSemanticElement author)
    {
        return author.Bounds.Y + (author.Bounds.Height / 2f);
    }

    private static float AuthorRowTolerance(PdfSemanticElement first, PdfSemanticElement second)
    {
        return Math.Clamp(MathF.Min(first.Bounds.Height, second.Bounds.Height) * 0.45f, 8f, 18f);
    }

    private static string AuthorCountClass(int count)
    {
        return "pdf-author-count-" + Math.Clamp(count, 1, 6).ToString(CultureInfo.InvariantCulture);
    }

    private static int WriteFootnoteSection(
        StringBuilder html,
        IReadOnlyList<PdfSemanticElement> elements,
        int startIndex,
        FootnoteContext footnotes,
        PdfLayoutPath? footnoteRule)
    {
        html.Append("      <section class=\"pdf-semantic-footnotes\" aria-label=\"Footnotes\"");
        if (footnoteRule != null)
        {
            html.Append(" style=\"")
                .Append(HtmlAttribute(FootnoteRuleStyle(footnoteRule)))
                .Append('"');
        }

        html.AppendLine(">");
        int index = startIndex;
        for (; index < elements.Count && elements[index].Kind == PdfSemanticElementKind.Footnote; index++)
        {
            WriteFootnote(html, elements[index], footnotes);
        }

        html.AppendLine("      </section>");
        return index - 1;
    }

    private static string FootnoteRuleStyle(PdfLayoutPath path)
    {
        PdfLayoutColor color = path.Stroke?.Color ?? path.FillColor ?? new PdfLayoutColor(0, 0, 0, 1, null);
        float thickness = path.Stroke?.Width ?? MathF.Max(0.5f, path.Bounds.Height);
        return "--pdf-footnote-rule-width:" + CssPoints(path.Bounds.Width) +
            ";--pdf-footnote-rule-thickness:" + CssPoints(thickness) +
            ";--pdf-footnote-rule-color:" + ColorHex(color);
    }

    private static void WriteFlowSemanticElement(
        StringBuilder html,
        PdfSemanticElement element,
        FootnoteContext footnotes,
        PdfLayoutPage? page = null,
        bool allowMeasuredWidth = true)
    {
        string tagName = SemanticTagName(element);
        html.Append("      <")
            .Append(tagName)
            .Append(" class=\"")
            .Append(SemanticClassNames(element, page, allowMeasuredWidth))
            .Append('"');
        string style = FlowSemanticStyle(element, page, allowMeasuredWidth);
        if (style.Length > 0)
        {
            html.Append(" style=\"")
                .Append(HtmlAttribute(style))
                .Append('"');
        }

        html.Append(">");
        WriteSemanticText(html, element, footnotes);
        html.Append("</")
            .Append(tagName)
            .AppendLine(">");
    }

    private static void WritePositionedSemanticElement(
        StringBuilder html,
        PdfLayoutPage page,
        PdfSemanticElement element,
        FootnoteContext footnotes,
        float scale)
    {
        string tagName = SemanticTagName(element);
        html.Append("    <")
            .Append(tagName)
            .Append(" class=\"")
            .Append(SemanticClassNames(element))
            .Append(" pdf-semantic-positioned pdf-semantic-vertical")
            .Append("\" style=\"")
            .Append(PositionStyle(page, element, scale))
            .Append("\">");
        WriteSemanticText(html, element, footnotes);
        html.Append("</")
            .Append(tagName)
            .AppendLine(">");
    }

    private static void WriteSemanticText(
        StringBuilder html,
        PdfSemanticElement element,
        FootnoteContext footnotes)
    {
        if (element.Kind == PdfSemanticElementKind.AuthorBlock)
        {
            foreach (PdfSemanticLine line in element.Lines)
            {
                html.Append("<span class=\"")
                    .Append(SemanticLineClassNames(line))
                    .Append("\">");
                WriteTextWithFootnoteReferences(html, line.Text, footnotes);
                html.Append("</span>");
            }

            return;
        }

        if (IsSameRowLineGroup(element))
        {
            foreach (PdfSemanticLine line in SameRowLines(element))
            {
                html.Append("<span class=\"")
                    .Append(SemanticLineClassNames(line))
                    .Append("\">");
                WriteTextWithFootnoteReferences(html, line.Text, footnotes);
                html.Append("</span>");
            }

            return;
        }

        if (element.Kind is PdfSemanticElementKind.Header or PdfSemanticElementKind.Footer)
        {
            for (int index = 0; index < element.Lines.Count; index++)
            {
                if (index > 0)
                {
                    html.Append("<br />");
                }

                WriteTextWithFootnoteReferences(html, element.Lines[index].Text, footnotes);
            }

            return;
        }

        WriteTextWithFootnoteReferences(html, element.Text, footnotes);
    }

    private static void WriteFootnote(
        StringBuilder html,
        PdfSemanticElement element,
        FootnoteContext footnotes)
    {
        string text = element.Text.Trim();
        string marker = text.Length > 0 ? text[..1] : "";
        string body = footnotes.Contains(marker) && text.Length > marker.Length
            ? text[marker.Length..].TrimStart()
            : text;
        html.Append("        <p id=\"")
            .Append(HtmlAttribute(footnotes.IdFor(marker)))
            .Append("\" class=\"")
            .Append(SemanticClassNames(element))
            .Append("\"><a class=\"pdf-semantic-footnote-backref\" href=\"")
            .Append(HtmlAttribute(footnotes.FirstReferenceHref(marker)))
            .Append("\">")
            .Append(Html(marker))
            .Append("</a> ");
        html.Append(Html(body));
        html.AppendLine("</p>");
    }

    private static string SemanticTagName(PdfSemanticElement element)
    {
        return element.Kind switch
        {
            PdfSemanticElementKind.Heading => "h" + Math.Clamp(element.HeadingLevel, 1, 6).ToString(CultureInfo.InvariantCulture),
            PdfSemanticElementKind.Paragraph => "p",
            PdfSemanticElementKind.AuthorBlock => "address",
            PdfSemanticElementKind.Footnote => "p",
            PdfSemanticElementKind.Footer => "footer",
            PdfSemanticElementKind.Header => "header",
            _ => "div"
        };
    }

    private static string SemanticClassNames(
        PdfSemanticElement element,
        PdfLayoutPage? page = null,
        bool allowMeasuredWidth = true)
    {
        List<string> classes =
        [
            "pdf-semantic-element",
            SemanticClassName(element.Kind),
            FontClass(SemanticFontName(element)),
            FontSizeClass(SemanticFontSize(element)),
            ColorClass(SemanticColor(element))
        ];
        if (IsTitleElement(element))
        {
            classes.Add("pdf-semantic-title");
        }

        if (page != null)
        {
            string? alignmentClass = SourceAlignmentClass(page, element);
            if (alignmentClass != null)
            {
                classes.Add(alignmentClass);
            }

            if (IsFigureCaption(element))
            {
                classes.Add("pdf-semantic-caption");
            }

            if (IsSameRowLineGroup(element))
            {
                classes.Add("pdf-semantic-line-row");
            }
        }

        if (element.Kind == PdfSemanticElementKind.Paragraph && page != null)
        {
            if (IsJustifiedParagraph(element))
            {
                classes.Add("pdf-semantic-justified");
            }

            if (allowMeasuredWidth &&
                TryGetParagraphWidthPercent(page, element, out float widthPercent) &&
                ShouldUseMeasuredParagraphWidth(widthPercent))
            {
                classes.Add("pdf-semantic-measured-width");
            }
        }

        return string.Join(" ", classes);
    }

    private static string FlowSemanticStyle(
        PdfSemanticElement element,
        PdfLayoutPage? page,
        bool allowMeasuredWidth)
    {
        List<string> styles = [];
        if (IsSameRowLineGroup(element))
        {
            styles.Add("--pdf-semantic-line-count:" + SameRowLines(element).Length.ToString(CultureInfo.InvariantCulture));
        }

        if (allowMeasuredWidth &&
            page != null &&
            element.Kind == PdfSemanticElementKind.Paragraph &&
            TryGetParagraphWidthPercent(page, element, out float widthPercent) &&
            ShouldUseMeasuredParagraphWidth(widthPercent))
        {
            styles.Add("--pdf-semantic-width:" + CssPercent(widthPercent));
            string? alignSelf = ParagraphAlignSelf(page, element);
            if (alignSelf != null)
            {
                styles.Add("--pdf-semantic-align-self:" + alignSelf);
            }
        }

        return string.Join(";", styles);
    }

    private static bool TryGetParagraphWidthPercent(
        PdfLayoutPage page,
        PdfSemanticElement element,
        out float widthPercent)
    {
        widthPercent = 100f;
        if (element.Kind != PdfSemanticElementKind.Paragraph ||
            element.Lines.Count < 4 ||
            element.Text.Length < 240 ||
            !TryGetRepresentativeLineWidth(element, out float representativeWidth))
        {
            return false;
        }

        float flowWidth = SemanticFlowWidth(page);
        if (flowWidth <= 0.01f)
        {
            return false;
        }

        widthPercent = Math.Clamp((representativeWidth / flowWidth) * 100f, 25f, 100f);
        return true;
    }

    private static bool TryGetRepresentativeLineWidth(PdfSemanticElement element, out float width)
    {
        float[] widths = element.Lines
            .Where(static line => MathF.Abs(line.Direction) < 0.01f)
            .Where(static line => !string.IsNullOrWhiteSpace(line.Text))
            .Select(static line => line.Bounds.Width)
            .Where(static width => width > 0.01f)
            .OrderDescending()
            .ToArray();
        if (widths.Length == 0)
        {
            width = 0f;
            return false;
        }

        int count = widths.Length <= 2
            ? widths.Length
            : Math.Max(1, (int)MathF.Ceiling(widths.Length * 0.65f));
        width = widths.Take(count).Average();
        return true;
    }

    private static bool ShouldUseMeasuredParagraphWidth(float widthPercent)
    {
        return widthPercent <= 92f;
    }

    private static string? ParagraphAlignSelf(PdfLayoutPage page, PdfSemanticElement element)
    {
        float elementCenter = element.Bounds.X + (element.Bounds.Width / 2f);
        return MathF.Abs(elementCenter - (page.Width / 2f)) <= page.Width * 0.04f
            ? "center"
            : null;
    }

    private static string? SourceAlignmentClass(PdfLayoutPage page, PdfSemanticElement element)
    {
        if (!ShouldDetectSourceAlignment(element))
        {
            return null;
        }

        PdfSemanticLine[] lines = HorizontalTextLines(element);
        if (lines.Length == 0)
        {
            return null;
        }

        float weight = lines.Sum(static line => MathF.Max(1f, line.Bounds.Width));
        float center = lines.Sum(static line => (line.Bounds.X + line.Bounds.Width / 2f) * MathF.Max(1f, line.Bounds.Width)) / weight;
        float tolerance = MathF.Max(page.Width * 0.035f, SemanticFontSize(element) * 1.75f);
        if (MathF.Abs(center - page.Width / 2f) <= tolerance)
        {
            return "pdf-semantic-align-center";
        }

        float right = lines.Average(static line => line.Bounds.Right);
        if (page.Width - right <= page.Width * 0.08f)
        {
            return "pdf-semantic-align-right";
        }

        return null;
    }

    private static bool ShouldDetectSourceAlignment(PdfSemanticElement element)
    {
        return element.Kind is PdfSemanticElementKind.Heading or PdfSemanticElementKind.Header or PdfSemanticElementKind.Footer ||
            ShouldDetectFigureCaptionAlignment(element) ||
            IsSameRowLineGroup(element);
    }

    private static bool ShouldDetectFigureCaptionAlignment(PdfSemanticElement element)
    {
        return IsFigureCaption(element) &&
            (element.Text.Length <= 120 || HorizontalTextLines(element).All(static line => line.Text.Length <= 80));
    }

    private static bool IsFigureCaption(PdfSemanticElement element)
    {
        if (element.Kind != PdfSemanticElementKind.Paragraph && element.Kind != PdfSemanticElementKind.Heading)
        {
            return false;
        }

        string text = element.Text.TrimStart();
        if (!text.StartsWith("Figure ", StringComparison.Ordinal))
        {
            return false;
        }

        int colon = text.IndexOf(':', StringComparison.Ordinal);
        if (colon < 8 || colon > 18)
        {
            return false;
        }

        return text[7..colon].All(static character => char.IsDigit(character));
    }

    private static bool IsSameRowLineGroup(PdfSemanticElement element)
    {
        return SameRowLines(element).Length >= 2;
    }

    private static PdfSemanticLine[] SameRowLines(PdfSemanticElement element)
    {
        if (element.Kind != PdfSemanticElementKind.Paragraph || element.Text.Length > 120)
        {
            return [];
        }

        PdfSemanticLine[] lines = HorizontalTextLines(element);
        if (lines.Length != element.Lines.Count || lines.Any(static line => line.Text.Length > 64))
        {
            return [];
        }

        if (lines.Length >= 2 && TryGetSameRowLines(element, lines, out PdfSemanticLine[] rowLines))
        {
            return rowLines;
        }

        return lines.Length == 1 ? SameRowRunClusters(lines[0]) : [];
    }

    private static bool TryGetSameRowLines(
        PdfSemanticElement element,
        PdfSemanticLine[] lines,
        out PdfSemanticLine[] rowLines)
    {
        rowLines = [];
        float maxHeight = lines.Max(static line => line.Bounds.Height);
        float ySpan = lines.Max(static line => line.Bounds.Y) - lines.Min(static line => line.Bounds.Y);
        if (ySpan > MathF.Max(3f, maxHeight * 0.60f))
        {
            return false;
        }

        PdfSemanticLine[] ordered = lines
            .OrderBy(static line => line.Bounds.X)
            .ToArray();
        float minimumGap = MathF.Max(14f, SemanticFontSize(element) * 2f);
        for (int index = 1; index < ordered.Length; index++)
        {
            if (HorizontalGap(ordered[index - 1].Bounds, ordered[index].Bounds) < minimumGap)
            {
                return false;
            }
        }

        rowLines = ordered;
        return true;
    }

    private static PdfSemanticLine[] SameRowRunClusters(PdfSemanticLine line)
    {
        PdfTextRun[] runs = line.Runs
            .Where(static run => MathF.Abs(run.Direction) < 0.01f)
            .Where(static run => !string.IsNullOrWhiteSpace(run.Text))
            .OrderBy(static run => run.Bounds.X)
            .ToArray();
        if (runs.Length < 2)
        {
            return [];
        }

        List<List<PdfTextRun>> clusters = [];
        List<PdfTextRun> current = [];
        PdfTextRun? previous = null;
        float splitGap = MathF.Max(24f, line.DominantFontSize * 3f);
        foreach (PdfTextRun run in runs)
        {
            if (previous != null && HorizontalGap(previous.Bounds, run.Bounds) >= splitGap)
            {
                clusters.Add(current);
                current = [];
            }

            current.Add(run);
            previous = run;
        }

        if (current.Count > 0)
        {
            clusters.Add(current);
        }

        if (clusters.Count < 2)
        {
            return [];
        }

        PdfSemanticLine[] rowLines = clusters
            .Select(CreateSyntheticRowLine)
            .Where(static rowLine => rowLine.Text.Length is > 0 and <= 64)
            .ToArray();
        return rowLines.Length == clusters.Count ? rowLines : [];
    }

    private static PdfSemanticLine[] HorizontalTextLines(PdfSemanticElement element)
    {
        return element.Lines
            .Where(static line => MathF.Abs(line.Direction) < 0.01f)
            .Where(static line => !string.IsNullOrWhiteSpace(line.Text))
            .ToArray();
    }

    private static PdfSemanticLine CreateSyntheticRowLine(IReadOnlyList<PdfTextRun> runs)
    {
        string text = ReconstructText(runs.SelectMany(static run => run.Glyphs));
        PdfTextRun dominant = runs
            .GroupBy(static run => (
                FontName: NormalizeFontName(run.FontName),
                FontSize: MathF.Round(run.FontSize * 2f) / 2f,
                Direction: MathF.Round(run.Direction),
                Color: ColorClass(run.Color)))
            .OrderByDescending(static group => group.Sum(static run => Math.Max(1, run.Text.Length)))
            .Select(static group => group.First())
            .First();
        return new PdfSemanticLine(
            text,
            UnionRectangles(runs.Select(static run => run.Bounds)),
            NormalizeFontName(dominant.FontName),
            MathF.Round(dominant.FontSize * 2f) / 2f,
            MathF.Round(dominant.Direction),
            dominant.Color,
            runs);
    }

    private static string ReconstructText(IEnumerable<PdfTextGlyph> glyphSource)
    {
        PdfTextGlyph[] glyphs = glyphSource
            .Where(static glyph => !string.IsNullOrEmpty(glyph.Text))
            .OrderBy(static glyph => glyph.Bounds.X)
            .ThenBy(static glyph => glyph.Bounds.Y)
            .ToArray();
        if (glyphs.Length == 0)
        {
            return "";
        }

        StringBuilder text = new();
        PdfTextGlyph? previous = null;
        foreach (PdfTextGlyph glyph in glyphs)
        {
            if (previous != null && ShouldInsertWordBoundary(previous, glyph))
            {
                AppendSpaceIfNeeded(text);
            }

            if (string.IsNullOrWhiteSpace(glyph.Text))
            {
                AppendSpaceIfNeeded(text);
            }
            else
            {
                text.Append(glyph.Text);
            }

            previous = glyph;
        }

        return CollapseWhitespace(text.ToString());
    }

    private static bool ShouldInsertWordBoundary(PdfTextGlyph previous, PdfTextGlyph glyph)
    {
        if (glyph.Bounds.X <= previous.Bounds.X)
        {
            return false;
        }

        string previousText = previous.Text;
        string currentText = glyph.Text;
        if (previousText.Length == 0 || currentText.Length == 0)
        {
            return false;
        }

        if (NoSpaceBefore(currentText[0]) || NoSpaceAfter(previousText[^1]))
        {
            return false;
        }

        float gap = glyph.Bounds.X - previous.Bounds.Right;
        float threshold = MathF.Max(0.8f, MathF.Min(previous.FontSize, glyph.FontSize) * 0.16f);
        return gap > threshold;
    }

    private static void AppendSpaceIfNeeded(StringBuilder text)
    {
        if (text.Length > 0 && text[^1] != ' ')
        {
            text.Append(' ');
        }
    }

    private static string CollapseWhitespace(string text)
    {
        StringBuilder normalized = new(text.Length);
        bool pendingWhitespace = false;
        foreach (char character in text.Trim())
        {
            if (char.IsWhiteSpace(character))
            {
                pendingWhitespace = normalized.Length > 0;
                continue;
            }

            if (pendingWhitespace)
            {
                normalized.Append(' ');
                pendingWhitespace = false;
            }

            normalized.Append(character);
        }

        return normalized.ToString();
    }

    private static bool NoSpaceBefore(char character)
    {
        return character is ',' or '.' or ';' or ':' or '!' or '?' or ')' or ']' or '}' or '\'' or '’';
    }

    private static bool NoSpaceAfter(char character)
    {
        return character is '(' or '[' or '{' or '\'' or '‘';
    }

    private static string NormalizeFontName(string fontName)
    {
        int subsetSeparator = fontName.IndexOf('+', StringComparison.Ordinal);
        return subsetSeparator >= 0 && subsetSeparator + 1 < fontName.Length
            ? fontName[(subsetSeparator + 1)..]
            : fontName;
    }

    private static bool IsJustifiedParagraph(PdfSemanticElement element)
    {
        if (element.Kind != PdfSemanticElementKind.Paragraph)
        {
            return false;
        }

        PdfSemanticLine[] lines = element.Lines
            .Where(static line => MathF.Abs(line.Direction) < 0.01f)
            .Where(static line => !string.IsNullOrWhiteSpace(line.Text))
            .ToArray();
        if (lines.Length < 3)
        {
            return false;
        }

        float[] nonFinalWidths = lines
            .Take(lines.Length - 1)
            .Select(static line => line.Bounds.Width)
            .Where(static width => width > 0.01f)
            .ToArray();
        if (nonFinalWidths.Length < 2)
        {
            return false;
        }

        float max = nonFinalWidths.Max();
        float mean = nonFinalWidths.Average();
        float min = nonFinalWidths.Min();
        float variance = nonFinalWidths.Sum(width => MathF.Pow(width - mean, 2f)) / nonFinalWidths.Length;
        float standardDeviation = MathF.Sqrt(variance);
        float averageCharacters = (float)lines.Take(lines.Length - 1).Average(static line => line.Text.Length);
        return max >= 160f &&
            averageCharacters >= 36f &&
            mean >= max * 0.86f &&
            min >= max * 0.70f &&
            standardDeviation <= max * 0.13f;
    }

    private static float SemanticFlowWidth(PdfLayoutPage page)
    {
        return MathF.Min(396f, MathF.Max(0f, page.Width - 144f));
    }

    private static string SemanticLineClassNames(PdfSemanticLine line)
    {
        return string.Join(
            " ",
            "pdf-semantic-line",
            FontClass(line.DominantFontName),
            FontSizeClass(line.DominantFontSize),
            ColorClass(line.Color));
    }

    private static string SemanticClassName(PdfSemanticElementKind kind)
    {
        return kind switch
        {
            PdfSemanticElementKind.Heading => "pdf-semantic-heading",
            PdfSemanticElementKind.Paragraph => "pdf-semantic-paragraph",
            PdfSemanticElementKind.AuthorBlock => "pdf-semantic-author-block",
            PdfSemanticElementKind.Footnote => "pdf-semantic-footnote",
            PdfSemanticElementKind.Footer => "pdf-semantic-footer",
            PdfSemanticElementKind.Header => "pdf-semantic-header",
            _ => "pdf-semantic-other"
        };
    }

    private static bool IsPositionedSemanticElement(PdfSemanticElement element)
    {
        return element.Lines.Any(static line => MathF.Abs(line.Direction) > 0.01f);
    }

    private static bool IsTitleElement(PdfSemanticElement element)
    {
        return element.Kind == PdfSemanticElementKind.Heading &&
            element.HeadingLevel == 1 &&
            SemanticFontSize(element) >= 16f &&
            !char.IsDigit(element.Text.TrimStart().FirstOrDefault());
    }

    private static bool IsSemanticFlowRulePath(
        PdfLayoutPage page,
        PdfSemanticPage semanticPage,
        PdfLayoutPath path)
    {
        return IsDecorativeTitleRulePath(page, semanticPage, path) ||
            IsDecorativeFootnoteRulePath(page, semanticPage, path);
    }

    private static bool IsDecorativeTitleRulePath(
        PdfLayoutPage page,
        PdfSemanticPage semanticPage,
        PdfLayoutPath path)
    {
        PdfSemanticElement? title = semanticPage.Elements.FirstOrDefault(IsTitleElement);
        if (title == null)
        {
            return false;
        }

        bool horizontalRuleShape = path.Bounds.Width >= MathF.Max(title.Bounds.Width * 0.95f, page.Width * 0.45f) &&
            path.Bounds.Height <= 6f &&
            path.Bounds.X <= title.Bounds.X + 6f &&
            path.Bounds.Right >= title.Bounds.Right - 6f;
        if (!horizontalRuleShape)
        {
            return false;
        }

        bool closeAboveTitle = path.Bounds.Bottom <= title.Bounds.Y + 3f &&
            title.Bounds.Y - path.Bounds.Bottom <= 32f;
        bool closeBelowTitle = path.Bounds.Y >= title.Bounds.Bottom - 3f &&
            path.Bounds.Y - title.Bounds.Bottom <= 32f;
        return closeAboveTitle || closeBelowTitle;
    }

    private static bool IsDecorativeFootnoteRulePath(
        PdfLayoutPage page,
        PdfSemanticPage semanticPage,
        PdfLayoutPath path)
    {
        PdfSemanticElement[] footnotes = semanticPage.Elements
            .Where(static element => element.Kind == PdfSemanticElementKind.Footnote)
            .ToArray();
        if (footnotes.Length == 0)
        {
            return false;
        }

        float footnoteTop = footnotes.Min(static footnote => footnote.Bounds.Y);
        float footnoteLeft = footnotes.Min(static footnote => footnote.Bounds.X);
        bool horizontalRuleShape = path.Bounds.Width >= page.Width * 0.10f &&
            path.Bounds.Width <= page.Width * 0.45f &&
            path.Bounds.Height <= 4f &&
            path.Bounds.X <= footnoteLeft + 16f;
        if (!horizontalRuleShape)
        {
            return false;
        }

        return path.Bounds.Y <= footnoteTop + 4f &&
            footnoteTop - path.Bounds.Y <= 28f;
    }

    private static PdfLayoutPath? DecorativeFootnoteRulePath(PdfLayoutPage page, PdfSemanticPage semanticPage)
    {
        PdfSemanticElement[] footnotes = semanticPage.Elements
            .Where(static element => element.Kind == PdfSemanticElementKind.Footnote)
            .ToArray();
        if (footnotes.Length == 0)
        {
            return null;
        }

        float footnoteTop = footnotes.Min(static footnote => footnote.Bounds.Y);
        return page.Paths
            .Where(path => IsDecorativeFootnoteRulePath(page, semanticPage, path))
            .OrderBy(path => MathF.Abs(path.Bounds.Y - footnoteTop))
            .ThenByDescending(static path => path.Bounds.Width)
            .FirstOrDefault();
    }

    private static string PositionStyle(PdfLayoutPage page, PdfSemanticElement element, float scale)
    {
        float direction = SemanticDirection(element);
        float left = element.Bounds.X;
        float top = element.Bounds.Y;
        if (MathF.Abs(direction - 90f) < 0.01f)
        {
            left = element.Bounds.Y;
            top = (page.Height + element.Bounds.Width) / 2f;
        }
        else if (MathF.Abs(direction - 270f) < 0.01f)
        {
            left = page.Width - element.Bounds.Y;
            top = (page.Height - element.Bounds.Width) / 2f;
        }

        return "left:" + CssPoints(left * scale) +
            ";top:" + CssPoints(top * scale) +
            ";width:" + CssPoints(element.Bounds.Width * scale);
    }

    private static float SemanticFontSize(PdfSemanticElement element)
    {
        return element.Lines.Count == 0
            ? 10f
            : element.Lines
                .GroupBy(static line => MathF.Round(line.DominantFontSize * 2f) / 2f)
                .OrderByDescending(static group => group.Sum(static line => Math.Max(1, line.Text.Length)))
                .ThenByDescending(static group => group.Key)
                .Select(static group => group.Key)
                .First();
    }

    private static string SemanticFontName(PdfSemanticElement element)
    {
        return element.Lines
            .GroupBy(static line => line.DominantFontName, StringComparer.Ordinal)
            .OrderByDescending(static group => group.Sum(static line => Math.Max(1, line.Text.Length)))
            .Select(static group => group.Key)
            .FirstOrDefault() ?? "";
    }

    private static float SemanticDirection(PdfSemanticElement element)
    {
        return element.Lines
            .GroupBy(static line => MathF.Round(line.Direction))
            .OrderByDescending(static group => group.Sum(static line => Math.Max(1, line.Text.Length)))
            .Select(static group => group.Key)
            .FirstOrDefault();
    }

    private static PdfLayoutColor SemanticColor(PdfSemanticElement element)
    {
        return element.Lines
            .GroupBy(static line => ColorClass(line.Color), StringComparer.Ordinal)
            .OrderByDescending(static group => group.Sum(static line => Math.Max(1, line.Text.Length)))
            .Select(static group => group.First().Color)
            .FirstOrDefault();
    }

    private static void WriteTextWithFootnoteReferences(
        StringBuilder html,
        string text,
        FootnoteContext footnotes)
    {
        for (int index = 0; index < text.Length; index++)
        {
            string marker = text[index].ToString();
            if (footnotes.Contains(marker) && IsFootnoteReferenceBoundary(text, index))
            {
                string referenceId = footnotes.NextReferenceId(marker);
                html.Append("<sup id=\"")
                    .Append(HtmlAttribute(referenceId))
                    .Append("\"><a class=\"pdf-semantic-footnote-ref\" href=\"#")
                    .Append(HtmlAttribute(footnotes.IdFor(marker)))
                    .Append("\">")
                    .Append(Html(marker))
                    .Append("</a></sup>");
                continue;
            }

            html.Append(Html(text[index].ToString()));
        }
    }

    private static bool IsFootnoteReferenceBoundary(string text, int index)
    {
        bool before = index == 0 || char.IsWhiteSpace(text[index - 1]) || text[index - 1] == '(';
        bool after = index == text.Length - 1 || char.IsWhiteSpace(text[index + 1]) || text[index + 1] is ',' or ';' or '.' or ')';
        return before && after;
    }

    private static string FontClass(string fontName)
    {
        return "pdf-font-" + CssClassToken(string.IsNullOrWhiteSpace(fontName) ? "default" : fontName);
    }

    private static string FontSizeClass(float fontSize)
    {
        return "pdf-font-size-" + CssClassToken(fontSize.ToString("0.#", CultureInfo.InvariantCulture).Replace('.', '-'));
    }

    private static string ColorClass(PdfLayoutColor color)
    {
        return "pdf-color-" +
            ByteHex(color.Red).ToLowerInvariant() +
            ByteHex(color.Green).ToLowerInvariant() +
            ByteHex(color.Blue).ToLowerInvariant() +
            "-" +
            ByteHex(color.Alpha).ToLowerInvariant();
    }

    private static string CssClassToken(string value)
    {
        StringBuilder builder = new(value.Length);
        foreach (char character in value)
        {
            if (character is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or (>= '0' and <= '9'))
            {
                builder.Append(char.ToLowerInvariant(character));
            }
            else if (builder.Length > 0 && builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        string token = builder.ToString().Trim('-');
        return token.Length == 0 ? "value" : token;
    }

    private sealed class FootnoteContext
    {
        private readonly int _pageNumber;
        private readonly Dictionary<string, string> _footnoteIds;
        private readonly Dictionary<string, int> _referenceCounts = new(StringComparer.Ordinal);

        private FootnoteContext(int pageNumber, Dictionary<string, string> footnoteIds)
        {
            _pageNumber = pageNumber;
            _footnoteIds = footnoteIds;
        }

        public static FootnoteContext Create(int pageNumber, IReadOnlyList<PdfSemanticElement> elements)
        {
            Dictionary<string, string> ids = new(StringComparer.Ordinal);
            foreach (PdfSemanticElement footnote in elements.Where(static element => element.Kind == PdfSemanticElementKind.Footnote))
            {
                string text = footnote.Text.Trim();
                if (text.Length == 0)
                {
                    continue;
                }

                string marker = text[..1];
                ids.TryAdd(marker, $"page-{pageNumber.ToString(CultureInfo.InvariantCulture)}-fn-{FootnoteMarkerToken(marker)}");
            }

            return new FootnoteContext(pageNumber, ids);
        }

        public bool Contains(string marker)
        {
            return _footnoteIds.ContainsKey(marker);
        }

        public string IdFor(string marker)
        {
            return _footnoteIds.TryGetValue(marker, out string? id)
                ? id
                : $"page-{_pageNumber.ToString(CultureInfo.InvariantCulture)}-fn";
        }

        public string NextReferenceId(string marker)
        {
            _referenceCounts.TryGetValue(marker, out int count);
            count++;
            _referenceCounts[marker] = count;
            return $"{IdFor(marker)}-ref-{count.ToString(CultureInfo.InvariantCulture)}";
        }

        public string FirstReferenceHref(string marker)
        {
            return "#" + IdFor(marker) + "-ref-1";
        }

        private static string FootnoteMarkerToken(string marker)
        {
            return marker switch
            {
                "*" or "∗" => "asterisk",
                "†" => "dagger",
                "‡" => "double-dagger",
                _ => CssClassToken(marker)
            };
        }
    }

    private static void WriteLink(StringBuilder html, PdfLayoutLink link, float scale)
    {
        html.Append("    <a class=\"pdf-link-overlay\" href=\"")
            .Append(HtmlAttribute(LinkHref(link)))
            .Append("\" data-link-kind=\"")
            .Append(HtmlAttribute(link.Kind.ToString().ToLowerInvariant()));

        if (!string.IsNullOrEmpty(link.Uri))
        {
            html.Append("\" data-uri=\"")
                .Append(HtmlAttribute(link.Uri));
        }

        if (!string.IsNullOrEmpty(link.Destination))
        {
            html.Append("\" data-destination=\"")
                .Append(HtmlAttribute(link.Destination));
        }

        html.Append("\" aria-label=\"")
            .Append(HtmlAttribute(LinkLabel(link)))
            .Append("\" style=\"position:absolute;left:")
            .Append(CssPoints(link.Bounds.X * scale))
            .Append(";top:")
            .Append(CssPoints(link.Bounds.Y * scale))
            .Append(";width:")
            .Append(CssPoints(link.Bounds.Width * scale))
            .Append(";height:")
            .Append(CssPoints(link.Bounds.Height * scale))
            .AppendLine("\"></a>");
    }

    private static string LinkHref(PdfLayoutLink link)
    {
        if (!string.IsNullOrEmpty(link.Uri))
        {
            return link.Uri;
        }

        if (link.DestinationPageNumber.HasValue)
        {
            return "#page-" + link.DestinationPageNumber.Value.ToString(CultureInfo.InvariantCulture);
        }

        return string.IsNullOrEmpty(link.Destination)
            ? "#"
            : "#" + Uri.EscapeDataString(link.Destination);
    }

    private static string LinkLabel(PdfLayoutLink link)
    {
        if (!string.IsNullOrEmpty(link.Uri))
        {
            return link.Uri;
        }

        if (!string.IsNullOrEmpty(link.Destination))
        {
            return link.Destination;
        }

        return "PDF link";
    }

    private static string NormalizeCssPath(string cssPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cssPath);
        return cssPath.Replace('\\', '/').TrimStart('/');
    }

    private static string CssPoints(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture) + "pt";
    }

    private static string CssPercent(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture) + "%";
    }

    private static string SvgNumber(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string SvgPathData(IReadOnlyList<PdfLayoutPathCommand> commands)
    {
        StringBuilder builder = new();
        foreach (PdfLayoutPathCommand command in commands)
        {
            if (builder.Length > 0)
            {
                builder.Append(' ');
            }

            switch (command.Kind)
            {
                case PdfLayoutPathCommandKind.MoveTo:
                    builder.Append("M ")
                        .Append(SvgNumber(command.X1))
                        .Append(' ')
                        .Append(SvgNumber(command.Y1));
                    break;
                case PdfLayoutPathCommandKind.LineTo:
                    builder.Append("L ")
                        .Append(SvgNumber(command.X1))
                        .Append(' ')
                        .Append(SvgNumber(command.Y1));
                    break;
                case PdfLayoutPathCommandKind.CurveTo:
                    builder.Append("C ")
                        .Append(SvgNumber(command.X1))
                        .Append(' ')
                        .Append(SvgNumber(command.Y1))
                        .Append(' ')
                        .Append(SvgNumber(command.X2))
                        .Append(' ')
                        .Append(SvgNumber(command.Y2))
                        .Append(' ')
                        .Append(SvgNumber(command.X3))
                        .Append(' ')
                        .Append(SvgNumber(command.Y3));
                    break;
                case PdfLayoutPathCommandKind.ClosePath:
                    builder.Append('Z');
                    break;
            }
        }

        return builder.ToString();
    }

    private static string ColorHex(PdfLayoutColor color)
    {
        return "#"
            + ByteHex(color.Red)
            + ByteHex(color.Green)
            + ByteHex(color.Blue);
    }

    private static string ByteHex(float value)
    {
        return ((int)MathF.Round(Math.Clamp(value, 0f, 1f) * 255f)).ToString("X2", CultureInfo.InvariantCulture);
    }

    private static string FillRule(int? fillRule)
    {
        return fillRule == 0 ? "evenodd" : "nonzero";
    }

    private static string LineCap(int lineCap)
    {
        return lineCap switch
        {
            1 => "round",
            2 => "square",
            _ => "butt"
        };
    }

    private static string LineJoin(int lineJoin)
    {
        return lineJoin switch
        {
            1 => "round",
            2 => "bevel",
            _ => "miter"
        };
    }

    private static string CssFontFamily(string fontName)
    {
        if (string.IsNullOrWhiteSpace(fontName))
        {
            return "sans-serif";
        }

        string fallback = FontFallback(fontName);
        string escaped = fontName.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("'", "\\'", StringComparison.Ordinal);
        return "'" + escaped + "', " + fallback;
    }

    private static string FontFallback(string fontName)
    {
        if (fontName.Contains("SFTT", StringComparison.OrdinalIgnoreCase) ||
            fontName.Contains("Mono", StringComparison.OrdinalIgnoreCase) ||
            fontName.Contains("Courier", StringComparison.OrdinalIgnoreCase))
        {
            return "monospace";
        }

        if (fontName.Contains("NimbusRom", StringComparison.OrdinalIgnoreCase) ||
            fontName.Contains("Times", StringComparison.OrdinalIgnoreCase) ||
            fontName.Contains("CMR", StringComparison.OrdinalIgnoreCase) ||
            fontName.Contains("CMBX", StringComparison.OrdinalIgnoreCase))
        {
            return "serif";
        }

        return "sans-serif";
    }

    private static string Html(string value)
    {
        return WebUtility.HtmlEncode(value);
    }

    private static string HtmlAttribute(string value)
    {
        return WebUtility.HtmlEncode(value).Replace("\"", "&quot;", StringComparison.Ordinal);
    }
}
