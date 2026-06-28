/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: tools/src/main/java/org/apache/pdfbox/tools/Decrypt.java
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

namespace PdfBox.Net.Tools;

public static class Decrypt
{
    public static int Run(string[] args, TextWriter? error = null)
    {
        error ??= Console.Error;
        try
        {
            DecryptOptions options = ParseOptions(args);
            DecryptFile(
                options.InputFile,
                options.OutputFile,
                options.Password,
                options.KeyStore,
                options.Alias);
            return 0;
        }
        catch (ArgumentException ex)
        {
            error.WriteLine(ex.Message);
            return 1;
        }
        catch (IOException ex)
        {
            error.WriteLine($"Error decrypting document [{ex.GetType().Name}]: {ex.Message}");
            return 4;
        }
    }

    public static void Run() => throw ToolSupport.NotSupported(nameof(Decrypt));

    public static void DecryptFile(
        string inputFile,
        string? outputFile = null,
        string? password = null,
        string? keyStore = null,
        string? alias = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputFile);

        using Stream? keyStoreStream = keyStore is null ? null : File.OpenRead(keyStore);
        using PDDocument document = Loader.LoadPDF(inputFile, password, keyStoreStream, alias);
        if (!document.IsEncrypted())
        {
            throw new IOException("Document is not encrypted.");
        }

        if (!document.GetCurrentAccessPermission().IsOwnerPermission())
        {
            throw new IOException("You are only allowed to decrypt a document with the owner password.");
        }

        document.SetAllSecurityToBeRemoved(true);
        document.Save(outputFile ?? inputFile);
    }

    private static DecryptOptions ParseOptions(string[]? args)
    {
        args ??= [];
        string? input = null;
        string? output = null;
        string? password = null;
        string? keyStore = null;
        string? alias = null;

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
                case "-keyStore":
                    keyStore = ReadOptionValue(args, ref i, arg);
                    break;
                case "-alias":
                    alias = ReadOptionValue(args, ref i, arg);
                    break;
                default:
                    throw new ArgumentException($"Unknown option: {arg}");
            }
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Missing required option -i/--input.");
        }

        return new DecryptOptions(input, output, password, keyStore, alias);
    }

    private static string ReadOptionValue(string[] args, ref int index, string optionName)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"Missing value for {optionName}.");
        }

        return args[++index];
    }

    private sealed record DecryptOptions(
        string InputFile,
        string? OutputFile,
        string? Password,
        string? KeyStore,
        string? Alias);
}
