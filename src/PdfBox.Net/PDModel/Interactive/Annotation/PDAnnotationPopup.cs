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

public sealed partial class PDAnnotationPopup : PDAnnotation
{
    private static ILogger<PDAnnotationPopup> LOG => PdfBoxLogging.CreateLogger<PDAnnotationPopup>();

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
        if (dictionary is null)
        {
            return null;
        }

        try
        {
            PDAnnotation annotation = CreateAnnotation(dictionary);
            if (annotation is PDAnnotationMarkup markup)
            {
                return markup;
            }

            LOG.LogError("parent annotation is of type {AnnotationType} but should be of type PDAnnotationMarkup",
                annotation.GetType().Name);
        }
        catch (IOException ex)
        {
            LOG.LogDebug(ex, "An exception while trying to get the parent markup - ignoring");
        }

        return null;
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

}
