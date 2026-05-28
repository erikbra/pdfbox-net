/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/measurement/PDViewportDictionary.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Interactive.Measurement;

public class PDViewportDictionary : COSObjectable
{
    public const string TYPE = "Viewport";

    private readonly COSDictionary _dictionary;

    public PDViewportDictionary()
    {
        _dictionary = new COSDictionary();
    }

    public PDViewportDictionary(COSDictionary dictionary)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    public COSBase GetCOSObject() => _dictionary;

    public string GetViewportType() => TYPE;

    public PDRectangle? GetBBox()
    {
        COSArray? bbox = _dictionary.GetCOSArray(COSName.BBOX);
        return bbox != null ? new PDRectangle(bbox) : null;
    }

    public void SetBBox(PDRectangle rectangle) => _dictionary.SetItem(COSName.BBOX, rectangle);

    public string? GetName() => _dictionary.GetNameAsString(COSName.NAME);
    public void SetName(string? name) => _dictionary.SetName(COSName.NAME, name);

    public PDMeasureDictionary? GetMeasure()
    {
        COSDictionary? measure = _dictionary.GetCOSDictionary(COSName.GetPDFName("Measure"));
        return measure != null ? new PDMeasureDictionary(measure) : null;
    }

    public void SetMeasure(PDMeasureDictionary? measure) => _dictionary.SetItem(COSName.GetPDFName("Measure"), measure);
}
