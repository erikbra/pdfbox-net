/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: tools/src/main/java/org/apache/pdfbox/tools/PDFBox.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

namespace PdfBox.Net.Tools;

public static class PDFBox
{
    public static int Run(string[] args, TextWriter? output = null, TextWriter? error = null)
    {
        output ??= Console.Out;
        error ??= Console.Error;
        args ??= [];

        if (args.Length == 0 || string.Equals(args[0], "version", StringComparison.OrdinalIgnoreCase))
        {
            output.WriteLine(Version.GetVersion() ?? "unknown");
            return 0;
        }

        error.WriteLine($"Unknown command: {args[0]}");
        return 1;
    }
}
