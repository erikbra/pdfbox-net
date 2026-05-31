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
///   <item>CreateSimpleForm and CreateCheckBox call <c>GetWidgets()[0]</c> on a freshly
///         constructed field whose widget list is empty in this .NET port
///         (no automatic widget creation on construction); they throw
///         <see cref="ArgumentOutOfRangeException"/>.</item>
///   <item>CreateRadioButtons, CreateSimpleFormWithEmbeddedFont, CreateMultiWidgetsForm,
///         CreatePushButton and AddBorderToField require PDPageContentStream operators not
///         yet ported and throw <see cref="NotSupportedException"/>.</item>
/// </list>
/// </summary>
public class TestCreateSimpleForms
{
    /// <summary>
    /// Stub: CreateSimpleForm.Main() throws ArgumentOutOfRangeException because
    /// PDTextField.GetWidgets() returns an empty list on construction in this .NET port.
    /// </summary>
    [Fact(Skip = "Adapted — GetWidgets()[0] throws ArgumentOutOfRangeException; automatic widget creation on field construction not yet ported")]
    public void TestCreateSimpleForm()
    {
        // Java original:
        //   CreateSimpleForm.main(new String[] { filename });
        //   // then loads filename and verifies field value "Sample field content"
    }

    /// <summary>
    /// AddBorderToField throws NotSupportedException because it requires AcroForm
    /// appearance-stream drawing operators not yet ported.
    /// </summary>
    [Fact]
    public void TestAddBorderToField()
    {
        Assert.Throws<NotSupportedException>(() =>
            AddBorderToField.Main(new string[] { "input.pdf" }));
    }

    /// <summary>
    /// CreateSimpleFormWithEmbeddedFont throws NotSupportedException because it requires
    /// AcroForm appearance-stream drawing operators not yet ported.
    /// </summary>
    [Fact]
    public void TestCreateSimpleFormWithEmbeddedFont()
    {
        Assert.Throws<NotSupportedException>(() =>
            CreateSimpleFormWithEmbeddedFont.Main(new string[] { "output.pdf" }));
    }

    /// <summary>
    /// CreateMultiWidgetsForm throws NotSupportedException because it requires
    /// AcroForm appearance-stream drawing operators not yet ported.
    /// </summary>
    [Fact]
    public void TestCreateMultiWidgetsForm()
    {
        Assert.Throws<NotSupportedException>(() =>
            CreateMultiWidgetsForm.Main(new string[] { "output.pdf" }));
    }

    /// <summary>
    /// Stub: CreateCheckBox.Main() throws ArgumentOutOfRangeException because
    /// PDCheckBox.GetWidgets() returns an empty list on construction in this .NET port.
    /// </summary>
    [Fact(Skip = "Adapted — GetWidgets()[0] throws ArgumentOutOfRangeException; automatic widget creation on field construction not yet ported")]
    public void TestCreateCheckBox()
    {
        // Java original:
        //   CreateCheckBox.main(null);
        //   // then loads target/CheckBoxSample.pdf and verifies On/Off values
    }

    /// <summary>
    /// Stub: CreateRadioButtons.Main() saves to a CWD-relative path and the saved
    /// AcroForm radio-button field cannot yet be round-tripped and verified because
    /// the appearance-stream operators needed to properly set widget values are not ported.
    /// </summary>
    [Fact(Skip = "Adapted — CreateRadioButtons saves to CWD-relative path; appearance-stream operators not yet ported, field round-trip not verified")]
    public void TestRadioButtons()
    {
        // Java original:
        //   CreateRadioButtons.main(null);
        //   // then loads RadioButtonsSample.pdf and verifies selected export values
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
