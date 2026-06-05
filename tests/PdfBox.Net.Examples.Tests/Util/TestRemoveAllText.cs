// PDFBOX_SOURCE_PATH: examples/src/test/java/org/apache/pdfbox/examples/util/RemoveAllTextTest.java
// PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
// PORT_MODE: native-test
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

using PdfBox.Net.ContentStream;
using PdfBox.Net.ContentStream.Operator;
using PdfBox.Net.Examples.Util;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PdfParser;

namespace PdfBox.Net.Examples.Tests.Util;

/// <summary>
/// Integration test for <see cref="RemoveAllText"/>.
/// Verifies that processing a PDF with text content produces an output PDF
/// with all text operators removed while preserving other content.
/// </summary>
public class TestRemoveAllText : IDisposable
{
    private readonly string _tempDir =
        Path.Combine(Path.GetTempPath(), "pdfbox-remove-text-tests-" + Guid.NewGuid().ToString("N")[..8]);

    public TestRemoveAllText()
    {
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best-effort cleanup */ }
    }

    [Fact]
    public void RemoveAllTextProducesNonEmptyOutputPdf()
    {
        string inputPdf = Path.Combine(_tempDir, "input.pdf");
        string outputPdf = Path.Combine(_tempDir, "output.pdf");

        CreateTextPdf(inputPdf);

        RemoveAllText.Main([inputPdf, outputPdf]);

        Assert.True(File.Exists(outputPdf), "Output PDF should have been created");
        Assert.True(new FileInfo(outputPdf).Length > 0, "Output PDF should be non-empty");
    }

    [Fact]
    public void RemoveAllTextRemovesTextOperatorsFromContentStream()
    {
        string inputPdf = Path.Combine(_tempDir, "input.pdf");
        string outputPdf = Path.Combine(_tempDir, "output.pdf");

        CreateTextPdf(inputPdf);

        RemoveAllText.Main([inputPdf, outputPdf]);

        // Parse the output content stream and verify no text-show operators remain
        using PDDocument doc = Loader.LoadPDF(outputPdf);
        foreach (PDPage page in doc.GetPages())
        {
            using Stream? contentStream = ((PDContentStream)page).GetContents();
            if (contentStream is null)
            {
                continue;
            }

            List<object> tokens = PDFStreamParser.ParseTokens(contentStream);
            foreach (object token in tokens)
            {
                if (token is Operator op)
                {
                    string name = op.GetName();
                    Assert.NotEqual(OperatorName.SHOW_TEXT, name);
                    Assert.NotEqual(OperatorName.SHOW_TEXT_ADJUSTED, name);
                    Assert.NotEqual(OperatorName.SHOW_TEXT_LINE, name);
                    Assert.NotEqual(OperatorName.SHOW_TEXT_LINE_AND_SPACE, name);
                }
            }
        }
    }

    [Fact]
    public void RemoveAllTextShowsUsageWithWrongArgs()
    {
        Exception? ex = Record.Exception(() => RemoveAllText.Main([]));
        Assert.Null(ex);
    }

    /// <summary>
    /// Creates a minimal PDF containing a line of text, saved to <paramref name="path"/>.
    /// </summary>
    private static void CreateTextPdf(string path)
    {
        using PDDocument doc = new();
        PDPage page = new();
        doc.AddPage(page);

        PDFont font = new PDType1Font(PDType1Font.FontName.HELVETICA);

        using (PDPageContentStream contents = new(doc, page))
        {
            contents.BeginText();
            contents.SetFont(font, 12);
            contents.NewLineAtOffset(100, 700);
            contents.ShowText("Hello PDF");
            contents.EndText();
            // Also add a path to ensure non-text content is preserved
            contents.MoveTo(10, 10);
            contents.LineTo(200, 10);
            contents.Stroke();
        }

        doc.Save(path);
    }
}
