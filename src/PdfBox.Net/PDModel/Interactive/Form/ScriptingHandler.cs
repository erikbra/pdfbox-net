/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/form/ScriptingHandler.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.PDModel.Interactive.Action;

namespace PdfBox.Net.PDModel.Interactive.Form;

public interface ScriptingHandler
{
    string Keyboard(PDActionJavaScript javaScriptAction, string value);
    string Format(PDActionJavaScript javaScriptAction, string value);
    bool Validate(PDActionJavaScript javaScriptAction, string value);
    string Calculate(PDActionJavaScript javaScriptAction, string value);
}
