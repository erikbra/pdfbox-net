/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/ui/DocumentEntry.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 */

namespace PdfBox.Net.Debugger.Ui;

/// <summary>Represents an abstract view of a document in the tree view.</summary>
public class DocumentEntry
{
    public DocumentEntry(PdfBox.Net.PDModel.PDDocument doc, string filename)
    {
        Doc = doc;
        Filename = filename;
    }

    public PdfBox.Net.PDModel.PDDocument Doc { get; }

    public string Filename { get; }

    public int GetPageCount() => Doc.GetNumberOfPages();

    public PageEntry GetPage(int index)
    {
        PdfBox.Net.PDModel.PDPage page = Doc.GetPage(index);
        string? pageLabel = GetPageLabel(Doc, index);
        PdfBox.Net.COS.COSDictionary dict = page.GetCOSObject() as PdfBox.Net.COS.COSDictionary ?? new PdfBox.Net.COS.COSDictionary();
        return new PageEntry(dict, index + 1, pageLabel);
    }

    public int IndexOf(PageEntry page) => page.PageNum - 1;

    public override string ToString() => Filename;

    public static string? GetPageLabel(PdfBox.Net.PDModel.PDDocument doc, int pageIndex)
    {
        string[]? labels = doc.GetDocumentCatalog().GetPageLabels()?.GetLabelsByPageIndices();
        return labels is not null && pageIndex >= 0 && pageIndex < labels.Length ? labels[pageIndex] : null;
    }
}
