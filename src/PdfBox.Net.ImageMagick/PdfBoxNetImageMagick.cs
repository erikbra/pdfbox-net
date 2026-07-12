/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 */

using PdfBox.Net.Filter;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.Image;

namespace PdfBox.Net.ImageMagick;

public static class PdfBoxNetImageMagick
{
    public static void Register()
    {
        PdfBoxNetImageServices.Register(
            jpegRasterDecoder: MagickJpegRasterDecoder.Instance,
            jpxRasterDecoder: MagickJpxRasterDecoder.Instance,
            iccColorTransformFactory: MagickIccColorTransformFactory.Instance,
            tiffRasterDecoder: MagickTiffRasterDecoder.Instance);
    }
}
