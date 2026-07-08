using System.Text;
using PdfBox.Net.COS;
using PdfBox.Net.Layout;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.PDModel.Interactive.Action;
using PdfBox.Net.PDModel.Interactive.Annotation;

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
    public void Extract_LinkAnnotation_CapturesUriBoundsAndQuadBounds()
    {
        using PDDocument document = CreateTextDocument("""
            BT
            /F1 12 Tf
            72 700 Td
            (Linked text) Tj
            ET
            """);
        PDAnnotationLink link = new();
        link.SetRectangle(new PDRectangle(72, 680, 120, 24));
        link.SetQuadPoints([72, 704, 192, 704, 72, 680, 192, 680]);
        PDActionURI action = new();
        action.SetURI("https://example.com/pdfbox");
        link.SetAction(action);
        document.GetPage(0).SetAnnotations([link]);

        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document);

        PdfLayoutLink layoutLink = Assert.Single(Assert.Single(layout.Pages).Links);
        Assert.Equal(0, layoutLink.Index);
        Assert.Equal(PdfLayoutLinkKind.Uri, layoutLink.Kind);
        Assert.Equal("https://example.com/pdfbox", layoutLink.Uri);
        Assert.Null(layoutLink.Destination);
        Assert.Null(layoutLink.DestinationPageNumber);
        AssertClose(72, layoutLink.Bounds.X);
        AssertClose(88, layoutLink.Bounds.Y);
        AssertClose(120, layoutLink.Bounds.Width);
        AssertClose(24, layoutLink.Bounds.Height);
        PdfLayoutRectangle quad = Assert.Single(layoutLink.QuadBounds);
        AssertClose(layoutLink.Bounds.X, quad.X);
        AssertClose(layoutLink.Bounds.Y, quad.Y);
        AssertClose(layoutLink.Bounds.Width, quad.Width);
        AssertClose(layoutLink.Bounds.Height, quad.Height);
    }

    [Fact]
    public void Extract_LinkCollectionCanBeDisabled()
    {
        using PDDocument document = CreateTextDocument("");
        PDAnnotationLink link = new();
        link.SetRectangle(new PDRectangle(72, 680, 120, 24));
        document.GetPage(0).SetAnnotations([link]);

        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document, new PdfLayoutOptions
        {
            IncludeLinks = false
        });

        Assert.Empty(Assert.Single(layout.Pages).Links);
    }

    [Fact]
    public void Extract_XObjectImage_CapturesIntrinsicSizePlacementAndMetadata()
    {
        using PDDocument document = CreateImageDocument();

        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document);

        PdfLayoutImage image = Assert.Single(Assert.Single(layout.Pages).Images);
        Assert.Empty(layout.Diagnostics);
        Assert.Empty(layout.ImageAssets);
        Assert.Equal(0, image.Index);
        Assert.Equal("page-1-image-0", image.AssetId);
        Assert.Equal(PdfLayoutImageKind.XObject, image.Kind);
        Assert.Equal("Im0", image.SourceName);
        Assert.Equal(2, image.IntrinsicWidth);
        Assert.Equal(2, image.IntrinsicHeight);
        Assert.Equal(8, image.BitsPerComponent);
        Assert.Equal("DeviceRGB", image.ColorSpaceName);
        Assert.False(image.Interpolate);
        AssertClose(72, image.Bounds.X);
        AssertClose(132, image.Bounds.Y);
        AssertClose(120, image.Bounds.Width);
        AssertClose(60, image.Bounds.Height);
        AssertClose(120, image.Transform.A);
        AssertClose(0, image.Transform.B);
        AssertClose(0, image.Transform.C);
        AssertClose(60, image.Transform.D);
        AssertClose(72, image.Transform.E);
        AssertClose(600, image.Transform.F);
    }

    [Fact]
    public void Extract_InlineImage_CapturesIntrinsicSizePlacementAndMetadata()
    {
        using PDDocument document = CreateInlineImageDocument();

        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document);

        PdfLayoutImage image = Assert.Single(Assert.Single(layout.Pages).Images);
        Assert.Empty(layout.Diagnostics);
        Assert.Empty(layout.ImageAssets);
        Assert.Equal(0, image.Index);
        Assert.Equal("page-1-image-0", image.AssetId);
        Assert.Equal(PdfLayoutImageKind.InlineImage, image.Kind);
        Assert.Null(image.SourceName);
        Assert.Equal(2, image.IntrinsicWidth);
        Assert.Equal(2, image.IntrinsicHeight);
        Assert.Equal(8, image.BitsPerComponent);
        Assert.Equal("DeviceRGB", image.ColorSpaceName);
        Assert.False(image.Interpolate);
        AssertClose(72, image.Bounds.X);
        AssertClose(132, image.Bounds.Y);
        AssertClose(120, image.Bounds.Width);
        AssertClose(60, image.Bounds.Height);
        AssertClose(120, image.Transform.A);
        AssertClose(0, image.Transform.B);
        AssertClose(0, image.Transform.C);
        AssertClose(60, image.Transform.D);
        AssertClose(72, image.Transform.E);
        AssertClose(600, image.Transform.F);
    }

    [Fact]
    public void Extract_ImageCollectionCanBeDisabled()
    {
        using PDDocument document = CreateImageDocument();

        PdfLayoutDocument layout = PdfLayoutExtractor.Extract(document, new PdfLayoutOptions
        {
            IncludeImages = false
        });

        Assert.Empty(Assert.Single(layout.Pages).Images);
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

    private static void AssertClose(float expected, float actual)
    {
        Assert.InRange(actual, expected - 0.01f, expected + 0.01f);
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

    private static PDDocument CreateImageDocument()
    {
        PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);
        byte[] rgb =
        [
            255, 0, 0,
            0, 255, 0,
            0, 0, 255,
            255, 255, 255
        ];
        PDImageXObject image = LosslessFactory.CreateFromRawData(document, rgb, 2, 2, 8, 3);
        using (PDPageContentStream content = new(document, page))
        {
            content.DrawImage(image, 72, 600, 120, 60);
        }

        return document;
    }

    private static PDDocument CreateInlineImageDocument()
    {
        PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);

        COSDictionary pageDictionary = (COSDictionary)page.GetCOSObject();
        pageDictionary.SetItem(COSName.CONTENTS, CreateInlineImageContentStream());
        return document;
    }

    private static COSStream CreateInlineImageContentStream()
    {
        COSStream stream = new();
        using Stream output = stream.CreateOutputStream();
        WriteLatin1(output, "q\n120 0 0 60 72 600 cm\nBI\n/W 2 /H 2 /BPC 8 /CS /RGB\nID\n");
        output.Write([
            255, 0, 0,
            0, 255, 0,
            0, 0, 255,
            255, 255, 255
        ]);
        WriteLatin1(output, "\nEI\nQ\n");
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

    private static void WriteLatin1(Stream stream, string value)
    {
        byte[] bytes = Encoding.Latin1.GetBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    }
}
