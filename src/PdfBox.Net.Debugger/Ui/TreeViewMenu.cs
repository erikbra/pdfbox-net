/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/ui/TreeViewMenu.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 */

namespace PdfBox.Net.Debugger.Ui;

public sealed class TreeViewMenu
{
    public const string VIEW_PAGES = "pages";
    public const string VIEW_STRUCTURE = "structure";
    public const string VIEW_CROSS_REFERENCE = "cross-reference";

    public static bool IsValidViewMode(string? viewMode)
    {
        return viewMode is VIEW_PAGES or VIEW_STRUCTURE or VIEW_CROSS_REFERENCE;
    }
 }
