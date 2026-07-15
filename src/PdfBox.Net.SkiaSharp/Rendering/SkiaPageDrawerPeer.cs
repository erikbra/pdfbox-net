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

using System.Diagnostics;
using System.Globalization;
using PdfBox.Net.ContentStream;
using PdfBox.Net.COS;
using PdfBox.Net.FontBox.TTF;
using PdfBox.Net.PDModel.Annotations;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Common.Function;
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
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Util;
using PdfBox.Net.Util.Geometry;
using SkiaSharp;

namespace PdfBox.Net.Rendering;

/// <summary>
/// Page drawer backed by SkiaSharp.
/// Path fill/stroke, images, text, form XObjects, annotation appearances,
/// and conservative shading/pattern fallbacks are implemented.
/// </summary>
internal class SkiaPageDrawerPeer : PDFGraphicsStreamEngine, IPageDrawerPeer
{
    private readonly PageDrawer _owner;
    private readonly PageDrawerParameters _parameters;
    private readonly Dictionary<PDVectorFont, GlyphCache> _glyphCaches = new();
    private readonly Dictionary<COSDictionary, PDTrueTypeFont?> _mappedTrueTypeFonts = new();
    private readonly Dictionary<FallbackTypefaceKey, SKTypeface?> _fallbackTypefaces = new();
    private readonly Dictionary<COSStream, bool> _blendModeCache = new();
    private readonly Dictionary<COSStream, bool> _iccSourceColorSpaceCache = new();
    private readonly TilingPaintFactory _tilingPaintFactory;
    private AnnotationFilter _annotationFilter = _ => true;
    private Graphics2D? _graphics;
    private AffineTransform _xform = new();
    private readonly GeneralPath _linePath = new();
    private Point2D? _currentPoint;
    private List<PDFStreamEngine.PathSegment>? _textClippings;
    private readonly Stack<bool> _hiddenMarkedContentStack = new();
    private int _nestedHiddenOptionalContentCount;
    private const float GlyphWidthTolerance = 0.0001f;
    private const string ForceUnicodeFallbackEnvironmentVariable = "PDFBOX_NET_RENDER_FORCE_UNICODE_FALLBACK";
    private const string FallbackDiagnosticsEnvironmentVariable = "PDFBOX_NET_RENDER_FALLBACK_DIAGNOSTICS";
    private static readonly COSName FontDescriptorKey = COSName.GetPDFName("FontDescriptor");
    private static readonly COSName FontFile2Key = COSName.GetPDFName("FontFile2");
    private static readonly COSName FontNameKey = COSName.GetPDFName("FontName");
    private static readonly COSName FontSubtypeKey = COSName.GetPDFName("Subtype");
    private static readonly bool ForceUnicodeFallback = IsEnvironmentFlagEnabled(ForceUnicodeFallbackEnvironmentVariable);
    private static readonly bool FallbackDiagnosticsEnabled = IsEnvironmentFlagEnabled(FallbackDiagnosticsEnvironmentVariable);
    private long _unicodeFallbackGlyphCount;
    private long _fallbackTypefaceLookupCount;
    private long _fallbackTypefaceLookupTicks;
    private long _fallbackTypefaceCacheHits;
    private long _fallbackTypefaceCacheMisses;
    private ITransparencyGroupCompositor? _transparencyGroupCompositor;
    private bool _disposed;

    private readonly record struct FallbackTypefaceKey(string FontName, string Family, bool Bold, bool Italic);

    // Page crop box in PDF points, used to translate visible content and flip
    // from PDF space (Y-up, origin bottom-left) to canvas space (Y-down).
    private float _pageHeightPt;
    private float _pageLowerLeftXPt;
    private float _pageLowerLeftYPt;

    internal SkiaPageDrawerPeer(PageDrawer owner, PageDrawerParameters parameters)
        : base(parameters.GetPage())
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        _tilingPaintFactory = new TilingPaintFactory(this);
    }

    public AnnotationFilter GetAnnotationFilter() => _annotationFilter;

    public void SetAnnotationFilter(AnnotationFilter annotationFilter)
    {
        _annotationFilter = annotationFilter ?? throw new ArgumentNullException(nameof(annotationFilter));
    }

    public PDFRenderer GetRenderer() => _parameters.GetRenderer();

    protected Graphics2D? GetGraphics() => _graphics;

    protected GeneralPath GetLinePath() => _linePath;

    public override Matrix GetInitialMatrix() => base.GetInitialMatrix();

    private void SetRenderingHints()
    {
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (FallbackDiagnosticsEnabled && (_unicodeFallbackGlyphCount > 0 || _fallbackTypefaceLookupCount > 0))
        {
            double lookupMs = _fallbackTypefaceLookupTicks * 1000d / Stopwatch.Frequency;
            string lookupMsText = lookupMs.ToString("F3", CultureInfo.InvariantCulture);
            Console.Error.WriteLine(
                $"pdfbox.net skia fallback diagnostics: glyphs={_unicodeFallbackGlyphCount}; " +
                $"typefaceLookups={_fallbackTypefaceLookupCount}; " +
                $"typefaceCacheHits={_fallbackTypefaceCacheHits}; " +
                $"typefaceCacheMisses={_fallbackTypefaceCacheMisses}; " +
                $"typefaceLookupMs={lookupMsText}");
        }

        foreach (SKTypeface? typeface in _fallbackTypefaces.Values)
        {
            typeface?.Dispose();
        }

        _fallbackTypefaces.Clear();
    }

    public void DrawPage(Graphics2D graphics, PDRectangle pageSize)
    {
        _graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
        _xform = graphics.GetTransform();
        ArgumentNullException.ThrowIfNull(pageSize);
        _pageHeightPt = pageSize.GetHeight();
        _pageLowerLeftXPt = pageSize.GetLowerLeftX();
        _pageLowerLeftYPt = pageSize.GetLowerLeftY();
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
        float savedPageLowerLeftX = _pageLowerLeftXPt;
        float savedPageLowerLeftY = _pageLowerLeftYPt;
        _graphics = graphics;
        if (graphics.BitmapHeight is int bitmapHeight && bitmapHeight > 0)
        {
            _pageHeightPt = bitmapHeight;
        }
        _pageLowerLeftXPt = 0;
        _pageLowerLeftYPt = 0;

        try
        {
            SetRenderingHints();
            ProcessTilingPattern(pattern, color, colorSpace, patternMatrix);
        }
        finally
        {
            _graphics = savedGraphics;
            _pageHeightPt = savedPageHeight;
            _pageLowerLeftXPt = savedPageLowerLeftX;
            _pageLowerLeftYPt = savedPageLowerLeftY;
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
        if (ForceUnicodeFallback)
        {
            DrawUnicodeGlyphFallback(textRenderingMatrix, font, code, displacement);
            return;
        }

        if (font is PDVectorFont vectorFont)
        {
            GeneralPath path = GetGlyphCache(vectorFont).GetPathForCharacterCode(code);
            if (path.Segments.Count > 0)
            {
                Matrix glyphMatrix = Matrix.Concatenate(textRenderingMatrix, font.GetFontMatrix());
                float stretch = CalculateVectorGlyphStretch(font, code, displacement);
                if (stretch != 1f)
                {
                    glyphMatrix = ApplyGlyphStretch(glyphMatrix, stretch);
                }

                BufferTextClipPath(path, glyphMatrix);
                if (_graphics?.GetSkiaCanvas() is null || !IsContentRendered())
                {
                    return;
                }

                using SKPath skPath = BuildSkPath(path, glyphMatrix);
                DrawTextPath(skPath);
                return;
            }
        }

        if (!DrawMappedTrueTypeGlyph(textRenderingMatrix, font, code, displacement))
        {
            DrawUnicodeGlyphFallback(textRenderingMatrix, font, code, displacement);
        }
    }

    private void DrawGlyph(GeneralPath path, PDFont font, int code, Vector displacement, AffineTransform at)
    {
        if (path.Segments.Count == 0)
        {
            return;
        }

        Matrix matrix = new(at);
        BufferTextClipPath(path, matrix);
        if (_graphics?.GetSkiaCanvas() is null || !IsContentRendered())
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

        DrawUnicodeGlyphFallback(textRenderingMatrix, font, code, displacement);
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
            using SKPaint fillShapePaint = CreateShapePaint(fillPaint);
            DrawWithCurrentClip(
                canvas => canvas.DrawPath(path, fillPaint),
                canvas => canvas.DrawPath(path, fillShapePaint),
                GetGraphicsState().GetNonStrokingColor());
        }

        if (renderingMode.IsStroke())
        {
            using SKPaint strokePaint = CreateSkiaPaint(GetGraphicsState(), stroke: true);
            using SKPaint strokeShapePaint = CreateShapePaint(strokePaint);
            DrawWithCurrentClip(
                canvas => canvas.DrawPath(path, strokePaint),
                canvas => canvas.DrawPath(path, strokeShapePaint),
                GetGraphicsState().GetStrokingColor());
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
                case GeneralPath.SegmentType.CurveTo:
                {
                    if (!hasCurrentPoint)
                    {
                        AddTransformedPathSegment(textClippings, PDFStreamEngine.PathSegmentType.MoveTo, matrix, segment.X3, segment.Y3);
                        currentX = startX = segment.X3;
                        currentY = startY = segment.Y3;
                        hasCurrentPoint = true;
                        break;
                    }

                    AddTransformedCubicSegment(
                        textClippings,
                        matrix,
                        new SKPoint(segment.X1, segment.Y1),
                        new SKPoint(segment.X2, segment.Y2),
                        new SKPoint(segment.X3, segment.Y3));
                    currentX = segment.X3;
                    currentY = segment.Y3;
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

    private BufferedImage AdjustImage(BufferedImage gray)
    {
        // The Skia group bitmap is already rendered in device orientation. Keep the Java-shaped
        // hook for Paint consumers, but no second rotation/scaling pass is necessary here.
        return gray;
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
        if (_graphics?.GetSkiaCanvas() is null || path.Count == 0) return;
        using SKPath skPath = BuildSkPath(path, graphicsState);
        using SKPaint paint = CreateSkiaPaint(graphicsState, stroke: true);
        using SKPaint shapePaint = CreateShapePaint(paint);
        DrawWithCurrentClip(
            canvas => canvas.DrawPath(skPath, paint),
            canvas => canvas.DrawPath(skPath, shapePaint),
            graphicsState.GetStrokingColor());
    }

    /// <inheritdoc />
    protected override void OnFillPath(int windingRule, IReadOnlyList<PDFStreamEngine.PathSegment> path, PDGraphicsState graphicsState)
    {
        if (_graphics?.GetSkiaCanvas() is null || path.Count == 0) return;
        using SKPath skPath = BuildSkPath(path, graphicsState);
        skPath.FillType = windingRule == 0 ? SKPathFillType.EvenOdd : SKPathFillType.Winding;
        using SKPaint paint = CreateSkiaPaint(graphicsState, stroke: false);
        using SKPaint shapePaint = CreateShapePaint(paint);
        DrawWithCurrentClip(
            canvas => canvas.DrawPath(skPath, paint),
            canvas => canvas.DrawPath(skPath, shapePaint),
            graphicsState.GetNonStrokingColor());
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
        if (_graphics?.GetSkiaCanvas() is null)
        {
            return;
        }

        Matrix matrix = new(at);
        using SKPaint paint = CreateImagePaint(GetGraphicsState());
        DrawBitmap(image.GetSkiaBitmap(), matrix, paint);
    }

    private static BufferedImage ApplyTransferFunction(BufferedImage image, COSBase transfer)
    {
        return image;
    }

    public override void ShadingFill(COSName shadingName)
    {
        if (_graphics?.GetSkiaCanvas() is null || !IsContentRendered())
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
        using SKPaint shapePaint = CreateShapePaint(paint);
        DrawWithCurrentClip(
            canvas => canvas.DrawRect(bounds, paint),
            canvas => canvas.DrawRect(bounds, shapePaint));
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
        ShowTransparencyGroupOnGraphics(form, _graphics!);
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
        if (!IsContentRendered() || graphics?.GetSkiaCanvas() is not SKCanvas canvas)
        {
            return;
        }

        Graphics2D? savedGraphics = _graphics;
        _graphics = graphics;
        try
        {
            PDGraphicsState graphicsState = GetGraphicsState();
            using TransparencyGroup group = CreateTransparencyGroup(
                form,
                false,
                graphicsState.GetCurrentTransformationMatrix(),
                null);
            if (group.GetImage() is not BufferedImage image)
            {
                return;
            }

            float groupOpacity = Math.Clamp(graphicsState.GetNonStrokeAlphaConstant(), 0f, 1f);
            using SKPaint paint = new()
            {
                IsAntialias = true,
                Color = SKColors.White.WithAlpha((byte)Math.Round(groupOpacity * 255f)),
                BlendMode = ToSkiaBlendMode(graphicsState.GetBlendMode()),
            };
            using SKPaint sourcePaint = paint.Clone();
            sourcePaint.BlendMode = SKBlendMode.SrcOver;
            using SKPaint shapePaint = CreateShapePaint(paint);
            ComponentSource? componentSource = group.GetComponentSource();
            DrawWithCurrentClip(targetCanvas =>
            {
                targetCanvas.ResetMatrix();
                targetCanvas.DrawBitmap(
                    image.GetSkiaBitmap(),
                    0,
                    0,
                    SkiaRenderingBackend.ImageSamplingOptions,
                    paint);
            }, targetCanvas =>
            {
                targetCanvas.ResetMatrix();
                targetCanvas.DrawBitmap(
                    image.GetSkiaBitmap(),
                    0,
                    0,
                    SkiaRenderingBackend.ImageSamplingOptions,
                    shapePaint);
            }, drawSource: targetCanvas =>
            {
                targetCanvas.ResetMatrix();
                targetCanvas.DrawBitmap(
                    image.GetSkiaBitmap(),
                    0,
                    0,
                    SkiaRenderingBackend.ImageSamplingOptions,
                    sourcePaint);
            }, componentSource: componentSource);
        }
        finally
        {
            _graphics = savedGraphics;
        }
    }

    int IPageDrawerPeer.GetSubsampling(PDImage pdImage, AffineTransform at)
    {
        return GetSubsampling(pdImage, at);
    }

    void IPageDrawerPeer.ShowTransparencyGroupOnGraphics(PDTransparencyGroup form, Graphics2D graphics)
    {
        ShowTransparencyGroupOnGraphics(form, graphics);
    }

    private TransparencyGroup CreateTransparencyGroup(PDTransparencyGroup form, bool isSoftMask, Matrix ctm, PDColor? backdropColor)
    {
        return new TransparencyGroup(this, form, isSoftMask, ctm, backdropColor);
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
        _owner.InvokeShowGlyphHook(textRenderingMatrix, font, code, displacement);

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

    private void DrawWithCurrentClip(
        Action<SKCanvas> draw,
        Action<SKCanvas>? drawShape = null,
        PDColor? sourceColor = null,
        Action<SKCanvas>? drawSource = null,
        ComponentSource? componentSource = null)
    {
        if (_graphics?.GetSkiaCanvas() is not SKCanvas canvas)
        {
            return;
        }

        if (_transparencyGroupCompositor is not null)
        {
            _transparencyGroupCompositor.Draw(
                canvas,
                canvas.TotalMatrix,
                target => DrawWithCurrentClipCore(target, draw),
                target => DrawShapeWithCurrentClipCore(target, drawShape ?? draw),
                target => DrawWithCurrentClipCore(target, drawSource ?? draw),
                sourceColor,
                componentSource,
                GetGraphicsState().GetBlendMode());
            return;
        }

        DrawWithCurrentClipCore(canvas, draw);
    }

    private void DrawShapeWithCurrentClipCore(SKCanvas canvas, Action<SKCanvas> draw)
    {
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

    private void DrawWithCurrentClipCore(SKCanvas canvas, Action<SKCanvas> draw)
    {
        canvas.Save();
        try
        {
            ApplyCurrentClip(canvas);
            PDSoftMask? softMask = GetGraphicsState().GetSoftMask();
            if (softMask is null)
            {
                draw(canvas);
                return;
            }

            using SoftMaskData? mask = CreateSoftMaskData(softMask);
            if (mask is null)
            {
                draw(canvas);
                return;
            }

            canvas.SaveLayer();
            try
            {
                draw(canvas);
                ApplySoftMaskToCurrentLayer(canvas, mask);
            }
            finally
            {
                canvas.Restore();
            }
        }
        finally
        {
            canvas.Restore();
        }
    }

    private static void ApplySoftMaskToCurrentLayer(SKCanvas canvas, SoftMaskData? mask)
    {
        if (mask is null)
        {
            return;
        }

        canvas.Save();
        try
        {
            canvas.ResetMatrix();
            using SKPaint maskPaint = new() { BlendMode = SKBlendMode.DstIn, IsAntialias = true };
            canvas.DrawBitmap(
                mask.Image.GetSkiaBitmap(),
                0,
                0,
                SkiaRenderingBackend.ImageSamplingOptions,
                maskPaint);
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
                    (float x, float y) = PdfToCanvas(segment.X1, segment.Y1, clip.CurrentTransformationMatrix);
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
        using var builder = new SKPathBuilder();
        foreach (PDFStreamEngine.PathSegment segment in clip.Segments)
        {
            switch (segment.Type)
            {
                case PDFStreamEngine.PathSegmentType.MoveTo:
                {
                    (float x, float y) = PdfToCanvas(segment.X1, segment.Y1, clip.CurrentTransformationMatrix);
                    builder.MoveTo(x, y);
                    break;
                }
                case PDFStreamEngine.PathSegmentType.LineTo:
                {
                    (float x, float y) = PdfToCanvas(segment.X1, segment.Y1, clip.CurrentTransformationMatrix);
                    builder.LineTo(x, y);
                    break;
                }
                case PDFStreamEngine.PathSegmentType.CurveTo:
                {
                    (float x1, float y1) = PdfToCanvas(segment.X1, segment.Y1, clip.CurrentTransformationMatrix);
                    (float x2, float y2) = PdfToCanvas(segment.X2, segment.Y2, clip.CurrentTransformationMatrix);
                    (float x3, float y3) = PdfToCanvas(segment.X3, segment.Y3, clip.CurrentTransformationMatrix);
                    builder.CubicTo(x1, y1, x2, y2, x3, y3);
                    break;
                }
                case PDFStreamEngine.PathSegmentType.Close:
                    builder.Close();
                    break;
            }
        }

        return builder.Detach();
    }

    /// <summary>
    /// Builds an <see cref="SKPath"/> from the accumulated path segments,
    /// applying the current transformation matrix and flipping Y so that
    /// PDF (Y-up, bottom-left) maps to canvas (Y-down, top-left).
    /// </summary>
    private SKPath BuildSkPath(IReadOnlyList<PDFStreamEngine.PathSegment> segments, PDGraphicsState graphicsState)
    {
        using var builder = new SKPathBuilder();
        Matrix ctm = graphicsState.GetCurrentTransformationMatrix();

        foreach (PDFStreamEngine.PathSegment seg in segments)
        {
            switch (seg.Type)
            {
                case PDFStreamEngine.PathSegmentType.MoveTo:
                {
                    (float cx, float cy) = PdfToCanvas(seg.X1, seg.Y1, ctm);
                    builder.MoveTo(cx, cy);
                    break;
                }
                case PDFStreamEngine.PathSegmentType.LineTo:
                {
                    (float cx, float cy) = PdfToCanvas(seg.X1, seg.Y1, ctm);
                    builder.LineTo(cx, cy);
                    break;
                }
                case PDFStreamEngine.PathSegmentType.CurveTo:
                {
                    (float cx1, float cy1) = PdfToCanvas(seg.X1, seg.Y1, ctm);
                    (float cx2, float cy2) = PdfToCanvas(seg.X2, seg.Y2, ctm);
                    (float cx3, float cy3) = PdfToCanvas(seg.X3, seg.Y3, ctm);
                    builder.CubicTo(cx1, cy1, cx2, cy2, cx3, cy3);
                    break;
                }
                case PDFStreamEngine.PathSegmentType.Close:
                    builder.Close();
                    break;
            }
        }

        return builder.Detach();
    }

    /// <summary>
    /// Converts a point from PDF user space to SkiaSharp canvas space.
    /// Applies the CTM then flips Y.
    /// </summary>
    private (float x, float y) PdfToCanvas(float x, float y, Matrix ctm)
    {
        return PdfToCanvas(x, y, ctm, _pageHeightPt, _pageLowerLeftXPt, _pageLowerLeftYPt);
    }

    private static (float x, float y) PdfToCanvas(
        float x,
        float y,
        Matrix ctm,
        float pageHeightPt,
        float pageLowerLeftXPt,
        float pageLowerLeftYPt)
    {
        Vector v = ctm.Transform(x, y);
        return (v.GetX() - pageLowerLeftXPt, pageHeightPt - v.GetY() + pageLowerLeftYPt);
    }

    private void DrawImage(PDImage image, Matrix matrix)
    {
        DrawDecodedImage(
            SampledImageReader.GetRGBImage(image, GetColorManagementContext()),
            image.GetWidth(),
            image.GetHeight(),
            matrix,
            image.GetInterpolate());
    }

    private void DrawImageXObject(PDImageXObject image)
    {
        int width = image.GetWidth();
        int height = image.GetHeight();
        byte[] rgb = SampledImageReader.GetRGBImage(image, GetColorManagementContext());
        byte[]? alpha = CreateSoftMaskAlpha(image, width, height);

        DrawDecodedImage(
            rgb,
            width,
            height,
            GetGraphicsState().GetCurrentTransformationMatrix(),
            image.GetInterpolate(),
            alpha,
            preferDctSampling: IsDctDecodeImage(image));
    }

    private void DrawDecodedImage(
        byte[] rgb,
        int width,
        int height,
        Matrix matrix,
        bool interpolate,
        byte[]? alpha = null,
        bool preferDctSampling = false)
    {
        if (_graphics?.GetSkiaCanvas() is null || !IsContentRendered())
        {
            return;
        }

        if (width <= 0 || height <= 0 || rgb.Length < width * height * 3)
        {
            return;
        }

        using SKBitmap bitmap = CreateBitmapFromRgb(rgb, width, height, alpha);
        using SKPaint paint = CreateImagePaint(GetGraphicsState());
        DrawBitmap(bitmap, matrix, paint, GetImageSamplingOptions(width, height, matrix, interpolate, preferDctSampling));
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
        using var builder = new SKPathBuilder();
        foreach (GeneralPath.Segment segment in path.Segments)
        {
            switch (segment.Type)
            {
                case GeneralPath.SegmentType.MoveTo:
                {
                    (float x, float y) = PdfToCanvas(segment.X1, segment.Y1, matrix);
                    builder.MoveTo(x, y);
                    break;
                }
                case GeneralPath.SegmentType.LineTo:
                {
                    (float x, float y) = PdfToCanvas(segment.X1, segment.Y1, matrix);
                    builder.LineTo(x, y);
                    break;
                }
                case GeneralPath.SegmentType.QuadTo:
                {
                    (float x1, float y1) = PdfToCanvas(segment.X1, segment.Y1, matrix);
                    (float x2, float y2) = PdfToCanvas(segment.X2, segment.Y2, matrix);
                    builder.QuadTo(x1, y1, x2, y2);
                    break;
                }
                case GeneralPath.SegmentType.CurveTo:
                {
                    (float x1, float y1) = PdfToCanvas(segment.X1, segment.Y1, matrix);
                    (float x2, float y2) = PdfToCanvas(segment.X2, segment.Y2, matrix);
                    (float x3, float y3) = PdfToCanvas(segment.X3, segment.Y3, matrix);
                    builder.CubicTo(x1, y1, x2, y2, x3, y3);
                    break;
                }
                case GeneralPath.SegmentType.Close:
                    builder.Close();
                    break;
            }
        }

        return builder.Detach();
    }

    private void DrawUnicodeGlyphFallback(Matrix textRenderingMatrix, PDFont font, int code, Vector displacement)
    {
        string? unicode = font.ToUnicode(code, PdfBox.Net.PDModel.Font.Encoding.GlyphList.GetAdobeGlyphList());
        unicode ??= code is >= 0 and <= 255 ? ((char)code).ToString() : null;
        if (string.IsNullOrEmpty(unicode))
        {
            return;
        }

        RenderingMode renderingMode = GetGraphicsState().GetTextState().GetRenderingModeInstance();
        Matrix fallbackGlyphMatrix = CreateFallbackGlyphMatrix(textRenderingMatrix);
        _unicodeFallbackGlyphCount++;
        SKTypeface? typeface = GetFallbackTypeface(font);
        // Java2D renders fallback glyphs with grayscale antialiasing and fractional placement.
        using SKFont skFont = new(typeface ?? SKTypeface.Default, GetFallbackFontSize(font))
        {
            Edging = SKFontEdging.Antialias,
            Hinting = SKFontHinting.Normal,
            Subpixel = true,
        };
        float stretch = CalculateFallbackGlyphStretch(font, code, displacement, skFont.MeasureText(unicode));
        if (stretch != 1f)
        {
            fallbackGlyphMatrix = ApplyGlyphStretch(fallbackGlyphMatrix, stretch);
        }

        SKMatrix matrix = ToCanvasMatrix(fallbackGlyphMatrix);
        if (renderingMode.IsClip())
        {
            using SKPath clipPath = skFont.GetTextPath(unicode, new SKPoint(0, 0));
            BufferTextClipPath(clipPath, fallbackGlyphMatrix);
        }

        if (_graphics?.GetSkiaCanvas() is null || !IsContentRendered() || (!renderingMode.IsFill() && !renderingMode.IsStroke()))
        {
            return;
        }

        DrawWithCurrentClip(canvas =>
        {
            canvas.Concat(in matrix);
            if (renderingMode.IsFill())
            {
                using SKPaint fillPaint = CreateSkiaPaint(GetGraphicsState(), stroke: false);
                canvas.DrawText(unicode, 0, 0, SKTextAlign.Left, skFont, fillPaint);
            }

            if (renderingMode.IsStroke())
            {
                using SKPaint strokePaint = CreateSkiaPaint(GetGraphicsState(), stroke: true);
                canvas.DrawText(unicode, 0, 0, SKTextAlign.Left, skFont, strokePaint);
            }
        }, shapeCanvas =>
        {
            shapeCanvas.Concat(in matrix);
            if (renderingMode.IsFill())
            {
                using SKPaint fillPaint = CreateSkiaPaint(GetGraphicsState(), stroke: false);
                using SKPaint fillShapePaint = CreateShapePaint(fillPaint);
                shapeCanvas.DrawText(unicode, 0, 0, SKTextAlign.Left, skFont, fillShapePaint);
            }

            if (renderingMode.IsStroke())
            {
                using SKPaint strokePaint = CreateSkiaPaint(GetGraphicsState(), stroke: true);
                using SKPaint strokeShapePaint = CreateShapePaint(strokePaint);
                shapeCanvas.DrawText(unicode, 0, 0, SKTextAlign.Left, skFont, strokeShapePaint);
            }
        });
    }

    private bool DrawMappedTrueTypeGlyph(Matrix textRenderingMatrix, PDFont font, int code, Vector displacement)
    {
        PDTrueTypeFont? mappedFont = GetMappedTrueTypeFont(font);
        if (mappedFont is null)
        {
            return false;
        }

        GeneralPath path = GetGlyphCache(mappedFont).GetPathForCharacterCode(code);
        if (path.Segments.Count == 0)
        {
            return false;
        }

        Matrix glyphMatrix = Matrix.Concatenate(textRenderingMatrix, font.GetFontMatrix());
        float stretch = CalculateGlyphStretch(font, code, displacement, mappedFont.GetWidthFromFont(code));
        if (stretch != 1f)
        {
            glyphMatrix = ApplyGlyphStretch(glyphMatrix, stretch);
        }

        BufferTextClipPath(path, glyphMatrix);
        if (_graphics?.GetSkiaCanvas() is null || !IsContentRendered())
        {
            return true;
        }

        using SKPath skPath = BuildSkPath(path, glyphMatrix);
        DrawTextPath(skPath);
        return true;
    }

    private PDTrueTypeFont? GetMappedTrueTypeFont(PDFont font)
    {
        COSDictionary fontDictionary = font.GetCOSObject();
        if (_mappedTrueTypeFonts.TryGetValue(fontDictionary, out PDTrueTypeFont? cachedFont))
        {
            return cachedFont;
        }

        PDTrueTypeFont? mappedFont = LoadMappedTrueTypeFont(font);
        _mappedTrueTypeFonts[fontDictionary] = mappedFont;
        return mappedFont;
    }

    private static PDTrueTypeFont? LoadMappedTrueTypeFont(PDFont font)
    {
        COSDictionary fontDictionary = font.GetCOSObject();
        if (font is not PDDictionaryFont ||
            !string.Equals(fontDictionary.GetNameAsString(FontSubtypeKey), "TrueType", StringComparison.Ordinal) ||
            fontDictionary.GetCOSDictionary(FontDescriptorKey)?.ContainsKey(FontFile2Key) == true)
        {
            return null;
        }

        foreach (string fontName in GetMappingNames(font, fontDictionary))
        {
            string? fontPath = FontMappers.Instance.FindFontFile(fontName);
            if (string.IsNullOrWhiteSpace(fontPath) || !File.Exists(fontPath))
            {
                continue;
            }

            try
            {
                string extension = Path.GetExtension(fontPath);
                if (extension.Equals(".ttf", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".otf", StringComparison.OrdinalIgnoreCase))
                {
                    byte[] bytes = File.ReadAllBytes(fontPath);
                    TrueTypeFont trueTypeFont = new TTFParser().Parse(bytes);
                    return new PDTrueTypeFont(fontDictionary, trueTypeFont);
                }

                if (extension.Equals(".ttc", StringComparison.OrdinalIgnoreCase))
                {
                    using TrueTypeCollection collection = new(fontPath);
                    TrueTypeFont? trueTypeFont = collection.GetFontByName(fontName)
                                                 ?? collection.GetFontByName(Path.GetFileNameWithoutExtension(fontPath));
                    if (trueTypeFont is not null)
                    {
                        return new PDTrueTypeFont(fontDictionary, trueTypeFont);
                    }
                }
            }
            catch
            {
                // Keep Unicode fallback rendering when a substitute font cannot be parsed.
            }
        }

        return null;
    }

    private static IEnumerable<string> GetMappingNames(PDFont font, COSDictionary fontDictionary)
    {
        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
        foreach (string name in GetCandidateMappingNames(font, fontDictionary))
        {
            if (!string.IsNullOrWhiteSpace(name) && seen.Add(name))
            {
                yield return name;
            }
        }
    }

    private static IEnumerable<string> GetCandidateMappingNames(PDFont font, COSDictionary fontDictionary)
    {
        string baseFont = font.GetName();
        if (!string.IsNullOrWhiteSpace(baseFont))
        {
            yield return baseFont;
            foreach (string alias in GetCommonTrueTypeFallbackNames(baseFont))
            {
                yield return alias;
            }
        }

        if (fontDictionary.GetCOSDictionary(FontDescriptorKey) is COSDictionary descriptorDictionary)
        {
            string? fontName = descriptorDictionary.GetNameAsString(FontNameKey);
            if (!string.IsNullOrWhiteSpace(fontName) && !string.Equals(fontName, baseFont, StringComparison.OrdinalIgnoreCase))
            {
                yield return fontName;
                foreach (string alias in GetCommonTrueTypeFallbackNames(fontName))
                {
                    yield return alias;
                }
            }
        }
    }

    private static IEnumerable<string> GetCommonTrueTypeFallbackNames(string fontName)
    {
        string compact = fontName
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace(",", string.Empty, StringComparison.Ordinal);
        bool bold = compact.Contains("Bold", StringComparison.OrdinalIgnoreCase) ||
                    compact.Contains("Black", StringComparison.OrdinalIgnoreCase);
        bool italic = compact.Contains("Italic", StringComparison.OrdinalIgnoreCase) ||
                      compact.Contains("Oblique", StringComparison.OrdinalIgnoreCase);

        if (compact.Contains("Arial", StringComparison.OrdinalIgnoreCase) ||
            compact.Contains("Helvetica", StringComparison.OrdinalIgnoreCase) ||
            compact.Contains("LiberationSans", StringComparison.OrdinalIgnoreCase))
        {
            foreach (string alias in GetStyledTrueTypeAliases(bold, italic, "Arial", "LiberationSans", "DejaVuSans", "NimbusSans", "Helvetica"))
            {
                yield return alias;
            }
        }
        else if (compact.Contains("Courier", StringComparison.OrdinalIgnoreCase) ||
                 compact.Contains("LiberationMono", StringComparison.OrdinalIgnoreCase))
        {
            foreach (string alias in GetStyledTrueTypeAliases(bold, italic, "Courier New", "LiberationMono", "DejaVuSansMono", "NimbusMonoPS", "Courier"))
            {
                yield return alias;
            }
        }
        else if (compact.Contains("Times", StringComparison.OrdinalIgnoreCase) ||
                 compact.Contains("LiberationSerif", StringComparison.OrdinalIgnoreCase))
        {
            foreach (string alias in GetStyledTrueTypeAliases(bold, italic, "Times New Roman", "LiberationSerif", "DejaVuSerif", "NimbusRoman", "Times"))
            {
                yield return alias;
            }
        }
    }

    private static IEnumerable<string> GetStyledTrueTypeAliases(bool bold, bool italic, params string[] families)
    {
        foreach (string family in families)
        {
            if (family.Contains(' '))
            {
                yield return GetSpacedStyleName(family, bold, italic);
            }

            yield return GetHyphenStyleName(family.Replace(" ", string.Empty, StringComparison.Ordinal), bold, italic);
            yield return family;
        }
    }

    private static string GetSpacedStyleName(string family, bool bold, bool italic)
    {
        return (bold, italic) switch
        {
            (true, true) => family + " Bold Italic",
            (true, false) => family + " Bold",
            (false, true) => family + " Italic",
            _ => family,
        };
    }

    private static string GetHyphenStyleName(string family, bool bold, bool italic)
    {
        return (bold, italic) switch
        {
            (true, true) => family + "-BoldItalic",
            (true, false) => family + "-Bold",
            (false, true) => family + "-Italic",
            _ => family + "-Regular",
        };
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

    private static Matrix ApplyGlyphStretch(Matrix glyphMatrix, float stretch)
    {
        return Matrix.GetScaleInstance(stretch, 1f).Multiply(glyphMatrix);
    }

    private static float CalculateVectorGlyphStretch(PDFont font, int code, Vector displacement)
    {
        return CalculateGlyphStretch(font, code, displacement, font.GetWidthFromFont(code));
    }

    private static float CalculateFallbackGlyphStretch(PDFont font, int code, Vector displacement, float measuredTextWidth)
    {
        return CalculateGlyphStretch(font, code, displacement, measuredTextWidth * 1000f);
    }

    private static float CalculateGlyphStretch(PDFont font, int code, Vector displacement, float fontWidth)
    {
        if (font.IsEmbedded() || font.IsVertical() || font.IsStandard14() || !font.HasExplicitWidth(code))
        {
            return 1f;
        }

        float pdfWidth = displacement.GetX() * 1000f;
        if (displacement.GetX() <= 0 || fontWidth <= 0 || !float.IsFinite(fontWidth))
        {
            return 1f;
        }

        if (MathF.Abs(fontWidth - pdfWidth) <= GlyphWidthTolerance)
        {
            return 1f;
        }

        float stretch = pdfWidth / fontWidth;
        return float.IsFinite(stretch) && stretch > 0 ? stretch : 1f;
    }

    private static float GetFallbackFontSize(PDFont font)
    {
        return font is PDDictionaryFont ? 1.12f : 1f;
    }

    private SKTypeface? GetFallbackTypeface(PDFont font)
    {
        FallbackTypefaceKey key = CreateFallbackTypefaceKey(font);
        if (_fallbackTypefaces.TryGetValue(key, out SKTypeface? cachedTypeface))
        {
            _fallbackTypefaceCacheHits++;
            return cachedTypeface;
        }

        _fallbackTypefaceCacheMisses++;
        long lookupStart = Stopwatch.GetTimestamp();
        SKTypeface? typeface = CreateFallbackTypeface(key);
        _fallbackTypefaceLookupCount++;
        _fallbackTypefaceLookupTicks += Stopwatch.GetTimestamp() - lookupStart;
        _fallbackTypefaces[key] = typeface;
        return typeface;
    }

    private static FallbackTypefaceKey CreateFallbackTypefaceKey(PDFont font)
    {
        string fontName = StripSubsetPrefix(font.GetName());
        PDFontDescriptor? descriptor = font.GetFontDescriptor();
        string family = StripSubsetPrefix(descriptor?.GetFontFamily());
        bool bold = IsBold(fontName, descriptor);
        bool italic = IsItalic(fontName, descriptor);
        return new FallbackTypefaceKey(fontName, family, bold, italic);
    }

    private static SKTypeface? CreateFallbackTypeface(FallbackTypefaceKey key)
    {
        SKFontStyleWeight weight = key.Bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
        SKFontStyleSlant slant = key.Italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright;
        SKTypeface? firstAvailable = null;

        foreach (string candidate in GetFallbackFamilies(key.FontName, key.Family))
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

    private static bool IsEnvironmentFlagEnabled(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        return value is not null &&
               !value.Equals("0", StringComparison.OrdinalIgnoreCase) &&
               !value.Equals("false", StringComparison.OrdinalIgnoreCase) &&
               !value.Equals("no", StringComparison.OrdinalIgnoreCase);
    }

    private SKMatrix ToCanvasMatrix(Matrix matrix)
    {
        return ToCanvasMatrix(matrix, _pageHeightPt, _pageLowerLeftXPt, _pageLowerLeftYPt);
    }

    private static SKMatrix ToCanvasMatrix(
        Matrix matrix,
        float pageHeightPt,
        float pageLowerLeftXPt,
        float pageLowerLeftYPt)
    {
        return new SKMatrix
        {
            ScaleX = matrix.GetScaleX(),
            SkewX = matrix.GetShearX(),
            TransX = matrix.GetTranslateX() - pageLowerLeftXPt,
            SkewY = -matrix.GetShearY(),
            ScaleY = -matrix.GetScaleY(),
            TransY = pageHeightPt - matrix.GetTranslateY() + pageLowerLeftYPt,
            Persp2 = 1f,
        };
    }

    private static byte[]? CreateSoftMaskAlpha(PDImageXObject image, int width, int height)
    {
        PDImageXObject? softMask = image.GetSoftMask();
        if (softMask is null || width <= 0 || height <= 0)
        {
            return null;
        }

        int maskWidth = softMask.GetWidth();
        int maskHeight = softMask.GetHeight();
        if (maskWidth <= 0 || maskHeight <= 0)
        {
            return null;
        }

        byte[] maskRgb = SampledImageReader.GetRGBImage(softMask);
        if (maskRgb.Length < maskWidth * maskHeight * 3)
        {
            return null;
        }

        byte[] alpha = new byte[width * height];
        for (int y = 0; y < height; y++)
        {
            int maskY = Math.Min(maskHeight - 1, y * maskHeight / height);
            for (int x = 0; x < width; x++)
            {
                int maskX = Math.Min(maskWidth - 1, x * maskWidth / width);
                alpha[(y * width) + x] = maskRgb[((maskY * maskWidth) + maskX) * 3];
            }
        }

        return alpha;
    }

    private static SKBitmap CreateBitmapFromRgb(byte[] rgb, int width, int height, byte[]? alpha = null)
    {
        bool hasAlpha = alpha is { Length: var alphaLength } && alphaLength >= width * height;
        var bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, hasAlpha ? SKAlphaType.Premul : SKAlphaType.Opaque);
        int src = 0;
        int alphaIndex = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                byte a = hasAlpha ? alpha![alphaIndex] : (byte)255;
                bitmap.SetPixel(x, y, new SKColor(rgb[src], rgb[src + 1], rgb[src + 2], a));
                src += 3;
                alphaIndex++;
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
            BlendMode = ToSkiaBlendMode(graphicsState.GetBlendMode()),
        };
    }

    private void DrawBitmap(SKBitmap bitmap, Matrix matrix, SKPaint paint)
    {
        DrawBitmap(bitmap, matrix, paint, SkiaRenderingBackend.ImageSamplingOptions);
    }

    private void DrawBitmap(SKBitmap bitmap, Matrix matrix, SKPaint paint, SKSamplingOptions samplingOptions)
    {
        using SKImage image = SKImage.FromBitmap(bitmap);
        SKMatrix imageTransform = CreateImageTransform(matrix, bitmap.Width, bitmap.Height);
        SKRect sourceRect = new(0, 0, bitmap.Width, bitmap.Height);
        using SKPaint shapePaint = new()
        {
            Color = SKColors.White,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            BlendMode = SKBlendMode.SrcOver,
        };
        using SKPaint sourcePaint = paint.Clone();
        sourcePaint.BlendMode = SKBlendMode.SrcOver;
        DrawWithCurrentClip(canvas =>
        {
            canvas.Concat(in imageTransform);
            canvas.DrawImage(image, sourceRect, samplingOptions, paint);
        }, shapeCanvas =>
        {
            shapeCanvas.Concat(in imageTransform);
            shapeCanvas.DrawImage(image, sourceRect, samplingOptions, shapePaint);
        }, drawSource: sourceCanvas =>
        {
            sourceCanvas.Concat(in imageTransform);
            sourceCanvas.DrawImage(image, sourceRect, samplingOptions, sourcePaint);
        });
    }

    private SKMatrix CreateImageTransform(Matrix matrix, int imageWidth, int imageHeight)
    {
        (float x0, float y0) = PdfToCanvas(0, 1, matrix);
        (float x1, float y1) = PdfToCanvas(1, 1, matrix);
        (float x2, float y2) = PdfToCanvas(0, 0, matrix);

        return new SKMatrix
        {
            ScaleX = (x1 - x0) / imageWidth,
            SkewY = (y1 - y0) / imageWidth,
            SkewX = (x2 - x0) / imageHeight,
            ScaleY = (y2 - y0) / imageHeight,
            TransX = x0,
            TransY = y0,
            Persp2 = 1,
        };
    }

    private SKSamplingOptions GetImageSamplingOptions(int imageWidth, int imageHeight, Matrix matrix, bool interpolate, bool preferDctSampling)
    {
        if (!interpolate && IsImageScaledUp(imageWidth, imageHeight, matrix))
        {
            return new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.None);
        }

        return preferDctSampling
            ? SkiaRenderingBackend.DctImageSamplingOptions
            : SkiaRenderingBackend.ImageSamplingOptions;
    }

    private static bool IsDctDecodeImage(PDImageXObject image)
    {
        return image.GetStream()?.GetFilters().Any(IsDctDecodeFilter) == true;
    }

    private static bool IsDctDecodeFilter(COSName filter)
    {
        return filter.Equals(COSName.DCT_DECODE) || filter.Equals(COSName.DCT_DECODE_ABBREVIATION);
    }

    private bool IsImageScaledUp(int imageWidth, int imageHeight, Matrix matrix)
    {
        Matrix deviceTransform = new(_xform);
        float scaleX = matrix.GetScalingFactorX() * deviceTransform.GetScalingFactorX();
        float scaleY = matrix.GetScalingFactorY() * deviceTransform.GetScalingFactorY();
        return imageWidth <= AbsJavaRounded(scaleX) ||
               imageHeight <= AbsJavaRounded(scaleY);
    }

    private static int AbsJavaRounded(float value)
    {
        return Math.Abs((int)MathF.Floor(value + 0.5f));
    }

    /// <summary>Creates a SkiaSharp paint from the current graphics state.</summary>
    private SKPaint CreateSkiaPaint(PDGraphicsState graphicsState, bool stroke)
    {
        PDColor pdColor = stroke
            ? graphicsState.GetStrokingColor()
            : graphicsState.GetNonStrokingColor();

        SKShader? shader = CreatePatternShader(pdColor);
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
            BlendMode = ToSkiaBlendMode(graphicsState.GetBlendMode()),
        };
        if (shader is not null)
        {
            paint.Shader = shader;
            shader.Dispose();
        }

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

    private static SKPaint CreateShapePaint(SKPaint source)
    {
        return new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = source.IsAntialias,
            Style = source.Style,
            StrokeWidth = source.StrokeWidth,
            StrokeCap = source.StrokeCap,
            StrokeJoin = source.StrokeJoin,
            StrokeMiter = source.StrokeMiter,
            PathEffect = source.PathEffect,
            BlendMode = SKBlendMode.SrcOver,
        };
    }

    private SKShader? CreatePatternShader(PDColor color)
    {
        PDColorSpace? colorSpace = color.GetColorSpace();
        if (colorSpace is not PDPattern patternColorSpace)
        {
            return null;
        }

        COSName? patternName = color.GetPatternName();
        PDAbstractPattern? pattern = patternName is null ? null : patternColorSpace.GetResources()?.GetPattern(patternName);
        if (pattern is not PDTilingPattern tilingPattern)
        {
            return null;
        }

        PDColorSpace? underlyingColorSpace = tilingPattern.GetPaintType() == PDTilingPattern.PAINT_UNCOLORED
            ? patternColorSpace.GetUnderlyingColorSpace()
            : null;
        PDColor? uncoloredPatternColor = underlyingColorSpace is null ? null : color;
        IPaint tilingPaint = _tilingPaintFactory.Create(tilingPattern, underlyingColorSpace, uncoloredPatternColor, _xform);
        return tilingPaint is TilingPaint paint ? CreateTextureShader(paint.TexturePaint) : null;
    }

    private static SKShader CreateTextureShader(TexturePaint texturePaint)
    {
        BufferedImage image = texturePaint.Image;
        Rectangle2D anchor = texturePaint.AnchorRect;
        double anchorWidth = Math.Abs(anchor.Width) > double.Epsilon ? Math.Abs(anchor.Width) : image.Width;
        double anchorHeight = Math.Abs(anchor.Height) > double.Epsilon ? Math.Abs(anchor.Height) : image.Height;
        float scaleX = (float)(image.Width / anchorWidth);
        float scaleY = (float)(image.Height / anchorHeight);
        SKMatrix localMatrix = new()
        {
            ScaleX = scaleX,
            ScaleY = scaleY,
            TransX = (float)(-anchor.X * scaleX),
            TransY = (float)(-anchor.Y * scaleY),
            Persp2 = 1,
        };

        return SKShader.CreateBitmap(
            image.GetSkiaBitmap(),
            SKShaderTileMode.Repeat,
            SKShaderTileMode.Repeat,
            localMatrix);
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

    private int ResolveUncoloredPatternColor(PDPattern patternColorSpace, PDColor color)
    {
        PDColorSpace? underlying = patternColorSpace.GetUnderlyingColorSpace();
        if (underlying is null)
        {
            return 0;
        }

        return SafeToRgb(new PDColor(color.GetComponents(), underlying), 0);
    }

    private int ResolveShadingColor(PDShading shading, PDGraphicsState? graphicsState = null)
    {
        COSArray? background = shading.GetBackground();
        if (background is not null && background.Size() > 0)
        {
            return SafeToRgb(new PDColor(background, shading.GetColorSpace()), 0, graphicsState);
        }

        try
        {
            float[] color = shading.EvalFunction([0.5f]);
            return SafeToRgb(new PDColor(color, shading.GetColorSpace()), 0, graphicsState);
        }
        catch (Exception ex) when (IsRecoverableRenderingException(ex))
        {
            return 0;
        }
    }

    private int SafeToRgb(PDColor color, int fallback, PDGraphicsState? graphicsState = null)
    {
        try
        {
            PDColorSpace? colorSpace = color.GetColorSpace();
            if (colorSpace is not null)
            {
                PDColorSpace effectiveColorSpace = GetColorManagementContext(graphicsState)
                    ?.ResolveColorSpace(colorSpace) ?? colorSpace;
                if (!ReferenceEquals(effectiveColorSpace, colorSpace))
                {
                    color = new PDColor(color.GetComponents(), effectiveColorSpace);
                }
            }

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
        (float x0, float y0) = PdfToCanvas(bbox.GetLowerLeftX(), bbox.GetLowerLeftY(), ctm);
        (float x1, float y1) = PdfToCanvas(bbox.GetUpperRightX(), bbox.GetUpperRightY(), ctm);
        return new SKRect(Math.Min(x0, x1), Math.Min(y0, y1), Math.Max(x0, x1), Math.Max(y0, y1));
    }

    private SKPaint CreateShadingPaint(PDShading shading, PDGraphicsState graphicsState)
    {
        int rgb = ResolveShadingColor(shading, graphicsState);
        byte r = (byte)((rgb >> 16) & 0xFF);
        byte g = (byte)((rgb >> 8) & 0xFF);
        byte b = (byte)(rgb & 0xFF);
        byte a = (byte)Math.Round(Math.Clamp(graphicsState.GetNonStrokeAlphaConstant(), 0f, 1f) * 255f);
        SKPaint paint = new()
        {
            Color = new SKColor(r, g, b, a),
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            BlendMode = ToSkiaBlendMode(graphicsState.GetBlendMode()),
        };

        SKShader? shader = CreateShadingShader(shading, graphicsState);
        if (shader is not null)
        {
            paint.Shader = shader;
        }

        return paint;
    }

    private SKShader? CreateShadingShader(PDShading shading, PDGraphicsState graphicsState)
    {
        try
        {
            return shading switch
            {
                PDShadingType3 radial => CreateRadialShadingShader(radial, graphicsState),
                PDShadingType2 axial => CreateAxialShadingShader(axial, graphicsState),
                _ => null
            };
        }
        catch (Exception ex) when (IsRecoverableRenderingException(ex))
        {
            return null;
        }
    }

    private SKShader? CreateAxialShadingShader(PDShadingType2 shading, PDGraphicsState graphicsState)
    {
        float[]? coordinates = shading.GetCoords()?.ToFloatArray();
        if (coordinates is null || coordinates.Length < 4)
        {
            return null;
        }

        (SKColor[] colors, float[] positions) = CreateShadingGradientStops(shading, graphicsState);
        if (colors.Length < 2)
        {
            return null;
        }

        Matrix matrix = graphicsState.GetCurrentTransformationMatrix();
        (float startX, float startY) = PdfToCanvas(coordinates[0], coordinates[1], matrix);
        (float endX, float endY) = PdfToCanvas(coordinates[2], coordinates[3], matrix);
        return SKShader.CreateLinearGradient(
            new SKPoint(startX, startY),
            new SKPoint(endX, endY),
            colors,
            positions,
            SKShaderTileMode.Clamp);
    }

    private SKShader? CreateRadialShadingShader(PDShadingType3 shading, PDGraphicsState graphicsState)
    {
        float[]? coordinates = shading.GetCoords()?.ToFloatArray();
        if (coordinates is null || coordinates.Length < 6)
        {
            return null;
        }

        (SKColor[] colors, float[] positions) = CreateShadingGradientStops(shading, graphicsState);
        if (colors.Length < 2)
        {
            return null;
        }

        Matrix matrix = graphicsState.GetCurrentTransformationMatrix();
        (float startX, float startY) = PdfToCanvas(coordinates[0], coordinates[1], matrix);
        (float endX, float endY) = PdfToCanvas(coordinates[3], coordinates[4], matrix);
        float startRadius = TransformWidth(graphicsState, coordinates[2]);
        float endRadius = TransformWidth(graphicsState, coordinates[5]);
        return SKShader.CreateTwoPointConicalGradient(
            new SKPoint(startX, startY),
            startRadius,
            new SKPoint(endX, endY),
            endRadius,
            colors,
            positions,
            SKShaderTileMode.Clamp);
    }

    private (SKColor[] Colors, float[] Positions) CreateShadingGradientStops(
        PDShadingType2 shading,
        PDGraphicsState graphicsState)
    {
        const int stopCount = 9;
        float[] domain = shading.GetDomain()?.ToFloatArray() ?? [0, 1];
        float domainStart = domain.Length > 0 ? domain[0] : 0;
        float domainEnd = domain.Length > 1 ? domain[1] : 1;
        byte alpha = (byte)Math.Round(Math.Clamp(graphicsState.GetNonStrokeAlphaConstant(), 0f, 1f) * 255f);
        SKColor[] colors = new SKColor[stopCount];
        float[] positions = new float[stopCount];
        for (int index = 0; index < stopCount; index++)
        {
            float position = index / (float)(stopCount - 1);
            float input = domainStart + ((domainEnd - domainStart) * position);
            int rgb = SafeToRgb(
                new PDColor(shading.EvalFunction(input), shading.GetColorSpace()),
                0,
                graphicsState);
            colors[index] = new SKColor(
                (byte)((rgb >> 16) & 0xFF),
                (byte)((rgb >> 8) & 0xFF),
                (byte)(rgb & 0xFF),
                alpha);
            positions[index] = position;
        }

        return (colors, positions);
    }

    private PDColorManagementContext? GetColorManagementContext(PDGraphicsState? graphicsState = null)
    {
        RenderingIntent renderingIntent = (graphicsState ?? GetGraphicsState()).GetRenderingIntentInstance();
        return _parameters.GetColorManagementContext(renderingIntent);
    }

    private static bool IsRecoverableRenderingException(Exception ex)
    {
        return ex is IOException
            or InvalidOperationException
            or NotSupportedException
            or ArgumentException;
    }

    private SoftMaskData? CreateSoftMaskData(PDSoftMask? softMask)
    {
        PDTransparencyGroup? form = softMask?.GetGroup();
        if (form is null)
        {
            return null;
        }

        string? subType = softMask!.GetSubType()?.GetName();
        bool isAlpha = string.Equals(subType, "Alpha", StringComparison.Ordinal);
        bool isLuminosity = string.Equals(subType, "Luminosity", StringComparison.Ordinal);
        if (!isAlpha && !isLuminosity)
        {
            return null;
        }

        PDColor? backdropColor = null;
        if (isLuminosity && softMask.GetBackdropColor() is COSArray backdropArray)
        {
            PDColorSpace? colorSpace = form.GetGroup()?.GetColorSpace(form.GetResources());
            if (colorSpace is not null && colorSpace.GetNumberOfComponents() == backdropArray.Size())
            {
                backdropColor = new PDColor(backdropArray, colorSpace);
            }
        }

        using TransparencyGroup group = CreateTransparencyGroup(
            form,
            true,
            softMask.GetInitialTransformationMatrix() ?? GetGraphicsState().GetCurrentTransformationMatrix(),
            backdropColor);
        if (group.GetImage() is not BufferedImage source)
        {
            return null;
        }

        BufferedImage gray = new(source.Width, source.Height, BufferedImage.TYPE_INT_ARGB);
        gray.Clear(Color.Transparent);
        PDFunction? transferFunction = softMask.GetTransferFunction();
        Rectangle2D groupBounds = group.GetBounds();
        int minX = Math.Clamp((int)Math.Floor(groupBounds.X), 0, source.Width);
        int minY = Math.Clamp((int)Math.Floor(groupBounds.Y), 0, source.Height);
        int maxX = Math.Clamp((int)Math.Ceiling(groupBounds.X + groupBounds.Width), 0, source.Width);
        int maxY = Math.Clamp((int)Math.Ceiling(groupBounds.Y + groupBounds.Height), 0, source.Height);
        for (int y = minY; y < maxY; y++)
        {
            for (int x = minX; x < maxX; x++)
            {
                int argb = source.GetRgb(x, y);
                int alpha = (argb >> 24) & 0xFF;
                float maskValue;
                if (isAlpha)
                {
                    maskValue = alpha / 255f;
                }
                else
                {
                    float red = ((argb >> 16) & 0xFF) / 255f;
                    float green = ((argb >> 8) & 0xFF) / 255f;
                    float blue = (argb & 0xFF) / 255f;
                    maskValue = ((0.3f * red) + (0.59f * green) + (0.11f * blue)) * (alpha / 255f);
                }

                if (transferFunction is not null)
                {
                    try
                    {
                        float[] result = transferFunction.Eval([maskValue]);
                        if (result.Length > 0)
                        {
                            maskValue = result[0];
                        }
                    }
                    catch (Exception ex) when (IsRecoverableRenderingException(ex))
                    {
                        // Match PDFBox's best-effort rendering when a transfer function is malformed.
                    }
                }

                int maskAlpha = (int)MathF.Round(Math.Clamp(maskValue, 0f, 1f) * 255f);
                gray.SetRgb(x, y, (maskAlpha << 24) | 0x00FFFFFF);
            }
        }

        gray = AdjustImage(gray);
        return new SoftMaskData(
            gray,
            new Rectangle2D(0, 0, gray.Width, gray.Height),
            backdropColor,
            transferFunction);
    }

    private void RenderTransparencyGroupContent(
        PDTransparencyGroup form,
        Graphics2D groupGraphics,
        bool isSoftMask,
        Matrix ctm,
        ITransparencyGroupCompositor? groupCompositor)
    {
        Graphics2D? savedGraphics = _graphics;
        PDGraphicsState graphicsState = GetGraphicsState();
        Matrix savedCtm = graphicsState.GetCurrentTransformationMatrix();
        BlendMode savedBlendMode = graphicsState.GetBlendMode();
        float savedStrokeAlpha = graphicsState.GetAlphaConstant();
        float savedFillAlpha = graphicsState.GetNonStrokeAlphaConstant();
        PDSoftMask? savedSoftMask = graphicsState.GetSoftMask();
        PDColorSpace savedStrokingColorSpace = graphicsState.GetStrokingColorSpace();
        PDColorSpace savedNonStrokingColorSpace = graphicsState.GetNonStrokingColorSpace();
        PDColor savedStrokingColor = graphicsState.GetStrokingColor();
        PDColor savedNonStrokingColor = graphicsState.GetNonStrokingColor();
        ITransparencyGroupCompositor? savedGroupCompositor = _transparencyGroupCompositor;

        _graphics = groupGraphics;
        try
        {
            _transparencyGroupCompositor = groupCompositor;
            graphicsState.SetCurrentTransformationMatrix(ctm);
            graphicsState.SetBlendMode(BlendMode.NORMAL);
            graphicsState.SetAlphaConstant(1f);
            graphicsState.SetNonStrokeAlphaConstant(1f);
            graphicsState.SetSoftMask(null);
            if (isSoftMask)
            {
                // Soft masks are resolved while the parent paint operator still owns its current
                // path. The mask stream starts with an independent path, as in PDFBox's saved
                // linePath handling; the parent Skia path has already been materialized by then.
                EndPath();
                graphicsState.SetStrokingColorSpace(PDDeviceGray.Instance);
                graphicsState.SetNonStrokingColorSpace(PDDeviceGray.Instance);
                graphicsState.SetStrokingColor(PDDeviceGray.Instance.GetInitialColor());
                graphicsState.SetNonStrokingColor(PDDeviceGray.Instance.GetInitialColor());
            }

            base.XObject(form);
        }
        finally
        {
            graphicsState.SetCurrentTransformationMatrix(savedCtm);
            graphicsState.SetBlendMode(savedBlendMode);
            graphicsState.SetAlphaConstant(savedStrokeAlpha);
            graphicsState.SetNonStrokeAlphaConstant(savedFillAlpha);
            graphicsState.SetSoftMask(savedSoftMask);
            graphicsState.SetStrokingColorSpace(savedStrokingColorSpace);
            graphicsState.SetNonStrokingColorSpace(savedNonStrokingColorSpace);
            graphicsState.SetStrokingColor(savedStrokingColor);
            graphicsState.SetNonStrokingColor(savedNonStrokingColor);
            groupCompositor?.Complete();
            _transparencyGroupCompositor = savedGroupCompositor;
            _graphics = savedGraphics;
        }
    }

    private bool HasBlendMode(PDTransparencyGroup group, HashSet<COSStream> groupsInProgress)
    {
        COSStream? groupStream = group.GetCOSObject();
        if (groupStream is null)
        {
            return false;
        }

        if (!groupsInProgress.Add(groupStream))
        {
            return false;
        }

        if (_blendModeCache.TryGetValue(groupStream, out bool cached))
        {
            return cached;
        }

        PDResources? resources = group.GetResources();
        if (resources is null)
        {
            _blendModeCache[groupStream] = false;
            return false;
        }

        foreach (COSName name in resources.GetExtGStateNames())
        {
            PDExtendedGraphicsState? extGState = resources.GetExtGState(name);
            if (extGState is not null && extGState.GetBlendMode() != BlendMode.NORMAL)
            {
                _blendModeCache[groupStream] = true;
                return true;
            }
        }

        foreach (COSName name in resources.GetXObjectNames())
        {
            try
            {
                if (resources.GetXObject(name) is PDTransparencyGroup nestedGroup &&
                    HasBlendMode(nestedGroup, groupsInProgress))
                {
                    _blendModeCache[groupStream] = true;
                    return true;
                }
            }
            catch (IOException)
            {
                // Match PDFBox's best-effort resource traversal for malformed XObjects.
            }
        }

        _blendModeCache[groupStream] = false;
        return false;
    }

    private bool HasIccSourceColorSpace(PDTransparencyGroup group, HashSet<COSStream> groupsInProgress)
    {
        COSStream? groupStream = group.GetCOSObject();
        if (groupStream is null)
        {
            return false;
        }

        if (!groupsInProgress.Add(groupStream))
        {
            return false;
        }

        if (_iccSourceColorSpaceCache.TryGetValue(groupStream, out bool cached))
        {
            return cached;
        }

        PDResources? resources = group.GetResources();
        if (resources is null)
        {
            _iccSourceColorSpaceCache[groupStream] = false;
            return false;
        }

        COSDictionary? colorSpaces = resources.GetCOSObject().GetCOSDictionary(COSName.GetPDFName("ColorSpace"));
        if (colorSpaces is not null)
        {
            foreach (COSName name in colorSpaces.KeySet())
            {
                try
                {
                    if (resources.GetColorSpace(name) is PDICCBased)
                    {
                        _iccSourceColorSpaceCache[groupStream] = true;
                        return true;
                    }
                }
                catch (IOException)
                {
                    // Match PDFBox's best-effort resource traversal for malformed color spaces.
                }
            }
        }

        foreach (COSName name in resources.GetXObjectNames())
        {
            try
            {
                if (resources.GetXObject(name) is PDTransparencyGroup nestedGroup &&
                    HasIccSourceColorSpace(nestedGroup, groupsInProgress))
                {
                    _iccSourceColorSpaceCache[groupStream] = true;
                    return true;
                }
            }
            catch (IOException)
            {
                // Match PDFBox's best-effort resource traversal for malformed XObjects.
            }
        }

        _iccSourceColorSpaceCache[groupStream] = false;
        return false;
    }

    private static bool HasDirectBlendMode(PDTransparencyGroup group)
    {
        PDResources? resources = group.GetResources();
        if (resources is null)
        {
            return false;
        }

        foreach (COSName name in resources.GetExtGStateNames())
        {
            PDExtendedGraphicsState? extGState = resources.GetExtGState(name);
            if (extGState is not null && extGState.GetBlendMode() != BlendMode.NORMAL)
            {
                return true;
            }
        }

        return false;
    }

    private static SKBlendMode ToSkiaBlendMode(BlendMode blendMode)
    {
        return blendMode switch
        {
            BlendMode.MULTIPLY => SKBlendMode.Multiply,
            BlendMode.SCREEN => SKBlendMode.Screen,
            BlendMode.OVERLAY => SKBlendMode.Overlay,
            BlendMode.DARKEN => SKBlendMode.Darken,
            BlendMode.LIGHTEN => SKBlendMode.Lighten,
            BlendMode.COLOR_DODGE => SKBlendMode.ColorDodge,
            BlendMode.COLOR_BURN => SKBlendMode.ColorBurn,
            BlendMode.HARD_LIGHT => SKBlendMode.HardLight,
            BlendMode.SOFT_LIGHT => SKBlendMode.SoftLight,
            BlendMode.DIFFERENCE => SKBlendMode.Difference,
            BlendMode.EXCLUSION => SKBlendMode.Exclusion,
            BlendMode.HUE => SKBlendMode.Hue,
            BlendMode.SATURATION => SKBlendMode.Saturation,
            BlendMode.COLOR => SKBlendMode.Color,
            BlendMode.LUMINOSITY => SKBlendMode.Luminosity,
            _ => SKBlendMode.SrcOver,
        };
    }

    private sealed record SoftMaskData(
        BufferedImage Image,
        Rectangle2D Bounds,
        PDColor? BackdropColor,
        PDFunction? TransferFunction) : IDisposable
    {
        public void Dispose() => Image.Dispose();
    }

    private sealed record ComponentSource(
        float[] Components,
        byte[] Alpha,
        int Width,
        int Height,
        int ComponentCount);

    private interface ITransparencyGroupCompositor : IDisposable
    {
        bool UsesComponents { get; }

        void Draw(
            SKCanvas targetCanvas,
            SKMatrix matrix,
            Action<SKCanvas> draw,
            Action<SKCanvas> drawShape,
            Action<SKCanvas> drawSource,
            PDColor? sourceColor,
            ComponentSource? componentSource,
            BlendMode blendMode);

        void Complete();

        ComponentSource? GetComponentSource();
    }

    private sealed class NonIsolatedGroupCompositor : ITransparencyGroupCompositor
    {
        private readonly SKBitmap _target;
        private readonly SKBitmap _backdrop;
        private readonly SKBitmap _groupAlpha;
        private readonly SKCanvas _groupAlphaCanvas;
        private readonly int _left;
        private readonly int _top;
        private readonly int _right;
        private readonly int _bottom;

        public NonIsolatedGroupCompositor(
            Graphics2D targetGraphics,
            Graphics2D parentGraphics,
            Rectangle2D bounds)
        {
            _target = targetGraphics.GetSkiaBitmap()
                ?? throw new InvalidOperationException("Transparency groups require a bitmap-backed target.");
            SKBitmap parent = parentGraphics.GetSkiaBitmap()
                ?? throw new InvalidOperationException("Non-isolated groups require a bitmap-backed backdrop.");
            _backdrop = CreateBitmap(_target.Width, _target.Height);
            _groupAlpha = CreateBitmap(_target.Width, _target.Height);
            _groupAlphaCanvas = new SKCanvas(_groupAlpha);
            _left = Math.Clamp((int)Math.Floor(bounds.X), 0, _target.Width);
            _top = Math.Clamp((int)Math.Floor(bounds.Y), 0, _target.Height);
            _right = Math.Clamp((int)Math.Ceiling(bounds.X + bounds.Width), _left, _target.Width);
            _bottom = Math.Clamp((int)Math.Ceiling(bounds.Y + bounds.Height), _top, _target.Height);

            CopyBitmap(parent, _backdrop);
            CopyBitmap(_backdrop, _target);
            _groupAlpha.Erase(SKColors.Transparent);
        }

        public bool UsesComponents => false;

        public void Draw(
            SKCanvas targetCanvas,
            SKMatrix matrix,
            Action<SKCanvas> draw,
            Action<SKCanvas> drawShape,
            Action<SKCanvas> drawSource,
            PDColor? sourceColor,
            ComponentSource? componentSource,
            BlendMode blendMode)
        {
            draw(targetCanvas);
            _groupAlphaCanvas.SetMatrix(matrix);
            draw(_groupAlphaCanvas);
        }

        public void Complete()
        {
            _groupAlphaCanvas.Flush();
            for (int y = _top; y < _bottom; y++)
            {
                for (int x = _left; x < _right; x++)
                {
                    byte groupAlpha = _groupAlpha.GetPixel(x, y).Alpha;
                    if (groupAlpha == 0)
                    {
                        _target.SetPixel(x, y, SKColors.Transparent);
                        continue;
                    }

                    SKColor groupColor = _target.GetPixel(x, y);
                    SKColor backdropColor = _backdrop.GetPixel(x, y);
                    float alphaFactor = backdropColor.Alpha / (float)groupAlpha - backdropColor.Alpha / 255f;
                    _target.SetPixel(x, y, new SKColor(
                        RemoveBackdrop(groupColor.Red, backdropColor.Red, alphaFactor),
                        RemoveBackdrop(groupColor.Green, backdropColor.Green, alphaFactor),
                        RemoveBackdrop(groupColor.Blue, backdropColor.Blue, alphaFactor),
                        groupAlpha));
                }
            }
        }

        public ComponentSource? GetComponentSource() => null;

        public void Dispose()
        {
            _groupAlphaCanvas.Dispose();
            _groupAlpha.Dispose();
            _backdrop.Dispose();
        }

        private static SKBitmap CreateBitmap(int width, int height)
        {
            return new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        }

        private static void CopyBitmap(SKBitmap source, SKBitmap target)
        {
            using SKCanvas canvas = new(target);
            using SKPaint paint = new() { BlendMode = SKBlendMode.Src };
            canvas.DrawBitmap(source, 0, 0, SkiaRenderingBackend.ImageSamplingOptions, paint);
        }

        private static byte RemoveBackdrop(byte group, byte backdrop, float alphaFactor)
        {
            return (byte)Math.Clamp((int)MathF.Round(group + ((group - backdrop) * alphaFactor)), 0, 255);
        }
    }

    private sealed class KnockoutGroupCompositor : ITransparencyGroupCompositor
    {
        // Each knockout child blends with the original backdrop, then replaces prior group content
        // within its independent shape while group alpha accumulates separately.
        private readonly SKBitmap _target;
        private readonly SKBitmap _backdrop;
        private readonly SKBitmap _objectColor;
        private readonly SKBitmap _objectAlpha;
        private readonly SKBitmap _objectShape;
        private readonly SKCanvas _objectColorCanvas;
        private readonly SKCanvas _objectAlphaCanvas;
        private readonly SKCanvas _objectShapeCanvas;
        private readonly byte[] _groupAlpha;
        private readonly float[]? _componentBackdrop;
        private readonly float[]? _componentTarget;
        private readonly float[]? _componentTargetAlpha;
        private readonly float[]? _componentOutput;
        private readonly Func<float[], int>? _componentToRgb;
        private readonly Func<PDColor, float[]?>? _sourceToComponents;
        private readonly Dictionary<int, float[]> _rgbToComponentCache = [];
        private readonly bool _isolated;
        private readonly bool _knockout;
        private readonly int _left;
        private readonly int _top;
        private readonly int _right;
        private readonly int _bottom;

        public KnockoutGroupCompositor(
            Graphics2D targetGraphics,
            Graphics2D? parentGraphics,
            bool isolated,
            Rectangle2D bounds,
            bool knockout = true,
            Func<float[], int>? componentToRgb = null,
            Func<PDColor, float[]?>? sourceToComponents = null)
        {
            _target = targetGraphics.GetSkiaBitmap()
                ?? throw new InvalidOperationException("Knockout groups require a bitmap-backed target.");
            _isolated = isolated || parentGraphics?.GetSkiaBitmap() is not SKBitmap;
            _knockout = knockout;
            _backdrop = CreateBitmap(_target.Width, _target.Height);
            _objectColor = CreateBitmap(_target.Width, _target.Height);
            _objectAlpha = CreateBitmap(_target.Width, _target.Height);
            _objectShape = CreateBitmap(_target.Width, _target.Height);
            _objectColorCanvas = new SKCanvas(_objectColor);
            _objectAlphaCanvas = new SKCanvas(_objectAlpha);
            _objectShapeCanvas = new SKCanvas(_objectShape);
            _groupAlpha = new byte[_target.Width * _target.Height];
            _componentToRgb = componentToRgb;
            _sourceToComponents = sourceToComponents;
            _left = Math.Clamp((int)Math.Floor(bounds.X), 0, _target.Width);
            _top = Math.Clamp((int)Math.Floor(bounds.Y), 0, _target.Height);
            _right = Math.Clamp((int)Math.Ceiling(bounds.X + bounds.Width), _left, _target.Width);
            _bottom = Math.Clamp((int)Math.Ceiling(bounds.Y + bounds.Height), _top, _target.Height);

            if (_isolated)
            {
                _backdrop.Erase(SKColors.Transparent);
                _target.Erase(SKColors.Transparent);
            }
            else
            {
                CopyBitmap(parentGraphics!.GetSkiaBitmap()!, _backdrop);
                CopyBitmap(_backdrop, _target);
            }

            if (_componentToRgb is not null)
            {
                _componentBackdrop = new float[checked(_target.Width * _target.Height * 4)];
                _componentTarget = new float[_componentBackdrop.Length];
                _componentTargetAlpha = new float[checked(_target.Width * _target.Height)];
                _componentOutput = new float[_componentBackdrop.Length];
                InitializeComponentBackdrop();
            }
        }

        public bool UsesComponents => _componentToRgb is not null;

        public void Draw(
            SKCanvas targetCanvas,
            SKMatrix matrix,
            Action<SKCanvas> draw,
            Action<SKCanvas> drawShape,
            Action<SKCanvas> drawSource,
            PDColor? sourceColor,
            ComponentSource? componentSource,
            BlendMode blendMode)
        {
            if (_componentToRgb is not null)
            {
                DrawComponentObject(
                    matrix,
                    draw,
                    drawShape,
                    drawSource,
                    sourceColor,
                    componentSource,
                    blendMode);
                return;
            }

            CopyBitmap(_backdrop, _objectColor);
            _objectAlpha.Erase(SKColors.Transparent);
            _objectShape.Erase(SKColors.Transparent);
            _objectColorCanvas.SetMatrix(matrix);
            _objectAlphaCanvas.SetMatrix(matrix);
            _objectShapeCanvas.SetMatrix(matrix);
            draw(_objectColorCanvas);
            draw(_objectAlphaCanvas);
            drawShape(_objectShapeCanvas);
            _objectColorCanvas.Flush();
            _objectAlphaCanvas.Flush();
            _objectShapeCanvas.Flush();

            for (int y = _top; y < _bottom; y++)
            {
                for (int x = _left; x < _right; x++)
                {
                    SKColor sourceAlphaColor = _objectAlpha.GetPixel(x, y);
                    float sourceAlpha = sourceAlphaColor.Alpha / 255f;
                    float shape = _objectShape.GetPixel(x, y).Alpha / 255f;
                    if (shape <= 0f)
                    {
                        continue;
                    }

                    SKColor previous = _target.GetPixel(x, y);
                    SKColor current = _objectColor.GetPixel(x, y);
                    _target.SetPixel(x, y, InterpolatePremultiplied(previous, current, shape));

                    int index = x + y * _target.Width;
                    float previousGroupAlpha = _groupAlpha[index] / 255f;
                    float groupAlpha = sourceAlpha + ((1f - shape) * previousGroupAlpha);
                    _groupAlpha[index] = ToByte(groupAlpha);
                }
            }
        }

        public void Complete()
        {
            if (_componentToRgb is not null)
            {
                CompleteComponentGroup();
                return;
            }

            for (int y = _top; y < _bottom; y++)
            {
                for (int x = _left; x < _right; x++)
                {
                    int index = x + y * _target.Width;
                    byte groupAlpha = _groupAlpha[index];
                    if (groupAlpha == 0)
                    {
                        _target.SetPixel(x, y, SKColors.Transparent);
                        continue;
                    }

                    if (_isolated)
                    {
                        SKColor color = _target.GetPixel(x, y);
                        _target.SetPixel(x, y, new SKColor(color.Red, color.Green, color.Blue, groupAlpha));
                        continue;
                    }

                    // PDF 32000-1 section 11.4.4 removes the copied backdrop before the group is
                    // composited back into its parent.
                    SKColor groupColor = _target.GetPixel(x, y);
                    SKColor backdropColor = _backdrop.GetPixel(x, y);
                    float alphaFactor = backdropColor.Alpha / (float)groupAlpha - backdropColor.Alpha / 255f;
                    _target.SetPixel(x, y, new SKColor(
                        RemoveBackdrop(groupColor.Red, backdropColor.Red, alphaFactor),
                        RemoveBackdrop(groupColor.Green, backdropColor.Green, alphaFactor),
                        RemoveBackdrop(groupColor.Blue, backdropColor.Blue, alphaFactor),
                        groupAlpha));
                }
            }
        }

        public ComponentSource? GetComponentSource()
        {
            return _componentOutput is null
                ? null
                : new ComponentSource(
                    _componentOutput,
                    _groupAlpha,
                    _target.Width,
                    _target.Height,
                    4);
        }

        private void InitializeComponentBackdrop()
        {
            float[] backdrop = _componentBackdrop!;
            float[] target = _componentTarget!;
            float[] targetAlpha = _componentTargetAlpha!;
            for (int y = _top; y < _bottom; y++)
            {
                for (int x = _left; x < _right; x++)
                {
                    int pixel = x + y * _target.Width;
                    if (_isolated)
                    {
                        targetAlpha[pixel] = 0f;
                        continue;
                    }

                    SKColor color = _backdrop.GetPixel(x, y);
                    float[] components = ConvertRgbToComponents(color);
                    int offset = pixel * 4;
                    components.CopyTo(backdrop, offset);
                    components.CopyTo(target, offset);
                    targetAlpha[pixel] = color.Alpha / 255f;
                }
            }
        }

        private void DrawComponentObject(
            SKMatrix matrix,
            Action<SKCanvas> draw,
            Action<SKCanvas> drawShape,
            Action<SKCanvas> drawSource,
            PDColor? sourceColor,
            ComponentSource? componentSource,
            BlendMode blendMode)
        {
            _objectColor.Erase(SKColors.Transparent);
            _objectAlpha.Erase(SKColors.Transparent);
            _objectShape.Erase(SKColors.Transparent);
            _objectColorCanvas.SetMatrix(matrix);
            _objectAlphaCanvas.SetMatrix(matrix);
            _objectShapeCanvas.SetMatrix(matrix);
            if (sourceColor is null)
            {
                drawSource(_objectColorCanvas);
            }
            drawSource(_objectAlphaCanvas);
            drawShape(_objectShapeCanvas);
            _objectColorCanvas.Flush();
            _objectAlphaCanvas.Flush();
            _objectShapeCanvas.Flush();

            float[]? uniformSourceComponents = sourceColor is null
                ? null
                : ConvertSourceToComponents(sourceColor);
            float[] backdrop = _componentBackdrop!;
            float[] target = _componentTarget!;
            float[] targetAlpha = _componentTargetAlpha!;
            BlendComposite composite = BlendComposite.GetInstance(blendMode, 1f);
            Span<float> blended = stackalloc float[4];
            for (int y = _top; y < _bottom; y++)
            {
                for (int x = _left; x < _right; x++)
                {
                    float shape = _objectShape.GetPixel(x, y).Alpha / 255f;
                    if (shape <= 0f)
                    {
                        continue;
                    }

                    float sourceAlpha = Math.Clamp(
                        (_objectAlpha.GetPixel(x, y).Alpha / 255f) / shape,
                        0f,
                        1f);
                    int pixel = x + y * _target.Width;
                    int offset = pixel * 4;
                    ReadOnlySpan<float> sourceComponents;
                    if (uniformSourceComponents is not null)
                    {
                        sourceComponents = uniformSourceComponents;
                    }
                    else if (componentSource is not null &&
                        componentSource.ComponentCount == 4 &&
                        componentSource.Width == _target.Width &&
                        componentSource.Height == _target.Height)
                    {
                        sourceComponents = componentSource.Components.AsSpan(offset, 4);
                    }
                    else
                    {
                        sourceComponents = ConvertRgbToComponents(_objectColor.GetPixel(x, y));
                    }
                    ReadOnlySpan<float> blendBackdrop = _knockout
                        ? backdrop.AsSpan(offset, 4)
                        : target.AsSpan(offset, 4);
                    float blendBackdropAlpha = _knockout
                        ? (_isolated ? 0f : _backdrop.GetPixel(x, y).Alpha / 255f)
                        : targetAlpha[pixel];
                    float blendedAlpha = ComposeComponentBlend(
                        composite,
                        sourceComponents,
                        sourceAlpha,
                        blendBackdrop,
                        blendBackdropAlpha,
                        blended);

                    if (_knockout)
                    {
                        float previousAlpha = targetAlpha[pixel];
                        float inverseShape = 1f - shape;
                        float interpolatedAlpha = (shape * blendedAlpha) + (inverseShape * previousAlpha);
                        for (int component = 0; component < 4; component++)
                        {
                            target[offset + component] = interpolatedAlpha <= 0f
                                ? 0f
                                : Math.Clamp(
                                    ((shape * blended[component] * blendedAlpha) +
                                     (inverseShape * target[offset + component] * previousAlpha)) /
                                    interpolatedAlpha,
                                    0f,
                                    1f);
                        }

                        targetAlpha[pixel] = interpolatedAlpha;
                        float previousGroupAlpha = _groupAlpha[pixel] / 255f;
                        _groupAlpha[pixel] = ToByte(
                            (shape * sourceAlpha) + (inverseShape * previousGroupAlpha));
                    }
                    else
                    {
                        float previousAlpha = targetAlpha[pixel];
                        float inverseShape = 1f - shape;
                        float interpolatedAlpha =
                            (shape * blendedAlpha) + (inverseShape * previousAlpha);
                        for (int component = 0; component < 4; component++)
                        {
                            target[offset + component] = interpolatedAlpha <= 0f
                                ? 0f
                                : Math.Clamp(
                                    ((shape * blended[component] * blendedAlpha) +
                                     (inverseShape * target[offset + component] * previousAlpha)) /
                                    interpolatedAlpha,
                                    0f,
                                    1f);
                        }

                        targetAlpha[pixel] = interpolatedAlpha;
                        float previousGroupAlpha = _groupAlpha[pixel] / 255f;
                        float effectiveSourceAlpha = shape * sourceAlpha;
                        _groupAlpha[pixel] = ToByte(
                            effectiveSourceAlpha +
                            ((1f - effectiveSourceAlpha) * previousGroupAlpha));
                    }
                }
            }
        }

        private float ComposeComponentBlend(
            BlendComposite composite,
            ReadOnlySpan<float> sourceComponents,
            float sourceAlpha,
            ReadOnlySpan<float> backdropComponents,
            float backdropAlpha,
            Span<float> blended)
        {
            return composite.Compose(
                sourceComponents,
                sourceAlpha,
                backdropComponents,
                backdropAlpha,
                blended,
                subtractive: true);
        }

        private float[] ConvertSourceToComponents(PDColor color)
        {
            float[] components = color.GetComponents();
            if (color.GetColorSpace() is PDDeviceCMYK && components.Length >= 4)
            {
                return
                [
                    Math.Clamp(components[0], 0f, 1f),
                    Math.Clamp(components[1], 0f, 1f),
                    Math.Clamp(components[2], 0f, 1f),
                    Math.Clamp(components[3], 0f, 1f),
                ];
            }

            float[]? converted = _sourceToComponents?.Invoke(color);
            if (converted is { Length: >= 4 })
            {
                return
                [
                    Math.Clamp(converted[0], 0f, 1f),
                    Math.Clamp(converted[1], 0f, 1f),
                    Math.Clamp(converted[2], 0f, 1f),
                    Math.Clamp(converted[3], 0f, 1f),
                ];
            }

            int rgb = color.ToRGB();
            return ConvertRgbToComponents(new SKColor(
                (byte)((rgb >> 16) & 0xFF),
                (byte)((rgb >> 8) & 0xFF),
                (byte)(rgb & 0xFF)));
        }

        private float[] ConvertRgbToComponents(SKColor color)
        {
            int key = (color.Red << 16) | (color.Green << 8) | color.Blue;
            if (_rgbToComponentCache.TryGetValue(key, out float[]? cached))
            {
                return cached;
            }

            float red = color.Red / 255f;
            float green = color.Green / 255f;
            float blue = color.Blue / 255f;
            float black = 1f - Math.Max(red, Math.Max(green, blue));
            float denominator = 1f - black;
            float[] best = denominator <= 0f
                ? [0f, 0f, 0f, 1f]
                :
                [
                    (1f - red - black) / denominator,
                    (1f - green - black) / denominator,
                    (1f - blue - black) / denominator,
                    black,
                ];
            int bestScore = GetRgbDistanceSquared(key, _componentToRgb!(best));

            for (int corner = 0; corner < 16 && bestScore > 0; corner++)
            {
                float[] candidate =
                [
                    (corner & 1) == 0 ? 0f : 1f,
                    (corner & 2) == 0 ? 0f : 1f,
                    (corner & 4) == 0 ? 0f : 1f,
                    (corner & 8) == 0 ? 0f : 1f,
                ];
                int score = GetRgbDistanceSquared(key, _componentToRgb(candidate));
                if (score < bestScore)
                {
                    best = candidate;
                    bestScore = score;
                }
            }

            foreach (float step in new[] { 0.25f, 0.1f, 0.04f, 0.015f })
            {
                for (int component = 0; component < 4 && bestScore > 0; component++)
                {
                    foreach (float direction in new[] { -1f, 1f })
                    {
                        float[] candidate = (float[])best.Clone();
                        candidate[component] = Math.Clamp(candidate[component] + (direction * step), 0f, 1f);
                        int score = GetRgbDistanceSquared(key, _componentToRgb(candidate));
                        if (score < bestScore)
                        {
                            best = candidate;
                            bestScore = score;
                        }
                    }
                }
            }

            _rgbToComponentCache[key] = best;
            return best;
        }

        private void CompleteComponentGroup()
        {
            float[] backdrop = _componentBackdrop!;
            float[] target = _componentTarget!;
            float[] componentOutput = _componentOutput!;
            float[] output = new float[4];
            for (int y = _top; y < _bottom; y++)
            {
                for (int x = _left; x < _right; x++)
                {
                    int pixel = x + y * _target.Width;
                    byte groupAlpha = _groupAlpha[pixel];
                    if (groupAlpha == 0)
                    {
                        componentOutput.AsSpan(pixel * 4, 4).Clear();
                        _target.SetPixel(x, y, SKColors.Transparent);
                        continue;
                    }

                    int offset = pixel * 4;
                    if (_isolated)
                    {
                        target.AsSpan(offset, 4).CopyTo(output);
                    }
                    else
                    {
                        float backdropAlpha = _backdrop.GetPixel(x, y).Alpha / 255f;
                        float alphaFactor = backdropAlpha / (groupAlpha / 255f) - backdropAlpha;
                        for (int component = 0; component < 4; component++)
                        {
                            output[component] = Math.Clamp(
                                target[offset + component] +
                                ((target[offset + component] - backdrop[offset + component]) * alphaFactor),
                                0f,
                                1f);
                        }
                    }

                    output.AsSpan().CopyTo(componentOutput.AsSpan(offset, 4));

                    int rgb = _componentToRgb!(output);
                    _target.SetPixel(x, y, new SKColor(
                        (byte)((rgb >> 16) & 0xFF),
                        (byte)((rgb >> 8) & 0xFF),
                        (byte)(rgb & 0xFF),
                        groupAlpha));
                }
            }
        }

        private static int GetRgbDistanceSquared(int expected, int actual)
        {
            int red = ((expected >> 16) & 0xFF) - ((actual >> 16) & 0xFF);
            int green = ((expected >> 8) & 0xFF) - ((actual >> 8) & 0xFF);
            int blue = (expected & 0xFF) - (actual & 0xFF);
            return (red * red) + (green * green) + (blue * blue);
        }

        public void Dispose()
        {
            _objectColorCanvas.Dispose();
            _objectAlphaCanvas.Dispose();
            _objectShapeCanvas.Dispose();
            _objectColor.Dispose();
            _objectAlpha.Dispose();
            _objectShape.Dispose();
            _backdrop.Dispose();
        }

        private static SKBitmap CreateBitmap(int width, int height)
        {
            return new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        }

        private static void CopyBitmap(SKBitmap source, SKBitmap target)
        {
            using SKCanvas canvas = new(target);
            using SKPaint paint = new() { BlendMode = SKBlendMode.Src };
            canvas.DrawBitmap(source, 0, 0, SkiaRenderingBackend.ImageSamplingOptions, paint);
        }

        private static SKColor InterpolatePremultiplied(SKColor previous, SKColor current, float shape)
        {
            float previousAlpha = previous.Alpha / 255f;
            float currentAlpha = current.Alpha / 255f;
            float inverseShape = 1f - shape;
            float alpha = (shape * currentAlpha) + (inverseShape * previousAlpha);
            if (alpha <= 0f)
            {
                return SKColors.Transparent;
            }

            byte red = ToByte(((shape * current.Red * currentAlpha) +
                (inverseShape * previous.Red * previousAlpha)) / alpha / 255f);
            byte green = ToByte(((shape * current.Green * currentAlpha) +
                (inverseShape * previous.Green * previousAlpha)) / alpha / 255f);
            byte blue = ToByte(((shape * current.Blue * currentAlpha) +
                (inverseShape * previous.Blue * previousAlpha)) / alpha / 255f);
            return new SKColor(red, green, blue, ToByte(alpha));
        }

        private static byte RemoveBackdrop(byte group, byte backdrop, float alphaFactor)
        {
            return (byte)Math.Clamp((int)MathF.Round(group + ((group - backdrop) * alphaFactor)), 0, 255);
        }

        private static byte ToByte(float value)
        {
            return (byte)Math.Clamp((int)MathF.Round(Math.Clamp(value, 0f, 1f) * 255f), 0, 255);
        }
    }

    private sealed class TransparencyGroup : IDisposable
    {
        private readonly BufferedImage? _image;
        private readonly PDRectangle? _bbox;
        private readonly Rectangle2D _bounds = new();
        private readonly ComponentSource? _componentSource;

        public TransparencyGroup(
            SkiaPageDrawerPeer drawer,
            PDTransparencyGroup form,
            bool isSoftMask,
            Matrix ctm,
            PDColor? backdropColor)
        {
            if (drawer._graphics?.GetSkiaCanvas() is not SKCanvas parentCanvas ||
                drawer._graphics.GetSkiaBitmapSize() is not { } targetSize ||
                form.GetBBox() is not PDRectangle formBBox ||
                formBBox.GetWidth() <= 0 || formBBox.GetHeight() <= 0)
            {
                return;
            }

            Matrix transform = Matrix.Concatenate(ctm, form.GetMatrix());
            SKPoint[] localPoints =
            [
                ToPoint(drawer.PdfToCanvas(formBBox.GetLowerLeftX(), formBBox.GetLowerLeftY(), transform)),
                ToPoint(drawer.PdfToCanvas(formBBox.GetUpperRightX(), formBBox.GetLowerLeftY(), transform)),
                ToPoint(drawer.PdfToCanvas(formBBox.GetUpperRightX(), formBBox.GetUpperRightY(), transform)),
                ToPoint(drawer.PdfToCanvas(formBBox.GetLowerLeftX(), formBBox.GetUpperRightY(), transform)),
            ];
            float localLeft = localPoints.Min(static point => point.X);
            float localTop = localPoints.Min(static point => point.Y);
            float localRight = localPoints.Max(static point => point.X);
            float localBottom = localPoints.Max(static point => point.Y);
            _bbox = new PDRectangle(localLeft, localTop, localRight - localLeft, localBottom - localTop);

            SKMatrix deviceMatrix = parentCanvas.TotalMatrix;
            SKPoint[] devicePoints = localPoints.Select(point => MapPoint(deviceMatrix, point)).ToArray();
            int left = Math.Max(0, (int)MathF.Floor(devicePoints.Min(static point => point.X)));
            int top = Math.Max(0, (int)MathF.Floor(devicePoints.Min(static point => point.Y)));
            int right = Math.Min(targetSize.Width, (int)MathF.Ceiling(devicePoints.Max(static point => point.X)));
            int bottom = Math.Min(targetSize.Height, (int)MathF.Ceiling(devicePoints.Max(static point => point.Y)));
            if (right <= left || bottom <= top)
            {
                return;
            }

            _bounds = new Rectangle2D(left, top, right - left, bottom - top);
            _image = new BufferedImage(targetSize.Width, targetSize.Height, BufferedImage.TYPE_INT_ARGB);
            _image.Clear(Color.Transparent);
            using Graphics2D groupGraphics = _image.CreateGraphics();
            SKCanvas groupCanvas = groupGraphics.GetSkiaCanvas()!;

            if (isSoftMask && backdropColor is not null)
            {
                int rgb = backdropColor.ToRGB();
                using SKPathBuilder backdropBuilder = new();
                backdropBuilder.MoveTo(devicePoints[0].X, devicePoints[0].Y);
                backdropBuilder.LineTo(devicePoints[1].X, devicePoints[1].Y);
                backdropBuilder.LineTo(devicePoints[2].X, devicePoints[2].Y);
                backdropBuilder.LineTo(devicePoints[3].X, devicePoints[3].Y);
                backdropBuilder.Close();
                using SKPath backdropPath = backdropBuilder.Detach();
                using SKPaint backdropPaint = new()
                {
                    Color = new SKColor(
                        (byte)((rgb >> 16) & 0xFF),
                        (byte)((rgb >> 8) & 0xFF),
                        (byte)(rgb & 0xFF),
                        255),
                    BlendMode = SKBlendMode.Src,
                    Style = SKPaintStyle.Fill,
                };
                groupCanvas.DrawPath(backdropPath, backdropPaint);
            }

            groupCanvas.SetMatrix(deviceMatrix);
            PDTransparencyGroupAttributes? groupAttributes = form.GetGroup();
            using ITransparencyGroupCompositor? groupCompositor = CreateGroupCompositor(
                drawer,
                form,
                groupGraphics,
                groupAttributes,
                isSoftMask,
                _bounds);
            drawer.RenderTransparencyGroupContent(form, groupGraphics, isSoftMask, ctm, groupCompositor);
            _componentSource = groupCompositor?.GetComponentSource();
        }

        public Rectangle2D GetBounds() => _bounds;

        public PDRectangle? GetBBox() => _bbox;

        public BufferedImage? GetImage() => _image;

        public ComponentSource? GetComponentSource() => _componentSource;

        public void Dispose() => _image?.Dispose();

        private static ITransparencyGroupCompositor? CreateGroupCompositor(
            SkiaPageDrawerPeer drawer,
            PDTransparencyGroup form,
            Graphics2D groupGraphics,
            PDTransparencyGroupAttributes? attributes,
            bool isSoftMask,
            Rectangle2D bounds)
        {
            if (isSoftMask || attributes is null)
            {
                return null;
            }

            bool hasBlendMode = drawer.HasBlendMode(form, []);
            bool requiresIccComponentBlending = hasBlendMode && drawer.HasIccSourceColorSpace(form, []);
            if (attributes.IsKnockout())
            {
                PDColorSpace? blendColorSpace = attributes.GetColorSpace(form.GetResources());
                Func<float[], int>? componentToRgb = blendColorSpace is PDDeviceCMYK &&
                    (requiresIccComponentBlending || drawer._transparencyGroupCompositor?.UsesComponents == true)
                    ? components => drawer.SafeToRgb(new PDColor(components, blendColorSpace), 0)
                    : null;
                return new KnockoutGroupCompositor(
                    groupGraphics,
                    drawer._graphics,
                    attributes.IsIsolated(),
                    bounds,
                    knockout: true,
                    componentToRgb: componentToRgb,
                    sourceToComponents: CreateSourceToComponents(drawer, componentToRgb));
            }

            PDColorSpace? groupColorSpace = attributes.GetColorSpace(form.GetResources());
            bool requiresDeviceCmykComponentBlending =
                requiresIccComponentBlending ||
                drawer._transparencyGroupCompositor?.UsesComponents == true ||
                (attributes.IsIsolated() && HasDirectBlendMode(form));
            if (groupColorSpace is PDDeviceCMYK && requiresDeviceCmykComponentBlending)
            {
                Func<float[], int> componentToRgb = components =>
                    drawer.SafeToRgb(new PDColor(components, groupColorSpace), 0);
                return new KnockoutGroupCompositor(
                    groupGraphics,
                    drawer._graphics,
                    attributes.IsIsolated(),
                    bounds,
                    knockout: false,
                    componentToRgb: componentToRgb,
                    sourceToComponents: CreateSourceToComponents(drawer, componentToRgb));
            }

            if (!attributes.IsIsolated() &&
                drawer._graphics is not null &&
                drawer._graphics.GetSkiaBitmap() is not null &&
                hasBlendMode)
            {
                return new NonIsolatedGroupCompositor(groupGraphics, drawer._graphics, bounds);
            }

            return null;
        }

        private static Func<PDColor, float[]?>? CreateSourceToComponents(
            SkiaPageDrawerPeer drawer,
            Func<float[], int>? componentToRgb)
        {
            if (componentToRgb is null)
            {
                return null;
            }

            return color =>
            {
                PDColorManagementContext? context = drawer.GetColorManagementContext();
                return context is not null && context.TryConvertToOutput(color, out float[] output) &&
                    output.Length == 4
                    ? output
                    : null;
            };
        }

        private static SKPoint ToPoint((float x, float y) point) => new(point.x, point.y);

        private static SKPoint MapPoint(SKMatrix matrix, SKPoint point)
        {
            float denominator = (matrix.Persp0 * point.X) + (matrix.Persp1 * point.Y) + matrix.Persp2;
            if (denominator == 0)
            {
                denominator = 1;
            }

            return new SKPoint(
                ((matrix.ScaleX * point.X) + (matrix.SkewX * point.Y) + matrix.TransX) / denominator,
                ((matrix.SkewY * point.X) + (matrix.ScaleY * point.Y) + matrix.TransY) / denominator);
        }
    }
}
