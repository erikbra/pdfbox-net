/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * OCSP/LTV parity tests for issue #531 with AI assistance.
 *
 * PORT_MODE: native-test
 */

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Security;
using PdfBox.Net.Cryptography.Signature.Cert;
using BcX509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace PdfBox.Net.Tests;

public class Issue531OcspParityTest
{
    [Fact]
    public void BouncyCastleOcspClient_ParsesGoodResponseAndPreservesRawBytes()
    {
        using RSA issuerKey = RSA.Create(2048);
        CertificateRequest issuerRequest = new(
            "CN=PdfBox.Net Test Issuer",
            issuerKey,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        issuerRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        issuerRequest.CertificateExtensions.Add(new X509KeyUsageExtension(
            X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.DigitalSignature,
            true));
        using X509Certificate2 issuer = issuerRequest.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(30));

        using RSA leafKey = RSA.Create(2048);
        CertificateRequest leafRequest = new(
            "CN=PdfBox.Net Test Leaf",
            leafKey,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        using X509Certificate2 leaf = leafRequest.Create(
            issuer,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(7),
            [0x01, 0x02, 0x03, 0x04]);

        byte[] ocspResponse = CreateGoodOcspResponse(leaf, issuer, issuerKey);

        BouncyCastleOcspResponse parsed = BouncyCastleOcspClient.ParseAndValidateResponse(
            ocspResponse,
            leaf,
            issuer,
            DateTime.UtcNow);

        Assert.Equal(ocspResponse, parsed.EncodedResponse);
        Assert.Equal(40, parsed.SignatureHashHex.Length);
        Assert.NotNull(parsed.ResponderCertificate);
        Assert.Equal(issuer.Subject, parsed.ResponderCertificate.Subject);
    }

    private static byte[] CreateGoodOcspResponse(
        X509Certificate2 leaf,
        X509Certificate2 issuer,
        RSA issuerKey)
    {
        BcX509Certificate bcLeaf = DotNetUtilities.FromX509Certificate(leaf);
        BcX509Certificate bcIssuer = DotNetUtilities.FromX509Certificate(issuer);
        CertificateID certificateId = new(CertificateID.DigestSha1, bcIssuer, bcLeaf.SerialNumber);
        BasicOcspRespGenerator generator = new(bcIssuer.GetPublicKey());
        generator.AddResponse(
            certificateId,
            CertificateStatus.Good,
            DateTime.UtcNow.AddMinutes(-1),
            DateTime.UtcNow.AddDays(1),
            null);

        BasicOcspResp basicResponse = generator.Generate(
            "SHA256WITHRSA",
            DotNetUtilities.GetKeyPair(issuerKey).Private,
            [bcIssuer],
            DateTime.UtcNow);
        OcspResp response = new OCSPRespGenerator().Generate(OcspRespStatus.Successful, basicResponse);
        return response.GetEncoded();
    }
}
