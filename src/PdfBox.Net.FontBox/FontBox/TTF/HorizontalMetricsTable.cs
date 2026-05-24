/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/HorizontalMetricsTable.java
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

using System.IO;

namespace PdfBox.Net.FontBox.TTF;

public sealed class HorizontalMetricsTable() : TTFTable(TAG)
{
    public const string TAG = "hmtx";
    private int[] _advanceWidth = [];
    private short[] _leftSideBearing = [];
    private short[] _nonHorizontalLeftSideBearing = [];
    private int _numHMetrics;

    internal override void Read(TrueTypeFont font, TTFDataStream data)
    {
        HorizontalHeaderTable hHeader = font.GetHorizontalHeader() ?? throw new IOException("Could not get hmtx table");
        _numHMetrics = hHeader.NumberOfHMetrics;
        int numGlyphs = font.GetNumberOfGlyphs();

        int bytesRead = 0;
        _advanceWidth = new int[_numHMetrics];
        _leftSideBearing = new short[_numHMetrics];
        for (int i = 0; i < _numHMetrics; i++)
        {
            _advanceWidth[i] = data.ReadUnsignedShort();
            _leftSideBearing[i] = data.ReadSignedShort();
            bytesRead += 4;
        }

        int numberNonHorizontal = numGlyphs - _numHMetrics;
        if (numberNonHorizontal < 0)
        {
            numberNonHorizontal = numGlyphs;
        }

        _nonHorizontalLeftSideBearing = new short[numberNonHorizontal];
        if (bytesRead < Length)
        {
            for (int i = 0; i < numberNonHorizontal && bytesRead < Length; i++)
            {
                _nonHorizontalLeftSideBearing[i] = data.ReadSignedShort();
                bytesRead += 2;
            }
        }

        Initialized = true;
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

        if (gid < _numHMetrics)
        {
            return _leftSideBearing[gid];
        }

        int index = gid - _numHMetrics;
        return index >= 0 && index < _nonHorizontalLeftSideBearing.Length ? _nonHorizontalLeftSideBearing[index] : 0;
    }
}
