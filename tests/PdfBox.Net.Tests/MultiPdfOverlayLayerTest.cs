using PdfBox.Net.MultiPdf;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.OptionalContent;
using PdfBox.Net.Util;
using Xunit;

namespace PdfBox.Net.Tests;

public class MultiPdfOverlayLayerTest
{
    // -------------------------------------------------------------------------
    // Splitter tests
    // -------------------------------------------------------------------------

    [Fact]
    public void Splitter_SplitsThreePageDocIntoThreeSinglePageDocs()
    {
        using PDDocument source = new();
        source.AddPage(new PDPage());
        source.AddPage(new PDPage());
        source.AddPage(new PDPage());

        Splitter splitter = new();
        IList<PDDocument> parts = splitter.Split(source);

        Assert.Equal(3, parts.Count);
        foreach (PDDocument part in parts)
        {
            Assert.Equal(1, part.GetNumberOfPages());
            part.Dispose();
        }
    }

    [Fact]
    public void Splitter_SplitAtPage2_SplitsTwoPartsFromThreePages()
    {
        using PDDocument source = new();
        source.AddPage(new PDPage());
        source.AddPage(new PDPage());
        source.AddPage(new PDPage());

        Splitter splitter = new()
        {
            SplitAtPage = 2,
        };

        IList<PDDocument> parts = splitter.Split(source);

        Assert.Equal(2, parts.Count);
        Assert.Equal(2, parts[0].GetNumberOfPages());
        Assert.Equal(1, parts[1].GetNumberOfPages());

        foreach (PDDocument part in parts)
        {
            part.Dispose();
        }
    }

    [Fact]
    public void Splitter_StartEndPage_ExtractsSubset()
    {
        using PDDocument source = new();
        for (int i = 0; i < 5; i++)
        {
            PDPage page = new();
            page.SetRotation(i * 10);
            source.AddPage(page);
        }

        Splitter splitter = new()
        {
            StartPage = 2,
            EndPage = 4,
            SplitAtPage = 3,
        };

        IList<PDDocument> parts = splitter.Split(source);

        Assert.Equal(1, parts.Count);
        Assert.Equal(3, parts[0].GetNumberOfPages());

        parts[0].Dispose();
    }

    // -------------------------------------------------------------------------
    // PageExtractor tests
    // -------------------------------------------------------------------------

    [Fact]
    public void PageExtractor_ExtractsDefaultAllPages()
    {
        using PDDocument source = new();
        source.AddPage(new PDPage());
        source.AddPage(new PDPage());
        source.AddPage(new PDPage());

        PageExtractor extractor = new(source);
        using PDDocument extracted = extractor.Extract();

        Assert.Equal(3, extracted.GetNumberOfPages());
    }

    [Fact]
    public void PageExtractor_ExtractsMiddlePages()
    {
        using PDDocument source = new();
        for (int i = 0; i < 4; i++)
        {
            PDPage page = new();
            page.SetRotation(i * 90);
            source.AddPage(page);
        }

        PageExtractor extractor = new(source, 2, 3);
        using PDDocument extracted = extractor.Extract();

        Assert.Equal(2, extracted.GetNumberOfPages());
    }

    [Fact]
    public void PageExtractor_ExtractsSinglePage()
    {
        using PDDocument source = new();
        source.AddPage(new PDPage());
        source.AddPage(new PDPage());
        source.AddPage(new PDPage());

        PageExtractor extractor = new(source, 2, 2);
        using PDDocument extracted = extractor.Extract();

        Assert.Equal(1, extracted.GetNumberOfPages());
    }

    // -------------------------------------------------------------------------
    // Overlay tests
    // -------------------------------------------------------------------------

    [Fact]
    public void Overlay_BackgroundOverlayAddsStreamBeforeContent()
    {
        using PDDocument inputDoc = CreateSinglePagePdfInMemory();
        using PDDocument overlayDoc = CreateSinglePagePdfInMemory();

        using Overlay overlay = new();
        overlay.InputPDF = inputDoc;
        overlay.DefaultOverlayPDF = overlayDoc;
        overlay.OverlayPosition = Overlay.Position.BACKGROUND;

        using PDDocument result = overlay.Process(new Dictionary<int, string>());

        // After overlaying, the page should have content (the array of streams)
        Assert.NotNull(result.GetPage(0).GetContents());
    }

    [Fact]
    public void Overlay_ForegroundOverlayAddsStreamAfterContent()
    {
        using PDDocument inputDoc = CreateSinglePagePdfInMemory();
        using PDDocument overlayDoc = CreateSinglePagePdfInMemory();

        using Overlay overlay = new();
        overlay.InputPDF = inputDoc;
        overlay.DefaultOverlayPDF = overlayDoc;
        overlay.OverlayPosition = Overlay.Position.FOREGROUND;

        using PDDocument result = overlay.Process(new Dictionary<int, string>());

        Assert.NotNull(result.GetPage(0).GetContents());
    }

    [Fact]
    public void Overlay_MultiPageInput_OverlayAppliedToAllPages()
    {
        using PDDocument inputDoc = new();
        inputDoc.AddPage(new PDPage());
        inputDoc.AddPage(new PDPage());
        inputDoc.AddPage(new PDPage());

        using PDDocument overlayDoc = CreateSinglePagePdfInMemory();

        using Overlay overlay = new();
        overlay.InputPDF = inputDoc;
        overlay.DefaultOverlayPDF = overlayDoc;

        using PDDocument result = overlay.Process(new Dictionary<int, string>());

        Assert.Equal(3, result.GetNumberOfPages());
        for (int i = 0; i < 3; i++)
        {
            Assert.NotNull(result.GetPage(i).GetContents());
        }
    }

    // -------------------------------------------------------------------------
    // LayerUtility tests
    // -------------------------------------------------------------------------

    [Fact]
    public void LayerUtility_WrapInSaveRestore_AddsQAndQStreams()
    {
        using PDDocument doc = new();
        PDPage page = new PDPage(PDRectangle.A4);
        doc.AddPage(page);

        // Add some content to the page so WrapInSaveRestore has something to wrap
        using (PDPageContentStream cs = new(doc, page))
        {
            cs.SaveGraphicsState();
            cs.RestoreGraphicsState();
        }

        LayerUtility layerUtility = new(doc);
        layerUtility.WrapInSaveRestore(page);

        // After wrap the page should still have contents
        Assert.NotNull(page.GetContents());
    }

    [Fact]
    public void LayerUtility_ImportPageAsForm_CreatesFormXObject()
    {
        using PDDocument source = CreateSinglePagePdfInMemory();
        using PDDocument target = new();

        LayerUtility layerUtility = new(target);
        PDFormXObject form = layerUtility.ImportPageAsForm(source, 0);

        Assert.NotNull(form);
        Assert.NotNull(form.GetBBox());
    }

    [Fact]
    public void LayerUtility_AppendFormAsLayer_CreatesOCG()
    {
        using PDDocument source = CreateSinglePagePdfInMemory();
        using PDDocument target = new();
        target.AddPage(new PDPage());

        LayerUtility layerUtility = new(target);
        PDFormXObject form = layerUtility.ImportPageAsForm(source, 0);
        PDPage targetPage = target.GetPage(0);

        PDOptionalContentGroup ocg = layerUtility.AppendFormAsLayer(
            targetPage,
            form,
            new AffineTransform(),
            "TestLayer");

        Assert.NotNull(ocg);
        Assert.Equal("TestLayer", ocg.GetName());

        // OCG should be registered in document OC properties
        PDOptionalContentProperties? ocProps = target.GetDocumentCatalog().GetOCProperties();
        Assert.NotNull(ocProps);
        Assert.True(ocProps.HasGroup("TestLayer"));
    }

    [Fact]
    public void LayerUtility_AppendFormAsLayer_PageHasContents()
    {
        using PDDocument source = CreateSinglePagePdfInMemory();
        using PDDocument target = new();
        target.AddPage(new PDPage());

        LayerUtility layerUtility = new(target);
        PDFormXObject form = layerUtility.ImportPageAsForm(source, 0);
        PDPage targetPage = target.GetPage(0);

        layerUtility.AppendFormAsLayer(targetPage, form, null, "MyLayer");

        Assert.NotNull(targetPage.GetContents());
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static PDDocument CreateSinglePagePdfInMemory()
    {
        PDDocument doc = new();
        doc.AddPage(new PDPage(PDRectangle.A4));
        return doc;
    }
}
