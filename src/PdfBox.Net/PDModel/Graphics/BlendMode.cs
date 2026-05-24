/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted blend mode enum for extended graphics state support.
 *
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Graphics;

public enum BlendMode
{
    NORMAL,
    MULTIPLY,
    SCREEN,
    OVERLAY,
    DARKEN,
    LIGHTEN
}

internal static class BlendModeExtensions
{
    public static BlendMode FromCos(COSBase? baseValue)
    {
        string? name = baseValue switch
        {
            COSName cosName => cosName.GetName(),
            COSArray array when array.Size() > 0 && array.GetObject(0) is COSName cosName => cosName.GetName(),
            _ => null
        };

        return name?.ToLowerInvariant() switch
        {
            "multiply" => BlendMode.MULTIPLY,
            "screen" => BlendMode.SCREEN,
            "overlay" => BlendMode.OVERLAY,
            "darken" => BlendMode.DARKEN,
            "lighten" => BlendMode.LIGHTEN,
            _ => BlendMode.NORMAL
        };
    }

    public static COSName ToCosName(this BlendMode mode)
    {
        return mode switch
        {
            BlendMode.MULTIPLY => COSName.GetPDFName("Multiply"),
            BlendMode.SCREEN => COSName.GetPDFName("Screen"),
            BlendMode.OVERLAY => COSName.GetPDFName("Overlay"),
            BlendMode.DARKEN => COSName.GetPDFName("Darken"),
            BlendMode.LIGHTEN => COSName.GetPDFName("Lighten"),
            _ => COSName.GetPDFName("Normal")
        };
    }
}
