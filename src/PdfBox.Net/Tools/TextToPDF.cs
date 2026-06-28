/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: tools/src/main/java/org/apache/pdfbox/tools/TextToPDF.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
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
using System.Text;

namespace PdfBox.Net.Tools;

public static class TextToPDF
{
    public static void Run() => throw ToolSupport.NotSupported(nameof(TextToPDF));

    public static int Run(string[] args, TextWriter? error = null)
    {
        error ??= Console.Error;
        try
        {
            TextToPDFOptions options = ParseOptions(args);
            Convert(options.InputFile, options.OutputFile, options.FontSize);
            return 0;
        }
        catch (ArgumentException ex)
        {
            error.WriteLine(ex.Message);
            return 1;
        }
        catch (IOException ex)
        {
            error.WriteLine($"Error creating PDF from text [{ex.GetType().Name}]: {ex.Message}");
            return 4;
        }
    }

    public static void Convert(string inputFile, string outputFile, float fontSize = 12f)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputFile);
        if (fontSize <= 0)
        {
            throw new ArgumentException("Font size must be positive.", nameof(fontSize));
        }

        using PDDocument document = new();
        PDFont font = new PDType1Font(PDType1Font.FontName.HELVETICA);
        PDRectangle pageSize = PDRectangle.LETTER;
        float margin = 72f;
        float leading = fontSize * 1.2f;
        float y = pageSize.GetHeight() - margin;
        PDPage page = AddPage(document, pageSize);
        PDPageContentStream content = StartText(document, page, font, fontSize, margin, y);

        try
        {
            foreach (string line in File.ReadLines(inputFile, Encoding.UTF8))
            {
                if (y - leading < margin)
                {
                    content.EndText();
                    content.Dispose();
                    page = AddPage(document, pageSize);
                    y = pageSize.GetHeight() - margin;
                    content = StartText(document, page, font, fontSize, margin, y);
                }

                content.ShowText(line);
                content.NewLineAtOffset(0, -leading);
                y -= leading;
            }

            content.EndText();
        }
        finally
        {
            content.Dispose();
        }

        document.Save(outputFile);
    }

    private static PDPage AddPage(PDDocument document, PDRectangle pageSize)
    {
        PDPage page = new(pageSize);
        document.AddPage(page);
        return page;
    }

    private static PDPageContentStream StartText(
        PDDocument document,
        PDPage page,
        PDFont font,
        float fontSize,
        float x,
        float y)
    {
        PDPageContentStream content = new(document, page);
        content.BeginText();
        content.SetFont(font, fontSize);
        content.NewLineAtOffset(x, y);
        return content;
    }

    private static TextToPDFOptions ParseOptions(string[]? args)
    {
        args ??= [];
        string? input = null;
        string? output = null;
        float fontSize = 12f;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            switch (arg)
            {
                case "-i":
                case "--input":
                    input = ReadOptionValue(args, ref i, arg);
                    break;
                case "-o":
                case "--output":
                    output = ReadOptionValue(args, ref i, arg);
                    break;
                case "-fontSize":
                    if (!float.TryParse(ReadOptionValue(args, ref i, arg), out fontSize))
                    {
                        throw new ArgumentException("Font size must be a number.");
                    }
                    break;
                default:
                    throw new ArgumentException($"Unknown option: {arg}");
            }
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Missing required option -i/--input.");
        }
        if (string.IsNullOrWhiteSpace(output))
        {
            throw new ArgumentException("Missing required option -o/--output.");
        }

        return new TextToPDFOptions(input, output, fontSize);
    }

    private static string ReadOptionValue(string[] args, ref int index, string optionName)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"Missing value for {optionName}.");
        }

        return args[++index];
    }

    private sealed record TextToPDFOptions(string InputFile, string OutputFile, float FontSize);
}
