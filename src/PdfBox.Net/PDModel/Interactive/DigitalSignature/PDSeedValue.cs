using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature;

public class PDSeedValue : COSObjectable
{
    private static readonly HashSet<string> AllowedDigestNames = ["SHA1", "SHA256", "SHA384", "SHA512", "RIPEMD160"];

    public const int FLAG_FILTER = 1;
    public const int FLAG_SUBFILTER = 1 << 1;
    public const int FLAG_V = 1 << 2;
    public const int FLAG_REASON = 1 << 3;
    public const int FLAG_LEGAL_ATTESTATION = 1 << 4;
    public const int FLAG_ADD_REV_INFO = 1 << 5;
    public const int FLAG_DIGEST_METHOD = 1 << 6;

    private readonly COSDictionary _dictionary;

    public PDSeedValue()
    {
        _dictionary = new COSDictionary();
        _dictionary.SetItem(COSName.TYPE, COSName.GetPDFName("SV"));
        _dictionary.SetDirect(true);
    }

    public PDSeedValue(COSDictionary dictionary)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
        _dictionary.SetDirect(true);
    }

    public COSBase GetCOSObject() => _dictionary;

    public bool IsFilterRequired() => _dictionary.GetFlag(COSName.GetPDFName("FF"), FLAG_FILTER);
    public void SetFilterRequired(bool flag) => _dictionary.SetFlag(COSName.GetPDFName("FF"), FLAG_FILTER, flag);
    public bool IsSubFilterRequired() => _dictionary.GetFlag(COSName.GetPDFName("FF"), FLAG_SUBFILTER);
    public void SetSubFilterRequired(bool flag) => _dictionary.SetFlag(COSName.GetPDFName("FF"), FLAG_SUBFILTER, flag);
    public bool IsDigestMethodRequired() => _dictionary.GetFlag(COSName.GetPDFName("FF"), FLAG_DIGEST_METHOD);
    public void SetDigestMethodRequired(bool flag) => _dictionary.SetFlag(COSName.GetPDFName("FF"), FLAG_DIGEST_METHOD, flag);
    public bool IsVRequired() => _dictionary.GetFlag(COSName.GetPDFName("FF"), FLAG_V);
    public void SetVRequired(bool flag) => _dictionary.SetFlag(COSName.GetPDFName("FF"), FLAG_V, flag);
    public bool IsReasonRequired() => _dictionary.GetFlag(COSName.GetPDFName("FF"), FLAG_REASON);
    public void SetReasonRequired(bool flag) => _dictionary.SetFlag(COSName.GetPDFName("FF"), FLAG_REASON, flag);
    public bool IsLegalAttestationRequired() => _dictionary.GetFlag(COSName.GetPDFName("FF"), FLAG_LEGAL_ATTESTATION);
    public void SetLegalAttestationRequired(bool flag) => _dictionary.SetFlag(COSName.GetPDFName("FF"), FLAG_LEGAL_ATTESTATION, flag);
    public bool IsAddRevInfoRequired() => _dictionary.GetFlag(COSName.GetPDFName("FF"), FLAG_ADD_REV_INFO);
    public void SetAddRevInfoRequired(bool flag) => _dictionary.SetFlag(COSName.GetPDFName("FF"), FLAG_ADD_REV_INFO, flag);

    public string? GetFilter() => _dictionary.GetNameAsString(COSName.FILTER);
    public void SetFilter(COSName filter) => _dictionary.SetItem(COSName.FILTER, filter);

    public List<string> GetSubFilter() => _dictionary.GetCOSArray(COSName.GetPDFName("SubFilter"))?.ToCOSNameStringList() ?? [];
    public void SetSubFilter(List<string> values) => _dictionary.SetItem(COSName.GetPDFName("SubFilter"), COSArray.OfCOSNames(values));

    public List<string> GetDigestMethod() => _dictionary.GetCOSArray(COSName.GetPDFName("DigestMethod"))?.ToCOSNameStringList() ?? [];

    public void SetDigestMethod(List<string> digestMethod)
    {
        foreach (string digestName in digestMethod)
        {
            if (!AllowedDigestNames.Contains(digestName))
            {
                throw new ArgumentException($"Specified digest {digestName} isn't allowed.", nameof(digestMethod));
            }
        }

        _dictionary.SetItem(COSName.GetPDFName("DigestMethod"), COSArray.OfCOSNames(digestMethod));
    }

    public float GetV() => _dictionary.GetFloat(COSName.V);
    public void SetV(float value) => _dictionary.SetFloat(COSName.V, value);

    public List<string> GetReasons() => _dictionary.GetCOSArray(COSName.GetPDFName("Reasons"))?.ToCOSStringStringList() ?? [];
    public void SetReasons(List<string> reasons) => _dictionary.SetItem(COSName.GetPDFName("Reasons"), COSArray.OfCOSStrings(reasons));

    public PDSeedValueMDP? GetMDP() => _dictionary.GetCOSDictionary(COSName.GetPDFName("MDP")) is COSDictionary d ? new PDSeedValueMDP(d) : null;
    public void SetMPD(PDSeedValueMDP? value) => _dictionary.SetItem(COSName.GetPDFName("MDP"), value?.GetCOSObject());

    public PDSeedValueCertificate? GetSeedValueCertificate() => _dictionary.GetCOSDictionary(COSName.GetPDFName("Cert")) is COSDictionary d ? new PDSeedValueCertificate(d) : null;
    public void SetSeedValueCertificate(PDSeedValueCertificate? value) => _dictionary.SetItem(COSName.GetPDFName("Cert"), value);

    public PDSeedValueTimeStamp? GetTimeStamp() => _dictionary.GetCOSDictionary(COSName.GetPDFName("TimeStamp")) is COSDictionary d ? new PDSeedValueTimeStamp(d) : null;
    public void SetTimeStamp(PDSeedValueTimeStamp? value) => _dictionary.SetItem(COSName.GetPDFName("TimeStamp"), value?.GetCOSObject());

    public List<string> GetLegalAttestation() => _dictionary.GetCOSArray(COSName.GetPDFName("LegalAttestation"))?.ToCOSStringStringList() ?? [];
    public void SetLegalAttestation(List<string> legalAttestation) => _dictionary.SetItem(COSName.GetPDFName("LegalAttestation"), COSArray.OfCOSStrings(legalAttestation));
}
