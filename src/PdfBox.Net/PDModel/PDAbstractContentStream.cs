/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDAbstractContentStream.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
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
using PdfBox.Net.PDModel.DocumentInterchange.MarkedContent;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.PdfWriter;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel;

/// <summary>
/// Base class for PDModel content stream writers.
/// </summary>
public abstract class PDAbstractContentStream : IDisposable
{
    private readonly Stream _output;
    private readonly bool _ownsStream;
    private readonly ContentStreamWriter _writer;
    private bool _disposed;

    protected PDResources? Resources { get; }

    protected PDAbstractContentStream(Stream output, PDResources? resources, bool ownsStream)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _ownsStream = ownsStream;
        _writer = new ContentStreamWriter(_output);
        Resources = resources;
    }

    public virtual void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _output.Flush();
        if (_ownsStream)
        {
            _output.Dispose();
        }
    }

    public void BeginText() => WriteOperator("BT");
    public void EndText() => WriteOperator("ET");
    public void ShowText(string text) => WriteOperator("Tj", new COSString(text ?? string.Empty));
    public void NewLineAtOffset(float tx, float ty) => WriteOperator("Td", tx, ty);
    public void SetFont(COSName fontName, float fontSize) => WriteOperator("Tf", fontName, fontSize);

    public void MoveTo(float x, float y) => WriteOperator("m", x, y);
    public void LineTo(float x, float y) => WriteOperator("l", x, y);
    public void CurveTo(float x1, float y1, float x2, float y2, float x3, float y3) => WriteOperator("c", x1, y1, x2, y2, x3, y3);
    public void AddRect(float x, float y, float width, float height) => WriteOperator("re", x, y, width, height);
    public void ClosePath() => WriteOperator("h");
    public void Stroke() => WriteOperator("S");
    public void Fill() => WriteOperator("f");
    public void FillAndStroke() => WriteOperator("B");
    public void CloseAndStroke() => WriteOperator("s");
    public void CloseAndFillAndStroke() => WriteOperator("b");

    public void SaveGraphicsState() => WriteOperator("q");
    public void RestoreGraphicsState() => WriteOperator("Q");

    /// <summary>Sets the stroking color in the DeviceRGB color space. Range is 0..1.</summary>
    public void SetStrokingColor(float r, float g, float b) => WriteOperator("RG", r, g, b);

    /// <summary>Sets the non-stroking (fill) color in the DeviceRGB color space. Range is 0..1.</summary>
    public void SetNonStrokingColor(float r, float g, float b) => WriteOperator("rg", r, g, b);

    public void Clip()
    {
        WriteOperator("W");
        WriteOperator("n");
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

    public void DrawForm(PDFormXObject form)
    {
        ArgumentNullException.ThrowIfNull(form);
        if (Resources is null)
        {
            throw new MissingResourceException("Content stream has no resource dictionary.");
        }

        COSName formName = Resources.Add(form, "Form");
        WriteOperator("Do", formName);
    }

    public void BeginMarkedContent(COSName tag, PDPropertyList properties)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(properties);
        if (Resources is null)
        {
            throw new MissingResourceException("Content stream has no resource dictionary.");
        }

        COSName propName = Resources.Add(properties, "MC");
        WriteOperator("BDC", tag, propName);
    }

    public void EndMarkedContent() => WriteOperator("EMC");

    protected void WriteOperator(string name, params object[] operands)
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
                $"Unsupported content stream operand type {operand.GetType().FullName}",
                nameof(operand))
        };
}
