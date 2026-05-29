/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: tools/src/main/java/org/apache/pdfbox/tools/PDFSplit.java
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

using PdfBox.Net.MultiPdf;
using PdfBox.Net.PDModel;

namespace PdfBox.Net.Tools;

public static class PDFSplit
{
    public static IReadOnlyList<string> Split(string inputFileName, string outputDirectory, int splitAtPage = 1)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputFileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        using PDDocument source = Loader.LoadPDF(inputFileName);
        Splitter splitter = new() { SplitAtPage = splitAtPage };
        IList<PDDocument> parts = splitter.Split(source);

        List<string> paths = new(parts.Count);
        for (int i = 0; i < parts.Count; i++)
        {
            using PDDocument part = parts[i];
            string path = Path.Combine(outputDirectory, $"split-{i + 1}.pdf");
            part.Save(path);
            paths.Add(path);
        }

        return paths;
    }
}
