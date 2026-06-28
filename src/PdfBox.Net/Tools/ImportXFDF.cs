/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: tools/src/main/java/org/apache/pdfbox/tools/ImportXFDF.java
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
using PdfBox.Net.PDModel.Fdf;
using PdfBox.Net.PDModel.Interactive.Form;

namespace PdfBox.Net.Tools;

public static class ImportXFDF
{
    public static void Run() => throw ToolSupport.NotSupported(nameof(ImportXFDF));

    public static int Run(string[] args, TextWriter? error = null)
    {
        error ??= Console.Error;
        try
        {
            ImportXFDFOptions options = ParseOptions(args);
            Import(options.InputFile, options.DataFile, options.OutputFile);
            return 0;
        }
        catch (ArgumentException ex)
        {
            error.WriteLine(ex.Message);
            return 1;
        }
        catch (IOException ex)
        {
            error.WriteLine($"Error importing XFDF data [{ex.GetType().Name}]: {ex.Message}");
            return 4;
        }
    }

    public static void Import(string inputFile, string dataFile, string? outputFile = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(dataFile);

        using PDDocument pdf = Loader.LoadPDF(inputFile);
        using FDFDocument fdf = Loader.LoadXFDF(dataFile);
        ImportFDFDocument(pdf, fdf);
        pdf.Save(outputFile ?? inputFile);
    }

    public static void ImportFDFDocument(PDDocument pdfDocument, FDFDocument fdfDocument)
    {
        ArgumentNullException.ThrowIfNull(pdfDocument);
        ArgumentNullException.ThrowIfNull(fdfDocument);
        PDAcroForm? acroForm = pdfDocument.GetDocumentCatalog().GetAcroForm();
        if (acroForm is null)
        {
            return;
        }

        acroForm.ImportFDF(fdfDocument);
    }

    private static ImportXFDFOptions ParseOptions(string[]? args)
    {
        args ??= [];
        string? input = null;
        string? output = null;
        string? data = null;

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
                case "--data":
                    data = ReadOptionValue(args, ref i, arg);
                    break;
                default:
                    throw new ArgumentException($"Unknown option: {arg}");
            }
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Missing required option -i/--input.");
        }
        if (string.IsNullOrWhiteSpace(data))
        {
            throw new ArgumentException("Missing required option --data.");
        }

        return new ImportXFDFOptions(input, output, data);
    }

    private static string ReadOptionValue(string[] args, ref int index, string optionName)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"Missing value for {optionName}.");
        }

        return args[++index];
    }

    private sealed record ImportXFDFOptions(string InputFile, string? OutputFile, string DataFile);
}
