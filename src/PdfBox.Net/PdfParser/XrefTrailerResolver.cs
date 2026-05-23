/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdfparser/XrefTrailerResolver.java
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

namespace PdfBox.Net.PdfParser;

/// <summary>
/// Collects xref/trailer sections and resolves the active chain based on startxref/Prev pointers.
/// </summary>
public class XrefTrailerResolver
{
    /// <summary>
    /// The XRefType of a trailer.
    /// </summary>
    public enum XRefType
    {
        /// <summary>
        /// XRef table type.
        /// </summary>
        TABLE,

        /// <summary>
        /// XRef stream type.
        /// </summary>
        STREAM
    }

    /// <summary>
    /// A class which represents a xref/trailer object.
    /// </summary>
    private sealed class XrefTrailerObj
    {
        public COSDictionary? Trailer;

        public XRefType XrefType = XRefType.TABLE;

        public readonly Dictionary<COSObjectKey, long> XrefTable = [];

        public void Reset()
        {
            XrefTable.Clear();
        }
    }

    private readonly Dictionary<long, XrefTrailerObj> _bytePosToXrefMap = [];
    private XrefTrailerObj? _curXrefTrailerObj;
    private XrefTrailerObj? _resolvedXrefTrailer;

    /// <summary>
    /// Returns the first trailer if at least one exists.
    /// </summary>
    /// <returns>The first trailer or null.</returns>
    public COSDictionary? GetFirstTrailer()
    {
        if (_bytePosToXrefMap.Count == 0)
        {
            return null;
        }

        long first = _bytePosToXrefMap.Keys.Min();
        return _bytePosToXrefMap[first].Trailer;
    }

    /// <summary>
    /// Returns the last trailer if at least one exists.
    /// </summary>
    /// <returns>The last trailer or null.</returns>
    public COSDictionary? GetLastTrailer()
    {
        if (_bytePosToXrefMap.Count == 0)
        {
            return null;
        }

        long last = _bytePosToXrefMap.Keys.Max();
        return _bytePosToXrefMap[last].Trailer;
    }

    /// <summary>
    /// Returns the count of trailers.
    /// </summary>
    /// <returns>The count of trailers.</returns>
    public int GetTrailerCount()
    {
        return _bytePosToXrefMap.Count;
    }

    /// <summary>
    /// Signals that a new XRef object (table or stream) starts.
    /// </summary>
    /// <param name="startBytePos">The offset to start at.</param>
    /// <param name="type">The type of the XRef object.</param>
    public void NextXrefObj(long startBytePos, XRefType type)
    {
        _curXrefTrailerObj = new XrefTrailerObj
        {
            XrefType = type
        };
        _bytePosToXrefMap[startBytePos] = _curXrefTrailerObj;
    }

    /// <summary>
    /// Returns the XRefType of the resolved trailer.
    /// </summary>
    /// <returns>The XRefType or null.</returns>
    public XRefType? GetXrefType()
    {
        return _resolvedXrefTrailer?.XrefType;
    }

    /// <summary>
    /// Populates the xref table map of current xref object.
    /// </summary>
    /// <param name="objKey">The object key with number and generation.</param>
    /// <param name="offset">The byte offset in file.</param>
    public void SetXRef(COSObjectKey objKey, long offset)
    {
        if (_curXrefTrailerObj is null)
        {
            return;
        }

        if (!_curXrefTrailerObj.XrefTable.ContainsKey(objKey))
        {
            _curXrefTrailerObj.XrefTable[objKey] = offset;
        }
    }

    /// <summary>
    /// Adds trailer information for current xref object.
    /// </summary>
    /// <param name="trailer">The current document trailer dictionary.</param>
    public void SetTrailer(COSDictionary trailer)
    {
        if (_curXrefTrailerObj is null)
        {
            return;
        }

        _curXrefTrailerObj.Trailer = trailer;
    }

    /// <summary>
    /// Returns the trailer last set by <see cref="SetTrailer(COSDictionary)"/>.
    /// </summary>
    /// <returns>The current trailer.</returns>
    public COSDictionary? GetCurrentTrailer()
    {
        return _curXrefTrailerObj?.Trailer;
    }

    /// <summary>
    /// Sets the byte position of the first xref (last read startxref value).
    /// Used to resolve the chain of active xref/trailer sections.
    /// </summary>
    /// <param name="startxrefBytePosValue">Starting position of the first xref.</param>
    public void SetStartxref(long startxrefBytePosValue)
    {
        if (_resolvedXrefTrailer is not null)
        {
            return;
        }

        _resolvedXrefTrailer = new XrefTrailerObj
        {
            Trailer = new COSDictionary()
        };

        XrefTrailerObj? curObj = _bytePosToXrefMap.GetValueOrDefault(startxrefBytePosValue);
        List<long> xrefSeqBytePos = [];

        if (curObj is null)
        {
            xrefSeqBytePos.AddRange(_bytePosToXrefMap.Keys);
            xrefSeqBytePos.Sort();
        }
        else
        {
            _resolvedXrefTrailer.XrefType = curObj.XrefType;
            xrefSeqBytePos.Add(startxrefBytePosValue);
            while (curObj.Trailer is not null)
            {
                long prevBytePos = curObj.Trailer.GetLong(COSName.GetPDFName("Prev"), -1L);
                if (prevBytePos == -1)
                {
                    break;
                }

                curObj = _bytePosToXrefMap.GetValueOrDefault(prevBytePos);
                if (curObj is null)
                {
                    break;
                }

                xrefSeqBytePos.Add(prevBytePos);
                if (xrefSeqBytePos.Count >= _bytePosToXrefMap.Count)
                {
                    break;
                }
            }

            xrefSeqBytePos.Reverse();
        }

        foreach (long bPos in xrefSeqBytePos)
        {
            curObj = _bytePosToXrefMap.GetValueOrDefault(bPos);
            if (curObj is null)
            {
                continue;
            }

            if (curObj.Trailer is not null)
            {
                _resolvedXrefTrailer.Trailer!.AddAll(curObj.Trailer);
            }

            foreach ((COSObjectKey key, long value) in curObj.XrefTable)
            {
                _resolvedXrefTrailer.XrefTable[key] = value;
            }
        }
    }

    /// <summary>
    /// Gets the resolved trailer. Returns null if startxref wasn't resolved yet.
    /// </summary>
    /// <returns>The resolved trailer if available.</returns>
    public COSDictionary? GetTrailer()
    {
        return _resolvedXrefTrailer?.Trailer;
    }

    /// <summary>
    /// Gets the resolved xref table. Returns null if startxref wasn't resolved yet.
    /// </summary>
    /// <returns>The resolved xref table if available.</returns>
    public Dictionary<COSObjectKey, long>? GetXrefTable()
    {
        return _resolvedXrefTrailer?.XrefTable;
    }

    /// <summary>
    /// Returns object numbers referenced as contained in the given object stream.
    /// </summary>
    /// <param name="objstmObjNr">Object number of object stream.</param>
    /// <returns>
    /// Set of referenced object numbers for the given object stream,
    /// or null if startxref wasn't resolved yet.
    /// </returns>
    public HashSet<long>? GetContainedObjectNumbers(int objstmObjNr)
    {
        if (_resolvedXrefTrailer is null)
        {
            return null;
        }

        HashSet<long> refObjNrs = [];
        long cmpVal = -objstmObjNr;

        foreach ((COSObjectKey key, long value) in _resolvedXrefTrailer.XrefTable)
        {
            if (value == cmpVal)
            {
                refObjNrs.Add(key.GetNumber());
            }
        }

        return refObjNrs;
    }

    /// <summary>
    /// Resets all data so that this resolver can be reused.
    /// </summary>
    protected internal void Reset()
    {
        foreach (XrefTrailerObj trailerObj in _bytePosToXrefMap.Values)
        {
            trailerObj.Reset();
        }

        _curXrefTrailerObj = null;
        _resolvedXrefTrailer = null;
    }
}
