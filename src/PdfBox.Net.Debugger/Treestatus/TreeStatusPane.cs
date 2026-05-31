/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/treestatus/TreeStatusPane.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 */

namespace PdfBox.Net.Debugger.Treestatus;

/// <summary>Minimal adapted container for the debugger tree status string.</summary>
public sealed class TreeStatusPane
{
    private TreeStatus? _treeStatus;

    public string? StatusString { get; private set; }

    public void SetRoot(object rootNode)
    {
        _treeStatus = new TreeStatus(rootNode);
        StatusString = null;
    }

    public void CapturePath(object[] path)
    {
        StatusString = _treeStatus?.GetStringForPath(path);
    }

    public object[]? RestorePath()
    {
        return StatusString is null ? null : _treeStatus?.GetPathForString(StatusString);
    }
}
