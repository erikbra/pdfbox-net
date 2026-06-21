// PDFBOX_SOURCE_PATH: examples/src/test/java/org/apache/pdfbox/examples/pdmodel/TestRubberStampWithImage.java
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
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.Examples.PDModel;
using SkiaSharp;

namespace PdfBox.Net.Examples.Tests.PDModel;

/// <summary>
/// Test for RubberStampWithImage example.
/// Ported from TestRubberStampWithImage.java with programmatically created fixture files.
/// </summary>
public class TestRubberStampWithImage
{
    private static readonly string OutputDir =
        Path.Combine(Path.GetTempPath(), "pdfbox-examples-tests-rubberstampwithimage");

    public TestRubberStampWithImage()
    {
        Directory.CreateDirectory(OutputDir);
    }

    [Fact]
    public void TestRubberStampWithImageCreatesFile()
    {
        string inputPdf = CreateTempPdf("rubberstamp-input.pdf");
        string imagePath = CreateTempJpeg("rubberstamp-stamp.jpg");
        string outputFile = Path.Combine(OutputDir, "RubberStampWithImage.pdf");
        File.Delete(outputFile);

        RubberStampWithImage.Main(new[] { inputPdf, outputFile, imagePath });

        Assert.True(File.Exists(outputFile),
            "RubberStampWithImage should have created the output PDF");
    }

    [Fact]
    public void TestRubberStampWithImageHasAnnotation()
    {
        string inputPdf = CreateTempPdf("rubberstamp-annot-input.pdf");
        string imagePath = CreateTempJpeg("rubberstamp-annot-stamp.jpg");
        string outputFile = Path.Combine(OutputDir, "RubberStampWithImage-annot.pdf");
        File.Delete(outputFile);

        RubberStampWithImage.Main(new[] { inputPdf, outputFile, imagePath });

        using PDDocument doc = Loader.LoadPDF(outputFile);
        PDPage page = doc.GetPage(0);
        IList<PDAnnotation> annotations = page.GetAnnotations();
        Assert.NotEmpty(annotations);
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
