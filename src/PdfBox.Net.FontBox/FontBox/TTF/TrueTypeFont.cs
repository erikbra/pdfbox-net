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

    public HeaderTable? GetHeader() => GetTable("head") as HeaderTable;

    public MaximumProfileTable? GetMaximumProfile() => GetTable("maxp") as MaximumProfileTable;

    public NamingTable? GetNaming() => GetTable("name") as NamingTable;

    public HorizontalHeaderTable? GetHorizontalHeader() => GetTable("hhea") as HorizontalHeaderTable;

    public HorizontalMetricsTable? GetHorizontalMetrics() => GetTable("hmtx") as HorizontalMetricsTable;

    public IndexToLocationTable? GetIndexToLocation() => GetTable("loca") as IndexToLocationTable;

    public GlyphTable? GetGlyph() => GetTable("glyf") as GlyphTable;

    public PostScriptTable? GetPostScript() => GetTable("post") as PostScriptTable;

    public CmapTable? GetCmap() => GetTable("cmap") as CmapTable;

    public int GetNumberOfGlyphs() => GetMaximumProfile()?.NumGlyphs ?? 0;

    public int GetUnitsPerEm() => GetHeader()?.UnitsPerEm ?? 1000;

    public string? GetName()
    {
        NamingTable? naming = GetNaming();
        return naming?.GetEnglishName(4) ??
               naming?.GetEnglishName(6) ??
               naming?.GetEnglishName(1);
    }
}
