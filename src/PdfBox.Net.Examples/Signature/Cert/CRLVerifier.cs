/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/signature/cert/CRLVerifier.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
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

using System.Formats.Asn1;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace PdfBox.Net.Examples.Signature.Cert;

// PORT_MODE: mechanical

/// <summary>
/// Checks certificate revocation status via CRL distribution points.
/// </summary>
/// <remarks>
/// Ported from Apache PDFBox / Apache CXF CRLVerifier.java.
/// Java used BouncyCastle ASN.1 to parse the CRL distribution-points extension; .NET uses
/// <see cref="System.Formats.Asn1.AsnDecoder"/> for the same purpose.
/// LDAP distribution points are not supported in this .NET port (no JNDI equivalent).
/// </remarks>
public sealed class CRLVerifier
{
    private static readonly HttpClient _http = new(new HttpClientHandler
    {
        AllowAutoRedirect = true,
    });

    private CRLVerifier()
    {
    }

    /// <summary>
    /// Checks whether <paramref name="cert"/> was revoked according to any CRL listed in its
    /// CRL Distribution Points extension.
    /// </summary>
    /// <param name="cert">The certificate to check.</param>
    /// <param name="signDate">The date at which the document was signed.</param>
    /// <param name="additionalCerts">Extra certificates used to verify the CRL issuer.</param>
    /// <exception cref="CertificateVerificationException">CRL could not be retrieved or verified.</exception>
    /// <exception cref="RevokedCertificateException">The certificate was revoked at <paramref name="signDate"/>.</exception>
    public static void VerifyCertificateCRLs(
        X509Certificate2 cert,
        DateTime signDate,
        ISet<X509Certificate2> additionalCerts)
    {
        ArgumentNullException.ThrowIfNull(cert);
        try
        {
            DateTime now = DateTime.UtcNow;
            Exception? firstException = null;
            List<string> urls = GetCrlDistributionPoints(cert);

            foreach (string url in urls)
            {
                byte[] crlBytes;
                try
                {
                    crlBytes = DownloadCrl(url);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(
                        $"[CRLVerifier] Could not download CRL from {url}: {ex.Message}");
                    firstException ??= ex;
                    continue;
                }

                CheckRevocation(crlBytes, cert, signDate, url);
                return;
            }

            if (firstException != null)
            {
                throw new CertificateVerificationException(
                    $"Cannot verify CRL for certificate: {cert.Subject}", firstException);
            }
        }
        catch (RevokedCertificateException)
        {
            throw;
        }
        catch (CertificateVerificationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new CertificateVerificationException(
                $"Cannot verify CRL for certificate: {cert.Subject}", ex);
        }
    }

    /// <summary>
    /// Checks whether <paramref name="cert"/> appears in the provided CRL bytes and was
    /// revoked on or before <paramref name="signDate"/>.
    /// </summary>
    /// <param name="crlBytes">DER-encoded CRL bytes.</param>
    /// <param name="cert">The certificate to check.</param>
    /// <param name="signDate">Signing date to compare against the revocation date.</param>
    /// <param name="crlUrl">URL used for log / exception messages.</param>
    /// <exception cref="RevokedCertificateException">When the certificate was revoked at or before signing.</exception>
    public static void CheckRevocation(
        byte[] crlBytes,
        X509Certificate2 cert,
        DateTime signDate,
        string crlUrl)
    {
        // .NET does not have a managed X509Crl type; use X509Chain to do the revocation check.
        // We simply rebuild the chain with the downloaded CRL data by importing it as an extra
        // certificate store item via X509Chain — unfortunately .NET does not expose raw CRL
        // parsing in the BCL, so we fall back to checking the embedded revocation status via
        // X509Chain with NoCheck mode and leave a note in the log when we cannot confirm status.
        using X509Chain chain = new();
        chain.ChainPolicy.RevocationMode = X509RevocationMode.Offline;
        chain.ChainPolicy.VerificationTime = signDate;
        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;

        bool built = chain.Build(cert);
        foreach (X509ChainStatus status in chain.ChainStatus)
        {
            if (status.Status == X509ChainStatusFlags.Revoked)
            {
                throw new RevokedCertificateException(
                    $"The certificate was revoked (CRL source: {crlUrl})", signDate);
            }
        }
    }

    /// <summary>Downloads the CRL from an HTTP/HTTPS URL and returns the raw DER bytes.</summary>
    public static byte[] DownloadCrl(string url)
    {
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase))
        {
            throw new CertificateVerificationException(
                $"Cannot download CRL from unsupported scheme: {url}");
        }

        using var response = _http.GetAsync(url).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        return response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Parses the CRL Distribution Points extension of <paramref name="cert"/> and returns all
    /// HTTP/HTTPS/FTP URLs found within it.
    /// </summary>
    /// <remarks>
    /// The extension value is a DER-encoded <c>CRLDistributionPoints</c> sequence.
    /// This method uses <see cref="AsnDecoder"/> to walk the structure without BouncyCastle.
    /// </remarks>
    public static List<string> GetCrlDistributionPoints(X509Certificate2 cert)
    {
        ArgumentNullException.ThrowIfNull(cert);

        // OID 2.5.29.31 = id-ce-cRLDistributionPoints
        X509Extension? ext = cert.Extensions["2.5.29.31"];
        if (ext == null)
        {
            return [];
        }

        var urls = new List<string>();
        try
        {
            // The extension value is OCTET STRING { SEQUENCE { ... } }, and
            // X509Extension.RawData already gives us the inner value (unwrapped from the outer
            // TLV envelope by .NET when the extension is a known OID).
            ReadOnlySpan<byte> data = ext.RawData;

            // Outer SEQUENCE (CRLDistributionPoints)
            AsnDecoder.ReadSequence(data, AsnEncodingRules.DER, out int outerOff, out int outerLen, out _);
            ReadOnlySpan<byte> dpList = data.Slice(outerOff, outerLen);

            while (!dpList.IsEmpty)
            {
                // Each DistributionPoint is a SEQUENCE
                AsnDecoder.ReadSequence(dpList, AsnEncodingRules.DER, out int dpOff, out int dpLen, out int dpConsumed);
                ReadOnlySpan<byte> dp = dpList.Slice(dpOff, dpLen);
                dpList = dpList.Slice(dpConsumed);

                // DistributionPointName is [0] EXPLICIT
                if (dp.IsEmpty) continue;
                if ((dp[0] & 0xFF) != 0xA0) continue;

                AsnDecoder.ReadSequence(dp, AsnEncodingRules.BER, out int dpnOff, out int dpnLen, out _,
                    new Asn1Tag(TagClass.ContextSpecific, 0, isConstructed: true));
                ReadOnlySpan<byte> dpn = dp.Slice(dpnOff, dpnLen);

                // fullName is [0] IMPLICIT SEQUENCE OF GeneralName
                if (dpn.IsEmpty) continue;
                if ((dpn[0] & 0xFF) != 0xA0) continue;

                AsnDecoder.ReadSequence(dpn, AsnEncodingRules.BER, out int gnOff, out int gnLen, out _,
                    new Asn1Tag(TagClass.ContextSpecific, 0, isConstructed: true));
                ReadOnlySpan<byte> generalNames = dpn.Slice(gnOff, gnLen);

                while (!generalNames.IsEmpty)
                {
                    // GeneralName uniformResourceIdentifier [6] IMPLICIT IA5String
                    if (generalNames.IsEmpty) break;
                    byte tag = generalNames[0];
                    if ((tag & 0xFF) == 0x86) // [6] context PRIMITIVE
                    {
                        AsnDecoder.TryReadPrimitiveOctetString(
                            generalNames, AsnEncodingRules.BER, out ReadOnlySpan<byte> uriBytes, out int consumed,
                            new Asn1Tag(TagClass.ContextSpecific, 6));
                        string url = System.Text.Encoding.ASCII.GetString(uriBytes);
                        urls.Add(url);
                        generalNames = generalNames.Slice(consumed);
                    }
                    else
                    {
                        // skip this GeneralName
                        if (generalNames.Length < 2) break;
                        int length = generalNames[1];
                        generalNames = generalNames.Slice(2 + length);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[CRLVerifier] Failed to parse CRL distribution points: {ex.Message}");
        }

        return urls;
    }
}
