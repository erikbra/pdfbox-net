// PDFBOX_SOURCE_PATH: examples/src/test/java/org/apache/pdfbox/examples/interactive/form/TestCreateSimpleForms.java
// PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
// PORT_MODE: adapted
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
/// Ported from TestCreateSimpleForms.java — adapted because:
/// <list type="bullet">
///   <item>CreatePushButton requires AcroForm appearance-stream drawing operators not yet ported.</item>
///   <item>CreateSimpleFormWithEmbeddedFont needs a TrueType font file at test runtime.</item>
/// </list>
/// Examples that save to a relative <c>target/</c> path are run from a temporary directory
/// that contains a pre-created <c>target/</c> subdirectory, matching the Maven convention.
/// </summary>
public class TestCreateSimpleForms : IDisposable
{
    private static readonly string LiberationSansRegular =
        "/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf";

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
        if (!File.Exists(LiberationSansRegular))
            Assert.Skip("LiberationSans-Regular.ttf not available on this system");

        CreateSimpleFormWithEmbeddedFont.Main(new string[] { LiberationSansRegular });
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
    /// CreatePushButton throws NotSupportedException because it requires AcroForm
    /// appearance-stream drawing operators not yet ported.
    /// </summary>
    [Fact]
    public void TestCreatePushButton()
    {
        Assert.Throws<NotSupportedException>(() =>
            CreatePushButton.Main(new string[] { "output.pdf" }));
    }
}
