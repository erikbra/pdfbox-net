/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox-layout-awt/src/main/java/org/apache/pdfbox/glyphlayout/examples/GlyphLayoutHelloWorldAWT.java
 * PDFBOX_SOURCE_COMMIT: 56575fd583792844b6bd182d67739d26568b1d01
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 56575fd583792844b6bd182d67739d26568b1d01
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

using PdfBox.Net.GlyphLayout.Awt;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Font;

namespace PdfBox.Net.GlyphLayout.Examples;

/// <summary>
/// Creates a simple document with a TrueType font using <see cref="GlyphLayoutProcessorAwt"/>.
/// </summary>
public class GlyphLayoutHelloWorldAWT
{
    public static void Main(string[] args)
    {
        new GlyphLayoutHelloWorldAWT().Test(args);
    }

    public void Test(string[] args)
    {
        if (args.Length != 3)
        {
            throw new ArgumentException(
                $"Usage: {GetType().FullName} <output-file> <Message> <ttf-file>",
                nameof(args));
        }

        string pdfPath = args[0];
        string message = args[1];
        string ttfPath = args[2];

        using PDDocument doc = new();
        PDPage page = new();
        doc.AddPage(page);

        GlyphLayoutProcessorAwt glyphLayoutProcessorAwt = new();
        GlyphLayoutFontLoaderAwt.FontOptions fontOptions = new();
        using FileStream input = File.OpenRead(ttfPath);
        PDType0Font font = glyphLayoutProcessorAwt.LoadFont(doc, input, fontOptions);

        using (PDPageContentStream contents = new(doc, page))
        {
            contents.SetGlyphLayoutProcessor(glyphLayoutProcessorAwt);
            contents.BeginText();
            contents.SetFont(font, 20);
            contents.NewLineAtOffset(100, 700);
            contents.ShowText(message);
            contents.EndText();
        }

        doc.Save(pdfPath);
    }
}
