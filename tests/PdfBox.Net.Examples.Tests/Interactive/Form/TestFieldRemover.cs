// PDFBOX_SOURCE_PATH: examples/src/test/java/org/apache/pdfbox/examples/interactive/form/TestFieldRemover.java
// PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
// PORT_MODE: adapted
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

namespace PdfBox.Net.Examples.Tests.Interactive.Form;

/// <summary>
/// Test for FieldRemover example.
/// Ported from TestFieldRemover.java — adapted because the Java test relies on a specific
/// pre-existing AcroForm fixture PDF and an instance <c>remove(inPath, outPath, fieldName)</c>
/// method that is not yet present in the .NET port of FieldRemover.  The test is retained as
/// a traceability stub until both the fixture and the API are available.
/// </summary>
public class TestFieldRemover
{
    /// <summary>
    /// Stub: requires fixture PDF "PDFBOX-2469-1-AcroForm-AES128.pdf" and
    /// a <c>FieldRemover.Remove(string, string, string)</c> instance method
    /// not yet present in this .NET port.
    /// </summary>
    [Fact(Skip = "Adapted — requires fixture PDF and FieldRemover.Remove() instance method not yet ported")]
    public void TestFieldRemoval()
    {
        // Java original:
        //   FieldRemover fieldRemover = new FieldRemover();
        //   fieldRemover.remove(inPath, outPath, fullyQualifiedFieldName);
        // Not yet implemented in this .NET port.
    }
}
