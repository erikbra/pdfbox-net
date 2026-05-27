/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * FDF field/page/template model regression tests for issue #70.
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Fdf;
using PdfBox.Net.PDModel.Interactive.Action;
using PdfBox.Net.PDModel.Interactive.Annotation;

namespace PdfBox.Net.Tests;

public class FDFFieldPageTemplateModelTest
{
    [Fact]
    public void FieldHierarchyAndFieldPropertiesRoundTrip()
    {
        FDFField parent = new();
        parent.SetPartialFieldName("Address");

        FDFField child = new();
        child.SetPartialFieldName("City");
        child.SetValue("Oslo");
        parent.SetKids([child]);

        parent.SetFieldFlags(10);
        parent.SetSetFieldFlags(11);
        parent.SetClearFieldFlags(12);
        parent.SetWidgetFieldFlags(13);
        parent.SetSetWidgetFieldFlags(14);
        parent.SetClearWidgetFieldFlags(15);

        FDFOptionElement option = new();
        option.SetOption("Export");
        option.SetDefaultAppearanceString("/Helv 12 Tf 0 g");
        parent.SetOptions(["display", option]);

        PDActionJavaScript action = new("app.alert('clicked')");
        parent.SetAction(action);

        PDAdditionalActions additionalActions = new();
        additionalActions.SetF(new PDActionJavaScript("app.alert('format')"));
        parent.SetAdditionalActions(additionalActions);

        parent.SetRichText(new COSString("<body><p>rich</p></body>"));

        PDAppearanceDictionary appearanceDictionary = new();
        parent.SetAppearanceDictionary(appearanceDictionary);

        FDFNamedPageReference appearanceReference = new();
        appearanceReference.SetName("TemplateCover");
        parent.SetAppearanceStreamReference(appearanceReference);

        FDFIconFit iconFit = new();
        iconFit.SetScaleOption(FDFIconFit.SCALE_OPTION_NEVER);
        iconFit.SetScaleType(FDFIconFit.SCALE_TYPE_ANAMORPHIC);
        iconFit.SetScaleToFitAnnotation(true);
        parent.SetIconFit(iconFit);

        Assert.Equal("Address", parent.GetPartialFieldName());
        Assert.Equal("City", parent.GetKids()![0].GetPartialFieldName());
        Assert.Equal("Oslo", parent.GetKids()![0].GetValue());
        Assert.Equal(10, parent.GetFieldFlags());
        Assert.Equal(11, parent.GetSetFieldFlags());
        Assert.Equal(12, parent.GetClearFieldFlags());
        Assert.Equal(13, parent.GetWidgetFieldFlags());
        Assert.Equal(14, parent.GetSetWidgetFieldFlags());
        Assert.Equal(15, parent.GetClearWidgetFieldFlags());
        Assert.Equal("display", parent.GetOptions()![0]);
        Assert.Equal("Export", ((FDFOptionElement)parent.GetOptions()![1]).GetOption());
        Assert.IsType<PDActionJavaScript>(parent.GetAction());
        Assert.NotNull(parent.GetAdditionalActions());
        Assert.Equal("<body><p>rich</p></body>", parent.GetRichText());
        Assert.NotNull(parent.GetAppearanceDictionary());
        Assert.Equal("TemplateCover", parent.GetAppearanceStreamReference()?.GetName());
        Assert.Equal(FDFIconFit.SCALE_OPTION_NEVER, parent.GetIconFit()?.GetScaleOption());
        Assert.Equal(FDFIconFit.SCALE_TYPE_ANAMORPHIC, parent.GetIconFit()?.GetScaleType());
        Assert.True(parent.GetIconFit()?.ShouldScaleToFitAnnotation());
        Assert.Equal(0.5f, parent.GetIconFit()!.GetFractionalSpaceToAllocate().GetMin());
        Assert.Equal(0.5f, parent.GetIconFit()!.GetFractionalSpaceToAllocate().GetMax());
    }

    [Fact]
    public void DictionaryPageAndTemplateModelRoundTrip()
    {
        using FDFDocument document = new();
        FDFDictionary dictionary = document.GetCatalog().GetFDF();

        FDFField field = new();
        field.SetPartialFieldName("Root");
        dictionary.SetFields([field]);

        FDFNamedPageReference templateReference = new();
        templateReference.SetName("Cover");

        FDFTemplate template = new();
        template.SetTemplateReference(templateReference);
        template.SetFields([field]);
        template.SetRename(true);

        FDFPage page = new();
        page.SetTemplates([template]);
        page.SetPageInfo(new FDFPageInfo());
        dictionary.SetPages([page]);

        Assert.Single(dictionary.GetFields()!);
        Assert.Equal("Root", dictionary.GetFields()![0].GetPartialFieldName());
        Assert.Single(dictionary.GetPages()!);

        FDFPage loadedPage = dictionary.GetPages()![0];
        Assert.NotNull(loadedPage.GetPageInfo());
        Assert.Single(loadedPage.GetTemplates()!);
        Assert.Equal("Cover", loadedPage.GetTemplates()![0].GetTemplateReference()?.GetName());
        Assert.True(loadedPage.GetTemplates()![0].ShouldRename());
        Assert.Single(loadedPage.GetTemplates()![0].GetFields()!);
        Assert.Equal("Root", loadedPage.GetTemplates()![0].GetFields()![0].GetPartialFieldName());
    }
}
