/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cff/Type1CharString.java
 * PDFBOX_SOURCE_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc
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
using System.Numerics;
using System.Text;
using System.Globalization;
using PdfBox.Net.Util.Geometry;
using Microsoft.Extensions.Logging;
using PdfBox.Net.Logging;

using PdfBox.Net.FontBox.Type1;
using PdfBox.Net.FontBox.Util;
using PdfBox.Net.FontBox.Encoding;

namespace PdfBox.Net.FontBox.CFF;




/// <summary>
/// This class represents and renders a Type 1 CharString.
/// </summary>
/// <remarks>
/// Author: Villu Ruusmann
/// </remarks>
/// <remarks>
/// Author: John Hewson
/// </remarks>
public class Type1CharString
{
    private static ILogger<Type1CharString> LOG => PdfBoxLogging.CreateLogger<Type1CharString>();

    private Type1CharStringReader? font;
    private string fontName;
    private string glyphName;
    private GeneralPath? path;
    private int width = 0;
    private Type1Point leftSideBearing = new(0, 0);
    private Type1Point current;
    private bool isFlex = false;
    private List<Type1Point> flexPoints = new();
    private List<object> type1Sequence = new();
    private int commandCount = 0;
    private readonly object _renderLock = new();

    /// <summary>
    /// Constructs a new Type1CharString object.
    /// </summary>
    /// <param name="font">
    /// Parent Type 1 CharString font.
    /// </param>
    /// <param name="fontName">
    /// Name of the font.
    /// </param>
    /// <param name="glyphName">
    /// Name of the glyph.
    /// </param>
    /// <param name="sequence">
    /// Type 1 char string sequence
    /// </param>
    public Type1CharString(Type1CharStringReader? font, string fontName, string glyphName,
            List<object> sequence) : this(font, fontName, glyphName)
    {
        type1Sequence.AddRange(sequence);
    }

    /// <summary>
    /// Constructor for use in subclasses.
    /// </summary>
    /// <param name="font">
    /// Parent Type 1 CharString font.
    /// </param>
    /// <param name="fontName">
    /// Name of the font.
    /// </param>
    /// <param name="glyphName">
    /// Name of the glyph.
    /// </param>
    protected Type1CharString(Type1CharStringReader? font, string fontName, string glyphName)
    {
        this.font = font;
        this.fontName = fontName;
        this.glyphName = glyphName;
        this.current = new Type1Point(0, 0);
    }

    // Compatibility constructor for the existing byte-oriented .NET API.
    public Type1CharString(string fontName, string glyphName, byte[] bytes)
        : this(null, fontName, glyphName, new Type1CharStringParser(fontName).Parse(bytes, [], glyphName))
    {
        Bytes = bytes;
    }

    internal Type1CharString(Type1CharStringReader font, string fontName, string glyphName,
        byte[] bytes, IList<byte[]> subrs)
        : this(font, fontName, glyphName, new Type1CharStringParser(fontName).Parse(bytes, subrs, glyphName))
    {
        Bytes = bytes;
    }

    public string FontName => fontName;
    public string GlyphName => glyphName;
    public byte[] Bytes { get; private set; } = [];

    public string GetName()
    {
        return glyphName;
    }

    /// <summary>
    /// Returns the bounds of the renderer path.
    /// </summary>
    /// <returns>
    /// the bounds as Rectangle2D
    /// </returns>
    public BoundingBox GetBounds()
    {
        lock (_renderLock)
        {
            if (path == null)
            {
                Render();
            }
        }
        return PathBounds(path!);
    }

    /// <summary>
    /// Returns the advance width of the glyph.
    /// </summary>
    /// <returns>
    /// the width
    /// </returns>
    public virtual float GetWidth()
    {
        lock (_renderLock)
        {
            if (path == null)
            {
                Render();
            }
        }
        return width;
    }

    /// <summary>
    /// Returns the path of the character.
    /// </summary>
    /// <returns>
    /// the path
    /// </returns>
    public virtual GeneralPath GetPath()
    {
        lock (_renderLock)
        {
            if (path == null)
            {
                Render();
            }
        }
        return path!;
    }

    /// <summary>
    /// Renders the Type 1 char string sequence to a GeneralPath.
    /// </summary>
    private void Render()
    {
        path = new GeneralPath();
        leftSideBearing = new Type1Point(0, 0);
        width = 0;
        List<double> numbers = new();
        type1Sequence.ForEach(obj => {
            if (obj is CharStringCommand)
            {
                HandleType1Command(numbers, (CharStringCommand) obj);
            }
            else
            {
                numbers.Add(Convert.ToDouble(obj, CultureInfo.InvariantCulture));
            }
        });
    }

    private void HandleType1Command(List<double> numbers, CharStringCommand command)
    {
        commandCount++;
        Type1KeyWord? type1KeyWord = command.GetType1KeyWord();
        if (type1KeyWord == null)
        {
            // indicates an invalid charstring
            LOG.LogWarning("Unknown charstring command in glyph {GlyphName} of font {FontName}", glyphName, fontName);
            numbers.Clear();
            return;
        }
        switch(type1KeyWord)
        {
        case Type1KeyWord.RMOVETO:
            if (numbers.Count >= 2)
            {
                if (isFlex)
                {
                    flexPoints.Add(new Type1Point(numbers[0], numbers[1]));
                }
                else
                {
                    // PDFBOX-6267 / https://github.com/mozilla/pdf.js/issues/10175
                    // use the last two instead of the first two numbers
                    RmoveTo(numbers[numbers.Count - 2], numbers[numbers.Count - 1]);
                }
            }
            break;
        case Type1KeyWord.VMOVETO:
            if (!(numbers.Count == 0))
            {
                if (isFlex)
                {
                    // not in the Type 1 spec, but exists in some fonts
                    flexPoints.Add(new Type1Point(0f, numbers[0]));
                }
                else
                {
                    RmoveTo(0, numbers[0]);
                }
            }
            break;
        case Type1KeyWord.HMOVETO:
            if (!(numbers.Count == 0))
            {
                if (isFlex)
                {
                    // not in the Type 1 spec, but exists in some fonts
                    flexPoints.Add(new Type1Point(numbers[0], 0f));
                }
                else
                {
                    RmoveTo(numbers[0], 0);
                }
            }
            break;
        case Type1KeyWord.RLINETO:
            if (numbers.Count >= 2)
            {
                RlineTo(numbers[0], numbers[1]);
            }
            break;
        case Type1KeyWord.HLINETO:
            if (!(numbers.Count == 0))
            {
                RlineTo(numbers[0], 0);
            }
            break;
        case Type1KeyWord.VLINETO:
            if (!(numbers.Count == 0))
            {
                RlineTo(0, numbers[0]);
            }
            break;
        case Type1KeyWord.RRCURVETO:
            if (numbers.Count >= 6)
            {
                RrcurveTo(numbers[0], numbers[1], numbers[2],
                        numbers[3], numbers[4], numbers[5]);
            }
            break;
        case Type1KeyWord.CLOSEPATH:
            CloseCharString1Path();
            break;
        case Type1KeyWord.SBW:
            if (numbers.Count >= 3)
            {
                leftSideBearing = new Type1Point(numbers[0], numbers[1]);
                width = JavaIntValue(numbers[2]);
                current.SetLocation(leftSideBearing);
            }
            break;
        case Type1KeyWord.HSBW:
            if (numbers.Count >= 2)
            {
                leftSideBearing = new Type1Point(numbers[0], 0);
                width = JavaIntValue(numbers[1]);
                current.SetLocation(leftSideBearing);
            }
            break;
        case Type1KeyWord.VHCURVETO:
            if (numbers.Count >= 4)
            {
                RrcurveTo(0, numbers[0], numbers[1],
                        numbers[2], numbers[3], 0);
            }
            break;
        case Type1KeyWord.HVCURVETO:
            if (numbers.Count >= 4)
            {
                RrcurveTo(numbers[0], 0, numbers[1],
                        numbers[2], 0, numbers[3]);
            }
            break;
        case Type1KeyWord.SEAC:
            if (numbers.Count >= 5)
            {
                Seac(numbers[0], numbers[1], numbers[2], numbers[3], numbers[4]);
            }
            break;
        case Type1KeyWord.SETCURRENTPOINT:
            if (numbers.Count >= 2)
            {
                SetCurrentPoint(numbers[0], numbers[1]);
            }
            break;
        case Type1KeyWord.CALLOTHERSUBR:
            if (!(numbers.Count == 0))
            {
                CallOtherSubr(JavaIntValue(numbers[0]));
            }
            break;
        case Type1KeyWord.DIV:
            if (numbers.Count >= 2)
            {
                float b = (float)numbers[numbers.Count - 1];
                float a = (float)numbers[numbers.Count - 2];

                float result = a / b;

                numbers.RemoveAt(numbers.Count - 1);
                numbers.RemoveAt(numbers.Count - 1);
                numbers.Add(result);
                return;
            }
            break;
        case Type1KeyWord.HSTEM:
        case Type1KeyWord.VSTEM:
        case Type1KeyWord.HSTEM3:
        case Type1KeyWord.VSTEM3:
        case Type1KeyWord.DOTSECTION:
            // ignore hints
            break;
        case Type1KeyWord.ENDCHAR:
            // end
            break;
        case Type1KeyWord.RET:
        case Type1KeyWord.CALLSUBR:
            // indicates an invalid charstring
            LOG.LogWarning("Unexpected charstring command: {Command} in glyph {GlyphName} of font {FontName}", type1KeyWord, glyphName, fontName);
            break;
        default:
            // indicates a PDFBox bug
            throw new ArgumentException("Unhandled command: " + type1KeyWord);
        }
        numbers.Clear();
    }

    /// <summary>
    /// Sets the current absolute point without performing a moveto.
    /// Used only with results from callothersubr
    /// </summary>
    private void SetCurrentPoint(double x, double y)
    {
        current.SetLocation(x, y);
    }

    /// <summary>
    /// Flex (via OtherSubrs)
    /// </summary>
    /// <param name="num">
    /// OtherSubrs entry number
    /// </param>
    private void CallOtherSubr(int num)
    {
        if (num == 0)
        {
            // end flex
            isFlex = false;

            if (flexPoints.Count < 7)
            {
                LOG.LogWarning("flex without moveTo in font {FontName}, glyph {GlyphName}, command {CommandCount}", fontName, glyphName, commandCount);
                return;
            }

            // reference point is relative to start point
            Type1Point reference = flexPoints[0];
            reference.SetLocation(current.GetX() + reference.GetX(),
                                  current.GetY() + reference.GetY());

            // first point is relative to reference point
            Type1Point first = flexPoints[1];
            first.SetLocation(reference.GetX() + first.GetX(), reference.GetY() + first.GetY());

            // make the first point relative to the start point
            first.SetLocation(first.GetX() - current.GetX(), first.GetY() - current.GetY());

            Type1Point p1 = flexPoints[1];
            Type1Point p2 = flexPoints[2];
            Type1Point p3 = flexPoints[3];
            RrcurveTo(p1.GetX(), p1.GetY(), p2.GetX(), p2.GetY(), p3.GetX(), p3.GetY());

            Type1Point p4 = flexPoints[4];
            Type1Point p5 = flexPoints[5];
            Type1Point p6 = flexPoints[6];
            RrcurveTo(p4.GetX(), p4.GetY(), p5.GetX(), p5.GetY(), p6.GetX(), p6.GetY());

            flexPoints.Clear();
        }
        else if (num == 1)
        {
            // begin flex
            isFlex = true;
        }
        else
        {
            LOG.LogWarning("Invalid callothersubr parameter: {Number}", num);
        }
    }

    /// <summary>
    /// Relative moveto.
    /// </summary>
    private void RmoveTo(double dx, double dy)
    {
        float x = (float)current.GetX() + (float)dx;
        float y = (float)current.GetY() + (float)dy;
        path!.MoveTo(x, y);
        current.SetLocation(x, y);
    }

    /// <summary>
    /// Relative lineto.
    /// </summary>
    private void RlineTo(double dx, double dy)
    {
        float x = (float)current.GetX() + (float)dx;
        float y = (float)current.GetY() + (float)dy;
        if (path!.Segments.Count == 0)
        {
            LOG.LogWarning("rlineTo without initial moveTo in font {FontName}, glyph {GlyphName}", fontName, glyphName);
            path!.MoveTo(x, y);
        }
        else
        {
            path!.LineTo(x, y);
        }
        current.SetLocation(x, y);
    }

    /// <summary>
    /// Relative curveto.
    /// </summary>
    private void RrcurveTo(double dx1, double dy1, double dx2, double dy2,
            double dx3, double dy3)
    {
        float x1 = (float) current.GetX() + (float)dx1;
        float y1 = (float) current.GetY() + (float)dy1;
        float x2 = x1 + (float)dx2;
        float y2 = y1 + (float)dy2;
        float x3 = x2 + (float)dx3;
        float y3 = y2 + (float)dy3;
        if (path!.Segments.Count == 0)
        {
            LOG.LogWarning("rrcurveTo without initial moveTo in font {FontName}, glyph {GlyphName}", fontName, glyphName);
            path!.MoveTo(x3, y3);
        }
        else
        {
            path!.CurveTo(x1, y1, x2, y2, x3, y3);
        }
        current.SetLocation(x3, y3);
    }

    /// <summary>
    /// Close path.
    /// </summary>
    private void CloseCharString1Path()
    {
        if (path!.Segments.Count == 0)
        {
            LOG.LogWarning("closepath without initial moveTo in font {FontName}, glyph {GlyphName}", fontName, glyphName);
        }
        else
        {
            path!.ClosePath();
        }
        path!.MoveTo(current.GetX(), current.GetY());
    }

    /// <summary>
    /// Standard Encoding Accented Character
    ///
    /// Makes an accented character from two other characters.
    /// </summary>
    /// <param name="asb">
    /// </param>
    private void Seac(double asb, double adx, double ady, double bchar, double achar)
    {
        // base character
        string baseName = StandardEncoding.INSTANCE.GetName(JavaIntValue(bchar));
        try
        {
            Type1CharString baseChar = ReadCharString(baseName);
            AppendPath(path!, baseChar.GetPath(), 0, 0);
        }
        catch (IOException e)
        {
            LOG.LogWarning(e, "invalid seac character in glyph {GlyphName} of font {FontName}", glyphName, fontName);
        }
        // accent character
        string accentName = StandardEncoding.INSTANCE.GetName(JavaIntValue(achar));
        try
        {
            Type1CharString accent = ReadCharString(accentName);
            GeneralPath accentPath = accent.GetPath();
            if (path == accentPath)
            {
                // PDFBOX-5339: avoid ArrayIndexOutOfBoundsException
                // reproducable with poc file crash-4698e0dc7833a3f959d06707e01d03cda52a83f4
                LOG.LogWarning("Path for {BaseName} and for accent {AccentName} are same, ignored", baseName, accentName);
                return;
            }
            AppendPath(path!, accentPath, leftSideBearing.GetX() + (float)adx - (float)asb,
                    leftSideBearing.GetY() + (float)ady);
        }
        catch (IOException e)
        {
            LOG.LogWarning(e, "invalid seac character in glyph {GlyphName} of font {FontName}", glyphName, fontName);
        }
    }

    /// <summary>
    /// Add a command to the type1 sequence.
    /// </summary>
    /// <param name="numbers">
    /// the parameters of the command to be added
    /// </param>
    /// <param name="command">
    /// the command to be added
    /// </param>
    protected void AddCommand(List<double> numbers, CharStringCommand command)
    {
        type1Sequence.AddRange(numbers.Cast<object>());
        type1Sequence.Add(command);
    }

    /// <summary>
    /// Indicates if the underlying type1 sequence is empty.
    /// </summary>
    /// <returns>
    /// true if the sequence is empty
    /// </returns>
    protected bool IsSequenceEmpty()
    {
        return (type1Sequence.Count == 0);
    }

    /// <summary>
    /// Returns the last entry of the underlying type1 sequence.
    /// </summary>
    /// <returns>
    /// the last entry of the type 1 sequence or null if empty
    /// </returns>
    protected object? GetLastSequenceEntry()
    {
        if (!(type1Sequence.Count == 0))
        {
            return type1Sequence[^1];
        }
        return null;
    }


    public override string ToString()
    {
        return string.Join(", ", type1Sequence).Replace("|","\n").Replace(",", " ");
    }
    private Type1CharString ReadCharString(string name) => font?.GetType1CharString(name)
        ?? throw new IOException("A parent font is required to resolve a seac character.");

    private static int JavaIntValue(double value) => double.IsNaN(value) ? 0
        : value >= int.MaxValue ? int.MaxValue : value <= int.MinValue ? int.MinValue : (int)value;

    private sealed class Type1Point
    {
        private float x;
        private float y;
        public Type1Point(double x, double y) { this.x = (float)x; this.y = (float)y; }
        public float GetX() => x;
        public float GetY() => y;
        public void SetLocation(double newX, double newY) { x = (float)newX; y = (float)newY; }
        public void SetLocation(Type1Point point) => SetLocation(point.GetX(), point.GetY());
    }

    private static void AppendPath(GeneralPath target, GeneralPath source, float dx, float dy)
    {
        foreach (var segment in source.Segments.ToArray())
        {
            switch (segment.Type)
            {
                case GeneralPath.SegmentType.MoveTo: target.MoveTo(segment.X1 + dx, segment.Y1 + dy); break;
                case GeneralPath.SegmentType.LineTo: target.LineTo(segment.X1 + dx, segment.Y1 + dy); break;
                case GeneralPath.SegmentType.QuadTo: target.QuadTo(segment.X1 + dx, segment.Y1 + dy, segment.X2 + dx, segment.Y2 + dy); break;
                case GeneralPath.SegmentType.CurveTo: target.CurveTo(segment.X1 + dx, segment.Y1 + dy, segment.X2 + dx, segment.Y2 + dy, segment.X3 + dx, segment.Y3 + dy); break;
                case GeneralPath.SegmentType.Close: target.ClosePath(); break;
            }
        }
    }

    private static BoundingBox PathBounds(GeneralPath path)
    {
        List<(float x, float y)> points = [];
        foreach (var segment in path.Segments)
        {
            if (segment.Type == GeneralPath.SegmentType.Close) continue;
            points.Add((segment.X1, segment.Y1));
            if (segment.Type is GeneralPath.SegmentType.QuadTo or GeneralPath.SegmentType.CurveTo) points.Add((segment.X2, segment.Y2));
            if (segment.Type == GeneralPath.SegmentType.CurveTo) points.Add((segment.X3, segment.Y3));
        }
        return points.Count == 0 ? new BoundingBox() : new BoundingBox(points.Min(p => p.x), points.Min(p => p.y), points.Max(p => p.x), points.Max(p => p.y));
    }

}
