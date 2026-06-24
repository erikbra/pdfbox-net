/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 */

using ImageMagick;

namespace PdfBox.Net.Filter;

internal sealed class MagickJpegRasterDecoder : IJpegRasterDecoder
{
    public static readonly MagickJpegRasterDecoder Instance = new();

    private MagickJpegRasterDecoder()
    {
    }

    public DecodedJpegRaster Decode(byte[] jpegBytes)
    {
        try
        {
            using MagickImage image = new(jpegBytes, MagickFormat.Jpeg);
            image.ColorSpace = ColorSpace.CMYK;
            using IPixelCollection<byte> pixels = image.GetPixels();
            byte[] samples = pixels.ToByteArray("CMYK")
                ?? throw new IOException("DCTDecode failed to extract CMYK JPEG samples.");

            return new DecodedJpegRaster(checked((int)image.Width), checked((int)image.Height), 4, samples);
        }
        catch (MagickException ex)
        {
            throw new IOException("DCTDecode failed to decode CMYK/YCCK JPEG data.", ex);
        }
    }
}
