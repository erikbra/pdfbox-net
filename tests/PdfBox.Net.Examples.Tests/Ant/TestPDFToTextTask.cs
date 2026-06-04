// PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/ant/PDFToTextTask.java
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

using PdfBox.Net.Examples.Ant;

namespace PdfBox.Net.Examples.Tests.Ant;

public class TestPDFToTextTask : IDisposable
{
    private readonly string _tempDir =
        Path.Combine(Path.GetTempPath(), "pdfbox-ant-tests-" + Guid.NewGuid().ToString("N")[..8]);

    public TestPDFToTextTask()
    {
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best-effort cleanup */ }
    }

    [Fact]
    public void ExecuteWritesExtractedText()
    {
        string pdfPath = GetFixturePath("minimal-document-fixture.pdf");
        string textPath = Path.Combine(_tempDir, "execute.txt");

        PDFToTextTask task = new()
        {
            PdfFile = pdfPath,
            OutputFile = textPath
        };

        task.Execute();

        Assert.True(File.Exists(textPath));
        Assert.Equal(string.Empty, File.ReadAllText(textPath));
    }

    [Fact]
    public void MainWritesExtractedText()
    {
        string pdfPath = GetFixturePath("minimal-document-fixture.pdf");
        string textPath = Path.Combine(_tempDir, "main.txt");

        PDFToTextTask.Main(new[] { pdfPath, textPath });

        Assert.True(File.Exists(textPath));
        Assert.Equal(string.Empty, File.ReadAllText(textPath));
    }

    private static string GetFixturePath(string fixtureName)
    {
        return Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..",
            "PdfBox.Net.Tests",
            "Fixtures",
            fixtureName));
    }
}
