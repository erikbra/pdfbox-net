using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed class PDAppearanceDictionary : COSObjectable
{
    private static readonly COSName NormalName = COSName.N;
    private static readonly COSName RolloverName = COSName.GetPDFName("R");
    private static readonly COSName DownName = COSName.D;

    private readonly COSDictionary _dictionary;

    public PDAppearanceDictionary()
        : this(new COSDictionary())
    {
    }

    public PDAppearanceDictionary(COSDictionary dictionary)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    public COSBase GetCOSObject() => _dictionary;

    public PDAppearanceEntry? GetNormalAppearance()
    {
        return _dictionary.GetDictionaryObject(NormalName) is COSBase entry ? new PDAppearanceEntry(entry) : null;
    }

    public void SetNormalAppearance(PDAppearanceEntry appearance) => _dictionary.SetItem(NormalName, appearance);

    public void SetNormalAppearance(PDAppearanceStream appearance) => _dictionary.SetItem(NormalName, appearance);

    public PDAppearanceEntry? GetRolloverAppearance()
    {
        return _dictionary.GetDictionaryObject(RolloverName) is COSBase entry ? new PDAppearanceEntry(entry) : null;
    }

    public void SetRolloverAppearance(PDAppearanceEntry appearance) => _dictionary.SetItem(RolloverName, appearance);

    public PDAppearanceEntry? GetDownAppearance()
    {
        return _dictionary.GetDictionaryObject(DownName) is COSBase entry ? new PDAppearanceEntry(entry) : null;
    }

    public void SetDownAppearance(PDAppearanceEntry appearance) => _dictionary.SetItem(DownName, appearance);
}
