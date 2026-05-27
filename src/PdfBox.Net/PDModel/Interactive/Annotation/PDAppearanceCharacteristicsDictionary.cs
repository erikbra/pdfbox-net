/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAppearanceCharacteristicsDictionary.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed class PDAppearanceCharacteristicsDictionary : COSObjectable
{
    private readonly COSDictionary dictionary;

    public PDAppearanceCharacteristicsDictionary()
        : this(new COSDictionary())
    {
    }

    public PDAppearanceCharacteristicsDictionary(COSDictionary dictionary)
    {
        this.dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    public int GetRotation()
    {
        return dictionary.GetInt(COSName.GetPDFName("R"), 0);
    }

    public void SetRotation(int rotation)
    {
        dictionary.SetInt(COSName.GetPDFName("R"), rotation);
    }

    public string? GetNormalCaption()
    {
        return dictionary.GetString(COSName.GetPDFName("CA"));
    }

    public void SetNormalCaption(string? caption)
    {
        dictionary.SetString(COSName.GetPDFName("CA"), caption);
    }

    public string? GetRolloverCaption()
    {
        return dictionary.GetString(COSName.GetPDFName("RC"));
    }

    public void SetRolloverCaption(string? caption)
    {
        dictionary.SetString(COSName.GetPDFName("RC"), caption);
    }

    public string? GetAlternateCaption()
    {
        return dictionary.GetString(COSName.GetPDFName("AC"));
    }

    public void SetAlternateCaption(string? caption)
    {
        dictionary.SetString(COSName.GetPDFName("AC"), caption);
    }

    public COSBase GetCOSObject()
    {
        return dictionary;
    }
}
