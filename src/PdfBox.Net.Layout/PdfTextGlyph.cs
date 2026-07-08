namespace PdfBox.Net.Layout;

/// <summary>
/// Positioned text item captured from the PDF content stream.
/// </summary>
public sealed record PdfTextGlyph(
    string Text,
    string FontName,
    float FontSize,
    float Direction,
    PdfLayoutRectangle Bounds);
