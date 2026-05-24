/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationLine.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed class PDAnnotationLine : PDAnnotationMarkup
{
    public const string SUB_TYPE = "Line";

    public PDAnnotationLine()
    {
        GetCOSDictionary().SetName(COSName.SUBTYPE, SUB_TYPE);
    }

    public PDAnnotationLine(COSDictionary dict)
        : base(dict)
    {
    }

    public float[]? GetLine()
    {
        COSArray? lineArray = GetCOSDictionary().GetCOSArray(COSName.GetPDFName("L"));
        if (lineArray == null)
        {
            return null;
        }

        float[] values = new float[lineArray.Size()];
        for (int i = 0; i < lineArray.Size(); i++)
        {
            values[i] = lineArray.GetObject(i) is COSNumber number ? number.FloatValue() : 0;
        }
        return values;
    }

    public void SetLine(float[]? line)
    {
        if (line == null)
        {
            GetCOSDictionary().RemoveItem(COSName.GetPDFName("L"));
            return;
        }

        COSArray lineArray = new();
        foreach (float value in line)
        {
            lineArray.Add(new COSFloat(value));
        }
        GetCOSDictionary().SetItem(COSName.GetPDFName("L"), lineArray);
    }
}
