/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: tools/src/main/java/org/apache/pdfbox/tools/ExtractXMP.java
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

namespace PdfBox.Net.Tools;

public static class ExtractXMP
{
    public static void Run() => throw ToolSupport.NotSupported(nameof(ExtractXMP));

    public static int Run(string[] args, TextWriter? output = null, TextWriter? error = null)
    {
        output ??= Console.Out;
        error ??= Console.Error;
        try
        {
            ExtractXMPOptions options = ParseOptions(args);
            byte[] metadata = ExtractMetadata(options.InputFile, options.Page, options.Password);
            if (options.ToConsole)
            {
                output.Write(System.Text.Encoding.UTF8.GetString(metadata));
            }
            else
            {
                string outputPath = options.OutputFile ?? Path.ChangeExtension(options.InputFile, ".xml");
                File.WriteAllBytes(outputPath, metadata);
            }
            return 0;
        }
        catch (ArgumentException ex)
        {
            error.WriteLine(ex.Message);
            return 1;
        }
        catch (IOException ex)
        {
            error.WriteLine($"Error extracting XMP for document [{ex.GetType().Name}]: {ex.Message}");
            return 4;
        }
    }

    public static byte[] ExtractMetadata(string inputFile, int page = 0, string? password = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputFile);
        using PDDocument document = Loader.LoadPDF(inputFile, password);
        PDMetadata? metadata;
        if (page == 0)
        {
            metadata = document.GetDocumentCatalog().GetMetadata();
        }
        else
        {
            if (page < 1 || page > document.GetNumberOfPages())
            {
                throw new ArgumentException($"Page {page} doesn't exist.");
            }
            metadata = document.GetPage(page - 1).GetMetadata();
        }

        if (metadata is null)
        {
            throw new ArgumentException("No XMP metadata available.");
        }

        using Stream stream = metadata.ExportXMPMetadata();
        using MemoryStream buffer = new();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }

    private static ExtractXMPOptions ParseOptions(string[]? args)
    {
        args ??= [];
        string? input = null;
        string? output = null;
        string? password = null;
        int page = 0;
        bool toConsole = false;

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
                case "-password":
                    password = ReadOptionValue(args, ref i, arg);
                    break;
                case "-page":
                    if (!int.TryParse(ReadOptionValue(args, ref i, arg), out page))
                    {
                        throw new ArgumentException("Page must be an integer.");
                    }
                    break;
                case "-console":
                    toConsole = true;
                    break;
                default:
                    throw new ArgumentException($"Unknown option: {arg}");
            }
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Missing required option -i/--input.");
        }

        return new ExtractXMPOptions(input, output, password, page, toConsole);
    }

    private static string ReadOptionValue(string[] args, ref int index, string optionName)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"Missing value for {optionName}.");
        }

        return args[++index];
    }

    private sealed record ExtractXMPOptions(
        string InputFile,
        string? OutputFile,
        string? Password,
        int Page,
        bool ToConsole);
}
