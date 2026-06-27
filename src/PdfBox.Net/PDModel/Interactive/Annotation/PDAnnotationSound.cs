/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationSound.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed class PDAnnotationSound : PDAnnotationMarkup
{
    private PDAppearanceHandler? customAppearanceHandler;

    public const string SUB_TYPE = "Sound";

    public PDAnnotationSound()
    {
        GetCOSDictionary().SetName(COSName.SUBTYPE, SUB_TYPE);
    }

    public PDAnnotationSound(COSDictionary dictionary)
        : base(dictionary)
    {
    }

    public COSStream? GetSound()
    {
        return GetCOSDictionary().GetCOSStream(COSName.GetPDFName("Sound"));
    }

    public void SetSound(COSStream? sound)
    {
        GetCOSDictionary().SetItem(COSName.GetPDFName("Sound"), sound);
    }

    public string? GetName()
    {
        return GetCOSDictionary().GetNameAsString(COSName.NAME);
    }

    public void SetName(string? name)
    {
        GetCOSDictionary().SetName(COSName.NAME, name);
    }

    public void SetCustomAppearanceHandler(PDAppearanceHandler? appearanceHandler)
    {
        customAppearanceHandler = appearanceHandler;
    }

    public override void ConstructAppearances()
    {
        ConstructAppearances(null);
    }

    public override void ConstructAppearances(PDDocument? document)
    {
        customAppearanceHandler ??= new PDSoundAppearanceHandler(this, document);
        customAppearanceHandler.GenerateAppearanceStreams();
    }
}
