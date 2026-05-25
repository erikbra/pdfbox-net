/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Closeout regression tests for issue #47 documentinterchange milestone:
 * fixture-based tagged read path, attribute-owner dispatch completion,
 * artifact marked-content handling, and remaining documentinterchange helper types.
 *
 * PORT_MODE: native-test
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;
using PdfBox.Net.PDModel.DocumentInterchange.MarkedContent;
using PdfBox.Net.PDModel.DocumentInterchange.Prepress;
using PdfBox.Net.PDModel.DocumentInterchange.TaggedPdf;
using Xunit;

namespace PdfBox.Net.Tests;

public class DocumentInterchangeRegressionCloseoutTest
{
    [Fact]
    public void TaggedFixture_LoadsAndResolvesParentTreeReadPath()
    {
        string fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "TaggedPdf", "minimal-tagged.pdf");
        Assert.True(File.Exists(fixturePath), $"Fixture not found: {fixturePath}");

        using PDDocument document = PDDocument.Load(fixturePath);
        PDDocumentCatalog catalog = document.GetDocumentCatalog();

        PDMarkInfo? markInfo = catalog.GetMarkInfo();
        Assert.NotNull(markInfo);
        Assert.True(markInfo!.IsMarked());

        PDStructureTreeRoot? treeRoot = catalog.GetStructureTreeRoot();
        Assert.NotNull(treeRoot);
        Assert.Equal(1, treeRoot!.GetParentTreeNextKey());

        IList<PDStructureElement> entries = treeRoot.GetParentTreeEntries(0);
        PDStructureElement element = Assert.Single(entries);
        Assert.Equal("P", element.GetStructureType());
    }

    [Fact]
    public void MarkedContent_Create_DispatchesArtifactSubtype()
    {
        COSDictionary properties = new();
        properties.SetName(COSName.TYPE, "Pagination");
        properties.SetName(COSName.SUBTYPE, "Header");
        properties.SetItem(COSName.GetPDFName("BBox"), COSArray.Of(0f, 0f, 100f, 25f));
        COSArray attached = new();
        attached.Add(COSName.GetPDFName("Top"));
        attached.Add(COSName.GetPDFName("Left"));
        properties.SetItem(COSName.GetPDFName("Attached"), attached);

        PDMarkedContent markedContent = PDMarkedContent.Create(COSName.GetPDFName("Artifact"), properties);
        PDArtifactMarkedContent artifact = Assert.IsType<PDArtifactMarkedContent>(markedContent);

        Assert.Equal("Pagination", artifact.GetArtifactType());
        Assert.Equal("Header", artifact.GetSubtype());
        Assert.NotNull(artifact.GetBBox());
        Assert.True(artifact.IsTopAttached());
        Assert.True(artifact.IsLeftAttached());
        Assert.False(artifact.IsRightAttached());
    }

    [Fact]
    public void AttributeFactory_Dispatches_PrintField_And_ExportFormatOwners()
    {
        COSDictionary printDict = new();
        printDict.SetName(COSName.GetPDFName("O"), PDPrintFieldAttributeObject.OwnerPrintField);
        Assert.IsType<PDPrintFieldAttributeObject>(PDAttributeObject.Create(printDict));

        COSDictionary exportDict = new();
        exportDict.SetName(COSName.GetPDFName("O"), PDExportFormatAttributeObject.OwnerHtml4_01);
        Assert.IsType<PDExportFormatAttributeObject>(PDAttributeObject.Create(exportDict));
    }

    [Fact]
    public void PrintFieldAndExportAttributes_RoundTrip()
    {
        PDPrintFieldAttributeObject print = new();
        print.SetRole(PDPrintFieldAttributeObject.RoleCb);
        print.SetCheckedState(PDPrintFieldAttributeObject.CheckedStateOn);
        print.SetAlternateName("Consent checkbox");
        Assert.Equal(PDPrintFieldAttributeObject.RoleCb, print.GetRole());
        Assert.Equal(PDPrintFieldAttributeObject.CheckedStateOn, print.GetCheckedState());
        Assert.Equal("Consent checkbox", print.GetAlternateName());

        PDExportFormatAttributeObject export = new(PDExportFormatAttributeObject.OwnerCss2_00);
        export.SetListNumbering(PDListAttributeObject.ListNumberingLowerAlpha);
        Assert.Equal(PDExportFormatAttributeObject.OwnerCss2_00, export.GetOwner());
        Assert.Equal(PDListAttributeObject.ListNumberingLowerAlpha, export.GetListNumbering());
    }

    [Fact]
    public void BoxStyle_DefaultsAndRoundTrip()
    {
        PDBoxStyle style = new();
        Assert.Equal(PDBoxStyle.GuidelineStyleSolid, style.GetGuidelineStyle());
        Assert.Equal(1f, style.GetGuidelineWidth(), precision: 3);
        Assert.NotNull(style.GetGuidelineColor());
        Assert.NotNull(style.GetLineDashPattern());

        style.SetGuidelineStyle(PDBoxStyle.GuidelineStyleDashed);
        style.SetGuidelineWidth(2.5f);
        Assert.Equal(PDBoxStyle.GuidelineStyleDashed, style.GetGuidelineStyle());
        Assert.Equal(2.5f, style.GetGuidelineWidth(), precision: 3);
    }

    [Fact]
    public void PropertyListAndStandardStructureTypes_AreUsable()
    {
        COSDictionary dict = new();
        dict.SetName(COSName.TYPE, "Custom");
        PDPropertyList list = PDPropertyList.Create(dict);
        Assert.Same(dict, list.GetCOSObject());

        Assert.Contains(StandardStructureTypes.P, StandardStructureTypes.Types);
        Assert.Contains(StandardStructureTypes.Table, StandardStructureTypes.Types);
    }

    [Fact]
    public void ParentTreeValue_WrapsDictionaryAndArray()
    {
        COSDictionary dictionary = new();
        COSArray array = new();
        PDParentTreeValue dictValue = new(dictionary);
        PDParentTreeValue arrayValue = new(array);

        Assert.Same(dictionary, dictValue.GetCOSObject());
        Assert.Same(array, arrayValue.GetCOSObject());
    }

    [Fact]
    public void FourColours_StoresEdgeColors()
    {
        PDFourColours colors = new();
        Assert.Null(colors.GetBeforeColor());

        colors.SetBeforeColor(new PdfBox.Net.PDModel.Graphics.Color.PDColor(new[] { 1f, 0f, 0f }, PdfBox.Net.PDModel.Graphics.Color.PDDeviceRGB.Instance));
        Assert.NotNull(colors.GetBeforeColor());
    }
}
