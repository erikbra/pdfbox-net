/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: tools/src/main/java/org/apache/pdfbox/tools/DecompressObjectstreams.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc
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

namespace PdfBox.Net.Tools;

public static class DecompressObjectstreams
{
    /// <summary>Runs the decompress command, optionally overwriting the input PDF.</summary>
    public static int Run(string[] args, TextWriter? error = null)
    {
        error ??= Console.Error;
        string? input = null;
        string? output = null;
        try
        {
            for (int i = 0; i < args.Length; i++)
            {
                string option = args[i];
                if (option is not ("-i" or "--input" or "-o" or "--output"))
                {
                    throw new ArgumentException($"Unknown option: {option}");
                }
                if (++i >= args.Length)
                {
                    throw new ArgumentException($"Missing value for {option}.");
                }
                if (option is "-i" or "--input") input = args[i];
                else output = args[i];
            }
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Missing required option -i/--input.");
            }
            Rewrite(input, output ?? input);
            return 0;
        }
        catch (ArgumentException exception)
        {
            error.WriteLine(exception.Message);
            return 1;
        }
        catch (IOException exception)
        {
            error.WriteLine($"Error processing file [{exception.GetType().Name}]: {exception.Message}");
            return 4;
        }
    }

    public static void Rewrite(string inputFile, string outputFile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputFile);

        using PDDocument document = Loader.LoadPDF(inputFile);
        document.Save(outputFile);
    }
}
