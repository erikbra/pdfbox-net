/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/signature/validation/CertInformationCollector.java
 * PDFBOX_SOURCE_COMMIT: ddef86fcb1a5407035fdd1c8587832c3d1c761b9
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: ddef86fcb1a5407035fdd1c8587832c3d1c761b9
 */

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

using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using PdfBox.Net.PDModel.Interactive.DigitalSignature;

namespace PdfBox.Net.Examples.Signature.Validation;

/// <summary>
/// Collects X.509 certificate information for PDF signature validation.
/// </summary>
/// <remarks>
/// <para>
/// Ported from the Java <c>CertInformationCollector</c> example.
/// BouncyCastle <c>CMSSignedData</c> is replaced with <see cref="SignedCms"/>.
/// BouncyCastle <c>JcaX509CertificateConverter</c> is not needed because
/// <see cref="X509Certificate2"/> is used throughout.
/// </para>
/// <para>
/// Issuer discovery uses DN byte-array comparison
/// (<c>cert.IssuerName.RawData.SequenceEqual(issuer.SubjectName.RawData)</c>)
/// rather than a full cryptographic verify, which is consistent with the goal of
/// building a certificate chain for LTV validation purposes.
/// </para>
/// </remarks>
public class CertInformationCollector
{
    private static readonly HttpClient _http = new(new HttpClientHandler { AllowAutoRedirect = true });

    private const int MaxCertificateChainDepth = 5;

    /// <summary>OID for id-aa-signatureTimeStampToken (RFC 3161 embedded timestamp).</summary>
    private const string IdAaSignatureTimeStampToken = "1.2.840.113549.1.9.16.2.14";

    /// <summary>OID for id-pe-authorityInfoAccess.</summary>
    private const string AuthorityInfoAccessOid = "1.3.6.1.5.5.7.1.1";

    /// <summary>OID for id-ce-cRLDistributionPoints.</summary>
    private const string CrlDistributionPointsOid = "2.5.29.31";

    private readonly HashSet<X509Certificate2> _certificateSet = [];
    private readonly HashSet<string> _urlSet = [];

    private CertSignatureInformation? _rootCertInfo;

    /// <summary>
    /// Gets the certificate information for the last signature in the document.
    /// </summary>
    /// <param name="signature">The PDF signature to analyse.</param>
    /// <param name="fileName">Path to the source PDF file (used to read signature bytes).</param>
    public CertSignatureInformation GetLastCertInfo(PDSignature signature, string fileName)
    {
        ArgumentNullException.ThrowIfNull(signature);
        using var docStream = File.OpenRead(fileName);
        byte[] signatureContent = signature.GetContents(docStream);
        return GetCertInfoFromBytes(signatureContent);
    }

    /// <summary>
    /// Adds certificates from raw DER-encoded certificate bytes.
    /// </summary>
    public void AddAllCertsFromHolders(IEnumerable<byte[]> certDerList)
    {
        foreach (byte[] der in certDerList)
        {
            try
            {
                _certificateSet.Add(X509CertificateLoader.LoadCertificate(der));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    "[CertInformationCollector] Failed to parse certificate: " + ex.Message);
            }
        }
    }

    /// <summary>
    /// Traverses the certificate chain starting from <paramref name="certificate"/> and
    /// returns a populated <see cref="CertSignatureInformation"/>.
    /// </summary>
    public CertSignatureInformation GetCertInfo(X509Certificate2 certificate)
    {
        try
        {
            var certSigInfo = new CertSignatureInformation();
            TraverseChain(certificate, certSigInfo, MaxCertificateChainDepth);
            return certSigInfo;
        }
        catch (IOException ex)
        {
            throw new CertificateProccessingException("Error traversing certificate chain", ex);
        }
    }

    /// <summary>Gets the set of all processed certificates found so far.</summary>
    public ISet<X509Certificate2> GetCertificateSet() => _certificateSet;

    // -------------------------------------------------------------------------
    // Private implementation
    // -------------------------------------------------------------------------

    private CertSignatureInformation GetCertInfoFromBytes(byte[] signatureContent)
    {
        _rootCertInfo = new CertSignatureInformation
        {
            SignatureHash = CertInformationHelper.GetSha1Hash(signatureContent)
        };

        try
        {
            SignedCms signedCms = new();
            signedCms.Decode(signatureContent);

            SignerInfo signerInfo = ProcessSignerStore(signedCms, _rootCertInfo);
            AddTimestampCerts(signerInfo);
        }
        catch (CryptographicException ex)
        {
            Console.Error.WriteLine(
                "[CertInformationCollector] Error getting certificate info from signature: " + ex.Message);
            throw new CertificateProccessingException(
                "Error occurred getting Certificate Information from Signature", ex);
        }

        return _rootCertInfo;
    }

    private void AddTimestampCerts(SignerInfo signerInfo)
    {
        // Look for embedded RFC 3161 timestamp token in unsigned attributes.
        CryptographicAttributeObject? tsAttr = null;
        foreach (CryptographicAttributeObject attr in signerInfo.UnsignedAttributes)
        {
            if (attr.Oid.Value == IdAaSignatureTimeStampToken)
            {
                tsAttr = attr;
                break;
            }
        }

        if (tsAttr == null) return;

        foreach (AsnEncodedData value in tsAttr.Values)
        {
            try
            {
                // The attribute value is a ContentInfo / SignedData DER-blob.
                SignedCms tsCms = new();
                tsCms.Decode(value.RawData);
                _rootCertInfo!.TsaCerts = new CertSignatureInformation();
                ProcessSignerStore(tsCms, _rootCertInfo.TsaCerts);
                return; // handle only first timestamp
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    "[CertInformationCollector] Error parsing timestamp token: " + ex.Message);
            }
        }
    }

    private SignerInfo ProcessSignerStore(
        SignedCms signedCms,
        CertSignatureInformation certInfo)
    {
        // Add all embedded certificates to the set.
        foreach (X509Certificate2 embeddedCert in signedCms.Certificates)
        {
            _certificateSet.Add(embeddedCert);
        }

        if (signedCms.SignerInfos.Count == 0)
        {
            throw new CertificateProccessingException("No SignerInfo found in SignedCms.");
        }

        SignerInfo signerInfo = signedCms.SignerInfos[0];

        // Find the signer's certificate.
        X509Certificate2? signerCert = signerInfo.Certificate;

        // If not directly available, search in the embedded set by issuer/serial.
        if (signerCert == null)
        {
            foreach (X509Certificate2 c in _certificateSet)
            {
                if (MatchesSigner(c, signerInfo))
                {
                    signerCert = c;
                    break;
                }
            }
        }

        if (signerCert == null)
        {
            throw new CertificateProccessingException("Cannot find signer certificate in SignedCms.");
        }

        _certificateSet.Add(signerCert);
        TraverseChain(signerCert, certInfo, MaxCertificateChainDepth);
        return signerInfo;
    }

    private static bool MatchesSigner(X509Certificate2 cert, SignerInfo signerInfo)
    {
        // If the SignedCms decoder already populated Certificate, use it directly.
        if (signerInfo.Certificate != null)
        {
            return cert.SerialNumber == signerInfo.Certificate.SerialNumber &&
                   cert.IssuerName.RawData.SequenceEqual(signerInfo.Certificate.IssuerName.RawData);
        }

        // Fallback: use reflection to read serial number from SubjectIdentifier.Value
        // (avoids a direct reference to X509IssuerSerial whose namespace may vary by TFM).
        try
        {
            var sid = signerInfo.SignerIdentifier;
            if (sid.Type == SubjectIdentifierType.IssuerAndSerialNumber && sid.Value != null)
            {
                var serialProp = sid.Value.GetType().GetProperty("SerialNumber");
                if (serialProp?.GetValue(sid.Value) is string serial)
                {
                    return cert.SerialNumber.Equals(
                        serial.Replace(" ", ""), StringComparison.OrdinalIgnoreCase);
                }
            }
            if (sid.Type == SubjectIdentifierType.SubjectKeyIdentifier && sid.Value is string ski)
            {
                var skiExt = cert.Extensions["2.5.29.14"];
                return skiExt != null &&
                       Convert.ToHexString(skiExt.RawData)
                           .Contains(ski.Replace(" ", ""), StringComparison.OrdinalIgnoreCase);
            }
        }
        catch
        {
            // ignore
        }

        return false;
    }

    private void TraverseChain(
        X509Certificate2 certificate,
        CertSignatureInformation certInfo,
        int maxDepth)
    {
        certInfo.Certificate = certificate;

        // Authority Information Access  (OID 1.3.6.1.5.5.7.1.1)
        X509Extension? aiaExt = certificate.Extensions[AuthorityInfoAccessOid];
        if (aiaExt != null)
        {
            CertInformationHelper.GetAuthorityInfoExtensionValue(aiaExt.RawData, certInfo);
        }

        if (certInfo.IssuerUrl != null)
        {
            GetAlternativeIssuerCertificate(certInfo, maxDepth);
        }

        // CRL Distribution Points  (OID 2.5.29.31)
        X509Extension? crlExt = certificate.Extensions[CrlDistributionPointsOid];
        if (crlExt != null)
        {
            certInfo.CrlUrl = CertInformationHelper.GetCrlUrlFromExtensionValue(crlExt.RawData);
        }

        certInfo.IsSelfSigned = IsSelfSigned(certificate);
        if (maxDepth <= 0 || certInfo.IsSelfSigned)
        {
            return;
        }

        int found = 0;
        foreach (X509Certificate2 issuer in _certificateSet)
        {
            if (!IsIssuerOf(certificate, issuer)) continue;

            Console.WriteLine($"[CertInformationCollector] Found issuer for {certificate.Subject}: {issuer.Subject}");
            certInfo.IssuerCertificates.Add(issuer);
            certInfo.CertChain = new CertSignatureInformation();
            TraverseChain(issuer, certInfo.CertChain, maxDepth - 1);
            ++found;
        }

        if (certInfo.IssuerCertificates.Count == 0)
        {
            throw new IOException(
                $"No Issuer Certificate found for Cert: '{certificate.Subject}', " +
                $"i.e. Cert '{certificate.Issuer}' is missing in the chain");
        }

        if (found > 1)
        {
            Console.WriteLine($"[CertInformationCollector] Several issuers for Cert: '{certificate.Subject}'");
        }
    }

    private void GetAlternativeIssuerCertificate(
        CertSignatureInformation certInfo,
        int maxDepth)
    {
        if (certInfo.IssuerUrl == null || _urlSet.Contains(certInfo.IssuerUrl)) return;
        _urlSet.Add(certInfo.IssuerUrl);

        Console.WriteLine($"[CertInformationCollector] Fetching alternative issuer cert from: {certInfo.IssuerUrl}");
        try
        {
            byte[] certBytes = _http.GetByteArrayAsync(certInfo.IssuerUrl).GetAwaiter().GetResult();
            X509Certificate2 altIssuerCert = X509CertificateLoader.LoadCertificate(certBytes);
            _certificateSet.Add(altIssuerCert);

            certInfo.AlternativeCertChain = new CertSignatureInformation();
            TraverseChain(altIssuerCert, certInfo.AlternativeCertChain, maxDepth - 1);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"[CertInformationCollector] Error getting alternative issuer cert from " +
                $"{certInfo.IssuerUrl}: {ex.Message}");
        }
    }

    /// <summary>
    /// Returns <c>true</c> when <paramref name="cert"/> appears to have been issued by
    /// <paramref name="issuer"/> (i.e. the cert's IssuerName matches the issuer's SubjectName).
    /// Uses byte-level DN comparison to avoid encoding-variant false negatives.
    /// </summary>
    private static bool IsIssuerOf(X509Certificate2 cert, X509Certificate2 issuer)
    {
        return cert.IssuerName.RawData.SequenceEqual(issuer.SubjectName.RawData);
    }

    /// <summary>
    /// Returns <c>true</c> when <paramref name="cert"/> is self-signed
    /// (its Subject and Issuer DN raw bytes are identical).
    /// </summary>
    private static bool IsSelfSigned(X509Certificate2 cert)
    {
        return cert.SubjectName.RawData.SequenceEqual(cert.IssuerName.RawData);
    }

    // -------------------------------------------------------------------------
    // Nested data class
    // -------------------------------------------------------------------------

    /// <summary>
    /// Data class that holds a signature's certificate chain and revocation information.
    /// </summary>
    public class CertSignatureInformation
    {
        /// <summary>The signer's certificate.</summary>
        public X509Certificate2? Certificate { get; set; }

        /// <summary>SHA-1 hash of the raw signature bytes (hex string).</summary>
        public string? SignatureHash { get; set; }

        /// <summary><c>true</c> if <see cref="Certificate"/> is self-signed.</summary>
        public bool IsSelfSigned { get; set; }

        /// <summary>OCSP responder URL (from AIA extension), or <c>null</c>.</summary>
        public string? OcspUrl { get; set; }

        /// <summary>First CRL distribution-point URL, or <c>null</c>.</summary>
        public string? CrlUrl { get; set; }

        /// <summary>
        /// Issuer certificate download URL from the AIA <c>caIssuers</c> access method.
        /// </summary>
        public string? IssuerUrl { get; set; }

        /// <summary>Issuer certificates found for <see cref="Certificate"/>.</summary>
        public HashSet<X509Certificate2> IssuerCertificates { get; } = [];

        /// <summary>Next link in the certificate chain.</summary>
        public CertSignatureInformation? CertChain { get; set; }

        /// <summary>Certificate information for an embedded RFC 3161 timestamp token.</summary>
        public CertSignatureInformation? TsaCerts { get; set; }

        /// <summary>Alternative chain downloaded from the AIA <c>caIssuers</c> URL.</summary>
        public CertSignatureInformation? AlternativeCertChain { get; set; }
    }
}
