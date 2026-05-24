/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache FontBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/CmapSubtable.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: trunk
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

using System;
using System.Collections.Generic;
using System.IO;

namespace PdfBox.Net.FontBox.TTF;

public class CmapSubtable : CmapLookup
{
    private const long LeadOffset = 0xD800L - (0x10000 >> 10);
    private const long SurrogateOffset = 0x10000L - (0xD800 << 10) - 0xDC00;

    private int _platformId;
    private int _platformEncodingId;
    private long _subTableOffset;
    private int[]? _glyphIdToCharacterCode;
    private readonly Dictionary<int, List<int>> _glyphIdToCharacterCodeMultiple = [];
    private Dictionary<int, int> _characterCodeToGlyphId = [];

    public int PlatformId
    {
        get => _platformId;
        set => _platformId = value;
    }

    public int PlatformEncodingId
    {
        get => _platformEncodingId;
        set => _platformEncodingId = value;
    }

    internal void InitData(TTFDataStream data)
    {
        _platformId = data.ReadUnsignedShort();
        _platformEncodingId = data.ReadUnsignedShort();
        _subTableOffset = data.ReadUnsignedInt();
    }

    internal void InitSubtable(CmapTable cmap, int numGlyphs, TTFDataStream data)
    {
        data.Seek(cmap.Offset + _subTableOffset);
        int subtableFormat = data.ReadUnsignedShort();
        if (subtableFormat < 8)
        {
            _ = data.ReadUnsignedShort();
            _ = data.ReadUnsignedShort();
        }
        else
        {
            _ = data.ReadUnsignedShort();
            _ = data.ReadUnsignedInt();
            _ = data.ReadUnsignedInt();
        }

        switch (subtableFormat)
        {
            case 0:
                ProcessSubtype0(data);
                break;
            case 2:
                ProcessSubtype2(data, numGlyphs);
                break;
            case 4:
                ProcessSubtype4(data, numGlyphs);
                break;
            case 6:
                ProcessSubtype6(data, numGlyphs);
                break;
            case 8:
                ProcessSubtype8(data, numGlyphs);
                break;
            case 10:
                ProcessSubtype10(data, numGlyphs);
                break;
            case 12:
                ProcessSubtype12(data, numGlyphs);
                break;
            case 13:
                ProcessSubtype13(data, numGlyphs);
                break;
            case 14:
                ProcessSubtype14(data, numGlyphs);
                break;
            default:
                throw new IOException($"Unknown cmap format:{subtableFormat}");
        }
    }

    internal void ProcessSubtype8(TTFDataStream data, int numGlyphs)
    {
        int[] is32 = data.ReadUnsignedByteArray(8192);
        long nbGroups = data.ReadUnsignedInt();
        if (nbGroups > 65536)
        {
            throw new IOException("CMap ( Subtype8 ) is invalid");
        }

        _glyphIdToCharacterCode = NewGlyphIdToCharacterCode(numGlyphs);
        _characterCodeToGlyphId = new Dictionary<int, int>(numGlyphs);
        if (numGlyphs == 0)
        {
            Console.Error.WriteLine("subtable has no glyphs");
            return;
        }

        for (long i = 0; i < nbGroups; ++i)
        {
            long firstCode = data.ReadUnsignedInt();
            long endCode = data.ReadUnsignedInt();
            long startGlyph = data.ReadUnsignedInt();
            if (firstCode > endCode || firstCode < 0)
            {
                throw new IOException("Range invalid");
            }

            for (long j = firstCode; j <= endCode; ++j)
            {
                if (j > int.MaxValue || (int)j / 8 >= is32.Length)
                {
                    throw new IOException($"[Sub Format 8] Invalid character code {j}");
                }

                int currentCharCode;
                if ((is32[(int)j / 8] & (1 << ((int)j % 8))) == 0)
                {
                    currentCharCode = (int)j;
                }
                else
                {
                    long lead = LeadOffset + (j >> 10);
                    long trail = 0xDC00 + (j & 0x3FF);
                    long codepoint = (lead << 10) + trail + SurrogateOffset;
                    if (codepoint > int.MaxValue)
                    {
                        throw new IOException($"[Sub Format 8] Invalid character code {codepoint}");
                    }

                    currentCharCode = (int)codepoint;
                }

                long glyphIndex = startGlyph + (j - firstCode);
                if (glyphIndex > numGlyphs || glyphIndex > int.MaxValue)
                {
                    throw new IOException("CMap contains an invalid glyph index");
                }

                _glyphIdToCharacterCode[(int)glyphIndex] = currentCharCode;
                _characterCodeToGlyphId[currentCharCode] = (int)glyphIndex;
            }
        }
    }

    internal void ProcessSubtype10(TTFDataStream data, int numGlyphs)
    {
        long startCode = data.ReadUnsignedInt();
        long numChars = data.ReadUnsignedInt();
        if (numChars > int.MaxValue)
        {
            throw new IOException("Invalid number of Characters");
        }

        if (startCode < 0 || startCode > 0x0010FFFF || (startCode + numChars) > 0x0010FFFF ||
            ((startCode + numChars) >= 0x0000D800 && (startCode + numChars) <= 0x0000DFFF))
        {
            throw new IOException($"Invalid character codes, startCode: 0x{startCode:X}, numChars: {numChars}");
        }
    }

    internal void ProcessSubtype12(TTFDataStream data, int numGlyphs)
    {
        int maxGlyphId = 0;
        long nbGroups = data.ReadUnsignedInt();
        _glyphIdToCharacterCode = NewGlyphIdToCharacterCode(numGlyphs);
        _characterCodeToGlyphId = new Dictionary<int, int>(numGlyphs);
        if (numGlyphs == 0)
        {
            Console.Error.WriteLine("subtable has no glyphs");
            return;
        }

        for (long i = 0; i < nbGroups; ++i)
        {
            long firstCode = data.ReadUnsignedInt();
            long endCode = data.ReadUnsignedInt();
            long startGlyph = data.ReadUnsignedInt();

            if (firstCode < 0 || firstCode > 0x0010FFFF || (firstCode >= 0x0000D800 && firstCode <= 0x0000DFFF))
            {
                throw new IOException($"Invalid character code 0x{firstCode:X}");
            }

            if ((endCode > 0 && endCode < firstCode) || endCode > 0x0010FFFF ||
                (endCode >= 0x0000D800 && endCode <= 0x0000DFFF))
            {
                throw new IOException($"Invalid character code 0x{endCode:X}");
            }

            for (long j = 0; j <= endCode - firstCode; ++j)
            {
                long glyphIndex = startGlyph + j;
                if (glyphIndex >= numGlyphs)
                {
                    Console.Error.WriteLine("Format 12 cmap contains an invalid glyph index");
                    break;
                }

                if (firstCode + j > 0x10FFFF)
                {
                    Console.Error.WriteLine("Format 12 cmap contains character beyond UCS-4");
                }

                maxGlyphId = Math.Max(maxGlyphId, (int)glyphIndex);
                _characterCodeToGlyphId[(int)(firstCode + j)] = (int)glyphIndex;
            }
        }

        BuildGlyphIdToCharacterCodeLookup(maxGlyphId);
    }

    internal void ProcessSubtype13(TTFDataStream data, int numGlyphs)
    {
        long nbGroups = data.ReadUnsignedInt();
        _glyphIdToCharacterCode = NewGlyphIdToCharacterCode(numGlyphs);
        _characterCodeToGlyphId = new Dictionary<int, int>(numGlyphs);
        if (numGlyphs == 0)
        {
            Console.Error.WriteLine("subtable has no glyphs");
            return;
        }

        for (long i = 0; i < nbGroups; ++i)
        {
            long firstCode = data.ReadUnsignedInt();
            long endCode = data.ReadUnsignedInt();
            long glyphId = data.ReadUnsignedInt();

            if (glyphId > numGlyphs)
            {
                Console.Error.WriteLine("Format 13 cmap contains an invalid glyph index");
                break;
            }

            if (firstCode < 0 || firstCode > 0x0010FFFF || (firstCode >= 0x0000D800 && firstCode <= 0x0000DFFF))
            {
                throw new IOException($"Invalid character code 0x{firstCode:X}");
            }

            if ((endCode > 0 && endCode < firstCode) || endCode > 0x0010FFFF ||
                (endCode >= 0x0000D800 && endCode <= 0x0000DFFF))
            {
                throw new IOException($"Invalid character code 0x{endCode:X}");
            }

            for (long j = 0; j <= endCode - firstCode; ++j)
            {
                if (firstCode + j > int.MaxValue)
                {
                    throw new IOException("Character Code greater than Integer.MAX_VALUE");
                }

                if (firstCode + j > 0x10FFFF)
                {
                    Console.Error.WriteLine("Format 13 cmap contains character beyond UCS-4");
                }

                _glyphIdToCharacterCode[(int)glyphId] = (int)(firstCode + j);
                _characterCodeToGlyphId[(int)(firstCode + j)] = (int)glyphId;
            }
        }
    }

    internal void ProcessSubtype14(TTFDataStream data, int numGlyphs)
    {
        Console.Error.WriteLine("Format 14 cmap table is not supported and will be ignored");
    }

    internal void ProcessSubtype6(TTFDataStream data, int numGlyphs)
    {
        int firstCode = data.ReadUnsignedShort();
        int entryCount = data.ReadUnsignedShort();
        if (entryCount == 0)
        {
            return;
        }

        _characterCodeToGlyphId = new Dictionary<int, int>(numGlyphs);
        int[] glyphIdArray = data.ReadUnsignedShortArray(entryCount);
        int maxGlyphId = 0;
        for (int i = 0; i < entryCount; i++)
        {
            maxGlyphId = Math.Max(maxGlyphId, glyphIdArray[i]);
            _characterCodeToGlyphId[firstCode + i] = glyphIdArray[i];
        }

        BuildGlyphIdToCharacterCodeLookup(maxGlyphId);
    }

    internal void ProcessSubtype4(TTFDataStream data, int numGlyphs)
    {
        int segCountX2 = data.ReadUnsignedShort();
        int segCount = segCountX2 / 2;
        _ = data.ReadUnsignedShort();
        _ = data.ReadUnsignedShort();
        _ = data.ReadUnsignedShort();
        int[] endCount = data.ReadUnsignedShortArray(segCount);
        _ = data.ReadUnsignedShort();
        int[] startCount = data.ReadUnsignedShortArray(segCount);
        int[] idDelta = data.ReadUnsignedShortArray(segCount);
        long idRangeOffsetPosition = data.GetCurrentPosition();
        int[] idRangeOffset = data.ReadUnsignedShortArray(segCount);

        _characterCodeToGlyphId = new Dictionary<int, int>(numGlyphs);
        int maxGlyphId = 0;
        for (int i = 0; i < segCount; i++)
        {
            int start = startCount[i];
            int end = endCount[i];
            if (start != 65535 && end != 65535)
            {
                int delta = idDelta[i];
                int rangeOffset = idRangeOffset[i];
                long segmentRangeOffset = idRangeOffsetPosition + (i * 2L) + rangeOffset;
                for (int j = start; j <= end; j++)
                {
                    if (rangeOffset == 0)
                    {
                        int glyphId = (j + delta) & 0xFFFF;
                        maxGlyphId = Math.Max(glyphId, maxGlyphId);
                        _characterCodeToGlyphId[j] = glyphId;
                    }
                    else
                    {
                        long glyphOffset = segmentRangeOffset + ((j - start) * 2L);
                        data.Seek(glyphOffset);
                        int glyphIndex = data.ReadUnsignedShort();
                        if (glyphIndex != 0)
                        {
                            glyphIndex = (glyphIndex + delta) & 0xFFFF;
                            maxGlyphId = Math.Max(glyphIndex, maxGlyphId);
                            _characterCodeToGlyphId[j] = glyphIndex;
                        }
                    }
                }
            }
        }

        if (_characterCodeToGlyphId.Count == 0)
        {
            Console.Error.WriteLine("cmap format 4 subtable is empty");
            return;
        }

        BuildGlyphIdToCharacterCodeLookup(maxGlyphId);
    }

    private void BuildGlyphIdToCharacterCodeLookup(int maxGlyphId)
    {
        _glyphIdToCharacterCode = NewGlyphIdToCharacterCode(maxGlyphId + 1);
        foreach ((int key, int value) in _characterCodeToGlyphId)
        {
            if (_glyphIdToCharacterCode[value] == -1)
            {
                _glyphIdToCharacterCode[value] = key;
            }
            else
            {
                if (!_glyphIdToCharacterCodeMultiple.TryGetValue(value, out List<int>? mappedValues))
                {
                    mappedValues = new List<int>(2)
                    {
                        _glyphIdToCharacterCode[value]
                    };
                    _glyphIdToCharacterCodeMultiple[value] = mappedValues;
                    _glyphIdToCharacterCode[value] = int.MinValue;
                }

                mappedValues.Add(key);
            }
        }
    }

    internal void ProcessSubtype2(TTFDataStream data, int numGlyphs)
    {
        int[] subHeaderKeys = new int[256];
        int maxSubHeaderIndex = 0;
        for (int i = 0; i < 256; i++)
        {
            subHeaderKeys[i] = data.ReadUnsignedShort();
            maxSubHeaderIndex = Math.Max(maxSubHeaderIndex, subHeaderKeys[i] / 8);
        }

        SubHeader[] subHeaders = new SubHeader[maxSubHeaderIndex + 1];
        for (int i = 0; i <= maxSubHeaderIndex; ++i)
        {
            int firstCode = data.ReadUnsignedShort();
            int entryCount = data.ReadUnsignedShort();
            short idDelta = data.ReadSignedShort();
            int idRangeOffset = data.ReadUnsignedShort() - (maxSubHeaderIndex + 1 - i - 1) * 8 - 2;
            subHeaders[i] = new SubHeader(firstCode, entryCount, idDelta, idRangeOffset);
        }

        long startGlyphIndexOffset = data.GetCurrentPosition();
        _glyphIdToCharacterCode = NewGlyphIdToCharacterCode(numGlyphs);
        _characterCodeToGlyphId = new Dictionary<int, int>(numGlyphs);
        if (numGlyphs == 0)
        {
            Console.Error.WriteLine("subtable has no glyphs");
            return;
        }

        HashSet<int> logged = [];
        bool maxLoggingReached = false;
        for (int i = 0; i <= maxSubHeaderIndex; ++i)
        {
            SubHeader sh = subHeaders[i];
            int firstCode = sh.FirstCode;
            int idRangeOffset = sh.IdRangeOffset;
            int idDelta = sh.IdDelta;
            int entryCount = sh.EntryCount;
            data.Seek(startGlyphIndexOffset + idRangeOffset);
            for (int j = 0; j < entryCount; ++j)
            {
                int charCode = (i << 8) + (firstCode + j);
                int p = data.ReadUnsignedShort();
                if (p > 0)
                {
                    p = (p + idDelta) % 65536;
                    if (p < 0)
                    {
                        p += 65536;
                    }
                }

                if (p >= numGlyphs)
                {
                    if (!maxLoggingReached && !logged.Contains(p))
                    {
                        Console.Error.WriteLine($"glyphId {p} for charcode {charCode} ignored, numGlyphs is {numGlyphs}");
                        logged.Add(p);
                        if (logged.Count > 10)
                        {
                            Console.Error.WriteLine("too many bad glyphIds, more won't be reported for this table");
                            maxLoggingReached = true;
                        }
                    }

                    continue;
                }

                _glyphIdToCharacterCode[p] = charCode;
                _characterCodeToGlyphId[charCode] = p;
            }
        }
    }

    internal void ProcessSubtype0(TTFDataStream data)
    {
        byte[] glyphMapping = data.ReadBytes(256);
        _glyphIdToCharacterCode = NewGlyphIdToCharacterCode(256);
        _characterCodeToGlyphId = new Dictionary<int, int>(glyphMapping.Length);
        for (int i = 0; i < glyphMapping.Length; i++)
        {
            int glyphIndex = glyphMapping[i] & 0xFF;
            _glyphIdToCharacterCode[glyphIndex] = i;
            _characterCodeToGlyphId[i] = glyphIndex;
        }
    }

    public int GetGlyphId(int codePoint)
    {
        return _characterCodeToGlyphId.TryGetValue(codePoint, out int glyphId) ? glyphId : 0;
    }

    public List<int>? GetCharCodes(int gid)
    {
        int code = GetCharCode(gid);
        if (code == -1)
        {
            return null;
        }

        if (code == int.MinValue)
        {
            if (_glyphIdToCharacterCodeMultiple.TryGetValue(gid, out List<int>? mappedValues))
            {
                List<int> codes = new(mappedValues);
                codes.Sort();
                return codes;
            }

            return null;
        }

        return [code];
    }

    private int GetCharCode(int gid)
    {
        if (gid < 0 || _glyphIdToCharacterCode is null || gid >= _glyphIdToCharacterCode.Length)
        {
            return -1;
        }

        return _glyphIdToCharacterCode[gid];
    }

    private static int[] NewGlyphIdToCharacterCode(int size)
    {
        int[] gidToCode = new int[size];
        Array.Fill(gidToCode, -1);
        return gidToCode;
    }

    public override string ToString() => $"{{{PlatformId} {PlatformEncodingId}}}";

    private sealed class SubHeader(int firstCode, int entryCount, short idDelta, int idRangeOffset)
    {
        public int FirstCode { get; } = firstCode;
        public int EntryCount { get; } = entryCount;
        public short IdDelta { get; } = idDelta;
        public int IdRangeOffset { get; } = idRangeOffset;
    }
}
