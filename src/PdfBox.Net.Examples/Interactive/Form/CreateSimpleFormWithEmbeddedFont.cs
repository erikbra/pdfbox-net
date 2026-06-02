/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/interactive/form/CreateSimpleFormWithEmbeddedFont.java
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
/// An example of creating an AcroForm and a form field from scratch with a font fully embedded to
/// allow non-WinAnsiEncoding input.
///
/// The form field is created with properties similar to creating a form with default settings in
/// Adobe Acrobat.
///
/// Expects a TrueType font file path as the first argument (e.g. LiberationSans-Regular.ttf).
/// </summary>
public class CreateSimpleFormWithEmbeddedFont
{
    private CreateSimpleFormWithEmbeddedFont()
    {
    }

    public static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine("usage: CreateSimpleFormWithEmbeddedFont <ttf-file>");
            return;
        }

        string ttfPath = args[0];

        // Create a new document with an empty page.
        using (PDDocument doc = new PDDocument())
        {
            PDPage page = new PDPage(PDRectangle.A4);
            doc.AddPage(page);
            PDAcroForm acroForm = new PDAcroForm(doc);
            doc.GetDocumentCatalog().SetAcroForm(acroForm);

            // Note that the font is fully embedded. If you use a different font, make sure that
            // its license allows full embedding.
            PDFont formFont = PDType0Font.Load(doc, ttfPath);

            // Add and set the resources and default appearance at the form level
            PDResources resources = new PDResources();
            acroForm.SetDefaultResources(resources);

            // Assign a name for the embedded font in the resource dictionary
            COSName fontKey = COSName.GetPDFName("F1");
            resources.Put(fontKey, formFont);
            string fontName = fontKey.GetName();

            // Acrobat sets the font size on the form level to be
            // auto sized as default. This is done by setting the font size to '0'
            string defaultAppearanceString = "/" + fontName + " 0 Tf 0 g";

            PDTextField textBox = new PDTextField(acroForm);
            textBox.SetPartialName("SampleField");
            textBox.SetDefaultAppearance(defaultAppearanceString);
            acroForm.GetFields().Add(textBox);

            // Specify the widget annotation associated with the field
            PDAnnotationWidget widget = textBox.GetWidgets()[0];
            PDRectangle rect = new PDRectangle(50, 700, 200, 50);
            widget.SetRectangle(rect);
            widget.SetPage(page);
            page.GetAnnotations().Add(widget);

            // set green border and yellow background
            PDAppearanceCharacteristicsDictionary fieldAppearance =
                new PDAppearanceCharacteristicsDictionary(new COSDictionary());
            fieldAppearance.SetBorderColour(new PDColor(new float[] { 0, 1, 0 }, PDDeviceRGB.Instance));
            fieldAppearance.SetBackground(new PDColor(new float[] { 1, 1, 0 }, PDDeviceRGB.Instance));
            widget.SetAppearanceCharacteristics(fieldAppearance);

            // set the field value
            textBox.SetValue("Sample field");

            // put some text near the field
            using (PDPageContentStream cs = new PDPageContentStream(doc, page))
            {
                cs.BeginText();
                cs.SetFont(new PDType1Font(PDType1Font.FontName.HELVETICA), 15);
                cs.NewLineAtOffset(50, 760);
                cs.ShowText("Field:");
                cs.EndText();
            }

            doc.Save("target/SimpleFormWithEmbeddedFont.pdf");
        }
    }
}
