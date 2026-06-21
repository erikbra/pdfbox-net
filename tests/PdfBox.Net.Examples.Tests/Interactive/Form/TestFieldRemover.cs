// PDFBOX_SOURCE_PATH: examples/src/test/java/org/apache/pdfbox/examples/interactive/form/TestFieldRemover.java
// PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
// PORT_MODE: mechanical
// PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf

/*
 * Copyright 2026 The Apache Software Foundation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
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
using PdfBox.Net.Examples.Interactive.Form;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Interactive.Form;

namespace PdfBox.Net.Examples.Tests.Interactive.Form;

/// <summary>
/// Test for FieldRemover example.
/// </summary>
public class TestFieldRemover
{
    [Fact]
    public void TestFieldRemoval()
    {
        string workspace = Path.Combine(Path.GetTempPath(), $"pdfbox-field-remover-{Guid.NewGuid():N}");
        string inputPath = Path.Combine(workspace, "PDFBOX-2469-1-AcroForm-AES128.pdf");
        string outputPath = Path.Combine(workspace, "FieldRemover.pdf");
        Directory.CreateDirectory(workspace);

        try
        {
            CreateFixture(inputPath);
            using (PDDocument input = Loader.LoadPDF(inputPath))
            {
                PDAcroForm? inputForm = input.GetDocumentCatalog().GetAcroForm();
                Assert.NotNull(inputForm);
                Assert.Contains(inputForm!.GetFields(), field =>
                    field.GetFullyQualifiedName() == "remove-me");
            }

            new FieldRemover().Remove(inputPath, outputPath, "remove-me");

            using PDDocument document = Loader.LoadPDF(outputPath);
            PDAcroForm? acroForm = document.GetDocumentCatalog().GetAcroForm();
            Assert.NotNull(acroForm);
            PDField remainingField = Assert.Single(acroForm!.GetFields());
            Assert.Equal("keep-me", remainingField.GetFullyQualifiedName());
        }
        finally
        {
            Directory.Delete(workspace, recursive: true);
        }
    }

    private static void CreateFixture(string path)
    {
        using PDDocument document = new();
        document.AddPage(new PDPage());

        PDAcroForm acroForm = new(document);
        PDTextField remove = new(acroForm);
        remove.SetPartialName("remove-me");
        PDTextField keep = new(acroForm);
        keep.SetPartialName("keep-me");
        acroForm.SetFields([remove, keep]);
        document.GetDocumentCatalog().SetAcroForm(acroForm);
        document.Save(path);
    }
}
