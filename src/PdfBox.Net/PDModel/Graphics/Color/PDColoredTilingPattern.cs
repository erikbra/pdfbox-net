using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.PDModel.Graphics.Color;

public sealed class PDColoredTilingPattern : PDPattern
{
    public PDColoredTilingPattern(PDResources? resources = null)
        : base(resources)
    {
    }
}
