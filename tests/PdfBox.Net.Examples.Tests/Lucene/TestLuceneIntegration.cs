// PDFBOX_SOURCE_PATH: examples/src/test/java/org/apache/pdfbox/examples/lucene/IndexFilesTest.java
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

using Lucene.Net.Documents;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using PdfBox.Net.Examples.Lucene;
using PdfBox.Net.PDModel;
using PdfBox.Net.Text;
using Xunit;

namespace PdfBox.Net.Examples.Tests.Lucene;

/// <summary>
/// Integration coverage for the Lucene example pair.
/// Verifies that <see cref="IndexPDFFiles"/> can index a small generated PDF
/// and that the resulting Lucene index is searchable.
/// </summary>
public sealed class TestLuceneIntegration : IDisposable
{
    private readonly string _tempDir =
        Path.Combine(Path.GetTempPath(), "pdfbox-lucene-tests-" + Guid.NewGuid().ToString("N")[..8]);

    public TestLuceneIntegration()
    {
        System.IO.Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try
        {
            System.IO.Directory.Delete(_tempDir, recursive: true);
        }
        catch
        {
            // Best-effort cleanup for temp files created during indexing.
        }
    }

    [Fact]
    public void IndexPDFFiles_CreatesSearchableIndexFromGeneratedPdf()
    {
        string docsDir = Path.Combine(_tempDir, "docs");
        string indexDir = Path.Combine(_tempDir, "index");
        System.IO.Directory.CreateDirectory(docsDir);
        System.IO.Directory.CreateDirectory(indexDir);

        string pdfPath = Path.Combine(docsDir, "lucene-sample.pdf");
        CreateMinimalSearchablePdf(pdfPath, "Lucene integration works");

        using (PDDocument loaded = PDDocument.Load(pdfPath))
        using (StringWriter writer = new())
        {
            PDFTextStripper stripper = new();
            stripper.WriteText(loaded, writer);
            string extractedText = writer.ToString();
            Assert.NotEmpty(extractedText.Trim());

            string searchTerm = extractedText
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                .First()
                .ToLowerInvariant();
            Assert.NotEmpty(searchTerm);

            Document luceneDocument = LucenePDFDocument.GetDocument(pdfPath);
            Assert.Equal(pdfPath, luceneDocument.Get("path"));

            using FSDirectory directory = FSDirectory.Open(new DirectoryInfo(indexDir));
            using StandardAnalyzer analyzer = new(LuceneVersion.LUCENE_48);
            IndexWriterConfig config = new(LuceneVersion.LUCENE_48, analyzer)
            {
                OpenMode = OpenMode.CREATE
            };

            using (IndexWriter indexWriter = new(directory, config))
            {
                indexWriter.AddDocument(luceneDocument);
            }

            using DirectoryReader reader = DirectoryReader.Open(directory);
            IndexSearcher searcher = new(reader);

            Assert.Equal(1, reader.NumDocs);

            TopDocs hits = searcher.Search(new TermQuery(new Term("contents", searchTerm)), 10);

            Assert.Single(hits.ScoreDocs);

            Document storedDocument = searcher.Doc(hits.ScoreDocs[0].Doc);
            Assert.Equal(pdfPath, storedDocument.Get("path"));
            Assert.Contains(searchTerm, storedDocument.Get("summary")!.ToLowerInvariant());
        }
    }

    private static void CreateMinimalSearchablePdf(string path, string text)
    {
        byte[] contentBytes = System.Text.Encoding.ASCII.GetBytes(
            "BT\n/F1 12 Tf\n100 700 Td\n(" + EscapePdfString(text) + ") Tj\nET\n");

        List<byte[]> objects =
        [
            System.Text.Encoding.ASCII.GetBytes("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n"),
            System.Text.Encoding.ASCII.GetBytes("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n"),
            System.Text.Encoding.ASCII.GetBytes(
                "3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] "
                + "/Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>\nendobj\n"),
            System.Text.Encoding.ASCII.GetBytes(
                "4 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n"),
            BuildContentStreamObject(contentBytes),
        ];

        using MemoryStream stream = new();
        stream.Write(System.Text.Encoding.ASCII.GetBytes("%PDF-1.4\n"));

        List<int> offsets = [0];
        foreach (byte[] obj in objects)
        {
            offsets.Add((int)stream.Position);
            stream.Write(obj, 0, obj.Length);
        }

        int xrefOffset = (int)stream.Position;
        WriteAscii(stream, "xref\n0 6\n");
        WriteAscii(stream, "0000000000 65535 f \n");
        for (int i = 1; i <= 5; i++)
        {
            WriteAscii(stream, offsets[i].ToString("D10", System.Globalization.CultureInfo.InvariantCulture));
            WriteAscii(stream, " 00000 n \n");
        }

        WriteAscii(stream, "trailer\n<< /Root 1 0 R /Size 6 >>\nstartxref\n");
        WriteAscii(stream, xrefOffset.ToString(System.Globalization.CultureInfo.InvariantCulture));
        WriteAscii(stream, "\n%%EOF\n");

        File.WriteAllBytes(path, stream.ToArray());
    }

    private static byte[] BuildContentStreamObject(byte[] contentBytes)
    {
        using MemoryStream stream = new();
        WriteAscii(stream, "5 0 obj\n<< /Length ");
        WriteAscii(stream, contentBytes.Length.ToString(System.Globalization.CultureInfo.InvariantCulture));
        WriteAscii(stream, " >>\nstream\n");
        stream.Write(contentBytes, 0, contentBytes.Length);
        WriteAscii(stream, "endstream\nendobj\n");
        return stream.ToArray();
    }

    private static void WriteAscii(Stream stream, string value)
    {
        byte[] bytes = System.Text.Encoding.ASCII.GetBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    }

    private static string EscapePdfString(string value)
    {
        return value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
    }
}
