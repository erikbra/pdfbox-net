/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/interactive/form/SetField.java
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

using PdfBox.Net;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Interactive.Form;

namespace PdfBox.Net.Examples.Interactive.Form;

/// <summary>
/// An example of setting a field value in a PDF.
/// </summary>
public class SetField
{
    private SetField()
    {
    }

    public static void Main(string[] args)
    {
        if (args.Length != 3)
        {
            Console.Error.WriteLine("usage: SetField <input-pdf> <field-name> <value>");
            return;
        }

        string pdfFile = args[0];
        string fieldName = args[1];
        string value = args[2];

        using (PDDocument document = Loader.LoadPDF(pdfFile))
        {
            PDAcroForm? acroForm = document.GetDocumentCatalog().GetAcroForm();
            if (acroForm != null)
            {
                PDField? field = null;
                foreach (PDField f in acroForm.GetFieldTree())
                {
                    if (f.GetFullyQualifiedName() == fieldName)
                    {
                        field = f;
                        break;
                    }
                }
                if (field is PDTextField textField)
                {
                    textField.SetValue(value);
                }
                else if (field is PDChoice choiceField)
                {
                    choiceField.SetValue(value);
                }
                else if (field != null)
                {
                    Console.Error.WriteLine($"Field '{fieldName}' is of type {field.GetType().Name} — SetValue not supported on this type.");
                }
                else
                {
                    Console.Error.WriteLine("Field not found: " + fieldName);
                }
            }
            document.Save(pdfFile);
        }
    }
}
