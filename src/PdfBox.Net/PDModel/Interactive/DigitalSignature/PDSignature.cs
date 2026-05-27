using System.Text;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature;

public class PDSignature : COSObjectable
{
    public static readonly COSName FILTER_ADOBE_PPKLITE = COSName.GetPDFName("Adobe.PPKLite");
    public static readonly COSName FILTER_ENTRUST_PPKEF = COSName.GetPDFName("Entrust.PPKEF");
    public static readonly COSName FILTER_CICI_SIGNIT = COSName.GetPDFName("CICI.SignIt");
    public static readonly COSName FILTER_VERISIGN_PPKVS = COSName.GetPDFName("VeriSign.PPKVS");

    public static readonly COSName SUBFILTER_ADBE_X509_RSA_SHA1 = COSName.GetPDFName("adbe.x509.rsa_sha1");
    public static readonly COSName SUBFILTER_ADBE_PKCS7_DETACHED = COSName.GetPDFName("adbe.pkcs7.detached");
    public static readonly COSName SUBFILTER_ETSI_CADES_DETACHED = COSName.GetPDFName("ETSI.CAdES.detached");
    public static readonly COSName SUBFILTER_ADBE_PKCS7_SHA1 = COSName.GetPDFName("adbe.pkcs7.sha1");

    private readonly COSDictionary _dictionary;

    public PDSignature()
    {
        _dictionary = new COSDictionary();
        _dictionary.SetItem(COSName.TYPE, COSName.GetPDFName("Sig"));
    }

    public PDSignature(COSDictionary dictionary)
    {
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    public COSBase GetCOSObject() => _dictionary;

    public void SetType(COSName type) => _dictionary.SetItem(COSName.TYPE, type);
    public void SetFilter(COSName filter) => _dictionary.SetItem(COSName.FILTER, filter);
    public void SetSubFilter(COSName subfilter) => _dictionary.SetItem(COSName.GetPDFName("SubFilter"), subfilter);
    public void SetName(string? name) => _dictionary.SetString(COSName.NAME, name);
    public void SetLocation(string? location) => _dictionary.SetString(COSName.GetPDFName("Location"), location);
    public void SetReason(string? reason) => _dictionary.SetString(COSName.GetPDFName("Reason"), reason);
    public void SetContactInfo(string? contactInfo) => _dictionary.SetString(COSName.GetPDFName("ContactInfo"), contactInfo);
    public void SetSignDate(DateTimeOffset date) => _dictionary.SetDate(COSName.M, date);

    public string? GetFilter() => _dictionary.GetNameAsString(COSName.FILTER);
    public string? GetSubFilter() => _dictionary.GetNameAsString(COSName.GetPDFName("SubFilter"));
    public string? GetName() => _dictionary.GetString(COSName.NAME);
    public string? GetLocation() => _dictionary.GetString(COSName.GetPDFName("Location"));
    public string? GetReason() => _dictionary.GetString(COSName.GetPDFName("Reason"));
    public string? GetContactInfo() => _dictionary.GetString(COSName.GetPDFName("ContactInfo"));
    public DateTimeOffset? GetSignDate() => _dictionary.GetDate(COSName.M);

    public void SetByteRange(int[] range)
    {
        if (range is null || range.Length != 4)
        {
            return;
        }

        COSArray array = new();
        foreach (int i in range)
        {
            array.Add(COSInteger.Get(i));
        }

        array.SetDirect(true);
        _dictionary.SetItem(COSName.GetPDFName("ByteRange"), array);
    }

    public int[] GetByteRange()
    {
        COSArray? byteRange = _dictionary.GetCOSArray(COSName.GetPDFName("ByteRange"));
        if (byteRange == null)
        {
            return [];
        }

        int[] result = new int[byteRange.Size()];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = byteRange.GetInt(i);
        }

        return result;
    }

    public byte[] GetContents()
    {
        COSBase? value = _dictionary.GetDictionaryObject(COSName.CONTENTS);
        return value is COSString content ? content.GetBytes() : [];
    }

    public byte[] GetContents(Stream pdfFile)
    {
        int[] byteRange = GetByteRange();
        int begin = byteRange[0] + byteRange[1] + 1;
        int len = byteRange[2] - begin;
        return GetConvertedContents(new COSFilterInputStream(pdfFile, [begin, len]));
    }

    public byte[] GetContents(byte[] pdfFile)
    {
        int[] byteRange = GetByteRange();
        int begin = byteRange[0] + byteRange[1] + 1;
        int len = byteRange[2] - begin - 1;
        return GetConvertedContents(new MemoryStream(pdfFile, begin, len));
    }

    private static byte[] GetConvertedContents(Stream stream)
    {
        using (stream)
        {
            using MemoryStream output = new();
            stream.CopyTo(output);
            byte[] bytes = output.ToArray();

            if (bytes.Length > 0 && (bytes[0] == 0x3C || bytes[0] == 0x28))
            {
                bytes = bytes[1..];
            }

            if (bytes.Length > 0 && (bytes[^1] == 0x3E || bytes[^1] == 0x29))
            {
                bytes = bytes[..^1];
            }

            string text = Encoding.Latin1.GetString(bytes);
            return COSString.ParseHex(text).GetBytes();
        }
    }

    public void SetContents(byte[] bytes) => _dictionary.SetItem(COSName.CONTENTS, new COSString(bytes, true));

    public byte[] GetSignedContent(Stream pdfFile)
    {
        using COSFilterInputStream stream = new(pdfFile, GetByteRange());
        return stream.ToByteArray();
    }

    public byte[] GetSignedContent(byte[] pdfFile)
    {
        using COSFilterInputStream stream = new(pdfFile, GetByteRange());
        return stream.ToByteArray();
    }

    public PDPropBuild? GetPropBuild()
    {
        COSDictionary? dictionary = _dictionary.GetCOSDictionary(COSName.GetPDFName("Prop_Build"));
        return dictionary != null ? new PDPropBuild(dictionary) : null;
    }

    public void SetPropBuild(PDPropBuild? propBuild) => _dictionary.SetItem(COSName.GetPDFName("Prop_Build"), propBuild);
}
