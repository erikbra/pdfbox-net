/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted CIDSystemInfo wrapper for CID fonts.
 *
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Font;

public sealed class PDCIDSystemInfo
{
    private static readonly COSName RegistryKey = COSName.GetPDFName("Registry");
    private static readonly COSName OrderingKey = COSName.GetPDFName("Ordering");
    private static readonly COSName SupplementKey = COSName.GetPDFName("Supplement");

    private readonly COSDictionary _dictionary;

    public PDCIDSystemInfo(COSDictionary dictionary)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    public string Registry => _dictionary.GetString(RegistryKey, string.Empty);
    public string Ordering => _dictionary.GetString(OrderingKey, string.Empty);
    public int Supplement => _dictionary.GetInt(SupplementKey, 0);

    public COSDictionary GetCOSObject() => _dictionary;
}
