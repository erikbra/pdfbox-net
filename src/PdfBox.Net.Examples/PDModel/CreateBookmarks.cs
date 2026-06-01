/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/CreateBookmarks.java
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
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Outline;

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// This is an example on how to add bookmarks to a PDF document.  It simply
/// adds 1 bookmark for every page.
/// </summary>
public static class CreateBookmarks
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
                PDDocumentOutline outline = new PDDocumentOutline();
                document.GetDocumentCatalog().SetDocumentOutline(outline);
                PDOutlineItem pagesOutline = new PDOutlineItem();
                pagesOutline.SetTitle("All Pages");
                outline.AddLast(pagesOutline);
                int pageNum = 0;
                foreach (PDPage page in document.GetPages())
                {
                    pageNum++;
                    PDPageDestination dest = new PDPageFitWidthDestination();
                    dest.SetPage(page);
                    PDOutlineItem bookmark = new PDOutlineItem();
                    bookmark.SetDestination(dest);
                    bookmark.SetTitle("Page " + pageNum);
                    pagesOutline.AddLast(bookmark);
                }
                pagesOutline.OpenNode();
                outline.OpenNode();

                document.GetDocumentCatalog().SetPageMode(PageMode.UseOutlines);

                document.Save(args[1]);
            }
        }
    }

    /// <summary>
    /// This will print the usage for this document.
    /// </summary>
    private static void Usage()
    {
        Console.Error.WriteLine("Usage: CreateBookmarks <input-pdf> <output-pdf>");
    }
}
