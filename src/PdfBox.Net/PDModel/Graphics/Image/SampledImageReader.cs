/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/image/SampledImageReader.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 */

/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace PdfBox.Net.PDModel.Graphics.Image;

using PdfBox.Net.COS;
using PdfBox.Net.Filter;
using PdfBox.Net.PDModel.Graphics.Color;

/// <summary>
/// Reads a sampled image (raster) from a PDImage.
/// </summary>
/// <remarks>
/// NOTE: This class is an adapted stub. Full sampled-image reading is not yet implemented
/// for the .NET port (requires platform-specific imaging APIs).
/// </remarks>
internal static class SampledImageReader
{
    /// <summary>
    /// Returns a raster representation of the given image XObject.
    /// </summary>
    public static byte[] GetRGBImage(PDImageXObject image)
    {
        return GetRGBImage(image, null);
    }

    internal static byte[] GetRGBImage(PDImageXObject image, PDColorManagementContext? colorManagementContext)
    {
        ArgumentNullException.ThrowIfNull(image);
        (byte[] data, DecodeResult decodeResult) = image.DecodeImageData();
        COSDictionary decodeParameters = decodeResult.GetParameters();
        PDColorSpace colorSpace = decodeResult.GetJPXColorSpace() as PDColorSpace ?? image.GetColorSpace();
        colorSpace = colorManagementContext?.ResolveColorSpace(colorSpace) ?? colorSpace;

        return GetRGBImage(
            decodeParameters.GetInt(COSName.WIDTH, image.GetWidth()),
            decodeParameters.GetInt(COSName.HEIGHT, image.GetHeight()),
            decodeParameters.GetInt(COSName.BITS_PER_COMPONENT, image.GetBitsPerComponent()),
            colorSpace,
            data,
            decodeParameters.GetCOSArray(COSName.DECODE) ?? image.GetCOSObject()?.GetCOSArray(COSName.DECODE));
    }

    /// <summary>
    /// Returns a raster representation of the given image.
    /// </summary>
    public static byte[] GetRGBImage(PDImage image)
    {
        return GetRGBImage(image, null);
    }

    internal static byte[] GetRGBImage(PDImage image, PDColorManagementContext? colorManagementContext)
    {
        ArgumentNullException.ThrowIfNull(image);
        PDColorSpace colorSpace = image.GetColorSpace();
        colorSpace = colorManagementContext?.ResolveColorSpace(colorSpace) ?? colorSpace;
        return GetRGBImage(
            image.GetWidth(),
            image.GetHeight(),
            image.GetBitsPerComponent(),
            colorSpace,
            image.GetData(),
            image.GetDecode());
    }

    internal static byte[] GetRGBImage(
        int width,
        int height,
        int bitsPerComponent,
        PDColorSpace colorSpace,
        byte[] imageData,
        COSArray? decodeArray)
    {
        if (width <= 0 || height <= 0)
        {
            throw new IOException("image width and height must be positive");
        }

        if (imageData.Length == 0)
        {
            throw new IOException("Image stream is empty");
        }

        if (bitsPerComponent is < 1 or > 16)
        {
            throw new NotSupportedException(
                $"SampledImageReader.GetRGBImage supports 1- to 16-bit images, not {bitsPerComponent}-bit images.");
        }

        return FromPackedSamples(colorSpace, imageData, width, height, bitsPerComponent, decodeArray);
    }

    private static byte[] FromPackedSamples(
        PDColorSpace colorSpace,
        byte[] imageData,
        int width,
        int height,
        int bitsPerComponent,
        COSArray? decodeArray)
    {
        int components = colorSpace.GetNumberOfComponents();
        int rowBits = checked(width * components * bitsPerComponent);
        int rowBytes = (rowBits + 7) / 8;
        int expectedLength = checked(rowBytes * height);
        if (imageData.Length < expectedLength)
        {
            throw new IOException($"Image stream ended before all {bitsPerComponent}-bit samples were available.");
        }

        bool isIndexed = colorSpace is PDIndexed;
        float sampleMax = (1 << bitsPerComponent) - 1f;
        float[] decode = GetDecodeArray(colorSpace, bitsPerComponent, components, decodeArray);
        if (colorSpace.SupportsBatchConversion)
        {
            byte[] profileSamples = UnpackSamples(
                imageData,
                width,
                height,
                bitsPerComponent,
                components,
                rowBytes,
                sampleMax,
                decode);
            if (colorSpace.TryConvertToRgb8(profileSamples, width, height, out byte[] profileRgb))
            {
                return profileRgb;
            }
        }

        byte[] rgb = new byte[checked(width * height * 3)];
        float[] componentValues = new float[components];
        // Separation has one component, and its decoded tint is deterministic for each raw sample.
        // Cache after the normal decode/conversion path so no precision or rounding behavior changes.
        int separationSampleCount = colorSpace is PDSeparation ? 1 << bitsPerComponent : 0;
        byte[]? separationRgb = separationSampleCount == 0 ? null : new byte[checked(separationSampleCount * 3)];
        bool[]? convertedSeparationSamples = separationSampleCount == 0 ? null : new bool[separationSampleCount];
        int dst = 0;
        for (int y = 0; y < height; y++)
        {
            int bitOffset = y * rowBytes * 8;
            for (int x = 0; x < width; x++)
            {
                int firstSample = 0;
                for (int component = 0; component < components; component++)
                {
                    int sample = ReadBits(imageData, bitOffset, bitsPerComponent);
                    bitOffset += bitsPerComponent;
                    if (component == 0)
                    {
                        firstSample = sample;
                    }

                    float dMin = decode[component * 2];
                    float dMax = decode[(component * 2) + 1];
                    float decoded = dMin + (sample * ((dMax - dMin) / sampleMax));
                    componentValues[component] = isIndexed
                        ? MathF.Round(decoded)
                        : decoded;
                }

                if (separationRgb is not null && convertedSeparationSamples is not null)
                {
                    int lookupOffset = firstSample * 3;
                    if (!convertedSeparationSamples[firstSample])
                    {
                        float[] converted = colorSpace.ToRGB(componentValues);
                        separationRgb[lookupOffset] = ToByte(converted, 0);
                        separationRgb[lookupOffset + 1] = ToByte(converted, 1);
                        separationRgb[lookupOffset + 2] = ToByte(converted, 2);
                        convertedSeparationSamples[firstSample] = true;
                    }

                    rgb[dst++] = separationRgb[lookupOffset];
                    rgb[dst++] = separationRgb[lookupOffset + 1];
                    rgb[dst++] = separationRgb[lookupOffset + 2];
                }
                else
                {
                    float[] converted = colorSpace.ToRGB(componentValues);
                    rgb[dst++] = ToByte(converted, 0);
                    rgb[dst++] = ToByte(converted, 1);
                    rgb[dst++] = ToByte(converted, 2);
                }
            }
        }

        return rgb;
    }

    private static byte[] UnpackSamples(
        byte[] imageData,
        int width,
        int height,
        int bitsPerComponent,
        int components,
        int rowBytes,
        float sampleMax,
        float[] decode)
    {
        byte[] unpacked = new byte[checked(width * height * components)];
        int destination = 0;
        for (int y = 0; y < height; y++)
        {
            int bitOffset = y * rowBytes * 8;
            for (int x = 0; x < width; x++)
            {
                for (int component = 0; component < components; component++)
                {
                    int sample = ReadBits(imageData, bitOffset, bitsPerComponent);
                    bitOffset += bitsPerComponent;
                    float dMin = decode[component * 2];
                    float dMax = decode[(component * 2) + 1];
                    float decoded = dMin + (sample * ((dMax - dMin) / sampleMax));
                    unpacked[destination++] = (byte)Math.Clamp(
                        (int)MathF.Round(Math.Clamp(decoded, 0f, 1f) * 255f),
                        0,
                        255);
                }
            }
        }

        return unpacked;
    }

    private static int ReadBits(byte[] data, int bitOffset, int count)
    {
        int value = 0;
        for (int i = 0; i < count; i++)
        {
            int absoluteBit = bitOffset + i;
            int bit = (data[absoluteBit / 8] >> (7 - (absoluteBit % 8))) & 1;
            value = (value << 1) | bit;
        }

        return value;
    }

    private static float[] GetDecodeArray(PDColorSpace colorSpace, int bitsPerComponent, int components, COSArray? decodeArray)
    {
        if (decodeArray is not null && decodeArray.Size() >= components * 2)
        {
            float[] decode = new float[components * 2];
            for (int i = 0; i < decode.Length; i++)
            {
                if (decodeArray.GetObject(i) is not COSNumber number)
                {
                    return colorSpace.GetDefaultDecode(bitsPerComponent);
                }

                decode[i] = number.FloatValue();
            }

            return decode;
        }

        return colorSpace.GetDefaultDecode(bitsPerComponent);
    }

    private static byte ToByte(float[] values, int index)
    {
        float value = index < values.Length ? values[index] : 0f;
        return (byte)Math.Clamp((int)MathF.Round(value * 255f), 0, 255);
    }
}
