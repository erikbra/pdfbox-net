/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationWidget.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Interactive.Action;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed class PDAnnotationWidget : PDAnnotation
{
    public const string SUB_TYPE = "Widget";

    public PDAnnotationWidget()
    {
        GetCOSDictionary().SetName(COSName.SUBTYPE, SUB_TYPE);
    }

    public PDAnnotationWidget(COSDictionary dict)
        : base(dict)
    {
    }

    public PDAction? GetAction()
    {
        COSDictionary? action = GetCOSDictionary().GetCOSDictionary(COSName.A);
        return action != null ? PDActionFactory.CreateAction(action) : null;
    }

    public void SetAction(PDAction? action)
    {
        GetCOSDictionary().SetItem(COSName.A, action);
    }
}
