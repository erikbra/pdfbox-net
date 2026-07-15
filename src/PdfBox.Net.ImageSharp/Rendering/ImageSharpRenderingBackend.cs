/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * ImageSharp backend for Java AWT/ImageIO proxy types.
 *
 * PORT_MODE: native-adapter
 */

using System.Numerics;
using PdfBox.Net.PDModel.Annotations;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.Util;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using ImageSharpImage = SixLabors.ImageSharp.Image;
using PdfColor = PdfBox.Net.Rendering.Color;
using RgbaImage = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;

namespace PdfBox.Net.Rendering;

public sealed class ImageSharpRenderingBackend : IRenderingBackend
{
    public static ImageSharpRenderingBackend Instance { get; } = new();

    private ImageSharpRenderingBackend()
    {
    }

    public IImageCodecPeer ImageCodec { get; } = new ImageSharpImageCodecPeer();

    public static void Register()
    {
        RenderingBackend.Register(Instance);
    }

    public IBufferedImagePeer CreateBufferedImage(int width, int height, int type)
    {
        return new ImageSharpBufferedImagePeer(width, height, type);
    }

    public IPageDrawerPeer CreatePageDrawerPeer(PageDrawer owner, PageDrawerParameters parameters)
    {
        return new ImageSharpPageDrawerPeer();
    }
}

internal sealed class ImageSharpBufferedImagePeer : IBufferedImagePeer
{
    private readonly RgbaImage _image;

    internal ImageSharpBufferedImagePeer(int width, int height, int type)
    {
        Width = width;
        Height = height;
        Type = type;
        _image = new RgbaImage(width, height);
        if (type != BufferedImage.TYPE_INT_ARGB)
        {
            FillRectPixels(0, 0, width, height, Color.White);
        }
    }

    internal ImageSharpBufferedImagePeer(RgbaImage image)
    {
        _image = image ?? throw new ArgumentNullException(nameof(image));
        Width = image.Width;
        Height = image.Height;
        Type = HasAlpha(image) ? BufferedImage.TYPE_INT_ARGB : BufferedImage.TYPE_INT_RGB;
    }

    internal RgbaImage Image => _image;

    public int Width { get; }

    public int Height { get; }

    public int Type { get; }

    public int GetRgb(int x, int y)
    {
        Rgba32 color = _image[x, y];
        return (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
    }

    public void SetPixel(int x, int y, int red, int green, int blue, int alpha)
    {
        _image[x, y] = ToRgba32(red, green, blue, Type == BufferedImage.TYPE_INT_ARGB ? alpha : 255);
    }

    public void Clear(PdfColor color)
    {
        FillRectPixels(0, 0, Width, Height, color);
    }

    public IGraphics2DPeer CreateGraphics()
    {
        return new ImageSharpGraphics2DPeer(this);
    }

    public void Dispose()
    {
        _image.Dispose();
    }

    internal void FillRectPixels(int x, int y, int width, int height, PdfColor color)
    {
        FillRectPixels(x, y, width, height, ToRgba32(color, Type));
    }

    internal void FillRectPixels(int x, int y, int width, int height, Rgba32 color)
    {
        int left = Math.Clamp(x, 0, Width);
        int top = Math.Clamp(y, 0, Height);
        int right = Math.Clamp(x + width, 0, Width);
        int bottom = Math.Clamp(y + height, 0, Height);
        for (int yy = top; yy < bottom; yy++)
        {
            for (int xx = left; xx < right; xx++)
            {
                _image[xx, yy] = color;
            }
        }
    }

    internal void DrawImageUnscaled(ImageSharpBufferedImagePeer source, int x, int y)
    {
        int left = Math.Max(0, x);
        int top = Math.Max(0, y);
        int right = Math.Min(Width, x + source.Width);
        int bottom = Math.Min(Height, y + source.Height);
        for (int yy = top; yy < bottom; yy++)
        {
            int sy = yy - y;
            for (int xx = left; xx < right; xx++)
            {
                int sx = xx - x;
                _image[xx, yy] = Blend(source.Image[sx, sy], _image[xx, yy]);
            }
        }
    }

    internal void DrawImageTransformed(ImageSharpBufferedImagePeer source, Matrix3x2 transform)
    {
        Vector2[] corners =
        [
            Vector2.Transform(new Vector2(0, 0), transform),
            Vector2.Transform(new Vector2(source.Width, 0), transform),
            Vector2.Transform(new Vector2(0, source.Height), transform),
            Vector2.Transform(new Vector2(source.Width, source.Height), transform)
        ];

        if (!TryGetBounds(corners, Width, Height, out int left, out int top, out int right, out int bottom) ||
            !Matrix3x2.Invert(transform, out Matrix3x2 inverse))
        {
            return;
        }

        for (int y = top; y < bottom; y++)
        {
            for (int x = left; x < right; x++)
            {
                Vector2 local = Vector2.Transform(new Vector2(x + 0.5f, y + 0.5f), inverse);
                int sx = (int)Math.Floor(local.X);
                int sy = (int)Math.Floor(local.Y);
                if (sx >= 0 && sy >= 0 && sx < source.Width && sy < source.Height)
                {
                    _image[x, y] = Blend(source.Image[sx, sy], _image[x, y]);
                }
            }
        }
    }

    internal void FillRectTransformed(int x, int y, int width, int height, PdfColor color, Matrix3x2 transform)
    {
        Vector2[] corners =
        [
            Vector2.Transform(new Vector2(x, y), transform),
            Vector2.Transform(new Vector2(x + width, y), transform),
            Vector2.Transform(new Vector2(x, y + height), transform),
            Vector2.Transform(new Vector2(x + width, y + height), transform)
        ];

        if (!TryGetBounds(corners, Width, Height, out int left, out int top, out int right, out int bottom) ||
            !Matrix3x2.Invert(transform, out Matrix3x2 inverse))
        {
            return;
        }

        Rgba32 fill = ToRgba32(color, Type);
        for (int yy = top; yy < bottom; yy++)
        {
            for (int xx = left; xx < right; xx++)
            {
                Vector2 local = Vector2.Transform(new Vector2(xx + 0.5f, yy + 0.5f), inverse);
                if (local.X >= x && local.Y >= y && local.X < x + width && local.Y < y + height)
                {
                    _image[xx, yy] = fill;
                }
            }
        }
    }

    internal static ImageSharpBufferedImagePeer FromBufferedImage(BufferedImage image)
    {
        if (image.Peer is ImageSharpBufferedImagePeer peer)
        {
            return peer;
        }

        var copy = new ImageSharpBufferedImagePeer(image.Width, image.Height, image.Type);
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                int argb = image.GetRgb(x, y);
                copy.Image[x, y] = new Rgba32(
                    (byte)((argb >> 16) & 0xFF),
                    (byte)((argb >> 8) & 0xFF),
                    (byte)(argb & 0xFF),
                    (byte)((argb >> 24) & 0xFF));
            }
        }

        return copy;
    }

    internal static Rgba32 ToRgba32(PdfColor color, int imageType = BufferedImage.TYPE_INT_ARGB)
    {
        return ToRgba32(color.Red, color.Green, color.Blue, imageType == BufferedImage.TYPE_INT_ARGB ? color.Alpha : 255);
    }

    private static Rgba32 ToRgba32(int red, int green, int blue, int alpha)
    {
        return new Rgba32(
            (byte)Math.Clamp(red, 0, 255),
            (byte)Math.Clamp(green, 0, 255),
            (byte)Math.Clamp(blue, 0, 255),
            (byte)Math.Clamp(alpha, 0, 255));
    }

    private static Rgba32 Blend(Rgba32 source, Rgba32 destination)
    {
        float sa = source.A / 255f;
        if (sa <= 0f)
        {
            return destination;
        }

        if (sa >= 1f)
        {
            return source;
        }

        float da = destination.A / 255f;
        float outA = sa + (da * (1f - sa));
        if (outA <= 0f)
        {
            return new Rgba32(0, 0, 0, 0);
        }

        byte r = ToByte(((source.R * sa) + (destination.R * da * (1f - sa))) / outA);
        byte g = ToByte(((source.G * sa) + (destination.G * da * (1f - sa))) / outA);
        byte b = ToByte(((source.B * sa) + (destination.B * da * (1f - sa))) / outA);
        byte a = ToByte(outA * 255f);
        return new Rgba32(r, g, b, a);
    }

    private static byte ToByte(float value)
    {
        return (byte)Math.Clamp((int)MathF.Round(value), 0, 255);
    }

    private static bool HasAlpha(RgbaImage image)
    {
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                if (image[x, y].A != 255)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool TryGetBounds(
        Vector2[] corners,
        int maxWidth,
        int maxHeight,
        out int left,
        out int top,
        out int right,
        out int bottom)
    {
        float minX = corners.Min(corner => corner.X);
        float minY = corners.Min(corner => corner.Y);
        float maxX = corners.Max(corner => corner.X);
        float maxY = corners.Max(corner => corner.Y);
        left = Math.Clamp((int)Math.Floor(minX), 0, maxWidth);
        top = Math.Clamp((int)Math.Floor(minY), 0, maxHeight);
        right = Math.Clamp((int)Math.Ceiling(maxX), 0, maxWidth);
        bottom = Math.Clamp((int)Math.Ceiling(maxY), 0, maxHeight);
        return left < right && top < bottom;
    }
}

internal sealed class ImageSharpGraphics2DPeer : IGraphics2DPeer
{
    private readonly ImageSharpBufferedImagePeer? _image;
    private Matrix3x2 _transform;

    internal ImageSharpGraphics2DPeer(ImageSharpBufferedImagePeer image)
        : this(image, Matrix3x2.Identity)
    {
    }

    private ImageSharpGraphics2DPeer(ImageSharpBufferedImagePeer? image, Matrix3x2 transform)
    {
        _image = image;
        _transform = transform;
    }

    public int? BitmapHeight => _image?.Height;

    public IGraphics2DPeer Create()
    {
        return new ImageSharpGraphics2DPeer(_image, _transform);
    }

    public void ClearRect(int x, int y, int width, int height, PdfColor background)
    {
        if (_image is null)
        {
            return;
        }

        if (_transform.IsIdentity)
        {
            _image.FillRectPixels(x, y, width, height, background);
        }
        else
        {
            _image.FillRectTransformed(x, y, width, height, background, _transform);
        }
    }

    public void DrawImage(BufferedImage image, int x, int y)
    {
        if (_image is null)
        {
            return;
        }

        ImageSharpBufferedImagePeer source = ImageSharpBufferedImagePeer.FromBufferedImage(image);
        try
        {
            if (_transform.IsIdentity)
            {
                _image.DrawImageUnscaled(source, x, y);
            }
            else
            {
                Matrix3x2 drawTransform = Matrix3x2.CreateTranslation(x, y) * _transform;
                _image.DrawImageTransformed(source, drawTransform);
            }
        }
        finally
        {
            if (!ReferenceEquals(source, image.Peer))
            {
                source.Dispose();
            }
        }
    }

    public void Rotate(double theta)
    {
        _transform *= Matrix3x2.CreateRotation((float)theta);
    }

    public void Scale(double scaleX, double scaleY)
    {
        _transform *= Matrix3x2.CreateScale((float)scaleX, (float)scaleY);
    }

    public void Translate(double tx, double ty)
    {
        _transform *= Matrix3x2.CreateTranslation((float)tx, (float)ty);
    }

    public void Dispose()
    {
    }
}

internal sealed class ImageSharpImageCodecPeer : IImageCodecPeer
{
    public BufferedImage? Decode(byte[] data)
    {
        try
        {
            return new BufferedImage(new ImageSharpBufferedImagePeer(ImageSharpImage.Load<Rgba32>(data)));
        }
        catch (SixLabors.ImageSharp.UnknownImageFormatException)
        {
            return null;
        }
    }

    public byte[] Encode(BufferedImage image, EncodedImageFormat format, int quality)
    {
        ImageSharpBufferedImagePeer source = ImageSharpBufferedImagePeer.FromBufferedImage(image);
        try
        {
            using var stream = new MemoryStream();
            if (format == EncodedImageFormat.Jpeg)
            {
                source.Image.Save(stream, new JpegEncoder { Quality = Math.Clamp(quality, 1, 100) });
            }
            else
            {
                source.Image.Save(stream, new PngEncoder());
            }

            return stream.ToArray();
        }
        finally
        {
            if (!ReferenceEquals(source, image.Peer))
            {
                source.Dispose();
            }
        }
    }

    public byte[] EncodePng(InterleavedPixelData pixels)
    {
        byte[] packed = GetPackedPixelData(pixels);
        using var stream = new MemoryStream();
        if (pixels.PixelFormat == InterleavedPixelFormat.Rgba32)
        {
            using var image = ImageSharpImage.LoadPixelData<Rgba32>(packed, pixels.Width, pixels.Height);
            image.Save(stream, new PngEncoder());
        }
        else
        {
            using var image = ImageSharpImage.LoadPixelData<Rgb24>(packed, pixels.Width, pixels.Height);
            image.Save(stream, new PngEncoder());
        }

        return stream.ToArray();
    }

    private static byte[] GetPackedPixelData(InterleavedPixelData pixels)
    {
        if (pixels.RowStride == pixels.RowByteCount)
        {
            return pixels.Data;
        }

        byte[] packed = GC.AllocateUninitializedArray<byte>(checked(pixels.RowByteCount * pixels.Height));
        for (int y = 0; y < pixels.Height; y++)
        {
            Buffer.BlockCopy(
                pixels.Data,
                y * pixels.RowStride,
                packed,
                y * pixels.RowByteCount,
                pixels.RowByteCount);
        }

        return packed;
    }
}

internal sealed class ImageSharpPageDrawerPeer : IPageDrawerPeer
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
            "PdfBox.Net.ImageSharp currently implements BufferedImage, Graphics2D, and ImageIO-style operations only. " +
            "Full PDF page rendering still requires PdfBox.Net.SkiaSharp.");
    }
}
