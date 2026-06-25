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
    private List<PDFStreamEngine.PathSegment>? _textClippings;
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
        Graphics2D? savedGraphics = _graphics;
        float savedPageHeight = _pageHeightPt;
        _graphics = graphics;
        if (graphics.BitmapHeight is int bitmapHeight && bitmapHeight > 0)
        {
            _pageHeightPt = bitmapHeight;
        }

        try
        {
            SetRenderingHints();
            ProcessTilingPattern(pattern, color, colorSpace, patternMatrix);
        }
        finally
        {
            _graphics = savedGraphics;
            _pageHeightPt = savedPageHeight;
        }
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

    public override void BeginText()
    {
        SetClip();
        BeginTextClip();
    }

    public override void EndText()
    {
        EndTextClip();
    }

    private void BeginTextClip()
    {
        _textClippings = [];
    }

    private void EndTextClip()
    {
        List<PDFStreamEngine.PathSegment>? textClippings = _textClippings;
        _textClippings = null;

        RenderingMode renderingMode = GetGraphicsState().GetTextState().GetRenderingModeInstance();
        if (renderingMode.IsClip() && textClippings is { Count: > 0 })
        {
            GetGraphicsState().IntersectClippingPath(textClippings, new Matrix(), 1);
        }
    }

    protected virtual void ShowFontGlyph(Matrix textRenderingMatrix, PDFont font, int code, Vector displacement)
    {
        if (font is PDVectorFont vectorFont)
        {
            GeneralPath path = GetGlyphCache(vectorFont).GetPathForCharacterCode(code);
            if (path.Segments.Count > 0)
            {
                Matrix glyphMatrix = Matrix.Concatenate(textRenderingMatrix, font.GetFontMatrix());
                BufferTextClipPath(path, glyphMatrix);
                if (_graphics?.Canvas is null || !IsContentRendered())
                {
                    return;
                }

                using SKPath skPath = BuildSkPath(path, glyphMatrix);
                DrawTextPath(skPath);
                return;
            }
        }

        DrawUnicodeGlyphFallback(textRenderingMatrix, font, code);
    }

    private void DrawGlyph(GeneralPath path, PDFont font, int code, Vector displacement, AffineTransform at)
    {
        if (path.Segments.Count == 0)
        {
            return;
        }

        Matrix matrix = new(at);
        BufferTextClipPath(path, matrix);
        if (_graphics?.Canvas is null || !IsContentRendered())
        {
            return;
        }

        using SKPath skPath = BuildSkPath(path, matrix);
        DrawTextPath(skPath);
    }

    protected virtual void ShowType3Glyph(Matrix textRenderingMatrix, PDType3Font font, int code, Vector displacement)
    {
        if (GetGraphicsState().GetTextState().GetRenderingModeInstance() == RenderingMode.NEITHER)
        {
            return;
        }

        DrawUnicodeGlyphFallback(textRenderingMatrix, font, code);
    }

    private void DrawTextPath(SKPath path)
    {
        RenderingMode renderingMode = GetGraphicsState().GetTextState().GetRenderingModeInstance();
        if (!renderingMode.IsFill() && !renderingMode.IsStroke())
        {
            return;
        }

        if (renderingMode.IsFill())
        {
            using SKPaint fillPaint = CreateSkiaPaint(GetGraphicsState(), stroke: false);
            DrawWithCurrentClip(canvas => canvas.DrawPath(path, fillPaint));
        }

        if (renderingMode.IsStroke())
        {
            using SKPaint strokePaint = CreateSkiaPaint(GetGraphicsState(), stroke: true);
            DrawWithCurrentClip(canvas => canvas.DrawPath(path, strokePaint));
        }
    }

    private void BufferTextClipPath(GeneralPath path, Matrix matrix)
    {
        RenderingMode renderingMode = GetGraphicsState().GetTextState().GetRenderingModeInstance();
        if (!renderingMode.IsClip() || _textClippings is not { } textClippings)
        {
            return;
        }

        float currentX = 0;
        float currentY = 0;
        float startX = 0;
        float startY = 0;
        bool hasCurrentPoint = false;

        foreach (GeneralPath.Segment segment in path.Segments)
        {
            switch (segment.Type)
            {
                case GeneralPath.SegmentType.MoveTo:
                {
                    AddTransformedPathSegment(textClippings, PDFStreamEngine.PathSegmentType.MoveTo, matrix, segment.X1, segment.Y1);
                    currentX = startX = segment.X1;
                    currentY = startY = segment.Y1;
                    hasCurrentPoint = true;
                    break;
                }
                case GeneralPath.SegmentType.LineTo:
                {
                    AddTransformedPathSegment(textClippings, PDFStreamEngine.PathSegmentType.LineTo, matrix, segment.X1, segment.Y1);
                    currentX = segment.X1;
                    currentY = segment.Y1;
                    hasCurrentPoint = true;
                    break;
                }
                case GeneralPath.SegmentType.QuadTo:
                {
                    if (!hasCurrentPoint)
                    {
                        AddTransformedPathSegment(textClippings, PDFStreamEngine.PathSegmentType.MoveTo, matrix, segment.X2, segment.Y2);
                        currentX = startX = segment.X2;
                        currentY = startY = segment.Y2;
                        hasCurrentPoint = true;
                        break;
                    }

                    float x1 = currentX + (2f / 3f * (segment.X1 - currentX));
                    float y1 = currentY + (2f / 3f * (segment.Y1 - currentY));
                    float x2 = segment.X2 + (2f / 3f * (segment.X1 - segment.X2));
                    float y2 = segment.Y2 + (2f / 3f * (segment.Y1 - segment.Y2));
                    Vector c1 = matrix.Transform(x1, y1);
                    Vector c2 = matrix.Transform(x2, y2);
                    Vector end = matrix.Transform(segment.X2, segment.Y2);
                    textClippings.Add(new PDFStreamEngine.PathSegment(
                        PDFStreamEngine.PathSegmentType.CurveTo,
                        c1.GetX(),
                        c1.GetY(),
                        c2.GetX(),
                        c2.GetY(),
                        end.GetX(),
                        end.GetY()));
                    currentX = segment.X2;
                    currentY = segment.Y2;
                    hasCurrentPoint = true;
                    break;
                }
                case GeneralPath.SegmentType.Close:
                    textClippings.Add(new PDFStreamEngine.PathSegment(PDFStreamEngine.PathSegmentType.Close, 0, 0, 0, 0, 0, 0));
                    currentX = startX;
                    currentY = startY;
                    hasCurrentPoint = true;
                    break;
            }
        }
    }

    private void BufferTextClipPath(SKPath path, Matrix matrix)
    {
        RenderingMode renderingMode = GetGraphicsState().GetTextState().GetRenderingModeInstance();
        if (!renderingMode.IsClip() || _textClippings is not { } textClippings)
        {
            return;
        }

        using SKPath.RawIterator iterator = path.CreateRawIterator();
        SKPoint[] points = new SKPoint[4];
        SKPathVerb verb;
        while ((verb = iterator.Next(points)) != SKPathVerb.Done)
        {
            switch (verb)
            {
                case SKPathVerb.Move:
                    AddTransformedPathSegment(textClippings, PDFStreamEngine.PathSegmentType.MoveTo, matrix, points[0].X, points[0].Y);
                    break;
                case SKPathVerb.Line:
                    AddTransformedPathSegment(textClippings, PDFStreamEngine.PathSegmentType.LineTo, matrix, points[1].X, points[1].Y);
                    break;
                case SKPathVerb.Quad:
                    AddTransformedQuadSegment(textClippings, matrix, points[0], points[1], points[2]);
                    break;
                case SKPathVerb.Conic:
                    AddTransformedConicSegment(textClippings, matrix, points[0], points[1], points[2], iterator.ConicWeight());
                    break;
                case SKPathVerb.Cubic:
                    AddTransformedCubicSegment(textClippings, matrix, points[1], points[2], points[3]);
                    break;
                case SKPathVerb.Close:
                    textClippings.Add(new PDFStreamEngine.PathSegment(PDFStreamEngine.PathSegmentType.Close, 0, 0, 0, 0, 0, 0));
                    break;
            }
        }
    }

    private static void AddTransformedQuadSegment(
        List<PDFStreamEngine.PathSegment> segments,
        Matrix matrix,
        SKPoint start,
        SKPoint control,
        SKPoint end)
    {
        SKPoint c1 = new(
            start.X + (2f / 3f * (control.X - start.X)),
            start.Y + (2f / 3f * (control.Y - start.Y)));
        SKPoint c2 = new(
            end.X + (2f / 3f * (control.X - end.X)),
            end.Y + (2f / 3f * (control.Y - end.Y)));
        AddTransformedCubicSegment(segments, matrix, c1, c2, end);
    }

    private static void AddTransformedConicSegment(
        List<PDFStreamEngine.PathSegment> segments,
        Matrix matrix,
        SKPoint start,
        SKPoint control,
        SKPoint end,
        float weight)
    {
        const int Pow2 = 2;
        SKPoint[] quads = new SKPoint[1 + (2 * (1 << Pow2))];
        int quadCount = SKPath.ConvertConicToQuads(start, control, end, weight, quads, Pow2);
        if (quadCount <= 0)
        {
            AddTransformedQuadSegment(segments, matrix, start, control, end);
            return;
        }

        for (int i = 0; i < quadCount; i++)
        {
            int index = i * 2;
            AddTransformedQuadSegment(segments, matrix, quads[index], quads[index + 1], quads[index + 2]);
        }
    }

    private static void AddTransformedCubicSegment(
        List<PDFStreamEngine.PathSegment> segments,
        Matrix matrix,
        SKPoint control1,
        SKPoint control2,
        SKPoint end)
    {
        Vector c1 = matrix.Transform(control1.X, control1.Y);
        Vector c2 = matrix.Transform(control2.X, control2.Y);
        Vector transformedEnd = matrix.Transform(end.X, end.Y);
        segments.Add(new PDFStreamEngine.PathSegment(
            PDFStreamEngine.PathSegmentType.CurveTo,
            c1.GetX(),
            c1.GetY(),
            c2.GetX(),
            c2.GetY(),
            transformedEnd.GetX(),
            transformedEnd.GetY()));
    }

    private static void AddTransformedPathSegment(
        List<PDFStreamEngine.PathSegment> segments,
        PDFStreamEngine.PathSegmentType type,
        Matrix matrix,
        float x,
        float y)
    {
        Vector transformed = matrix.Transform(x, y);
        segments.Add(new PDFStreamEngine.PathSegment(type, transformed.GetX(), transformed.GetY(), 0, 0, 0, 0));
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
        DrawWithCurrentClip(canvas => canvas.DrawPath(skPath, paint));
    }

    /// <inheritdoc />
    protected override void OnFillPath(int windingRule, IReadOnlyList<PDFStreamEngine.PathSegment> path, PDGraphicsState graphicsState)
    {
        if (_graphics?.Canvas is null || path.Count == 0) return;
        using SKPath skPath = BuildSkPath(path, graphicsState);
        skPath.FillType = windingRule == 0 ? SKPathFillType.EvenOdd : SKPathFillType.Winding;
        using SKPaint paint = CreateSkiaPaint(graphicsState, stroke: false);
        DrawWithCurrentClip(canvas => canvas.DrawPath(skPath, paint));
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
        DrawWithCurrentClip(canvas => canvas.DrawRect(bounds, paint));
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

        base.XObject(form);
    }

    private void ShowAnnotationAppearance(PDAnnotation annotation, PDAppearanceStream appearance)
    {
        if (!IsContentRendered())
        {
            return;
        }

        PDRectangle? rectangle = annotation.GetRectangle();
        PDRectangle? bbox = appearance.GetBBox();
        Matrix? placement = CreateAnnotationPlacementMatrix(rectangle, bbox, appearance.GetMatrix());
        if (placement is null)
        {
            return;
        }

        SaveGraphicsState();
        try
        {
            GetGraphicsState().SetCurrentTransformationMatrix(placement);
            base.XObject(appearance);
        }
        finally
        {
            RestoreGraphicsState();
        }
    }

    private static Matrix? CreateAnnotationPlacementMatrix(PDRectangle? rectangle, PDRectangle? bbox, Matrix appearanceMatrix)
    {
        if (rectangle is null || bbox is null ||
            rectangle.GetWidth() <= 0 || rectangle.GetHeight() <= 0 ||
            bbox.GetWidth() <= 0 || bbox.GetHeight() <= 0)
        {
            return null;
        }

        (float x, float y, float width, float height) = GetTransformedBounds(bbox, appearanceMatrix);
        if (width <= 0 || height <= 0)
        {
            return null;
        }

        Matrix placement = new(
            rectangle.GetWidth() / width,
            0,
            0,
            rectangle.GetHeight() / height,
            rectangle.GetLowerLeftX(),
            rectangle.GetLowerLeftY());

        return placement.Translate(-x, -y);
    }

    private static (float X, float Y, float Width, float Height) GetTransformedBounds(PDRectangle bbox, Matrix matrix)
    {
        Vector p0 = matrix.TransformPoint(bbox.GetLowerLeftX(), bbox.GetLowerLeftY());
        Vector p1 = matrix.TransformPoint(bbox.GetUpperRightX(), bbox.GetLowerLeftY());
        Vector p2 = matrix.TransformPoint(bbox.GetUpperRightX(), bbox.GetUpperRightY());
        Vector p3 = matrix.TransformPoint(bbox.GetLowerLeftX(), bbox.GetUpperRightY());

        float minX = Math.Min(Math.Min(p0.GetX(), p1.GetX()), Math.Min(p2.GetX(), p3.GetX()));
        float maxX = Math.Max(Math.Max(p0.GetX(), p1.GetX()), Math.Max(p2.GetX(), p3.GetX()));
        float minY = Math.Min(Math.Min(p0.GetY(), p1.GetY()), Math.Min(p2.GetY(), p3.GetY()));
        float maxY = Math.Max(Math.Max(p0.GetY(), p1.GetY()), Math.Max(p2.GetY(), p3.GetY()));
        return (minX, minY, maxX - minX, maxY - minY);
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

    private void DrawWithCurrentClip(Action<SKCanvas> draw)
    {
        if (_graphics?.Canvas is not SKCanvas canvas)
        {
            return;
        }

        canvas.Save();
        try
        {
            ApplyCurrentClip(canvas);
            draw(canvas);
        }
        finally
        {
            canvas.Restore();
        }
    }

    private void ApplyCurrentClip(SKCanvas canvas)
    {
        foreach (PDGraphicsState.ClippingPath clip in GetGraphicsState().GetCurrentClippingPaths())
        {
            if (TryGetClipRect(clip, out SKRect rect))
            {
                canvas.ClipRect(rect, SKClipOperation.Intersect, antialias: true);
                continue;
            }

            using SKPath skPath = BuildClipPath(clip);
            skPath.FillType = clip.WindingRule == 0 ? SKPathFillType.EvenOdd : SKPathFillType.Winding;
            canvas.ClipPath(skPath, SKClipOperation.Intersect, antialias: true);
        }
    }

    private bool TryGetClipRect(PDGraphicsState.ClippingPath clip, out SKRect rect)
    {
        rect = SKRect.Empty;
        List<SKPoint> points = [];
        foreach (PDFStreamEngine.PathSegment segment in clip.Segments)
        {
            switch (segment.Type)
            {
                case PDFStreamEngine.PathSegmentType.MoveTo:
                case PDFStreamEngine.PathSegmentType.LineTo:
                {
                    (float x, float y) = PdfToCanvas(segment.X1, segment.Y1, clip.CurrentTransformationMatrix, _pageHeightPt);
                    points.Add(new SKPoint(x, y));
                    break;
                }
                case PDFStreamEngine.PathSegmentType.Close:
                    break;
                default:
                    return false;
            }
        }

        if (points.Count != 4)
        {
            return false;
        }

        float left = points.Min(p => p.X);
        float right = points.Max(p => p.X);
        float top = points.Min(p => p.Y);
        float bottom = points.Max(p => p.Y);
        if (right <= left || bottom <= top)
        {
            return false;
        }

        foreach (SKPoint point in points)
        {
            bool onXEdge = NearlyEqual(point.X, left) || NearlyEqual(point.X, right);
            bool onYEdge = NearlyEqual(point.Y, top) || NearlyEqual(point.Y, bottom);
            if (!onXEdge || !onYEdge)
            {
                return false;
            }
        }

        rect = new SKRect(left, top, right, bottom);
        return true;
    }

    private static bool NearlyEqual(float a, float b)
    {
        return MathF.Abs(a - b) <= 0.001f;
    }

    private SKPath BuildClipPath(PDGraphicsState.ClippingPath clip)
    {
        var skPath = new SKPath();
        foreach (PDFStreamEngine.PathSegment segment in clip.Segments)
        {
            switch (segment.Type)
            {
                case PDFStreamEngine.PathSegmentType.MoveTo:
                {
                    (float x, float y) = PdfToCanvas(segment.X1, segment.Y1, clip.CurrentTransformationMatrix, _pageHeightPt);
                    skPath.MoveTo(x, y);
                    break;
                }
                case PDFStreamEngine.PathSegmentType.LineTo:
                {
                    (float x, float y) = PdfToCanvas(segment.X1, segment.Y1, clip.CurrentTransformationMatrix, _pageHeightPt);
                    skPath.LineTo(x, y);
                    break;
                }
                case PDFStreamEngine.PathSegmentType.CurveTo:
                {
                    (float x1, float y1) = PdfToCanvas(segment.X1, segment.Y1, clip.CurrentTransformationMatrix, _pageHeightPt);
                    (float x2, float y2) = PdfToCanvas(segment.X2, segment.Y2, clip.CurrentTransformationMatrix, _pageHeightPt);
                    (float x3, float y3) = PdfToCanvas(segment.X3, segment.Y3, clip.CurrentTransformationMatrix, _pageHeightPt);
                    skPath.CubicTo(x1, y1, x2, y2, x3, y3);
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
        string? unicode = font.ToUnicode(code, PdfBox.Net.PDModel.Font.Encoding.GlyphList.GetAdobeGlyphList());
        unicode ??= code is >= 0 and <= 255 ? ((char)code).ToString() : null;
        if (string.IsNullOrEmpty(unicode))
        {
            return;
        }

        RenderingMode renderingMode = GetGraphicsState().GetTextState().GetRenderingModeInstance();
        Matrix fallbackGlyphMatrix = CreateFallbackGlyphMatrix(textRenderingMatrix);
        SKMatrix matrix = ToCanvasMatrix(fallbackGlyphMatrix, _pageHeightPt);
        using SKTypeface? typeface = CreateFallbackTypeface(font);
        // Java2D renders fallback glyphs with grayscale antialiasing and fractional placement.
        using SKFont skFont = new(typeface ?? SKTypeface.Default, GetFallbackFontSize(font))
        {
            Edging = SKFontEdging.Antialias,
            Hinting = SKFontHinting.Normal,
            Subpixel = true,
        };
        if (renderingMode.IsClip())
        {
            using SKPath clipPath = skFont.GetTextPath(unicode, new SKPoint(0, 0));
            BufferTextClipPath(clipPath, fallbackGlyphMatrix);
        }

        if (_graphics?.Canvas is null || !IsContentRendered() || (!renderingMode.IsFill() && !renderingMode.IsStroke()))
        {
            return;
        }

        DrawWithCurrentClip(canvas =>
        {
            canvas.Concat(in matrix);
            if (renderingMode.IsFill())
            {
                using SKPaint fillPaint = CreateSkiaPaint(GetGraphicsState(), stroke: false);
                canvas.DrawText(unicode, 0, 0, skFont, fillPaint);
            }

            if (renderingMode.IsStroke())
            {
                using SKPaint strokePaint = CreateSkiaPaint(GetGraphicsState(), stroke: true);
                canvas.DrawText(unicode, 0, 0, skFont, strokePaint);
            }
        });
    }

    private static Matrix CreateFallbackGlyphMatrix(Matrix textRenderingMatrix)
    {
        return new Matrix(
            textRenderingMatrix.GetScaleX(),
            textRenderingMatrix.GetShearY(),
            -textRenderingMatrix.GetShearX(),
            -textRenderingMatrix.GetScaleY(),
            textRenderingMatrix.GetTranslateX(),
            textRenderingMatrix.GetTranslateY());
    }

    private static float GetFallbackFontSize(PDFont font)
    {
        return font is PDDictionaryFont ? 1.12f : 1f;
    }

    private static SKTypeface? CreateFallbackTypeface(PDFont font)
    {
        string fontName = StripSubsetPrefix(font.GetName());
        PDFontDescriptor? descriptor = font.GetFontDescriptor();
        string family = StripSubsetPrefix(descriptor?.GetFontFamily());
        bool bold = IsBold(fontName, descriptor);
        bool italic = IsItalic(fontName, descriptor);
        SKFontStyleWeight weight = bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
        SKFontStyleSlant slant = italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright;
        SKTypeface? firstAvailable = null;

        foreach (string candidate in GetFallbackFamilies(fontName, family))
        {
            SKTypeface? typeface = SKTypeface.FromFamilyName(candidate, weight, SKFontStyleWidth.Normal, slant);
            if (typeface is null)
            {
                continue;
            }

            if (TypefaceFamilyMatches(typeface, candidate))
            {
                firstAvailable?.Dispose();
                return typeface;
            }

            if (firstAvailable is null)
            {
                firstAvailable = typeface;
            }
            else
            {
                typeface.Dispose();
            }
        }

        return firstAvailable;
    }

    private static bool TypefaceFamilyMatches(SKTypeface typeface, string requestedFamily)
    {
        return string.Equals(
            typeface.FamilyName?.Replace(" ", string.Empty, StringComparison.Ordinal),
            requestedFamily.Replace(" ", string.Empty, StringComparison.Ordinal),
            StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> GetFallbackFamilies(string fontName, string family)
    {
        string normalized = fontName.Replace(" ", string.Empty, StringComparison.Ordinal);
        if (normalized.Contains("Helvetica", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("Arial", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("LiberationSans", StringComparison.OrdinalIgnoreCase))
        {
            yield return "Liberation Sans";
            yield return "Arial";
            yield return "Helvetica";
        }
        else if (normalized.Contains("Times", StringComparison.OrdinalIgnoreCase) ||
                 normalized.Contains("LiberationSerif", StringComparison.OrdinalIgnoreCase))
        {
            yield return "Liberation Serif";
            yield return "Times New Roman";
            yield return "Times";
        }
        else if (normalized.Contains("Courier", StringComparison.OrdinalIgnoreCase) ||
                 normalized.Contains("LiberationMono", StringComparison.OrdinalIgnoreCase))
        {
            yield return "Liberation Mono";
            yield return "Courier New";
            yield return "Courier";
        }

        if (!string.IsNullOrWhiteSpace(family))
        {
            yield return family;
        }

        if (!string.IsNullOrWhiteSpace(fontName))
        {
            yield return fontName;
        }
    }

    private static bool IsBold(string fontName, PDFontDescriptor? descriptor)
    {
        return fontName.Contains("Bold", StringComparison.OrdinalIgnoreCase) ||
               descriptor?.IsForceBold() == true ||
               descriptor?.GetFontWeight() >= 600;
    }

    private static bool IsItalic(string fontName, PDFontDescriptor? descriptor)
    {
        return fontName.Contains("Italic", StringComparison.OrdinalIgnoreCase) ||
               fontName.Contains("Oblique", StringComparison.OrdinalIgnoreCase) ||
               descriptor?.IsItalic() == true ||
               descriptor?.GetItalicAngle() != 0;
    }

    private static string StripSubsetPrefix(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        int plus = name.IndexOf('+', StringComparison.Ordinal);
        return plus >= 0 && plus + 1 < name.Length ? name[(plus + 1)..] : name;
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
        DrawWithCurrentClip(canvas => canvas.DrawImage(image, dest, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear), paint));
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
            paint.StrokeWidth = Math.Max(0.25f, TransformWidth(graphicsState, graphicsState.GetLineWidth()));
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
            ApplyLineDashPattern(paint, graphicsState);
        }

        return paint;
    }

    private static float TransformWidth(PDGraphicsState graphicsState, float width)
    {
        Matrix ctm = graphicsState.GetCurrentTransformationMatrix();
        float x = ctm.GetScaleX() + ctm.GetShearX();
        float y = ctm.GetScaleY() + ctm.GetShearY();
        return width * MathF.Sqrt(((x * x) + (y * y)) * 0.5f);
    }

    private static void ApplyLineDashPattern(SKPaint paint, PDGraphicsState graphicsState)
    {
        PDLineDashPattern dashPattern = graphicsState.GetLineDashPattern();
        float[] dashArray = dashPattern.GetDashArray();
        if (dashArray.Length == 0)
        {
            return;
        }

        if (IsAllZeroDash(dashArray))
        {
            paint.Color = paint.Color.WithAlpha(0);
            return;
        }

        List<float> intervals = [];
        foreach (float dash in dashArray)
        {
            if (!float.IsFinite(dash))
            {
                return;
            }

            float transformed = TransformWidth(graphicsState, dash);
            intervals.Add(Math.Max(transformed, 0.062f));
        }

        if (intervals.Count % 2 == 1)
        {
            intervals.AddRange(intervals);
        }

        float phase = Math.Max(0f, TransformWidth(graphicsState, dashPattern.GetPhaseStart()));
        paint.PathEffect = SKPathEffect.CreateDash(intervals.ToArray(), phase);
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
