/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/util/PDFMergerExample.java
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

using PdfBox.Net.MultiPdf;

namespace PdfBox.Net.Examples.Util;

/// <summary>
/// This example demonstrates how to merge multiple PDF documents into one.
/// </summary>
public class PDFMergerExample
{
    private PDFMergerExample()
    {
    }

    /// <summary>
    /// Creates a compound PDF document from the list of input documents.
    /// </summary>
    /// <param name="inputFiles">The array of source PDF files.</param>
    /// <param name="destinationFileName">The path to the destination PDF.</param>
    public static void Merge(string[] inputFiles, string destinationFileName)
    {
        PDFMergerUtility merger = new PDFMergerUtility();
        merger.DestinationFileName = destinationFileName;
        foreach (string inputFile in inputFiles)
        {
            merger.AddSource(inputFile);
        }
        merger.MergeDocuments();
    }

    public static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("usage: PDFMergerExample <output-file> <input1> [input2 ...]");
            return;
        }

        string outputFile = args[0];
        string[] inputFiles = args[1..];
        Merge(inputFiles, outputFile);
        Console.WriteLine("Merged into: " + outputFile);
    }
}
