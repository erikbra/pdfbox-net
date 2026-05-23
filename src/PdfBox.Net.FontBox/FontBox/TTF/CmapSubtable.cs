/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache FontBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/CmapSubtable.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: adapted
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

namespace PdfBox.Net.FontBox.TTF;

public sealed class CmapSubtable : CmapLookup
{
    private readonly Dictionary<int, int> _characterCodeToGlyphId = [];
    private readonly Dictionary<int, List<int>> _glyphIdToCharacterCodes = [];

    public ushort PlatformId { get; private set; }

    public ushort PlatformEncodingId { get; private set; }

    internal uint SubTableOffset { get; private set; }

    internal void InitData(TTFDataStream data)
    {
        PlatformId = data.ReadUnsignedShort();
        PlatformEncodingId = data.ReadUnsignedShort();
        SubTableOffset = data.ReadUnsignedInt();
    }

    internal void InitSubtable(CmapTable cmap, int numGlyphs, TTFDataStream data)
    {
        data.Seek(cmap.Offset + SubTableOffset);
        ushort format = data.ReadUnsignedShort();
        if (format < 8)
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

        switch (format)
        {
            case 0:
                ProcessSubtype0(data);
                break;
            case 4:
                ProcessSubtype4(data, numGlyphs);
                break;
            case 6:
                ProcessSubtype6(data, numGlyphs);
                break;
            case 12:
                ProcessSubtype12(data, numGlyphs);
                break;
        }
    }

    public int GetGlyphId(int codePoint)
    {
        return _characterCodeToGlyphId.TryGetValue(codePoint, out int glyphId) ? glyphId : 0;
    }

    public IReadOnlyList<int>? GetCharCodes(int gid)
    {
        if (!_glyphIdToCharacterCodes.TryGetValue(gid, out List<int>? codes))
        {
            return null;
        }

        return codes.Count <= 1 ? codes : codes.OrderBy(code => code).ToArray();
    }

    private void ProcessSubtype0(TTFDataStream data)
    {
        for (int code = 0; code < 256; code++)
        {
            RegisterMapping(code, data.ReadUnsignedByte());
        }
    }

    private void ProcessSubtype4(TTFDataStream data, int numGlyphs)
    {
        int segCount = data.ReadUnsignedShort() / 2;
        _ = data.ReadUnsignedShort();
        _ = data.ReadUnsignedShort();
        _ = data.ReadUnsignedShort();
        ushort[] endCount = data.ReadUnsignedShortArray(segCount);
        _ = data.ReadUnsignedShort();
        ushort[] startCount = data.ReadUnsignedShortArray(segCount);
        ushort[] idDelta = data.ReadUnsignedShortArray(segCount);
        long idRangeOffsetPosition = data.Position;
        ushort[] idRangeOffset = data.ReadUnsignedShortArray(segCount);

        for (int segment = 0; segment < segCount; segment++)
        {
            int start = startCount[segment];
            int end = endCount[segment];
            if (start == 0xFFFF && end == 0xFFFF)
            {
                continue;
            }

            int delta = unchecked((short)idDelta[segment]);
            int rangeOffset = idRangeOffset[segment];
            long segmentRangeOffset = idRangeOffsetPosition + segment * 2L + rangeOffset;
            for (int code = start; code <= end; code++)
            {
                int glyphId;
                if (rangeOffset == 0)
                {
                    glyphId = (code + delta) & 0xFFFF;
                }
                else
                {
                    data.Seek(segmentRangeOffset + (code - start) * 2L);
                    glyphId = data.ReadUnsignedShort();
                    if (glyphId != 0)
                    {
                        glyphId = (glyphId + delta) & 0xFFFF;
                    }
                }

                if (glyphId < numGlyphs)
                {
                    RegisterMapping(code, glyphId);
                }
            }
        }
    }

    private void ProcessSubtype6(TTFDataStream data, int numGlyphs)
    {
        int firstCode = data.ReadUnsignedShort();
        int entryCount = data.ReadUnsignedShort();
        for (int i = 0; i < entryCount; i++)
        {
            int glyphId = data.ReadUnsignedShort();
            if (glyphId < numGlyphs)
            {
                RegisterMapping(firstCode + i, glyphId);
            }
        }
    }

    private void ProcessSubtype12(TTFDataStream data, int numGlyphs)
    {
        uint groupCount = data.ReadUnsignedInt();
        for (uint group = 0; group < groupCount; group++)
        {
            uint firstCode = data.ReadUnsignedInt();
            uint endCode = data.ReadUnsignedInt();
            uint startGlyphId = data.ReadUnsignedInt();
            for (uint code = firstCode; code <= endCode; code++)
            {
                uint glyphId = startGlyphId + (code - firstCode);
                if (glyphId >= numGlyphs || code > int.MaxValue)
                {
                    break;
                }

                RegisterMapping((int)code, (int)glyphId);
            }
        }
    }

    private void RegisterMapping(int codePoint, int glyphId)
    {
        _characterCodeToGlyphId[codePoint] = glyphId;
        if (!_glyphIdToCharacterCodes.TryGetValue(glyphId, out List<int>? codes))
        {
            codes = [];
            _glyphIdToCharacterCodes[glyphId] = codes;
        }

        codes.Add(codePoint);
    }
}
