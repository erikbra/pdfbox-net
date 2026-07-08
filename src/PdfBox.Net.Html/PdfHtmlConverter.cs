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
        StringBuilder html = new();
        html.AppendLine("<!doctype html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("  <meta charset=\"utf-8\" />");
        html.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
        html.Append("  <title>").Append(Html(options.Title)).AppendLine("</title>");
        html.Append("  <link rel=\"stylesheet\" href=\"").Append(HtmlAttribute(cssPath)).AppendLine("\" />");
        html.AppendLine("</head>");
        html.AppendLine("<body class=\"pdf-document\">");

        foreach (PdfLayoutPage page in layout.Pages)
        {
            WritePage(html, page, imageAssets, options.Scale);
        }

        html.AppendLine("</body>");
        html.AppendLine("</html>");
        PdfHtmlAsset[] assets = imageAssets.Values
            .Select(asset => new PdfHtmlAsset(asset.RelativePath, asset.ContentType, asset.Data))
            .ToArray();
        return new PdfHtmlDocument(html.ToString(), cssPath, Css, assets);
    }

    private static void WritePage(
        StringBuilder html,
        PdfLayoutPage page,
        IReadOnlyDictionary<string, PdfLayoutImageAsset> imageAssets,
        float scale)
    {
        html.Append("  <section class=\"pdf-page\" data-page-number=\"")
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
            WriteVectorLayer(html, page, scale);
        }

        foreach (PdfTextRun run in page.Runs)
        {
            WriteTextRun(html, run, scale);
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

    private static void WriteVectorLayer(StringBuilder html, PdfLayoutPage page, float scale)
    {
        html.Append("    <svg class=\"pdf-vector-layer\" data-path-count=\"")
            .Append(page.Paths.Count.ToString(CultureInfo.InvariantCulture))
            .Append("\" viewBox=\"0 0 ")
            .Append(SvgNumber(page.Width))
            .Append(' ')
            .Append(SvgNumber(page.Height))
            .Append("\" style=\"position:absolute;left:0;top:0;width:")
            .Append(CssPoints(page.Width * scale))
            .Append(";height:")
            .Append(CssPoints(page.Height * scale))
            .AppendLine("\" aria-hidden=\"true\">");

        foreach (PdfLayoutPath path in page.Paths)
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
            .Append("\">")
            .Append(Html(run.Text))
            .AppendLine("</span>");
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

        string escaped = fontName.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("'", "\\'", StringComparison.Ordinal);
        return "'" + escaped + "', sans-serif";
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
