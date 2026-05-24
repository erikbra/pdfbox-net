/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted dictionary encoding resolver for PDF font dictionaries.
 *
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Font.Encoding;

public sealed class DictionaryEncoding : Encoding
{
    private static readonly COSName BaseEncodingKey = COSName.GetPDFName("BaseEncoding");
    private static readonly COSName DifferencesKey = COSName.GetPDFName("Differences");
    private static readonly COSName EncodingKey = COSName.GetPDFName("Encoding");

    public DictionaryEncoding(COSDictionary fontDictionary)
    {
        ArgumentNullException.ThrowIfNull(fontDictionary);

        Encoding baseEncoding = ResolveBaseEncoding(fontDictionary.GetDictionaryObject(EncodingKey));
        foreach (KeyValuePair<int, string> kv in baseEncoding.GetCodeToNameMap())
        {
            AddCharacterEncoding(kv.Key, kv.Value);
        }

        COSBase? encoding = fontDictionary.GetDictionaryObject(EncodingKey);
        if (encoding is COSDictionary encodingDictionary && encodingDictionary.GetCOSArray(DifferencesKey) is COSArray differences)
        {
            ApplyDifferences(differences);
        }
    }

    public static Encoding ResolveEncoding(COSDictionary fontDictionary)
    {
        COSBase? encoding = fontDictionary.GetDictionaryObject(EncodingKey);
        return encoding switch
        {
            COSName name => ResolveNamedEncoding(name.GetName()),
            COSDictionary => new DictionaryEncoding(fontDictionary),
            _ => Standard14Fonts.IsSymbolicFont(fontDictionary.GetNameAsString(COSName.GetPDFName("BaseFont")) ?? string.Empty)
                ? SymbolEncoding.INSTANCE
                : WinAnsiEncoding.INSTANCE,
        };
    }

    private static Encoding ResolveBaseEncoding(COSBase? encoding)
    {
        if (encoding is COSDictionary encodingDictionary)
        {
            if (encodingDictionary.GetDictionaryObject(BaseEncodingKey) is COSName baseName)
            {
                return ResolveNamedEncoding(baseName.GetName());
            }

            return WinAnsiEncoding.INSTANCE;
        }

        if (encoding is COSName namedEncoding)
        {
            return ResolveNamedEncoding(namedEncoding.GetName());
        }

        return WinAnsiEncoding.INSTANCE;
    }

    private static Encoding ResolveNamedEncoding(string name)
    {
        return name switch
        {
            "MacRomanEncoding" => MacRomanEncoding.INSTANCE,
            "WinAnsiEncoding" => WinAnsiEncoding.INSTANCE,
            "SymbolEncoding" => SymbolEncoding.INSTANCE,
            "ZapfDingbatsEncoding" => ZapfDingbatsEncoding.INSTANCE,
            _ => WinAnsiEncoding.INSTANCE,
        };
    }

    private void ApplyDifferences(COSArray differences)
    {
        int currentCode = -1;
        for (int i = 0; i < differences.Size(); i++)
        {
            COSBase? item = differences.GetObject(i);
            if (item is COSNumber number)
            {
                currentCode = number.IntValue();
            }
            else if (item is COSName name && currentCode >= 0)
            {
                AddCharacterEncoding(currentCode, name.GetName());
                currentCode++;
            }
        }
    }
}
