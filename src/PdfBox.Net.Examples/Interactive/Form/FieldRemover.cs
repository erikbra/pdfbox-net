/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/interactive/form/FieldRemover.java
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
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Interactive.Form;

namespace PdfBox.Net.Examples.Interactive.Form;

/// <summary>
/// Remove all form fields from a PDF, keeping the visual appearance.
/// </summary>
public class FieldRemover
{
    public FieldRemover()
    {
    }

    /// <summary>
    /// Removes the named form field from a PDF document.
    /// </summary>
    /// <param name="inputPath">Path to the input PDF.</param>
    /// <param name="outputPath">Path to write the updated PDF.</param>
    /// <param name="fullyQualifiedFieldName">Fully qualified name of the field to remove.</param>
    public void Remove(string inputPath, string outputPath, string fullyQualifiedFieldName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullyQualifiedFieldName);

        using PDDocument document = Loader.LoadPDF(inputPath);
        PDAcroForm? acroForm = document.GetDocumentCatalog().GetAcroForm();
        List<PDField>? topLevelFields = acroForm?.GetFields().ToList();
        PDField? field = acroForm?.GetFieldTree()
            .FirstOrDefault(candidate => string.Equals(
                candidate.GetFullyQualifiedName(),
                fullyQualifiedFieldName,
                StringComparison.Ordinal));

        if (field is not null)
        {
            COSDictionary fieldDictionary = (COSDictionary)field.GetCOSObject();
            COSDictionary? parent = fieldDictionary.GetCOSDictionary(COSName.PARENT);
            if (parent is null)
            {
                topLevelFields!.RemoveAll(candidate =>
                    ReferenceEquals(candidate.GetCOSObject(), field.GetCOSObject()));
                acroForm!.SetFields(topLevelFields);
            }
            else
            {
                parent.GetCOSArray(COSName.KIDS)?.Remove(fieldDictionary);
            }
        }

        document.Save(outputPath);
    }

    public static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.Error.WriteLine("usage: FieldRemover <input-pdf> <output-pdf>");
            return;
        }

        using (PDDocument document = Loader.LoadPDF(args[0]))
        {
            PDAcroForm? acroForm = document.GetDocumentCatalog().GetAcroForm();
            if (acroForm != null)
            {
                // Remove all fields
                acroForm.GetFields().Clear();
            }
            document.Save(args[1]);
        }
    }
}
