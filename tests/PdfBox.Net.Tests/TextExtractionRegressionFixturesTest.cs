using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.Text;
using System.Text;

namespace PdfBox.Net.Tests;

public class TextExtractionRegressionFixturesTest
{
    [Fact]
    public void PDFTextStripper_GetText_SimpleFixture_MatchesExpected()
    {
        using PDDocument document = CreateFixtureDocument("simple-content.txt");
        string extracted = new PDFTextStripper().GetText(document);

        Assert.Equal(ReadFixtureText("simple-expected.txt"), NormalizeLineEndings(extracted));
    }

    [Fact]
    public void PDFTextStripper_GetText_LineBreakFixture_MatchesExpected()
    {
        using PDDocument document = CreateFixtureDocument("line-breaks-content.txt");
        string extracted = new PDFTextStripper().GetText(document);

        Assert.Equal(ReadFixtureText("line-breaks-expected.txt"), NormalizeLineEndings(extracted));
    }

    [Fact]
    public void PDFTextStripper_GetText_SpacingSensitiveFixture_MatchesExpected()
    {
        using PDDocument document = CreateFixtureDocument("spacing-sensitive-content.txt");
        string extracted = new PDFTextStripper().GetText(document);

        Assert.Equal(ReadFixtureText("spacing-sensitive-expected.txt"), NormalizeLineEndings(extracted));
    }

    [Fact]
    public void PDFTextStripper_GetText_ParagraphBoundaryFixture_DocumentsKnownGap()
    {
        using PDDocument document = CreateFixtureDocument("paragraph-boundary-content.txt");
        string extracted = new PDFTextStripper().GetText(document);

        Assert.Equal(ReadFixtureText("paragraph-boundary-expected.txt"), NormalizeLineEndings(extracted));
    }

    [Fact]
    public void PDFMarkedContentExtractor_ProcessPage_MarkedContentFixture_MatchesExpected()
    {
        using PDDocument document = CreateFixtureDocument("marked-content-content.txt");
        PDFMarkedContentExtractor extractor = new();
        extractor.ProcessPage(document.GetPage(0));

        string actual = string.Join('\n', extractor.GetMarkedContents()
            .Select(mc => $"{mc.Tag.GetName()}|{string.Concat(mc.GetTexts().Select(tp => tp.GetUnicode()))}")) + '\n';

        Assert.Equal(ReadFixtureText("marked-content-expected.txt"), NormalizeLineEndings(actual));
    }

    [Fact]
    public void PDFTextStripper_GetText_MultiPageFixture_MatchesExpected()
    {
        using PDDocument document = CreateFixtureDocument(
            "multi-page-page1-content.txt",
            "multi-page-page2-content.txt");

        string extracted = new PDFTextStripper().GetText(document);
        Assert.Equal(ReadFixtureText("multi-page-expected.txt"), NormalizeLineEndings(extracted));
    }

    [Fact]
    public void PDFTextStripper_GetText_TrueTypeEmbeddedFont_UsesUnicodeFallback()
    {
        var trueTypeFont = new COSDictionary();
        trueTypeFont.SetItem(COSName.TYPE, COSName.GetPDFName("Font"));
        trueTypeFont.SetItem(COSName.GetPDFName("Subtype"), COSName.GetPDFName("TrueType"));
        trueTypeFont.SetItem(COSName.GetPDFName("BaseFont"), COSName.GetPDFName("MiniTTF"));

        var descriptor = new COSDictionary();
        descriptor.SetItem(COSName.GetPDFName("FontFile2"), CreateBinaryStream(FontBoxTestFixtures.CreateMinimalTrueType()));
        trueTypeFont.SetItem(COSName.GetPDFName("FontDescriptor"), descriptor);

        using PDDocument document = CreateDocumentWithSingleFontAndContent(
            trueTypeFont,
            """
            BT
            /F1 12 Tf
            50 700 Td
            (A) Tj
            ET
            """);

        string extracted = new PDFTextStripper().GetText(document);
        Assert.Equal($"A{Environment.NewLine}", extracted);
    }

    [Fact]
    public void PDFTextStripper_GetText_Type0WithCidType2Descendant_UsesCompositeFallback()
    {
        var descendant = new COSDictionary();
        descendant.SetName(COSName.GetPDFName("Subtype"), "CIDFontType2");
        var descriptor = new COSDictionary();
        descriptor.SetItem(COSName.GetPDFName("FontFile2"), CreateBinaryStream(FontBoxTestFixtures.CreateMinimalTrueType()));
        descendant.SetItem(COSName.GetPDFName("FontDescriptor"), descriptor);

        var descendants = new COSArray();
        descendants.Add(descendant);

        var type0Font = new COSDictionary();
        type0Font.SetItem(COSName.TYPE, COSName.GetPDFName("Font"));
        type0Font.SetItem(COSName.GetPDFName("Subtype"), COSName.GetPDFName("Type0"));
        type0Font.SetItem(COSName.GetPDFName("BaseFont"), COSName.GetPDFName("MiniType0"));
        type0Font.SetItem(COSName.GetPDFName("Encoding"), CreateEncodingCMapStream("1 begincidrange\n<21> <21> 1\nendcidrange", "<00> <FF>"));
        type0Font.SetItem(COSName.GetPDFName("DescendantFonts"), descendants);

        using PDDocument document = CreateDocumentWithSingleFontAndContent(
            type0Font,
            """
            BT
            /F1 12 Tf
            50 700 Td
            (!) Tj
            ET
            """);

        string extracted = new PDFTextStripper().GetText(document);
        Assert.Equal($"A{Environment.NewLine}", extracted);
    }

    [Fact]
    public void PDFTextStripper_GetText_Type0WithCidType0Descendant_UsesToUnicodeCMap()
    {
        var descendant = new COSDictionary();
        descendant.SetName(COSName.GetPDFName("Subtype"), "CIDFontType0");
        descendant.SetFloat(COSName.GetPDFName("DW"), 500f);

        var descendants = new COSArray();
        descendants.Add(descendant);

        var type0Font = new COSDictionary();
        type0Font.SetItem(COSName.TYPE, COSName.GetPDFName("Font"));
        type0Font.SetItem(COSName.GetPDFName("Subtype"), COSName.GetPDFName("Type0"));
        type0Font.SetItem(COSName.GetPDFName("BaseFont"), COSName.GetPDFName("MiniType0"));
        type0Font.SetItem(COSName.GetPDFName("Encoding"), CreateEncodingCMapStream("1 begincidrange\n<21> <21> 65\nendcidrange", "<00> <FF>"));
        type0Font.SetItem(COSName.GetPDFName("ToUnicode"), CreateToUnicodeCMapStream("<21> <0042>"));
        type0Font.SetItem(COSName.GetPDFName("DescendantFonts"), descendants);

        using PDDocument document = CreateDocumentWithSingleFontAndContent(
            type0Font,
            """
            BT
            /F1 12 Tf
            50 700 Td
            (!) Tj
            ET
            """);

        string extracted = new PDFTextStripper().GetText(document);
        Assert.Equal($"B{Environment.NewLine}", extracted);
    }

    [Fact]
    public void Chunk4ParityMatrix_ListsSelectedFeatureSupportAndKnownGap()
    {
        string matrix = ReadFixtureText("parity-matrix.md");
        Assert.Contains("| metadata completeness | string metadata fields | ✅ supported |", matrix);
        Assert.Contains("| metadata completeness | date metadata fields | ✅ supported |", matrix);
        Assert.Contains("| metadata completeness | trapped/custom metadata | ✅ supported |", matrix);
        Assert.Contains("| outlines/forms | document outline tree operations | ✅ supported |", matrix);
        Assert.Contains("| outlines/forms | acroform field roundtrip | ✅ supported |", matrix);
        Assert.Contains("| text baseline | simple text extraction | ✅ supported |", matrix);
        Assert.Contains("| text baseline | line breaks | ✅ supported |", matrix);
        Assert.Contains("| text baseline | spacing-sensitive extraction | ✅ supported |", matrix);
        Assert.Contains("| text baseline | marked-content capture | ✅ supported |", matrix);
        Assert.Contains("| text baseline | multi-page extraction | ✅ supported |", matrix);
        Assert.Contains("| text baseline | paragraph boundaries | ⚠️ known gap |", matrix);
    }

    private static PDDocument CreateFixtureDocument(params string[] contentFixtureNames)
    {
        PDDocument document = new();
        foreach (string fixtureName in contentFixtureNames)
        {
            PDPage page = new();
            document.AddPage(page);

            COSDictionary pageDict = (COSDictionary)page.GetCOSObject();
            pageDict.SetItem(COSName.RESOURCES, CreateDefaultResourcesDictionary());

            COSStream stream = new();
            using (Stream output = stream.CreateOutputStream())
            {
                byte[] bytes = Encoding.Latin1.GetBytes(ReadFixtureText(fixtureName));
                output.Write(bytes, 0, bytes.Length);
            }

            pageDict.SetItem(COSName.CONTENTS, stream);
        }

        return document;
    }

    private static PDDocument CreateDocumentWithSingleFontAndContent(COSDictionary fontDictionary, string contentStream)
    {
        PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);

        COSDictionary pageDict = (COSDictionary)page.GetCOSObject();
        pageDict.SetItem(COSName.RESOURCES, CreateResourcesDictionaryWithSingleFont(fontDictionary));

        COSStream stream = new();
        using (Stream output = stream.CreateOutputStream())
        {
            byte[] bytes = Encoding.Latin1.GetBytes(contentStream);
            output.Write(bytes, 0, bytes.Length);
        }

        pageDict.SetItem(COSName.CONTENTS, stream);
        return document;
    }

    private static COSDictionary CreateDefaultResourcesDictionary()
    {
        COSDictionary fontDictionary = new();
        fontDictionary.SetItem(COSName.TYPE, COSName.GetPDFName("Font"));
        fontDictionary.SetItem(COSName.GetPDFName("Subtype"), COSName.GetPDFName("Type1"));
        fontDictionary.SetItem(COSName.GetPDFName("BaseFont"), COSName.GetPDFName("Helvetica"));

        return CreateResourcesDictionaryWithSingleFont(fontDictionary);
    }

    private static COSDictionary CreateResourcesDictionaryWithSingleFont(COSDictionary fontDictionary)
    {
        COSDictionary fonts = new();
        fonts.SetItem(COSName.GetPDFName("F1"), fontDictionary);

        COSDictionary resources = new();
        resources.SetItem(COSName.GetPDFName("Font"), fonts);
        return resources;
    }

    private static COSStream CreateBinaryStream(byte[] bytes)
    {
        COSStream stream = new();
        using Stream output = stream.CreateOutputStream();
        output.Write(bytes, 0, bytes.Length);
        output.Close();
        return stream;
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

        return CreateBinaryStream(Encoding.ASCII.GetBytes(cmap));
    }

    private static COSStream CreateToUnicodeCMapStream(string bfCharLines)
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
        1 beginbfchar
        {bfCharLines}
        endbfchar
        endcmap
        CMapName currentdict /CMap defineresource pop
        end
        end
        """;

        return CreateBinaryStream(Encoding.ASCII.GetBytes(cmap));
    }

    private static string ReadFixtureText(string fixtureName)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "TextExtraction", fixtureName);
        return NormalizeLineEndings(File.ReadAllText(path));
    }

    private static string NormalizeLineEndings(string text)
    {
        return text.ReplaceLineEndings("\n");
    }
}
