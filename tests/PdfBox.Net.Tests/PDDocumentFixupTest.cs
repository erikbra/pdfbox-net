/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PORT_MODE: native-test
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Fixup;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.Form;

namespace PdfBox.Net.Tests;

public class PDDocumentFixupTest
{
    [Fact]
    public void GetAcroFormAppliesDefaultFixupResources()
    {
        using PDDocument document = new();
        PDAcroForm acroForm = new(document);
        document.GetDocumentCatalog().SetAcroForm(acroForm);

        PDAcroForm? fixedUp = document.GetDocumentCatalog().GetAcroForm();

        Assert.NotNull(fixedUp);
        Assert.Equal("/Helv 0 Tf 0 g ", fixedUp!.GetDefaultAppearance());
        Assert.NotNull(fixedUp.GetDefaultResources());
        Assert.NotNull(fixedUp.GetDefaultResources()!.GetFont(COSName.GetPDFName("Helv")));
        Assert.NotNull(fixedUp.GetDefaultResources()!.GetFont(COSName.GetPDFName("ZaDb")));
    }

    [Fact]
    public void GetAcroFormWithoutFixupLeavesOriginalValuesUntouched()
    {
        using PDDocument document = new();
        PDAcroForm acroForm = new(document);
        document.GetDocumentCatalog().SetAcroForm(acroForm);

        PDAcroForm? untouched = document.GetDocumentCatalog().GetAcroForm(null);

        Assert.NotNull(untouched);
        Assert.Equal(string.Empty, untouched!.GetDefaultAppearance());
        Assert.Null(untouched.GetDefaultResources());
    }

    [Fact]
    public void GetAcroFormUsesProcessorDispatchForDistinctFixupInstances()
    {
        using PDDocument document = new();
        document.GetDocumentCatalog().SetAcroForm(new PDAcroForm(document));

        CountingFixup firstFixup = new(document);
        document.GetDocumentCatalog().GetAcroForm(firstFixup);
        document.GetDocumentCatalog().GetAcroForm(firstFixup);

        CountingFixup secondFixup = new(document);
        document.GetDocumentCatalog().GetAcroForm(secondFixup);

        Assert.Equal(1, firstFixup.ApplyCount);
        Assert.Equal(1, secondFixup.ApplyCount);
    }

    [Fact]
    public void DefaultFixupRebuildsOrphanWidgetsAndGeneratesAppearances()
    {
        using PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);

        PDAcroForm acroForm = new(document);
        acroForm.SetNeedAppearances(true);
        acroForm.SetFields([]);
        document.GetDocumentCatalog().SetAcroForm(acroForm);

        PDAnnotationWidget widget = new();
        widget.SetRectangle(new PDRectangle(20, 20, 120, 24));
        widget.GetCOSDictionary().SetName(COSName.GetPDFName("FT"), "Tx");
        widget.GetCOSDictionary().SetString(COSName.T, "orphanText");
        widget.GetCOSDictionary().SetString(COSName.GetPDFName("V"), "orphan value");

        page.SetAnnotations([widget]);

        PDAcroForm fixedUp = document.GetDocumentCatalog().GetAcroForm()!;

        Assert.False(fixedUp.GetNeedAppearances());
        PDField field = Assert.Single(fixedUp.GetFields());
        Assert.Equal("orphanText", field.GetPartialName());
        PDAnnotationWidget resolvedWidget = Assert.Single(field.GetWidgets());
        Assert.NotNull(resolvedWidget.GetNormalAppearanceStream());
    }

    private sealed class CountingFixup : AbstractFixup
    {
        public int ApplyCount { get; private set; }

        public CountingFixup(PDDocument document)
            : base(document)
        {
        }

        public override void Apply()
        {
            ApplyCount++;
        }
    }
}
