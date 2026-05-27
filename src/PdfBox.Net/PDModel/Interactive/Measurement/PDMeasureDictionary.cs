using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Interactive.Measurement;

public class PDMeasureDictionary : COSObjectable
{
    public const string TYPE = "Measure";

    private readonly COSDictionary _dictionary;

    protected PDMeasureDictionary()
    {
        _dictionary = new COSDictionary();
        _dictionary.SetName(COSName.TYPE, TYPE);
    }

    public PDMeasureDictionary(COSDictionary dictionary)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    public COSBase GetCOSObject() => _dictionary;

    protected COSDictionary Dictionary => _dictionary;

    public string GetMeasureType() => TYPE;

    public string GetSubtype() => _dictionary.GetNameAsString(COSName.SUBTYPE, PDRectlinearMeasureDictionary.SUBTYPE) ?? PDRectlinearMeasureDictionary.SUBTYPE;

    protected void SetSubtype(string subtype) => _dictionary.SetName(COSName.SUBTYPE, subtype);
}
