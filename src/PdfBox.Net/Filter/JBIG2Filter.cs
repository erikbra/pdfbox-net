/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/filter/JBIG2Filter.java
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

using System.Drawing;
using PdfBox.Net.COS;
using PdfBox.Net.IO;

namespace PdfBox.Net.Filter;

/// <summary>
/// Decompresses data encoded using the JBIG2 standard.
/// </summary>
public sealed class JBIG2Filter : Filter
{
    private readonly IJbig2RasterDecoder _decoder;

    public JBIG2Filter()
        : this(MissingJbig2RasterDecoder.Instance)
    {
    }

    internal JBIG2Filter(IJbig2RasterDecoder decoder)
    {
        _decoder = decoder;
    }

    public override DecodeResult Decode(Stream input, Stream output, COSDictionary parameters, int index, DecodeOptions options)
    {
        COSDictionary decodeParameters = GetDecodeParams(parameters, index);
        byte[] encoded = IOUtils.ToByteArray(input);
        byte[]? globals = ReadGlobals(decodeParameters);
        int bitsPerComponent = parameters.GetInt(COSName.BITS_PER_COMPONENT, 1);
        Jbig2DecodeOptions decoderOptions = new(
            options.GetSourceRegion(),
            Math.Max(1, options.GetSubsamplingX()),
            Math.Max(1, options.GetSubsamplingY()),
            Math.Max(0, options.GetSubsamplingOffsetX()),
            Math.Max(0, options.GetSubsamplingOffsetY()),
            bitsPerComponent);

        byte[] decoded = _decoder.Decode(encoded, globals, decoderOptions);
        output.Write(decoded, 0, decoded.Length);
        options.SetFilterSubsampled(true);
        return new DecodeResult(parameters);
    }

    public override void Encode(Stream input, Stream output, COSDictionary parameters, int index)
    {
        throw new NotSupportedException("JBIG2 encoding is not implemented.");
    }

    private static byte[]? ReadGlobals(COSDictionary decodeParameters)
    {
        COSStream? globals = decodeParameters.GetCOSStream(COSName.JBIG2_GLOBALS);
        if (globals is null)
        {
            return null;
        }

        using Stream input = globals.CreateInputStream();
        return IOUtils.ToByteArray(input);
    }
}

internal interface IJbig2RasterDecoder
{
    byte[] Decode(byte[] encoded, byte[]? globals, Jbig2DecodeOptions options);
}

internal sealed class MissingJbig2RasterDecoder : IJbig2RasterDecoder
{
    public static readonly MissingJbig2RasterDecoder Instance = new();

    private MissingJbig2RasterDecoder()
    {
    }

    public byte[] Decode(byte[] encoded, byte[]? globals, Jbig2DecodeOptions options)
    {
        throw new IOException("JBIG2 decoder is not installed; PdfBox.Net requires a JBIG2 decoder integration for JBIG2Decode streams.");
    }
}

internal sealed record Jbig2DecodeOptions(
    Rectangle? SourceRegion,
    int SubsamplingX,
    int SubsamplingY,
    int SubsamplingOffsetX,
    int SubsamplingOffsetY,
    int BitsPerComponent);
