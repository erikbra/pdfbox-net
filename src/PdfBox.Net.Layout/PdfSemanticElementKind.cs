namespace PdfBox.Net.Layout;

/// <summary>
/// Coarse semantic role inferred from positioned PDF layout content.
/// </summary>
public enum PdfSemanticElementKind
{
    Other,
    Header,
    Heading,
    Paragraph,
    Table,
    AuthorBlock,
    Footnote,
    Footer,
    FrontMatter
}
