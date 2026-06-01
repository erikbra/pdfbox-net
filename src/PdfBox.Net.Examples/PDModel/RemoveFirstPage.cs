/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/RemoveFirstPage.java
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
/// This is an example on how to remove pages from a PDF document. Do not use this tool if other
/// pages link to this one or if your document has a structure tree for accessibility unless you are
/// able to fix these as well. In such cases it is better to use the splitter() class which will do
/// these fixes.
/// </summary>
public static class RemoveFirstPage
{
    /// <summary>
    /// This will print the documents data.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    public static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Usage();
        }
        else
        {
            using (PDDocument document = Loader.LoadPDF(args[0]))
            {
                if (document.GetNumberOfPages() <= 1)
                {
                    throw new IOException("Error: A PDF document must have at least one page, " +
                                          "cannot remove the last page!");
                }
                document.RemovePage(0);
                document.Save(args[1]);
            }
        }
    }

    /// <summary>
    /// This will print the usage for this document.
    /// </summary>
    private static void Usage()
    {
        Console.Error.WriteLine("Usage: RemoveFirstPage <input-pdf> <output-pdf>");
    }
}
