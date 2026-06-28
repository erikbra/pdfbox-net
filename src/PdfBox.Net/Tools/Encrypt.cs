/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: tools/src/main/java/org/apache/pdfbox/tools/Encrypt.java
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
using PdfBox.Net.PDModel.Encryption;

namespace PdfBox.Net.Tools;

public static class Encrypt
{
    public static void Run() => throw ToolSupport.NotSupported(nameof(Encrypt));

    public static int Run(string[] args, TextWriter? error = null)
    {
        error ??= Console.Error;
        try
        {
            EncryptOptions options = ParseOptions(args);
            EncryptFile(
                options.InputFile,
                options.OutputFile,
                options.OwnerPassword,
                options.UserPassword,
                options.KeyLength,
                options.Permissions);
            return 0;
        }
        catch (ArgumentException ex)
        {
            error.WriteLine(ex.Message);
            return 1;
        }
        catch (IOException ex)
        {
            error.WriteLine($"Error encrypting PDF [{ex.GetType().Name}]: {ex.Message}");
            return 4;
        }
    }

    public static void EncryptFile(
        string inputFile,
        string? outputFile = null,
        string? ownerPassword = null,
        string? userPassword = null,
        int keyLength = 256,
        AccessPermission? permissions = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputFile);
        using PDDocument document = Loader.LoadPDF(inputFile);
        if (document.IsEncrypted())
        {
            throw new IOException("Document is already encrypted.");
        }

        StandardProtectionPolicy policy = new(
            ownerPassword ?? string.Empty,
            userPassword ?? string.Empty,
            permissions ?? new AccessPermission());
        policy.SetEncryptionKeyLength(keyLength);
        document.Protect(policy);
        document.Save(outputFile ?? inputFile);
    }

    private static EncryptOptions ParseOptions(string[]? args)
    {
        args ??= [];
        string? input = null;
        string? output = null;
        string? ownerPassword = null;
        string? userPassword = null;
        int keyLength = 256;
        AccessPermission permissions = new();

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
                case "-O":
                    ownerPassword = ReadOptionValue(args, ref i, arg);
                    break;
                case "-U":
                    userPassword = ReadOptionValue(args, ref i, arg);
                    break;
                case "-keyLength":
                    if (!int.TryParse(ReadOptionValue(args, ref i, arg), out keyLength))
                    {
                        throw new ArgumentException("Key length must be an integer.");
                    }
                    break;
                case "-canAssemble":
                    permissions.SetCanAssembleDocument(ReadBooleanOption(args, ref i, arg));
                    break;
                case "-canExtractContent":
                    permissions.SetCanExtractContent(ReadBooleanOption(args, ref i, arg));
                    break;
                case "-canExtractForAccessibility":
                    permissions.SetCanExtractForAccessibility(ReadBooleanOption(args, ref i, arg));
                    break;
                case "-canFillInForm":
                    permissions.SetCanFillInForm(ReadBooleanOption(args, ref i, arg));
                    break;
                case "-canModify":
                    permissions.SetCanModify(ReadBooleanOption(args, ref i, arg));
                    break;
                case "-canModifyAnnotations":
                    permissions.SetCanModifyAnnotations(ReadBooleanOption(args, ref i, arg));
                    break;
                case "-canPrint":
                    permissions.SetCanPrint(ReadBooleanOption(args, ref i, arg));
                    break;
                case "-canPrintFaithful":
                    permissions.SetCanPrintFaithful(ReadBooleanOption(args, ref i, arg));
                    break;
                default:
                    throw new ArgumentException($"Unknown option: {arg}");
            }
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Missing required option -i/--input.");
        }

        return new EncryptOptions(input, output, ownerPassword, userPassword, keyLength, permissions);
    }

    private static bool ReadBooleanOption(string[] args, ref int index, string optionName)
    {
        string value = ReadOptionValue(args, ref index, optionName);
        if (bool.TryParse(value, out bool result))
        {
            return result;
        }

        throw new ArgumentException($"Value for {optionName} must be true or false.");
    }

    private static string ReadOptionValue(string[] args, ref int index, string optionName)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"Missing value for {optionName}.");
        }

        return args[++index];
    }

    private sealed record EncryptOptions(
        string InputFile,
        string? OutputFile,
        string? OwnerPassword,
        string? UserPassword,
        int KeyLength,
        AccessPermission Permissions);
}
