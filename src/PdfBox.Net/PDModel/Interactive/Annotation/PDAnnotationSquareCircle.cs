/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationSquareCircle.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Color;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public abstract partial class PDAnnotationSquareCircle : PDAnnotationMarkup
{
    protected PDAnnotationSquareCircle()
    {
    }

    protected PDAnnotationSquareCircle(COSDictionary dictionary)
        : base(dictionary)
    {
    }

    public override PDBorderStyleDictionary? GetBorderStyle()
    {
        COSDictionary? dictionary = GetCOSDictionary().GetCOSDictionary(COSName.GetPDFName("BS"));
        return dictionary != null ? new PDBorderStyleDictionary(dictionary) : null;
    }

    public override void SetBorderStyle(PDBorderStyleDictionary? borderStyle)
    {
        GetCOSDictionary().SetItem(COSName.GetPDFName("BS"), borderStyle);
    }

    public PDBorderEffectDictionary? GetBorderEffect()
    {
        COSDictionary? dictionary = GetCOSDictionary().GetCOSDictionary(COSName.GetPDFName("BE"));
        return dictionary != null ? new PDBorderEffectDictionary(dictionary) : null;
    }

    public void SetBorderEffect(PDBorderEffectDictionary? borderEffect)
    {
        GetCOSDictionary().SetItem(COSName.GetPDFName("BE"), borderEffect);
    }

    public override PDColor? GetInteriorColor()
    {
        COSArray? c = GetCOSDictionary().GetCOSArray(COSName.GetPDFName("IC"));
        if (c == null)
        {
            return null;
        }

        return c.Size() switch
        {
            1 => new PDColor(c, PDDeviceGray.Instance),
            3 => new PDColor(c, PDDeviceRGB.Instance),
            4 => new PDColor(c, PDDeviceCMYK.Instance),
            _ => null
        };
    }

    public override void SetInteriorColor(PDColor? color)
    {
        GetCOSDictionary().SetItem(COSName.GetPDFName("IC"), color?.ToCOSArray());
    }

    public PDRectangle? GetRectDifference()
    {
        COSArray? array = GetCOSDictionary().GetCOSArray(COSName.GetPDFName("RD"));
        return array != null ? new PDRectangle(array) : null;
    }

    public void SetRectDifference(PDRectangle? rd)
    {
        GetCOSDictionary().SetItem(COSName.GetPDFName("RD"), rd?.GetCOSArray());
    }
}
