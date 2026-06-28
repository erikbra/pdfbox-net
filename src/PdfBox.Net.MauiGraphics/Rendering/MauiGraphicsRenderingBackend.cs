/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Microsoft.Maui.Graphics backend for Java AWT/ImageIO proxy types.
 *
 * PORT_MODE: native-adapter
 */

using Microsoft.Maui.Graphics.Skia;
using PdfBox.Net.PDModel.Annotations;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.Util;
using SkiaSharp;
using MauiImage = Microsoft.Maui.Graphics.IImage;
using MauiImageFormat = Microsoft.Maui.Graphics.ImageFormat;
using PdfColor = PdfBox.Net.Rendering.Color;

namespace PdfBox.Net.Rendering;

public sealed class MauiGraphicsRenderingBackend : IRenderingBackend
{
    public static MauiGraphicsRenderingBackend Instance { get; } = new();

    private MauiGraphicsRenderingBackend()
    {
    }

    public IImageCodecPeer ImageCodec { get; } = new MauiGraphicsImageCodecPeer();

    public static void Register()
    {
        RenderingBackend.Register(Instance);
    }

    public IBufferedImagePeer CreateBufferedImage(int width, int height, int type)
    {
        return new MauiGraphicsBufferedImagePeer(width, height, type);
    }

    public IPageDrawerPeer CreatePageDrawerPeer(PageDrawer owner, PageDrawerParameters parameters)
    {
        return new MauiGraphicsPageDrawerPeer();
    }
}

internal sealed class MauiGraphicsBufferedImagePeer : IBufferedImagePeer
{
    private readonly SkiaBitmapExportContext _context;

    internal MauiGraphicsBufferedImagePeer(int width, int height, int type)
    {
        Width = width;
        Height = height;
        Type = type;
        _context = CreateContext(width, height, type);
    }

    internal MauiGraphicsBufferedImagePeer(SKBitmap bitmap)
    {
        Width = bitmap.Width;
        Height = bitmap.Height;
        Type = bitmap.AlphaType == SKAlphaType.Opaque ? BufferedImage.TYPE_INT_RGB : BufferedImage.TYPE_INT_ARGB;
        _context = CreateContext(Width, Height, Type);
        using var canvas = new SKCanvas(_context.Bitmap);
        canvas.Clear(Type == BufferedImage.TYPE_INT_ARGB ? SKColors.Transparent : SKColors.White);
        canvas.DrawBitmap(bitmap, 0, 0);
    }

    internal SKBitmap Bitmap => _context.Bitmap;

    public int Width { get; }

    public int Height { get; }

    public int Type { get; }

    public int GetRgb(int x, int y)
    {
        SKColor color = _context.Bitmap.GetPixel(x, y);
        return (color.Alpha << 24) | (color.Red << 16) | (color.Green << 8) | color.Blue;
    }

    public void SetPixel(int x, int y, int red, int green, int blue, int alpha)
    {
        if (Type != BufferedImage.TYPE_INT_ARGB)
        {
            alpha = 255;
        }

        _context.Bitmap.SetPixel(
            x,
            y,
            new SKColor(
                (byte)Math.Clamp(red, 0, 255),
                (byte)Math.Clamp(green, 0, 255),
                (byte)Math.Clamp(blue, 0, 255),
                (byte)Math.Clamp(alpha, 0, 255)));
    }

    public void Clear(PdfColor color)
    {
        FillRectPixels(0, 0, Width, Height, color);
    }

    public IGraphics2DPeer CreateGraphics()
    {
        return new MauiGraphicsGraphics2DPeer(this);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    internal void FillRectPixels(int x, int y, int width, int height, PdfColor color)
    {
        SKColor skColor = ToSkColor(color, Type);
        int left = Math.Clamp(x, 0, Width);
        int top = Math.Clamp(y, 0, Height);
        int right = Math.Clamp(x + width, 0, Width);
        int bottom = Math.Clamp(y + height, 0, Height);
        for (int yy = top; yy < bottom; yy++)
        {
            for (int xx = left; xx < right; xx++)
            {
                _context.Bitmap.SetPixel(xx, yy, skColor);
            }
        }
    }

    internal static SKColor ToSkColor(PdfColor color, int imageType = BufferedImage.TYPE_INT_ARGB)
    {
        return new SKColor(
            (byte)Math.Clamp(color.Red, 0, 255),
            (byte)Math.Clamp(color.Green, 0, 255),
            (byte)Math.Clamp(color.Blue, 0, 255),
            imageType == BufferedImage.TYPE_INT_ARGB ? (byte)Math.Clamp(color.Alpha, 0, 255) : byte.MaxValue);
    }

    private static SkiaBitmapExportContext CreateContext(int width, int height, int type)
    {
        return new SkiaBitmapExportContext(
            width,
            height,
            displayScale: 1,
            dpi: 72,
            disposeBitmap: true,
            transparent: type == BufferedImage.TYPE_INT_ARGB);
    }
}

internal sealed class MauiGraphicsGraphics2DPeer : IGraphics2DPeer
{
    private readonly MauiGraphicsBufferedImagePeer? _image;
    private SKCanvas? _canvas;
    private readonly bool _ownsCanvas;
    private readonly bool _ownsCanvasState;
    private bool _isTransformed;

    internal MauiGraphicsGraphics2DPeer(MauiGraphicsBufferedImagePeer image)
    {
        _image = image;
        _canvas = new SKCanvas(image.Bitmap);
        _ownsCanvas = true;
    }

    private MauiGraphicsGraphics2DPeer(
        MauiGraphicsBufferedImagePeer? image,
        SKCanvas? canvas,
        bool ownsCanvasState,
        bool isTransformed)
    {
        _image = image;
        _canvas = canvas;
        _ownsCanvasState = ownsCanvasState;
        _isTransformed = isTransformed;
    }

    public int? BitmapHeight => _image?.Height;

    public IGraphics2DPeer Create()
    {
        _canvas?.Save();
        return new MauiGraphicsGraphics2DPeer(_image, _canvas, ownsCanvasState: true, isTransformed: _isTransformed);
    }

    public void ClearRect(int x, int y, int width, int height, PdfColor background)
    {
        if (_canvas is null)
        {
            return;
        }

        if (!_isTransformed && _image is not null)
        {
            _image.FillRectPixels(x, y, width, height, background);
            return;
        }

        using var paint = new SKPaint
        {
            Color = MauiGraphicsBufferedImagePeer.ToSkColor(background, _image?.Type ?? BufferedImage.TYPE_INT_ARGB),
            BlendMode = SKBlendMode.Src,
            Style = SKPaintStyle.Fill
        };
        _canvas.DrawRect(x, y, width, height, paint);
    }

    public void DrawImage(BufferedImage image, int x, int y)
    {
        if (_canvas is null)
        {
            return;
        }

        using SKBitmap bitmap = MauiGraphicsImageCodecPeer.ToBitmap(image);
        _canvas.DrawBitmap(bitmap, x, y);
    }

    public void Rotate(double theta)
    {
        _isTransformed = true;
        _canvas?.RotateRadians((float)theta);
    }

    public void Scale(double scaleX, double scaleY)
    {
        _isTransformed = true;
        _canvas?.Scale((float)scaleX, (float)scaleY);
    }

    public void Translate(double tx, double ty)
    {
        _isTransformed = true;
        _canvas?.Translate((float)tx, (float)ty);
    }

    public void Dispose()
    {
        if (_ownsCanvasState)
        {
            _canvas?.Restore();
        }

        if (_ownsCanvas)
        {
            _canvas?.Dispose();
            _canvas = null;
        }
    }
}

internal sealed class MauiGraphicsImageCodecPeer : IImageCodecPeer
{
    public BufferedImage? Decode(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using MauiImage? image = new SkiaImageLoadingService().FromStream(stream, MauiImageFormat.Png);
        return image is SkiaImage skiaImage
            ? new BufferedImage(new MauiGraphicsBufferedImagePeer(skiaImage.PlatformRepresentation))
            : null;
    }

    public byte[] Encode(BufferedImage image, EncodedImageFormat format, int quality)
    {
        using var mauiImage = new SkiaImage(ToBitmap(image));
        using var stream = new MemoryStream();
        mauiImage.Save(stream, ToMauiFormat(format), Math.Clamp(quality, 0, 100) / 100f);
        return stream.ToArray();
    }

    internal static SKBitmap ToBitmap(BufferedImage image)
    {
        if (image.Peer is MauiGraphicsBufferedImagePeer peer)
        {
            return peer.Bitmap.Copy();
        }

        var bitmap = new SKBitmap(
            image.Width,
            image.Height,
            SKColorType.Bgra8888,
            image.Type == BufferedImage.TYPE_INT_ARGB ? SKAlphaType.Premul : SKAlphaType.Opaque);
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                int argb = image.GetRgb(x, y);
                bitmap.SetPixel(
                    x,
                    y,
                    new SKColor(
                        (byte)((argb >> 16) & 0xFF),
                        (byte)((argb >> 8) & 0xFF),
                        (byte)(argb & 0xFF),
                        (byte)((argb >> 24) & 0xFF)));
            }
        }

        return bitmap;
    }

    private static MauiImageFormat ToMauiFormat(EncodedImageFormat format)
    {
        return format == EncodedImageFormat.Jpeg ? MauiImageFormat.Jpeg : MauiImageFormat.Png;
    }
}

internal sealed class MauiGraphicsPageDrawerPeer : IPageDrawerPeer
{
    private AnnotationFilter _annotationFilter = _ => true;

    public AnnotationFilter GetAnnotationFilter() => _annotationFilter;

    public void SetAnnotationFilter(AnnotationFilter annotationFilter)
    {
        _annotationFilter = annotationFilter ?? throw new ArgumentNullException(nameof(annotationFilter));
    }

    public void DrawPage(Graphics2D graphics, PDRectangle pageSize)
    {
        throw CreateNotSupported();
    }

    public void ShowAnnotation(PDAnnotation annotation)
    {
        throw CreateNotSupported();
    }

    public void ShowForm(PDFormXObject form)
    {
        throw CreateNotSupported();
    }

    public void ShowTransparencyGroup(PDTransparencyGroup form)
    {
        throw CreateNotSupported();
    }

    public int GetSubsampling(PDImage pdImage, AffineTransform at)
    {
        return 1;
    }

    public void ShowTransparencyGroupOnGraphics(PDTransparencyGroup form, Graphics2D graphics)
    {
        throw CreateNotSupported();
    }

    public void Dispose()
    {
    }

    private static NotSupportedException CreateNotSupported()
    {
        return new NotSupportedException(
            "PdfBox.Net.MauiGraphics currently implements BufferedImage, Graphics2D, and ImageIO-style operations only. " +
            "Full PDF page rendering still requires PdfBox.Net.SkiaSharp.");
    }
}
