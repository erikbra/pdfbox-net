// PDFBOX_SOURCE_PATH: examples/src/test/java/org/apache/pdfbox/examples/pdmodel/TestImageToPDF.java
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
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Examples.PDModel;
using SkiaSharp;

namespace PdfBox.Net.Examples.Tests.PDModel;

/// <summary>
/// Test of the ImageToPDF example.
/// Adapted from the Java equivalent — fixture images are synthesised via SkiaSharp.
/// </summary>
public class TestImageToPDF
{
    private static readonly string OutputDir =
        Path.Combine(Path.GetTempPath(), "pdfbox-examples-tests-imagetopdf");

    public TestImageToPDF()
    {
        Directory.CreateDirectory(OutputDir);
    }

    [Fact]
    public void TestImageToPDFCreatesFile()
    {
        string imagePath = CreateTempJpeg("imagetopdf-input.jpg");
        string outputFile = Path.Combine(OutputDir, "ImageToPDF.pdf");
        File.Delete(outputFile);

        ImageToPDF.Main(new[] { imagePath, outputFile });

        Assert.True(File.Exists(outputFile), "ImageToPDF should have created the PDF");
    }

    [Fact]
    public void TestImageToPDFEmbeddedImageXObject()
    {
        string imagePath = CreateTempJpeg("imagetopdf-xobj.jpg");
        string outputFile = Path.Combine(OutputDir, "ImageToPDF-xobj.pdf");
        File.Delete(outputFile);

        ImageToPDF.Main(new[] { imagePath, outputFile });

        // Reload and verify that at least one image XObject is embedded.
        using PDDocument doc = Loader.LoadPDF(outputFile);
        PDPage page = doc.GetPage(0);
        PDResources? resources = page.GetResources();
        Assert.NotNull(resources);
        Assert.NotEmpty(resources!.GetXObjectNames());
        COSName imageName = Assert.Single(resources.GetXObjectNames());
        Assert.True(resources.IsImageXObject(imageName),
            "The embedded XObject should be an image XObject");
    }

    [Fact]
    public void TestImageToPDFEmbeddedImageXObjectFromPng()
    {
        string imagePath = CreateTempPng("imagetopdf-xobj.png");
        string outputFile = Path.Combine(OutputDir, "ImageToPDF-xobj-png.pdf");
        File.Delete(outputFile);

        ImageToPDF.Main(new[] { imagePath, outputFile });

        Assert.True(File.Exists(outputFile), "ImageToPDF should have created the PDF");

        using PDDocument doc = Loader.LoadPDF(outputFile);
        PDPage page = doc.GetPage(0);
        PDResources? resources = page.GetResources();
        Assert.NotNull(resources);
        Assert.NotEmpty(resources!.GetXObjectNames());
        COSName imageName = Assert.Single(resources.GetXObjectNames());
        Assert.True(resources.IsImageXObject(imageName),
            "The embedded XObject should be an image XObject");
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

    /// <summary>Writes a small synthetic PNG to a temp file and returns its path.</summary>
    private static string CreateTempPng(string filename)
    {
        string path = Path.Combine(Path.GetTempPath(), filename);
        using SKBitmap bitmap = new(4, 4);
        bitmap.SetPixel(0, 0, new SKColor(255, 0, 0));
        using SKImage skImage = SKImage.FromBitmap(bitmap);
        using SKData encoded = skImage.Encode(SKEncodedImageFormat.Png, 100);
        File.WriteAllBytes(path, encoded.ToArray());
        return path;
    }
}
