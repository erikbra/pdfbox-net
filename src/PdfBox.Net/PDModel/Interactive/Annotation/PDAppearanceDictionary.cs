/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAppearanceDictionary.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed class PDAppearanceDictionary : COSObjectable
{
    private readonly COSDictionary _dictionary;

    public PDAppearanceDictionary()
        : this(new COSDictionary())
    {
    }

    public PDAppearanceDictionary(COSDictionary dictionary)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    public COSBase GetCOSObject() => _dictionary;

    public PDAppearanceEntry? GetNormalAppearance() => GetEntry(COSName.N);

    public void SetNormalAppearance(PDAppearanceStream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        _dictionary.SetItem(COSName.N, stream.GetCOSObject());
    }

    public void SetNormalAppearance(PDAppearanceEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _dictionary.SetItem(COSName.N, entry.GetCOSObject());
    }

    public PDAppearanceEntry? GetRolloverAppearance() => GetEntry(COSName.R);

    public void SetRolloverAppearance(PDAppearanceEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _dictionary.SetItem(COSName.R, entry.GetCOSObject());
    }

    public PDAppearanceEntry? GetDownAppearance() => GetEntry(COSName.D);

    public void SetDownAppearance(PDAppearanceEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _dictionary.SetItem(COSName.D, entry.GetCOSObject());
    }

    private PDAppearanceEntry? GetEntry(COSName name)
    {
        COSBase? entry = _dictionary.GetDictionaryObject(name);
        return entry != null ? new PDAppearanceEntry(entry) : null;
    }
}
