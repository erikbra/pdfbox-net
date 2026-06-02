/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/ImageToPDF.java
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

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// Creates a PDF document with a single page that has an image as its full content.
/// </summary>
public class ImageToPDF
{
    private ImageToPDF()
    {
    }

    public static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("usage: ImageToPDF <output-file> <image-file> [<image-file2> ...]");
            return;
        }

        string outputFile = args[0];
        string[] imageFiles = args[1..];

        using (PDDocument doc = new PDDocument())
        {
            // NOTE: PDImageXObject.CreateFromFile and PDPageContentStream.DrawImage are
            // not yet implemented in this .NET port.
            throw new NotSupportedException(
                "Image drawing (PDImageXObject.CreateFromFile, PDPageContentStream.DrawImage) " +
                "is not yet implemented in this .NET port.");
        }
    }
}
