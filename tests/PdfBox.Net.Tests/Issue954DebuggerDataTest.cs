using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using PdfBox.Net.COS;
using PdfBox.Net.Debugger;
using PdfBox.Net.Debugger.Certificatepane;
using PdfBox.Net.Debugger.Flagbitspane;

namespace PdfBox.Net.Tests;

public class Issue954DebuggerDataTest
{
    [Fact]
    public void CertificateDetailsAreDecodedFromStringAndStreamAndShownByInspector()
    {
        using RSA key = RSA.Create(2048);
        CertificateRequest request = new("CN=PDFBox Certificate Test", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using X509Certificate2 certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1));
        byte[] encoded = certificate.Export(X509ContentType.Cert);
        COSString value = new(encoded);
        using COSStream stream = new();
        using (Stream output = stream.CreateOutputStream())
        {
            output.Write(encoded);
        }

        Assert.Contains("PDFBox Certificate Test", new CertificatePane(value).GetText(), StringComparison.Ordinal);
        Assert.Equal(new CertificatePane(value).GetText(), new CertificatePane(stream).GetText());

        COSDictionary signature = new();
        signature.SetItem(COSName.GetPDFName("Cert"), value);
        COSArray certificates = new();
        certificates.Add(stream);
        signature.SetItem(COSName.GetPDFName("Certs"), certificates);
        using StringWriter dump = new();
        PDFDebugger.DumpCOSTree(signature, dump);
        Assert.Contains("Certificate View:", dump.ToString(), StringComparison.Ordinal);
        Assert.Contains("PDFBox Certificate Test", dump.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void MalformedCertificateProducesTextDiagnostic()
    {
        CertificatePane pane = new(new COSString([0, 1, 2]));
        Assert.NotEmpty(pane.GetText());
        Assert.Throws<ArgumentException>(() => new CertificatePane(new COSDictionary()));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(12)]
    [InlineData(15)]
    public void PanoseDataIsNormalizedAndInvalidValuesRemainInspectable(int length)
    {
        COSDictionary style = new();
        style.SetItem(COSName.GetPDFName("Panose"), new COSString(Enumerable.Repeat((byte)255, length).ToArray()));
        PanoseFlag flags = new(style);
        Assert.Equal("Panose byte: " + new string('F', Math.Min(length, 12) * 2) + new string('0', Math.Max(12 - length, 0) * 2), flags.GetFlagValue());
        object[][] rows = flags.GetFlagBits();
        Assert.Equal(10, rows.Length);
        Assert.Equal(length > 2 ? "invalid value" : "Any", rows[0][3]);
        Assert.Equal(length > 11 ? "invalid value" : "Any", rows[9][3]);
    }

    [Fact]
    public void MissingPanoseUsesZeroValues()
    {
        PanoseFlag flags = new(new COSDictionary());
        Assert.All(flags.GetFlagBits(), row => Assert.Equal("Any", row[3]));
        Assert.Equal("Panose byte: " + new string('0', 24), flags.GetFlagValue());
    }
}
