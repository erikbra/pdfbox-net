/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/image/LosslessFactory.java
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

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Color;
using SkiaSharp;

namespace PdfBox.Net.PDModel.Graphics.Image;

/// <summary>
/// Factory for creating a PDImageXObject from a losslessly compressed (FLATE/PNG) image.
/// </summary>
public static class LosslessFactory
{
    /// <summary>
    /// Creates a new image XObject from a raster image file (PNG, BMP, GIF, etc.).
    /// Pixel data is stored using lossless FLATE (zlib/deflate) compression with the
    /// DeviceRGB color space at 8 bits per component.
    /// </summary>
    /// <param name="document">The PDF document that will own the image.</param>
    /// <param name="bitmap">The source bitmap. The caller retains ownership.</param>
    /// <returns>A new <see cref="PDImageXObject"/> backed by FlateDecode-compressed DeviceRGB data.</returns>
    public static PDImageXObject CreateFromImage(PDDocument document, SKBitmap bitmap)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(bitmap);

        int width = bitmap.Width;
        int height = bitmap.Height;

        // Obtain pixel colors in reading order (row-major, top-left first).
        SKColor[] pixels = bitmap.Pixels;

        // Build a packed RGB byte array (3 bytes per pixel, no alpha).
        // The alpha channel is intentionally discarded; PDF DeviceRGB images are always opaque.
        byte[] rgbData = new byte[width * height * 3];
        int dst = 0;
        foreach (SKColor pixel in pixels)
        {
            rgbData[dst++] = pixel.Red;
            rgbData[dst++] = pixel.Green;
            rgbData[dst++] = pixel.Blue;
        }

        PDStream pdStream = new(document);
        using (Stream output = pdStream.CreateOutputStream(COSName.FLATE_DECODE))
        {
            output.Write(rgbData, 0, rgbData.Length);
        }

        COSStream cosStream = pdStream.GetCOSObject();
        cosStream.SetInt(COSName.WIDTH, width);
        cosStream.SetInt(COSName.HEIGHT, height);
        cosStream.SetInt(COSName.BITS_PER_COMPONENT, 8);
        cosStream.SetItem(COSName.COLORSPACE, COSName.GetPDFName("DeviceRGB"));

        return new PDImageXObject(pdStream, null);
    }

    /// <summary>
    /// Creates a new image XObject from a raw byte array using lossless (FLATE) compression.
    /// </summary>
    public static PDImageXObject CreateFromRawData(PDDocument document, byte[] data,
        int width, int height, int bitsPerComponent, int numberOfComponents)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(data);
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Image width and height must be positive.");
        }

        if (bitsPerComponent is not 1 and not 2 and not 4 and not 8 and not 16)
        {
            throw new ArgumentOutOfRangeException(nameof(bitsPerComponent), "Bits per component must be 1, 2, 4, 8, or 16.");
        }

        PDColorSpace colorSpace = numberOfComponents switch
        {
            1 => PDDeviceGray.Instance,
            3 => PDDeviceRGB.Instance,
            4 => PDDeviceCMYK.Instance,
            _ => throw new ArgumentOutOfRangeException(nameof(numberOfComponents), "Only 1, 3, and 4 component images are supported.")
        };

        long bitsPerRow = (long)width * numberOfComponents * bitsPerComponent;
        long expectedLength = ((bitsPerRow + 7) / 8) * height;
        if (data.LongLength != expectedLength)
        {
            throw new IOException(
                $"Raw image data length {data.LongLength} does not match expected length {expectedLength} " +
                $"for {width}x{height}x{numberOfComponents} at {bitsPerComponent} bpc.");
        }

        PDStream pdStream = new(document);
        using (Stream output = pdStream.CreateOutputStream(COSName.FLATE_DECODE))
        {
            output.Write(data, 0, data.Length);
        }

        COSStream cosStream = pdStream.GetCOSObject();
        cosStream.SetInt(COSName.WIDTH, width);
        cosStream.SetInt(COSName.HEIGHT, height);
        cosStream.SetInt(COSName.BITS_PER_COMPONENT, bitsPerComponent);
        cosStream.SetItem(COSName.COLORSPACE, colorSpace.GetCOSObject());

        return new PDImageXObject(pdStream, null);
    }
}
