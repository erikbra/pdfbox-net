/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * System.Drawing backend for Java AWT/ImageIO proxy types.
 *
 * PORT_MODE: native-adapter
 */

using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using PdfBox.Net.PDModel.Annotations;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.Util;
using GdiBitmap = System.Drawing.Bitmap;
using GdiBrush = System.Drawing.SolidBrush;
using GdiColor = System.Drawing.Color;
using GdiEncoder = System.Drawing.Imaging.Encoder;
using GdiGraphics = System.Drawing.Graphics;
using GdiImage = System.Drawing.Image;
using GdiImageFormat = System.Drawing.Imaging.ImageFormat;
using PdfColor = PdfBox.Net.Rendering.Color;

namespace PdfBox.Net.Rendering;

[SupportedOSPlatform("windows")]
public sealed class SystemDrawingRenderingBackend : IRenderingBackend
{
    public static SystemDrawingRenderingBackend Instance { get; } = new();

    private SystemDrawingRenderingBackend()
    {
    }

    public IImageCodecPeer ImageCodec { get; } = new SystemDrawingImageCodecPeer();

    public static void Register()
    {
        RenderingBackend.Register(Instance);
    }

    public IBufferedImagePeer CreateBufferedImage(int width, int height, int type)
    {
        return new SystemDrawingBufferedImagePeer(width, height, type);
    }

    public IPageDrawerPeer CreatePageDrawerPeer(PageDrawer owner, PageDrawerParameters parameters)
    {
        return new SystemDrawingPageDrawerPeer();
    }
}

[SupportedOSPlatform("windows")]
internal sealed class SystemDrawingBufferedImagePeer : IBufferedImagePeer
{
    private readonly GdiBitmap _bitmap;

    internal SystemDrawingBufferedImagePeer(int width, int height, int type)
    {
        Width = width;
        Height = height;
        Type = type;
        _bitmap = new GdiBitmap(width, height, ToPixelFormat(type));
    }

    internal SystemDrawingBufferedImagePeer(GdiImage image)
    {
        Width = image.Width;
        Height = image.Height;
        Type = GdiImage.IsAlphaPixelFormat(image.PixelFormat) ? BufferedImage.TYPE_INT_ARGB : BufferedImage.TYPE_INT_RGB;
        _bitmap = new GdiBitmap(Width, Height, ToPixelFormat(Type));
        using GdiGraphics graphics = GdiGraphics.FromImage(_bitmap);
        graphics.DrawImage(image, 0, 0, Width, Height);
    }

    internal GdiBitmap Bitmap => _bitmap;

    public int Width { get; }

    public int Height { get; }

    public int Type { get; }

    public int GetRgb(int x, int y)
    {
        GdiColor color = _bitmap.GetPixel(x, y);
        return (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
    }

    public void SetPixel(int x, int y, int red, int green, int blue, int alpha)
    {
        if (Type != BufferedImage.TYPE_INT_ARGB)
        {
            alpha = 255;
        }

        _bitmap.SetPixel(x, y, GdiColor.FromArgb(
            Math.Clamp(alpha, 0, 255),
            Math.Clamp(red, 0, 255),
            Math.Clamp(green, 0, 255),
            Math.Clamp(blue, 0, 255)));
    }

    public void Clear(PdfColor color)
    {
        using GdiGraphics graphics = GdiGraphics.FromImage(_bitmap);
        CompositingMode oldMode = graphics.CompositingMode;
        graphics.CompositingMode = CompositingMode.SourceCopy;
        graphics.Clear(ToGdiColor(color));
        graphics.CompositingMode = oldMode;
    }

    public IGraphics2DPeer CreateGraphics()
    {
        return new SystemDrawingGraphics2DPeer(this);
    }

    public void Dispose()
    {
        _bitmap.Dispose();
    }

    internal static GdiColor ToGdiColor(PdfColor color)
    {
        return GdiColor.FromArgb(
            Math.Clamp(color.Alpha, 0, 255),
            Math.Clamp(color.Red, 0, 255),
            Math.Clamp(color.Green, 0, 255),
            Math.Clamp(color.Blue, 0, 255));
    }

    private static PixelFormat ToPixelFormat(int type)
    {
        return type == BufferedImage.TYPE_INT_ARGB
            ? PixelFormat.Format32bppArgb
            : PixelFormat.Format32bppRgb;
    }
}

[SupportedOSPlatform("windows")]
internal sealed class SystemDrawingGraphics2DPeer : IGraphics2DPeer
{
    private readonly SystemDrawingBufferedImagePeer? _image;
    private GdiGraphics? _graphics;
    private readonly bool _ownsGraphics;

    internal SystemDrawingGraphics2DPeer(SystemDrawingBufferedImagePeer image)
    {
        _image = image;
        _graphics = GdiGraphics.FromImage(image.Bitmap);
        _graphics.SmoothingMode = SmoothingMode.AntiAlias;
        _graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        _ownsGraphics = true;
    }

    private SystemDrawingGraphics2DPeer(SystemDrawingBufferedImagePeer? image, GdiGraphics? graphics, bool ownsGraphics)
    {
        _image = image;
        _graphics = graphics;
        _ownsGraphics = ownsGraphics;
    }

    public int? BitmapHeight => _image?.Height;

    public IGraphics2DPeer Create()
    {
        return new SystemDrawingGraphics2DPeer(_image, _graphics, ownsGraphics: false);
    }

    public void ClearRect(int x, int y, int width, int height, PdfColor background)
    {
        if (_graphics is null)
        {
            return;
        }

        CompositingMode oldMode = _graphics.CompositingMode;
        _graphics.CompositingMode = CompositingMode.SourceCopy;
        using var brush = new GdiBrush(SystemDrawingBufferedImagePeer.ToGdiColor(background));
        _graphics.FillRectangle(brush, x, y, width, height);
        _graphics.CompositingMode = oldMode;
    }

    public void DrawImage(BufferedImage image, int x, int y)
    {
        if (_graphics is null)
        {
            return;
        }

        using GdiBitmap bitmap = SystemDrawingImageCodecPeer.ToBitmap(image);
        _graphics.DrawImageUnscaled(bitmap, x, y);
    }

    public void Rotate(double theta)
    {
        _graphics?.RotateTransform((float)(theta * 180d / Math.PI));
    }

    public void Scale(double scaleX, double scaleY)
    {
        _graphics?.ScaleTransform((float)scaleX, (float)scaleY);
    }

    public void Translate(double tx, double ty)
    {
        _graphics?.TranslateTransform((float)tx, (float)ty);
    }

    public void Dispose()
    {
        if (_ownsGraphics)
        {
            _graphics?.Dispose();
            _graphics = null;
        }
    }
}

[SupportedOSPlatform("windows")]
internal sealed class SystemDrawingImageCodecPeer : IImageCodecPeer
{
    public BufferedImage? Decode(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using GdiImage image = GdiImage.FromStream(stream);
        return new BufferedImage(new SystemDrawingBufferedImagePeer(image));
    }

    public byte[] Encode(BufferedImage image, EncodedImageFormat format, int quality)
    {
        using GdiBitmap bitmap = ToBitmap(image);
        using var stream = new MemoryStream();
        if (format == EncodedImageFormat.Jpeg)
        {
            SaveJpeg(bitmap, stream, quality);
        }
        else
        {
            bitmap.Save(stream, GdiImageFormat.Png);
        }

        return stream.ToArray();
    }

    public byte[] EncodePng(InterleavedPixelData pixels)
    {
        PixelFormat pixelFormat = pixels.PixelFormat == InterleavedPixelFormat.Rgba32
            ? PixelFormat.Format32bppArgb
            : PixelFormat.Format24bppRgb;
        using var bitmap = new GdiBitmap(pixels.Width, pixels.Height, pixelFormat);
        System.Drawing.Rectangle bounds = new(0, 0, pixels.Width, pixels.Height);
        BitmapData bitmapData = bitmap.LockBits(bounds, ImageLockMode.WriteOnly, pixelFormat);
        try
        {
            int destinationBytesPerPixel = pixels.PixelFormat == InterleavedPixelFormat.Rgba32 ? 4 : 3;
            byte[] destinationRow = GC.AllocateUninitializedArray<byte>(checked(pixels.Width * destinationBytesPerPixel));
            for (int y = 0; y < pixels.Height; y++)
            {
                int sourceOffset = y * pixels.RowStride;
                int destinationOffset = 0;
                for (int x = 0; x < pixels.Width; x++)
                {
                    byte red = pixels.Data[sourceOffset++];
                    byte green = pixels.Data[sourceOffset++];
                    byte blue = pixels.Data[sourceOffset++];
                    destinationRow[destinationOffset++] = blue;
                    destinationRow[destinationOffset++] = green;
                    destinationRow[destinationOffset++] = red;
                    if (pixels.PixelFormat == InterleavedPixelFormat.Rgba32)
                    {
                        destinationRow[destinationOffset++] = pixels.Data[sourceOffset++];
                    }
                }

                Marshal.Copy(
                    destinationRow,
                    0,
                    IntPtr.Add(bitmapData.Scan0, y * bitmapData.Stride),
                    destinationRow.Length);
            }
        }
        finally
        {
            bitmap.UnlockBits(bitmapData);
        }

        using var stream = new MemoryStream();
        bitmap.Save(stream, GdiImageFormat.Png);
        return stream.ToArray();
    }

    internal static GdiBitmap ToBitmap(BufferedImage image)
    {
        if (image.Peer is SystemDrawingBufferedImagePeer peer)
        {
            return new GdiBitmap(peer.Bitmap);
        }

        var bitmap = new GdiBitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                int argb = image.GetRgb(x, y);
                bitmap.SetPixel(
                    x,
                    y,
                    GdiColor.FromArgb(
                        (argb >> 24) & 0xFF,
                        (argb >> 16) & 0xFF,
                        (argb >> 8) & 0xFF,
                        argb & 0xFF));
            }
        }

        return bitmap;
    }

    private static void SaveJpeg(GdiBitmap bitmap, Stream stream, int quality)
    {
        ImageCodecInfo? jpegCodec = ImageCodecInfo.GetImageEncoders()
            .FirstOrDefault(codec => codec.FormatID == GdiImageFormat.Jpeg.Guid);
        if (jpegCodec is null)
        {
            bitmap.Save(stream, GdiImageFormat.Jpeg);
            return;
        }

        using var parameters = new EncoderParameters(1);
        parameters.Param[0] = new EncoderParameter(GdiEncoder.Quality, Math.Clamp(quality, 0, 100));
        bitmap.Save(stream, jpegCodec, parameters);
    }
}

internal sealed class SystemDrawingPageDrawerPeer : IPageDrawerPeer
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
            "PdfBox.Net.SystemDrawing currently implements BufferedImage, Graphics2D, and ImageIO-style operations only. " +
            "Full PDF page rendering still requires PdfBox.Net.SkiaSharp.");
    }
}
