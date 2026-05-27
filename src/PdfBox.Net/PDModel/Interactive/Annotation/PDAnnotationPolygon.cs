/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationPolygon.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed class PDAnnotationPolygon : PDAnnotationMarkup
{
    private PDAppearanceHandler? customAppearanceHandler;

    public const string SUB_TYPE = "Polygon";

    public PDAnnotationPolygon()
    {
        GetCOSDictionary().SetName(COSName.SUBTYPE, SUB_TYPE);
    }

    public PDAnnotationPolygon(COSDictionary dictionary)
        : base(dictionary)
    {
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

    public override void ConstructAppearances(PDDocument? document)
    {
        customAppearanceHandler ??= new PDPolygonAppearanceHandler(this, document);
        customAppearanceHandler.GenerateAppearanceStreams();
    }
}
