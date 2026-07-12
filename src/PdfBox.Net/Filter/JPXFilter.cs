/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/filter/JPXFilter.java
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

using PdfBox.Net.COS;

namespace PdfBox.Net.Filter;

/// <summary>
/// Decompresses data encoded using the wavelet-based JPEG 2000 standard.
/// </summary>
public sealed class JPXFilter : Filter
{
    private readonly IJpxRasterDecoder? _jpxRasterDecoder;

    public JPXFilter()
        : this(null)
    {
    }

    internal JPXFilter(IJpxRasterDecoder? jpxRasterDecoder)
    {
        _jpxRasterDecoder = jpxRasterDecoder;
    }

    public override DecodeResult Decode(Stream input, Stream output, COSDictionary parameters, int index, DecodeOptions options)
    {
        return (_jpxRasterDecoder ?? PdfBoxNetImageServices.JpxRasterDecoder)
            .Decode(ReadBytes(input), output, parameters, options);
    }

    public override void Encode(Stream input, Stream output, COSDictionary parameters, int index)
    {
        throw new NotSupportedException("JPX encoding is intentionally unsupported; PDFBox.Net currently provides JPX decoding only.");
    }

    private static byte[] ReadBytes(Stream input)
    {
        using MemoryStream memory = new();
        input.CopyTo(memory);
        return memory.ToArray();
    }
}
