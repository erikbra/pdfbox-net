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
    private int _startPage = 1;
    private int _endPage;

    /// <summary>
    /// Creates a new extractor targeting all pages of the given document.
    /// </summary>
    /// <param name="sourceDocument">The document to extract pages from.</param>
    public PageExtractor(PDDocument sourceDocument)
    {
        _sourceDocument = sourceDocument ?? throw new ArgumentNullException(nameof(sourceDocument));
        _endPage = sourceDocument.GetNumberOfPages();
    }

    /// <summary>
    /// Creates a new extractor targeting a specific page range.
    /// </summary>
    /// <param name="sourceDocument">The document to extract pages from.</param>
    /// <param name="startPage">The first page to extract (1-based, inclusive).</param>
    /// <param name="endPage">The last page to extract (1-based, inclusive).</param>
    public PageExtractor(PDDocument sourceDocument, int startPage, int endPage)
    {
        _sourceDocument = sourceDocument ?? throw new ArgumentNullException(nameof(sourceDocument));
        _startPage = startPage;
        _endPage = endPage;
    }

    /// <summary>
    /// Gets or sets the first page number to extract (1-based, inclusive).
    /// </summary>
    public int StartPage
    {
        get => _startPage;
        set => _startPage = value;
    }

    /// <summary>
    /// Gets or sets the last page number to extract (1-based, inclusive).
    /// </summary>
    public int EndPage
    {
        get => _endPage;
        set => _endPage = value;
    }

    /// <summary>
    /// Extracts the configured page range into a new document.
    /// </summary>
    /// <returns>A new document containing the extracted pages.</returns>
    public PDDocument Extract()
    {
        int totalPages = _sourceDocument.GetNumberOfPages();
        int start = Math.Max(_startPage, 1);
        int end = Math.Min(_endPage, totalPages);

        if (end - start + 1 <= 0)
        {
            return new PDDocument();
        }

        Splitter splitter = new();
        splitter.StartPage = start;
        splitter.EndPage = end;
        splitter.SplitAtPage = end - start + 1;

        IList<PDDocument> parts = splitter.Split(_sourceDocument);
        return parts.Count > 0 ? parts[0] : new PDDocument();
    }
}
