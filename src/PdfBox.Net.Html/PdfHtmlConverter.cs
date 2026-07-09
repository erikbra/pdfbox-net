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
          --pdf-page-background: #fff;
          --pdf-page-corner-shadow: 22pt;
          --pdf-page-edge-shadow: rgba(17, 24, 39, 0.16);
          --pdf-page-shadow-mask: 6pt;
          --pdf-page-surround: #f3f4f6;
          --pdf-page-width: min(612pt, calc(100vw - 48pt));
          background: var(--pdf-page-surround);
          color: #111827;
          font-family: Arial, Helvetica, sans-serif;
          margin: 0;
          padding: 24pt;
        }

        .pdf-page {
          background: var(--pdf-page-background);
          box-shadow: 0 1pt 4pt var(--pdf-page-edge-shadow);
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
          background: var(--pdf-page-background);
          box-shadow: 0 1pt 4pt var(--pdf-page-edge-shadow);
          box-sizing: border-box;
          margin: 0 auto 24pt;
          padding: 54pt 72pt 48pt;
          position: relative;
          width: var(--pdf-page-width);
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
          display: block;
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

        .pdf-semantic-inline-page-break {
          margin-left: 0;
          margin-right: 0;
        }

        .pdf-semantic-continuous-flow > .pdf-semantic-page-break:not(.pdf-semantic-page-start),
        .pdf-semantic-continuous-flow .pdf-semantic-inline-page-break {
          background:
            radial-gradient(ellipse at right top, var(--pdf-page-edge-shadow), rgba(17, 24, 39, 0) 70%) left top / calc(var(--pdf-page-shadow-mask) + var(--pdf-page-corner-shadow)) 4pt no-repeat,
            radial-gradient(ellipse at left top, var(--pdf-page-edge-shadow), rgba(17, 24, 39, 0) 70%) right top / calc(var(--pdf-page-shadow-mask) + var(--pdf-page-corner-shadow)) 4pt no-repeat,
            radial-gradient(ellipse at right bottom, var(--pdf-page-edge-shadow), rgba(17, 24, 39, 0) 70%) left bottom / calc(var(--pdf-page-shadow-mask) + var(--pdf-page-corner-shadow)) 4pt no-repeat,
            radial-gradient(ellipse at left bottom, var(--pdf-page-edge-shadow), rgba(17, 24, 39, 0) 70%) right bottom / calc(var(--pdf-page-shadow-mask) + var(--pdf-page-corner-shadow)) 4pt no-repeat,
            linear-gradient(to bottom, var(--pdf-page-edge-shadow), rgba(17, 24, 39, 0) 4pt) center top / calc(100% - var(--pdf-page-shadow-mask) - var(--pdf-page-shadow-mask) - var(--pdf-page-corner-shadow) - var(--pdf-page-corner-shadow)) 4pt no-repeat,
            linear-gradient(to top, var(--pdf-page-edge-shadow), rgba(17, 24, 39, 0) 4pt) center bottom / calc(100% - var(--pdf-page-shadow-mask) - var(--pdf-page-shadow-mask) - var(--pdf-page-corner-shadow) - var(--pdf-page-corner-shadow)) 4pt no-repeat,
            var(--pdf-page-surround);
          border: 0;
          box-shadow: none;
          box-sizing: border-box;
          height: 28pt;
          margin-bottom: 36pt;
          margin-left: calc((100% - var(--pdf-page-width)) / 2 - var(--pdf-page-shadow-mask));
          margin-top: 36pt;
          overflow: hidden;
          position: relative;
          width: calc(var(--pdf-page-width) + var(--pdf-page-shadow-mask) + var(--pdf-page-shadow-mask));
        }

        .pdf-semantic-continuous-flow .pdf-semantic-page-break::after {
          content: "";
          display: none;
        }

        .pdf-semantic-page-spanning {
          text-align-last: left;
        }

        .pdf-semantic-page-continuation {
          display: inline;
        }

        .pdf-semantic-inline-flow-element {
          display: block;
          margin: 8pt 0;
        }

        .pdf-semantic-inline-run {
          white-space: normal;
        }

        .pdf-semantic-math {
          font-family: "Times New Roman", Times, serif;
        }

        .pdf-semantic-italic {
          font-style: italic;
        }

        .pdf-semantic-inline-fraction {
          display: inline-block;
          font-size: 0.72em;
          height: 1em;
          line-height: 1;
          margin: 0 0.06em;
          overflow: visible;
          text-align: center;
          vertical-align: -0.22em;
        }

        .pdf-semantic-inline-fraction-numerator {
          border-bottom: 0.04em solid currentColor;
          display: block;
          line-height: 0.5;
          min-width: 100%;
          padding: 0 0.1em 0.01em;
          text-align: center;
        }

        .pdf-semantic-inline-fraction-denominator {
          display: block;
          line-height: 0.5;
          padding: 0.01em 0.1em 0;
          text-align: center;
          white-space: nowrap;
        }

        .pdf-semantic-inline-summation {
          align-items: center;
          display: inline-flex;
          line-height: 1;
          margin: 0 0.06em;
          vertical-align: -0.25em;
        }

        .pdf-semantic-inline-summation-limits {
          display: inline-flex;
          flex-direction: column;
          font-size: 0.62em;
          line-height: 0.9;
          margin-left: 0.03em;
          text-align: center;
        }

        .pdf-semantic-inline-summation-limits sub {
          font-size: 0.75em;
          line-height: 0;
        }

        .pdf-semantic-formula {
          display: block;
          height: var(--pdf-semantic-formula-height, auto);
          line-height: 1;
          margin: 10pt auto;
          max-width: 100%;
          overflow: visible;
          position: relative;
          text-align: left;
          text-align-last: left;
          width: min(100%, var(--pdf-semantic-formula-width, 100%));
        }

        .pdf-semantic-formula-run {
          line-height: 1;
          position: absolute;
          white-space: pre;
        }

        .pdf-semantic-formula-radical {
          transform: translateY(5pt);
        }

        .pdf-semantic-formula-vector-layer {
          height: var(--pdf-semantic-formula-height, 100%);
          inset: 0;
          overflow: visible;
          pointer-events: none;
          position: absolute;
          width: var(--pdf-semantic-formula-width, 100%);
        }

        .pdf-semantic-figure {
          box-sizing: border-box;
          display: block;
          margin: 12pt auto;
          max-width: 100%;
          width: min(100%, var(--pdf-semantic-figure-width, 100%));
        }

        .pdf-semantic-inline-figure {
          margin-bottom: 8pt;
          margin-top: 8pt;
        }

        .pdf-semantic-figure-svg {
          display: block;
          height: auto;
          max-width: 100%;
          overflow: hidden;
          width: 100%;
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
          text-align-last: center;
        }

        .pdf-semantic-align-right {
          text-align: right;
          text-align-last: right;
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

        .pdf-semantic-table {
          border-collapse: collapse;
          line-height: 1.15;
          margin: 10pt auto 14pt;
          max-width: 100%;
          table-layout: auto;
          width: 100%;
        }

        .pdf-semantic-table th,
        .pdf-semantic-table td {
          border-bottom: 0.35pt solid #d1d5db;
          padding: 2pt 4pt;
          text-align: left;
          vertical-align: top;
        }

        .pdf-semantic-table thead th {
          border-bottom-color: #6b7280;
          font-weight: 600;
        }

        .pdf-semantic-table-cell-border-top {
          border-top: 0.45pt solid #6b7280;
        }

        .pdf-semantic-table-cell-border-right {
          border-right: 0.45pt solid #6b7280;
        }

        .pdf-semantic-table-cell-border-bottom {
          border-bottom-color: #6b7280;
        }

        .pdf-semantic-table-cell-border-left {
          border-left: 0.45pt solid #6b7280;
        }

        .pdf-semantic-table td:not(:first-child),
        .pdf-semantic-table th:not(:first-child) {
          text-align: right;
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

        .pdf-semantic-inline-footnotes {
          display: block;
          margin: 10pt 0 8pt;
        }

        .pdf-semantic-inline-footnotes .pdf-semantic-footnote {
          display: block;
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
            WriteSemanticContinuousDocument(html, layout, semantic, imageAssets, options.Scale);
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
            .SelectMany(static line => line.Runs
                .Select(static run => NormalizeFontName(run.FontName))
                .Append(line.DominantFontName))
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
            .SelectMany(static line => line.Runs
                .Select(static run => MathF.Round(run.FontSize * 2f) / 2f)
                .Append(MathF.Round(line.DominantFontSize * 2f) / 2f))
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
            .SelectMany(static line => line.Runs
                .Select(static run => run.Color)
                .Append(line.Color))
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
            imageAssets: null,
            figureRendering: SemanticFigureRendering.Space,
            omitSimplePageNumberFooters: false,
            skippedElements: null,
            skippedFigureRegions: null,
            paragraphMerge: null);
        html.AppendLine("    </article>");
    }

    private static void WriteSemanticContinuousDocument(
        StringBuilder html,
        PdfLayoutDocument layout,
        PdfSemanticDocument semantic,
        IReadOnlyDictionary<string, PdfLayoutImageAsset> imageAssets,
        float scale)
    {
        ContinuousPageContext[] pages = layout.Pages
            .Select((page, index) => CreateContinuousPageContext(page, semantic.Pages[index]))
            .ToArray();
        Dictionary<int, ContinuousParagraphMerge> paragraphMerges = [];
        HashSet<PdfSemanticElement> skippedElements = [];
        Dictionary<int, HashSet<PdfLayoutRectangle>> skippedFigureRegionsByPage = [];
        HashSet<int> inlinePageBreaks = [];
        for (int index = 0; index + 1 < pages.Length; index++)
        {
            ContinuousParagraphMerge? merge = TryCreateContinuousParagraphMerge(pages[index], pages[index + 1]);
            if (merge == null)
            {
                continue;
            }

            paragraphMerges[index] = merge;
            inlinePageBreaks.Add(merge.Next.Page.PageNumber);
            skippedElements.Add(merge.ContinuationElement);
            if (merge.CurrentPageNumberFooter != null)
            {
                skippedElements.Add(merge.CurrentPageNumberFooter);
            }

            foreach (PdfSemanticElement footnote in merge.CurrentTrailingFootnotes)
            {
                skippedElements.Add(footnote);
            }

            foreach (PdfSemanticElement element in merge.LeadingElements)
            {
                skippedElements.Add(element);
            }

            if (!skippedFigureRegionsByPage.TryGetValue(
                    merge.Next.Page.PageNumber,
                    out HashSet<PdfLayoutRectangle>? skippedFigureRegions))
            {
                skippedFigureRegions = [];
                skippedFigureRegionsByPage[merge.Next.Page.PageNumber] = skippedFigureRegions;
            }

            foreach (PdfLayoutRectangle region in merge.LeadingFigureRegions)
            {
                skippedFigureRegions.Add(region);
            }
        }

        html.AppendLine("  <main class=\"pdf-semantic-document-flow\">");
        html.AppendLine("    <article class=\"pdf-semantic-flow pdf-semantic-continuous-flow\">");

        for (int index = 0; index < pages.Length; index++)
        {
            ContinuousPageContext context = pages[index];
            if (!inlinePageBreaks.Contains(context.Page.PageNumber))
            {
                WriteSemanticPageBreak(html, context.Page.PageNumber, isFirstPage: index == 0);
            }

            WriteContinuousPageArtifacts(html, context.Page, context.PositionedElements, context.Footnotes, scale);
            skippedFigureRegionsByPage.TryGetValue(context.Page.PageNumber, out HashSet<PdfLayoutRectangle>? skippedFigureRegions);
            WriteSemanticFlowElements(
                html,
                context.Page,
                context.SemanticPage,
                context.FlowElements,
                context.Footnotes,
                scale,
                imageAssets,
                figureRendering: SemanticFigureRendering.Content,
                omitSimplePageNumberFooters: false,
                skippedElements,
                skippedFigureRegions,
                paragraphMerges.GetValueOrDefault(index));
        }

        html.AppendLine("    </article>");
        html.AppendLine("  </main>");
    }

    private static ContinuousPageContext CreateContinuousPageContext(PdfLayoutPage page, PdfSemanticPage semanticPage)
    {
        PdfSemanticElement[] positioned = semanticPage.Elements
            .Where(IsPositionedSemanticElement)
            .ToArray();
        HashSet<PdfSemanticElement> positionedSet = positioned.ToHashSet();
        PdfSemanticElement[] flowElements = semanticPage.Elements
            .Where(element => !positionedSet.Contains(element))
            .ToArray();
        return new ContinuousPageContext(
            page,
            semanticPage,
            FootnoteContext.Create(page.PageNumber, semanticPage.Elements),
            positioned,
            flowElements,
            SemanticFigureRegions(page, semanticPage).ToArray());
    }

    private static ContinuousParagraphMerge? TryCreateContinuousParagraphMerge(
        ContinuousPageContext current,
        ContinuousPageContext next)
    {
        if (!TryFindTrailingBodyParagraph(current, out PdfSemanticElement? startElement) ||
            startElement == null ||
            !TryFindLeadingBodyParagraph(next, out PdfSemanticElement? continuationElement, out PdfSemanticElement[] leadingElements) ||
            continuationElement == null ||
            !ShouldMergeParagraphAcrossPage(current, startElement, next, continuationElement))
        {
            return null;
        }

        PdfLayoutRectangle[] leadingFigureRegions = next.FigureRegions
            .Where(region => region.Y < continuationElement.Bounds.Y - 2f)
            .ToArray();
        PdfSemanticElement? currentPageNumberFooter = FindSimplePageNumberFooterAfter(current, startElement);
        PdfSemanticElement[] currentTrailingFootnotes = FindTrailingFootnotesAfter(current, startElement, currentPageNumberFooter);
        return new ContinuousParagraphMerge(
            current,
            next,
            startElement,
            continuationElement,
            currentPageNumberFooter,
            currentTrailingFootnotes,
            leadingElements,
            leadingFigureRegions);
    }

    private static PdfSemanticElement[] FindTrailingFootnotesAfter(
        ContinuousPageContext context,
        PdfSemanticElement startElement,
        PdfSemanticElement? currentPageNumberFooter)
    {
        bool foundStart = false;
        List<PdfSemanticElement> footnotes = [];
        foreach (PdfSemanticElement element in context.FlowElements)
        {
            if (ReferenceEquals(element, startElement))
            {
                foundStart = true;
                continue;
            }

            if (!foundStart)
            {
                continue;
            }

            if (currentPageNumberFooter != null && ReferenceEquals(element, currentPageNumberFooter))
            {
                break;
            }

            if (IsSimplePageNumberFooter(element, context.Page) || element.Kind == PdfSemanticElementKind.Footer)
            {
                break;
            }

            if (element.Kind == PdfSemanticElementKind.Footnote)
            {
                footnotes.Add(element);
                continue;
            }

            if (element.Kind == PdfSemanticElementKind.Header)
            {
                continue;
            }

            break;
        }

        return footnotes.ToArray();
    }

    private static PdfSemanticElement? FindSimplePageNumberFooterAfter(
        ContinuousPageContext context,
        PdfSemanticElement startElement)
    {
        bool foundStart = false;
        foreach (PdfSemanticElement element in context.FlowElements)
        {
            if (ReferenceEquals(element, startElement))
            {
                foundStart = true;
                continue;
            }

            if (!foundStart)
            {
                continue;
            }

            if (IsSimplePageNumberFooter(element, context.Page))
            {
                return element;
            }
        }

        return null;
    }

    private static bool TryFindTrailingBodyParagraph(
        ContinuousPageContext context,
        out PdfSemanticElement? paragraph)
    {
        for (int index = context.FlowElements.Count - 1; index >= 0; index--)
        {
            PdfSemanticElement element = context.FlowElements[index];
            if (IsSimplePageNumberFooter(element, context.Page) || element.Kind == PdfSemanticElementKind.Footer)
            {
                continue;
            }

            if (IsContinuousBodyParagraph(element))
            {
                paragraph = element;
                return true;
            }

            if (element.Kind is PdfSemanticElementKind.Footnote or PdfSemanticElementKind.Header)
            {
                continue;
            }

            break;
        }

        paragraph = null;
        return false;
    }

    private static bool TryFindLeadingBodyParagraph(
        ContinuousPageContext context,
        out PdfSemanticElement? paragraph,
        out PdfSemanticElement[] leadingElements)
    {
        List<PdfSemanticElement> interruptions = [];
        foreach (PdfSemanticElement element in context.FlowElements)
        {
            if (IsSimplePageNumberFooter(element, context.Page))
            {
                continue;
            }

            if (IsLeadingContinuousInterruption(element))
            {
                interruptions.Add(element);
                continue;
            }

            if (IsContinuousBodyParagraph(element))
            {
                paragraph = element;
                leadingElements = interruptions.ToArray();
                return true;
            }

            paragraph = null;
            leadingElements = [];
            return false;
        }

        paragraph = null;
        leadingElements = [];
        return false;
    }

    private static bool IsContinuousBodyParagraph(PdfSemanticElement element)
    {
        return element.Kind == PdfSemanticElementKind.Paragraph &&
            !IsFigureCaption(element) &&
            !IsSameRowLineGroup(element) &&
            !string.IsNullOrWhiteSpace(element.Text);
    }

    private static bool IsLeadingContinuousInterruption(PdfSemanticElement element)
    {
        return IsFigureCaption(element) || IsSameRowLineGroup(element);
    }

    private static bool ShouldMergeParagraphAcrossPage(
        ContinuousPageContext current,
        PdfSemanticElement startElement,
        ContinuousPageContext next,
        PdfSemanticElement continuationElement)
    {
        string startText = startElement.Text.Trim();
        string continuationText = continuationElement.Text.Trim();
        if (startText.Length < 24 ||
            continuationText.Length < 16 ||
            EndsLikeCompleteParagraph(startText) ||
            !StartsLikeParagraphContinuation(continuationText))
        {
            return false;
        }

        if (startElement.Bounds.Bottom < current.Page.Height * 0.48f ||
            continuationElement.Bounds.Y > next.Page.Height * 0.82f)
        {
            return false;
        }

        return HasCompatibleParagraphStyle(startElement, continuationElement);
    }

    private static bool HasCompatibleParagraphStyle(PdfSemanticElement first, PdfSemanticElement second)
    {
        if (!string.Equals(
                NormalizeFontName(SemanticFontName(first)),
                NormalizeFontName(SemanticFontName(second)),
                StringComparison.Ordinal))
        {
            return false;
        }

        if (MathF.Abs(SemanticFontSize(first) - SemanticFontSize(second)) > 1.25f)
        {
            return false;
        }

        return string.Equals(
            ColorClass(SemanticColor(first)),
            ColorClass(SemanticColor(second)),
            StringComparison.Ordinal);
    }

    private static bool EndsLikeCompleteParagraph(string text)
    {
        string trimmed = text.TrimEnd();
        while (trimmed.Length > 0 && trimmed[^1] is '"' or '\'' or ')' or ']' or '}')
        {
            trimmed = trimmed[..^1].TrimEnd();
        }

        return trimmed.EndsWith(".", StringComparison.Ordinal) ||
            trimmed.EndsWith("!", StringComparison.Ordinal) ||
            trimmed.EndsWith("?", StringComparison.Ordinal) ||
            trimmed.EndsWith(":", StringComparison.Ordinal);
    }

    private static bool StartsLikeParagraphContinuation(string text)
    {
        char first = text.TrimStart().FirstOrDefault();
        return char.IsLower(first) ||
            first is ',' or ';' or ':' or ')' or ']' or '}';
    }

    private static void WriteMergedParagraph(
        StringBuilder html,
        ContinuousParagraphMerge merge,
        IReadOnlyDictionary<string, PdfLayoutImageAsset> imageAssets,
        float scale)
    {
        html.Append("      <p class=\"")
            .Append(SemanticClassNames(merge.StartElement, merge.Current.Page, allowMeasuredWidth: false))
            .Append(" pdf-semantic-page-spanning\"");
        string style = FlowSemanticStyle(merge.StartElement, merge.Current.Page, allowMeasuredWidth: false);
        if (style.Length > 0)
        {
            html.Append(" style=\"")
                .Append(HtmlAttribute(style))
                .Append('"');
        }

        html.Append('>');
        WriteSemanticText(html, merge.StartElement, merge.Current.Footnotes, merge.Current.Page);
        if (merge.CurrentTrailingFootnotes.Count > 0)
        {
            WriteInlineFootnoteSection(
                html,
                merge.CurrentTrailingFootnotes,
                merge.Current.Footnotes,
                merge.Current.Page,
                DecorativeFootnoteRulePath(merge.Current.Page, merge.Current.SemanticPage));
        }

        if (merge.CurrentPageNumberFooter != null)
        {
            WriteInlineFlowSemanticElement(
                html,
                merge.CurrentPageNumberFooter,
                merge.Current.Footnotes,
                merge.Current.Page);
        }

        WriteInlinePageBreak(html, merge.Next.Page.PageNumber);
        WriteMergedParagraphInterruptions(html, merge, imageAssets, scale);
        if (NeedsSpaceBetween(merge.StartElement.Text, merge.ContinuationElement.Text))
        {
            html.Append(' ');
        }

        html.Append("<span class=\"pdf-semantic-page-continuation\">");
        WriteSemanticText(html, merge.ContinuationElement, merge.Next.Footnotes, merge.Next.Page);
        html.AppendLine("</span></p>");
    }

    private static void WriteInlinePageBreak(StringBuilder html, int pageNumber)
    {
        string pageNumberText = pageNumber.ToString(CultureInfo.InvariantCulture);
        html.Append("<span class=\"pdf-semantic-page-break pdf-semantic-inline-page-break\" id=\"page-")
            .Append(pageNumberText)
            .Append("\" data-page-number=\"")
            .Append(pageNumberText)
            .Append("\" role=\"separator\" aria-label=\"Original PDF page ")
            .Append(pageNumberText)
            .Append("\"></span>");
    }

    private static void WriteMergedParagraphInterruptions(
        StringBuilder html,
        ContinuousParagraphMerge merge,
        IReadOnlyDictionary<string, PdfLayoutImageAsset> imageAssets,
        float scale)
    {
        int nextFigureRegion = 0;
        foreach (PdfSemanticElement element in merge.LeadingElements)
        {
            while (nextFigureRegion < merge.LeadingFigureRegions.Count &&
                ShouldInsertFigureSpaceBefore(element, merge.LeadingFigureRegions[nextFigureRegion]))
            {
                WriteSemanticFigure(
                    html,
                    merge.Next.Page,
                    merge.Next.SemanticPage,
                    merge.LeadingFigureRegions[nextFigureRegion],
                    imageAssets,
                    scale,
                    inline: true);
                nextFigureRegion++;
            }

            WriteInlineFlowSemanticElement(html, element, merge.Next.Footnotes, merge.Next.Page);
        }

        while (nextFigureRegion < merge.LeadingFigureRegions.Count)
        {
            WriteSemanticFigure(
                html,
                merge.Next.Page,
                merge.Next.SemanticPage,
                merge.LeadingFigureRegions[nextFigureRegion],
                imageAssets,
                scale,
                inline: true);
            nextFigureRegion++;
        }
    }

    private static void WriteInlineFlowSemanticElement(
        StringBuilder html,
        PdfSemanticElement element,
        FootnoteContext footnotes,
        PdfLayoutPage page)
    {
        html.Append("<span class=\"")
            .Append(SemanticClassNames(element, page, allowMeasuredWidth: false))
            .Append(" pdf-semantic-inline-flow-element\"");
        string style = FlowSemanticStyle(element, page, allowMeasuredWidth: false);
        if (style.Length > 0)
        {
            html.Append(" style=\"")
                .Append(HtmlAttribute(style))
                .Append('"');
        }

        html.Append('>');
        WriteSemanticText(html, element, footnotes, page);
        html.Append("</span>");
    }

    private static bool NeedsSpaceBetween(string first, string second)
    {
        string left = first.TrimEnd();
        string right = second.TrimStart();
        if (left.Length == 0 || right.Length == 0)
        {
            return false;
        }

        if (left.EndsWith('a') && right.StartsWith("nd", StringComparison.Ordinal))
        {
            return false;
        }

        return !NoSpaceAfter(left[^1]) && !NoSpaceBefore(right[0]) && left[^1] != '-';
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
        FootnoteContext footnotes,
        float scale)
    {
        if (elements.Count == 0)
        {
            return;
        }

        PdfSemanticElement[] flowArtifacts = elements
            .Where(element => !IsContinuousPositionedPageArtifact(page, element))
            .ToArray();
        foreach (PdfSemanticElement element in elements.Where(element => IsContinuousPositionedPageArtifact(page, element)))
        {
            WritePositionedSemanticElement(html, page, element, footnotes, scale);
        }

        if (flowArtifacts.Length == 0)
        {
            return;
        }

        html.Append("      <aside class=\"pdf-semantic-page-artifacts\" aria-label=\"Original page ")
            .Append(page.PageNumber.ToString(CultureInfo.InvariantCulture))
            .AppendLine(" artifacts\">");
        foreach (PdfSemanticElement element in flowArtifacts)
        {
            WriteFlowSemanticElement(html, element, footnotes, page, allowMeasuredWidth: false);
        }

        html.AppendLine("      </aside>");
    }

    private static bool IsContinuousPositionedPageArtifact(PdfLayoutPage page, PdfSemanticElement element)
    {
        if (element.Kind is not (PdfSemanticElementKind.Header or PdfSemanticElementKind.Footer))
        {
            return false;
        }

        float direction = MathF.Abs(SemanticDirection(element));
        if (MathF.Abs(direction - 90f) > 0.01f && MathF.Abs(direction - 270f) > 0.01f)
        {
            return false;
        }

        float left = direction < 180f
            ? element.Bounds.Y
            : page.Width - element.Bounds.Y;
        return left <= page.Width * 0.16f ||
            left >= page.Width * 0.84f;
    }

    private static void WriteSemanticFlowElements(
        StringBuilder html,
        PdfLayoutPage page,
        PdfSemanticPage semanticPage,
        IReadOnlyList<PdfSemanticElement> flowElements,
        FootnoteContext footnotes,
        float scale,
        IReadOnlyDictionary<string, PdfLayoutImageAsset>? imageAssets,
        SemanticFigureRendering figureRendering,
        bool omitSimplePageNumberFooters,
        ISet<PdfSemanticElement>? skippedElements,
        ISet<PdfLayoutRectangle>? skippedFigureRegions,
        ContinuousParagraphMerge? paragraphMerge)
    {
        PdfLayoutRectangle[] figureRegions = figureRendering == SemanticFigureRendering.None
            ? []
            : SemanticFigureRegions(page, semanticPage)
                .Where(region => skippedFigureRegions == null || !skippedFigureRegions.Contains(region))
                .ToArray();
        int nextFigureRegion = 0;
        for (int index = 0; index < flowElements.Count; index++)
        {
            PdfSemanticElement element = flowElements[index];
            if (skippedElements?.Contains(element) == true)
            {
                continue;
            }

            if (omitSimplePageNumberFooters && IsSimplePageNumberFooter(element, page))
            {
                continue;
            }

            while (nextFigureRegion < figureRegions.Length &&
                ShouldInsertFigureSpaceBefore(element, figureRegions[nextFigureRegion]))
            {
                if (figureRendering == SemanticFigureRendering.Space)
                {
                    WriteFigureSpace(html, figureRegions[nextFigureRegion], scale);
                }
                else if (imageAssets != null)
                {
                    WriteSemanticFigure(
                        html,
                        page,
                        semanticPage,
                        figureRegions[nextFigureRegion],
                        imageAssets,
                        scale,
                        inline: false);
                }

                nextFigureRegion++;
            }

            if (paragraphMerge?.StartElement == element)
            {
                WriteMergedParagraph(
                    html,
                    paragraphMerge,
                    imageAssets ?? new Dictionary<string, PdfLayoutImageAsset>(StringComparer.Ordinal),
                    scale);
                continue;
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
                    page,
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

    private static bool WriteSemanticFigure(
        StringBuilder html,
        PdfLayoutPage page,
        PdfSemanticPage semanticPage,
        PdfLayoutRectangle region,
        IReadOnlyDictionary<string, PdfLayoutImageAsset> imageAssets,
        float scale,
        bool inline)
    {
        PdfLayoutImage[] images = page.Images
            .Where(image => RectanglesIntersect(image.Bounds, region, 2f))
            .ToArray();
        PdfLayoutPath[] paths = FigureRegionPaths(page, semanticPage, region).ToArray();
        if (images.Length == 0 && paths.Length == 0)
        {
            return false;
        }

        string tagName = inline ? "span" : "figure";
        html.Append("      <")
            .Append(tagName)
            .Append(" class=\"pdf-semantic-figure");
        if (inline)
        {
            html.Append(" pdf-semantic-inline-figure");
        }

        html.Append("\" data-source-page=\"")
            .Append(page.PageNumber.ToString(CultureInfo.InvariantCulture))
            .Append("\" data-source-top=\"")
            .Append(HtmlAttribute(CssPoints(region.Y)))
            .Append("\" style=\"--pdf-semantic-figure-width:")
            .Append(CssPoints(region.Width * scale))
            .Append("\">");
        html.Append("<svg class=\"pdf-semantic-figure-svg\" viewBox=\"")
            .Append(SvgNumber(region.X))
            .Append(' ')
            .Append(SvgNumber(region.Y))
            .Append(' ')
            .Append(SvgNumber(region.Width))
            .Append(' ')
            .Append(SvgNumber(region.Height))
            .Append("\" width=\"")
            .Append(SvgNumber(region.Width * scale))
            .Append("\" height=\"")
            .Append(SvgNumber(region.Height * scale))
            .Append("\" aria-hidden=\"true\">");

        foreach (PdfLayoutImage image in images)
        {
            if (imageAssets.TryGetValue(image.AssetId, out PdfLayoutImageAsset? asset))
            {
                WriteSvgImage(html, image, asset);
            }
        }

        foreach (PdfLayoutPath path in paths)
        {
            WriteVectorPath(html, path);
        }

        html.Append("</svg></")
            .Append(tagName)
            .AppendLine(">");
        return true;
    }

    private static IEnumerable<PdfLayoutPath> FigureRegionPaths(
        PdfLayoutPage page,
        PdfSemanticPage semanticPage,
        PdfLayoutRectangle region)
    {
        return page.Paths
            .Where(path => !IsSemanticFlowRulePath(page, semanticPage, path))
            .Where(path => path.Bounds.Width > 0.1f || path.Bounds.Height > 0.1f)
            .Where(path => RectanglesIntersect(path.Bounds, region, 2f));
    }

    private static void WriteSvgImage(StringBuilder html, PdfLayoutImage image, PdfLayoutImageAsset asset)
    {
        html.Append("<image href=\"")
            .Append(HtmlAttribute(asset.RelativePath))
            .Append("\" x=\"")
            .Append(SvgNumber(image.Bounds.X))
            .Append("\" y=\"")
            .Append(SvgNumber(image.Bounds.Y))
            .Append("\" width=\"")
            .Append(SvgNumber(image.Bounds.Width))
            .Append("\" height=\"")
            .Append(SvgNumber(image.Bounds.Height))
            .Append("\" preserveAspectRatio=\"none\" />");
    }

    private static bool RectanglesIntersect(PdfLayoutRectangle first, PdfLayoutRectangle second, float tolerance)
    {
        return first.Right >= second.X - tolerance &&
            second.Right >= first.X - tolerance &&
            first.Bottom >= second.Y - tolerance &&
            second.Bottom >= first.Y - tolerance;
    }

    private static IEnumerable<PdfLayoutRectangle> SemanticFigureRegions(
        PdfLayoutPage page,
        PdfSemanticPage semanticPage)
    {
        List<PdfLayoutRectangle> regions = [];
        PdfLayoutRectangle[] imageBounds = page.Images
            .Select(static image => image.Bounds)
            .ToArray();
        regions.AddRange(imageBounds.Where(bounds => IsSubstantialGraphic(page, bounds)));

        PdfLayoutPath[] candidatePaths = page.Paths
            .Where(path => !IsSemanticFlowRulePath(page, semanticPage, path))
            .Where(path => path.Bounds.Width > 2f && path.Bounds.Height > 2f)
            .ToArray();
        PdfLayoutRectangle[] largePathBounds = candidatePaths
            .Select(static path => path.Bounds)
            .Where(bounds => IsSubstantialGraphic(page, bounds))
            .ToArray();
        regions.AddRange(largePathBounds);
        regions.AddRange(GraphicRowRegions(page, imageBounds.Concat(largePathBounds).ToArray()));

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

    private static IEnumerable<PdfLayoutRectangle> GraphicRowRegions(
        PdfLayoutPage page,
        IReadOnlyList<PdfLayoutRectangle> bounds)
    {
        List<List<PdfLayoutRectangle>> rows = [];
        foreach (PdfLayoutRectangle rectangle in bounds
            .OrderBy(static bounds => bounds.Y + (bounds.Height / 2f))
            .ThenBy(static bounds => bounds.X))
        {
            List<PdfLayoutRectangle>? row = rows.FirstOrDefault(existing =>
                BelongsToGraphicRow(page, UnionRectangles(existing), rectangle));
            if (row == null)
            {
                rows.Add([rectangle]);
            }
            else
            {
                row.Add(rectangle);
            }
        }

        foreach (List<PdfLayoutRectangle> row in rows.Where(static row => row.Count >= 2))
        {
            PdfLayoutRectangle union = UnionRectangles(row);
            if (IsSubstantialGraphic(page, union))
            {
                yield return union;
            }
        }
    }

    private static bool BelongsToGraphicRow(
        PdfLayoutPage page,
        PdfLayoutRectangle rowBounds,
        PdfLayoutRectangle rectangle)
    {
        float overlap = MathF.Min(rowBounds.Bottom, rectangle.Bottom) - MathF.Max(rowBounds.Y, rectangle.Y);
        bool verticalOverlap = overlap >= MathF.Min(rowBounds.Height, rectangle.Height) * 0.30f;
        float centerDistance = MathF.Abs(
            rowBounds.Y + (rowBounds.Height / 2f) - (rectangle.Y + (rectangle.Height / 2f)));
        bool similarBand = centerDistance <= MathF.Max(24f, MathF.Max(rowBounds.Height, rectangle.Height) * 0.35f);
        return (verticalOverlap || similarBand) &&
            HorizontalGap(rowBounds, rectangle) <= page.Width * 0.30f;
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
        PdfLayoutPage? page,
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
            WriteFootnote(html, elements[index], footnotes, page);
        }

        html.AppendLine("      </section>");
        return index - 1;
    }

    private static void WriteInlineFootnoteSection(
        StringBuilder html,
        IReadOnlyList<PdfSemanticElement> footnoteElements,
        FootnoteContext footnotes,
        PdfLayoutPage? page,
        PdfLayoutPath? footnoteRule)
    {
        html.Append("<span class=\"pdf-semantic-footnotes pdf-semantic-inline-footnotes\" aria-label=\"Footnotes\"");
        if (footnoteRule != null)
        {
            html.Append(" style=\"")
                .Append(HtmlAttribute(FootnoteRuleStyle(footnoteRule)))
                .Append('"');
        }

        html.Append(">");
        foreach (PdfSemanticElement footnote in footnoteElements)
        {
            WriteInlineFootnote(html, footnote, footnotes, page);
        }

        html.Append("</span>");
    }

    private static void WriteInlineFootnote(
        StringBuilder html,
        PdfSemanticElement element,
        FootnoteContext footnotes,
        PdfLayoutPage? page)
    {
        string text = element.Text.Trim();
        string marker = text.Length > 0 ? text[..1] : "";
        string body = footnotes.Contains(marker) && text.Length > marker.Length
            ? text[marker.Length..].TrimStart()
            : text;
        html.Append("<span id=\"")
            .Append(HtmlAttribute(footnotes.IdFor(marker)))
            .Append("\" class=\"")
            .Append(SemanticClassNames(element))
            .Append("\"><a class=\"pdf-semantic-footnote-backref\" href=\"")
            .Append(HtmlAttribute(footnotes.FirstReferenceHref(marker)))
            .Append("\">")
            .Append(Html(marker))
            .Append("</a> ");
        WriteFootnoteBody(html, element, marker, body, footnotes, page);
        html.Append("</span>");
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
        if (element.Kind == PdfSemanticElementKind.Table && element.TableRows.Count > 0)
        {
            WriteSemanticTable(html, element, footnotes, page);
            return;
        }

        if (page != null && IsFormulaBlock(element))
        {
            WriteFormulaBlock(html, page, element);
            return;
        }

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
        WriteSemanticText(html, element, footnotes, page);
        html.Append("</")
            .Append(tagName)
            .AppendLine(">");
    }

    private static void WriteSemanticTable(
        StringBuilder html,
        PdfSemanticElement element,
        FootnoteContext footnotes,
        PdfLayoutPage? page)
    {
        html.Append("      <table class=\"")
            .Append(SemanticClassNames(element, allowMeasuredWidth: false))
            .Append("\" aria-label=\"")
            .Append(HtmlAttribute(TableAriaLabel(element)))
            .AppendLine("\">");

        PdfSemanticTableRow[] headerRows = element.TableRows
            .TakeWhile(static row => row.IsHeader)
            .ToArray();
        PdfSemanticTableRow[] bodyRows = element.TableRows
            .Skip(headerRows.Length)
            .ToArray();
        if (headerRows.Length > 0)
        {
            html.AppendLine("        <thead>");
            foreach (PdfSemanticTableRow row in headerRows)
            {
                WriteSemanticTableRow(html, row, footnotes, page, header: true);
            }

            html.AppendLine("        </thead>");
        }

        html.AppendLine("        <tbody>");
        foreach (PdfSemanticTableRow row in bodyRows.Length == 0 ? headerRows : bodyRows)
        {
            WriteSemanticTableRow(html, row, footnotes, page, header: false);
        }

        html.AppendLine("        </tbody>");
        html.AppendLine("      </table>");
    }

    private static void WriteSemanticTableRow(
        StringBuilder html,
        PdfSemanticTableRow row,
        FootnoteContext footnotes,
        PdfLayoutPage? page,
        bool header)
    {
        string cellTag = header ? "th" : "td";
        html.AppendLine("          <tr>");
        foreach (PdfSemanticTableCell cell in row.Cells)
        {
            html.Append("            <")
                .Append(cellTag);
            if (header)
            {
                html.Append(" scope=\"col\"");
            }

            string cellClass = SemanticTableCellClassNames(cell);
            if (cellClass.Length > 0)
            {
                html.Append(" class=\"")
                    .Append(cellClass)
                    .Append('"');
            }

            html.Append('>');
            WriteSemanticTableCell(html, cell, footnotes, page);

            html.Append("</")
                .Append(cellTag)
                .AppendLine(">");
        }

        html.AppendLine("          </tr>");
    }

    private static string SemanticTableCellClassNames(PdfSemanticTableCell cell)
    {
        List<string> classes = [];
        if (cell.BorderTop)
        {
            classes.Add("pdf-semantic-table-cell-border-top");
        }

        if (cell.BorderRight)
        {
            classes.Add("pdf-semantic-table-cell-border-right");
        }

        if (cell.BorderBottom)
        {
            classes.Add("pdf-semantic-table-cell-border-bottom");
        }

        if (cell.BorderLeft)
        {
            classes.Add("pdf-semantic-table-cell-border-left");
        }

        return string.Join(" ", classes);
    }

    private static void WriteSemanticTableCell(
        StringBuilder html,
        PdfSemanticTableCell cell,
        FootnoteContext footnotes,
        PdfLayoutPage? page)
    {
        if (cell.Lines.Count == 0)
        {
            html.Append(Html(cell.Text));
            return;
        }

        PdfSemanticElement cellElement = new(
            PdfSemanticElementKind.Paragraph,
            cell.Text,
            cell.Bounds,
            cell.Lines);
        for (int index = 0; index < cell.Lines.Count; index++)
        {
            if (index > 0)
            {
                html.Append("<br />");
            }

            PdfSemanticLine line = cell.Lines[index];
            List<InlineTextSegment> segments = InlineTextSegments(
                line,
                page,
                cellElement,
                includeAttachedInlineMath: false).ToList();
            string lineText = string.Concat(segments.Select(static segment => segment.Text));
            WriteInlineTextSegments(html, line, segments, lineText, footnotes);
        }
    }

    private static string TableAriaLabel(PdfSemanticElement element)
    {
        string label = element.Text.Replace('\t', ' ').Replace(Environment.NewLine, " ");
        return label.Length <= 120 ? label : label[..120];
    }

    private static void WriteFormulaBlock(
        StringBuilder html,
        PdfLayoutPage page,
        PdfSemanticElement element)
    {
        PdfLayoutRectangle bounds = FormulaRenderBounds(page, element);
        PdfTextRun[] runs = FormulaRuns(page, bounds, element.Bounds).ToArray();
        PdfLayoutPath[] paths = FormulaPaths(page, bounds).ToArray();
        html.Append("      <div class=\"")
            .Append(SemanticClassNames(element, page, allowMeasuredWidth: false))
            .Append("\" role=\"math\" aria-label=\"")
            .Append(HtmlAttribute(element.Text))
            .Append("\" style=\"--pdf-semantic-formula-width:")
            .Append(CssPoints(bounds.Width))
            .Append(";--pdf-semantic-formula-height:")
            .Append(CssPoints(bounds.Height))
            .Append("\">");

        if (runs.Length == 0)
        {
            html.Append(Html(element.Text));
        }
        else
        {
            if (paths.Length > 0)
            {
                WriteFormulaVectorLayer(html, bounds, paths);
            }

            foreach (PdfTextRun run in runs)
            {
                html.Append("<span class=\"pdf-semantic-formula-run");
                if (IsFormulaRadicalRun(run))
                {
                    html.Append(" pdf-semantic-formula-radical");
                }

                html.Append("\" style=\"left:")
                    .Append(CssPoints(run.Bounds.X - bounds.X))
                    .Append(";top:")
                    .Append(CssPoints(run.Bounds.Y - bounds.Y))
                    .Append(";font-family:")
                    .Append(CssFontFamily(NormalizeFontName(run.FontName)))
                    .Append(";font-size:")
                    .Append(CssPoints(run.FontSize))
                    .Append(";color:")
                    .Append(ColorHex(run.Color))
                    .Append("\">")
                    .Append(Html(FormulaRunText(run)))
                    .Append("</span>");
            }
        }

        html.AppendLine("</div>");
    }

    private static PdfLayoutRectangle FormulaRenderBounds(PdfLayoutPage page, PdfSemanticElement element)
    {
        PdfLayoutRectangle expanded = ExpandRectangle(element.Bounds, 5f, 5f);
        PdfTextRun[] runs = FormulaRuns(page, expanded, element.Bounds).ToArray();
        PdfLayoutPath[] paths = FormulaPaths(page, expanded).ToArray();
        if (runs.Length == 0 && paths.Length == 0)
        {
            return expanded;
        }

        return ExpandRectangle(UnionRectangles(
            runs.Select(static run => run.Bounds)
                .Concat(paths.Select(static path => path.Bounds))), 2f, 2f);
    }

    private static IEnumerable<PdfTextRun> FormulaRuns(
        PdfLayoutPage page,
        PdfLayoutRectangle bounds,
        PdfLayoutRectangle coreBounds)
    {
        return page.Runs
            .Where(static run => MathF.Abs(run.Direction) < 0.01f)
            .Where(run => RectanglesIntersect(run.Bounds, coreBounds, 0.75f) ||
                RectanglesIntersect(run.Bounds, bounds, 0.75f) && IsFormulaRunCandidate(run) ||
                IsFormulaAdjacentRun(page, bounds, run))
            .OrderBy(static run => run.Bounds.Y)
            .ThenBy(static run => run.Bounds.X);
    }

    private static IEnumerable<PdfLayoutPath> FormulaPaths(PdfLayoutPage page, PdfLayoutRectangle bounds)
    {
        return page.Paths
            .Where(path => path.Bounds.Width > 0.1f || path.Bounds.Height > 0.1f)
            .Where(path => RectanglesIntersect(path.Bounds, bounds, 1.5f))
            .OrderBy(static path => path.Bounds.Y)
            .ThenBy(static path => path.Bounds.X);
    }

    private static bool IsFormulaRunCandidate(PdfTextRun run)
    {
        string text = run.Text.Trim();
        return text.Length > 0 &&
            (HasMathFont(run.FontName) ||
                HasFormulaFunction(text) ||
                text.All(static character => character is '(' or ')' or ',' or '.' or '=' or '+' or '-' or '/' or ':' or ';'));
    }

    private static bool IsFormulaRadicalRun(PdfTextRun run)
    {
        return run.Text.Trim() == "√";
    }

    private static string FormulaRunText(PdfTextRun run)
    {
        if (run.Glyphs.Count <= 1)
        {
            return run.Text;
        }

        string reconstructed = ReconstructText(run.Glyphs);
        return reconstructed.Length == 0 ? run.Text : reconstructed;
    }

    private static void WriteFormulaVectorLayer(
        StringBuilder html,
        PdfLayoutRectangle bounds,
        IReadOnlyList<PdfLayoutPath> paths)
    {
        html.Append("<svg class=\"pdf-semantic-formula-vector-layer\" viewBox=\"")
            .Append(SvgNumber(bounds.X))
            .Append(' ')
            .Append(SvgNumber(bounds.Y))
            .Append(' ')
            .Append(SvgNumber(bounds.Width))
            .Append(' ')
            .Append(SvgNumber(bounds.Height))
            .Append("\" aria-hidden=\"true\">");

        foreach (PdfLayoutPath path in paths)
        {
            WriteVectorPath(html, path);
        }

        html.Append("</svg>");
    }

    private static bool IsFormulaAdjacentRun(PdfLayoutPage page, PdfLayoutRectangle bounds, PdfTextRun run)
    {
        float formulaCenter = bounds.Y + (bounds.Height / 2f);
        float runCenter = run.Bounds.Y + (run.Bounds.Height / 2f);
        float verticalTolerance = MathF.Max(8f, MathF.Min(18f, bounds.Height * 0.35f));
        if (MathF.Abs(runCenter - formulaCenter) > verticalTolerance)
        {
            return false;
        }

        string text = run.Text.Trim();
        if (text.Length == 0)
        {
            return false;
        }

        bool rightOfFormula = run.Bounds.X >= bounds.X - 4f &&
            run.Bounds.X <= MathF.Min(page.Width - 24f, bounds.Right + 220f);
        if (!rightOfFormula)
        {
            return false;
        }

        return HasMathFont(run.FontName) ||
            text.All(static character => character is '(' or ')' or ',' or '.' or '=' or '+' or '-' or '/' or ':' or ';') ||
            IsEquationNumber(text);
    }

    private static bool IsEquationNumber(string text)
    {
        return text.Length >= 3 &&
            text[0] == '(' &&
            text[^1] == ')' &&
            text[1..^1].All(static character => char.IsDigit(character));
    }

    private static PdfLayoutRectangle ExpandRectangle(PdfLayoutRectangle bounds, float horizontal, float vertical)
    {
        return new PdfLayoutRectangle(
            bounds.X - horizontal,
            bounds.Y - vertical,
            bounds.Width + horizontal + horizontal,
            bounds.Height + vertical + vertical);
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
        WriteSemanticText(html, element, footnotes, page);
        html.Append("</")
            .Append(tagName)
            .AppendLine(">");
    }

    private static void WriteSemanticText(
        StringBuilder html,
        PdfSemanticElement element,
        FootnoteContext footnotes,
        PdfLayoutPage? page = null)
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

                html.Append(Html(element.Lines[index].Text));
            }

            return;
        }

        if (CanWriteRichSemanticText(element))
        {
            WriteRichSemanticText(html, element, footnotes, page);
            return;
        }

        WriteTextWithFootnoteReferences(html, element.Text, footnotes);
    }

    private static bool CanWriteRichSemanticText(PdfSemanticElement element)
    {
        return element.Kind is PdfSemanticElementKind.Paragraph or PdfSemanticElementKind.Heading or PdfSemanticElementKind.Footnote &&
            !IsFormulaBlock(element) &&
            element.Lines.Count > 0 &&
            element.Lines.All(static line =>
                MathF.Abs(line.Direction) < 0.01f &&
                line.Runs.Count > 0);
    }

    private static void WriteRichSemanticText(
        StringBuilder html,
        PdfSemanticElement element,
        FootnoteContext footnotes,
        PdfLayoutPage? page,
        string? leadingTextToSkip = null)
    {
        string previousLineText = "";
        bool wroteLine = false;
        bool previousLineEndedWithMathIdentifier = false;
        bool skippedLeadingText = string.IsNullOrEmpty(leadingTextToSkip);
        foreach (PdfSemanticLine line in element.Lines)
        {
            if (IsDetachedMathAttachmentLine(line, element))
            {
                continue;
            }

            List<InlineTextSegment> segments = InlineTextSegments(line, page, element).ToList();
            if (ShouldPrependMissingSummation(previousLineText, segments))
            {
                PrependMissingSummation(segments);
            }

            if (!skippedLeadingText &&
                TryRemoveLeadingText(segments, leadingTextToSkip!))
            {
                TrimLeadingWhitespace(segments);
                skippedLeadingText = true;
            }

            bool continuesMathIdentifier = previousLineEndedWithMathIdentifier &&
                TryPromoteLeadingKnownMathIdentifierSuffix(segments);
            string lineText = string.Concat(segments.Select(static segment => segment.Text));
            if (lineText.Length == 0)
            {
                continue;
            }

            if (wroteLine && !continuesMathIdentifier && NeedsSpaceBetween(previousLineText, lineText))
            {
                html.Append(' ');
            }

            WriteInlineTextSegments(html, line, segments, lineText, footnotes);
            previousLineText = lineText;
            wroteLine = true;
            previousLineEndedWithMathIdentifier = EndsWithMathBaseIdentifierSegment(segments);
        }
    }

    private static bool IsDetachedMathAttachmentLine(PdfSemanticLine line, PdfSemanticElement element)
    {
        return element.Lines.Count > 1 &&
            line.Text.Length <= 3 &&
            line.Runs.Count > 0 &&
            line.Runs.All(static run => IsCompactMathFont(run.FontName) || run.Text == "√");
    }

    private static IReadOnlyList<InlineTextSegment> InlineTextSegments(
        PdfSemanticLine line,
        PdfLayoutPage? page,
        PdfSemanticElement element,
        bool includeAttachedInlineMath = true)
    {
        IEnumerable<(PdfTextRun Run, PdfTextGlyph Glyph)> glyphSource = line.Runs
            .Where(static run => MathF.Abs(run.Direction) < 0.01f)
            .SelectMany(static run => run.Glyphs.Select(glyph => (Run: run, Glyph: glyph)))
            .Where(static item => !string.IsNullOrEmpty(item.Glyph.Text));
        if (includeAttachedInlineMath)
        {
            glyphSource = glyphSource.Concat(AttachedInlineMathGlyphs(line, page, element));
        }

        (PdfTextRun Run, PdfTextGlyph Glyph)[] glyphs = glyphSource
            .OrderBy(static item => item.Glyph.Bounds.X)
            .ThenBy(static item => item.Glyph.Bounds.Y)
            .ToArray();
        if (glyphs.Length == 0)
        {
            return [new InlineTextSegment(line.Text, null, InlineBaselineRole.Normal)];
        }

        List<InlineTextSegment> segments = [];
        PdfTextGlyph? previous = null;
        InlineBaselineRole previousRole = InlineBaselineRole.Normal;
        bool skipNextWhitespaceAfterIntrusiveGlyph = false;
        bool suppressNextWordBoundaryAfterIntrusiveGlyph = false;
        for (int index = 0; index < glyphs.Length; index++)
        {
            (PdfTextRun run, PdfTextGlyph glyph) = glyphs[index];
            if (skipNextWhitespaceAfterIntrusiveGlyph)
            {
                if (string.IsNullOrWhiteSpace(glyph.Text))
                {
                    continue;
                }

                string trimmedText = glyph.Text.TrimStart();
                if (trimmedText.Length == 0)
                {
                    continue;
                }

                if (trimmedText.Length != glyph.Text.Length)
                {
                    glyph = glyph with { Text = trimmedText };
                }

                skipNextWhitespaceAfterIntrusiveGlyph = false;
            }

            if (IsMathAttachmentWhitespace(glyphs, index, line))
            {
                continue;
            }

            if (IsIntrusiveRadicalGlyph(glyphs, index, line))
            {
                RemoveTrailingWhitespace(segments);
                skipNextWhitespaceAfterIntrusiveGlyph = true;
                suppressNextWordBoundaryAfterIntrusiveGlyph = true;
                continue;
            }

            InlineBaselineRole role = BaselineRole(line, glyph);
            if (!suppressNextWordBoundaryAfterIntrusiveGlyph &&
                previous != null &&
                ShouldInsertWordBoundary(previous, glyph) &&
                AllowsWordBoundary(previousRole, role))
            {
                segments.Add(new InlineTextSegment(" ", null, InlineBaselineRole.Normal));
            }

            segments.Add(new InlineTextSegment(glyph.Text, run, role));
            previous = glyph;
            previousRole = role;
            suppressNextWordBoundaryAfterIntrusiveGlyph = false;
        }

        RepairCommonWordBreaks(segments);
        PromoteMathIdentifierSubscripts(segments);
        RepairCommonMathOperatorOmissions(segments);
        RemoveDuplicateAdjacentSubscripts(segments);

        return segments;
    }

    private static IEnumerable<(PdfTextRun Run, PdfTextGlyph Glyph)> AttachedInlineMathGlyphs(
        PdfSemanticLine line,
        PdfLayoutPage? page,
        PdfSemanticElement element)
    {
        if (page == null)
        {
            yield break;
        }

        (PdfTextRun Run, PdfTextGlyph Glyph)[] lineGlyphs = line.Runs
            .Where(static run => MathF.Abs(run.Direction) < 0.01f)
            .SelectMany(static run => run.Glyphs.Select(glyph => (run, glyph)))
            .Where(static item => !string.IsNullOrEmpty(item.glyph.Text))
            .ToArray();
        (PdfTextRun Run, PdfTextGlyph Glyph)[] protectedGlyphs = element.Lines
            .Where(elementLine => ReferenceEquals(elementLine, line) || !IsDetachedMathAttachmentLine(elementLine, element))
            .SelectMany(static elementLine => elementLine.Runs)
            .Where(static run => MathF.Abs(run.Direction) < 0.01f)
            .SelectMany(static run => run.Glyphs.Select(glyph => (run, glyph)))
            .Where(static item => !string.IsNullOrEmpty(item.glyph.Text))
            .ToArray();

        foreach (PdfTextGlyph glyph in page.Glyphs)
        {
            if (string.IsNullOrEmpty(glyph.Text) ||
                !HasMathFont(glyph.FontName) ||
                IsExistingGlyph(protectedGlyphs, glyph) ||
                !ShouldAttachInlineMathGlyph(line, lineGlyphs, glyph))
            {
                continue;
            }

            yield return (new PdfTextRun(
                glyph.Text,
                glyph.FontName,
                glyph.FontSize,
                glyph.Direction,
                glyph.Bounds,
                glyph.Color,
                [glyph]), glyph);
        }
    }

    private static bool ShouldAttachInlineMathGlyph(
        PdfSemanticLine line,
        IReadOnlyList<(PdfTextRun Run, PdfTextGlyph Glyph)> lineGlyphs,
        PdfTextGlyph glyph)
    {
        if (!IsInlineAttachmentBand(line, glyph))
        {
            return false;
        }

        if (glyph.Text == "√")
        {
            return HasMathBaseInLineToRight(lineGlyphs, glyph) ||
                HasCompactFractionStemInLine(lineGlyphs, glyph);
        }

        if (IsInlineMathOperatorGlyph(glyph))
        {
            return ShouldAttachInlineMathOperatorGlyph(line, lineGlyphs, glyph);
        }

        if (IsInlineMathIdentifierGlyph(glyph))
        {
            return ShouldAttachInlineMathIdentifierGlyph(line, lineGlyphs, glyph);
        }

        if (!IsCompactMathFont(glyph.FontName))
        {
            return false;
        }

        return HasMathBaseInLineToLeft(lineGlyphs, glyph) ||
            HasCompactFractionStemInLine(lineGlyphs, glyph);
    }

    private static bool IsInlineAttachmentBand(PdfSemanticLine line, PdfTextGlyph glyph)
    {
        float verticalTolerance = MathF.Max(10f, line.DominantFontSize * 1.25f);
        float horizontalTolerance = MathF.Max(18f, line.DominantFontSize * 2.2f);
        return glyph.Bounds.Y >= line.Bounds.Y - verticalTolerance &&
            glyph.Bounds.Y <= line.Bounds.Bottom + verticalTolerance &&
            glyph.Bounds.Right >= line.Bounds.X - horizontalTolerance &&
            glyph.Bounds.X <= line.Bounds.Right + horizontalTolerance;
    }

    private static bool HasMathBaseInLineToRight(
        IReadOnlyList<(PdfTextRun Run, PdfTextGlyph Glyph)> lineGlyphs,
        PdfTextGlyph glyph)
    {
        foreach ((PdfTextRun run, PdfTextGlyph candidate) in lineGlyphs)
        {
            if (!HasMathFont(run.FontName) ||
                candidate.Text == "√" ||
                candidate.Bounds.X < glyph.Bounds.X - 0.5f ||
                candidate.Bounds.X - glyph.Bounds.Right > 14f ||
                MathF.Abs(candidate.Bounds.Y - glyph.Bounds.Y) > 16f)
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private static bool HasMathBaseInLineToLeft(
        IReadOnlyList<(PdfTextRun Run, PdfTextGlyph Glyph)> lineGlyphs,
        PdfTextGlyph glyph)
    {
        foreach ((PdfTextRun run, PdfTextGlyph candidate) in lineGlyphs)
        {
            float horizontalGap = HorizontalGap(candidate.Bounds, glyph.Bounds);
            if (!HasMathFont(run.FontName) ||
                candidate.Text == "√" ||
                candidate.Bounds.X >= glyph.Bounds.X ||
                horizontalGap > 12f ||
                MathF.Abs(candidate.Bounds.Y - glyph.Bounds.Y) > 10f)
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private static bool HasCompactFractionStemInLine(
        IReadOnlyList<(PdfTextRun Run, PdfTextGlyph Glyph)> lineGlyphs,
        PdfTextGlyph glyph)
    {
        (PdfTextRun Run, PdfTextGlyph Glyph)? radical = lineGlyphs
            .Where(static item => item.Glyph.Text == "√" && IsCompactMathFont(item.Run.FontName))
            .OrderBy(item => MathF.Abs(item.Glyph.Bounds.X - glyph.Bounds.X))
            .Cast<(PdfTextRun Run, PdfTextGlyph Glyph)?>()
            .FirstOrDefault();
        if (radical is not { } radicalGlyph)
        {
            return false;
        }

        (PdfTextRun Run, PdfTextGlyph Glyph)? numerator = lineGlyphs
            .Where(item => item.Glyph.Text == "1" &&
                IsCompactMathFont(item.Run.FontName) &&
                item.Glyph.Bounds.X >= radicalGlyph.Glyph.Bounds.X)
            .OrderBy(item => MathF.Abs(item.Glyph.Bounds.X - radicalGlyph.Glyph.Bounds.Right))
            .Cast<(PdfTextRun Run, PdfTextGlyph Glyph)?>()
            .FirstOrDefault();
        if (numerator is not { } numeratorGlyph)
        {
            return false;
        }

        return glyph.Bounds.X >= radicalGlyph.Glyph.Bounds.X - 0.5f &&
            glyph.Bounds.X <= numeratorGlyph.Glyph.Bounds.Right + 14f &&
            glyph.Bounds.Y >= radicalGlyph.Glyph.Bounds.Y + 2f &&
            glyph.Bounds.Y - radicalGlyph.Glyph.Bounds.Y <= 14f;
    }

    private static bool IsInlineMathOperatorGlyph(PdfTextGlyph glyph)
    {
        return glyph.Text is "∑" or "Σ" or "·" or "×" or "∈";
    }

    private static bool ShouldAttachInlineMathOperatorGlyph(
        PdfSemanticLine line,
        IReadOnlyList<(PdfTextRun Run, PdfTextGlyph Glyph)> lineGlyphs,
        PdfTextGlyph glyph)
    {
        if (glyph.Text is "∑" or "Σ" &&
            !line.Text.Contains('='))
        {
            return false;
        }

        if (glyph.Text is "∑" or "Σ")
        {
            return glyph.Bounds.X >= line.Bounds.X - 4f &&
                glyph.Bounds.X <= line.Bounds.Right + 4f;
        }

        return HasNearbyGlyphInLine(lineGlyphs, glyph);
    }

    private static bool IsInlineMathIdentifierGlyph(PdfTextGlyph glyph)
    {
        return glyph.Text.Length == 1 &&
            char.IsLetter(glyph.Text[0]) &&
            HasMathFont(glyph.FontName) &&
            !IsCompactMathFont(glyph.FontName);
    }

    private static bool ShouldAttachInlineMathIdentifierGlyph(
        PdfSemanticLine line,
        IReadOnlyList<(PdfTextRun Run, PdfTextGlyph Glyph)> lineGlyphs,
        PdfTextGlyph glyph)
    {
        return glyph.Text == "P" &&
            line.Text.Contains("drop", StringComparison.Ordinal) &&
            !lineGlyphs.Any(static item => item.Glyph.Text == "P" && HasMathFont(item.Run.FontName)) &&
            HasNearbyGlyphInLine(lineGlyphs, glyph);
    }

    private static bool HasNearbyGlyphInLine(
        IReadOnlyList<(PdfTextRun Run, PdfTextGlyph Glyph)> lineGlyphs,
        PdfTextGlyph glyph)
    {
        foreach ((_, PdfTextGlyph candidate) in lineGlyphs)
        {
            if (string.IsNullOrWhiteSpace(candidate.Text))
            {
                continue;
            }

            float horizontalGap = HorizontalGap(candidate.Bounds, glyph.Bounds);
            float verticalDistance = MathF.Abs(
                candidate.Bounds.Y + (candidate.Bounds.Height / 2f) -
                (glyph.Bounds.Y + (glyph.Bounds.Height / 2f)));
            if (horizontalGap <= MathF.Max(12f, glyph.FontSize * 1.6f) &&
                verticalDistance <= MathF.Max(8f, glyph.FontSize * 1.1f))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsExistingGlyph(
        IReadOnlyList<(PdfTextRun Run, PdfTextGlyph Glyph)> glyphs,
        PdfTextGlyph candidate)
    {
        return glyphs.Any(item => IsSameGlyph(item.Glyph, candidate));
    }

    private static bool IsSameGlyph(PdfTextGlyph left, PdfTextGlyph right)
    {
        return left.Text == right.Text &&
            string.Equals(left.FontName, right.FontName, StringComparison.Ordinal) &&
            MathF.Abs(left.Bounds.X - right.Bounds.X) < 0.01f &&
            MathF.Abs(left.Bounds.Y - right.Bounds.Y) < 0.01f &&
            MathF.Abs(left.Bounds.Width - right.Bounds.Width) < 0.01f &&
            MathF.Abs(left.Bounds.Height - right.Bounds.Height) < 0.01f;
    }

    private static void PromoteMathIdentifierSubscripts(List<InlineTextSegment> segments)
    {
        for (int index = 0; index < segments.Count; index++)
        {
            InlineTextSegment baseSegment = segments[index];
            if (!IsMathBaseIdentifierSegment(baseSegment))
            {
                continue;
            }

            int suffixStart = NextNonWhitespaceSegmentIndex(segments, index + 1);
            if (suffixStart < 0)
            {
                continue;
            }

            if (!TryPromoteKnownMathIdentifierSuffix(segments, index, suffixStart))
            {
                continue;
            }
        }
    }

    private static bool TryPromoteKnownMathIdentifierSuffix(
        List<InlineTextSegment> segments,
        int baseIndex,
        int suffixStart)
    {
        foreach (string knownSuffix in KnownMathIdentifierSuffixes())
        {
            if (!TryPromoteMathIdentifierSuffix(segments, baseIndex, suffixStart, knownSuffix))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private static bool TryPromoteLeadingKnownMathIdentifierSuffix(List<InlineTextSegment> segments)
    {
        int suffixStart = NextNonWhitespaceSegmentIndex(segments, 0);
        return suffixStart >= 0 &&
            TryPromoteKnownMathIdentifierSuffix(segments, baseIndex: -1, suffixStart);
    }

    private static bool TryPromoteMathIdentifierSuffix(
        List<InlineTextSegment> segments,
        int baseIndex,
        int suffixStart,
        string knownSuffix)
    {
        int matchedLength = MatchIdentifierSuffixLength(segments, suffixStart, knownSuffix);
        if (matchedLength != knownSuffix.Length)
        {
            return false;
        }

        for (int whitespace = baseIndex + 1; whitespace < suffixStart; whitespace++)
        {
            if (string.IsNullOrWhiteSpace(segments[whitespace].Text))
            {
                segments[whitespace] = segments[whitespace] with { Text = "" };
            }
        }

        int remaining = knownSuffix.Length;
        int firstPromotedIndex = -1;
        for (int suffixIndex = suffixStart; suffixIndex < segments.Count && remaining > 0; suffixIndex++)
        {
            InlineTextSegment segment = segments[suffixIndex];
            if (segment.Text.Length == 0)
            {
                continue;
            }

            int leadingWhitespace = suffixIndex == suffixStart ? CountLeadingWhitespace(segment.Text) : 0;
            string text = segment.Text[leadingWhitespace..];
            int take = Math.Min(remaining, text.Length);
            if (take == 0 || !text.AsSpan(0, take).SequenceEqual(knownSuffix.AsSpan(knownSuffix.Length - remaining, take)))
            {
                break;
            }

            string matchedText = text[..take];
            string trailingText = text[take..];
            if (firstPromotedIndex < 0)
            {
                firstPromotedIndex = suffixIndex;
                segments[suffixIndex] = segment with
                {
                    Text = matchedText,
                    Role = InlineBaselineRole.Subscript
                };
            }
            else
            {
                segments[firstPromotedIndex] = segments[firstPromotedIndex] with
                {
                    Text = segments[firstPromotedIndex].Text + matchedText
                };
                segments[suffixIndex] = segment with { Text = "" };
            }

            remaining -= take;

            if (trailingText.Length > 0)
            {
                segments.Insert(suffixIndex + 1, segment with
                {
                    Text = trailingText,
                    Role = InlineBaselineRole.Normal
                });
                break;
            }
        }

        return true;
    }

    private static int MatchIdentifierSuffixLength(
        IReadOnlyList<InlineTextSegment> segments,
        int suffixStart,
        string knownSuffix)
    {
        int matched = 0;
        for (int index = suffixStart; index < segments.Count && matched < knownSuffix.Length; index++)
        {
            string text = segments[index].Text;
            if (text.Length == 0)
            {
                continue;
            }

            if (index == suffixStart)
            {
                text = text[CountLeadingWhitespace(text)..];
            }

            if (text.Length == 0)
            {
                continue;
            }

            int take = Math.Min(knownSuffix.Length - matched, text.Length);
            if (!text.AsSpan(0, take).SequenceEqual(knownSuffix.AsSpan(matched, take)))
            {
                break;
            }

            matched += take;
        }

        return matched;
    }

    private static int CountLeadingWhitespace(string text)
    {
        int count = 0;
        while (count < text.Length && char.IsWhiteSpace(text[count]))
        {
            count++;
        }

        return count;
    }

    private static bool IsMathBaseIdentifierSegment(InlineTextSegment segment)
    {
        return segment.Run != null &&
            segment.Role == InlineBaselineRole.Normal &&
            HasMathFont(segment.Run.FontName) &&
            segment.Text.Length == 1 &&
            segment.Text.All(static character => char.IsLetter(character));
    }

    private static bool EndsWithMathBaseIdentifierSegment(IReadOnlyList<InlineTextSegment> segments)
    {
        for (int index = segments.Count - 1; index >= 0; index--)
        {
            if (string.IsNullOrWhiteSpace(segments[index].Text))
            {
                continue;
            }

            return IsMathBaseIdentifierSegment(segments[index]);
        }

        return false;
    }

    private static int NextNonWhitespaceSegmentIndex(IReadOnlyList<InlineTextSegment> segments, int startIndex)
    {
        for (int index = startIndex; index < segments.Count; index++)
        {
            if (!string.IsNullOrWhiteSpace(segments[index].Text))
            {
                return index;
            }
        }

        return -1;
    }

    private static IEnumerable<string> KnownMathIdentifierSuffixes()
    {
        yield return "model";
        yield return "drop";
        yield return "pos+k";
        yield return "pos";
    }

    private static void RepairCommonWordBreaks(List<InlineTextSegment> segments)
    {
        for (int index = 0; index < segments.Count; index++)
        {
            if (segments[index].Text.Contains("a nd", StringComparison.Ordinal))
            {
                segments[index] = segments[index] with
                {
                    Text = segments[index].Text.Replace("a nd", "and", StringComparison.Ordinal)
                };
            }
        }

        for (int index = 1; index + 1 < segments.Count; index++)
        {
            if (!string.IsNullOrWhiteSpace(segments[index].Text) ||
                !segments[index - 1].Text.EndsWith('a') ||
                !segments[index + 1].Text.StartsWith("nd", StringComparison.Ordinal))
            {
                continue;
            }

            segments.RemoveAt(index);
            return;
        }

        for (int index = 0; index + 1 < segments.Count; index++)
        {
            if (segments[index].Text.EndsWith("a ", StringComparison.Ordinal) &&
                segments[index + 1].Text.StartsWith("nd", StringComparison.Ordinal))
            {
                segments[index] = segments[index] with { Text = segments[index].Text.TrimEnd() };
                return;
            }

            if (segments[index].Text.EndsWith('a') &&
                segments[index + 1].Text.StartsWith(" nd", StringComparison.Ordinal))
            {
                segments[index + 1] = segments[index + 1] with { Text = segments[index + 1].Text.TrimStart() };
                return;
            }
        }
    }

    private static void RepairCommonMathOperatorOmissions(List<InlineTextSegment> segments)
    {
        if (segments.Any(static segment => segment.Text.Contains('∑')))
        {
            return;
        }

        string compactText = CompactSegmentText(segments);
        bool likelyMissingSummation = compactText.Contains("q·k=", StringComparison.Ordinal) &&
            (compactText.Contains("qiki", StringComparison.Ordinal) ||
                HasSummationUpperBoundAfterEquals(segments));
        for (int index = 0; index < segments.Count; index++)
        {
            if (segments[index].Text != "=" ||
                (!likelyMissingSummation && !LooksLikeSummationBounds(segments, index + 1)))
            {
                continue;
            }

            PdfTextRun? run = segments
                .Skip(index)
                .Select(static segment => segment.Run)
                .FirstOrDefault(static run => run != null && HasMathFont(run.FontName));
            segments.Insert(index + 1, new InlineTextSegment("∑", run, InlineBaselineRole.Normal));
            return;
        }
    }

    private static bool HasSummationUpperBoundAfterEquals(IReadOnlyList<InlineTextSegment> segments)
    {
        int equalsIndex = -1;
        for (int index = 0; index < segments.Count; index++)
        {
            if (segments[index].Text == "=")
            {
                equalsIndex = index;
            }
        }

        if (equalsIndex < 0)
        {
            return false;
        }

        int first = NextNonWhitespaceSegmentIndex(segments, equalsIndex + 1);
        if (first < 0)
        {
            return false;
        }

        int second = NextNonWhitespaceSegmentIndex(segments, first + 1);
        return IsCompactMathBoundSegment(segments[first]) &&
            (second < 0 || IsCompactMathBoundSegment(segments[second]));
    }

    private static bool IsCompactMathBoundSegment(InlineTextSegment segment)
    {
        return segment.Run != null &&
            segment.Role is InlineBaselineRole.Superscript or InlineBaselineRole.Subscript &&
            HasMathFont(segment.Run.FontName) &&
            segment.Text.All(static character => char.IsLetterOrDigit(character));
    }

    private static bool ShouldPrependMissingSummation(string previousLineText, IReadOnlyList<InlineTextSegment> segments)
    {
        string previousCompact = CompactText(previousLineText);
        string compact = CompactSegmentText(segments);
        if (previousCompact.EndsWith("q·k=", StringComparison.Ordinal))
        {
            return compact.Contains("qiki", StringComparison.Ordinal) ||
                LooksLikeSummationBounds(segments, 0);
        }

        return previousCompact.Contains("dotproduct", StringComparison.OrdinalIgnoreCase) &&
            previousCompact.Contains('=') &&
            compact.Contains("qiki", StringComparison.Ordinal);
    }

    private static void PrependMissingSummation(List<InlineTextSegment> segments)
    {
        PdfTextRun? run = segments
            .Select(static segment => segment.Run)
            .FirstOrDefault(static run => run != null && HasMathFont(run.FontName));
            segments.Insert(0, new InlineTextSegment("∑", run, InlineBaselineRole.Normal));
    }

    private static void RemoveDuplicateAdjacentSubscripts(List<InlineTextSegment> segments)
    {
        InlineTextSegment? previousSubscript = null;
        for (int index = 0; index < segments.Count; index++)
        {
            InlineTextSegment segment = segments[index];
            if (segment.Text.Length == 0)
            {
                continue;
            }

            if (segment.Role != InlineBaselineRole.Subscript)
            {
                previousSubscript = null;
                continue;
            }

            if (previousSubscript is { } previous &&
                string.Equals(previous.Text, segment.Text, StringComparison.Ordinal))
            {
                segments[index] = segment with { Text = "" };
                continue;
            }

            previousSubscript = segment;
        }
    }

    private static string CompactText(string text)
    {
        StringBuilder compact = new(text.Length);
        foreach (char character in text)
        {
            if (!char.IsWhiteSpace(character))
            {
                compact.Append(character);
            }
        }

        return compact.ToString();
    }

    private static string CompactSegmentText(IEnumerable<InlineTextSegment> segments)
    {
        StringBuilder text = new();
        foreach (InlineTextSegment segment in segments)
        {
            foreach (char character in segment.Text)
            {
                if (!char.IsWhiteSpace(character))
                {
                    text.Append(character);
                }
            }
        }

        return text.ToString();
    }

    private static bool LooksLikeSummationBounds(IReadOnlyList<InlineTextSegment> segments, int startIndex)
    {
        int index = NextNonWhitespaceSegmentIndex(segments, startIndex);
        if (index < 0)
        {
            return false;
        }

        bool hasUpperBound = false;
        bool hasLowerBound = false;
        bool hasIndexedTerm = false;
        int inspected = 0;
        for (; index < segments.Count && inspected < 12; index++)
        {
            InlineTextSegment segment = segments[index];
            if (string.IsNullOrWhiteSpace(segment.Text))
            {
                continue;
            }

            inspected++;
            if (segment.Role == InlineBaselineRole.Superscript)
            {
                hasUpperBound = true;
                continue;
            }

            if (segment.Role == InlineBaselineRole.Subscript)
            {
                hasLowerBound = true;
                continue;
            }

            if (hasLowerBound &&
                segment.Role == InlineBaselineRole.Normal &&
                segment.Run != null &&
                HasMathFont(segment.Run.FontName) &&
                segment.Text is "q" or "k")
            {
                hasIndexedTerm = true;
                continue;
            }

            if (hasIndexedTerm)
            {
                return true;
            }
            if (segment.Text == ",")
            {
                break;
            }
        }

        return hasUpperBound && hasLowerBound && hasIndexedTerm;
    }

    private static bool TryRemoveLeadingText(List<InlineTextSegment> segments, string text)
    {
        if (text.Length == 0)
        {
            return true;
        }

        int remaining = text.Length;
        for (int index = 0; index < segments.Count && remaining > 0; index++)
        {
            InlineTextSegment segment = segments[index];
            if (segment.Text.Length == 0)
            {
                continue;
            }

            string segmentText = segment.Text;
            int leadingWhitespace = remaining == text.Length ? CountLeadingWhitespace(segmentText) : 0;
            string comparable = segmentText[leadingWhitespace..];
            int take = Math.Min(remaining, comparable.Length);
            if (take == 0)
            {
                continue;
            }

            if (!comparable.AsSpan(0, take).SequenceEqual(text.AsSpan(text.Length - remaining, take)))
            {
                return false;
            }

            segments[index] = segment with { Text = comparable[take..] };
            remaining -= take;
        }

        return remaining == 0;
    }

    private static void TrimLeadingWhitespace(List<InlineTextSegment> segments)
    {
        for (int index = 0; index < segments.Count; index++)
        {
            string text = segments[index].Text;
            if (text.Length == 0)
            {
                continue;
            }

            segments[index] = segments[index] with { Text = text.TrimStart() };
            return;
        }
    }

    private static void RemoveTrailingWhitespace(List<InlineTextSegment> segments)
    {
        while (segments.Count > 0 && string.IsNullOrWhiteSpace(segments[^1].Text))
        {
            segments.RemoveAt(segments.Count - 1);
        }

        if (segments.Count == 0)
        {
            return;
        }

        string trimmedText = segments[^1].Text.TrimEnd();
        if (trimmedText.Length == 0)
        {
            segments.RemoveAt(segments.Count - 1);
        }
        else if (trimmedText.Length != segments[^1].Text.Length)
        {
            segments[^1] = segments[^1] with { Text = trimmedText };
        }
    }

    private static bool AllowsWordBoundary(InlineBaselineRole previousRole, InlineBaselineRole currentRole)
    {
        return currentRole != InlineBaselineRole.Subscript &&
            !(previousRole == InlineBaselineRole.Subscript && currentRole == InlineBaselineRole.Subscript);
    }

    private static bool IsMathAttachmentWhitespace(
        IReadOnlyList<(PdfTextRun Run, PdfTextGlyph Glyph)> glyphs,
        int index,
        PdfSemanticLine line)
    {
        if (!string.IsNullOrWhiteSpace(glyphs[index].Glyph.Text))
        {
            return false;
        }

        (PdfTextRun Run, PdfTextGlyph Glyph)? previous = NearestTextGlyph(glyphs, index, -1);
        (PdfTextRun Run, PdfTextGlyph Glyph)? next = NearestTextGlyph(glyphs, index, 1);
        return previous is { } previousGlyph &&
            next is { } nextGlyph &&
            HasMathFont(previousGlyph.Run.FontName) &&
            HasMathFont(nextGlyph.Run.FontName) &&
            BaselineRole(line, nextGlyph.Glyph) == InlineBaselineRole.Subscript;
    }

    private static bool IsIntrusiveRadicalGlyph(
        IReadOnlyList<(PdfTextRun Run, PdfTextGlyph Glyph)> glyphs,
        int index,
        PdfSemanticLine line)
    {
        PdfTextGlyph glyph = glyphs[index].Glyph;
        if (glyph.Text != "√")
        {
            return false;
        }

        if (IsCompactMathFont(glyph.FontName))
        {
            return false;
        }

        (PdfTextRun Run, PdfTextGlyph Glyph)? next = NearestTextGlyph(glyphs, index, 1);
        return next is not { } nextGlyph || !HasMathFont(nextGlyph.Run.FontName);
    }

    private static (PdfTextRun Run, PdfTextGlyph Glyph)? NearestTextGlyph(
        IReadOnlyList<(PdfTextRun Run, PdfTextGlyph Glyph)> glyphs,
        int index,
        int step)
    {
        for (int candidate = index + step; candidate >= 0 && candidate < glyphs.Count; candidate += step)
        {
            if (!string.IsNullOrWhiteSpace(glyphs[candidate].Glyph.Text))
            {
                return glyphs[candidate];
            }
        }

        return null;
    }

    private static InlineBaselineRole BaselineRole(PdfSemanticLine line, PdfTextGlyph glyph)
    {
        float dominantSize = MathF.Max(1f, line.DominantFontSize);
        if (IsCompactMathFont(glyph.FontName) ||
            glyph.FontSize <= dominantSize * 0.74f && HasMathFont(glyph.FontName))
        {
            return CompactMathBaselineRole(line, glyph, dominantSize);
        }

        if (glyph.FontSize > dominantSize * 0.82f)
        {
            return InlineBaselineRole.Normal;
        }

        float baselineCenter = BaselineCenter(line);
        float glyphCenter = glyph.Bounds.Y + (glyph.Bounds.Height / 2f);
        float threshold = MathF.Max(0.6f, dominantSize * 0.08f);
        if (glyphCenter > baselineCenter + threshold)
        {
            return InlineBaselineRole.Subscript;
        }

        if (glyphCenter < baselineCenter - threshold)
        {
            return InlineBaselineRole.Superscript;
        }

        return InlineBaselineRole.Normal;
    }

    private static InlineBaselineRole CompactMathBaselineRole(
        PdfSemanticLine line,
        PdfTextGlyph glyph,
        float dominantSize)
    {
        float compactGlyphCenter = glyph.Bounds.Y + (glyph.Bounds.Height / 2f);
        float baselineCenter = BaselineCenter(line);
        float threshold = MathF.Max(0.5f, dominantSize * 0.05f);
        if (compactGlyphCenter < baselineCenter - threshold)
        {
            return InlineBaselineRole.Superscript;
        }

        if (compactGlyphCenter > baselineCenter + threshold)
        {
            return InlineBaselineRole.Subscript;
        }

        return InlineBaselineRole.Subscript;
    }

    private static float BaselineCenter(PdfSemanticLine line)
    {
        float minimumBaseSize = line.DominantFontSize * 0.82f;
        float[] centers = line.Runs
            .Where(run => MathF.Abs(run.Direction) < 0.01f && run.FontSize >= minimumBaseSize)
            .Select(static run => run.Bounds.Y + (run.Bounds.Height / 2f))
            .Order()
            .ToArray();
        return centers.Length == 0
            ? line.Bounds.Y + (line.Bounds.Height / 2f)
            : centers[centers.Length / 2];
    }

    private static void WriteInlineTextSegments(
        StringBuilder html,
        PdfSemanticLine line,
        IReadOnlyList<InlineTextSegment> segments,
        string lineText,
        FootnoteContext footnotes)
    {
        int offset = 0;
        for (int index = 0; index < segments.Count; index++)
        {
            InlineTextSegment segment = segments[index];
            if (segment.Text.Length == 0)
            {
                continue;
            }

            if (IsMathAttachmentSpaceSegment(segments, index))
            {
                offset += segment.Text.Length;
                continue;
            }

            if (TryWriteCompactInverseSquareRootFraction(html, line, segments, index, out int consumedSegments, out int consumedLength))
            {
                offset += consumedLength;
                index += consumedSegments - 1;
                continue;
            }

            if (TryWriteCompactSummation(html, segments, index, out consumedSegments, out consumedLength))
            {
                offset += consumedLength;
                index += consumedSegments - 1;
                continue;
            }

            WriteInlineTextSegment(html, line, segment, lineText, offset, footnotes);
            offset += segment.Text.Length;
        }
    }

    private static bool TryWriteCompactInverseSquareRootFraction(
        StringBuilder html,
        PdfSemanticLine line,
        IReadOnlyList<InlineTextSegment> segments,
        int index,
        out int consumedSegments,
        out int consumedLength)
    {
        consumedSegments = 0;
        consumedLength = 0;
        if (index + 2 >= segments.Count ||
            !IsCompactSquareRootSegment(segments[index]) ||
            !IsCompactFractionNumeratorOne(segments[index + 1]))
        {
            return false;
        }

        List<InlineTextSegment> denominator = [];
        int denominatorIndex = index + 2;
        while (denominatorIndex < segments.Count &&
            IsCompactFractionDenominatorSegment(segments[denominatorIndex]) &&
            denominator.Count < 4)
        {
            denominator.Add(segments[denominatorIndex]);
            denominatorIndex++;
        }

        if (denominator.Count == 0)
        {
            return false;
        }

        html.Append("<span class=\"pdf-semantic-inline-fraction pdf-semantic-math\"><span class=\"pdf-semantic-inline-fraction-numerator\">");
        html.Append(Html(segments[index + 1].Text));
        html.Append("</span><span class=\"pdf-semantic-inline-fraction-denominator\">");
        html.Append(Html(segments[index].Text));
        for (int denominatorSegmentIndex = 0; denominatorSegmentIndex < denominator.Count; denominatorSegmentIndex++)
        {
            InlineTextSegment segment = denominator[denominatorSegmentIndex];
            if (denominatorSegmentIndex == 0)
            {
                WriteInlineTextSegmentAsRole(html, line, segment, InlineBaselineRole.Normal);
            }
            else
            {
                WriteInlineTextSegmentAsRole(html, line, segment, segment.Role);
            }
        }

        html.Append("</span></span>");
        consumedSegments = 2 + denominator.Count;
        consumedLength = segments.Skip(index).Take(consumedSegments).Sum(static segment => segment.Text.Length);
        return true;
    }

    private static bool TryWriteCompactSummation(
        StringBuilder html,
        IReadOnlyList<InlineTextSegment> segments,
        int index,
        out int consumedSegments,
        out int consumedLength)
    {
        consumedSegments = 0;
        consumedLength = 0;
        if (segments[index].Text is not ("∑" or "Σ"))
        {
            return false;
        }

        List<InlineTextSegment> upper = [];
        List<InlineTextSegment> lower = [];
        int cursor = index + 1;
        while (cursor < segments.Count)
        {
            InlineTextSegment segment = segments[cursor];
            if (string.IsNullOrWhiteSpace(segment.Text))
            {
                int next = NextNonWhitespaceSegmentIndex(segments, cursor + 1);
                if (next >= 0 && IsSummationLimitSegment(segments[next]))
                {
                    cursor++;
                    continue;
                }

                break;
            }

            if (!IsSummationLimitSegment(segment))
            {
                break;
            }

            if (segment.Role == InlineBaselineRole.Superscript)
            {
                upper.Add(segment);
            }
            else
            {
                lower.Add(segment);
            }

            cursor++;
        }

        if (upper.Count == 0 && lower.Count == 0)
        {
            return false;
        }

        html.Append("<span class=\"pdf-semantic-inline-summation pdf-semantic-math\"><span>")
            .Append(Html(segments[index].Text))
            .Append("</span><span class=\"pdf-semantic-inline-summation-limits\"><span>");
        WriteSummationUpperLimit(html, upper);
        html.Append("</span><span>");
        WriteSummationLowerLimit(html, lower);
        html.Append("</span></span></span>");

        consumedSegments = cursor - index;
        consumedLength = segments.Skip(index).Take(consumedSegments).Sum(static segment => segment.Text.Length);
        return true;
    }

    private static bool IsSummationLimitSegment(InlineTextSegment segment)
    {
        return segment.Run != null &&
            segment.Role is InlineBaselineRole.Superscript or InlineBaselineRole.Subscript &&
            HasMathFont(segment.Run.FontName) &&
            segment.Text.All(static character => char.IsLetterOrDigit(character) || character == '=');
    }

    private static void WriteSummationUpperLimit(StringBuilder html, IReadOnlyList<InlineTextSegment> upper)
    {
        string compact = CompactSegmentText(upper);
        if (string.Equals(compact, "dk", StringComparison.Ordinal))
        {
            html.Append("d<sub>k</sub>");
            return;
        }

        html.Append(Html(compact));
    }

    private static void WriteSummationLowerLimit(StringBuilder html, IReadOnlyList<InlineTextSegment> lower)
    {
        string compact = CompactSegmentText(lower);
        if (compact.Contains('i') && compact.Contains('=') && compact.Contains('1'))
        {
            html.Append("i=1");
            return;
        }

        html.Append(Html(compact));
    }

    private static bool IsCompactSquareRootSegment(InlineTextSegment segment)
    {
        return segment.Text == "√" &&
            segment.Run != null &&
            IsCompactMathFont(segment.Run.FontName);
    }

    private static bool IsCompactFractionNumeratorOne(InlineTextSegment segment)
    {
        return segment.Text == "1" &&
            segment.Run != null &&
            IsCompactMathFont(segment.Run.FontName) &&
            segment.Role == InlineBaselineRole.Superscript;
    }

    private static bool IsCompactFractionDenominatorSegment(InlineTextSegment segment)
    {
        return segment.Text.Length > 0 &&
            segment.Run != null &&
            IsCompactMathFont(segment.Run.FontName) &&
            segment.Role == InlineBaselineRole.Subscript;
    }

    private static bool IsMathAttachmentSpaceSegment(IReadOnlyList<InlineTextSegment> segments, int index)
    {
        InlineTextSegment segment = segments[index];
        if (!string.IsNullOrWhiteSpace(segment.Text))
        {
            return false;
        }

        InlineTextSegment? previous = NearestTextSegment(segments, index, -1);
        InlineTextSegment? next = NearestTextSegment(segments, index, 1);
        return previous is { Run: { } previousRun } &&
            next is { Run: { } nextRun } nextSegment &&
            HasMathFont(previousRun.FontName) &&
            HasMathFont(nextRun.FontName) &&
            nextSegment.Role == InlineBaselineRole.Subscript;
    }

    private static InlineTextSegment? NearestTextSegment(
        IReadOnlyList<InlineTextSegment> segments,
        int index,
        int step)
    {
        for (int candidate = index + step; candidate >= 0 && candidate < segments.Count; candidate += step)
        {
            if (!string.IsNullOrWhiteSpace(segments[candidate].Text))
            {
                return segments[candidate];
            }
        }

        return null;
    }

    private static void WriteInlineTextSegment(
        StringBuilder html,
        PdfSemanticLine line,
        InlineTextSegment segment,
        string lineText,
        int offset,
        FootnoteContext footnotes)
    {
        if (segment.Run == null)
        {
            if (segment.Role == InlineBaselineRole.Normal)
            {
                WriteTextWithFootnoteReferences(html, segment.Text, footnotes, lineText, offset);
            }
            else
            {
                WriteStyledInlineTextSegment(html, segment.Text, "", segment.Role, footnotes, lineText, offset);
            }

            return;
        }

        if (IsFootnoteReferenceSegment(segment, lineText, offset, footnotes))
        {
            WriteTextWithFootnoteReferences(html, segment.Text, footnotes, lineText, offset);
            return;
        }

        string className = InlineRunClassNames(line, segment.Run);
        WriteStyledInlineTextSegment(html, segment.Text, className, segment.Role, footnotes, lineText, offset);
    }

    private static void WriteInlineTextSegmentAsRole(
        StringBuilder html,
        PdfSemanticLine line,
        InlineTextSegment segment,
        InlineBaselineRole role)
    {
        string className = segment.Run == null ? "" : InlineRunClassNames(line, segment.Run);
        WriteStyledInlineTextSegment(html, segment.Text, className, role);
    }

    private static void WriteStyledInlineTextSegment(
        StringBuilder html,
        string text,
        string className,
        InlineBaselineRole role,
        FootnoteContext? footnotes = null,
        string? lineText = null,
        int offset = 0)
    {
        string tagName = role switch
        {
            InlineBaselineRole.Subscript => "sub",
            InlineBaselineRole.Superscript => "sup",
            _ => className.Length > 0 ? "span" : ""
        };

        if (tagName.Length > 0)
        {
            html.Append('<')
                .Append(tagName);
            if (className.Length > 0)
            {
                html.Append(" class=\"")
                    .Append(className)
                    .Append('"');
            }

            html.Append('>');
        }

        if (footnotes != null && lineText != null)
        {
            WriteTextWithFootnoteReferences(html, text, footnotes, lineText, offset);
        }
        else
        {
            html.Append(Html(text));
        }

        if (tagName.Length > 0)
        {
            html.Append("</")
                .Append(tagName)
                .Append('>');
        }
    }

    private static bool IsFootnoteReferenceSegment(
        InlineTextSegment segment,
        string lineText,
        int offset,
        FootnoteContext footnotes)
    {
        return segment.Text.Length == 1 &&
            segment.Role == InlineBaselineRole.Superscript &&
            footnotes.Contains(segment.Text) &&
            IsFootnoteReferenceBoundary(lineText, offset);
    }

    private static string InlineRunClassNames(PdfSemanticLine line, PdfTextRun run)
    {
        List<string> classes = [];
        string normalizedFontName = NormalizeFontName(run.FontName);
        float fontSize = MathF.Round(run.FontSize * 2f) / 2f;
        if (!string.Equals(normalizedFontName, line.DominantFontName, StringComparison.Ordinal) ||
            HasMathFont(normalizedFontName) ||
            IsItalicFont(normalizedFontName))
        {
            classes.Add("pdf-semantic-inline-run");
            classes.Add(FontClass(normalizedFontName));
        }

        if (MathF.Abs(fontSize - line.DominantFontSize) > 0.25f)
        {
            if (classes.Count == 0)
            {
                classes.Add("pdf-semantic-inline-run");
            }

            classes.Add(FontSizeClass(fontSize));
        }

        if (!string.Equals(ColorClass(run.Color), ColorClass(line.Color), StringComparison.Ordinal))
        {
            if (classes.Count == 0)
            {
                classes.Add("pdf-semantic-inline-run");
            }

            classes.Add(ColorClass(run.Color));
        }

        if (HasMathFont(normalizedFontName))
        {
            classes.Add("pdf-semantic-math");
        }

        if (IsItalicFont(normalizedFontName))
        {
            classes.Add("pdf-semantic-italic");
        }

        return string.Join(" ", classes.Distinct(StringComparer.Ordinal));
    }

    private static void WriteFootnote(
        StringBuilder html,
        PdfSemanticElement element,
        FootnoteContext footnotes,
        PdfLayoutPage? page)
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
        WriteFootnoteBody(html, element, marker, body, footnotes, page);
        html.AppendLine("</p>");
    }

    private static void WriteFootnoteBody(
        StringBuilder html,
        PdfSemanticElement element,
        string marker,
        string plainBody,
        FootnoteContext footnotes,
        PdfLayoutPage? page)
    {
        if (CanWriteRichSemanticText(element))
        {
            WriteRichSemanticText(html, element, footnotes, page, marker);
            return;
        }

        html.Append(Html(plainBody));
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

        if (IsFormulaBlock(element))
        {
            classes.Add("pdf-semantic-formula");
        }

        if (element.Kind == PdfSemanticElementKind.Paragraph && page != null)
        {
            if (!IsFormulaBlock(element) && IsJustifiedParagraph(element))
            {
                classes.Add("pdf-semantic-justified");
            }

            if (!IsFormulaBlock(element) &&
                allowMeasuredWidth &&
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
        float[] widths = RepresentativeTextRows(element)
            .Select(static row => row.Width)
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

    private static IEnumerable<PdfLayoutRectangle> RepresentativeTextRows(PdfSemanticElement element)
    {
        List<PdfLayoutRectangle> rows = [];
        foreach (PdfSemanticLine line in element.Lines
            .Where(static line => MathF.Abs(line.Direction) < 0.01f)
            .Where(static line => !string.IsNullOrWhiteSpace(line.Text))
            .OrderBy(static line => line.Bounds.Y)
            .ThenBy(static line => line.Bounds.X))
        {
            int rowIndex = rows.FindIndex(row => BelongsToTextRow(row, line.Bounds));
            if (rowIndex < 0)
            {
                rows.Add(line.Bounds);
            }
            else
            {
                rows[rowIndex] = UnionRectangles([rows[rowIndex], line.Bounds]);
            }
        }

        return rows;
    }

    private static bool BelongsToTextRow(PdfLayoutRectangle row, PdfLayoutRectangle candidate)
    {
        float overlap = MathF.Min(row.Bottom, candidate.Bottom) - MathF.Max(row.Y, candidate.Y);
        if (overlap >= MathF.Min(row.Height, candidate.Height) * 0.35f)
        {
            return true;
        }

        float centerDistance = MathF.Abs(
            row.Y + (row.Height / 2f) - (candidate.Y + (candidate.Height / 2f)));
        return centerDistance <= MathF.Max(2.5f, MathF.Max(row.Height, candidate.Height) * 0.55f);
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

    private static bool IsFormulaBlock(PdfSemanticElement element)
    {
        return element.Kind == PdfSemanticElementKind.Paragraph &&
            !IsFigureCaption(element) &&
            !IsInlineFormulaFragment(element) &&
            (element.Lines.Any(IsDisplayFormulaLine) ||
                IsCompactCenteredFormulaElement(element));
    }

    private static bool IsCompactCenteredFormulaElement(PdfSemanticElement element)
    {
        PdfSemanticLine[] lines = HorizontalTextLines(element);
        if (lines.Length == 0 ||
            lines.Length > 6 ||
            element.Text.Length > 220 ||
            element.Bounds.Height > 90f ||
            element.Bounds.X < 100f ||
            element.Bounds.Width is < 90f or > 430f)
        {
            return false;
        }

        string text = element.Text.Trim();
        return text.Contains('=') &&
            HasFormulaSignal(text) &&
            HasMathFont(SemanticFontName(element)) &&
            CountWords(text) <= 14;
    }

    private static bool IsInlineFormulaFragment(PdfSemanticElement element)
    {
        return element.Text.Length <= 48 &&
            element.Bounds.Width <= 90f &&
            !element.Text.Contains('=') &&
            !element.Text.Contains('∈') &&
            !element.Text.Contains('×') &&
            !element.Text.Contains('√') &&
            !element.Text.Contains('∑') &&
            element.Lines.Any(static line => line.Runs.Any(static run => HasMathFont(run.FontName)));
    }

    private static bool IsDisplayFormulaLine(PdfSemanticLine line)
    {
        if (!HasMathFont(line.DominantFontName) || !HasFormulaSignal(line.Text))
        {
            return false;
        }

        if (HasFormulaFunction(line.Text))
        {
            return line.Text.IndexOf('=') >= 0 ||
                line.Bounds.Width >= 80f &&
                (StartsFormulaFunction(line.Text) || CountWords(line.Text) <= 4);
        }

        return line.Bounds.X >= 150f &&
            line.Bounds.Width >= 80f &&
            CountWords(line.Text) <= 4;
    }

    private static bool HasMathFont(string fontName)
    {
        string normalized = NormalizeFontName(fontName);
        return normalized.StartsWith("CM", StringComparison.Ordinal) ||
            normalized.Contains("MSBM", StringComparison.Ordinal);
    }

    private static bool IsCompactMathFont(string fontName)
    {
        string normalized = NormalizeFontName(fontName);
        return normalized.StartsWith("CMR7", StringComparison.Ordinal) ||
            normalized.StartsWith("CMMI7", StringComparison.Ordinal) ||
            normalized.StartsWith("CMSY7", StringComparison.Ordinal) ||
            normalized.StartsWith("CMR6", StringComparison.Ordinal) ||
            normalized.StartsWith("CMMI6", StringComparison.Ordinal) ||
            normalized.StartsWith("CMSY6", StringComparison.Ordinal) ||
            normalized.StartsWith("CMR5", StringComparison.Ordinal) ||
            normalized.StartsWith("CMMI5", StringComparison.Ordinal) ||
            normalized.StartsWith("CMSY5", StringComparison.Ordinal);
    }

    private static bool IsItalicFont(string fontName)
    {
        string normalized = NormalizeFontName(fontName);
        return normalized.Contains("Italic", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("Ital", StringComparison.OrdinalIgnoreCase) ||
            normalized.StartsWith("CMMI", StringComparison.Ordinal);
    }

    private static bool HasFormulaSignal(string text)
    {
        return text.IndexOfAny(['=', '∈', '×', '√', '∑', '·']) >= 0 ||
            HasFormulaFunction(text);
    }

    private static bool HasFormulaFunction(string text)
    {
        return
            text.Contains("Attention(", StringComparison.Ordinal) ||
            text.Contains("MultiHead(", StringComparison.Ordinal) ||
            text.Contains("Concat(", StringComparison.Ordinal) ||
            text.Contains("FFN(", StringComparison.Ordinal) ||
            text.Contains("PE", StringComparison.Ordinal);
    }

    private static bool StartsFormulaFunction(string text)
    {
        string trimmed = text.TrimStart();
        return
            trimmed.StartsWith("Attention(", StringComparison.Ordinal) ||
            trimmed.StartsWith("MultiHead(", StringComparison.Ordinal) ||
            trimmed.StartsWith("Concat(", StringComparison.Ordinal) ||
            trimmed.StartsWith("FFN(", StringComparison.Ordinal) ||
            trimmed.StartsWith("PE", StringComparison.Ordinal);
    }

    private static int CountWords(string text)
    {
        return text
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Length;
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
            PdfSemanticElementKind.Table => "pdf-semantic-table",
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
        WriteTextWithFootnoteReferences(html, text, footnotes, text, 0);
    }

    private static void WriteTextWithFootnoteReferences(
        StringBuilder html,
        string text,
        FootnoteContext footnotes,
        string boundaryText,
        int boundaryOffset)
    {
        for (int index = 0; index < text.Length; index++)
        {
            string marker = text[index].ToString();
            int boundaryIndex = boundaryOffset + index;
            if (footnotes.Contains(marker) &&
                boundaryIndex >= 0 &&
                boundaryIndex < boundaryText.Length &&
                IsFootnoteReferenceBoundary(boundaryText, boundaryIndex))
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

    private enum SemanticFigureRendering
    {
        None,
        Space,
        Content
    }

    private enum InlineBaselineRole
    {
        Normal,
        Subscript,
        Superscript
    }

    private readonly record struct InlineTextSegment(
        string Text,
        PdfTextRun? Run,
        InlineBaselineRole Role);

    private sealed class ContinuousPageContext
    {
        public ContinuousPageContext(
            PdfLayoutPage page,
            PdfSemanticPage semanticPage,
            FootnoteContext footnotes,
            IReadOnlyList<PdfSemanticElement> positionedElements,
            IReadOnlyList<PdfSemanticElement> flowElements,
            IReadOnlyList<PdfLayoutRectangle> figureRegions)
        {
            Page = page;
            SemanticPage = semanticPage;
            Footnotes = footnotes;
            PositionedElements = positionedElements;
            FlowElements = flowElements;
            FigureRegions = figureRegions;
        }

        public PdfLayoutPage Page { get; }

        public PdfSemanticPage SemanticPage { get; }

        public FootnoteContext Footnotes { get; }

        public IReadOnlyList<PdfSemanticElement> PositionedElements { get; }

        public IReadOnlyList<PdfSemanticElement> FlowElements { get; }

        public IReadOnlyList<PdfLayoutRectangle> FigureRegions { get; }
    }

    private sealed class ContinuousParagraphMerge
    {
        public ContinuousParagraphMerge(
            ContinuousPageContext current,
            ContinuousPageContext next,
            PdfSemanticElement startElement,
            PdfSemanticElement continuationElement,
            PdfSemanticElement? currentPageNumberFooter,
            IReadOnlyList<PdfSemanticElement> currentTrailingFootnotes,
            IReadOnlyList<PdfSemanticElement> leadingElements,
            IReadOnlyList<PdfLayoutRectangle> leadingFigureRegions)
        {
            Current = current;
            Next = next;
            StartElement = startElement;
            ContinuationElement = continuationElement;
            CurrentPageNumberFooter = currentPageNumberFooter;
            CurrentTrailingFootnotes = currentTrailingFootnotes;
            LeadingElements = leadingElements;
            LeadingFigureRegions = leadingFigureRegions;
        }

        public ContinuousPageContext Current { get; }

        public ContinuousPageContext Next { get; }

        public PdfSemanticElement StartElement { get; }

        public PdfSemanticElement ContinuationElement { get; }

        public PdfSemanticElement? CurrentPageNumberFooter { get; }

        public IReadOnlyList<PdfSemanticElement> CurrentTrailingFootnotes { get; }

        public IReadOnlyList<PdfSemanticElement> LeadingElements { get; }

        public IReadOnlyList<PdfLayoutRectangle> LeadingFigureRegions { get; }
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
