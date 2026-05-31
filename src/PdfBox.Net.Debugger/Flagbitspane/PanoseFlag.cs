/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/flagbitspane/PanoseFlag.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 */

namespace PdfBox.Net.Debugger.Flagbitspane;

public sealed class PanoseFlag : IFlag
{
    private static readonly string[] Flags =
    [
        "FamilyKind",
        "SerifStyle",
        "Weight",
        "Proportion",
        "Contrast",
        "StrokeVariation",
        "ArmStyle",
        "Letterform",
        "Midline",
        "XHeight"
    ];

    public string GetPdfName() => "PANOSE classification";

    public string[] GetFlags() => Flags;

    public bool[] GetValues(int flagValue)
    {
        bool[] values = new bool[Flags.Length];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = (flagValue & (1 << i)) != 0;
        }
        return values;
    }
}
