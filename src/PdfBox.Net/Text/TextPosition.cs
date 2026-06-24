/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/text/TextPosition.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
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

using PdfBox.Net.PDModel.Font;
using PdfBox.Net.Util;
using System.Text;
using System.Linq;

namespace PdfBox.Net.Text;

/// <summary>
/// This represents a string and a position on the screen of those characters.
/// </summary>
public sealed class TextPosition
{
    private const float Tolerance = 1E-07f;
    private static readonly Dictionary<int, string> Diacritics = CreateDiacritics();

    private readonly int[] _charCodes;
    private readonly Matrix _textMatrix;
    private readonly float _endX;
    private readonly float _endY;
    private readonly float _maxTextHeight;
    private readonly int _rotation;
    private readonly float _x;
    private readonly float _y;
    private readonly float _pageHeight;
    private readonly float _pageWidth;
    private readonly float _widthOfSpace;
    private string _unicode;
    private readonly PDFont _font;
    private readonly float _fontSize;
    private readonly int _fontSizePt;

    private float[]? _individualWidths;
    private float _fontSizeInPt;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="pageRotation">Rotation of the page that the text is located in.</param>
    /// <param name="pageWidth">Width of the page that the text is located in.</param>
    /// <param name="pageHeight">Height of the page that the text is located in.</param>
    /// <param name="textMatrix">Text rendering matrix for this text.</param>
    /// <param name="endX">X coordinate of the end of the text.</param>
    /// <param name="endY">Y coordinate of the end of the text.</param>
    /// <param name="maxHeight">Maximum height of this text.</param>
    /// <param name="individualWidth">Individual width of this text's character(s).</param>
    /// <param name="spaceWidth">Width of a single space character.</param>
    /// <param name="unicode">The string of Unicode characters to be displayed.</param>
    /// <param name="codes">Internal PDF character codes.</param>
    /// <param name="font">The font.</param>
    /// <param name="fontSize">The font size in pt.</param>
    /// <param name="fontSizeInPt">The font size in pt.</param>
    public TextPosition(int pageRotation, float pageWidth, float pageHeight, Matrix textMatrix,
        float endX, float endY, float maxHeight, float individualWidth,
        float spaceWidth, string unicode, int[] codes, PDFont font,
        float fontSize, int fontSizeInPt)
    {
        _rotation = pageRotation;
        _textMatrix = textMatrix;
        _endX = endX;
        _endY = endY;
        _maxTextHeight = maxHeight;
        _pageHeight = pageHeight;
        _pageWidth = pageWidth;
        _individualWidths = [individualWidth];
        _widthOfSpace = spaceWidth;
        _unicode = unicode;
        _charCodes = codes;
        _font = font;
        _fontSize = fontSize;
        _fontSizePt = fontSizeInPt;
        _fontSizeInPt = fontSizeInPt;

        _x = GetXRot(_rotation);
        if (_rotation == 0 || _rotation == 180)
        {
            _y = _pageHeight - GetYLowerLeftRot(_rotation);
        }
        else
        {
            _y = _pageWidth - GetYLowerLeftRot(_rotation);
        }
    }

    private float GetXRot(float rotation)
    {
        if (rotation.CompareTo(0f) == 0)
        {
            return _textMatrix.GetTranslateX();
        }
        else if (rotation.CompareTo(90f) == 0)
        {
            return _textMatrix.GetTranslateY();
        }
        else if (rotation.CompareTo(180f) == 0)
        {
            return _pageWidth - _textMatrix.GetTranslateX();
        }
        else if (rotation.CompareTo(270f) == 0)
        {
            return _pageHeight - _textMatrix.GetTranslateY();
        }

        return 0;
    }

    private float GetYLowerLeftRot(float rotation)
    {
        if (rotation.CompareTo(0f) == 0)
        {
            return _textMatrix.GetTranslateY();
        }
        else if (rotation.CompareTo(90f) == 0)
        {
            return _pageWidth - _textMatrix.GetTranslateX();
        }
        else if (rotation.CompareTo(180f) == 0)
        {
            return _pageHeight - _textMatrix.GetTranslateY();
        }
        else if (rotation.CompareTo(270f) == 0)
        {
            return _textMatrix.GetTranslateX();
        }

        return 0;
    }

    /// <summary>
    /// Return the string of characters represented by this object.
    /// </summary>
    /// <returns>The string of characters.</returns>
    public string GetUnicode()
    {
        return _unicode;
    }

    /// <summary>
    /// Return the internal PDF character codes.
    /// </summary>
    /// <returns>The internal PDF character codes.</returns>
    public int[] GetCharacterCodes()
    {
        return _charCodes;
    }

    /// <summary>
    /// Returns the text matrix. Contains at least the rotation and scaling for this text position.
    /// </summary>
    /// <returns>The text matrix.</returns>
    public Matrix GetTextMatrix()
    {
        return _textMatrix;
    }

    /// <summary>
    /// Returns the rotation for the page that this text object is on.
    /// </summary>
    /// <returns>The page rotation.</returns>
    public int GetRotation()
    {
        return _rotation;
    }

    /// <summary>
    /// Returns the X position of the text, adjusted so that 0 is at the left and the value
    /// increases to the right.
    /// </summary>
    /// <returns>The X position.</returns>
    public float GetX()
    {
        return _x;
    }

    /// <summary>
    /// Returns the Y position of the text, adjusted so that 0 is at the top and the value
    /// increases downward.
    /// </summary>
    /// <returns>The Y position.</returns>
    public float GetY()
    {
        return _y;
    }

    /// <summary>
    /// Returns the X position of the text in device space, taking into account page rotation.
    /// </summary>
    /// <returns>The X position.</returns>
    public float GetXDirAdj()
    {
        return GetXRot(GetDir());
    }

    /// <summary>
    /// Returns the Y position of the text, adjusted for the page rotation, so that 0 is the point
    /// from where y-value increases.
    /// </summary>
    /// <returns>The Y position.</returns>
    public float GetYDirAdj()
    {
        float dir = GetDir();
        if (dir.CompareTo(0f) == 0 || dir.CompareTo(180f) == 0)
        {
            return _pageHeight - GetYLowerLeftRot(dir);
        }
        else
        {
            return _pageWidth - GetYLowerLeftRot(dir);
        }
    }

    /// <summary>
    /// Get the X position of the start of this text.
    /// </summary>
    /// <returns>The X start position of the text.</returns>
    public float GetXScale()
    {
        return _textMatrix.GetScaleX();
    }

    /// <summary>
    /// Get the Y scale (skew and scale) of this text.
    /// </summary>
    /// <returns>The Y scale.</returns>
    public float GetYScale()
    {
        return _textMatrix.GetScaleY();
    }

    /// <summary>
    /// Gets the X coordinate of the end of the text. Used for text right-to-left.
    /// </summary>
    /// <returns>The X end position of the text.</returns>
    public float GetXPad()
    {
        return _endX;
    }

    /// <summary>
    /// Gets the Y coordinate of the end of the text. Used for text right-to-left.
    /// </summary>
    /// <returns>The Y end position of the text.</returns>
    public float GetYPad()
    {
        return _endY;
    }

    /// <summary>
    /// Gets the font size in points as it would be rendered.
    /// </summary>
    /// <returns>The font size in points.</returns>
    public float GetFontSize()
    {
        return _fontSize;
    }

    /// <summary>
    /// Gets the font size in points as it would be rendered.
    /// </summary>
    /// <returns>The font size in points.</returns>
    public int GetFontSizeInPt()
    {
        return _fontSizePt;
    }

    /// <summary>
    /// This will get the font for this text position.
    /// </summary>
    /// <returns>The font for this text position.</returns>
    public PDFont GetFont()
    {
        return _font;
    }

    /// <summary>
    /// This will get the width of the text in display units. This is derived from the individual
    /// character widths.
    /// </summary>
    /// <returns>The width of the text in display units.</returns>
    public float GetWidth()
    {
        return GetWidthRot(_rotation);
    }

    private float GetWidthRot(float rotation)
    {
        if (rotation.CompareTo(90f) == 0 || rotation.CompareTo(270f) == 0)
        {
            return MathF.Abs(_endY - _textMatrix.GetTranslateY());
        }

        return MathF.Abs(_endX - _textMatrix.GetTranslateX());
    }

    /// <summary>
    /// This will get the width of a space character.
    /// </summary>
    /// <returns>The width of a space character in display units.</returns>
    public float GetWidthOfSpace()
    {
        return _widthOfSpace;
    }

    /// <summary>
    /// This will get the maximum height of this text.
    /// </summary>
    /// <returns>The maximum height of this text.</returns>
    public float GetHeight()
    {
        return _maxTextHeight;
    }

    /// <summary>
    /// This will get the height in display units.
    /// </summary>
    /// <returns>The height.</returns>
    public float GetHeightDir()
    {
        return _maxTextHeight;
    }

    /// <summary>
    /// This will get the font size that this object is supposed to be drawn at.
    /// </summary>
    /// <returns>The font size.</returns>
    public float GetFontSizeInPtFloat()
    {
        return _fontSizeInPt;
    }

    /// <summary>
    /// This will return the direction/orientation of the string.
    /// </summary>
    /// <returns>The direction of the text.</returns>
    public float GetDir()
    {
        float a = _textMatrix.GetScaleY();
        float b = _textMatrix.GetShearY();
        float c = _textMatrix.GetShearX();
        float d = _textMatrix.GetScaleX();

        if (a > 0 && MathF.Abs(b) < d && MathF.Abs(c) < a && d > 0)
        {
            return 0;
        }
        else if (a < 0 && MathF.Abs(b) < MathF.Abs(d) && MathF.Abs(c) < MathF.Abs(a) && d < 0)
        {
            return 180;
        }
        else if (MathF.Abs(a) < MathF.Abs(c) && b > 0 && c < 0 && MathF.Abs(d) < b)
        {
            return 90;
        }
        else if (MathF.Abs(a) < c && b < 0 && c > 0 && MathF.Abs(d) < MathF.Abs(b))
        {
            return 270;
        }

        return 0;
    }

    /// <summary>
    /// This is the page height value that was passed in the constructor.
    /// </summary>
    /// <returns>The page height.</returns>
    public float GetPageHeight()
    {
        return _pageHeight;
    }

    /// <summary>
    /// This is the page width value that was passed in the constructor.
    /// </summary>
    /// <returns>The page width.</returns>
    public float GetPageWidth()
    {
        return _pageWidth;
    }

    /// <summary>
    /// This will return the text direction adjusted width of the string.
    /// </summary>
    /// <returns>The direction adjusted width.</returns>
    public float GetWidthDirAdj()
    {
        return GetWidthRot(GetDir());
    }

    /// <summary>
    /// This will set the individual character widths.
    /// </summary>
    /// <param name="individualWidths">An array of individual character widths.</param>
    internal void SetIndividualWidths(float[] individualWidths)
    {
        _individualWidths = individualWidths;
    }

    /// <summary>
    /// This will get the individual character widths.
    /// </summary>
    /// <returns>An array of individual character widths.</returns>
    public float[] GetIndividualWidths()
    {
        return _individualWidths ?? Array.Empty<float>();
    }

    /// <summary>
    /// Show the string data for this text position.
    /// </summary>
    /// <returns>A human readable form of this object.</returns>
    public override string ToString()
    {
        return GetUnicode();
    }

    /// <summary>
    /// Returns the width of the glyph in text space units.
    /// </summary>
    /// <param name="pdfMatrix">The rendering matrix to transform the raw unscaled metrics.</param>
    /// <returns>The width of the glyph in text space units.</returns>
    public float GetWidthRaw(Matrix pdfMatrix)
    {
        if (_individualWidths == null || _individualWidths.Length == 0)
        {
            return 0;
        }

        return _individualWidths[0] / (pdfMatrix.GetScaleX() * _textMatrix.GetScaleX());
    }

    /// <summary>
    /// Returns the height of the rendering matrix in text space units.
    /// </summary>
    /// <param name="pdfMatrix">The rendering matrix to transform the raw unscaled metrics.</param>
    /// <returns>The height of the rendering matrix in text space units.</returns>
    public float GetHeightRaw(Matrix pdfMatrix)
    {
        if (_rotation == 90 || _rotation == 270)
        {
            return pdfMatrix.GetScaleX() / _textMatrix.GetScalingFactorX();
        }

        return pdfMatrix.GetScaleY() / _textMatrix.GetScalingFactorY();
    }

    /// <summary>
    /// Merge a single character result from a font into this text position.
    /// </summary>
    /// <param name="textPosition">The <see cref="TextPosition"/> to merge into this one.</param>
    public void MergeSingleMultiplePositions(TextPosition textPosition)
    {
        if (textPosition._individualWidths != null)
        {
            float[] source = _individualWidths ?? Array.Empty<float>();
            float[] tmp = new float[source.Length + textPosition._individualWidths.Length];
            Array.Copy(source, 0, tmp, 0, source.Length);
            Array.Copy(textPosition._individualWidths, 0, tmp, source.Length,
                textPosition._individualWidths.Length);
            _individualWidths = tmp;
        }
    }

    public void SetUnicode(string unicode)
    {
        _unicode = unicode;
    }

    public string GetVisuallyOrderedUnicode()
    {
        int length = _unicode.Length;
        int nextIndex;
        for (int index = 0; index < length; index = nextIndex)
        {
            int codePoint = char.ConvertToUtf32(_unicode, index);
            nextIndex = index + (char.IsSurrogatePair(_unicode, index) ? 2 : 1);
            if (IsRightToLeft(codePoint) && (index != 0 || nextIndex < length))
            {
                char[] chars = _unicode.ToCharArray();
                Array.Reverse(chars);
                return new string(chars);
            }
        }

        return _unicode;
    }

    private static bool IsRightToLeft(int codePoint)
    {
        return (codePoint >= 0x0590 && codePoint <= 0x08FF)
            || (codePoint >= 0xFB1D && codePoint <= 0xFDFF)
            || (codePoint >= 0xFE70 && codePoint <= 0xFEFF);
    }

    public bool Contains(TextPosition other)
    {
        double thisXStart = GetXDirAdj();
        double thisWidth = GetWidthDirAdj();
        double thisXEnd = thisXStart + thisWidth;

        double otherXStart = other.GetXDirAdj();
        double otherXEnd = otherXStart + other.GetWidthDirAdj();

        if (otherXEnd <= thisXStart || otherXStart >= thisXEnd)
        {
            return false;
        }

        double thisYStart = GetYDirAdj();
        double otherYStart = other.GetYDirAdj();
        if (otherYStart + other.GetHeightDir() < thisYStart
            || otherYStart > thisYStart + GetHeightDir())
        {
            return false;
        }

        if (otherXStart > thisXStart && otherXEnd > thisXEnd)
        {
            double overlap = thisXEnd - otherXStart;
            double overlapPercent = overlap / thisWidth;
            return overlapPercent > .15;
        }

        if (otherXStart < thisXStart && otherXEnd < thisXEnd)
        {
            double overlap = otherXEnd - thisXStart;
            double overlapPercent = overlap / thisWidth;
            return overlapPercent > .15;
        }

        return true;
    }

    public bool CompletelyContains(TextPosition other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return other.GetXDirAdj() >= GetXDirAdj() &&
               other.GetYDirAdj() >= GetYDirAdj() - GetHeightDir() &&
               other.GetXDirAdj() + other.GetWidthDirAdj() <= GetXDirAdj() + GetWidthDirAdj() &&
               other.GetYDirAdj() <= GetYDirAdj();
    }

    public void MergeDiacritic(TextPosition textPosition)
    {
        ArgumentNullException.ThrowIfNull(textPosition);
        if (textPosition.GetUnicode().Length > 1)
        {
            return;
        }

        float diacriticXStart = textPosition.GetXDirAdj();
        float diacriticXEnd = diacriticXStart + (textPosition._individualWidths is { Length: > 0 } widths
            ? widths[0]
            : textPosition.GetWidthDirAdj());

        float currentCharXStart = GetXDirAdj();

        int strLength = _unicode.Length;
        bool wasAdded = false;
        float[] currentWidths = _individualWidths ?? Array.Empty<float>();

        for (int i = 0; i < strLength && !wasAdded; i++)
        {
            if (i >= currentWidths.Length)
            {
                break;
            }

            float currentCharXEnd = currentCharXStart + currentWidths[i];
            if (diacriticXStart < currentCharXStart && diacriticXEnd <= currentCharXEnd)
            {
                if (i == 0)
                {
                    InsertDiacritic(i, textPosition);
                }
                else
                {
                    float distanceOverlapping1 = diacriticXEnd - currentCharXStart;
                    float percentage1 = distanceOverlapping1 / currentWidths[i];

                    float distanceOverlapping2 = currentCharXStart - diacriticXStart;
                    float percentage2 = distanceOverlapping2 / currentWidths[i - 1];

                    InsertDiacritic(percentage1 >= percentage2 ? i : i - 1, textPosition);
                }

                wasAdded = true;
            }
            else if (diacriticXStart < currentCharXStart)
            {
                InsertDiacritic(i, textPosition);
                wasAdded = true;
            }
            else if (diacriticXEnd <= currentCharXEnd)
            {
                InsertDiacritic(i, textPosition);
                wasAdded = true;
            }
            else if (i == strLength - 1)
            {
                InsertDiacritic(i, textPosition);
                wasAdded = true;
            }

            currentCharXStart += currentWidths[i];
        }
    }

    private void InsertDiacritic(int index, TextPosition diacritic)
    {
        float[] widths = _individualWidths ?? Array.Empty<float>();
        StringBuilder builder = new();
        builder.Append(_unicode, 0, index);

        float[] widths2 = new float[widths.Length + 1];
        Array.Copy(widths, 0, widths2, 0, index);
        widths2[index] = widths[index];
        widths2[index + 1] = 0;
        Array.Copy(widths, index + 1, widths2, index + 2, widths.Length - index - 1);

        builder.Append(_unicode[index]);
        if (index < _unicode.Length - 1 && char.IsSurrogatePair(_unicode[index], _unicode[index + 1]))
        {
            builder.Append(_unicode[index + 1]);
            index++;
        }

        builder.Append(CombineDiacritic(diacritic.GetUnicode()));
        builder.Append(_unicode[(index + 1)..]);

        _unicode = builder.ToString();
        _individualWidths = widths2;
    }

    private static string CombineDiacritic(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        int codePoint = char.ConvertToUtf32(value, 0);
        if (Diacritics.TryGetValue(codePoint, out string? mapped))
        {
            return mapped;
        }

        return value.Normalize(NormalizationForm.FormKC).Trim();
    }

    public bool IsDiacritic()
    {
        if (_unicode.Length != 1 || _unicode == "ー")
        {
            return false;
        }

        System.Globalization.UnicodeCategory type = char.GetUnicodeCategory(_unicode[0]);
        return type == System.Globalization.UnicodeCategory.NonSpacingMark
            || type == System.Globalization.UnicodeCategory.ModifierSymbol
            || type == System.Globalization.UnicodeCategory.ModifierLetter;
    }

    private static Dictionary<int, string> CreateDiacritics()
    {
        return new Dictionary<int, string>
        {
            [0x0060] = "\u0300",
            [0x02CB] = "\u0300",
            [0x0027] = "\u0301",
            [0x02B9] = "\u0301",
            [0x02CA] = "\u0301",
            [0x005e] = "\u0302",
            [0x02C6] = "\u0302",
            [0x007E] = "\u0303",
            [0x02C9] = "\u0304",
            [0x00B0] = "\u030A",
            [0x02BA] = "\u030B",
            [0x02C7] = "\u030C",
            [0x02C8] = "\u030D",
            [0x0022] = "\u030E",
            [0x02BB] = "\u0312",
            [0x02BC] = "\u0313",
            [0x0486] = "\u0313",
            [0x055A] = "\u0313",
            [0x02BD] = "\u0314",
            [0x0485] = "\u0314",
            [0x0559] = "\u0314",
            [0x02D4] = "\u031D",
            [0x02D5] = "\u031E",
            [0x02D6] = "\u031F",
            [0x02D7] = "\u0320",
            [0x02B2] = "\u0321",
            [0x02CC] = "\u0329",
            [0x02B7] = "\u032B",
            [0x02CD] = "\u0331",
            [0x204E] = "\u0359"
        };
    }

    private static bool NearlyZero(float value)
    {
        return MathF.Abs(value) <= Tolerance;
    }
}
