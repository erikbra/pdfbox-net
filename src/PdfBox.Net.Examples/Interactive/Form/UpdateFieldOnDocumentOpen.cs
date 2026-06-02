/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/interactive/form/UpdateFieldOnDocumentOpen.java
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

using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Interactive.Action;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.Form;

namespace PdfBox.Net.Examples.Interactive.Form;

/// <summary>
/// Create a PDF document that updates a field value when the document is opened.
/// </summary>
public class UpdateFieldOnDocumentOpen
{
    private UpdateFieldOnDocumentOpen()
    {
    }

    public static void Main(string[] args)
    {
        using (PDDocument document = new PDDocument())
        {
            PDPage page = new PDPage();
            document.AddPage(page);

            PDAcroForm acroForm = new PDAcroForm(document);
            document.GetDocumentCatalog().SetAcroForm(acroForm);

            PDTextField textBox = new PDTextField(acroForm);
            textBox.SetPartialName("DateField");
            textBox.SetDefaultValue("Dynamic date goes here");

            PDAnnotationWidget widget = textBox.GetWidgets()[0];
            widget.SetRectangle(new PDRectangle(50, 750, 200, 20));
            widget.SetPage(page);

            page.GetAnnotations().Add(widget);
            acroForm.GetFields().Add(textBox);

            // JavaScript to update field on document open
            PDActionJavaScript updateAction = new PDActionJavaScript(
                "this.getField('DateField').value = util.printd('mm/dd/yyyy', new Date());");
            document.GetDocumentCatalog().SetOpenAction(updateAction);

            document.Save("update-on-open.pdf");
        }
    }
}
