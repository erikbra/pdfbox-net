/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/KerningSubtable.java
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

using System;
using System.IO;

namespace PdfBox.Net.FontBox.TTF;

public class KerningSubtable
{
    private const int CoverageHorizontal = 0x0001;
    private const int CoverageMinimums = 0x0002;
    private const int CoverageCrossStream = 0x0004;
    private const int CoverageFormat = 0xFF00;

    private const int CoverageHorizontalShift = 0;
    private const int CoverageMinimumsShift = 1;
    private const int CoverageCrossStreamShift = 2;
    private const int CoverageFormatShift = 8;

    private bool horizontal;
    private bool minimums;
    private bool crossStream;
    private PairData? pairs;

    internal void Read(TTFDataStream data, int version)
    {
        if (version == 0)
        {
            ReadSubtable0(data);
        }
        else if (version == 1)
        {
            ReadSubtable1(data);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    public bool IsHorizontalKerning() => IsHorizontalKerning(false);

    public bool IsHorizontalKerning(bool cross)
    {
        if (!horizontal || minimums)
        {
            return false;
        }

        return cross ? crossStream : !crossStream;
    }

    public int[]? GetKerning(int[] glyphs)
    {
        if (pairs == null)
        {
            return null;
        }

        int ng = glyphs.Length;
        int[] kerning = new int[ng];
        for (int i = 0; i < ng; ++i)
        {
            int l = glyphs[i];
            int r = -1;
            for (int k = i + 1; k < ng; ++k)
            {
                int g = glyphs[k];
                if (g >= 0)
                {
                    r = g;
                    break;
                }
            }

            kerning[i] = GetKerning(l, r);
        }

        return kerning;
    }

    public int GetKerning(int l, int r)
    {
        return pairs?.GetKerning(l, r) ?? 0;
    }

    private void ReadSubtable0(TTFDataStream data)
    {
        int version = data.ReadUnsignedShort();
        if (version != 0)
        {
            return;
        }

        int length = data.ReadUnsignedShort();
        if (length < 6)
        {
            return;
        }

        int coverage = data.ReadUnsignedShort();
        if (IsBitsSet(coverage, CoverageHorizontal, CoverageHorizontalShift))
        {
            horizontal = true;
        }
        if (IsBitsSet(coverage, CoverageMinimums, CoverageMinimumsShift))
        {
            minimums = true;
        }
        if (IsBitsSet(coverage, CoverageCrossStream, CoverageCrossStreamShift))
        {
            crossStream = true;
        }

        int format = GetBits(coverage, CoverageFormat, CoverageFormatShift);
        switch (format)
        {
            case 0:
                ReadSubtable0Format0(data);
                break;
            case 2:
                ReadSubtable0Format2(data);
                break;
        }
    }

    private void ReadSubtable0Format0(TTFDataStream data)
    {
        pairs = new PairData0Format0();
        pairs.Read(data);
    }

    private static void ReadSubtable0Format2(TTFDataStream data)
    {
    }

    private static void ReadSubtable1(TTFDataStream data)
    {
    }

    private static bool IsBitsSet(int bits, int mask, int shift)
    {
        return GetBits(bits, mask, shift) != 0;
    }

    private static int GetBits(int bits, int mask, int shift)
    {
        return (bits & mask) >> shift;
    }

    private interface PairData
    {
        void Read(TTFDataStream data);

        int GetKerning(int l, int r);
    }

    private sealed class PairData0Format0 : PairData, IComparer<int[]>
    {
        private int[][] pairs = [];

        public void Read(TTFDataStream data)
        {
            int numPairs = data.ReadUnsignedShort();
            _ = data.ReadUnsignedShort() / 6;
            _ = data.ReadUnsignedShort();
            _ = data.ReadUnsignedShort();
            pairs = new int[numPairs][];
            for (int i = 0; i < numPairs; ++i)
            {
                int left = data.ReadUnsignedShort();
                int right = data.ReadUnsignedShort();
                int value = data.ReadSignedShort();
                pairs[i] = [left, right, value];
            }
        }

        public int GetKerning(int l, int r)
        {
            int index = Array.BinarySearch(pairs, new[] { l, r, 0 }, this);
            return index >= 0 ? pairs[index][2] : 0;
        }

        public int Compare(int[]? p1, int[]? p2)
        {
            ArgumentNullException.ThrowIfNull(p1);
            ArgumentNullException.ThrowIfNull(p2);
            int cmp1 = p1[0].CompareTo(p2[0]);
            return cmp1 != 0 ? cmp1 : p1[1].CompareTo(p2[1]);
        }
    }
}
