/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationPolyline.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed partial class PDAnnotationPolyline : PDAnnotationMarkup
{
    private PDAppearanceHandler? customAppearanceHandler;

    public const string SUB_TYPE = "PolyLine";

    public PDAnnotationPolyline()
    {
        GetCOSDictionary().SetName(COSName.SUBTYPE, SUB_TYPE);
    }

    public PDAnnotationPolyline(COSDictionary dictionary)
        : base(dictionary)
    {
    }

    public void SetStartPointEndingStyle(string? style)
    {
        string actualStyle = style ?? PDAnnotationLine.LE_NONE;
        COSArray? array = GetCOSDictionary().GetCOSArray(COSName.GetPDFName("LE"));
        if (array == null || array.IsEmpty())
        {
            array = new COSArray();
            array.Add(COSName.GetPDFName(actualStyle));
            array.Add(COSName.GetPDFName(PDAnnotationLine.LE_NONE));
            GetCOSDictionary().SetItem(COSName.GetPDFName("LE"), array);
        }
        else
        {
            array.SetName(0, actualStyle);
        }
    }

    public string GetStartPointEndingStyle()
    {
        COSArray? array = GetCOSDictionary().GetCOSArray(COSName.GetPDFName("LE"));
        return array != null && array.Size() >= 2 ? array.GetName(0, PDAnnotationLine.LE_NONE)! : PDAnnotationLine.LE_NONE;
    }

    public void SetEndPointEndingStyle(string? style)
    {
        string actualStyle = style ?? PDAnnotationLine.LE_NONE;
        COSArray? array = GetCOSDictionary().GetCOSArray(COSName.GetPDFName("LE"));
        if (array == null || array.Size() < 2)
        {
            array = new COSArray();
            array.Add(COSName.GetPDFName(PDAnnotationLine.LE_NONE));
            array.Add(COSName.GetPDFName(actualStyle));
            GetCOSDictionary().SetItem(COSName.GetPDFName("LE"), array);
        }
        else
        {
            array.SetName(1, actualStyle);
        }
    }

    public string GetEndPointEndingStyle()
    {
        COSArray? array = GetCOSDictionary().GetCOSArray(COSName.GetPDFName("LE"));
        return array != null && array.Size() >= 2 ? array.GetName(1, PDAnnotationLine.LE_NONE)! : PDAnnotationLine.LE_NONE;
    }

    public override PDColor? GetInteriorColor()
    {
        return base.GetInteriorColor();
    }

    public override void SetInteriorColor(PDColor? color)
    {
        base.SetInteriorColor(color);
    }

    public float[]? GetVertices()
    {
        COSArray? vertices = GetCOSDictionary().GetCOSArray(COSName.GetPDFName("Vertices"));
        if (vertices == null)
        {
            return null;
        }

        float[] values = new float[vertices.Size()];
        for (int i = 0; i < vertices.Size(); i++)
        {
            values[i] = vertices.GetObject(i) is COSNumber number ? number.FloatValue() : 0;
        }
        return values;
    }

    public void SetVertices(float[]? values)
    {
        if (values == null)
        {
            GetCOSDictionary().RemoveItem(COSName.GetPDFName("Vertices"));
            return;
        }

        COSArray array = new();
        foreach (float value in values)
        {
            array.Add(new COSFloat(value));
        }

        GetCOSDictionary().SetItem(COSName.GetPDFName("Vertices"), array);
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
        customAppearanceHandler ??= new PDPolylineAppearanceHandler(this, document);
        customAppearanceHandler.GenerateAppearanceStreams();
    }
}
