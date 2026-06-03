/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/signature/CreateVisibleSignature2.java
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

using System.Security.Cryptography.X509Certificates;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.DigitalSignature;
using PdfBox.Net.PDModel.Interactive.Form;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Util;

namespace PdfBox.Net.Examples.Signature;

/// <summary>
/// A second example for visual signing a PDF. Unlike <see cref="CreateVisibleSignature"/>, this
/// class does not use the <c>PDVisibleSignDesigner</c> pattern; it builds the appearance stream
/// manually using <see cref="PDPageContentStream"/> targeting a <see cref="PDAppearanceStream"/>.
/// See the discussion in PDFBOX-3198.
/// </summary>
/// <remarks>
/// Ported from the Java <c>CreateVisibleSignature2</c> example.
/// The BouncyCastle DN/CN extraction is replaced with <see cref="X509Certificate2.GetNameInfo"/>.
/// <c>System.Drawing.Color</c> is not used; colors are passed as normalized RGB floats.
/// </remarks>
public class CreateVisibleSignature2 : CreateSignatureBase
{
    private bool _lateExternalSigning;
    private string? _imageFile;

    /// <summary>Gets or sets the path to the optional background image file.</summary>
    public string? ImageFile
    {
        get => _imageFile;
        set => _imageFile = value;
    }

    /// <summary>
    /// Gets or sets whether to activate the late-external-signing demo path (off by default).
    /// </summary>
    public bool LateExternalSigning
    {
        get => _lateExternalSigning;
        set => _lateExternalSigning = value;
    }

    /// <summary>
    /// Sign the PDF at <paramref name="inputFile"/> and write the result to
    /// <paramref name="signedFile"/>.
    /// </summary>
    /// <param name="inputFile">Source PDF path.</param>
    /// <param name="signedFile">Destination signed PDF path.</param>
    /// <param name="humanX">Signature X position from the top-left of the page (points).</param>
    /// <param name="humanY">Signature Y position from the top-left of the page (points).</param>
    /// <param name="humanWidth">Signature field width (points).</param>
    /// <param name="humanHeight">Signature field height (points).</param>
    /// <param name="tsaUrl">Optional TSA URL for an RFC 3161 timestamp.</param>
    public void SignPDF(string inputFile, string signedFile,
        float humanX, float humanY, float humanWidth, float humanHeight, string? tsaUrl)
        => SignPDF(inputFile, signedFile, humanX, humanY, humanWidth, humanHeight, tsaUrl, null);

    /// <summary>
    /// Sign the PDF at <paramref name="inputFile"/> and write the result to
    /// <paramref name="signedFile"/>, optionally using an existing (unsigned) signature field.
    /// </summary>
    public void SignPDF(
        string inputFile, string signedFile,
        float humanX, float humanY, float humanWidth, float humanHeight,
        string? tsaUrl, string? signatureFieldName)
    {
        if (!File.Exists(inputFile))
        {
            throw new IOException("Document for signing does not exist: " + inputFile);
        }

        SetTsaClient(tsaUrl != null ? new TSAClient(tsaUrl) : null);

        SignatureOptions? signatureOptions = null;
        FileStream fos = File.Create(signedFile);
        try
        {
            using PDDocument doc = PDDocument.Load(inputFile);

            int accessPermissions = SigUtils.GetMDPPermission(doc);
            if (accessPermissions == 1)
            {
                throw new InvalidOperationException(
                    "No changes to the document are permitted due to DocMDP transform parameters dictionary");
            }

            PDSignature? signature = null;
            PDAcroForm? acroForm = doc.GetDocumentCatalog().GetAcroForm();
            PDRectangle? rect = null;

            if (acroForm != null && signatureFieldName != null)
            {
                signature = FindExistingSignature(acroForm, signatureFieldName);
                if (signature != null)
                {
                    foreach (PDField field in acroForm.GetFieldTree())
                    {
                        if (signatureFieldName.Equals(field.GetFullyQualifiedName(),
                                StringComparison.Ordinal))
                        {
                            var widgets = field.GetWidgets();
                            if (widgets.Count > 0)
                                rect = widgets[0].GetRectangle();
                            break;
                        }
                    }
                }
            }

            signature ??= new PDSignature();
            rect ??= CreateSignatureRectangle(doc, humanX, humanY, humanWidth, humanHeight);

            if (doc.GetVersion() >= 1.5f && accessPermissions == 0)
            {
                SigUtils.SetMDPPermission(doc, signature, 2);
            }

            if (acroForm?.GetNeedAppearances() == true)
            {
                if (acroForm.GetFields().Count == 0)
                {
                    ((COSDictionary)acroForm.GetCOSObject()).RemoveItem(COSName.NEED_APPEARANCES);
                }
                else
                {
                    Console.WriteLine("/NeedAppearances is set, signature may be ignored by Adobe Reader");
                }
            }

            signature.SetFilter(PDSignature.FILTER_ADOBE_PPKLITE);
            signature.SetSubFilter(PDSignature.SUBFILTER_ADBE_PKCS7_DETACHED);
            signature.SetName("Name");
            signature.SetLocation("Location");
            signature.SetReason("Reason");
            signature.SetSignDate(DateTimeOffset.Now);

            SignatureInterface? signatureInterface = IsExternalSigning ? null : this;

            signatureOptions = new SignatureOptions();
            signatureOptions.SetVisualSignature(CreateVisualSignatureTemplate(doc, 0, rect, signature));
            signatureOptions.SetPage(0);
            doc.AddSignature(signature, signatureInterface, signatureOptions);

            if (IsExternalSigning)
            {
                ExternalSigningSupport externalSigning = doc.SaveIncrementalForExternalSigning(fos);
                byte[] cmsSignature = Sign(externalSigning.GetContent());

                if (_lateExternalSigning)
                {
                    externalSigning.SetSignature([]);
                    int offset = signature.GetByteRange()[1] + 1;
                    // Flush and close fos before re-opening the file for random access.
                    fos.Flush();
                    fos.Dispose();
                    using var raf = new FileStream(signedFile, FileMode.Open, FileAccess.ReadWrite);
                    raf.Seek(offset, SeekOrigin.Begin);
                    byte[] hexBytes = System.Text.Encoding.ASCII.GetBytes(
                        Convert.ToHexString(cmsSignature));
                    raf.Write(hexBytes, 0, hexBytes.Length);
                }
                else
                {
                    externalSigning.SetSignature(cmsSignature);
                    fos.Dispose();
                }
            }
            else
            {
                doc.SaveIncremental(fos);
                fos.Dispose();
            }
        }
        finally
        {
            signatureOptions?.Dispose();
            // fos may already be disposed; FileStream.Dispose is idempotent
            fos.Dispose();
        }
    }

    private static PDRectangle CreateSignatureRectangle(
        PDDocument doc, float humanX, float humanY, float humanWidth, float humanHeight)
    {
        PDPage page = doc.GetPage(0);
        PDRectangle pageRect = page.GetCropBox() ?? page.GetMediaBox()!;
        PDRectangle rect = new();
        switch (page.GetRotation())
        {
            case 90:
                rect.SetLowerLeftY(humanX);
                rect.SetUpperRightY(humanX + humanWidth);
                rect.SetLowerLeftX(humanY);
                rect.SetUpperRightX(humanY + humanHeight);
                break;
            case 180:
                rect.SetUpperRightX(pageRect.GetWidth() - humanX);
                rect.SetLowerLeftX(pageRect.GetWidth() - humanX - humanWidth);
                rect.SetLowerLeftY(humanY);
                rect.SetUpperRightY(humanY + humanHeight);
                break;
            case 270:
                rect.SetLowerLeftY(pageRect.GetHeight() - humanX - humanWidth);
                rect.SetUpperRightY(pageRect.GetHeight() - humanX);
                rect.SetLowerLeftX(pageRect.GetWidth() - humanY - humanHeight);
                rect.SetUpperRightX(pageRect.GetWidth() - humanY);
                break;
            case 0:
            default:
                rect.SetLowerLeftX(humanX);
                rect.SetUpperRightX(humanX + humanWidth);
                rect.SetLowerLeftY(pageRect.GetHeight() - humanY - humanHeight);
                rect.SetUpperRightY(pageRect.GetHeight() - humanY);
                break;
        }
        return rect;
    }

    // Creates a template PDF with an appearance stream for the visual signature and returns it as
    // a stream.
    private Stream CreateVisualSignatureTemplate(
        PDDocument srcDoc, int pageNum, PDRectangle rect, PDSignature signature)
    {
        using PDDocument doc = new();
        PDPage page = new(srcDoc.GetPage(pageNum).GetMediaBox()!);
        doc.AddPage(page);
        PDAcroForm acroForm = new(doc);
        doc.GetDocumentCatalog().SetAcroForm(acroForm);
        PDSignatureField signatureField = new(acroForm);
        PDAnnotationWidget widget = signatureField.GetWidgets()[0];
        var acroFormFields = acroForm.GetFields();
        acroForm.SetSignaturesExist(true);
        acroForm.SetAppendOnly(true);
        ((COSDictionary)acroForm.GetCOSObject()).SetDirect(true);
        acroFormFields.Add(signatureField);

        widget.SetRectangle(rect);

        // From PDVisualSigBuilder.createHolderForm()
        PDStream stream = new(doc);
        PDFormXObject form = new(stream);
        PDResources res = new();
        form.SetResources(res);
        form.SetFormType(1);
        PDRectangle bbox = new(rect.GetWidth(), rect.GetHeight());
        float bboxHeight = bbox.GetHeight();
        Matrix? initialScale = null;
        switch (srcDoc.GetPage(pageNum).GetRotation())
        {
            case 90:
                form.SetMatrix(AffineTransform.GetQuadrantRotateInstance(1));
                initialScale = Matrix.GetScaleInstance(
                    bbox.GetWidth() / bbox.GetHeight(),
                    bbox.GetHeight() / bbox.GetWidth());
                bboxHeight = bbox.GetWidth();
                break;
            case 180:
                form.SetMatrix(AffineTransform.GetQuadrantRotateInstance(2));
                break;
            case 270:
                form.SetMatrix(AffineTransform.GetQuadrantRotateInstance(3));
                initialScale = Matrix.GetScaleInstance(
                    bbox.GetWidth() / bbox.GetHeight(),
                    bbox.GetHeight() / bbox.GetWidth());
                bboxHeight = bbox.GetWidth();
                break;
            case 0:
            default:
                break;
        }
        form.SetBBox(bbox);
        PDFont font = new PDType1Font(PDType1Font.FontName.HELVETICA_BOLD);

        // From PDVisualSigBuilder.createAppearanceDictionary()
        PDAppearanceDictionary appearance = new();
        ((COSDictionary)appearance.GetCOSObject()).SetDirect(true);
        PDAppearanceStream appearanceStream = new(form.GetCOSObject());
        appearance.SetNormalAppearance(appearanceStream);
        widget.SetAppearance(appearance);

        using (PDPageContentStream cs = new(doc, appearanceStream))
        {
            if (initialScale != null)
            {
                cs.Transform(initialScale);
            }

            // Yellow background (debug/visibility aid — mirrors the Java example)
            cs.SetNonStrokingColor(1f, 1f, 0f);
            cs.AddRect(-5000, -5000, 10000, 10000);
            cs.Fill();

            if (_imageFile != null && File.Exists(_imageFile))
            {
                cs.SaveGraphicsState();
                cs.Transform(Matrix.GetScaleInstance(0.25f, 0.25f));
                PDImageXObject img = PDImageXObject.CreateFromFile(_imageFile, doc);
                cs.DrawImage(img, 0, 0);
                cs.RestoreGraphicsState();
            }

            float fontSize = 10;
            float leading = fontSize * 1.5f;
            cs.BeginText();
            cs.SetFont(font, fontSize);
            cs.SetNonStrokingColor(0f, 0f, 0f); // black
            cs.NewLineAtOffset(fontSize, bboxHeight - leading);
            cs.SetLeading(leading);

            X509Certificate2? cert = GetCertificate();
            if (cert != null)
            {
                string name = cert.GetNameInfo(X509NameType.SimpleName, false) ?? cert.Subject;
                string date = signature.GetSignDate()?.ToString("g") ?? DateTimeOffset.Now.ToString("g");
                string? reason = signature.GetReason();

                cs.ShowText("Signer: " + name);
                cs.NewLine();
                cs.ShowText(date);
                cs.NewLine();
                if (!string.IsNullOrEmpty(reason))
                {
                    cs.ShowText("Reason: " + reason);
                }
            }

            cs.EndText();
        }

        using var baos = new MemoryStream();
        doc.Save(baos);
        return new MemoryStream(baos.ToArray());
    }

    // Finds an existing empty (unsigned) signature field by name.
    private static PDSignature? FindExistingSignature(PDAcroForm acroForm, string? sigFieldName)
    {
        if (sigFieldName == null) return null;

        foreach (PDField field in acroForm.GetFieldTree())
        {
            if (!sigFieldName.Equals(field.GetFullyQualifiedName(), StringComparison.Ordinal))
                continue;

            if (field is PDSignatureField sigField)
            {
                PDSignature? existing = sigField.GetSignature();
                if (existing != null)
                {
                    throw new InvalidOperationException(
                        $"The signature field '{sigFieldName}' is already signed.");
                }
                PDSignature sig = new();
                ((COSDictionary)sigField.GetCOSObject()).SetItem(COSName.V, sig);
                return sig;
            }
        }

        return null;
    }

    /// <summary>
    /// CLI entry point:
    /// <c>CreateVisibleSignature2 &lt;keystore.p12&gt; &lt;pin&gt; &lt;input.pdf&gt; [&lt;image&gt;] [-tsa &lt;url&gt;] [-e]</c>
    /// </summary>
    public static void Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine(
                $"Usage: {nameof(CreateVisibleSignature2)} <keystore.p12> <pin> <input.pdf> [<sign-image>] [-tsa <url>] [-e]");
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

        CreateVisibleSignature2 signing = new();
        signing.SetKeystore(keystoreFile, pin);
        signing.IsExternalSigning = externalSig;

        if (args.Length >= 4 && !"-tsa".Equals(args[3], StringComparison.OrdinalIgnoreCase)
                             && !"-e".Equals(args[3], StringComparison.OrdinalIgnoreCase))
        {
            signing.ImageFile = args[3];
        }

        string name = Path.GetFileNameWithoutExtension(documentFile);
        string signedFile = Path.Combine(
            Path.GetDirectoryName(documentFile) ?? ".", name + "_signed.pdf");

        // Signature placed 100pt from left, 200pt from top, 150 x 50 pt
        signing.SignPDF(documentFile, signedFile, 100, 200, 150, 50, tsaUrl, "Signature1");
        Console.WriteLine("Signed PDF written to: " + signedFile);
    }
}
