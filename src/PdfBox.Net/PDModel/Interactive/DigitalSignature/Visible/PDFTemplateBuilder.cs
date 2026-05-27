namespace PdfBox.Net.PDModel.Interactive.DigitalSignature.Visible;

public interface PDFTemplateBuilder
{
    PDFTemplateStructure GetStructure();
    Stream BuildPDF(PDVisibleSignDesigner properties);
}
