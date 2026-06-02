/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/BengaliPdfGenerationHelloWorld.java
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

using System.Reflection;
using System.Text;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Font;

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// Inspired from HelloWorldTTF. This attempts to correctly demonstrate to what extent Bengali text
/// rendering is supported. We read large amount of text from a file and try to render it properly.
/// </summary>
public class BengaliPdfGenerationHelloWorld
{
    private const int LineGap = 5;
    private const string LohitBengaliTtf = "PdfBox.Net.Examples.Resources.ttf.Lohit-Bengali.ttf";
    private const string TextSourceFile = "PdfBox.Net.Examples.Resources.ttf.bengali-samples.txt";
    private const int FontSize = 20;
    private const int Margin = 20;

    private BengaliPdfGenerationHelloWorld()
    {
    }

    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine(
                "usage: " + nameof(BengaliPdfGenerationHelloWorld) + " <output-file> ");
            return;
        }

        string filename = args[0];

        Console.WriteLine("The generated pdf filename is: " + filename);

        using (PDDocument doc = new PDDocument())
        {
            Stream? fontStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(LohitBengaliTtf);
            if (fontStream == null)
            {
                throw new InvalidOperationException(
                    "Embedded resource not found: " + LohitBengaliTtf);
            }

            PDFont font = PDType0Font.Load(doc, fontStream, true);
            PDRectangle rectangle = GetPageSize();
            float workablePageWidth = rectangle.GetWidth() - 2 * Margin;
            float workablePageHeight = rectangle.GetHeight() - 2 * Margin;

            IList<IList<string>> pagedTexts = GetReAlignedTextBasedOnPageHeight(
                GetReAlignedTextBasedOnPageWidth(GetBengaliTextFromFile(), font,
                    workablePageWidth),
                font, workablePageHeight);

            foreach (IList<string> linesForPage in pagedTexts)
            {
                PDPage page = new PDPage(GetPageSize());
                doc.AddPage(page);

                using (PDPageContentStream contents = new PDPageContentStream(doc, page))
                {
                    contents.BeginText();
                    contents.SetFont(font, FontSize);
                    contents.NewLineAtOffset(rectangle.GetLowerLeftX() + Margin,
                        rectangle.GetUpperRightY() - Margin);

                    foreach (string line in linesForPage)
                    {
                        contents.ShowText(line);
                        contents.NewLineAtOffset(0, -(FontSize + LineGap));
                    }

                    contents.EndText();
                }
            }

            doc.Save(filename);
        }
    }

    private static IList<IList<string>> GetReAlignedTextBasedOnPageHeight(IList<string> originalLines,
        PDFont font, float workablePageHeight)
    {
        float newLineHeight = font.GetFontDescriptor()?.GetFontBoundingBox().GetHeight() / 1000f * FontSize + LineGap
            ?? FontSize + LineGap;
        IList<IList<string>> realignedTexts = new List<IList<string>>();
        float consumedHeight = 0;
        IList<string> linesInAPage = new List<string>();
        foreach (string line in originalLines)
        {
            if (newLineHeight + consumedHeight < workablePageHeight)
            {
                consumedHeight += newLineHeight;
            }
            else
            {
                consumedHeight = newLineHeight;
                realignedTexts.Add(linesInAPage);
                linesInAPage = new List<string>();
            }

            linesInAPage.Add(line);
        }
        realignedTexts.Add(linesInAPage);
        return realignedTexts;
    }

    private static IList<string> GetReAlignedTextBasedOnPageWidth(IList<string> originalLines,
        PDFont font, float workablePageWidth)
    {
        IList<string> uniformlyWideTexts = new List<string>();
        float consumedWidth = 0;
        StringBuilder sb = new StringBuilder();
        float newTokenWidth = 0;
        foreach (string line in originalLines)
        {
            string[] tokens = line.Split(' ', StringSplitOptions.None);
            for (int t = 0; t < tokens.Length; t++)
            {
                string token = tokens[t];
                // Include the space separator for non-last tokens, mirroring the Java StringTokenizer
                // which returns delimiters as separate tokens.
                string tokenWithSeparator = (t < tokens.Length - 1) ? token + " " : token;
                newTokenWidth = font.GetStringWidth(tokenWithSeparator) / 1000f * FontSize;
                if (newTokenWidth + consumedWidth < workablePageWidth)
                {
                    consumedWidth += newTokenWidth;
                }
                else
                {
                    // add a new text chunk
                    uniformlyWideTexts.Add(sb.ToString());
                    consumedWidth = newTokenWidth;
                    sb = new StringBuilder();
                }

                sb.Append(tokenWithSeparator);
            }

            // add a new text chunk
            uniformlyWideTexts.Add(sb.ToString());
            consumedWidth = newTokenWidth;
            sb = new StringBuilder();
        }

        return uniformlyWideTexts;
    }

    private static PDRectangle GetPageSize()
    {
        return PDRectangle.A4;
    }

    private static IList<string> GetBengaliTextFromFile()
    {
        IList<string> lines = new List<string>();

        Stream? textStream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(TextSourceFile);
        if (textStream == null)
        {
            throw new InvalidOperationException(
                "Embedded resource not found: " + TextSourceFile);
        }

        using (StreamReader br = new StreamReader(textStream, Encoding.UTF8))
        {
            while (true)
            {
                string? line = br.ReadLine();

                if (line == null)
                {
                    break;
                }

                if (!line.StartsWith("#"))
                {
                    lines.Add(line);
                }
            }
        }

        return lines;
    }
}
