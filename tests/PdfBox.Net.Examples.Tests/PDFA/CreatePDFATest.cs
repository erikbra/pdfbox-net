// PDFBOX_SOURCE_PATH: examples/src/test/java/org/apache/pdfbox/examples/pdfa/CreatePDFATest.java
// PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
// PORT_MODE: adapted
// PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf

/*
 * Copyright 2015 The Apache Software Foundation.
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

namespace PdfBox.Net.Examples.Tests.PDFA;

/// <summary>
/// Test of CreatePDFA example.
/// Ported from CreatePDFATest.java — adapted because:
/// <list type="bullet">
///   <item><c>CreatePDFA</c> requires PDType0Font, XMP schema typed setters, and
///         PDPageContentStream text operators not yet ported, so it throws
///         <see cref="NotSupportedException"/>.</item>
///   <item>VeraPDF (used for PDF/A-1b compliance validation) is a Java-only library
///         with no .NET equivalent currently integrated into this port.</item>
///   <item>The signing step (<c>CreateSignature</c>) requires BouncyCastle cryptographic
///         primitives not yet ported.</item>
/// </list>
/// </summary>
public class CreatePDFATest
{
    [Fact]
    public void TestCreatePDFAThrows()
    {
        string outDir = Path.Combine(Path.GetTempPath(), "pdfbox-examples-tests-pdfa");
        Directory.CreateDirectory(outDir);
        string pdfaFilename = Path.Combine(outDir, "PDFA.pdf");
        string message = "The quick brown fox";
        string fontfile = "LiberationSans-Regular.ttf"; // not present; throws before use

        Assert.Throws<NotSupportedException>(() =>
            CreatePDFA.Main(new string[] { pdfaFilename, message, fontfile }));
    }
}
