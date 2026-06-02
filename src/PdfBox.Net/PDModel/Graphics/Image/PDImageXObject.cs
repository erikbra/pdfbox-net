/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/graphics/image/PDImageXObject.java
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
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.PDModel.Graphics.Image;

public sealed class PDImageXObject : PDXObject
{
    private readonly PDResources? _resources;

    public PDImageXObject(PDStream stream, PDResources? resources)
        : base(stream)
    {
        _resources = resources;
        COSStream? cos = GetCOSObject();
        if (cos is not null)
        {
            cos.SetName(COSName.TYPE, "XObject");
            cos.SetName(COSName.GetPDFName("Subtype"), "Image");
        }
    }

    public static PDImageXObject CreateFromFile(string imagePath, PDDocument document)
    {
        ArgumentNullException.ThrowIfNull(imagePath);
        ArgumentNullException.ThrowIfNull(document);

        string extension = Path.GetExtension(imagePath);
        if (string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(extension, ".jpeg", StringComparison.OrdinalIgnoreCase))
        {
            return JPEGFactory.CreateFromFile(document, imagePath);
        }

        if (string.Equals(extension, ".tif", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(extension, ".tiff", StringComparison.OrdinalIgnoreCase))
        {
            return CCITTFactory.CreateFromFile(document, imagePath);
        }

        throw new NotImplementedException(
            $"PDImageXObject.CreateFromFile does not support extension '{extension}'. " +
            "Supported extensions in this port are .jpg, .jpeg, .tif, and .tiff.");
    }

    public int GetWidth() => GetCOSObject()?.GetInt(COSName.WIDTH, 0) ?? 0;
    public int GetHeight() => GetCOSObject()?.GetInt(COSName.HEIGHT, 0) ?? 0;
    public int GetBitsPerComponent() => GetCOSObject()?.GetInt(COSName.BITS_PER_COMPONENT, 8) ?? 8;

    public PDColorSpace GetColorSpace()
    {
        COSBase? colorSpace = GetCOSObject()?.GetDictionaryObject(COSName.COLORSPACE);
        if (colorSpace is null)
        {
            return PDDeviceGray.Instance;
        }

        return PDColorSpace.Create(colorSpace, _resources);
    }

    public byte[] GetImageData()
    {
        PDStream? stream = GetStream();
        if (stream is null)
        {
            return Array.Empty<byte>();
        }

        using Stream input = stream.CreateInputStream();
        using MemoryStream output = new();
        input.CopyTo(output);
        return output.ToArray();
    }
}
