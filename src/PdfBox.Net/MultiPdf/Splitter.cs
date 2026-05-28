/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/multipdf/Splitter.java
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

using PdfBox.Net.COS;
using PdfBox.Net.PDModel;

namespace PdfBox.Net.MultiPdf;

/// <summary>
/// Splits a document into several other documents, one per <see cref="SplitAtPage"/> pages.
/// </summary>
public class Splitter
{
    private int _splitLength = 1;
    private int _startPage = int.MinValue;
    private int _endPage = int.MaxValue;

    /// <summary>
    /// Gets or sets the number of pages to include in each split document.
    /// Defaults to 1 (one page per document).
    /// </summary>
    public int SplitAtPage
    {
        get => _splitLength;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "SplitAtPage must be greater than zero.");
            }

            _splitLength = value;
        }
    }

    /// <summary>
    /// Gets or sets the 1-based first page to include in the split (inclusive).
    /// </summary>
    public int StartPage
    {
        get => _startPage;
        set => _startPage = value;
    }

    /// <summary>
    /// Gets or sets the 1-based last page to include in the split (inclusive).
    /// </summary>
    public int EndPage
    {
        get => _endPage;
        set => _endPage = value;
    }

    /// <summary>
    /// Splits the given document and returns a list of new documents.
    /// The caller is responsible for saving and disposing each returned document.
    /// </summary>
    /// <param name="document">The document to split.</param>
    /// <returns>A list of split documents.</returns>
    public IList<PDDocument> Split(PDDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        int numberOfPages = document.GetNumberOfPages();
        int effectiveStart = Math.Max(_startPage == int.MinValue ? 1 : _startPage, 1);
        int effectiveEnd = Math.Min(_endPage == int.MaxValue ? numberOfPages : _endPage, numberOfPages);

        List<PDDocument> destinations = new();

        if (effectiveStart > effectiveEnd)
        {
            return destinations;
        }

        PDFCloneUtility cloner = new(CreateNewDocument());
        PDDocument? current = null;
        int pagesInCurrent = 0;

        try
        {
            for (int pageNumber = effectiveStart; pageNumber <= effectiveEnd; pageNumber++)
            {
                if (current is null || pagesInCurrent >= _splitLength)
                {
                    if (current is not null)
                    {
                        destinations.Add(current);
                    }

                    current = CreateNewDocument();
                    cloner = new PDFCloneUtility(current);
                    pagesInCurrent = 0;
                }

                PDPage sourcePage = document.GetPage(pageNumber - 1);
                COSDictionary pageDictionary = (COSDictionary)sourcePage.GetCOSObject();
                COSDictionary cloned = cloner.CloneForNewDocument(pageDictionary)
                    ?? throw new IOException($"Unable to clone page dictionary for page {pageNumber}.");

                cloned.RemoveItem(COSName.PARENT);
                current.AddPage(new PDPage(cloned));
                pagesInCurrent++;
            }

            if (current is not null)
            {
                destinations.Add(current);
                current = null;
            }
        }
        catch
        {
            current?.Dispose();
            foreach (PDDocument doc in destinations)
            {
                doc.Dispose();
            }

            throw;
        }

        return destinations;
    }

    /// <summary>
    /// Override this method to control how the destination documents are created.
    /// </summary>
    protected virtual PDDocument CreateNewDocument()
    {
        return new PDDocument();
    }
}
