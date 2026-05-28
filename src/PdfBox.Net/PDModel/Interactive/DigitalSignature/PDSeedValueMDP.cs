/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/digitalsignature/PDSeedValueMDP.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature;

public class PDSeedValueMDP
{
    private readonly COSDictionary _dictionary;

    public PDSeedValueMDP()
    {
        _dictionary = new COSDictionary();
        _dictionary.SetDirect(true);
    }

    public PDSeedValueMDP(COSDictionary dictionary)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
        _dictionary.SetDirect(true);
    }

    public COSDictionary GetCOSObject() => _dictionary;

    public int GetP() => _dictionary.GetInt(COSName.P);

    public void SetP(int p)
    {
        if (p < 0 || p > 3)
        {
            throw new ArgumentException("Only values between 0 and 3 are allowed.", nameof(p));
        }

        _dictionary.SetInt(COSName.P, p);
    }
}
