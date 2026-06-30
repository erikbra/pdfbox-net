/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: tools/src/main/java/org/apache/pdfbox/tools/PDFSplit.java
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

using PdfBox.Net.MultiPdf;
using PdfBox.Net.PDModel;

namespace PdfBox.Net.Tools;

public static class PDFSplit
{
    public static int Run(string[] args, TextWriter? error = null)
    {
        error ??= Console.Error;
        try
        {
            SplitOptions options = ParseOptions(args);
            SplitToPrefix(
                options.InputFile,
                options.OutputPrefix,
                options.SplitAtPage,
                options.Password,
                options.StartPage,
                options.EndPage);
            return 0;
        }
        catch (ArgumentException ex)
        {
            return ToolSupport.Usage(error, ex.Message);
        }
        catch (IOException ex)
        {
            return ToolSupport.IoError(error, "splitting PDF document", ex);
        }
    }

    public static IReadOnlyList<string> Split(string inputFileName, string outputDirectory, int splitAtPage = 1)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputFileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        using PDDocument source = Loader.LoadPDF(inputFileName);
        Splitter splitter = new() { SplitAtPage = splitAtPage };
        IList<PDDocument> parts = splitter.Split(source);

        List<string> paths = new(parts.Count);
        for (int i = 0; i < parts.Count; i++)
        {
            using PDDocument part = parts[i];
            string path = Path.Combine(outputDirectory, $"split-{i + 1}.pdf");
            part.Save(path);
            paths.Add(path);
        }

        return paths;
    }

    public static IReadOnlyList<string> SplitToPrefix(
        string inputFileName,
        string outputPrefix,
        int splitAtPage = 1,
        string? password = null,
        int startPage = 1,
        int endPage = int.MaxValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputFileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPrefix);
        if (splitAtPage < 1)
        {
            throw new ArgumentException("Split page interval must be at least 1.", nameof(splitAtPage));
        }

        string? directory = Path.GetDirectoryName(outputPrefix);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using PDDocument source = Loader.LoadPDF(inputFileName, password);
        Splitter splitter = new()
        {
            SplitAtPage = splitAtPage,
            StartPage = startPage,
            EndPage = endPage,
        };
        IList<PDDocument> parts = splitter.Split(source);

        List<string> paths = new(parts.Count);
        for (int i = 0; i < parts.Count; i++)
        {
            using PDDocument part = parts[i];
            string path = $"{outputPrefix}-{i + 1}.pdf";
            part.Save(path);
            paths.Add(path);
        }

        return paths;
    }

    private static SplitOptions ParseOptions(string[]? args)
    {
        args ??= [];
        string? input = null;
        string? outputPrefix = null;
        string? password = null;
        int splitAtPage = 1;
        int startPage = 1;
        int endPage = int.MaxValue;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            switch (arg)
            {
                case "-i":
                case "--input":
                    input = ToolSupport.ReadOptionValue(args, ref i, arg);
                    break;
                case "-password":
                    password = ToolSupport.ReadOptionValue(args, ref i, arg);
                    break;
                case "-split":
                    splitAtPage = ToolSupport.ReadIntOption(args, ref i, arg);
                    break;
                case "-outputPrefix":
                    outputPrefix = ToolSupport.ReadOptionValue(args, ref i, arg);
                    break;
                case "-startPage":
                    startPage = ToolSupport.ReadIntOption(args, ref i, arg);
                    break;
                case "-endPage":
                    endPage = ToolSupport.ReadIntOption(args, ref i, arg);
                    break;
                default:
                    throw new ArgumentException($"Unknown option: {arg}");
            }
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Missing required option -i/--input.");
        }

        outputPrefix ??= Path.Combine(
            Path.GetDirectoryName(Path.GetFullPath(input)) ?? string.Empty,
            Path.GetFileNameWithoutExtension(input));

        return new SplitOptions(input, outputPrefix, splitAtPage, password, startPage, endPage);
    }

    private sealed record SplitOptions(
        string InputFile,
        string OutputPrefix,
        int SplitAtPage,
        string? Password,
        int StartPage,
        int EndPage);
}
