namespace PdfBox.Net.Layout;

/// <summary>
/// A browser-representable PDF shading paint operation.
/// </summary>
public sealed class PdfLayoutShading
{
    public PdfLayoutShading(
        int index,
        int shadingType,
        PdfLayoutRectangle bounds,
        float startX,
        float startY,
        float startRadius,
        float endX,
        float endY,
        float endRadius,
        IReadOnlyList<PdfLayoutGradientStop> stops)
    {
        Index = index;
        ShadingType = shadingType;
        Bounds = bounds;
        StartX = startX;
        StartY = startY;
        StartRadius = startRadius;
        EndX = endX;
        EndY = endY;
        EndRadius = endRadius;
        Stops = stops.ToArray();
    }

    /// <summary>
    /// Gets the zero-based paint-operation index on the page.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Gets the PDF shading type. Browser output currently supports axial (2) and radial (3) shadings.
    /// </summary>
    public int ShadingType { get; }

    /// <summary>
    /// Gets the page-space region to paint.
    /// </summary>
    public PdfLayoutRectangle Bounds { get; }

    /// <summary>
    /// Gets the normalized start point in page-space coordinates.
    /// </summary>
    public float StartX { get; }

    /// <summary>
    /// Gets the normalized start point in page-space coordinates.
    /// </summary>
    public float StartY { get; }

    /// <summary>
    /// Gets the start circle radius for radial shadings.
    /// </summary>
    public float StartRadius { get; }

    /// <summary>
    /// Gets the normalized end point in page-space coordinates.
    /// </summary>
    public float EndX { get; }

    /// <summary>
    /// Gets the normalized end point in page-space coordinates.
    /// </summary>
    public float EndY { get; }

    /// <summary>
    /// Gets the end circle radius for radial shadings.
    /// </summary>
    public float EndRadius { get; }

    /// <summary>
    /// Gets sampled color stops from the PDF shading function.
    /// </summary>
    public IReadOnlyList<PdfLayoutGradientStop> Stops { get; }
}

/// <summary>
/// A color sample in a browser-representable PDF shading.
/// </summary>
public readonly record struct PdfLayoutGradientStop(float Offset, PdfLayoutColor Color);
