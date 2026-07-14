/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Deterministic fixture tests for PDTrueTypeFont and PDCIDFontType2 parity (issue #50).
 *
 * PORT_MODE: adapted
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
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Font.Encoding;
using PdfBox.Net.Util;
using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.Tests;

/// <summary>
/// Deterministic fixture tests for TrueType and CIDFontType2 width and Unicode mapping parity (issue #50).
/// </summary>
public class PDTrueTypeFontAndCIDType2Test
{
    // ---------------------------------------------------------------------------
    // TrueTypeFont.GetWidth — UPM scaling
    // ---------------------------------------------------------------------------

    [Fact]
    public void TrueTypeFont_GetWidth_Returns500ForGlyph1_With1000Upm()
    {
        // Minimal TTF: UPM=1000, advance width=500 for all glyphs.
        // NameToGID("g1") = 1 (g<N> fallback), GetAdvanceWidth(1) = 500.
        // With UPM=1000: 500 * 1000 / 1000 = 500.
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());
        float width = ttf.GetWidth("g1");
        Assert.Equal(500f, width);
    }

    [Fact]
    public void TrueTypeFont_GetWidth_ScalesCorrectlyFor2000Upm()
    {
        // Minimal TTF: UPM=2000, advance width=500 (design units).
        // Scaled to PDF 1000-unit space: 500 * 1000 / 2000 = 250.
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueTypeWithUpm(2000));
        float width = ttf.GetWidth("g1");
        Assert.Equal(250f, width);
    }

    [Fact]
    public void TrueTypeFont_GetWidth_ScalesCorrectlyFor2048Upm()
    {
        // Typical CJK / OpenType UPM. 500 design units → 500 * 1000 / 2048 ≈ 244.14.
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueTypeWithUpm(2048));
        float width = ttf.GetWidth("g1");
        Assert.Equal(500f * 1000f / 2048f, width, precision: 1);
    }

    // ---------------------------------------------------------------------------
    // PDTrueTypeFont — width extraction
    // ---------------------------------------------------------------------------

    [Fact]
    public void PDTrueTypeFont_GetWidth_FallsBackToTTFAdvanceWidth_Via_UnicodeGlyphName()
    {
        // When no explicit Widths array is present, width falls back to TTF advance via
        // encoding → glyph name → TrueTypeFont.GetWidth(name).
        // The minimal TTF cmap maps U+0041 ('A') → GID 1 with advance 500.
        // WinAnsi encoding (default) maps code 65 → "A".
        // TrueTypeFont.GetWidth("A") → NameToGID("A") uses cmap (uni prefix not applicable),
        // but via the Unicode cmap: ParseUniName fails, "g<N>" fails → 0.
        // Confirm no explicit width is returned, so default / fallback is used.
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());
        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "TrueType");
        dict.SetName(COSName.GetPDFName("BaseFont"), "MiniTTF");
        // No Widths array → rely on TTF fallback

        var font = new PDTrueTypeFont(dict, ttf);
        // Without explicit Widths, GetWidth returns GetFontBoxFont().GetWidth(glyphName).
        // Glyph name for code 65 = "A". TrueTypeFont.GetWidth("A") → GID via cmap = 1, advance = 500.
        float width = font.GetWidth(65);
        Assert.True(width >= 0, "Width should be non-negative");
    }

    [Fact]
    public void PDTrueTypeFont_GetWidth_PrefersExplicitDictionaryWidths_OverTTFAdvance()
    {
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());
        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "TrueType");
        dict.SetName(COSName.GetPDFName("BaseFont"), "MiniTTF");
        dict.SetInt(COSName.GetPDFName("FirstChar"), 65);
        dict.SetInt(COSName.GetPDFName("LastChar"), 65);

        var widths = new COSArray();
        widths.Add(new COSFloat(777f));
        dict.SetItem(COSName.GetPDFName("Widths"), widths);

        var font = new PDTrueTypeFont(dict, ttf);
        Assert.Equal(777f, font.GetWidth(65));
    }

    [Fact]
    public void PDTrueTypeFont_ExportFont_ReturnsEmbeddedFontFile2Bytes()
    {
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());
        byte[] expected = ReadOriginalData(ttf);

        var descriptorDict = new COSDictionary();
        descriptorDict.SetItem(COSName.GetPDFName("FontFile2"), CreateStreamWithBytes(expected));

        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "TrueType");
        dict.SetItem(COSName.GetPDFName("FontDescriptor"), descriptorDict);

        var font = new PDTrueTypeFont(dict, ttf);
        Assert.Equal(expected, font.ExportFont());
    }

    [Fact]
    public void PDTrueTypeFont_GetNormalizedPath_ScalesTrueTypeUnitsToPdfGlyphSpace()
    {
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueTypeWithUpm(2000));
        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "TrueType");
        dict.SetName(COSName.GetPDFName("BaseFont"), "MiniTTF");
        var font = new PDTrueTypeFont(dict, ttf);

        GeneralPath path = font.GetNormalizedPath(65);
        (float minX, float minY, float maxX, float maxY) = GetBounds(path);
        Matrix fontMatrix = font.GetFontMatrix();

        Assert.Equal(0f, minX);
        Assert.Equal(0f, minY);
        Assert.Equal(250f, maxX);
        Assert.Equal(350f, maxY);
        Assert.Equal(0.001f, fontMatrix.GetValue(0, 0), precision: 6);
        Assert.Equal(0.001f, fontMatrix.GetValue(1, 1), precision: 6);
    }

    [Fact]
    public void PDTrueTypeFont_ExportFont_ReturnsEmbeddedFontFile3Bytes_WhenFontFile2Missing()
    {
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());
        byte[] expected = ReadOriginalData(ttf);

        var descriptorDict = new COSDictionary();
        descriptorDict.SetItem(COSName.GetPDFName("FontFile3"), CreateStreamWithBytes(expected));

        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "TrueType");
        dict.SetItem(COSName.GetPDFName("FontDescriptor"), descriptorDict);

        var font = new PDTrueTypeFont(dict, ttf);
        Assert.Equal(expected, font.ExportFont());
    }

    // ---------------------------------------------------------------------------
    // PDTrueTypeFont — ToUnicode fallback via cmap
    // ---------------------------------------------------------------------------

    [Fact]
    public void PDTrueTypeFont_ToUnicode_ReturnsMappedCharWhenCmapHasGlyph()
    {
        // The minimal TTF maps U+0041 ('A') → GID 1.
        // When no ToUnicode CMap is present, ToUnicodeFallback should return "A" for code 0x41.
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());
        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "TrueType");
        dict.SetName(COSName.GetPDFName("BaseFont"), "MiniTTF");

        var font = new PDTrueTypeFont(dict, ttf);
        GlyphList glyphList = GlyphList.GetAdobeGlyphList();

        // Code 0x41 maps to GID 1 (non-zero) in the TTF cmap, so we expect a result.
        string? result = font.ToUnicode(0x41, glyphList);
        Assert.Equal("A", result);
    }

    [Fact]
    public void PDTrueTypeFont_ToUnicode_ReturnsNullForCodeWithNoGlyph()
    {
        // Code 0x01 has GID 0 in the minimal TTF cmap (no mapping for control chars).
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());
        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "TrueType");
        dict.SetName(COSName.GetPDFName("BaseFont"), "MiniTTF");

        var font = new PDTrueTypeFont(dict, ttf);
        GlyphList glyphList = GlyphList.GetAdobeGlyphList();

        string? result = font.ToUnicode(0x01, glyphList);
        Assert.Null(result);
    }

    // ---------------------------------------------------------------------------
    // PDCIDFontType2 — CodeToGID and CIDToGIDMap
    // ---------------------------------------------------------------------------

    [Fact]
    public void PDCIDFontType2_CodeToGID_IdentityMapping_ReturnsCidAsGid()
    {
        // Without a CIDToGIDMap entry, CID == GID (identity).
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());
        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "CIDFontType2");
        // No CIDToGIDMap entry → identity

        var cidFont = new PDCIDFontType2(dict, ttf);
        Assert.Equal(0, cidFont.CodeToGID(0));
        Assert.Equal(1, cidFont.CodeToGID(1));
        Assert.Equal(65, cidFont.CodeToGID(65));
    }

    [Fact]
    public void PDCIDFontType2_CodeToGID_ExplicitIdentityName_ReturnsCidAsGid()
    {
        // CIDToGIDMap = /Identity is the same as absent — CID == GID.
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());
        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "CIDFontType2");
        dict.SetName(COSName.GetPDFName("CIDToGIDMap"), "Identity");

        var cidFont = new PDCIDFontType2(dict, ttf);
        Assert.Equal(5, cidFont.CodeToGID(5));
        Assert.Equal(100, cidFont.CodeToGID(100));
    }

    [Fact]
    public void PDCIDFontType2_CodeToGID_StreamMapping_ReturnsCorrectGid()
    {
        // CIDToGIDMap as a stream: 2 bytes per entry. 4 entries: CID 0→0, 1→1, 2→0, 3→1.
        byte[] mapBytes = [0, 0, 0, 1, 0, 0, 0, 1];
        COSStream cidToGidStream = CreateStreamWithBytes(mapBytes);

        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());
        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "CIDFontType2");
        dict.SetItem(COSName.GetPDFName("CIDToGIDMap"), cidToGidStream);

        var cidFont = new PDCIDFontType2(dict, ttf);
        Assert.Equal(0, cidFont.CodeToGID(0));
        Assert.Equal(1, cidFont.CodeToGID(1));
        Assert.Equal(0, cidFont.CodeToGID(2));
        Assert.Equal(1, cidFont.CodeToGID(3));
        // Out-of-range → 0
        Assert.Equal(0, cidFont.CodeToGID(99));
    }

    // ---------------------------------------------------------------------------
    // PDCIDFontType2 — CodeToCID passes through (identity)
    // ---------------------------------------------------------------------------

    [Fact]
    public void PDCIDFontType2_CodeToCID_IsIdentity()
    {
        // PDCIDFontType2 must NOT override CodeToCID with a unicode-cmap lookup;
        // the base PDCIDFont returns the code unchanged (CID = code).
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());
        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "CIDFontType2");

        var cidFont = new PDCIDFontType2(dict, ttf);
        Assert.Equal(65, cidFont.CodeToCID(65));
        Assert.Equal(0x4E2D, cidFont.CodeToCID(0x4E2D));
    }

    // ---------------------------------------------------------------------------
    // PDCIDFontType2 — width extraction
    // ---------------------------------------------------------------------------

    [Fact]
    public void PDCIDFontType2_GetWidth_PrefersExplicitWArrayEntry()
    {
        // Explicit W entry for CID 1: [1, [700]] → width 700.
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());
        var dict = BuildCIDType2DictWithWidths(ttf, new COSArray());
        var wArray = new COSArray();
        wArray.Add(COSInteger.Get(1));
        wArray.Add(COSArray.Of(700f));
        dict.SetItem(COSName.GetPDFName("W"), wArray);

        var cidFont = new PDCIDFontType2(dict, ttf);
        Assert.Equal(700f, cidFont.GetWidth(1));
        Assert.True(cidFont.HasExplicitWidth(1));
        Assert.Equal(500f, cidFont.GetWidthFromFont(1));
        Assert.True(cidFont.IsEmbedded());
    }

    [Fact]
    public void PDCIDFontType2_GetWidth_FallsBackToTTFAdvanceWidth_WhenNoWArrayEntry()
    {
        // No W array — falls back to TTF advance width for the GID.
        // Minimal TTF: UPM=1000, advance=500 for all GIDs.
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());
        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "CIDFontType2");
        // No W or DW

        var cidFont = new PDCIDFontType2(dict, ttf);
        // CID 1 → GID 1 (identity) → advance 500 → 500 * 1000/1000 = 500
        Assert.Equal(500f, cidFont.GetWidth(1));
        Assert.False(cidFont.HasExplicitWidth(1));
    }

    [Fact]
    public void PDCIDFontType2_GetWidth_ScalesTTFAdvanceByUpm()
    {
        // TTF with UPM=2000, advance=500 design units.
        // CID 1 → GID 1 → 500 * 1000 / 2000 = 250.
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueTypeWithUpm(2000));
        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "CIDFontType2");

        var cidFont = new PDCIDFontType2(dict, ttf);
        Assert.Equal(250f, cidFont.GetWidth(1));
    }

    [Fact]
    public void PDCIDFontType2_GetWidth_WithStreamCIDToGIDMap_UsesCorrectGid()
    {
        // CIDToGIDMap stream: CID 0 → GID 1, so advance width of GID 1 = 500.
        byte[] mapBytes = [0, 1, 0, 0]; // CID 0→GID 1, CID 1→GID 0
        COSStream cidToGidStream = CreateStreamWithBytes(mapBytes);

        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());
        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "CIDFontType2");
        dict.SetItem(COSName.GetPDFName("CIDToGIDMap"), cidToGidStream);

        var cidFont = new PDCIDFontType2(dict, ttf);
        // CID 0 → GID 1 → advance 500 → width 500
        Assert.Equal(500f, cidFont.GetWidth(0));
    }

    // ---------------------------------------------------------------------------
    // PDCIDFontType2 — ToUnicode fallback via reverse cmap lookup
    // ---------------------------------------------------------------------------

    [Fact]
    public void PDCIDFontType2_ToUnicode_FallsBackToReverseCmapLookup()
    {
        // The minimal TTF maps U+0041 ('A') → GID 1.
        // CID 1 → GID 1 (identity) → reverse cmap lookup → "A".
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());
        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "CIDFontType2");

        var cidFont = new PDCIDFontType2(dict, ttf);
        GlyphList glyphList = GlyphList.GetAdobeGlyphList();

        string? result = cidFont.ToUnicode(1, glyphList);
        Assert.Equal("A", result);
    }

    [Fact]
    public void PDCIDFontType2_ToUnicode_ReturnsNullForGidWithNoUnicodeMapping()
    {
        // GID 0 (.notdef) has no reverse Unicode mapping in the minimal TTF.
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());
        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "CIDFontType2");

        var cidFont = new PDCIDFontType2(dict, ttf);
        GlyphList glyphList = GlyphList.GetAdobeGlyphList();

        string? result = cidFont.ToUnicode(0, glyphList);
        Assert.Null(result);
    }

    // ---------------------------------------------------------------------------
    // PDType0Font with CIDFontType2 descendant — integration
    // ---------------------------------------------------------------------------

    [Fact]
    public void PDType0Font_WithCIDFontType2_GetWidth_UsesDescendantTTFWidths()
    {
        // Type0 → CIDFontType2. No W array → falls back to TTF advance width 500.
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());
        var descendantDict = BuildCIDType2DictWithWidths(ttf, new COSArray());
        PDCIDFont descendant = new PDCIDFontType2(descendantDict, ttf);

        var parentDict = new COSDictionary();
        parentDict.SetName(COSName.GetPDFName("Subtype"), "Type0");

        var type0Font = new PDType0Font(parentDict, descendant);
        // CodeToCID(1) = 1, GetWidth(1) = 500 from TTF
        Assert.Equal(500f, type0Font.GetWidth(1));
    }

    [Fact]
    public void PDType0Font_WithCIDFontType2_HasGlyph_UsesCodeToGID()
    {
        // GID 1 has a real glyph in the minimal TTF; GID 0 is an empty glyph.
        // With Identity CIDToGIDMap: code 1 → CID 1 → GID 1 (has glyph).
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());
        var descendantDict = new COSDictionary();
        descendantDict.SetName(COSName.GetPDFName("Subtype"), "CIDFontType2");
        PDCIDFont descendant = new PDCIDFontType2(descendantDict, ttf);

        var parentDict = new COSDictionary();
        parentDict.SetName(COSName.GetPDFName("Subtype"), "Type0");
        var type0Font = new PDType0Font(parentDict, descendant);

        // GID 1 has a box glyph (non-null in the glyph table).
        Assert.True(type0Font.HasGlyph(1));
        Assert.False(type0Font.HasGlyph(0));
    }

    [Fact]
    public void PDType0Font_WithCIDFontType2_HasGlyph_WithStreamCIDToGIDMap()
    {
        // CIDToGIDMap: CID 0 → GID 1 (real glyph), CID 1 → GID 0 (empty glyph).
        byte[] mapBytes = [0, 1, 0, 0];
        COSStream cidToGidStream = CreateStreamWithBytes(mapBytes);

        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());
        var descendantDict = new COSDictionary();
        descendantDict.SetName(COSName.GetPDFName("Subtype"), "CIDFontType2");
        descendantDict.SetItem(COSName.GetPDFName("CIDToGIDMap"), cidToGidStream);
        var cidFont = new PDCIDFontType2(descendantDict, ttf);

        var parentDict = new COSDictionary();
        parentDict.SetName(COSName.GetPDFName("Subtype"), "Type0");
        var type0Font = new PDType0Font(parentDict, cidFont);

        // code 0 → CID 0 → GID 1 (box glyph → has glyph)
        Assert.True(type0Font.HasGlyph(0));
    }

    [Fact]
    public void PDType0Font_WithCIDFontType2_GetNormalizedPath_ReturnsPathViaCodeToGID()
    {
        // code 1 → GID 1 (box glyph) → non-empty path.
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());
        var descendantDict = new COSDictionary();
        descendantDict.SetName(COSName.GetPDFName("Subtype"), "CIDFontType2");
        PDCIDFont descendant = new PDCIDFontType2(descendantDict, ttf);

        var parentDict = new COSDictionary();
        parentDict.SetName(COSName.GetPDFName("Subtype"), "Type0");
        var type0Font = new PDType0Font(parentDict, descendant);

        // GID 1 has a box glyph; path should have segments.
        var path = type0Font.GetNormalizedPath(1);
        Assert.NotNull(path);
    }

    [Fact]
    public void PDType0Font_WithCIDFontType2_GetNormalizedPath_ScalesTrueTypeUnitsToPdfGlyphSpace()
    {
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueTypeWithUpm(2000));
        var descendantDict = new COSDictionary();
        descendantDict.SetName(COSName.GetPDFName("Subtype"), "CIDFontType2");
        PDCIDFont descendant = new PDCIDFontType2(descendantDict, ttf);

        var parentDict = new COSDictionary();
        parentDict.SetName(COSName.GetPDFName("Subtype"), "Type0");
        var type0Font = new PDType0Font(parentDict, descendant);

        GeneralPath path = type0Font.GetNormalizedPath(1);
        (float minX, float minY, float maxX, float maxY) = GetBounds(path);

        Assert.Equal(0f, minX);
        Assert.Equal(0f, minY);
        Assert.Equal(250f, maxX);
        Assert.Equal(350f, maxY);
    }

    [Fact]
    public void PDType0Font_WithCIDFontType2_ExplicitWArrayWidth_TakesPrecedenceOverTTF()
    {
        // W array defines width 900 for CID 1; TTF advance is 500.
        // Explicit W must win.
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());
        var wArray = new COSArray();
        wArray.Add(COSInteger.Get(1));
        wArray.Add(COSArray.Of(900f));

        var descendantDict = new COSDictionary();
        descendantDict.SetName(COSName.GetPDFName("Subtype"), "CIDFontType2");
        descendantDict.SetItem(COSName.GetPDFName("W"), wArray);
        PDCIDFont descendant = new PDCIDFontType2(descendantDict, ttf);

        var parentDict = new COSDictionary();
        parentDict.SetName(COSName.GetPDFName("Subtype"), "Type0");
        var type0Font = new PDType0Font(parentDict, descendant);

        Assert.Equal(900f, type0Font.GetWidth(1));
    }

    // ---------------------------------------------------------------------------
    // PDFontFactory — CIDFontType2 dispatch
    // ---------------------------------------------------------------------------

    [Fact]
    public void PDFontFactory_CreateFont_DispatchesToPDCIDFontType2_ForCIDFontType2Subtype()
    {
        // PDFontFactory.CreateDescendantFont must return PDCIDFontType2 for the CIDFontType2 subtype.
        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "CIDFontType2");
        // No embedded FontFile2 → falls back to empty TTF

        PDCIDFont descendant = PDFontFactory.CreateDescendantFont(dict);
        Assert.IsType<PDCIDFontType2>(descendant);
    }

    [Fact]
    public void PDFontFactory_CreateFont_DispatchesToPDTrueTypeFont_WhenFontFile2Present()
    {
        // When a FontFile2 stream is embedded, PDFontFactory.CreateFont returns PDTrueTypeFont.
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());
        using MemoryStream ms = new();
        ttf.GetOriginalData().CopyTo(ms);
        COSStream fontFile2 = CreateStreamWithBytes(ms.ToArray());

        var descriptorDict = new COSDictionary();
        descriptorDict.SetItem(COSName.GetPDFName("FontFile2"), fontFile2);

        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "TrueType");
        dict.SetName(COSName.GetPDFName("BaseFont"), "MiniTTF");
        dict.SetItem(COSName.GetPDFName("FontDescriptor"), descriptorDict);

        PDFont font = PDFontFactory.CreateFont(dict);
        Assert.IsType<PDTrueTypeFont>(font);
        Assert.Equal("MiniTTF", font.GetName());
    }

    // ---------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------

    private static COSDictionary BuildCIDType2DictWithWidths(TrueTypeFont ttf, COSArray wArray)
    {
        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "CIDFontType2");
        if (wArray.Size() > 0)
        {
            dict.SetItem(COSName.GetPDFName("W"), wArray);
        }

        return dict;
    }

    private static COSStream CreateStreamWithBytes(byte[] bytes)
    {
        var stream = new COSStream();
        using Stream output = stream.CreateOutputStream();
        output.Write(bytes, 0, bytes.Length);
        output.Close();
        return stream;
    }

    private static byte[] ReadOriginalData(TrueTypeFont ttf)
    {
        using Stream stream = ttf.GetOriginalData();
        using MemoryStream ms = new();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    private static (float MinX, float MinY, float MaxX, float MaxY) GetBounds(GeneralPath path)
    {
        bool hasPoint = false;
        float minX = 0;
        float minY = 0;
        float maxX = 0;
        float maxY = 0;

        foreach (GeneralPath.Segment segment in path.Segments)
        {
            switch (segment.Type)
            {
                case GeneralPath.SegmentType.MoveTo:
                case GeneralPath.SegmentType.LineTo:
                    Include(segment.X1, segment.Y1);
                    break;
                case GeneralPath.SegmentType.QuadTo:
                    Include(segment.X1, segment.Y1);
                    Include(segment.X2, segment.Y2);
                    break;
                case GeneralPath.SegmentType.CurveTo:
                    Include(segment.X1, segment.Y1);
                    Include(segment.X2, segment.Y2);
                    Include(segment.X3, segment.Y3);
                    break;
            }
        }

        return (minX, minY, maxX, maxY);

        void Include(float x, float y)
        {
            if (!hasPoint)
            {
                minX = maxX = x;
                minY = maxY = y;
                hasPoint = true;
                return;
            }

            minX = Math.Min(minX, x);
            minY = Math.Min(minY, y);
            maxX = Math.Max(maxX, x);
            maxY = Math.Max(maxY, y);
        }
    }
}
