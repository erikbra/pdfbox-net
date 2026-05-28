/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/multipdf/PageExtractor.java
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

namespace PdfBox.Net.MultiPdf;

/// <summary>
/// Extracts one or more sequential pages from a source document into a new document.
/// </summary>
public class PageExtractor
{
    private readonly PDDocument _sourceDocument;

    /// <summary>
    /// Gets or sets the 1-based first page to extract (inclusive). Defaults to 1.
    /// </summary>
    public int StartPage { get; set; } = 1;

    /// <summary>
    /// Gets or sets the 1-based last page to extract (inclusive).
    /// Defaults to the total number of pages in the source document.
    /// </summary>
    public int EndPage { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="PageExtractor"/> that will extract all pages from
    /// the given source document.
    /// </summary>
    /// <param name="sourceDocument">The document from which pages are extracted.</param>
    public PageExtractor(PDDocument sourceDocument)
    {
        _sourceDocument = sourceDocument ?? throw new ArgumentNullException(nameof(sourceDocument));
        EndPage = sourceDocument.GetNumberOfPages();
    }

    /// <summary>
    /// Creates a new instance of <see cref="PageExtractor"/> that will extract the specified
    /// range of pages from the given source document.
    /// </summary>
    /// <param name="sourceDocument">The document from which pages are extracted.</param>
    /// <param name="startPage">The 1-based first page to extract (inclusive).</param>
    /// <param name="endPage">The 1-based last page to extract (inclusive).</param>
    public PageExtractor(PDDocument sourceDocument, int startPage, int endPage)
    {
        _sourceDocument = sourceDocument ?? throw new ArgumentNullException(nameof(sourceDocument));
        StartPage = startPage;
        EndPage = endPage;
    }

    /// <summary>
    /// Extracts the configured page range into a new document and returns it.
    /// </summary>
    /// <remarks>
    /// Both <see cref="StartPage"/> and <see cref="EndPage"/> are inclusive. If
    /// <see cref="EndPage"/> exceeds the number of pages in the source document the extraction
    /// stops at the last page. If <see cref="StartPage"/> is less than 1 it is treated as 1.
    /// If the resulting range is empty, an empty document is returned.
    /// </remarks>
    /// <returns>A new <see cref="PDDocument"/> containing the extracted pages.</returns>
    public PDDocument Extract()
    {
        int effectiveStart = Math.Max(StartPage, 1);
        int effectiveEnd = Math.Min(EndPage, _sourceDocument.GetNumberOfPages());

        if (effectiveEnd - effectiveStart + 1 <= 0)
        {
            return new PDDocument();
        }

        Splitter splitter = new();
        splitter.SetStartPage(effectiveStart);
        splitter.SetEndPage(effectiveEnd);
        splitter.SetSplitAtPage(effectiveEnd - effectiveStart + 1);

        List<PDDocument> split = splitter.Split(_sourceDocument);
        return split[0];
    }
}
