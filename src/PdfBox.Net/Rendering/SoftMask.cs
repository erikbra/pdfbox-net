/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/rendering/SoftMask.java
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

using PdfBox.Net.PDModel.Common.Function;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.Util;

namespace PdfBox.Net.Rendering;

internal class SoftMask : IPaint
{
    private readonly IPaint _paint;
    private readonly BufferedImage _mask;
    private readonly Rectangle2D _bboxDevice;
    private readonly PDFunction? _transferFunction;
    private readonly int _backdropComponent;

    internal SoftMask(IPaint paint, BufferedImage mask, Rectangle2D bboxDevice, PDColor? backdropColor, PDFunction? transferFunction)
    {
        _paint = paint;
        _mask = mask;
        _bboxDevice = bboxDevice;
        _transferFunction = transferFunction is PDFunctionTypeIdentity ? null : transferFunction;
        _backdropComponent = backdropColor is null ? 0 : 0;
    }

    public PaintContext CreateContext(ColorModel cm, Rectangle deviceBounds, Rectangle2D userBounds, AffineTransform xform, RenderingHints hints)
    {
        return new SoftPaintContext();
    }

    public int GetTransparency()
    {
        return Transparency.TRANSLUCENT;
    }

    private sealed class SoftPaintContext : PaintContext
    {
        public void Dispose()
        {
        }

        public ColorModel GetColorModel()
        {
            return new ColorModel();
        }

        public Raster GetRaster(int x, int y, int width, int height)
        {
            return new WritableRaster(width, height);
        }
    }
}
