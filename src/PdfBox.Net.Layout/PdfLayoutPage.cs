namespace PdfBox.Net.Layout;

/// <summary>
/// Layout data for one PDF page.
/// </summary>
public sealed class PdfLayoutPage
{
    public PdfLayoutPage(
        int pageNumber,
        PdfLayoutRectangle mediaBox,
        PdfLayoutRectangle cropBox,
        float width,
        float height,
        int rotation,
        IReadOnlyList<PdfTextGlyph> glyphs,
        IReadOnlyList<PdfTextRun> runs,
        IReadOnlyList<PdfTextLine> lines,
        IReadOnlyList<PdfTextBlock> blocks,
        IReadOnlyList<PdfLayoutLink> links,
        IReadOnlyList<PdfLayoutDiagnostic> diagnostics)
    {
        PageNumber = pageNumber;
        MediaBox = mediaBox;
        CropBox = cropBox;
        Width = width;
        Height = height;
        Rotation = rotation;
        Glyphs = glyphs.ToArray();
        Runs = runs.ToArray();
        Lines = lines.ToArray();
        Blocks = blocks.ToArray();
        Links = links.ToArray();
        Diagnostics = diagnostics.ToArray();
    }

    /// <summary>
    /// Gets the one-based page number.
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    /// Gets the page media box from the PDF.
    /// </summary>
    public PdfLayoutRectangle MediaBox { get; }

    /// <summary>
    /// Gets the page crop box from the PDF.
    /// </summary>
    public PdfLayoutRectangle CropBox { get; }

    /// <summary>
    /// Gets the normalized visible page width.
    /// </summary>
    public float Width { get; }

    /// <summary>
    /// Gets the normalized visible page height.
    /// </summary>
    public float Height { get; }

    /// <summary>
    /// Gets the page rotation in degrees.
    /// </summary>
    public int Rotation { get; }

    /// <summary>
    /// Gets positioned text glyphs.
    /// </summary>
    public IReadOnlyList<PdfTextGlyph> Glyphs { get; }

    /// <summary>
    /// Gets text runs.
    /// </summary>
    public IReadOnlyList<PdfTextRun> Runs { get; }

    /// <summary>
    /// Gets text lines.
    /// </summary>
    public IReadOnlyList<PdfTextLine> Lines { get; }

    /// <summary>
    /// Gets text blocks.
    /// </summary>
    public IReadOnlyList<PdfTextBlock> Blocks { get; }

    /// <summary>
    /// Gets link annotations on this page.
    /// </summary>
    public IReadOnlyList<PdfLayoutLink> Links { get; }

    /// <summary>
    /// Gets diagnostics emitted for this page.
    /// </summary>
    public IReadOnlyList<PdfLayoutDiagnostic> Diagnostics { get; }

    /// <summary>
    /// Gets the page text in reading order.
    /// </summary>
    public string Text => string.Join(Environment.NewLine, Lines.Select(line => line.Text));
}
