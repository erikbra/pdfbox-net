/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted pdmodel.font backfill regression tests with AI assistance.
 *
 * PORT_MODE: adapted
 */

using System.Text;
using PdfBox.Net.COS;
using PdfBox.Net.FontBox;
using PdfBox.Net.FontBox.Util;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Font.Encoding;
using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.Tests;

public class PDModelFontBackfillTest
{
    [Fact]
    public void PDFontFactory_Type3_ReturnsConcreteType3Font_AndUsesCharProcMetrics()
    {
        var fontDict = new COSDictionary();
        fontDict.SetName(COSName.SUBTYPE, "Type3");
        fontDict.SetName(COSName.NAME, "MiniType3");
        fontDict.SetItem(COSName.GetPDFName("FontMatrix"), COSArray.Of(0.001f, 0f, 0f, 0.001f, 0f, 0f));
        fontDict.SetItem(COSName.GetPDFName("FontBBox"), COSArray.Of(0f, 0f, 0f, 0f));
        fontDict.SetInt(COSName.GetPDFName("FirstChar"), 65);
        fontDict.SetInt(COSName.GetPDFName("LastChar"), 65);
        fontDict.SetName(COSName.GetPDFName("Encoding"), "WinAnsiEncoding");

        var charProcs = new COSDictionary();
        charProcs.SetItem(COSName.GetPDFName("A"), CreateContentStream("500 0 -10 -20 200 300 d1"));
        fontDict.SetItem(COSName.GetPDFName("CharProcs"), charProcs);

        PDFont font = PDFontFactory.CreateFont(fontDict);

        var type3 = Assert.IsType<PDType3Font>(font);
        Assert.Equal("MiniType3", type3.GetName());
        Assert.True(type3.HasGlyph(65));
        Assert.Equal(500f, type3.GetWidth(65));
        BoundingBox bbox = type3.GetBoundingBox();
        Assert.Equal(-10f, bbox.GetLowerLeftX());
        Assert.Equal(300f, bbox.GetUpperRightY());
    }

    [Fact]
    public void PDType3Font_MissingName_UsesTypeSafeFallbacksWithoutFontBox()
    {
        COSDictionary fontDict = new();
        fontDict.SetName(COSName.SUBTYPE, "Type3");
        fontDict.SetName(COSName.GetPDFName("Encoding"), "WinAnsiEncoding");
        COSDictionary charProcs = new();
        charProcs.SetItem(COSName.GetPDFName("A"), CreateContentStream("500 0 d0"));
        fontDict.SetItem(COSName.GetPDFName("CharProcs"), charProcs);

        PDType3Font unnamed = new(fontDict);

        Assert.Equal("Type3", unnamed.GetName());
        Assert.True(unnamed.HasGlyph("A"));
        Assert.False(unnamed.HasGlyph("missing"));

        fontDict.SetName(COSName.GetPDFName("BaseFont"), "Type3FallbackName");
        PDType3Font baseFontNamed = new(fontDict);

        Assert.Equal("Type3FallbackName", baseFontNamed.GetName());
        Assert.Throws<NotSupportedException>(() => baseFontNamed.GetFontBoxFont());
    }

    [Fact]
    public void PDType3CharProc_ReadsWidthAndGlyphBBox()
    {
        var fontDict = new COSDictionary();
        fontDict.SetName(COSName.SUBTYPE, "Type3");
        fontDict.SetName(COSName.GetPDFName("Encoding"), "WinAnsiEncoding");
        PDType3Font font = new(fontDict);
        PDType3CharProc charProc = new(font, CreateContentStream("600 0 1 2 11 22 d1"));

        Assert.Equal(600f, charProc.GetWidth());
        var bbox = Assert.IsType<PdfBox.Net.PDModel.Common.PDRectangle>(charProc.GetGlyphBBox());
        Assert.Equal(1f, bbox.GetLowerLeftX());
        Assert.Equal(20f, bbox.GetHeight());
    }

    [Fact]
    public void PDFontFactory_MMType1_ReturnsPDMMType1Font()
    {
        var dict = new COSDictionary();
        dict.SetName(COSName.SUBTYPE, "MMType1");
        dict.SetName(COSName.GetPDFName("BaseFont"), "Helvetica");

        PDFont font = PDFontFactory.CreateFont(dict);

        Assert.IsType<PDMMType1Font>(font);
    }

    [Fact]
    public void PDFontFactory_Type1_WithEmbeddedFontFile3_ReturnsPDType1CFont()
    {
        var descriptor = new COSDictionary();
        descriptor.SetItem(COSName.GetPDFName("FontFile3"), CreateBinaryStream(FontBoxTestFixtures.CreateMinimalOpenTypeCff()));

        var dict = new COSDictionary();
        dict.SetName(COSName.SUBTYPE, "Type1");
        dict.SetName(COSName.GetPDFName("BaseFont"), "MiniCFF");
        dict.SetItem(COSName.GetPDFName("FontDescriptor"), descriptor);

        PDFont font = PDFontFactory.CreateFont(dict);

        var type1C = Assert.IsType<PDType1CFont>(font);
        Assert.Equal("A", type1C.ToUnicode(65, GlyphList.GetAdobeGlyphList()));
        Assert.True(type1C.GetWidth(65) >= 0f);
    }

    [Fact]
    public void ToUnicodeWriter_WritesRangesAndBatches()
    {
        ToUnicodeWriter writer = new();
        for (int i = 0; i < ToUnicodeWriter.MAX_ENTRIES_PER_OPERATOR + 1; i++)
        {
            writer.Add(i, "A");
        }

        using MemoryStream stream = new();
        writer.WriteTo(stream);
        string text = System.Text.Encoding.ASCII.GetString(stream.ToArray());

        Assert.Contains("beginbfrange", text);
        Assert.Contains("endbfrange", text);
        Assert.Contains("<0000>", text);
        Assert.Equal(2, text.Split("beginbfrange").Length - 1);
    }

    [Theory]
    [InlineData(0x1, "uni0001")]
    [InlineData(0x41, "uni0041")]
    [InlineData(0x123, "uni0123")]
    [InlineData(0x1F600, "uni1F600")]
    public void UniUtil_FormatsUnicodeNames(int codePoint, string expected)
    {
        Assert.Equal(expected, UniUtil.GetUniNameOfCodePoint(codePoint));
    }

    [Fact]
    public void DictionaryEncoding_ResolvesAdditionalNamedEncodings()
    {
        var dict = new COSDictionary();
        dict.SetName(COSName.GetPDFName("Encoding"), "StandardEncoding");
        Assert.Equal("A", DictionaryEncoding.ResolveEncoding(dict).GetName(65));

        dict.SetName(COSName.GetPDFName("Encoding"), "MacOSRomanEncoding");
        Assert.Equal("apple", DictionaryEncoding.ResolveEncoding(dict).GetName(240));

        dict.SetName(COSName.GetPDFName("Encoding"), "MacExpertEncoding");
        Assert.Equal("AEsmall", DictionaryEncoding.ResolveEncoding(dict).GetName(190));
    }

    [Fact]
    public void BuiltInEncoding_UsesProvidedMappings()
    {
        BuiltInEncoding encoding = new(new Dictionary<int, string> { [7] = "seven" });
        Assert.Equal("seven", encoding.GetName(7));
    }

    [Fact]
    public void FontCache_ReturnsCachedFont()
    {
        FontCache cache = new();
        TestFontInfo info = new();
        DummyFontBoxFont font = new();

        cache.AddFont(info, font);

        Assert.Same(font, cache.GetFont(info));
    }

    private static COSStream CreateContentStream(string content)
    {
        return CreateBinaryStream(System.Text.Encoding.ASCII.GetBytes(content));
    }

    private static COSStream CreateBinaryStream(byte[] bytes)
    {
        var stream = new COSStream();
        using Stream output = stream.CreateOutputStream();
        output.Write(bytes, 0, bytes.Length);
        output.Close();
        return stream;
    }

    private sealed class DummyFontBoxFont : FontBoxFont
    {
        public string GetName() => "Dummy";
        public BoundingBox GetFontBBox() => new(0, 0, 10, 10);
        public IList<float> GetFontMatrix() => [0.001f, 0f, 0f, 0.001f, 0f, 0f];
        public GeneralPath GetPath(string name) => new();
        public float GetWidth(string name) => 10f;
        public bool HasGlyph(string name) => true;
    }

    private sealed class TestFontInfo : FontInfo
    {
        private readonly DummyFontBoxFont _font = new();

        public override string GetPostScriptName() => "DummyPS";
        public override FontFormat GetFormat() => FontFormat.TTF;
        public override PDCIDSystemInfo? GetCIDSystemInfo() => null;
        public override FontBoxFont GetFont() => _font;
        public override int GetFamilyClass() => 0;
        public override int GetWeightClass() => 400;
        public override int GetCodePageRange1() => 0;
        public override int GetCodePageRange2() => 0;
        public override int GetMacStyle() => 0;
        public override PDPanoseClassification? GetPanose() => null;
    }
}
