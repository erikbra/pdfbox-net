/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/GlyphTable.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
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

using System.IO;

namespace PdfBox.Net.FontBox.TTF;

public sealed class GlyphTable() : TTFTable(TAG)
{
    public const string TAG = "glyf";
    private GlyphData?[]? _glyphs;
    private IndexToLocationTable? _loca;
    private int _numGlyphs;
    private HorizontalMetricsTable? _hmt;
    private MaximumProfileTable? _maxp;
    private MemoryTTFDataStream? _data;

    internal override void Read(TrueTypeFont font, TTFDataStream data)
    {
        _loca = font.GetIndexToLocation();
        _numGlyphs = font.GetNumberOfGlyphs();
        byte[] tableBytes = data.ReadBytes((int)Length);
        _data = new MemoryTTFDataStream(tableBytes);
        _hmt = font.GetHorizontalMetrics();
        _maxp = font.GetMaximumProfile();
        if (_numGlyphs < 5000)
        {
            _glyphs = new GlyphData?[_numGlyphs];
        }

        Initialized = true;
    }

    public void SetGlyphs(GlyphData[] glyphs)
    {
        _glyphs = Array.ConvertAll(glyphs, value => (GlyphData?)value);
    }

    public GlyphData? GetGlyph(int gid) => GetGlyph(gid, 0);

    internal GlyphData? GetGlyph(int gid, int level)
    {
        if (gid < 0 || gid >= _numGlyphs || _loca is null || _data is null)
        {
            return null;
        }

        if (_glyphs is not null && _glyphs[gid] is not null)
        {
            return _glyphs[gid];
        }

        long[] offsets = _loca.GetOffsets();
        GlyphData glyph;
        if (offsets[gid] == offsets[gid + 1] || offsets[gid] == _data.GetOriginalDataSize())
        {
            glyph = new GlyphData();
            glyph.InitEmptyData();
        }
        else
        {
            long currentPosition = _data.GetCurrentPosition();
            _data.Seek(offsets[gid]);
            glyph = GetGlyphData(gid, level);
            _data.Seek(currentPosition);
        }

        if (_glyphs is not null)
        {
            _glyphs[gid] = glyph;
        }

        return glyph;
    }

    private GlyphData GetGlyphData(int gid, int level)
    {
        if (_data is null)
        {
            throw new IOException("Glyph data stream is not available");
        }

        if (_maxp is not null && level > _maxp.MaxComponentDepth)
        {
            throw new IOException($"composite glyph maximum level ({_maxp.MaxComponentDepth}) reached");
        }

        GlyphData glyph = new();
        int leftSideBearing = _hmt?.GetLeftSideBearing(gid) ?? 0;
        glyph.InitData(this, _data, leftSideBearing, level);
        if (glyph.Description?.IsComposite() == true)
        {
            glyph.Description.Resolve();
        }

        return glyph;
    }
}
