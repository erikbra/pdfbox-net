/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationCaret.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed class PDAnnotationCaret : PDAnnotationMarkup
{
    private PDAppearanceHandler? customAppearanceHandler;

    public const string SUB_TYPE = "Caret";

    public PDAnnotationCaret()
    {
        GetCOSDictionary().SetName(COSName.SUBTYPE, SUB_TYPE);
    }

    public PDAnnotationCaret(COSDictionary dictionary)
        : base(dictionary)
    {
    }

    public string? GetSymbol()
    {
        return GetCOSDictionary().GetNameAsString(COSName.GetPDFName("Sy"));
    }

    public void SetSymbol(string? symbol)
    {
        GetCOSDictionary().SetName(COSName.GetPDFName("Sy"), symbol);
    }

    public void SetCustomAppearanceHandler(PDAppearanceHandler? appearanceHandler)
    {
        customAppearanceHandler = appearanceHandler;
    }

    public override void ConstructAppearances(PDDocument? document)
    {
        customAppearanceHandler ??= new PDCaretAppearanceHandler(this, document);
        customAppearanceHandler.GenerateAppearanceStreams();
    }
}
