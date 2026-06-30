/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: tools/src/main/java/org/apache/pdfbox/tools/PDFBox.java
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


namespace PdfBox.Net.Tools;

public static class PDFBox
{
    public static int Run(string[] args, TextWriter? output = null, TextWriter? error = null)
    {
        output ??= Console.Out;
        error ??= Console.Error;
        args ??= [];

        if (args.Length == 0)
        {
            error.WriteLine("Error: Subcommand required");
            WriteUsage(error);
            return ToolSupport.UsageExitCode;
        }

        if (ToolSupport.IsHelpOption(args[0]))
        {
            return WriteHelp(args.Skip(1).ToArray(), output);
        }

        if (string.Equals(args[0], "version", StringComparison.OrdinalIgnoreCase))
        {
            output.WriteLine(Version.GetVersion() ?? "unknown");
            return 0;
        }

        string[] commandArgs = args.Skip(1).ToArray();
        if (commandArgs.Length == 1 && ToolSupport.IsHelpOption(commandArgs[0]))
        {
            return WriteHelp([args[0]], output);
        }

        return args[0].ToLowerInvariant() switch
        {
            "debug" => UnsupportedCommand(
                "debug",
                "The Java Swing PDFDebugger UI is accepted as a debugger product adaptation in release/3.0; use the non-packable pdfdebugger inspection app, PdfBox.Net.Debugger models, or build a separate UI package.",
                error),
            "decrypt" => Decrypt.Run(commandArgs, error),
            "encrypt" => Encrypt.Run(commandArgs, error),
            "decode" => WriteDecodedDoc.Run(commandArgs, error),
            "writedecodeddoc" => WriteDecodedDoc.Run(commandArgs, error),
            "decompressobjectstreams" => DecompressObjectstreams.Run(commandArgs, error),
            "export:fdf" => ExportFDF.Run(commandArgs, error),
            "exportfdf" => ExportFDF.Run(commandArgs, error),
            "export:xfdf" => ExportXFDF.Run(commandArgs, error),
            "exportxfdf" => ExportXFDF.Run(commandArgs, error),
            "export:images" => ExtractImages.Run(commandArgs, error),
            "extractimages" => ExtractImages.Run(commandArgs, error),
            "export:xmp" => ExtractXMP.Run(commandArgs, output, error),
            "extractxmp" => ExtractXMP.Run(commandArgs, output, error),
            "export:text" => ExtractText.Run(commandArgs, output, error),
            "extracttext" => ExtractText.Run(commandArgs, output, error),
            "overlay" => OverlayPDF.Run(commandArgs, error),
            "overlaypdf" => OverlayPDF.Run(commandArgs, error),
            "render" => PDFToImage.Run(commandArgs, error),
            "pdftoimage" => PDFToImage.Run(commandArgs, error),
            "merge" => PDFMerger.Run(commandArgs, error),
            "pdfmerger" => PDFMerger.Run(commandArgs, error),
            "split" => PDFSplit.Run(commandArgs, error),
            "pdfsplit" => PDFSplit.Run(commandArgs, error),
            "fromimage" => ImageToPDF.Run(commandArgs, error),
            "imagetopdf" => ImageToPDF.Run(commandArgs, error),
            "import:fdf" => ImportFDF.Run(commandArgs, error),
            "importfdf" => ImportFDF.Run(commandArgs, error),
            "import:xfdf" => ImportXFDF.Run(commandArgs, error),
            "importxfdf" => ImportXFDF.Run(commandArgs, error),
            "print" => PrintPDF.Run(commandArgs, error),
            "printpdf" => PrintPDF.Run(commandArgs, error),
            "fromtext" => TextToPDF.Run(commandArgs, error),
            "texttopdf" => TextToPDF.Run(commandArgs, error),
            _ => UnknownCommand(args[0], error)
        };
    }

    private static int WriteHelp(string[] args, TextWriter output)
    {
        if (args.Length == 0)
        {
            WriteUsage(output);
            return 0;
        }

        string command = args[0].ToLowerInvariant();
        output.WriteLine(command switch
        {
            "decrypt" => "Usage: pdfbox decrypt -i <input.pdf> [-o <output.pdf>] [-password <password>] [-keyStore <file> -alias <alias>]",
            "encrypt" => "Usage: pdfbox encrypt -i <input.pdf> [-o <output.pdf>] [-O <owner>] [-U <user>] [-keyLength <40|128|256>]",
            "decode" or "writedecodeddoc" => "Usage: pdfbox decode [-password <password>] [-skipImages] <input.pdf> [output.pdf]",
            "export:images" => "Usage: pdfbox export:images -i <input.pdf> -o <output-prefix>",
            "export:xmp" => "Usage: pdfbox export:xmp -i <input.pdf> [-o <output.xml>]",
            "export:text" => "Usage: pdfbox export:text -i <input.pdf> [-o <output.txt>] [-console] [-html|-md] [-startPage <n>] [-endPage <n>]",
            "export:fdf" => "Usage: pdfbox export:fdf -i <input.pdf> -o <output.fdf>",
            "export:xfdf" => "Usage: pdfbox export:xfdf -i <input.pdf> -o <output.xfdf>",
            "import:fdf" => "Usage: pdfbox import:fdf -i <input.pdf> --data <input.fdf> -o <output.pdf>",
            "import:xfdf" => "Usage: pdfbox import:xfdf -i <input.pdf> --data <input.xfdf> -o <output.pdf>",
            "overlay" => "Usage: pdfbox overlay -i <input.pdf> -o <output.pdf> -default <overlay.pdf> [options]",
            "print" => "Usage: pdfbox print -i <input.pdf> [options]",
            "render" => "Usage: pdfbox render -i <input.pdf> [-prefix <output-prefix>] [-format jpg|png] [-startPage <n>] [-endPage <n>] [-dpi <n>]",
            "merge" => "Usage: pdfbox merge -i <input1.pdf> <input2.pdf> [...] -o <output.pdf>",
            "split" => "Usage: pdfbox split -i <input.pdf> [-outputPrefix <prefix>] [-split <pages>]",
            "fromimage" => "Usage: pdfbox fromimage -i <input-image> -o <output.pdf>",
            "fromtext" => "Usage: pdfbox fromtext -i <input.txt> -o <output.pdf>",
            "debug" => "Usage: pdfbox debug <input.pdf> is not provided by the core dispatcher; use the non-packable pdfdebugger inspection app.",
            "version" => "Usage: pdfbox version",
            _ => $"No help available for command: {args[0]}"
        });
        return 0;
    }

    private static void WriteUsage(TextWriter writer)
    {
        writer.WriteLine("Usage: pdfbox [COMMAND] [OPTIONS]");
        writer.WriteLine("Commands: debug, decrypt, encrypt, decode, export:images, export:xmp, export:text, export:fdf, export:xfdf, import:fdf, import:xfdf, overlay, print, render, merge, split, fromimage, fromtext, version, help");
        writer.WriteLine("See 'pdfbox help <command>' to read about a specific subcommand.");
    }

    private static int UnknownCommand(string command, TextWriter error)
    {
        error.WriteLine($"Unknown command: {command}");
        return ToolSupport.UsageExitCode;
    }

    private static int UnsupportedCommand(string command, string message, TextWriter error)
    {
        error.WriteLine($"Unsupported command: {command}. {message}");
        return ToolSupport.UsageExitCode;
    }
}
