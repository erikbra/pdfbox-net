namespace PdfBox.Net.Layout;

/// <summary>
/// Layout data extracted from a PDF document.
/// </summary>
public sealed class PdfLayoutDocument
{
    public PdfLayoutDocument(IReadOnlyList<PdfLayoutPage> pages, IReadOnlyList<PdfLayoutDiagnostic> diagnostics)
    {
        Pages = pages.ToArray();
        Diagnostics = diagnostics.ToArray();
    }

    /// <summary>
    /// Gets the extracted pages.
    /// </summary>
    public IReadOnlyList<PdfLayoutPage> Pages { get; }

    /// <summary>
    /// Gets document-level diagnostics.
    /// </summary>
    public IReadOnlyList<PdfLayoutDiagnostic> Diagnostics { get; }

    /// <summary>
    /// Gets extracted text in page order.
    /// </summary>
    public string Text => string.Join(Environment.NewLine, Pages.Select(page => page.Text).Where(text => text.Length > 0));
}
