/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDCIDFontType2.java
 * PDFBOX_SOURCE_COMMIT: fee11b453d66725c2b3a28b6f862a8dc24d33177
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: fee11b453d66725c2b3a28b6f862a8dc24d33177
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
using PdfBox.Net.FontBox.TTF;
using PdfBox.Net.PDModel.Font.Encoding;
using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.PDModel.Font;

public partial class PDCIDFontType2 : PDCIDFont
{
    private static ILogger<PDCIDFontType2> LOG => PdfBoxLogging.CreateLogger<PDCIDFontType2>();

    private static readonly COSName FontDescriptorKey = COSName.GetPDFName("FontDescriptor");
    private static readonly COSName FontFileKey = COSName.GetPDFName("FontFile");
    private static readonly COSName FontFile2Key = COSName.GetPDFName("FontFile2");
    private static readonly COSName FontFile3Key = COSName.GetPDFName("FontFile3");
    private static readonly COSName CIDToGIDMapKey = COSName.GetPDFName("CIDToGIDMap");

    private readonly TrueTypeFont _trueTypeFont;
    private readonly CmapLookup? _unicodeCmap;
    private readonly int[]? _cidToGid;
    private readonly bool _isEmbedded;

    public PDCIDFontType2(COSDictionary dictionary, TrueTypeFont? trueTypeFont = null)
        : base(dictionary)
    {
        _isEmbedded = trueTypeFont is not null;
        _trueTypeFont = trueTypeFont ?? new TrueTypeFont();
        _unicodeCmap = _trueTypeFont.GetUnicodeCmapLookup(false);
        _cidToGid = ReadCidToGidMap(dictionary);
    }

    internal static PDCIDFontType2 Load(COSDictionary dictionary)
    {
        TrueTypeFont? ttf = null;
        try
        {
            if (dictionary.GetDictionaryObject(FontDescriptorKey) is COSDictionary descriptor)
            {
                ttf = TryParseEmbeddedFont(descriptor, FontFile2Key, preferOpenType: false)
                    ?? TryParseEmbeddedFont(descriptor, FontFile3Key, preferOpenType: true)
                    ?? TryParseEmbeddedFont(descriptor, FontFileKey, preferOpenType: false);
            }
        }
        catch
        {
            // Keep non-throwing CID font construction behavior.
        }

        return new PDCIDFontType2(dictionary, ttf);
    }

    private static TrueTypeFont? TryParseEmbeddedFont(COSDictionary descriptor, COSName key, bool preferOpenType)
    {
        if (descriptor.GetDictionaryObject(key) is not COSStream fontFile)
        {
            return null;
        }

        byte[] bytes;
        using (Stream stream = fontFile.CreateInputStream())
        using (MemoryStream buffer = new())
        {
            stream.CopyTo(buffer);
            bytes = buffer.ToArray();
        }

        if (bytes.Length == 0)
        {
            return null;
        }

        TrueTypeFont? parsed;
        Exception? parseException;
        if (preferOpenType)
        {
            parsed = TryParseOpenType(bytes, out parseException);
            if (parsed == null)
            {
                parsed = TryParseTrueType(bytes, out Exception? fallbackException);
                parseException ??= fallbackException;
            }
        }
        else
        {
            parsed = TryParseTrueType(bytes, out parseException);
            if (parsed == null)
            {
                parsed = TryParseOpenType(bytes, out Exception? fallbackException);
                parseException ??= fallbackException;
            }
        }
        if (parsed is OpenTypeFont openType && !openType.IsSupportedOTF())
        {
            LOG.LogWarning("Found an OpenType font using CFF2 outlines which are not supported {FontName}",
                descriptor.GetNameAsString(COSName.GetPDFName("FontName")));
            return null;
        }
        if (parsed is null)
        {
            string? fontName = descriptor.GetNameAsString(COSName.GetPDFName("FontName"));
            LOG.LogWarning(parseException, "Could not read embedded OTF for font {FontName}",
                fontName);
        }
        return parsed;
    }

    private static TrueTypeFont? TryParseTrueType(byte[] bytes, out Exception? exception)
    {
        try
        {
            TrueTypeFont font = new TTFParser(isEmbedded: true)
                .ParseEmbedded(new MemoryStream(bytes, writable: false));
            exception = null;
            return font;
        }
        catch (Exception ex)
        {
            exception = ex;
            return null;
        }
    }

    private static TrueTypeFont? TryParseOpenType(byte[] bytes, out Exception? exception)
    {
        try
        {
            TrueTypeFont font = new OTFParser(isEmbedded: true)
                .ParseEmbedded(new MemoryStream(bytes, writable: false));
            exception = null;
            return font;
        }
        catch (Exception ex)
        {
            exception = ex;
            return null;
        }
    }

    public override bool IsVertical()
    {
        return _trueTypeFont.GetVerticalHeader() != null && _trueTypeFont.GetVerticalMetrics() != null;
    }

    public TrueTypeFont GetTrueTypeFont() => _trueTypeFont;
    public override bool IsEmbedded() => _isEmbedded;
    public override bool IsDamaged() => false;

    /// <summary>
    /// Maps a CID to a glyph ID (GID) using the CIDToGIDMap entry.
    /// When the CIDToGIDMap is "Identity" or absent, returns the CID as the GID.
    /// </summary>
    public int CodeToGID(int cid)
    {
        if (_cidToGid != null)
        {
            if (!_isEmbedded)
            {
                LOG.LogWarning("Using non-embedded GIDs in font {FontName}", GetName());
            }
            return (cid >= 0 && cid < _cidToGid.Length) ? _cidToGid[cid] : 0;
        }

        return cid;
    }

    /// <summary>
    /// Returns the ToUnicode string for the given character code via the TTF unicode cmap
    /// when no explicit ToUnicode CMap is present in the font dictionary.
    /// </summary>
    protected override string? ToUnicodeFallback(int code, GlyphList glyphList)
    {
        if (_unicodeCmap != null)
        {
            List<int>? charCodes = _unicodeCmap.GetCharCodes(CodeToGID(code));
            if (charCodes != null && charCodes.Count > 0)
            {
                return char.ConvertFromUtf32(charCodes[0]);
            }
        }

        return base.ToUnicodeFallback(code, glyphList);
    }

    /// <summary>
    /// Returns the width for the given character code. Explicit W/DW dictionary entries take
    /// precedence; when absent, falls back to the TTF advance width scaled to PDF text units.
    /// </summary>
    public override float GetWidth(int code)
    {
        if (HasExplicitCidWidth(code))
        {
            return base.GetWidth(code);
        }

        // Fall back to TTF advance width scaled to 1000-unit PDF space.
        int gid = CodeToGID(code);
        int unitsPerEm = _trueTypeFont.GetUnitsPerEm();
        return _trueTypeFont.GetAdvanceWidth(gid) * 1000f / unitsPerEm;
    }

    public override float GetWidthFromFont(int code)
    {
        int gid = CodeToGID(code);
        int unitsPerEm = _trueTypeFont.GetUnitsPerEm();
        return _trueTypeFont.GetAdvanceWidth(gid) * 1000f / Math.Max(1, unitsPerEm);
    }

    public override float GetHeight(int code)
    {
        return GetBoundingBox().GetHeight();
    }

    public bool HasGlyph(int code)
    {
        int gid = CodeToGID(code);
        return _trueTypeFont.GetGlyph()?.GetGlyph(gid) != null;
    }

    public GeneralPath GetNormalizedPath(int code)
    {
        int gid = CodeToGID(code);
        return TrueTypePathNormalizer.GetNormalizedPath(_trueTypeFont, gid, drawGidZero: IsEmbedded());
    }

    public GeneralPath GetPath(int code) => GetNormalizedPath(code);

    public byte[] EncodeGlyphId(int glyphId)
    {
        return [(byte)(glyphId >> 8), (byte)glyphId];
    }

    private static int[]? ReadCidToGidMap(COSDictionary dictionary)
    {
        COSBase? entry = dictionary.GetDictionaryObject(CIDToGIDMapKey);
        if (entry is COSName name && string.Equals(name.GetName(), "Identity", StringComparison.Ordinal))
        {
            // Identity: CID == GID, no explicit mapping needed.
            return null;
        }

        if (entry is COSStream stream)
        {
            try
            {
                using Stream input = stream.CreateInputStream();
                using MemoryStream buffer = new();
                input.CopyTo(buffer);
                byte[] bytes = buffer.ToArray();
                int count = bytes.Length / 2;
                int[] map = new int[count];
                for (int i = 0; i < count; i++)
                {
                    map[i] = (bytes[i * 2] << 8) | bytes[i * 2 + 1];
                }

                return map;
            }
            catch
            {
                // Fall back to identity mapping.
            }
        }

        return null;
    }
}
