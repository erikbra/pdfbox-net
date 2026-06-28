/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * BouncyCastle-backed OCSP helpers for LTV/DSS workflows.
 *
 * PORT_MODE: native-adapter
 */

using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Security;
using BcX509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace PdfBox.Net.Cryptography.Signature.Cert;

/// <summary>
/// Result of a BouncyCastle OCSP response validation.
/// </summary>
public sealed record BouncyCastleOcspResponse(
    byte[] EncodedResponse,
    string SignatureHashHex,
    X509Certificate2? ResponderCertificate);

/// <summary>
/// Builds OCSP requests and validates OCSP responses while preserving raw response bytes for DSS embedding.
/// </summary>
public sealed class BouncyCastleOcspClient
{
    private static readonly HttpClient SharedHttpClient = new();

    private readonly HttpClient _httpClient;

    public BouncyCastleOcspClient()
        : this(SharedHttpClient)
    {
    }

    public BouncyCastleOcspClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public BouncyCastleOcspResponse FetchAndValidateResponse(
        X509Certificate2 certificate,
        X509Certificate2 issuerCertificate,
        string ocspUrl,
        DateTime signDate)
    {
        ArgumentNullException.ThrowIfNull(certificate);
        ArgumentNullException.ThrowIfNull(issuerCertificate);
        ArgumentException.ThrowIfNullOrWhiteSpace(ocspUrl);

        byte[] requestBytes = CreateRequest(certificate, issuerCertificate);
        using ByteArrayContent content = new(requestBytes);
        content.Headers.ContentType = new("application/ocsp-request");
        using HttpRequestMessage request = new(HttpMethod.Post, ocspUrl);
        request.Headers.Accept.ParseAdd("application/ocsp-response");
        request.Content = content;

        using HttpResponseMessage response = _httpClient.Send(request);
        response.EnsureSuccessStatusCode();
        byte[] responseBytes = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
        return ParseAndValidateResponse(responseBytes, certificate, issuerCertificate, signDate);
    }

    public static byte[] CreateRequest(X509Certificate2 certificate, X509Certificate2 issuerCertificate)
    {
        ArgumentNullException.ThrowIfNull(certificate);
        ArgumentNullException.ThrowIfNull(issuerCertificate);

        BcX509Certificate bcCertificate = DotNetUtilities.FromX509Certificate(certificate);
        BcX509Certificate bcIssuer = DotNetUtilities.FromX509Certificate(issuerCertificate);
        CertificateID certId = new(CertificateID.DigestSha1, bcIssuer, bcCertificate.SerialNumber);
        OcspReqGenerator generator = new();
        generator.AddRequest(certId);
        return generator.Generate().GetEncoded();
    }

    public static BouncyCastleOcspResponse ParseAndValidateResponse(
        byte[] encodedResponse,
        X509Certificate2 certificate,
        X509Certificate2 issuerCertificate,
        DateTime signDate)
    {
        ArgumentNullException.ThrowIfNull(encodedResponse);
        ArgumentNullException.ThrowIfNull(certificate);
        ArgumentNullException.ThrowIfNull(issuerCertificate);

        BcX509Certificate bcCertificate = DotNetUtilities.FromX509Certificate(certificate);
        BcX509Certificate bcIssuer = DotNetUtilities.FromX509Certificate(issuerCertificate);
        OcspResp response = new(encodedResponse);
        if (response.Status != OcspRespStatus.Successful)
        {
            throw new CryptographicException($"OCSP responder returned status {response.Status}.");
        }

        if (response.GetResponseObject() is not BasicOcspResp basicResponse)
        {
            throw new CryptographicException("OCSP response does not contain a basic response.");
        }

        X509Certificate2? responderCertificate = ValidateBasicResponseSignature(basicResponse, bcIssuer);
        SingleResp singleResponse = FindSingleResponse(basicResponse, bcCertificate);
        ValidateCertificateStatus(singleResponse.GetCertStatus(), certificate, signDate);

        byte[] berEncodedSignature = new BerOctetString(basicResponse.GetSignature()).GetEncoded();
        string signatureHashHex = Convert.ToHexString(SHA1.HashData(berEncodedSignature));
        return new BouncyCastleOcspResponse(
            (byte[])encodedResponse.Clone(),
            signatureHashHex,
            responderCertificate);
    }

    private static X509Certificate2? ValidateBasicResponseSignature(
        BasicOcspResp basicResponse,
        BcX509Certificate issuerCertificate)
    {
        foreach (BcX509Certificate candidate in basicResponse.GetCerts())
        {
            if (basicResponse.Verify(candidate.GetPublicKey()))
            {
                return X509CertificateLoader.LoadCertificate(candidate.GetEncoded());
            }
        }

        if (basicResponse.Verify(issuerCertificate.GetPublicKey()))
        {
            return X509CertificateLoader.LoadCertificate(issuerCertificate.GetEncoded());
        }

        throw new CryptographicException("OCSP response signature could not be verified.");
    }

    private static SingleResp FindSingleResponse(BasicOcspResp basicResponse, BcX509Certificate certificate)
    {
        SingleResp[] responses = basicResponse.Responses;
        if (responses.Length != 1)
        {
            throw new CryptographicException($"OCSP response contained {responses.Length} single responses instead of 1.");
        }

        SingleResp response = responses[0];
        if (!response.GetCertID().SerialNumber.Equals(certificate.SerialNumber))
        {
            throw new CryptographicException("OCSP response does not match the requested certificate serial number.");
        }

        return response;
    }

    private static void ValidateCertificateStatus(object? status, X509Certificate2 certificate, DateTime signDate)
    {
        if (ReferenceEquals(status, CertificateStatus.Good))
        {
            return;
        }

        if (status is RevokedStatus revokedStatus)
        {
            if (revokedStatus.RevocationTime <= signDate)
            {
                throw new CryptographicException(
                    $"OCSP reports certificate {certificate.Subject} revoked since {revokedStatus.RevocationTime:O}.");
            }

            return;
        }

        throw new CryptographicException($"OCSP status for certificate {certificate.Subject} is unknown.");
    }
}
