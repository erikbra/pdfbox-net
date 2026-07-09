namespace PdfBox.Net.Layout;

/// <summary>
/// Identifies the PDF image source represented by a layout image.
/// </summary>
public enum PdfLayoutImageKind
{
    /// <summary>
    /// Image XObject invoked through the PDF Do operator.
    /// </summary>
    XObject,

    /// <summary>
    /// Inline image embedded directly in a content stream.
    /// </summary>
    InlineImage,

    /// <summary>
    /// Rasterized annotation appearance placed over the page content.
    /// </summary>
    AnnotationAppearance
}
