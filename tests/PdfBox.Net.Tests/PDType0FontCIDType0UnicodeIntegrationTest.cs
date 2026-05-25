/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted composite-font integration tests for PDType0/PDCIDFontType0 with AI assistance.
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

namespace PdfBox.Net.Tests;

public class PDType0FontCIDType0UnicodeIntegrationTest
{
    [Fact]
    public void PDType0Font_CodeToCID_UsesEncodingCMap_ForCIDFontType0Descendant()
    {
        var descendantDict = new COSDictionary();
        descendantDict.SetName(COSName.GetPDFName("Subtype"), "CIDFontType0");
        descendantDict.SetFloat(COSName.GetPDFName("DW"), 333f);

        var widths = new COSArray();
        widths.Add(COSInteger.Get(65));
        widths.Add(COSArray.Of(700f));
        descendantDict.SetItem(COSName.GetPDFName("W"), widths);

        var descendants = new COSArray();
        descendants.Add(descendantDict);

        var parentDict = new COSDictionary();
        parentDict.SetName(COSName.GetPDFName("Subtype"), "Type0");
        parentDict.SetItem(COSName.GetPDFName("Encoding"), CreateEncodingCMapStream("1 begincidrange\n<21> <21> 65\nendcidrange", "<00> <FF>"));
        parentDict.SetItem(COSName.GetPDFName("DescendantFonts"), descendants);

        PDFont font = PDFontFactory.CreateFont(parentDict);

        var type0 = Assert.IsType<PDType0Font>(font);
        Assert.Equal(65, type0.CodeToCID(0x21));
        Assert.Equal(700f, type0.GetWidth(0x21));
    }

    [Fact]
    public void PDType0Font_ToUnicode_UsesMappedCid_WithCIDFontType2DescendantFallback()
    {
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());
        var descendantDict = new COSDictionary();
        descendantDict.SetName(COSName.GetPDFName("Subtype"), "CIDFontType2");
        var descendant = new PDCIDFontType2(descendantDict, ttf);

        var parentDict = new COSDictionary();
        parentDict.SetName(COSName.GetPDFName("Subtype"), "Type0");
        parentDict.SetItem(COSName.GetPDFName("Encoding"), CreateEncodingCMapStream("1 begincidrange\n<21> <21> 1\nendcidrange", "<00> <FF>"));

        var type0 = new PDType0Font(parentDict, descendant);
        string? unicode = type0.ToUnicode(0x21, GlyphList.GetAdobeGlyphList());
        Assert.Equal("A", unicode);
    }

    [Fact]
    public void PDType0Font_IdentityH_ProvidesIdentityCodeToCID_WhenPredefinedCMapResourcesAreUnavailable()
    {
        var descendantDict = new COSDictionary();
        descendantDict.SetName(COSName.GetPDFName("Subtype"), "CIDFontType0");
        var descendant = new PDCIDFontType0(descendantDict);

        var parentDict = new COSDictionary();
        parentDict.SetName(COSName.GetPDFName("Subtype"), "Type0");
        parentDict.SetName(COSName.GetPDFName("Encoding"), "Identity-H");

        var type0 = new PDType0Font(parentDict, descendant);
        Assert.Equal(0x1234, type0.CodeToCID(0x1234));
    }

    [Fact]
    public void PDFontFactory_CreateDescendantFont_DispatchesToPDCIDFontType0()
    {
        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "CIDFontType0");

        PDCIDFont descendant = PDFontFactory.CreateDescendantFont(dict);
        Assert.IsType<PDCIDFontType0>(descendant);
    }

    private static COSStream CreateEncodingCMapStream(string cidMapping, string codeSpaceRange)
    {
        string cmap = $"""
/CIDInit /ProcSet findresource begin
12 dict begin
begincmap
/CIDSystemInfo << /Registry (Adobe) /Ordering (Identity) /Supplement 0 >> def
/CMapName /CustomIdentity def
/CMapType 1 def
1 begincodespacerange
{codeSpaceRange}
endcodespacerange
{cidMapping}
endcmap
CMapName currentdict /CMap defineresource pop
end
end
""";

        var stream = new COSStream();
        using Stream output = stream.CreateOutputStream();
        byte[] bytes = System.Text.Encoding.ASCII.GetBytes(cmap);
        output.Write(bytes, 0, bytes.Length);
        output.Close();
        return stream;
    }
}
