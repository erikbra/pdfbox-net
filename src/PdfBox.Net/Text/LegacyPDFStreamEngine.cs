/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/text/LegacyPDFStreamEngine.java
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

using PdfBox.Net.ContentStream;
using PdfBox.Net.ContentStream.Operator;
using PdfBox.Net.ContentStream.Operator.State;
using PdfBox.Net.ContentStream.Operator.Text;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Font.Encoding;
using PdfBox.Net.PDModel.Graphics.State;
using PdfBox.Net.Util;

namespace PdfBox.Net.Text;

/// <summary>
/// LEGACY text calculations which are known to be incorrect but are depended on by PDFTextStripper.
/// </summary>
public class LegacyPDFStreamEngine : PDFStreamEngine
{
    private int _pageRotation;
    private PDRectangle? _pageSize;
    private Matrix? _translateMatrix;
    private static readonly GlyphList GLYPHLIST = new(GlyphList.GetAdobeGlyphList(), null);
    private readonly Dictionary<COSDictionary, float> _fontHeightMap = new();

    internal LegacyPDFStreamEngine()
    {
        AddOperator(new BeginText(this));
        AddOperator(new Concatenate(this));
        AddOperator(new DrawObject(this));
        AddOperator(new EndText(this));
        AddOperator(new SetGraphicsStateParameters(this));
        AddOperator(new Save(this));
        AddOperator(new Restore(this));
        AddOperator(new NextLine(this));
        AddOperator(new SetCharSpacing(this));
        AddOperator(new MoveText(this));
        AddOperator(new MoveTextSetLeading(this));
        AddOperator(new SetFontAndSize(this));
        AddOperator(new ShowText(this));
        AddOperator(new ShowTextAdjusted(this));
        AddOperator(new SetTextLeading(this));
        AddOperator(new SetMatrix(this));
        AddOperator(new SetTextRenderingMode(this));
        AddOperator(new SetTextRise(this));
        AddOperator(new SetWordSpacing(this));
        AddOperator(new SetTextHorizontalScaling(this));
        AddOperator(new ShowTextLine(this));
        AddOperator(new ShowTextLineAndSpace(this));
    }

    public override void ProcessPage(PDPage page)
    {
        _pageRotation = page.GetRotation();
        _pageSize = page.GetCropBox();
        if (_pageSize.GetLowerLeftX().CompareTo(0f) == 0 && _pageSize.GetLowerLeftY().CompareTo(0f) == 0)
        {
            _translateMatrix = null;
        }
        else
        {
            _translateMatrix = Matrix.GetTranslateInstance(-_pageSize.GetLowerLeftX(), -_pageSize.GetLowerLeftY());
        }

        base.ProcessPage(page);
    }

    protected override void ShowGlyph(Matrix textRenderingMatrix, PDFont font, int code, Vector displacement)
    {
        PDGraphicsState state = GetGraphicsState();
        Matrix ctm = state.GetCurrentTransformationMatrix();
        float fontSize = state.GetTextState().GetFontSize();
        float horizontalScaling = state.GetTextState().GetHorizontalScaling() / 100f;
        Matrix textMatrix = GetTextMatrix();

        float displacementX = displacement.GetX();
        if (font.IsVertical())
        {
            displacementX = font.GetWidth(code) / 1000f;
            TrueTypeFont? ttf = null;
            if (font is PDTrueTypeFont trueTypeFont)
            {
                ttf = trueTypeFont.GetTrueTypeFont();
            }
            else if (font is PDType0Font type0Font)
            {
                PDCIDFont? cidFont = type0Font.GetDescendantFont();
                if (cidFont is PDCIDFontType2 cidFontType2)
                {
                    ttf = cidFontType2.GetTrueTypeFont();
                }
            }

            if (ttf != null && ttf.GetUnitsPerEm() != 1000)
            {
                displacementX *= 1000f / ttf.GetUnitsPerEm();
            }
        }

        float tx = displacementX * fontSize * horizontalScaling;
        float ty = displacement.GetY() * fontSize;
        Matrix td = Matrix.GetTranslateInstance(tx, ty);
        Matrix nextTextRenderingMatrix = td.Multiply(textMatrix).Multiply(ctm);
        float nextX = nextTextRenderingMatrix.GetTranslateX();
        float nextY = nextTextRenderingMatrix.GetTranslateY();

        if (!_fontHeightMap.TryGetValue(font.GetCOSObject(), out float fontHeight))
        {
            fontHeight = ComputeFontHeight(font);
            _fontHeightMap[font.GetCOSObject()] = fontHeight;
        }

        float dxDisplay = nextX - textRenderingMatrix.GetTranslateX();
        float dyDisplay = fontHeight * textRenderingMatrix.GetScalingFactorY();

        float glyphSpaceToTextSpaceFactor = 1 / 1000f;
        if (font is PDType3Font)
        {
            glyphSpaceToTextSpaceFactor = font.GetFontMatrix().GetScaleX();
        }

        float spaceWidthText;
        try
        {
            spaceWidthText = font.GetSpaceWidth() * glyphSpaceToTextSpaceFactor;
        }
        catch
        {
            spaceWidthText = 0f;
        }

        if (spaceWidthText.CompareTo(0f) == 0)
        {
            spaceWidthText = font.GetAverageFontWidth() * glyphSpaceToTextSpaceFactor;
            spaceWidthText *= .80f;
        }

        if (spaceWidthText.CompareTo(0f) == 0)
        {
            spaceWidthText = 1.0f;
        }

        float spaceWidthDisplay = spaceWidthText * textRenderingMatrix.GetScalingFactorX();
        string? unicode = font.ToUnicode(code, GLYPHLIST);
        if (unicode == null)
        {
            if (font is PDSimpleFont)
            {
                unicode = ((char)code).ToString();
            }
            else
            {
                return;
            }
        }

        Matrix translatedTextRenderingMatrix;
        if (_translateMatrix == null || _pageSize == null)
        {
            translatedTextRenderingMatrix = textRenderingMatrix;
        }
        else
        {
            translatedTextRenderingMatrix = Matrix.Concatenate(_translateMatrix, textRenderingMatrix);
            nextX -= _pageSize.GetLowerLeftX();
            nextY -= _pageSize.GetLowerLeftY();
        }

        if (_pageSize == null)
        {
            return;
        }

        ProcessTextPosition(new TextPosition(
            _pageRotation,
            _pageSize.GetWidth(),
            _pageSize.GetHeight(),
            translatedTextRenderingMatrix,
            nextX,
            nextY,
            MathF.Abs(dyDisplay),
            dxDisplay,
            MathF.Abs(spaceWidthDisplay),
            unicode,
            [code],
            font,
            fontSize,
            (int)(fontSize * textMatrix.GetScalingFactorX())));
    }

    protected float ComputeFontHeight(PDFont font)
    {
        BoundingBox bbox = font.GetBoundingBox();
        if (bbox.GetLowerLeftY() < short.MinValue)
        {
            bbox.SetLowerLeftY(-(bbox.GetLowerLeftY() + 65536));
        }

        float glyphHeight = bbox.GetHeight() / 2;
        PDFontDescriptor? fontDescriptor = font.GetFontDescriptor();
        if (fontDescriptor != null)
        {
            float capHeight = fontDescriptor.GetCapHeight();
            if (capHeight.CompareTo(0f) != 0 && (capHeight < glyphHeight || glyphHeight.CompareTo(0f) == 0))
            {
                glyphHeight = capHeight;
            }

            float ascent = fontDescriptor.GetAscent();
            float descent = fontDescriptor.GetDescent();
            if (capHeight > ascent && ascent > 0 && descent < 0 && (((ascent - descent) / 2) < glyphHeight || glyphHeight.CompareTo(0f) == 0))
            {
                glyphHeight = (ascent - descent) / 2;
            }
        }

        if (font is PDType3Font)
        {
            return font.GetFontMatrix().TransformPoint(0, glyphHeight).GetY();
        }

        return glyphHeight / 1000f;
    }

    protected virtual void ProcessTextPosition(TextPosition text)
    {
    }
}
