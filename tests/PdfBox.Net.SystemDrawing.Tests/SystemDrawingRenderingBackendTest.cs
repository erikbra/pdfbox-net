using PdfBox.Net.Rendering;
using PdfBox.Net.Printing;

namespace PdfBox.Net.SystemDrawing.Tests;

public class SystemDrawingRenderingBackendTest
{
    [Fact]
    public void PrintBackendRegistersWindowsSpoolerAdapter()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Skip("System.Drawing.Printing is supported only on Windows.");
        }

        SystemDrawingPrintBackend.Register();

        Assert.True(PrintingBackend.IsRegistered);
        Assert.Same(SystemDrawingPrintBackend.Instance, PrintingBackend.Current);
        Assert.Equal("System.Drawing.Printing", PrintingBackend.Current.Name);
        Assert.True(PrintingBackend.Current.IsSupported);
    }

    [Fact]
    public void BufferedImageRoundTripsPixelsAndPngEncoding()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Skip("System.Drawing.Common is supported only on Windows.");
        }

        SystemDrawingRenderingBackend.Register();

        using BufferedImage image = new(2, 2, BufferedImage.TYPE_INT_ARGB);
        image.SetRgb(0, 0, unchecked((int)0x80402010));
        Assert.Equal(unchecked((int)0x80402010), image.GetRgb(0, 0));

        using Graphics2D graphics = image.CreateGraphics();
        graphics.SetBackground(new Color(1, 2, 3, 4));
        graphics.ClearRect(1, 1, 1, 1);
        Assert.Equal(unchecked((int)0x04010203), image.GetRgb(1, 1));

        byte[] encoded = RenderingBackend.Current.ImageCodec.Encode(image, EncodedImageFormat.Png, 100);
        Assert.NotEmpty(encoded);

        using BufferedImage? decoded = RenderingBackend.Current.ImageCodec.Decode(encoded);
        Assert.NotNull(decoded);
        Assert.Equal(image.Width, decoded.Width);
        Assert.Equal(image.Height, decoded.Height);
    }

    [Fact]
    public void BulkPngEncodingHonorsRgbRgbaAndRowStride()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Skip("System.Drawing.Common is supported only on Windows.");
        }

        SystemDrawingRenderingBackend.Register();

        byte[] rgb =
        [
            255, 0, 0, 0, 255, 0, 91, 92,
            0, 0, 255, 255, 255, 255, 93, 94
        ];
        byte[] rgbPng = RenderingBackend.Current.ImageCodec.EncodePng(
            new InterleavedPixelData(rgb, 2, 2, 8, InterleavedPixelFormat.Rgb24));
        using BufferedImage rgbImage = RenderingBackend.Current.ImageCodec.Decode(rgbPng)!;
        Assert.Equal(unchecked((int)0xFFFF0000), rgbImage.GetRgb(0, 0));
        Assert.Equal(unchecked((int)0xFF00FF00), rgbImage.GetRgb(1, 0));
        Assert.Equal(unchecked((int)0xFF0000FF), rgbImage.GetRgb(0, 1));
        Assert.Equal(unchecked((int)0xFFFFFFFF), rgbImage.GetRgb(1, 1));

        byte[] rgba = [64, 32, 16, 128, 8, 16, 24, 255, 95, 96];
        byte[] rgbaPng = RenderingBackend.Current.ImageCodec.EncodePng(
            new InterleavedPixelData(rgba, 2, 1, 10, InterleavedPixelFormat.Rgba32));
        using BufferedImage rgbaImage = RenderingBackend.Current.ImageCodec.Decode(rgbaPng)!;
        Assert.Equal(unchecked((int)0x80402010), rgbaImage.GetRgb(0, 0));
        Assert.Equal(unchecked((int)0xFF081018), rgbaImage.GetRgb(1, 0));
    }
}
