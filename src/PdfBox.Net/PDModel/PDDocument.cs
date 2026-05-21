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

namespace PdfBox.Net.PDModel;

public sealed class PDDocument : IDisposable
{
    private static readonly COSName RootName = COSName.GetPDFName("Root");
    private static readonly COSName InfoName = COSName.GetPDFName("Info");
    private static readonly COSName PagesName = COSName.GetPDFName("Pages");
    private static readonly COSName KidsName = COSName.GetPDFName("Kids");
    private static readonly COSName CountName = COSName.GetPDFName("Count");
    private static readonly COSName CatalogName = COSName.GetPDFName("Catalog");
    private static readonly COSName PagesTypeName = COSName.GetPDFName("Pages");

    private readonly COSDictionary _trailer;
    private bool _disposed;
    private PDDocumentCatalog? _documentCatalog;
    private PDDocumentInformation? _documentInformation;

    public PDDocument()
        : this(CreateEmptyTrailer())
    {
    }

    private PDDocument(COSDictionary trailer)
    {
        _trailer = trailer;
    }

    public static PDDocument Load(Stream input)
    {
        ArgumentNullException.ThrowIfNull(input);
        COSBase parsed = COSParser.Parse(input);
        if (parsed is not COSDictionary trailer)
        {
            throw new IOException("Expected document trailer dictionary.");
        }

        return new PDDocument(trailer);
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
            _documentCatalog = new PDDocumentCatalog(root);
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
        COSWriter writer = new(output);
        writer.Write(_trailer);
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
        root.SetString(COSName.GetPDFName("Version"), "1.4");
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
}
