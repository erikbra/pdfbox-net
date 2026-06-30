/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: tools/src/main/java/org/apache/pdfbox/tools/ExtractText.java
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
using PdfBox.Net.Text;
using System.Text;

namespace PdfBox.Net.Tools;

public static class ExtractText
{
    public static string GetText(string inputPath, string? password = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        using PDDocument document = Loader.LoadPDF(inputPath, password);
        return new PDFTextStripper().GetText(document);
    }

    public static string GetText(
        string inputPath,
        string? password,
        int startPage,
        int endPage,
        bool sortByPosition,
        bool ignoreBeads)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        using PDDocument document = Loader.LoadPDF(inputPath, password);
        PDFTextStripper stripper = new();
        stripper.SetStartPage(startPage);
        stripper.SetEndPage(endPage);
        stripper.SetSortByPosition(sortByPosition);
        if (ignoreBeads)
        {
            stripper.SetShouldSeparateByBeads(false);
        }

        return stripper.GetText(document);
    }

    public static void WriteText(string inputPath, string outputPath, string? password = null)
    {
        string text = GetText(inputPath, password);
        File.WriteAllText(outputPath, text);
    }

    public static int Run(string[] args, TextWriter? output = null, TextWriter? error = null)
    {
        output ??= Console.Out;
        error ??= Console.Error;
        try
        {
            ExtractTextOptions options = ParseOptions(args);
            string text = GetText(
                options.InputFile,
                options.Password,
                options.StartPage,
                options.EndPage,
                options.SortByPosition,
                options.IgnoreBeads);

            if (options.ToHtml)
            {
                text = PDFText2HTML.ConvertText(text);
            }
            else if (options.ToMarkdown)
            {
                text = PDFText2Markdown.ConvertText(text);
            }

            if (options.AddFileName)
            {
                text = $"{options.InputFile}{Environment.NewLine}{text}";
            }

            if (options.ToConsole)
            {
                output.Write(text);
            }
            else
            {
                string outputPath = options.OutputFile ?? DefaultOutputPath(options.InputFile, options.ToHtml, options.ToMarkdown);
                Encoding encoding = Encoding.GetEncoding(options.EncodingName);
                if (options.Append)
                {
                    File.AppendAllText(outputPath, text, encoding);
                }
                else
                {
                    File.WriteAllText(outputPath, text, encoding);
                }
            }

            return 0;
        }
        catch (ArgumentException ex)
        {
            return ToolSupport.Usage(error, ex.Message);
        }
        catch (IOException ex)
        {
            return ToolSupport.IoError(error, "extracting text", ex);
        }
    }

    private static ExtractTextOptions ParseOptions(string[]? args)
    {
        args ??= [];
        string? input = null;
        string? output = null;
        string? password = null;
        string encodingName = "UTF-8";
        int startPage = 1;
        int endPage = int.MaxValue;
        bool toConsole = false;
        bool toHtml = false;
        bool toMarkdown = false;
        bool sortByPosition = false;
        bool ignoreBeads = false;
        bool addFileName = false;
        bool append = false;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            switch (arg)
            {
                case "-i":
                case "--input":
                    input = ToolSupport.ReadOptionValue(args, ref i, arg);
                    break;
                case "-o":
                case "--output":
                    output = ToolSupport.ReadOptionValue(args, ref i, arg);
                    break;
                case "-password":
                    password = ToolSupport.ReadOptionValue(args, ref i, arg);
                    break;
                case "-encoding":
                    encodingName = ToolSupport.ReadOptionValue(args, ref i, arg);
                    break;
                case "-startPage":
                    startPage = ToolSupport.ReadIntOption(args, ref i, arg);
                    break;
                case "-endPage":
                    endPage = ToolSupport.ReadIntOption(args, ref i, arg);
                    break;
                case "-console":
                    toConsole = true;
                    break;
                case "-html":
                    toHtml = true;
                    break;
                case "-md":
                    toMarkdown = true;
                    break;
                case "-sort":
                    sortByPosition = true;
                    break;
                case "-ignoreBeads":
                    ignoreBeads = true;
                    break;
                case "-addFileName":
                    addFileName = true;
                    break;
                case "-append":
                    append = true;
                    break;
                case "-alwaysNext":
                case "-debug":
                case "-rotationMagic":
                    break;
                default:
                    throw new ArgumentException($"Unknown option: {arg}");
            }
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Missing required option -i/--input.");
        }

        if (toHtml && toMarkdown)
        {
            throw new ArgumentException("Only one of -html and -md can be used.");
        }

        return new ExtractTextOptions(
            input,
            output,
            password,
            encodingName,
            startPage,
            endPage,
            toConsole,
            toHtml,
            toMarkdown,
            sortByPosition,
            ignoreBeads,
            addFileName,
            append);
    }

    private static string DefaultOutputPath(string inputPath, bool toHtml, bool toMarkdown)
    {
        string extension = toHtml ? ".html" : toMarkdown ? ".md" : ".txt";
        string? directory = Path.GetDirectoryName(inputPath);
        string fileName = Path.GetFileNameWithoutExtension(inputPath) + extension;
        return string.IsNullOrEmpty(directory) ? fileName : Path.Combine(directory, fileName);
    }

    private sealed record ExtractTextOptions(
        string InputFile,
        string? OutputFile,
        string? Password,
        string EncodingName,
        int StartPage,
        int EndPage,
        bool ToConsole,
        bool ToHtml,
        bool ToMarkdown,
        bool SortByPosition,
        bool IgnoreBeads,
        bool AddFileName,
        bool Append);
}
