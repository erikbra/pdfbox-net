using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.PDModel.Graphics.Color;

/// <summary>
/// Pattern color space for uncolored tiling patterns, where color components are supplied at paint time.
/// </summary>
public sealed class PDUncoloredTilingPattern : PDPattern
{
    /// <summary>
    /// Creates an uncolored tiling pattern color space with the given underlying color space.
    /// </summary>
    /// <param name="underlyingColorSpace">The underlying color space used for supplied color components.</param>
    public PDUncoloredTilingPattern(PDColorSpace underlyingColorSpace)
        : this(null, underlyingColorSpace)
    {
    }

    /// <summary>
    /// Creates an uncolored tiling pattern color space with resources and an underlying color space.
    /// </summary>
    /// <param name="resources">Optional resources associated with the pattern color space.</param>
    /// <param name="underlyingColorSpace">The underlying color space used for supplied color components.</param>
    public PDUncoloredTilingPattern(PDResources? resources, PDColorSpace underlyingColorSpace)
        : base(resources, underlyingColorSpace ?? throw new ArgumentNullException(nameof(underlyingColorSpace)))
    {
    }
}
