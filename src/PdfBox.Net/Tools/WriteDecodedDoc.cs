/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: tools/src/main/java/org/apache/pdfbox/tools/WriteDecodedDoc.java
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

public static class WriteDecodedDoc
{
    public static int Run(string[] args, TextWriter? error = null)
    {
        error ??= Console.Error;
        try
        {
            DecodedDocOptions options = ParseOptions(args);
            Rewrite(options.InputFile, options.OutputFile, options.Password);
            return 0;
        }
        catch (ArgumentException ex)
        {
            return ToolSupport.Usage(error, ex.Message);
        }
        catch (IOException ex)
        {
            return ToolSupport.IoError(error, "writing decoded PDF document", ex);
        }
    }

    public static void Rewrite(string inputFile, string outputFile)
    {
        DecompressObjectstreams.Rewrite(inputFile, outputFile);
    }

    public static void Rewrite(string inputFile, string outputFile, string? password)
    {
        DecompressObjectstreams.Rewrite(inputFile, outputFile, password);
    }

    private static DecodedDocOptions ParseOptions(string[]? args)
    {
        args ??= [];
        string? input = null;
        string? output = null;
        string? password = null;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            switch (arg)
            {
                case "-password":
                    password = ToolSupport.ReadOptionValue(args, ref i, arg);
                    break;
                case "-skipImages":
                    break;
                case "-i":
                case "--input":
                    input = ToolSupport.ReadOptionValue(args, ref i, arg);
                    break;
                case "-o":
                case "--output":
                    output = ToolSupport.ReadOptionValue(args, ref i, arg);
                    break;
                default:
                    if (ToolSupport.IsOption(arg))
                    {
                        throw new ArgumentException($"Unknown option: {arg}");
                    }

                    if (input is null)
                    {
                        input = arg;
                    }
                    else if (output is null)
                    {
                        output = arg;
                    }
                    else
                    {
                        throw new ArgumentException($"Unexpected argument: {arg}");
                    }
                    break;
            }
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Missing required input PDF.");
        }

        output ??= CalculateOutputFilename(input);
        return new DecodedDocOptions(input, output, password);
    }

    private static string CalculateOutputFilename(string input)
    {
        string directory = Path.GetDirectoryName(input) ?? string.Empty;
        string name = Path.GetFileNameWithoutExtension(input);
        string outputName = $"{name}_unc.pdf";
        return string.IsNullOrEmpty(directory) ? outputName : Path.Combine(directory, outputName);
    }

    private sealed record DecodedDocOptions(string InputFile, string OutputFile, string? Password);
}
