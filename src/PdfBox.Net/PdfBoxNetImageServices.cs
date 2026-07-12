/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 */

using PdfBox.Net.Filter;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.Image;

namespace PdfBox.Net;

internal static class PdfBoxNetImageServices
{
    private static IJpegRasterDecoder _jpegRasterDecoder = MissingJpegRasterDecoder.Instance;
    private static IJpxRasterDecoder _jpxRasterDecoder = MissingJpxRasterDecoder.Instance;
    private static IIccColorTransformFactory _iccColorTransformFactory = MissingIccColorTransformFactory.Instance;
    private static ITiffRasterDecoder _tiffRasterDecoder = MissingTiffRasterDecoder.Instance;

    internal static IJpegRasterDecoder JpegRasterDecoder => _jpegRasterDecoder;

    internal static IJpxRasterDecoder JpxRasterDecoder => _jpxRasterDecoder;

    internal static IIccColorTransformFactory IccColorTransformFactory => _iccColorTransformFactory;

    internal static ITiffRasterDecoder TiffRasterDecoder => _tiffRasterDecoder;

    internal static void Register(
        IJpegRasterDecoder? jpegRasterDecoder = null,
        IJpxRasterDecoder? jpxRasterDecoder = null,
        IIccColorTransformFactory? iccColorTransformFactory = null,
        ITiffRasterDecoder? tiffRasterDecoder = null)
    {
        if (jpegRasterDecoder is not null)
        {
            _jpegRasterDecoder = jpegRasterDecoder;
        }

        if (jpxRasterDecoder is not null)
        {
            _jpxRasterDecoder = jpxRasterDecoder;
        }

        if (iccColorTransformFactory is not null)
        {
            _iccColorTransformFactory = iccColorTransformFactory;
        }

        if (tiffRasterDecoder is not null)
        {
            _tiffRasterDecoder = tiffRasterDecoder;
        }
    }
}
