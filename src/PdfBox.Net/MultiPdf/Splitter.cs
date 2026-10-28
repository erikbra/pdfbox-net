/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/multipdf/Splitter.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: bd21bd44f5dd050fa716c9bb033adcf725b7ebd3
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
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;
using Microsoft.Extensions.Logging;
using PdfBox.Net.Logging;

namespace PdfBox.Net.MultiPdf;

/// <summary>
/// Splits a document into several other documents, one per <see cref="SplitAtPage"/> pages.
/// </summary>
public class Splitter
{
    private static ILogger<Splitter> LOG => PdfBoxLogging.CreateLogger<Splitter>();

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
        set => _startPage = value > 0 ? value : throw new ArgumentException("Start page is smaller than one");
    }

    /// <summary>
    /// Gets or sets the 1-based last page to include in the split (inclusive).
    /// </summary>
    public int EndPage
    {
        get => _endPage;
        set => _endPage = value > 0 ? value : throw new ArgumentException("End page is smaller than one");
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

        if (_endPage < _startPage)
        {
            throw new ArgumentException("End page is smaller than startPage");
        }
        int numberOfPages = document.GetNumberOfPages();
        int effectiveStart = Math.Max(_startPage == int.MinValue ? 1 : _startPage, 1);
        int effectiveEnd = Math.Min(_endPage == int.MaxValue ? numberOfPages : _endPage, numberOfPages);

        List<PDDocument> destinations = new();
        Dictionary<COSDictionary, (PDDocument Document, COSDictionary Page)> pageMap = new(ReferenceEqualityComparer.Instance);

        if (effectiveStart > effectiveEnd)
        {
            return destinations;
        }

        PDFCloneUtility? cloner = null;
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
                COSDictionary cloned = cloner!.CloneForNewDocument(pageDictionary)
                    ?? throw new IOException($"Unable to clone page dictionary for page {pageNumber}.");

                cloned.RemoveItem(COSName.PARENT);
                current.AddPage(new PDPage(cloned));
                pageMap[pageDictionary] = (current, cloned);
                pagesInCurrent++;
            }

            if (current is not null)
            {
                destinations.Add(current);
                current = null;
            }
            FixNameDestinations(document, destinations, pageMap);
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
    /// PDFBOX-6283: clone named destinations only into the split document that contains their page.
    /// Handles both the /Names/Dests tree and the legacy /Dests dictionary.
    /// </summary>
    private static void FixNameDestinations(PDDocument source, IList<PDDocument> destinations,
        Dictionary<COSDictionary, (PDDocument Document, COSDictionary Page)> pageMap)
    {
        var sourceCatalog = source.GetDocumentCatalog();
        PDDestinationNameTreeNode? tree = sourceCatalog.GetNames()?.GetDests();
        Dictionary<PDDocument, Dictionary<string, PDPageDestination>> trees = new();
        if (tree is not null)
        {
            foreach (var entry in tree.GetTreeAsMap())
            {
                if (CloneDestination(entry.Value, pageMap) is { } clone)
                {
                    if (!trees.TryGetValue(clone.Document, out var names))
                    {
                        names = new(StringComparer.Ordinal);
                        trees[clone.Document] = names;
                    }
                    names[entry.Key] = clone.Destination;
                }
            }
            foreach (var entry in trees)
            {
                PDDocumentCatalog catalog = entry.Key.GetDocumentCatalog();
                PDDocumentNameDictionary names = new(catalog);
                PDDestinationNameTreeNode destinationTree = new();
                destinationTree.SetNames(entry.Value);
                names.SetDests(destinationTree);
                catalog.SetNames(names);
            }
        }
        PDDocumentNameDestinationDictionary? legacy = sourceCatalog.GetDests();
        if (legacy is null)
        {
            return;
        }
        foreach (COSName key in legacy.GetCOSObject().KeySet())
        {
            PDDestination? destination = legacy.GetDestination(key.GetName());
            if (destination is PDPageDestination pageDestination)
            {
                if (CloneDestination(pageDestination, pageMap) is { } clone)
                {
                    COSDictionary catalog = (COSDictionary)clone.Document.GetDocumentCatalog().GetCOSObject();
                    COSDictionary destinationDictionary = catalog.GetCOSDictionary(COSName.DESTS) ?? new();
                    destinationDictionary.SetItem(key, clone.Destination);
                    catalog.SetItem(COSName.DESTS, destinationDictionary);
                }
            }
            else if (destination is not null)
            {
                foreach (PDDocument document in destinations)
                {
                    COSDictionary catalog = (COSDictionary)document.GetDocumentCatalog().GetCOSObject();
                    COSDictionary destinationDictionary = catalog.GetCOSDictionary(COSName.DESTS) ?? new();
                    destinationDictionary.SetItem(key, destination);
                    catalog.SetItem(COSName.DESTS, destinationDictionary);
                }
            }
        }
    }

    private static (PDDocument Document, PDPageDestination Destination)? CloneDestination(
        PDPageDestination? source,
        Dictionary<COSDictionary, (PDDocument Document, COSDictionary Page)> pageMap)
    {
        if (source?.GetPage() is not PDPage page ||
            !pageMap.TryGetValue((COSDictionary)page.GetCOSObject(), out var target))
        {
            return null;
        }
        COSArray array = new();
        foreach (COSBase? value in (COSArray)source.GetCOSObject())
        {
            array.Add(value);
        }
        PDPageDestination clone = (PDPageDestination)PDDestination.Create(array)!;
        clone.SetPage(new PDPage(target.Page));
        return (target.Document, clone);
    }

    /// <summary>
    /// Override this method to control how the destination documents are created.
    /// </summary>
    protected virtual PDDocument CreateNewDocument()
    {
        return new PDDocument();
    }
}
