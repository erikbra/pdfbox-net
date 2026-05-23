/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Stub implementations for Java AWT types used by the rendering package.
 * These types have no direct .NET equivalents; they are placeholders until
 * a platform-specific rendering implementation is introduced.
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

namespace PdfBox.Net.Rendering;

public class BufferedImage : Image
{
    public const int TYPE_INT_RGB = 1;
    public const int TYPE_INT_ARGB = 2;
    public const int TYPE_INT_BGR = 4;
    public const int TYPE_3BYTE_BGR = 5;
    public const int TYPE_BYTE_GRAY = 10;
    public const int TYPE_BYTE_BINARY = 12;

    private readonly WritableRaster _raster;

    public BufferedImage(int width, int height, int type)
    {
        Width = width;
        Height = height;
        Type = type;
        _raster = new WritableRaster(width, height);
    }

    public int Width { get; }

    public int Height { get; }

    public int Type { get; }

    public Graphics2D CreateGraphics()
    {
        return new Graphics2D(this);
    }

    public ColorModel GetColorModel()
    {
        return new ColorModel();
    }

    public WritableRaster GetRaster()
    {
        return _raster;
    }
}

public class Graphics
{
    public virtual Graphics Create() => new Graphics();

    public virtual void Dispose()
    {
    }
}

public class Graphics2D : Graphics
{
    private readonly BufferedImage? _image;
    private Color _background = Color.White;
    private Shape? _clip;
    private Stroke _stroke = new();
    private IPaint _paint = Color.Black;

    public Graphics2D()
    {
    }

    public Graphics2D(BufferedImage? image)
    {
        _image = image;
    }

    public override Graphics Create()
    {
        return new Graphics2D(_image);
    }

    public virtual void ClearRect(int x, int y, int width, int height)
    {
    }

    public virtual void Clip(Shape shape)
    {
        _clip = shape;
    }

    public virtual void DrawImage(BufferedImage image, int x, int y, object? observer = null)
    {
    }

    public virtual Color GetBackground() => _background;

    public virtual Shape? GetClip() => _clip;

    public virtual GraphicsConfiguration? GetDeviceConfiguration() => new();

    public virtual IPaint GetPaint() => _paint;

    public virtual Stroke GetStroke() => _stroke;

    public virtual void Rotate(double theta)
    {
    }

    public virtual void Scale(double scaleX, double scaleY)
    {
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
