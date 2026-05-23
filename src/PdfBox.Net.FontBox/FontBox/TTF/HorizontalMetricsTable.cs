/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache FontBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/HorizontalMetricsTable.java
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

public sealed class HorizontalMetricsTable() : TTFTable("hmtx")
{
    private int[] _advanceWidth = [];
    private short[] _leftSideBearing = [];
    private short[] _nonHorizontalLeftSideBearing = [];
    private int _numHMetrics;

    internal override void Read(TrueTypeFont font, TTFDataStream dataStream)
    {
        HorizontalHeaderTable? header = font.GetHorizontalHeader();
        if (header is null)
        {
            throw new IOException("Could not get hhea table");
        }

        _numHMetrics = header.NumberOfHMetrics;
        int numGlyphs = font.GetNumberOfGlyphs();
        _advanceWidth = new int[_numHMetrics];
        _leftSideBearing = new short[_numHMetrics];
        for (int i = 0; i < _numHMetrics; i++)
        {
            _advanceWidth[i] = dataStream.ReadUnsignedShort();
            _leftSideBearing[i] = dataStream.ReadSignedShort();
        }

        int numberNonHorizontal = Math.Max(0, numGlyphs - _numHMetrics);
        _nonHorizontalLeftSideBearing = new short[numberNonHorizontal];
        for (int i = 0; i < numberNonHorizontal && dataStream.Position < Offset + Length; i++)
        {
            _nonHorizontalLeftSideBearing[i] = dataStream.ReadSignedShort();
        }
    }

    public int GetAdvanceWidth(int gid)
    {
        if (_advanceWidth.Length == 0)
        {
            return 250;
        }

        return gid < _numHMetrics ? _advanceWidth[gid] : _advanceWidth[^1];
    }

    public int GetLeftSideBearing(int gid)
    {
        if (_leftSideBearing.Length == 0)
        {
            return 0;
        }

        return gid < _numHMetrics ? _leftSideBearing[gid] : _nonHorizontalLeftSideBearing[gid - _numHMetrics];
    }
}
