/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/PrintBookmarks.java
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
using PdfBox.Net.PDModel.Interactive.Action;
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Outline;

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// This is an example on how to access the bookmarks that are part of a pdf document.
/// </summary>
public class PrintBookmarks
{
    /// <summary>
    /// This will print the documents data.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Usage();
        }
        else
        {
            using (PDDocument document = Loader.LoadPDF(args[0]))
            {
                PrintBookmarks meta = new PrintBookmarks();
                PDDocumentOutline? outline = document.GetDocumentCatalog().GetDocumentOutline();
                if (outline != null)
                {
                    meta.PrintBookmark(document, outline, "");
                }
                else
                {
                    Console.WriteLine("This document does not contain any bookmarks");
                }
            }
        }
    }

    /// <summary>
    /// This will print the usage for this document.
    /// </summary>
    private static void Usage()
    {
        Console.Error.WriteLine("Usage: PrintBookmarks <input-pdf>");
    }

    /// <summary>
    /// This will print the documents bookmarks to Console.Out.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="bookmark">The bookmark to print out.</param>
    /// <param name="indentation">A pretty printing parameter.</param>
    public void PrintBookmark(PDDocument document, PDOutlineNode bookmark, string indentation)
    {
        PDOutlineItem? current = bookmark.GetFirstChild();
        while (current != null)
        {
            if (current.GetDestination() is PDPageDestination pd)
            {
                Console.WriteLine(indentation + "Destination page: " + (pd.RetrievePageNumber() + 1));
            }
            else if (current.GetDestination() is PDNamedDestination namedDest)
            {
                PDPageDestination? pd2 = document.GetDocumentCatalog().FindNamedDestinationPage(namedDest);
                if (pd2 != null)
                {
                    Console.WriteLine(indentation + "Destination page: " + (pd2.RetrievePageNumber() + 1));
                }
            }
            else if (current.GetDestination() != null)
            {
                Console.WriteLine(indentation + "Destination class: " + current.GetDestination()!.GetType().Name);
            }

            if (current.GetAction() is PDActionGoTo gta)
            {
                if (gta.GetDestination() is PDPageDestination pd3)
                {
                    Console.WriteLine(indentation + "Destination page: " + (pd3.RetrievePageNumber() + 1));
                }
                else if (gta.GetDestination() is PDNamedDestination namedDest2)
                {
                    PDPageDestination? pd4 = document.GetDocumentCatalog().FindNamedDestinationPage(namedDest2);
                    if (pd4 != null)
                    {
                        Console.WriteLine(indentation + "Destination page: " + (pd4.RetrievePageNumber() + 1));
                    }
                }
                else if (gta.GetDestination() != null)
                {
                    Console.WriteLine(indentation + "Destination class: " + gta.GetDestination()!.GetType().Name);
                }
            }
            else if (current.GetAction() != null)
            {
                Console.WriteLine(indentation + "Action class: " + current.GetAction()!.GetType().Name);
            }

            Console.WriteLine(indentation + current.GetTitle());
            PrintBookmark(document, current, indentation + "    ");
            current = current.GetNextSibling();
        }
    }
}
