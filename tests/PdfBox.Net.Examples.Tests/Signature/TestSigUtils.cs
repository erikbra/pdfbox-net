using PdfBox.Net.Examples.Signature;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Interactive.DigitalSignature;
using PdfBox.Net.PDModel.Interactive.Form;

namespace PdfBox.Net.Examples.Tests.Signature;

public class TestSigUtils
{
    [Fact]
    public void TimestampEmbeddingRejectsInvalidByteRangeBeforeContactingTsa()
    {
        string directory = Directory.CreateTempSubdirectory("pdfbox-invalid-signature-").FullName;
        try
        {
            string input = Path.Combine(directory, "input.pdf");
            string output = Path.Combine(directory, "output.pdf");
            using (PDDocument document = new())
            {
                document.AddPage(new PDPage());
                PDAcroForm form = new(document);
                PDSignatureField field = new(form);
                PDSignature signature = new();
                COSArray range = new();
                range.Add(COSInteger.Get(0));
                range.Add(COSInteger.Get(10));
                ((COSDictionary)signature.GetCOSObject()).SetItem(COSName.GetPDFName("ByteRange"), range);
                field.SetValue(signature);
                form.SetFields([field]);
                document.GetDocumentCatalog().SetAcroForm(form);
                document.Save(input);
            }

            IOException exception = Assert.Throws<IOException>(() =>
                new CreateEmbeddedTimeStamp("http://example.invalid/tsa").EmbedTimeStamp(input, output));

            Assert.Equal("/ByteRange should have length 4, but is [0, 10]", exception.Message);
            Assert.False(File.Exists(output));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void OpenURLFollowsOnlyOneExactHttpsUpgrade()
    {
        const string source = "http://www.pki.admin.ch/aia/RegularCA01.crt";
        using RedirectHandler handler = new("https://www.pki.admin.ch/aia/RegularCA01.crt");
        using HttpClient client = new(handler);

        using Stream stream = SigUtils.OpenURL(source, client);
        using StreamReader reader = new(stream);

        Assert.Equal("certificate", reader.ReadToEnd());
        Assert.Equal([source, "https://www.pki.admin.ch/aia/RegularCA01.crt"], handler.Requests);
    }

    [Theory]
    [InlineData("https://example.invalid/certificate.crt")]
    [InlineData("https://www.pki.admin.ch/aia/other.crt")]
    [InlineData("http://example.invalid/certificate.crt")]
    public void OpenURLDoesNotFollowOtherRedirectLocations(string location)
    {
        using RedirectHandler handler = new(location);
        using HttpClient client = new(handler);

        using Stream stream = SigUtils.OpenURL("http://www.pki.admin.ch/aia/RegularCA01.crt", client);

        Assert.Single(handler.Requests);
    }

    [Fact]
    public void OpenURLDoesNotFollowAnotherRedirectAfterHttpsUpgrade()
    {
        using RedirectHandler handler = new("https://www.pki.admin.ch/aia/RegularCA01.crt", redirectSecondRequest: true);
        using HttpClient client = new(handler);

        using Stream stream = SigUtils.OpenURL("http://www.pki.admin.ch/aia/RegularCA01.crt", client);

        Assert.Equal(2, handler.Requests.Count);
        Assert.DoesNotContain(handler.Requests, url => url.Contains("example.invalid", StringComparison.Ordinal));
    }

    [Fact]
    public void CheckAccessAcceptsExactUpstreamCertificateUrl()
    {
        SigUtils.CheckAccess(new Uri("http://www.pki.admin.ch/aia/RegularCA01.crt"));
    }

    [Theory]
    [InlineData("http://example.invalid/certificate.crt")]
    [InlineData("http://www.pki.admin.ch/aia/RegularCA01.crt?redirect=elsewhere")]
    [InlineData("http://www.pki.admin.ch.evil.invalid/aia/RegularCA01.crt")]
    public void OpenURLRejectsUnlistedUrlBeforeNetworkAccess(string url)
    {
        IOException exception = Assert.Throws<IOException>(() => SigUtils.OpenURL(url));

        Assert.Equal($"URL '{url}' not in allowUrlSet", exception.Message);
    }

    [Fact]
    public void CrlDownloadRejectsUnlistedHttpUrlBeforeNetworkAccess()
    {
        IOException exception = Assert.Throws<IOException>(() =>
            PdfBox.Net.Examples.Signature.Cert.CRLVerifier.DownloadCrl("http://example.invalid/certificate.crl"));

        Assert.Contains("not in allowUrlSet", exception.Message);
    }

    [Fact]
    public void CrlDownloadRejectsFtpProtocol()
    {
        Assert.Throws<PdfBox.Net.Examples.Signature.Cert.CertificateVerificationException>(() =>
            PdfBox.Net.Examples.Signature.Cert.CRLVerifier.DownloadCrl("ftp://example.invalid/certificate.crl"));
    }

    [Fact]
    public void OpenURL_RejectsNonHttpProtocol()
    {
        IOException exception = Assert.Throws<IOException>(
            () => SigUtils.OpenURL("ftp://example.invalid/certificate.crt"));

        Assert.Equal("ftp protocol not supported", exception.Message);
    }

    private sealed class RedirectHandler(string location, bool redirectSecondRequest = false) : HttpMessageHandler
    {
        public List<string> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request.RequestUri!.OriginalString);
            if (Requests.Count == 1 || redirectSecondRequest)
            {
                HttpResponseMessage response = new(System.Net.HttpStatusCode.Found)
                {
                    Content = new StringContent("redirect")
                };
                response.Headers.Location = new Uri(Requests.Count == 1 ? location : "https://example.invalid/redirected");
                return Task.FromResult(response);
            }
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("certificate")
            });
        }
    }
}
