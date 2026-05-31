/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/CreateLandscapePDF.java
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

using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.Util;

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// This is an example of how to create a page with a landscape orientation.
/// </summary>
public class CreateLandscapePDF
{
    /// <summary>
    /// Creates a sample document with a landscape orientation and some text surrounded by a box.
    /// </summary>
    /// <param name="message">The message to write in the file.</param>
    /// <param name="outfile">The resulting PDF.</param>
    public void DoIt(string message, string outfile)
    {
        using (PDDocument doc = new PDDocument())
        {
            PDPage page = new PDPage(PDRectangle.A4);
            page.SetRotation(90);
            doc.AddPage(page);

            // NOTE: PDType1Font construction from FontName enum and PDPageContentStream drawing
            // operators (SetFont, BeginText, NewLineAtOffset, ShowText, EndText, MoveTo, LineTo, Stroke)
            // are not yet implemented in this .NET port.
            throw new NotSupportedException(
                "Text and path drawing operators in PDPageContentStream are not yet implemented in this .NET port.");
        }
    }

    public static void Main(string[] args)
    {
        CreateLandscapePDF app = new CreateLandscapePDF();
        if (args.Length != 2)
        {
            app.Usage();
        }
        else
        {
            app.DoIt(args[0], args[1]);
        }
    }

    private void Usage()
    {
        Console.Error.WriteLine("usage: CreateLandscapePDF <Message> <output-file>");
    }
}
