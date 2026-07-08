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
        ArgumentNullException.ThrowIfNull(image);
        return ExportRgbAsPng(SampledImageReader.GetRGBImage(image), image.GetWidth(), image.GetHeight());
    }

    /// <summary>
    /// Exports an inline image as PNG bytes.
    /// </summary>
    /// <param name="image">The inline image to export.</param>
    /// <returns>The exported PNG result.</returns>
    public static PdfImageExportResult ExportPng(PDImage image)
    {
        ArgumentNullException.ThrowIfNull(image);
        return ExportRgbAsPng(SampledImageReader.GetRGBImage(image), image.GetWidth(), image.GetHeight());
    }

    private static PdfImageExportResult ExportRgbAsPng(byte[] rgb, int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            throw new IOException("Image width and height must be positive.");
        }

        if (rgb.Length < width * height * 3)
        {
            throw new IOException("Decoded image data is shorter than expected.");
        }

        using BufferedImage bitmap = new(width, height, BufferedImage.TYPE_INT_RGB);
        int offset = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int red = rgb[offset++];
                int green = rgb[offset++];
                int blue = rgb[offset++];
                bitmap.SetRgb(x, y, unchecked((int)0xFF000000) | (red << 16) | (green << 8) | blue);
            }
        }

        byte[] data = RenderingBackend.Current.ImageCodec.Encode(bitmap, EncodedImageFormat.Png, 100);
        return new PdfImageExportResult("image/png", "png", data);
    }
}
