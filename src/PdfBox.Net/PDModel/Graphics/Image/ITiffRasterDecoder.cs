/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 */

namespace PdfBox.Net.PDModel.Graphics.Image;

internal interface ITiffRasterDecoder
{
    DecodedTiffRaster DecodeOneBitRows(Stream stream);
}

internal readonly record struct DecodedTiffRaster(int Width, int Height, byte[] OneBitRows);

internal sealed class MissingTiffRasterDecoder : ITiffRasterDecoder
{
    internal static readonly MissingTiffRasterDecoder Instance = new();

    private MissingTiffRasterDecoder()
    {
    }

    public DecodedTiffRaster DecodeOneBitRows(Stream stream)
    {
        throw new NotSupportedException(
            "TIFF import through CCITTFactory requires the optional PdfBox.Net.ImageMagick package. " +
            "Reference PdfBox.Net.ImageMagick or PdfBox.Net.Rendering and call its registration method before importing TIFF images.");
    }
}
