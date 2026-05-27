using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature;

public class PDPropBuild : COSObjectable
{
    private readonly COSDictionary _dictionary;

    public PDPropBuild()
    {
        _dictionary = new COSDictionary();
        _dictionary.SetDirect(true);
    }

    public PDPropBuild(COSDictionary dictionary)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
        _dictionary.SetDirect(true);
    }

    public COSBase GetCOSObject() => _dictionary;

    public PDPropBuildDataDict? GetFilter()
    {
        COSDictionary? filter = _dictionary.GetCOSDictionary(COSName.FILTER);
        return filter != null ? new PDPropBuildDataDict(filter) : null;
    }

    public void SetPDPropBuildFilter(PDPropBuildDataDict? filter) => _dictionary.SetItem(COSName.FILTER, filter);

    public PDPropBuildDataDict? GetPubSec()
    {
        COSDictionary? value = _dictionary.GetCOSDictionary(COSName.GetPDFName("PubSec"));
        return value != null ? new PDPropBuildDataDict(value) : null;
    }

    public void SetPDPropBuildPubSec(PDPropBuildDataDict? pubSec) => _dictionary.SetItem(COSName.GetPDFName("PubSec"), pubSec);

    public PDPropBuildDataDict? GetApp()
    {
        COSDictionary? app = _dictionary.GetCOSDictionary(COSName.GetPDFName("App"));
        return app != null ? new PDPropBuildDataDict(app) : null;
    }

    public void SetPDPropBuildApp(PDPropBuildDataDict? app) => _dictionary.SetItem(COSName.GetPDFName("App"), app);
}
