/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger-app/pom.xml
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 */

using PdfBox.Net;
using PdfBox.Net.Debugger;

if (args.Length == 0)
{
    System.Console.WriteLine("Usage: pdfdebugger <file.pdf> [password]");
    return 1;
}

using PdfBox.Net.PDModel.PDDocument doc = Loader.LoadPDF(args[0], args.Length > 1 ? args[1] : null);
PDFDebugger.InspectDocument(doc, System.Console.Out);
return 0;
