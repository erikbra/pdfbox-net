/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/ui/ArrayEntry.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 */

namespace PdfBox.Net.Debugger.Ui;

/// <summary>Represents an abstract view of an array item in the tree view.</summary>
public class ArrayEntry
{
    public int Index { get; set; }

    public PdfBox.Net.COS.COSBase? Value { get; set; }

    public PdfBox.Net.COS.COSBase? Item { get; set; }

    public int GetIndex() => Index;

    public void SetIndex(int index) => Index = index;

    public PdfBox.Net.COS.COSBase? GetValue() => Value;

    public void SetValue(PdfBox.Net.COS.COSBase? value) => Value = value;

    public PdfBox.Net.COS.COSBase? GetItem() => Item;

    public void SetItem(PdfBox.Net.COS.COSBase? item) => Item = item;

    public override string ToString() => $"[{Index}]";
}
