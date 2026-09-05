/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/ExtractEmbeddedFiles.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: 046747da99a870902217efabf1c41297de157059
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
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Common.FileSpecification;
using PdfBox.Net.PDModel.Interactive.Annotation;

namespace PdfBox.Net.Examples.PDModel;

/// <summary>This is an example on how to extract all embedded files from a PDF document.</summary>
public class ExtractEmbeddedFiles
{
    private ExtractEmbeddedFiles()
    {
    }

    /// <summary>Extracts embedded files beside the input PDF.</summary>
    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Usage: ExtractEmbeddedFiles <input-pdf>");
            return;
        }

        string directoryPath = CanonicalPath(Path.GetDirectoryName(Path.GetFullPath(args[0]))!);
        using PDDocument document = Loader.LoadPDF(args[0]);
        PDDocumentNameDictionary namesDictionary = new(document.GetDocumentCatalog());
        PDEmbeddedFilesNameTreeNode? efTree = namesDictionary.GetEmbeddedFiles();
        if (efTree is not null)
        {
            ExtractFilesFromTree(efTree, directoryPath);
        }

        foreach (PDPage page in document.GetPages())
        {
            foreach (PDAnnotation annotation in page.GetAnnotations())
            {
                if (annotation is PDAnnotationFileAttachment attachment &&
                    attachment.GetFile() is PDComplexFileSpecification fileSpec)
                {
                    ExtractFile(fileSpec, directoryPath);
                }
            }
        }
    }

    private static void ExtractFilesFromTree(PDNameTreeNode<PDComplexFileSpecification> efTree, string directoryPath)
    {
        IReadOnlyDictionary<string, PDComplexFileSpecification>? names = efTree.GetNames();
        if (names is not null)
        {
            foreach (PDComplexFileSpecification fileSpec in names.Values)
            {
                ExtractFile(fileSpec, directoryPath);
            }
            return;
        }

        IList<PDNameTreeNode<PDComplexFileSpecification>>? kids = efTree.GetKids();
        if (kids is not null)
        {
            foreach (PDNameTreeNode<PDComplexFileSpecification> kid in kids)
            {
                ExtractFilesFromTree(kid, directoryPath);
            }
        }
    }

    private static void ExtractFile(PDComplexFileSpecification fileSpec, string directoryPath)
    {
        PDEmbeddedFile? embeddedFile = fileSpec.GetEmbeddedFileUnicode()
            ?? fileSpec.GetEmbeddedFileDos()
            ?? fileSpec.GetEmbeddedFileMac()
            ?? fileSpec.GetEmbeddedFileUnix()
            ?? fileSpec.GetEmbeddedFile();
        if (embeddedFile is null || fileSpec.GetFilename() is not string filename)
        {
            return;
        }

        string file = CanonicalPath(Path.Combine(directoryPath, filename));
        string parent = Path.GetDirectoryName(file)!;
        StringComparison comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        string directoryPrefix = Path.TrimEndingDirectorySeparator(directoryPath) + Path.DirectorySeparatorChar;
        if (!parent.Equals(directoryPath, comparison) && !parent.StartsWith(directoryPrefix, comparison))
        {
            Console.Error.WriteLine("Ignoring " + filename + " (different directory)");
            return;
        }

        if (!Directory.Exists(parent))
        {
            Console.WriteLine("Creating " + parent);
            Directory.CreateDirectory(parent);
        }
        Console.WriteLine("Writing " + file);
        File.WriteAllBytes(file, embeddedFile.ToByteArray());
    }

    // PORT-LOCAL-START: .NET equivalent of File.getCanonicalFile(), including a final file symlink.
    private static string CanonicalPath(string path)
    {
        string fullPath = Path.GetFullPath(path);
        string root = Path.GetPathRoot(fullPath)!;
        string current = root;
        foreach (string part in fullPath[root.Length..].Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
        {
            current = Path.Combine(current, part);
            FileSystemInfo entry = Directory.Exists(current) ? new DirectoryInfo(current) : new FileInfo(current);
            if (entry.LinkTarget is not null)
            {
                current = entry.ResolveLinkTarget(returnFinalTarget: true)?.FullName
                    ?? throw new IOException("Cannot resolve symbolic link: " + current);
            }
        }
        return Path.GetFullPath(current);
    }
    // PORT-LOCAL-END
}
