/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDCIDFontType0.java
 * PDFBOX_SOURCE_COMMIT: bf37c60dfa43cb9fb21497b44a667d091d809084
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 046747da99a870902217efabf1c41297de157059
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
using PdfBox.Net.FontBox.CFF;
using PdfBox.Net.FontBox.TTF;
using PdfBox.Net.FontBox.Util;
using PdfBox.Net.IO;
using PdfBox.Net.Util;
using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.PDModel.Font;

public partial class PDCIDFontType0 : PDCIDFont
{
    private static ILogger<PDCIDFontType0> LOG => PdfBoxLogging.CreateLogger<PDCIDFontType0>();

    private readonly CFFCIDFont? _cidFont;
    private readonly FontBoxFont? _t1Font;
    // The substitute has a different ROS: this font's CIDs are meaningless in it.
    // Resolve glyphs via Unicode instead (PDFBOX-6249).
    private readonly CmapLookup? _substituteUnicodeCmap;
    private readonly bool _isEmbedded;
    private readonly bool _isDamaged;
    private readonly int[]? _cidToGid;
    private Matrix? _fontMatrix;
    private BoundingBox? _fontBBox;

    public PDCIDFontType0(COSDictionary dictionary)
        : this(dictionary, null)
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="dictionary">The font dictionary according to the PDF specification.</param>
    /// <param name="resourceCache">Resource cache; may be <see langword="null"/>.</param>
    public PDCIDFontType0(COSDictionary dictionary, ResourceCache? resourceCache)
        : this(dictionary, resourceCache, FontMappers.Instance)
    {
    }

    internal PDCIDFontType0(COSDictionary dictionary, ResourceCache? resourceCache, FontMapper mapper)
        : base(dictionary, resourceCache)
    {
        CFFFont? cffFont = null;
        PDFontDescriptor? descriptor = GetFontDescriptor();
        if (descriptor?.GetCOSObject().GetDictionaryObject(COSName.GetPDFName("FontFile3")) is COSStream fontFile)
        {
            try
            {
                using Stream input = fontFile.CreateInputStream();
                using RandomAccessRead reader = new RandomAccessReadBuffer(input);
                if (reader.Length() > 0 && reader.Peek() == '%')
                {
                    LOG.LogWarning("Found PFB but expected embedded CFF font {FontName}", descriptor.GetFontName());
                    _isDamaged = true;
                }
                else
                {
                    cffFont = new CFFParser().Parse(reader).FirstOrDefault();
                }
            }
            catch (IOException exception)
            {
                LOG.LogError(exception, "Can't read the embedded CFF font {FontName}", descriptor.GetFontName());
                _isDamaged = true;
            }
        }

        if (cffFont is not null)
        {
            _cidFont = cffFont as CFFCIDFont;
            _t1Font = _cidFont is null ? cffFont : null;
            _isEmbedded = true;
            _isDamaged = false;
            _cidToGid = ReadCidToGidMap(dictionary);
        }
        else
        {
            CIDFontMapping? mapping = mapper.GetCIDFont(GetBaseFont(), descriptor, GetCIDSystemInfo());
            if (mapping?.IsCIDFont() == true && mapping.GetFont() is OpenTypeFont openType)
            {
                cffFont = openType.GetCFF().GetFont();
                _cidFont = cffFont as CFFCIDFont;
                _t1Font = _cidFont is null ? cffFont : null;
                if (_cidFont is not null && !IsCharacterCollectionMatch(_cidFont) && _cidFont.Ordering == "Identity")
                {
                    try
                    {
                        _substituteUnicodeCmap = openType.GetUnicodeCmapLookup();
                    }
                    catch (IOException exception)
                    {
                        LOG.LogWarning(exception, "Could not read cmap of the substitute for font {BaseFont}", GetBaseFont());
                    }
                }
            }
            else
            {
                _t1Font = mapping?.GetTrueTypeFont();
            }

            if (mapping?.IsFallback() == true && GetFontBoxFont() is FontBoxFont font)
            {
                LOG.LogWarning("Using fallback {FallbackFont} for CID-keyed font {BaseFont}", font.GetName(), GetBaseFont());
            }
        }
    }

    public string GetBaseFont() => GetName();
    public override bool IsEmbedded() => _isEmbedded;
    public override bool IsDamaged() => _isDamaged;

    /// <summary>Returns the embedded CFF font, or null if the substitute is not a CFF font.</summary>
    public CFFFont? GetCFFFont() => _cidFont ?? _t1Font as CFFFont;

    /// <summary>Returns the embedded or substituted font.</summary>
    public FontBoxFont? GetFontBoxFont() => _cidFont ?? _t1Font;

    /// <summary>Returns the Type 2 charstring for the given CID, or null for a non-CFF substitute.</summary>
    public Type2CharString? GetType2CharString(int cid) => GetCFFFont()?.GetType2CharString(cid);

    public override Matrix GetFontMatrix()
    {
        if (_fontMatrix is not null)
        {
            return _fontMatrix;
        }
        try
        {
            IList<float>? values = GetFontBoxFont()?.GetFontMatrix();
            _fontMatrix = values?.Count == 6
                ? new Matrix(values[0], values[1], values[2], values[3], values[4], values[5])
                : new Matrix(0.001f, 0, 0, 0.001f, 0, 0);
        }
        catch (IOException exception)
        {
            LOG.LogDebug(exception, "Couldn't get font matrix - returning default value");
            return new Matrix(0.001f, 0, 0, 0.001f, 0, 0);
        }
        return _fontMatrix;
    }

    public override BoundingBox GetBoundingBox()
    {
        return _fontBBox ??= GenerateBoundingBox();
    }

    private BoundingBox GenerateBoundingBox()
    {
        BoundingBox? descriptorBounds = GetFontDescriptor()?.GetFontBoundingBox();
        if (descriptorBounds is not null && (descriptorBounds.GetLowerLeftX() != 0 || descriptorBounds.GetLowerLeftY() != 0 ||
            descriptorBounds.GetUpperRightX() != 0 || descriptorBounds.GetUpperRightY() != 0))
        {
            return descriptorBounds;
        }
        try
        {
            return GetFontBoxFont()?.GetFontBBox() ?? new BoundingBox();
        }
        catch (IOException exception)
        {
            LOG.LogDebug(exception, "Couldn't get font bounding box - returning default value");
            return new BoundingBox();
        }
    }

    private bool IsCharacterCollectionMatch(CFFCIDFont substitute)
    {
        PDCIDSystemInfo? ros = GetCIDSystemInfo();
        return ros is not null && ros.Registry == substitute.Registry && ros.Ordering == substitute.Ordering;
    }

    private int CodeToSubstituteGID(int code, PDType0Font parent)
    {
        string? unicode = parent.ToUnicode(code);
        return string.IsNullOrEmpty(unicode) ? -1 : _substituteUnicodeCmap!.GetGlyphId(char.ConvertToUtf32(unicode, 0));
    }

    private static string GetGlyphName(int code, PDType0Font parent)
    {
        string? unicode = parent.ToUnicode(code);
        return string.IsNullOrEmpty(unicode) ? ".notdef" : UniUtil.GetUniNameOfCodePoint(char.ConvertToUtf32(unicode, 0));
    }

    internal int CodeToGID(int code, PDType0Font parent)
    {
        if (_substituteUnicodeCmap is not null)
        {
            return Math.Max(CodeToSubstituteGID(code, parent), 0);
        }
        int cid = parent.CodeToCID(code);
        return _cidFont?.GetCharset().GetGIDForCID(cid) ?? cid;
    }

    internal bool HasGlyph(int code, PDType0Font parent)
    {
        if (_substituteUnicodeCmap is not null)
        {
            return CodeToSubstituteGID(code, parent) > 0;
        }
        Type2CharString? charString = GetType2CharString(parent.CodeToCID(code));
        return charString is not null ? charString.GID != 0 : _t1Font?.HasGlyph(GetGlyphName(code, parent)) == true;
    }

    internal GeneralPath GetPath(int code, PDType0Font parent)
    {
        int cid = parent.CodeToCID(code);
        if (_substituteUnicodeCmap is not null)
        {
            return GetType2CharString(Math.Max(CodeToSubstituteGID(code, parent), 0))!.GetPath();
        }
        if (_cidToGid is not null && _isEmbedded)
        {
            cid = (uint)cid < (uint)_cidToGid.Length ? _cidToGid[cid] : 0;
        }
        return GetType2CharString(cid)?.GetPath() ?? _t1Font?.GetPath(GetGlyphName(code, parent)) ?? new GeneralPath();
    }

    internal float GetWidthFromFont(int code, PDType0Font parent)
    {
        int cid = parent.CodeToCID(code);
        float width;
        if (_substituteUnicodeCmap is not null)
        {
            width = GetType2CharString(Math.Max(CodeToSubstituteGID(code, parent), 0))!.GetWidth();
        }
        else if (_cidFont is not null || _isEmbedded && _t1Font is CFFType1Font)
        {
            width = GetType2CharString(cid)!.GetWidth();
        }
        else
        {
            width = _t1Font?.GetWidth(GetGlyphName(code, parent)) ?? base.GetWidthFromFont(cid);
        }
        return GetFontMatrix().Transform(width * 1000, 0).GetX();
    }

    private static int[]? ReadCidToGidMap(COSDictionary dictionary)
    {
        if (dictionary.GetDictionaryObject(COSName.GetPDFName("CIDToGIDMap")) is not COSStream stream)
        {
            return null;
        }
        using Stream input = stream.CreateInputStream();
        List<int> map = [];
        for (int high = input.ReadByte(); high >= 0; high = input.ReadByte())
        {
            int low = input.ReadByte();
            if (low < 0) break;
            map.Add((high << 8) | low);
        }
        return map.ToArray();
    }
}
