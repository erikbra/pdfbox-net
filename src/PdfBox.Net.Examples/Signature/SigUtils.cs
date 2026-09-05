/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/signature/SigUtils.java
 * PDFBOX_SOURCE_COMMIT: 1187c45f9dcee38ed5ac12bc15df04913b348875
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 046747da99a870902217efabf1c41297de157059
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

using Microsoft.Extensions.Logging;
using PdfBox.Net.Logging;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Interactive.DigitalSignature;

namespace PdfBox.Net.Examples.Signature;

/// <summary>
/// Utility methods for working with PDF digital signatures.
/// </summary>
public sealed class SigUtils
{
    private static ILogger<SigUtils> LOG => PdfBoxLogging.CreateLogger<SigUtils>();
    private static readonly HttpClient HttpClient = new(new HttpClientHandler
    {
        AllowAutoRedirect = false
    });

    // Certificates / CRLs / LDAP needed for the upstream unit tests; add yours
    // or create your own logic in CheckAccess().
    private static readonly HashSet<string> AllowUrlSet = new(StringComparer.Ordinal)
    {
        "http://www.pki.admin.ch/aia/RegularCA01.crt",
        "http://www.pki.admin.ch/aia/RootCAII.crt",
        "http://www.pki.admin.ch/aia/RootCAIV.crt",
        "http://www.pki.admin.ch/crl/RegularCA01.crl",
        "http://www.pki.admin.ch/crl/RootCAII.crl",
        "http://www.pki.admin.ch/aia/RegulatedCA02.crt",
        "http://www.pki.admin.ch/aia/ocsp",
        "http://www.pki.admin.ch/crl/RegulatedCA02.crl",
        "http://repository.certum.pl/ctnca2.cer",
        "http://repository.certum.pl/ctnca.cer",
        "http://subca.repository.certum.pl/ctsca2021.cer",
        "http://crl.geotrust.com/crls/adobeca1.crl",
        "http://crl.adobe.com/cds.crl",
        "http://subca.crl.certum.pl/ctsca2021.crl",
        "http://subca.ocsp-certum.com",
        "http://crl.certum.pl/ctnca2.crl",
        "http://www.freetsa.org/tsa.crt",
        "http://www.freetsa.org:2560",
        "http://www.freetsa.org/crl/root_ca.crl",
        "http://www.gemboxsoftware.com/test/pki/cert/GemBoxCA.crt",
        "http://www.gemboxsoftware.com/test/pki/cert/GemBoxRSA.crt",
        "http://www.ca.gov.si/crt/si-trust-root.crt",
        "ldap://x500.gov.si/cn=SI-TRUST%20Root,oi=VATSI-17659957,o=Republika%20Slovenija,c=SI?certificateRevocationList"
    };

    private SigUtils()
    {
    }

    /// <summary>MDP (certify) permission levels.</summary>
    public const int MDPPermissionNoChanges = 1;
    public const int MDPPermissionFillForms = 2;
    public const int MDPPermissionAnnotations = 3;

    /// <summary>
    /// Returns the MDP (DocMDP) permission level of the first certifying signature in
    /// <paramref name="doc"/>, or 0 if none is present.
    /// </summary>
    public static int GetMDPPermission(PDDocument doc)
    {
        ArgumentNullException.ThrowIfNull(doc);
        foreach (PDSignature sig in doc.GetSignatureDictionaries())
        {
            COSBase? cosObj = ((COSDictionary)sig.GetCOSObject())
                .GetDictionaryObject(COSName.GetPDFName("Reference"));
            if (cosObj is not COSArray refArray || refArray.IsEmpty())
            {
                continue;
            }

            for (int i = 0; i < refArray.Size(); i++)
            {
                if (refArray.GetObject(i) is not COSDictionary transformDict)
                {
                    continue;
                }

                string? transformMethod = transformDict.GetNameAsString(
                    COSName.GetPDFName("TransformMethod"));
                if ("DocMDP".Equals(transformMethod, StringComparison.Ordinal))
                {
                    COSDictionary? transformParams = transformDict.GetCOSDictionary(
                        COSName.GetPDFName("TransformParams"));
                    if (transformParams != null)
                    {
                        int p = transformParams.GetInt(COSName.GetPDFName("P"), 2);
                        return p;
                    }
                }
            }
        }

        return 0;
    }

    /// <summary>
    /// Embeds a DocMDP transform reference in <paramref name="signature"/> that certifies
    /// the document with the given <paramref name="accessPermissions"/> level (1–3).
    /// </summary>
    public static void SetMDPPermission(
        PDDocument doc,
        PDSignature signature,
        int accessPermissions)
    {
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(signature);

        // Build the TransformParams dictionary.
        COSDictionary transformParams = new();
        transformParams.SetItem(COSName.TYPE, COSName.GetPDFName("TransformParams"));
        transformParams.SetItem(COSName.GetPDFName("P"), COSInteger.Get(accessPermissions));
        transformParams.SetItem(COSName.V, COSName.GetPDFName("1.2"));
        transformParams.SetDirect(true);

        // Build the Reference array entry.
        COSDictionary sigRef = new();
        sigRef.SetItem(COSName.TYPE, COSName.GetPDFName("SigRef"));
        sigRef.SetItem(COSName.GetPDFName("TransformMethod"), COSName.GetPDFName("DocMDP"));
        sigRef.SetItem(COSName.GetPDFName("DigestMethod"), COSName.GetPDFName("SHA256"));
        sigRef.SetItem(COSName.GetPDFName("TransformParams"), transformParams);
        sigRef.SetDirect(true);

        COSArray referenceArray = new();
        referenceArray.Add(sigRef);
        referenceArray.SetDirect(true);

        ((COSDictionary)signature.GetCOSObject()).SetItem(
            COSName.GetPDFName("Reference"), referenceArray);

        // Lock the document permissions entry in the catalog.
        COSDictionary catalogDict = (COSDictionary)doc.GetDocumentCatalog().GetCOSObject();
        COSDictionary perms = catalogDict.GetCOSDictionary(COSName.GetPDFName("Perms"))
            ?? new COSDictionary();
        perms.SetItem(COSName.GetPDFName("DocMDP"), signature);
        perms.SetDirect(true);
        catalogDict.SetItem(COSName.GetPDFName("Perms"), perms);
    }

    /// <summary>
    /// Checks that the key usage extension of <paramref name="certificate"/> includes the
    /// usage flags required for PDF non-repudiation signing.
    /// Throws <see cref="InvalidOperationException"/> if the certificate is not suitable.
    /// </summary>
    public static void CheckCertificateUsage(X509Certificate2 certificate)
    {
        ArgumentNullException.ThrowIfNull(certificate);

        // Check that the certificate has not expired.
        DateTimeOffset now = DateTimeOffset.UtcNow;
        if (now < certificate.NotBefore || now > certificate.NotAfter)
        {
            throw new InvalidOperationException(
                $"Certificate '{certificate.Subject}' is not within its validity period.");
        }

        // Check Key Usage (OID 2.5.29.15).
        X509KeyUsageExtension? keyUsage = certificate.Extensions
            .OfType<X509KeyUsageExtension>()
            .FirstOrDefault();
        if (keyUsage != null)
        {
            bool hasDigitalSignature =
                (keyUsage.KeyUsages & X509KeyUsageFlags.DigitalSignature) != 0 ||
                (keyUsage.KeyUsages & X509KeyUsageFlags.NonRepudiation) != 0;

            if (!hasDigitalSignature)
            {
                LOG.LogError(
                    "Certificate key usage does not include digitalSignature nor nonRepudiation");
                throw new InvalidOperationException(
                    $"Certificate '{certificate.Subject}' does not have the DigitalSignature " +
                    "or NonRepudiation key usage flag required for PDF signing.");
            }
        }

        X509EnhancedKeyUsageExtension? extendedKeyUsage = certificate.Extensions
            .OfType<X509EnhancedKeyUsageExtension>()
            .FirstOrDefault();
        if (extendedKeyUsage != null)
        {
            HashSet<string?> usages = extendedKeyUsage.EnhancedKeyUsages
                .Cast<Oid>()
                .Select(static oid => oid.Value)
                .ToHashSet(StringComparer.Ordinal);
            if (!usages.Contains("1.3.6.1.5.5.7.3.4") &&
                !usages.Contains("1.3.6.1.5.5.7.3.3") &&
                !usages.Contains("2.5.29.37.0") &&
                !usages.Contains("1.2.840.113583.1.1.5") &&
                !usages.Contains("1.3.6.1.4.1.311.10.3.12"))
            {
                LOG.LogError(
                    "Certificate extended key usage does not include emailProtection, nor codeSigning, nor anyExtendedKeyUsage, nor 'Adobe Authentic Documents Trust'");
            }
        }
    }

    /// <summary>
    /// Returns the most recently applied, document-level signature from <paramref name="doc"/>
    /// that covers the entire original file (i.e., ByteRange starts at 0).
    /// </summary>
    public static PDSignature? GetLastRelevantSignature(PDDocument doc)
    {
        ArgumentNullException.ThrowIfNull(doc);
        PDSignature? last = null;
        int lastByteRange1 = -1;

        foreach (PDSignature sig in doc.GetSignatureDictionaries())
        {
            int[] range = sig.GetByteRange();
            if (range.Length >= 2 && range[0] == 0 && range[1] > lastByteRange1)
            {
                lastByteRange1 = range[1];
                last = sig;
            }
        }

        return last;
    }

    /// <summary>
    /// A simple but very restrictive access control logic. Create your own using a zero-trust mindset.
    /// </summary>
    /// <param name="uri">The URI to check against the example allowlist.</param>
    /// <exception cref="IOException">The URI is not in the allowlist.</exception>
    public static void CheckAccess(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        if (!AllowUrlSet.Contains(uri.OriginalString))
        {
            throw new IOException($"URL '{uri.OriginalString}' not in allowUrlSet");
        }
    }

    /// <summary>
    /// Like opening a URL stream, but follows redirection from HTTP to HTTPS.
    /// </summary>
    /// <param name="urlString">HTTP URL string.</param>
    /// <returns>A readable stream containing the URL response body.</returns>
    /// <exception cref="IOException">If the scheme is not HTTP(S) or the request fails.</exception>
    public static Stream OpenURL(string urlString) => OpenURL(urlString, HttpClient);

    internal static Stream OpenURL(string urlString, HttpClient client)
    {
        ArgumentNullException.ThrowIfNull(urlString);

        if (!Uri.TryCreate(urlString, UriKind.Absolute, out Uri? url))
        {
            throw new IOException($"Invalid URL: {urlString}");
        }

        if (!string.Equals(url.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(url.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new IOException(url.Scheme + " protocol not supported");
        }

        CheckAccess(url);

        try
        {
            using HttpResponseMessage response = client.GetAsync(url).GetAwaiter().GetResult();
            LOG.LogInformation("{StatusCode} {ReasonPhrase}", (int)response.StatusCode, response.ReasonPhrase);
            if (response.StatusCode is System.Net.HttpStatusCode.MovedPermanently or
                System.Net.HttpStatusCode.Found or System.Net.HttpStatusCode.SeeOther &&
                response.Headers.Location is Uri location &&
                urlString.StartsWith("http://", StringComparison.Ordinal) &&
                location.OriginalString.StartsWith("https://", StringComparison.Ordinal) &&
                urlString[7..] == location.OriginalString[8..])
            {
                LOG.LogInformation("redirection to {Location} followed", location);
                using HttpResponseMessage redirected = client.GetAsync(location).GetAwaiter().GetResult();
                return ReadResponse(redirected);
            }

            if (response.Headers.Location is Uri ignoredLocation)
            {
                LOG.LogInformation("redirection to {Location} ignored", ignoredLocation);
            }
            return ReadResponse(response);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new IOException("Could not open URL: " + url, ex);
        }
    }

    private static Stream ReadResponse(HttpResponseMessage response)
    {
        if ((int)response.StatusCode >= 400)
        {
            response.EnsureSuccessStatusCode();
        }
        byte[] data = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
        return new MemoryStream(data, writable: false);
    }
}
