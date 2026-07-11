/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * SkiaSharp backend for Java AWT/ImageIO proxy types.
 *
 * PORT_MODE: native-adapter
 */

using PdfBox.Net.PDModel.Common;
using SkiaSharp;

namespace PdfBox.Net.Rendering;

public sealed class SkiaRenderingBackend : IRenderingBackend
{
    public static SkiaRenderingBackend Instance { get; } = new();
    internal static readonly SKSamplingOptions ImageSamplingOptions = new(SKFilterMode.Linear, SKMipmapMode.Linear);
    internal static readonly SKSamplingOptions DctImageSamplingOptions = new(SKCubicResampler.Mitchell);

    private SkiaRenderingBackend()
    {
    }

    public IImageCodecPeer ImageCodec { get; } = new SkiaImageCodecPeer();

    public static void Register()
    {
        RenderingBackend.Register(Instance);
    }

    public IBufferedImagePeer CreateBufferedImage(int width, int height, int type)
    {
        return new SkiaBufferedImagePeer(width, height, type);
    }

    public IPageDrawerPeer CreatePageDrawerPeer(PageDrawer owner, PageDrawerParameters parameters)
    {
        return new SkiaPageDrawerPeer(owner, parameters);
    }
}

internal sealed class SkiaBufferedImagePeer : IBufferedImagePeer
{
    private readonly SKBitmap _bitmap;

    internal SkiaBufferedImagePeer(int width, int height, int type)
    {
        Width = width;
        Height = height;
        Type = type;
        SKAlphaType alphaType = type == BufferedImage.TYPE_INT_ARGB ? SKAlphaType.Premul : SKAlphaType.Opaque;
        _bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, alphaType);
    }

    internal SkiaBufferedImagePeer(SKBitmap bitmap, int type)
    {
        _bitmap = bitmap.Copy(SKColorType.Bgra8888);
        Width = _bitmap.Width;
        Height = _bitmap.Height;
        Type = type;
    }

    public int Width { get; }

    public int Height { get; }

    public int Type { get; }

    internal SKBitmap Bitmap => _bitmap;

    public int GetRgb(int x, int y)
    {
        SKColor c = _bitmap.GetPixel(x, y);
        return (c.Alpha << 24) | (c.Red << 16) | (c.Green << 8) | c.Blue;
    }

    public void SetPixel(int x, int y, int red, int green, int blue, int alpha)
    {
        _bitmap.SetPixel(
            x,
            y,
            new SKColor(
                (byte)Math.Clamp(red, 0, 255),
                (byte)Math.Clamp(green, 0, 255),
                (byte)Math.Clamp(blue, 0, 255),
                (byte)Math.Clamp(alpha, 0, 255)));
    }

    public void Clear(Color color)
    {
        _bitmap.Erase(ToSkColor(color));
    }

    public IGraphics2DPeer CreateGraphics()
    {
        return new SkiaGraphics2DPeer(this);
    }

    public void Dispose()
    {
        _bitmap.Dispose();
    }

    internal static SKColor ToSkColor(Color color)
    {
        return new SKColor((byte)color.Red, (byte)color.Green, (byte)color.Blue, (byte)color.Alpha);
    }
}

internal sealed class SkiaGraphics2DPeer : IGraphics2DPeer
{
    private readonly SkiaBufferedImagePeer? _image;
    private SKCanvas? _canvas;
    private readonly bool _ownsCanvas;

    internal SkiaGraphics2DPeer(SkiaBufferedImagePeer image)
    {
        _image = image;
        _canvas = new SKCanvas(image.Bitmap);
        _ownsCanvas = true;
    }

    private SkiaGraphics2DPeer(SkiaBufferedImagePeer? image, SKCanvas? canvas, bool ownsCanvas)
    {
        _image = image;
        _canvas = canvas;
        _ownsCanvas = ownsCanvas;
    }

    public int? BitmapHeight => _image?.Height;

    internal (int Width, int Height)? BitmapSize => _image is null ? null : (_image.Width, _image.Height);

    internal SKBitmap? Bitmap => _image?.Bitmap;

    internal SKCanvas? Canvas => _canvas;

    public IGraphics2DPeer Create()
    {
        return new SkiaGraphics2DPeer(_image, _canvas, ownsCanvas: false);
    }

    public void ClearRect(int x, int y, int width, int height, Color background)
    {
        if (_canvas is null)
        {
            return;
        }

        using var paint = new SKPaint { Color = SkiaBufferedImagePeer.ToSkColor(background), BlendMode = SKBlendMode.Src };
        _canvas.DrawRect(x, y, width, height, paint);
    }

    public void DrawImage(BufferedImage image, int x, int y)
    {
        if (_canvas is null)
        {
            return;
        }

        _canvas.DrawBitmap(image.GetSkiaBitmap(), x, y, SkiaRenderingBackend.ImageSamplingOptions, paint: null);
    }

    public void Rotate(double theta)
    {
        _canvas?.RotateRadians((float)theta);
    }

    public void Scale(double scaleX, double scaleY)
    {
        _canvas?.Scale((float)scaleX, (float)scaleY);
    }

    public void Translate(double tx, double ty)
    {
        _canvas?.Translate((float)tx, (float)ty);
    }

    public void Dispose()
    {
        if (_ownsCanvas)
        {
            _canvas?.Dispose();
            _canvas = null;
        }
    }
}

internal sealed class SkiaImageCodecPeer : IImageCodecPeer
{
    public BufferedImage? Decode(byte[] data)
    {
        using SKBitmap? bitmap = SKBitmap.Decode(data);
        return bitmap is null ? null : CreateBufferedImage(bitmap);
    }

    public byte[] Encode(BufferedImage image, EncodedImageFormat format, int quality)
    {
        using SKImage skImage = SKImage.FromBitmap(image.GetSkiaBitmap());
        SKEncodedImageFormat skFormat = format switch
        {
            EncodedImageFormat.Jpeg => SKEncodedImageFormat.Jpeg,
            _ => SKEncodedImageFormat.Png
        };
        using SKData data = skImage.Encode(skFormat, quality);
        return data.ToArray();
    }

    internal static BufferedImage CreateBufferedImage(SKBitmap bitmap)
    {
        int type = bitmap.AlphaType == SKAlphaType.Opaque ? BufferedImage.TYPE_INT_RGB : BufferedImage.TYPE_INT_ARGB;
        return new BufferedImage(new SkiaBufferedImagePeer(bitmap, type));
    }
}

internal static class SkiaPeerExtensions
{
    internal static SKBitmap GetSkiaBitmap(this BufferedImage image)
    {
        return image.Peer is SkiaBufferedImagePeer peer
            ? peer.Bitmap
            : throw new InvalidOperationException("BufferedImage is not backed by the SkiaSharp rendering backend.");
    }

    internal static SKCanvas? GetSkiaCanvas(this Graphics2D graphics)
    {
        return graphics.Peer is SkiaGraphics2DPeer peer ? peer.Canvas : null;
    }

    internal static (int Width, int Height)? GetSkiaBitmapSize(this Graphics2D graphics)
    {
        return graphics.Peer is SkiaGraphics2DPeer peer ? peer.BitmapSize : null;
    }

    internal static SKBitmap? GetSkiaBitmap(this Graphics2D graphics)
    {
        return graphics.Peer is SkiaGraphics2DPeer peer ? peer.Bitmap : null;
    }

}
