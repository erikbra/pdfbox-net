namespace PdfBox.Net.Layout;

/// <summary>
/// Semantic document structure inferred from low-level page layout data.
/// </summary>
public sealed class PdfSemanticDocument
{
    public PdfSemanticDocument(IReadOnlyList<PdfSemanticPage> pages)
    {
        Pages = pages.ToArray();
    }

    public IReadOnlyList<PdfSemanticPage> Pages { get; }

    public IReadOnlyList<PdfSemanticElement> Elements => Pages.SelectMany(page => page.Elements).ToArray();
}

/// <summary>
/// Semantic elements inferred for a single page.
/// </summary>
public sealed class PdfSemanticPage
{
    public PdfSemanticPage(int pageNumber, IReadOnlyList<PdfSemanticElement> elements)
    {
        PageNumber = pageNumber;
        Elements = elements.ToArray();
    }

    public int PageNumber { get; }

    public IReadOnlyList<PdfSemanticElement> Elements { get; }
}

/// <summary>
/// A semantic text element such as a heading, paragraph, author cell, footnote, or footer.
/// </summary>
public sealed class PdfSemanticElement
{
    public PdfSemanticElement(
        PdfSemanticElementKind kind,
        string text,
        PdfLayoutRectangle bounds,
        IReadOnlyList<PdfSemanticLine> lines,
        int headingLevel = 0)
    {
        Kind = kind;
        Text = text;
        Bounds = bounds;
        Lines = lines.ToArray();
        HeadingLevel = headingLevel;
    }

    public PdfSemanticElementKind Kind { get; }

    public string Text { get; }

    public PdfLayoutRectangle Bounds { get; }

    public IReadOnlyList<PdfSemanticLine> Lines { get; }

    public int HeadingLevel { get; }
}

/// <summary>
/// A reconstructed text line used by semantic grouping.
/// </summary>
public sealed class PdfSemanticLine
{
    public PdfSemanticLine(
        string text,
        PdfLayoutRectangle bounds,
        string dominantFontName,
        float dominantFontSize,
        IReadOnlyList<PdfTextRun> runs)
    {
        Text = text;
        Bounds = bounds;
        DominantFontName = dominantFontName;
        DominantFontSize = dominantFontSize;
        Runs = runs.ToArray();
    }

    public string Text { get; }

    public PdfLayoutRectangle Bounds { get; }

    public string DominantFontName { get; }

    public float DominantFontSize { get; }

    public IReadOnlyList<PdfTextRun> Runs { get; }
}
