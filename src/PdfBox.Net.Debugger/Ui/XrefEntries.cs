/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/ui/XrefEntries.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 */

namespace PdfBox.Net.Debugger.Ui;

/// <summary>Represents an abstract view of the cross references of a PDF.</summary>
public class XrefEntries
{
    public static readonly string PATH = "CRT";

    private readonly System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<PdfBox.Net.COS.COSObjectKey, long>> _entries;
    private readonly PdfBox.Net.COS.COSDocument _document;

    public XrefEntries(PdfBox.Net.PDModel.PDDocument document)
    {
        System.Collections.Generic.Dictionary<PdfBox.Net.COS.COSObjectKey, long> xrefTable = document.GetDocument().GetXrefTable();
        _entries = xrefTable.OrderBy(static entry => entry.Key.GetNumber()).ToList();
        _document = document.GetDocument();
    }

    public int GetXrefEntryCount() => _entries.Count;

    public XrefEntry GetXrefEntry(int index)
    {
        System.Collections.Generic.KeyValuePair<PdfBox.Net.COS.COSObjectKey, long> entry = _entries[index];
        PdfBox.Net.COS.COSObject? objectFromPool = _document.GetObjectFromPool(entry.Key);
        return new XrefEntry(index, entry.Key, entry.Value, objectFromPool);
    }

    public int IndexOf(XrefEntry xrefEntry)
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            if (_entries[i].Key.Equals(xrefEntry.GetKey()))
            {
                return i;
            }
        }

        return 0;
    }

    public override string ToString() => PATH;
}
