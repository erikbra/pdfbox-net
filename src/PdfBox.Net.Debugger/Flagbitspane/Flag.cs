/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/flagbitspane/Flag.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 */

namespace PdfBox.Net.Debugger.Flagbitspane;

public interface IFlag
{
    string GetPdfName();
    string[] GetFlags();
    bool[] GetValues(int flagValue);
}

public abstract class BitFlagBase : IFlag
{
    private readonly string _pdfName;
    private readonly string[] _flags;
    private readonly int[] _bitPositions;

    protected BitFlagBase(string pdfName, string[] flags, int[] bitPositions)
    {
        _pdfName = pdfName;
        _flags = flags;
        _bitPositions = bitPositions;
    }

    public string GetPdfName() => _pdfName;

    public string[] GetFlags() => _flags;

    public virtual bool[] GetValues(int flagValue)
    {
        bool[] values = new bool[_flags.Length];
        for (int i = 0; i < _bitPositions.Length && i < values.Length; i++)
        {
            values[i] = (flagValue & (1 << (_bitPositions[i] - 1))) != 0;
        }
        return values;
    }
}
