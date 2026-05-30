/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/VerticalMetricsTable.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: mechanical
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

public class VerticalMetricsTable() : TTFTable(TAG)
{
    public const string TAG = "vmtx";

    private int[] advanceHeight = [];
    private short[] topSideBearing = [];
    private short[] additionalTopSideBearing = [];
    private int numVMetrics;

    internal override void Read(TrueTypeFont ttf, TTFDataStream data)
    {
        VerticalHeaderTable vHeader = ttf.GetVerticalHeader() ?? throw new IOException("Could not get vhea table");
        numVMetrics = vHeader.GetNumberOfVMetrics();
        int numGlyphs = ttf.GetNumberOfGlyphs();

        int bytesRead = 0;
        advanceHeight = new int[numVMetrics];
        topSideBearing = new short[numVMetrics];
        for (int i = 0; i < numVMetrics; i++)
        {
            advanceHeight[i] = data.ReadUnsignedShort();
            topSideBearing[i] = data.ReadSignedShort();
            bytesRead += 4;
        }

        if (bytesRead < GetLength())
        {
            int numberNonVertical = numGlyphs - numVMetrics;
            if (numberNonVertical < 0)
            {
                numberNonVertical = numGlyphs;
            }

            additionalTopSideBearing = new short[numberNonVertical];
            for (int i = 0; i < numberNonVertical; i++)
            {
                if (bytesRead < GetLength())
                {
                    additionalTopSideBearing[i] = data.ReadSignedShort();
                    bytesRead += 2;
                }
            }
        }

        initialized = true;
    }

    public int GetTopSideBearing(int gid)
    {
        return gid < numVMetrics ? topSideBearing[gid] : additionalTopSideBearing[gid - numVMetrics];
    }

    public int GetAdvanceHeight(int gid)
    {
        return gid < numVMetrics ? advanceHeight[gid] : advanceHeight[^1];
    }
}
