/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/UsingTextMatrix.java
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
using PdfBox.Net.Util;

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// This is an example of how to use a text matrix.
/// </summary>
public class UsingTextMatrix
{
    /// <summary>
    /// Creates a sample document with some text using a text matrix.
    /// </summary>
    /// <param name="message">The message to write in the file.</param>
    /// <param name="outfile">The resulting PDF.</param>
    public void DoIt(string message, string outfile)
    {
        // the document
        using (PDDocument doc = new PDDocument())
        {
            // Page 1
            PDFont font = new PDType1Font(PDType1Font.FontName.HELVETICA);
            PDPage page = new PDPage(PDRectangle.A4);
            doc.AddPage(page);
            float fontSize = 12.0f;

            PDRectangle pageSize = page.GetMediaBox();
            float centeredXPosition = (pageSize.GetWidth() - fontSize / 1000f) / 2f;
            float stringWidth = font.GetStringWidth(message);
            float centeredYPosition = (pageSize.GetHeight() - (stringWidth * fontSize) / 1000f) / 3f;

            using (PDPageContentStream contentStream = new PDPageContentStream(doc, page,
                PDPageContentStream.AppendMode.OVERWRITE, false))
            {
                contentStream.SetFont(font, fontSize);
                contentStream.BeginText();
                // counterclockwise rotation
                for (int i = 0; i < 8; i++)
                {
                    contentStream.SetTextMatrix(Matrix.GetRotateInstance(i * Math.PI * 0.25,
                        centeredXPosition, pageSize.GetHeight() - centeredYPosition));
                    contentStream.ShowText(message + " " + i);
                }
                // clockwise rotation
                for (int i = 0; i < 8; i++)
                {
                    contentStream.SetTextMatrix(Matrix.GetRotateInstance(-i * Math.PI * 0.25,
                        centeredXPosition, centeredYPosition));
                    contentStream.ShowText(message + " " + i);
                }

                contentStream.EndText();
            }

            // Page 2
            page = new PDPage(PDRectangle.A4);
            doc.AddPage(page);
            fontSize = 1.0f;

            using (PDPageContentStream contentStream = new PDPageContentStream(doc, page,
                PDPageContentStream.AppendMode.OVERWRITE, false))
            {
                contentStream.SetFont(font, fontSize);
                contentStream.BeginText();

                // text scaling and translation
                for (int i = 0; i < 10; i++)
                {
                    contentStream.SetTextMatrix(new Matrix(12f + (i * 6), 0, 0, 12f + (i * 6),
                        100, 100f + i * 50));
                    contentStream.ShowText(message + " " + i);
                }
                contentStream.EndText();
            }

            // Page 3
            page = new PDPage(PDRectangle.A4);
            doc.AddPage(page);
            fontSize = 1.0f;

            using (PDPageContentStream contentStream = new PDPageContentStream(doc, page,
                PDPageContentStream.AppendMode.OVERWRITE, false))
            {
                contentStream.SetFont(font, fontSize);
                contentStream.BeginText();

                int idx = 0;
                // text scaling combined with rotation
                contentStream.SetTextMatrix(new Matrix(12, 0, 0, 12, centeredXPosition, centeredYPosition * 1.5f));
                contentStream.ShowText(message + " " + idx++);

                contentStream.SetTextMatrix(new Matrix(0, 18, -18, 0, centeredXPosition, centeredYPosition * 1.5f));
                contentStream.ShowText(message + " " + idx++);

                contentStream.SetTextMatrix(new Matrix(-24, 0, 0, -24, centeredXPosition, centeredYPosition * 1.5f));
                contentStream.ShowText(message + " " + idx++);

                contentStream.SetTextMatrix(new Matrix(0, -30, 30, 0, centeredXPosition, centeredYPosition * 1.5f));
                contentStream.ShowText(message + " " + idx++);

                contentStream.EndText();
            }

            doc.Save(outfile);
        }
    }

    /// <summary>
    /// This will create a PDF document with some examples how to use a text matrix.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    public static void Main(string[] args)
    {
        UsingTextMatrix app = new UsingTextMatrix();
        if (args.Length != 2)
        {
            Console.Error.WriteLine("usage: UsingTextMatrix <Message> <output-file>");
        }
        else
        {
            app.DoIt(args[0], args[1]);
        }
    }
}
