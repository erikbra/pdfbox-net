/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Backend SPI for PDF printing implementations.
 *
 * PORT_MODE: native-adapter
 */

using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.Rendering;

namespace PdfBox.Net.Printing;

public interface IPDFPrintBackend
{
    string Name { get; }

    bool IsSupported { get; }

    void Print(PDFPrintJob job);
}

public static class PrintingBackend
{
    private static IPDFPrintBackend? _current;

    public static bool IsRegistered => _current is not null;

    public static IPDFPrintBackend Current => _current ?? UnsupportedPDFPrintBackend.Instance;

    public static void Register(IPDFPrintBackend backend)
    {
        _current = backend ?? throw new ArgumentNullException(nameof(backend));
    }
}

public sealed class PDFPrintJob
{
    internal PDFPrintJob(
        PDDocument document,
        PDFRenderer renderer,
        string? printerName,
        bool printToFile,
        string? printFileName,
        Scaling scaling,
        bool showPageBorder,
        float dpi,
        bool center)
    {
        Document = document;
        Renderer = renderer;
        PrinterName = printerName;
        PrintToFile = printToFile;
        PrintFileName = printFileName;
        Scaling = scaling;
        ShowPageBorder = showPageBorder;
        Dpi = dpi;
        Center = center;
    }

    public PDDocument Document { get; }

    public PDFRenderer Renderer { get; }

    public string? PrinterName { get; }

    public bool PrintToFile { get; }

    public string? PrintFileName { get; }

    public Scaling Scaling { get; }

    public bool ShowPageBorder { get; }

    public float Dpi { get; }

    public bool Center { get; }

    public int NumberOfPages => Document.GetNumberOfPages();

    public PDPage GetPage(int pageIndex) => Document.GetPage(pageIndex);

    public BufferedImage RenderPageImage(int pageIndex, float scale, ImageType imageType = ImageType.RGB)
    {
        return Renderer.RenderImage(pageIndex, scale, imageType, RenderDestination.PRINT);
    }

    public PDRectangle GetRotatedCropBox(int pageIndex)
    {
        return PDFPrintable.GetRotatedCropBox(GetPage(pageIndex));
    }
}

internal sealed class UnsupportedPDFPrintBackend : IPDFPrintBackend
{
    internal static UnsupportedPDFPrintBackend Instance { get; } = new();

    private UnsupportedPDFPrintBackend()
    {
    }

    public string Name => "Unsupported";

    public bool IsSupported => false;

    public void Print(PDFPrintJob job)
    {
        throw new PlatformNotSupportedException(
            "No PdfBox.Net print backend is registered. Reference an optional print backend package, " +
            "such as PdfBox.Net.SystemDrawing, and call its registration method before printing.");
    }
}
