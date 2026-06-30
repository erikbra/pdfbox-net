// PDFBOX_SOURCE_PATH: examples/src/test/java/org/apache/pdfbox/examples/interactive/form/TestCreateSimpleForms.java
// PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
// PORT_MODE: mechanical
// PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf

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
using PdfBox.Net.Examples.Interactive.Form;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Interactive.Form;

namespace PdfBox.Net.Examples.Tests.Interactive.Form;

/// <summary>
/// Test of some the form examples.
/// Ported from TestCreateSimpleForms.java.
/// Examples that save to a relative <c>target/</c> path are run from a temporary directory
/// that contains a pre-created <c>target/</c> subdirectory, matching the Maven convention.
/// </summary>
public class TestCreateSimpleForms : IDisposable
{
    private readonly string _tempDir;
    private readonly string _originalDir;

    public TestCreateSimpleForms()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "pdfbox-form-tests-" + Guid.NewGuid().ToString("N")[..8]);
        _originalDir = Directory.GetCurrentDirectory();
        Directory.CreateDirectory(Path.Combine(_tempDir, "target"));
        Directory.SetCurrentDirectory(_tempDir);
    }

    public void Dispose()
    {
        Directory.SetCurrentDirectory(_originalDir);
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best-effort cleanup */ }
    }

    [Fact]
    public void TestCreateSimpleForm()
    {
        CreateSimpleForm.Main(Array.Empty<string>());
        Assert.True(File.Exists(Path.Combine(_tempDir, "target", "SimpleForm.pdf")));
    }

    [Fact]
    public void TestAddBorderToField()
    {
        // AddBorderToField reads the PDF produced by CreateSimpleForm
        CreateSimpleForm.Main(Array.Empty<string>());
        AddBorderToField.Main(Array.Empty<string>());
        Assert.True(File.Exists(Path.Combine(_tempDir, "target", "AddBorderToField.pdf")));
    }

    [Fact]
    public void TestCreateSimpleFormWithEmbeddedFont()
    {
        string fontPath = ExampleTestResources.WriteLiberationSansRegular(_tempDir);

        CreateSimpleFormWithEmbeddedFont.Main(new string[] { fontPath });
        Assert.True(File.Exists(Path.Combine(_tempDir, "target", "SimpleFormWithEmbeddedFont.pdf")));
    }

    [Fact]
    public void TestCreateMultiWidgetsForm()
    {
        CreateMultiWidgetsForm.Main(Array.Empty<string>());
        Assert.True(File.Exists(Path.Combine(_tempDir, "target", "MultiWidgetsForm.pdf")));
    }

    [Fact]
    public void TestCreateCheckBox()
    {
        CreateCheckBox.Main(Array.Empty<string>());
        Assert.True(File.Exists(Path.Combine(_tempDir, "checkbox.pdf")));
    }

    [Fact]
    public void TestRadioButtons()
    {
        CreateRadioButtons.Main(Array.Empty<string>());
        Assert.True(File.Exists(Path.Combine(_tempDir, "radiobuttons.pdf")));
    }

    /// <summary>
    /// CreatePushButton creates a push button form field in a PDF.
    /// </summary>
    [Fact]
    public void TestCreatePushButton()
    {
        string outputFile = Path.Combine(_tempDir, "pushbutton.pdf");
        CreatePushButton.Main(new string[] { outputFile });
        Assert.True(File.Exists(outputFile));
    }
}
