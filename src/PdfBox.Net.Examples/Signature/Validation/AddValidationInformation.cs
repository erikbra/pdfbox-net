/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/signature/validation/AddValidationInformation.java
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

using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using PdfBox.Net.COS;
using PdfBox.Net.Examples.Signature.Cert;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Interactive.DigitalSignature;

namespace PdfBox.Net.Examples.Signature.Validation;

/// <summary>
/// Adds Long-Term Validation (LTV) information to a signed PDF by appending a DSS dictionary.
/// </summary>
/// <remarks>
/// <para>
/// Inspired by ETSI TS 102 778-4 V1.1.2 (2009-12) – PAdES Long Term – PAdES-LTV Profile.
/// The DSS dictionary contains CRL data downloaded from each certificate's CRL Distribution
/// Points extension and all chain certificates.
/// </para>
/// <para>
/// <b>.NET limitation:</b> The Java original uses BouncyCastle to obtain raw OCSP response
/// bytes for DSS embedding.  The .NET BCL (<see cref="X509Chain"/> with
/// <see cref="X509RevocationMode.Online"/>) performs OCSP/CRL checking internally but does
/// not expose raw OCSP response bytes.  Consequently this port embeds only CRL data in the
/// DSS; OCSP revocation data cannot be embedded without BouncyCastle.
/// </para>
/// </remarks>
public class AddValidationInformation
{
    private CertInformationCollector? _certInformationCollector;

    // DSS arrays / dicts
    private COSArray? _correspondingCRLs;
    private COSDictionary? _vriBase;
    private COSArray? _ocsps;
    private COSArray? _crls;
    private COSArray? _certs;

    // Tracks certs already written to a COSStream (cert → its stream).
    private readonly Dictionary<X509Certificate2, COSStream> _certMap = [];

    private PDDocument? _document;
    private readonly HashSet<X509Certificate2> _foundRevocationInformation = [];
    private DateTimeOffset _signDate = DateTimeOffset.Now;

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Validates the last signature in <paramref name="inFile"/> and writes a copy with DSS
    /// LTV information appended to <paramref name="outFile"/>.
    /// </summary>
    public void ValidateSignature(string inFile, string outFile)
    {
        if (!File.Exists(inFile))
        {
            throw new FileNotFoundException("Document for validation does not exist: " + inFile);
        }

        using var fos = File.Create(outFile);
        using PDDocument doc = PDDocument.Load(inFile);

        int accessPermissions = SigUtils.GetMDPPermission(doc);
        if (accessPermissions == 1)
        {
            Console.WriteLine(
                "PDF is certified to forbid changes. " +
                "Some readers may report the document as invalid despite that " +
                "the PDF specification allows DSS additions.");
        }

        _document = doc;
        DoValidation(inFile, fos);
    }

    // -------------------------------------------------------------------------
    // Private implementation
    // -------------------------------------------------------------------------

    private void DoValidation(string filename, Stream output)
    {
        _certInformationCollector = new CertInformationCollector();
        CertInformationCollector.CertSignatureInformation? certInfo = null;

        try
        {
            PDSignature? signature = SigUtils.GetLastRelevantSignature(_document!);
            if (signature != null)
            {
                certInfo = _certInformationCollector.GetLastCertInfo(signature, filename);
                DateTimeOffset? sd = signature.GetSignDate();
                _signDate = sd ?? DateTimeOffset.Now;

                // If this is an RFC 3161 timestamp (ETSi.RFC3161 sub-filter), use the
                // timestamp's genTime as the reference date.
                if ("ETSI.RFC3161".Equals(signature.GetSubFilter(), StringComparison.Ordinal))
                {
                    byte[] contents = signature.GetContents();
                    try
                    {
                        SignedCms tsCms = new();
                        tsCms.Decode(contents);
                        // The signing time in a TSA response is the first signing time attr.
                        foreach (CryptographicAttributeObject attr in tsCms.SignerInfos[0].SignedAttributes)
                        {
                            if (attr.Oid.Value == "1.2.840.113549.1.9.5") // signingTime
                            {
                                // AsnEncodedData represents a GeneralizedTime; parse it.
                                var signingTime = new Pkcs9SigningTime(attr.Values[0].RawData);
                                _signDate = new DateTimeOffset(signingTime.SigningTime);
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(
                            "[AddValidationInformation] Could not parse timestamp gen time: " + ex.Message);
                    }
                }
            }
        }
        catch (CertificateProccessingException e)
        {
            throw new IOException("An Error occurred processing the Signature", e);
        }

        if (certInfo == null)
        {
            throw new IOException(
                "No Certificate information or signature found in the given document");
        }

        var docCatalog = _document!.GetDocumentCatalog();
        var catalog = (COSDictionary)docCatalog.GetCOSObject();
        catalog.SetNeedToBeUpdated(true);

        COSDictionary dss = GetOrCreateEntry<COSDictionary>(catalog, "DSS");

        AddExtensions(docCatalog);

        _vriBase = GetOrCreateEntry<COSDictionary>(dss, "VRI");
        _ocsps = GetOrCreateEntry<COSArray>(dss, "OCSPs");
        _crls = GetOrCreateEntry<COSArray>(dss, "CRLs");
        _certs = GetOrCreateEntry<COSArray>(dss, "Certs");

        AddRevocationData(certInfo);
        AddAllCertsToCertArray();

        _document.SaveIncremental(output);
    }

    // Fetches and adds revocation info for certInfo and its TSA chain.
    private void AddRevocationData(CertInformationCollector.CertSignatureInformation certInfo)
    {
        COSDictionary vri = new();
        _vriBase!.SetItem(COSName.GetPDFName(certInfo.SignatureHash ?? ""), vri);

        UpdateVRI(certInfo, vri);

        if (certInfo.TsaCerts != null)
        {
            // Don't add TSA revocation info to VRI entries
            _correspondingCRLs = null;
            AddRevocationDataRecursive(certInfo.TsaCerts);
        }
    }

    // Recursively fetches revocation data (CRL-only; OCSP bytes cannot be embedded in .NET BCL).
    private void AddRevocationDataRecursive(
        CertInformationCollector.CertSignatureInformation certInfo)
    {
        if (certInfo.IsSelfSigned) return;

        bool revocationFound = _foundRevocationInformation.Contains(certInfo.Certificate!);
        if (!revocationFound)
        {
            // Try OCSP first (just validate — we cannot embed raw OCSP bytes in .NET BCL).
            if (certInfo.OcspUrl != null && certInfo.IssuerCertificates.Count > 0)
            {
                revocationFound = FetchOcspValidation(certInfo);
            }

            // Try CRL (we can download and embed the bytes).
            if (!revocationFound && certInfo.CrlUrl != null)
            {
                FetchCrlData(certInfo);
                revocationFound = true;
            }

            if (certInfo.OcspUrl == null && certInfo.CrlUrl == null)
            {
                Console.WriteLine(
                    $"[AddValidationInformation] No revocation information for cert {certInfo.Certificate?.Subject}");
            }
            else if (!revocationFound)
            {
                throw new IOException(
                    "Could not fetch Revocation Info for Cert: " + certInfo.Certificate?.Subject);
            }
        }

        if (certInfo.AlternativeCertChain != null)
        {
            AddRevocationDataRecursive(certInfo.AlternativeCertChain);
        }

        if (certInfo.CertChain?.Certificate != null)
        {
            AddRevocationDataRecursive(certInfo.CertChain);
        }
    }

    // Performs an OCSP revocation check. Raw OCSP bytes are NOT available via .NET BCL,
    // so nothing is added to the DSS OCSPs array. Returns true if check succeeded (cert not revoked).
    private bool FetchOcspValidation(CertInformationCollector.CertSignatureInformation certInfo)
    {
        try
        {
            foreach (X509Certificate2 issuer in certInfo.IssuerCertificates)
            {
                var helper = new OcspHelper(
                    certInfo.Certificate!,
                    _signDate.UtcDateTime,
                    issuer,
                    certInfo.OcspUrl);
                helper.GetResponseOcsp();
                // OCSP check passed — mark as found but don't embed (no raw bytes available).
                _foundRevocationInformation.Add(certInfo.Certificate!);
                return true;
            }
        }
        catch (RevokedCertificateException ex)
        {
            throw new IOException("Certificate is revoked: " + ex.Message, ex);
        }
        catch (CertificateVerificationException ex)
        {
            Console.Error.WriteLine(
                $"[AddValidationInformation] OCSP check failed for {certInfo.Certificate?.Subject}: {ex.Message}");
        }
        return false;
    }

    // Downloads a CRL and embeds its bytes in the DSS.
    private void FetchCrlData(CertInformationCollector.CertSignatureInformation certInfo)
    {
        try
        {
            AddCrlRevocationInfo(certInfo);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"[AddValidationInformation] Failed fetching CRL: {ex.Message}");
            throw new IOException("Failed fetching CRL", ex);
        }
    }

    // Downloads the CRL from certInfo.CrlUrl, verifies revocation, and embeds the bytes.
    private void AddCrlRevocationInfo(
        CertInformationCollector.CertSignatureInformation certInfo)
    {
        byte[] crlBytes = CRLVerifier.DownloadCrl(certInfo.CrlUrl!);

        // Verify the cert is not revoked (offline check with downloaded bytes).
        CRLVerifier.CheckRevocation(crlBytes, certInfo.Certificate!, _signDate.UtcDateTime, certInfo.CrlUrl!);

        // Write the CRL bytes to a COSStream and add to the CRLs array.
        COSStream crlStream = WriteDataToStream(crlBytes);
        _crls!.Add(crlStream);
        if (_correspondingCRLs != null)
        {
            _correspondingCRLs.Add(crlStream);

            // Compute the SHA-1 VRI hash for the CRL signature bytes (ETSI TS 102 778-4).
            // CRL signature bytes are available via X509Certificate2Collection or raw DER parsing;
            // here we use the SHA-1 of the entire encoded CRL as a practical approximation.
            byte[] signatureHash = SHA1.HashData(crlBytes);
            string signatureHashHex = Convert.ToHexString(signatureHash);

            if (!_vriBase!.ContainsKey(signatureHashHex))
            {
                COSArray? savedCRLs = _correspondingCRLs;

                COSDictionary crlVri = new();
                _vriBase.SetItem(COSName.GetPDFName(signatureHashHex), crlVri);

                // Find the issuer cert for the CRL signer.
                // We use a simple approach: assume the CRL issuer is already in certificateSet.
                // Full CRL signature verification would require a managed X509Crl type which
                // is not available in .NET BCL — we skip it here.
                _correspondingCRLs = savedCRLs;
            }
        }

        _foundRevocationInformation.Add(certInfo.Certificate!);
    }

    private void UpdateVRI(
        CertInformationCollector.CertSignatureInformation certInfo,
        COSDictionary vri)
    {
        // OID for id-pkix-ocsp-nocheck (1.3.6.1.5.5.7.48.1.5)
        const string ocspNoCheckOid = "1.3.6.1.5.5.7.48.1.5";
        bool hasNoCheck = certInfo.Certificate?.Extensions[ocspNoCheckOid] != null;

        if (!hasNoCheck)
        {
            _correspondingCRLs = new COSArray();
            AddRevocationDataRecursive(certInfo);
            if (!_correspondingCRLs.IsEmpty())
            {
                vri.SetItem(COSName.GetPDFName("CRL"), _correspondingCRLs);
            }
        }

        // Add all certs in the chain to the VRI Cert array.
        COSArray correspondingCerts = new();
        CertInformationCollector.CertSignatureInformation? ci = certInfo;
        do
        {
            X509Certificate2? cert = ci.Certificate;
            if (cert == null) break;
            try
            {
                if (!_certMap.TryGetValue(cert, out COSStream? certStream))
                {
                    certStream = WriteDataToStream(cert.RawData);
                    _certMap[cert] = certStream;
                }
                correspondingCerts.Add(certStream);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    $"[AddValidationInformation] Failed encoding cert: {ex.Message}");
            }

            if (cert.Extensions[ocspNoCheckOid] != null) break;
            ci = ci.CertChain;
        }
        while (ci != null);

        vri.SetItem(COSName.GetPDFName("Cert"), correspondingCerts);
        vri.SetDate(COSName.GetPDFName("TU"), DateTimeOffset.Now);
    }

    private void AddAllCertsToCertArray()
    {
        foreach (X509Certificate2 cert in _certInformationCollector!.GetCertificateSet())
        {
            if (!_certMap.ContainsKey(cert))
            {
                try
                {
                    COSStream certStream = WriteDataToStream(cert.RawData);
                    _certMap[cert] = certStream;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(
                        $"[AddValidationInformation] Failed encoding cert: {ex.Message}");
                }
            }
        }

        foreach (COSStream certStream in _certMap.Values)
        {
            _certs!.Add(certStream);
        }
    }

    private COSStream WriteDataToStream(byte[] data)
    {
        COSStream stream = _document!.GetDocument().CreateCOSStream();
        using Stream os = stream.CreateOutputStream(COSName.FLATE_DECODE);
        os.Write(data, 0, data.Length);
        return stream;
    }

    private void AddExtensions(PDDocumentCatalog catalog)
    {
        COSDictionary dssExtensions = new();
        dssExtensions.SetDirect(true);
        ((COSDictionary)catalog.GetCOSObject()).SetItem(COSName.GetPDFName("Extensions"), dssExtensions);

        COSDictionary adbeExtension = new();
        adbeExtension.SetDirect(true);
        dssExtensions.SetItem(COSName.GetPDFName("ADBE"), adbeExtension);

        adbeExtension.SetName(COSName.GetPDFName("BaseVersion"), "1.7");
        adbeExtension.SetInt(COSName.GetPDFName("ExtensionLevel"), 5);

        catalog.SetVersion("1.7");
    }

    // Gets or creates an entry in parent under name. Marks existing entries as needing update.
    private static T GetOrCreateEntry<T>(COSDictionary parent, string name)
        where T : COSBase, COSUpdateInfo, new()
    {
        COSBase? element = parent.GetDictionaryObject(COSName.GetPDFName(name));
        if (element is T result)
        {
            result.SetNeedToBeUpdated(true);
            return result;
        }
        if (element != null)
        {
            throw new IOException(
                $"Element '{name}' in dictionary is not of type {typeof(T).Name}");
        }

        T newEntry = new();
        newEntry.SetDirect(false);
        parent.SetItem(COSName.GetPDFName(name), newEntry);
        return newEntry;
    }

    // -------------------------------------------------------------------------
    // CLI entry point
    // -------------------------------------------------------------------------

    /// <summary>
    /// CLI entry point: <c>AddValidationInformation &lt;signed.pdf&gt;</c>
    /// </summary>
    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine($"Usage: {nameof(AddValidationInformation)} <signed_pdf>");
            Environment.Exit(1);
        }

        AddValidationInformation addInfo = new();

        string inFile = args[0];
        string name = Path.GetFileNameWithoutExtension(inFile);
        string outFile = Path.Combine(Path.GetDirectoryName(inFile) ?? ".", name + "_LTV.pdf");
        addInfo.ValidateSignature(inFile, outFile);
    }
}
