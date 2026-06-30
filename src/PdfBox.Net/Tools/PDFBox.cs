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

        if (args.Length == 0 || string.Equals(args[0], "version", StringComparison.OrdinalIgnoreCase))
        {
            output.WriteLine(Version.GetVersion() ?? "unknown");
            return 0;
        }

        string[] commandArgs = args.Skip(1).ToArray();
        return args[0].ToLowerInvariant() switch
        {
            "decrypt" => Decrypt.Run(commandArgs, error),
            "encrypt" => Encrypt.Run(commandArgs, error),
            "export:fdf" => ExportFDF.Run(commandArgs, error),
            "exportfdf" => ExportFDF.Run(commandArgs, error),
            "export:xfdf" => ExportXFDF.Run(commandArgs, error),
            "exportxfdf" => ExportXFDF.Run(commandArgs, error),
            "export:images" => ExtractImages.Run(commandArgs, error),
            "extractimages" => ExtractImages.Run(commandArgs, error),
            "export:xmp" => ExtractXMP.Run(commandArgs, output, error),
            "extractxmp" => ExtractXMP.Run(commandArgs, output, error),
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

    private static int UnknownCommand(string command, TextWriter error)
    {
        error.WriteLine($"Unknown command: {command}");
        return 1;
    }
}
