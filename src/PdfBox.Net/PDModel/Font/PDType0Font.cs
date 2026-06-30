/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDType0Font.java
 * PDFBOX_SOURCE_COMMIT: e369837044c655ad3ee34f41cd30bef2465d9566
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: e369837044c655ad3ee34f41cd30bef2465d9566
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
using PdfBox.Net.FontBox.CMap;
using PdfBox.Net.FontBox.TTF;
using PdfBox.Net.FontBox.Util;
using PdfBox.Net.IO;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Font.Encoding;
using PdfBox.Net.Util;
using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.PDModel.Font;

public partial class PDType0Font : PDVectorFont
{
    private static readonly COSName EncodingKey = COSName.GetPDFName("Encoding");
    private static readonly COSName CidSystemInfoKey = COSName.GetPDFName("CIDSystemInfo");

    private readonly PDCIDFont? _descendantFont;
    private readonly CMap? _cMap;
    private readonly CMap? _cMapUcs2;
    private readonly bool _isCMapPredefined;
    private readonly bool _isDescendantCjk;

    public PDType0Font(COSDictionary dictionary, PDCIDFont? descendantFont)
        : base(dictionary)
    {
        _descendantFont = descendantFont;
        (_cMap, _isCMapPredefined) = ReadEncodingCMap(dictionary);
        _isDescendantCjk = IsDescendantCjk(descendantFont);
        _cMapUcs2 = ReadUcs2CMap(dictionary, _cMap, _isCMapPredefined, _isDescendantCjk);
    }

    internal static PDType0Font Load(COSDictionary dictionary)
    {
        PDCIDFont? descendant = null;
        COSArray? descendants = dictionary.GetCOSArray(COSName.GetPDFName("DescendantFonts"));
        if (descendants != null && descendants.Size() > 0 && descendants.GetObject(0) is COSDictionary descendantDict)
        {
            descendant = PDFontFactory.CreateDescendantFont(descendantDict);
        }

        return new PDType0Font(dictionary, descendant);
    }

    public static PDType0Font Load(PDDocument document, string filePath)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        using FileStream input = File.OpenRead(filePath);
        return Load(document, input);
    }

    public static PDType0Font Load(PDDocument document, Stream input)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(input);
        using MemoryStream buffer = new();
        input.CopyTo(buffer);
        return Load(document, buffer.ToArray());
    }

    /// <summary>
    /// Loads a TTF to be embedded into a document as a composite (Type 0) font.
    /// The <paramref name="embedSubset"/> flag is accepted for API parity with the upstream Java
    /// method but is currently ignored — the full font is always embedded.
    /// </summary>
    /// <param name="document">The PDF document that will hold the embedded font.</param>
    /// <param name="input">A TTF font stream. It will be read to completion before returning.</param>
    /// <param name="embedSubset">
    /// When <see langword="true"/> in upstream Java, only the glyphs actually used in the
    /// document are embedded (subsetting). This parameter is currently ignored in this port.
    /// </param>
    /// <returns>A <see cref="PDType0Font"/> instance.</returns>
    public static PDType0Font Load(PDDocument document, Stream input, bool embedSubset)
        => Load(document, input);

    private static PDType0Font Load(PDDocument document, byte[] fontBytes)
    {
        ArgumentNullException.ThrowIfNull(document);
        TrueTypeFont trueTypeFont = new TTFParser().Parse(fontBytes);

        string baseFontName = trueTypeFont.GetName();
        if (string.IsNullOrWhiteSpace(baseFontName))
        {
            baseFontName = "CIDFont+F0";
        }

        COSDictionary descendantDictionary = new();
        descendantDictionary.SetName(COSName.TYPE, "Font");
        descendantDictionary.SetName(COSName.SUBTYPE, "CIDFontType2");
        descendantDictionary.SetName(COSName.GetPDFName("BaseFont"), baseFontName);
        descendantDictionary.SetItem(CidSystemInfoKey, CreateCidSystemInfo());
        descendantDictionary.SetItem(
            COSName.GetPDFName("FontDescriptor"),
            CreateEmbeddedFontDescriptor(document, trueTypeFont, fontBytes, baseFontName));
        descendantDictionary.SetItem(COSName.GetPDFName("CIDToGIDMap"), COSName.GetPDFName("Identity"));

        COSDictionary type0Dictionary = new();
        type0Dictionary.SetName(COSName.TYPE, "Font");
        type0Dictionary.SetName(COSName.SUBTYPE, "Type0");
        type0Dictionary.SetName(COSName.GetPDFName("BaseFont"), baseFontName);
        type0Dictionary.SetItem(EncodingKey, COSName.GetPDFName("Identity-H"));

        COSArray descendants = new();
        descendants.Add(descendantDictionary);
        type0Dictionary.SetItem(COSName.GetPDFName("DescendantFonts"), descendants);

        PDCIDFontType2 descendantFont = new(descendantDictionary, trueTypeFont);
        return new PDType0Font(type0Dictionary, descendantFont);
    }

    private static COSDictionary CreateCidSystemInfo()
    {
        COSDictionary info = new();
        info.SetString(COSName.GetPDFName("Registry"), "Adobe");
        info.SetString(COSName.GetPDFName("Ordering"), "Identity");
        info.SetInt(COSName.GetPDFName("Supplement"), 0);
        return info;
    }

    private static COSDictionary CreateEmbeddedFontDescriptor(
        PDDocument document, TrueTypeFont trueTypeFont, byte[] fontBytes, string baseFontName)
    {
        COSDictionary descriptor = new();
        descriptor.SetName(COSName.TYPE, "FontDescriptor");
        descriptor.SetName(COSName.GetPDFName("FontName"), baseFontName);
        descriptor.SetInt(COSName.GetPDFName("Flags"), 32);
        descriptor.SetFloat(COSName.GetPDFName("StemV"), 80f);

        BoundingBox box = trueTypeFont.GetFontBBox();
        descriptor.SetItem(
            COSName.GetPDFName("FontBBox"),
            COSArray.Of(box.GetLowerLeftX(), box.GetLowerLeftY(), box.GetUpperRightX(), box.GetUpperRightY()));

        if (trueTypeFont.GetHorizontalHeader() is { } horizontalHeader)
        {
            descriptor.SetFloat(COSName.GetPDFName("Ascent"), horizontalHeader.Ascender);
            descriptor.SetFloat(COSName.GetPDFName("Descent"), horizontalHeader.Descender);
        }

        using MemoryStream fontStream = new(fontBytes, writable: false);
        PDStream fontFile = new(document, fontStream, COSName.FLATE_DECODE);
        descriptor.SetItem(COSName.GetPDFName("FontFile2"), fontFile.GetCOSObject());
        return descriptor;
    }

    public int CodeToCID(int code)
    {
        if (_cMap != null)
        {
            return _cMap.ToCID(code);
        }

        return _descendantFont?.CodeToCID(code) ?? code;
    }

    public PDCIDFont? GetDescendantFont() => _descendantFont;
    public CMap? GetCMap() => _cMap;
    public CMap? GetCMapUCS2() => _cMapUcs2;
    public string GetBaseFont() => base.GetName();

    public override bool IsVertical() => _cMap?.WMode == 1;
    public override bool IsEmbedded() => _descendantFont?.IsEmbedded() ?? false;
    public override bool IsDamaged() => _descendantFont?.IsDamaged() ?? false;
    public override Matrix GetFontMatrix() => _descendantFont?.GetFontMatrix() ?? base.GetFontMatrix();
    public override string GetName() => GetBaseFont();
    public override bool IsStandard14() => false;
    public override bool HasExplicitWidth(int code) => _descendantFont?.HasExplicitWidth(CodeToCID(code)) ?? base.HasExplicitWidth(code);
    public override float GetWidthFromFont(int code) => _descendantFont?.GetWidthFromFont(CodeToCID(code)) ?? base.GetWidthFromFont(code);
    public override float GetWidth(int code) => _descendantFont?.GetWidth(CodeToCID(code)) ?? base.GetWidth(code);
    public override float GetAverageFontWidth() => _descendantFont?.GetAverageFontWidth() ?? base.GetAverageFontWidth();
    public override float GetSpaceWidth() => _descendantFont?.GetSpaceWidth() ?? base.GetSpaceWidth();
    public override PDFontDescriptor? GetFontDescriptor() => base.GetFontDescriptor() ?? _descendantFont?.GetFontDescriptor();

    public override int ReadCode(Stream input)
    {
        return _cMap?.ReadCode(input) ?? base.ReadCode(input);
    }

    public override Vector GetDisplacement(int code)
    {
        return IsVertical()
            ? new Vector(0, _descendantFont?.GetVerticalDisplacementVectorY(CodeToCID(code)) / 1000f ?? 0f)
            : new Vector(GetWidth(code) / 1000f, 0);
    }

    public override Vector GetPositionVector(int code)
    {
        return _descendantFont?.GetPositionVector(CodeToCID(code)).Scale(-1 / 1000f) ?? base.GetPositionVector(code);
    }

    public override string? ToUnicode(int code, GlyphList glyphList)
    {
        string? unicode = base.ToUnicode(code, glyphList);
        if (!string.IsNullOrEmpty(unicode))
        {
            return unicode;
        }

        if ((_isCMapPredefined || _isDescendantCjk) && _cMapUcs2 != null)
        {
            int cid = CodeToCID(code);
            string? mapped = _cMapUcs2.ToUnicode(cid);
            if (!string.IsNullOrEmpty(mapped))
            {
                return mapped;
            }
        }

        return _descendantFont?.ToUnicode(CodeToCID(code), glyphList);
    }

    public override string? ToUnicode(int code) => ToUnicode(code, GlyphList.GetAdobeGlyphList());

    public override BoundingBox GetBoundingBox()
    {
        BoundingBox bbox = base.GetBoundingBox();
        return bbox.GetWidth() == 0 && bbox.GetHeight() == 0
            ? _descendantFont?.GetBoundingBox() ?? bbox
            : bbox;
    }

    public override bool HasGlyph(int code)
    {
        if (_descendantFont is PDCIDFontType2 type2)
        {
            int gid = type2.CodeToGID(CodeToCID(code));
            return type2.GetTrueTypeFont().GetGlyph()?.GetGlyph(gid) != null;
        }

        return false;
    }

    public int CodeToGID(int code)
    {
        return _descendantFont is PDCIDFontType2 type2
            ? type2.CodeToGID(CodeToCID(code))
            : CodeToCID(code);
    }

    public override GeneralPath GetNormalizedPath(int code)
    {
        if (_descendantFont is PDCIDFontType2 type2)
        {
            int gid = type2.CodeToGID(CodeToCID(code));
            return TrueTypePathNormalizer.GetNormalizedPath(
                type2.GetTrueTypeFont(),
                gid,
                drawGidZero: type2.IsEmbedded());
        }

        return new GeneralPath();
    }

    public override GeneralPath GetPath(int code) => GetNormalizedPath(code);

    public override float GetHeight(int code)
    {
        return _descendantFont?.GetHeight(CodeToCID(code)) ?? base.GetHeight(code);
    }

    public override string ToString()
    {
        string? descendant = _descendantFont?.GetType().Name;
        return $"{GetType().Name}/{descendant}, PostScript name: {GetBaseFont()}";
    }

    private static (CMap? CMap, bool IsPredefined) ReadEncodingCMap(COSDictionary dictionary)
    {
        COSBase? encoding = dictionary.GetDictionaryObject(EncodingKey);
        if (encoding is COSName encodingName)
        {
            CMap? predefined = TryParsePredefinedCMap(encodingName.GetName());
            if (predefined == null &&
                (string.Equals(encodingName.GetName(), "Identity-H", StringComparison.Ordinal) ||
                 string.Equals(encodingName.GetName(), "Identity-V", StringComparison.Ordinal)))
            {
                predefined = CreateIdentityCMap(encodingName.GetName());
            }

            return (predefined, predefined != null);
        }

        if (encoding is not null)
        {
            return (ReadCMap(encoding), false);
        }

        return (null, false);
    }

    private static CMap? ReadUcs2CMap(COSDictionary dictionary, CMap? cMap, bool isCMapPredefined, bool isDescendantCjk)
    {
        string? baseName = null;
        if (isDescendantCjk && dictionary.GetCOSArray(COSName.GetPDFName("DescendantFonts"))?.GetObject(0) is COSDictionary descendantDict)
        {
            if (descendantDict.GetDictionaryObject(CidSystemInfoKey) is COSDictionary cidSystemInfoDict)
            {
                PDCIDSystemInfo cidSystemInfo = new(cidSystemInfoDict);
                baseName = $"{cidSystemInfo.Registry}-{cidSystemInfo.Ordering}-{cidSystemInfo.Supplement}";
            }
        }
        else if (isCMapPredefined && dictionary.GetDictionaryObject(EncodingKey) is COSName encodingName)
        {
            baseName = encodingName.GetName();
        }
        else if (isDescendantCjk && cMap != null && !string.IsNullOrWhiteSpace(cMap.Name))
        {
            baseName = cMap.Name;
        }

        if (string.IsNullOrWhiteSpace(baseName))
        {
            return null;
        }

        CMap? predefined = TryParsePredefinedCMap(baseName);
        if (predefined is null || string.IsNullOrWhiteSpace(predefined.Registry) || string.IsNullOrWhiteSpace(predefined.Ordering))
        {
            return null;
        }

        string ucs2Name = $"{predefined.Registry}-{predefined.Ordering}-UCS2";
        return TryParsePredefinedCMap(ucs2Name);
    }

    private static bool IsDescendantCjk(PDCIDFont? descendantFont)
    {
        PDCIDSystemInfo? ros = descendantFont?.GetCIDSystemInfo();
        if (ros == null || !string.Equals(ros.Registry, "Adobe", StringComparison.Ordinal))
        {
            return false;
        }

        return string.Equals(ros.Ordering, "GB1", StringComparison.Ordinal) ||
               string.Equals(ros.Ordering, "CNS1", StringComparison.Ordinal) ||
               string.Equals(ros.Ordering, "Japan1", StringComparison.Ordinal) ||
               string.Equals(ros.Ordering, "Korea1", StringComparison.Ordinal);
    }

    private static CMap? TryParsePredefinedCMap(string name)
    {
        try
        {
            return new CMapParser().ParsePredefined(name);
        }
        catch
        {
            return null;
        }
    }

    private static CMap CreateIdentityCMap(string name)
    {
        string wMode = string.Equals(name, "Identity-V", StringComparison.Ordinal) ? "/WMode 1 def\n" : string.Empty;
        string identityMap = $$"""
/CIDInit /ProcSet findresource begin
12 dict begin
begincmap
/CIDSystemInfo << /Registry (Adobe) /Ordering (Identity) /Supplement 0 >> def
/CMapName /{{name}} def
/CMapType 1 def
{{wMode}}1 begincodespacerange
<0000> <FFFF>
endcodespacerange
1 begincidrange
<0000> <FFFF> 0
endcidrange
endcmap
CMapName currentdict /CMap defineresource pop
end
end
""";

        byte[] bytes = System.Text.Encoding.ASCII.GetBytes(identityMap);
        using RandomAccessRead randomAccess = new RandomAccessReadBuffer(bytes);
        return new CMapParser().Parse(randomAccess);
    }

    private static CMap? ReadCMap(COSBase encoding)
    {
        if (encoding is not COSStream stream)
        {
            return null;
        }

        try
        {
            using Stream input = stream.CreateInputStream();
            using MemoryStream buffer = new();
            input.CopyTo(buffer);
            using RandomAccessRead randomAccess = new RandomAccessReadBuffer(buffer.ToArray());
            return new CMapParser().Parse(randomAccess);
        }
        catch
        {
            return null;
        }
    }
}
