using PdfBox.Net.MultiPdf;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using Xunit;

namespace PdfBox.Net.Tests;

public class MultiPdfExtractionSplittingTest
{
    // ── Splitter ──────────────────────────────────────────────────────────────

    [Fact]
    public void Splitter_SplitsEveryPageByDefault()
    {
        using PDDocument source = CreateMultiPagePdf(3, [90, 180, 270]);

        Splitter splitter = new();
        List<PDDocument> results = splitter.Split(source);

        try
        {
            Assert.Equal(3, results.Count);
            Assert.Equal(1, results[0].GetNumberOfPages());
            Assert.Equal(1, results[1].GetNumberOfPages());
            Assert.Equal(1, results[2].GetNumberOfPages());
            Assert.Equal(90, results[0].GetPage(0).GetRotation());
            Assert.Equal(180, results[1].GetPage(0).GetRotation());
            Assert.Equal(270, results[2].GetPage(0).GetRotation());
        }
        finally
        {
            foreach (PDDocument doc in results) doc.Dispose();
        }
    }

    [Fact]
    public void Splitter_SplitAtTwoPagesGroupsPairsCorrectly()
    {
        // Use distinct MediaBox widths as per-page markers (100, 200, 300, 400, 500 points).
        using PDDocument source = CreateMultiPagePdfByWidth(5, [100f, 200f, 300f, 400f, 500f]);

        Splitter splitter = new();
        splitter.SetSplitAtPage(2);
        List<PDDocument> results = splitter.Split(source);

        try
        {
            // pages [1,2], [3,4], [5]
            Assert.Equal(3, results.Count);
            Assert.Equal(2, results[0].GetNumberOfPages());
            Assert.Equal(2, results[1].GetNumberOfPages());
            Assert.Equal(1, results[2].GetNumberOfPages());
            Assert.Equal(100f, results[0].GetPage(0).GetMediaBox().GetWidth());
            Assert.Equal(200f, results[0].GetPage(1).GetMediaBox().GetWidth());
            Assert.Equal(300f, results[1].GetPage(0).GetMediaBox().GetWidth());
            Assert.Equal(400f, results[1].GetPage(1).GetMediaBox().GetWidth());
            Assert.Equal(500f, results[2].GetPage(0).GetMediaBox().GetWidth());
        }
        finally
        {
            foreach (PDDocument doc in results) doc.Dispose();
        }
    }

    [Fact]
    public void Splitter_StartEndPageFiltersRange()
    {
        // Use distinct MediaBox widths so page identity survives GetRotation() normalisation.
        using PDDocument source = CreateMultiPagePdfByWidth(5, [100f, 200f, 300f, 400f, 500f]);

        Splitter splitter = new();
        splitter.SetStartPage(2);
        splitter.SetEndPage(4);
        List<PDDocument> results = splitter.Split(source);

        try
        {
            Assert.Equal(3, results.Count);
            Assert.Equal(200f, results[0].GetPage(0).GetMediaBox().GetWidth());
            Assert.Equal(300f, results[1].GetPage(0).GetMediaBox().GetWidth());
            Assert.Equal(400f, results[2].GetPage(0).GetMediaBox().GetWidth());
        }
        finally
        {
            foreach (PDDocument doc in results) doc.Dispose();
        }
    }

    [Fact]
    public void Splitter_RoundtripPreservesRotation()
    {
        byte[] sourceBytes = CreateSinglePagePdfBytes(270);
        using MemoryStream ms = new(sourceBytes);
        using PDDocument source = PDDocument.Load(ms);

        Splitter splitter = new();
        List<PDDocument> results = splitter.Split(source);

        try
        {
            Assert.Single(results);

            using MemoryStream output = new();
            results[0].Save(output);
            output.Position = 0;
            using PDDocument reloaded = PDDocument.Load(output);
            Assert.Equal(270, reloaded.GetPage(0).GetRotation());
        }
        finally
        {
            foreach (PDDocument doc in results) doc.Dispose();
        }
    }

    [Fact]
    public void Splitter_RejectsInvalidSplitAtPage()
    {
        Splitter splitter = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => splitter.SetSplitAtPage(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => splitter.SetSplitAtPage(-1));
    }

    [Fact]
    public void Splitter_RejectsInvalidStartPage()
    {
        Splitter splitter = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => splitter.SetStartPage(0));
    }

    [Fact]
    public void Splitter_RejectsInvalidEndPage()
    {
        Splitter splitter = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => splitter.SetEndPage(0));
    }

    [Fact]
    public void Splitter_ClonesPageDictionaryNotSameReference()
    {
        using PDDocument source = CreateMultiPagePdf(2, [0, 90]);

        Splitter splitter = new();
        splitter.SetSplitAtPage(2);
        List<PDDocument> results = splitter.Split(source);

        try
        {
            PDPage srcPage = source.GetPage(0);
            PDPage dstPage = results[0].GetPage(0);
            Assert.NotSame(srcPage.GetCOSObject(), dstPage.GetCOSObject());
        }
        finally
        {
            foreach (PDDocument doc in results) doc.Dispose();
        }
    }

    // ── PageExtractor ─────────────────────────────────────────────────────────

    [Fact]
    public void PageExtractor_ExtractsAllPagesByDefault()
    {
        using PDDocument source = CreateMultiPagePdfByWidth(3, [100f, 200f, 300f]);

        PageExtractor extractor = new(source);
        using PDDocument result = extractor.Extract();

        Assert.Equal(3, result.GetNumberOfPages());
        Assert.Equal(100f, result.GetPage(0).GetMediaBox().GetWidth());
        Assert.Equal(200f, result.GetPage(1).GetMediaBox().GetWidth());
        Assert.Equal(300f, result.GetPage(2).GetMediaBox().GetWidth());
    }

    [Fact]
    public void PageExtractor_ExtractsSingleMiddlePage()
    {
        using PDDocument source = CreateMultiPagePdfByWidth(5, [100f, 200f, 300f, 400f, 500f]);

        PageExtractor extractor = new(source, startPage: 3, endPage: 3);
        using PDDocument result = extractor.Extract();

        Assert.Equal(1, result.GetNumberOfPages());
        Assert.Equal(300f, result.GetPage(0).GetMediaBox().GetWidth());
    }

    [Fact]
    public void PageExtractor_ExtractsSubRange()
    {
        using PDDocument source = CreateMultiPagePdfByWidth(5, [100f, 200f, 300f, 400f, 500f]);

        PageExtractor extractor = new(source, startPage: 2, endPage: 4);
        using PDDocument result = extractor.Extract();

        Assert.Equal(3, result.GetNumberOfPages());
        Assert.Equal(200f, result.GetPage(0).GetMediaBox().GetWidth());
        Assert.Equal(300f, result.GetPage(1).GetMediaBox().GetWidth());
        Assert.Equal(400f, result.GetPage(2).GetMediaBox().GetWidth());
    }

    [Fact]
    public void PageExtractor_ReturnsEmptyDocumentWhenStartPageExceedsTotal()
    {
        using PDDocument source = CreateMultiPagePdf(3, [90, 180, 270]);

        PageExtractor extractor = new(source, startPage: 10, endPage: 12);
        using PDDocument result = extractor.Extract();

        Assert.Equal(0, result.GetNumberOfPages());
    }

    [Fact]
    public void PageExtractor_ClipsEndPageToDocumentLength()
    {
        using PDDocument source = CreateMultiPagePdfByWidth(3, [100f, 200f, 300f]);

        PageExtractor extractor = new(source, startPage: 2, endPage: 999);
        using PDDocument result = extractor.Extract();

        Assert.Equal(2, result.GetNumberOfPages());
        Assert.Equal(200f, result.GetPage(0).GetMediaBox().GetWidth());
        Assert.Equal(300f, result.GetPage(1).GetMediaBox().GetWidth());
    }

    [Fact]
    public void PageExtractor_ReturnsEmptyDocumentWhenRangeInverted()
    {
        using PDDocument source = CreateMultiPagePdf(3, [90, 180, 270]);

        PageExtractor extractor = new(source);
        extractor.StartPage = 4;
        extractor.EndPage = 2;
        using PDDocument result = extractor.Extract();

        Assert.Equal(0, result.GetNumberOfPages());
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    /// <summary>Creates a document whose pages are tagged by rotation (must be multiples of 90).</summary>
    private static PDDocument CreateMultiPagePdf(int pageCount, int[] rotations)
    {
        PDDocument document = new();
        for (int i = 0; i < pageCount; i++)
        {
            PDPage page = new();
            page.SetRotation(rotations[i]);
            document.AddPage(page);
        }

        return document;
    }

    /// <summary>Creates a document whose pages are tagged by a unique MediaBox width.</summary>
    private static PDDocument CreateMultiPagePdfByWidth(int pageCount, float[] widths)
    {
        PDDocument document = new();
        for (int i = 0; i < pageCount; i++)
        {
            PDRectangle box = new(widths[i], 792f);
            PDPage page = new(box);
            document.AddPage(page);
        }

        return document;
    }

    private static byte[] CreateSinglePagePdfBytes(int rotation)
    {
        using PDDocument document = new();
        PDPage page = new();
        page.SetRotation(rotation);
        document.AddPage(page);
        using MemoryStream output = new();
        document.Save(output);
        return output.ToArray();
    }
}
