/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/printing/PDFPrinter.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
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

using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.Rendering;
using SkiaSharp;
using System.Drawing.Printing;

namespace PdfBox.Net.Printing;

/// <summary>
/// High-level printer helper for PDF documents.
/// <para>
/// Uses <see cref="PrintDocument"/> on Windows. On non-Windows platforms, methods throw
/// <see cref="PlatformNotSupportedException"/> to keep the API available while preserving
/// cross-platform behavior.
/// </para>
/// </summary>
public sealed class PDFPrinter
{
    private readonly PDDocument _document;
    private readonly PDFRenderer _renderer;

    public PDFPrinter(PDDocument document)
        : this(document, new PDFRenderer(document))
    {
    }

    public PDFPrinter(PDDocument document, PDFRenderer renderer)
    {
        _document = document;
        _renderer = renderer;
    }

    public string? PrinterName { get; set; }

    public Scaling Scaling { get; set; } = Scaling.ShrinkToFit;

    public bool ShowPageBorder { get; set; }

    public float Dpi { get; set; } = PDFPrintable.RasterizeOff;

    public bool Center { get; set; } = true;

    public void Print()
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("PDFPrinter.Print() is currently supported on Windows only.");
        }

        using PrintDocument printDocument = new();
        if (!string.IsNullOrWhiteSpace(PrinterName))
        {
            printDocument.PrinterSettings.PrinterName = PrinterName;
        }

        if (!printDocument.PrinterSettings.IsValid)
        {
            throw new InvalidOperationException($"Printer '{printDocument.PrinterSettings.PrinterName}' is not available.");
        }

        int pageIndex = 0;
        printDocument.PrintPage += (_, e) =>
        {
            if (e.Graphics is null)
            {
                throw new InvalidOperationException("Print page graphics context was not provided by the platform.");
            }

            RenderPage(e.Graphics, e.MarginBounds, pageIndex);
            pageIndex++;
            e.HasMorePages = pageIndex < _document.GetNumberOfPages();
        };

        printDocument.Print();
    }

    private void RenderPage(System.Drawing.Graphics graphics, System.Drawing.Rectangle imageableBounds, int pageIndex)
    {
        PDPage page = _document.GetPage(pageIndex);
        PDRectangle cropBox = PDFPrintable.GetRotatedCropBox(page);

        double scale = 1;
        if (Scaling != Scaling.ActualSize)
        {
            double scaleX = imageableBounds.Width / cropBox.GetWidth();
            double scaleY = imageableBounds.Height / cropBox.GetHeight();
            scale = Math.Min(scaleX, scaleY);

            if (scale > 1 && Scaling == Scaling.ShrinkToFit)
            {
                scale = 1;
            }
            if (scale < 1 && Scaling == Scaling.StretchToFit)
            {
                scale = 1;
            }
        }

        float rasterDpi = Dpi;
        if (rasterDpi == PDFPrintable.RasterizeDpiAuto)
        {
            rasterDpi = graphics.DpiX;
        }

        float rasterScale = rasterDpi > 0 ? rasterDpi / 72f : 1f;
        float renderScale = Math.Max((float)(scale * rasterScale), 0.01f);

        using BufferedImage image = _renderer.RenderImage(pageIndex, renderScale, ImageType.RGB, RenderDestination.PRINT);
        using System.Drawing.Image drawingImage = ToDrawingImage(image);

        float drawWidth = (float)(cropBox.GetWidth() * scale);
        float drawHeight = (float)(cropBox.GetHeight() * scale);
        float x = imageableBounds.Left;
        float y = imageableBounds.Top;

        if (Center)
        {
            float dx = (imageableBounds.Width - drawWidth) / 2f;
            float dy = (imageableBounds.Height - drawHeight) / 2f;
            if (dx >= 0 && dy >= 0)
            {
                x += dx;
                y += dy;
            }
        }

        graphics.FillRectangle(System.Drawing.Brushes.White, imageableBounds);
        graphics.DrawImage(drawingImage, x, y, drawWidth, drawHeight);

        if (ShowPageBorder)
        {
            graphics.DrawRectangle(System.Drawing.Pens.Gray, x, y, drawWidth, drawHeight);
        }
    }

    private static System.Drawing.Image ToDrawingImage(BufferedImage image)
    {
        using SKImage skImage = SKImage.FromBitmap(image.Bitmap);
        using SKData data = skImage.Encode(SKEncodedImageFormat.Png, 100);
        using MemoryStream pngStream = new(data.ToArray());
        using System.Drawing.Image decoded = System.Drawing.Image.FromStream(pngStream);
        return new System.Drawing.Bitmap(decoded);
    }
}
