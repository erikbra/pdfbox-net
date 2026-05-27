namespace PdfBox.Net.PDModel.Interactive.DigitalSignature;

public sealed class SigningSupport : ExternalSigningSupport, IDisposable
{
    private readonly Func<Stream> _contentFactory;
    private readonly Action<byte[]> _signatureWriter;

    public SigningSupport(Func<Stream> contentFactory, Action<byte[]> signatureWriter)
    {
        _contentFactory = contentFactory ?? throw new ArgumentNullException(nameof(contentFactory));
        _signatureWriter = signatureWriter ?? throw new ArgumentNullException(nameof(signatureWriter));
    }

    public Stream GetContent() => _contentFactory();

    public void SetSignature(byte[] signature)
    {
        ArgumentNullException.ThrowIfNull(signature);
        _signatureWriter(signature);
    }

    public void Dispose()
    {
    }
}
