/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/ShowColorBoxes.java
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
using PdfBox.Net.Util;

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// Draws color boxes on a page. Note that a PDPageContentStream is used with different
/// color spaces.
/// </summary>
public sealed class ShowColorBoxes
{
    private ShowColorBoxes()
    {
    }

    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("usage: ShowColorBoxes <output-file>");
            Environment.Exit(1);
        }

        string filename = args[0];

        using (PDDocument doc = new PDDocument())
        {
            PDPage page = new PDPage();
            doc.AddPage(page);

            using (PDPageContentStream contents = new PDPageContentStream(doc, page))
            {
                // fill the entire background with cyan
                contents.SetNonStrokingColor(0f, 1f, 1f);
                contents.AddRect(0, 0, page.GetMediaBox().GetWidth(), page.GetMediaBox().GetHeight());
                contents.Fill();

                // draw a red box in the lower left hand corner
                contents.SetNonStrokingColor(1f, 0f, 0f);
                contents.AddRect(10, 10, 100, 100);
                contents.Fill();

                // draw a blue box with rect x=200, y=500, w=200, h=100
                // 105° rotation is around the bottom left corner
                contents.SaveGraphicsState();
                contents.SetNonStrokingColor(0f, 0f, 1f);
                contents.Transform(Matrix.GetRotateInstance(Math.PI / 180.0 * 105, 200, 500));
                contents.AddRect(0, 0, 200, 100);
                contents.Fill();
                contents.RestoreGraphicsState();
            }

            doc.Save(filename);
        }
    }
}
