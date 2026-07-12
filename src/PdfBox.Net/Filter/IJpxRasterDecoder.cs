/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.Filter;

internal interface IJpxRasterDecoder
{
    DecodeResult Decode(byte[] encoded, Stream output, COSDictionary parameters, DecodeOptions options);
}

internal sealed class MissingJpxRasterDecoder : IJpxRasterDecoder
{
    internal static readonly MissingJpxRasterDecoder Instance = new();

    private MissingJpxRasterDecoder()
    {
    }

    public DecodeResult Decode(byte[] encoded, Stream output, COSDictionary parameters, DecodeOptions options)
    {
        throw new NotSupportedException(
            "JPXDecode requires the optional PdfBox.Net.ImageMagick package. " +
            "Reference PdfBox.Net.ImageMagick or PdfBox.Net.Rendering and call its registration method before decoding JPEG 2000 images.");
    }
}
