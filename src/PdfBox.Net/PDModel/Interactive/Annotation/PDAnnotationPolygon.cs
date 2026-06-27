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
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Interactive.Annotation.Handlers;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed partial class PDAnnotationPolygon : PDAnnotationMarkup
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

    public override PDColor? GetInteriorColor()
    {
        return base.GetInteriorColor();
    }

    public override void SetInteriorColor(PDColor? color)
    {
        base.SetInteriorColor(color);
    }

    public void SetBorderEffect(PDBorderEffectDictionary? borderEffect)
    {
        GetCOSDictionary().SetItem(COSName.BE, borderEffect);
    }

    public PDBorderEffectDictionary? GetBorderEffect()
    {
        return GetCOSDictionary().GetCOSDictionary(COSName.BE) is COSDictionary dictionary
            ? new PDBorderEffectDictionary(dictionary)
            : null;
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

    public float[][]? GetPath()
    {
        COSArray? path = GetCOSDictionary().GetCOSArray(COSName.GetPDFName("Path"));
        if (path == null)
        {
            return null;
        }

        float[][] values = new float[path.Size()][];
        for (int i = 0; i < path.Size(); i++)
        {
            values[i] = path.GetObject(i) is COSArray array ? array.ToFloatArray() : [];
        }
        return values;
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
        customAppearanceHandler ??= new PDPolygonAppearanceHandler(this, document);
        customAppearanceHandler.GenerateAppearanceStreams();
    }
}
