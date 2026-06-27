/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Java-shaped PageDrawer facade backed by a pluggable rendering backend.
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
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Annotations;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.Util;
using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.Rendering;

/// <summary>
/// Java-shaped page drawer facade. Concrete drawing is supplied by the
/// registered rendering backend, e.g. PdfBox.Net.SkiaSharp.
/// </summary>
public partial class PageDrawer : PDFGraphicsStreamEngine, IDisposable
{
    private readonly PageDrawerParameters _parameters;
    private readonly IPageDrawerPeer _peer;
    private readonly GeneralPath _linePath = new();
    private Graphics2D? _graphics;

    public PageDrawer(PageDrawerParameters parameters)
        : base(parameters.GetPage())
    {
        _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        _peer = RenderingBackend.Current.CreatePageDrawerPeer(this, parameters);
    }

    internal IPageDrawerPeer Peer => _peer;

    private PDFGraphicsStreamEngine PeerStreamEngine =>
        _peer as PDFGraphicsStreamEngine
        ?? throw new InvalidOperationException("The registered page drawer backend does not provide a PDF graphics stream engine.");

    public AnnotationFilter GetAnnotationFilter()
    {
        return _peer.GetAnnotationFilter();
    }

    public void SetAnnotationFilter(AnnotationFilter annotationFilter)
    {
        _peer.SetAnnotationFilter(annotationFilter);
    }

    public void DrawPage(Graphics2D graphics, PDRectangle pageSize)
    {
        _graphics = graphics;
        _peer.DrawPage(graphics, pageSize);
    }

    public PDFRenderer GetRenderer() => _parameters.GetRenderer();

    protected Graphics2D? GetGraphics() => _graphics;

    protected GeneralPath GetLinePath() => _linePath;

    public override void AppendRectangle(Point2D p0, Point2D p1, Point2D p2, Point2D p3)
    {
        PeerStreamEngine.AppendRectangle(p0, p1, p2, p3);
    }

    public override void BeginMarkedContentSequence(COSName tag, COSDictionary? properties)
    {
        PeerStreamEngine.BeginMarkedContentSequence(tag, properties);
    }

    public override void BeginText()
    {
        PeerStreamEngine.BeginText();
    }

    public override void Clip(int windingRule)
    {
        PeerStreamEngine.Clip(windingRule);
    }

    public override void ClosePath()
    {
        PeerStreamEngine.ClosePath();
    }

    public override void CurveTo(float x1, float y1, float x2, float y2, float x3, float y3)
    {
        PeerStreamEngine.CurveTo(x1, y1, x2, y2, x3, y3);
    }

    public override void DrawImage(PDImage pdImage)
    {
        PeerStreamEngine.DrawImage(pdImage);
    }

    public override void EndMarkedContentSequence()
    {
        PeerStreamEngine.EndMarkedContentSequence();
    }

    public override void EndPath()
    {
        PeerStreamEngine.EndPath();
    }

    public override void EndText()
    {
        PeerStreamEngine.EndText();
    }

    public override void FillAndStrokePath(int windingRule)
    {
        PeerStreamEngine.FillAndStrokePath(windingRule);
    }

    public override void FillPath(int windingRule)
    {
        PeerStreamEngine.FillPath(windingRule);
    }

    public override Point2D? GetCurrentPoint()
    {
        return PeerStreamEngine.GetCurrentPoint();
    }

    protected virtual int GetSubsampling(PDImage pdImage, AffineTransform at)
    {
        return _peer.GetSubsampling(pdImage, at);
    }

    public override void LineTo(float x, float y)
    {
        PeerStreamEngine.LineTo(x, y);
    }

    public override void MoveTo(float x, float y)
    {
        PeerStreamEngine.MoveTo(x, y);
    }

    public override void ShadingFill(COSName shadingName)
    {
        PeerStreamEngine.ShadingFill(shadingName);
    }

    public virtual void ShowAnnotation(PDAnnotation annotation)
    {
        _peer.ShowAnnotation(annotation);
    }

    public virtual void ShowForm(PDFormXObject form)
    {
        _peer.ShowForm(form);
    }

    public virtual void ShowTransparencyGroup(PDTransparencyGroup form)
    {
        _peer.ShowTransparencyGroup(form);
    }

    protected virtual void ShowTransparencyGroupOnGraphics(PDTransparencyGroup form, Graphics2D graphics)
    {
        _peer.ShowTransparencyGroupOnGraphics(form, graphics);
    }

    public override void StrokePath()
    {
        PeerStreamEngine.StrokePath();
    }

    protected virtual IPaint GetPaint(PDColor color)
    {
        return Color.Black;
    }

    protected virtual IPaint GetNonStrokingPaint()
    {
        return Color.Black;
    }

    protected virtual void SetClip()
    {
    }

    protected virtual void TransferClip(Graphics2D graphics)
    {
    }

    internal void InvokeShowGlyphHook(Matrix textRenderingMatrix, PDFont font, int code, Vector displacement)
    {
        ShowGlyph(textRenderingMatrix, font, code, displacement);
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

    protected virtual void ShowFontGlyph(Matrix textRenderingMatrix, PDFont font, int code, Vector displacement)
    {
        base.ShowGlyph(textRenderingMatrix, font, code, displacement);
    }

    protected virtual void ShowType3Glyph(Matrix textRenderingMatrix, PDType3Font font, int code, Vector displacement)
    {
        base.ShowGlyph(textRenderingMatrix, font, code, displacement);
    }

    public void Dispose()
    {
        _peer.Dispose();
    }
}
