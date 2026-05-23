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
using System.Linq;

namespace PdfBox.Net.Text;

/// <summary>
/// This represents a string and a position on the screen of those characters.
/// </summary>
public sealed class TextPosition
{
    private const float Tolerance = 1E-07f;

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

        if (_rotation == 0)
        {
            _x = GetXRot(textMatrix);
            if (_textMatrix.GetScaleY() > 0)
            {
                _y = pageHeight - GetYLowerLeftRot(textMatrix);
            }
            else
            {
                _y = GetYLowerLeftRot(textMatrix);
            }
        }
        else if (_rotation == 90)
        {
            _x = GetYLowerLeftRot(textMatrix);
            _y = GetXRot(textMatrix);
        }
        else if (_rotation == 180)
        {
            _x = pageWidth - GetXRot(textMatrix);
            if (_textMatrix.GetScaleY() > 0)
            {
                _y = GetYLowerLeftRot(textMatrix);
            }
            else
            {
                _y = pageHeight - GetYLowerLeftRot(textMatrix);
            }
        }
        else if (_rotation == 270)
        {
            _x = pageWidth - GetYLowerLeftRot(textMatrix);
            _y = pageHeight - GetXRot(textMatrix);
        }
        else
        {
            _x = 0;
            _y = 0;
        }
    }

    private static float GetXRot(Matrix matrix)
    {
        return matrix.GetTranslateX();
    }

    private static float GetYLowerLeftRot(Matrix matrix)
    {
        return matrix.GetTranslateY();
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
        if (_rotation == 0)
        {
            return GetX();
        }
        else if (_rotation == 90)
        {
            return GetY();
        }
        else if (_rotation == 180)
        {
            return GetPageWidth() - GetX();
        }
        else if (_rotation == 270)
        {
            return GetPageHeight() - GetY();
        }

        return GetX();
    }

    /// <summary>
    /// Returns the Y position of the text, adjusted for the page rotation, so that 0 is the point
    /// from where y-value increases.
    /// </summary>
    /// <returns>The Y position.</returns>
    public float GetYDirAdj()
    {
        if (_rotation == 0 || _rotation == 90)
        {
            return GetY();
        }
        else if (_rotation == 180)
        {
            return GetPageHeight() - GetY();
        }
        else
        {
            return GetPageWidth() - GetX();
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
        if (_individualWidths == null)
        {
            return 0;
        }

        float sum = 0;
        foreach (float width in _individualWidths)
        {
            sum += width;
        }

        return sum;
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
        if (_rotation == 90 || _rotation == 270)
        {
            return GetWidth();
        }

        return GetHeight();
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
        float a = _textMatrix.GetScaleX();
        float b = _textMatrix.GetShearX();
        float c = _textMatrix.GetShearY();
        float d = _textMatrix.GetScaleY();

        if (NearlyZero(a) && NearlyZero(b) && NearlyZero(c) && NearlyZero(d))
        {
            return 0;
        }

        double angle;
        if (Math.Abs(a) < Math.Abs(b))
        {
            angle = Math.Atan2(a, -b);
        }
        else
        {
            angle = Math.Atan2(-c, d);
        }

        return (float)(angle * (180.0 / Math.PI));
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
        if (_rotation == 90 || _rotation == 270)
        {
            return GetHeight();
        }

        return GetWidth();
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
        return _unicode;
    }

    public bool IsDiacritic()
    {
        if (string.IsNullOrEmpty(_unicode))
        {
            return false;
        }

        return _unicode.All(ch => char.GetUnicodeCategory(ch) == System.Globalization.UnicodeCategory.NonSpacingMark);
    }

    public bool Contains(TextPosition other)
    {
        return CompletelyContains(other);
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
        _unicode += textPosition.GetUnicode();
        MergeSingleMultiplePositions(textPosition);
    }

    private static bool NearlyZero(float value)
    {
        return MathF.Abs(value) <= Tolerance;
    }
}
