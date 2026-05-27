using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Interactive.Form;
using PdfBox.Net.PDModel.Interactive.DigitalSignature;

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature.Visible;

public class PDFTemplateStructure
{
    public PDDocument? Template { get; set; }
    public PDPage? Page { get; set; }
    public PDAcroForm? AcroForm { get; set; }
    public PDSignatureField? SignatureField { get; set; }
    public PDSignature? Signature { get; set; }
    public PDRectangle? SignatureRectangle { get; set; }
    public COSDocument? VisualSignature { get; set; }
}
