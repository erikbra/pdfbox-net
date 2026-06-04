/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/EmbeddedMultipleFonts.java
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

using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Font;

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// This example creates a PDF with multiple embedded fonts.
/// </summary>
public class EmbeddedMultipleFonts
{
    public static void Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("usage: EmbeddedMultipleFonts <output-file> <font-file1> <font-file2> ...");
            return;
        }

        string outputFile = args[0];
        string[] fontFiles = args[1..];

        using (PDDocument document = new PDDocument())
        {
            PDPage page = new PDPage(PDRectangle.A4);
            document.AddPage(page);

            float fontSize = 12;
            float leading = fontSize * 1.5f;
            float yStart = page.GetMediaBox().GetUpperRightY() - 50;

            using (PDPageContentStream stream = new PDPageContentStream(document, page))
            {
                stream.BeginText();
                stream.SetLeading(leading);
                stream.NewLineAtOffset(50, yStart);

                foreach (string fontFile in fontFiles)
                {
                    PDType0Font font = PDType0Font.Load(document, fontFile);
                    stream.SetFont(font, fontSize);
                    stream.ShowText("Hello World from font: " + Path.GetFileNameWithoutExtension(fontFile));
                    stream.NewLine();
                }

                stream.EndText();
            }

            document.Save(outputFile);
        }
    }
}
