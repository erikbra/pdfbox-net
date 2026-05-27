/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAppearanceEntry.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed class PDAppearanceEntry : COSObjectable
{
    private readonly COSBase _entry;

    public PDAppearanceEntry(COSBase entry)
    {
        _entry = entry ?? throw new ArgumentNullException(nameof(entry));
    }

    public COSBase GetCOSObject() => _entry;

    public bool IsSubDictionary() => _entry is COSDictionary and not COSStream;

    public bool IsStream() => _entry is COSStream;

    public PDAppearanceStream GetAppearanceStream()
    {
        if (_entry is not COSStream stream)
        {
            throw new InvalidOperationException("Appearance entry does not contain a stream.");
        }

        return new PDAppearanceStream(stream);
    }

    public IDictionary<COSName, PDAppearanceStream> GetSubDictionary()
    {
        if (_entry is not COSDictionary dictionary)
        {
            throw new InvalidOperationException("Appearance entry does not contain a sub-dictionary.");
        }

        Dictionary<COSName, PDAppearanceStream> result = new();
        foreach (COSName key in dictionary.KeySet())
        {
            if (dictionary.GetDictionaryObject(key) is COSStream stream)
            {
                result[key] = new PDAppearanceStream(stream);
            }
        }

        return result;
    }
}
