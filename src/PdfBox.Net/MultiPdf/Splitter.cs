/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/multipdf/Splitter.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: a9ae066a942c122c9dded9bfa1b08dfae4078ba4
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
    /// Takes a document and splits it into several other documents.
    /// </summary>
    /// <remarks>
    /// This class is not thread-safe. Use one instance per thread, and do not access the source
    /// document from other threads while it is being split.
    /// All created documents are closed if the method throws. In that case the caller should
    /// take care to close the source as well.
    /// </remarks>
    /// <param name="document">The document to split.</param>
    /// <returns>
    /// All split documents. Save all of them before closing any document, including the source.
    /// Perform further operations after reloading them to avoid problems due to resource sharing.
    /// For the same reason, they should not be saved with encryption.
    /// </returns>
    /// <exception cref="IOException">If there is an I/O error.</exception>
    /// <exception cref="ArgumentException">If the end page is smaller than the start page.</exception>
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

        Dictionary<PDDocument, List<(PDPageDestination Destination, COSDictionary SourcePage)>> destinationsToFix = new();
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
                PDPage imported = new(cloned);
                current.AddPage(imported);
                PrepareLinkDestinations(sourcePage, imported, current, destinationsToFix);
                pageMap[pageDictionary] = (current, cloned);
                pagesInCurrent++;
            }

            if (current is not null)
            {
                destinations.Add(current);
                current = null;
            }
            FixLinkDestinations(destinationsToFix, pageMap);
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
    /// Prepares link destinations so that they don't pull pages of other split documents into
    /// this one. The destinations are fixed or removed after all pages have been processed.
    /// </summary>
    private static void PrepareLinkDestinations(PDPage source, PDPage target, PDDocument destination,
        Dictionary<PDDocument, List<(PDPageDestination Destination, COSDictionary SourcePage)>> destinationsToFix)
    {
        var sourceAnnotations = source.GetAnnotations();
        var targetAnnotations = target.GetAnnotations();
        for (int i = 0; i < sourceAnnotations.Count; i++)
        {
            COSDictionary sourceAnnotation = (COSDictionary)sourceAnnotations[i].GetCOSObject();
            if (sourceAnnotation.GetNameAsString(COSName.SUBTYPE) != "Link")
            {
                continue;
            }
            COSDictionary targetAnnotation = (COSDictionary)targetAnnotations[i].GetCOSObject();
            COSName key = COSName.DEST;
            COSDictionary? action = sourceAnnotation.GetCOSDictionary(COSName.A);
            if (action?.GetNameAsString(COSName.S) == "GoTo")
            {
                sourceAnnotation = action;
                targetAnnotation = targetAnnotation.GetCOSDictionary(COSName.A)!;
                key = COSName.D;
            }
            if (sourceAnnotation.GetDictionaryObject(key) is not COSArray array ||
                PDDestination.Create(array) is not PDPageDestination pageDestination ||
                pageDestination.GetPage() is not PDPage sourcePage)
            {
                continue;
            }
            COSArray clone = new();
            foreach (COSBase? value in array)
            {
                clone.Add(value);
            }
            PDPageDestination clonedDestination = (PDPageDestination)PDDestination.Create(clone)!;
            targetAnnotation.SetItem(key, clonedDestination);
            if (!destinationsToFix.TryGetValue(destination, out var list))
            {
                list = new();
                destinationsToFix[destination] = list;
            }
            list.Add((clonedDestination, (COSDictionary)sourcePage.GetCOSObject()));
        }
    }

    /// <summary>
    /// Replaces each source page in a link destination by the corresponding page of that split
    /// document, or removes the page if it is not part of that document.
    /// </summary>
    private static void FixLinkDestinations(
        Dictionary<PDDocument, List<(PDPageDestination Destination, COSDictionary SourcePage)>> destinationsToFix,
        Dictionary<COSDictionary, (PDDocument Document, COSDictionary Page)> pageMap)
    {
        foreach (var entry in destinationsToFix)
        {
            foreach (var link in entry.Value)
            {
                if (pageMap.TryGetValue(link.SourcePage, out var target) && ReferenceEquals(target.Document, entry.Key))
                {
                    link.Destination.SetPage(new PDPage(target.Page));
                }
                else
                {
                    // SetPage's existing .NET signature is non-nullable; the PDF destination
                    // array itself supports the null page required for a cross-document link.
                    ((COSArray)link.Destination.GetCOSObject()).Set(0, (COSBase?)null);
                }
            }
        }
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
