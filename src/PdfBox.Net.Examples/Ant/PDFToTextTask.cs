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

// Chosen approach: keep this example as a standalone CLI-style wrapper because invoking a tool
// from build scripts is more portable in .NET pipelines than taking an MSBuild task dependency.

using PdfBox.Net.Tools;

namespace PdfBox.Net.Examples.Ant;

/// <summary>
/// The upstream Java example is an Apache Ant <c>Task</c>, but this .NET port keeps the
/// same class name as a standalone command-style example because that is the most portable
/// build-pipeline integration and avoids taking an MSBuild dependency in the examples project.
/// </summary>
public class PDFToTextTask
{
    private string? _pdfFile;
    private string? _outputFile;

    public PDFToTextTask()
    {
    }

    public string? PdfFile
    {
        get => _pdfFile;
        set => _pdfFile = value;
    }

    public string? OutputFile
    {
        get => _outputFile;
        set => _outputFile = value;
    }

    /// <summary>
    /// Execute the configured extraction.
    /// </summary>
    public void Execute()
    {
        Run(_pdfFile, _outputFile);
    }

    public static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.Error.WriteLine("usage: PDFToTextTask <input-pdf> <output-text>");
            return;
        }

        Run(args[0], args[1]);
    }

    private static void Run(string? pdfFile, string? outputFile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pdfFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputFile);

        ExtractText.WriteText(pdfFile, outputFile);
    }
}
