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
            WritePage(html, page, options.Scale);
        }

        html.AppendLine("</body>");
        html.AppendLine("</html>");
        return new PdfHtmlDocument(html.ToString(), cssPath, Css);
    }

    private static void WritePage(StringBuilder html, PdfLayoutPage page, float scale)
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
