/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/signature/cert/OcspHelper.java
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

using System.Security.Cryptography.X509Certificates;
using PdfBox.Net.Cryptography.Signature.Cert;

namespace PdfBox.Net.Examples.Signature.Cert;

// PORT_MODE: mechanical

/// <summary>
/// Checks certificate revocation status via OCSP.
/// </summary>
/// <remarks>
/// <para>
/// Uses the optional PdfBox.Net.Cryptography BouncyCastle backend to construct explicit OCSP
/// requests and preserve raw OCSP response bytes for DSS/LTV embedding.
/// </para>
/// </remarks>
public sealed class OcspHelper
{
    private readonly X509Certificate2 _certToCheck;
    private readonly DateTime _signDate;
    private readonly X509Certificate2 _issuerCert;
    private readonly string? _ocspUrl;

    /// <summary>
    /// Creates an OCSP helper for the given certificate.
    /// </summary>
    /// <param name="certToCheck">The end-entity certificate to check.</param>
    /// <param name="signDate">The signing date used as the reference time.</param>
    /// <param name="issuerCert">The issuer of <paramref name="certToCheck"/>.</param>
    /// <param name="ocspUrl">OCSP responder URL (unused in the .NET platform path but retained for API compatibility).</param>
    public OcspHelper(
        X509Certificate2 certToCheck,
        DateTime signDate,
        X509Certificate2 issuerCert,
        string? ocspUrl)
    {
        _certToCheck = certToCheck ?? throw new ArgumentNullException(nameof(certToCheck));
        _signDate = signDate;
        _issuerCert = issuerCert ?? throw new ArgumentNullException(nameof(issuerCert));
        _ocspUrl = ocspUrl;
    }

    public string? ResponseSignatureHashHex { get; private set; }

    public X509Certificate2? ResponderCertificate { get; private set; }

    /// <summary>
    /// Checks the revocation status of the configured certificate via OCSP and returns the raw
    /// encoded OCSP response for DSS embedding.
    /// </summary>
    /// <exception cref="RevokedCertificateException">
    /// Thrown when the certificate is reported as revoked.
    /// </exception>
    /// <exception cref="CertificateVerificationException">
    /// Thrown when the revocation status could not be determined.
    /// </exception>
    public byte[] GetResponseOcsp()
    {
        if (string.IsNullOrWhiteSpace(_ocspUrl))
        {
            throw new CertificateVerificationException(
                $"Could not verify certificate {_certToCheck.Subject}: no OCSP responder URL was supplied.");
        }

        try
        {
            BouncyCastleOcspResponse response = new BouncyCastleOcspClient()
                .FetchAndValidateResponse(_certToCheck, _issuerCert, _ocspUrl, _signDate);
            ResponseSignatureHashHex = response.SignatureHashHex;
            ResponderCertificate = response.ResponderCertificate;
            return response.EncodedResponse;
        }
        catch (System.Security.Cryptography.CryptographicException ex)
        {
            throw new CertificateVerificationException(
                $"Could not verify certificate {_certToCheck.Subject}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Retrieves the OCSP responder URL from the certificate's Authority Information Access
    /// extension (OID 1.3.6.1.5.5.7.48.1).
    /// </summary>
    /// <param name="cert">The certificate to inspect.</param>
    /// <returns>The OCSP URL, or <c>null</c> if not present.</returns>
    public static string? GetOcspUrlFromCertificate(X509Certificate2 cert)
    {
        ArgumentNullException.ThrowIfNull(cert);

        // OID 1.3.6.1.5.5.7.1.1 = id-pe-authorityInfoAccess
        X509Extension? aiaExt = cert.Extensions["1.3.6.1.5.5.7.1.1"];
        if (aiaExt == null) return null;

        // Walk the raw DER looking for the OCSP AccessDescription (OID 1.3.6.1.5.5.7.48.1).
        string formatted = aiaExt.Format(false);
        // The formatted string contains lines like:
        //   Method=Online Certificate Status Protocol (1.3.6.1.5.5.7.48.1), Location=http://...
        foreach (string part in formatted.Split(',', StringSplitOptions.TrimEntries))
        {
            if (part.StartsWith("Location=", StringComparison.OrdinalIgnoreCase))
            {
                string url = part.Substring("Location=".Length).Trim();
                if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    return url;
                }
            }
        }

        return null;
    }
}
