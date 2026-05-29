/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: tools/src/main/java/org/apache/pdfbox/tools/PDFSplit.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.MultiPdf;
using PdfBox.Net.PDModel;

namespace PdfBox.Net.Tools;

public static class PDFSplit
{
    public static IReadOnlyList<string> Split(string inputFileName, string outputDirectory, int splitAtPage = 1)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputFileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        using PDDocument source = Loader.LoadPDF(inputFileName);
        Splitter splitter = new() { SplitAtPage = splitAtPage };
        IList<PDDocument> parts = splitter.Split(source);

        List<string> paths = new(parts.Count);
        for (int i = 0; i < parts.Count; i++)
        {
            using PDDocument part = parts[i];
            string path = Path.Combine(outputDirectory, $"split-{i + 1}.pdf");
            part.Save(path);
            paths.Add(path);
        }

        return paths;
    }
}
