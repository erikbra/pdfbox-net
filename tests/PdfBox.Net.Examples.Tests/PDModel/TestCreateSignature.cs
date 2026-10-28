/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java tests with AI assistance.
 */

// PDFBOX_SOURCE_PATH: examples/src/test/java/org/apache/pdfbox/examples/pdmodel/TestCreateSignature.java
// PDFBOX_SOURCE_COMMIT: ddef86fcb1a5407035fdd1c8587832c3d1c761b9
// PORT_MODE: adapted
// PORT_LAST_SYNC_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc

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

using PdfBox.Net.COS;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using PdfBox.Net.Examples.Signature;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Interactive.DigitalSignature;
using PdfBox.Net.PDModel.Interactive.Form;

namespace PdfBox.Net.Examples.Tests.PDModel;

/// <summary>
/// Test for signature creation and validation examples.
/// A self-signed, in-memory PKCS#12 keystore keeps the detached-signature test deterministic
/// and suitable for CI. Tests which contact a TSA or revocation service remain skipped.
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

        SignedCms cms = new(new ContentInfo(signature.GetSignedContent(signedPdfBytes)), detached: true);
        cms.Decode(signature.GetContents(signedPdfBytes));
        cms.CheckSignature(verifySignatureOnly: true);
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public void TestSignEncrypted(bool external, bool omitType, bool aes)
    {
        byte[] inputPdf = CreateInputPdf();
        using MemoryStream encrypted = new();
        using (PDDocument document = PDDocument.Load(new MemoryStream(inputPdf)))
        {
            var policy = new PdfBox.Net.PDModel.Encryption.StandardProtectionPolicy("owner", "12345", new());
            policy.SetEncryptionKeyLength(aes ? 128 : 40);
            policy.SetPreferAes(aes);
            document.Protect(policy);
            document.Save(encrypted);
        }
        byte[] keyStore = CreateKeyStore("password");
        CreateSignature signer = new(keyStore, "password") { IsExternalSigning = external };
        using MemoryStream signed = new();
        encrypted.Position = 0;
        if (!omitType)
        {
            signer.SignDetached(encrypted, signed, tsaClient: null, password: "12345");
        }
        else
        {
            using PDDocument document = PDDocument.Load(encrypted, "12345");
            PDSignature pending = new();
            pending.SetFilter(PDSignature.FILTER_ADOBE_PPKLITE);
            pending.SetSubFilter(PDSignature.SUBFILTER_ADBE_PKCS7_DETACHED);
            pending.SetName(signer.GetCertificate()!.GetNameInfo(X509NameType.SimpleName, false));
            SignatureOptions options = new();
            if (external)
            {
                document.AddSignature(pending, options);
                ((COSDictionary)pending.GetCOSObject()).RemoveItem(COSName.TYPE);
                ExternalSigningSupport support = document.SaveIncrementalForExternalSigning(signed);
                support.SetSignature(signer.Sign(support.GetContent()));
            }
            else
            {
                document.AddSignature(pending, signer, options);
                ((COSDictionary)pending.GetCOSObject()).RemoveItem(COSName.TYPE);
                document.SaveIncremental(signed);
            }
        }
        byte[] bytes = signed.ToArray();
        using PDDocument result = PDDocument.Load(new MemoryStream(bytes), "12345");
        Assert.True(result.IsEncrypted());
        Assert.Equal(aes ? 4 : 1, result.GetEncryption()!.GetVersion());
        PDSignature signature = Assert.Single(result.GetSignatureDictionaries());
        Assert.Equal(omitType, ((COSDictionary)signature.GetCOSObject()).GetCOSName(COSName.TYPE) is null);
        SignedCms cms = new(new ContentInfo(signature.GetSignedContent(bytes)), detached: true);
        cms.Decode(signature.GetContents(bytes));
        cms.CheckSignature(verifySignatureOnly: true);
        Assert.Equal(signer.GetCertificate()!.GetNameInfo(X509NameType.SimpleName, false), signature.GetName());
        byte[] contents = signature.GetContents();
        Assert.All(contents.TakeLast(5), value => Assert.Equal((byte)0, value));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FullSaveLeavesOptionalTypeSignatureContentsUnencrypted(bool aes)
    {
        using PDDocument document = new();
        document.AddPage(new());
        PDAcroForm form = new(document);
        PDSignatureField field = new(form);
        PDSignature signature = new();
        signature.SetName("signature-owner");
        signature.SetByteRange([0, 1, 2, 3]);
        signature.SetContents([0x30, 3, 2, 1, 1, 0, 0]);
        ((COSDictionary)signature.GetCOSObject()).RemoveItem(COSName.TYPE);
        field.SetValue(signature);
        form.SetFields([field]);
        document.GetDocumentCatalog().SetAcroForm(form);
        var policy = new PdfBox.Net.PDModel.Encryption.StandardProtectionPolicy("owner", "user", new());
        policy.SetEncryptionKeyLength(aes ? 128 : 40);
        policy.SetPreferAes(aes);
        document.Protect(policy);
        using MemoryStream output = new();
        document.Save(output);
        byte[] bytes = output.ToArray();
        string raw = System.Text.Encoding.Latin1.GetString(bytes);
        Assert.Contains("<30030201010000>", raw);
        Assert.DoesNotContain("signature-owner", raw);
        using PDDocument loaded = PDDocument.Load(new MemoryStream(bytes), "user");
        PDSignature reloaded = Assert.Single(loaded.GetSignatureDictionaries());
        Assert.Equal([0x30, 3, 2, 1, 1, 0, 0], reloaded.GetContents());
        Assert.Equal("signature-owner", reloaded.GetName());
    }

    [Fact(Skip = "Requires an external TSA endpoint.")]
    public void TestDetachedSha256WithTSA()
    {
    }

    [Fact(Skip = "Visible-signature appearance parity is not covered by this deterministic suite.")]
    public void TestCreateVisibleSignature()
    {
    }

    [Fact(Skip = "Visible-signature appearance parity is not covered by this deterministic suite.")]
    public void TestCreateVisibleSignature2()
    {
    }

    [Fact(Skip = "Requires online OCSP and CRL responders.")]
    public void TestAddValidationInformation()
    {
    }

    [Fact(Skip = "Requires an external TSA endpoint.")]
    public void TestCreateEmbeddedTimeStamp()
    {
    }

    [Fact(Skip = "Requires an external TSA endpoint.")]
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
