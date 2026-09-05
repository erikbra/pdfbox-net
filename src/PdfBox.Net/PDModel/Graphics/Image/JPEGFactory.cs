/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/image/JPEGFactory.java
 * PDFBOX_SOURCE_COMMIT: 1187c45f9dcee38ed5ac12bc15df04913b348875
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 046747da99a870902217efabf1c41297de157059
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
public sealed class JPEGFactory
{
    private static ILogger<JPEGFactory> LOG => PdfBoxLogging.CreateLogger<JPEGFactory>();

    private JPEGFactory()
    {
    }

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

        PDStream pdStream = new(document);
        COSStream cosStream = pdStream.GetCOSObject();
        using (Stream output = cosStream.CreateRawOutputStream())
        {
            stream.CopyTo(output);
        }

        (int width, int height, int numComponents) dimensions;
        using (Stream rawInput = cosStream.CreateRawInputStream())
        {
            dimensions = ParseJpegInfo(rawInput);
        }
        (int width, int height, int numComponents) = dimensions;

        COSName colorSpace = numComponents switch
        {
            1 => COSName.GetPDFName("DeviceGray"),
            3 => COSName.GetPDFName("DeviceRGB"),
            4 => COSName.GetPDFName("DeviceCMYK"),
            _ => throw new IOException($"Unsupported number of JPEG color components: {numComponents}")
        };

        // Create the image around the already-populated stream, no further copying.
        cosStream.SetItem(COSName.FILTER, COSName.DCT_DECODE);

        cosStream.SetInt(COSName.WIDTH, width);
        cosStream.SetInt(COSName.HEIGHT, height);
        cosStream.SetInt(COSName.BITS_PER_COMPONENT, 8);
        cosStream.SetItem(COSName.COLORSPACE, colorSpace);

        return new PDImageXObject(pdStream, null);
    }

    /// <summary>
    /// Creates a new JPEG image XObject from a byte array containing JPEG data.
    /// </summary>
    /// <param name="document">The document where the image will be created.</param>
    /// <param name="byteArray">Bytes of a JPEG image.</param>
    /// <returns>A new image XObject.</returns>
    /// <exception cref="IOException">The JPEG metadata cannot be read.</exception>
    public static PDImageXObject CreateFromByteArray(PDDocument document, byte[] byteArray)
    {
        ArgumentNullException.ThrowIfNull(byteArray);
        using MemoryStream input = new(byteArray, writable: false);
        return CreateFromStream(document, input);
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
    private static (int Width, int Height, int NumComponents) ParseJpegInfo(Stream stream)
    {
        if (stream.ReadByte() != 0xFF || stream.ReadByte() != 0xD8)
        {
            throw new IOException("Not a valid JPEG: missing SOI marker (FF D8)");
        }

        Span<byte> segmentHeader = stackalloc byte[2];
        Span<byte> frameHeader = stackalloc byte[6];
        Span<byte> skipBuffer = stackalloc byte[512];
        while (true)
        {
            int prefix = stream.ReadByte();
            if (prefix < 0)
            {
                break;
            }
            if (prefix != 0xFF)
            {
                throw new IOException($"Expected JPEG marker prefix FF, found {prefix:X2}");
            }

            int marker;
            do
            {
                marker = stream.ReadByte();
            }
            while (marker == 0xFF);

            if (marker < 0 || marker == 0xD9)
            {
                break;
            }
            // SOI, TEM, and restart markers have no data segment.
            if (marker == 0xD8 || marker == 0x01 || marker is >= 0xD0 and <= 0xD7)
            {
                continue;
            }

            stream.ReadExactly(segmentHeader);
            int segmentLength = (segmentHeader[0] << 8) | segmentHeader[1];
            if (segmentLength < 2)
            {
                throw new IOException("Invalid JPEG segment length");
            }

            // SOF markers: C0..CF except C4 (DHT), C8 (reserved), CC (DAC)
            bool isSof = marker >= 0xC0 && marker <= 0xCF
                         && marker != 0xC4 && marker != 0xC8 && marker != 0xCC;
            if (isSof && segmentLength >= 8)
            {
                stream.ReadExactly(frameHeader);
                int height = (frameHeader[1] << 8) | frameHeader[2];
                int width = (frameHeader[3] << 8) | frameHeader[4];
                return (width, height, frameHeader[5]);
            }

            for (int remaining = segmentLength - 2; remaining > 0;)
            {
                int count = Math.Min(remaining, skipBuffer.Length);
                stream.ReadExactly(skipBuffer[..count]);
                remaining -= count;
            }
        }

        throw new IOException("No SOF marker found in JPEG data");
    }
}
