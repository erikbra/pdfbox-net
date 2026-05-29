using System.Text;
using PdfBox.Net.ContentStream.Operator;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PdfParser;
using PdfBox.Net.Text;

namespace PdfBox.Net.Tests;

public class DebuggerModuleParityCloseoutTest
{
    [Fact]
    public void CosTreeInspection_Flow_ExposesCatalogPagesAndPageContents()
    {
        using PDDocument document = CreateFixtureDocument("(debugger tree)");

        COSDictionary catalog = (COSDictionary)document.GetDocumentCatalog().GetCOSObject();
        COSDictionary pages = Assert.IsType<COSDictionary>(catalog.GetDictionaryObject(COSName.PAGES));
        COSDictionary page = (COSDictionary)document.GetPage(0).GetCOSObject();

        Assert.Equal(COSName.CATALOG, catalog.GetItem(COSName.TYPE));
        Assert.Equal(COSName.PAGES, pages.GetItem(COSName.TYPE));
        Assert.NotNull(page.GetDictionaryObject(COSName.CONTENTS));
    }

    [Fact]
    public void StreamInspection_Flow_ParsesContentStreamOperators()
    {
        using PDDocument document = CreateFixtureDocument("(debugger stream)");
        COSStream contents = Assert.IsType<COSStream>(document.GetPage(0).GetContents());

        using Stream input = contents.CreateInputStream();
        IList<object> tokens = new PDFStreamParser(input).ParseTokens();

        Assert.Contains(tokens, token => token is Operator op && op.GetName() == OperatorName.BEGIN_TEXT);
        Assert.Contains(tokens, token => token is Operator op && op.GetName() == OperatorName.END_TEXT);
        Assert.Contains(tokens, token => token is COSString str && str.GetString().Contains("debugger stream", StringComparison.Ordinal));
    }

    [Fact]
    public void TextInspection_Flow_ExtractsSearchableText()
    {
        using PDDocument document = CreateFixtureDocument("(debugger text search)");

        string extracted = new PDFTextStripper().GetText(document).ReplaceLineEndings("\n");

        Assert.False(string.IsNullOrWhiteSpace(extracted));
        Assert.Contains("debug", extracted, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("tx", extracted, StringComparison.OrdinalIgnoreCase);
    }

    private static PDDocument CreateFixtureDocument(string textOperand)
    {
        PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);

        COSDictionary pageDict = (COSDictionary)page.GetCOSObject();
        pageDict.SetItem(COSName.RESOURCES, CreateDefaultResourcesDictionary());

        COSStream stream = new();
        using (Stream output = stream.CreateOutputStream())
        {
            string content = $"BT\n/F1 12 Tf\n72 720 Td\n{textOperand} Tj\nET\n";
            byte[] bytes = Encoding.Latin1.GetBytes(content);
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

        COSDictionary fonts = new();
        fonts.SetItem(COSName.GetPDFName("F1"), fontDictionary);

        COSDictionary resources = new();
        resources.SetItem(COSName.GetPDFName("Font"), fonts);
        return resources;
    }
}
