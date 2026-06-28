/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: tools/src/main/java/org/apache/pdfbox/tools/ExtractImages.java
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
using PdfBox.Net.PDModel.Graphics;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Rendering;
using PdfBox.Net.Tools.ImageIO;

namespace PdfBox.Net.Tools;

public static class ExtractImages
{
    public static void Run() => throw ToolSupport.NotSupported(nameof(ExtractImages));

    public static int Run(string[] args, TextWriter? error = null)
    {
        error ??= Console.Error;
        try
        {
            ExtractImagesOptions options = ParseOptions(args);
            Extract(options.InputFile, options.OutputPrefix, options.Password);
            return 0;
        }
        catch (ArgumentException ex)
        {
            error.WriteLine(ex.Message);
            return 1;
        }
        catch (IOException ex)
        {
            error.WriteLine($"Error extracting images [{ex.GetType().Name}]: {ex.Message}");
            return 4;
        }
    }

    public static IReadOnlyList<string> Extract(string inputFile, string outputPrefix, string? password = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPrefix);

        using PDDocument document = Loader.LoadPDF(inputFile, password);
        List<string> output = [];
        int counter = 1;
        foreach (PDPage page in document.GetPages())
        {
            ExtractFromResources(page.GetResources(), outputPrefix, output, ref counter);
        }

        return output;
    }

    private static void ExtractFromResources(
        PDResources? resources,
        string outputPrefix,
        List<string> output,
        ref int counter)
    {
        if (resources is null)
        {
            return;
        }

        foreach (PdfBox.Net.COS.COSName name in resources.GetXObjectNames())
        {
            PDXObject? xObject = resources.GetXObject(name);
            if (xObject is PDImageXObject image)
            {
                using BufferedImage bitmap = ToBufferedImage(image);
                string path = $"{outputPrefix}-{counter++}.png";
                ImageIOUtil.WriteImage(bitmap, path, 96);
                output.Add(path);
            }
            else if (xObject is PDFormXObject form)
            {
                ExtractFromResources(form.GetResources(), outputPrefix, output, ref counter);
            }
        }
    }

    private static BufferedImage ToBufferedImage(PDImageXObject image)
    {
        byte[] rgb = SampledImageReader.GetRGBImage(image);
        int width = image.GetWidth();
        int height = image.GetHeight();
        BufferedImage bitmap = new(width, height, BufferedImage.TYPE_INT_RGB);
        int offset = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int red = rgb[offset++];
                int green = rgb[offset++];
                int blue = rgb[offset++];
                bitmap.SetRgb(x, y, unchecked((int)0xFF000000) | (red << 16) | (green << 8) | blue);
            }
        }

        return bitmap;
    }

    private static ExtractImagesOptions ParseOptions(string[]? args)
    {
        args ??= [];
        string? input = null;
        string? outputPrefix = null;
        string? password = null;

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
                case "--output-prefix":
                    outputPrefix = ReadOptionValue(args, ref i, arg);
                    break;
                case "-password":
                    password = ReadOptionValue(args, ref i, arg);
                    break;
                default:
                    throw new ArgumentException($"Unknown option: {arg}");
            }
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Missing required option -i/--input.");
        }

        return new ExtractImagesOptions(input, outputPrefix ?? Path.ChangeExtension(input, null), password);
    }

    private static string ReadOptionValue(string[] args, ref int index, string optionName)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"Missing value for {optionName}.");
        }

        return args[++index];
    }

    private sealed record ExtractImagesOptions(string InputFile, string OutputPrefix, string? Password);
}
