/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/hexviewer/HexModel.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 */

namespace PdfBox.Net.Debugger.Hexviewer;

public sealed class HexModel
{
    private readonly System.Collections.Generic.List<byte> _bytes = new();

    public int Count => _bytes.Count;

    public byte this[int index] => _bytes[index];

    public void SetBytes(System.Collections.Generic.IEnumerable<byte> data)
    {
        _bytes.Clear();
        _bytes.AddRange(data);
    }

    public byte[] ToArray() => _bytes.ToArray();
}
