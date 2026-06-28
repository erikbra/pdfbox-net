using PdfBox.Net.COS;
using PdfBox.Net.MultiPdf;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using System.Text;
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

    [Fact]
    public void PDFMergerUtility_DestinationAccessorsRoundtrip()
    {
        using PDDocument metadataOwner = new();
        PDDocumentInformation info = CreateDestinationInformation();
        PDMetadata metadata = CreateMetadata(metadataOwner, "roundtrip-marker");

        PDFMergerUtility merger = new();

        merger.SetDestinationDocumentInformation(info);
        merger.SetDestinationMetadata(metadata);

        Assert.Same(info, merger.GetDestinationDocumentInformation());
        Assert.Same(metadata, merger.GetDestinationMetadata());

        merger.SetDestinationDocumentInformation(null);
        merger.SetDestinationMetadata(null);

        Assert.Null(merger.GetDestinationDocumentInformation());
        Assert.Null(merger.GetDestinationMetadata());
    }

    [Fact]
    public void PDFMergerUtility_AppliesDestinationInformationAndMetadata()
    {
        byte[] source1 = CreateSinglePagePdf(0);
        byte[] source2 = CreateSinglePagePdf(90);
        using PDDocument metadataOwner = new();
        PDMetadata metadata = CreateMetadata(metadataOwner, "merged-metadata-marker");
        using MemoryStream output = new();

        PDFMergerUtility merger = new()
        {
            DestinationStream = output
        };
        merger.AddSource(new MemoryStream(source1));
        merger.AddSource(new MemoryStream(source2));
        merger.SetDestinationDocumentInformation(CreateDestinationInformation());
        merger.SetDestinationMetadata(metadata);

        merger.MergeDocuments();

        output.Position = 0;
        using PDDocument merged = PDDocument.Load(output);

        Assert.Equal(2, merged.GetNumberOfPages());
        Assert.Equal("Merged destination", merged.GetDocumentInformation().GetTitle());
        Assert.Equal("PdfBox.Net", merged.GetDocumentInformation().GetAuthor());

        PDMetadata? mergedMetadata = merged.GetDocumentCatalog().GetMetadata();
        Assert.NotNull(mergedMetadata);
        using Stream metadataStream = mergedMetadata.ExportXMPMetadata();
        using StreamReader reader = new(metadataStream, Encoding.UTF8);
        Assert.Contains("merged-metadata-marker", reader.ReadToEnd());
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

    private static PDDocumentInformation CreateDestinationInformation()
    {
        PDDocumentInformation info = new();
        info.SetTitle("Merged destination");
        info.SetAuthor("PdfBox.Net");
        return info;
    }

    private static PDMetadata CreateMetadata(PDDocument owner, string marker)
    {
        PDMetadata metadata = new(owner);
        string xmp = $"""
            <?xpacket begin="" id="W5M0MpCehiHzreSzNTczkc9d"?>
            <x:xmpmeta xmlns:x="adobe:ns:meta/">
              <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#">
                <rdf:Description xmlns:pdfbox="https://github.com/erikbra/pdfbox-net/" pdfbox:marker="{marker}" />
              </rdf:RDF>
            </x:xmpmeta>
            <?xpacket end="w"?>
            """;
        metadata.ImportXMPMetadata(Encoding.UTF8.GetBytes(xmp));
        return metadata;
    }
}
