using PdfBox.Net.ContentStream.Operator;
using PdfBox.Net.COS;
using PdfBox.Net.Filter;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.DocumentInterchange.MarkedContent;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.PDModel.Graphics.OptionalContent;
using PdfBox.Net.PDModel.Graphics.Patterns;
using PdfBox.Net.PDModel.Graphics.Shading;
using PdfBox.Net.PDModel.Graphics.State;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.PdfParser;
using PdfBox.Net.PdfWriter;
using PdfBox.Net.Rendering;

namespace PdfBox.Net.Tests;

public class PatternOptionalContentInlineImageTest
{
    [Fact]
    public void TilingAndShadingPatterns_RoundTripThroughResources()
    {
        PDTilingPattern tilingPattern = new();
        tilingPattern.SetPaintType(PDTilingPattern.PAINT_UNCOLORED);
        tilingPattern.SetTilingType(PDTilingPattern.TILING_NO_DISTORTION);
        tilingPattern.SetXStep(12.5f);
        tilingPattern.SetYStep(7.25f);
        tilingPattern.SetBBox(new PDRectangle(1, 2, 30, 40));

        COSDictionary shadingDict = new();
        shadingDict.SetInt(COSName.SHADING_TYPE, PDShading.SHADING_TYPE2);
        shadingDict.SetItem(COSName.COLORSPACE, COSName.GetPDFName("DeviceRGB"));
        shadingDict.SetItem(COSName.COORDS, COSArray.Of(0f, 0f, 10f, 0f));
        PDShadingType2 shading = new(shadingDict);

        PDExtendedGraphicsState extGState = new(new COSDictionary());
        extGState.GetCOSObject().SetFloat(COSName.CA, 0.5f);

        PDShadingPattern shadingPattern = new();
        shadingPattern.SetExtendedGraphicsState(extGState);
        shadingPattern.SetShading(shading);

        COSDictionary patternDict = new();
        COSDictionary patterns = new();
        patterns.SetItem(COSName.GetPDFName("P1"), tilingPattern.GetCOSObject());
        patterns.SetItem(COSName.GetPDFName("P2"), shadingPattern.GetCOSObject());
        patternDict.SetItem(COSName.GetPDFName("Pattern"), patterns);

        PDResources resources = new(patternDict);

        PDTilingPattern resolvedTiling = Assert.IsType<PDTilingPattern>(resources.GetPattern(COSName.GetPDFName("P1")));
        Assert.Equal(PDTilingPattern.PAINT_UNCOLORED, resolvedTiling.GetPaintType());
        Assert.Equal(PDTilingPattern.TILING_NO_DISTORTION, resolvedTiling.GetTilingType());
        Assert.Equal(12.5f, resolvedTiling.GetXStep());
        Assert.Equal(7.25f, resolvedTiling.GetYStep());
        Assert.Equal(30f, resolvedTiling.GetBBox()!.GetWidth());

        PDShadingPattern resolvedShading = Assert.IsType<PDShadingPattern>(resources.GetPattern(COSName.GetPDFName("P2")));
        Assert.IsType<PDExtendedGraphicsState>(resolvedShading.GetExtendedGraphicsState());
        Assert.IsType<PDShadingType2>(resolvedShading.GetShading());
        Assert.Equal(["P1", "P2"], resources.GetPatternNames().Select(static name => name.GetName()).OrderBy(static n => n).ToArray());
    }

    [Fact]
    public void OptionalContentProperties_ResolveTypedPropertyListsAndRendererVisibility()
    {
        PDOptionalContentGroup group = new("Layer 1");
        PDOptionalContentMembershipDictionary membership = new();
        membership.SetOCGs([group]);
        membership.SetVisibilityPolicy(COSName.GetPDFName("AllOn"));

        COSDictionary propertiesRoot = new();
        COSDictionary propertyLists = new();
        propertyLists.SetItem(COSName.GetPDFName("MC0"), group.GetCOSObject());
        propertyLists.SetItem(COSName.GetPDFName("MC1"), membership.GetCOSObject());
        propertiesRoot.SetItem(COSName.GetPDFName("Properties"), propertyLists);

        PDResources resources = new(propertiesRoot);
        Assert.IsType<PDOptionalContentGroup>(resources.GetProperties(COSName.GetPDFName("MC0")));
        Assert.IsType<PDOptionalContentMembershipDictionary>(resources.GetProperties(COSName.GetPDFName("MC1")));
        Assert.Equal(["MC0", "MC1"], resources.GetPropertiesNames().Select(static name => name.GetName()).OrderBy(static n => n).ToArray());

        PDOptionalContentProperties ocProperties = new();
        ocProperties.AddGroup(group);
        Assert.True(ocProperties.HasGroup("Layer 1"));
        Assert.Same(group.GetCOSObject(), ocProperties.GetGroup("Layer 1")!.GetCOSObject());
        Assert.True(ocProperties.IsGroupEnabled(group));

        _ = ocProperties.SetGroupEnabled(group, false);
        Assert.False(ocProperties.IsGroupEnabled(group));

        using PDDocument document = new();
        document.GetDocumentCatalog().SetOCProperties(ocProperties);
        PDFRenderer renderer = new(document);
        Assert.False(renderer.IsGroupEnabled(group));

        _ = ocProperties.SetGroupEnabled("Layer 1", true);
        Assert.True(renderer.IsGroupEnabled(group));

        Assert.IsType<PDOptionalContentGroup>(PDPropertyList.Create(group.GetCOSObject()));
        Assert.IsType<PDOptionalContentMembershipDictionary>(PDPropertyList.Create(membership.GetCOSObject()));
    }

    [Fact]
    public void InlineImage_DecodesAbbreviatedDictionaryEntries()
    {
        byte[] decoded = [0x01, 0x02, 0x03, 0x04];
        byte[] encoded = EncodeFlate(decoded);

        COSDictionary parameters = new();
        parameters.SetInt(COSName.GetPDFName("W"), 2);
        parameters.SetInt(COSName.H, 2);
        parameters.SetInt(COSName.GetPDFName("BPC"), 8);
        parameters.SetItem(COSName.CS, COSName.GetPDFName("RGB"));
        parameters.SetItem(COSName.F, COSName.GetPDFName("Fl"));

        PDInlineImage image = new(parameters, encoded, new PDResources());

        Assert.Equal(2, image.GetWidth());
        Assert.Equal(2, image.GetHeight());
        Assert.Equal(8, image.GetBitsPerComponent());
        Assert.Equal("DeviceRGB", image.GetColorSpace().GetName());
        Assert.Equal(["Fl"], image.GetFilters());
        Assert.Equal(decoded, image.GetData());

        using Stream stopFiltered = image.CreateInputStream(["Fl"]);
        using MemoryStream raw = new();
        stopFiltered.CopyTo(raw);
        Assert.Equal(encoded, raw.ToArray());
    }

    [Fact]
    public void ParsedInlineImageOperator_CanBeWrappedAsPdInlineImage()
    {
        Operator inlineImage = Operator.GetOperator(OperatorName.BEGIN_INLINE_IMAGE);
        COSDictionary parameters = new();
        parameters.SetInt(COSName.GetPDFName("W"), 1);
        parameters.SetInt(COSName.H, 1);
        parameters.SetInt(COSName.GetPDFName("BPC"), 8);
        parameters.SetItem(COSName.CS, COSName.GetPDFName("G"));
        inlineImage.SetImageParameters(parameters);
        inlineImage.SetImageData([0x45, 0x49, 0x00]);

        using MemoryStream stream = new();
        new ContentStreamWriter(stream).WriteToken(inlineImage);
        stream.Position = 0;

        Operator parsed = Assert.IsType<Operator>(Assert.Single(new PDFStreamParser(stream).ParseTokens()));
        PDInlineImage image = new(parsed.GetImageParameters()!, parsed.GetImageData()!, new PDResources());

        Assert.Equal(1, image.GetWidth());
        Assert.Equal("DeviceGray", image.GetColorSpace().GetName());
        Assert.Equal(new byte[] { 0x45, 0x49, 0x00 }, image.GetData());
    }

    private static byte[] EncodeFlate(byte[] data)
    {
        using MemoryStream input = new(data);
        using MemoryStream output = new();
        FilterFactory.Instance.GetFilter(COSName.FLATE_DECODE).Encode(input, output, new COSDictionary(), 0);
        return output.ToArray();
    }
}
