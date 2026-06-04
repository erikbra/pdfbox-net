// PDFBOX_SOURCE_PATH: examples/src/test/java/org/apache/pdfbox/examples/pdmodel/ (no direct Java equivalent)
// PORT_MODE: mechanical

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
using PdfBox.Net.PDModel;
using System.Text;

namespace PdfBox.Net.Examples.Tests.PDModel;

/// <summary>
/// Tests for examples that exercise text matrix, character/word spacing, and TJ operators.
/// </summary>
public class TestTextOperators
{
    private static readonly string OutputDir =
        Path.Combine(Path.GetTempPath(), "pdfbox-examples-tests-textops");

    public TestTextOperators()
    {
        Directory.CreateDirectory(OutputDir);
    }

    [Fact]
    public void TestUsingTextMatrixCreatesFile()
    {
        string outputFile = Path.Combine(OutputDir, "UsingTextMatrix.pdf");
        File.Delete(outputFile);
        UsingTextMatrix.Main(new[] { "Hello World!", outputFile });
        Assert.True(File.Exists(outputFile), "UsingTextMatrix should have created the PDF");
        Assert.True(new FileInfo(outputFile).Length > 0, "Output PDF should not be empty");
        Assert.Contains("Tm", ReadAllPageContents(outputFile));
    }

    [Fact]
    public void TestShowTextWithPositioningCreatesFile()
    {
        string outputFile = Path.Combine(OutputDir, "ShowTextWithPositioning.pdf");
        File.Delete(outputFile);
        ShowTextWithPositioning.DoIt("Hello World, this is a test!", outputFile);
        Assert.True(File.Exists(outputFile), "ShowTextWithPositioning should have created the PDF");
        Assert.True(new FileInfo(outputFile).Length > 0, "Output PDF should not be empty");
        string content = ReadAllPageContents(outputFile);
        Assert.Contains("Tm", content);
        Assert.Contains("Tw", content);
        Assert.Contains("TJ", content);
    }

    [Fact]
    public void TestBengaliPdfGenerationCreatesFile()
    {
        string outputFile = Path.Combine(OutputDir, "BengaliPdfGenerationHelloWorld.pdf");
        File.Delete(outputFile);
        BengaliPdfGenerationHelloWorld.Main(new[] { outputFile });
        Assert.True(File.Exists(outputFile),
            "BengaliPdfGenerationHelloWorld should have created the PDF");
        Assert.True(new FileInfo(outputFile).Length > 0, "Output PDF should not be empty");
        Assert.Contains("Td", ReadAllPageContents(outputFile));
    }

    private static string ReadAllPageContents(string pdfPath)
    {
        using PDDocument document = PDDocument.Load(pdfPath);
        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < document.GetNumberOfPages(); i++)
        {
            using Stream stream = ((PDContentStream)document.GetPage(i)).GetContents()!;
            using StreamReader reader = new StreamReader(stream, Encoding.ASCII);
            builder.Append(reader.ReadToEnd());
        }

        return builder.ToString();
    }
}
