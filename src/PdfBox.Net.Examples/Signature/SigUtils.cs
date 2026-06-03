/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/signature/SigUtils.java
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

/*
 * PORT_MODE: mechanical
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
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Interactive.DigitalSignature;

namespace PdfBox.Net.Examples.Signature;

/// <summary>
/// Utility methods for working with PDF digital signatures.
/// </summary>
public static class SigUtils
{
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
                throw new InvalidOperationException(
                    $"Certificate '{certificate.Subject}' does not have the DigitalSignature " +
                    "or NonRepudiation key usage flag required for PDF signing.");
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
}
