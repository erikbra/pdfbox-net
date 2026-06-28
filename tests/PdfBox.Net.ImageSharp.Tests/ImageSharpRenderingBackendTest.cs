using PdfBox.Net.Rendering;

namespace PdfBox.Net.ImageSharp.Tests;

public class ImageSharpRenderingBackendTest
{
    [Fact]
    public void BufferedImageRoundTripsPixelsAndPngEncoding()
    {
        ImageSharpRenderingBackend.Register();

        using BufferedImage image = new(2, 2, BufferedImage.TYPE_INT_ARGB);
        image.SetRgb(0, 0, unchecked((int)0x04010203));
        Assert.Equal(unchecked((int)0x04010203), image.GetRgb(0, 0));

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
        Assert.Equal(image.GetRgb(0, 0), decoded.GetRgb(0, 0));
    }
}
