using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.PDModel.Graphics.Color;

/// <summary>
/// Pattern color space for colored tiling patterns, where the pattern content stream sets its own color values.
/// </summary>
public sealed class PDColoredTilingPattern : PDPattern
{
    /// <summary>
    /// Creates a colored tiling pattern color space.
    /// </summary>
    /// <param name="resources">Optional resources associated with the pattern color space.</param>
    public PDColoredTilingPattern(PDResources? resources = null)
        : base(resources)
    {
    }
}
