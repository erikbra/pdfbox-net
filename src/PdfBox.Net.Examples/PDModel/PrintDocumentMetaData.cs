/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/PrintDocumentMetaData.java
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

using PdfBox.Net;
using PdfBox.Net.PDModel;

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// This example shows how to print out the metadata of a PDF document.
/// </summary>
public static class PrintDocumentMetaData
{
    /// <summary>
    /// This will print the documents metadata to System.out.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Usage();
            return;
        }

        using (PDDocument document = Loader.LoadPDF(args[0]))
        {
            PDDocumentInformation info = document.GetDocumentInformation();
            Console.WriteLine("Page Count=" + document.GetNumberOfPages());
            Console.WriteLine("Title=" + info.GetTitle());
            Console.WriteLine("Author=" + info.GetAuthor());
            Console.WriteLine("Subject=" + info.GetSubject());
            Console.WriteLine("Keywords=" + info.GetKeywords());
            Console.WriteLine("Creator=" + info.GetCreator());
            Console.WriteLine("Producer=" + info.GetProducer());
            Console.WriteLine("Creation Date=" + info.GetCreationDate());
            Console.WriteLine("Modification Date=" + info.GetModificationDate());
            Console.WriteLine("Trapped=" + info.GetTrapped());
        }
    }

    /// <summary>
    /// This will print the usage for this document.
    /// </summary>
    private static void Usage()
    {
        Console.Error.WriteLine("Usage: PrintDocumentMetaData <input-pdf>");
    }
}
