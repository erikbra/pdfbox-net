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

/// <summary>
/// Reads a sampled image (raster) from a PDImageXObject.
/// </summary>
/// <remarks>
/// NOTE: This class is an adapted stub. Full sampled-image reading is not yet implemented
/// for the .NET port (requires platform-specific imaging APIs).
/// </remarks>
internal static class SampledImageReader
{
    /// <summary>
    /// Returns a raster representation of the given image.
    /// </summary>
    /// <exception cref="NotImplementedException">Always thrown – not yet implemented.</exception>
    public static byte[] GetRGBImage(PDImageXObject image)
    {
        ArgumentNullException.ThrowIfNull(image);

        int width = image.GetWidth();
        int height = image.GetHeight();
        int bitsPerComponent = image.GetBitsPerComponent();
        if (width <= 0 || height <= 0)
        {
            throw new IOException("image width and height must be positive");
        }

        byte[] imageData = image.GetImageData();
        if (imageData.Length == 0)
        {
            throw new IOException("Image stream is empty");
        }

        return bitsPerComponent switch
        {
            1 => FromOneBit(image, imageData, width, height),
            8 => FromEightBit(image, imageData, width, height),
            _ => throw new NotSupportedException(
                $"SampledImageReader.GetRGBImage supports 1-bit and 8-bit images, not {bitsPerComponent}-bit images.")
        };
    }

    private static byte[] FromOneBit(PDImageXObject image, byte[] imageData, int width, int height)
    {
        int components = image.GetColorSpace().GetNumberOfComponents();
        if (components != 1)
        {
            throw new NotSupportedException("1-bit sampled image reading is only supported for single-component images.");
        }

        byte[] rgb = new byte[checked(width * height * 3)];
        int rowBytes = (width + 7) / 8;
        if (imageData.Length < rowBytes * height)
        {
            throw new IOException("Image stream ended before all 1-bit samples were available.");
        }

        int dst = 0;
        for (int y = 0; y < height; y++)
        {
            int rowOffset = y * rowBytes;
            for (int x = 0; x < width; x++)
            {
                int bit = (imageData[rowOffset + (x / 8)] >> (7 - (x % 8))) & 1;
                byte value = bit == 0 ? (byte)0 : (byte)255;
                rgb[dst++] = value;
                rgb[dst++] = value;
                rgb[dst++] = value;
            }
        }

        return rgb;
    }

    private static byte[] FromEightBit(PDImageXObject image, byte[] imageData, int width, int height)
    {
        PdfBox.Net.PDModel.Graphics.Color.PDColorSpace colorSpace = image.GetColorSpace();
        int components = colorSpace.GetNumberOfComponents();
        int pixels = checked(width * height);
        int expectedLength = checked(pixels * components);
        if (imageData.Length < expectedLength)
        {
            throw new IOException("Image stream ended before all 8-bit samples were available.");
        }

        byte[] rgb = new byte[checked(pixels * 3)];
        float[] componentValues = new float[components];
        int src = 0;
        int dst = 0;
        for (int i = 0; i < pixels; i++)
        {
            for (int component = 0; component < components; component++)
            {
                componentValues[component] = imageData[src++] / 255f;
            }

            float[] converted = colorSpace.ToRGB(componentValues);
            rgb[dst++] = ToByte(converted, 0);
            rgb[dst++] = ToByte(converted, 1);
            rgb[dst++] = ToByte(converted, 2);
        }

        return rgb;
    }

    private static byte ToByte(float[] values, int index)
    {
        float value = index < values.Length ? values[index] : 0f;
        return (byte)Math.Clamp((int)MathF.Round(value * 255f), 0, 255);
    }
}
