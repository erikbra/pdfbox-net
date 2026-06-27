/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * API parity tests for signature/encryption issue #510 with AI assistance.
 *
 * PORT_MODE: adapted
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.DigitalSignature;
using PdfBox.Net.PDModel.Interactive.DigitalSignature.Visible;
using PdfBox.Net.PDModel.Interactive.Form;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Util;

namespace PdfBox.Net.Tests;

public class Issue510SignatureEncryptionApiParityTest
{
    [Fact]
    public void VisibleTemplateStructure_RoundTripsJavaNamedMembers()
    {
        using PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);
        PDAcroForm acroForm = new(document);
        PDSignatureField signatureField = new(acroForm);
        PDFTemplateStructure structure = new();
        PDSignature signature = new();
        COSDictionary acroFormDictionary = new();
        PDRectangle signatureRectangle = new(1, 2, 3, 4);
        AffineTransform affineTransform = new(1, 2, 3, 4, 5, 6);
        COSArray procSet = new();
        PDImageXObject image = new(new PDStream(document), null);
        PDRectangle formatterRectangle = new(10, 20);
        PDStream holderFormStream = new(document);
        PDResources holderFormResources = new();
        PDFormXObject holderForm = new(holderFormStream);
        PDAppearanceDictionary appearanceDictionary = new();
        PDStream innerFormStream = new(document);
        PDResources innerFormResources = new();
        PDFormXObject innerForm = new(innerFormStream);
        PDStream imageFormStream = new(document);
        PDResources imageFormResources = new();
        COSName innerFormName = COSName.GetPDFName("FRM");
        COSName imageFormName = COSName.GetPDFName("n2");
        COSName imageName = COSName.GetPDFName("img0");
        COSDictionary widgetDictionary = new();

        structure.SetTemplate(document);
        structure.SetPage(page);
        structure.SetAcroForm(acroForm);
        structure.SetSignatureField(signatureField);
        structure.SetPdSignature(signature);
        structure.SetAcroFormDictionary(acroFormDictionary);
        structure.SetSignatureRectangle(signatureRectangle);
        structure.SetAffineTransform(affineTransform);
        structure.SetProcSet(procSet);
        structure.SetImage(image);
        structure.SetFormatterRectangle(formatterRectangle);
        structure.SetHolderFormStream(holderFormStream);
        structure.SetHolderFormResources(holderFormResources);
        structure.SetHolderForm(holderForm);
        structure.SetAppearanceDictionary(appearanceDictionary);
        structure.SetInnterFormStream(innerFormStream);
        structure.SetInnerFormResources(innerFormResources);
        structure.SetInnerForm(innerForm);
        structure.SetImageFormStream(imageFormStream);
        structure.SetImageFormResources(imageFormResources);
        structure.SetAcroFormFields([signatureField]);
        structure.SetInnerFormName(innerFormName);
        structure.SetImageFormName(imageFormName);
        structure.SetImageName(imageName);
        structure.SetVisualSignature(document.GetDocument());
        structure.SetImageForm(innerForm);
        structure.SetWidgetDictionary(widgetDictionary);

        Assert.Same(document, structure.GetTemplate());
        Assert.Same(page, structure.GetPage());
        Assert.Same(acroForm, structure.GetAcroForm());
        Assert.Same(signatureField, structure.GetSignatureField());
        Assert.Same(signature, structure.GetPdSignature());
        Assert.Same(acroFormDictionary, structure.GetAcroFormDictionary());
        Assert.Same(signatureRectangle, structure.GetSignatureRectangle());
        Assert.Same(affineTransform, structure.GetAffineTransform());
        Assert.Same(procSet, structure.GetProcSet());
        Assert.Same(image, structure.GetImage());
        Assert.Same(formatterRectangle, structure.GetFormatterRectangle());
        Assert.Same(holderFormStream, structure.GetHolderFormStream());
        Assert.Same(holderFormResources, structure.GetHolderFormResources());
        Assert.Same(holderForm, structure.GetHolderForm());
        Assert.Same(appearanceDictionary, structure.GetAppearanceDictionary());
        Assert.Same(innerFormStream, structure.GetInnerFormStream());
        Assert.Same(innerFormResources, structure.GetInnerFormResources());
        Assert.Same(innerForm, structure.GetInnerForm());
        Assert.Same(imageFormStream, structure.GetImageFormStream());
        Assert.Same(imageFormResources, structure.GetImageFormResources());
        Assert.Same(signatureField, Assert.Single(structure.GetAcroFormFields()!));
        Assert.Same(innerFormName, structure.GetInnerFormName());
        Assert.Same(imageFormName, structure.GetImageFormName());
        Assert.Same(imageName, structure.GetImageName());
        Assert.Same(document.GetDocument(), structure.GetVisualSignature());
        Assert.Same(innerForm, structure.GetImageForm());
        Assert.Same(widgetDictionary, structure.GetWidgetDictionary());
    }

    [Fact]
    public void VisibleSigBuilder_CreatesTemplatePartsThroughParityApi()
    {
        using PDDocument sourceDocument = new();
        PDPage sourcePage = new(new PDRectangle(300, 200));
        sourceDocument.AddPage(sourcePage);
        using MemoryStream imageStream = new([1, 2, 3]);
        PDVisibleSignDesigner designer = new PDVisibleSignDesigner(sourceDocument, imageStream, 1)
            .Coordinates(10, 20)
            .Width(50)
            .Height(25);

        PDVisibleSigBuilder builder = new();
        builder.CreateProcSetArray();
        builder.CreatePage(designer);
        builder.CreateTemplate(builder.GetStructure().GetPage()!);
        PDDocument template = builder.GetStructure().GetTemplate()!;
        builder.CreateAcroForm(template);
        builder.CreateSignatureField(builder.GetStructure().GetAcroForm()!);
        builder.CreateSignature(builder.GetStructure().GetSignatureField()!, builder.GetStructure().GetPage()!, "Signer");
        builder.CreateAcroFormDictionary(builder.GetStructure().GetAcroForm()!, builder.GetStructure().GetSignatureField()!);
        builder.CreateAffineTransform(new AffineTransform(1, 0, 0, 1, 2, 3));
        builder.CreateSignatureRectangle(builder.GetStructure().GetSignatureField()!, designer);
        builder.CreateFormatterRectangle(designer.GetFormatterRectangleParameters());
        PDImageXObject image = new(new PDStream(template), null);
        builder.CreateSignatureImage(template, image);
        builder.CreateHolderFormStream(template);
        builder.CreateHolderFormResources();
        builder.CreateHolderForm(builder.GetStructure().GetHolderFormResources()!, builder.GetStructure().GetHolderFormStream()!, builder.GetStructure().GetFormatterRectangle()!);
        builder.CreateAppearanceDictionary(builder.GetStructure().GetHolderForm()!, builder.GetStructure().GetSignatureField()!);
        builder.CreateInnerFormStream(template);
        builder.CreateInnerFormResource();
        builder.CreateInnerForm(builder.GetStructure().GetInnerFormResources()!, builder.GetStructure().GetInnerFormStream()!, builder.GetStructure().GetFormatterRectangle()!);
        builder.InsertInnerFormToHolderResources(builder.GetStructure().GetInnerForm()!, builder.GetStructure().GetHolderFormResources()!);
        builder.CreateImageFormStream(template);
        builder.CreateImageFormResources();
        builder.CreateImageForm(
            builder.GetStructure().GetImageFormResources()!,
            builder.GetStructure().GetInnerFormResources()!,
            builder.GetStructure().GetImageFormStream()!,
            builder.GetStructure().GetFormatterRectangle()!,
            builder.GetStructure().GetAffineTransform()!,
            builder.GetStructure().GetImage()!);
        builder.CreateBackgroundLayerForm(builder.GetStructure().GetInnerFormResources()!, builder.GetStructure().GetFormatterRectangle()!);
        builder.InjectProcSetArray(
            builder.GetStructure().GetInnerForm()!,
            builder.GetStructure().GetPage()!,
            builder.GetStructure().GetInnerFormResources()!,
            builder.GetStructure().GetImageFormResources()!,
            builder.GetStructure().GetHolderFormResources()!,
            builder.GetStructure().GetProcSet()!);
        builder.InjectAppearanceStreams(
            builder.GetStructure().GetHolderFormStream()!,
            builder.GetStructure().GetInnerFormStream()!,
            builder.GetStructure().GetImageFormStream()!,
            builder.GetStructure().GetImageFormName()!,
            builder.GetStructure().GetImageName()!,
            builder.GetStructure().GetInnerFormName()!,
            designer);
        builder.CreateVisualSignature(template);
        builder.CreateWidgetDictionary(builder.GetStructure().GetSignatureField()!, builder.GetStructure().GetHolderFormResources()!);

        Assert.Equal("Signer", builder.GetStructure().GetPdSignature()!.GetName());
        Assert.True(builder.GetStructure().GetAcroForm()!.IsSignaturesExist());
        Assert.NotNull(builder.GetStructure().GetAppearanceDictionary());
        Assert.NotNull(builder.GetStructure().GetWidgetDictionary());
        Assert.NotNull(builder.GetStructure().GetVisualSignature());
    }

    [Fact]
    public void VisibleSignDesigner_ExposesImageTransformRotationAndUnsupportedText()
    {
        using PDDocument document = new();
        PDPage page = new(new PDRectangle(200, 100));
        page.SetRotation(90);
        document.AddPage(page);

        byte[] initialImage = [1, 2, 3, 4];
        using MemoryStream imageStream = new(initialImage);
        PDVisibleSignDesigner designer = new PDVisibleSignDesigner(document, imageStream, 1)
            .Coordinates(10, 20)
            .Width(30)
            .Height(40);
        AffineTransform transform = new(1, 2, 3, 4, 5, 6);
        designer.Transform(transform);

        Assert.Equal(initialImage, designer.GetImage());
        Assert.Equal(5, designer.GetTransform().TranslateX);

        designer.AdjustForRotation();

        Assert.Equal(20, designer.GetxAxis());
        Assert.Equal(60, designer.GetyAxis());
        Assert.Equal(40, designer.GetWidth());
        Assert.Equal(30, designer.GetHeight());

        string imagePath = Path.Combine(Path.GetTempPath(), "pdfbox-net-visible-signature-image.bin");
        try
        {
            File.WriteAllBytes(imagePath, [9, 8, 7]);
            Assert.Same(designer, designer.SignatureImage(imagePath));
            Assert.Equal([9, 8, 7], designer.GetImage());
        }
        finally
        {
            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
            }
        }

        Assert.Throws<NotSupportedException>(() => designer.GetSignatureText());
        Assert.Throws<NotSupportedException>(() => designer.SignatureText("text"));
    }
}
