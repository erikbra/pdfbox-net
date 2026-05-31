/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/flagbitspane/FlagBitsPane.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 */

namespace PdfBox.Net.Debugger.Flagbitspane;

/// <summary>Provides flag bit analysis for PDF integer values.</summary>
public static class FlagBitsPane
{
    public static System.Collections.Generic.IEnumerable<(string Name, bool Value)> GetFlags(IFlag flag, int value)
    {
        string[] names = flag.GetFlags();
        bool[] values = flag.GetValues(value);
        for (int i = 0; i < names.Length; i++)
        {
            yield return (names[i], i < values.Length && values[i]);
        }
    }
}
