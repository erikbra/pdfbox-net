namespace PdfBox.Net.Layout;

/// <summary>
/// Consecutive glyphs on a line with compatible text state.
/// </summary>
public sealed class PdfTextRun
{
    public PdfTextRun(
        string text,
        string fontName,
        float fontSize,
        float direction,
        PdfLayoutRectangle bounds,
        PdfLayoutColor color,
        IReadOnlyList<PdfTextGlyph> glyphs,
        PdfLayoutRectangle? pageBounds = null)
    {
        Text = text;
        FontName = fontName;
        FontSize = fontSize;
        Direction = direction;
        Bounds = bounds;
        Color = color;
        Glyphs = glyphs.ToArray();
        PageBounds = pageBounds ?? bounds;
    }

    /// <summary>
    /// Gets the run text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the PDF font name used by the run.
    /// </summary>
    public string FontName { get; }

    /// <summary>
    /// Gets the rendered font size.
    /// </summary>
    public float FontSize { get; }

    /// <summary>
    /// Gets the text direction in degrees.
    /// </summary>
    public float Direction { get; }

    /// <summary>
    /// Gets the run bounds.
    /// </summary>
    public PdfLayoutRectangle Bounds { get; }

    /// <summary>
    /// Gets the run bounds in normalized page coordinates before direction-adjusted reading order normalization.
    /// </summary>
    public PdfLayoutRectangle PageBounds { get; }

    /// <summary>
    /// Gets the resolved fill color used by the run.
    /// </summary>
    public PdfLayoutColor Color { get; }

    /// <summary>
    /// Gets the glyphs that make up the run.
    /// </summary>
    public IReadOnlyList<PdfTextGlyph> Glyphs { get; }
}
