/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDAppearanceContentStream.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 56575fd583792844b6bd182d67739d26568b1d01
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

using PdfBox.Net.ContentStream.Operator;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.State;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.PdfWriter;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel;

public sealed class PDAppearanceContentStream : ContentStreamForGlyphLayoutInterface, IDisposable
{
    private readonly PDAppearanceStream _appearance;
    private readonly Stream _output;
    private readonly bool _ownsStream;
    private readonly ContentStreamWriter _writer;
    private int _graphicsStateCounter;
    private GlyphLayoutProcessorInterface? _glyphLayoutProcessor;
    private PDFont? _currentFont;
    private float _currentFontSize;

    public PDAppearanceContentStream(PDAppearanceStream appearance)
        : this(appearance, appearance.GetStream().CreateOutputStream(), ownsStream: true)
    {
    }

    public PDAppearanceContentStream(PDAppearanceStream appearance, Stream output)
        : this(appearance, output, ownsStream: false)
    {
    }

    private PDAppearanceContentStream(PDAppearanceStream appearance, Stream output, bool ownsStream)
    {
        _appearance = appearance ?? throw new ArgumentNullException(nameof(appearance));
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _ownsStream = ownsStream;
        _writer = new ContentStreamWriter(_output);
        SaveGraphicsState();
    }

    public void Dispose()
    {
        RestoreGraphicsState();
        _output.Flush();
        if (_ownsStream)
        {
            _output.Dispose();
        }
    }

    public void BeginText() => WriteOperator("BT");

    public void EndText() => WriteOperator("ET");

    public void NewLineAtOffset(float tx, float ty) => WriteOperator("Td", tx, ty);

    public void ShowText(string text)
    {
        if (_glyphLayoutProcessor != null &&
            _currentFont is PDType0Font type0Font &&
            _glyphLayoutProcessor.SupportsFont(type0Font))
        {
            _glyphLayoutProcessor.ShowText(this, type0Font, _currentFontSize, text ?? string.Empty);
            return;
        }

        string value = text ?? string.Empty;
        COSString encoded = _currentFont is PDSimpleFont
            ? new COSString(_currentFont.Encode(value))
            : new COSString(value);
        WriteOperator("Tj", encoded);
    }

    public void SetFont(COSName fontName, float fontSize)
    {
        _currentFont = null;
        _currentFontSize = fontSize;
        WriteOperator("Tf", fontName, fontSize);
    }

    internal void SetFont(COSName fontName, PDFont font, float fontSize)
    {
        ArgumentNullException.ThrowIfNull(font);
        _currentFont = font;
        _currentFontSize = fontSize;
        WriteOperator("Tf", fontName, fontSize);
    }

    public void SetGlyphLayoutProcessor(GlyphLayoutProcessorInterface? glyphLayoutProcessor)
    {
        _glyphLayoutProcessor = glyphLayoutProcessor;
    }

    public void ShowGlyphsWithPositioning(GlyphsAndPositions glyphsAndPositions)
    {
        WriteOperator("TJ", GlyphLayoutContentStreamSupport.ToGlyphsAndPositionsArray(glyphsAndPositions));
    }

    public void ShowGlyphCodes(int[] glyphCodes)
    {
        ArgumentNullException.ThrowIfNull(glyphCodes);
        WriteOperator("Tj", GlyphLayoutContentStreamSupport.ToGlyphCodeString(glyphCodes));
    }

    public void SetTextRise(float rise) => WriteOperator("Ts", rise);

    public void MoveTo(float x, float y) => WriteOperator("m", x, y);

    public void LineTo(float x, float y) => WriteOperator("l", x, y);

    public void CurveTo(float x1, float y1, float x2, float y2, float x3, float y3) =>
        WriteOperator("c", x1, y1, x2, y2, x3, y3);

    public void AddRect(float x, float y, float width, float height) => WriteOperator("re", x, y, width, height);

    public void ClosePath() => WriteOperator("h");

    public void Stroke() => WriteOperator("S");

    public void Fill() => WriteOperator("f");

    public void FillAndStroke() => WriteOperator("B");

    public void CloseAndStroke() => WriteOperator("s");

    public void CloseAndFillAndStroke() => WriteOperator("b");

    public void Clip()
    {
        WriteOperator("W");
        WriteOperator("n");
    }

    public void SaveGraphicsState() => WriteOperator("q");

    public void RestoreGraphicsState() => WriteOperator("Q");

    public void SetLineWidth(float value) => WriteOperator("w", value);

    public void SetLineWidthOnDemand(float value)
    {
        if (Math.Abs(value - 1f) > 1e-6f)
        {
            SetLineWidth(value);
        }
    }

    public void SetLineCapStyle(int value) => WriteOperator("J", value);

    public void SetLineJoinStyle(int value) => WriteOperator("j", value);

    public void SetMiterLimit(float value) => WriteOperator("M", value);

    public void SetLineDashPattern(float[] dashArray, int phase)
    {
        COSArray dash = new();
        foreach (float value in dashArray)
        {
            dash.Add(new COSFloat(value));
        }

        WriteOperator("d", dash, phase);
    }

    public void Transform(Matrix matrix)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        WriteOperator("cm",
            matrix.GetScaleX(),
            matrix.GetShearY(),
            matrix.GetShearX(),
            matrix.GetScaleY(),
            matrix.GetTranslateX(),
            matrix.GetTranslateY());
    }

    public bool SetStrokingColorOnDemand(PDColor? color)
    {
        if (color == null || color.GetComponents().Length == 0)
        {
            return false;
        }

        SetStrokingColor(color);
        return true;
    }

    public bool SetNonStrokingColorOnDemand(PDColor? color)
    {
        if (color == null || color.GetComponents().Length == 0)
        {
            return false;
        }

        SetNonStrokingColor(color);
        return true;
    }

    public void SetStrokingColor(PDColor color) => SetColor(color.GetComponents(), stroking: true);

    public void SetNonStrokingColor(PDColor color) => SetColor(color.GetComponents(), stroking: false);

    public void SetStrokingColor(float gray) => SetColor([gray], stroking: true);

    public void SetNonStrokingColor(float gray) => SetColor([gray], stroking: false);

    public void SetNonStrokingColor(float r, float g, float b) => SetColor([r, g, b], stroking: false);

    public void SetBorderLine(float lineWidth, PDBorderStyleDictionary? borderStyle, COSArray border)
    {
        if (borderStyle != null &&
            string.Equals(borderStyle.GetStyle(), PDBorderStyleDictionary.STYLE_DASHED, StringComparison.Ordinal))
        {
            COSArray? dashArray = borderStyle.GetDashStyle();
            if (dashArray != null)
            {
                SetLineDashPattern(dashArray.ToFloatArray(), 0);
            }
        }
        else if (border.Size() > 3 && border.GetObject(3) is COSArray dashArray)
        {
            SetLineDashPattern(dashArray.ToFloatArray(), 0);
        }

        SetLineWidthOnDemand(lineWidth);
    }

    public void DrawShape(float lineWidth, bool hasStroke, bool hasFill)
    {
        bool stroke = hasStroke && lineWidth >= 1e-6f;
        if (hasFill && stroke)
        {
            FillAndStroke();
        }
        else if (stroke)
        {
            Stroke();
        }
        else if (hasFill)
        {
            Fill();
        }
        else
        {
            WriteOperator("n");
        }
    }

    public void SetGraphicsStateParameters(PDExtendedGraphicsState graphicsState)
    {
        ArgumentNullException.ThrowIfNull(graphicsState);

        COSName resourceName = COSName.GetPDFName($"GS{_graphicsStateCounter++}");
        PDResources resources = _appearance.GetResources() ?? new PDResources();
        _appearance.SetResources(resources);
        resources.Put(resourceName, graphicsState);
        WriteOperator("gs", resourceName);
    }

    private void SetColor(float[] components, bool stroking)
    {
        string? op = components.Length switch
        {
            1 when stroking => "G",
            1 => "g",
            3 when stroking => "RG",
            3 => "rg",
            4 when stroking => "K",
            4 => "k",
            _ => null
        };

        if (op == null)
        {
            return;
        }
        WriteOperator(op, components.Cast<object>().ToArray());
    }

    private void WriteOperator(string name, params object[] operands)
    {
        List<object> tokens = new(operands.Length + 1);
        foreach (object operand in operands)
        {
            tokens.Add(ToCosOperand(operand));
        }

        tokens.Add(Operator.GetOperator(name));
        _writer.WriteTokens(tokens);
    }

    private static object ToCosOperand(object operand) =>
        operand switch
        {
            COSBase cosBase => cosBase,
            COSObjectable objectable => objectable.GetCOSObject(),
            int intValue => COSInteger.Get(intValue),
            float floatValue => new COSFloat(floatValue),
            double doubleValue => new COSFloat((float)doubleValue),
            long longValue => COSInteger.Get(longValue),
            string text => new COSString(text),
            _ => throw new ArgumentException(
                $"Unsupported appearance operand type {operand.GetType().FullName}",
                nameof(operand))
        };
}
