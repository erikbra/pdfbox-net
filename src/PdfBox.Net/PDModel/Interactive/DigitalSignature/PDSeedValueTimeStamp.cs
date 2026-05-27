using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature;

public class PDSeedValueTimeStamp
{
    private readonly COSDictionary _dictionary;

    public PDSeedValueTimeStamp()
    {
        _dictionary = new COSDictionary();
        _dictionary.SetDirect(true);
    }

    public PDSeedValueTimeStamp(COSDictionary dictionary)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
        _dictionary.SetDirect(true);
    }

    public COSDictionary GetCOSObject() => _dictionary;

    public string GetURL() => _dictionary.GetString(COSName.GetPDFName("URL"), string.Empty);
    public void SetURL(string? url) => _dictionary.SetString(COSName.GetPDFName("URL"), url);

    public bool IsTimestampRequired() => _dictionary.GetInt(COSName.GetPDFName("FF"), 0) != 0;
    public void SetTimestampRequired(bool flag) => _dictionary.SetInt(COSName.GetPDFName("FF"), flag ? 1 : 0);
}
