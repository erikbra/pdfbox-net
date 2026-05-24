/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 * Rendering hooks now backed by SkiaSharp.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/rendering/PageDrawer.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: adapted
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
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Annotations;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.PDModel.Graphics.OptionalContent;
using PdfBox.Net.PDModel.Graphics.Patterns;
using PdfBox.Net.PDModel.Graphics.State;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.Util;
using PdfBox.Net.Util.Geometry;
using SkiaSharp;

namespace PdfBox.Net.Rendering;

/// <summary>
/// Page drawer backed by SkiaSharp.
/// Path fill and stroke operations are implemented; complex operations
/// (transparency groups, shading, Type-3 glyphs) remain stubs pending
/// future issues.
/// </summary>
public class PageDrawer : PDFGraphicsStreamEngine
{
    private readonly PageDrawerParameters _parameters;
    private AnnotationFilter _annotationFilter = _ => true;
    private Graphics2D? _graphics;
    private readonly GeneralPath _linePath = new();
    private readonly Matrix _initialMatrix = new();
    private Point2D? _currentPoint;

    // Page height in PDF points, used to flip the Y axis when converting
    // from PDF space (Y-up, origin bottom-left) to canvas space (Y-down).
    private float _pageHeightPt;

    public PageDrawer(PageDrawerParameters parameters)
        : base(parameters.GetPage())
    {
        _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    public AnnotationFilter GetAnnotationFilter() => _annotationFilter;

    public void SetAnnotationFilter(AnnotationFilter annotationFilter)
    {
        _annotationFilter = annotationFilter ?? throw new ArgumentNullException(nameof(annotationFilter));
    }

    public PDFRenderer GetRenderer() => _parameters.GetRenderer();

    protected Graphics2D? GetGraphics() => _graphics;

    protected GeneralPath GetLinePath() => _linePath;

    public override Matrix GetInitialMatrix() => _initialMatrix;

    private void SetRenderingHints()
    {
    }

    public void DrawPage(Graphics2D graphics, PDRectangle pageSize)
    {
        _graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
        ArgumentNullException.ThrowIfNull(pageSize);
        _pageHeightPt = pageSize.GetHeight();
        SetRenderingHints();
        ProcessPage(Page);
    }

    internal void DrawTilingPattern(Graphics2D graphics, PDTilingPattern pattern, PDColorSpace? colorSpace, PDColor? color, Matrix patternMatrix)
    {
        // TODO: requires full tiling paint support (issue #22 scope).
    }

    private static float ClampColor(float color)
    {
        return Math.Clamp(color, 0f, 1f);
    }

    protected virtual IPaint GetPaint(PDColor color)
    {
        return Color.Black;
    }

    protected virtual void SetClip()
    {
    }

    protected virtual void TransferClip(Graphics2D graphics)
    {
    }

    public void BeginText()
    {
    }

    public void EndText()
    {
    }

    private void BeginTextClip()
    {
    }

    private void EndTextClip()
    {
    }

    protected virtual void ShowFontGlyph(Matrix textRenderingMatrix, PDFont font, int code, Vector displacement)
    {
        // TODO: glyph rendering (issue scope).
    }

    private void DrawGlyph(GeneralPath path, PDFont font, int code, Vector displacement, AffineTransform at)
    {
        // TODO: glyph outline rendering.
    }

    protected virtual void ShowType3Glyph(Matrix textRenderingMatrix, PDType3Font font, int code, Vector displacement)
    {
        // TODO: Type-3 glyph rendering.
    }

    public void AppendRectangle(Point2D p0, Point2D p1, Point2D p2, Point2D p3)
    {
        _currentPoint = p3;
    }

    private static IPaint ApplySoftMaskToPaint(IPaint parentPaint, PDSoftMask softMask)
    {
        return parentPaint;
    }

    private static BufferedImage AdjustImage(BufferedImage gray)
    {
        return gray;
    }

    private IPaint GetStrokingPaint()
    {
        return Color.Black;
    }

    protected virtual IPaint GetNonStrokingPaint()
    {
        return Color.Black;
    }

    private Stroke GetStroke()
    {
        return new Stroke();
    }

    private static bool IsAllZeroDash(float[] dashArray)
    {
        return dashArray.All(v => v == 0);
    }

    private static float[] GetDashArray(PDLineDashPattern dashPattern)
    {
        return dashPattern.GetDashArray();
    }

    // ── Rendering hooks ───────────────────────────────────────────────────────

    /// <inheritdoc />
    protected override void OnStrokePath(IReadOnlyList<PDFStreamEngine.PathSegment> path, PDGraphicsState graphicsState)
    {
        if (_graphics?.Canvas is null || path.Count == 0) return;
        using SKPath skPath = BuildSkPath(path, graphicsState);
        using SKPaint paint = CreateSkiaPaint(graphicsState, stroke: true);
        _graphics.Canvas.DrawPath(skPath, paint);
    }

    /// <inheritdoc />
    protected override void OnFillPath(int windingRule, IReadOnlyList<PDFStreamEngine.PathSegment> path, PDGraphicsState graphicsState)
    {
        if (_graphics?.Canvas is null || path.Count == 0) return;
        using SKPath skPath = BuildSkPath(path, graphicsState);
        skPath.FillType = windingRule == 0 ? SKPathFillType.EvenOdd : SKPathFillType.Winding;
        using SKPaint paint = CreateSkiaPaint(graphicsState, stroke: false);
        _graphics.Canvas.DrawPath(skPath, paint);
    }

    /// <inheritdoc />
    protected override void OnFillAndStrokePath(int windingRule, IReadOnlyList<PDFStreamEngine.PathSegment> path, PDGraphicsState graphicsState)
    {
        OnFillPath(windingRule, path, graphicsState);
        OnStrokePath(path, graphicsState);
    }

    // ── Path construction and painting (legacy public surface kept for API compat) ──

    public new void StrokePath()
    {
    }

    public new void FillPath(int windingRule)
    {
    }

    private void IntersectShadingBBox(PDColor color, Area area)
    {
    }

    private static bool IsRectangular(GeneralPath path)
    {
        return false;
    }

    public new void FillAndStrokePath(int windingRule)
    {
    }

    public new void Clip(int windingRule)
    {
    }

    public new void MoveTo(float x, float y)
    {
        _currentPoint = new Point2D(x, y);
    }

    public new void LineTo(float x, float y)
    {
        _currentPoint = new Point2D(x, y);
    }

    public new void CurveTo(float x1, float y1, float x2, float y2, float x3, float y3)
    {
        _currentPoint = new Point2D(x3, y3);
    }

    public new Point2D? GetCurrentPoint()
    {
        return _currentPoint;
    }

    public new void ClosePath()
    {
    }

    public new void EndPath()
    {
        _currentPoint = null;
    }

    private static GeneralPath AdjustClip(GeneralPath linePath)
    {
        return linePath;
    }

    public void DrawImage(PDImage pdImage)
    {
        // TODO: image rendering.
    }

    protected virtual int GetSubsampling(PDImage pdImage, AffineTransform at)
    {
        return 1;
    }

    private void DrawBufferedImage(PDImage pdImage, BufferedImage image, AffineTransform at)
    {
        // TODO: buffered image compositing.
    }

    private static BufferedImage ApplyTransferFunction(BufferedImage image, COSBase transfer)
    {
        return image;
    }

    public override void ShadingFill(COSName shadingName)
    {
        // TODO: shading fills (issue #22 scope).
    }

    public void ShowAnnotation(PDAnnotation annotation)
    {
        if (ShouldSkipAnnotation(annotation))
        {
            return;
        }

        // TODO: annotation rendering.
    }

    private bool ShouldSkipAnnotation(PDAnnotation annotation)
    {
        return !_annotationFilter(annotation);
    }

    private static bool HasTransparency(PDFormXObject form)
    {
        return false;
    }

    public void ShowForm(PDFormXObject form)
    {
        // TODO: form XObject rendering.
    }

    public void ShowTransparencyGroup(PDTransparencyGroup form)
    {
        // TODO: transparency group rendering.
    }

    protected virtual void ShowTransparencyGroupOnGraphics(PDTransparencyGroup form, Graphics2D graphics)
    {
        // TODO: transparency group compositing.
    }

    private TransparencyGroup CreateTransparencyGroup(PDTransparencyGroup form, bool isSoftMask, Matrix ctm, PDColor backdropColor)
    {
        return new TransparencyGroup(form, isSoftMask, ctm, backdropColor);
    }

    private static BufferedImage Create2ByteGrayAlphaImage(int width, int height)
    {
        return new BufferedImage(width, height, BufferedImage.TYPE_INT_ARGB);
    }

    private static bool IsGray(PDColorSpace colorSpace)
    {
        return false;
    }

    private static bool HasBlendMode(PDTransparencyGroup group, ISet<COSBase> groupsDone)
    {
        return false;
    }

    public override void BeginMarkedContentSequence(COSName tag, COSDictionary? properties)
    {
    }

    public override void EndMarkedContentSequence()
    {
    }

    private bool IsContentRendered()
    {
        return true;
    }

    private bool IsHiddenOCG(PDPropertyList propertyList)
    {
        return false;
    }

    private bool IsHiddenOCMD(PDOptionalContentMembershipDictionary ocmd)
    {
        return false;
    }

    private bool IsHiddenVisibilityExpression(COSArray veArray)
    {
        return false;
    }

    private bool IsHiddenAndVisibilityExpression(COSArray veArray)
    {
        return false;
    }

    private bool IsHiddenOrVisibilityExpression(COSArray veArray)
    {
        return false;
    }

    private bool IsHiddenNotVisibilityExpression(COSArray veArray)
    {
        return false;
    }

    private static LookupTable GetInvLookupTable()
    {
        return new LookupTable();
    }

    // ── SkiaSharp helpers ─────────────────────────────────────────────────────

    /// <summary>
    /// Builds an <see cref="SKPath"/> from the accumulated path segments,
    /// applying the current transformation matrix and flipping Y so that
    /// PDF (Y-up, bottom-left) maps to canvas (Y-down, top-left).
    /// </summary>
    private SKPath BuildSkPath(IReadOnlyList<PDFStreamEngine.PathSegment> segments, PDGraphicsState graphicsState)
    {
        var skPath = new SKPath();
        Matrix ctm = graphicsState.GetCurrentTransformationMatrix();
        float pageH = _pageHeightPt;

        foreach (PDFStreamEngine.PathSegment seg in segments)
        {
            switch (seg.Type)
            {
                case PDFStreamEngine.PathSegmentType.MoveTo:
                {
                    (float cx, float cy) = PdfToCanvas(seg.X1, seg.Y1, ctm, pageH);
                    skPath.MoveTo(cx, cy);
                    break;
                }
                case PDFStreamEngine.PathSegmentType.LineTo:
                {
                    (float cx, float cy) = PdfToCanvas(seg.X1, seg.Y1, ctm, pageH);
                    skPath.LineTo(cx, cy);
                    break;
                }
                case PDFStreamEngine.PathSegmentType.CurveTo:
                {
                    (float cx1, float cy1) = PdfToCanvas(seg.X1, seg.Y1, ctm, pageH);
                    (float cx2, float cy2) = PdfToCanvas(seg.X2, seg.Y2, ctm, pageH);
                    (float cx3, float cy3) = PdfToCanvas(seg.X3, seg.Y3, ctm, pageH);
                    skPath.CubicTo(cx1, cy1, cx2, cy2, cx3, cy3);
                    break;
                }
                case PDFStreamEngine.PathSegmentType.Close:
                    skPath.Close();
                    break;
            }
        }

        return skPath;
    }

    /// <summary>
    /// Converts a point from PDF user space to SkiaSharp canvas space.
    /// Applies the CTM then flips Y.
    /// </summary>
    private static (float x, float y) PdfToCanvas(float x, float y, Matrix ctm, float pageHeightPt)
    {
        Vector v = ctm.Transform(x, y);
        return (v.GetX(), pageHeightPt - v.GetY());
    }

    /// <summary>Creates a SkiaSharp paint from the current graphics state.</summary>
    private static SKPaint CreateSkiaPaint(PDGraphicsState graphicsState, bool stroke)
    {
        PDColor pdColor = stroke
            ? graphicsState.GetStrokingColor()
            : graphicsState.GetNonStrokingColor();

        int rgb = pdColor.ToRGB();
        byte r = (byte)((rgb >> 16) & 0xFF);
        byte g = (byte)((rgb >> 8) & 0xFF);
        byte b = (byte)(rgb & 0xFF);

        float alpha = stroke
            ? graphicsState.GetAlphaConstant()
            : graphicsState.GetNonStrokeAlphaConstant();
        byte a = (byte)Math.Round(Math.Clamp(alpha, 0f, 1f) * 255f);

        var paint = new SKPaint
        {
            Color = new SKColor(r, g, b, a),
            IsAntialias = true,
            Style = stroke ? SKPaintStyle.Stroke : SKPaintStyle.Fill,
        };

        if (stroke)
        {
            paint.StrokeWidth = Math.Max(0f, graphicsState.GetLineWidth());
            paint.StrokeCap = graphicsState.GetLineCap() switch
            {
                1 => SKStrokeCap.Round,
                2 => SKStrokeCap.Square,
                _ => SKStrokeCap.Butt,
            };
            paint.StrokeJoin = graphicsState.GetLineJoin() switch
            {
                1 => SKStrokeJoin.Round,
                2 => SKStrokeJoin.Bevel,
                _ => SKStrokeJoin.Miter,
            };
            paint.StrokeMiter = graphicsState.GetMiterLimit();
        }

        return paint;
    }

    private sealed class TransparencyGroup
    {
        private readonly BufferedImage _image = new(1, 1, BufferedImage.TYPE_INT_ARGB);
        private readonly PDRectangle _bbox = new();
        private readonly Rectangle2D _bounds = new();

        public TransparencyGroup(PDTransparencyGroup form, bool isSoftMask, Matrix ctm, PDColor backdropColor)
        {
        }

        public Rectangle2D GetBounds() => _bounds;

        public PDRectangle GetBBox() => _bbox;

        public BufferedImage GetImage() => _image;
    }
}
