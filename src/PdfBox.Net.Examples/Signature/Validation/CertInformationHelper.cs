/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/signature/validation/CertInformationHelper.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
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
using System.Security.Cryptography;

namespace PdfBox.Net.Examples.Signature.Validation;

/// <summary>
/// Helper methods for certificate information retrieval.
/// </summary>
/// <remarks>
/// Ported from the Java example.  BouncyCastle ASN.1 parsing is replaced with
/// <see cref="System.Formats.Asn1.AsnDecoder"/>.
/// </remarks>
public class CertInformationHelper
{
    private CertInformationHelper()
    {
    }

    /// <summary>
    /// Computes the SHA-1 hash of <paramref name="content"/> and returns it as an
    /// upper-case hex string.
    /// </summary>
    internal static string? GetSha1Hash(byte[] content)
    {
        try
        {
            byte[] hash = SHA1.HashData(content);
            return Convert.ToHexString(hash);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("[CertInformationHelper] No SHA-1 Algorithm found: " + ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Parses the Authority Information Access extension value and populates
    /// <paramref name="certInfo"/> with the OCSP URL and issuer URL.
    /// </summary>
    /// <param name="rawExtensionValue">
    /// The raw DER bytes of the AIA extension value as returned by
    /// <c>cert.Extensions[oid].RawData</c>.  These bytes are the DER-encoded
    /// <c>AuthorityInfoAccessSyntax</c> SEQUENCE (without the OCTET STRING wrapper
    /// that .NET strips when reading from an <see cref="System.Security.Cryptography.X509Certificates.X509Certificate2"/>).
    /// </param>
    /// <param name="certInfo">The <see cref="CertInformationCollector.CertSignatureInformation"/> to update.</param>
    internal static void GetAuthorityInfoExtensionValue(
        byte[] rawExtensionValue,
        CertInformationCollector.CertSignatureInformation certInfo)
    {
        try
        {
            // AuthorityInfoAccessSyntax ::= SEQUENCE SIZE (1..MAX) OF AccessDescription
            ReadOnlySpan<byte> data = rawExtensionValue;
            AsnDecoder.ReadSequence(data, AsnEncodingRules.DER,
                out int seqOff, out int seqLen, out _);
            ReadOnlySpan<byte> seq = data.Slice(seqOff, seqLen);

            while (!seq.IsEmpty)
            {
                // AccessDescription ::= SEQUENCE { accessMethod OID, accessLocation GeneralName }
                AsnDecoder.ReadSequence(seq, AsnEncodingRules.DER,
                    out int adOff, out int adLen, out int adConsumed);
                ReadOnlySpan<byte> ad = seq.Slice(adOff, adLen);
                seq = seq.Slice(adConsumed);

                // accessMethod OID
                string oid = AsnDecoder.ReadObjectIdentifier(ad, AsnEncodingRules.DER,
                    out int oidConsumed);
                ReadOnlySpan<byte> locationBytes = ad.Slice(oidConsumed);

                // accessLocation: GeneralName [6] IMPLICIT IA5String = uniformResourceIdentifier
                if (locationBytes.IsEmpty || (locationBytes[0] & 0x1F) != 6) continue;

                bool read = AsnDecoder.TryReadPrimitiveOctetString(
                    locationBytes, AsnEncodingRules.BER,
                    out ReadOnlySpan<byte> urlBytes, out _,
                    new Asn1Tag(TagClass.ContextSpecific, 6));
                if (!read) continue;

                string url = System.Text.Encoding.ASCII.GetString(urlBytes);

                // id-ad-ocsp  1.3.6.1.5.5.7.48.1
                if (oid == "1.3.6.1.5.5.7.48.1")
                {
                    certInfo.OcspUrl = url;
                }
                // id-ad-caIssuers  1.3.6.1.5.5.7.48.2
                else if (oid == "1.3.6.1.5.5.7.48.2")
                {
                    certInfo.IssuerUrl = url;
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                "[CertInformationHelper] Failed to parse AIA extension: " + ex.Message);
        }
    }

    /// <summary>
    /// Extracts the first HTTP/HTTPS CRL URL from the CRL Distribution Points extension value.
    /// </summary>
    /// <param name="rawExtensionValue">
    /// The raw DER bytes of the CRL DP extension (<c>cert.Extensions["2.5.29.31"].RawData</c>).
    /// </param>
    /// <returns>The first HTTP(S) CRL URL found, or <c>null</c> if none.</returns>
    internal static string? GetCrlUrlFromExtensionValue(byte[] rawExtensionValue)
    {
        try
        {
            // CRLDistributionPoints ::= SEQUENCE OF DistributionPoint
            ReadOnlySpan<byte> data = rawExtensionValue;
            AsnDecoder.ReadSequence(data, AsnEncodingRules.DER,
                out int outerOff, out int outerLen, out _);
            ReadOnlySpan<byte> dpList = data.Slice(outerOff, outerLen);

            while (!dpList.IsEmpty)
            {
                // DistributionPoint ::= SEQUENCE
                AsnDecoder.ReadSequence(dpList, AsnEncodingRules.DER,
                    out int dpOff, out int dpLen, out int dpConsumed);
                ReadOnlySpan<byte> dp = dpList.Slice(dpOff, dpLen);
                dpList = dpList.Slice(dpConsumed);

                // distributionPoint is [0] EXPLICIT DistributionPointName
                if (dp.IsEmpty || (dp[0] & 0xFF) != 0xA0) continue;

                AsnDecoder.ReadSequence(dp, AsnEncodingRules.BER,
                    out int dpnOff, out int dpnLen, out _,
                    new Asn1Tag(TagClass.ContextSpecific, 0, isConstructed: true));
                ReadOnlySpan<byte> dpn = dp.Slice(dpnOff, dpnLen);

                // fullName is [0] IMPLICIT SEQUENCE OF GeneralName
                if (dpn.IsEmpty || (dpn[0] & 0xFF) != 0xA0) continue;

                AsnDecoder.ReadSequence(dpn, AsnEncodingRules.BER,
                    out int gnOff, out int gnLen, out _,
                    new Asn1Tag(TagClass.ContextSpecific, 0, isConstructed: true));
                ReadOnlySpan<byte> generalNames = dpn.Slice(gnOff, gnLen);

                while (!generalNames.IsEmpty)
                {
                    if ((generalNames[0] & 0xFF) == 0x86) // [6] uniformResourceIdentifier
                    {
                        bool ok = AsnDecoder.TryReadPrimitiveOctetString(
                            generalNames, AsnEncodingRules.BER,
                            out ReadOnlySpan<byte> uriBytes, out int consumed,
                            new Asn1Tag(TagClass.ContextSpecific, 6));
                        if (ok)
                        {
                            string url = System.Text.Encoding.ASCII.GetString(uriBytes);
                            if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                            {
                                return url;
                            }
                            generalNames = generalNames.Slice(consumed);
                        }
                        else break;
                    }
                    else
                    {
                        if (generalNames.Length < 2) break;
                        int len = generalNames[1];
                        int headerLen = 2;
                        if ((len & 0x80) != 0)
                        {
                            int numLenBytes = len & 0x7F;
                            len = 0;
                            for (int i = 0; i < numLenBytes; i++)
                                len = (len << 8) | (generalNames[2 + i] & 0xFF);
                            headerLen = 2 + numLenBytes;
                        }
                        generalNames = generalNames.Slice(headerLen + len);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                "[CertInformationHelper] Failed to parse CRL DP extension: " + ex.Message);
        }

        return null;
    }
}
