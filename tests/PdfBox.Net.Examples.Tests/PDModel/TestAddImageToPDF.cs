// PDFBOX_SOURCE_PATH: examples/src/test/java/org/apache/pdfbox/examples/pdmodel/TestAddImageToPDF.java
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
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Examples.PDModel;
using SkiaSharp;

namespace PdfBox.Net.Examples.Tests.PDModel;

/// <summary>
/// Test of the AddImageToPDF example.
/// Fixture PDFs and images are synthesised programmatically.
/// </summary>
public class TestAddImageToPDF
{
    private static readonly string OutputDir =
        Path.Combine(Path.GetTempPath(), "pdfbox-examples-tests-addimagetopdf");

    public TestAddImageToPDF()
    {
        Directory.CreateDirectory(OutputDir);
    }

    [Fact]
    public void TestAddImageCreatesOutputFile()
    {
        string inputPdf = CreateTempPdf("addimagetopdf-input.pdf");
        string imagePath = CreateTempJpeg("addimagetopdf-input.jpg");
        string outputFile = Path.Combine(OutputDir, "AddImageToPDF.pdf");
        File.Delete(outputFile);

        AddImageToPDF app = new AddImageToPDF();
        app.CreatePDFFromImage(inputPdf, imagePath, outputFile);

        Assert.True(File.Exists(outputFile), "AddImageToPDF should have created the output PDF");
    }

    [Fact]
    public void TestAddImageEmbeddedImageXObject()
    {
        string inputPdf = CreateTempPdf("addimagetopdf-xobj-input.pdf");
        string imagePath = CreateTempJpeg("addimagetopdf-xobj.jpg");
        string outputFile = Path.Combine(OutputDir, "AddImageToPDF-xobj.pdf");
        File.Delete(outputFile);

        AddImageToPDF app = new AddImageToPDF();
        app.CreatePDFFromImage(inputPdf, imagePath, outputFile);

        // Reload and verify that at least one image XObject was appended.
        using PDDocument doc = Loader.LoadPDF(outputFile);
        PDPage page = doc.GetPage(0);
        PDResources? resources = page.GetResources();
        Assert.NotNull(resources);
        Assert.NotEmpty(resources!.GetXObjectNames());
        COSName imageName = Assert.Single(resources.GetXObjectNames());
        Assert.True(resources.IsImageXObject(imageName),
            "The appended XObject should be an image XObject");
    }

    /// <summary>Creates a minimal single-page PDF in a temp file and returns its path.</summary>
    private static string CreateTempPdf(string filename)
    {
        string path = Path.Combine(Path.GetTempPath(), filename);
        using PDDocument doc = new();
        doc.AddPage(new PDPage());
        doc.Save(path);
        return path;
    }

    /// <summary>Writes a small synthetic JPEG to a temp file and returns its path.</summary>
    private static string CreateTempJpeg(string filename)
    {
        string path = Path.Combine(Path.GetTempPath(), filename);
        using SKBitmap bitmap = new(4, 4);
        bitmap.SetPixel(0, 0, new SKColor(255, 0, 0));
        using SKImage skImage = SKImage.FromBitmap(bitmap);
        using SKData encoded = skImage.Encode(SKEncodedImageFormat.Jpeg, 90);
        File.WriteAllBytes(path, encoded.ToArray());
        return path;
    }
}
