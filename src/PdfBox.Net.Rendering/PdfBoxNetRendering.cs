/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 */

using PdfBox.Net.ImageMagick;

namespace PdfBox.Net.Rendering;

public static class PdfBoxNetRendering
{
    public static void Register()
    {
        SkiaRenderingBackend.Register();
        PdfBoxNetImageMagick.Register();
    }
}
