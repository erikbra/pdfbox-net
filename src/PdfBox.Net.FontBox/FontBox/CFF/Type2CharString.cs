/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cff/Type2CharString.java
 * PDFBOX_SOURCE_COMMIT: 8faadfeed02acd2255ec8fae2227316407ad05d8
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 8faadfeed02acd2255ec8fae2227316407ad05d8
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

using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.FontBox.CFF;

public class Type2CharString
{
    private readonly List<object> _sequence;
    private readonly int _defaultWidthX;
    private readonly int _nominalWidthX;
    private GeneralPath? _path;
    private float _currentX;
    private float _currentY;
    private float _startX;
    private float _startY;
    private float _width;
    private bool _rendered;
    private bool _widthSet;
    private bool _contourOpen;

    public Type2CharString(string fontName, string glyphName, byte[] bytes)
        : this(
            fontName,
            glyphName,
            0,
            new Type2CharStringParser(fontName).Parse(bytes, null, null),
            1000,
            0,
            bytes)
    {
    }

    public Type2CharString(
        string fontName,
        string glyphName,
        int gid,
        List<object> sequence,
        int defaultWidthX,
        int nominalWidthX)
        : this(fontName, glyphName, gid, sequence, defaultWidthX, nominalWidthX, [])
    {
    }

    private Type2CharString(
        string fontName,
        string glyphName,
        int gid,
        List<object> sequence,
        int defaultWidthX,
        int nominalWidthX,
        byte[] bytes)
    {
        FontName = fontName;
        GlyphName = glyphName;
        GID = gid;
        Bytes = bytes;
        _sequence = sequence ?? throw new ArgumentNullException(nameof(sequence));
        _defaultWidthX = defaultWidthX;
        _nominalWidthX = nominalWidthX;
        _width = defaultWidthX;
    }

    public string FontName { get; }
    public string GlyphName { get; }
    public int GID { get; }
    public byte[] Bytes { get; private init; }
    public virtual GeneralPath GetPath()
    {
        EnsureRendered();
        return _path!;
    }

    public virtual float GetWidth()
    {
        EnsureRendered();
        return _width;
    }

    private void EnsureRendered()
    {
        if (_rendered)
        {
            return;
        }

        _path = new GeneralPath();
        _currentX = 0;
        _currentY = 0;
        _startX = 0;
        _startY = 0;
        _width = _defaultWidthX;
        _widthSet = false;
        _contourOpen = false;

        List<double> numbers = [];
        foreach (object entry in CollapseDivOperations(_sequence))
        {
            if (entry is CharStringCommand command)
            {
                HandleCommand(numbers, command);
            }
            else if (entry is double doubleValue)
            {
                numbers.Add(doubleValue);
            }
            else if (entry is int intValue)
            {
                numbers.Add(intValue);
            }
            else if (entry is float floatValue)
            {
                numbers.Add(floatValue);
            }
        }

        ClosePath();
        _rendered = true;
    }

    private static List<object> CollapseDivOperations(List<object> sequence)
    {
        List<object> collapsed = new(sequence.Count);
        foreach (object entry in sequence)
        {
            if (entry is CharStringCommand.DIV && collapsed.Count >= 2)
            {
                object denominator = collapsed[^1];
                object numerator = collapsed[^2];
                if (TryNumber(numerator, out double num) && TryNumber(denominator, out double den) && Math.Abs(den) > double.Epsilon)
                {
                    collapsed.RemoveAt(collapsed.Count - 1);
                    collapsed.RemoveAt(collapsed.Count - 1);
                    collapsed.Add(num / den);
                    continue;
                }
            }

            collapsed.Add(entry);
        }

        return collapsed;
    }

    private static bool TryNumber(object value, out double number)
    {
        switch (value)
        {
            case double d:
                number = d;
                return true;
            case float f:
                number = f;
                return true;
            case int i:
                number = i;
                return true;
            default:
                number = 0;
                return false;
        }
    }

    private void HandleCommand(List<double> numbers, CharStringCommand command)
    {
        switch (command.GetType2KeyWord())
        {
            case Type2KeyWord.HSTEM:
            case Type2KeyWord.HSTEMHM:
            case Type2KeyWord.VSTEM:
            case Type2KeyWord.VSTEMHM:
            case Type2KeyWord.HINTMASK:
            case Type2KeyWord.CNTRMASK:
                ClearStack(numbers, numbers.Count % 2 != 0);
                numbers.Clear();
                break;
            case Type2KeyWord.RMOVETO:
                ClearStack(numbers, numbers.Count > 2);
                if (numbers.Count >= 2)
                {
                    MoveToRelative(numbers[0], numbers[1]);
                }
                numbers.Clear();
                break;
            case Type2KeyWord.HMOVETO:
                ClearStack(numbers, numbers.Count > 1);
                if (numbers.Count >= 1)
                {
                    MoveToRelative(numbers[0], 0);
                }
                numbers.Clear();
                break;
            case Type2KeyWord.VMOVETO:
                ClearStack(numbers, numbers.Count > 1);
                if (numbers.Count >= 1)
                {
                    MoveToRelative(0, numbers[0]);
                }
                numbers.Clear();
                break;
            case Type2KeyWord.RLINETO:
                AddRelativeLines(numbers);
                numbers.Clear();
                break;
            case Type2KeyWord.HLINETO:
                AddAlternatingLines(numbers, horizontal: true);
                numbers.Clear();
                break;
            case Type2KeyWord.VLINETO:
                AddAlternatingLines(numbers, horizontal: false);
                numbers.Clear();
                break;
            case Type2KeyWord.RRCURVETO:
                AddRelativeCurves(numbers);
                numbers.Clear();
                break;
            case Type2KeyWord.RCURVELINE:
                AddCurveLine(numbers);
                numbers.Clear();
                break;
            case Type2KeyWord.RLINECURVE:
                AddLineCurve(numbers);
                numbers.Clear();
                break;
            case Type2KeyWord.VVCURVETO:
                AddVerticalCurves(numbers);
                numbers.Clear();
                break;
            case Type2KeyWord.HHCURVETO:
                AddHorizontalCurves(numbers);
                numbers.Clear();
                break;
            case Type2KeyWord.VHCURVETO:
                AddAlternatingCurves(numbers, horizontal: false);
                numbers.Clear();
                break;
            case Type2KeyWord.HVCURVETO:
                AddAlternatingCurves(numbers, horizontal: true);
                numbers.Clear();
                break;
            case Type2KeyWord.HFLEX:
                AddHFlex(numbers);
                numbers.Clear();
                break;
            case Type2KeyWord.FLEX:
                AddFlex(numbers);
                numbers.Clear();
                break;
            case Type2KeyWord.HFLEX1:
                AddHFlex1(numbers);
                numbers.Clear();
                break;
            case Type2KeyWord.FLEX1:
                AddFlex1(numbers);
                numbers.Clear();
                break;
            case Type2KeyWord.ENDCHAR:
                ClearStack(numbers, numbers.Count is 1 or 5);
                ClosePath();
                numbers.Clear();
                break;
            case Type2KeyWord.ADD:
                Binary(numbers, (a, b) => a + b);
                break;
            case Type2KeyWord.SUB:
                Binary(numbers, (a, b) => a - b);
                break;
            case Type2KeyWord.MUL:
                Binary(numbers, (a, b) => a * b);
                break;
            case Type2KeyWord.DIV:
                Binary(numbers, (a, b) => Math.Abs(b) > double.Epsilon ? a / b : 0);
                break;
            case Type2KeyWord.NEG:
                Unary(numbers, a => -a);
                break;
            case Type2KeyWord.ABS:
                Unary(numbers, Math.Abs);
                break;
            case Type2KeyWord.SQRT:
                Unary(numbers, Math.Sqrt);
                break;
            case Type2KeyWord.DROP:
                if (numbers.Count > 0)
                {
                    numbers.RemoveAt(numbers.Count - 1);
                }
                break;
            default:
                numbers.Clear();
                break;
        }
    }

    private void ClearStack(List<double> numbers, bool hasWidth)
    {
        if (_widthSet)
        {
            return;
        }

        if (hasWidth && numbers.Count > 0)
        {
            _width = _nominalWidthX + (float)numbers[0];
            numbers.RemoveAt(0);
        }
        else
        {
            _width = _defaultWidthX;
        }

        _widthSet = true;
    }

    private void MoveToRelative(double dx, double dy)
    {
        ClosePath();
        _currentX += (float)dx;
        _currentY += (float)dy;
        _path!.MoveTo(_currentX, _currentY);
        _startX = _currentX;
        _startY = _currentY;
        _contourOpen = true;
    }

    private void LineToRelative(double dx, double dy)
    {
        _currentX += (float)dx;
        _currentY += (float)dy;
        if (!_contourOpen)
        {
            _path!.MoveTo(_currentX, _currentY);
            _startX = _currentX;
            _startY = _currentY;
            _contourOpen = true;
            return;
        }

        _path!.LineTo(_currentX, _currentY);
    }

    private void CurveToRelative(double dx1, double dy1, double dx2, double dy2, double dx3, double dy3)
    {
        float x1 = _currentX + (float)dx1;
        float y1 = _currentY + (float)dy1;
        float x2 = x1 + (float)dx2;
        float y2 = y1 + (float)dy2;
        float x3 = x2 + (float)dx3;
        float y3 = y2 + (float)dy3;
        if (!_contourOpen)
        {
            _path!.MoveTo(x3, y3);
            _startX = x3;
            _startY = y3;
            _contourOpen = true;
        }
        else
        {
            _path!.CurveTo(x1, y1, x2, y2, x3, y3);
        }

        _currentX = x3;
        _currentY = y3;
    }

    private void ClosePath()
    {
        if (!_contourOpen)
        {
            return;
        }

        _path!.ClosePath();
        _contourOpen = false;
    }

    private void AddRelativeLines(List<double> numbers)
    {
        for (int i = 0; i + 1 < numbers.Count; i += 2)
        {
            LineToRelative(numbers[i], numbers[i + 1]);
        }
    }

    private void AddAlternatingLines(List<double> numbers, bool horizontal)
    {
        foreach (double value in numbers)
        {
            if (horizontal)
            {
                LineToRelative(value, 0);
            }
            else
            {
                LineToRelative(0, value);
            }

            horizontal = !horizontal;
        }
    }

    private void AddRelativeCurves(List<double> numbers)
    {
        for (int i = 0; i + 5 < numbers.Count; i += 6)
        {
            CurveToRelative(numbers[i], numbers[i + 1], numbers[i + 2], numbers[i + 3], numbers[i + 4], numbers[i + 5]);
        }
    }

    private void AddCurveLine(List<double> numbers)
    {
        int curveLimit = numbers.Count - 2;
        for (int i = 0; i + 5 < curveLimit; i += 6)
        {
            CurveToRelative(numbers[i], numbers[i + 1], numbers[i + 2], numbers[i + 3], numbers[i + 4], numbers[i + 5]);
        }

        if (numbers.Count >= 2)
        {
            LineToRelative(numbers[^2], numbers[^1]);
        }
    }

    private void AddLineCurve(List<double> numbers)
    {
        int lineLimit = numbers.Count - 6;
        for (int i = 0; i + 1 < lineLimit; i += 2)
        {
            LineToRelative(numbers[i], numbers[i + 1]);
        }

        if (numbers.Count >= 6)
        {
            int i = numbers.Count - 6;
            CurveToRelative(numbers[i], numbers[i + 1], numbers[i + 2], numbers[i + 3], numbers[i + 4], numbers[i + 5]);
        }
    }

    private void AddVerticalCurves(List<double> numbers)
    {
        int index = 0;
        double dx1 = numbers.Count % 4 == 1 ? numbers[index++] : 0;
        while (index + 3 < numbers.Count)
        {
            CurveToRelative(dx1, numbers[index], numbers[index + 1], numbers[index + 2], 0, numbers[index + 3]);
            index += 4;
            dx1 = 0;
        }
    }

    private void AddHorizontalCurves(List<double> numbers)
    {
        int index = 0;
        double dy1 = numbers.Count % 4 == 1 ? numbers[index++] : 0;
        while (index + 3 < numbers.Count)
        {
            CurveToRelative(numbers[index], dy1, numbers[index + 1], numbers[index + 2], numbers[index + 3], 0);
            index += 4;
            dy1 = 0;
        }
    }

    private void AddAlternatingCurves(List<double> numbers, bool horizontal)
    {
        int index = 0;
        while (index + 3 < numbers.Count)
        {
            bool last = numbers.Count - index == 5;
            if (horizontal)
            {
                CurveToRelative(
                    numbers[index],
                    0,
                    numbers[index + 1],
                    numbers[index + 2],
                    last ? numbers[index + 4] : 0,
                    numbers[index + 3]);
            }
            else
            {
                CurveToRelative(
                    0,
                    numbers[index],
                    numbers[index + 1],
                    numbers[index + 2],
                    numbers[index + 3],
                    last ? numbers[index + 4] : 0);
            }

            index += last ? 5 : 4;
            horizontal = !horizontal;
        }
    }

    private void AddHFlex(List<double> numbers)
    {
        if (numbers.Count < 7)
        {
            return;
        }

        CurveToRelative(numbers[0], 0, numbers[1], numbers[2], numbers[3], 0);
        CurveToRelative(numbers[4], 0, numbers[5], -numbers[2], numbers[6], 0);
    }

    private void AddFlex(List<double> numbers)
    {
        if (numbers.Count < 12)
        {
            return;
        }

        CurveToRelative(numbers[0], numbers[1], numbers[2], numbers[3], numbers[4], numbers[5]);
        CurveToRelative(numbers[6], numbers[7], numbers[8], numbers[9], numbers[10], numbers[11]);
    }

    private void AddHFlex1(List<double> numbers)
    {
        if (numbers.Count < 9)
        {
            return;
        }

        CurveToRelative(numbers[0], numbers[1], numbers[2], numbers[3], numbers[4], 0);
        CurveToRelative(numbers[5], 0, numbers[6], numbers[7], numbers[8], 0);
    }

    private void AddFlex1(List<double> numbers)
    {
        if (numbers.Count < 11)
        {
            return;
        }

        double dx = 0;
        double dy = 0;
        for (int i = 0; i < 5; i++)
        {
            dx += numbers[i * 2];
            dy += numbers[i * 2 + 1];
        }

        bool dxIsBigger = Math.Abs(dx) > Math.Abs(dy);
        CurveToRelative(numbers[0], numbers[1], numbers[2], numbers[3], numbers[4], numbers[5]);
        CurveToRelative(
            numbers[6],
            numbers[7],
            numbers[8],
            numbers[9],
            dxIsBigger ? numbers[10] : -dx,
            dxIsBigger ? -dy : numbers[10]);
    }

    private static void Binary(List<double> numbers, Func<double, double, double> op)
    {
        if (numbers.Count < 2)
        {
            return;
        }

        double b = numbers[^1];
        double a = numbers[^2];
        numbers.RemoveAt(numbers.Count - 1);
        numbers.RemoveAt(numbers.Count - 1);
        numbers.Add(op(a, b));
    }

    private static void Unary(List<double> numbers, Func<double, double> op)
    {
        if (numbers.Count == 0)
        {
            return;
        }

        numbers[^1] = op(numbers[^1]);
    }
}
