/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache FontBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/CmapTable.java
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

public sealed class CmapTable() : TTFTable("cmap")
{
    public const int PlatformUnicode = 0;
    public const int PlatformMacintosh = 1;
    public const int PlatformWindows = 3;

    public const int EncodingMacRoman = 0;
    public const int EncodingWinUnicodeBmp = 1;
    public const int EncodingWinUnicodeFull = 10;
    public const int EncodingUnicode10 = 0;
    public const int EncodingUnicode11 = 1;
    public const int EncodingUnicode20Bmp = 3;
    public const int EncodingUnicode20Full = 4;

    private readonly List<CmapSubtable> _cmaps = [];

    public IReadOnlyList<CmapSubtable> Cmaps => _cmaps;

    internal override void Read(TrueTypeFont font, TTFDataStream dataStream)
    {
        _ = dataStream.ReadUnsignedShort();
        ushort numberOfTables = dataStream.ReadUnsignedShort();
        _cmaps.Clear();
        for (int i = 0; i < numberOfTables; i++)
        {
            CmapSubtable cmap = new();
            cmap.InitData(dataStream);
            _cmaps.Add(cmap);
        }

        int numberOfGlyphs = font.GetNumberOfGlyphs();
        foreach (CmapSubtable cmap in _cmaps)
        {
            cmap.InitSubtable(this, numberOfGlyphs, dataStream);
        }
    }

    public CmapSubtable? GetSubtable(int platformId, int platformEncodingId)
    {
        return _cmaps.FirstOrDefault(cmap =>
            cmap.PlatformId == platformId &&
            cmap.PlatformEncodingId == platformEncodingId);
    }
}
