// PDFBOX_SOURCE_PATH: examples/src/test/java/org/apache/pdfbox/examples/pdfa/MergePDFATest.java
// PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
// PORT_MODE: mechanical
// PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf

/*
 * Copyright 2024 The Apache Software Foundation.
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

using PdfBox.Net.Examples.PDModel;
using PdfBox.Net.Examples.Util;
using PdfBox.Net.PDModel;

namespace PdfBox.Net.Examples.Tests.PDFA;

/// <summary>
/// Test of the PDF/A merge example.
/// Ported from MergePDFATest.java. Full PDF/A compliance validation is covered by the
/// Preflight/VeraPDF external-validation adaptation ledger; this test covers deterministic
/// offline creation and merge behavior.
/// </summary>
public class MergePDFATest
{
    [Fact]
    public void TestMergePDFA()
    {
        string outDir = ExampleTestResources.CreateTempDirectory("examples-tests-pdfa-merge");
        string fontPath = ExampleTestResources.WriteLiberationSansRegular(outDir);
        string sourceFile = Path.Combine(outDir, "Source_PDFA.pdf");
        string mergedFile = Path.Combine(outDir, "Merged_PDFA.pdf");

        CreatePDFA.Main(new string[] { sourceFile, "The quick brown fox", fontPath });
        PDFMergerExample.Merge(new[] { sourceFile, sourceFile }, mergedFile);

        using PDDocument document = PDDocument.Load(mergedFile);
        Assert.Equal(2, document.GetNumberOfPages());
        Assert.NotNull(document.GetDocumentCatalog().GetMetadata());
    }
}
