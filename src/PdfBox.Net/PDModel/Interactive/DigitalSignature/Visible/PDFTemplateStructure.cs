/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/digitalsignature/visible/PDFTemplateStructure.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
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

public class PDFTemplateStructure
{
    public PDDocument? Template { get; set; }
    public PDPage? Page { get; set; }
    public PDAcroForm? AcroForm { get; set; }
    public PDSignatureField? SignatureField { get; set; }
    public PDSignature? Signature { get; set; }
    public COSDictionary? AcroFormDictionary { get; set; }
    public PDRectangle? SignatureRectangle { get; set; }
    public AffineTransform? AffineTransform { get; set; }
    public COSArray? ProcSet { get; set; }
    public PDImageXObject? Image { get; set; }
    public PDRectangle? FormatterRectangle { get; set; }
    public PDStream? HolderFormStream { get; set; }
    public PDResources? HolderFormResources { get; set; }
    public PDFormXObject? HolderForm { get; set; }
    public PDAppearanceDictionary? AppearanceDictionary { get; set; }
    public PDStream? InnerFormStream { get; set; }
    public PDResources? InnerFormResources { get; set; }
    public PDFormXObject? InnerForm { get; set; }
    public PDStream? ImageFormStream { get; set; }
    public PDResources? ImageFormResources { get; set; }
    public IList<PDField>? AcroFormFields { get; set; }
    public COSName? InnerFormName { get; set; }
    public COSName? ImageFormName { get; set; }
    public COSName? ImageName { get; set; }
    public COSDocument? VisualSignature { get; set; }
    public PDFormXObject? ImageForm { get; set; }
    public COSDictionary? WidgetDictionary { get; set; }

    public PDPage? GetPage() => Page;
    public void SetPage(PDPage? page) => Page = page;
    public PDDocument? GetTemplate() => Template;
    public void SetTemplate(PDDocument? template) => Template = template;
    public PDAcroForm? GetAcroForm() => AcroForm;
    public void SetAcroForm(PDAcroForm? acroForm) => AcroForm = acroForm;
    public PDSignatureField? GetSignatureField() => SignatureField;
    public void SetSignatureField(PDSignatureField? signatureField) => SignatureField = signatureField;
    public PDSignature? GetPdSignature() => Signature;
    public void SetPdSignature(PDSignature? pdSignature) => Signature = pdSignature;
    public COSDictionary? GetAcroFormDictionary() => AcroFormDictionary;
    public void SetAcroFormDictionary(COSDictionary? acroFormDictionary) => AcroFormDictionary = acroFormDictionary;
    public PDRectangle? GetSignatureRectangle() => SignatureRectangle;
    public void SetSignatureRectangle(PDRectangle? signatureRectangle) => SignatureRectangle = signatureRectangle;
    public AffineTransform? GetAffineTransform() => AffineTransform;
    public void SetAffineTransform(AffineTransform? affineTransform) => AffineTransform = affineTransform;
    public COSArray? GetProcSet() => ProcSet;
    public void SetProcSet(COSArray? procSet) => ProcSet = procSet;
    public PDImageXObject? GetImage() => Image;
    public void SetImage(PDImageXObject? image) => Image = image;
    public PDRectangle? GetFormatterRectangle() => FormatterRectangle;
    public void SetFormatterRectangle(PDRectangle? formatterRectangle) => FormatterRectangle = formatterRectangle;
    public PDStream? GetHolderFormStream() => HolderFormStream;
    public void SetHolderFormStream(PDStream? holderFormStream) => HolderFormStream = holderFormStream;
    public PDResources? GetHolderFormResources() => HolderFormResources;
    public void SetHolderFormResources(PDResources? holderFormResources) => HolderFormResources = holderFormResources;
    public PDFormXObject? GetHolderForm() => HolderForm;
    public void SetHolderForm(PDFormXObject? holderForm) => HolderForm = holderForm;
    public PDAppearanceDictionary? GetAppearanceDictionary() => AppearanceDictionary;
    public void SetAppearanceDictionary(PDAppearanceDictionary? appearanceDictionary) => AppearanceDictionary = appearanceDictionary;
    public PDStream? GetInnerFormStream() => InnerFormStream;
    public void SetInnterFormStream(PDStream? innerFormStream) => InnerFormStream = innerFormStream;
    public void SetInnerFormStream(PDStream? innerFormStream) => InnerFormStream = innerFormStream;
    public PDResources? GetInnerFormResources() => InnerFormResources;
    public void SetInnerFormResources(PDResources? innerFormResources) => InnerFormResources = innerFormResources;
    public PDFormXObject? GetInnerForm() => InnerForm;
    public void SetInnerForm(PDFormXObject? innerForm) => InnerForm = innerForm;
    public COSName? GetInnerFormName() => InnerFormName;
    public void SetInnerFormName(COSName? innerFormName) => InnerFormName = innerFormName;
    public PDStream? GetImageFormStream() => ImageFormStream;
    public void SetImageFormStream(PDStream? imageFormStream) => ImageFormStream = imageFormStream;
    public PDResources? GetImageFormResources() => ImageFormResources;
    public void SetImageFormResources(PDResources? imageFormResources) => ImageFormResources = imageFormResources;
    public PDFormXObject? GetImageForm() => ImageForm;
    public void SetImageForm(PDFormXObject? imageForm) => ImageForm = imageForm;
    public COSName? GetImageFormName() => ImageFormName;
    public void SetImageFormName(COSName? imageFormName) => ImageFormName = imageFormName;
    public COSName? GetImageName() => ImageName;
    public void SetImageName(COSName? imageName) => ImageName = imageName;
    public COSDocument? GetVisualSignature() => VisualSignature;
    public void SetVisualSignature(COSDocument? visualSignature) => VisualSignature = visualSignature;
    public IList<PDField>? GetAcroFormFields() => AcroFormFields;
    public void SetAcroFormFields(IList<PDField>? acroFormFields) => AcroFormFields = acroFormFields;
    public COSDictionary? GetWidgetDictionary() => WidgetDictionary;
    public void SetWidgetDictionary(COSDictionary? widgetDictionary) => WidgetDictionary = widgetDictionary;
}
