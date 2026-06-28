using PdfBox.Net.Rendering;

namespace PdfBox.Net.MauiGraphics.Tests;

public class MauiGraphicsRenderingBackendTest
{
    [Fact]
    public void BufferedImageRoundTripsPixelsAndPngEncoding()
    {
        MauiGraphicsRenderingBackend.Register();

        using BufferedImage image = new(2, 2, BufferedImage.TYPE_INT_ARGB);
        image.SetRgb(0, 0, unchecked((int)0x80402010));
        Assert.Equal(unchecked((int)0x80402010), image.GetRgb(0, 0));

        using Graphics2D graphics = image.CreateGraphics();
        graphics.SetBackground(new Color(64, 32, 16, 128));
        graphics.ClearRect(1, 1, 1, 1);
        Assert.Equal(unchecked((int)0x80402010), image.GetRgb(1, 1));

        byte[] encoded = RenderingBackend.Current.ImageCodec.Encode(image, EncodedImageFormat.Png, 100);
        Assert.NotEmpty(encoded);

        using BufferedImage? decoded = RenderingBackend.Current.ImageCodec.Decode(encoded);
        Assert.NotNull(decoded);
        Assert.Equal(image.Width, decoded.Width);
        Assert.Equal(image.Height, decoded.Height);
    }
}
