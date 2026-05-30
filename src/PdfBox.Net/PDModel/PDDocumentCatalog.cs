/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDDocumentCatalog.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
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
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Fixup;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.DocumentInterchange.LogicalStructure;
using PdfBox.Net.PDModel.Graphics.OptionalContent;
using PdfBox.Net.PDModel.Interactive.Action;
using PdfBox.Net.PDModel.Interactive.Form;
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Destination;
using PdfBox.Net.PDModel.Interactive.DocumentNavigation.Outline;
using PdfBox.Net.PDModel.Interactive.PageNavigation;
using PdfBox.Net.PDModel.Interactive.ViewerPreferences;

namespace PdfBox.Net.PDModel;

/// <summary>
/// The Document Catalog of a PDF.
/// </summary>
/// <remarks>
/// Ported from Apache PDFBox <c>PDDocumentCatalog</c>.
/// </remarks>
public sealed class PDDocumentCatalog : COSObjectable
{
    private readonly COSDictionary _root;
    private readonly PDDocument _document;
    private PDDocumentFixup? _acroFormFixupApplied;
    private PDAcroForm? _cachedAcroForm;

    /// <summary>
    /// Constructor. Internal PDFBox use only! If you need to get the document catalog, call
    /// <see cref="PDDocument.GetDocumentCatalog()"/>.
    /// </summary>
    /// <param name="doc">The document that this catalog is part of.</param>
    internal PDDocumentCatalog(PDDocument doc)
    {
        _document = doc ?? throw new ArgumentNullException(nameof(doc));
        _root = new COSDictionary();
        _root.SetItem(COSName.TYPE, COSName.CATALOG);
        _document.GetDocument().SetItem(COSName.ROOT, _root);
    }

    /// <summary>
    /// Constructor. Internal PDFBox use only! If you need to get the document catalog, call
    /// <see cref="PDDocument.GetDocumentCatalog()"/>.
    /// </summary>
    /// <param name="doc">The document that this catalog is part of.</param>
    /// <param name="rootDictionary">The root dictionary that this object wraps.</param>
    internal PDDocumentCatalog(PDDocument doc, COSDictionary rootDictionary)
    {
        _document = doc ?? throw new ArgumentNullException(nameof(doc));
        _root = rootDictionary ?? throw new ArgumentNullException(nameof(rootDictionary));
    }

    /// <summary>
    /// Returns the underlying COS dictionary.
    /// </summary>
    /// <returns>The catalog dictionary.</returns>
    public COSBase GetCOSObject()
    {
        return _root;
    }

    /// <summary>
    /// Returns all pages in the document, as a page tree.
    /// </summary>
    /// <returns><see cref="PDPageTree"/> providing all pages of the document.</returns>
    /// <exception cref="IOException">Thrown when the catalog does not contain a <c>/Pages</c> dictionary.</exception>
    public PDPageTree GetPages()
    {
        COSDictionary pages = _root.GetCOSDictionary(COSName.PAGES) ?? throw new IOException("Document catalog is missing /Pages dictionary.");
        return new PDPageTree(pages, _document);
    }

    /// <summary>
    /// Returns the number of pages in the page tree.
    /// </summary>
    /// <returns>The page count.</returns>
    public int GetPageCount()
    {
        return GetPages().GetCount();
    }

    /// <summary>
    /// Returns the PDF version stored in the catalog, if present.
    /// </summary>
    /// <returns>The catalog version, or <see langword="null"/>.</returns>
    public string? GetVersion()
    {
        return _root.GetString(COSName.VERSION) ?? _root.GetNameAsString(COSName.VERSION);
    }

    /// <summary>
    /// Sets the PDF version in the catalog dictionary.
    /// </summary>
    /// <param name="version">The version value, for example <c>1.7</c>.</param>
    public void SetVersion(string? version)
    {
        _root.SetName(COSName.VERSION, version);
    }

    /// <summary>
    /// Gets the document catalog type name.
    /// </summary>
    /// <returns>The type name, typically <c>Catalog</c>.</returns>
    public string? GetTypeName()
    {
        return _root.GetNameAsString(COSName.TYPE);
    }

    /// <summary>
    /// Gets the page layout to be used when the document is opened.
    /// </summary>
    /// <returns>The <see cref="PageLayout"/>, or <see langword="null"/> if not set.</returns>
    public PageLayout? GetPageLayout()
    {
        string? value = _root.GetNameAsString(COSName.PAGE_LAYOUT);
        if (value is null)
        {
            return null;
        }

        try
        {
            return PageLayoutExtensions.FromString(value);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    /// <summary>
    /// Sets the page layout.
    /// </summary>
    /// <param name="layout">The new page layout, or <see langword="null"/> to remove it.</param>
    public void SetPageLayout(PageLayout? layout)
    {
        _root.SetName(COSName.PAGE_LAYOUT, layout?.StringValue());
    }

    /// <summary>
    /// Gets the document's page mode, specifying how the document shall be displayed when opened.
    /// </summary>
    /// <returns>The <see cref="PageMode"/>, or <see langword="null"/> if not set.</returns>
    public PageMode? GetPageMode()
    {
        string? value = _root.GetNameAsString(COSName.PAGE_MODE);
        if (value is null)
        {
            return null;
        }

        try
        {
            return PageModeExtensions.FromString(value);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    /// <summary>
    /// Sets the page mode.
    /// </summary>
    /// <param name="mode">The new page mode, or <see langword="null"/> to remove it.</param>
    public void SetPageMode(PageMode? mode)
    {
        _root.SetName(COSName.PAGE_MODE, mode?.StringValue());
    }

    /// <summary>
    /// Gets the document's natural language, or <see langword="null"/> if not set.
    /// </summary>
    /// <returns>The natural language identifier of the document.</returns>
    public string? GetLanguage()
    {
        return _root.GetString(COSName.LANG);
    }

    /// <summary>
    /// Sets the natural language of the document.
    /// </summary>
    /// <param name="language">The natural language identifier, e.g. <c>en-US</c>.</param>
    public void SetLanguage(string? language)
    {
        _root.SetString(COSName.LANG, language);
    }

    public PDPageLabels? GetPageLabels()
    {
        COSDictionary? dict = _root.GetCOSDictionary(COSName.PAGE_LABELS);
        return dict is null ? null : new PDPageLabels(_document, dict);
    }

    public void SetPageLabels(PDPageLabels? labels)
    {
        _root.SetItem(COSName.PAGE_LABELS, labels);
    }

    public IList<PDThread> GetThreads()
    {
        COSArray? threads = _root.GetCOSArray(COSName.THREADS);
        if (threads is null)
        {
            return new COSArrayList<PDThread>(_root, COSName.THREADS);
        }

        List<PDThread> pdObjects = new(threads.Size());
        for (int i = 0; i < threads.Size(); i++)
        {
            if (threads.GetObject(i) is COSDictionary dictionary)
            {
                pdObjects.Add(new PDThread(dictionary));
            }
        }

        return new COSArrayList<PDThread>(pdObjects, threads);
    }

    public void SetThreads(IList<PDThread>? threads)
    {
        _root.SetItem(COSName.THREADS, COSArrayList<object>.ConverterToCOSArray(threads?.Cast<object>().ToList()));
    }

    /// <summary>
    /// Get the outline associated with this document or null if it does not exist.
    /// </summary>
    /// <returns>The document's outline.</returns>
    public PDDocumentOutline? GetDocumentOutline()
    {
        COSDictionary? outlineDict = _root.GetCOSDictionary(COSName.OUTLINES);
        return outlineDict != null ? new PDDocumentOutline(outlineDict) : null;
    }

    /// <summary>
    /// Sets the document outlines.
    /// </summary>
    /// <param name="outlines">The new document outlines.</param>
    public void SetDocumentOutline(PDDocumentOutline? outlines)
    {
        _root.SetItem(COSName.OUTLINES, outlines);
    }

    /// <summary>
    /// Sets the Document Open Action for this object.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    public void SetOpenAction(PDDestinationOrAction? action)
    {
        _root.SetItem(COSName.OPEN_ACTION, action);
    }

    /// <summary>
    /// Get the Document Open Action for this object.
    /// </summary>
    /// <returns>The action to perform when the document is opened.</returns>
    public PDDestinationOrAction? GetOpenAction()
    {
        COSBase? openAction = _root.GetDictionaryObject(COSName.OPEN_ACTION);
        if (openAction is COSDictionary actionDict)
        {
            return PDActionFactory.CreateAction(actionDict);
        }
        else if (openAction is COSArray)
        {
            return PDDestination.Create(openAction);
        }
        return null;
    }

    /// <summary>
    /// Returns the additional actions for this document.
    /// </summary>
    public PDDocumentCatalogAdditionalActions GetActions()
    {
        COSDictionary? addAction = _root.GetCOSDictionary(COSName.AA);
        if (addAction == null)
        {
            addAction = new COSDictionary();
            _root.SetItem(COSName.AA, addAction);
        }
        return new PDDocumentCatalogAdditionalActions(addAction);
    }

    /// <summary>
    /// Sets the additional actions for the document.
    /// </summary>
    public void SetActions(PDDocumentCatalogAdditionalActions? actions)
    {
        _root.SetItem(COSName.AA, actions);
    }

    /// <summary>
    /// Gets the viewer preferences dictionary, if present.
    /// </summary>
    public PDViewerPreferences? GetViewerPreferences()
    {
        COSDictionary? viewerPreferences = _root.GetCOSDictionary(COSName.VIEWER_PREFERENCES);
        return viewerPreferences is null ? null : new PDViewerPreferences(viewerPreferences);
    }

    /// <summary>
    /// Sets the viewer preferences dictionary.
    /// </summary>
    public void SetViewerPreferences(PDViewerPreferences? preferences)
    {
        _root.SetItem(COSName.VIEWER_PREFERENCES, preferences);
    }

    public IList<PDOutputIntent> GetOutputIntents()
    {
        COSArray? outputIntents = _root.GetCOSArray(COSName.OUTPUT_INTENTS);
        if (outputIntents is null)
        {
            return [];
        }

        List<PDOutputIntent> retval = new(outputIntents.Size());
        for (int i = 0; i < outputIntents.Size(); i++)
        {
            if (outputIntents.GetObject(i) is COSDictionary dictionary)
            {
                retval.Add(new PDOutputIntent(dictionary));
            }
        }

        return retval;
    }

    public void AddOutputIntent(PDOutputIntent outputIntent)
    {
        ArgumentNullException.ThrowIfNull(outputIntent);

        List<PDOutputIntent> outputIntents = [.. GetOutputIntents(), outputIntent];
        SetOutputIntents(outputIntents);
    }

    public void SetOutputIntents(IList<PDOutputIntent>? outputIntents)
    {
        _root.SetItem(COSName.OUTPUT_INTENTS, COSArrayList<object>.ConverterToCOSArray(outputIntents?.Cast<object>().ToList()));
    }

    /// <summary>
    /// Gets the names dictionary, if present.
    /// </summary>
    public PDDocumentNameDictionary? GetNames()
    {
        COSDictionary? names = _root.GetCOSDictionary(COSName.NAMES);
        return names is null ? null : new PDDocumentNameDictionary(this, names);
    }

    /// <summary>
    /// Sets the names dictionary.
    /// </summary>
    public void SetNames(PDDocumentNameDictionary? names)
    {
        _root.SetItem(COSName.NAMES, names);
    }

    /// <summary>
    /// Returns the document AcroForm dictionary if present.
    /// </summary>
    public PDAcroForm? GetAcroForm()
    {
        return GetAcroForm(new AcroFormDefaultFixup(_document));
    }

    /// <summary>
    /// Returns the document AcroForm dictionary if present.
    /// </summary>
    public PDAcroForm? GetAcroForm(PDDocumentFixup? acroFormFixup)
    {
        if (acroFormFixup != null && !ReferenceEquals(acroFormFixup, _acroFormFixupApplied))
        {
            acroFormFixup.Apply();
            _cachedAcroForm = null;
            _acroFormFixupApplied = acroFormFixup;
        }

        if (_cachedAcroForm == null)
        {
            COSDictionary? formDictionary = _root.GetCOSDictionary(COSName.ACRO_FORM);
            _cachedAcroForm = formDictionary == null ? null : new PDAcroForm(_document, formDictionary);
        }

        return _cachedAcroForm;
    }

    /// <summary>
    /// Sets the document AcroForm dictionary.
    /// </summary>
    public void SetAcroForm(PDAcroForm? acroForm)
    {
        _root.SetItem(COSName.ACRO_FORM, acroForm);
        _cachedAcroForm = null;
    }

    /// <summary>
    /// Returns the structure tree root, if present.
    /// </summary>
    public PDStructureTreeRoot? GetStructureTreeRoot()
    {
        COSDictionary? structureTreeRootDictionary = _root.GetCOSDictionary(COSName.GetPDFName("StructTreeRoot"));
        return structureTreeRootDictionary is null ? null : new PDStructureTreeRoot(structureTreeRootDictionary);
    }

    /// <summary>
    /// Sets the structure tree root.
    /// </summary>
    public void SetStructureTreeRoot(PDStructureTreeRoot? structureTreeRoot)
    {
        _root.SetItem(COSName.GetPDFName("StructTreeRoot"), structureTreeRoot);
    }

    /// <summary>
    /// Returns the MarkInfo dictionary, if present.
    /// </summary>
    public PDMarkInfo? GetMarkInfo()
    {
        COSDictionary? markInfoDictionary = _root.GetCOSDictionary(COSName.MARK_INFO);
        return markInfoDictionary is null ? null : new PDMarkInfo(markInfoDictionary);
    }

    /// <summary>
    /// Sets the MarkInfo dictionary.
    /// </summary>
    public void SetMarkInfo(PDMarkInfo? markInfo)
    {
        _root.SetItem(COSName.MARK_INFO, markInfo);
    }

    public PDOptionalContentProperties? GetOCProperties()
    {
        COSDictionary? properties = _root.GetCOSDictionary(COSName.GetPDFName("OCProperties"));
        return properties is null ? null : new PDOptionalContentProperties(properties);
    }

    public void SetOCProperties(PDOptionalContentProperties? ocProperties)
    {
        _root.SetItem(COSName.GetPDFName("OCProperties"), ocProperties);
    }

    /// <summary>
    /// Returns the document-level URI dictionary.
    /// </summary>
    public PDURIDictionary? GetURI()
    {
        COSDictionary? uri = _root.GetCOSDictionary(COSName.URI);
        return uri == null ? null : new PDURIDictionary(uri);
    }

    /// <summary>
    /// Sets the document-level URI dictionary.
    /// </summary>
    public void SetURI(PDURIDictionary? uri)
    {
        _root.SetItem(COSName.URI, uri);
    }

    /// <summary>
    /// Find the page destination from a named destination.
    /// </summary>
    /// <param name="namedDest">The named destination.</param>
    /// <returns>A PDPageDestination object or null if not found.</returns>
    public PDPageDestination? FindNamedDestinationPage(PDNamedDestination namedDest)
    {
        string? name = namedDest.GetNamedDestination();
        if (name is null)
        {
            return null;
        }

        PDDocumentNameDictionary? namesDict = GetNames();
        if (namesDict is not null)
        {
            PDDestinationNameTreeNode? destsTree = namesDict.GetDests();
            if (destsTree is not null)
            {
                PDPageDestination? treeDestination = destsTree.GetValue(name);
                if (treeDestination is not null)
                {
                    return treeDestination;
                }
            }
        }

        COSDictionary? destsDict = _root.GetCOSDictionary(COSName.DESTS);
        if (destsDict is not null)
        {
            PDDestination? destination = new PDDocumentNameDestinationDictionary(destsDict).GetDestination(name);
            return destination as PDPageDestination;
        }

        return null;
    }
}
