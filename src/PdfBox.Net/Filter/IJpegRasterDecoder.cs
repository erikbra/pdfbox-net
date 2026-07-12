/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 */

namespace PdfBox.Net.Filter;

internal interface IJpegRasterDecoder
{
    DecodedJpegRaster Decode(byte[] jpegBytes);
}

internal readonly record struct DecodedJpegRaster(int Width, int Height, int Components, byte[] Samples);

internal sealed class MissingJpegRasterDecoder : IJpegRasterDecoder
{
    internal static readonly MissingJpegRasterDecoder Instance = new();

    private MissingJpegRasterDecoder()
    {
    }

    public DecodedJpegRaster Decode(byte[] jpegBytes)
    {
        throw new NotSupportedException(
            "DCTDecode for 4-component JPEG data requires the optional PdfBox.Net.ImageMagick package. " +
            "Reference PdfBox.Net.ImageMagick or PdfBox.Net.Rendering and call its registration method before decoding CMYK/YCCK JPEG images.");
    }
}
