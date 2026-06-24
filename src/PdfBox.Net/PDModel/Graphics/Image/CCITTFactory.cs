/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/image/CCITTFactory.java
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

using PdfBox.Net.PDModel;
using PdfBox.Net.COS;
using PdfBox.Net.Filter;
using PdfBox.Net.PDModel.Common;
using ImageMagick;

namespace PdfBox.Net.PDModel.Graphics.Image;

/// <summary>
/// Factory for creating a PDImageXObject containing a CCITT compressed TIFF image.
/// </summary>
/// <remarks>
/// NOTE: This class is an adapted stub. Full CCITT/TIFF image creation is not yet implemented
/// for the .NET port (requires platform-specific imaging APIs).
/// </remarks>
public static class CCITTFactory
{
    /// <summary>
    /// Creates a new image XObject from a single-channel (single-page) TIFF file.
    /// </summary>
    /// <exception cref="NotImplementedException">Always thrown – not yet implemented.</exception>
    public static PDImageXObject CreateFromFile(PDDocument document, string path)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(path);

        using FileStream file = File.OpenRead(path);
        return CreateFromStream(document, file);
    }

    /// <summary>
    /// Creates a new image XObject from a TIFF stream.
    /// </summary>
    /// <exception cref="NotImplementedException">Always thrown – not yet implemented.</exception>
    public static PDImageXObject CreateFromStream(PDDocument document, Stream stream)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(stream);

        using MagickImage image = ReadTiff(stream);
        byte[] oneBitRows = ExtractOneBitRows(image, out int width, out int height);
        byte[] encoded = EncodeGroup4(oneBitRows, width, height);

        PDStream pdStream = new(document);
        COSStream cosStream = pdStream.GetCOSObject();
        cosStream.SetItem(COSName.FILTER, COSName.CCITTFAX_DECODE);
        using (Stream raw = cosStream.CreateRawOutputStream())
        {
            raw.Write(encoded, 0, encoded.Length);
        }

        cosStream.SetInt(COSName.WIDTH, width);
        cosStream.SetInt(COSName.HEIGHT, height);
        cosStream.SetInt(COSName.BITS_PER_COMPONENT, 1);
        cosStream.SetItem(COSName.COLORSPACE, COSName.GetPDFName("DeviceGray"));

        COSDictionary decodeParms = new();
        decodeParms.SetInt(COSName.COLUMNS, width);
        decodeParms.SetInt(COSName.ROWS, height);
        decodeParms.SetInt(COSName.K, -1);
        decodeParms.SetBoolean(COSName.BLACK_IS_1, true);
        cosStream.SetItem(COSName.DECODE_PARMS, decodeParms);

        return new PDImageXObject(pdStream, null);
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

    private static byte[] EncodeGroup4(byte[] oneBitRows, int width, int height)
    {
        COSDictionary parameters = new();
        parameters.SetInt(COSName.COLUMNS, width);
        parameters.SetInt(COSName.ROWS, height);

        using MemoryStream input = new(oneBitRows);
        using MemoryStream output = new();
        new CCITTFaxDecodeFilter().Encode(input, output, parameters, 0);
        return output.ToArray();
    }
}
