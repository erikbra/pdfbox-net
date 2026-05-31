/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/flagbitspane/AnnotFlag.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 */

namespace PdfBox.Net.Debugger.Flagbitspane;

public sealed class AnnotFlag : BitFlagBase
{
    public AnnotFlag()
        : base("Annotation flags",
        ["Print", "NoZoom", "NoRotate", "NoView", "ReadOnly", "Locked", "ToggleNoView", "LockedContents", "Invisible", "Hidden"],
        [3, 4, 5, 6, 7, 8, 9, 10, 1, 2])
    {
    }
}
