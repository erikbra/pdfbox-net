/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/EmbeddedFiles.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 */

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
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Common.FileSpecification;

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// This is an example that creates a simple document and embeds a file into it.
/// </summary>
public class EmbeddedFiles
{
    private EmbeddedFiles()
    {
    }

    /// <summary>
    /// Create the second sample document from the PDF file format specification.
    /// </summary>
    /// <param name="file">The file to write the PDF to.</param>
    public void DoIt(string file)
    {
        using (PDDocument doc = new PDDocument())
        {
            PDPage page = new PDPage();
            doc.AddPage(page);

            // NOTE: PDType1Font construction from FontName enum and PDPageContentStream text
            // drawing operators are not yet implemented in this .NET port.
            // The embedded file structure below is ported fully.

            PDEmbeddedFilesNameTreeNode efTree = new PDEmbeddedFilesNameTreeNode();

            PDComplexFileSpecification fs = new PDComplexFileSpecification();
            fs.SetFile("Test.txt");
            fs.SetFileUnicode("Test.txt");

            byte[] data = Encoding.Latin1.GetBytes("This is the contents of the embedded file");
            using MemoryStream fakeFile = new MemoryStream(data);
            PDEmbeddedFile ef = new PDEmbeddedFile(doc, fakeFile);
            ef.SetSubtype("text/plain");
            ef.SetSize(data.Length);
            ef.SetCreationDate(DateTimeOffset.UtcNow);
            fs.SetEmbeddedFile(ef);
            fs.SetEmbeddedFileUnicode(ef);
            fs.SetFileDescription("Very interesting file");

            PDEmbeddedFilesNameTreeNode treeNode = new PDEmbeddedFilesNameTreeNode();
            treeNode.SetNames(new Dictionary<string, PDComplexFileSpecification>
            {
                ["My first attachment"] = fs,
            });
            efTree.SetKids(new List<PDNameTreeNode<PDComplexFileSpecification>> { treeNode });

            PDDocumentNameDictionary names = new PDDocumentNameDictionary(doc.GetDocumentCatalog());
            names.SetEmbeddedFiles(efTree);
            doc.GetDocumentCatalog().SetNames(names);
            doc.GetDocumentCatalog().SetPageMode(PageMode.UseAttachments);

            doc.Save(file);
        }
    }

    public static void Main(string[] args)
    {
        EmbeddedFiles app = new EmbeddedFiles();
        if (args.Length != 1)
        {
            app.Usage();
        }
        else
        {
            app.DoIt(args[0]);
        }
    }

    private void Usage()
    {
        Console.Error.WriteLine("usage: EmbeddedFiles <output-file>");
    }
}
