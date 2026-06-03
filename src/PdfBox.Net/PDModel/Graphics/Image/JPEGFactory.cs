/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/image/JPEGFactory.java
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

namespace PdfBox.Net.PDModel.Graphics.Image;

/// <summary>
/// Factory for creating a PDImageXObject containing a JPEG compressed image.
/// </summary>
public static class JPEGFactory
{
    /// <summary>
    /// Creates a new image XObject from a JPEG stream.
    /// The raw JPEG bytes are embedded directly in the PDF stream using the DCTDecode filter.
    /// </summary>
    /// <param name="document">The PDF document that will own the image.</param>
    /// <param name="stream">A stream of JPEG-encoded data.</param>
    /// <returns>A new <see cref="PDImageXObject"/> backed by DCTDecode-compressed data.</returns>
    public static PDImageXObject CreateFromStream(PDDocument document, Stream stream)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(stream);

        byte[] jpegBytes;
        using (MemoryStream ms = new())
        {
            stream.CopyTo(ms);
            jpegBytes = ms.ToArray();
        }

        (int width, int height, int numComponents) = ParseJpegInfo(jpegBytes);

        COSName colorSpace = numComponents switch
        {
            1 => COSName.GetPDFName("DeviceGray"),
            3 => COSName.GetPDFName("DeviceRGB"),
            4 => COSName.GetPDFName("DeviceCMYK"),
            _ => throw new IOException($"Unsupported number of JPEG color components: {numComponents}")
        };

        // Write raw JPEG bytes directly (they are already DCT-encoded).
        PDStream pdStream = new(document);
        COSStream cosStream = pdStream.GetCOSObject();
        cosStream.SetItem(COSName.FILTER, COSName.DCT_DECODE);
        using (Stream raw = cosStream.CreateRawOutputStream())
        {
            raw.Write(jpegBytes, 0, jpegBytes.Length);
        }

        cosStream.SetInt(COSName.WIDTH, width);
        cosStream.SetInt(COSName.HEIGHT, height);
        cosStream.SetInt(COSName.BITS_PER_COMPONENT, 8);
        cosStream.SetItem(COSName.COLORSPACE, colorSpace);

        return new PDImageXObject(pdStream, null);
    }

    /// <summary>
    /// Creates a new image XObject from a JPEG file path.
    /// </summary>
    /// <param name="document">The PDF document that will own the image.</param>
    /// <param name="path">Path to a JPEG file.</param>
    /// <returns>A new <see cref="PDImageXObject"/> backed by DCTDecode-compressed data.</returns>
    public static PDImageXObject CreateFromFile(PDDocument document, string path)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(path);

        using FileStream fs = File.OpenRead(path);
        return CreateFromStream(document, fs);
    }

    /// <summary>
    /// Parses JPEG Start-of-Frame (SOF) markers to extract image dimensions and component count.
    /// </summary>
    private static (int Width, int Height, int NumComponents) ParseJpegInfo(byte[] data)
    {
        // Need at least SOI (2 bytes) + one full marker header (4 bytes) to begin iterating.
        if (data.Length < 4 || data[0] != 0xFF || data[1] != 0xD8)
            throw new IOException("Not a valid JPEG: missing SOI marker (FF D8)");

        int i = 2; // skip SOI (FF D8)

        while (i < data.Length - 1)
        {
            if (data[i] != 0xFF)
                throw new IOException($"Expected 0xFF marker prefix at offset {i}, found 0x{data[i]:X2}");
            i++; // skip leading 0xFF

            // Skip any extra 0xFF padding bytes
            while (i < data.Length && data[i] == 0xFF)
                i++;

            if (i >= data.Length)
                break;

            byte marker = data[i++];

            // Markers with no data segment: SOI (D8), EOI (D9), RST0–RST7 (D0–D7)
            if (marker == 0xD8 || marker == 0xD9 || (marker >= 0xD0 && marker <= 0xD7))
                continue;

            if (i + 2 > data.Length)
                break;

            // Segment length field (big-endian) includes the 2 length bytes themselves
            int segLen = (data[i] << 8) | data[i + 1];

            // SOF markers: C0..CF except C4 (DHT), C8 (reserved), CC (DAC)
            bool isSof = marker >= 0xC0 && marker <= 0xCF
                         && marker != 0xC4 && marker != 0xC8 && marker != 0xCC;

            if (isSof && segLen >= 8 && i + 8 <= data.Length)
            {
                // Segment layout after the length field:
                //   [0]     sample precision  (1 byte, typically 8)
                //   [1..2]  image height      (2 bytes, big-endian)
                //   [3..4]  image width       (2 bytes, big-endian)
                //   [5]     number of components
                int height = (data[i + 3] << 8) | data[i + 4];
                int width  = (data[i + 5] << 8) | data[i + 6];
                int numComponents = data[i + 7];
                return (width, height, numComponents);
            }

            i += segLen;
        }

        throw new IOException("No SOF marker found in JPEG data");
    }
}
