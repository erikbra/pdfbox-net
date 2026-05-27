namespace PdfBox.Net.PDModel.Interactive.DigitalSignature.Visible;

public class PDFTemplateCreator
{
    private readonly PDFTemplateBuilder _pdfBuilder;

    public PDFTemplateCreator(PDFTemplateBuilder templateBuilder)
    {
        _pdfBuilder = templateBuilder;
    }

    public PDFTemplateStructure GetPdfStructure() => _pdfBuilder.GetStructure();

    public Stream BuildPDF(PDVisibleSignDesigner properties)
    {
        return _pdfBuilder.BuildPDF(properties);
    }
}
