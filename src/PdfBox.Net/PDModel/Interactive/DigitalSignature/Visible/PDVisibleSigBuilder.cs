/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/digitalsignature/visible/PDVisibleSigBuilder.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using System.Text;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.Form;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature.Visible;

public class PDVisibleSigBuilder : PDFTemplateBuilder
{
    private readonly PDFTemplateStructure _structure = new();

    public PDFTemplateStructure GetStructure() => _structure;

    public void CreatePage(PDVisibleSignDesigner properties)
    {
        ArgumentNullException.ThrowIfNull(properties);
        _structure.SetPage(new PDPage(new PDRectangle(properties.GetPageWidth(), properties.GetPageHeight())));
    }

    public void CreateTemplate(PDPage page)
    {
        ArgumentNullException.ThrowIfNull(page);
        PDDocument template = new();
        template.AddPage(page);
        _structure.SetTemplate(template);
    }

    public void CreateAcroForm(PDDocument template)
    {
        ArgumentNullException.ThrowIfNull(template);
        PDAcroForm acroForm = new(template);
        template.GetDocumentCatalog().SetAcroForm(acroForm);
        _structure.SetAcroForm(acroForm);
    }

    public void CreateSignatureField(PDAcroForm acroForm)
    {
        ArgumentNullException.ThrowIfNull(acroForm);
        _structure.SetSignatureField(new PDSignatureField(acroForm));
    }

    public void CreateSignature(PDSignatureField pdSignatureField, PDPage page, string signerName)
    {
        ArgumentNullException.ThrowIfNull(pdSignatureField);
        ArgumentNullException.ThrowIfNull(page);

        PDSignature signature = new();
        PDAnnotationWidget widget = pdSignatureField.GetWidgets()[0];
        pdSignatureField.SetValue(signature);
        widget.SetPage(page);
        page.GetAnnotations().Add(widget);
        if (!string.IsNullOrEmpty(signerName))
        {
            signature.SetName(signerName);
        }

        _structure.SetPdSignature(signature);
    }

    public void CreateAcroFormDictionary(PDAcroForm acroForm, PDSignatureField signatureField)
    {
        ArgumentNullException.ThrowIfNull(acroForm);
        ArgumentNullException.ThrowIfNull(signatureField);

        IList<PDField> fields = acroForm.GetFields();
        COSDictionary acroFormDictionary = (COSDictionary)acroForm.GetCOSObject();
        acroForm.SetSignaturesExist(true);
        acroForm.SetAppendOnly(true);
        acroFormDictionary.SetDirect(true);
        fields.Add(signatureField);
        acroForm.SetDefaultAppearance("/sylfaen 0 Tf 0 g");
        _structure.SetAcroFormFields(fields);
        _structure.SetAcroFormDictionary(acroFormDictionary);
    }

    public void CreateSignatureRectangle(PDSignatureField signatureField, PDVisibleSignDesigner properties)
    {
        ArgumentNullException.ThrowIfNull(signatureField);
        ArgumentNullException.ThrowIfNull(properties);

        PDRectangle rectangle = new(
            properties.GetxAxis(),
            properties.GetTemplateHeight() - properties.GetyAxis() - properties.GetHeight(),
            properties.GetWidth(),
            properties.GetHeight());
        signatureField.GetWidgets()[0].SetRectangle(rectangle);
        _structure.SetSignatureRectangle(rectangle);
    }

    public void CreateAffineTransform(AffineTransform affineTransform)
    {
        ArgumentNullException.ThrowIfNull(affineTransform);
        _structure.SetAffineTransform(affineTransform.Clone());
    }

    public void CreateProcSetArray()
    {
        COSArray procSet = new();
        procSet.Add(COSName.GetPDFName("PDF"));
        procSet.Add(COSName.GetPDFName("Text"));
        procSet.Add(COSName.GetPDFName("ImageB"));
        procSet.Add(COSName.GetPDFName("ImageC"));
        procSet.Add(COSName.GetPDFName("ImageI"));
        _structure.SetProcSet(procSet);
    }

    public void CreateSignatureImage(PDDocument template, PDImageXObject image)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(image);
        _structure.SetImage(image);
    }

    public void CreateFormatterRectangle(int[] parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        if (parameters.Length < 4)
        {
            throw new ArgumentException("Formatter rectangle requires at least four values.", nameof(parameters));
        }

        PDRectangle rectangle = new(
            Math.Min(parameters[0], parameters[2]),
            Math.Min(parameters[1], parameters[3]),
            Math.Abs(parameters[2] - parameters[0]),
            Math.Abs(parameters[3] - parameters[1]));
        _structure.SetFormatterRectangle(rectangle);
    }

    public void CreateHolderFormStream(PDDocument template)
    {
        ArgumentNullException.ThrowIfNull(template);
        _structure.SetHolderFormStream(new PDStream(template));
    }

    public void CreateHolderFormResources()
    {
        _structure.SetHolderFormResources(new PDResources());
    }

    public void CreateHolderForm(PDResources holderFormResources, PDStream holderFormStream, PDRectangle bbox)
    {
        ArgumentNullException.ThrowIfNull(holderFormResources);
        ArgumentNullException.ThrowIfNull(holderFormStream);
        ArgumentNullException.ThrowIfNull(bbox);

        PDFormXObject holderForm = new(holderFormStream);
        holderForm.SetResources(holderFormResources);
        holderForm.SetBBox(bbox);
        holderForm.SetFormType(1);
        _structure.SetHolderForm(holderForm);
    }

    public void CreateAppearanceDictionary(PDFormXObject holderForm, PDSignatureField signatureField)
    {
        ArgumentNullException.ThrowIfNull(holderForm);
        ArgumentNullException.ThrowIfNull(signatureField);

        PDAppearanceDictionary appearance = new();
        appearance.GetCOSObject().SetDirect(true);
        COSStream holderStream = holderForm.GetCOSObject() ?? throw new InvalidOperationException("Holder form has no COS stream.");
        appearance.SetNormalAppearance(new PDAppearanceStream(holderStream));
        signatureField.GetWidgets()[0].SetAppearance(appearance);
        _structure.SetAppearanceDictionary(appearance);
    }

    public void CreateInnerFormStream(PDDocument template)
    {
        ArgumentNullException.ThrowIfNull(template);
        _structure.SetInnterFormStream(new PDStream(template));
    }

    public void CreateInnerFormResource()
    {
        _structure.SetInnerFormResources(new PDResources());
    }

    public void CreateInnerForm(PDResources innerFormResources, PDStream innerFormStream, PDRectangle bbox)
    {
        ArgumentNullException.ThrowIfNull(innerFormResources);
        ArgumentNullException.ThrowIfNull(innerFormStream);
        ArgumentNullException.ThrowIfNull(bbox);

        PDFormXObject innerForm = new(innerFormStream);
        innerForm.SetResources(innerFormResources);
        innerForm.SetBBox(bbox);
        innerForm.SetFormType(1);
        _structure.SetInnerForm(innerForm);
    }

    public void InsertInnerFormToHolderResources(PDFormXObject innerForm, PDResources holderFormResources)
    {
        ArgumentNullException.ThrowIfNull(innerForm);
        ArgumentNullException.ThrowIfNull(holderFormResources);

        COSName formName = COSName.GetPDFName("FRM");
        holderFormResources.Put(formName, innerForm);
        _structure.SetInnerFormName(formName);
    }

    public void CreateImageFormStream(PDDocument template)
    {
        ArgumentNullException.ThrowIfNull(template);
        _structure.SetImageFormStream(new PDStream(template));
    }

    public void CreateImageFormResources()
    {
        _structure.SetImageFormResources(new PDResources());
    }

    public void CreateImageForm(PDResources imageFormResources, PDResources innerFormResource, PDStream imageFormStream, PDRectangle bbox, AffineTransform affineTransform, PDImageXObject img)
    {
        ArgumentNullException.ThrowIfNull(imageFormResources);
        ArgumentNullException.ThrowIfNull(innerFormResource);
        ArgumentNullException.ThrowIfNull(imageFormStream);
        ArgumentNullException.ThrowIfNull(bbox);
        ArgumentNullException.ThrowIfNull(affineTransform);
        ArgumentNullException.ThrowIfNull(img);

        PDFormXObject imageForm = new(imageFormStream);
        imageForm.SetBBox(bbox);
        imageForm.SetMatrix(affineTransform);
        imageForm.SetResources(imageFormResources);
        imageForm.SetFormType(1);
        imageFormResources.GetCOSObject().SetDirect(true);

        COSName imageFormName = COSName.GetPDFName("n2");
        innerFormResource.Put(imageFormName, imageForm);
        COSName imageName = imageFormResources.Add(img, "img");
        _structure.SetImageForm(imageForm);
        _structure.SetImageFormName(imageFormName);
        _structure.SetImageName(imageName);
    }

    public void CreateBackgroundLayerForm(PDResources innerFormResource, PDRectangle bbox)
    {
        ArgumentNullException.ThrowIfNull(innerFormResource);
        ArgumentNullException.ThrowIfNull(bbox);

        PDDocument template = _structure.GetTemplate()
            ?? throw new InvalidOperationException("Template must be created before background layer form.");
        PDFormXObject background = new(new PDStream(template));
        background.SetBBox(bbox);
        background.SetResources(new PDResources());
        background.SetFormType(1);
        innerFormResource.Put(COSName.GetPDFName("n0"), background);
    }

    public void InjectProcSetArray(PDFormXObject innerForm, PDPage page, PDResources innerFormResources, PDResources imageFormResources, PDResources holderFormResources, COSArray procSet)
    {
        ArgumentNullException.ThrowIfNull(innerForm);
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(innerFormResources);
        ArgumentNullException.ThrowIfNull(imageFormResources);
        ArgumentNullException.ThrowIfNull(holderFormResources);
        ArgumentNullException.ThrowIfNull(procSet);

        COSName procSetName = COSName.GetPDFName("ProcSet");
        innerForm.GetResources()?.GetCOSObject().SetItem(procSetName, procSet);
        ((COSDictionary)page.GetCOSObject()).SetItem(procSetName, procSet);
        innerFormResources.GetCOSObject().SetItem(procSetName, procSet);
        imageFormResources.GetCOSObject().SetItem(procSetName, procSet);
        holderFormResources.GetCOSObject().SetItem(procSetName, procSet);
    }

    public void InjectAppearanceStreams(PDStream holderFormStream, PDStream innerFormStream, PDStream imageFormStream, COSName imageFormName, COSName imageName, COSName innerFormName, PDVisibleSignDesigner properties)
    {
        ArgumentNullException.ThrowIfNull(holderFormStream);
        ArgumentNullException.ThrowIfNull(innerFormStream);
        ArgumentNullException.ThrowIfNull(imageFormStream);
        ArgumentNullException.ThrowIfNull(imageFormName);
        ArgumentNullException.ThrowIfNull(imageName);
        ArgumentNullException.ThrowIfNull(innerFormName);
        ArgumentNullException.ThrowIfNull(properties);

        PDRectangle formatterRectangle = _structure.GetFormatterRectangle()
            ?? throw new InvalidOperationException("Formatter rectangle must be created before appearance streams.");
        int width = (int)formatterRectangle.GetWidth();
        int height = (int)formatterRectangle.GetHeight();

        string imageFormContent = $"q {width} 0 0 {height} 0 0 cm /{imageName.GetName()} Do Q\n";
        string holderFormContent = $"q 1 0 0 1 0 0 cm /{innerFormName.GetName()} Do Q\n";
        string innerFormContent = $"q 1 0 0 1 0 0 cm /n0 Do Q q 1 0 0 1 0 0 cm /{imageFormName.GetName()} Do Q\n";

        AppendRawCommands(holderFormStream.CreateOutputStream(), holderFormContent);
        AppendRawCommands(innerFormStream.CreateOutputStream(), innerFormContent);
        AppendRawCommands(imageFormStream.CreateOutputStream(), imageFormContent);
    }

    public void AppendRawCommands(Stream os, string commands)
    {
        ArgumentNullException.ThrowIfNull(os);
        ArgumentNullException.ThrowIfNull(commands);
        using (os)
        {
            os.Write(Encoding.UTF8.GetBytes(commands));
        }
    }

    public void CreateVisualSignature(PDDocument template)
    {
        ArgumentNullException.ThrowIfNull(template);
        _structure.SetVisualSignature(template.GetDocument());
    }

    public void CreateWidgetDictionary(PDSignatureField signatureField, PDResources holderFormResources)
    {
        ArgumentNullException.ThrowIfNull(signatureField);
        ArgumentNullException.ThrowIfNull(holderFormResources);

        COSDictionary widgetDictionary = signatureField.GetWidgets()[0].GetCOSDictionary();
        widgetDictionary.SetNeedToBeUpdated(true);
        widgetDictionary.SetItem(COSName.GetPDFName("DR"), holderFormResources.GetCOSObject());
        _structure.SetWidgetDictionary(widgetDictionary);
    }

    public void CloseTemplate(PDDocument template)
    {
        template.Dispose();
        _structure.GetTemplate()?.Dispose();
    }

    public Stream BuildPDF(PDVisibleSignDesigner properties)
    {
        using PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);

        PDAcroForm acroForm = new(document);
        document.GetDocumentCatalog().SetAcroForm(acroForm);

        PDSignatureField signatureField = new(acroForm);
        signatureField.SetPartialName(properties.GetSignatureFieldName());

        PDRectangle rectangle = new(
            properties.GetxAxis(),
            properties.GetTemplateHeight() - properties.GetyAxis() - properties.GetHeight(),
            properties.GetWidth(),
            properties.GetHeight());

        PDAnnotationWidget widget = signatureField.GetWidgets().First();
        widget.SetRectangle(rectangle);
        page.GetAnnotations().Add(widget);

        List<PDField> fields = acroForm.GetFields().ToList();
        fields.Add(signatureField);
        acroForm.SetFields(fields);

        MemoryStream output = new();
        document.Save(output);
        output.Position = 0;

        _structure.Template = document;
        _structure.Page = page;
        _structure.AcroForm = acroForm;
        _structure.SignatureField = signatureField;
        _structure.SignatureRectangle = rectangle;
        return output;
    }
}
