/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/digitalsignature/PDSeedValueTimeStamp.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature;

public class PDSeedValueTimeStamp
{
    private readonly COSDictionary _dictionary;

    public PDSeedValueTimeStamp()
    {
        _dictionary = new COSDictionary();
        _dictionary.SetDirect(true);
    }

    public PDSeedValueTimeStamp(COSDictionary dictionary)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
        _dictionary.SetDirect(true);
    }

    public COSDictionary GetCOSObject() => _dictionary;

    public string GetURL() => _dictionary.GetString(COSName.GetPDFName("URL"), string.Empty);
    public void SetURL(string? url) => _dictionary.SetString(COSName.GetPDFName("URL"), url);

    public bool IsTimestampRequired() => _dictionary.GetInt(COSName.GetPDFName("FF"), 0) != 0;
    public void SetTimestampRequired(bool flag) => _dictionary.SetInt(COSName.GetPDFName("FF"), flag ? 1 : 0);
}
