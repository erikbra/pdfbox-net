// PDFBOX_SOURCE_PATH: examples/src/test/java/org/apache/pdfbox/examples/pdfa/MergePDFATest.java
// PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
// PORT_MODE: adapted
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

namespace PdfBox.Net.Examples.Tests.PDFA;

/// <summary>
/// Test of the PDF/A merge example.
/// Ported from MergePDFATest.java — adapted because:
/// <list type="bullet">
///   <item>The test depends on <c>CreatePDFA</c> producing a valid PDF/A-1b file, which itself
///         throws <see cref="NotSupportedException"/> (see <see cref="CreatePDFATest"/>).</item>
///   <item>PDF/A compliance validation via VeraPDF is a Java-only dependency with no equivalent
///         currently integrated in this port.</item>
/// </list>
/// </summary>
public class MergePDFATest
{
    /// <summary>
    /// Stub: depends on CreatePDFA (not yet ported) and VeraPDF (Java-only).
    /// </summary>
    [Fact(Skip = "Adapted — depends on CreatePDFA (NotSupportedException) and VeraPDF Java library not available in .NET")]
    public void TestMergePDFA()
    {
        // Java original merges two PDF/A-1b files created by CreatePDFA and then
        // validates the merged result using VeraPDF.
        // Neither CreatePDFA nor VeraPDF is available in this .NET port.
    }
}
