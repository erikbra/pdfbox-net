/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/signature/CreateSignatureBase.java
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
using PdfBox.Net.PDModel.Interactive.DigitalSignature;

namespace PdfBox.Net.Examples.Signature;

/// <summary>
/// Base class for creating PDF signatures.
/// Loads a PKCS#12 keystore and implements <see cref="SignatureInterface"/> using
/// <see cref="SignedCms"/> (detached CMS / PKCS#7 signature).
/// </summary>
public class CreateSignatureBase : SignatureInterface
{
    private X509Certificate2? _certificate;
    private X509Certificate2Collection _certificateChain = [];
    private TSAClient? _tsaClient;
    private bool _externalSigning;

    /// <summary>
    /// Loads the signing certificate and chain from a PKCS#12 file.
    /// </summary>
    public void SetKeystore(string keystorePath, string password)
    {
        ArgumentNullException.ThrowIfNull(keystorePath);
        X509Certificate2Collection col =
            X509CertificateLoader.LoadPkcs12CollectionFromFile(
                keystorePath, password, X509KeyStorageFlags.EphemeralKeySet);
        InitFromCollection(col);
    }

    /// <summary>
    /// Loads the signing certificate and chain from a PKCS#12 byte array.
    /// </summary>
    public void SetKeystore(byte[] pfxData, string password)
    {
        ArgumentNullException.ThrowIfNull(pfxData);
        X509Certificate2Collection col =
            X509CertificateLoader.LoadPkcs12Collection(
                pfxData, password, X509KeyStorageFlags.EphemeralKeySet);
        InitFromCollection(col);
    }

    private void InitFromCollection(X509Certificate2Collection col)
    {
        // Find the certificate that has a private key.
        foreach (X509Certificate2 cert in col)
        {
            if (cert.HasPrivateKey)
            {
                _certificate = cert;
                break;
            }
        }

        if (_certificate == null)
        {
            throw new InvalidOperationException(
                "No certificate with a private key found in the keystore.");
        }

        _certificateChain = col;
    }

    /// <summary>Sets an optional TSA client for embedding a trusted timestamp.</summary>
    public void SetTsaClient(TSAClient? tsaClient) => _tsaClient = tsaClient;

    /// <summary>
    /// Gets or sets whether to use external signing (caller handles the actual signing step).
    /// </summary>
    public bool IsExternalSigning { get => _externalSigning; set => _externalSigning = value; }

    /// <summary>Returns the signing certificate.</summary>
    public X509Certificate2? GetCertificate() => _certificate;

    /// <summary>Returns the full certificate chain loaded from the keystore.</summary>
    public X509Certificate2Collection GetCertificateChain() => _certificateChain;

    /// <summary>
    /// Computes a detached CMS signature over <paramref name="content"/> and returns the
    /// DER-encoded <c>SignedData</c> bytes.
    /// </summary>
    public virtual byte[] Sign(Stream content)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (_certificate == null)
        {
            throw new InvalidOperationException(
                "A keystore must be loaded before signing. Call SetKeystore first.");
        }

        // Buffer the content for signing.
        byte[] contentBytes;
        using (var ms = new MemoryStream())
        {
            content.CopyTo(ms);
            contentBytes = ms.ToArray();
        }

        // Build a detached CMS signature.
        ContentInfo contentInfo = new(contentBytes);
        SignedCms signedCms = new(contentInfo, detached: true);

        CmsSigner signer = new(_certificate)
        {
            DigestAlgorithm = new Oid("2.16.840.1.101.3.4.2.1"), // SHA-256
            IncludeOption = X509IncludeOption.WholeChain,
        };

        // Add any additional chain certificates.
        foreach (X509Certificate2 chainCert in _certificateChain)
        {
            if (!chainCert.Equals(_certificate))
            {
                signer.Certificates.Add(chainCert);
            }
        }

        signedCms.ComputeSignature(signer);
        byte[] signedData = signedCms.Encode();

        // Optionally extend with a TSA timestamp.
        if (_tsaClient != null)
        {
            signedData = _tsaClient.AddTimestamp(signedData);
        }

        return signedData;
    }
}
