using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature;

public class PDPropBuildDataDict : COSObjectable
{
    private readonly COSDictionary _dictionary;

    public PDPropBuildDataDict()
    {
        _dictionary = new COSDictionary();
        _dictionary.SetDirect(true);
    }

    public PDPropBuildDataDict(COSDictionary dictionary)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
        _dictionary.SetDirect(true);
    }

    public COSBase GetCOSObject() => _dictionary;

    public string? GetName() => _dictionary.GetNameAsString(COSName.NAME);
    public void SetName(string name) => _dictionary.SetName(COSName.NAME, name);

    public string? GetDate() => _dictionary.GetString(COSName.GetPDFName("Date"));
    public void SetDate(string? date) => _dictionary.SetString(COSName.GetPDFName("Date"), date);

    public string? GetVersion() => _dictionary.GetString("REx");
    public void SetVersion(string? version) => _dictionary.SetString("REx", version);

    public long GetRevision() => _dictionary.GetLong(COSName.GetPDFName("R"));
    public void SetRevision(long revision) => _dictionary.SetLong(COSName.GetPDFName("R"), revision);

    public long GetMinimumRevision() => _dictionary.GetLong(COSName.V);
    public void SetMinimumRevision(long revision) => _dictionary.SetLong(COSName.V, revision);

    public bool GetPreRelease() => _dictionary.GetBoolean(COSName.GetPDFName("PreRelease"), false);
    public void SetPreRelease(bool preRelease) => _dictionary.SetBoolean(COSName.GetPDFName("PreRelease"), preRelease);

    public string? GetOS()
    {
        COSName osName = COSName.GetPDFName("OS");
        COSArray? osArray = _dictionary.GetCOSArray(osName);
        return osArray != null ? osArray.GetName(0) : _dictionary.GetString(osName);
    }

    public void SetOS(string? os)
    {
        if (os is null)
        {
            _dictionary.RemoveItem(COSName.GetPDFName("OS"));
            return;
        }

        COSName osName = COSName.GetPDFName("OS");
        COSArray osArray = _dictionary.GetCOSArray(osName) ?? new COSArray();
        osArray.SetDirect(true);
        if (osArray.Size() == 0)
        {
            osArray.Add(COSName.GetPDFName(os));
        }
        else
        {
            osArray.Set(0, COSName.GetPDFName(os));
        }

        _dictionary.SetItem(osName, osArray);
    }

    public bool GetNonEFontNoWarn() => _dictionary.GetBoolean(COSName.GetPDFName("NonEFontNoWarn"), true);
    public void SetNonEFontNoWarn(bool noEmbedFontWarning) => _dictionary.SetBoolean(COSName.GetPDFName("NonEFontNoWarn"), noEmbedFontWarning);

    public bool GetTrustedMode() => _dictionary.GetBoolean(COSName.GetPDFName("TrustedMode"), false);
    public void SetTrustedMode(bool trustedMode) => _dictionary.SetBoolean(COSName.GetPDFName("TrustedMode"), trustedMode);
}
