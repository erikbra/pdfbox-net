/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/HelloWorldTTF.java
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
using PdfBox.Net.PDModel.Font;

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// Creates a simple document with a TrueType font.
/// </summary>
public class HelloWorldTTF
{
    private HelloWorldTTF()
    {
    }

    public static void Main(string[] args)
    {
        if (args.Length != 3)
        {
            Console.Error.WriteLine("usage: HelloWorldTTF <output-file> <Message> <ttf-file>");
            return;
        }

        string pdfPath = args[0];
        string message = args[1];
        string ttfPath = args[2];

        using (PDDocument doc = new PDDocument())
        {
            PDPage page = new PDPage();
            doc.AddPage(page);

            PDFont font = PDType0Font.Load(doc, ttfPath);

            using (PDPageContentStream contents = new PDPageContentStream(doc, page))
            {
                contents.BeginText();
                contents.SetFont(font, 12);
                contents.NewLineAtOffset(100, 700);
                contents.ShowText(message);
                contents.EndText();
            }

            doc.Save(pdfPath);
            Console.WriteLine(pdfPath + " created!");
        }
    }
}
