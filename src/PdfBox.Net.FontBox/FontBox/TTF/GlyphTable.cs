/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache FontBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/GlyphTable.java
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

public sealed class GlyphTable() : TTFTable("glyf")
{
    private byte[] _glyphBytes = [];
    private GlyphData?[] _glyphs = [];
    private IndexToLocationTable? _loca;
    private HorizontalMetricsTable? _horizontalMetrics;
    private MaximumProfileTable? _maximumProfile;
    private int _numGlyphs;

    internal override void Read(TrueTypeFont font, TTFDataStream dataStream)
    {
        _loca = font.GetIndexToLocation();
        _horizontalMetrics = font.GetHorizontalMetrics();
        _maximumProfile = font.GetMaximumProfile();
        _numGlyphs = font.GetNumberOfGlyphs();
        _glyphs = new GlyphData?[_numGlyphs];
        _glyphBytes = dataStream.ReadBytes((int)Length);
    }

    public GlyphData? GetGlyph(int gid)
    {
        return GetGlyph(gid, 0);
    }

    internal GlyphData? GetGlyph(int gid, int level)
    {
        if (gid < 0 || gid >= _numGlyphs || _loca is null)
        {
            return null;
        }

        if (_glyphs[gid] is not null)
        {
            return _glyphs[gid];
        }

        long[] offsets = _loca.Offsets;
        GlyphData glyph;
        if (offsets[gid] == offsets[gid + 1] || offsets[gid] >= _glyphBytes.Length)
        {
            glyph = new GlyphData();
            glyph.InitEmptyData();
        }
        else
        {
            if (_maximumProfile is not null && level > _maximumProfile.MaxComponentDepth)
            {
                throw new IOException($"composite glyph maximum level ({_maximumProfile.MaxComponentDepth}) reached");
            }

            MemoryTTFDataStream stream = new(_glyphBytes);
            stream.Seek(offsets[gid]);
            glyph = new GlyphData();
            int leftSideBearing = _horizontalMetrics?.GetLeftSideBearing(gid) ?? 0;
            glyph.InitData(this, stream, leftSideBearing, level);
            if (glyph.Description.IsComposite())
            {
                glyph.Description.Resolve();
            }
        }

        _glyphs[gid] = glyph;
        return glyph;
    }
}
