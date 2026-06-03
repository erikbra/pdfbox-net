/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/lucene/IndexFiles.java
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

using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;

namespace PdfBox.Net.Examples.Lucene;

/// <summary>
/// Indexes all PDF files under a directory using Lucene.Net.
/// <para>
/// This is a command-line application demonstrating simple Lucene indexing.
/// Run it with no command-line arguments for usage information.
/// </para>
/// </summary>
public sealed class IndexPDFFiles
{
    private IndexPDFFiles()
    {
    }

    /// <summary>
    /// Index all PDF files under a directory.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    public static void Main(string[] args)
    {
        string usage = "dotnet run -- [-index INDEX_PATH] [-docs DOCS_PATH] [-update]\n\n"
            + "This indexes all PDF documents in DOCS_PATH, creating a Lucene index "
            + "in INDEX_PATH that can be searched with SearchFiles";
        string indexPath = "index";
        string? docsPath = null;
        bool create = true;

        for (int i = 0; i < args.Length; i++)
        {
            if ("-index".Equals(args[i], StringComparison.Ordinal) && i + 1 < args.Length)
            {
                indexPath = args[++i];
            }
            else if ("-docs".Equals(args[i], StringComparison.Ordinal) && i + 1 < args.Length)
            {
                docsPath = args[++i];
            }
            else if ("-update".Equals(args[i], StringComparison.Ordinal))
            {
                create = false;
            }
        }

        if (docsPath == null)
        {
            Console.Error.WriteLine("Usage: " + usage);
            return;
        }

        if (!System.IO.Directory.Exists(docsPath))
        {
            Console.WriteLine("Document directory '" + Path.GetFullPath(docsPath)
                + "' does not exist or is not readable, please check the path");
            return;
        }

        long start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        try
        {
            Console.WriteLine("Indexing to directory '" + indexPath + "'...");

            using FSDirectory dir = FSDirectory.Open(new System.IO.DirectoryInfo(indexPath));
            using StandardAnalyzer analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);
            IndexWriterConfig iwc = new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer);

            if (create)
            {
                // Create a new index in the directory, removing any
                // previously indexed documents:
                iwc.OpenMode = OpenMode.CREATE;
            }
            else
            {
                // Add new documents to an existing index:
                iwc.OpenMode = OpenMode.CREATE_OR_APPEND;
            }

            // Optional: for better indexing performance, if you are indexing many
            // documents, increase the RAM buffer. But if you do this, increase the
            // max heap size to the runtime:
            //
            // iwc.RAMBufferSizeMB = 256.0;

            using IndexWriter writer = new IndexWriter(dir, iwc);
            IndexDocs(writer, docsPath);

            // NOTE: if you want to maximize search performance, you can optionally
            // call forceMerge here. This can be a terribly costly operation, so
            // generally it's only worth it when your index is relatively static:
            //
            // writer.ForceMerge(1);
        }
        catch (IOException e)
        {
            Console.WriteLine(" caught a " + e.GetType().Name + "\n with message: " + e.Message);
        }

        long end = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Console.WriteLine((end - start) + " total milliseconds");
    }

    /// <summary>
    /// Indexes the given file using the given writer, or if a directory is given, recurses
    /// over files and directories found under the given directory.
    /// </summary>
    /// <param name="writer">Writer to the index where the given file/dir info will be stored.</param>
    /// <param name="path">The file to index, or the directory to recurse into to find files to index.</param>
    /// <exception cref="IOException">If there is a low-level I/O error.</exception>
    internal static void IndexDocs(IndexWriter writer, string path)
    {
        if (System.IO.Directory.Exists(path))
        {
            foreach (string entry in System.IO.Directory.GetFileSystemEntries(path))
            {
                IndexDocs(writer, entry);
            }
        }
        else if (File.Exists(path))
        {
            try
            {
                if (path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Indexing PDF document: " + path);
                    Document doc = LucenePDFDocument.GetDocument(path);

                    if (writer.Config.OpenMode == OpenMode.CREATE)
                    {
                        // New index, so we just add the document (no old document can be there):
                        Console.WriteLine("adding " + path);
                        writer.AddDocument(doc);
                    }
                    else
                    {
                        // Existing index (an old copy of this document may have been indexed) so
                        // we use UpdateDocument instead to replace the old one matching the exact
                        // path, if present:
                        Console.WriteLine("updating " + path);
                        writer.UpdateDocument(new Term("uid", LucenePDFDocument.CreateUID(path)), doc);
                    }
                }
                else
                {
                    Console.WriteLine("Skipping " + path);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // at least on windows, some temporary files raise this exception with an "access denied" message
                // checking if the file can be read doesn't help
            }
        }
    }
}
