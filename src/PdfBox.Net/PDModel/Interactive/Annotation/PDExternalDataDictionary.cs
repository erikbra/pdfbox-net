/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDExternalDataDictionary.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed class PDExternalDataDictionary : COSObjectable
{
    private readonly COSDictionary dictionary;

    public PDExternalDataDictionary()
        : this(new COSDictionary())
    {
    }

    public PDExternalDataDictionary(COSDictionary dictionary)
    {
        this.dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
        this.dictionary.SetItem(COSName.TYPE, COSName.GetPDFName("ExData"));
    }

    public string? GetSubtype()
    {
        return dictionary.GetNameAsString(COSName.SUBTYPE);
    }

    public void SetSubtype(string? subtype)
    {
        dictionary.SetName(COSName.SUBTYPE, subtype);
    }

    public COSBase GetCOSObject()
    {
        return dictionary;
    }
}
