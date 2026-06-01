/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/SuperimposePage.java
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

using PdfBox.Net;
using PdfBox.Net.MultiPdf;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.Util;

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// Example to show superimposing a PDF page onto another PDF.
/// </summary>
public sealed class SuperimposePage
{
    private SuperimposePage()
    {
    }

    public static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.Error.WriteLine("usage: SuperimposePage <source-pdf> <dest-pdf>");
            return;
        }

        string sourcePath = args[0];
        string destPath = args[1];

        using (PDDocument sourceDoc = Loader.LoadPDF(sourcePath))
        {
            int sourcePage = 1;

            // create a new PDF and add a blank page
            using (PDDocument doc = new PDDocument())
            {
                PDPage page = new PDPage();
                doc.AddPage(page);

                // write some sample text to the new page
                using (PDPageContentStream contents = new PDPageContentStream(doc, page))
                {
                    contents.BeginText();
                    contents.SetFont(new PDType1Font(PDType1Font.FontName.HELVETICA_BOLD), 12);
                    contents.NewLineAtOffset(2, PDRectangle.LETTER.GetHeight() - 12);
                    contents.ShowText("Sample text");
                    contents.EndText();

                    // Create a Form XObject from the source document using LayerUtility
                    LayerUtility layerUtility = new LayerUtility(doc);
                    PDFormXObject form = layerUtility.ImportPageAsForm(sourceDoc, sourcePage - 1);

                    // draw the full form
                    contents.DrawForm(form);

                    // draw a scaled form
                    contents.SaveGraphicsState();
                    // scale by 0.5 in both axes
                    Matrix scaleMatrix = new Matrix(0.5f, 0, 0, 0.5f, 0, 0);
                    contents.Transform(scaleMatrix);
                    contents.DrawForm(form);
                    contents.RestoreGraphicsState();

                    // draw a scaled and rotated form
                    contents.SaveGraphicsState();
                    double angle = 1.8 * Math.PI;
                    float cos = (float)Math.Cos(angle);
                    float sin = (float)Math.Sin(angle);
                    Matrix rotateMatrix = Matrix.Concatenate(scaleMatrix, new Matrix(cos, sin, -sin, cos, 0, 0));
                    contents.Transform(rotateMatrix);
                    contents.DrawForm(form);
                    contents.RestoreGraphicsState();
                }

                doc.Save(destPath);
            }
        }
    }
}
