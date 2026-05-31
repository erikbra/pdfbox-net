/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/flagbitspane/FontFlag.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 */

namespace PdfBox.Net.Debugger.Flagbitspane;

public sealed class FontFlag : BitFlagBase
{
    public FontFlag()
        : base("Font descriptor flags",
        ["FixedPitch", "Serif", "Symbolic", "Script", "Nonsymbolic", "Italic", "AllCap", "SmallCap", "ForceBold"],
        [1, 2, 3, 4, 6, 7, 17, 18, 19])
    {
    }
}
