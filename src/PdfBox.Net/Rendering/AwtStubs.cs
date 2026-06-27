/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Java AWT-style proxy classes backed by a pluggable .NET rendering backend.
 *
 * PORT_MODE: adapted
 */

/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using PdfBox.Net.Util;

namespace PdfBox.Net.Rendering;

/// <summary>
/// Java-shaped raster image proxy backed by the registered rendering backend.
/// Replaces the Java AWT <c>BufferedImage</c> stub.
/// </summary>
public class BufferedImage : Image, IDisposable
{
    public const int TYPE_INT_RGB = 1;
    public const int TYPE_INT_ARGB = 2;
    public const int TYPE_INT_BGR = 4;
    public const int TYPE_3BYTE_BGR = 5;
    public const int TYPE_BYTE_GRAY = 10;
    public const int TYPE_BYTE_BINARY = 12;

    private readonly IBufferedImagePeer _peer;
    private bool _disposed;

    public BufferedImage(int width, int height, int type)
    {
        _peer = RenderingBackend.Current.CreateBufferedImage(width, height, type);
        Width = _peer.Width;
        Height = _peer.Height;
        Type = _peer.Type;
    }

    internal BufferedImage(IBufferedImagePeer peer)
    {
        _peer = peer ?? throw new ArgumentNullException(nameof(peer));
        Width = _peer.Width;
        Height = _peer.Height;
        Type = _peer.Type;
    }

    public int Width { get; }

    public int Height { get; }

    public int Type { get; }

    internal IBufferedImagePeer Peer => _peer;

    public Graphics2D CreateGraphics()
    {
        return new Graphics2D(_peer.CreateGraphics());
    }

    public ColorModel GetColorModel()
    {
        return new ColorModel();
    }

    public WritableRaster GetRaster()
    {
        return new BitmapWritableRaster(this);
    }

    /// <summary>Returns the ARGB value of the pixel at (<paramref name="x"/>, <paramref name="y"/>).</summary>
    public int GetRgb(int x, int y)
    {
        return _peer.GetRgb(x, y);
    }

    public void SetRgb(int x, int y, int argb)
    {
        _peer.SetPixel(
            x,
            y,
            (argb >> 16) & 0xFF,
            (argb >> 8) & 0xFF,
            argb & 0xFF,
            (argb >> 24) & 0xFF);
    }

    public void Clear(Color color)
    {
        _peer.Clear(color);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _peer.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}

public class Graphics
{
    public virtual Graphics Create() => new Graphics();

    public virtual void Dispose()
    {
    }
}

/// <summary>
/// 2-D drawing context proxy backed by the registered rendering backend.
/// Replaces the Java AWT <c>Graphics2D</c> stub.
/// </summary>
public class Graphics2D : Graphics, IDisposable
{
    private readonly IGraphics2DPeer _peer;
    private Color _background = Color.White;
    private Shape? _clip;
    private Stroke _stroke = new();
    private IPaint _paint = Color.Black;
    private AffineTransform _transform = new();

    public Graphics2D()
    {
        _peer = NullGraphics2DPeer.Instance;
    }

    internal Graphics2D(IGraphics2DPeer peer)
    {
        _peer = peer;
    }

    private Graphics2D(IGraphics2DPeer peer, AffineTransform? transform)
    {
        _peer = peer;
        _transform = transform?.Clone() ?? new AffineTransform();
    }

    internal IGraphics2DPeer Peer => _peer;

    internal int? BitmapHeight => _peer.BitmapHeight;

    public override Graphics Create()
    {
        return new Graphics2D(_peer.Create(), _transform);
    }

    public virtual void ClearRect(int x, int y, int width, int height)
    {
        _peer.ClearRect(x, y, width, height, _background);
    }

    public virtual void Clip(Shape shape)
    {
        _clip = shape;
    }

    public virtual void DrawImage(BufferedImage image, int x, int y, object? observer = null)
    {
        _peer.DrawImage(image, x, y);
    }

    public virtual Color GetBackground() => _background;

    public virtual Shape? GetClip() => _clip;

    public virtual GraphicsConfiguration? GetDeviceConfiguration() => new();

    public virtual IPaint GetPaint() => _paint;

    public virtual Stroke GetStroke() => _stroke;

    public virtual AffineTransform GetTransform() => _transform.Clone();

    public virtual void Rotate(double theta)
    {
        _transform.Rotate(theta);
        _peer.Rotate(theta);
    }

    public virtual void Scale(double scaleX, double scaleY)
    {
        _transform.Scale(scaleX, scaleY);
        _peer.Scale(scaleX, scaleY);
    }

    public virtual void SetBackground(Color color)
    {
        _background = color;
    }

    public virtual void SetClip(Shape? clip)
    {
        _clip = clip;
    }

    public virtual void SetPaint(IPaint paint)
    {
        _paint = paint;
    }

    public virtual void SetStroke(Stroke stroke)
    {
        _stroke = stroke;
    }

    public virtual void Translate(double tx, double ty)
    {
        _transform.Translate(tx, ty);
        _peer.Translate(tx, ty);
    }

    public override void Dispose()
    {
        _peer.Dispose();
    }
}

public class Color : IContextPaint
{
    public static readonly Color White = new(255, 255, 255, 255);
    public static readonly Color Black = new(0, 0, 0, 255);
    public static readonly Color Transparent = new(0, 0, 0, 0);

    public Color()
        : this(0, 0, 0, 255)
    {
    }

    public Color(int red, int green, int blue, int alpha = 255)
    {
        Red = red;
        Green = green;
        Blue = blue;
        Alpha = alpha;
    }

    public int Alpha { get; }

    public int Blue { get; }

    public int Green { get; }

    public int Red { get; }

    public PaintContext CreateContext(ColorModel cm, Rectangle deviceBounds, Rectangle2D userBounds, AffineTransform xform, RenderingHints hints)
    {
        return new SolidColorPaintContext(this);
    }
}

public interface IPaint
{
}

public interface IContextPaint : IPaint
{
    PaintContext CreateContext(ColorModel cm, Rectangle deviceBounds, Rectangle2D userBounds, AffineTransform xform, RenderingHints hints);
}

public interface PaintContext : IDisposable
{
    ColorModel GetColorModel();

    Raster GetRaster(int x, int y, int width, int height);
}

public class ColorModel
{
    public virtual WritableRaster CreateCompatibleWritableRaster(int width, int height)
    {
        return new WritableRaster(width, height);
    }

    public virtual int GetAlpha(object? pixel) => GetComponent(pixel, 3, 255);

    public virtual int GetBlue(object? pixel) => GetComponent(pixel, 2, 0);

    public virtual int GetGreen(object? pixel) => GetComponent(pixel, 1, 0);

    public virtual int GetRed(object? pixel) => GetComponent(pixel, 0, 0);

    private static int GetComponent(object? pixel, int component, int fallback)
    {
        if (pixel is int[] values)
        {
            return component < values.Length ? values[component] : fallback;
        }

        if (pixel is int argb)
        {
            return component switch
            {
                0 => (argb >> 16) & 0xFF,
                1 => (argb >> 8) & 0xFF,
                2 => argb & 0xFF,
                3 => (argb >> 24) & 0xFF,
                _ => fallback
            };
        }

        return fallback;
    }
}

public class Raster
{
    public Raster(int width, int height)
        : this(width, height, allocatePixels: true)
    {
    }

    protected Raster(int width, int height, bool allocatePixels)
    {
        Width = width;
        Height = height;
        Pixels = allocatePixels ? new int[Math.Max(0, width) * Math.Max(0, height) * 4] : null;
    }

    public int Height { get; }

    public int Width { get; }

    protected int[]? Pixels { get; }

    public virtual object? GetDataElements(int x, int y, object? element)
    {
        int[] pixel = element as int[] ?? new int[4];
        if (pixel.Length < 4)
        {
            pixel = new int[4];
        }

        GetPixel(x, y, pixel);
        return pixel;
    }

    public virtual int[] GetPixel(int x, int y, int[] pixel)
    {
        ArgumentNullException.ThrowIfNull(pixel);
        if (pixel.Length < 4)
        {
            throw new ArgumentException("Pixel buffer must have at least four components.", nameof(pixel));
        }

        ReadPixel(x, y, pixel);
        return pixel;
    }

    protected virtual void ReadPixel(int x, int y, int[] pixel)
    {
        Array.Clear(pixel, 0, Math.Min(4, pixel.Length));
        if (Pixels is null || x < 0 || y < 0 || x >= Width || y >= Height)
        {
            return;
        }

        int offset = ((y * Width) + x) * 4;
        pixel[0] = Pixels[offset];
        pixel[1] = Pixels[offset + 1];
        pixel[2] = Pixels[offset + 2];
        pixel[3] = Pixels[offset + 3];
    }
}

public class WritableRaster : Raster
{
    public WritableRaster(int width, int height)
        : base(width, height)
    {
    }

    protected WritableRaster(int width, int height, bool allocatePixels)
        : base(width, height, allocatePixels)
    {
    }

    public virtual void SetPixel(int x, int y, int[] pixel)
    {
        ArgumentNullException.ThrowIfNull(pixel);
        if (Pixels is null || x < 0 || y < 0 || x >= Width || y >= Height)
        {
            return;
        }

        int offset = ((y * Width) + x) * 4;
        Pixels[offset] = pixel.Length > 0 ? pixel[0] : 0;
        Pixels[offset + 1] = pixel.Length > 1 ? pixel[1] : 0;
        Pixels[offset + 2] = pixel.Length > 2 ? pixel[2] : 0;
        Pixels[offset + 3] = pixel.Length > 3 ? pixel[3] : 255;
    }
}

public class Point2D
{
    public Point2D()
    {
    }

    public Point2D(double x, double y)
    {
        X = x;
        Y = y;
    }

    public double X { get; set; }

    public double Y { get; set; }
}

public interface Shape
{
}

public class Rectangle : Shape
{
    public Rectangle()
    {
    }

    public Rectangle(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public int Height { get; set; }

    public int Width { get; set; }

    public int X { get; set; }

    public int Y { get; set; }
}

public class Rectangle2D : Shape
{
    public Rectangle2D()
    {
    }

    public Rectangle2D(double x, double y, double width, double height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public double Height { get; set; }

    public double Width { get; set; }

    public double X { get; set; }

    public double Y { get; set; }

    public bool Contains(double x, double y)
    {
        return x >= X && y >= Y && x <= X + Width && y <= Y + Height;
    }
}

public class Area : Shape
{
    public Area()
    {
    }

    public Area(Shape shape)
    {
    }
}

public class Stroke
{
}

public class Composite
{
}

public class Image
{
}

public class ImageObserver
{
}

public class BufferedImageOp
{
}

public class RenderedImage
{
}

public class RenderableImage
{
}

public class AttributedCharacterIterator
{
}

public class Font
{
}

public class FontMetrics
{
}

public class FontRenderContext
{
}

public class GlyphVector
{
}

public class TexturePaint : IContextPaint
{
    public TexturePaint(BufferedImage image, Rectangle2D anchorRect)
    {
        Image = image;
        AnchorRect = anchorRect;
    }

    public Rectangle2D AnchorRect { get; }

    public BufferedImage Image { get; }

    public PaintContext CreateContext(ColorModel cm, Rectangle deviceBounds, Rectangle2D userBounds, AffineTransform xform, RenderingHints hints)
    {
        return new TexturePaintContext(Image, AnchorRect);
    }
}

internal sealed class SolidColorPaintContext : PaintContext
{
    private readonly Color _color;

    internal SolidColorPaintContext(Color color)
    {
        _color = color;
    }

    public ColorModel GetColorModel()
    {
        return new ColorModel();
    }

    public Raster GetRaster(int x, int y, int width, int height)
    {
        WritableRaster raster = new(width, height);
        int[] pixel = [_color.Red, _color.Green, _color.Blue, _color.Alpha];
        for (int py = 0; py < height; py++)
        {
            for (int px = 0; px < width; px++)
            {
                raster.SetPixel(px, py, pixel);
            }
        }

        return raster;
    }

    public void Dispose()
    {
    }
}

internal sealed class TexturePaintContext : PaintContext
{
    private readonly BufferedImage _image;
    private readonly Rectangle2D _anchor;

    internal TexturePaintContext(BufferedImage image, Rectangle2D anchor)
    {
        _image = image;
        _anchor = anchor;
    }

    public ColorModel GetColorModel()
    {
        return new ColorModel();
    }

    public Raster GetRaster(int x, int y, int width, int height)
    {
        WritableRaster raster = new(width, height);
        int[] pixel = new int[4];
        double anchorWidth = Math.Abs(_anchor.Width) > double.Epsilon ? Math.Abs(_anchor.Width) : _image.Width;
        double anchorHeight = Math.Abs(_anchor.Height) > double.Epsilon ? Math.Abs(_anchor.Height) : _image.Height;

        for (int py = 0; py < height; py++)
        {
            for (int px = 0; px < width; px++)
            {
                int sourceX = ScaleWrappedCoordinate(x + px - _anchor.X, anchorWidth, _image.Width);
                int sourceY = ScaleWrappedCoordinate(y + py - _anchor.Y, anchorHeight, _image.Height);
                int argb = _image.GetRgb(sourceX, sourceY);
                pixel[0] = (argb >> 16) & 0xFF;
                pixel[1] = (argb >> 8) & 0xFF;
                pixel[2] = argb & 0xFF;
                pixel[3] = (argb >> 24) & 0xFF;
                raster.SetPixel(px, py, pixel);
            }
        }

        return raster;
    }

    public void Dispose()
    {
    }

    private static int ScaleWrappedCoordinate(double value, double period, int limit)
    {
        if (limit <= 1)
        {
            return 0;
        }

        double wrapped = value % period;
        if (wrapped < 0)
        {
            wrapped += period;
        }

        return Math.Clamp((int)Math.Floor(wrapped / period * limit), 0, limit - 1);
    }
}

internal sealed class BitmapWritableRaster : WritableRaster
{
    private readonly BufferedImage _image;

    internal BitmapWritableRaster(BufferedImage image)
        : base(image.Width, image.Height, allocatePixels: false)
    {
        _image = image;
    }

    protected override void ReadPixel(int x, int y, int[] pixel)
    {
        Array.Clear(pixel, 0, Math.Min(4, pixel.Length));
        if (x < 0 || y < 0 || x >= Width || y >= Height)
        {
            return;
        }

        int argb = _image.GetRgb(x, y);
        pixel[0] = (argb >> 16) & 0xFF;
        pixel[1] = (argb >> 8) & 0xFF;
        pixel[2] = argb & 0xFF;
        pixel[3] = (argb >> 24) & 0xFF;
    }

    public override void SetPixel(int x, int y, int[] pixel)
    {
        ArgumentNullException.ThrowIfNull(pixel);
        if (x < 0 || y < 0 || x >= Width || y >= Height)
        {
            return;
        }

        int r = Math.Clamp(pixel.Length > 0 ? pixel[0] : 0, 0, 255);
        int g = Math.Clamp(pixel.Length > 1 ? pixel[1] : 0, 0, 255);
        int b = Math.Clamp(pixel.Length > 2 ? pixel[2] : 0, 0, 255);
        int a = Math.Clamp(pixel.Length > 3 ? pixel[3] : 255, 0, 255);
        _image.Peer.SetPixel(x, y, r, g, b, a);
    }
}

public class GraphicsConfiguration
{
    public GraphicsDevice? GetDevice() => new();
}

public class GraphicsDevice
{
    public DisplayMode? GetDisplayMode() => new(32);
}

public class DisplayMode
{
    public DisplayMode(int bitDepth)
    {
        BitDepth = bitDepth;
    }

    public int BitDepth { get; }
}

public static class Transparency
{
    public const int TRANSLUCENT = 3;
}

public class LookupTable
{
}
