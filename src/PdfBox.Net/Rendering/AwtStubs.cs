/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Real .NET graphics implementations backed by SkiaSharp, replacing the
 * previous empty stub placeholders for Java AWT types.
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

using SkiaSharp;

namespace PdfBox.Net.Rendering;

/// <summary>
/// Raster image backed by an <see cref="SKBitmap"/>.
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

    private readonly SKBitmap _bitmap;
    private bool _disposed;

    public BufferedImage(int width, int height, int type)
    {
        Width = width;
        Height = height;
        Type = type;
        SKColorType colorType = type == TYPE_INT_ARGB ? SKColorType.Bgra8888 : SKColorType.Bgra8888;
        SKAlphaType alphaType = type == TYPE_INT_ARGB ? SKAlphaType.Premul : SKAlphaType.Opaque;
        _bitmap = new SKBitmap(width, height, colorType, alphaType);
    }

    public int Width { get; }

    public int Height { get; }

    public int Type { get; }

    /// <summary>Returns the underlying SkiaSharp bitmap for direct pixel access.</summary>
    public SKBitmap Bitmap => _bitmap;

    public Graphics2D CreateGraphics()
    {
        return new Graphics2D(_bitmap);
    }

    public ColorModel GetColorModel()
    {
        return new ColorModel();
    }

    public WritableRaster GetRaster()
    {
        return new WritableRaster(Width, Height);
    }

    /// <summary>Returns the ARGB value of the pixel at (<paramref name="x"/>, <paramref name="y"/>).</summary>
    public int GetRgb(int x, int y)
    {
        SKColor c = _bitmap.GetPixel(x, y);
        return (c.Alpha << 24) | (c.Red << 16) | (c.Green << 8) | c.Blue;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _bitmap.Dispose();
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
/// 2-D drawing context backed by an <see cref="SKCanvas"/>.
/// Replaces the Java AWT <c>Graphics2D</c> stub.
/// </summary>
public class Graphics2D : Graphics, IDisposable
{
    private readonly SKBitmap? _bitmap;
    private SKCanvas? _canvas;
    private bool _ownsCanvas;
    private Color _background = Color.White;
    private Shape? _clip;
    private Stroke _stroke = new();
    private IPaint _paint = Color.Black;

    public Graphics2D()
    {
    }

    public Graphics2D(SKBitmap bitmap)
    {
        _bitmap = bitmap;
        _canvas = new SKCanvas(bitmap);
        _ownsCanvas = true;
    }

    internal Graphics2D(SKBitmap? bitmap, SKCanvas canvas, bool ownsCanvas = false)
    {
        _bitmap = bitmap;
        _canvas = canvas;
        _ownsCanvas = ownsCanvas;
    }

    /// <summary>Returns the underlying SkiaSharp canvas (may be null for a default-constructed instance).</summary>
    public SKCanvas? Canvas => _canvas;

    public override Graphics Create()
    {
        if (_canvas is null)
        {
            return new Graphics2D();
        }
        // Return a wrapper sharing the same canvas (the canvas is not owned by the copy).
        return new Graphics2D(_bitmap, _canvas, ownsCanvas: false);
    }

    public virtual void ClearRect(int x, int y, int width, int height)
    {
        if (_canvas is null) return;
        using var paint = new SKPaint { Color = ToSkColor(_background), BlendMode = SKBlendMode.Src };
        _canvas.DrawRect(x, y, width, height, paint);
    }

    public virtual void Clip(Shape shape)
    {
        _clip = shape;
    }

    public virtual void DrawImage(BufferedImage image, int x, int y, object? observer = null)
    {
        if (_canvas is null) return;
        _canvas.DrawBitmap(image.Bitmap, x, y);
    }

    public virtual Color GetBackground() => _background;

    public virtual Shape? GetClip() => _clip;

    public virtual GraphicsConfiguration? GetDeviceConfiguration() => new();

    public virtual IPaint GetPaint() => _paint;

    public virtual Stroke GetStroke() => _stroke;

    public virtual void Rotate(double theta)
    {
        _canvas?.RotateRadians((float)theta);
    }

    public virtual void Scale(double scaleX, double scaleY)
    {
        _canvas?.Scale((float)scaleX, (float)scaleY);
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
        _canvas?.Translate((float)tx, (float)ty);
    }

    public override void Dispose()
    {
        if (_ownsCanvas)
        {
            _canvas?.Dispose();
            _canvas = null;
        }
    }

    internal static SKColor ToSkColor(Color color)
    {
        return new SKColor((byte)color.Red, (byte)color.Green, (byte)color.Blue, (byte)color.Alpha);
    }
}

public class Color : IPaint
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
}

public interface IPaint
{
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

    public virtual int GetAlpha(object? pixel) => 255;

    public virtual int GetBlue(object? pixel) => 0;

    public virtual int GetGreen(object? pixel) => 0;

    public virtual int GetRed(object? pixel) => 0;
}

public class Raster
{
    public Raster(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public int Height { get; }

    public int Width { get; }

    public virtual object? GetDataElements(int x, int y, object? element) => null;

    public virtual int[] GetPixel(int x, int y, int[] pixel) => pixel;
}

public class WritableRaster : Raster
{
    public WritableRaster(int width, int height)
        : base(width, height)
    {
    }

    public virtual void SetPixel(int x, int y, int[] pixel)
    {
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

public class TexturePaint : IPaint
{
    public TexturePaint(BufferedImage image, Rectangle2D anchorRect)
    {
        Image = image;
        AnchorRect = anchorRect;
    }

    public Rectangle2D AnchorRect { get; }

    public BufferedImage Image { get; }
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
