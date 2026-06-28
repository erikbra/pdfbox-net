/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox FlateFilter.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/filter/FlateFilter.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
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

using System.IO.Compression;
using PdfBox.Net.COS;

namespace PdfBox.Net.Filter;

public sealed class FlateFilter : Filter
{
    public override DecodeResult Decode(Stream input, Stream output, COSDictionary parameters, int index, DecodeOptions options)
    {
        COSDictionary decodeParams = GetDecodeParams(parameters, index);
        byte[] inflated = Inflate(input);
        byte[] predictorDecoded = Predictor.Decode(inflated, decodeParams);
        output.Write(predictorDecoded, 0, predictorDecoded.Length);
        output.Flush();
        return new DecodeResult(parameters);
    }

    public override void Encode(Stream input, Stream output, COSDictionary parameters, int index)
    {
        ZLibCompressionOptions options = new()
        {
            CompressionLevel = GetCompressionLevel()
        };

        using (ZLibStream zlib = new(output, options, leaveOpen: true))
        {
            input.CopyTo(zlib);
        }

        output.Flush();
    }

    private static byte[] Inflate(Stream input)
    {
        using MemoryStream copy = new();
        input.CopyTo(copy);
        byte[] encoded = copy.ToArray();

        if (encoded.Length == 0)
        {
            return [];
        }

        try
        {
            using MemoryStream encodedStream = new(encoded, writable: false);
            using ZLibStream zlib = new(encodedStream, CompressionMode.Decompress, leaveOpen: false);
            using MemoryStream decoded = new();
            zlib.CopyTo(decoded);
            return decoded.ToArray();
        }
        catch (InvalidDataException)
        {
            using MemoryStream encodedStream = new(encoded, writable: false);
            using DeflateStream deflate = new(encodedStream, CompressionMode.Decompress, leaveOpen: false);
            using MemoryStream decoded = new();
            deflate.CopyTo(decoded);
            return decoded.ToArray();
        }
    }
}
