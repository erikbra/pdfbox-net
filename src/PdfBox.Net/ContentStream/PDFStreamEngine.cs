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
using PdfBox.Net.ContentStream.Operator.Color;
using PdfBox.Net.ContentStream.Operator.Graphics;
using PdfBox.Net.ContentStream.Operator.MarkedContent;
using PdfBox.Net.ContentStream.Operator.State;
using PdfBox.Net.ContentStream.Operator.Text;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Graphics;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.State;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.PdfParser;
using PdfBox.Net.Rendering;
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
    protected internal enum PathSegmentType
    {
        MoveTo,
        LineTo,
        CurveTo,
        Close
    }

    protected internal readonly record struct PathSegment(
        PathSegmentType Type,
        float X1,
        float Y1,
        float X2,
        float Y2,
        float X3,
        float Y3);

    private readonly Dictionary<string, OperatorProcessor> _operatorsByName = new();
    private readonly Stack<PDGraphicsState> _graphicsStateStack = new();
    private readonly List<PathSegment> _currentPath = [];
    private PDGraphicsState _currentGraphicsState = new();
    private Matrix _textMatrix = new();
    private Matrix _textLineMatrix = new();
    private (float X, float Y)? _currentPoint;
    private int? _pendingClipWindingRule;
    private int _compatibilitySectionDepth;
    private PDPage? _currentPage;
    private PDResources? _resources;

    public PDFStreamEngine()
    {
        RegisterOperators();
    }

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
        _currentPath.Clear();
        _currentPoint = null;
        _pendingClipWindingRule = null;
        _compatibilitySectionDepth = 0;
        _resources = page.GetResources();

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

    internal void SetLineWidth(float width) => _currentGraphicsState.SetLineWidth(width);
    internal void SetLineCap(int lineCap) => _currentGraphicsState.SetLineCap(lineCap);
    internal void SetLineJoin(int lineJoin) => _currentGraphicsState.SetLineJoin(lineJoin);
    internal void SetMiterLimit(float miterLimit) => _currentGraphicsState.SetMiterLimit(miterLimit);
    internal void SetLineDashPattern(float[] dashArray, int phase) => _currentGraphicsState.SetLineDashPattern(new PDLineDashPattern(dashArray, phase));
    internal void SetFlatness(float flatness) => _currentGraphicsState.SetFlatness(flatness);
    internal void SetRenderingIntent(string renderingIntent) => _currentGraphicsState.SetRenderingIntent(renderingIntent);
    internal void SetStrokingColorSpace(PDColorSpace colorSpace) => _currentGraphicsState.SetStrokingColorSpace(colorSpace);
    internal void SetNonStrokingColorSpace(PDColorSpace colorSpace) => _currentGraphicsState.SetNonStrokingColorSpace(colorSpace);
    internal void SetStrokingColor(PDColor color) => _currentGraphicsState.SetStrokingColor(color);
    internal void SetNonStrokingColor(PDColor color) => _currentGraphicsState.SetNonStrokingColor(color);

    // ── Path and clipping state ────────────────────────────────────────────────

    public virtual void MoveTo(float x, float y)
    {
        _currentPath.Add(new PathSegment(PathSegmentType.MoveTo, x, y, 0, 0, 0, 0));
        _currentPoint = (x, y);
    }

    public virtual void LineTo(float x, float y)
    {
        _currentPath.Add(new PathSegment(PathSegmentType.LineTo, x, y, 0, 0, 0, 0));
        _currentPoint = (x, y);
    }

    public virtual void CurveTo(float x1, float y1, float x2, float y2, float x3, float y3)
    {
        _currentPath.Add(new PathSegment(PathSegmentType.CurveTo, x1, y1, x2, y2, x3, y3));
        _currentPoint = (x3, y3);
    }

    public virtual void ClosePath()
    {
        _currentPath.Add(new PathSegment(PathSegmentType.Close, 0, 0, 0, 0, 0, 0));
    }

    public virtual void AppendRectangle(float x, float y, float width, float height)
    {
        MoveTo(x, y);
        LineTo(x + width, y);
        LineTo(x + width, y + height);
        LineTo(x, y + height);
        ClosePath();
    }

    public virtual Point2D? GetCurrentPoint()
    {
        return _currentPoint.HasValue ? new Point2D(_currentPoint.Value.X, _currentPoint.Value.Y) : null;
    }
    protected internal IReadOnlyList<PathSegment> GetCurrentPathSegments() => _currentPath;

    private void ApplyPendingClip()
    {
        if (_pendingClipWindingRule.HasValue)
        {
            _currentGraphicsState.SetClippingWindingRule(_pendingClipWindingRule.Value);
            _pendingClipWindingRule = null;
        }
    }

    public virtual void Clip(int windingRule)
    {
        _pendingClipWindingRule = windingRule;
    }

    public virtual void StrokePath()
    {
        ApplyPendingClip();
        OnStrokePath(GetCurrentPathSegments(), GetGraphicsState());
        _currentPath.Clear();
        _currentPoint = null;
    }

    public virtual void FillPath(int windingRule)
    {
        ApplyPendingClip();
        OnFillPath(windingRule, GetCurrentPathSegments(), GetGraphicsState());
        _currentPath.Clear();
        _currentPoint = null;
    }

    public virtual void FillAndStrokePath(int windingRule)
    {
        ApplyPendingClip();
        OnFillAndStrokePath(windingRule, GetCurrentPathSegments(), GetGraphicsState());
        _currentPath.Clear();
        _currentPoint = null;
    }

    /// <summary>Called when the current path should be stroked. Override to provide rendering.</summary>
    protected virtual void OnStrokePath(IReadOnlyList<PathSegment> path, PDGraphicsState graphicsState)
    {
    }

    /// <summary>Called when the current path should be filled. Override to provide rendering.</summary>
    protected virtual void OnFillPath(int windingRule, IReadOnlyList<PathSegment> path, PDGraphicsState graphicsState)
    {
    }

    /// <summary>Called when the current path should be filled then stroked. Override to provide rendering.</summary>
    protected virtual void OnFillAndStrokePath(int windingRule, IReadOnlyList<PathSegment> path, PDGraphicsState graphicsState)
    {
    }

    public virtual void EndPath()
    {
        ApplyPendingClip();
        _currentPath.Clear();
        _currentPoint = null;
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

    protected internal bool IsInCompatibilitySection() => _compatibilitySectionDepth > 0;

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

    /// <summary>Returns the currently active resource dictionary.</summary>
    protected internal PDResources? GetResources() => _resources;

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
        if (xobject is PDFormXObject form)
        {
            PDResources? previousResources = _resources;
            _resources = form.GetResources() ?? previousResources;
            try
            {
                using Stream content = form.GetContents();
                ProcessStream(content);
            }
            finally
            {
                _resources = previousResources;
            }
        }
    }

    public virtual void BeginInlineImage()
    {
    }

    public virtual void BeginInlineImageData()
    {
    }

    public virtual void EndInlineImage()
    {
    }

    public virtual void ShadingFill(COSName shadingName)
    {
    }

    public virtual void SetType3GlyphWidth(float wx, float wy)
    {
    }

    public virtual void SetType3GlyphWidthAndBoundingBox(float wx, float wy, float llx, float lly, float urx, float ury)
    {
    }

    public virtual void BeginCompatibilitySection()
    {
        _compatibilitySectionDepth++;
    }

    public virtual void EndCompatibilitySection()
    {
        if (_compatibilitySectionDepth > 0)
        {
            _compatibilitySectionDepth--;
        }
    }

    protected virtual void ShowGlyph(Matrix textRenderingMatrix, PDFont font, int code, Vector displacement)
    {
    }

    private void RegisterOperators()
    {
        // Existing operator families.
        AddOperator(new PdfBox.Net.ContentStream.Operator.DrawObject(this));
        AddOperator(new BeginMarkedContentSequence(this));
        AddOperator(new BeginMarkedContentSequenceWithProperties(this));
        AddOperator(new EndMarkedContentSequence(this));
        AddOperator(new MarkedContentPoint(this));
        AddOperator(new MarkedContentPointWithProperties(this));
        AddOperator(new Concatenate(this));
        AddOperator(new Restore(this));
        AddOperator(new Save(this));
        AddOperator(new SetGraphicsStateParameters(this));
        AddOperator(new SetMatrix(this));
        AddOperator(new BeginText(this));
        AddOperator(new EndText(this));
        AddOperator(new MoveText(this));
        AddOperator(new MoveTextSetLeading(this));
        AddOperator(new NextLine(this));
        AddOperator(new SetCharSpacing(this));
        AddOperator(new SetFontAndSize(this));
        AddOperator(new SetTextHorizontalScaling(this));
        AddOperator(new SetTextLeading(this));
        AddOperator(new SetTextRenderingMode(this));
        AddOperator(new SetTextRise(this));
        AddOperator(new SetWordSpacing(this));
        AddOperator(new ShowText(this));
        AddOperator(new ShowTextAdjusted(this));
        AddOperator(new ShowTextLine(this));
        AddOperator(new ShowTextLineAndSpace(this));

        // State operators.
        AddOperator(new SetLineWidth(this));
        AddOperator(new SetLineCap(this));
        AddOperator(new SetLineJoin(this));
        AddOperator(new SetMiterLimit(this));
        AddOperator(new SetLineDashPattern(this));
        AddOperator(new SetFlatness(this));
        AddOperator(new SetRenderingIntent(this));

        // Path construction and painting.
        AddOperator(new PdfBox.Net.ContentStream.Operator.Graphics.MoveTo(this));
        AddOperator(new PdfBox.Net.ContentStream.Operator.Graphics.LineTo(this));
        AddOperator(new PdfBox.Net.ContentStream.Operator.Graphics.CurveTo(this));
        AddOperator(new PdfBox.Net.ContentStream.Operator.Graphics.CurveToReplicateFinalPoint(this));
        AddOperator(new PdfBox.Net.ContentStream.Operator.Graphics.CurveToReplicateInitialPoint(this));
        AddOperator(new PdfBox.Net.ContentStream.Operator.Graphics.AppendRectangleToPath(this));
        AddOperator(new PdfBox.Net.ContentStream.Operator.Graphics.ClosePath(this));
        AddOperator(new PdfBox.Net.ContentStream.Operator.Graphics.StrokePath(this));
        AddOperator(new PdfBox.Net.ContentStream.Operator.Graphics.CloseAndStrokePath(this));
        AddOperator(new PdfBox.Net.ContentStream.Operator.Graphics.CloseAndFillNonZeroAndStrokePath(this));
        AddOperator(new PdfBox.Net.ContentStream.Operator.Graphics.CloseAndFillEvenOddAndStrokePath(this));
        AddOperator(new PdfBox.Net.ContentStream.Operator.Graphics.FillNonZeroRule(this));
        AddOperator(new PdfBox.Net.ContentStream.Operator.Graphics.FillNonZeroRule(this, OperatorName.LEGACY_FILL_NON_ZERO));
        AddOperator(new PdfBox.Net.ContentStream.Operator.Graphics.FillEvenOddRule(this));
        AddOperator(new PdfBox.Net.ContentStream.Operator.Graphics.FillNonZeroAndStrokePath(this));
        AddOperator(new PdfBox.Net.ContentStream.Operator.Graphics.FillEvenOddAndStrokePath(this));
        AddOperator(new PdfBox.Net.ContentStream.Operator.Graphics.ClipNonZeroRule(this));
        AddOperator(new PdfBox.Net.ContentStream.Operator.Graphics.ClipEvenOddRule(this));
        AddOperator(new PdfBox.Net.ContentStream.Operator.Graphics.EndPath(this));

        // Color operators.
        AddOperator(new SetNonStrokingColor(this));
        AddOperator(new SetNonStrokingColorN(this));
        AddOperator(new SetNonStrokingColorSpace(this));
        AddOperator(new SetNonStrokingDeviceCMYKColor(this));
        AddOperator(new SetNonStrokingDeviceGrayColor(this));
        AddOperator(new SetNonStrokingDeviceRGBColor(this));
        AddOperator(new SetStrokingColor(this));
        AddOperator(new SetStrokingColorN(this));
        AddOperator(new SetStrokingColorSpace(this));
        AddOperator(new SetStrokingDeviceCMYKColor(this));
        AddOperator(new SetStrokingDeviceGrayColor(this));
        AddOperator(new SetStrokingDeviceRGBColor(this));

        // Inline images, shading, type3 and compatibility.
        AddOperator(new PdfBox.Net.ContentStream.Operator.Graphics.BeginInlineImage(this));
        AddOperator(new PdfBox.Net.ContentStream.Operator.Graphics.BeginInlineImageData(this));
        AddOperator(new PdfBox.Net.ContentStream.Operator.Graphics.EndInlineImage(this));
        AddOperator(new PdfBox.Net.ContentStream.Operator.Graphics.ShadingFill(this));
        AddOperator(new PdfBox.Net.ContentStream.Operator.Graphics.SetType3GlyphWidth(this));
        AddOperator(new PdfBox.Net.ContentStream.Operator.Graphics.SetType3GlyphWidthAndBoundingBox(this));
        AddOperator(new PdfBox.Net.ContentStream.Operator.Graphics.BeginCompatibilitySection(this));
        AddOperator(new PdfBox.Net.ContentStream.Operator.Graphics.EndCompatibilitySection(this));
    }
}
