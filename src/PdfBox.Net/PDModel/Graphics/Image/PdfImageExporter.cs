using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.Rendering;

namespace PdfBox.Net.PDModel.Graphics.Image;

/// <summary>
/// Exports PDF images to browser-safe raster formats without exposing a concrete rendering backend.
/// </summary>
public static class PdfImageExporter
{
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

    private static PdfImageExportResult ExportRgbAsPng(byte[] rgb, int width, int height, byte[]? alpha = null)
    {
        if (width <= 0 || height <= 0)
        {
            throw new IOException("Image width and height must be positive.");
        }

        if (rgb.Length < width * height * 3)
        {
            throw new IOException("Decoded image data is shorter than expected.");
        }

        bool hasAlpha = alpha is { Length: var alphaLength } && alphaLength >= width * height;
        using BufferedImage bitmap = new(width, height, hasAlpha ? BufferedImage.TYPE_INT_ARGB : BufferedImage.TYPE_INT_RGB);
        int offset = 0;
        int alphaOffset = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int red = rgb[offset++];
                int green = rgb[offset++];
                int blue = rgb[offset++];
                int opacity = hasAlpha ? alpha![alphaOffset++] : 0xFF;
                bitmap.SetRgb(x, y, (opacity << 24) | (red << 16) | (green << 8) | blue);
            }
        }

        byte[] data = RenderingBackend.Current.ImageCodec.Encode(bitmap, EncodedImageFormat.Png, 100);
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
