/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdfparser/PDFParser.java
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
using PdfBox.Net.PDModel.Encryption;

namespace PdfBox.Net.PdfParser;

public sealed class PDFParser
{
    private static readonly byte[] EndStreamBytes = Encoding.ASCII.GetBytes("endstream");
    private static readonly byte[] XrefBytes = Encoding.ASCII.GetBytes("xref");

    private readonly byte[] _data;
    private readonly COSDocument _document = new();
    private readonly Dictionary<COSObjectKey, COSObject> _objectPool;
    private readonly Dictionary<COSObjectKey, long> _resolvedXrefTable = [];
    private readonly string? _password;
    private SecurityHandler<ProtectionPolicy>? _securityHandler;
    private COSDictionary? _encryptionDictionary;

    public PDFParser(Stream input, string? password = null)
    {
        ArgumentNullException.ThrowIfNull(input);
        using MemoryStream buffer = new();
        input.CopyTo(buffer);
        _data = buffer.ToArray();
        _objectPool = _document.GetObjectPool();
        _password = password;
    }

    public ParsedPDFDocument Parse()
    {
        ParserBootstrapState bootstrap = PDFDocumentParser.ParseDocumentStart(_data);
        float headerVersion = bootstrap.HeaderVersion;
        long startXref = bootstrap.StartXrefOffset;

        XrefTrailerResolver resolver = new();
        HashSet<long> visited = [];
        ParseXrefSection(startXref, resolver, visited);
        resolver.SetStartxref(startXref);

        COSDictionary trailer = resolver.GetTrailer() ?? throw new IOException("Unable to resolve trailer dictionary.");
        Dictionary<COSObjectKey, long> xrefTable = resolver.GetXrefTable() ?? throw new IOException("Unable to resolve xref table.");

        _document.SetVersion(headerVersion);
        _document.SetStartXref(startXref);
        _document.AddXRefTable(xrefTable);

        foreach ((COSObjectKey key, long value) in xrefTable)
        {
            _resolvedXrefTable[key] = value;
            _objectPool.TryAdd(key, new COSObject(key));
        }

        LoadIndirectObjectsFromXref();
        PrepareDecryption(trailer);
        DecryptLoadedIndirectObjects();
        LoadCompressedObjects();
        BindTrailerReferences(trailer);
        RecoverPageTreeCounts(trailer);
        _document.SetTrailer(trailer);
        if (_securityHandler is not null)
        {
            _document.SetDecrypted();
        }

        _document.GetDocumentState().SetParsing(false);

        return new ParsedPDFDocument(_document, trailer, headerVersion);
    }

    private void PrepareDecryption(COSDictionary trailer)
    {
        _encryptionDictionary = trailer.GetCOSDictionary(COSName.GetPDFName("Encrypt"));
        if (_encryptionDictionary is null)
        {
            return;
        }

        PDEncryption encryption = new(_encryptionDictionary);
        string filter = encryption.GetFilter() ?? PDEncryption.DEFAULT_NAME;
        _securityHandler = SecurityHandlerFactory.INSTANCE.NewSecurityHandlerForFilter(filter)
            ?? throw new IOException($"No security handler available for filter '{filter}'.");

        DecryptionMaterial material = _securityHandler switch
        {
            StandardSecurityHandler => new StandardDecryptionMaterial(_password ?? string.Empty),
            PublicKeySecurityHandler => throw new IOException("Public-key encrypted documents require PublicKeyDecryptionMaterial and are not supported by this Load overload."),
            _ => throw new IOException($"Unsupported security handler type '{_securityHandler.GetType().FullName}'.")
        };

        _securityHandler.PrepareForDecryption(encryption, trailer.GetCOSArray(COSName.GetPDFName("ID")), material);
    }

    private void DecryptLoadedIndirectObjects()
    {
        if (_securityHandler is null)
        {
            return;
        }

        HashSet<COSBase> visited = new(ReferenceEqualityComparer.Instance);
        foreach (COSObject cosObject in _objectPool.Values)
        {
            COSObjectKey? key = cosObject.GetKey();
            COSBase? value = cosObject.GetObject();
            if (key is null || value is null || ReferenceEquals(value, _encryptionDictionary))
            {
                continue;
            }

            DecryptObjectGraph(value, key.GetNumber(), key.GetGeneration(), visited);
        }
    }

    private void DecryptObjectGraph(COSBase obj, long objNum, long genNum, HashSet<COSBase> visited)
    {
        if (_securityHandler is null || !visited.Add(obj) || ReferenceEquals(obj, _encryptionDictionary))
        {
            return;
        }

        switch (obj)
        {
            case COSObject cosObject:
            {
                COSObjectKey? key = cosObject.GetKey();
                COSBase? inner = cosObject.GetObject();
                if (inner is not null)
                {
                    DecryptObjectGraph(inner, key?.GetNumber() ?? objNum, key?.GetGeneration() ?? genNum, visited);
                }

                break;
            }

            case COSString cosString:
                _securityHandler.DecryptString(objNum, genNum, cosString);
                break;

            case COSStream cosStream:
                if (ShouldDecryptStream(cosStream))
                {
                    DecryptStream(cosStream, objNum, genNum);
                }

                DecryptDictionaryItems(cosStream, objNum, genNum, visited);
                break;

            case COSDictionary cosDictionary:
                DecryptDictionaryItems(cosDictionary, objNum, genNum, visited);
                break;

            case COSArray cosArray:
                for (int i = 0; i < cosArray.Size(); i++)
                {
                    COSBase? value = cosArray.Get(i);
                    if (value is not null)
                    {
                        DecryptObjectGraph(value, objNum, genNum, visited);
                    }
                }

                break;
        }
    }

    private void DecryptDictionaryItems(COSDictionary dictionary, long objNum, long genNum, HashSet<COSBase> visited)
    {
        foreach (COSName key in dictionary.KeySet().ToList())
        {
            COSBase? value = dictionary.GetItem(key);
            if (value is not null)
            {
                DecryptObjectGraph(value, objNum, genNum, visited);
            }
        }
    }

    private bool ShouldDecryptStream(COSStream stream)
    {
        return _securityHandler is not null
            && !COSName.IDENTITY.Equals(_securityHandler.GetStreamFilterName())
            && !COSName.GetPDFName("XRef").Equals(stream.GetCOSName(COSName.TYPE))
            && (_securityHandler.IsDecryptMetadata() || !COSName.METADATA.Equals(stream.GetCOSName(COSName.TYPE)));
    }

    private void DecryptStream(COSStream stream, long objNum, long genNum)
    {
        using MemoryStream decrypted = new();
        using (Stream encrypted = stream.CreateRawInputStream())
        {
            _securityHandler!.DecryptData(objNum, genNum, encrypted, decrypted);
        }

        using Stream output = stream.CreateRawOutputStream();
        byte[] data = decrypted.ToArray();
        output.Write(data, 0, data.Length);
    }

    private void ParseXrefSection(long offset, XrefTrailerResolver resolver, HashSet<long> visited)
    {
        if (offset < 0 || offset >= _data.Length || !visited.Add(offset))
        {
            return;
        }

        SyntaxReader reader = new(_data, checked((int)offset));
        reader.SkipSpacesAndComments();

        if (reader.TryReadKeyword("xref"))
        {
            resolver.NextXrefObj(offset, XrefTrailerResolver.XRefType.TABLE);
            ParseXrefTable(reader, resolver, visited);
            return;
        }

        if (TryFindNearbyXrefTableOffset(offset, out long correctedOffset))
        {
            ParseXrefSection(correctedOffset, resolver, visited);
            return;
        }

        resolver.NextXrefObj(offset, XrefTrailerResolver.XRefType.STREAM);
        ParsedIndirectObject xrefStreamObject = ParseIndirectObjectAt(offset);
        if (xrefStreamObject.Value is not COSStream stream)
        {
            throw new IOException("Expected cross-reference stream object.");
        }

        COSDictionary streamDictionary = stream;
        resolver.SetTrailer(streamDictionary);
        ParseXrefStreamEntries(streamDictionary, stream, resolver);

        long prev = streamDictionary.GetLong(COSName.GetPDFName("Prev"), -1);
        if (prev >= 0)
        {
            ParseXrefSection(prev, resolver, visited);
        }
    }

    private void ParseXrefTable(SyntaxReader reader, XrefTrailerResolver resolver, HashSet<long> visited)
    {
        while (true)
        {
            reader.SkipSpacesAndComments();
            int checkpoint = reader.Position;
            string token = reader.ReadToken();
            if (token.Length == 0)
            {
                throw new IOException("Unexpected EOF while parsing xref table.");
            }

            if (token.Equals("trailer", StringComparison.Ordinal))
            {
                break;
            }

            if (!long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out long firstObject) || firstObject < 0)
            {
                throw new IOException("Malformed xref table subsection header.");
            }

            string countToken = reader.ReadToken();
            if (!int.TryParse(countToken, NumberStyles.Integer, CultureInfo.InvariantCulture, out int count) || count < 0)
            {
                throw new IOException("Malformed xref table subsection length.");
            }

            for (int i = 0; i < count; i++)
            {
                reader.SkipSpacesAndComments();
                string offsetToken = reader.ReadToken();
                string generationToken = reader.ReadToken();
                string inUseToken = reader.ReadToken();
                if (!long.TryParse(offsetToken, NumberStyles.Integer, CultureInfo.InvariantCulture, out long entryOffset) ||
                    !int.TryParse(generationToken, NumberStyles.Integer, CultureInfo.InvariantCulture, out int generation))
                {
                    throw new IOException("Malformed xref entry.");
                }

                if (inUseToken.Equals("n", StringComparison.Ordinal))
                {
                    COSObjectKey key = new(firstObject + i, generation);
                    resolver.SetXRef(key, entryOffset);
                }
                else if (inUseToken.EndsWith('n'))
                {
                    COSObjectKey key = new(firstObject + i, generation);
                    resolver.SetXRef(key, entryOffset);
                }
                else if (!inUseToken.Equals("f", StringComparison.Ordinal))
                {
                    throw new IOException("Malformed xref entry state.");
                }
            }

            if (checkpoint == reader.Position)
            {
                throw new IOException("Unable to advance while parsing xref table.");
            }
        }

        COSBase trailerObject = ParseObject(reader);
        if (trailerObject is not COSDictionary trailer)
        {
            throw new IOException("Expected trailer dictionary after xref table.");
        }

        resolver.SetTrailer(trailer);

        long prev = trailer.GetLong(COSName.GetPDFName("Prev"), -1);
        if (prev >= 0)
        {
            ParseXrefSection(prev, resolver, visited);
        }

        long xrefStreamOffset = trailer.GetLong(COSName.GetPDFName("XRefStm"), -1);
        if (xrefStreamOffset >= 0)
        {
            ParseXrefSection(xrefStreamOffset, resolver, visited);
        }
    }

    private void ParseXrefStreamEntries(COSDictionary streamDictionary, COSStream stream, XrefTrailerResolver resolver)
    {
        COSArray? wArray = streamDictionary.GetCOSArray(COSName.GetPDFName("W"));
        if (wArray is null || wArray.Size() < 3)
        {
            throw new IOException("Cross-reference stream missing /W entry.");
        }

        int w0 = wArray.GetInt(0, 0);
        int w1 = wArray.GetInt(1, 0);
        int w2 = wArray.GetInt(2, 0);
        int rowLength = w0 + w1 + w2;
        if (rowLength <= 0)
        {
            throw new IOException("Cross-reference stream /W widths are invalid.");
        }

        List<(long firstObjectNumber, int count)> sections = GetXrefSections(streamDictionary);
        using Stream decoded = stream.CreateInputStream();
        using MemoryStream copy = new();
        decoded.CopyTo(copy);
        byte[] bytes = copy.ToArray();

        int cursor = 0;
        foreach ((long firstObjectNumber, int count) in sections)
        {
            for (int i = 0; i < count; i++)
            {
                if (cursor + rowLength > bytes.Length)
                {
                    throw new IOException("Cross-reference stream ended unexpectedly.");
                }

                long type = ReadBigEndian(bytes, cursor, w0, 1);
                cursor += w0;
                long field2 = ReadBigEndian(bytes, cursor, w1, 0);
                cursor += w1;
                long field3 = ReadBigEndian(bytes, cursor, w2, 0);
                cursor += w2;

                switch (type)
                {
                    case 0:
                        break;
                    case 1:
                        COSObjectKey key = new(firstObjectNumber + i, checked((int)field3));
                        resolver.SetXRef(key, field2);
                        break;
                    case 2:
                        COSObjectKey compressedObjectKey = new(firstObjectNumber + i, 0);
                        resolver.SetXRef(compressedObjectKey, -field2);
                        break;
                }
            }
        }
    }

    private List<(long firstObjectNumber, int count)> GetXrefSections(COSDictionary dictionary)
    {
        COSArray? index = dictionary.GetCOSArray(COSName.GetPDFName("Index"));
        if (index is null)
        {
            int size = dictionary.GetInt(COSName.GetPDFName("Size"), 0);
            return [(0, size)];
        }

        List<(long firstObjectNumber, int count)> sections = [];
        for (int i = 0; i + 1 < index.Size(); i += 2)
        {
            long first = index.Get(i) is COSNumber firstNumber ? firstNumber.LongValue() : 0;
            int count = index.Get(i + 1) is COSNumber countNumber ? countNumber.IntValue() : 0;
            sections.Add((first, count));
        }

        return sections;
    }

    private void LoadIndirectObjectsFromXref()
    {
        foreach ((COSObjectKey key, long offset) in _resolvedXrefTable.OrderBy(kvp => kvp.Value))
        {
            if (offset <= 0)
            {
                continue;
            }

            if (offset >= _data.Length)
            {
                continue;
            }

            long effectiveOffset = offset;
            if (!TryParseIndirectObjectAt(offset, out ParsedIndirectObject? parsed) || parsed is null || !parsed.Key.Equals(key))
            {
                if (!TryFindIndirectObjectOffset(key, out long recoveredOffset)
                    || !TryParseIndirectObjectAt(recoveredOffset, out parsed)
                    || parsed is null
                    || !parsed.Key.Equals(key))
                {
                    continue;
                }

                effectiveOffset = recoveredOffset;
            }

            if (effectiveOffset != _resolvedXrefTable[key])
            {
                _resolvedXrefTable[key] = effectiveOffset;
                _document.AddXRefTable(new Dictionary<COSObjectKey, long> { [key] = effectiveOffset });
            }

            ParsedIndirectObject parsedObject = parsed ?? throw new InvalidOperationException("Object parsing unexpectedly produced no result.");
            COSObject objectShell = GetOrCreateIndirectObject(parsedObject.Key);
            parsedObject.Value.SetKey(parsedObject.Key);
            objectShell.SetObject(parsedObject.Value);
        }
    }

    private void LoadCompressedObjects()
    {
        Dictionary<long, List<COSObjectKey>> groupedByObjectStream = [];
        foreach ((COSObjectKey key, long offset) in _resolvedXrefTable)
        {
            if (offset < 0)
            {
                long objectStreamObjectNumber = -offset;
                if (!groupedByObjectStream.TryGetValue(objectStreamObjectNumber, out List<COSObjectKey>? list))
                {
                    list = [];
                    groupedByObjectStream[objectStreamObjectNumber] = list;
                }

                list.Add(key);
            }
        }

        foreach ((long objectStreamObjectNumber, List<COSObjectKey> referencedKeys) in groupedByObjectStream)
        {
            COSObject? objectStreamContainer = _objectPool.Keys
                .Where(k => k.GetNumber() == objectStreamObjectNumber)
                .Select(k => _objectPool[k])
                .FirstOrDefault();

            if (objectStreamContainer?.GetObject() is not COSStream objectStream)
            {
                continue;
            }

            PDFObjectStreamParser parser = new(objectStream, _objectPool);
            foreach ((COSObjectKey key, COSBase value) in parser.Parse())
            {
                if (!referencedKeys.Contains(key))
                {
                    continue;
                }

                COSObject shell = GetOrCreateIndirectObject(key);
                value.SetKey(key);
                shell.SetObject(value);
            }
        }
    }

    private void BindTrailerReferences(COSDictionary trailer)
    {
        foreach (COSName key in trailer.KeySet().ToList())
        {
            COSBase? value = trailer.GetItem(key);
            if (value is COSObject reference)
            {
                COSObject? resolved = reference.GetKey() is null ? null : _objectPool.GetValueOrDefault(reference.GetKey()!);
                if (resolved is not null)
                {
                    trailer.SetItem(key, resolved);
                }
            }
        }
    }

    private static void RecoverPageTreeCounts(COSDictionary trailer)
    {
        COSDictionary? root = trailer.GetCOSDictionary(COSName.ROOT);
        COSDictionary? pages = root?.GetCOSDictionary(COSName.PAGES);
        if (pages is null || !NeedsPageTreeCountRecovery(pages))
        {
            return;
        }

        CheckPagesDictionary(pages, []);
    }

    private static bool NeedsPageTreeCountRecovery(COSDictionary pages)
    {
        COSArray? kids = pages.GetCOSArray(COSName.KIDS);
        COSBase? count = pages.GetDictionaryObject(COSName.COUNT);
        if (count is not COSNumber)
        {
            return true;
        }

        if (kids is not null && kids.Size() > 0 && pages.GetInt(COSName.COUNT, 0) <= 0)
        {
            return true;
        }

        if (kids is null)
        {
            return false;
        }

        for (int i = 0; i < kids.Size(); i++)
        {
            if (kids.Get(i) is COSObject kidObject && (kidObject.GetObject() is null or COSNull))
            {
                return true;
            }
        }

        return false;
    }

    private static int CheckPagesDictionary(COSDictionary pagesDict, HashSet<COSObject> visited)
    {
        COSArray? kidsArray = pagesDict.GetCOSArray(COSName.KIDS);
        int numberOfPages = 0;
        if (kidsArray is not null)
        {
            for (int i = 0; i < kidsArray.Size();)
            {
                COSBase? kid = kidsArray.Get(i);
                if (kid is not COSObject kidObject || visited.Contains(kidObject))
                {
                    kidsArray.Remove(i);
                    continue;
                }

                COSBase? kidBaseObject = kidObject.GetObject();
                if (kidBaseObject is null or COSNull)
                {
                    kidsArray.Remove(i);
                    continue;
                }

                if (kidBaseObject is COSDictionary kidDictionary)
                {
                    COSName? type = kidDictionary.GetCOSName(COSName.TYPE);
                    if (COSName.PAGES.Equals(type))
                    {
                        visited.Add(kidObject);
                        numberOfPages += CheckPagesDictionary(kidDictionary, visited);
                    }
                    else if (COSName.PAGE.Equals(type))
                    {
                        numberOfPages++;
                    }
                }

                i++;
            }
        }

        if (pagesDict.GetInt(COSName.COUNT, -1) != numberOfPages)
        {
            pagesDict.SetInt(COSName.COUNT, numberOfPages);
        }

        return numberOfPages;
    }

    private ParsedIndirectObject ParseIndirectObjectAt(long offset)
    {
        if (offset < 0 || offset >= _data.Length)
        {
            throw new IOException("Indirect object offset is outside the PDF byte range.");
        }

        SyntaxReader reader = new(_data, checked((int)offset));
        reader.SkipSpacesAndComments();

        string objectNumberToken = reader.ReadToken();
        string generationToken = reader.ReadToken();
        if (!long.TryParse(objectNumberToken, NumberStyles.Integer, CultureInfo.InvariantCulture, out long objectNumber) ||
            !int.TryParse(generationToken, NumberStyles.Integer, CultureInfo.InvariantCulture, out int generation))
        {
            throw new IOException("Malformed indirect object header.");
        }

        if (!reader.TryReadKeyword("obj"))
        {
            throw new IOException("Malformed indirect object header: missing obj keyword.");
        }

        COSObjectKey key = new(objectNumber, generation);
        COSBase parsedValue = ParseObject(reader);

        if (parsedValue is COSDictionary dictionary && reader.TryReadKeyword("stream"))
        {
            ConsumeSingleEndOfLine(reader);
            parsedValue = ReadStreamObject(reader, dictionary);
        }

        reader.TryReadKeyword("endobj");
        return new ParsedIndirectObject(key, parsedValue);
    }

    private bool TryParseIndirectObjectAt(long offset, out ParsedIndirectObject? parsed)
    {
        try
        {
            parsed = ParseIndirectObjectAt(offset);
            return true;
        }
        catch (IOException)
        {
            parsed = null;
            return false;
        }
    }

    private bool TryFindIndirectObjectOffset(COSObjectKey key, out long offset)
    {
        byte[] objectNumber = Encoding.ASCII.GetBytes(key.GetNumber().ToString(CultureInfo.InvariantCulture));
        byte[] generation = Encoding.ASCII.GetBytes(key.GetGeneration().ToString(CultureInfo.InvariantCulture));

        for (int i = 0; i < _data.Length; i++)
        {
            if (!IsTokenBoundaryBefore(i) || !MatchesBytes(i, objectNumber))
            {
                continue;
            }

            int position = i + objectNumber.Length;
            if (position >= _data.Length || !IsWhiteSpace(_data[position]))
            {
                continue;
            }

            position = SkipWhiteSpace(position);
            if (!MatchesBytes(position, generation))
            {
                continue;
            }

            position += generation.Length;
            if (position >= _data.Length || !IsWhiteSpace(_data[position]))
            {
                continue;
            }

            position = SkipWhiteSpace(position);
            if (MatchesKeyword(position, "obj") && IsTokenBoundaryAfter(position + 3))
            {
                offset = i;
                return true;
            }
        }

        offset = -1;
        return false;
    }

    private bool MatchesBytes(int offset, byte[] pattern)
    {
        if (offset < 0 || offset + pattern.Length > _data.Length)
        {
            return false;
        }

        for (int i = 0; i < pattern.Length; i++)
        {
            if (_data[offset + i] != pattern[i])
            {
                return false;
            }
        }

        return true;
    }

    private bool MatchesKeyword(int offset, string keyword)
    {
        if (offset < 0 || offset + keyword.Length > _data.Length)
        {
            return false;
        }

        for (int i = 0; i < keyword.Length; i++)
        {
            if (_data[offset + i] != keyword[i])
            {
                return false;
            }
        }

        return true;
    }

    private int SkipWhiteSpace(int position)
    {
        while (position < _data.Length && IsWhiteSpace(_data[position]))
        {
            position++;
        }

        return position;
    }

    private bool IsTokenBoundaryBefore(int position)
    {
        return position == 0 || IsWhiteSpace(_data[position - 1]) || IsDelimiter(_data[position - 1]);
    }

    private bool IsTokenBoundaryAfter(int position)
    {
        return position >= _data.Length || IsWhiteSpace(_data[position]) || IsDelimiter(_data[position]);
    }

    private COSStream ReadStreamObject(SyntaxReader reader, COSDictionary dictionary)
    {
        int streamStart = reader.Position;
        long declaredLength = ResolveLengthValue(dictionary);
        byte[] streamBytes;

        if (declaredLength >= 0 && streamStart + declaredLength <= _data.Length)
        {
            streamBytes = _data.AsSpan(streamStart, checked((int)declaredLength)).ToArray();
            reader.Position = checked(streamStart + (int)declaredLength);

            if (!reader.TryReadKeyword("endstream"))
            {
                int markerPosition = IndexOf(_data, EndStreamBytes, streamStart);
                if (markerPosition < 0)
                {
                    throw new IOException("Stream object is missing endstream marker.");
                }

                streamBytes = _data.AsSpan(streamStart, markerPosition - streamStart).ToArray();
                TrimTrailingLineBreak(streamBytes, out streamBytes);
                reader.Position = markerPosition;
                _ = reader.TryReadKeyword("endstream");
            }
        }
        else
        {
            int markerPosition = IndexOf(_data, EndStreamBytes, streamStart);
            if (markerPosition < 0)
            {
                throw new IOException("Stream object is missing endstream marker.");
            }

            streamBytes = _data.AsSpan(streamStart, markerPosition - streamStart).ToArray();
            TrimTrailingLineBreak(streamBytes, out streamBytes);
            reader.Position = markerPosition;
            _ = reader.TryReadKeyword("endstream");
        }

        COSStream stream = new();
        foreach ((COSName key, COSBase value) in dictionary.EntrySet())
        {
            stream.SetItem(key, value);
        }

        using (Stream output = stream.CreateRawOutputStream())
        {
            output.Write(streamBytes, 0, streamBytes.Length);
        }

        return stream;
    }

    private long ResolveLengthValue(COSDictionary dictionary)
    {
        COSBase? lengthBase = dictionary.GetItem(COSName.LENGTH);
        if (lengthBase is COSNumber number)
        {
            return number.LongValue();
        }

        if (lengthBase is COSObject reference)
        {
            COSBase? resolved = reference.GetObject();
            if (resolved is null && reference.GetKey() is not null && _objectPool.TryGetValue(reference.GetKey()!, out COSObject? resolvedObject))
            {
                resolved = resolvedObject.GetObject();
            }

            if (resolved is COSNumber resolvedNumber)
            {
                return resolvedNumber.LongValue();
            }
        }

        return -1;
    }

    private COSBase ParseObject(SyntaxReader reader)
    {
        reader.SkipSpacesAndComments();
        int first = reader.ReadByte();
        if (first == -1)
        {
            throw new EndOfStreamException("Unexpected EOF while parsing PDF object.");
        }

        switch ((char)first)
        {
            case '<':
            {
                int next = reader.ReadByte();
                if (next == '<')
                {
                    return ParseDictionary(reader);
                }

                reader.UnreadByte(next);
                return ParseHexString(reader);
            }
            case '[':
                return ParseArray(reader);
            case '/':
                return ParseName(reader);
            case '(':
                return ParseLiteralString(reader);
            default:
                return ParseAtomicToken(reader, first);
        }
    }

    private COSBase ParseDictionary(SyntaxReader reader)
    {
        COSDictionary dictionary = new();
        while (true)
        {
            reader.SkipSpacesAndComments();
            int b = reader.ReadByte();
            if (b == '>')
            {
                if (reader.ReadByte() != '>')
                {
                    throw new IOException("Malformed dictionary close marker.");
                }

                break;
            }

            reader.UnreadByte(b);
            COSBase keyObject = ParseObject(reader);
            if (keyObject is not COSName key)
            {
                throw new IOException("Dictionary key must be a COS name.");
            }

            COSBase valueObject = ParseObject(reader);
            dictionary.SetItem(key, valueObject);
        }

        return dictionary;
    }

    private COSBase ParseArray(SyntaxReader reader)
    {
        COSArray array = new();
        while (true)
        {
            reader.SkipSpacesAndComments();
            int b = reader.ReadByte();
            if (b == ']')
            {
                break;
            }

            if (b == -1)
            {
                throw new EndOfStreamException("Unexpected EOF while parsing array.");
            }

            reader.UnreadByte(b);
            array.Add(ParseObject(reader));
        }

        return array;
    }

    private COSBase ParseName(SyntaxReader reader)
    {
        StringBuilder builder = new();
        while (true)
        {
            int b = reader.ReadByte();
            if (b == -1 || IsWhiteSpace(b) || IsDelimiter(b))
            {
                reader.UnreadByte(b);
                break;
            }

            if (b == '#')
            {
                int high = reader.ReadByte();
                int low = reader.ReadByte();
                if (high == -1 || low == -1)
                {
                    throw new EndOfStreamException("Unexpected EOF in name escape.");
                }

                int decoded = ParseHex(high) * 16 + ParseHex(low);
                if (decoded < 0)
                {
                    throw new IOException("Invalid hex escape in name token.");
                }

                builder.Append((char)decoded);
                continue;
            }

            builder.Append((char)b);
        }

        return COSName.GetPDFName(builder.ToString());
    }

    private COSBase ParseLiteralString(SyntaxReader reader)
    {
        List<byte> bytes = [];
        int nesting = 1;
        while (nesting > 0)
        {
            int b = reader.ReadByte();
            if (b == -1)
            {
                throw new EndOfStreamException("Unexpected EOF in literal string.");
            }

            if (b == '\\')
            {
                int escaped = reader.ReadByte();
                if (escaped == -1)
                {
                    throw new EndOfStreamException("Unexpected EOF in string escape.");
                }

                if (escaped is >= '0' and <= '7')
                {
                    int octal = escaped - '0';
                    for (int i = 0; i < 2; i++)
                    {
                        int next = reader.PeekByte();
                        if (next is >= '0' and <= '7')
                        {
                            octal = (octal * 8) + (reader.ReadByte() - '0');
                        }
                        else
                        {
                            break;
                        }
                    }

                    bytes.Add((byte)octal);
                    continue;
                }

                switch ((char)escaped)
                {
                    case 'n': bytes.Add((byte)'\n'); break;
                    case 'r': bytes.Add((byte)'\r'); break;
                    case 't': bytes.Add((byte)'\t'); break;
                    case 'b': bytes.Add((byte)'\b'); break;
                    case 'f': bytes.Add((byte)'\f'); break;
                    case '\r':
                        if (reader.PeekByte() == '\n')
                        {
                            _ = reader.ReadByte();
                        }

                        break;
                    case '\n':
                        break;
                    default:
                        bytes.Add((byte)escaped);
                        break;
                }

                continue;
            }

            if (b == '(')
            {
                nesting++;
            }
            else if (b == ')')
            {
                nesting--;
                if (nesting == 0)
                {
                    break;
                }
            }

            bytes.Add((byte)b);
        }

        return new COSString([.. bytes]);
    }

    private COSBase ParseHexString(SyntaxReader reader)
    {
        StringBuilder hex = new();
        while (true)
        {
            int b = reader.ReadByte();
            if (b == -1)
            {
                throw new EndOfStreamException("Unexpected EOF in hex string.");
            }

            if (b == '>')
            {
                break;
            }

            if (!IsWhiteSpace(b))
            {
                hex.Append((char)b);
            }
        }

        return COSString.ParseHex(hex.ToString());
    }

    private COSBase ParseAtomicToken(SyntaxReader reader, int first)
    {
        string token = reader.ReadTokenStartingWith(first);
        if (token.Length == 0)
        {
            throw new IOException("Unexpected empty token.");
        }

        return token switch
        {
            "true" => COSBoolean.TRUE,
            "false" => COSBoolean.FALSE,
            "null" => COSNull.NULL,
            _ => ParseNumberOrReference(reader, token)
        };
    }

    private COSBase ParseNumberOrReference(SyntaxReader reader, string numberToken)
    {
        if (!long.TryParse(numberToken, NumberStyles.Integer, CultureInfo.InvariantCulture, out long objectNumber) || objectNumber < 0)
        {
            return COSNumber.Get(numberToken);
        }

        int rollbackPosition = reader.Position;
        reader.SkipSpacesAndComments();

        if (reader.TryReadIntegerToken(out int generation) && generation >= 0)
        {
            reader.SkipSpacesAndComments();
            if (reader.TryReadKeyword("R"))
            {
                COSObjectKey key = new(objectNumber, generation);
                return GetOrCreateIndirectObject(key);
            }
        }

        reader.Position = rollbackPosition;
        return COSNumber.Get(numberToken);
    }

    private COSObject GetOrCreateIndirectObject(COSObjectKey key)
    {
        if (_objectPool.TryGetValue(key, out COSObject? existing))
        {
            return existing;
        }

        COSObject created = new(key);
        _objectPool[key] = created;
        return created;
    }

    private static long ReadBigEndian(byte[] bytes, int offset, int width, long defaultValue)
    {
        if (width == 0)
        {
            return defaultValue;
        }

        long value = 0;
        for (int i = 0; i < width; i++)
        {
            value = (value << 8) | bytes[offset + i];
        }

        return value;
    }

    private static void ConsumeSingleEndOfLine(SyntaxReader reader)
    {
        int current = reader.PeekByte();
        if (current == '\r')
        {
            _ = reader.ReadByte();
            if (reader.PeekByte() == '\n')
            {
                _ = reader.ReadByte();
            }
        }
        else if (current == '\n')
        {
            _ = reader.ReadByte();
        }
    }

    private static void TrimTrailingLineBreak(byte[] input, out byte[] output)
    {
        int end = input.Length;
        if (end > 0 && input[end - 1] == '\n')
        {
            end--;
            if (end > 0 && input[end - 1] == '\r')
            {
                end--;
            }
        }
        else if (end > 0 && input[end - 1] == '\r')
        {
            end--;
        }

        output = end == input.Length ? input : input.AsSpan(0, end).ToArray();
    }

    private static int IndexOf(byte[] source, byte[] pattern, int start)
    {
        for (int i = start; i <= source.Length - pattern.Length; i++)
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

    private bool TryFindNearbyXrefTableOffset(long declaredOffset, out long correctedOffset)
    {
        const int correctionWindow = 16;
        int start = checked((int)Math.Max(0, declaredOffset - correctionWindow));
        int end = checked((int)Math.Min(_data.Length, declaredOffset + correctionWindow + XrefBytes.Length));
        for (int i = start; i <= end - XrefBytes.Length; i++)
        {
            bool matched = true;
            for (int j = 0; j < XrefBytes.Length; j++)
            {
                if (_data[i + j] != XrefBytes[j])
                {
                    matched = false;
                    break;
                }
            }

            if (matched)
            {
                correctedOffset = i;
                return true;
            }
        }

        correctedOffset = -1;
        return false;
    }

    private static bool IsWhiteSpace(int value)
    {
        return value is 0 or 9 or 10 or 12 or 13 or 32;
    }

    private static bool IsDelimiter(int value)
    {
        return value is '(' or ')' or '<' or '>' or '[' or ']' or '{' or '}' or '/' or '%';
    }

    private static int ParseHex(int value)
    {
        return value switch
        {
            >= '0' and <= '9' => value - '0',
            >= 'A' and <= 'F' => value - 'A' + 10,
            >= 'a' and <= 'f' => value - 'a' + 10,
            _ => -1
        };
    }

    private sealed record ParsedIndirectObject(COSObjectKey Key, COSBase Value);

    private sealed class SyntaxReader(byte[] data, int position)
    {
        private const int NoLookAhead = -2;
        private int _lookAhead = NoLookAhead;

        public int Position
        {
            get => position;
            set
            {
                position = value;
                _lookAhead = NoLookAhead;
            }
        }

        public int PeekByte()
        {
            if (_lookAhead == NoLookAhead)
            {
                _lookAhead = position >= data.Length ? -1 : data[position];
            }

            return _lookAhead;
        }

        public int ReadByte()
        {
            if (_lookAhead != NoLookAhead)
            {
                int value = _lookAhead;
                _lookAhead = NoLookAhead;
                if (value != -1)
                {
                    position++;
                }

                return value;
            }

            if (position >= data.Length)
            {
                return -1;
            }

            return data[position++];
        }

        public void UnreadByte(int value)
        {
            if (value == -1)
            {
                return;
            }

            if (_lookAhead != NoLookAhead)
            {
                throw new InvalidOperationException("Reader lookahead buffer already occupied.");
            }

            _lookAhead = value;
            position--;
        }

        public void SkipSpacesAndComments()
        {
            while (true)
            {
                int b = ReadByte();
                if (b == -1)
                {
                    return;
                }

                if (IsWhiteSpace(b))
                {
                    continue;
                }

                if (b == '%')
                {
                    while (true)
                    {
                        int c = ReadByte();
                        if (c is -1 or '\n' or '\r')
                        {
                            break;
                        }
                    }

                    continue;
                }

                UnreadByte(b);
                return;
            }
        }

        public string ReadToken()
        {
            SkipSpacesAndComments();
            int b = ReadByte();
            if (b == -1)
            {
                return string.Empty;
            }

            return ReadTokenStartingWith(b);
        }

        public string ReadTokenStartingWith(int first)
        {
            StringBuilder token = new();
            token.Append((char)first);
            while (true)
            {
                int b = ReadByte();
                if (b == -1 || IsWhiteSpace(b) || IsDelimiter(b))
                {
                    UnreadByte(b);
                    break;
                }

                token.Append((char)b);
            }

            return token.ToString();
        }

        public bool TryReadKeyword(string expected)
        {
            int rollback = Position;
            string token = ReadToken();
            if (token.Equals(expected, StringComparison.Ordinal))
            {
                return true;
            }

            Position = rollback;
            return false;
        }

        public bool TryReadIntegerToken(out int value)
        {
            int rollback = Position;
            string token = ReadToken();
            if (int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }

            Position = rollback;
            return false;
        }
    }
}

public sealed record ParsedPDFDocument(COSDocument Document, COSDictionary Trailer, float HeaderVersion);
