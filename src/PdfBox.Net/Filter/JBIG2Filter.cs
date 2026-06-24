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
using JBig2Decoder.NETStandard;
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
        : this(Jbig2NetRasterDecoder.Instance)
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
        throw new NotSupportedException("JBIG2 encoding is intentionally unsupported; PDFBox.Net currently provides JBIG2 decoding only.");
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

internal sealed class Jbig2NetRasterDecoder : IJbig2RasterDecoder
{
    public static readonly Jbig2NetRasterDecoder Instance = new();

    private Jbig2NetRasterDecoder()
    {
    }

    public byte[] Decode(byte[] encoded, byte[]? globals, Jbig2DecodeOptions options)
    {
        try
        {
            byte[] input = globals is { Length: > 0 } ? [.. globals, .. encoded] : encoded;
            var decoder = new JBIG2StreamDecoder();
            byte[] rgb = decoder.DecodeJBIG2(input, out int width, out int height);
            return PackRgbToOneBitRows(rgb, width, height, options);
        }
        catch (Exception ex) when (ex is not IOException)
        {
            throw new IOException("Could not read JBIG2 image.", ex);
        }
    }

    private static byte[] PackRgbToOneBitRows(byte[] rgb, int width, int height, Jbig2DecodeOptions options)
    {
        if (width <= 0 || height <= 0)
        {
            return [];
        }

        if (rgb.Length < width * height * 3)
        {
            throw new IOException("JBIG2 decoder returned fewer RGB samples than expected.");
        }

        Rectangle source = options.SourceRegion ?? new Rectangle(0, 0, width, height);
        source.Intersect(new Rectangle(0, 0, width, height));
        if (source.Width <= 0 || source.Height <= 0)
        {
            return [];
        }

        int subsamplingX = Math.Max(1, options.SubsamplingX);
        int subsamplingY = Math.Max(1, options.SubsamplingY);
        int offsetX = Math.Clamp(options.SubsamplingOffsetX, 0, subsamplingX - 1);
        int offsetY = Math.Clamp(options.SubsamplingOffsetY, 0, subsamplingY - 1);
        int outputWidth = CountSamples(source.Width, offsetX, subsamplingX);
        int outputHeight = CountSamples(source.Height, offsetY, subsamplingY);
        int stride = (outputWidth + 7) / 8;
        byte[] packed = new byte[checked(stride * outputHeight)];

        int outputY = 0;
        for (int y = source.Y + offsetY; y < source.Bottom; y += subsamplingY, outputY++)
        {
            int rowOffset = outputY * stride;
            int outputX = 0;
            for (int x = source.X + offsetX; x < source.Right; x += subsamplingX, outputX++)
            {
                int sample = ((y * width) + x) * 3;
                int luma = (299 * rgb[sample]) + (587 * rgb[sample + 1]) + (114 * rgb[sample + 2]);
                if (luma >= 128000)
                {
                    packed[rowOffset + (outputX / 8)] |= (byte)(0x80 >> (outputX % 8));
                }
            }
        }

        return packed;
    }

    private static int CountSamples(int length, int offset, int step)
    {
        if (offset >= length)
        {
            return 0;
        }

        return ((length - offset - 1) / step) + 1;
    }
}

internal sealed record Jbig2DecodeOptions(
    Rectangle? SourceRegion,
    int SubsamplingX,
    int SubsamplingY,
    int SubsamplingOffsetX,
    int SubsamplingOffsetY,
    int BitsPerComponent);
