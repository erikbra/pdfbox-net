/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/digitalsignature/visible/PDFTemplateBuilder.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.PDModel.Interactive.Form;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature.Visible;

public interface PDFTemplateBuilder
{
    void CreateAffineTransform(AffineTransform affineTransform);
    void CreatePage(PDVisibleSignDesigner properties);
    void CreateTemplate(PDPage page);
    void CreateAcroForm(PDDocument template);
    void CreateSignatureField(PDAcroForm acroForm);
    void CreateSignature(PDSignatureField pdSignatureField, PDPage page, string signerName);
    void CreateAcroFormDictionary(PDAcroForm acroForm, PDSignatureField signatureField);
    void CreateSignatureRectangle(PDSignatureField signatureField, PDVisibleSignDesigner properties);
    void CreateProcSetArray();
    void CreateSignatureImage(PDDocument template, PDImageXObject image);
    void CreateFormatterRectangle(int[] parameters);
    void CreateHolderFormStream(PDDocument template);
    void CreateHolderFormResources();
    void CreateHolderForm(PDResources holderFormResources, PDStream holderFormStream, PDRectangle bbox);
    void CreateAppearanceDictionary(PDFormXObject holderForm, PDSignatureField signatureField);
    void CreateInnerFormStream(PDDocument template);
    void CreateInnerFormResource();
    void CreateInnerForm(PDResources innerFormResources, PDStream innerFormStream, PDRectangle bbox);
    void InsertInnerFormToHolderResources(PDFormXObject innerForm, PDResources holderFormResources);
    void CreateImageFormStream(PDDocument template);
    void CreateImageFormResources();
    void CreateImageForm(PDResources imageFormResources, PDResources innerFormResource, PDStream imageFormStream, PDRectangle bbox, AffineTransform affineTransform, PDImageXObject img);
    void CreateBackgroundLayerForm(PDResources innerFormResource, PDRectangle bbox);
    void InjectProcSetArray(PDFormXObject innerForm, PDPage page, PDResources innerFormResources, PDResources imageFormResources, PDResources holderFormResources, COSArray procSet);
    void InjectAppearanceStreams(PDStream holderFormStream, PDStream innerFormStream, PDStream imageFormStream, COSName imageFormName, COSName imageName, COSName innerFormName, PDVisibleSignDesigner properties);
    void CreateVisualSignature(PDDocument template);
    void CreateWidgetDictionary(PDSignatureField signatureField, PDResources holderFormResources);
    PDFTemplateStructure GetStructure();
    void CloseTemplate(PDDocument template);
    Stream BuildPDF(PDVisibleSignDesigner properties);
}
