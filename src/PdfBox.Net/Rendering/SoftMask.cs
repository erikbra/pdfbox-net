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
        _backdropComponent = GetBackdropGray(backdropColor);
    }

    public PaintContext CreateContext(ColorModel cm, Rectangle deviceBounds, Rectangle2D userBounds, AffineTransform xform, RenderingHints hints)
    {
        return new SoftPaintContext(
            CreatePaintContext(_paint, cm, deviceBounds, userBounds, xform, hints),
            _mask,
            _bboxDevice,
            _transferFunction,
            _backdropComponent);
    }

    public int GetTransparency()
    {
        return Transparency.TRANSLUCENT;
    }

    private static PaintContext CreatePaintContext(IPaint paint, ColorModel cm, Rectangle deviceBounds, Rectangle2D userBounds, AffineTransform xform, RenderingHints hints)
    {
        if (paint is IContextPaint contextPaint)
        {
            return contextPaint.CreateContext(cm, deviceBounds, userBounds, xform, hints);
        }

        return Color.Transparent.CreateContext(cm, deviceBounds, userBounds, xform, hints);
    }

    private static int GetBackdropGray(PDColor? backdropColor)
    {
        if (backdropColor is null)
        {
            return 0;
        }

        try
        {
            int rgb = backdropColor.ToRGB();
            int r = (rgb >> 16) & 0xFF;
            int g = (rgb >> 8) & 0xFF;
            int b = rgb & 0xFF;
            return ((299 * r) + (587 * g) + (114 * b)) / 1000;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    private sealed class SoftPaintContext : PaintContext
    {
        private readonly PaintContext _context;
        private readonly BufferedImage _mask;
        private readonly Rectangle2D _bboxDevice;
        private readonly PDFunction? _transferFunction;
        private readonly int _backdropComponent;

        internal SoftPaintContext(PaintContext context, BufferedImage mask, Rectangle2D bboxDevice, PDFunction? transferFunction, int backdropComponent)
        {
            _context = context;
            _mask = mask;
            _bboxDevice = bboxDevice;
            _transferFunction = transferFunction;
            _backdropComponent = backdropComponent;
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public ColorModel GetColorModel()
        {
            return new ColorModel();
        }

        public Raster GetRaster(int x, int y, int width, int height)
        {
            Raster contextRaster = _context.GetRaster(x, y, width, height);
            ColorModel contextColorModel = _context.GetColorModel();
            WritableRaster outputRaster = GetColorModel().CreateCompatibleWritableRaster(width, height);
            WritableRaster maskRaster = _mask.GetRaster();

            int maskOffsetX = x - (int)_bboxDevice.X;
            int maskOffsetY = y - (int)_bboxDevice.Y;
            float[] input = new float[1];
            float?[] transferMap = new float?[256];
            int[] gray = new int[4];
            int[] output = new int[4];
            object? pixelInput = null;

            for (int py = 0; py < height; py++)
            {
                for (int px = 0; px < width; px++)
                {
                    pixelInput = contextRaster.GetDataElements(px, py, pixelInput);
                    output[0] = contextColorModel.GetRed(pixelInput);
                    output[1] = contextColorModel.GetGreen(pixelInput);
                    output[2] = contextColorModel.GetBlue(pixelInput);
                    output[3] = contextColorModel.GetAlpha(pixelInput);

                    int mx = maskOffsetX + px;
                    int my = maskOffsetY + py;
                    if (mx >= 0 && my >= 0 && mx < maskRaster.Width && my < maskRaster.Height)
                    {
                        maskRaster.GetPixel(mx, my, gray);
                        int g = Math.Clamp(gray[0], 0, 255);
                        output[3] = ApplyMaskAlpha(output[3], g, input, transferMap);
                    }
                    else
                    {
                        output[3] = (int)MathF.Round(output[3] * (_backdropComponent / 255f));
                    }

                    outputRaster.SetPixel(px, py, output);
                }
            }

            return outputRaster;
        }

        private int ApplyMaskAlpha(int alpha, int gray, float[] input, float?[] transferMap)
        {
            if (_transferFunction is null)
            {
                return (int)MathF.Round(alpha * (gray / 255f));
            }

            try
            {
                float factor;
                if (transferMap[gray].HasValue)
                {
                    factor = transferMap[gray]!.Value;
                }
                else
                {
                    input[0] = gray / 255f;
                    factor = _transferFunction.Eval(input)[0];
                    transferMap[gray] = factor;
                }

                return (int)MathF.Round(alpha * factor);
            }
            catch (Exception)
            {
                return (int)MathF.Round(alpha * (_backdropComponent / 255f));
            }
        }
    }
}
