/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/ant/PDFToTextTask.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
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

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using PdfBox.Net.PDModel;
using PdfBox.Net.Text;
using MSBuildTask = Microsoft.Build.Utilities.Task;

namespace PdfBox.Net.Examples.Ant;

/// <summary>
/// MSBuild task equivalent of the upstream Apache Ant task that extracts text from a PDF file.
/// </summary>
public class PDFToTextTask : MSBuildTask
{
    /// <summary>
    /// Gets or sets the source PDF file.
    /// </summary>
    [Required]
    public ITaskItem? InputFile { get; set; }

    /// <summary>
    /// Gets or sets the extracted text output file.
    /// </summary>
    [Required]
    public ITaskItem? OutputFile { get; set; }

    /// <summary>
    /// Execute the configured extraction.
    /// </summary>
    public override bool Execute()
    {
        if (InputFile == null)
        {
            Log.LogError("InputFile is required.");
            return false;
        }

        if (OutputFile == null)
        {
            Log.LogError("OutputFile is required.");
            return false;
        }

        try
        {
            ExtractText(InputFile.ItemSpec, OutputFile.ItemSpec);
            return !Log.HasLoggedErrors;
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException(ex, showStackTrace: true);
            return false;
        }
    }

    public static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.Error.WriteLine("usage: PDFToTextTask <input-pdf> <output-text>");
            return;
        }

        ExtractText(args[0], args[1]);
    }

    private static void ExtractText(string? pdfFile, string? outputFile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pdfFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputFile);

        using PDDocument document = Loader.LoadPDF(pdfFile);
        string text = new PDFTextStripper().GetText(document);
        File.WriteAllText(outputFile, text);
    }
}
