/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/EmbeddedFonts.java
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

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// An example of using an embedded TrueType font with Unicode text.
/// Adaptation note: uses repeated NewLineAtOffset instead of Java's SetLeading + NewLine
/// because SetLeading/NewLine are not yet in this .NET port.
/// </summary>
public sealed class EmbeddedFonts
{
    private EmbeddedFonts()
    {
    }

    public static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.Error.WriteLine("usage: EmbeddedFonts <output-file> <font-file>");
            return;
        }

        string outputFile = args[0];
        string fontFile = args[1];

        using (PDDocument document = new PDDocument())
        {
            PDPage page = new PDPage(PDRectangle.A4);
            document.AddPage(page);

            PDType0Font font = PDType0Font.Load(document, fontFile);
            float fontSize = 12;
            float leading = fontSize * 1.2f;

            using (PDPageContentStream stream = new PDPageContentStream(document, page))
            {
                stream.BeginText();
                stream.SetFont(font, fontSize);
                stream.SetLeading(leading);

                stream.NewLineAtOffset(50, 600);
                stream.ShowText("PDFBox's Unicode with Embedded TrueType Font");
                stream.NewLine();

                stream.ShowText("Supports full Unicode text \u263A");
                stream.NewLine();

                stream.ShowText("English \u0440\u0443\u0441\u0441\u043A\u0438\u0439 \u044F\u0437\u044B\u043A Ti\u1EBFng Vi\u1EC7t");
                stream.NewLine();

                // ligature
                stream.ShowText("Ligatures: \uFB01lm \uFB02ood");

                stream.EndText();
            }

            document.Save(outputFile);
        }
    }
}
