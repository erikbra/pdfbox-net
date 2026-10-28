/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/GaspTable.java
 * PDFBOX_SOURCE_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc
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
using System.Numerics;
using System.Text;
using System.Globalization;
using PdfBox.Net.Util.Geometry;
using Microsoft.Extensions.Logging;
using PdfBox.Net.Logging;

namespace PdfBox.Net.FontBox.TTF;


/// <summary>
/// The 'gasp' (Grid-fitting And Scan-conversion Procedure) table. It maps ppem ranges to flags that
/// advise whether grid-fitting (hinting) and/or grayscale anti-aliasing should be applied at that size.
/// The ranges are sorted by ascending maximum ppem; the last range always ends at 0xFFFF.
/// </summary>
/// <remarks>
/// Author: Apache PDFBox
/// </remarks>
public class GaspTable : TTFTable
{
    /// <summary>
    /// A tag that identifies this table type.
    /// </summary>
    public const string TAG = "gasp";

    /// <summary>
    /// Use grid-fitting (i.e. execute the hinting bytecode) at this size.
    /// </summary>
    public const int GASP_GRIDFIT = 0x0001;

    /// <summary>
    /// Use grayscale (anti-aliased) rendering at this size.
    /// </summary>
    public const int GASP_DOGRAY = 0x0002;

    /// <summary>
    /// Use grid-fitting with ClearType symmetric smoothing (gasp version 1).
    /// </summary>
    public const int GASP_SYMMETRIC_GRIDFIT = 0x0004;

    /// <summary>
    /// Use smoothing along multiple axes with ClearType (gasp version 1).
    /// </summary>
    public const int GASP_SYMMETRIC_SMOOTHING = 0x0008;

    private int version;
    private int[] rangeMaxPPEM = [];
    private int[] rangeFlags = [];

    internal GaspTable()
    {
    }

    /// <summary>
    /// This will read the required data from the stream.
    /// </summary>
    /// <param name="ttf">
    /// The font that is being read.
    /// </param>
    /// <param name="data">
    /// The stream to read the data from.
    /// </param>
    /// <exception cref="IOException">
    /// If there is an error reading the data.
    /// </exception>

    internal override void Read(TrueTypeFont ttf, TTFDataStream data)
    {
        version = data.ReadUnsignedShort();
        int numRanges = data.ReadUnsignedShort();
        rangeMaxPPEM = new int[numRanges];
        rangeFlags = new int[numRanges];
        for (int i = 0; i < numRanges; i++)
        {
            rangeMaxPPEM[i] = data.ReadUnsignedShort();
            rangeFlags[i] = data.ReadUnsignedShort();
        }
        initialized = true;
    }

    /// <returns>
    /// the table version (0 or 1)
    /// </returns>
    public int GetVersion()
    {
        return version;
    }

    /// <summary>
    /// Returns the upper ppem bound of each range, in ascending order. The last entry is 0xFFFF.
    /// </summary>
    /// <returns>
    /// the per-range maximum ppem values
    /// </returns>
    public int[] GetRangeMaxPPEM()
    {
        return rangeMaxPPEM;
    }

    /// <summary>
    /// Returns the flags for each range, parallel to #getRangeMaxPPEM().
    /// </summary>
    /// <returns>
    /// the per-range flags
    /// </returns>
    public int[] GetRangeFlags()
    {
        return rangeFlags;
    }

    /// <summary>
    /// Returns the behavior flags that apply at the given ppem - those of the first range whose
    /// maximum ppem is greater than or equal to the requested ppem.
    /// </summary>
    /// <param name="ppem">
    /// the pixels-per-em to look up
    /// </param>
    /// <returns>
    /// the flags for that ppem, or 0 if the table has no ranges
    /// </returns>
    public int GetFlags(int ppem)
    {
        if (rangeMaxPPEM == null)
        {
            return 0;
        }
        for (int i = 0; i < rangeMaxPPEM.Length; i++)
        {
            if (ppem <= rangeMaxPPEM[i])
            {
                return rangeFlags[i];
            }
        }
        // beyond the last range (should not happen as the last bound is 0xFFFF)
        return rangeFlags.Length > 0 ? rangeFlags[rangeFlags.Length - 1] : 0;
    }

    /// <summary>
    /// Convenience test for whether grid-fitting (hinting) is advised at the given ppem.
    /// </summary>
    /// <param name="ppem">
    /// the pixels-per-em to look up
    /// </param>
    /// <returns>
    /// true if #GASP_GRIDFIT is set for that ppem
    /// </returns>
    public bool IsGridFit(int ppem)
    {
        return (GetFlags(ppem) & GASP_GRIDFIT) != 0;
    }
}
