// PDFBOX_SOURCE_PATH: examples/src/test/java/org/apache/pdfbox/examples/pdmodel/TestEmbeddedFiles.java
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

using System.Text;
using PdfBox.Net;
using PdfBox.Net.Examples.PDModel;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Common.FileSpecification;

namespace PdfBox.Net.Examples.Tests.PDModel;

/// <summary>
/// Test for EmbeddedFiles, ExtractEmbeddedFiles and CreatePortableCollection examples.
/// Ported from TestEmbeddedFiles.java — adapted because:
/// <list type="bullet">
///   <item>EmbeddedFiles omits page-level text drawing (PDType1Font not yet ported) but the
///         embedded-file structure is fully functional; tests verify the PDF structure by
///         loading it with Loader rather than via ExtractEmbeddedFiles.</item>
///   <item>The .NET port of ExtractEmbeddedFiles uses the name-tree key as the output
///         filename (not the embedded file spec's filename as Java does), so the
///         round-trip via ExtractEmbeddedFiles is not tested here.</item>
/// </list>
/// </summary>
public class TestEmbeddedFiles
{
    private static readonly string OutputDir =
        Path.Combine(Path.GetTempPath(), "pdfbox-examples-embedded-tests");

    public TestEmbeddedFiles()
    {
        Directory.CreateDirectory(OutputDir);
    }

    /// <summary>
    /// Verifies that EmbeddedFiles.DoIt() creates a PDF containing an embedded file
    /// with the expected name and content.
    /// </summary>
    [Fact]
    public void TestEmbeddedFilesStructure()
    {
        string outputFile = Path.Combine(OutputDir, $"EmbeddedFile-{Guid.NewGuid():N}.pdf");

        EmbeddedFiles.Main(new string[] { outputFile });

        Assert.True(File.Exists(outputFile));

        using (PDDocument doc = Loader.LoadPDF(outputFile))
        {
            PDDocumentNameDictionary names = new PDDocumentNameDictionary(doc.GetDocumentCatalog());
            PDEmbeddedFilesNameTreeNode? efTree = names.GetEmbeddedFiles();
            Assert.NotNull(efTree);

            // Collect all embedded files from the tree
            var allFiles = new List<(string Key, PDComplexFileSpecification Spec)>();
            CollectFiles(efTree, allFiles);

            Assert.NotEmpty(allFiles);

            // The embedded file content should match what EmbeddedFiles.DoIt() wrote.
            string expectedContent = "This is the contents of the embedded file";
            bool found = allFiles.Any(entry =>
            {
                PDEmbeddedFile? ef = entry.Spec.GetEmbeddedFile();
                if (ef == null) return false;
                string content = Encoding.Latin1.GetString(ef.ToByteArray());
                return content == expectedContent;
            });
            Assert.True(found, "Expected embedded file with the correct content was not found.");
        }

        File.Delete(outputFile);
    }

    /// <summary>
    /// Verifies that CreatePortableCollection.DoIt() creates a PDF containing two embedded
    /// files with the expected content.
    /// </summary>
    [Fact]
    public void TestCreatePortableCollectionStructure()
    {
        string outputFile = Path.Combine(OutputDir, $"PortableCollection-{Guid.NewGuid():N}.pdf");

        CreatePortableCollection.Main(new string[] { outputFile });

        Assert.True(File.Exists(outputFile));

        using (PDDocument doc = Loader.LoadPDF(outputFile))
        {
            PDDocumentNameDictionary names = new PDDocumentNameDictionary(doc.GetDocumentCatalog());
            PDEmbeddedFilesNameTreeNode? efTree = names.GetEmbeddedFiles();
            Assert.NotNull(efTree);

            var allFiles = new List<(string Key, PDComplexFileSpecification Spec)>();
            CollectFiles(efTree, allFiles);

            Assert.Equal(2, allFiles.Count);

            string[] contents = allFiles
                .Select(e => e.Spec.GetEmbeddedFile())
                .Where(ef => ef != null)
                .Select(ef => Encoding.Latin1.GetString(ef!.ToByteArray()))
                .ToArray();

            Assert.Contains("This is the contents of the first embedded file", contents);
            Assert.Contains("This is the contents of the second embedded file", contents);
        }

        File.Delete(outputFile);
    }

    private static void CollectFiles(
        PDNameTreeNode<PDComplexFileSpecification> node,
        List<(string, PDComplexFileSpecification)> result)
    {
        IReadOnlyDictionary<string, PDComplexFileSpecification>? namesMap = node.GetNames();
        if (namesMap != null)
        {
            foreach (var entry in namesMap)
                result.Add((entry.Key, entry.Value));
        }

        IList<PDNameTreeNode<PDComplexFileSpecification>>? kids = node.GetKids();
        if (kids != null)
        {
            foreach (var kid in kids)
                CollectFiles(kid, result);
        }
    }
}
