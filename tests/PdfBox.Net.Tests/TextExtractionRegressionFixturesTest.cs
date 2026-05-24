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
    public void TextExtractionParityMatrix_ListsSupportedScenariosAndKnownGap()
    {
        string matrix = ReadFixtureText("parity-matrix.md");
        Assert.Contains("| simple text extraction | ✅ supported |", matrix);
        Assert.Contains("| line breaks | ✅ supported |", matrix);
        Assert.Contains("| spacing-sensitive extraction | ✅ supported |", matrix);
        Assert.Contains("| marked-content capture | ✅ supported |", matrix);
        Assert.Contains("| multi-page extraction | ✅ supported |", matrix);
        Assert.Contains("| paragraph boundaries | ⚠️ known gap |", matrix);
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

    private static COSDictionary CreateDefaultResourcesDictionary()
    {
        COSDictionary fontDictionary = new();
        fontDictionary.SetItem(COSName.TYPE, COSName.GetPDFName("Font"));
        fontDictionary.SetItem(COSName.GetPDFName("Subtype"), COSName.GetPDFName("Type1"));
        fontDictionary.SetItem(COSName.GetPDFName("BaseFont"), COSName.GetPDFName("Helvetica"));

        COSDictionary fonts = new();
        fonts.SetItem(COSName.GetPDFName("F1"), fontDictionary);

        COSDictionary resources = new();
        resources.SetItem(COSName.GetPDFName("Font"), fonts);
        return resources;
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
