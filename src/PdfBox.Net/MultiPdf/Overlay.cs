/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/multipdf/Overlay.java
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

using System.Globalization;
using System.Text;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Util;

namespace PdfBox.Net.MultiPdf;

/// <summary>
/// Adds an overlay to an existing PDF document.
/// Based on code contributed by Balazs Jerk.
/// </summary>
public class Overlay : IDisposable
{
    /// <summary>Possible placement of overlaid pages: foreground or background.</summary>
    public enum Position
    {
        /// <summary>Overlay is rendered on top of page content.</summary>
        FOREGROUND,

        /// <summary>Overlay is rendered behind page content.</summary>
        BACKGROUND,
    }

    private sealed class LayoutPage
    {
        public readonly PDRectangle OverlayMediaBox;
        public readonly COSStream OverlayCOSStream;
        public readonly COSDictionary OverlayResources;
        public int OverlayRotation;

        public LayoutPage(PDRectangle mediaBox, COSStream contentStream, COSDictionary resources, int rotation)
        {
            OverlayMediaBox = mediaBox;
            OverlayCOSStream = contentStream;
            OverlayResources = resources;
            OverlayRotation = rotation;
        }
    }

    private LayoutPage? _defaultOverlayPage;
    private readonly Dictionary<int, LayoutPage> _rotatedDefaultOverlayPagesMap = new();
    private LayoutPage? _firstPageOverlayPage;
    private LayoutPage? _lastPageOverlayPage;
    private LayoutPage? _oddPageOverlayPage;
    private LayoutPage? _evenPageOverlayPage;

    private readonly HashSet<PDDocument> _openDocumentsSet = new(ReferenceEqualityComparer.Instance);
    private Dictionary<int, LayoutPage> _specificPageOverlayMap = new();

    private Position _position = Position.BACKGROUND;

    private string? _inputFileName;
    private PDDocument? _inputPDFDocument;
    private bool _ownsInputDocument;

    private string? _defaultOverlayFilename;
    private PDDocument? _defaultOverlayDocument;

    private string? _firstPageOverlayFilename;
    private PDDocument? _firstPageOverlayDocument;

    private string? _lastPageOverlayFilename;
    private PDDocument? _lastPageOverlayDocument;

    private string? _allPagesOverlayFilename;
    private PDDocument? _allPagesOverlayDocument;

    private string? _oddPageOverlayFilename;
    private PDDocument? _oddPageOverlayDocument;

    private string? _evenPageOverlayFilename;
    private PDDocument? _evenPageOverlayDocument;

    private int _numberOfOverlayPages;
    private bool _useAllOverlayPages;
    private bool _adjustRotation;

    /// <summary>
    /// Adds overlays to the configured input document using per-page file mappings.
    /// </summary>
    /// <param name="specificPageOverlayMap">
    /// Optional map of overlay file paths for specific 1-based page numbers.
    /// Pass an empty (but non-null) map when no specific overrides are needed.
    /// </param>
    /// <returns>The modified input document; the caller must save and dispose it.</returns>
    public PDDocument Process(IDictionary<int, string> specificPageOverlayMap)
    {
        ArgumentNullException.ThrowIfNull(specificPageOverlayMap);

        Dictionary<string, LayoutPage> layouts = new(StringComparer.Ordinal);

        LoadPDFs();

        foreach (KeyValuePair<int, string> entry in specificPageOverlayMap)
        {
            string path = entry.Value;
            if (!layouts.TryGetValue(path, out LayoutPage? layoutPage))
            {
                PDDocument doc = PDDocument.Load(path);
                layoutPage = CreateLayoutPageFromDocument(doc);
                layouts[path] = layoutPage;
                _openDocumentsSet.Add(doc);
            }

            _specificPageOverlayMap[entry.Key] = layoutPage;
        }

        ProcessPages(_inputPDFDocument!);
        return _inputPDFDocument!;
    }

    /// <summary>
    /// Adds overlays to the configured input document using per-page document mappings.
    /// </summary>
    /// <param name="specificPageOverlayDocumentMap">
    /// Optional map of overlay documents for specific 1-based page numbers.
    /// Pass an empty (but non-null) map when no specific overrides are needed.
    /// </param>
    /// <returns>The modified input document; the caller must save and dispose it.</returns>
    public PDDocument OverlayDocuments(IDictionary<int, PDDocument> specificPageOverlayDocumentMap)
    {
        ArgumentNullException.ThrowIfNull(specificPageOverlayDocumentMap);

        LoadPDFs();

        foreach (KeyValuePair<int, PDDocument> entry in specificPageOverlayDocumentMap)
        {
            if (entry.Value is not null)
            {
                _specificPageOverlayMap[entry.Key] = CreateLayoutPageFromDocument(entry.Value);
            }
        }

        ProcessPages(_inputPDFDocument!);
        return _inputPDFDocument!;
    }

    /// <summary>
    /// Gets or sets the overlay position (foreground or background).
    /// </summary>
    public Position OverlayPosition
    {
        get => _position;
        set => _position = value;
    }

    /// <summary>
    /// Gets or sets the input file path to overlay.
    /// </summary>
    public string? InputFile
    {
        get => _inputFileName;
        set => _inputFileName = value;
    }

    /// <summary>
    /// Gets or sets the input PDF document to overlay.
    /// </summary>
    public PDDocument? InputPDF
    {
        get => _inputPDFDocument;
        set
        {
            _inputPDFDocument = value;
            _ownsInputDocument = false;
        }
    }

    /// <summary>
    /// Gets or sets the default overlay file path (applied to all unmatched pages).
    /// </summary>
    public string? DefaultOverlayFile
    {
        get => _defaultOverlayFilename;
        set => _defaultOverlayFilename = value;
    }

    /// <summary>
    /// Gets or sets the default overlay document.
    /// </summary>
    public PDDocument? DefaultOverlayPDF
    {
        get => _defaultOverlayDocument;
        set => _defaultOverlayDocument = value;
    }

    /// <summary>
    /// Gets or sets the first-page overlay file path.
    /// </summary>
    public string? FirstPageOverlayFile
    {
        get => _firstPageOverlayFilename;
        set => _firstPageOverlayFilename = value;
    }

    /// <summary>
    /// Gets or sets the first-page overlay document.
    /// </summary>
    public PDDocument? FirstPageOverlayPDF
    {
        get => _firstPageOverlayDocument;
        set => _firstPageOverlayDocument = value;
    }

    /// <summary>
    /// Gets or sets the last-page overlay file path.
    /// </summary>
    public string? LastPageOverlayFile
    {
        get => _lastPageOverlayFilename;
        set => _lastPageOverlayFilename = value;
    }

    /// <summary>
    /// Gets or sets the last-page overlay document.
    /// </summary>
    public PDDocument? LastPageOverlayPDF
    {
        get => _lastPageOverlayDocument;
        set => _lastPageOverlayDocument = value;
    }

    /// <summary>
    /// Gets or sets the all-pages overlay file path (cycles through all overlay pages).
    /// </summary>
    public string? AllPagesOverlayFile
    {
        get => _allPagesOverlayFilename;
        set => _allPagesOverlayFilename = value;
    }

    /// <summary>
    /// Gets or sets the all-pages overlay document.
    /// </summary>
    public PDDocument? AllPagesOverlayPDF
    {
        get => _allPagesOverlayDocument;
        set => _allPagesOverlayDocument = value;
    }

    /// <summary>
    /// Gets or sets the odd-pages overlay file path.
    /// </summary>
    public string? OddPageOverlayFile
    {
        get => _oddPageOverlayFilename;
        set => _oddPageOverlayFilename = value;
    }

    /// <summary>
    /// Gets or sets the odd-pages overlay document.
    /// </summary>
    public PDDocument? OddPageOverlayPDF
    {
        get => _oddPageOverlayDocument;
        set => _oddPageOverlayDocument = value;
    }

    /// <summary>
    /// Gets or sets the even-pages overlay file path.
    /// </summary>
    public string? EvenPageOverlayFile
    {
        get => _evenPageOverlayFilename;
        set => _evenPageOverlayFilename = value;
    }

    /// <summary>
    /// Gets or sets the even-pages overlay document.
    /// </summary>
    public PDDocument? EvenPageOverlayPDF
    {
        get => _evenPageOverlayDocument;
        set => _evenPageOverlayDocument = value;
    }

    /// <summary>
    /// Gets or sets whether the overlay is rotated to match the rotation of each page in the
    /// source document when applying the default overlay.
    /// </summary>
    public bool AdjustRotation
    {
        get => _adjustRotation;
        set => _adjustRotation = value;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _defaultOverlayDocument?.Dispose();
        _firstPageOverlayDocument?.Dispose();
        _lastPageOverlayDocument?.Dispose();
        _allPagesOverlayDocument?.Dispose();
        _oddPageOverlayDocument?.Dispose();
        _evenPageOverlayDocument?.Dispose();

        foreach (PDDocument doc in _openDocumentsSet)
        {
            doc.Dispose();
        }

        _openDocumentsSet.Clear();
        _specificPageOverlayMap.Clear();
        _rotatedDefaultOverlayPagesMap.Clear();

        if (_ownsInputDocument)
        {
            _inputPDFDocument?.Dispose();
        }
    }

    private void LoadPDFs()
    {
        if (_inputFileName is not null)
        {
            _inputPDFDocument = PDDocument.Load(_inputFileName);
            _ownsInputDocument = true;
        }

        if (_inputPDFDocument is null)
        {
            throw new InvalidOperationException("No input document configured.");
        }

        if (_defaultOverlayFilename is not null)
        {
            _defaultOverlayDocument = PDDocument.Load(_defaultOverlayFilename);
        }

        if (_defaultOverlayDocument is not null)
        {
            _defaultOverlayPage = CreateLayoutPageFromDocument(_defaultOverlayDocument);
        }

        if (_firstPageOverlayFilename is not null)
        {
            _firstPageOverlayDocument = PDDocument.Load(_firstPageOverlayFilename);
        }

        if (_firstPageOverlayDocument is not null)
        {
            _firstPageOverlayPage = CreateLayoutPageFromDocument(_firstPageOverlayDocument);
        }

        if (_lastPageOverlayFilename is not null)
        {
            _lastPageOverlayDocument = PDDocument.Load(_lastPageOverlayFilename);
        }

        if (_lastPageOverlayDocument is not null)
        {
            _lastPageOverlayPage = CreateLayoutPageFromDocument(_lastPageOverlayDocument);
        }

        if (_oddPageOverlayFilename is not null)
        {
            _oddPageOverlayDocument = PDDocument.Load(_oddPageOverlayFilename);
        }

        if (_oddPageOverlayDocument is not null)
        {
            _oddPageOverlayPage = CreateLayoutPageFromDocument(_oddPageOverlayDocument);
        }

        if (_evenPageOverlayFilename is not null)
        {
            _evenPageOverlayDocument = PDDocument.Load(_evenPageOverlayFilename);
        }

        if (_evenPageOverlayDocument is not null)
        {
            _evenPageOverlayPage = CreateLayoutPageFromDocument(_evenPageOverlayDocument);
        }

        if (_allPagesOverlayFilename is not null)
        {
            _allPagesOverlayDocument = PDDocument.Load(_allPagesOverlayFilename);
        }

        if (_allPagesOverlayDocument is not null)
        {
            _specificPageOverlayMap = CreatePageOverlayLayoutPageMap(_allPagesOverlayDocument);
            _useAllOverlayPages = true;
            _numberOfOverlayPages = _specificPageOverlayMap.Count;
        }
    }

    private LayoutPage CreateLayoutPageFromDocument(PDDocument doc)
    {
        return CreateLayoutPage(doc.GetPage(0));
    }

    private LayoutPage CreateLayoutPage(PDPage page)
    {
        COSBase? contents = page.GetContents();
        PDResources resources = page.GetResources() ?? new PDResources();
        return new LayoutPage(
            page.GetMediaBox(),
            CreateCombinedContentStream(contents),
            resources.GetCOSObject(),
            page.GetRotation());
    }

    private Dictionary<int, LayoutPage> CreatePageOverlayLayoutPageMap(PDDocument doc)
    {
        Dictionary<int, LayoutPage> layoutPages = new();
        int i = 0;
        foreach (PDPage page in doc.GetPages())
        {
            layoutPages[i] = CreateLayoutPage(page);
            i++;
        }

        return layoutPages;
    }

    private COSStream CreateCombinedContentStream(COSBase? contents)
    {
        List<COSStream> contentStreams = CreateContentStreamList(contents);
        COSStream concatStream = new();
        using Stream output = concatStream.CreateOutputStream(COSName.FLATE_DECODE);
        foreach (COSStream stream in contentStreams)
        {
            using Stream input = stream.CreateRawInputStream();
            input.CopyTo(output);
            output.Flush();
        }

        return concatStream;
    }

    private static List<COSStream> CreateContentStreamList(COSBase? contents)
    {
        if (contents is null)
        {
            return new List<COSStream>();
        }

        if (contents is COSStream directStream)
        {
            return new List<COSStream> { directStream };
        }

        List<COSStream> streams = new();
        if (contents is COSArray array)
        {
            for (int i = 0; i < array.Size(); i++)
            {
                streams.AddRange(CreateContentStreamList(array.GetObject(i)));
            }
        }
        else if (contents is COSObject cosObject)
        {
            streams.AddRange(CreateContentStreamList(cosObject.GetObject()));
        }
        else
        {
            throw new IOException($"Unknown content stream type: {contents.GetType().Name}");
        }

        return streams;
    }

    private void ProcessPages(PDDocument document)
    {
        PDFCloneUtility cloner = new(document);
        int pageCounter = 0;
        int numberOfPages = document.GetNumberOfPages();

        foreach (PDPage page in document.GetPages())
        {
            pageCounter++;
            LayoutPage? layoutPage = GetLayoutPage(pageCounter, numberOfPages);
            if (layoutPage is null)
            {
                continue;
            }

            COSDictionary pageDictionary = (COSDictionary)page.GetCOSObject();
            COSBase? originalContent = pageDictionary.GetDictionaryObject(COSName.CONTENTS);
            COSArray newContentArray = new();

            switch (_position)
            {
                case Position.FOREGROUND:
                    newContentArray.Add(CreateStream("q\n"));
                    AddOriginalContent(originalContent, newContentArray);
                    newContentArray.Add(CreateStream("Q\n"));
                    OverlayPage(page, layoutPage, newContentArray, cloner);
                    break;

                case Position.BACKGROUND:
                    OverlayPage(page, layoutPage, newContentArray, cloner);
                    AddOriginalContent(originalContent, newContentArray);
                    break;

                default:
                    throw new IOException($"Unknown overlay position: {_position}");
            }

            pageDictionary.SetItem(COSName.CONTENTS, newContentArray);
        }
    }

    private static void AddOriginalContent(COSBase? contents, COSArray contentArray)
    {
        if (contents is null)
        {
            return;
        }

        if (contents is COSStream stream)
        {
            contentArray.Add(stream);
        }
        else if (contents is COSArray array)
        {
            for (int i = 0; i < array.Size(); i++)
            {
                COSBase? item = array.Get(i);
                if (item is not null)
                {
                    contentArray.Add(item);
                }
            }
        }
        else
        {
            throw new IOException($"Unknown content type: {contents.GetType().Name}");
        }
    }

    private void OverlayPage(PDPage page, LayoutPage layoutPage, COSArray array, PDFCloneUtility cloner)
    {
        PDResources resources = page.GetResources() ?? new PDResources();
        page.SetResources(resources);

        PDFormXObject overlayForm = CreateOverlayFormXObject(layoutPage, cloner);
        COSName formXObjectId = resources.Add(overlayForm, "OL");
        array.Add(CreateOverlayStream(page, layoutPage, formXObjectId));
    }

    private LayoutPage? GetLayoutPage(int pageNumber, int numberOfPages)
    {
        if (!_useAllOverlayPages && _specificPageOverlayMap.TryGetValue(pageNumber, out LayoutPage? specific))
        {
            return specific;
        }

        if (pageNumber == 1 && _firstPageOverlayPage is not null)
        {
            return _firstPageOverlayPage;
        }

        if (pageNumber == numberOfPages && _lastPageOverlayPage is not null)
        {
            return _lastPageOverlayPage;
        }

        if (pageNumber % 2 == 1 && _oddPageOverlayPage is not null)
        {
            return _oddPageOverlayPage;
        }

        if (pageNumber % 2 == 0 && _evenPageOverlayPage is not null)
        {
            return _evenPageOverlayPage;
        }

        if (_defaultOverlayPage is not null)
        {
            if (_adjustRotation)
            {
                PDPage page = _inputPDFDocument!.GetPage(pageNumber - 1);
                int rotation = page.GetRotation();
                if (rotation != 0)
                {
                    return CreateAdjustedLayoutPage(rotation);
                }
            }

            return _defaultOverlayPage;
        }

        if (_useAllOverlayPages)
        {
            int usePageNum = (pageNumber - 1) % _numberOfOverlayPages;
            return _specificPageOverlayMap[usePageNum];
        }

        return null;
    }

    private LayoutPage CreateAdjustedLayoutPage(int rotation)
    {
        if (_rotatedDefaultOverlayPagesMap.TryGetValue(rotation, out LayoutPage? rotated))
        {
            return rotated;
        }

        rotated = CreateLayoutPage(_defaultOverlayDocument!.GetPage(0));
        int newRotation = (rotated.OverlayRotation - rotation + 360) % 360;
        rotated.OverlayRotation = newRotation;
        _rotatedDefaultOverlayPagesMap[rotation] = rotated;
        return rotated;
    }

    private PDFormXObject CreateOverlayFormXObject(LayoutPage layoutPage, PDFCloneUtility cloner)
    {
        PDFormXObject form = new(layoutPage.OverlayCOSStream);
        form.SetResources(new PDResources(cloner.CloneForNewDocument(layoutPage.OverlayResources)!));
        form.SetFormType(1);
        form.SetBBox(layoutPage.OverlayMediaBox.CreateRetranslatedRectangle());

        AffineTransform at = new();
        switch (layoutPage.OverlayRotation)
        {
            case 90:
                at.Translate(0, layoutPage.OverlayMediaBox.GetWidth());
                at.QuadrantRotate(3); // 270° CCW
                break;
            case 180:
                at.Translate(layoutPage.OverlayMediaBox.GetWidth(), layoutPage.OverlayMediaBox.GetHeight());
                at.QuadrantRotate(2); // 180°
                break;
            case 270:
                at.Translate(layoutPage.OverlayMediaBox.GetHeight(), 0);
                at.QuadrantRotate(1); // 90° CCW
                break;
        }

        form.SetMatrix(at);
        return form;
    }

    private COSStream CreateOverlayStream(PDPage page, LayoutPage layoutPage, COSName xObjectId)
    {
        StringBuilder overlayStream = new();
        overlayStream.Append("q\nq\n");

        PDRectangle overlayMediaBox = new PDRectangle(layoutPage.OverlayMediaBox.GetCOSArray());
        if (layoutPage.OverlayRotation == 90 || layoutPage.OverlayRotation == 270)
        {
            overlayMediaBox.SetLowerLeftX(layoutPage.OverlayMediaBox.GetLowerLeftY());
            overlayMediaBox.SetLowerLeftY(layoutPage.OverlayMediaBox.GetLowerLeftX());
            overlayMediaBox.SetUpperRightX(layoutPage.OverlayMediaBox.GetUpperRightY());
            overlayMediaBox.SetUpperRightY(layoutPage.OverlayMediaBox.GetUpperRightX());
        }

        AffineTransform at = CalculateAffineTransform(page, overlayMediaBox);
        double[] flatMatrix = new double[6];
        at.GetMatrix(flatMatrix);
        foreach (double v in flatMatrix)
        {
            overlayStream.Append(Float2String((float)v));
            overlayStream.Append(' ');
        }

        overlayStream.Append(" cm\n /");
        overlayStream.Append(xObjectId.GetName());
        overlayStream.Append(" Do Q\nQ\n");

        return CreateStream(overlayStream.ToString());
    }

    /// <summary>
    /// Calculates the affine transform for positioning the overlay on a page.
    /// The default implementation centers the overlay on the page.
    /// Override to position differently (e.g., corner, rotate, zoom).
    /// </summary>
    protected virtual AffineTransform CalculateAffineTransform(PDPage page, PDRectangle overlayMediaBox)
    {
        PDRectangle pageMediaBox = page.GetMediaBox();
        float hShift = pageMediaBox.GetLowerLeftX() + (pageMediaBox.GetWidth() - overlayMediaBox.GetWidth()) / 2.0f;
        float vShift = pageMediaBox.GetLowerLeftY() + (pageMediaBox.GetHeight() - overlayMediaBox.GetHeight()) / 2.0f;

        AffineTransform at = new();
        at.Translate(hShift, vShift);
        return at;
    }

    private static string Float2String(float value)
    {
        string s = value.ToString("G", CultureInfo.InvariantCulture);
        if (s.Contains('.') && !s.EndsWith(".0", StringComparison.Ordinal))
        {
            s = s.TrimEnd('0');
        }

        return s;
    }

    private COSStream CreateStream(string content)
    {
        COSStream stream = new();
        COSName? filter = content.Length > 20 ? COSName.FLATE_DECODE : null;
        if (filter is not null)
        {
            using Stream output = stream.CreateOutputStream(filter);
            output.Write(Encoding.Latin1.GetBytes(content));
        }
        else
        {
            using Stream output = stream.CreateOutputStream();
            output.Write(Encoding.Latin1.GetBytes(content));
        }

        return stream;
    }
}
