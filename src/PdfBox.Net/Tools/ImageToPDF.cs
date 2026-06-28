/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: tools/src/main/java/org/apache/pdfbox/tools/ImageToPDF.java
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
using PdfBox.Net.PDModel.Graphics.Image;

namespace PdfBox.Net.Tools;

public static class ImageToPDF
{
    public static void Run() => throw ToolSupport.NotSupported(nameof(ImageToPDF));

    public static int Run(string[] args, TextWriter? error = null)
    {
        error ??= Console.Error;
        try
        {
            ImageToPDFOptions options = ParseOptions(args);
            Convert(options.InputFile, options.OutputFile);
            return 0;
        }
        catch (ArgumentException ex)
        {
            error.WriteLine(ex.Message);
            return 1;
        }
        catch (IOException ex)
        {
            error.WriteLine($"Error creating PDF from image [{ex.GetType().Name}]: {ex.Message}");
            return 4;
        }
    }

    public static void Convert(string imageFile, string outputFile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(imageFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputFile);

        using PDDocument document = new();
        PDImageXObject image = PDImageXObject.CreateFromFile(imageFile, document);
        PDPage page = new();
        document.AddPage(page);

        using (PDPageContentStream content = new(document, page))
        {
            content.DrawImage(image, 20, 20, image.GetWidth(), image.GetHeight());
        }

        document.Save(outputFile);
    }

    private static ImageToPDFOptions ParseOptions(string[]? args)
    {
        args ??= [];
        string? input = null;
        string? output = null;

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

        return new ImageToPDFOptions(input, output);
    }

    private static string ReadOptionValue(string[] args, ref int index, string optionName)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"Missing value for {optionName}.");
        }

        return args[++index];
    }

    private sealed record ImageToPDFOptions(string InputFile, string OutputFile);
}
