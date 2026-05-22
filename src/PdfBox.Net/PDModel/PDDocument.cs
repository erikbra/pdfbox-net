/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/PDDocument.java
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
using PdfBox.Net.PdfParser;
using PdfBox.Net.PdfWriter;
using System.Globalization;
using System.Text;

namespace PdfBox.Net.PDModel;

/// <summary>
/// This is the in-memory representation of the PDF document.
/// The only way to get a PDF document object is to load it from a file or
/// to create it with the empty constructor.
/// </summary>
public sealed class PDDocument : IDisposable
{
    private const string DefaultVersion = "1.4";

    private readonly COSDictionary _trailer;
    private readonly float _headerVersion;
    private bool _disposed;
    private PDDocumentCatalog? _documentCatalog;
    private PDDocumentInformation? _documentInformation;

    public PDDocument()
        : this(CreateEmptyTrailer(), ParseVersion(DefaultVersion))
    {
    }

    private PDDocument(COSDictionary trailer, float headerVersion)
    {
        _trailer = trailer;
        _headerVersion = headerVersion;
    }

    /// <summary>
    /// Parses a PDF stream and returns a document.
    /// </summary>
    /// <param name="input">The input stream.</param>
    /// <returns>The loaded document.</returns>
    public static PDDocument Load(Stream input)
    {
        ArgumentNullException.ThrowIfNull(input);
        byte[] bytes = ReadAllBytes(input);
        string content = Encoding.Latin1.GetString(bytes);
        string payload = ExtractDictionaryPayload(content);
        float headerVersion = ExtractHeaderVersion(content);

        COSBase parsed = COSParser.Parse(payload);
        if (parsed is not COSDictionary trailer)
        {
            throw new IOException("Expected document trailer dictionary.");
        }

        return new PDDocument(trailer, headerVersion);
    }

    /// <summary>
    /// Parses a PDF file and returns a document.
    /// </summary>
    /// <param name="filePath">The file path to load.</param>
    /// <returns>The loaded document.</returns>
    public static PDDocument Load(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        using FileStream input = File.OpenRead(filePath);
        return Load(input);
    }

    /// <summary>
    /// Returns the underlying trailer dictionary of this document.
    /// </summary>
    /// <returns>The trailer dictionary.</returns>
    public COSDictionary GetDocument()
    {
        EnsureNotDisposed();
        return _trailer;
    }

    /// <summary>
    /// Returns the document catalog.
    /// </summary>
    /// <returns>The document catalog.</returns>
    public PDDocumentCatalog GetDocumentCatalog()
    {
        EnsureNotDisposed();
        if (_documentCatalog is null)
        {
            COSDictionary? root = _trailer.GetCOSDictionary(COSName.ROOT);
            _documentCatalog = root is null ? new PDDocumentCatalog(this) : new PDDocumentCatalog(this, root);
            EnsurePagesDictionary((COSDictionary)_documentCatalog.GetCOSObject());
        }

        return _documentCatalog;
    }

    /// <summary>
    /// Returns the document information dictionary.
    /// </summary>
    /// <returns>The document information wrapper.</returns>
    public PDDocumentInformation GetDocumentInformation()
    {
        EnsureNotDisposed();
        if (_documentInformation is null)
        {
            COSDictionary info = _trailer.GetCOSDictionary(COSName.GetPDFName("Info")) ?? new COSDictionary();
            _trailer.SetItem(COSName.GetPDFName("Info"), info);
            _documentInformation = new PDDocumentInformation(info);
        }

        return _documentInformation;
    }

    /// <summary>
    /// Save the document to an output stream.
    /// </summary>
    /// <param name="output">The output stream to write to.</param>
    public void Save(Stream output)
    {
        ArgumentNullException.ThrowIfNull(output);
        EnsureNotDisposed();
        _trailer.SetItem(COSName.ROOT, GetDocumentCatalog().GetCOSObject());
        _trailer.SetItem(COSName.GetPDFName("Info"), GetDocumentInformation().GetCOSObject());
        output.Write(Encoding.ASCII.GetBytes($"%PDF-{_headerVersion.ToString("0.0", CultureInfo.InvariantCulture)}\n"));
        COSWriter writer = new(output);
        writer.Write(_trailer);
        output.Write(Encoding.ASCII.GetBytes("\n%%EOF\n"));
    }

    /// <summary>
    /// Save the document to a file.
    /// </summary>
    /// <param name="filePath">The output file path.</param>
    public void Save(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        EnsureNotDisposed();
        using FileStream output = File.Create(filePath);
        Save(output);
    }

    /// <summary>
    /// Closes this document.
    /// </summary>
    public void Dispose()
    {
        _disposed = true;
    }

    /// <summary>
    /// Returns the document page tree.
    /// </summary>
    /// <returns>The page tree.</returns>
    public PDPageTree GetPages()
    {
        EnsureNotDisposed();
        return GetDocumentCatalog().GetPages();
    }

    /// <summary>
    /// Returns the number of pages in the document.
    /// </summary>
    /// <returns>The number of pages.</returns>
    public int GetNumberOfPages()
    {
        EnsureNotDisposed();
        return GetPages().GetCount();
    }

    /// <summary>
    /// Returns the page at a zero-based index.
    /// </summary>
    /// <param name="pageIndex">The page index.</param>
    /// <returns>The page at that index.</returns>
    public PDPage GetPage(int pageIndex)
    {
        EnsureNotDisposed();
        return GetPages().Get(pageIndex);
    }

    /// <summary>
    /// Adds a page to the end of the document.
    /// </summary>
    /// <param name="page">The page to add.</param>
    public void AddPage(PDPage page)
    {
        EnsureNotDisposed();
        GetPages().Add(page);
    }

    /// <summary>
    /// Removes the page at a zero-based index.
    /// </summary>
    /// <param name="pageIndex">The page index to remove.</param>
    public void RemovePage(int pageIndex)
    {
        EnsureNotDisposed();
        GetPages().Remove(pageIndex);
    }

    /// <summary>
    /// Returns the effective PDF version, using header/catalog precedence behavior.
    /// </summary>
    /// <returns>The effective PDF version.</returns>
    public float GetVersion()
    {
        EnsureNotDisposed();
        float catalogVersion = ParseVersion(GetDocumentCatalog().GetVersion(), _headerVersion);
        return Math.Max(_headerVersion, catalogVersion);
    }

    /// <summary>
    /// Sets the document version in the catalog.
    /// </summary>
    /// <param name="version">The version to set.</param>
    public void SetVersion(float version)
    {
        EnsureNotDisposed();
        if (version <= _headerVersion)
        {
            GetDocumentCatalog().SetVersion(_headerVersion.ToString("0.0", CultureInfo.InvariantCulture));
            return;
        }

        GetDocumentCatalog().SetVersion(version.ToString("0.0", CultureInfo.InvariantCulture));
    }

    private void EnsureNotDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private static COSDictionary CreateEmptyTrailer()
    {
        COSDictionary trailer = new();
        trailer.SetItem(COSName.ROOT, CreateCatalogDictionary());
        trailer.SetItem(COSName.GetPDFName("Info"), new COSDictionary());
        return trailer;
    }

    private static COSDictionary CreateCatalogDictionary()
    {
        COSDictionary root = new();
        root.SetItem(COSName.TYPE, COSName.CATALOG);
        root.SetName(COSName.VERSION, DefaultVersion);
        EnsurePagesDictionary(root);
        return root;
    }

    private static void EnsurePagesDictionary(COSDictionary root)
    {
        COSDictionary pages = root.GetCOSDictionary(COSName.PAGES) ?? new COSDictionary();
        pages.SetItem(COSName.TYPE, COSName.PAGES);
        if (!pages.ContainsKey(COSName.KIDS))
        {
            pages.SetItem(COSName.KIDS, new COSArray());
        }

        if (!pages.ContainsKey(COSName.COUNT))
        {
            pages.SetInt(COSName.COUNT, 0);
        }

        root.SetItem(COSName.PAGES, pages);
    }

    private static byte[] ReadAllBytes(Stream input)
    {
        using MemoryStream buffer = new();
        input.CopyTo(buffer);
        return buffer.ToArray();
    }

    private static string ExtractDictionaryPayload(string content)
    {
        if (!content.StartsWith("%PDF-", StringComparison.Ordinal))
        {
            return content;
        }

        int dictionaryStart = content.IndexOf("<<", StringComparison.Ordinal);
        if (dictionaryStart < 0)
        {
            throw new IOException("PDF content does not contain a COS trailer dictionary.");
        }

        int eofMarker = content.LastIndexOf("%%EOF", StringComparison.Ordinal);
        if (eofMarker < 0 || eofMarker <= dictionaryStart)
        {
            eofMarker = content.Length;
        }

        return content.Substring(dictionaryStart, eofMarker - dictionaryStart).Trim();
    }

    private static float ExtractHeaderVersion(string content)
    {
        if (!content.StartsWith("%PDF-", StringComparison.Ordinal))
        {
            return ParseVersion(DefaultVersion);
        }

        int headerEnd = content.IndexOf('\n');
        string headerLine = headerEnd >= 0 ? content[..headerEnd] : content;
        string value = headerLine.Substring("%PDF-".Length).Trim();
        return ParseVersion(value, ParseVersion(DefaultVersion));
    }

    private static float ParseVersion(string value)
    {
        return ParseVersion(value, 1.4f);
    }

    private static float ParseVersion(string? value, float fallback)
    {
        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed) ? parsed : fallback;
    }
}
