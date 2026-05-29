/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: tools/src/main/java/org/apache/pdfbox/tools/PDFMerger.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.MultiPdf;

namespace PdfBox.Net.Tools;

public static class PDFMerger
{
    public static void Merge(string destinationFileName, params string[] sourceFiles)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationFileName);
        ArgumentNullException.ThrowIfNull(sourceFiles);

        PDFMergerUtility merger = new()
        {
            DestinationFileName = destinationFileName,
        };

        foreach (string source in sourceFiles)
        {
            merger.AddSource(source);
        }

        merger.MergeDocuments();
    }
}
