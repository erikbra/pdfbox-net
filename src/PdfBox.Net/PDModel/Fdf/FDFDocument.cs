/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fdf/FDFDocument.java
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
using System.Xml;
using PdfBox.Net.COS;
using PdfBox.Net.IO;
using PdfBox.Net.PdfParser;
using PdfBox.Net.PdfWriter;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Fdf;

public sealed partial class FDFDocument : IDisposable
{
    private const string DefaultVersion = "1.2";

    private static readonly byte[] PdfHeaderBytes = Encoding.ASCII.GetBytes("%PDF-");
    private static readonly byte[] FdfHeaderBytes = Encoding.ASCII.GetBytes("%FDF-");

    private readonly COSDocument _document;
    private readonly COSDictionary _trailer;
    private readonly float _headerVersion;
    private readonly RandomAccessRead? _source;

    private bool _disposed;

    public FDFDocument()
        : this(CreateNewDocument())
    {
    }

    public FDFDocument(XmlDocument document)
        : this()
    {
        ArgumentNullException.ThrowIfNull(document);
        XmlElement xfdf = document.DocumentElement
            ?? throw new IOException("Error while importing xfdf document, root should be 'xfdf' and not ''");
        if (!string.Equals(xfdf.LocalName, "xfdf", StringComparison.Ordinal))
        {
            throw new IOException("Error while importing xfdf document, "
                + "root should be 'xfdf' and not '" + xfdf.LocalName + "'");
        }

        SetCatalog(new FDFCatalog(xfdf));
    }

    private FDFDocument(COSDocument document)
        : this(document, null)
    {
    }

    public FDFDocument(COSDocument document, RandomAccessRead? source)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _trailer = _document.GetTrailer() ?? throw new IOException("Document trailer dictionary is missing.");
        _headerVersion = _document.GetVersion();
        _source = source;
        _document.GetDocumentState().SetParsing(false);
    }

    public static FDFDocument Load(byte[] input)
    {
        ArgumentNullException.ThrowIfNull(input);
        using MemoryStream stream = new(input, writable: false);
        return Load(stream);
    }

    public static FDFDocument Load(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        using FileStream input = File.OpenRead(filePath);
        return Load(input);
    }

    public static FDFDocument Load(Stream input)
    {
        ArgumentNullException.ThrowIfNull(input);

        using MemoryStream copy = new();
        input.CopyTo(copy);

        byte[] normalized = NormalizeToPdfHeader(copy.ToArray());
        ParsedPDFDocument parsed = new PDFParser(new MemoryStream(normalized, writable: false)).Parse();
        return new FDFDocument(parsed.Document);
    }

    public static FDFDocument LoadXFDF(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        using FileStream input = File.OpenRead(filePath);
        return LoadXFDF(input);
    }

    public static FDFDocument LoadXFDF(Stream input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return new FDFDocument(XMLUtil.Parse(input));
    }

    public COSDocument GetDocument()
    {
        EnsureNotDisposed();
        return _document;
    }

    public FDFCatalog GetCatalog()
    {
        EnsureNotDisposed();

        COSDictionary? root = _trailer.GetCOSDictionary(COSName.ROOT);
        if (root is not null)
        {
            return new FDFCatalog(root);
        }

        FDFCatalog created = new();
        SetCatalog(created);
        return created;
    }

    public void SetCatalog(FDFCatalog? catalog)
    {
        EnsureNotDisposed();
        _trailer.SetItem(COSName.ROOT, catalog);
    }

    public void Save(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        EnsureNotDisposed();

        using FileStream output = File.Create(filePath);
        Save(output);
    }

    public void Save(Stream output)
    {
        ArgumentNullException.ThrowIfNull(output);
        EnsureNotDisposed();

        _trailer.SetItem(COSName.ROOT, GetCatalog().GetCOSObject());

        byte[] headerBytes = Encoding.ASCII.GetBytes($"%FDF-{_headerVersion.ToString("0.0", CultureInfo.InvariantCulture)}\n");
        output.Write(headerBytes);

        List<(COSObjectKey Key, COSBase Inner)> indirectObjects = CollectIndirectObjects(_trailer);

        Dictionary<COSObjectKey, long> objectOffsets = new(indirectObjects.Count);
        foreach ((COSObjectKey key, COSBase inner) in indirectObjects)
        {
            objectOffsets[key] = output.Position;
            output.Write(Encoding.ASCII.GetBytes($"{key.GetNumber()} {key.GetGeneration()} obj\n"));
            output.Write(COSWriter.Serialize(inner));
            output.Write(Encoding.ASCII.GetBytes("\nendobj\n"));
        }

        long maxObjectNumber = indirectObjects.Count > 0
            ? indirectObjects.Max(x => x.Key.GetNumber())
            : 0;

        long xrefOffset = output.Position;
        output.Write(Encoding.ASCII.GetBytes($"xref\n0 {maxObjectNumber + 1}\n"));
        output.Write(Encoding.ASCII.GetBytes("0000000000 65535 f \n"));
        for (long i = 1; i <= maxObjectNumber; i++)
        {
            COSObjectKey lookupKey = new(i, 0);
            if (objectOffsets.TryGetValue(lookupKey, out long objOffset))
            {
                output.Write(Encoding.ASCII.GetBytes($"{objOffset:D10} {lookupKey.GetGeneration():D5} n \n"));
            }
            else
            {
                output.Write(Encoding.ASCII.GetBytes("0000000000 65535 f \n"));
            }
        }

        _trailer.SetInt(COSName.SIZE, checked((int)(maxObjectNumber + 1)));

        output.Write(Encoding.ASCII.GetBytes("trailer\n"));
        output.Write(COSWriter.Serialize(_trailer));
        output.Write(Encoding.ASCII.GetBytes($"\nstartxref\n{xrefOffset.ToString(CultureInfo.InvariantCulture)}\n%%EOF\n"));
    }

    public void SaveXFDF(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        EnsureNotDisposed();

        using FileStream output = File.Create(filePath);
        SaveXFDF(output);
    }

    public void SaveXFDF(Stream output)
    {
        ArgumentNullException.ThrowIfNull(output);
        EnsureNotDisposed();

        using StreamWriter writer = new(output, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), leaveOpen: true);
        WriteXml(writer);
        writer.Flush();
    }

    public void SaveXFDF(TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(output);
        EnsureNotDisposed();

        WriteXml(output);
    }

    public void WriteXml(TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(output);
        EnsureNotDisposed();

        output.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
        output.Write("<xfdf xmlns=\"http://ns.adobe.com/xfdf/\" xml:space=\"preserve\">\n");
        GetCatalog().WriteXml(output);
        output.Write("</xfdf>\n");
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _source?.Dispose();
        _document.Dispose();
    }

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

    private static COSDocument CreateNewDocument()
    {
        COSDocument document = new();
        document.SetVersion(ParseVersion(DefaultVersion));
        document.SetTrailer(new COSDictionary());
        document.GetDocumentState().SetParsing(false);

        COSDictionary trailer = document.GetTrailer()!;
        trailer.SetItem(COSName.ROOT, new FDFCatalog().GetCOSObject());

        return document;
    }

    private static float ParseVersion(string value)
    {
        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed)
            ? parsed
            : 1.2f;
    }

    private static byte[] NormalizeToPdfHeader(byte[] data)
    {
        int fdfHeaderIndex = IndexOf(data, FdfHeaderBytes);
        if (fdfHeaderIndex < 0)
        {
            if (IndexOf(data, PdfHeaderBytes) >= 0)
            {
                return data;
            }

            throw new IOException("FDF header marker not found.");
        }

        byte[] normalized = (byte[])data.Clone();
        PdfHeaderBytes.CopyTo(normalized, fdfHeaderIndex);
        return normalized;
    }

    private static int IndexOf(byte[] source, byte[] pattern)
    {
        int last = source.Length - pattern.Length;
        for (int i = 0; i <= last; i++)
        {
            bool matched = true;
            for (int j = 0; j < pattern.Length; j++)
            {
                if (source[i + j] != pattern[j])
                {
                    matched = false;
                    break;
                }
            }

            if (matched)
            {
                return i;
            }
        }

        return -1;
    }

    private void EnsureNotDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
