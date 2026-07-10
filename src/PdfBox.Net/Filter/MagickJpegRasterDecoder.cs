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

            if (UsesAdobeInvertedCmyk(jpegBytes))
            {
                InvertCmykSamples(samples);
            }

            return new DecodedJpegRaster(checked((int)image.Width), checked((int)image.Height), 4, samples);
        }
        catch (MagickException ex)
        {
            throw new IOException("DCTDecode failed to decode CMYK/YCCK JPEG data.", ex);
        }
    }

    private static bool UsesAdobeInvertedCmyk(ReadOnlySpan<byte> jpegBytes)
    {
        for (int index = 2; index + 4 < jpegBytes.Length;)
        {
            if (jpegBytes[index] != 0xFF)
            {
                index++;
                continue;
            }

            while (index < jpegBytes.Length && jpegBytes[index] == 0xFF)
            {
                index++;
            }

            if (index >= jpegBytes.Length)
            {
                return false;
            }

            byte marker = jpegBytes[index++];
            if (marker is 0xD8 or 0xD9 or >= 0xD0 and <= 0xD7)
            {
                continue;
            }

            if (index + 2 > jpegBytes.Length)
            {
                return false;
            }

            int length = (jpegBytes[index] << 8) | jpegBytes[index + 1];
            if (length < 2 || index + length > jpegBytes.Length)
            {
                return false;
            }

            int data = index + 2;
            if (marker == 0xEE && length >= 14 &&
                jpegBytes[data] == (byte)'A' &&
                jpegBytes[data + 1] == (byte)'d' &&
                jpegBytes[data + 2] == (byte)'o' &&
                jpegBytes[data + 3] == (byte)'b' &&
                jpegBytes[data + 4] == (byte)'e')
            {
                byte transform = jpegBytes[data + 11];
                return transform is 0 or 2;
            }

            index += length;
        }

        return false;
    }

    private static void InvertCmykSamples(Span<byte> samples)
    {
        for (int index = 0; index < samples.Length; index++)
        {
            samples[index] = (byte)(255 - samples[index]);
        }
    }
}
