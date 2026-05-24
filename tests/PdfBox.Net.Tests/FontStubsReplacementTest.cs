/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Integration tests validating PDModel font implementation parity over former stubs.
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

public class FontStubsReplacementTest
{
    [Fact]
    public void Standard14Fonts_MapsAll14StandardFontNames()
    {
        string[] names =
        [
            "Times-Roman", "Times-Bold", "Times-Italic", "Times-BoldItalic",
            "Helvetica", "Helvetica-Bold", "Helvetica-Oblique", "Helvetica-BoldOblique",
            "Courier", "Courier-Bold", "Courier-Oblique", "Courier-BoldOblique",
            "Symbol", "ZapfDingbats",
        ];

        foreach (string name in names)
        {
            Assert.Equal(name, Standard14Fonts.GetMappedFontName(name));
            Assert.True(Standard14Fonts.IsStandard14Font(name));
        }
    }

    [Fact]
    public void PDFontDescriptor_ReturnsConfiguredMetrics()
    {
        var descriptorDict = new COSDictionary();
        descriptorDict.SetFloat(COSName.GetPDFName("CapHeight"), 700f);
        descriptorDict.SetFloat(COSName.GetPDFName("Ascent"), 800f);
        descriptorDict.SetFloat(COSName.GetPDFName("Descent"), -200f);
        descriptorDict.SetFloat(COSName.GetPDFName("StemV"), 80f);
        descriptorDict.SetFloat(COSName.GetPDFName("MissingWidth"), 555f);
        descriptorDict.SetItem(COSName.GetPDFName("FontBBox"), COSArray.Of(-10f, -20f, 900f, 880f));

        PDFontDescriptor descriptor = new(descriptorDict);

        Assert.Equal(700f, descriptor.GetCapHeight());
        Assert.Equal(800f, descriptor.GetAscent());
        Assert.Equal(-200f, descriptor.GetDescent());
        Assert.Equal(80f, descriptor.GetStemV());
        Assert.Equal(555f, descriptor.GetMissingWidth());

        var bbox = descriptor.GetFontBoundingBox();
        Assert.Equal(-10f, bbox.GetLowerLeftX());
        Assert.Equal(880f, bbox.GetUpperRightY());
    }

    [Fact]
    public void PDType1Font_UsesWidthsArrayFromFontDictionary()
    {
        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "Type1");
        dict.SetName(COSName.GetPDFName("BaseFont"), "Helvetica");
        dict.SetInt(COSName.GetPDFName("FirstChar"), 65);
        dict.SetInt(COSName.GetPDFName("LastChar"), 66);

        var widths = new COSArray();
        widths.Add(new COSFloat(610f));
        widths.Add(new COSFloat(620f));
        dict.SetItem(COSName.GetPDFName("Widths"), widths);

        PDFont font = PDFontFactory.CreateFont(dict);

        Assert.IsType<PDType1Font>(font);
        Assert.Equal(610f, font.GetWidth(65));
        Assert.Equal(620f, font.GetWidth(66));
    }

    [Fact]
    public void PDTrueTypeFont_UsesWidthsArrayFromFontDictionary()
    {
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());

        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "TrueType");
        dict.SetName(COSName.GetPDFName("BaseFont"), "MiniTTF");
        dict.SetInt(COSName.GetPDFName("FirstChar"), 65);
        dict.SetInt(COSName.GetPDFName("LastChar"), 65);

        var widths = new COSArray();
        widths.Add(new COSFloat(700f));
        dict.SetItem(COSName.GetPDFName("Widths"), widths);

        PDFont font = new PDTrueTypeFont(dict, ttf);

        Assert.Equal(700f, font.GetWidth(65));
        Assert.Equal("MiniTTF", font.GetName());
    }

    [Fact]
    public void PDFont_ToUnicode_UsesToUnicodeCMap()
    {
        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "Type1");
        dict.SetName(COSName.GetPDFName("BaseFont"), "Helvetica");
        dict.SetItem(COSName.GetPDFName("ToUnicode"), CreateToUnicodeStream("<41> <0041>\n<42> <0042>"));

        PDFont font = PDFontFactory.CreateFont(dict);
        GlyphList glyphList = GlyphList.GetAdobeGlyphList();

        Assert.Equal("A", font.ToUnicode(0x41, glyphList));
        Assert.Equal("B", font.ToUnicode(0x42, glyphList));
    }

    [Fact]
    public void PDCIDFontType2_GetTrueTypeFont_ReturnsUnderlyingFont()
    {
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());
        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "CIDFontType2");

        var cidFont = new PDCIDFontType2(dict, ttf);

        Assert.Same(ttf, cidFont.GetTrueTypeFont());
        Assert.Equal(1000, cidFont.GetTrueTypeFont().GetUnitsPerEm());
    }

    private static COSStream CreateToUnicodeStream(string bfCharLines)
    {
        string cmap = $"""
/CIDInit /ProcSet findresource begin
12 dict begin
begincmap
/CIDSystemInfo << /Registry (Adobe) /Ordering (UCS) /Supplement 0 >> def
/CMapName /Adobe-Identity-UCS def
/CMapType 2 def
1 begincodespacerange
<00> <FF>
endcodespacerange
2 beginbfchar
{bfCharLines}
endbfchar
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
