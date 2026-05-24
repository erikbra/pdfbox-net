/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/cos/COSDocument.java
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

using PdfBox.Net.IO;

namespace PdfBox.Net.COS;

/// <summary>
/// This is the in-memory representation of the PDF document.  You need to call
/// <see cref="Dispose"/> on this object when you are done using it!
/// </summary>
/// <remarks>Author: Ben Litchfield</remarks>
public class COSDocument : COSBase, IDisposable
{
    private float _version = 1.4f;

    /// <summary>
    /// Maps ObjectKeys to a COSObject. Note that references to these objects
    /// are also stored in COSDictionary objects that map a name to a specific object.
    /// </summary>
    private readonly Dictionary<COSObjectKey, COSObject> _objectPool = new();

    /// <summary>
    /// Maps object and generation id to object byte offsets.
    /// </summary>
    private readonly Dictionary<COSObjectKey, long> _xrefTable = new();

    /// <summary>
    /// List containing all streams which are created when creating a new pdf.
    /// </summary>
    private readonly List<COSStream> _streams = new();

    /// <summary>
    /// Document trailer dictionary.
    /// </summary>
    private COSDictionary? _trailer;

    /// <summary>
    /// Signal that document is already decrypted.
    /// </summary>
    private bool _isDecrypted = false;

    private long _startXref;

    private bool _closed = false;

    private bool _isXRefStream;

    private bool _hasHybridXRef = false;

    private readonly RandomAccessStreamCache? _streamCache;

    /// <summary>
    /// Used for incremental saving, to avoid XRef object numbers from being reused.
    /// </summary>
    private long _highestXRefObjectNumber;

    private readonly ICOSParser? _parser;

    private readonly COSDocumentState _documentState = new COSDocumentState();

    /// <summary>
    /// Constructor. Uses main memory to buffer PDF streams.
    /// </summary>
    public COSDocument()
        : this(IOUtils.CreateMemoryOnlyStreamCache())
    {
    }

    /// <summary>
    /// Constructor. Uses main memory to buffer PDF streams.
    /// </summary>
    /// <param name="parser">Parser to be used to parse the document on demand</param>
    public COSDocument(ICOSParser parser)
        : this(IOUtils.CreateMemoryOnlyStreamCache(), parser)
    {
    }

    /// <summary>
    /// Constructor that will use the provided function to create a stream cache for the storage of the PDF streams.
    /// </summary>
    /// <param name="streamCacheCreateFunction">a function to create an instance of a stream cache</param>
    public COSDocument(RandomAccessStreamCache.StreamCacheCreateFunction streamCacheCreateFunction)
        : this(streamCacheCreateFunction, null)
    {
    }

    /// <summary>
    /// Constructor that will use the provided function to create a stream cache for the storage of the PDF streams.
    /// </summary>
    /// <param name="streamCacheCreateFunction">a function to create an instance of a stream cache</param>
    /// <param name="parser">Parser to be used to parse the document on demand</param>
    public COSDocument(RandomAccessStreamCache.StreamCacheCreateFunction? streamCacheCreateFunction, ICOSParser? parser)
    {
        _streamCache = GetStreamCache(streamCacheCreateFunction);
        _parser = parser;
    }

    private static RandomAccessStreamCache? GetStreamCache(RandomAccessStreamCache.StreamCacheCreateFunction? streamCacheCreateFunction)
    {
        if (streamCacheCreateFunction == null)
        {
            return null;
        }
        try
        {
            return streamCacheCreateFunction();
        }
        catch (IOException)
        {
            // LOG.warn: An error occurred when creating stream cache. Using memory only cache as fallback.
        }
        try
        {
            return IOUtils.CreateMemoryOnlyStreamCache()();
        }
        catch (IOException)
        {
            // LOG.warn: An error occurred when creating stream cache for fallback.
        }
        return null;
    }

    /// <summary>
    /// Creates a new COSStream using the current configuration for scratch files.
    /// </summary>
    /// <returns>the new COSStream</returns>
    public COSStream CreateCOSStream()
    {
        COSStream stream = new COSStream(_streamCache);
        // collect all COSStreams so that they can be closed when closing the COSDocument.
        // This is limited to newly created pdfs as all COSStreams of an existing pdf are
        // collected within the map objectPool
        _streams.Add(stream);
        return stream;
    }

    /// <summary>
    /// Creates a new COSStream using the current configuration for scratch files. Not for public use.
    /// Only COSParser should call this method.
    /// </summary>
    /// <param name="dictionary">the corresponding dictionary</param>
    /// <param name="startPosition">the start position within the source</param>
    /// <param name="streamLength">the stream length</param>
    /// <returns>the new COSStream</returns>
    /// <exception cref="IOException">if the random access view can't be read</exception>
    public COSStream CreateCOSStream(COSDictionary dictionary, long startPosition,
            long streamLength)
    {
        COSStream stream = new COSStream(_streamCache,
                _parser!.CreateRandomAccessReadView(startPosition, streamLength));
        foreach (var entry in dictionary.EntrySet())
        {
            stream.SetItem(entry.Key, entry.Value);
        }
        stream.SetKey(dictionary.GetKey());
        return stream;
    }

    /// <summary>
    /// Get the dictionary containing the linearization information if the pdf is linearized.
    /// </summary>
    /// <returns>the dictionary containing the linearization information</returns>
    public COSDictionary? GetLinearizedDictionary()
    {
        // get all keys with a positive offset in ascending order, as the linearization dictionary shall be the first
        // within the pdf
        var objectKeys = _xrefTable
                .Where(e => e.Value > 0L)
                .OrderBy(e => e.Value)
                .Select(e => e.Key)
                .ToList();
        foreach (COSObjectKey objectKey in objectKeys)
        {
            COSObject? objectFromPool = GetObjectFromPool(objectKey);
            if (objectFromPool != null)
            {
                COSBase? realObject = objectFromPool.GetObject();
                if (realObject is COSDictionary dic)
                {
                    if (dic.GetItem(COSName.GetPDFName("Linearized")) != null)
                    {
                        return dic;
                    }
                }
            }
        }
        return null;
    }

    /// <summary>
    /// This will get all dictionaries objects by type.
    /// </summary>
    /// <param name="type">The type of the object.</param>
    /// <returns>This will return all objects with the specified type.</returns>
    public List<COSObject> GetObjectsByType(COSName type)
    {
        return GetObjectsByType(type, null);
    }

    /// <summary>
    /// This will get all dictionaries objects by type.
    /// </summary>
    /// <param name="type1">The first possible type of the object, mandatory.</param>
    /// <param name="type2">The second possible type of the object, usually an abbreviation, optional.</param>
    /// <returns>This will return all objects with the specified type(s).</returns>
    public List<COSObject> GetObjectsByType(COSName type1, COSName? type2)
    {
        List<COSObjectKey> originKeys = new List<COSObjectKey>(_xrefTable.Keys);
        List<COSObject> retval = GetObjectsByType(originKeys, type1, type2);
        // there might be some additional objects if the brute force parser was triggered
        // due to a broken cross reference table/stream
        if (originKeys.Count < _xrefTable.Count)
        {
            List<COSObjectKey> additionalKeys = new List<COSObjectKey>(_xrefTable.Keys);
            additionalKeys.RemoveAll(k => originKeys.Contains(k));
            retval.AddRange(GetObjectsByType(additionalKeys, type1, type2));
        }
        return retval;
    }

    private List<COSObject> GetObjectsByType(List<COSObjectKey> keys, COSName type1, COSName? type2)
    {
        List<COSObject> retval = new List<COSObject>();
        foreach (COSObjectKey objectKey in keys)
        {
            COSObject? objectFromPool = GetObjectFromPool(objectKey);
            if (objectFromPool != null)
            {
                COSBase? realObject = objectFromPool.GetObject();
                if (realObject is COSDictionary dic)
                {
                    COSName? dictType = dic.GetCOSName(COSName.TYPE);
                    if (type1.Equals(dictType) || (type2 != null && type2.Equals(dictType)))
                    {
                        retval.Add(objectFromPool);
                    }
                }
            }
        }
        return retval;
    }

    /// <summary>
    /// This will set the header version of this PDF document.
    /// </summary>
    /// <param name="versionValue">The version of the PDF document.</param>
    public void SetVersion(float versionValue)
    {
        _version = versionValue;
    }

    /// <summary>
    /// This will get the version extracted from the header of this PDF document.
    /// </summary>
    /// <returns>The header version.</returns>
    public float GetVersion()
    {
        return _version;
    }

    /// <summary>
    /// Signals that the document is decrypted completely.
    /// </summary>
    public void SetDecrypted()
    {
        _isDecrypted = true;
    }

    /// <summary>
    /// Indicates if a encrypted pdf is already decrypted after parsing.
    /// </summary>
    /// <returns>true indicates that the pdf is decrypted.</returns>
    public bool IsDecrypted()
    {
        return _isDecrypted;
    }

    /// <summary>
    /// This will tell if this is an encrypted document.
    /// </summary>
    /// <returns>true If this document is encrypted.</returns>
    public bool IsEncrypted()
    {
        return _trailer != null && _trailer.GetCOSDictionary(COSName.GetPDFName("Encrypt")) != null;
    }

    /// <summary>
    /// This will get the encryption dictionary if the document is encrypted or null if the document
    /// is not encrypted.
    /// </summary>
    /// <returns>The encryption dictionary.</returns>
    public COSDictionary? GetEncryptionDictionary()
    {
        return _trailer?.GetCOSDictionary(COSName.GetPDFName("Encrypt"));
    }

    /// <summary>
    /// This will set the encryption dictionary, this should only be called when
    /// encrypting the document.
    /// </summary>
    /// <param name="encDictionary">The encryption dictionary.</param>
    public void SetEncryptionDictionary(COSDictionary? encDictionary)
    {
        _trailer?.SetItem(COSName.GetPDFName("Encrypt"), encDictionary);
    }

    /// <summary>
    /// This will get the document ID.
    /// </summary>
    /// <returns>The document id.</returns>
    public COSArray? GetDocumentID()
    {
        return GetTrailer()?.GetCOSArray(COSName.GetPDFName("ID"));
    }

    /// <summary>
    /// This will set the document ID. This should be an array of two strings. This method cannot be
    /// used to remove the document id by passing null or an empty array; it will be recreated. Only
    /// the first existing string is used when writing, the second one is always recreated. If you
    /// don't want this, you'll have to modify the <c>COSWriter</c> class, look for
    /// <see cref="COSName.GetPDFName("ID")"/>.
    /// </summary>
    /// <param name="id">The document id.</param>
    public void SetDocumentID(COSArray id)
    {
        GetTrailer()?.SetItem(COSName.GetPDFName("ID"), id);
    }

    /// <summary>
    /// This will get the document trailer.
    /// </summary>
    /// <returns>the document trailer dict</returns>
    public COSDictionary? GetTrailer()
    {
        return _trailer;
    }

    /// <summary>
    /// This will set the document trailer.
    /// </summary>
    /// <param name="newTrailer">the document trailer dictionary</param>
    public void SetTrailer(COSDictionary newTrailer)
    {
        _trailer = newTrailer;
        _trailer.GetUpdateState().SetOriginDocumentState(_documentState);
    }

    /// <summary>
    /// Internal PDFBox use only. Get the object number of the highest XRef stream. This is needed to
    /// avoid reusing such a number in incremental saving.
    /// </summary>
    /// <returns>The object number of the highest XRef stream, or 0 if there was no XRef stream.</returns>
    public long GetHighestXRefObjectNumber()
    {
        return _highestXRefObjectNumber;
    }

    /// <summary>
    /// Internal PDFBox use only. Sets the object number of the highest XRef stream. This is needed
    /// to avoid reusing such a number in incremental saving.
    /// </summary>
    /// <param name="highestXRefObjectNumber">The object number of the highest XRef stream.</param>
    public void SetHighestXRefObjectNumber(long highestXRefObjectNumber)
    {
        _highestXRefObjectNumber = highestXRefObjectNumber;
    }

    /// <summary>
    /// visitor pattern double dispatch method.
    /// </summary>
    /// <param name="visitor">The object to notify when visiting this object.</param>
    /// <exception cref="IOException">If an error occurs while visiting this object.</exception>
    public override void Accept(ICOSVisitor visitor)
    {
        visitor.VisitFromDocument(this);
    }

    /// <summary>
    /// This will close all storage and delete the tmp files.
    /// </summary>
    public void Dispose()
    {
        if (_closed)
        {
            return;
        }

        // Make sure that:
        // - first Exception is kept
        // - all COSStreams are closed
        // - stream cache is closed
        // - there's a way to see which errors occurred
        IOException? firstException = null;

        // close all open I/O streams
        foreach (COSObject obj in _objectPool.Values)
        {
            if (!obj.IsObjectNull())
            {
                COSBase? cosObject = obj.GetObject();
                if (cosObject is COSStream cosStream)
                {
                    firstException = IOUtils.CloseAndLogException(cosStream, null, "COSStream", firstException);
                }
            }
        }

        foreach (COSStream stream in _streams)
        {
            firstException = IOUtils.CloseAndLogException(stream, null, "COSStream", firstException);
        }

        if (_streamCache != null)
        {
            firstException = IOUtils.CloseAndLogException(_streamCache, null, "Stream Cache", firstException);
        }
        _closed = true;

        // rethrow first exception to keep method contract
        if (firstException != null)
        {
            throw firstException;
        }
    }

    /// <summary>
    /// Returns true if this document has been closed.
    /// </summary>
    /// <returns>true if the document is already closed, false otherwise</returns>
    public bool IsClosed()
    {
        return _closed;
    }

    /// <summary>
    /// This will get an object from the pool.
    /// </summary>
    /// <param name="key">The object key.</param>
    /// <returns>The object in the pool or a new one if it has not been parsed yet.</returns>
    public COSObject? GetObjectFromPool(COSObjectKey? key)
    {
        COSObject? obj = null;
        if (key != null)
        {
            // make "proxy" object if this was a forward reference
            if (!_objectPool.TryGetValue(key, out obj))
            {
                obj = new COSObject(key);
                _objectPool[key] = obj;
            }
        }
        return obj;
    }

    /// <summary>
    /// Populate XRef HashMap with given values.
    /// Each entry maps ObjectKeys to byte offsets in the file.
    /// </summary>
    /// <param name="xrefTableValues">xref table entries to be added</param>
    public void AddXRefTable(Dictionary<COSObjectKey, long> xrefTableValues)
    {
        foreach (var kvp in xrefTableValues)
        {
            _xrefTable[kvp.Key] = kvp.Value;
        }
    }

    /// <summary>
    /// Returns the xrefTable which is a mapping of ObjectKeys to byte offsets in the file.
    /// </summary>
    /// <returns>mapping of ObjectsKeys to byte offsets</returns>
    public Dictionary<COSObjectKey, long> GetXrefTable()
    {
        return _xrefTable;
    }

    /// <summary>
    /// This method set the startxref value of the document. This will only
    /// be needed for incremental updates.
    /// </summary>
    /// <param name="startXrefValue">the value for startXref</param>
    public void SetStartXref(long startXrefValue)
    {
        _startXref = startXrefValue;
    }

    /// <summary>
    /// Return the startXref Position of the parsed document. This will only be needed for incremental updates.
    /// </summary>
    /// <returns>a long with the old position of the startxref</returns>
    public long GetStartXref()
    {
        return _startXref;
    }

    /// <summary>
    /// Determines if the trailer is a XRef stream or not.
    /// </summary>
    /// <returns>true if the trailer is a XRef stream</returns>
    public bool IsXRefStream()
    {
        return _isXRefStream;
    }

    /// <summary>
    /// Sets isXRefStream to the given value. You need to take care that the version of your PDF is
    /// 1.5 or higher.
    /// </summary>
    /// <param name="isXRefStreamValue">the new value for isXRefStream</param>
    public void SetIsXRefStream(bool isXRefStreamValue)
    {
        _isXRefStream = isXRefStreamValue;
    }

    /// <summary>
    /// Determines if the pdf has hybrid cross references, both plain tables and streams.
    /// </summary>
    /// <returns>true if the pdf has hybrid cross references</returns>
    public bool HasHybridXRef()
    {
        return _hasHybridXRef;
    }

    /// <summary>
    /// Marks the pdf as document using hybrid cross references.
    /// </summary>
    public void SetHasHybridXRef()
    {
        _hasHybridXRef = true;
    }

    /// <summary>
    /// Returns the <see cref="COSDocumentState"/> of this <see cref="COSDocument"/>.
    /// </summary>
    /// <returns>The <see cref="COSDocumentState"/> of this <see cref="COSDocument"/>.</returns>
    /// <seealso cref="COSDocumentState"/>
    public COSDocumentState GetDocumentState()
    {
        return _documentState;
    }
}
