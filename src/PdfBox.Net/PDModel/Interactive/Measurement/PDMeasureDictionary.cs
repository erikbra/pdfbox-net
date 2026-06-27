/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/measurement/PDMeasureDictionary.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Interactive.Measurement;

public class PDMeasureDictionary : COSObjectable
{
    public const string TYPE = "Measure";

    private readonly COSDictionary _dictionary;

    protected PDMeasureDictionary()
    {
        _dictionary = new COSDictionary();
        _dictionary.SetName(COSName.TYPE, TYPE);
    }

    public PDMeasureDictionary(COSDictionary dictionary)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    public COSBase GetCOSObject() => _dictionary;

    protected COSDictionary Dictionary => _dictionary;

    public string GetMeasureType() => TYPE;

    public string GetTypeName() => TYPE;

    public string GetSubtype() => _dictionary.GetNameAsString(COSName.SUBTYPE, PDRectlinearMeasureDictionary.SUBTYPE) ?? PDRectlinearMeasureDictionary.SUBTYPE;

    protected void SetSubtype(string subtype) => _dictionary.SetName(COSName.SUBTYPE, subtype);
}
