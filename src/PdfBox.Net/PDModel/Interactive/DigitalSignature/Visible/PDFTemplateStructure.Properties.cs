/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/digitalsignature/visible/PDFTemplateStructure.java
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.DigitalSignature;
using PdfBox.Net.PDModel.Interactive.Form;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature.Visible;

public partial class PDFTemplateStructure
{
    public PDAcroForm? AcroForm
    {
        get => GetAcroForm();
        set => SetAcroForm(value!);
    }

    public COSDictionary? AcroFormDictionary
    {
        get => GetAcroFormDictionary();
        set => SetAcroFormDictionary(value!);
    }

    public IList<PDField>? AcroFormFields
    {
        get => GetAcroFormFields();
        set => SetAcroFormFields(value!);
    }

    public AffineTransform? AffineTransform
    {
        get => GetAffineTransform();
        set => SetAffineTransform(value!);
    }

    public PDAppearanceDictionary? AppearanceDictionary
    {
        get => GetAppearanceDictionary();
        set => SetAppearanceDictionary(value!);
    }

    public PDRectangle? FormatterRectangle
    {
        get => GetFormatterRectangle();
        set => SetFormatterRectangle(value!);
    }

    public PDFormXObject? HolderForm
    {
        get => GetHolderForm();
        set => SetHolderForm(value!);
    }

    public PDResources? HolderFormResources
    {
        get => GetHolderFormResources();
        set => SetHolderFormResources(value!);
    }

    public PDStream? HolderFormStream
    {
        get => GetHolderFormStream();
        set => SetHolderFormStream(value!);
    }

    public PDImageXObject? Image
    {
        get => GetImage();
        set => SetImage(value!);
    }

    public PDFormXObject? ImageForm
    {
        get => GetImageForm();
        set => SetImageForm(value!);
    }

    public COSName? ImageFormName
    {
        get => GetImageFormName();
        set => SetImageFormName(value!);
    }

    public PDResources? ImageFormResources
    {
        get => GetImageFormResources();
        set => SetImageFormResources(value!);
    }

    public PDStream? ImageFormStream
    {
        get => GetImageFormStream();
        set => SetImageFormStream(value!);
    }

    public COSName? ImageName
    {
        get => GetImageName();
        set => SetImageName(value!);
    }

    public PDFormXObject? InnerForm
    {
        get => GetInnerForm();
        set => SetInnerForm(value!);
    }

    public COSName? InnerFormName
    {
        get => GetInnerFormName();
        set => SetInnerFormName(value!);
    }

    public PDResources? InnerFormResources
    {
        get => GetInnerFormResources();
        set => SetInnerFormResources(value!);
    }

    public PDStream? InnerFormStream
    {
        get => GetInnerFormStream();
        set => SetInnerFormStream(value!);
    }

    public PDPage? Page
    {
        get => GetPage();
        set => SetPage(value!);
    }

    public PDSignature? PdSignature
    {
        get => GetPdSignature();
        set => SetPdSignature(value!);
    }

    public COSArray? ProcSet
    {
        get => GetProcSet();
        set => SetProcSet(value!);
    }

    public PDSignatureField? SignatureField
    {
        get => GetSignatureField();
        set => SetSignatureField(value!);
    }

    public PDRectangle? SignatureRectangle
    {
        get => GetSignatureRectangle();
        set => SetSignatureRectangle(value!);
    }

    public PDDocument? Template
    {
        get => GetTemplate();
        set => SetTemplate(value!);
    }

    public COSDocument? VisualSignature
    {
        get => GetVisualSignature();
        set => SetVisualSignature(value!);
    }

    public COSDictionary? WidgetDictionary
    {
        get => GetWidgetDictionary();
        set => SetWidgetDictionary(value!);
    }
}
