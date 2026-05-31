/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDType1Font.java
 * PDFBOX_SOURCE_COMMIT: b07158974a4dbbcebf0e33d3797b9f0655cc62d9
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: b07158974a4dbbcebf0e33d3797b9f0655cc62d9
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
using PdfBox.Net.FontBox;
using PdfBox.Net.FontBox.TTF;
using PdfBox.Net.FontBox.Type1;
using PdfBox.Net.PDModel.Font.Encoding;

namespace PdfBox.Net.PDModel.Font;

public partial class PDType1Font : PDSimpleFont
{
    private static readonly COSName FontDescriptorKey = COSName.GetPDFName("FontDescriptor");
    private static readonly COSName FontFileKey = COSName.GetPDFName("FontFile");
    private static readonly COSName BaseFontKey = COSName.GetPDFName("BaseFont");

    private readonly Type1Font? _type1Font;
    private readonly FontBoxFont? _fontBoxFont;
    private readonly bool _isStandard14;

    public PDType1Font(COSDictionary dictionary, Type1Font? type1Font = null, FontBoxFont? fontBoxFont = null)
        : base(dictionary, ResolveType1Encoding(dictionary, type1Font))
    {
        _type1Font = type1Font;
        _isStandard14 = Standard14Fonts.IsStandard14Font(GetName());
        _fontBoxFont = fontBoxFont ?? type1Font as FontBoxFont ?? TryLoadMappedFont(dictionary.GetNameAsString(BaseFontKey));
    }

    internal static PDType1Font Load(COSDictionary dictionary)
    {
        Type1Font? font = null;
        FontBoxFont? fontBoxFont = null;
        try
        {
            if (dictionary.GetDictionaryObject(FontDescriptorKey) is COSDictionary descriptor &&
                descriptor.GetDictionaryObject(FontFileKey) is COSStream fontFile)
            {
                using Stream stream = fontFile.CreateInputStream();
                using MemoryStream buffer = new();
                stream.CopyTo(buffer);
                byte[] bytes = buffer.ToArray();
                font = TryCreateType1Font(bytes, fontFile);
                fontBoxFont = font;
            }
        }
        catch
        {
            // Keep dictionary-driven fallback behavior.
        }

        return new PDType1Font(dictionary, font, fontBoxFont);
    }

    public override FontBoxFont? GetFontBoxFont() => _fontBoxFont;
    public override bool IsStandard14() => _isStandard14;

    private static Encoding.Encoding ResolveType1Encoding(COSDictionary dictionary, Type1Font? type1Font)
    {
        if (dictionary.GetDictionaryObject(COSName.GetPDFName("Encoding")) is not null)
        {
            return DictionaryEncoding.ResolveEncoding(dictionary);
        }

        if (type1Font != null)
        {
            return new Type1Encoding(type1Font);
        }

        return DictionaryEncoding.ResolveStandard14FallbackEncoding(dictionary.GetNameAsString(BaseFontKey));
    }

    private static Type1Font? TryCreateType1Font(byte[] bytes, COSStream fontFile)
    {
        if (bytes.Length == 0)
        {
            return null;
        }

        if (bytes[0] == 0x80)
        {
            return Type1Font.CreateWithPFB(bytes);
        }

        int length1 = fontFile.GetInt(COSName.GetPDFName("Length1"), -1);
        int length2 = fontFile.GetInt(COSName.GetPDFName("Length2"), -1);
        if (length1 > 0 && length2 > 0 && length1 + length2 <= bytes.Length)
        {
            byte[] segment1 = bytes[..length1];
            byte[] segment2 = bytes[length1..(length1 + length2)];
            return Type1Font.CreateWithSegments(segment1, segment2);
        }

        return null;
    }

    private static FontBoxFont? TryLoadMappedFont(string? fontName)
    {
        string mappedName = Standard14Fonts.GetMappedFontName(fontName) ?? fontName ?? string.Empty;
        if (string.IsNullOrWhiteSpace(mappedName))
        {
            return null;
        }

        string? fontPath = FontMappers.Instance.FindFontFile(mappedName);
        if (string.IsNullOrWhiteSpace(fontPath) || !File.Exists(fontPath))
        {
            return null;
        }

        try
        {
            string extension = Path.GetExtension(fontPath);
            if (extension.Equals(".pfb", StringComparison.OrdinalIgnoreCase))
            {
                byte[] bytes = File.ReadAllBytes(fontPath);
                return Type1Font.CreateWithPFB(bytes);
            }

            if (extension.Equals(".ttf", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".otf", StringComparison.OrdinalIgnoreCase))
            {
                byte[] bytes = File.ReadAllBytes(fontPath);
                return new TTFParser().Parse(bytes);
            }
        }
        catch
        {
            // Keep dictionary-driven fallback behavior.
        }

        return null;
    }
}
