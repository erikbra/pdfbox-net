/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Backend SPI for Java AWT/ImageIO proxy types.
 *
 * PORT_MODE: native-adapter
 */

using PdfBox.Net.PDModel.Annotations;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.Util;

namespace PdfBox.Net.Rendering;

public enum EncodedImageFormat
{
    Png,
    Jpeg
}

public interface IBufferedImagePeer : IDisposable
{
    int Width { get; }

    int Height { get; }

    int Type { get; }

    int GetRgb(int x, int y);

    void SetPixel(int x, int y, int red, int green, int blue, int alpha);

    void Clear(Color color);

    IGraphics2DPeer CreateGraphics();
}

public interface IGraphics2DPeer : IDisposable
{
    int? BitmapHeight { get; }

    IGraphics2DPeer Create();

    void ClearRect(int x, int y, int width, int height, Color background);

    void DrawImage(BufferedImage image, int x, int y);

    void Rotate(double theta);

    void Scale(double scaleX, double scaleY);

    void Translate(double tx, double ty);
}

public interface IImageCodecPeer
{
    BufferedImage? Decode(byte[] data);

    byte[] Encode(BufferedImage image, EncodedImageFormat format, int quality);
}

public interface IPageDrawerPeer : IDisposable
{
    AnnotationFilter GetAnnotationFilter();

    void SetAnnotationFilter(AnnotationFilter annotationFilter);

    void DrawPage(Graphics2D graphics, PDRectangle pageSize);

    void ShowAnnotation(PDAnnotation annotation);

    void ShowForm(PDFormXObject form);

    void ShowTransparencyGroup(PDTransparencyGroup form);

    int GetSubsampling(PDImage pdImage, AffineTransform at);

    void ShowTransparencyGroupOnGraphics(PDTransparencyGroup form, Graphics2D graphics);
}

public interface IRenderingBackend
{
    IImageCodecPeer ImageCodec { get; }

    IBufferedImagePeer CreateBufferedImage(int width, int height, int type);

    IPageDrawerPeer CreatePageDrawerPeer(PageDrawer owner, PageDrawerParameters parameters);
}

public static class RenderingBackend
{
    private static IRenderingBackend? _current;

    public static bool IsRegistered => _current is not null;

    public static IRenderingBackend Current =>
        _current ?? throw new InvalidOperationException(
            "No PdfBox.Net rendering backend is registered. Reference an optional backend package, " +
            "such as PdfBox.Net.SkiaSharp, and call its registration method before rendering or image decoding.");

    public static void Register(IRenderingBackend backend)
    {
        _current = backend ?? throw new ArgumentNullException(nameof(backend));
    }
}

internal sealed class NullGraphics2DPeer : IGraphics2DPeer
{
    internal static readonly NullGraphics2DPeer Instance = new();

    private NullGraphics2DPeer()
    {
    }

    public int? BitmapHeight => null;

    public IGraphics2DPeer Create() => this;

    public void ClearRect(int x, int y, int width, int height, Color background)
    {
    }

    public void DrawImage(BufferedImage image, int x, int y)
    {
    }

    public void Rotate(double theta)
    {
    }

    public void Scale(double scaleX, double scaleY)
    {
    }

    public void Translate(double tx, double ty)
    {
    }

    public void Dispose()
    {
    }
}
