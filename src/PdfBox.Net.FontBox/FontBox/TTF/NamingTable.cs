/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/NamingTable.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: trunk
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

using TextEncoding = System.Text.Encoding;

namespace PdfBox.Net.FontBox.TTF;

/// <summary>
/// This 'name'-table is a required table in a TrueType font.
/// </summary>
public sealed class NamingTable : TTFTable
{
    public const string TAG = "name";

    private List<NameRecord> _nameRecords = [];
    private Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, string?>>>> _lookupTable = [];
    private string? _fontFamily;
    private string? _fontSubFamily;
    private string? _psName;

    public NamingTable() : base(TAG)
    {
    }

    internal override void Read(TrueTypeFont ttf, TTFDataStream data)
    {
        Read(ttf, data, false);
        initialized = true;
    }

    internal override void ReadHeaders(TrueTypeFont ttf, TTFDataStream data, FontHeaders outHeaders)
    {
        Read(ttf, data, true);
        outHeaders.SetName(_psName);
        outHeaders.SetFontFamily(_fontFamily, _fontSubFamily);
    }

    private void Read(TrueTypeFont ttf, TTFDataStream data, bool onlyHeaders)
    {
        _ = data.ReadUnsignedShort();
        int numberOfNameRecords = data.ReadUnsignedShort();
        _ = data.ReadUnsignedShort();
        _nameRecords = new List<NameRecord>(numberOfNameRecords);
        for (int i = 0; i < numberOfNameRecords; i++)
        {
            NameRecord nr = new();
            nr.InitData(ttf, data);
            if (!onlyHeaders || IsUsefulForOnlyHeaders(nr))
            {
                _nameRecords.Add(nr);
            }
        }

        foreach (NameRecord nr in _nameRecords)
        {
            if (nr.StringOffset > Length)
            {
                nr.String = null;
                continue;
            }

            data.Seek(Offset + (2L * 3) + numberOfNameRecords * 2L * 6 + nr.StringOffset);
            TextEncoding charset = GetCharset(nr);
            nr.String = data.ReadString(nr.StringLength, charset);
        }

        _lookupTable = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, string?>>>>(_nameRecords.Count);
        FillLookupTable();
        ReadInterestingStrings();
    }

    private static TextEncoding GetCharset(NameRecord nr)
    {
        int platform = nr.PlatformId;
        int encoding = nr.PlatformEncodingId;
        TextEncoding charset = TextEncoding.Latin1;
        if (platform == NameRecord.PLATFORM_WINDOWS &&
            (encoding == NameRecord.ENCODING_WINDOWS_SYMBOL || encoding == NameRecord.ENCODING_WINDOWS_UNICODE_BMP))
        {
            charset = TextEncoding.BigEndianUnicode;
        }
        else if (platform == NameRecord.PLATFORM_UNICODE)
        {
            charset = TextEncoding.BigEndianUnicode;
        }
        else if (platform == NameRecord.PLATFORM_ISO)
        {
            switch (encoding)
            {
                case 0:
                    charset = TextEncoding.ASCII;
                    break;
                case 1:
                    charset = TextEncoding.BigEndianUnicode;
                    break;
            }
        }

        return charset;
    }

    private void FillLookupTable()
    {
        foreach (NameRecord nr in _nameRecords)
        {
            if (!_lookupTable.TryGetValue(nr.NameId, out var platformLookup))
            {
                platformLookup = [];
                _lookupTable[nr.NameId] = platformLookup;
            }

            if (!platformLookup.TryGetValue(nr.PlatformId, out var encodingLookup))
            {
                encodingLookup = [];
                platformLookup[nr.PlatformId] = encodingLookup;
            }

            if (!encodingLookup.TryGetValue(nr.PlatformEncodingId, out var languageLookup))
            {
                languageLookup = new Dictionary<int, string?>(1);
                encodingLookup[nr.PlatformEncodingId] = languageLookup;
            }

            languageLookup[nr.LanguageId] = nr.String;
        }
    }

    private void ReadInterestingStrings()
    {
        _fontFamily = GetEnglishName(NameRecord.NAME_FONT_FAMILY_NAME);
        _fontSubFamily = GetEnglishName(NameRecord.NAME_FONT_SUB_FAMILY_NAME);

        _psName = GetName(NameRecord.NAME_POSTSCRIPT_NAME,
                NameRecord.PLATFORM_MACINTOSH,
                NameRecord.ENCODING_MACINTOSH_ROMAN,
                NameRecord.LANGUAGE_MACINTOSH_ENGLISH);
        if (_psName == null)
        {
            _psName = GetName(NameRecord.NAME_POSTSCRIPT_NAME,
                    NameRecord.PLATFORM_WINDOWS,
                    NameRecord.ENCODING_WINDOWS_UNICODE_BMP,
                    NameRecord.LANGUAGE_WINDOWS_EN_US);
        }
        if (_psName != null)
        {
            _psName = _psName.Trim();
        }
    }

    private static bool IsUsefulForOnlyHeaders(NameRecord nr)
    {
        int nameId = nr.NameId;
        if (nameId == NameRecord.NAME_POSTSCRIPT_NAME ||
            nameId == NameRecord.NAME_FONT_FAMILY_NAME ||
            nameId == NameRecord.NAME_FONT_SUB_FAMILY_NAME)
        {
            int languageId = nr.LanguageId;
            return languageId == NameRecord.LANGUAGE_UNICODE ||
                   languageId == NameRecord.LANGUAGE_WINDOWS_EN_US;
        }

        return false;
    }

    private string? GetEnglishName(int nameId)
    {
        for (int i = 4; i >= 0; i--)
        {
            string? nameUni = GetName(nameId, NameRecord.PLATFORM_UNICODE, i, NameRecord.LANGUAGE_UNICODE);
            if (nameUni != null)
            {
                return nameUni;
            }
        }

        string? nameWin = GetName(nameId,
                NameRecord.PLATFORM_WINDOWS,
                NameRecord.ENCODING_WINDOWS_UNICODE_BMP,
                NameRecord.LANGUAGE_WINDOWS_EN_US);
        if (nameWin != null)
        {
            return nameWin;
        }

        return GetName(nameId,
                NameRecord.PLATFORM_MACINTOSH,
                NameRecord.ENCODING_MACINTOSH_ROMAN,
                NameRecord.LANGUAGE_MACINTOSH_ENGLISH);
    }

    public string? GetName(int nameId, int platformId, int encodingId, int languageId)
    {
        if (!_lookupTable.TryGetValue(nameId, out var platforms))
        {
            return null;
        }
        if (!platforms.TryGetValue(platformId, out var encodings))
        {
            return null;
        }
        if (!encodings.TryGetValue(encodingId, out var languages))
        {
            return null;
        }

        return languages.TryGetValue(languageId, out var value) ? value : null;
    }

    public List<NameRecord> GetNameRecords() => _nameRecords;
    public IReadOnlyList<NameRecord> NameRecords => _nameRecords;
    public string? GetFontFamily() => _fontFamily;
    public string? FontFamily => _fontFamily;
    public string? GetFontSubFamily() => _fontSubFamily;
    public string? FontSubFamily => _fontSubFamily;
    public string? GetPostScriptName() => _psName;
    public string? PostScriptName => _psName;
    public string? GetEnglishName(ushort nameId) => GetEnglishName((int)nameId);
}
