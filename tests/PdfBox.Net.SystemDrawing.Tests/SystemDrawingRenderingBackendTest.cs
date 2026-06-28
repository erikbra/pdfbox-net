using PdfBox.Net.Rendering;

namespace PdfBox.Net.SystemDrawing.Tests;

public class SystemDrawingRenderingBackendTest
{
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
}
