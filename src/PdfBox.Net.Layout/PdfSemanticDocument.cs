namespace PdfBox.Net.Layout;

/// <summary>
/// Semantic document structure inferred from low-level page layout data.
/// </summary>
public sealed class PdfSemanticDocument
{
    public PdfSemanticDocument(IReadOnlyList<PdfSemanticPage> pages)
    {
        Pages = pages.ToArray();
        SectionTree = PdfSemanticSectionTree.Create(Pages);
    }

    public IReadOnlyList<PdfSemanticPage> Pages { get; }

    public IReadOnlyList<PdfSemanticElement> Elements => Pages.SelectMany(page => page.Elements).ToArray();

    /// <summary>
    /// Gets the deterministic section hierarchy inferred from document headings.
    /// </summary>
    public PdfSemanticSectionTree SectionTree { get; }
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
        int headingLevel = 0,
        IReadOnlyList<PdfSemanticTableRow>? tableRows = null,
        PdfSemanticList? semanticList = null,
        PdfSemanticDocumentIndex? documentIndex = null,
        bool isDocumentTitle = false,
        PdfSemanticBibliographyFragment? bibliographyFragment = null)
    {
        Kind = kind;
        Text = text;
        Bounds = bounds;
        Lines = lines.ToArray();
        HeadingLevel = headingLevel;
        TableRows = tableRows?.ToArray() ?? [];
        SemanticList = semanticList;
        DocumentIndex = documentIndex;
        BibliographyFragment = bibliographyFragment;
        IsDocumentTitle = isDocumentTitle;
    }

    public PdfSemanticElementKind Kind { get; }

    public string Text { get; }

    public PdfLayoutRectangle Bounds { get; }

    public IReadOnlyList<PdfSemanticLine> Lines { get; }

    public int HeadingLevel { get; }

    public IReadOnlyList<PdfSemanticTableRow> TableRows { get; }

    public PdfSemanticList? SemanticList { get; }

    public PdfSemanticDocumentIndex? DocumentIndex { get; }

    public PdfSemanticBibliographyFragment? BibliographyFragment { get; }

    /// <summary>
    /// Gets whether this heading is the inferred document title rather than a section heading.
    /// </summary>
    public bool IsDocumentTitle { get; }
}

/// <summary>
/// A row in an inferred semantic table.
/// </summary>
public sealed class PdfSemanticTableRow
{
    public PdfSemanticTableRow(IReadOnlyList<PdfSemanticTableCell> cells, bool isHeader)
    {
        Cells = cells.ToArray();
        IsHeader = isHeader;
    }

    public IReadOnlyList<PdfSemanticTableCell> Cells { get; }

    public bool IsHeader { get; }
}

/// <summary>
/// A cell in an inferred semantic table.
/// </summary>
public sealed class PdfSemanticTableCell
{
    public PdfSemanticTableCell(
        string text,
        PdfLayoutRectangle bounds,
        IReadOnlyList<PdfSemanticLine> lines,
        bool borderTop = false,
        bool borderRight = false,
        bool borderBottom = false,
        bool borderLeft = false,
        int rowSpan = 1,
        int columnSpan = 1,
        bool isPlaceholder = false)
    {
        Text = text;
        Bounds = bounds;
        Lines = lines.ToArray();
        BorderTop = borderTop;
        BorderRight = borderRight;
        BorderBottom = borderBottom;
        BorderLeft = borderLeft;
        RowSpan = Math.Max(1, rowSpan);
        ColumnSpan = Math.Max(1, columnSpan);
        IsPlaceholder = isPlaceholder;
    }

    public string Text { get; }

    public PdfLayoutRectangle Bounds { get; }

    public IReadOnlyList<PdfSemanticLine> Lines { get; }

    public bool BorderTop { get; }

    public bool BorderRight { get; }

    public bool BorderBottom { get; }

    public bool BorderLeft { get; }

    public int RowSpan { get; }

    public int ColumnSpan { get; }

    public bool IsPlaceholder { get; }
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
        float direction,
        PdfLayoutColor color,
        IReadOnlyList<PdfTextRun> runs)
    {
        Text = text;
        Bounds = bounds;
        DominantFontName = dominantFontName;
        DominantFontSize = dominantFontSize;
        Direction = direction;
        Color = color;
        Runs = runs.ToArray();
    }

    public string Text { get; }

    public PdfLayoutRectangle Bounds { get; }

    public string DominantFontName { get; }

    public float DominantFontSize { get; }

    public float Direction { get; }

    public PdfLayoutColor Color { get; }

    public IReadOnlyList<PdfTextRun> Runs { get; }
}
