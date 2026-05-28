using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed class PDAppearanceEntry : COSObjectable
{
    private readonly COSBase _entry;

    public PDAppearanceEntry(COSBase entry)
    {
        _entry = entry ?? throw new ArgumentNullException(nameof(entry));
    }

    public COSBase GetCOSObject() => _entry;

    public bool IsStream() => _entry is COSStream;

    public bool IsSubDictionary() => _entry is COSDictionary and not COSStream;

    public PDAppearanceStream GetAppearanceStream()
    {
        return _entry switch
        {
            COSStream stream => new PDAppearanceStream(stream),
            COSDictionary dictionary when dictionary.GetCOSStream(COSName.GetPDFName("Default")) is COSStream stream
                => new PDAppearanceStream(stream),
            _ => throw new IOException("Appearance entry is not a stream.")
        };
    }

    public IDictionary<COSName, PDAppearanceStream> GetSubDictionary()
    {
        if (_entry is not COSDictionary dictionary)
        {
            throw new InvalidOperationException("Appearance entry does not contain a sub-dictionary.");
        }

        Dictionary<COSName, PDAppearanceStream> result = new();
        foreach (COSName key in dictionary.KeySet())
        {
            if (dictionary.GetDictionaryObject(key) is COSStream stream)
            {
                result[key] = new PDAppearanceStream(stream);
            }
        }

        return result;
    }
}
