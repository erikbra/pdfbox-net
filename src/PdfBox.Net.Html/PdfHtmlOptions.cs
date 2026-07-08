namespace PdfBox.Net.Html;

/// <summary>
/// Options for fixed-layout HTML conversion.
/// </summary>
public sealed class PdfHtmlOptions
{
    /// <summary>
    /// Gets or sets the generated document title.
    /// </summary>
    public string Title { get; init; } = "PDF document";

    /// <summary>
    /// Gets or sets the CSS asset path referenced by the generated HTML.
    /// </summary>
    public string CssPath { get; init; } = "assets/pdfbox-net-fixed.css";

    /// <summary>
    /// Gets or sets the scalar applied to emitted page and text coordinates.
    /// </summary>
    public float Scale { get; init; } = 1.0f;
}
