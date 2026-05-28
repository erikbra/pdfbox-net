using PdfBox.Net.COS;
using PdfBox.Net.MultiPdf;
using PdfBox.Net.PDModel;
using Xunit;

namespace PdfBox.Net.Tests;

public class MultiPdfCloneMergeFoundationTest
{
    [Fact]
    public void PDFCloneUtility_ClonesSharedDictionaryOnlyOnce()
    {
        using PDDocument destination = new();
        PDFCloneUtility cloner = new(destination);

        COSDictionary shared = new();
        shared.SetString(COSName.GetPDFName("Value"), "shared");

        COSDictionary source = new();
        source.SetItem(COSName.GetPDFName("A"), shared);
        source.SetItem(COSName.GetPDFName("B"), shared);

        COSDictionary clone = cloner.CloneForNewDocument(source)!;

        COSDictionary cloneA = clone.GetCOSDictionary(COSName.GetPDFName("A"))!;
        COSDictionary cloneB = clone.GetCOSDictionary(COSName.GetPDFName("B"))!;

        Assert.NotSame(source, clone);
        Assert.NotSame(shared, cloneA);
        Assert.Same(cloneA, cloneB);

        cloneA.SetString(COSName.GetPDFName("Value"), "changed");
        Assert.Equal("shared", shared.GetString(COSName.GetPDFName("Value")));
    }

    [Fact]
    public void PDFCloneUtility_ResolvesSelfReferenceToClonedParent()
    {
        using PDDocument destination = new();
        PDFCloneUtility cloner = new(destination);

        COSDictionary source = new();
        source.SetItem(COSName.GetPDFName("Self"), new COSObject(source));

        COSDictionary clone = cloner.CloneForNewDocument(source)!;

        Assert.Same(clone, clone.GetItem(COSName.GetPDFName("Self")));
    }

    [Fact]
    public void PDFMergerUtility_MergesAllSourcePagesInOrder()
    {
        byte[] source1 = CreateSinglePagePdf(90);
        byte[] source2 = CreateSinglePagePdf(180);

        using MemoryStream destinationOutput = new();

        PDFMergerUtility merger = new()
        {
            DestinationStream = destinationOutput
        };

        merger.AddSource(new MemoryStream(source1));
        merger.AddSource(new MemoryStream(source2));
        merger.MergeDocuments();

        destinationOutput.Position = 0;
        using PDDocument merged = PDDocument.Load(destinationOutput);

        Assert.Equal(2, merged.GetNumberOfPages());
        Assert.Equal(90, merged.GetPage(0).GetRotation());
        Assert.Equal(180, merged.GetPage(1).GetRotation());
    }

    [Fact]
    public void PDFMergerUtility_AppendDocumentClonesPageDictionary()
    {
        using PDDocument source = new();
        PDPage sourcePage = new();
        sourcePage.SetRotation(270);
        source.AddPage(sourcePage);

        using PDDocument destination = new();
        PDFMergerUtility merger = new();

        merger.AppendDocument(destination, source);

        Assert.Equal(1, destination.GetNumberOfPages());
        Assert.Equal(270, destination.GetPage(0).GetRotation());
        Assert.NotSame(source.GetPage(0).GetCOSObject(), destination.GetPage(0).GetCOSObject());
    }

    private static byte[] CreateSinglePagePdf(int rotation)
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
