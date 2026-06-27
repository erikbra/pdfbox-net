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

public partial class PDFTemplateStructure
{
    private PDDocument? _template;
private PDPage? _page;
private PDAcroForm? _acroForm;
private PDSignatureField? _signatureField;
public PDSignature? Signature { get; set; }
    private COSDictionary? _acroFormDictionary;
private PDRectangle? _signatureRectangle;
private AffineTransform? _affineTransform;
private COSArray? _procSet;
private PDImageXObject? _image;
private PDRectangle? _formatterRectangle;
private PDStream? _holderFormStream;
private PDResources? _holderFormResources;
private PDFormXObject? _holderForm;
private PDAppearanceDictionary? _appearanceDictionary;
private PDStream? _innerFormStream;
private PDResources? _innerFormResources;
private PDFormXObject? _innerForm;
private PDStream? _imageFormStream;
private PDResources? _imageFormResources;
private IList<PDField>? _acroFormFields;
private COSName? _innerFormName;
private COSName? _imageFormName;
private COSName? _imageName;
private COSDocument? _visualSignature;
private PDFormXObject? _imageForm;
private COSDictionary? _widgetDictionary;
public PDPage? GetPage() => _page;
    public void SetPage(PDPage? page) => _page = page;
    public PDDocument? GetTemplate() => _template;
    public void SetTemplate(PDDocument? template) => _template = template;
    public PDAcroForm? GetAcroForm() => _acroForm;
    public void SetAcroForm(PDAcroForm? acroForm) => _acroForm = acroForm;
    public PDSignatureField? GetSignatureField() => _signatureField;
    public void SetSignatureField(PDSignatureField? signatureField) => _signatureField = signatureField;
    public PDSignature? GetPdSignature() => Signature;
    public void SetPdSignature(PDSignature? pdSignature) => Signature = pdSignature;
    public COSDictionary? GetAcroFormDictionary() => _acroFormDictionary;
    public void SetAcroFormDictionary(COSDictionary? acroFormDictionary) => _acroFormDictionary = acroFormDictionary;
    public PDRectangle? GetSignatureRectangle() => _signatureRectangle;
    public void SetSignatureRectangle(PDRectangle? signatureRectangle) => _signatureRectangle = signatureRectangle;
    public AffineTransform? GetAffineTransform() => _affineTransform;
    public void SetAffineTransform(AffineTransform? affineTransform) => _affineTransform = affineTransform;
    public COSArray? GetProcSet() => _procSet;
    public void SetProcSet(COSArray? procSet) => _procSet = procSet;
    public PDImageXObject? GetImage() => _image;
    public void SetImage(PDImageXObject? image) => _image = image;
    public PDRectangle? GetFormatterRectangle() => _formatterRectangle;
    public void SetFormatterRectangle(PDRectangle? formatterRectangle) => _formatterRectangle = formatterRectangle;
    public PDStream? GetHolderFormStream() => _holderFormStream;
    public void SetHolderFormStream(PDStream? holderFormStream) => _holderFormStream = holderFormStream;
    public PDResources? GetHolderFormResources() => _holderFormResources;
    public void SetHolderFormResources(PDResources? holderFormResources) => _holderFormResources = holderFormResources;
    public PDFormXObject? GetHolderForm() => _holderForm;
    public void SetHolderForm(PDFormXObject? holderForm) => _holderForm = holderForm;
    public PDAppearanceDictionary? GetAppearanceDictionary() => _appearanceDictionary;
    public void SetAppearanceDictionary(PDAppearanceDictionary? appearanceDictionary) => _appearanceDictionary = appearanceDictionary;
    public PDStream? GetInnerFormStream() => _innerFormStream;
    public void SetInnterFormStream(PDStream? innerFormStream) => InnerFormStream = innerFormStream;
    public void SetInnerFormStream(PDStream? innerFormStream) => _innerFormStream = innerFormStream;
    public PDResources? GetInnerFormResources() => _innerFormResources;
    public void SetInnerFormResources(PDResources? innerFormResources) => _innerFormResources = innerFormResources;
    public PDFormXObject? GetInnerForm() => _innerForm;
    public void SetInnerForm(PDFormXObject? innerForm) => _innerForm = innerForm;
    public COSName? GetInnerFormName() => _innerFormName;
    public void SetInnerFormName(COSName? innerFormName) => _innerFormName = innerFormName;
    public PDStream? GetImageFormStream() => _imageFormStream;
    public void SetImageFormStream(PDStream? imageFormStream) => _imageFormStream = imageFormStream;
    public PDResources? GetImageFormResources() => _imageFormResources;
    public void SetImageFormResources(PDResources? imageFormResources) => _imageFormResources = imageFormResources;
    public PDFormXObject? GetImageForm() => _imageForm;
    public void SetImageForm(PDFormXObject? imageForm) => _imageForm = imageForm;
    public COSName? GetImageFormName() => _imageFormName;
    public void SetImageFormName(COSName? imageFormName) => _imageFormName = imageFormName;
    public COSName? GetImageName() => _imageName;
    public void SetImageName(COSName? imageName) => _imageName = imageName;
    public COSDocument? GetVisualSignature() => _visualSignature;
    public void SetVisualSignature(COSDocument? visualSignature) => _visualSignature = visualSignature;
    public IList<PDField>? GetAcroFormFields() => _acroFormFields;
    public void SetAcroFormFields(IList<PDField>? acroFormFields) => _acroFormFields = acroFormFields;
    public COSDictionary? GetWidgetDictionary() => _widgetDictionary;
    public void SetWidgetDictionary(COSDictionary? widgetDictionary) => _widgetDictionary = widgetDictionary;
}
