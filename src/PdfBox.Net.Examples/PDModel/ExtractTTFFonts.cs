/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/ExtractTTFFonts.java
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

using System.IO;
using PdfBox.Net;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// This example extracts all the fonts from a PDF file and writes them as their original format files.
/// </summary>
public class ExtractTTFFonts
{
    private ExtractTTFFonts()
    {
    }

    public static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.Error.WriteLine("usage: ExtractTTFFonts <input-pdf> <output-folder>");
            return;
        }

        using (PDDocument document = Loader.LoadPDF(args[0]))
        {
            string outputDir = args[1];
            Directory.CreateDirectory(outputDir);
            for (int i = 0; i < document.GetNumberOfPages(); i++)
            {
                PDPage page = document.GetPage(i);
                ExtractFontsFromPage(page, outputDir);
            }
        }
    }

    private static void ExtractFontsFromPage(PDPage page, string outputDir)
    {
        PDResources? resources = page.GetResources();
        if (resources == null) return;
        foreach (var fontName in resources.GetFontNames())
        {
            PDFont? font = resources.GetFont(fontName);
            if (font is PDTrueTypeFont ttf)
            {
                // NOTE: PDTrueTypeFont.ExportFont is not yet implemented in this .NET port.
                // In the Java implementation, TTF bytes are extracted via
                // ttf.getTrueTypeFont().getOriginalData().
                Console.WriteLine("Found TTF font: " + font.GetName() + " (export not yet available)");
            }
        }
    }
}
