/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache FontBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/TrueTypeFont.java
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

public class TrueTypeFont
{
    private readonly Dictionary<string, TTFTable> _tables = new(StringComparer.Ordinal);

    public uint SfntVersion { get; internal set; }

    public ushort NumberOfTables { get; internal set; }

    public IReadOnlyDictionary<string, TTFTable> Tables => _tables;

    internal void AddTable(TTFTable table)
    {
        _tables[table.Tag] = table;
    }

    public TTFTable? GetTable(string tag)
    {
        return _tables.TryGetValue(tag, out TTFTable? table) ? table : null;
    }

    public HeaderTable? GetHeader()
    {
        return GetTable("head") as HeaderTable;
    }

    public MaximumProfileTable? GetMaximumProfile()
    {
        return GetTable("maxp") as MaximumProfileTable;
    }

    public int GetNumberOfGlyphs()
    {
        return GetMaximumProfile()?.NumGlyphs ?? 0;
    }

    public NamingTable? GetNaming()
    {
        return GetTable("name") as NamingTable;
    }

    public CmapTable? GetCmap()
    {
        return GetTable("cmap") as CmapTable;
    }

    public HorizontalHeaderTable? GetHorizontalHeader()
    {
        return GetTable("hhea") as HorizontalHeaderTable;
    }

    public HorizontalMetricsTable? GetHorizontalMetrics()
    {
        return GetTable("hmtx") as HorizontalMetricsTable;
    }

    public IndexToLocationTable? GetIndexToLocation()
    {
        return GetTable("loca") as IndexToLocationTable;
    }

    public GlyphTable? GetGlyphTable()
    {
        return GetTable("glyf") as GlyphTable;
    }

    public PostScriptTable? GetPostScript()
    {
        return GetTable("post") as PostScriptTable;
    }

    public int GetUnitsPerEm()
    {
        return GetHeader()?.UnitsPerEm ?? 1000;
    }

    public string? GetName()
    {
        NamingTable? naming = GetNaming();
        return naming?.GetEnglishName(4) ??
               naming?.GetEnglishName(6) ??
               naming?.GetEnglishName(1);
    }

    public CmapLookup? GetUnicodeCmapLookup()
    {
        CmapTable? cmap = GetCmap();
        return cmap?.GetSubtable(CmapTable.PlatformWindows, CmapTable.EncodingWinUnicodeFull) ??
               cmap?.GetSubtable(CmapTable.PlatformWindows, CmapTable.EncodingWinUnicodeBmp) ??
               cmap?.GetSubtable(CmapTable.PlatformUnicode, CmapTable.EncodingUnicode20Full) ??
               cmap?.GetSubtable(CmapTable.PlatformUnicode, CmapTable.EncodingUnicode20Bmp) ??
               cmap?.GetSubtable(CmapTable.PlatformUnicode, CmapTable.EncodingUnicode11) ??
               cmap?.GetSubtable(CmapTable.PlatformUnicode, CmapTable.EncodingUnicode10) ??
               cmap?.GetSubtable(CmapTable.PlatformMacintosh, CmapTable.EncodingMacRoman);
    }

    public int GetGlyphId(int codePoint)
    {
        return GetUnicodeCmapLookup()?.GetGlyphId(codePoint) ?? 0;
    }

    public GlyphData? GetGlyph(int gid)
    {
        return GetGlyphTable()?.GetGlyph(gid);
    }

    public int GetAdvanceWidth(int gid)
    {
        return GetHorizontalMetrics()?.GetAdvanceWidth(gid) ?? 0;
    }

    public int GetLeftSideBearing(int gid)
    {
        return GetHorizontalMetrics()?.GetLeftSideBearing(gid) ?? 0;
    }

    public string? GetName(int gid)
    {
        return GetPostScript()?.GetName(gid);
    }
}
