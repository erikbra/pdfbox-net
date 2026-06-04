/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/interactive/form/CreateComboBox.java
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

using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.Form;

namespace PdfBox.Net.Examples.Interactive.Form;

/// <summary>
/// Example for creating a combo box (drop-down list) AcroForm widget.
/// </summary>
public class CreateComboBox
{
    private CreateComboBox()
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

            PDComboBox comboBox = new PDComboBox(acroForm);
            comboBox.SetPartialName("MyComboBox");
            comboBox.SetOptions(new List<string> { "Option 1", "Option 2", "Option 3" });

            PDRectangle fieldArea = new PDRectangle(50, 650, 200, 20);
            PDAnnotationWidget widget = comboBox.GetWidgets()[0];
            widget.SetRectangle(fieldArea);
            widget.SetPage(page);

            page.GetAnnotations().Add(widget);
            acroForm.GetFields().Add(comboBox);

            document.Save("combobox.pdf");
        }
    }
}
