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
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Font.Encoding;
using PdfBox.Net.Util;

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

    [Theory]
    [InlineData("CourierCourierNew", "Courier")]
    [InlineData("Times", "Times-Roman")]
    [InlineData("Times,BoldItalic", "Times-BoldItalic")]
    [InlineData("Symbol,BoldItalic", "Symbol")]
    [InlineData("ArialMT", "Helvetica")]
    [InlineData("Arial-BoldItalicMT", "Helvetica-BoldOblique")]
    public void Standard14Fonts_MapsKnownAliases(string alias, string expectedCanonical)
    {
        Assert.Equal(expectedCanonical, Standard14Fonts.GetMappedFontName(alias));
        Assert.True(Standard14Fonts.IsStandard14Font(alias));
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
    public void PDFontDescriptor_NormalizesMetricsAndExposesFlags()
    {
        var descriptorDict = new COSDictionary();
        descriptorDict.SetFloat(COSName.GetPDFName("CapHeight"), -700f);
        descriptorDict.SetFloat(COSName.GetPDFName("XHeight"), -450f);
        descriptorDict.SetInt(COSName.GetPDFName("Flags"), 1 | 4 | 64);
        descriptorDict.SetString(COSName.GetPDFName("FontFamily"), "Mini Family");
        descriptorDict.SetName(COSName.GetPDFName("FontName"), "MiniPS");
        descriptorDict.SetFloat(COSName.GetPDFName("AvgWidth"), 520f);

        PDFontDescriptor descriptor = new(descriptorDict);

        Assert.Equal(700f, descriptor.GetCapHeight());
        Assert.Equal(450f, descriptor.GetXHeight());
        Assert.Equal("MiniPS", descriptor.GetFontName());
        Assert.Equal("Mini Family", descriptor.GetFontFamily());
        Assert.True(descriptor.HasWidths());
        Assert.True(descriptor.IsFixedPitch());
        Assert.True(descriptor.IsSymbolic());
        Assert.True(descriptor.IsItalic());
        Assert.False(descriptor.IsNonSymbolic());
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
    public void PDType1Font_GetSpaceWidth_UsesEncodingSpaceCode()
    {
        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "Type1");
        dict.SetName(COSName.GetPDFName("BaseFont"), "Helvetica");
        dict.SetInt(COSName.GetPDFName("FirstChar"), 1);
        dict.SetInt(COSName.GetPDFName("LastChar"), 32);

        var widths = new COSArray();
        widths.Add(new COSFloat(250f));
        for (int i = 2; i < 32; i++)
        {
            widths.Add(new COSFloat(500f));
        }

        widths.Add(new COSFloat(1000f));
        dict.SetItem(COSName.GetPDFName("Widths"), widths);

        var differences = new COSArray();
        differences.Add(COSInteger.Get(1));
        differences.Add(COSName.GetPDFName("space"));
        var encoding = new COSDictionary();
        encoding.SetItem(COSName.GetPDFName("Differences"), differences);
        dict.SetItem(COSName.GetPDFName("Encoding"), encoding);

        PDFont font = PDFontFactory.CreateFont(dict);

        Assert.Equal(250f, font.GetSpaceWidth());
        Assert.Equal(1000f, font.GetWidth(32));
    }

    [Fact]
    public void PDType3Font_GetDisplacement_AppliesFontMatrix()
    {
        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "Type3");
        dict.SetInt(COSName.GetPDFName("FirstChar"), 1);
        dict.SetInt(COSName.GetPDFName("LastChar"), 1);
        dict.SetItem(COSName.GetPDFName("FontMatrix"), COSArray.Of(0.01724f, 0f, 0f, -0.01724f, 0f, 0f));

        var widths = new COSArray();
        widths.Add(new COSFloat(38f));
        dict.SetItem(COSName.GetPDFName("Widths"), widths);

        PDFont font = PDFontFactory.CreateFont(dict);
        Vector displacement = font.GetDisplacement(1);

        Assert.Equal(38f * 0.01724f, displacement.GetX(), precision: 5);
        Assert.Equal(0f, displacement.GetY());
    }

    [Fact]
    public void PDType1Font_Standard14ConstructorSetsBaseFont()
    {
        PDType1Font font = new(PDType1Font.FontName.HELVETICA_BOLD);

        Assert.Equal("Helvetica-Bold", font.GetName());
        Assert.True(font.IsStandard14());
    }

    [Fact]
    public void PDType1Font_Standard14UsesEmbeddedAfmWidths()
    {
        PDType1Font font = new(PDType1Font.FontName.HELVETICA);

        Assert.Equal(278f, font.GetWidth(' '));
        Assert.Equal(500f, font.GetWidth('x'));
    }

    [Fact]
    public void PDType1Font_Standard14DictionaryWithoutEncodingUsesAfmEncoding()
    {
        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "Type1");
        dict.SetName(COSName.GetPDFName("BaseFont"), "Helvetica");

        PDFont font = PDFontFactory.CreateFont(dict);
        GlyphList glyphList = GlyphList.GetAdobeGlyphList();

        Assert.Equal("\u2019", font.ToUnicode(39, glyphList));
    }

    [Fact]
    public void PDType1Font_Standard14UsesEmbeddedAfmBoundingBox()
    {
        PDType1Font font = new(PDType1Font.FontName.HELVETICA);

        var bbox = font.GetBoundingBox();

        Assert.Equal(-166f, bbox.GetLowerLeftX());
        Assert.Equal(-225f, bbox.GetLowerLeftY());
        Assert.Equal(1000f, bbox.GetUpperRightX());
        Assert.Equal(931f, bbox.GetUpperRightY());
    }

    [Fact]
    public void PDType0Font_LoadFromStream_ReturnsFontWithType2Descendant()
    {
        using PDDocument document = new();
        using MemoryStream input = new(FontBoxTestFixtures.CreateMinimalTrueType());

        PDType0Font font = PDType0Font.Load(document, input);

        Assert.Equal("Type0", ((COSDictionary)font.GetCOSObject()).GetNameAsString(COSName.SUBTYPE));
        Assert.IsType<PDCIDFontType2>(font.GetDescendantFont());
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
    public void PDTrueTypeFont_MacRomanEncodingMapsGuillemetsThroughGlyphList()
    {
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());

        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "TrueType");
        dict.SetName(COSName.GetPDFName("BaseFont"), "MiniTTF");
        dict.SetName(COSName.GetPDFName("Encoding"), "MacRomanEncoding");

        PDFont font = new PDTrueTypeFont(dict, ttf);
        GlyphList glyphList = GlyphList.GetAdobeGlyphList();

        Assert.Equal("\u00AB", font.ToUnicode(199, glyphList));
        Assert.Equal("\u00BB", font.ToUnicode(200, glyphList));
    }

    [Fact]
    public void PDDictionaryFont_WinAnsiEncodingMapsSmartPunctuationThroughGlyphList()
    {
        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "TrueType");
        dict.SetName(COSName.GetPDFName("BaseFont"), "MissingEmbeddedTimes");
        dict.SetName(COSName.GetPDFName("Encoding"), "WinAnsiEncoding");

        PDFont font = PDFontFactory.CreateFont(dict);
        GlyphList glyphList = GlyphList.GetAdobeGlyphList();

        Assert.IsType<PDDictionaryFont>(font);
        Assert.Equal("\u2019", font.ToUnicode(0x92, glyphList));
        Assert.Equal("\u2014", font.ToUnicode(0x97, glyphList));
    }

    [Fact]
    public void PDDictionaryFont_GetSpaceWidth_UsesExplicitWidths()
    {
        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "TrueType");
        dict.SetName(COSName.GetPDFName("BaseFont"), "MissingEmbeddedTimes");
        dict.SetInt(COSName.GetPDFName("FirstChar"), 32);
        dict.SetInt(COSName.GetPDFName("LastChar"), 33);
        dict.SetItem(COSName.GetPDFName("Widths"), COSArray.Of(249f, 333f));

        PDFont font = PDFontFactory.CreateFont(dict);

        Assert.IsType<PDDictionaryFont>(font);
        Assert.Equal(249f, font.GetSpaceWidth());
    }

    [Fact]
    public void PDDictionaryFont_GetSpaceWidth_FallsBackWhenMetricsAreMissing()
    {
        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "TrueType");
        dict.SetName(COSName.GetPDFName("BaseFont"), "MissingEmbeddedTimes");

        PDFont font = PDFontFactory.CreateFont(dict);

        Assert.IsType<PDDictionaryFont>(font);
        Assert.Equal(20f, font.GetSpaceWidth());
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
    public void PDType1Font_UsesEmbeddedType1EncodingWhenDictionaryEncodingMissing()
    {
        var descriptor = new COSDictionary();
        descriptor.SetItem(COSName.GetPDFName("FontFile"), CreateFontFileStream(FontBoxTestFixtures.CreateMinimalType1Pfb()));

        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "Type1");
        dict.SetName(COSName.GetPDFName("BaseFont"), "TestFont");
        dict.SetItem(COSName.GetPDFName("FontDescriptor"), descriptor);

        PDFont font = PDFontFactory.CreateFont(dict);
        GlyphList glyphList = GlyphList.GetAdobeGlyphList();

        Assert.Equal("A", font.ToUnicode(65, glyphList));
        Assert.Null(font.ToUnicode(33, glyphList));
    }

    [Fact]
    public void PDType1Font_StreamConstructor_EmbedsFontProgramAndMetrics()
    {
        using PDDocument document = new();
        using MemoryStream pfb = new(FontBoxTestFixtures.CreateMinimalType1Pfb());
        PDType1Font font = new(document, pfb);

        COSDictionary fontDict = (COSDictionary)font.GetCOSObject();
        Assert.Equal("Type1", fontDict.GetNameAsString(COSName.SUBTYPE));
        Assert.Equal("TestFont", fontDict.GetNameAsString(COSName.GetPDFName("BaseFont")));
        Assert.Equal(0, fontDict.GetInt(COSName.GetPDFName("FirstChar"), -1));
        Assert.Equal(255, fontDict.GetInt(COSName.GetPDFName("LastChar"), -1));

        COSArray widths = Assert.IsType<COSArray>(fontDict.GetDictionaryObject(COSName.GetPDFName("Widths")));
        Assert.Equal(256, widths.Size());
        Assert.Equal(font.GetWidth(65), (widths.GetObject(65) as COSNumber)?.FloatValue() ?? -1f);

        COSDictionary descriptor = Assert.IsType<COSDictionary>(fontDict.GetDictionaryObject(COSName.GetPDFName("FontDescriptor")));
        COSStream fontFile = Assert.IsType<COSStream>(descriptor.GetDictionaryObject(COSName.GetPDFName("FontFile")));
        Assert.True(fontFile.GetInt(COSName.GetPDFName("Length1"), 0) > 0);
        Assert.True(fontFile.GetInt(COSName.GetPDFName("Length2"), 0) > 0);

        GlyphList glyphList = GlyphList.GetAdobeGlyphList();
        Assert.Equal("A", font.ToUnicode(65, glyphList));
    }

    [Fact]
    public void DictionaryEncoding_UsesZapfDingbatsEncodingForZapfBase14Font()
    {
        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("BaseFont"), "ZapfDingbats");

        PdfBox.Net.PDModel.Font.Encoding.Encoding encoding = DictionaryEncoding.ResolveEncoding(dict);

        Assert.Equal("a1", encoding.GetName(33));
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

    [Fact]
    public void PDFontFactory_Type0_UsesDescendantDescriptorAndWidths()
    {
        var descriptorDict = new COSDictionary();
        descriptorDict.SetFloat(COSName.GetPDFName("MissingWidth"), 333f);
        descriptorDict.SetItem(COSName.GetPDFName("FontBBox"), COSArray.Of(0f, -10f, 900f, 880f));

        var descendantDict = new COSDictionary();
        descendantDict.SetName(COSName.GetPDFName("Subtype"), "CIDFontType0");
        descendantDict.SetFloat(COSName.GetPDFName("DW"), 444f);
        descendantDict.SetItem(COSName.GetPDFName("FontDescriptor"), descriptorDict);

        var widths = new COSArray();
        widths.Add(COSInteger.Get(65));
        widths.Add(COSArray.Of(500f, 510f));
        widths.Add(COSInteger.Get(90));
        widths.Add(COSInteger.Get(91));
        widths.Add(new COSFloat(620f));
        descendantDict.SetItem(COSName.GetPDFName("W"), widths);

        var descendants = new COSArray();
        descendants.Add(descendantDict);

        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Subtype"), "Type0");
        dict.SetItem(COSName.GetPDFName("DescendantFonts"), descendants);

        PDFont font = PDFontFactory.CreateFont(dict);

        Assert.IsType<PDType0Font>(font);
        Assert.Equal(500f, font.GetWidth(65));
        Assert.Equal(510f, font.GetWidth(66));
        Assert.Equal(620f, font.GetWidth(90));
        Assert.Equal(620f, font.GetWidth(91));
        Assert.Equal(444f, font.GetWidth(120));
        Assert.Equal(333f, font.GetFontDescriptor()!.GetMissingWidth());

        var bbox = font.GetBoundingBox();
        Assert.Equal(-10f, bbox.GetLowerLeftY());
        Assert.Equal(900f, bbox.GetUpperRightX());
    }

    [Fact]
    public void FileSystemFontProvider_PrefersStableSortedFontPath()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        string firstDir = Path.Combine(root, "a");
        string secondDir = Path.Combine(root, "b");
        Directory.CreateDirectory(firstDir);
        Directory.CreateDirectory(secondDir);

        try
        {
            File.WriteAllBytes(Path.Combine(secondDir, "MiniFont.ttf"), FontBoxTestFixtures.CreateMinimalTrueType());
            string preferredPath = Path.Combine(firstDir, "MiniFont.ttf");
            File.WriteAllBytes(preferredPath, FontBoxTestFixtures.CreateMinimalTrueType());

            FileSystemFontProvider provider = new([root]);

            Assert.Equal(preferredPath, provider.FindFontFile("MiniFont"));
            Assert.Equal(preferredPath, provider.FindFontFile("ABCDEF+MiniFont"));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
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

    private static COSStream CreateFontFileStream(byte[] bytes)
    {
        var stream = new COSStream();
        using Stream output = stream.CreateOutputStream();
        output.Write(bytes, 0, bytes.Length);
        output.Close();
        return stream;
    }
}
