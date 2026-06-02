/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/interactive/form/CreateSimpleForm.java
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
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.Form;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.Examples.Interactive.Form;

/// <summary>
/// An example of creating an AcroForm and a form field from scratch.
///
/// The form field is created with properties similar to creating a form with default settings
/// in Adobe Acrobat.
/// </summary>
public sealed class CreateSimpleForm
{
    internal const string DefaultFilename = "target/SimpleForm.pdf";

    private CreateSimpleForm()
    {
    }

    public static void Main(string[] args)
    {
        // Create a new document with an empty page.
        using (PDDocument document = new PDDocument())
        {
            PDPage page = new PDPage(PDRectangle.A4);
            document.AddPage(page);

            // Adobe Acrobat uses Helvetica as a default font and
            // stores that under the name '/Helv' in the resources dictionary
            PDFont font = new PDType1Font(PDType1Font.FontName.HELVETICA);
            PDResources resources = new PDResources();
            resources.Put(COSName.GetPDFName("Helv"), font);

            // Add a new AcroForm and add that to the document
            PDAcroForm acroForm = new PDAcroForm(document);
            document.GetDocumentCatalog().SetAcroForm(acroForm);

            // Add and set the resources and default appearance at the form level
            acroForm.SetDefaultResources(resources);

            // Acrobat sets the font size on the form level to be
            // auto sized as default. This is done by setting the font size to '0'
            string defaultAppearanceString = "/Helv 0 Tf 0 g";
            acroForm.SetDefaultAppearance(defaultAppearanceString);

            // Add a form field to the form.
            PDTextField textBox = new PDTextField(acroForm);
            textBox.SetPartialName("SampleField");

            // Acrobat sets the font size to 12 as default
            // The text color is set to blue in this example.
            // To use black, replace "0 0 1 rg" with "0 0 0 rg" or "0 g".
            defaultAppearanceString = "/Helv 12 Tf 0 0 1 rg";
            textBox.SetDefaultAppearance(defaultAppearanceString);

            // add the field to the acroform
            acroForm.GetFields().Add(textBox);

            // Specify the widget annotation associated with the field
            PDAnnotationWidget widget = textBox.GetWidgets()[0];
            PDRectangle rect = new PDRectangle(50, 750, 200, 50);
            widget.SetRectangle(rect);
            widget.SetPage(page);

            // set green border and yellow background
            PDAppearanceCharacteristicsDictionary fieldAppearance =
                new PDAppearanceCharacteristicsDictionary(new COSDictionary());
            fieldAppearance.SetBorderColour(new PDColor(new float[] { 0, 1, 0 }, PDDeviceRGB.Instance));
            fieldAppearance.SetBackground(new PDColor(new float[] { 1, 1, 0 }, PDDeviceRGB.Instance));
            widget.SetAppearanceCharacteristics(fieldAppearance);

            // make sure the widget annotation is visible on screen and paper
            widget.SetPrinted(true);

            // Add the widget annotation to the page
            page.GetAnnotations().Add(widget);

            // set the alignment ("quadding") — 0=left, 1=centered, 2=right
            textBox.SetQ(1);

            // set the field value
            textBox.SetValue("Sample field content");

            // put some text near the field
            using (PDPageContentStream cs = new PDPageContentStream(document, page))
            {
                cs.BeginText();
                cs.SetFont(new PDType1Font(PDType1Font.FontName.HELVETICA), 15);
                cs.NewLineAtOffset(50, 810);
                cs.ShowText("Field:");
                cs.EndText();
            }

            if (args == null || args.Length == 0)
            {
                document.Save(DefaultFilename);
            }
            else
            {
                document.Save(args[0]); // used for concurrent build tests
            }
        }
    }
}
