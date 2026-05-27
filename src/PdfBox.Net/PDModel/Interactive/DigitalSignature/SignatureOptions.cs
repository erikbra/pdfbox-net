using PdfBox.Net.COS;
using PdfBox.Net.PdfParser;
using PdfBox.Net.PDModel.Interactive.DigitalSignature.Visible;

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature;

public sealed class SignatureOptions : IDisposable
{
    public const int DEFAULT_SIGNATURE_SIZE = 0x2500;

    private COSDocument? _visualSignature;
    private int _preferredSignatureSize;
    private int _pageNo;

    public void SetPage(int pageNo) => _pageNo = pageNo;

    public int GetPage() => _pageNo;

    public void SetVisualSignature(FileInfo file)
    {
        ArgumentNullException.ThrowIfNull(file);
        using Stream input = file.OpenRead();
        SetVisualSignature(input);
    }

    public void SetVisualSignature(Stream input)
    {
        ArgumentNullException.ThrowIfNull(input);
        PDFParser parser = new(input);
        _visualSignature = parser.Parse().Document;
    }

    public void SetVisualSignature(PDVisibleSigProperties visSignatureProperties)
    {
        ArgumentNullException.ThrowIfNull(visSignatureProperties);
        using Stream input = visSignatureProperties.GetVisibleSignature();
        SetVisualSignature(input);
    }

    public COSDocument? GetVisualSignature() => _visualSignature;

    public int GetPreferredSignatureSize() => _preferredSignatureSize;

    public void SetPreferredSignatureSize(int size)
    {
        if (size > 0)
        {
            _preferredSignatureSize = size;
        }
    }

    public void Dispose()
    {
        _visualSignature?.Dispose();
    }
}
