using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Annotations;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Common.FileSpecification;
using PdfBox.Net.PDModel.Common.Function;
using PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;
using PdfBox.Net.PDModel.DocumentInterchange.MarkedContent;
using PdfBox.Net.PDModel.DocumentInterchange.Prepress;
using PdfBox.Net.PDModel.Fdf;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.Measurement;
using PdfBox.Net.PdfWriter.Compress;

namespace PdfBox.Net.Tests;

public class Issue506PDModelApiParityTest
{
    [Fact]
    public void PDDocumentCompatibilityMembersRoundTrip()
    {
        using PDDocument document = new();
        document.AddPage(new PDPage());
        document.SetDocumentId(1234);

        using MemoryStream output = new();
        document.Save(output, CompressParameters.NoCompression);

        Assert.Equal(1234, document.GetDocumentId());
        Assert.True(document.GetCurrentAccessPermission().IsOwnerPermission());
        Assert.NotEmpty(output.ToArray());
    }

    [Fact]
    public void PDPageCompatibilityMembersRoundTrip()
    {
        using PDDocument document = new();
        PDPage page = new();
        PDStream contents = new();
        byte[] bytes = "BT ET"u8.ToArray();
        using (Stream output = contents.CreateOutputStream())
        {
            output.Write(bytes);
        }

        page.SetContents(contents);
        page.SetUserUnit(2.5f);
        page.SetMetadata(new PDMetadata(document));

        PDViewportDictionary viewport = new();
        page.SetViewports([viewport]);

        PDAnnotationText annotation = new();
        page.SetAnnotations([annotation]);
        AnnotationFilter keepNone = _ => false;

        Assert.Single(page.GetContentStreams());
        Assert.Equal(bytes.Length, page.GetContentsForStreamParsing()!.Length());
        Assert.Equal(page.GetCropBox().GetLowerLeftX(), page.GetBBox()!.GetLowerLeftX());
        Assert.NotNull(page.GetMatrix());
        Assert.NotNull(page.GetMetadata());
        Assert.Equal(2.5f, page.GetUserUnit());
        Assert.Single(page.GetViewports()!);
        Assert.Empty(page.GetAnnotations(keepNone));
    }

    [Fact]
    public void PDStreamFileAndDecodeParametersRoundTrip()
    {
        PDStream stream = new();
        Dictionary<string, object> decodeParams = new()
        {
            ["Predictor"] = 12,
            ["Columns"] = 4
        };

        stream.SetDecodeParms([decodeParams]);
        stream.SetFileDecodeParams([decodeParams]);
        stream.SetFileFilters(["FlateDecode"]);

        PDSimpleFileSpecification file = new();
        file.SetFile("external.bin");
        stream.SetFile(file);

        Assert.Equal("external.bin", stream.GetFile()?.GetFile());
        Assert.Equal("FlateDecode", Assert.Single(stream.GetFileFilters()));
        Assert.Equal(12, ((IDictionary<string, object>)Assert.Single(stream.GetDecodeParms()!))["Predictor"]);
        Assert.Equal(4, ((IDictionary<string, object>)Assert.Single(stream.GetFileDecodeParams()!))["Columns"]);
    }

    [Fact]
    public void FDFDictionaryCompatibilityMembersRoundTrip()
    {
        FDFDictionary dictionary = new();
        COSStream differences = new();
        PDSimpleFileSpecification embedded = new();
        embedded.SetFile("child.fdf");

        dictionary.SetDifferences(differences);
        dictionary.SetTarget("_blank");
        dictionary.SetEmbeddedFDFs([embedded]);

        Assert.Same(differences, dictionary.GetDifferences());
        Assert.Equal("_blank", dictionary.GetTarget());
        Assert.Equal("child.fdf", Assert.Single(dictionary.GetEmbeddedFDFs()!).GetFile());
    }

    [Fact]
    public void DocumentInterchangeCompatibilityMembersRoundTrip()
    {
        PDStructureTreeRoot root = new();
        PDStructureElementNameTreeNode idTree = new();
        root.SetIDTree(idTree);

        PDUserAttributeObject userAttributes = new();
        PDUserProperty property1 = new(userAttributes);
        PDUserProperty property2 = new(property1.GetCOSObject(), userAttributes);
        PDMarkedContent parent = PDMarkedContent.Create(COSName.GetPDFName("Div"), null);
        PDMarkedContent child = PDMarkedContent.Create(COSName.GetPDFName("Span"), null);
        parent.AddMarkedContent(child);

        Assert.NotNull(root.GetIDTree());
        Assert.Equal("UserProperties", PDUserAttributeObject.OWNER_USER_PROPERTIES);
        Assert.Equal(property1, property2);
        Assert.Same(child, Assert.Single(parent.GetContents()));
        Assert.Contains("contents=", parent.ToString(), StringComparison.Ordinal);
        Assert.Equal(PDBoxStyle.GuidelineStyleSolid, PDBoxStyle.GUIDELINE_STYLE_SOLID);
        Assert.Equal(PDBoxStyle.GuidelineStyleDashed, PDBoxStyle.GUIDELINE_STYLE_DASHED);
    }

    [Fact]
    public void PDFunctionType2ToStringUsesJavaShape()
    {
        PDFunctionType2 function = new(new COSDictionary());

        Assert.StartsWith("FunctionType2{", function.ToString(), StringComparison.Ordinal);
    }
}
