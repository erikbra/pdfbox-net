namespace PdfBox.Net.PDModel.Interactive.DigitalSignature;

public interface SignatureInterface
{
    byte[] Sign(Stream content);
}
