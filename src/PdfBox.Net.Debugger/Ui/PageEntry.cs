/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/ui/PageEntry.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 */

namespace PdfBox.Net.Debugger.Ui;

/// <summary>Represents an abstract view of a page in the tree view.</summary>
public class PageEntry
{
    public PageEntry(PdfBox.Net.COS.COSDictionary dict, int pageNum, string? pageLabel)
    {
        Dict = dict;
        PageNum = pageNum;
        PageLabel = pageLabel;
    }

    public PdfBox.Net.COS.COSDictionary Dict { get; set; }

    public int PageNum { get; set; }

    public string? PageLabel { get; set; }

    public PdfBox.Net.COS.COSDictionary GetDict() => Dict;

    public int GetPageNum() => PageNum;

    public string? GetPageLabel() => PageLabel;

    public string GetPath()
    {
        System.Text.StringBuilder builder = new();
        builder.Append("Root/Pages");

        PdfBox.Net.COS.COSDictionary node = Dict;
        while (node.ContainsKey(PdfBox.Net.COS.COSName.PARENT))
        {
            PdfBox.Net.COS.COSDictionary? parent = node.GetCOSDictionary(PdfBox.Net.COS.COSName.PARENT);
            if (parent is null)
            {
                return string.Empty;
            }

            PdfBox.Net.COS.COSArray? kids = parent.GetCOSArray(PdfBox.Net.COS.COSName.KIDS);
            if (kids is null)
            {
                return string.Empty;
            }

            int index = kids.IndexOfObject(node);
            if (index == -1)
            {
                break;
            }

            builder.Append("/Kids/[").Append(index).Append(']');
            node = parent;
        }

        return builder.ToString();
    }

    public override string ToString() => "Page: " + PageNum + (PageLabel is null ? string.Empty : " - " + PageLabel);
}
