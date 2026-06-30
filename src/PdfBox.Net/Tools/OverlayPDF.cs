/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: tools/src/main/java/org/apache/pdfbox/tools/OverlayPDF.java
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

public static class OverlayPDF
{
    public static int Run(string[] args, TextWriter? error = null)
    {
        error ??= Console.Error;
        try
        {
            OverlayOptions options = ParseOptions(args);
            Apply(options);
            return 0;
        }
        catch (ArgumentException ex)
        {
            return ToolSupport.Usage(error, ex.Message);
        }
        catch (IOException ex)
        {
            return ToolSupport.IoError(error, "overlaying PDF document", ex);
        }
    }

    public static void Apply(string inputFile, string overlayFile, string outputFile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(overlayFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputFile);

        using Overlay overlay = new()
        {
            InputFile = inputFile,
            DefaultOverlayFile = overlayFile,
        };

        using var overlaid = overlay.OverlayDocuments(new Dictionary<int, PdfBox.Net.PDModel.PDDocument>());
        overlaid.Save(outputFile);
    }

    private static void Apply(OverlayOptions options)
    {
        using Overlay overlay = new()
        {
            InputFile = options.InputFile,
            DefaultOverlayFile = options.DefaultOverlayFile,
            FirstPageOverlayFile = options.FirstPageOverlayFile,
            LastPageOverlayFile = options.LastPageOverlayFile,
            OddPageOverlayFile = options.OddPageOverlayFile,
            EvenPageOverlayFile = options.EvenPageOverlayFile,
            AllPagesOverlayFile = options.AllPagesOverlayFile,
            OverlayPosition = options.Position,
            AdjustRotation = options.AdjustRotation,
        };

        using var overlaid = overlay.Process(options.SpecificPageOverlayFile);
        overlaid.Save(options.OutputFile);
    }

    private static OverlayOptions ParseOptions(string[]? args)
    {
        args ??= [];
        string? input = null;
        string? output = null;
        string? defaultOverlay = null;
        string? first = null;
        string? last = null;
        string? odd = null;
        string? even = null;
        string? allPages = null;
        Overlay.Position position = Overlay.Position.BACKGROUND;
        bool adjustRotation = false;
        Dictionary<int, string> pageOverlays = [];

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
                case "-default":
                    defaultOverlay = ToolSupport.ReadOptionValue(args, ref i, arg);
                    break;
                case "-first":
                    first = ToolSupport.ReadOptionValue(args, ref i, arg);
                    break;
                case "-last":
                    last = ToolSupport.ReadOptionValue(args, ref i, arg);
                    break;
                case "-odd":
                    odd = ToolSupport.ReadOptionValue(args, ref i, arg);
                    break;
                case "-even":
                    even = ToolSupport.ReadOptionValue(args, ref i, arg);
                    break;
                case "-useAllPages":
                    allPages = ToolSupport.ReadOptionValue(args, ref i, arg);
                    break;
                case "-page":
                    int page = ToolSupport.ReadIntOption(args, ref i, arg);
                    string overlayFile = ToolSupport.ReadOptionValue(args, ref i, arg);
                    pageOverlays[page] = overlayFile;
                    break;
                case "-position":
                    if (!Enum.TryParse(ToolSupport.ReadOptionValue(args, ref i, arg), ignoreCase: true, out position))
                    {
                        throw new ArgumentException("Position must be FOREGROUND or BACKGROUND.");
                    }
                    break;
                case "-adjustRotation":
                    adjustRotation = true;
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

        if (defaultOverlay is null && first is null && last is null && odd is null && even is null && allPages is null && pageOverlays.Count == 0)
        {
            throw new ArgumentException("Missing overlay source; specify -default, -first, -last, -odd, -even, -useAllPages, or -page.");
        }

        return new OverlayOptions(
            input,
            output,
            defaultOverlay,
            first,
            last,
            odd,
            even,
            allPages,
            position,
            adjustRotation,
            pageOverlays);
    }

    private sealed record OverlayOptions(
        string InputFile,
        string OutputFile,
        string? DefaultOverlayFile,
        string? FirstPageOverlayFile,
        string? LastPageOverlayFile,
        string? OddPageOverlayFile,
        string? EvenPageOverlayFile,
        string? AllPagesOverlayFile,
        Overlay.Position Position,
        bool AdjustRotation,
        Dictionary<int, string> SpecificPageOverlayFile);
}
