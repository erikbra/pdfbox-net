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

    public bool IsSubDictionary() => _entry is COSDictionary;

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
}
