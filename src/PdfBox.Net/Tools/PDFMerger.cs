/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: tools/src/main/java/org/apache/pdfbox/tools/PDFMerger.java
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

namespace PdfBox.Net.Tools;

public static class PDFMerger
{
    public static int Run(string[] args, TextWriter? error = null)
    {
        error ??= Console.Error;
        try
        {
            MergeOptions options = ParseOptions(args);
            Merge(options.OutputFile, options.InputFiles.ToArray());
            return 0;
        }
        catch (ArgumentException ex)
        {
            return ToolSupport.Usage(error, ex.Message);
        }
        catch (IOException ex)
        {
            return ToolSupport.IoError(error, "merging PDF documents", ex);
        }
    }

    public static void Merge(string destinationFileName, params string[] sourceFiles)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationFileName);
        ArgumentNullException.ThrowIfNull(sourceFiles);
        if (sourceFiles.Length == 0)
        {
            throw new ArgumentException("At least one input PDF is required.", nameof(sourceFiles));
        }

        PDFMergerUtility merger = new()
        {
            DestinationFileName = destinationFileName,
        };

        foreach (string source in sourceFiles)
        {
            merger.AddSource(source);
        }

        merger.MergeDocuments();
    }

    private static MergeOptions ParseOptions(string[]? args)
    {
        args ??= [];
        List<string> inputs = [];
        string? output = null;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            switch (arg)
            {
                case "-i":
                case "--input":
                    int inputCountBeforeOption = inputs.Count;
                    while (i + 1 < args.Length && !ToolSupport.IsOption(args[i + 1]))
                    {
                        inputs.Add(args[++i]);
                    }

                    if (inputs.Count == inputCountBeforeOption)
                    {
                        throw new ArgumentException("Missing value for -i/--input.");
                    }
                    break;
                case "-o":
                case "--output":
                    output = ToolSupport.ReadOptionValue(args, ref i, arg);
                    break;
                default:
                    throw new ArgumentException($"Unknown option: {arg}");
            }
        }

        if (inputs.Count == 0)
        {
            throw new ArgumentException("Missing required option -i/--input.");
        }

        if (string.IsNullOrWhiteSpace(output))
        {
            throw new ArgumentException("Missing required option -o/--output.");
        }

        return new MergeOptions(inputs, output);
    }

    private sealed record MergeOptions(List<string> InputFiles, string OutputFile);
}
