using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature;

public class PDSeedValueCertificate : COSObjectable
{
    public const int FLAG_SUBJECT = 1;
    public const int FLAG_ISSUER = 1 << 1;
    public const int FLAG_OID = 1 << 2;
    public const int FLAG_SUBJECT_DN = 1 << 3;
    public const int FLAG_KEY_USAGE = 1 << 5;
    public const int FLAG_URL = 1 << 6;

    private readonly COSDictionary _dictionary;

    public PDSeedValueCertificate()
    {
        _dictionary = new COSDictionary();
        _dictionary.SetItem(COSName.TYPE, COSName.GetPDFName("SVCert"));
        _dictionary.SetDirect(true);
    }

    public PDSeedValueCertificate(COSDictionary dictionary)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
        _dictionary.SetDirect(true);
    }

    public COSBase GetCOSObject() => _dictionary;

    public bool IsSubjectRequired() => _dictionary.GetFlag(COSName.GetPDFName("FF"), FLAG_SUBJECT);
    public void SetSubjectRequired(bool flag) => _dictionary.SetFlag(COSName.GetPDFName("FF"), FLAG_SUBJECT, flag);
    public bool IsIssuerRequired() => _dictionary.GetFlag(COSName.GetPDFName("FF"), FLAG_ISSUER);
    public void SetIssuerRequired(bool flag) => _dictionary.SetFlag(COSName.GetPDFName("FF"), FLAG_ISSUER, flag);
    public bool IsOIDRequired() => _dictionary.GetFlag(COSName.GetPDFName("FF"), FLAG_OID);
    public void SetOIDRequired(bool flag) => _dictionary.SetFlag(COSName.GetPDFName("FF"), FLAG_OID, flag);
    public bool IsSubjectDNRequired() => _dictionary.GetFlag(COSName.GetPDFName("FF"), FLAG_SUBJECT_DN);
    public void SetSubjectDNRequired(bool flag) => _dictionary.SetFlag(COSName.GetPDFName("FF"), FLAG_SUBJECT_DN, flag);
    public bool IsKeyUsageRequired() => _dictionary.GetFlag(COSName.GetPDFName("FF"), FLAG_KEY_USAGE);
    public void SetKeyUsageRequired(bool flag) => _dictionary.SetFlag(COSName.GetPDFName("FF"), FLAG_KEY_USAGE, flag);
    public bool IsURLRequired() => _dictionary.GetFlag(COSName.GetPDFName("FF"), FLAG_URL);
    public void SetURLRequired(bool flag) => _dictionary.SetFlag(COSName.GetPDFName("FF"), FLAG_URL, flag);

    public List<byte[]>? GetSubject() => GetByteArrays(COSName.SUBJECT);
    public void SetSubject(List<byte[]> subjects) => _dictionary.SetItem(COSName.SUBJECT, ToByteArrayCOS(subjects));
    public void AddSubject(byte[] subject) => AddByteArray(COSName.SUBJECT, subject);
    public void RemoveSubject(byte[] subject) => RemoveByteArray(COSName.SUBJECT, subject);

    public List<byte[]>? GetIssuer() => GetByteArrays(COSName.GetPDFName("Issuer"));
    public void SetIssuer(List<byte[]> issuers) => _dictionary.SetItem(COSName.GetPDFName("Issuer"), ToByteArrayCOS(issuers));
    public void AddIssuer(byte[] issuer) => AddByteArray(COSName.GetPDFName("Issuer"), issuer);
    public void RemoveIssuer(byte[] issuer) => RemoveByteArray(COSName.GetPDFName("Issuer"), issuer);

    public List<byte[]>? GetOID() => GetByteArrays(COSName.GetPDFName("OID"));
    public void SetOID(List<byte[]> values) => _dictionary.SetItem(COSName.GetPDFName("OID"), ToByteArrayCOS(values));
    public void AddOID(byte[] oid) => AddByteArray(COSName.GetPDFName("OID"), oid);
    public void RemoveOID(byte[] oid) => RemoveByteArray(COSName.GetPDFName("OID"), oid);

    public List<string>? GetKeyUsage() => _dictionary.GetCOSArray(COSName.GetPDFName("KeyUsage"))?.OfType<COSString>().Select(s => s.GetString()).ToList();
    public void SetKeyUsage(List<string> keyUsage) => _dictionary.SetItem(COSName.GetPDFName("KeyUsage"), COSArray.OfCOSStrings(keyUsage));

    public void AddKeyUsage(string keyUsage)
    {
        if (keyUsage.Any(c => c is not ('0' or '1' or 'X')))
        {
            throw new ArgumentException("characters can only be 0, 1, X", nameof(keyUsage));
        }

        COSArray array = _dictionary.GetCOSArray(COSName.GetPDFName("KeyUsage")) ?? new COSArray();
        array.Add(new COSString(keyUsage));
        _dictionary.SetItem(COSName.GetPDFName("KeyUsage"), array);
    }

    public void RemoveKeyUsage(string keyUsage)
    {
        COSArray? array = _dictionary.GetCOSArray(COSName.GetPDFName("KeyUsage"));
        array?.Remove(new COSString(keyUsage));
    }

    public List<Dictionary<string, string>>? GetSubjectDN()
    {
        COSArray? array = _dictionary.GetCOSArray(COSName.GetPDFName("SubjectDN"));
        if (array == null)
        {
            return null;
        }

        List<Dictionary<string, string>> result = [];
        foreach (COSBase? item in array)
        {
            if (item is COSDictionary dict)
            {
                Dictionary<string, string> map = [];
                foreach (COSName key in dict.KeySet())
                {
                    string? value = dict.GetString(key);
                    if (value != null)
                    {
                        map[key.GetName()] = value;
                    }
                }
                result.Add(map);
            }
        }

        return result;
    }

    public void SetSubjectDN(List<Dictionary<string, string>> subjectDN)
    {
        COSArray array = new();
        foreach (Dictionary<string, string> item in subjectDN)
        {
            COSDictionary dict = new();
            foreach ((string key, string value) in item)
            {
                dict.SetItem(key, new COSString(value));
            }
            array.Add(dict);
        }
        _dictionary.SetItem(COSName.GetPDFName("SubjectDN"), array);
    }

    public string GetURL() => _dictionary.GetString(COSName.GetPDFName("URL"), string.Empty);
    public void SetURL(string? url) => _dictionary.SetString(COSName.GetPDFName("URL"), url);

    public string? GetURLType() => _dictionary.GetNameAsString(COSName.GetPDFName("URLType"));
    public void SetURLType(string? urlType) => _dictionary.SetName(COSName.GetPDFName("URLType"), urlType);

    private List<byte[]>? GetByteArrays(COSName key)
    {
        COSArray? array = _dictionary.GetCOSArray(key);
        if (array == null)
        {
            return null;
        }

        List<byte[]> result = [];
        foreach (COSBase? item in array)
        {
            if (item is COSString str)
            {
                result.Add(str.GetBytes());
            }
        }
        return result;
    }

    private static COSArray ToByteArrayCOS(List<byte[]> values)
    {
        COSArray array = new();
        foreach (byte[] value in values)
        {
            array.Add(new COSString(value));
        }
        return array;
    }

    private void AddByteArray(COSName key, byte[] value)
    {
        COSArray array = _dictionary.GetCOSArray(key) ?? new COSArray();
        array.Add(new COSString(value));
        _dictionary.SetItem(key, array);
    }

    private void RemoveByteArray(COSName key, byte[] value)
    {
        _dictionary.GetCOSArray(key)?.Remove(new COSString(value));
    }
}
