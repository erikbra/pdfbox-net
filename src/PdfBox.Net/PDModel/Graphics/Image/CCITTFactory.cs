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

namespace PdfBox.Net.PDModel.Graphics.Image;

/// <summary>
/// Factory for creating a PDImageXObject containing a CCITT compressed TIFF image.
/// </summary>
/// <remarks>
/// TIFF input is decoded by an optional image provider and re-encoded as CCITT Group 4 image data.
/// </remarks>
public static class CCITTFactory
{
    /// <summary>
    /// Creates a new image XObject from a single-channel (single-page) TIFF file.
    /// </summary>
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
    public static PDImageXObject CreateFromStream(PDDocument document, Stream stream)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(stream);

        DecodedTiffRaster raster = PdfBoxNetImageServices.TiffRasterDecoder.DecodeOneBitRows(stream);
        byte[] encoded = EncodeGroup4(raster.OneBitRows, raster.Width, raster.Height);

        PDStream pdStream = new(document);
        COSStream cosStream = pdStream.GetCOSObject();
        cosStream.SetItem(COSName.FILTER, COSName.CCITTFAX_DECODE);
        using (Stream raw = cosStream.CreateRawOutputStream())
        {
            raw.Write(encoded, 0, encoded.Length);
        }

        cosStream.SetInt(COSName.WIDTH, raster.Width);
        cosStream.SetInt(COSName.HEIGHT, raster.Height);
        cosStream.SetInt(COSName.BITS_PER_COMPONENT, 1);
        cosStream.SetItem(COSName.COLORSPACE, COSName.GetPDFName("DeviceGray"));

        COSDictionary decodeParms = new();
        decodeParms.SetInt(COSName.COLUMNS, raster.Width);
        decodeParms.SetInt(COSName.ROWS, raster.Height);
        decodeParms.SetInt(COSName.K, -1);
        decodeParms.SetBoolean(COSName.BLACK_IS_1, true);
        cosStream.SetItem(COSName.DECODE_PARMS, decodeParms);

        return new PDImageXObject(pdStream, null);
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
