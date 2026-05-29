/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: tools/src/main/java/org/apache/pdfbox/tools/OverlayPDF.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.MultiPdf;

namespace PdfBox.Net.Tools;

public static class OverlayPDF
{
    public static void Apply(string inputFile, string overlayFile, string outputFile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(overlayFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputFile);

        using Overlay overlay = new()
        {
            InputFile = inputFile,
            DefaultOverlayFile = overlayFile,
        };

        using var overlaid = overlay.OverlayDocuments(new Dictionary<int, PdfBox.Net.PDModel.PDDocument>());
        overlaid.Save(outputFile);
    }
}
