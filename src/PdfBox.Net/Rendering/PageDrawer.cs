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
using PdfBox.Net.PDModel.DocumentInterchange.MarkedContent;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Graphics;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.PDModel.Graphics.OptionalContent;
using PdfBox.Net.PDModel.Graphics.Patterns;
using PdfBox.Net.PDModel.Graphics.Shading;
using PdfBox.Net.PDModel.Graphics.State;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.Util;
using PdfBox.Net.Util.Geometry;
using SkiaSharp;

namespace PdfBox.Net.Rendering;

/// <summary>
/// Page drawer backed by SkiaSharp.
/// Path fill/stroke, images, text, form XObjects, annotation appearances,
/// and conservative shading/pattern fallbacks are implemented.
/// </summary>
public class PageDrawer : PDFGraphicsStreamEngine
{
    private readonly PageDrawerParameters _parameters;
    private readonly Dictionary<PDVectorFont, GlyphCache> _glyphCaches = new();
    private AnnotationFilter _annotationFilter = _ => true;
    private Graphics2D? _graphics;
    private readonly GeneralPath _linePath = new();
    private readonly Matrix _initialMatrix = new();
    private Point2D? _currentPoint;
    private readonly Stack<bool> _hiddenMarkedContentStack = new();
    private int _nestedHiddenOptionalContentCount;

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
        foreach (PDAnnotation annotation in Page.GetAnnotations())
        {
            ShowAnnotation(annotation);
        }
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
        if (_graphics?.Canvas is null || !IsContentRendered())
        {
            return;
        }

        if (font is PDVectorFont vectorFont)
        {
            GeneralPath path = GetGlyphCache(vectorFont).GetPathForCharacterCode(code);
            if (path.Segments.Count > 0)
            {
                Matrix glyphMatrix = font.GetFontMatrix().Multiply(textRenderingMatrix);
                using SKPath skPath = BuildSkPath(path, glyphMatrix);
                using SKPaint paint = CreateSkiaPaint(GetGraphicsState(), stroke: false);
                _graphics.Canvas.DrawPath(skPath, paint);
                return;
            }
        }

        DrawUnicodeGlyphFallback(textRenderingMatrix, font, code);
    }

    private void DrawGlyph(GeneralPath path, PDFont font, int code, Vector displacement, AffineTransform at)
    {
        if (_graphics?.Canvas is null || path.Segments.Count == 0)
        {
            return;
        }

        Matrix matrix = new(at);
        using SKPath skPath = BuildSkPath(path, matrix);
        using SKPaint paint = CreateSkiaPaint(GetGraphicsState(), stroke: false);
        _graphics.Canvas.DrawPath(skPath, paint);
    }

    protected virtual void ShowType3Glyph(Matrix textRenderingMatrix, PDType3Font font, int code, Vector displacement)
    {
        DrawUnicodeGlyphFallback(textRenderingMatrix, font, code);
    }

    public override void AppendRectangle(Point2D p0, Point2D p1, Point2D p2, Point2D p3)
    {
        base.AppendRectangle(p0, p1, p2, p3);
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

    public override void StrokePath()
    {
        base.StrokePath();
    }

    public override void FillPath(int windingRule)
    {
        base.FillPath(windingRule);
    }

    private void IntersectShadingBBox(PDColor color, Area area)
    {
    }

    private static bool IsRectangular(GeneralPath path)
    {
        return false;
    }

    public override void FillAndStrokePath(int windingRule)
    {
        base.FillAndStrokePath(windingRule);
    }

    public override void Clip(int windingRule)
    {
        base.Clip(windingRule);
    }

    public override void MoveTo(float x, float y)
    {
        base.MoveTo(x, y);
        _currentPoint = new Point2D(x, y);
    }

    public override void LineTo(float x, float y)
    {
        base.LineTo(x, y);
        _currentPoint = new Point2D(x, y);
    }

    public override void CurveTo(float x1, float y1, float x2, float y2, float x3, float y3)
    {
        base.CurveTo(x1, y1, x2, y2, x3, y3);
        _currentPoint = new Point2D(x3, y3);
    }

    public override Point2D? GetCurrentPoint()
    {
        return _currentPoint;
    }

    public override void ClosePath()
    {
        base.ClosePath();
    }

    public override void EndPath()
    {
        base.EndPath();
        _currentPoint = null;
    }

    private static GeneralPath AdjustClip(GeneralPath linePath)
    {
        return linePath;
    }

    public override void DrawImage(PDImage pdImage)
    {
        DrawImage(pdImage, GetGraphicsState().GetCurrentTransformationMatrix());
    }

    protected virtual int GetSubsampling(PDImage pdImage, AffineTransform at)
    {
        return 1;
    }

    private void DrawBufferedImage(PDImage pdImage, BufferedImage image, AffineTransform at)
    {
        if (_graphics?.Canvas is null)
        {
            return;
        }

        Matrix matrix = new(at);
        SKRect dest = GetImageDestination(matrix);
        using SKPaint paint = CreateImagePaint(GetGraphicsState());
        DrawBitmap(image.Bitmap, dest, paint);
    }

    private static BufferedImage ApplyTransferFunction(BufferedImage image, COSBase transfer)
    {
        return image;
    }

    public override void ShadingFill(COSName shadingName)
    {
        if (_graphics?.Canvas is null || !IsContentRendered())
        {
            return;
        }

        PDShading? shading = GetResources()?.GetShading(shadingName);
        if (shading is null)
        {
            return;
        }

        SKRect bounds = GetShadingBounds(shading);
        if (bounds.IsEmpty)
        {
            return;
        }

        using SKPaint paint = CreateShadingPaint(shading, GetGraphicsState());
        _graphics.Canvas.DrawRect(bounds, paint);
    }

    public void ShowAnnotation(PDAnnotation annotation)
    {
        if (ShouldSkipAnnotation(annotation))
        {
            return;
        }

        PDAppearanceStream? appearance = annotation.GetNormalAppearanceStream();
        if (appearance is null)
        {
            annotation.ConstructAppearances();
            appearance = annotation.GetNormalAppearanceStream();
        }

        if (appearance is null)
        {
            return;
        }

        ShowAnnotationAppearance(annotation, appearance);
    }

    private bool ShouldSkipAnnotation(PDAnnotation annotation)
    {
        return !_annotationFilter(annotation) || annotation.IsHidden() || annotation.IsInvisible() || annotation.IsNoView();
    }

    private static bool HasTransparency(PDFormXObject form)
    {
        return form is PDTransparencyGroup || form.GetCOSObject()?.GetCOSDictionary(COSName.GetPDFName("Group")) is not null;
    }

    public void ShowForm(PDFormXObject form)
    {
        if (!IsContentRendered())
        {
            return;
        }

        SaveGraphicsState();
        try
        {
            ConcatenateMatrix(form.GetMatrix());
            base.XObject(form);
        }
        finally
        {
            RestoreGraphicsState();
        }
    }

    private void ShowAnnotationAppearance(PDAnnotation annotation, PDAppearanceStream appearance)
    {
        if (!IsContentRendered())
        {
            return;
        }

        PDRectangle? rectangle = annotation.GetRectangle();
        PDRectangle? bbox = appearance.GetBBox();
        Matrix placement = CreateAnnotationPlacementMatrix(rectangle, bbox);

        SaveGraphicsState();
        try
        {
            ConcatenateMatrix(placement);
            ConcatenateMatrix(appearance.GetMatrix());
            base.XObject(appearance);
        }
        finally
        {
            RestoreGraphicsState();
        }
    }

    private static Matrix CreateAnnotationPlacementMatrix(PDRectangle? rectangle, PDRectangle? bbox)
    {
        if (rectangle is null)
        {
            return new Matrix();
        }

        float bboxWidth = bbox?.GetWidth() ?? rectangle.GetWidth();
        float bboxHeight = bbox?.GetHeight() ?? rectangle.GetHeight();
        float scaleX = bboxWidth == 0 ? 1f : rectangle.GetWidth() / bboxWidth;
        float scaleY = bboxHeight == 0 ? 1f : rectangle.GetHeight() / bboxHeight;
        float translateX = rectangle.GetLowerLeftX() - (bbox?.GetLowerLeftX() ?? 0f) * scaleX;
        float translateY = rectangle.GetLowerLeftY() - (bbox?.GetLowerLeftY() ?? 0f) * scaleY;

        return new Matrix(scaleX, 0, 0, scaleY, translateX, translateY);
    }

    public void ShowTransparencyGroup(PDTransparencyGroup form)
    {
        ShowForm(form);
    }

    public override void XObject(PDXObject xobject)
    {
        if (xobject is PDImageXObject image)
        {
            DrawImageXObject(image);
            return;
        }

        if (xobject is PDTransparencyGroup transparencyGroup)
        {
            ShowTransparencyGroup(transparencyGroup);
            return;
        }

        if (xobject is PDFormXObject form)
        {
            ShowForm(form);
            return;
        }

        base.XObject(xobject);
    }

    protected virtual void ShowTransparencyGroupOnGraphics(PDTransparencyGroup form, Graphics2D graphics)
    {
        ShowTransparencyGroup(form);
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
        bool hidden = properties is not null && IsHiddenOCG(PDPropertyList.Create(properties));
        _hiddenMarkedContentStack.Push(hidden);
        if (hidden)
        {
            _nestedHiddenOptionalContentCount++;
        }
    }

    public override void EndMarkedContentSequence()
    {
        if (_hiddenMarkedContentStack.Count == 0)
        {
            return;
        }

        if (_hiddenMarkedContentStack.Pop() && _nestedHiddenOptionalContentCount > 0)
        {
            _nestedHiddenOptionalContentCount--;
        }
    }

    private bool IsContentRendered()
    {
        return _nestedHiddenOptionalContentCount == 0;
    }

    protected override void ShowGlyph(Matrix textRenderingMatrix, PDFont font, int code, Vector displacement)
    {
        if (font is PDType3Font type3Font)
        {
            ShowType3Glyph(textRenderingMatrix, type3Font, code, displacement);
        }
        else
        {
            ShowFontGlyph(textRenderingMatrix, font, code, displacement);
        }
    }

    private bool IsHiddenOCG(PDPropertyList? propertyList)
    {
        return propertyList switch
        {
            null => false,
            PDOptionalContentGroup group => group.GetRenderState(_parameters.GetDestination()) switch
            {
                PDOptionalContentGroup.RenderState.ON => false,
                PDOptionalContentGroup.RenderState.OFF => true,
                _ => !GetRenderer().IsGroupEnabled(group)
            },
            PDOptionalContentMembershipDictionary ocmd => IsHiddenOCMD(ocmd),
            _ => false
        };
    }

    private bool IsHiddenOCMD(PDOptionalContentMembershipDictionary ocmd)
    {
        COSArray? visibilityExpression = ocmd.GetVisibilityExpression();
        if (visibilityExpression is not null)
        {
            return IsHiddenVisibilityExpression(visibilityExpression);
        }

        IReadOnlyList<PDPropertyList> ocgs = ocmd.GetOCGs();
        COSName visibilityPolicy = ocmd.GetVisibilityPolicy();
        string policy = visibilityPolicy.GetName();

        int hiddenCount = ocgs.Count(IsHiddenOCG);
        return policy switch
        {
            "AllOn" => hiddenCount > 0,
            "AnyOff" => hiddenCount == 0,
            "AllOff" => hiddenCount != ocgs.Count,
            _ => hiddenCount == ocgs.Count
        };
    }

    private bool IsHiddenVisibilityExpression(COSArray veArray)
    {
        return veArray.GetObject(0) switch
        {
            COSName op when op.GetName() == "And" => IsHiddenAndVisibilityExpression(veArray),
            COSName op when op.GetName() == "Or" => IsHiddenOrVisibilityExpression(veArray),
            COSName op when op.GetName() == "Not" => IsHiddenNotVisibilityExpression(veArray),
            _ => false
        };
    }

    private bool IsHiddenAndVisibilityExpression(COSArray veArray)
    {
        for (int i = 1; i < veArray.Size(); i++)
        {
            if (IsVisibilityOperandHidden(veArray.GetObject(i)))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsHiddenOrVisibilityExpression(COSArray veArray)
    {
        for (int i = 1; i < veArray.Size(); i++)
        {
            if (!IsVisibilityOperandHidden(veArray.GetObject(i)))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsHiddenNotVisibilityExpression(COSArray veArray)
    {
        return veArray.Size() < 2 || !IsVisibilityOperandHidden(veArray.GetObject(1));
    }

    private static LookupTable GetInvLookupTable()
    {
        return new LookupTable();
    }

    private bool IsVisibilityOperandHidden(COSBase? operand)
    {
        return operand switch
        {
            COSDictionary dictionary => IsHiddenOCG(PDPropertyList.Create(dictionary)),
            COSArray expression => IsHiddenVisibilityExpression(expression),
            _ => false
        };
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

    private void DrawImage(PDImage image, Matrix matrix)
    {
        DrawDecodedImage(SampledImageReader.GetRGBImage(image), image.GetWidth(), image.GetHeight(), matrix);
    }

    private void DrawImageXObject(PDImageXObject image)
    {
        DrawDecodedImage(
            SampledImageReader.GetRGBImage(image),
            image.GetWidth(),
            image.GetHeight(),
            GetGraphicsState().GetCurrentTransformationMatrix());
    }

    private void DrawDecodedImage(byte[] rgb, int width, int height, Matrix matrix)
    {
        if (_graphics?.Canvas is null || !IsContentRendered())
        {
            return;
        }

        if (width <= 0 || height <= 0 || rgb.Length < width * height * 3)
        {
            return;
        }

        using SKBitmap bitmap = CreateBitmapFromRgb(rgb, width, height);
        SKRect dest = GetImageDestination(matrix);
        if (dest.Width <= 0 || dest.Height <= 0)
        {
            return;
        }

        using SKPaint paint = CreateImagePaint(GetGraphicsState());
        DrawBitmap(bitmap, dest, paint);
    }

    private GlyphCache GetGlyphCache(PDVectorFont font)
    {
        if (!_glyphCaches.TryGetValue(font, out GlyphCache? cache))
        {
            cache = new GlyphCache(font);
            _glyphCaches.Add(font, cache);
        }

        return cache;
    }

    private SKPath BuildSkPath(GeneralPath path, Matrix matrix)
    {
        var skPath = new SKPath();
        foreach (GeneralPath.Segment segment in path.Segments)
        {
            switch (segment.Type)
            {
                case GeneralPath.SegmentType.MoveTo:
                {
                    (float x, float y) = PdfToCanvas(segment.X1, segment.Y1, matrix, _pageHeightPt);
                    skPath.MoveTo(x, y);
                    break;
                }
                case GeneralPath.SegmentType.LineTo:
                {
                    (float x, float y) = PdfToCanvas(segment.X1, segment.Y1, matrix, _pageHeightPt);
                    skPath.LineTo(x, y);
                    break;
                }
                case GeneralPath.SegmentType.QuadTo:
                {
                    (float x1, float y1) = PdfToCanvas(segment.X1, segment.Y1, matrix, _pageHeightPt);
                    (float x2, float y2) = PdfToCanvas(segment.X2, segment.Y2, matrix, _pageHeightPt);
                    skPath.QuadTo(x1, y1, x2, y2);
                    break;
                }
                case GeneralPath.SegmentType.Close:
                    skPath.Close();
                    break;
            }
        }

        return skPath;
    }

    private void DrawUnicodeGlyphFallback(Matrix textRenderingMatrix, PDFont font, int code)
    {
        if (_graphics?.Canvas is null)
        {
            return;
        }

        string? unicode = font.ToUnicode(code, PdfBox.Net.PDModel.Font.Encoding.GlyphList.GetAdobeGlyphList());
        unicode ??= code is >= 0 and <= 255 ? ((char)code).ToString() : null;
        if (string.IsNullOrEmpty(unicode))
        {
            return;
        }

        SKMatrix matrix = ToCanvasMatrix(textRenderingMatrix, _pageHeightPt);
        using SKPaint paint = CreateSkiaPaint(GetGraphicsState(), stroke: false);
        using SKTypeface? typeface = SKTypeface.FromFamilyName(font.GetName());
        using SKFont skFont = new(typeface ?? SKTypeface.Default, 1f);

        _graphics.Canvas.Save();
        _graphics.Canvas.Concat(in matrix);
        _graphics.Canvas.DrawText(unicode, 0, 0, skFont, paint);
        _graphics.Canvas.Restore();
    }

    private static SKMatrix ToCanvasMatrix(Matrix matrix, float pageHeightPt)
    {
        return new SKMatrix
        {
            ScaleX = matrix.GetScaleX(),
            SkewX = matrix.GetShearX(),
            TransX = matrix.GetTranslateX(),
            SkewY = -matrix.GetShearY(),
            ScaleY = -matrix.GetScaleY(),
            TransY = pageHeightPt - matrix.GetTranslateY(),
            Persp2 = 1f,
        };
    }

    private SKRect GetImageDestination(Matrix matrix)
    {
        (float x0, float y0) = PdfToCanvas(0, 0, matrix, _pageHeightPt);
        (float x1, float y1) = PdfToCanvas(1, 0, matrix, _pageHeightPt);
        (float x2, float y2) = PdfToCanvas(1, 1, matrix, _pageHeightPt);
        (float x3, float y3) = PdfToCanvas(0, 1, matrix, _pageHeightPt);

        float left = MathF.Min(MathF.Min(x0, x1), MathF.Min(x2, x3));
        float right = MathF.Max(MathF.Max(x0, x1), MathF.Max(x2, x3));
        float top = MathF.Min(MathF.Min(y0, y1), MathF.Min(y2, y3));
        float bottom = MathF.Max(MathF.Max(y0, y1), MathF.Max(y2, y3));
        return new SKRect(left, top, right, bottom);
    }

    private static SKBitmap CreateBitmapFromRgb(byte[] rgb, int width, int height)
    {
        var bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Opaque);
        int src = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bitmap.SetPixel(x, y, new SKColor(rgb[src], rgb[src + 1], rgb[src + 2]));
                src += 3;
            }
        }

        return bitmap;
    }

    private static SKPaint CreateImagePaint(PDGraphicsState graphicsState)
    {
        float alpha = Math.Clamp(graphicsState.GetNonStrokeAlphaConstant(), 0f, 1f);
        return new SKPaint
        {
            IsAntialias = true,
            Color = SKColors.White.WithAlpha((byte)Math.Round(alpha * 255f)),
        };
    }

    private void DrawBitmap(SKBitmap bitmap, SKRect dest, SKPaint paint)
    {
        using SKImage image = SKImage.FromBitmap(bitmap);
        _graphics!.Canvas!.DrawImage(image, dest, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear), paint);
    }

    /// <summary>Creates a SkiaSharp paint from the current graphics state.</summary>
    private SKPaint CreateSkiaPaint(PDGraphicsState graphicsState, bool stroke)
    {
        PDColor pdColor = stroke
            ? graphicsState.GetStrokingColor()
            : graphicsState.GetNonStrokingColor();

        int rgb = ResolvePaintColor(pdColor, stroke);
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

    private int ResolvePaintColor(PDColor color, bool stroke)
    {
        PDColorSpace? colorSpace = color.GetColorSpace();
        if (colorSpace is not PDPattern patternColorSpace)
        {
            return SafeToRgb(color, 0);
        }

        COSName? patternName = color.GetPatternName();
        PDAbstractPattern? pattern = patternName is null ? null : patternColorSpace.GetResources()?.GetPattern(patternName);
        return pattern switch
        {
            PDShadingPattern shadingPattern when shadingPattern.GetShading() is PDShading shading
                => ResolveShadingColor(shading),
            PDTilingPattern tilingPattern when tilingPattern.GetPaintType() == PDTilingPattern.PAINT_UNCOLORED
                => ResolveUncoloredPatternColor(patternColorSpace, color),
            _ => stroke ? 0x000000 : 0x000000
        };
    }

    private static int ResolveUncoloredPatternColor(PDPattern patternColorSpace, PDColor color)
    {
        PDColorSpace? underlying = patternColorSpace.GetUnderlyingColorSpace();
        if (underlying is null)
        {
            return 0;
        }

        return SafeToRgb(new PDColor(color.GetComponents(), underlying), 0);
    }

    private static int ResolveShadingColor(PDShading shading)
    {
        COSArray? background = shading.GetBackground();
        if (background is not null && background.Size() > 0)
        {
            return SafeToRgb(new PDColor(background, shading.GetColorSpace()), 0);
        }

        try
        {
            float[] color = shading.EvalFunction([0.5f]);
            return SafeToRgb(new PDColor(color, shading.GetColorSpace()), 0);
        }
        catch (Exception ex) when (IsRecoverableRenderingException(ex))
        {
            return 0;
        }
    }

    private static int SafeToRgb(PDColor color, int fallback)
    {
        try
        {
            return color.ToRGB();
        }
        catch (Exception ex) when (IsRecoverableRenderingException(ex))
        {
            return fallback;
        }
    }

    private SKRect GetShadingBounds(PDShading shading)
    {
        PDRectangle? bbox = shading.GetBBox();
        if (bbox is null)
        {
            return new SKRect(0, 0, _parameters.GetPage().GetCropBox().GetWidth(), _parameters.GetPage().GetCropBox().GetHeight());
        }

        Matrix ctm = GetGraphicsState().GetCurrentTransformationMatrix();
        (float x0, float y0) = PdfToCanvas(bbox.GetLowerLeftX(), bbox.GetLowerLeftY(), ctm, _pageHeightPt);
        (float x1, float y1) = PdfToCanvas(bbox.GetUpperRightX(), bbox.GetUpperRightY(), ctm, _pageHeightPt);
        return new SKRect(Math.Min(x0, x1), Math.Min(y0, y1), Math.Max(x0, x1), Math.Max(y0, y1));
    }

    private SKPaint CreateShadingPaint(PDShading shading, PDGraphicsState graphicsState)
    {
        int rgb = ResolveShadingColor(shading);
        byte r = (byte)((rgb >> 16) & 0xFF);
        byte g = (byte)((rgb >> 8) & 0xFF);
        byte b = (byte)(rgb & 0xFF);
        byte a = (byte)Math.Round(Math.Clamp(graphicsState.GetNonStrokeAlphaConstant(), 0f, 1f) * 255f);
        return new SKPaint
        {
            Color = new SKColor(r, g, b, a),
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };
    }

    private static bool IsRecoverableRenderingException(Exception ex)
    {
        return ex is IOException
            or InvalidOperationException
            or NotSupportedException
            or ArgumentException;
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
