/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/ui/XrefEntry.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 */

namespace PdfBox.Net.Debugger.Ui;

/// <summary>Represents an abstract view of a cross-reference entry in the tree view.</summary>
public class XrefEntry
{
    public XrefEntry(int index, PdfBox.Net.COS.COSObjectKey? key, long offset, PdfBox.Net.COS.COSObject? cosObject)
    {
        Index = index;
        Key = key;
        Offset = offset;
        CosObject = cosObject;
    }

    public int Index { get; set; }

    public PdfBox.Net.COS.COSObjectKey? Key { get; set; }

    public long Offset { get; set; }

    public PdfBox.Net.COS.COSObject? CosObject { get; set; }

    public PdfBox.Net.COS.COSObjectKey? GetKey() => Key;

    public int GetIndex() => Index;

    public PdfBox.Net.COS.COSObject? GetCOSObject() => CosObject;

    public PdfBox.Net.COS.COSBase? GetObject() => CosObject?.GetObject();

    public string GetPath() => XrefEntries.PATH + "/" + ToString();

    public override string ToString()
    {
        if (Key is null)
        {
            return "(null)";
        }

        return Offset >= 0 ? $"Offset: {Offset} [{Key}]" : $"Compressed object stream: {-Offset} [{Key}]";
    }
}
