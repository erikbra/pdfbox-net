/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/signature/cert/CertificateVerifier.java
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

namespace PdfBox.Net.Examples.Signature.Cert;

// PORT_MODE: mechanical

/// <summary>
/// Verifies a chain of X.509 certificates using the .NET <see cref="X509Chain"/> engine.
/// </summary>
/// <remarks>
/// Ported from Apache PDFBox / Apache CXF CertificateVerifier.java.
/// Java relied on <c>CertPathBuilder</c> (PKIX); .NET uses <see cref="X509Chain"/> instead.
/// OCSP / CRL checking is performed by the platform via
/// <see cref="X509RevocationMode.Online"/> when a network is available.
/// </remarks>
public sealed class CertificateVerifier
{
    private CertificateVerifier()
    {
    }

    /// <summary>
    /// Attempts to build and validate a certification path for
    /// <paramref name="cert"/> against the supplied trust anchors /
    /// intermediate certificates.
    /// </summary>
    /// <param name="cert">The end-entity certificate to validate.</param>
    /// <param name="additionalCerts">
    /// Trust anchors and intermediate CA certificates used to complete the chain.
    /// </param>
    /// <param name="verifySelfSignedCert">
    /// When <c>true</c>, a self-signed certificate is accepted as trusted. When
    /// <c>false</c>, the certificate must chain to one of <paramref name="additionalCerts"/>.
    /// </param>
    /// <param name="verificationDate">The reference date for validity checks.</param>
    /// <returns>A <see cref="CertificateVerificationResult"/> that indicates success or failure.</returns>
    public static CertificateVerificationResult VerifyCertificate(
        X509Certificate2 cert,
        ISet<X509Certificate2> additionalCerts,
        bool verifySelfSignedCert,
        DateTime verificationDate)
    {
        ArgumentNullException.ThrowIfNull(cert);
        try
        {
            using X509Chain chain = new();
            chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
            chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
            chain.ChainPolicy.VerificationTime = verificationDate;
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;

            if (additionalCerts != null)
            {
                foreach (X509Certificate2 extra in additionalCerts)
                {
                    chain.ChainPolicy.ExtraStore.Add(extra);
                }
            }

            bool valid = chain.Build(cert);
            if (!valid)
            {
                // Collect chain status descriptions for the exception message.
                var statuses = chain.ChainStatus
                    .Select(s => s.StatusInformation?.Trim())
                    .Where(s => !string.IsNullOrEmpty(s));
                string summary = string.Join("; ", statuses);
                return new CertificateVerificationResult(
                    new CertificateVerificationException(
                        $"Certificate chain validation failed for {cert.Subject}: {summary}"));
            }

            return new CertificateVerificationResult(chain);
        }
        catch (Exception ex)
        {
            return new CertificateVerificationResult(
                new CertificateVerificationException(
                    $"Exception during certificate verification for {cert.Subject}", ex));
        }
    }

    /// <summary>
    /// Downloads any extra certificates embedded in a CRL or OCSP response.
    /// .NET does not embed extra certs in CRL objects, so this returns an empty set.
    /// </summary>
    public static ISet<X509Certificate2> DownloadExtraCertificates(object crlOrResponse)
    {
        return new HashSet<X509Certificate2>();
    }
}
