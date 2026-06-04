using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.PDModel.Graphics.Color;

public sealed class PDUncoloredTilingPattern : PDPattern
{
    public PDUncoloredTilingPattern(PDColorSpace underlyingColorSpace)
        : this(null, underlyingColorSpace)
    {
    }

    public PDUncoloredTilingPattern(PDResources? resources, PDColorSpace underlyingColorSpace)
        : base(resources, underlyingColorSpace ?? throw new ArgumentNullException(nameof(underlyingColorSpace)))
    {
    }
}
