/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/NameRecord.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
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

namespace PdfBox.Net.FontBox.TTF;

/// <summary>
/// A name record in the name table.
/// </summary>
public sealed partial class NameRecord
{
    public const int PLATFORM_UNICODE = 0;
    public const int PLATFORM_MACINTOSH = 1;
    public const int PLATFORM_ISO = 2;
    public const int PLATFORM_WINDOWS = 3;

    public const int ENCODING_UNICODE_1_0 = 0;
    public const int ENCODING_UNICODE_1_1 = 1;
    public const int ENCODING_UNICODE_2_0_BMP = 3;
    public const int ENCODING_UNICODE_2_0_FULL = 4;

    public const int LANGUAGE_UNICODE = 0;

    public const int ENCODING_WINDOWS_SYMBOL = 0;
    public const int ENCODING_WINDOWS_UNICODE_BMP = 1;
    public const int ENCODING_WINDOWS_UNICODE_UCS4 = 10;

    public const int LANGUAGE_WINDOWS_EN_US = 0x0409;

    public const int ENCODING_MACINTOSH_ROMAN = 0;

    public const int LANGUAGE_MACINTOSH_ENGLISH = 0;

    public const int NAME_COPYRIGHT = 0;
    public const int NAME_FONT_FAMILY_NAME = 1;
    public const int NAME_FONT_SUB_FAMILY_NAME = 2;
    public const int NAME_UNIQUE_FONT_ID = 3;
    public const int NAME_FULL_FONT_NAME = 4;
    public const int NAME_VERSION = 5;
    public const int NAME_POSTSCRIPT_NAME = 6;
    public const int NAME_TRADEMARK = 7;

    private int _platformId;
    private int _platformEncodingId;
    private int _languageId;
    private int _nameId;
    private int _stringLength;
    private int _stringOffset;
    private string? _string;

    internal void InitData(TrueTypeFont ttf, TTFDataStream data)
    {
        _platformId = data.ReadUnsignedShort();
        _platformEncodingId = data.ReadUnsignedShort();
        _languageId = data.ReadUnsignedShort();
        _nameId = data.ReadUnsignedShort();
        _stringLength = data.ReadUnsignedShort();
        _stringOffset = data.ReadUnsignedShort();
    }

    public override string ToString()
    {
        return $"platform={_platformId} pEncoding={_platformEncodingId} language={_languageId} name={_nameId} {_string}";
    }

    public int GetStringLength() => _stringLength;
    public void SetStringLength(int stringLengthValue) => _stringLength = stringLengthValue;
    public int GetStringOffset() => _stringOffset;
    public void SetStringOffset(int stringOffsetValue) => _stringOffset = stringOffsetValue;
    public int GetLanguageId() => _languageId;
    public void SetLanguageId(int languageIdValue) => _languageId = languageIdValue;
    public int GetNameId() => _nameId;
    public void SetNameId(int nameIdValue) => _nameId = nameIdValue;
    public int GetPlatformEncodingId() => _platformEncodingId;
    public void SetPlatformEncodingId(int platformEncodingIdValue) => _platformEncodingId = platformEncodingIdValue;
    public int GetPlatformId() => _platformId;
    public void SetPlatformId(int platformIdValue) => _platformId = platformIdValue;
    public string? GetString() => _string;
    public void SetString(string? stringValue) => _string = stringValue;
}
