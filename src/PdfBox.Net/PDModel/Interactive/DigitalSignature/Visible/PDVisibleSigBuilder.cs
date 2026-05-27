using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.Form;

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature.Visible;

public class PDVisibleSigBuilder : PDFTemplateBuilder
{
    private readonly PDFTemplateStructure _structure = new();

    public PDFTemplateStructure GetStructure() => _structure;

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
