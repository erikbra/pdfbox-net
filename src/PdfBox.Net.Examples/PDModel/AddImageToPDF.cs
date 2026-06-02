/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/AddImageToPDF.java
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

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// This is an example that creates a reads a document and adds an image to it.
/// </summary>
public class AddImageToPDF
{
    /// <summary>
    /// Add an image to an existing PDF document.
    /// </summary>
    /// <param name="inputFile">The input PDF to add the image to.</param>
    /// <param name="imagePath">The filename of the image to put in the PDF.</param>
    /// <param name="outputFile">The file to write to the pdf to.</param>
    public void CreatePDFFromImage(string inputFile, string imagePath, string outputFile)
    {
        using (PDDocument doc = Loader.LoadPDF(inputFile))
        {
            // NOTE: PDImageXObject.CreateFromFile() and PDPageContentStream.DrawImage() are not
            // yet implemented in this .NET port.
            throw new NotSupportedException(
                "Image drawing (PDImageXObject.CreateFromFile, PDPageContentStream.DrawImage) " +
                "is not yet implemented in this .NET port.");
        }
    }

    public static void Main(string[] args)
    {
        AddImageToPDF app = new AddImageToPDF();
        if (args.Length != 3)
        {
            app.Usage();
        }
        else
        {
            app.CreatePDFFromImage(args[0], args[1], args[2]);
        }
    }

    private void Usage()
    {
        Console.Error.WriteLine("usage: AddImageToPDF <input-pdf> <image> <output-pdf>");
    }
}
