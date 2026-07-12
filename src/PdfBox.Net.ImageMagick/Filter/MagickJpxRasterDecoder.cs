/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 */

using ImageMagick;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.Filter;

internal sealed class MagickJpxRasterDecoder : IJpxRasterDecoder
{
    internal static readonly MagickJpxRasterDecoder Instance = new();

    private MagickJpxRasterDecoder()
    {
    }

    public DecodeResult Decode(byte[] encoded, Stream output, COSDictionary parameters, DecodeOptions options)
    {
        try
        {
            using MagickImage image = new(encoded);
            return DecodeImage(image, output, parameters, options);
        }
        catch (MagickException ex)
        {
            throw new IOException("Could not read JPEG 2000 (JPX) image.", ex);
        }
    }

    private static DecodeResult DecodeImage(MagickImage image, Stream output, COSDictionary parameters, DecodeOptions options)
    {
        if (image.Width > int.MaxValue || image.Height > int.MaxValue)
        {
            throw new IOException($"JPX image dimensions are too large: {image.Width}x{image.Height}");
        }

        int width = checked((int)image.Width);
        int height = checked((int)image.Height);
        string map = GetPixelMap(image, parameters);

        DecodeRegion region = (options.GetSourceRegion() ?? new DecodeRegion(0, 0, width, height))
            .Intersect(new DecodeRegion(0, 0, width, height));
        int subsamplingX = Math.Max(1, options.GetSubsamplingX());
        int subsamplingY = Math.Max(1, options.GetSubsamplingY());
        int offsetX = Math.Clamp(options.GetSubsamplingOffsetX(), 0, subsamplingX - 1);
        int offsetY = Math.Clamp(options.GetSubsamplingOffsetY(), 0, subsamplingY - 1);

        using IPixelCollection<byte> pixels = image.GetPixels();
        byte[] samples = pixels.ToByteArray(map)
            ?? throw new IOException("JPXDecode failed to extract image samples.");
        int components = map.Length;
        for (int y = region.Top + offsetY; y < region.Bottom; y += subsamplingY)
        {
            for (int x = region.Left + offsetX; x < region.Right; x += subsamplingX)
            {
                int sampleOffset = checked(((y * width) + x) * components);
                output.Write(samples, sampleOffset, components);
            }
        }

        options.SetFilterSubsampled(true);

        COSDictionary decodeParameters = new(parameters);
        decodeParameters.SetInt(COSName.BITS_PER_COMPONENT, 8);
        decodeParameters.SetInt(COSName.WIDTH, width);
        decodeParameters.SetInt(COSName.HEIGHT, height);
        if (!decodeParameters.GetBoolean(COSName.IMAGE_MASK, false))
        {
            decodeParameters.SetItem(COSName.DECODE, (COSBase?)null);
        }

        DecodeResult result = new(decodeParameters);
        if (!decodeParameters.ContainsKey(COSName.COLORSPACE))
        {
            result.SetColorSpace(new PDJPXColorSpace(components, CreateZeroes(components), CreateOnes(components)));
        }

        if (map.EndsWith('A'))
        {
            result.SetJPXSMask(ExtractAlphaSamples(image, width, height));
        }

        return result;
    }

    private static string GetPixelMap(MagickImage image, COSDictionary parameters)
    {
        string? requestedMap = GetPdfColorSpacePixelMap(parameters, image.HasAlpha);
        if (requestedMap is not null)
        {
            if (requestedMap.StartsWith("CMYK", StringComparison.Ordinal))
            {
                image.ColorSpace = ColorSpace.CMYK;
            }

            return requestedMap;
        }

        if (image.HasAlpha)
        {
            return image.ColorSpace == ColorSpace.Gray ? "IA" : "RGBA";
        }

        return image.ColorSpace switch
        {
            ColorSpace.Gray => "I",
            ColorSpace.CMYK => "CMYK",
            _ => "RGB"
        };
    }

    private static string? GetPdfColorSpacePixelMap(COSDictionary parameters, bool hasAlpha)
    {
        COSBase? colorSpace = parameters.GetDictionaryObject(COSName.COLORSPACE, COSName.CS);
        if (colorSpace is COSName name)
        {
            return name.GetName() switch
            {
                "DeviceGray" or "G" => hasAlpha ? "IA" : "I",
                "DeviceRGB" or "RGB" => hasAlpha ? "RGBA" : "RGB",
                "DeviceCMYK" or "CMYK" => hasAlpha ? "CMYKA" : "CMYK",
                _ => null
            };
        }

        if (colorSpace is COSArray array && array.Size() > 0 && array.GetObject(0) is COSName kind)
        {
            return kind.GetName() switch
            {
                "DeviceGray" => hasAlpha ? "IA" : "I",
                "DeviceRGB" => hasAlpha ? "RGBA" : "RGB",
                "DeviceCMYK" => hasAlpha ? "CMYKA" : "CMYK",
                _ => null
            };
        }

        return null;
    }

    private static byte[] ExtractAlphaSamples(MagickImage image, int width, int height)
    {
        using IPixelCollection<byte> pixels = image.GetPixels();
        byte[] rgba = pixels.ToByteArray("RGBA")
            ?? throw new IOException("JPXDecode failed to extract alpha samples.");
        byte[] alpha = new byte[checked(width * height)];
        for (int i = 0, sampleOffset = 3; i < alpha.Length; i++, sampleOffset += 4)
        {
            alpha[i] = rgba[sampleOffset];
        }

        return alpha;
    }

    private static float[] CreateZeroes(int count) => new float[count];

    private static float[] CreateOnes(int count)
    {
        float[] values = new float[count];
        Array.Fill(values, 1f);
        return values;
    }
}
