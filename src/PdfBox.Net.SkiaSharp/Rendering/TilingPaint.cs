/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/rendering/TilingPaint.java
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

using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.Patterns;
using PdfBox.Net.Util;
using SkiaSharp;

namespace PdfBox.Net.Rendering;

internal class TilingPaint : IContextPaint
{
    private const int MAXEDGE = 3000;
    private readonly TexturePaint _paint;
    private readonly Matrix _patternMatrix;

    internal TilingPaint(PageDrawer drawer, PDTilingPattern pattern, AffineTransform xform)
        : this(GetSkiaPeer(drawer), pattern, xform)
    {
    }

    internal TilingPaint(SkiaPageDrawerPeer drawer, PDTilingPattern pattern, AffineTransform xform)
        : this(drawer, pattern, null, null, xform)
    {
    }

    internal TilingPaint(SkiaPageDrawerPeer drawer, PDTilingPattern pattern, PDColorSpace? colorSpace, PDColor? color, AffineTransform xform)
    {
        _patternMatrix = Matrix.Concatenate(drawer.GetInitialMatrix(), pattern.GetMatrix());
        Rectangle2D anchorRect = GetAnchorRect(pattern);
        _paint = new TexturePaint(GetImage(drawer, pattern, colorSpace, color, xform, anchorRect, _patternMatrix), anchorRect);
    }

    internal TexturePaint TexturePaint => _paint;

    public PaintContext CreateContext(ColorModel cm, Rectangle deviceBounds, Rectangle2D userBounds, AffineTransform xform, RenderingHints hints)
    {
        AffineTransform xformPattern = xform.Clone();

        AffineTransform patternNoScale = _patternMatrix.CreateAffineTransform();
        float scaleX = _patternMatrix.GetScalingFactorX();
        float scaleY = _patternMatrix.GetScalingFactorY();
        if (scaleX != 0 && scaleY != 0)
        {
            patternNoScale.Scale(1 / scaleX, 1 / scaleY);
        }

        xformPattern.Concatenate(patternNoScale);
        return _paint.CreateContext(cm, deviceBounds, userBounds, xformPattern, hints);
    }

    public int GetTransparency()
    {
        return Transparency.TRANSLUCENT;
    }

    private static int Ceiling(double num)
    {
        return (int)Math.Ceiling(num);
    }

    private Rectangle2D GetAnchorRect(PDTilingPattern pattern)
    {
        PDRectangle bbox = pattern.GetBBox() ?? throw new IOException("Pattern /BBox is missing");
        float xStep = pattern.GetXStep();
        if (xStep == 0)
        {
            xStep = bbox.GetWidth();
        }

        float yStep = pattern.GetYStep();
        if (yStep == 0)
        {
            yStep = bbox.GetHeight();
        }

        float xScale = _patternMatrix.GetScalingFactorX();
        float yScale = _patternMatrix.GetScalingFactorY();
        float width = xStep * xScale;
        float height = yStep * yScale;

        if (Math.Abs(width * height) > MAXEDGE * MAXEDGE)
        {
            width = Math.Min(MAXEDGE, Math.Abs(width)) * Math.Sign(width);
            height = Math.Min(MAXEDGE, Math.Abs(height)) * Math.Sign(height);
        }

        return new Rectangle2D(
            bbox.GetLowerLeftX() * xScale,
            bbox.GetLowerLeftY() * yScale,
            width,
            height);
    }

    private static BufferedImage GetImage(SkiaPageDrawerPeer drawer, PDTilingPattern pattern, PDColorSpace? colorSpace, PDColor? color, AffineTransform xform, Rectangle2D anchorRect, Matrix patternMatrixForScale)
    {
        float width = (float)Math.Abs(anchorRect.Width);
        float height = (float)Math.Abs(anchorRect.Height);

        Matrix xformMatrix = new(xform);
        float xScale = Math.Abs(xformMatrix.GetScalingFactorX());
        float yScale = Math.Abs(xformMatrix.GetScalingFactorY());
        width *= xScale == 0 ? 1 : xScale;
        height *= yScale == 0 ? 1 : yScale;

        int rasterWidth = Math.Max(1, Ceiling(width));
        int rasterHeight = Math.Max(1, Ceiling(height));
        BufferedImage image = new(rasterWidth, rasterHeight, BufferedImage.TYPE_INT_ARGB);
        image.GetSkiaBitmap().Erase(SKColors.Transparent);

        using Graphics2D graphics = image.CreateGraphics();
        if (pattern.GetYStep() < 0)
        {
            graphics.Translate(0, rasterHeight);
            graphics.Scale(1, -1);
        }

        if (pattern.GetXStep() < 0)
        {
            graphics.Translate(rasterWidth, 0);
            graphics.Scale(-1, 1);
        }

        graphics.Scale(xScale == 0 ? 1 : xScale, yScale == 0 ? 1 : yScale);

        Matrix patternMatrix = Matrix.GetScaleInstance(
            Math.Abs(patternMatrixForScale.GetScalingFactorX()),
            Math.Abs(patternMatrixForScale.GetScalingFactorY()));

        PDRectangle bbox = pattern.GetBBox() ?? new PDRectangle(0, 0, rasterWidth, rasterHeight);
        patternMatrix = patternMatrix.Translate(-bbox.GetLowerLeftX(), -bbox.GetLowerLeftY());
        drawer.DrawTilingPattern(graphics, pattern, colorSpace, color, patternMatrix);

        return image;
    }

    private static SkiaPageDrawerPeer GetSkiaPeer(PageDrawer drawer)
    {
        return drawer.Peer as SkiaPageDrawerPeer
            ?? throw new InvalidOperationException("The supplied PageDrawer is not backed by the SkiaSharp rendering backend.");
    }
}
