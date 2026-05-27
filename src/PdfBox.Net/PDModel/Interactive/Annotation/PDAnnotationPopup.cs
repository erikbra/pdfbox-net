/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationPopup.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed class PDAnnotationPopup : PDAnnotation
{
    public const string SUB_TYPE = "Popup";

    public PDAnnotationPopup()
    {
        GetCOSDictionary().SetName(COSName.SUBTYPE, SUB_TYPE);
    }

    public PDAnnotationPopup(COSDictionary dictionary)
        : base(dictionary)
    {
    }

    public PDAnnotationMarkup? GetParent()
    {
        COSDictionary? dictionary = GetCOSDictionary().GetCOSDictionary(COSName.PARENT);
        return dictionary != null ? new PDAnnotationMarkupImpl(dictionary) : null;
    }

    public void SetParent(PDAnnotationMarkup? annotation)
    {
        GetCOSDictionary().SetItem(COSName.PARENT, annotation);
    }

    public bool GetOpen()
    {
        return GetCOSDictionary().GetBoolean(COSName.GetPDFName("Open"), false);
    }

    public void SetOpen(bool open)
    {
        GetCOSDictionary().SetBoolean(COSName.GetPDFName("Open"), open);
    }

    private sealed class PDAnnotationMarkupImpl : PDAnnotationMarkup
    {
        public PDAnnotationMarkupImpl(COSDictionary dictionary)
            : base(dictionary)
        {
        }
    }
}
