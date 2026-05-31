/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/AddMessageToEachPage.java
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

using PdfBox.Net;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Font;

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// This is an example of how to add a message to every page in a pdf document.
/// </summary>
public class AddMessageToEachPage
{
    /// <summary>
    /// Create the second sample document from the PDF file format specification.
    /// </summary>
    /// <param name="file">The file to write the PDF to.</param>
    /// <param name="message">The message to write in the file.</param>
    /// <param name="outfile">The resulting PDF.</param>
    public void DoIt(string file, string message, string outfile)
    {
        using (PDDocument doc = Loader.LoadPDF(file))
        {
            // NOTE: PDType1Font construction from FontName enum and PDPageContentStream text drawing
            // operators (SetFont, SetNonStrokingColor, SetTextMatrix, ShowText) are not yet
            // implemented in this .NET port.
            throw new NotSupportedException(
                "Text drawing operators in PDPageContentStream are not yet implemented in this .NET port.");
        }
    }

    /// <summary>
    /// This will create a hello world PDF document.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    public static void Main(string[] args)
    {
        AddMessageToEachPage app = new AddMessageToEachPage();
        if (args.Length != 3)
        {
            app.Usage();
        }
        else
        {
            app.DoIt(args[0], args[1], args[2]);
        }
    }

    /// <summary>
    /// This will print out a message telling how to use this example.
    /// </summary>
    private void Usage()
    {
        Console.Error.WriteLine("usage: AddMessageToEachPage <input-file> <Message> <output-file>");
    }
}
