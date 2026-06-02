/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/util/AddWatermarkText.java
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
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Graphics;
using PdfBox.Net.PDModel.Graphics.State;
using PdfBox.Net.Util;

namespace PdfBox.Net.Examples.Util;

/// <summary>
/// Add a diagonal watermark text to each page of a PDF.
/// </summary>
public class AddWatermarkText
{
    private AddWatermarkText()
    {
    }

    public static void Main(string[] args)
    {
        if (args.Length != 3)
        {
            Usage();
        }
        else
        {
            string srcFile = args[0];
            string dstFile = args[1];
            string text = args[2];

            using (PDDocument doc = Loader.LoadPDF(srcFile))
            {
                foreach (PDPage page in doc.GetPages())
                {
                    PDFont font = new PDType1Font(PDType1Font.FontName.HELVETICA);
                    AddWatermarkTextToPage(doc, page, font, text);
                }

                doc.Save(dstFile);
            }
        }
    }

    private static void AddWatermarkTextToPage(PDDocument doc, PDPage page, PDFont font, string text)
    {
        using (PDPageContentStream cs
            = new PDPageContentStream(doc, page, PDPageContentStream.AppendMode.APPEND, true))
        {
            float fontHeight = 100; // arbitrary for short text
            float width = page.GetMediaBox().GetWidth();
            float height = page.GetMediaBox().GetHeight();

            int rotation = page.GetRotation();
            switch (rotation)
            {
                case 90:
                    width = page.GetMediaBox().GetHeight();
                    height = page.GetMediaBox().GetWidth();
                    cs.Transform(Matrix.GetRotateInstance(Math.PI / 180.0 * 90, height, 0));
                    break;
                case 180:
                    cs.Transform(Matrix.GetRotateInstance(Math.PI / 180.0 * 180, width, height));
                    break;
                case 270:
                    width = page.GetMediaBox().GetHeight();
                    height = page.GetMediaBox().GetWidth();
                    cs.Transform(Matrix.GetRotateInstance(Math.PI / 180.0 * 270, 0, width));
                    break;
                default:
                    break;
            }

            float stringWidth = font.GetStringWidth(text) / 1000 * fontHeight;
            float diagonalLength = (float)Math.Sqrt(width * width + height * height);
            float angle = (float)Math.Atan2(height, width);
            float x = (diagonalLength - stringWidth) / 2; // "horizontal" position in rotated world
            float y = -fontHeight / 4; // 4 is a trial-and-error thing, this lowers the text a bit
            cs.Transform(Matrix.GetRotateInstance(angle, 0, 0));
            cs.SetFont(font, fontHeight);
            // cs.SetRenderingMode(RenderingMode.STROKE); // for "hollow" effect

            PDExtendedGraphicsState gs = new PDExtendedGraphicsState();
            gs.SetNonStrokingAlphaConstant(0.2f);
            gs.SetStrokingAlphaConstant(0.2f);
            gs.SetBlendMode(BlendMode.MULTIPLY);
            gs.SetLineWidth(3f);
            cs.SetGraphicsStateParameters(gs);

            cs.SetNonStrokingColor(1f, 0f, 0f);
            cs.SetStrokingColor(1f, 0f, 0f);

            cs.BeginText();
            cs.NewLineAtOffset(x, y);
            cs.ShowText(text);
            cs.EndText();
        }
    }

    /// <summary>
    /// This will print the usage.
    /// </summary>
    private static void Usage()
    {
        Console.Error.WriteLine("Usage: AddWatermarkText <input-pdf> <output-pdf> <short text>");
    }
}
