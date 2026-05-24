/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/contentstream/PDFStreamEngine.java
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

using PdfBox.Net.ContentStream.Operator;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Graphics;
using PdfBox.Net.PDModel.Graphics.State;
using PdfBox.Net.PdfParser;
using PdfBox.Net.Util;
using ContentOperator = PdfBox.Net.ContentStream.Operator.Operator;

namespace PdfBox.Net.ContentStream;

/// <summary>
/// Execution core for PDF content streams.
/// Manages an operator registry, a graphics-state stack, and text matrices,
/// and drives token-by-token dispatch of parsed content stream operators.
/// </summary>
public class PDFStreamEngine
{
    private readonly Dictionary<string, OperatorProcessor> _operatorsByName = new();
    private readonly Stack<PDGraphicsState> _graphicsStateStack = new();
    private PDGraphicsState _currentGraphicsState = new();
    private Matrix _textMatrix = new();
    private Matrix _textLineMatrix = new();
    private PDPage? _currentPage;

    // ── Operator registry ─────────────────────────────────────────────────────

    /// <summary>
    /// Registers an operator processor; a later registration for the same operator
    /// name replaces the earlier one.
    /// </summary>
    public void AddOperator(OperatorProcessor op)
    {
        if (op != null)
        {
            _operatorsByName[op.Name] = op;
        }
    }

    // ── Page / stream processing ──────────────────────────────────────────────

    /// <summary>
    /// Processes all content streams of the given page.
    /// </summary>
    public virtual void ProcessPage(PDPage page)
    {
        _currentPage = page;
        _currentGraphicsState = new PDGraphicsState();
        _graphicsStateStack.Clear();
        _textMatrix = new Matrix();
        _textLineMatrix = new Matrix();

        if (page.HasContents())
        {
            COSBase? contents = page.GetContents();
            if (contents is COSStream stream)
            {
                ProcessStream(stream.CreateInputStream());
            }
            else if (contents is COSArray array)
            {
                foreach (COSBase? item in array)
                {
                    if (item is COSStream s)
                    {
                        ProcessStream(s.CreateInputStream());
                    }
                }
            }
        }
    }

    /// <summary>
    /// Parses and executes all operators in the given raw content stream.
    /// </summary>
    protected void ProcessStream(Stream contentStream)
    {
        IList<object> tokens = PDFStreamParser.Parse(contentStream);
        var operands = new List<COSBase>();
        foreach (object token in tokens)
        {
            if (token is ContentOperator op)
            {
                // Pass a snapshot so processors and callers can safely retain references.
                ProcessOperator(op, new List<COSBase>(operands));
                operands.Clear();
            }
            else if (token is COSBase cosBase)
            {
                operands.Add(cosBase);
            }
        }
    }

    /// <summary>
    /// Dispatches a single operator to the registered processor (if any).
    /// </summary>
    protected virtual void ProcessOperator(ContentOperator op, IList<COSBase> operands)
    {
        if (_operatorsByName.TryGetValue(op.GetName(), out OperatorProcessor? processor))
        {
            processor.Process(op, operands);
        }
    }

    // ── Graphics-state stack ──────────────────────────────────────────────────

    /// <summary>Pushes a copy of the current graphics state (PDF "q" operator).</summary>
    internal void SaveGraphicsState()
    {
        _graphicsStateStack.Push(_currentGraphicsState);
        _currentGraphicsState = _currentGraphicsState.Clone();
    }

    /// <summary>Pops the most recently saved graphics state (PDF "Q" operator).</summary>
    internal void RestoreGraphicsState()
    {
        if (_graphicsStateStack.Count > 0)
        {
            _currentGraphicsState = _graphicsStateStack.Pop();
        }
    }

    /// <summary>
    /// Returns the depth of the graphics-state save stack (number of pending "q" saves).
    /// </summary>
    protected internal int GraphicsStateStackDepth => _graphicsStateStack.Count;

    /// <summary>
    /// Concatenates <paramref name="m"/> with the current transformation matrix:
    /// CTM' = m × CTM (PDF "cm" operator semantics).
    /// </summary>
    internal void ConcatenateMatrix(Matrix m)
    {
        _currentGraphicsState.SetCurrentTransformationMatrix(
            m.Multiply(_currentGraphicsState.GetCurrentTransformationMatrix()));
    }

    // ── Text-matrix management ────────────────────────────────────────────────

    /// <summary>Sets both text matrix and text line matrix simultaneously.</summary>
    internal void SetTextMatrices(Matrix textMatrix, Matrix textLineMatrix)
    {
        _textMatrix = textMatrix ?? new Matrix();
        _textLineMatrix = textLineMatrix ?? new Matrix();
    }

    /// <summary>Returns the current text line matrix.</summary>
    protected internal Matrix GetTextLineMatrix() => _textLineMatrix;

    /// <summary>Sets the text line matrix.</summary>
    internal void SetTextLineMatrix(Matrix textLineMatrix)
    {
        _textLineMatrix = textLineMatrix ?? new Matrix();
    }

    // ── Glyph rendering ───────────────────────────────────────────────────────

    /// <summary>
    /// Processes each byte of <paramref name="bytes"/> as a single-byte character code,
    /// computing the text rendering matrix and calling <see cref="ShowGlyph"/> for each,
    /// then advancing the text matrix.
    /// </summary>
    internal void ShowStringGlyphs(byte[] bytes)
    {
        if (bytes is null || bytes.Length == 0) return;

        PDTextState textState = _currentGraphicsState.GetTextState();
        float fontSize = textState.GetFontSize();
        float horizontalScaling = textState.GetHorizontalScaling() / 100f;
        float charSpacing = textState.GetCharacterSpacing();
        float wordSpacing = textState.GetWordSpacing();
        float rise = textState.GetRise();
        PDFont? font = textState.GetFont();
        Matrix ctm = _currentGraphicsState.GetCurrentTransformationMatrix();

        foreach (byte b in bytes)
        {
            int code = b & 0xFF;
            float w = font != null ? font.GetWidth(code) / 1000f : 0f;
            float tx = (w * fontSize + charSpacing) * horizontalScaling;
            if (code == 0x20)
            {
                tx += wordSpacing * horizontalScaling;
            }

            Matrix tdScale = new Matrix(fontSize * horizontalScaling, 0, 0, fontSize, 0, rise);
            Matrix textRenderingMatrix = tdScale.Multiply(_textMatrix).Multiply(ctm);

            ShowGlyph(textRenderingMatrix, font!, code, new Vector(tx, 0));

            Matrix advance = Matrix.GetTranslateInstance(tx, 0);
            _textMatrix = advance.Multiply(_textMatrix);
        }
    }

    // ── Protected / internal accessors used by subclasses and operators ───────

    /// <summary>Returns the current graphics state.</summary>
    protected internal PDGraphicsState GetGraphicsState() => _currentGraphicsState;

    /// <summary>Returns the current text matrix.</summary>
    protected internal Matrix GetTextMatrix() => _textMatrix;

    /// <summary>Sets the current text matrix.</summary>
    protected internal void SetTextMatrix(Matrix textMatrix)
    {
        _textMatrix = textMatrix ?? new Matrix();
    }

    /// <summary>Returns the current page being processed.</summary>
    protected internal PDPage? GetCurrentPage() => _currentPage;

    // ── Virtual hooks for subclasses ──────────────────────────────────────────

    public virtual void BeginMarkedContentSequence(COSName tag, COSDictionary? properties)
    {
    }

    public virtual void EndMarkedContentSequence()
    {
    }

    public virtual void MarkedContentPoint(COSName tag, COSDictionary? properties)
    {
    }

    public virtual void XObject(PDXObject xobject)
    {
    }

    protected virtual void ShowGlyph(Matrix textRenderingMatrix, PDFont font, int code, Vector displacement)
    {
    }
}

