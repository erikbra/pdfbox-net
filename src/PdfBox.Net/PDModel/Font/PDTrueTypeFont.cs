/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDTrueTypeFont.java
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

using System.Globalization;
using PdfBox.Net.COS;
using PdfBox.Net.FontBox;
using PdfBox.Net.FontBox.TTF;
using PdfBox.Net.PDModel.Font.Encoding;
using PdfBox.Net.Util;
using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.PDModel.Font;

public partial class PDTrueTypeFont : PDSimpleFont
{
    private static readonly COSName FontDescriptorKey = COSName.GetPDFName("FontDescriptor");
    private static readonly COSName FontFile2Key = COSName.GetPDFName("FontFile2");
    private static readonly COSName FontFile3Key = COSName.GetPDFName("FontFile3");

    private readonly TrueTypeFont _trueTypeFont;
    private readonly CmapLookup? _unicodeCmap;

    public PDTrueTypeFont(COSDictionary dictionary, TrueTypeFont trueTypeFont)
        : base(dictionary)
    {
        _trueTypeFont = trueTypeFont ?? throw new ArgumentNullException(nameof(trueTypeFont));
        _unicodeCmap = _trueTypeFont.GetUnicodeCmapLookup(false);
    }

    public PDTrueTypeFont(TrueTypeFont trueTypeFont)
        : this(new COSDictionary(), trueTypeFont)
    {
    }

    internal static PDTrueTypeFont? Load(COSDictionary dictionary)
    {
        try
        {
            if (dictionary.GetDictionaryObject(FontDescriptorKey) is COSDictionary descriptor &&
                descriptor.GetDictionaryObject(FontFile2Key) is COSStream fontFile)
            {
                using Stream stream = fontFile.CreateInputStream();
                TrueTypeFont ttf = new TTFParser(isEmbedded: true).ParseEmbedded(stream);
                return new PDTrueTypeFont(dictionary, ttf);
            }
        }
        catch
        {
            // Keep non-throwing font factory behavior.
        }

        return null;
    }

    public override FontBoxFont? GetFontBoxFont() => _trueTypeFont;
    public override bool IsStandard14() => false;
    public override bool IsEmbedded() => true;

    public TrueTypeFont GetTrueTypeFont() => _trueTypeFont;

    public override Matrix GetFontMatrix() => GetFontMatrixFromDictionary();

    public override float GetWidthFromFont(int code)
    {
        int gid = CodeToGID(code);
        int unitsPerEm = _trueTypeFont.GetUnitsPerEm();
        return _trueTypeFont.GetAdvanceWidth(gid) * 1000f / Math.Max(1, unitsPerEm);
    }

    public override bool HasGlyph(int code)
    {
        return CodeToGID(code) != 0;
    }

    public override GeneralPath GetNormalizedPath(int code)
    {
        int gid = CodeToGID(code);
        return TrueTypePathNormalizer.GetNormalizedPath(
            _trueTypeFont,
            gid,
            drawGidZero: IsEmbedded() || IsStandard14());
    }

    public byte[] ExportFont()
    {
        if (FontDictionary.GetDictionaryObject(FontDescriptorKey) is COSDictionary descriptor)
        {
            if (TryReadEmbeddedFont(descriptor, FontFile2Key, out byte[]? bytes) ||
                TryReadEmbeddedFont(descriptor, FontFile3Key, out bytes))
            {
                return bytes;
            }
        }

        using Stream originalData = _trueTypeFont.GetOriginalData();
        using MemoryStream buffer = new();
        originalData.CopyTo(buffer);
        return buffer.ToArray();
    }

    private static bool TryReadEmbeddedFont(COSDictionary descriptor, COSName key, out byte[] bytes)
    {
        if (descriptor.GetDictionaryObject(key) is COSStream fontFile)
        {
            using Stream input = fontFile.CreateInputStream();
            using MemoryStream buffer = new();
            input.CopyTo(buffer);
            bytes = buffer.ToArray();
            return true;
        }

        bytes = Array.Empty<byte>();
        return false;
    }

    protected override string? ToUnicodeFallback(int code, GlyphList glyphList)
    {
        string? mapped = base.ToUnicodeFallback(code, glyphList);
        if (mapped != null)
        {
            return mapped;
        }

        if (_unicodeCmap != null && _unicodeCmap.GetGlyphId(code) != 0)
        {
            return char.ConvertFromUtf32(code);
        }

        return null;
    }

    private int CodeToGID(int code)
    {
        if (_unicodeCmap is null)
        {
            return _trueTypeFont.NameToGID(code.ToString(CultureInfo.InvariantCulture));
        }

        string? unicode = base.ToUnicode(code, GlyphList.GetAdobeGlyphList());
        if (!string.IsNullOrEmpty(unicode))
        {
            int codePoint = char.ConvertToUtf32(unicode, 0);
            int gid = _unicodeCmap.GetGlyphId(codePoint);
            if (gid != 0)
            {
                return gid;
            }
        }

        return _unicodeCmap.GetGlyphId(code);
    }
}
