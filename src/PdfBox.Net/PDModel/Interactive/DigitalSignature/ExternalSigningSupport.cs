namespace PdfBox.Net.PDModel.Interactive.DigitalSignature;

public interface ExternalSigningSupport
{
    Stream GetContent();
    void SetSignature(byte[] signature);
}
