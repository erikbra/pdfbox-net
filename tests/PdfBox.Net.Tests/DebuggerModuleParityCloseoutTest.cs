using System.Text;
using PdfBox.Net.ContentStream.Operator;
using PdfBox.Net.COS;
using PdfBox.Net.Debugger.Fontencodingpane;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.PdfParser;
using PdfBox.Net.Rendering;
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
        Assert.Contains("text", extracted, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FontEncodingPaneController_Type3Font_CreatesType3Model()
    {
        COSName fontName = COSName.GetPDFName("FType3");
        COSDictionary resourcesDictionary = new();
        COSDictionary fonts = new();
        fonts.SetItem(fontName, CreateType3FontDictionary());
        resourcesDictionary.SetItem(COSName.GetPDFName("Font"), fonts);

        FontEncodingPaneController controller = new(fontName, resourcesDictionary);

        Type3Font pane = Assert.IsType<Type3Font>(controller.FontPane);
        Assert.Null(controller.ErrorMessage);
        Assert.Equal("MiniType3", pane.Attributes["Font"]);
        Assert.Equal("WinAnsiEncoding", pane.Attributes["Encoding"]);
        Assert.True(pane.TotalAvailableGlyph > 1);
    }

    [Fact]
    public void Type3Font_TableData_RendersGlyphPreviewWithRegisteredBackend()
    {
        PDType3Font font = new(CreateType3FontDictionary());
        Type3Font pane = new(font, font.GetResources() ?? new PDResources());

        object[] row = pane.TableData[65];

        Assert.Equal(65, row[0]);
        Assert.Equal("A", row[1]);
        Assert.Equal("A", row[2]);
        using BufferedImage image = Assert.IsType<BufferedImage>(row[3]);
        Assert.True(image.Width > 0);
        Assert.True(image.Height > 0);
        Assert.True(CountNonWhitePixels(image) > 0);
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

    private static COSDictionary CreateType3FontDictionary()
    {
        COSDictionary fontDictionary = new();
        fontDictionary.SetItem(COSName.TYPE, COSName.GetPDFName("Font"));
        fontDictionary.SetName(COSName.GetPDFName("Subtype"), "Type3");
        fontDictionary.SetName(COSName.NAME, "MiniType3");
        fontDictionary.SetItem(COSName.GetPDFName("FontMatrix"), COSArray.Of(0.001f, 0f, 0f, 0.001f, 0f, 0f));
        fontDictionary.SetItem(COSName.GetPDFName("FontBBox"), COSArray.Of(0f, 0f, 20f, 20f));
        fontDictionary.SetInt(COSName.GetPDFName("FirstChar"), 65);
        fontDictionary.SetInt(COSName.GetPDFName("LastChar"), 65);
        fontDictionary.SetName(COSName.GetPDFName("Encoding"), "WinAnsiEncoding");
        fontDictionary.SetItem(COSName.GetPDFName("Widths"), COSArray.Of(500f));

        COSDictionary charProcs = new();
        charProcs.SetItem(COSName.GetPDFName("A"), CreateType3CharProc("500 0 0 0 20 20 d1\n0 0 0 rg\n0 0 20 20 re\nf\n"));
        fontDictionary.SetItem(COSName.GetPDFName("CharProcs"), charProcs);
        fontDictionary.SetItem(COSName.RESOURCES, new COSDictionary());
        return fontDictionary;
    }

    private static COSStream CreateType3CharProc(string content)
    {
        COSStream stream = new();
        using Stream output = stream.CreateOutputStream();
        byte[] bytes = Encoding.Latin1.GetBytes(content);
        output.Write(bytes, 0, bytes.Length);
        return stream;
    }

    private static int CountNonWhitePixels(BufferedImage image)
    {
        int count = 0;
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                if ((image.GetRgb(x, y) & 0x00FFFFFF) != 0x00FFFFFF)
                {
                    count++;
                }
            }
        }

        return count;
    }
}
