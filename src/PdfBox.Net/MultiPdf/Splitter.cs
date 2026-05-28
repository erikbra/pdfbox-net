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
/// Splits a PDF document into one or more separate documents.
/// </summary>
public class Splitter
{
    private PDDocument? _sourceDocument;
    private PDDocument? _currentDestinationDocument;

    private int _splitLength = 1;
    private int _startPage = int.MinValue;
    private int _endPage = int.MaxValue;
    private List<PDDocument>? _destinationDocuments;

    private int _currentPageNumber;

    /// <summary>
    /// Splits the given document according to the configured start page, end page, and split length.
    /// </summary>
    /// <param name="document">The document to split.</param>
    /// <returns>
    /// A list of all split documents. Each document should be saved before any of them are
    /// closed (including the source document). Any further operations should be done after
    /// reloading from the saved data.
    /// </returns>
    public List<PDDocument> Split(PDDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        _currentPageNumber = 0;
        _destinationDocuments = [];
        _sourceDocument = document;

        ProcessPages();

        return _destinationDocuments;
    }

    /// <summary>
    /// Sets the number of pages at which to split. The default is 1, meaning every page becomes
    /// its own document. A value of 2 means each output document will contain up to 2 pages.
    /// </summary>
    /// <param name="split">The number of pages per split document.</param>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="split"/> is less than 1.</exception>
    public void SetSplitAtPage(int split)
    {
        if (split <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(split), "Number of pages is smaller than one.");
        }

        _splitLength = split;
    }

    /// <summary>
    /// Sets the 1-based first page to include in the split output. Pages before this are skipped.
    /// </summary>
    /// <param name="start">The 1-based start page (inclusive).</param>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="start"/> is less than 1.</exception>
    public void SetStartPage(int start)
    {
        if (start <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(start), "Start page is smaller than one.");
        }

        _startPage = start;
    }

    /// <summary>
    /// Sets the 1-based last page to include in the split output. Pages after this are skipped.
    /// </summary>
    /// <param name="end">The 1-based end page (inclusive).</param>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="end"/> is less than 1.</exception>
    public void SetEndPage(int end)
    {
        if (end <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(end), "End page is smaller than one.");
        }

        _endPage = end;
    }

    /// <summary>
    /// Determines whether a new output document should begin before the page at the given
    /// zero-based page number. Override to implement custom splitting logic.
    /// </summary>
    /// <param name="pageNumber">Zero-based page number within the selected range.</param>
    /// <returns><see langword="true"/> if a new document should be created.</returns>
    protected virtual bool SplitAtPage(int pageNumber)
    {
        return (pageNumber + 1 - Math.Max(1, _startPage)) % _splitLength == 0;
    }

    /// <summary>
    /// Creates a new destination document for the next split. Override to customize document
    /// creation (for example to copy catalog metadata from the source).
    /// </summary>
    /// <returns>A new empty <see cref="PDDocument"/>.</returns>
    protected virtual PDDocument CreateNewDocument()
    {
        PDDocument destination = new();
        PDDocumentCatalog destCatalog = destination.GetDocumentCatalog();
        PDDocumentCatalog srcCatalog = GetSourceDocument().GetDocumentCatalog();
        destCatalog.SetViewerPreferences(srcCatalog.GetViewerPreferences());
        destCatalog.SetLanguage(srcCatalog.GetLanguage());
        destCatalog.SetMarkInfo(srcCatalog.GetMarkInfo());
        return destination;
    }

    /// <summary>
    /// Processes and imports the given page into the current destination document. Override to
    /// apply custom per-page transformations.
    /// </summary>
    /// <param name="page">The source page to import.</param>
    protected virtual void ProcessPage(PDPage page)
    {
        CreateNewDocumentIfNecessary();

        PDFCloneUtility cloner = new(GetDestinationDocument());
        COSDictionary pageDictionary = (COSDictionary)page.GetCOSObject();
        COSDictionary clonedPageDictionary = cloner.CloneForNewDocument(pageDictionary)
            ?? throw new IOException("Unable to clone source page dictionary.");

        clonedPageDictionary.RemoveItem(COSName.PARENT);

        // Remove thread beads — their cross-page links are not valid in a split document.
        clonedPageDictionary.RemoveItem(COSName.B);

        PDPage newPage = new(clonedPageDictionary);
        GetDestinationDocument().AddPage(newPage);
    }

    /// <summary>
    /// Returns the source PDF document.
    /// </summary>
    protected PDDocument GetSourceDocument()
    {
        return _sourceDocument ?? throw new InvalidOperationException("Split has not been started.");
    }

    /// <summary>
    /// Returns the current destination document.
    /// </summary>
    protected PDDocument GetDestinationDocument()
    {
        return _currentDestinationDocument ?? throw new InvalidOperationException("No destination document is active.");
    }

    private void ProcessPages()
    {
        foreach (PDPage page in GetSourceDocument().GetPages())
        {
            int oneBasedPageNumber = _currentPageNumber + 1;
            if (oneBasedPageNumber >= _startPage && oneBasedPageNumber <= _endPage)
            {
                ProcessPage(page);
                _currentPageNumber++;
            }
            else
            {
                if (_currentPageNumber > _endPage)
                {
                    break;
                }

                _currentPageNumber++;
            }
        }
    }

    private void CreateNewDocumentIfNecessary()
    {
        if (SplitAtPage(_currentPageNumber) || _currentDestinationDocument is null)
        {
            _currentDestinationDocument = CreateNewDocument();
            _destinationDocuments!.Add(_currentDestinationDocument);
        }
    }
}
