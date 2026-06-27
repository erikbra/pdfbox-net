/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * SkiaSharp image factory adapters.
 *
 * PORT_MODE: native-adapter
 */

using PdfBox.Net.Rendering;
using SkiaSharp;

namespace PdfBox.Net.PDModel.Graphics.Image;

public static class SkiaLosslessFactory
{
    public static PDImageXObject CreateFromImage(PDDocument document, SKBitmap bitmap)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(bitmap);
        using BufferedImage image = SkiaImageCodecPeer.CreateBufferedImage(bitmap);
        return LosslessFactory.CreateFromImage(document, image);
    }
}
