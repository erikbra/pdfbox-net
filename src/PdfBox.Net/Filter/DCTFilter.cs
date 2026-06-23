/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/filter/DCTFilter.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using System.Drawing;
using PdfBox.Net.COS;
using SkiaSharp;

namespace PdfBox.Net.Filter;

/// <summary>
/// Decompresses data encoded using a DCT (discrete cosine transform)
/// technique based on the JPEG standard.
/// </summary>
public sealed class DCTFilter : Filter
{
    public override DecodeResult Decode(Stream input, Stream output, COSDictionary parameters, int index, DecodeOptions options)
    {
        byte[] jpegBytes = ReadJpegBytes(input);
        JpegInfo jpegInfo = ParseJpegInfo(jpegBytes);
        if (jpegInfo.Components == 4)
        {
            throw new NotSupportedException("DCTDecode 4-component CMYK/YCCK JPEGs are not yet implemented in PdfBox.Net.");
        }

        if (jpegInfo.Components is not 1 and not 3)
        {
            throw new IOException("Unsupported number of JPEG color components: " + jpegInfo.Components);
        }

        using SKBitmap bitmap = SKBitmap.Decode(jpegBytes)
            ?? throw new IOException("DCTDecode failed to decode JPEG data.");

        Rectangle region = options.GetSourceRegion() ?? new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        region.Intersect(new Rectangle(0, 0, bitmap.Width, bitmap.Height));
        int subsamplingX = Math.Max(1, options.GetSubsamplingX());
        int subsamplingY = Math.Max(1, options.GetSubsamplingY());
        int offsetX = Math.Clamp(options.GetSubsamplingOffsetX(), 0, subsamplingX - 1);
        int offsetY = Math.Clamp(options.GetSubsamplingOffsetY(), 0, subsamplingY - 1);

        for (int y = region.Top + offsetY; y < region.Bottom; y += subsamplingY)
        {
            for (int x = region.Left + offsetX; x < region.Right; x += subsamplingX)
            {
                SKColor color = bitmap.GetPixel(x, y);
                if (jpegInfo.Components == 1)
                {
                    output.WriteByte(color.Red);
                }
                else
                {
                    output.WriteByte(color.Red);
                    output.WriteByte(color.Green);
                    output.WriteByte(color.Blue);
                }
            }
        }

        options.SetFilterSubsampled(true);
        return new DecodeResult(parameters);
    }

    public override void Encode(Stream input, Stream output, COSDictionary parameters, int index)
    {
        throw new NotSupportedException("DCTFilter encoding not implemented, use the JPEGFactory methods instead.");
    }

    private static byte[] ReadJpegBytes(Stream input)
    {
        using MemoryStream memory = new();
        input.CopyTo(memory);
        byte[] data = memory.ToArray();
        if (data.Length > 0 && data[0] == 0x0A)
        {
            byte[] withoutLeadingLf = new byte[data.Length - 1];
            Array.Copy(data, 1, withoutLeadingLf, 0, withoutLeadingLf.Length);
            return withoutLeadingLf;
        }

        return data;
    }

    private static JpegInfo ParseJpegInfo(byte[] data)
    {
        if (data.Length < 4 || data[0] != 0xFF || data[1] != 0xD8)
        {
            throw new IOException("Not a valid JPEG: missing SOI marker (FF D8)");
        }

        int i = 2;
        while (i < data.Length - 1)
        {
            if (data[i] != 0xFF)
            {
                throw new IOException($"Expected 0xFF marker prefix at offset {i}, found 0x{data[i]:X2}");
            }

            i++;
            while (i < data.Length && data[i] == 0xFF)
            {
                i++;
            }

            if (i >= data.Length)
            {
                break;
            }

            byte marker = data[i++];
            if (marker == 0xD8 || marker == 0xD9 || marker is >= 0xD0 and <= 0xD7)
            {
                continue;
            }

            if (i + 2 > data.Length)
            {
                break;
            }

            int segmentLength = (data[i] << 8) | data[i + 1];
            bool isStartOfFrame = marker is >= 0xC0 and <= 0xCF && marker != 0xC4 && marker != 0xC8 && marker != 0xCC;
            if (isStartOfFrame && segmentLength >= 8 && i + 8 <= data.Length)
            {
                int height = (data[i + 3] << 8) | data[i + 4];
                int width = (data[i + 5] << 8) | data[i + 6];
                int components = data[i + 7];
                return new JpegInfo(width, height, components);
            }

            i += segmentLength;
        }

        throw new IOException("No SOF marker found in JPEG data");
    }

    private readonly record struct JpegInfo(int Width, int Height, int Components);
}
