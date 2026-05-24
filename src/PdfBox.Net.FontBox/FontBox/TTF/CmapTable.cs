/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache FontBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/CmapTable.java
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

namespace PdfBox.Net.FontBox.TTF;

public sealed class CmapTable() : TTFTable("cmap")
{
    public const int PlatformUnicode = 0;
    public const int PlatformMacintosh = 1;
    public const int PlatformWindows = 3;

    public const int EncodingMacRoman = 0;

    public const int EncodingWinSymbol = 0;
    public const int EncodingWinUnicodeBmp = 1;
    public const int EncodingWinShiftJis = 2;
    public const int EncodingWinBig5 = 3;
    public const int EncodingWinPrc = 4;
    public const int EncodingWinWansung = 5;
    public const int EncodingWinJohab = 6;
    public const int EncodingWinUnicodeFull = 10;

    public const int EncodingUnicode10 = 0;
    public const int EncodingUnicode11 = 1;
    public const int EncodingUnicode20Bmp = 3;
    public const int EncodingUnicode20Full = 4;

    private CmapSubtable[]? _cmaps;

    internal override void Read(TrueTypeFont font, TTFDataStream data)
    {
        _ = data.ReadUnsignedShort();
        int numberOfTables = data.ReadUnsignedShort();
        _cmaps = new CmapSubtable[numberOfTables];
        for (int i = 0; i < numberOfTables; i++)
        {
            CmapSubtable cmap = new();
            cmap.InitData(data);
            _cmaps[i] = cmap;
        }

        int numberOfGlyphs = font.GetNumberOfGlyphs();
        for (int i = 0; i < numberOfTables; i++)
        {
            _cmaps[i].InitSubtable(this, numberOfGlyphs, data);
        }

        Initialized = true;
    }

    public CmapSubtable[]? GetCmaps() => _cmaps is null ? null : [.. _cmaps];

    public void SetCmaps(CmapSubtable[]? cmaps) => _cmaps = cmaps;

    public CmapSubtable? GetSubtable(int platformId, int platformEncodingId)
    {
        if (_cmaps is null)
        {
            return null;
        }

        foreach (CmapSubtable cmap in _cmaps)
        {
            if (cmap.PlatformId == platformId && cmap.PlatformEncodingId == platformEncodingId)
            {
                return cmap;
            }
        }

        return null;
    }
}
