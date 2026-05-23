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

using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.Patterns;
using PdfBox.Net.Util;

namespace PdfBox.Net.Rendering;

internal class TilingPaint : IPaint
{
    private const int MAXEDGE = 3000;
    private readonly IPaint _paint;
    private readonly Matrix _patternMatrix;

    internal TilingPaint(PageDrawer drawer, PDTilingPattern pattern, AffineTransform xform)
        : this(drawer, pattern, null, null, xform)
    {
    }

    internal TilingPaint(PageDrawer drawer, PDTilingPattern pattern, PDColorSpace? colorSpace, PDColor? color, AffineTransform xform)
    {
        _patternMatrix = drawer.GetInitialMatrix();
        _paint = new TexturePaint(GetImage(drawer, pattern, colorSpace, color, xform, GetAnchorRect(pattern)), GetAnchorRect(pattern));
    }

    public PaintContext CreateContext(ColorModel cm, Rectangle deviceBounds, Rectangle2D userBounds, AffineTransform xform, RenderingHints hints)
    {
        throw new NotImplementedException("TODO: requires AWT equivalent");
    }

    public int GetTransparency()
    {
        return Transparency.TRANSLUCENT;
    }

    private static int Ceiling(double num)
    {
        return (int)Math.Ceiling(num);
    }

    private static Rectangle2D GetAnchorRect(PDTilingPattern pattern)
    {
        return new Rectangle2D(0, 0, Math.Min(MAXEDGE, 1), Math.Min(MAXEDGE, 1));
    }

    private static BufferedImage GetImage(PageDrawer drawer, PDTilingPattern pattern, PDColorSpace? colorSpace, PDColor? color, AffineTransform xform, Rectangle2D anchorRect)
    {
        throw new NotImplementedException("TODO: requires AWT equivalent");
    }
}
