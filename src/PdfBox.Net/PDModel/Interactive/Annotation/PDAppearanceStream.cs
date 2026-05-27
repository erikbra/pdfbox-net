/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAppearanceStream.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Form;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed class PDAppearanceStream : PDFormXObject
{
    public PDAppearanceStream(PDDocument document)
        : base(new PDStream(document))
    {
    }

    public PDAppearanceStream(COSStream stream)
        : base(stream)
    {
    }

    public void SetBBox(PDRectangle? rectangle)
    {
        if (rectangle == null)
        {
            GetCOSObject()?.RemoveItem(COSName.BBOX);
            return;
        }

        GetCOSObject()?.SetItem(COSName.BBOX, rectangle.GetCOSArray());
    }

    public void SetMatrix(float a, float b, float c, float d, float e, float f)
    {
        COSArray matrix = new();
        matrix.Add(new COSFloat(a));
        matrix.Add(new COSFloat(b));
        matrix.Add(new COSFloat(c));
        matrix.Add(new COSFloat(d));
        matrix.Add(new COSFloat(e));
        matrix.Add(new COSFloat(f));
        GetCOSObject()?.SetItem(COSName.GetPDFName("Matrix"), matrix);
    }
}
