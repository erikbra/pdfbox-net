/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: tools/src/main/java/org/apache/pdfbox/tools/ExtractText.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.PDModel;
using PdfBox.Net.Text;

namespace PdfBox.Net.Tools;

public static class ExtractText
{
    public static string GetText(string inputPath, string? password = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        using PDDocument document = Loader.LoadPDF(inputPath, password);
        return new PDFTextStripper().GetText(document);
    }

    public static void WriteText(string inputPath, string outputPath, string? password = null)
    {
        string text = GetText(inputPath, password);
        File.WriteAllText(outputPath, text);
    }
}
