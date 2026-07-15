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

/// <summary>
/// Describes the channel layout of interleaved image pixels supplied to an image codec.
/// </summary>
public enum InterleavedPixelFormat
{
    /// <summary>Three bytes per pixel in red, green, blue order.</summary>
    Rgb24,

    /// <summary>Four bytes per pixel in red, green, blue, alpha order.</summary>
    Rgba32
}

/// <summary>
/// Validated backend-neutral interleaved pixel data for bulk image encoding.
/// </summary>
public readonly struct InterleavedPixelData
{
    public InterleavedPixelData(
        byte[] data,
        int width,
        int height,
        int rowStride,
        InterleavedPixelFormat pixelFormat)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Image width must be positive.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Image height must be positive.");
        }

        int bytesPerPixel = pixelFormat switch
        {
            InterleavedPixelFormat.Rgb24 => 3,
            InterleavedPixelFormat.Rgba32 => 4,
            _ => throw new ArgumentOutOfRangeException(nameof(pixelFormat))
        };
        int rowByteCount = checked(width * bytesPerPixel);
        if (rowStride < rowByteCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(rowStride),
                "Image row stride must include every pixel in the row.");
        }

        long requiredLength = ((long)(height - 1) * rowStride) + rowByteCount;
        if (data.LongLength < requiredLength)
        {
            throw new ArgumentException("Pixel data is shorter than the declared image dimensions and row stride.", nameof(data));
        }

        Data = data;
        Width = width;
        Height = height;
        RowStride = rowStride;
        PixelFormat = pixelFormat;
        BytesPerPixel = bytesPerPixel;
        RowByteCount = rowByteCount;
    }

    public byte[] Data { get; }

    public int Width { get; }

    public int Height { get; }

    public int RowStride { get; }

    public InterleavedPixelFormat PixelFormat { get; }

    public int BytesPerPixel { get; }

    public int RowByteCount { get; }
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

    /// <summary>
    /// Encodes interleaved RGB or RGBA pixels directly as PNG without requiring per-pixel
    /// calls through <see cref="BufferedImage"/>.
    /// </summary>
    /// <remarks>
    /// Bundled backends override this method with a native bulk path. The default implementation
    /// preserves compatibility for third-party codec peers while they adopt the optimized API.
    /// </remarks>
    byte[] EncodePng(InterleavedPixelData pixels)
    {
        bool hasAlpha = pixels.PixelFormat == InterleavedPixelFormat.Rgba32;
        using BufferedImage image = new(
            pixels.Width,
            pixels.Height,
            hasAlpha ? BufferedImage.TYPE_INT_ARGB : BufferedImage.TYPE_INT_RGB);
        byte[] data = pixels.Data;
        for (int y = 0; y < pixels.Height; y++)
        {
            int sourceOffset = y * pixels.RowStride;
            for (int x = 0; x < pixels.Width; x++)
            {
                int red = data[sourceOffset++];
                int green = data[sourceOffset++];
                int blue = data[sourceOffset++];
                int alpha = hasAlpha ? data[sourceOffset++] : byte.MaxValue;
                image.SetRgb(x, y, (alpha << 24) | (red << 16) | (green << 8) | blue);
            }
        }

        return Encode(image, EncodedImageFormat.Png, 100);
    }
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
            "such as PdfBox.Net.Rendering for the supported full stack or PdfBox.Net.SkiaSharp for only the SkiaSharp renderer, " +
            "and call its registration method before rendering or image decoding.");

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
