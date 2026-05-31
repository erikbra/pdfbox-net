/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/streampane/tooltip/ToolTip.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 */

namespace PdfBox.Net.Debugger.Streampane.Tooltip;

/// <summary>Base adapted tooltip model for stream inspection.</summary>
public class ToolTip
{
    public ToolTip(string title, string? content = null)
    {
        Title = title;
        Content = content;
    }

    public string Title { get; }

    public string? Content { get; }
}
