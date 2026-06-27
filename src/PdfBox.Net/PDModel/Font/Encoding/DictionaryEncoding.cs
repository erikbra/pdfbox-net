/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/encoding/DictionaryEncoding.java
 * PDFBOX_SOURCE_COMMIT: b9fd9df12a5655a57c810fd2fa24a76817e19b0c
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: b9fd9df12a5655a57c810fd2fa24a76817e19b0c
 */

/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Font.Encoding;

public sealed class DictionaryEncoding : Encoding
{
    private static readonly COSName BaseEncodingKey = COSName.GetPDFName("BaseEncoding");
    private static readonly COSName DifferencesKey = COSName.GetPDFName("Differences");
    private static readonly COSName EncodingKey = COSName.GetPDFName("Encoding");

    private readonly COSBase? _encoding;
    private readonly Encoding? _baseEncoding;
    private readonly Dictionary<int, string> _differences = new();
    private readonly IReadOnlyDictionary<int, string> _readOnlyDifferences;

    public DictionaryEncoding(COSDictionary fontDictionary)
    {
        ArgumentNullException.ThrowIfNull(fontDictionary);
        _readOnlyDifferences = _differences;

        _encoding = fontDictionary.GetDictionaryObject(EncodingKey);
        Encoding baseEncoding = ResolveBaseEncoding(_encoding);
        _baseEncoding = baseEncoding;
        foreach (KeyValuePair<int, string> kv in baseEncoding.GetCodeToNameMap())
        {
            AddCharacterEncoding(kv.Key, kv.Value);
        }

        if (_encoding is COSDictionary encodingDictionary && encodingDictionary.GetCOSArray(DifferencesKey) is COSArray differences)
        {
            ApplyDifferences(differences);
        }
    }

    public DictionaryEncoding(COSName baseEncoding, COSArray differences)
        : this(CreateEncodingDictionary(baseEncoding, differences), true, null)
    {
    }

    public DictionaryEncoding(COSDictionary encodingDictionary, bool isNonSymbolic, Encoding? builtIn)
    {
        ArgumentNullException.ThrowIfNull(encodingDictionary);
        _readOnlyDifferences = _differences;
        _encoding = encodingDictionary;

        Encoding? baseEncoding = null;
        if (encodingDictionary.GetDictionaryObject(BaseEncodingKey) is COSName baseName)
        {
            baseEncoding = ResolveNamedEncodingOrNull(baseName.GetName());
        }

        baseEncoding ??= isNonSymbolic
            ? StandardEncoding.INSTANCE
            : builtIn ?? new Encoding();
        _baseEncoding = baseEncoding;

        foreach (KeyValuePair<int, string> kv in baseEncoding.GetCodeToNameMap())
        {
            AddCharacterEncoding(kv.Key, kv.Value);
        }

        if (encodingDictionary.GetCOSArray(DifferencesKey) is COSArray differences)
        {
            ApplyDifferences(differences);
        }
    }

    public Encoding? GetBaseEncoding() => _baseEncoding;

    public IReadOnlyDictionary<int, string> GetDifferences() => _readOnlyDifferences;

    public override COSBase? GetCOSObject() => _encoding;

    public override string GetEncodingName()
    {
        return _baseEncoding == null
            ? "differences"
            : $"{_baseEncoding.GetEncodingName()} with differences";
    }

    public static Encoding ResolveEncoding(COSDictionary fontDictionary)
    {
        COSBase? encoding = fontDictionary.GetDictionaryObject(EncodingKey);
        return encoding switch
        {
            COSName name => ResolveNamedEncoding(name.GetName()),
            COSDictionary => new DictionaryEncoding(fontDictionary),
            _ => ResolveStandard14FallbackEncoding(fontDictionary.GetNameAsString(COSName.GetPDFName("BaseFont"))),
        };
    }

    internal static Encoding ResolveStandard14FallbackEncoding(string? fontName)
    {
        string? mappedName = Standard14Fonts.GetMappedFontName(fontName);
        return mappedName switch
        {
            "Symbol" => SymbolEncoding.INSTANCE,
            "ZapfDingbats" => ZapfDingbatsEncoding.INSTANCE,
            _ => WinAnsiEncoding.INSTANCE,
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
            "MacOSRomanEncoding" => MacOSRomanEncoding.INSTANCE,
            "MacExpertEncoding" => MacExpertEncoding.INSTANCE,
            "StandardEncoding" => StandardEncoding.INSTANCE,
            "WinAnsiEncoding" => WinAnsiEncoding.INSTANCE,
            "SymbolEncoding" => SymbolEncoding.INSTANCE,
            "ZapfDingbatsEncoding" => ZapfDingbatsEncoding.INSTANCE,
            _ => WinAnsiEncoding.INSTANCE,
        };
    }

    private static Encoding? ResolveNamedEncodingOrNull(string name)
    {
        return name switch
        {
            "MacRomanEncoding" => MacRomanEncoding.INSTANCE,
            "MacOSRomanEncoding" => MacOSRomanEncoding.INSTANCE,
            "MacExpertEncoding" => MacExpertEncoding.INSTANCE,
            "StandardEncoding" => StandardEncoding.INSTANCE,
            "WinAnsiEncoding" => WinAnsiEncoding.INSTANCE,
            "SymbolEncoding" => SymbolEncoding.INSTANCE,
            "ZapfDingbatsEncoding" => ZapfDingbatsEncoding.INSTANCE,
            _ => null,
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
                _differences[currentCode] = name.GetName();
                currentCode++;
            }
        }
    }

    private static COSDictionary CreateEncodingDictionary(COSName baseEncoding, COSArray differences)
    {
        ArgumentNullException.ThrowIfNull(baseEncoding);
        ArgumentNullException.ThrowIfNull(differences);
        COSDictionary dictionary = new();
        dictionary.SetItem(BaseEncodingKey, baseEncoding);
        dictionary.SetItem(DifferencesKey, differences);
        return dictionary;
    }
}
