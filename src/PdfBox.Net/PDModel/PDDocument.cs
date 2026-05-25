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
using PdfBox.Net.PDModel.Encryption;
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

    private readonly COSDocument _document;
    private readonly COSDictionary _trailer;
    private readonly float _headerVersion;
    private bool _disposed;
    private PDDocumentCatalog? _documentCatalog;
    private PDDocumentInformation? _documentInformation;

    public PDDocument()
        : this(CreateNewDocument())
    {
    }

    private PDDocument(COSDocument document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _trailer = _document.GetTrailer() ?? throw new IOException("Document trailer dictionary is missing.");
        _headerVersion = _document.GetVersion();
    }

    /// <summary>
    /// Parses a PDF stream and returns a document.
    /// </summary>
    /// <param name="input">The input stream.</param>
    /// <returns>The loaded document.</returns>
    public static PDDocument Load(Stream input)
    {
        return Load(input, password: null);
    }

    /// <summary>
    /// Parses a password-protected PDF stream and returns a document.
    /// </summary>
    /// <param name="input">The input stream.</param>
    /// <param name="password">The user or owner password used to decrypt the document.</param>
    /// <returns>The loaded document.</returns>
    public static PDDocument Load(Stream input, string? password)
    {
        ArgumentNullException.ThrowIfNull(input);
        PDFParser parser = new(input);
        ParsedPDFDocument parsed = parser.Parse();
        PDDocument doc = new(parsed.Document);
        doc.DecryptIfNeeded(password);
        return doc;
    }

    /// <summary>
    /// Parses a PDF file and returns a document.
    /// </summary>
    /// <param name="filePath">The file path to load.</param>
    /// <returns>The loaded document.</returns>
    public static PDDocument Load(string filePath)
    {
        return Load(filePath, password: null);
    }

    /// <summary>
    /// Parses a password-protected PDF file and returns a document.
    /// </summary>
    /// <param name="filePath">The file path to load.</param>
    /// <param name="password">The user or owner password used to decrypt the document.</param>
    /// <returns>The loaded document.</returns>
    public static PDDocument Load(string filePath, string? password)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        using FileStream input = File.OpenRead(filePath);
        return Load(input, password);
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

        byte[] headerBytes = Encoding.ASCII.GetBytes($"%PDF-{_headerVersion.ToString("0.0", CultureInfo.InvariantCulture)}\n");
        output.Write(headerBytes);

        // Collect all indirect (non-stream) objects referenced from the trailer.
        List<(COSObjectKey Key, COSBase Inner)> indirectObjects = CollectIndirectObjects(_trailer);

        // Write each indirect object, recording its byte offset for the xref table.
        Dictionary<COSObjectKey, long> objectOffsets = new(indirectObjects.Count);
        foreach ((COSObjectKey key, COSBase inner) in indirectObjects)
        {
            objectOffsets[key] = output.Position;
            byte[] objHeader = Encoding.ASCII.GetBytes(
                $"{key.GetNumber()} {key.GetGeneration()} obj\n");
            output.Write(objHeader);
            byte[] body = COSWriter.Serialize(inner);
            output.Write(body);
            output.Write(Encoding.ASCII.GetBytes("\nendobj\n"));
        }

        // Determine the highest object number so we can size the xref table.
        long maxObjectNumber = indirectObjects.Count > 0
            ? indirectObjects.Max(x => x.Key.GetNumber())
            : 0;

        // Write the cross-reference table.
        long xrefOffset = output.Position;
        output.Write(Encoding.ASCII.GetBytes($"xref\n0 {maxObjectNumber + 1}\n"));
        output.Write(Encoding.ASCII.GetBytes("0000000000 65535 f \n"));
        for (long i = 1; i <= maxObjectNumber; i++)
        {
            COSObjectKey lookupKey = new(i, 0);
            if (objectOffsets.TryGetValue(lookupKey, out long objOffset))
            {
                output.Write(Encoding.ASCII.GetBytes(
                    $"{objOffset:D10} {lookupKey.GetGeneration():D5} n \n"));
            }
            else
            {
                output.Write(Encoding.ASCII.GetBytes("0000000000 65535 f \n"));
            }
        }

        // Update /Size in the trailer before serializing it.
        _trailer.SetInt(COSName.GetPDFName("Size"), checked((int)(maxObjectNumber + 1)));

        output.Write(Encoding.ASCII.GetBytes("trailer\n"));
        output.Write(COSWriter.Serialize(_trailer));
        output.Write(Encoding.ASCII.GetBytes(
            $"\nstartxref\n{xrefOffset.ToString(CultureInfo.InvariantCulture)}\n%%EOF\n"));
    }

    /// <summary>
    /// Traverses the object graph starting from <paramref name="trailer"/> and returns all
    /// indirect (non-stream) objects reachable from it, in object-number order.
    /// COSStream objects are excluded because their binary data cannot be portably
    /// serialized by this basic writer.
    /// </summary>
    private static List<(COSObjectKey Key, COSBase Inner)> CollectIndirectObjects(COSDictionary trailer)
    {
        Dictionary<COSObjectKey, COSBase> collected = [];
        Queue<COSBase> pending = new();
        pending.Enqueue(trailer);

        while (pending.Count > 0)
        {
            COSBase current = pending.Dequeue();
            switch (current)
            {
                case COSObject cosObj:
                    COSObjectKey? key = cosObj.GetKey();
                    COSBase? inner = cosObj.GetObject();
                    if (key is not null && inner is not null && inner is not COSStream
                        && !collected.ContainsKey(key))
                    {
                        collected[key] = inner;
                        pending.Enqueue(inner);
                    }

                    break;

                case COSDictionary dict:
                    foreach (COSName name in dict.KeySet())
                    {
                        COSBase? val = dict.GetItem(name);
                        if (val is not null)
                        {
                            pending.Enqueue(val);
                        }
                    }

                    break;

                case COSArray array:
                    for (int i = 0; i < array.Size(); i++)
                    {
                        COSBase? element = array.Get(i);
                        if (element is not null)
                        {
                            pending.Enqueue(element);
                        }
                    }

                    break;
            }
        }

        return collected
            .OrderBy(kv => kv.Key.GetNumber())
            .Select(kv => (kv.Key, kv.Value))
            .ToList();
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
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _document.Dispose();
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

    /// <summary>
    /// If the document is encrypted, initialises and runs the security handler to decrypt all
    /// string and stream objects reachable from the trailer.
    /// </summary>
    /// <param name="password">The user or owner password, or null / empty for no password.</param>
    private void DecryptIfNeeded(string? password)
    {
        COSDictionary? encryptDict = _trailer.GetCOSDictionary(COSName.GetPDFName("Encrypt"));
        if (encryptDict is null)
        {
            return;
        }

        COSArray? idArray = _trailer.GetCOSArray(COSName.GetPDFName("ID"));

        PDEncryption encryption = new(encryptDict);
        StandardSecurityHandler handler = new();
        StandardDecryptionMaterial material = new(password ?? string.Empty);
        handler.PrepareForDecryption(encryption, idArray, material);

        // Walk the object graph starting from the trailer, but only decrypt strings and
        // streams that belong to an indirect object (i.e., are inside a COSObject wrapper).
        // Strings that appear directly in the cross-reference trailer (e.g., /ID) must not
        // be decrypted per PDF spec section 7.6.5.
        HashSet<COSBase> visited = new(ReferenceEqualityComparer.Instance);
        DecryptObjectGraph(_trailer, handler, 0, 0, inIndirectObject: false, visited, encryptDict);
    }

    private static void DecryptObjectGraph(
        COSBase obj,
        SecurityHandler<ProtectionPolicy> handler,
        long contextObjNum,
        long contextGenNum,
        bool inIndirectObject,
        HashSet<COSBase> visited,
        COSDictionary encryptDict)
    {
        if (obj is null || !visited.Add(obj))
        {
            return;
        }

        // Determine the object context: if this COSBase is itself an indirect object,
        // use its own key; otherwise inherit the caller's context.
        COSObjectKey? key = obj.GetKey();
        long objNum = key is not null ? key.GetNumber() : contextObjNum;
        long genNum = key is not null ? (long)key.GetGeneration() : contextGenNum;

        switch (obj)
        {
            case COSObject cosObj:
                COSBase? inner = cosObj.GetObject();
                if (inner is not null)
                {
                    // Entering a COSObject marks the beginning of an indirect object scope.
                    DecryptObjectGraph(inner, handler, objNum, genNum, inIndirectObject: true, visited, encryptDict);
                }

                break;

            case COSString cosString:
                // Only decrypt strings that are inside an indirect object (never trailer-level
                // strings such as /ID, and never the encryption dictionary itself).
                if (inIndirectObject)
                {
                    handler.DecryptString(objNum, genNum, cosString);
                }

                break;

            case COSDictionary cosDictionary:
                // Skip the encryption dictionary entirely — it must never be decrypted.
                if (ReferenceEquals(cosDictionary, encryptDict))
                {
                    break;
                }

                foreach (COSName dictKey in cosDictionary.KeySet())
                {
                    COSBase? value = cosDictionary.GetItem(dictKey);
                    if (value is not null)
                    {
                        DecryptObjectGraph(value, handler, objNum, genNum, inIndirectObject, visited, encryptDict);
                    }
                }

                break;

            case COSArray cosArray:
                for (int i = 0; i < cosArray.Size(); i++)
                {
                    COSBase? element = cosArray.Get(i);
                    if (element is not null)
                    {
                        DecryptObjectGraph(element, handler, objNum, genNum, inIndirectObject, visited, encryptDict);
                    }
                }

                break;
        }
    }

    private static COSDocument CreateNewDocument()
    {
        COSDocument document = new();
        document.SetVersion(ParseVersion(DefaultVersion));
        document.SetTrailer(CreateEmptyTrailer());
        document.GetDocumentState().SetParsing(false);
        return document;
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

    private static float ParseVersion(string value)
    {
        return ParseVersion(value, 1.4f);
    }

    private static float ParseVersion(string? value, float fallback)
    {
        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed) ? parsed : fallback;
    }
}
