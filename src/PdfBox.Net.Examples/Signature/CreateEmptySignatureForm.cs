/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/signature/CreateEmptySignatureForm.java
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

using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.Form;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.Examples.Signature;

// PORT_MODE: mechanical

/// <summary>
/// Creates an AcroForm with an empty (unsigned) signature field in a new PDF document.
/// </summary>
/// <remarks>
/// A real signature can be added later — for example by signing with
/// <see cref="CreateSignature"/> or by clicking the field in Adobe Reader.
/// </remarks>
public sealed class CreateEmptySignatureForm
{
    private CreateEmptySignatureForm()
    {
    }

    /// <summary>
    /// Creates a new single-page PDF at <paramref name="outputPath"/> that contains one empty
    /// signature field named "Signature1".
    /// </summary>
    public static void CreateForm(string outputPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(outputPath);

        using PDDocument document = new();
        PDPage page = new(PDRectangle.A4);
        document.AddPage(page);

        // Adobe Acrobat stores the default form font under '/Helv' in the resources dictionary.
        PDFont font = new PDType1Font(PDType1Font.FontName.HELVETICA);
        PDResources resources = new();
        resources.Put(COSName.GetPDFName("Helv"), font);

        // Add a new AcroForm and set resources + default appearance.
        PDAcroForm acroForm = new(document);
        document.GetDocumentCatalog().SetAcroForm(acroForm);
        acroForm.SetDefaultResources(resources);
        // Font size 0 means auto-size.
        acroForm.SetDefaultAppearance("/Helv 0 Tf 0 g");

        // Create an empty signature field ("Signature1").
        PDSignatureField signatureField = new(acroForm);
        PDAnnotationWidget widget = signatureField.GetWidgets()[0];

        PDRectangle rect = new(50, 650, 200, 50);
        widget.SetRectangle(rect);
        widget.SetPage(page);
        widget.SetPrinted(true);

        page.GetAnnotations().Add(widget);
        acroForm.GetFields().Add(signatureField);

        document.Save(outputPath);
    }

    /// <summary>
    /// CLI entry point: <c>CreateEmptySignatureForm &lt;output.pdf&gt;</c>
    /// </summary>
    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine($"Usage: {nameof(CreateEmptySignatureForm)} <output.pdf>");
            Environment.Exit(1);
        }

        CreateForm(args[0]);
        Console.WriteLine("Created empty signature form: " + args[0]);
    }
}
