/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * API parity tests for PDModel font issue #508 with AI assistance.
 *
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;
using PdfBox.Net.FontBox.TTF;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Font.Encoding;
using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.Tests;

public class Issue508PDModelFontApiParityTest
{
    [Fact]
    public void PDFontLike_ExposesImplementedFontMetricsContract()
    {
        PDFontLike font = new PDType1Font(PDType1Font.FontName.HELVETICA);

        Assert.Equal("Helvetica", font.GetName());
        Assert.False(font.IsEmbedded());
        Assert.False(font.IsDamaged());
        Assert.NotNull(font.GetFontMatrix());
        Assert.NotNull(font.GetBoundingBox());
        Assert.Equal(font.GetBoundingBox().GetHeight(), font.GetHeight(65));
        Assert.True(font.GetWidth(65) >= 0f);
        Assert.True(font.GetAverageFontWidth() >= 0f);
        Assert.NotNull(font.GetPositionVector(65));

        PDType1Font type1Font = Assert.IsType<PDType1Font>(font);
        Assert.Equal("A", type1Font.CodeToName(65));
        Assert.False(type1Font.HasGlyph(".notdef"));
    }

    [Fact]
    public void Encoding_MetadataApis_MirrorJavaNamedEncodings()
    {
        Assert.Equal("WinAnsiEncoding", WinAnsiEncoding.INSTANCE.GetEncodingName());
        Assert.Equal("WinAnsiEncoding", Assert.IsType<COSName>(WinAnsiEncoding.INSTANCE.GetCOSObject()).GetName());
        Assert.Equal("MacExpertEncoding", new MacExpertEncoding().GetEncodingName());
        Assert.Equal("MacOSRomanEncoding", Assert.IsType<COSName>(new MacOSRomanEncoding().GetCOSObject()).GetName());

        COSArray differences = new();
        differences.Add(COSInteger.Get(65));
        differences.Add(COSName.GetPDFName("Alpha"));
        DictionaryEncoding dictionaryEncoding = new(COSName.GetPDFName("WinAnsiEncoding"), differences);

        Assert.Equal("Alpha", dictionaryEncoding.GetName(65));
        Assert.Equal("WinAnsiEncoding with differences", dictionaryEncoding.GetEncodingName());
        Assert.Equal("WinAnsiEncoding", dictionaryEncoding.GetBaseEncoding()!.GetEncodingName());
        Assert.Equal("Alpha", dictionaryEncoding.GetDifferences()[65]);
        Assert.IsType<COSDictionary>(dictionaryEncoding.GetCOSObject());

        Type1Encoding type1Encoding = Type1Encoding.FromFontBox(PdfBox.Net.FontBox.Encoding.StandardEncoding.INSTANCE);
        Assert.Equal("A", type1Encoding.GetName(65));
        Assert.Equal("built-in (Type 1)", type1Encoding.GetEncodingName());
        Assert.Null(type1Encoding.GetCOSObject());

        BuiltInEncoding builtInEncoding = new(new Dictionary<int, string> { [1] = "one" });
        Assert.Equal("built-in (TTF)", builtInEncoding.GetEncodingName());
        Assert.Throws<NotSupportedException>(() => builtInEncoding.GetCOSObject());
    }

    [Fact]
    public void PDPanose_UsesTwelveByteFamilyClassAndClassificationLayout()
    {
        byte[] bytes = [0x01, 0x02, 2, 11, 5, 3, 4, 7, 8, 9, 10, 6];
        PDPanose panose = new(bytes);

        Assert.Equal(12, PDPanose.LENGTH);
        Assert.Equal(12, PDPanose.PanoseLength);
        Assert.Equal(0x0102, panose.GetFamilyClass());
        Assert.Equal(bytes, panose.GetBytes());
        Assert.Equal(bytes[2..], panose.GetPanose().GetBytes());
        Assert.Equal(2, panose.FamilyKind);
        Assert.Equal(11, panose.SerifStyle);
        Assert.Equal(6, panose.XHeight);

        PDPanose signedFamilyClass = new([0xff, 0x80, 2, 11, 5, 3, 4, 7, 8, 9, 10, 6]);
        Assert.Equal(-128, signedFamilyClass.GetFamilyClass());
    }

    [Fact]
    public void TrueTypeAndCidType2_PublicGlyphHelpers_UseExistingGlyphData()
    {
        TrueTypeFont ttf = new TTFParser().Parse(FontBoxTestFixtures.CreateMinimalTrueType());

        COSDictionary simpleDictionary = new();
        simpleDictionary.SetName(COSName.SUBTYPE, "TrueType");
        simpleDictionary.SetName(COSName.GetPDFName("BaseFont"), "MiniTTF");
        PDTrueTypeFont trueTypeFont = new(simpleDictionary, ttf);

        Assert.False(trueTypeFont.IsDamaged());
        Assert.Equal(1, trueTypeFont.CodeToGID(65));
        AssertPathPresent(trueTypeFont.GetPath(65));
        Assert.NotNull(trueTypeFont.GetPath("A"));

        COSDictionary cidDictionary = new();
        cidDictionary.SetName(COSName.SUBTYPE, "CIDFontType2");
        PDCIDFontType2 cidFont = new(cidDictionary, ttf);

        Assert.False(cidFont.IsDamaged());
        Assert.True(cidFont.HasGlyph(1));
        AssertPathPresent(cidFont.GetPath(1));
        Assert.Equal([0x12, 0x34], cidFont.EncodeGlyphId(0x1234));

        COSDictionary type0Dictionary = new();
        type0Dictionary.SetName(COSName.SUBTYPE, "Type0");
        type0Dictionary.SetName(COSName.GetPDFName("BaseFont"), "MiniTTF");
        type0Dictionary.SetName(COSName.GetPDFName("Encoding"), "Identity-H");
        PDType0Font type0Font = new(type0Dictionary, cidFont);

        Assert.Equal("MiniTTF", type0Font.GetBaseFont());
        Assert.Equal("MiniTTF", type0Font.GetName());
        Assert.False(type0Font.IsDamaged());
        Assert.False(type0Font.IsStandard14());
        Assert.Contains("PostScript name: MiniTTF", type0Font.ToString());
        Assert.Equal(cidFont.CodeToGID(1), type0Font.CodeToGID(1));
        AssertPathPresent(type0Font.GetPath(1));
        Assert.Equal(type0Font.ToUnicode(1, GlyphList.GetAdobeGlyphList()), type0Font.ToUnicode(1));
    }

    private static void AssertPathPresent(GeneralPath path)
    {
        Assert.NotNull(path);
        Assert.NotEmpty(path.Segments);
    }
}
