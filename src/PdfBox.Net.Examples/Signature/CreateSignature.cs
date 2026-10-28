/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/signature/CreateSignature.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc
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

using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Interactive.DigitalSignature;

namespace PdfBox.Net.Examples.Signature;

/// <summary>
/// Creates a PDF signature using a PKCS#12 keystore.
/// Demonstrates basic detached CMS signing via
/// <see cref="System.Security.Cryptography.Pkcs.SignedCms"/>.
/// </summary>
public class CreateSignature : CreateSignatureBase
{
    private bool _ltvEnabled;

    /// <summary>
    /// Initialises the signer from a PKCS#12 file.
    /// </summary>
    public CreateSignature(string keystorePath, string password)
    {
        SetKeystore(keystorePath, password);
    }

    /// <summary>
    /// Initialises the signer from a PKCS#12 byte array.
    /// </summary>
    public CreateSignature(byte[] pfxData, string password)
    {
        SetKeystore(pfxData, password);
    }

    /// <summary>Gets or sets whether to embed LTV validation information.</summary>
    public bool IsLtvEnabled { get => _ltvEnabled; set => _ltvEnabled = value; }

    /// <summary>
    /// Signs the PDF from <paramref name="inputPdfPath"/> and writes the signed version to
    /// <paramref name="outputPdfPath"/>.
    /// </summary>
    public void SignDetached(string inputPdfPath, string outputPdfPath)
    {
        using FileStream input = File.OpenRead(inputPdfPath);
        using FileStream output = File.Create(outputPdfPath);
        SignDetached(input, output);
    }

    /// <summary>Signs a PDF, optionally adding a TSA timestamp.</summary>
    public void SignDetached(string inputPdfPath, string outputPdfPath, string? tsaUrl)
        => SignDetached(inputPdfPath, outputPdfPath, tsaUrl, password: null);

    /// <summary>Signs a PDF, optionally adding a TSA timestamp, using its opening password.</summary>
    public void SignDetached(string inputPdfPath, string outputPdfPath, string? tsaUrl, string? password)
    {
        using FileStream input = File.OpenRead(inputPdfPath);
        using FileStream output = File.Create(outputPdfPath);
        SignDetached(input, output, tsaUrl is null ? null : new TSAClient(tsaUrl), password);
    }

    /// <summary>Signs the PDF read from <paramref name="inputPdf"/> and writes to <paramref name="outputPdf"/>.</summary>
    public void SignDetached(Stream inputPdf, Stream outputPdf)
    {
        SignDetached(inputPdf, outputPdf, tsaClient: null);
    }

    /// <summary>
    /// Signs the PDF read from <paramref name="inputPdf"/>, optionally embedding a trusted
    /// timestamp from <paramref name="tsaClient"/>, and writes to <paramref name="outputPdf"/>.
    /// </summary>
    public void SignDetached(Stream inputPdf, Stream outputPdf, TSAClient? tsaClient)
        => SignDetached(inputPdf, outputPdf, tsaClient, password: null);

    /// <summary>Signs a PDF opened using the supplied PDF password.</summary>
    public void SignDetached(Stream inputPdf, Stream outputPdf, TSAClient? tsaClient, string? password)
    {
        SetTsaClient(tsaClient);

        using PDDocument doc = PDDocument.Load(inputPdf, password);

        PDSignature signature = new();
        signature.SetFilter(PDSignature.FILTER_ADOBE_PPKLITE);
        signature.SetSubFilter(PDSignature.SUBFILTER_ADBE_PKCS7_DETACHED);
        signature.SetName(GetCertificate()?.GetNameInfo(
            System.Security.Cryptography.X509Certificates.X509NameType.SimpleName, false));
        signature.SetSignDate(DateTimeOffset.UtcNow);

        SignatureOptions options = new();

        if (IsExternalSigning)
        {
            doc.AddSignature(signature, options);
            ExternalSigningSupport externalSigning = doc.SaveIncrementalForExternalSigning(outputPdf);
            byte[] signatureBytes = Sign(externalSigning.GetContent());
            externalSigning.SetSignature(signatureBytes);
        }
        else
        {
            doc.AddSignature(signature, this, options);
            doc.SaveIncremental(outputPdf);
        }
    }

    /// <summary>
    /// Entry point: signs a PDF.
    /// Usage: <c>CreateSignature &lt;keystore.p12&gt; &lt;password&gt; &lt;input.pdf&gt; &lt;output.pdf&gt;</c>
    /// </summary>
    public static void Main(string[] args)
    {
        if (args.Length < 4)
        {
            Console.Error.WriteLine(
                "Usage: CreateSignature <keystore.p12> <password> <input.pdf> <output.pdf>");
            Environment.Exit(1);
        }

        string keystorePath = args[0];
        string password = args[1];
        string inputPdf = args[2];
        string outputPdf = args[3];

        CreateSignature signing = new(keystorePath, password);
        signing.SignDetached(inputPdf, outputPdf);
        Console.WriteLine($"Signed PDF written to: {outputPdf}");
    }
}
