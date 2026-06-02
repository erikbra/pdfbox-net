// PDFBOX_SOURCE_PATH: examples/src/test/java/org/apache/pdfbox/examples/pdmodel/TestCreateGradientShadingPDF.java
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

using PdfBox.Net.Examples.PDModel;
using PdfBox.Net.ContentStream;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.Examples.Tests.PDModel;

/// <summary>
/// Test of CreateGradientShadingPDF example.
/// Ported from TestCreateGradientShadingPDF.java.
/// </summary>
public class TestCreateGradientShadingPDF
{
    [Fact]
    public void TestCreateGradientShading()
    {
        string filename = Path.Combine(Path.GetTempPath(), "GradientShading.pdf");
        CreateGradientShadingPDF creator = new CreateGradientShadingPDF();
        File.Delete(filename);

        creator.Create(filename);

        Assert.True(File.Exists(filename));

        using PDDocument document = PDDocument.Load(filename);
        PDResources resources = Assert.IsType<PDResources>(document.GetPage(0).GetResources());
        COSName[] shadingNames = resources.GetShadingNames().ToArray();
        Assert.Equal(3, shadingNames.Length);
        Assert.All(shadingNames, name => Assert.NotNull(resources.GetShading(name)));

        using Stream stream = ((PDContentStream)document.GetPage(0)).GetContents()!;
        using StreamReader reader = new(stream);
        string content = reader.ReadToEnd();
        Assert.Equal(3, shadingNames.Count(name => content.Contains($"/{name.GetName()} sh")));
    }
}
