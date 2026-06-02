/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/GoToSecondBookmarkOnOpen.java
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
/// This example opens an existing PDF and configures it so that
/// when opened, it navigates to the second bookmark.
/// </summary>
public class GoToSecondBookmarkOnOpen
{
    private GoToSecondBookmarkOnOpen()
    {
    }

    public static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.Error.WriteLine("Usage: GoToSecondBookmarkOnOpen <input.pdf> <output.pdf>");
            return;
        }

        using (PDDocument doc = Loader.LoadPDF(args[0]))
        {
            PDDocumentOutline? outline = doc.GetDocumentCatalog().GetDocumentOutline();
            if (outline == null)
            {
                Console.Error.WriteLine("Document has no bookmarks.");
                return;
            }

            PDOutlineItem? firstBookmark = outline.GetFirstChild();
            if (firstBookmark == null)
            {
                Console.Error.WriteLine("Document outline has no children.");
                return;
            }

            PDOutlineItem? secondBookmark = firstBookmark.GetNextSibling();
            if (secondBookmark == null)
            {
                Console.Error.WriteLine("Only one bookmark found.");
                return;
            }

            PDPageDestination? dest = secondBookmark.GetDestination() as PDPageDestination;
            if (dest == null)
            {
                Console.Error.WriteLine("Second bookmark does not have a page destination.");
                return;
            }

            PDActionGoTo action = new PDActionGoTo();
            action.SetDestination(dest);
            doc.GetDocumentCatalog().SetOpenAction(action);
            doc.Save(args[1]);
        }
    }
}
