// PDFBOX_SOURCE_PATH: examples/src/test/java/org/apache/pdfbox/examples/pdmodel/TestCreateSignature.java
// PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
// PORT_MODE: mechanical
// PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf

/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using PdfBox.Net.Examples.Signature;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.DigitalSignature;
using PdfBox.Net.PDModel.Interactive.Form;
using SkiaSharp;

namespace PdfBox.Net.Examples.Tests.PDModel;

/// <summary>
/// Test for signature creation and validation examples.
/// A self-signed, in-memory PKCS#12 keystore keeps the detached-signature test deterministic
/// and suitable for CI. Tests which contact a TSA or revocation service remain documented
/// external-service adaptations.
/// </summary>
public class TestCreateSignature
{
    [Fact]
    public void TestDetachedSha256()
    {
        byte[] inputPdf = CreateInputPdf();
        byte[] keyStore = CreateKeyStore("password");

        CreateSignature signer = new(keyStore, "password");
        using MemoryStream signedPdf = new();
        signer.SignDetached(new MemoryStream(inputPdf), signedPdf);

        byte[] signedPdfBytes = signedPdf.ToArray();
        Assert.NotEmpty(signedPdfBytes);

        using PDDocument document = PDDocument.Load(new MemoryStream(signedPdfBytes));
        PDSignature signature = Assert.Single(document.GetSignatureDictionaries());
        Assert.Equal("Adobe.PPKLite", signature.GetFilter());
        Assert.Equal("adbe.pkcs7.detached", signature.GetSubFilter());
        Assert.Equal(4, signature.GetByteRange().Length);
        AssertCmsSignatureVerifies(signature, signedPdfBytes);
    }

    [Fact(Skip = "Accepted #603 external-service adaptation: requires a TSA endpoint or a cached RFC 3161 response fixture.")]
    public void TestDetachedSha256WithTSA()
    {
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TestCreateVisibleSignature(bool externalSigning)
    {
        string outDir = ExampleTestResources.CreateTempDirectory("examples-tests-visible-signature");
        string inputPath = Path.Combine(outDir, "sign-me-visible.pdf");
        string imagePath = Path.Combine(outDir, "stamp.jpg");
        string outputPath = Path.Combine(outDir, externalSigning
            ? "signed-visible-external.pdf"
            : "signed-visible.pdf");
        File.WriteAllBytes(inputPath, CreateInputPdf());
        CreateStampJpeg(imagePath);

        CreateVisibleSignature signer = new();
        signer.SetKeystore(CreateKeyStore("password"), "password");
        signer.IsExternalSigning = externalSigning;
        using FileStream imageStream = File.OpenRead(imagePath);
        signer.SetVisibleSignDesigner(inputPath, 0, 0, -50, imageStream, 1);
        signer.SetVisibleSignatureProperties("name", "location", "Security", 0, 1, true);
        signer.SignPDF(inputPath, outputPath, tsaUrl: null);

        AssertVisibleSignatureOutput(outputPath, expectedReason: "Security");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TestCreateVisibleSignature2(bool externalSigning)
    {
        string outDir = ExampleTestResources.CreateTempDirectory("examples-tests-visible-signature2");
        string inputPath = Path.Combine(outDir, "sign-me-visible2.pdf");
        string imagePath = Path.Combine(outDir, "stamp.jpg");
        string outputPath = Path.Combine(outDir, externalSigning
            ? "signed-visible2-external.pdf"
            : "signed-visible2.pdf");
        File.WriteAllBytes(inputPath, CreateInputPdf());
        CreateStampJpeg(imagePath);

        CreateVisibleSignature2 signer = new()
        {
            ImageFile = imagePath,
            IsExternalSigning = externalSigning
        };
        signer.SetKeystore(CreateKeyStore("password"), "password");
        signer.SignPDF(inputPath, outputPath, 100, 200, 150, 50, tsaUrl: null);

        AssertVisibleSignatureOutput(outputPath, expectedReason: "Reason");
    }

    [Fact(Skip = "Accepted #603 external-service adaptation: requires online OCSP/CRL responders or a local revocation fixture.")]
    public void TestAddValidationInformation()
    {
    }

    [Fact(Skip = "Accepted #603 external-service adaptation: requires a TSA endpoint.")]
    public void TestCreateEmbeddedTimeStamp()
    {
    }

    [Fact(Skip = "Accepted #603 external-service adaptation: requires a TSA endpoint.")]
    public void TestCreateSignedTimeStamp()
    {
    }

    [Fact]
    public void TestEmptySignatureForm()
    {
        string outputPath = Path.Combine(Path.GetTempPath(), $"pdfbox-net-{Guid.NewGuid():N}.pdf");
        try
        {
            CreateEmptySignatureForm.CreateForm(outputPath);

            using PDDocument document = PDDocument.Load(outputPath);
            PDSignatureField signatureField = Assert.Single(document.GetSignatureFields());
            Assert.Null(signatureField.GetSignature());
            Assert.Equal(1, document.GetNumberOfPages());
        }
        finally
        {
            File.Delete(outputPath);
        }
    }

    private static byte[] CreateInputPdf()
    {
        using PDDocument document = new();
        document.AddPage(new PDPage());
        using MemoryStream output = new();
        document.Save(output);
        return output.ToArray();
    }

    private static void AssertVisibleSignatureOutput(string signedPdfPath, string expectedReason)
    {
        byte[] signedPdfBytes = File.ReadAllBytes(signedPdfPath);
        using PDDocument document = PDDocument.Load(new MemoryStream(signedPdfBytes));

        PDSignature signature = Assert.Single(document.GetSignatureDictionaries());
        Assert.Equal("Adobe.PPKLite", signature.GetFilter());
        Assert.Equal("adbe.pkcs7.detached", signature.GetSubFilter());
        Assert.Equal(expectedReason, signature.GetReason());
        Assert.Equal(4, signature.GetByteRange().Length);
        AssertCmsSignatureVerifies(signature, signedPdfBytes);

        PDSignatureField signatureField = Assert.Single(document.GetSignatureFields());
        Assert.NotNull(signatureField.GetSignature());
        PDAnnotationWidget widget = Assert.Single(signatureField.GetWidgets());
        Assert.NotNull(widget.GetRectangle());
        Assert.NotNull(widget.GetNormalAppearanceStream());
    }

    private static void AssertCmsSignatureVerifies(PDSignature signature, byte[] signedPdfBytes)
    {
        SignedCms cms = new(new ContentInfo(signature.GetSignedContent(signedPdfBytes)), detached: true);
        cms.Decode(signature.GetContents(signedPdfBytes));
        cms.CheckSignature(verifySignatureOnly: true);
    }

    private static void CreateStampJpeg(string path)
    {
        using SKBitmap bitmap = new(16, 16);
        using SKCanvas canvas = new(bitmap);
        canvas.Clear(new SKColor(255, 255, 255));
        using SKPaint paint = new()
        {
            Color = new SKColor(40, 90, 180),
            IsAntialias = true
        };
        canvas.DrawCircle(8, 8, 6, paint);
        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
        File.WriteAllBytes(path, data.ToArray());
    }

    private static byte[] CreateKeyStore(string password)
    {
        using RSA rsa = RSA.Create(2048);
        CertificateRequest request = new(
            "CN=pdfbox-net signing test",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509KeyUsageExtension(
            X509KeyUsageFlags.DigitalSignature,
            critical: true));

        using X509Certificate2 certificate = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(1));
        return certificate.Export(X509ContentType.Pkcs12, password);
    }
}
