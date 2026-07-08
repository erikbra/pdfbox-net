namespace PdfBox.Net.Layout;

/// <summary>
/// Options for extracting a page layout model from a PDF document.
/// </summary>
public sealed class PdfLayoutOptions
{
    /// <summary>
    /// Gets or sets whether positioned text should be collected.
    /// </summary>
    public bool IncludeText { get; init; } = true;

    /// <summary>
    /// Gets or sets whether link annotations should be collected.
    /// </summary>
    public bool IncludeLinks { get; init; } = true;

    /// <summary>
    /// Gets or sets whether image placements should be collected.
    /// </summary>
    public bool IncludeImages { get; init; } = true;

    /// <summary>
    /// Gets or sets whether text positions should be sorted into visual reading order.
    /// </summary>
    public bool SortTextByPosition { get; init; } = true;

    /// <summary>
    /// Gets or sets whether duplicate overlapping text should be suppressed by the underlying text stripper.
    /// </summary>
    public bool SuppressDuplicateOverlappingText { get; init; } = true;

    /// <summary>
    /// Gets or sets whether article beads should split text collection.
    /// </summary>
    public bool SeparateByBeads { get; init; }

    /// <summary>
    /// Gets or sets the maximum top-coordinate delta for glyphs to be grouped into the same line.
    /// </summary>
    public float SameLineTolerance { get; init; } = 2.0f;

    /// <summary>
    /// Gets or sets the multiplier applied to a glyph space width before inserting an inferred run boundary.
    /// </summary>
    public float WordSpacingMultiplier { get; init; } = 0.5f;
}
