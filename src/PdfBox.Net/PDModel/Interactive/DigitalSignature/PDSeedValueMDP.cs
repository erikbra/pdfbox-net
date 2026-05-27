using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature;

public class PDSeedValueMDP
{
    private readonly COSDictionary _dictionary;

    public PDSeedValueMDP()
    {
        _dictionary = new COSDictionary();
        _dictionary.SetDirect(true);
    }

    public PDSeedValueMDP(COSDictionary dictionary)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
        _dictionary.SetDirect(true);
    }

    public COSDictionary GetCOSObject() => _dictionary;

    public int GetP() => _dictionary.GetInt(COSName.P);

    public void SetP(int p)
    {
        if (p < 0 || p > 3)
        {
            throw new ArgumentException("Only values between 0 and 3 are allowed.", nameof(p));
        }

        _dictionary.SetInt(COSName.P, p);
    }
}
