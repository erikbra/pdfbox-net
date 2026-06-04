/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/CreatePortableCollection.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: mechanical
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
/// This is an example on how to create a portable collection PDF. It uses the COS methods because
/// there are not any PD classes yet.
/// </summary>
public class CreatePortableCollection
{
    private CreatePortableCollection()
    {
    }

    /// <summary>
    /// Create a portable collection PDF with two files.
    /// </summary>
    /// <param name="file">The file to write the PDF to.</param>
    public void DoIt(string file)
    {
        using (PDDocument doc = new PDDocument())
        {
            PDPage page = new PDPage();
            doc.AddPage(page);

            PDEmbeddedFilesNameTreeNode efTree = new PDEmbeddedFilesNameTreeNode();

            PDComplexFileSpecification fs1 = new PDComplexFileSpecification();
            fs1.SetFile("Test1.txt");
            fs1.SetFileUnicode("Test1.txt");
            byte[] data1 = Encoding.Latin1.GetBytes("This is the contents of the first embedded file");
            using MemoryStream ms1 = new MemoryStream(data1);
            PDEmbeddedFile ef1 = new PDEmbeddedFile(doc, ms1, COSName.FLATE_DECODE);
            ef1.SetSubtype("text/plain");
            ef1.SetSize(data1.Length);
            ef1.SetCreationDate(DateTimeOffset.UtcNow);
            fs1.SetEmbeddedFile(ef1);
            fs1.SetEmbeddedFileUnicode(ef1);
            fs1.SetFileDescription("The first file");

            PDComplexFileSpecification fs2 = new PDComplexFileSpecification();
            fs2.SetFile("Test2.txt");
            fs2.SetFileUnicode("Test2.txt");
            byte[] data2 = Encoding.Latin1.GetBytes("This is the contents of the second embedded file");
            using MemoryStream ms2 = new MemoryStream(data2);
            PDEmbeddedFile ef2 = new PDEmbeddedFile(doc, ms2, COSName.FLATE_DECODE);
            ef2.SetSubtype("text/plain");
            ef2.SetSize(data2.Length);
            ef2.SetCreationDate(DateTimeOffset.UtcNow);
            fs2.SetEmbeddedFile(ef2);
            fs2.SetEmbeddedFileUnicode(ef2);
            fs2.SetFileDescription("The second file");

            PDEmbeddedFilesNameTreeNode treeNode = new PDEmbeddedFilesNameTreeNode();
            treeNode.SetNames(new Dictionary<string, PDComplexFileSpecification>
            {
                ["Attachment 1"] = fs1,
                ["Attachment 2"] = fs2,
            });
            efTree.SetKids(new List<PDNameTreeNode<PDComplexFileSpecification>> { treeNode });

            PDDocumentNameDictionary names = new PDDocumentNameDictionary(doc.GetDocumentCatalog());
            names.SetEmbeddedFiles(efTree);
            doc.GetDocumentCatalog().SetNames(names);
            doc.GetDocumentCatalog().SetPageMode(PageMode.UseAttachments);
            doc.GetDocumentCatalog().SetVersion("1.7");

            doc.Save(file);
        }
    }

    public static void Main(string[] args)
    {
        CreatePortableCollection app = new CreatePortableCollection();
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
        Console.Error.WriteLine("usage: CreatePortableCollection <output-file>");
    }
}
