/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/rendering/ConvertPDFPagesToImages.java
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
using PdfBox.Net.Rendering;

namespace PdfBox.Net.Examples.Rendering;

/// <summary>
/// Convert PDF pages to images.
/// </summary>
public class ConvertPDFPagesToImages
{
    private ConvertPDFPagesToImages()
    {
    }

    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("usage: ConvertPDFPagesToImages <input-pdf>");
            return;
        }

        using (PDDocument document = Loader.LoadPDF(args[0]))
        {
            PDFRenderer pdfRenderer = new PDFRenderer(document);
            for (int page = 0; page < document.GetNumberOfPages(); page++)
            {
                // RenderImageWithDPI returns a BufferedImage (AWT stub).
                // Platform-specific save logic would follow here.
                BufferedImage bim = pdfRenderer.RenderImageWithDPI(page, 300);
                Console.WriteLine($"Rendered page {page + 1}: {bim.Width}x{bim.Height} pixels");
            }
        }
    }
}
