/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * System.Drawing.Printing backend for PDFPrinter.
 *
 * PORT_MODE: native-adapter
 */

using System.Drawing.Printing;
using System.Runtime.Versioning;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.Rendering;
using DrawingBrushes = System.Drawing.Brushes;
using DrawingGraphics = System.Drawing.Graphics;
using DrawingImage = System.Drawing.Image;
using DrawingPens = System.Drawing.Pens;
using DrawingRectangle = System.Drawing.Rectangle;

namespace PdfBox.Net.Printing;

[SupportedOSPlatform("windows")]
public sealed class SystemDrawingPrintBackend : IPDFPrintBackend
{
    public static SystemDrawingPrintBackend Instance { get; } = new();

    private SystemDrawingPrintBackend()
    {
    }

    public string Name => "System.Drawing.Printing";

    public bool IsSupported => OperatingSystem.IsWindows();

    public static void Register()
    {
        PrintingBackend.Register(Instance);
    }

    public void Print(PDFPrintJob job)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("SystemDrawingPrintBackend is supported on Windows only.");
        }

        using PrintDocument printDocument = new();
        if (!string.IsNullOrWhiteSpace(job.PrinterName))
        {
            printDocument.PrinterSettings.PrinterName = job.PrinterName;
        }

        if (job.PrintToFile)
        {
            printDocument.PrinterSettings.PrintToFile = true;
            printDocument.PrinterSettings.PrintFileName = job.PrintFileName!;
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

            RenderPage(job, e.Graphics, e.MarginBounds, pageIndex);
            pageIndex++;
            e.HasMorePages = pageIndex < job.NumberOfPages;
        };

        printDocument.Print();
    }

    private static void RenderPage(PDFPrintJob job, DrawingGraphics graphics, DrawingRectangle imageableBounds, int pageIndex)
    {
        PDRectangle cropBox = job.GetRotatedCropBox(pageIndex);

        double scale = 1;
        if (job.Scaling != Scaling.ActualSize)
        {
            double scaleX = imageableBounds.Width / cropBox.GetWidth();
            double scaleY = imageableBounds.Height / cropBox.GetHeight();
            scale = Math.Min(scaleX, scaleY);

            if (scale > 1 && job.Scaling == Scaling.ShrinkToFit)
            {
                scale = 1;
            }
            if (scale < 1 && job.Scaling == Scaling.StretchToFit)
            {
                scale = 1;
            }
        }

        float rasterDpi = job.Dpi;
        if (rasterDpi == PDFPrintable.RasterizeDpiAuto)
        {
            rasterDpi = graphics.DpiX;
        }

        float rasterScale = rasterDpi > 0 ? rasterDpi / 72f : 1f;
        float renderScale = Math.Max((float)(scale * rasterScale), 0.01f);

        using BufferedImage image = job.RenderPageImage(pageIndex, renderScale);
        using DrawingImage drawingImage = ToDrawingImage(image);

        float drawWidth = (float)(cropBox.GetWidth() * scale);
        float drawHeight = (float)(cropBox.GetHeight() * scale);
        float x = imageableBounds.Left;
        float y = imageableBounds.Top;

        if (job.Center)
        {
            float dx = (imageableBounds.Width - drawWidth) / 2f;
            float dy = (imageableBounds.Height - drawHeight) / 2f;
            if (dx >= 0 && dy >= 0)
            {
                x += dx;
                y += dy;
            }
        }

        graphics.FillRectangle(DrawingBrushes.White, imageableBounds);
        graphics.DrawImage(drawingImage, x, y, drawWidth, drawHeight);

        if (job.ShowPageBorder)
        {
            graphics.DrawRectangle(DrawingPens.Gray, x, y, drawWidth, drawHeight);
        }
    }

    private static DrawingImage ToDrawingImage(BufferedImage image)
    {
        byte[] data = RenderingBackend.Current.ImageCodec.Encode(image, EncodedImageFormat.Png, 100);
        using MemoryStream pngStream = new(data);
        using DrawingImage decoded = DrawingImage.FromStream(pngStream);
        return new System.Drawing.Bitmap(decoded);
    }
}
