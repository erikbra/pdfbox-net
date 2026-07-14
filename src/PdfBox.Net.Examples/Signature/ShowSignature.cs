/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/signature/ShowSignature.java
 * PDFBOX_SOURCE_COMMIT: 10950c29006e36cfba48e74d4031784e31562cbf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 10950c29006e36cfba48e74d4031784e31562cbf
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
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Interactive.DigitalSignature;

namespace PdfBox.Net.Examples.Signature;

/// <summary>
/// Shows information about PDF signatures and optionally verifies them.
/// Demonstrates use of <see cref="PDDocument.GetSignatureDictionaries"/> and
/// <see cref="SignedCms"/> for CMS / PKCS#7 verification.
/// </summary>
public class ShowSignature
{
    /// <summary>
    /// Displays information about all signatures in the PDF at <paramref name="pdfPath"/>.
    /// </summary>
    public static void Show(string pdfPath)
    {
        ArgumentNullException.ThrowIfNull(pdfPath);
        byte[] pdfBytes = File.ReadAllBytes(pdfPath);

        using PDDocument doc = PDDocument.Load(pdfPath);
        List<PDSignature> signatures = doc.GetSignatureDictionaries();

        if (signatures.Count == 0)
        {
            Console.WriteLine("No signatures found in: " + pdfPath);
            return;
        }

        Console.WriteLine($"Found {signatures.Count} signature(s) in: {pdfPath}");
        foreach (PDSignature sig in signatures)
        {
            ShowSignatureInfo(sig, pdfBytes);
        }
    }

    private static void ShowSignatureInfo(PDSignature sig, byte[] pdfBytes)
    {
        Console.WriteLine("----------------------------------------------------");
        Console.WriteLine("Filter:       " + (sig.GetFilter() ?? "(none)"));
        Console.WriteLine("SubFilter:    " + (sig.GetSubFilter() ?? "(none)"));
        Console.WriteLine("Name:         " + (sig.GetName() ?? "(none)"));
        Console.WriteLine("Sign date:    " + sig.GetSignDate()?.ToString("u") ?? "(none)");
        Console.WriteLine("Location:     " + (sig.GetLocation() ?? "(none)"));
        Console.WriteLine("Reason:       " + (sig.GetReason() ?? "(none)"));
        Console.WriteLine("Contact:      " + (sig.GetContactInfo() ?? "(none)"));

        byte[] contents = sig.GetContents();
        if (contents.Length == 0)
        {
            Console.WriteLine("Contents: (empty)");
            return;
        }

        string? subFilter = sig.GetSubFilter();
        bool isCms = subFilter != null && (
            subFilter.Equals("adbe.pkcs7.detached", StringComparison.OrdinalIgnoreCase) ||
            subFilter.Equals("adbe.pkcs7.sha1", StringComparison.OrdinalIgnoreCase) ||
            subFilter.Equals("ETSI.CAdES.detached", StringComparison.OrdinalIgnoreCase));

        if (!isCms)
        {
            Console.WriteLine("Contents: (non-CMS signature, cannot decode)");
            return;
        }

        try
        {
            byte[] signedContent = sig.GetSignedContent(pdfBytes);
            ContentInfo contentInfo = new(signedContent);
            SignedCms signedCms = new(contentInfo, detached: true);
            signedCms.Decode(contents);

            Console.WriteLine("Signer count: " + signedCms.SignerInfos.Count);
            foreach (SignerInfo si in signedCms.SignerInfos)
            {
                X509Certificate2? cert = si.Certificate;
                Console.WriteLine("  Signer: " + (cert?.Subject ?? "(no certificate)"));
                Console.WriteLine("  Digest algorithm: " + si.DigestAlgorithm.FriendlyName);

                try
                {
                    si.CheckSignature(verifySignatureOnly: false);
                    Console.WriteLine("  Signature: VALID");
                }
                catch (CryptographicException ex)
                {
                    Console.WriteLine("  Signature: INVALID – " + ex.Message);
                }
            }
        }
        catch (CryptographicException ex)
        {
            Console.WriteLine("Failed to decode CMS signature: " + ex.Message);
        }
    }

    /// <summary>
    /// Entry point.
    /// Usage: <c>ShowSignature &lt;input.pdf&gt;</c>
    /// </summary>
    public static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine("Usage: ShowSignature <input.pdf>");
            Environment.Exit(1);
        }

        Show(args[0]);
    }
}
