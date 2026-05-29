/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: tools/src/main/java/org/apache/pdfbox/tools/PDFText2HTML.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using System.Net;
using System.Text;

namespace PdfBox.Net.Tools;

public static class PDFText2HTML
{
    public static string ConvertText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        StringBuilder sb = new();
        sb.AppendLine("<html><body><pre>");
        sb.Append(WebUtility.HtmlEncode(text));
        sb.AppendLine("</pre></body></html>");
        return sb.ToString();
    }

    public static void ConvertFile(string inputPath, string outputPath, string? password = null)
    {
        string text = ExtractText.GetText(inputPath, password);
        File.WriteAllText(outputPath, ConvertText(text));
    }
}
