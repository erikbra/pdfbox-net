/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 */

namespace PdfBox.Net.Filter;

internal interface IJpegRasterDecoder
{
    DecodedJpegRaster Decode(byte[] jpegBytes);
}

internal readonly record struct DecodedJpegRaster(int Width, int Height, int Components, byte[] Samples);
