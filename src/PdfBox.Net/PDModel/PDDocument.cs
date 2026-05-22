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

public sealed class PDDocument : IDisposable
{
    private const string DefaultVersion = "1.4";
    private static readonly COSName RootName = COSName.GetPDFName("Root");
    private static readonly COSName InfoName = COSName.GetPDFName("Info");
    private static readonly COSName PagesName = COSName.GetPDFName("Pages");
    private static readonly COSName KidsName = COSName.GetPDFName("Kids");
    private static readonly COSName CountName = COSName.GetPDFName("Count");
    private static readonly COSName CatalogName = COSName.GetPDFName("Catalog");
    private static readonly COSName PagesTypeName = COSName.GetPDFName("Pages");

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

    public static PDDocument Load(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        using FileStream input = File.OpenRead(filePath);
        return Load(input);
    }

    public COSDictionary GetDocument()
    {
        EnsureNotDisposed();
        return _trailer;
    }

    public PDDocumentCatalog GetDocumentCatalog()
    {
        EnsureNotDisposed();
        if (_documentCatalog is null)
        {
            COSDictionary root = _trailer.GetCOSDictionary(RootName) ?? CreateCatalogDictionary();
            EnsurePagesDictionary(root);
            _trailer.SetItem(RootName, root);
            _documentCatalog = new PDDocumentCatalog(this, root);
        }

        return _documentCatalog;
    }

    public PDDocumentInformation GetDocumentInformation()
    {
        EnsureNotDisposed();
        if (_documentInformation is null)
        {
            COSDictionary info = _trailer.GetCOSDictionary(InfoName) ?? new COSDictionary();
            _trailer.SetItem(InfoName, info);
            _documentInformation = new PDDocumentInformation(info);
        }

        return _documentInformation;
    }

    public void Save(Stream output)
    {
        ArgumentNullException.ThrowIfNull(output);
        EnsureNotDisposed();
        _trailer.SetItem(RootName, GetDocumentCatalog().GetCOSObject());
        _trailer.SetItem(InfoName, GetDocumentInformation().GetCOSObject());
        output.Write(Encoding.ASCII.GetBytes($"%PDF-{_headerVersion.ToString("0.0", CultureInfo.InvariantCulture)}\n"));
        COSWriter writer = new(output);
        writer.Write(_trailer);
        output.Write(Encoding.ASCII.GetBytes("\n%%EOF\n"));
    }

    public void Save(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        EnsureNotDisposed();
        using FileStream output = File.Create(filePath);
        Save(output);
    }

    public void Dispose()
    {
        _disposed = true;
    }

    public PDPageTree GetPages()
    {
        EnsureNotDisposed();
        return GetDocumentCatalog().GetPages();
    }

    public int GetNumberOfPages()
    {
        EnsureNotDisposed();
        return GetPages().GetCount();
    }

    public PDPage GetPage(int pageIndex)
    {
        EnsureNotDisposed();
        return GetPages().Get(pageIndex);
    }

    public void AddPage(PDPage page)
    {
        EnsureNotDisposed();
        GetPages().Add(page);
    }

    public void RemovePage(int pageIndex)
    {
        EnsureNotDisposed();
        GetPages().Remove(pageIndex);
    }

    public float GetVersion()
    {
        EnsureNotDisposed();
        float catalogVersion = ParseVersion(GetDocumentCatalog().GetVersion(), _headerVersion);
        return Math.Max(_headerVersion, catalogVersion);
    }

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
        trailer.SetItem(RootName, CreateCatalogDictionary());
        trailer.SetItem(InfoName, new COSDictionary());
        return trailer;
    }

    private static COSDictionary CreateCatalogDictionary()
    {
        COSDictionary root = new();
        root.SetName(COSName.TYPE, CatalogName.GetName());
        root.SetName(COSName.GetPDFName("Version"), DefaultVersion);
        EnsurePagesDictionary(root);
        return root;
    }

    private static void EnsurePagesDictionary(COSDictionary root)
    {
        COSDictionary pages = root.GetCOSDictionary(PagesName) ?? new COSDictionary();
        pages.SetName(COSName.TYPE, PagesTypeName.GetName());
        if (!pages.ContainsKey(KidsName))
        {
            pages.SetItem(KidsName, new COSArray());
        }

        if (!pages.ContainsKey(CountName))
        {
            pages.SetInt(CountName, 0);
        }

        root.SetItem(PagesName, pages);
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
