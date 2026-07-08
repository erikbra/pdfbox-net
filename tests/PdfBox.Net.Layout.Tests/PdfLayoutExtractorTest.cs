using System.Text;
using PdfBox.Net.COS;
using PdfBox.Net.Layout;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.Layout.Tests;

public class PdfLayoutExtractorTest
{
    [Fact]
    public void Extract_SinglePageText_CapturesGlyphRunsLinesAndBounds()
    {
        using PDDocument document = CreateTextDocument("""
            BT
            /F1 12 Tf
            72 700 Td
            (Hello) Tj
            ET
            """);

        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document);

        PdfLayoutPage page = Assert.Single(layout.Pages);
        Assert.Equal(1, page.PageNumber);
        Assert.Equal(612, page.Width);
        Assert.Equal(792, page.Height);
        Assert.Empty(layout.Diagnostics);
        Assert.Equal("Hello", page.Text);
        Assert.Equal(5, page.Glyphs.Count);
        PdfTextLine line = Assert.Single(page.Lines);
        PdfTextRun run = Assert.Single(line.Runs);
        Assert.Equal("Hello", line.Text);
        Assert.Equal("Hello", run.Text);
        Assert.Equal(5, run.Glyphs.Count);
        Assert.All(page.Glyphs, glyph => Assert.Equal("Helvetica", glyph.FontName));
        Assert.All(page.Glyphs, glyph => Assert.InRange(glyph.FontSize, 11.9f, 12.1f));
        Assert.InRange(run.Bounds.X, 71.9f, 72.1f);
        Assert.InRange(run.Bounds.Y, 78f, 95f);
        Assert.True(run.Bounds.Width > 20);
        Assert.True(run.Bounds.Height > 5);
    }

    [Fact]
    public void Extract_MultiLineText_PreservesReadingOrder()
    {
        using PDDocument document = CreateTextDocument("""
            BT
            /F1 12 Tf
            72 700 Td
            (First line) Tj
            0 -24 Td
            (Second line) Tj
            ET
            """);

        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document);

        PdfLayoutPage page = Assert.Single(layout.Pages);
        Assert.Equal(["First line", "Second line"], page.Lines.Select(line => line.Text).ToArray());
        Assert.Equal($"First line{Environment.NewLine}Second line", page.Text);
        Assert.True(page.Lines[0].Bounds.Y < page.Lines[1].Bounds.Y);
    }

    [Fact]
    public void Extract_RotatedCroppedPage_NormalizesPageGeometryAndTextBounds()
    {
        using PDDocument document = CreateTextDocument("""
            BT
            /F1 12 Tf
            72 120 Td
            (Rotated) Tj
            ET
            """);
        PDPage page = document.GetPage(0);
        page.SetCropBox(new PDRectangle(10, 20, 200, 300));
        page.SetRotation(90);

        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document);

        PdfLayoutPage layoutPage = Assert.Single(layout.Pages);
        Assert.Equal(90, layoutPage.Rotation);
        Assert.Equal(300, layoutPage.Width);
        Assert.Equal(200, layoutPage.Height);
        Assert.Equal(new PdfLayoutRectangle(10, 20, 200, 300), layoutPage.CropBox);
        Assert.Equal("Rotated", layoutPage.Text);
        Assert.All(layoutPage.Glyphs, glyph =>
        {
            Assert.InRange(glyph.Bounds.X, 0, layoutPage.Width);
            Assert.InRange(glyph.Bounds.Y, -0.5f, layoutPage.Height + 0.5f);
        });
    }

    [Fact]
    public void Extract_ExistingRotationFixture_ExtractsTextWithoutDiagnostics()
    {
        using PDDocument document = Loader.LoadPDF(Path.Combine(AppContext.BaseDirectory, "Fixtures", "rotation.pdf"));

        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document);

        Assert.Equal(2, layout.Pages.Count);
        Assert.Empty(layout.Diagnostics);
        Assert.Contains(layout.Pages, page => page.Glyphs.Count > 0);
        Assert.Contains(layout.Pages, page => page.Text.Length > 0);
    }

    [Fact]
    public void Extract_IsDeterministicAcrossRepeatedRuns()
    {
        using PDDocument document = CreateTextDocument("""
            BT
            /F1 12 Tf
            72 700 Td
            (Alpha) Tj
            0 -24 Td
            (Beta) Tj
            ET
            """);

        PdfLayoutDocument first = PdfLayoutExtractor.Extract(document);
        PdfLayoutDocument second = PdfLayoutExtractor.Extract(document);

        Assert.Equal(Snapshot(first), Snapshot(second));
    }

    private static string Snapshot(PdfLayoutDocument document)
    {
        StringBuilder builder = new();
        foreach (PdfLayoutPage page in document.Pages)
        {
            builder.AppendLine($"{page.PageNumber}:{page.Width:0.###}:{page.Height:0.###}:{page.Rotation}:{page.Text}");
            foreach (PdfTextGlyph glyph in page.Glyphs)
            {
                builder.AppendLine(
                    $"{glyph.Text}:{glyph.FontName}:{glyph.FontSize:0.###}:{glyph.Direction:0.###}:{glyph.Bounds.X:0.###}:{glyph.Bounds.Y:0.###}:{glyph.Bounds.Width:0.###}:{glyph.Bounds.Height:0.###}");
            }
        }

        return builder.ToString();
    }

    private static PDDocument CreateTextDocument(string contentStream)
    {
        PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);

        COSDictionary pageDictionary = (COSDictionary)page.GetCOSObject();
        pageDictionary.SetItem(COSName.RESOURCES, CreateDefaultResourcesDictionary());
        pageDictionary.SetItem(COSName.CONTENTS, CreateContentStream(contentStream));
        return document;
    }

    private static COSStream CreateContentStream(string contentStream)
    {
        COSStream stream = new();
        using Stream output = stream.CreateOutputStream();
        byte[] bytes = Encoding.Latin1.GetBytes(contentStream);
        output.Write(bytes, 0, bytes.Length);
        return stream;
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
