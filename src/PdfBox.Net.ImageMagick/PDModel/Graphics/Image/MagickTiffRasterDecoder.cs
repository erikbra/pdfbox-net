/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 */

using ImageMagick;

namespace PdfBox.Net.PDModel.Graphics.Image;

internal sealed class MagickTiffRasterDecoder : ITiffRasterDecoder
{
    internal static readonly MagickTiffRasterDecoder Instance = new();

    private MagickTiffRasterDecoder()
    {
    }

    public DecodedTiffRaster DecodeOneBitRows(Stream stream)
    {
        using MagickImage image = ReadTiff(stream);
        byte[] oneBitRows = ExtractOneBitRows(image, out int width, out int height);
        return new DecodedTiffRaster(width, height, oneBitRows);
    }

    private static MagickImage ReadTiff(Stream stream)
    {
        try
        {
            return new MagickImage(stream, MagickFormat.Tiff);
        }
        catch (MagickException ex)
        {
            throw new IOException("Unable to read CCITT TIFF image data.", ex);
        }
    }

    private static byte[] ExtractOneBitRows(MagickImage image, out int width, out int height)
    {
        width = checked((int)image.Width);
        height = checked((int)image.Height);
        if (width <= 0 || height <= 0)
        {
            throw new IOException("Invalid CCITT image dimensions.");
        }

        image.ColorSpace = ColorSpace.Gray;
        using IPixelCollection<byte> pixels = image.GetPixels();
        byte[] gray = pixels.ToByteArray("I")
            ?? throw new IOException("Unable to extract grayscale TIFF pixels.");

        int rowBytes = (width + 7) / 8;
        byte[] packed = new byte[checked(rowBytes * height)];
        int src = 0;
        for (int y = 0; y < height; y++)
        {
            int rowOffset = y * rowBytes;
            for (int x = 0; x < width; x++)
            {
                if (gray[src++] < 128)
                {
                    packed[rowOffset + (x / 8)] |= (byte)(0x80 >> (x % 8));
                }
            }
        }

        return packed;
    }
}
