// PDFBOX_SOURCE_PATH: examples/src/test/java/org/apache/pdfbox/examples/pdmodel/TestRubberStampWithImage.java
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

using PdfBox.Net.Examples.PDModel;

namespace PdfBox.Net.Examples.Tests.PDModel;

/// <summary>
/// Test for RubberStampWithImage example.
/// Ported from TestRubberStampWithImage.java — adapted because:
/// <list type="bullet">
///   <item>The Java test uses fixture files (document.pdf, stamp.jpg) not yet included in
///         the .NET test resources.</item>
///   <item>The example's image drawing in annotation appearance streams is not yet implemented
///         (<c>PDImageXObject.CreateFromFile</c> / appearance stream drawing throws
///         <see cref="NotSupportedException"/>).</item>
/// </list>
/// </summary>
public class TestRubberStampWithImage
{
    /// <summary>
    /// Stub: requires test fixture PDFs (document.pdf, stamp.jpg) and
    /// PDImageXObject appearance-stream drawing not yet ported.
    /// </summary>
    [Fact(Skip = "Adapted — requires test fixture PDF/image and image appearance-stream drawing not yet ported")]
    public void Test()
    {
        // Java original:
        //   RubberStampWithImage rubberStamp = new RubberStampWithImage();
        //   rubberStamp.doIt(new String[]{ documentFile, outFile, stampFile });
        // Not yet implemented in this .NET port.
    }
}
