using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.Rendering;

namespace PdfBox.Net.PDModel.Graphics.Image;

/// <summary>
/// Exports PDF images to browser-safe raster formats without exposing a concrete rendering backend.
/// </summary>
public static class PdfImageExporter
{
    private static readonly COSName MaskKey = COSName.GetPDFName("Mask");

    /// <summary>
    /// Exports an image XObject in a browser-safe format, preserving a safe JPEG stream when possible.
    /// </summary>
    public static PdfImageExportResult ExportForBrowser(PDImageXObject image)
    {
        return ExportForBrowser(image, null);
    }

    /// <summary>
    /// Exports an image XObject in a browser-safe format using document-scoped color management.
    /// Safe DeviceRGB and sRGB ICCBased JPEG streams are preserved; other images are exported as PNG.
    /// </summary>
    public static PdfImageExportResult ExportForBrowser(
        PDImageXObject image,
        PDColorManagementContext? colorManagementContext)
    {
        ArgumentNullException.ThrowIfNull(image);
        if (TryExportJpeg(image, colorManagementContext, out PdfImageExportResult? jpeg))
        {
            return jpeg!;
        }

        return ExportPng(image, colorManagementContext);
    }

    /// <summary>
    /// Exports an image XObject as PNG bytes.
    /// </summary>
    /// <param name="image">The image XObject to export.</param>
    /// <returns>The exported PNG result.</returns>
    public static PdfImageExportResult ExportPng(PDImageXObject image)
    {
        return ExportPng(image, null);
    }

    /// <summary>
    /// Exports an image XObject as PNG bytes using document-scoped color management.
    /// </summary>
    public static PdfImageExportResult ExportPng(
        PDImageXObject image,
        PDColorManagementContext? colorManagementContext)
    {
        ArgumentNullException.ThrowIfNull(image);
        int width = image.GetWidth();
        int height = image.GetHeight();
        return ExportRgbAsPng(
            SampledImageReader.GetRGBImage(image, colorManagementContext),
            width,
            height,
            CreateSoftMaskAlpha(image, width, height));
    }

    /// <summary>
    /// Exports an inline image as PNG bytes.
    /// </summary>
    /// <param name="image">The inline image to export.</param>
    /// <returns>The exported PNG result.</returns>
    public static PdfImageExportResult ExportPng(PDImage image)
    {
        return ExportPng(image, null);
    }

    /// <summary>
    /// Exports an inline image as PNG bytes using document-scoped color management.
    /// </summary>
    public static PdfImageExportResult ExportPng(PDImage image, PDColorManagementContext? colorManagementContext)
    {
        ArgumentNullException.ThrowIfNull(image);
        return ExportRgbAsPng(
            SampledImageReader.GetRGBImage(image, colorManagementContext),
            image.GetWidth(),
            image.GetHeight());
    }

    private static bool TryExportJpeg(
        PDImageXObject image,
        PDColorManagementContext? colorManagementContext,
        out PdfImageExportResult? result)
    {
        result = null;
        PDStream? stream = image.GetStream();
        COSStream? dictionary = stream?.GetCOSObject();
        if (stream is null ||
            dictionary is null ||
            stream.GetFilters() is not [COSName filter] ||
            !filter.Equals(COSName.DCT_DECODE) ||
            dictionary.ContainsKey(COSName.SMASK) ||
            dictionary.ContainsKey(MaskKey) ||
            dictionary.ContainsKey(COSName.DECODE) ||
            dictionary.ContainsKey(COSName.DECODE_PARMS) ||
            dictionary.ContainsKey(COSName.DP))
        {
            return false;
        }

        PDColorSpace sourceColorSpace = image.GetColorSpace();
        PDColorSpace effectiveColorSpace = colorManagementContext?.ResolveColorSpace(sourceColorSpace) ?? sourceColorSpace;
        bool browserSafeColorSpace = sourceColorSpace.GetNumberOfComponents() == 3 &&
                                     ReferenceEquals(effectiveColorSpace, sourceColorSpace) &&
                                     (effectiveColorSpace is PDDeviceRGB ||
                                      effectiveColorSpace is PDICCBased iccBased && iccBased.IsSrgb());
        if (!browserSafeColorSpace)
        {
            return false;
        }

        using Stream rawInput = dictionary.CreateRawInputStream();
        using MemoryStream jpeg = new();
        rawInput.CopyTo(jpeg);
        byte[] data = jpeg.ToArray();
        if (!HasThreeJpegComponents(data))
        {
            return false;
        }

        result = new PdfImageExportResult("image/jpeg", "jpg", data);
        return true;
    }

    private static bool HasThreeJpegComponents(byte[] data)
    {
        if (data.Length < 4 || data[0] != 0xFF || data[1] != 0xD8)
        {
            return false;
        }

        int offset = 2;
        while (offset < data.Length - 1)
        {
            if (data[offset++] != 0xFF)
            {
                return false;
            }

            while (offset < data.Length && data[offset] == 0xFF)
            {
                offset++;
            }

            if (offset >= data.Length)
            {
                return false;
            }

            byte marker = data[offset++];
            if (marker == 0xD9 || marker == 0xDA)
            {
                return false;
            }

            if (marker == 0x01 || marker is >= 0xD0 and <= 0xD8)
            {
                continue;
            }

            if (offset > data.Length - 2)
            {
                return false;
            }

            int segmentLength = (data[offset] << 8) | data[offset + 1];
            if (segmentLength < 2 || offset > data.Length - segmentLength)
            {
                return false;
            }

            bool isStartOfFrame = marker is >= 0xC0 and <= 0xCF && marker is not (0xC4 or 0xC8 or 0xCC);
            if (isStartOfFrame)
            {
                return segmentLength >= 8 && data[offset + 7] == 3;
            }

            offset += segmentLength;
        }

        return false;
    }

    private static PdfImageExportResult ExportRgbAsPng(byte[] rgb, int width, int height, byte[]? alpha = null)
    {
        if (width <= 0 || height <= 0)
        {
            throw new IOException("Image width and height must be positive.");
        }

        int pixelCount;
        int rgbLength;
        try
        {
            pixelCount = checked(width * height);
            rgbLength = checked(pixelCount * 3);
        }
        catch (OverflowException exception)
        {
            throw new IOException("Image dimensions are too large to export.", exception);
        }

        if (rgb.Length < rgbLength)
        {
            throw new IOException("Decoded image data is shorter than expected.");
        }

        bool hasAlpha = alpha is { Length: var alphaLength } && alphaLength >= pixelCount;
        InterleavedPixelData pixels;
        if (hasAlpha)
        {
            byte[] rgba;
            try
            {
                rgba = GC.AllocateUninitializedArray<byte>(checked(pixelCount * 4));
            }
            catch (OverflowException exception)
            {
                throw new IOException("Image dimensions are too large to export.", exception);
            }

            int rgbOffset = 0;
            int rgbaOffset = 0;
            for (int pixel = 0; pixel < pixelCount; pixel++)
            {
                rgba[rgbaOffset++] = rgb[rgbOffset++];
                rgba[rgbaOffset++] = rgb[rgbOffset++];
                rgba[rgbaOffset++] = rgb[rgbOffset++];
                rgba[rgbaOffset++] = alpha![pixel];
            }

            pixels = new InterleavedPixelData(rgba, width, height, checked(width * 4), InterleavedPixelFormat.Rgba32);
        }
        else
        {
            pixels = new InterleavedPixelData(rgb, width, height, checked(width * 3), InterleavedPixelFormat.Rgb24);
        }

        byte[] data = RenderingBackend.Current.ImageCodec.EncodePng(pixels);
        return new PdfImageExportResult("image/png", "png", data);
    }

    private static byte[]? CreateSoftMaskAlpha(PDImageXObject image, int width, int height)
    {
        PDImageXObject? softMask = image.GetSoftMask();
        if (softMask is null || width <= 0 || height <= 0)
        {
            return null;
        }

        int maskWidth = softMask.GetWidth();
        int maskHeight = softMask.GetHeight();
        if (maskWidth <= 0 || maskHeight <= 0)
        {
            return null;
        }

        byte[] maskRgb = SampledImageReader.GetRGBImage(softMask);
        if (maskRgb.Length < maskWidth * maskHeight * 3)
        {
            return null;
        }

        byte[] alpha = new byte[width * height];
        for (int y = 0; y < height; y++)
        {
            int maskY = Math.Min(maskHeight - 1, y * maskHeight / height);
            for (int x = 0; x < width; x++)
            {
                int maskX = Math.Min(maskWidth - 1, x * maskWidth / width);
                alpha[(y * width) + x] = maskRgb[((maskY * maskWidth) + maskX) * 3];
            }
        }

        return alpha;
    }
}
