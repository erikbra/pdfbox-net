/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/signature/CreateVisibleSignature.java
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

using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Interactive.DigitalSignature;
using PdfBox.Net.PDModel.Interactive.DigitalSignature.Visible;
using PdfBox.Net.PDModel.Interactive.Form;

namespace PdfBox.Net.Examples.Signature;

/// <summary>
/// Creates a visible PDF signature using a PKCS#12 keystore and an optional image.
/// </summary>
/// <remarks>
/// Ported from the Java <c>CreateVisibleSignature</c> example.
/// <c>PDVisibleSignDesigner.AdjustForRotation()</c> is not yet ported; page rotation
/// compensation should be applied manually if needed.
/// </remarks>
public class CreateVisibleSignature : CreateSignatureBase
{
    private PDVisibleSignDesigner? _visibleSignDesigner;
    private readonly PDVisibleSigProperties _visibleSignatureProperties = new();
    private bool _lateExternalSigning;
    private PDDocument? _doc;

    /// <summary>Gets or sets whether to enable the late-external-signing demo path.</summary>
    public bool LateExternalSigning
    {
        get => _lateExternalSigning;
        set => _lateExternalSigning = value;
    }

    /// <summary>
    /// Opens the PDF, creates and sets the visible signature designer for a new signature field.
    /// </summary>
    public void SetVisibleSignDesigner(
        string filename, int x, int y, int zoomPercent, Stream imageStream, int page)
    {
        _doc = PDDocument.Load(filename);
        _visibleSignDesigner = new PDVisibleSignDesigner(_doc, imageStream, page);
        _visibleSignDesigner.XAxis(x).YAxis(y).Zoom(zoomPercent);
    }

    /// <summary>
    /// Sets the visible signature designer for an existing signature field.
    /// </summary>
    public void SetVisibleSignDesigner(int zoomPercent, Stream imageStream)
    {
        _visibleSignDesigner = new PDVisibleSignDesigner(imageStream);
        _visibleSignDesigner.Zoom(zoomPercent);
    }

    /// <summary>Sets visible signature properties for a new signature field.</summary>
    public void SetVisibleSignatureProperties(
        string name, string location, string reason, int preferredSize, int page, bool visualSignEnabled)
    {
        _visibleSignatureProperties
            .SignerName(name)
            .SignerLocation(location)
            .SignatureReason(reason)
            .PreferredSize(preferredSize)
            .Page(page)
            .VisualSignEnabled(visualSignEnabled)
            .SetPdVisibleSignature(_visibleSignDesigner
                ?? throw new InvalidOperationException("Call SetVisibleSignDesigner first."));
    }

    /// <summary>Sets visible signature properties for an existing signature field.</summary>
    public void SetVisibleSignatureProperties(
        string name, string location, string reason, bool visualSignEnabled)
    {
        _visibleSignatureProperties
            .SignerName(name)
            .SignerLocation(location)
            .SignatureReason(reason)
            .VisualSignEnabled(visualSignEnabled)
            .SetPdVisibleSignature(_visibleSignDesigner
                ?? throw new InvalidOperationException("Call SetVisibleSignDesigner first."));
    }

    /// <summary>
    /// Signs the PDF at <paramref name="inputFile"/> and writes the signed PDF to
    /// <paramref name="signedFile"/>.
    /// </summary>
    public void SignPDF(string inputFile, string signedFile, string? tsaUrl)
        => SignPDF(inputFile, signedFile, tsaUrl, null);

    /// <summary>
    /// Signs the PDF at <paramref name="inputFile"/> and writes the signed PDF to
    /// <paramref name="signedFile"/>.  Optionally uses an existing (unsigned) signature field
    /// identified by <paramref name="signatureFieldName"/>.
    /// </summary>
    public void SignPDF(
        string inputFile, string signedFile, string? tsaUrl, string? signatureFieldName)
    {
        if (!File.Exists(inputFile))
        {
            throw new FileNotFoundException("Document for signing does not exist.", inputFile);
        }

        if (tsaUrl != null)
        {
            SetTsaClient(new TSAClient(tsaUrl));
        }

        _doc ??= PDDocument.Load(inputFile);

        SignatureOptions? signatureOptions = null;
        try
        {
            using var fos = File.OpenWrite(signedFile);

            int accessPermissions = SigUtils.GetMDPPermission(_doc);
            if (accessPermissions == 1)
            {
                throw new InvalidOperationException(
                    "No changes to the document are permitted due to DocMDP transform parameters dictionary.");
            }

            PDSignature signature = FindExistingSignature(_doc, signatureFieldName) ?? new PDSignature();

            if (_doc.GetVersion() >= 1.5f && accessPermissions == 0)
            {
                SigUtils.SetMDPPermission(_doc, signature, 2);
            }

            PDAcroForm? acroForm = _doc.GetDocumentCatalog().GetAcroForm();
            if (acroForm?.GetNeedAppearances() == true)
            {
                if (acroForm.GetFields().Count == 0)
                {
                    ((PdfBox.Net.COS.COSDictionary)acroForm.GetCOSObject()).RemoveItem(COSName.GetPDFName("NeedAppearances"));
                }
                else
                {
                    Console.WriteLine("/NeedAppearances is set, signature may be ignored by Adobe Reader");
                }
            }

            signature.SetFilter(PDSignature.FILTER_ADOBE_PPKLITE);
            signature.SetSubFilter(PDSignature.SUBFILTER_ADBE_PKCS7_DETACHED);

            _visibleSignatureProperties.BuildSignature();

            signature.SetName(_visibleSignatureProperties.GetSignerName());
            signature.SetLocation(_visibleSignatureProperties.GetSignerLocation());
            signature.SetReason(_visibleSignatureProperties.GetSignatureReason());
            signature.SetSignDate(DateTimeOffset.Now);

            SignatureInterface? signInterface = IsExternalSigning ? null : this;

            if (_visibleSignatureProperties.IsVisualSignEnabled())
            {
                signatureOptions = new SignatureOptions();
                signatureOptions.SetVisualSignature(_visibleSignatureProperties.GetVisibleSignature());
                signatureOptions.SetPage(_visibleSignatureProperties.GetPage() - 1);
                if (signInterface is null)
                {
                    _doc.AddSignature(signature, signatureOptions);
                }
                else
                {
                    _doc.AddSignature(signature, signInterface, signatureOptions);
                }
            }
            else if (signInterface is null)
            {
                _doc.AddSignature(signature);
            }
            else
            {
                _doc.AddSignature(signature, signInterface);
            }

            if (IsExternalSigning)
            {
                ExternalSigningSupport externalSigning = _doc.SaveIncrementalForExternalSigning(fos);
                byte[] cmsSignature = Sign(externalSigning.GetContent());

                if (_lateExternalSigning)
                {
                    externalSigning.SetSignature([]);
                    // The caller can write cmsSignature manually at signature.GetByteRange()[1]+1.
                }
                else
                {
                    externalSigning.SetSignature(cmsSignature);
                }
            }
            else
            {
                _doc.SaveIncremental(fos);
            }
        }
        finally
        {
            signatureOptions?.Dispose();
            _doc?.Dispose();
            _doc = null;
        }
    }

    /// <summary>
    /// Locates an empty (unsigned) signature field by name in the AcroForm.
    /// Returns <c>null</c> if the field does not exist.
    /// </summary>
    private static PDSignature? FindExistingSignature(PDDocument doc, string? sigFieldName)
    {
        if (sigFieldName == null) return null;

        PDAcroForm? acroForm = doc.GetDocumentCatalog().GetAcroForm();
        if (acroForm == null) return null;

        foreach (PDField field in acroForm.GetFieldTree())
        {
            if (!sigFieldName.Equals(field.GetFullyQualifiedName(), StringComparison.Ordinal))
                continue;

            if (field is PDSignatureField sigField)
            {
                if (sigField.GetSignature() != null)
                {
                    throw new InvalidOperationException(
                        $"The signature field {sigFieldName} is already signed.");
                }
                PDSignature sig = new();
                ((PdfBox.Net.COS.COSDictionary)sigField.GetCOSObject()).SetItem(COSName.V, sig);
                return sig;
            }
        }

        return null;
    }

    /// <summary>
    /// CLI entry point:
    /// <c>CreateVisibleSignature &lt;keystore.p12&gt; &lt;pin&gt; &lt;input.pdf&gt; &lt;image&gt; [-tsa &lt;url&gt;] [-e]</c>
    /// </summary>
    public static void Main(string[] args)
    {
        if (args.Length < 4)
        {
            Console.Error.WriteLine(
                $"Usage: {nameof(CreateVisibleSignature)} <keystore.p12> <pin> <input.pdf> <sign-image> [-tsa <url>] [-e]");
            Environment.Exit(1);
        }

        string? tsaUrl = null;
        bool externalSig = false;
        for (int i = 0; i < args.Length; i++)
        {
            if ("-tsa".Equals(args[i], StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                tsaUrl = args[++i];
            if ("-e".Equals(args[i], StringComparison.OrdinalIgnoreCase))
                externalSig = true;
        }

        string keystoreFile = args[0];
        string pin = args[1];
        string documentFile = args[2];
        string imageFile = args[3];

        CreateVisibleSignature signing = new();
        signing.SetKeystore(keystoreFile, pin);
        signing.IsExternalSigning = externalSig;

        string name = Path.GetFileNameWithoutExtension(documentFile);
        string signedFile = Path.Combine(Path.GetDirectoryName(documentFile) ?? ".", name + "_signed.pdf");

        int page = 1;
        using (var imageStream = File.OpenRead(imageFile))
        {
            signing.SetVisibleSignDesigner(documentFile, 0, 0, -50, imageStream, page);
        }

        signing.SetVisibleSignatureProperties("name", "location", "Security", 0, page, true);
        signing.SignPDF(documentFile, signedFile, tsaUrl);
        Console.WriteLine("Signed PDF written to: " + signedFile);
    }
}
