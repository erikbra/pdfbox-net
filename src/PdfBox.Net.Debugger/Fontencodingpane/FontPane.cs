/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/fontencodingpane/FontPane.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
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

using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.Debugger.Fontencodingpane;

/// <summary>
/// Abstract base for font-encoding data models used by the debugger.
/// Adapted from Apache PDFBox FontPane (Apache Software Foundation).
/// </summary>
public abstract class FontPane
{
    /// <summary>
    /// Computes the vertical bounds shared by all rendered glyphs.
    /// </summary>
    /// <param name="tableData">2-D array of glyph table rows.</param>
    /// <param name="glyphIndex">Column index that holds the <see cref="GeneralPath"/> glyph paths.</param>
    /// <returns>Array [minY (≤ 0), maxY (≥ 0)].</returns>
    public double[] GetYBounds(object[][] tableData, int glyphIndex)
    {
        double minY = 0;
        double maxY = 0;
        foreach (object[] row in tableData)
        {
            if (row[glyphIndex] is not GeneralPath path)
            {
                continue;
            }

            if (!ComputeBounds(path, out double pMinY, out double pMaxY))
            {
                continue;
            }

            minY = Math.Min(minY, pMinY);
            maxY = Math.Max(maxY, pMaxY);
        }

        return [minY, maxY];
    }

    private static bool ComputeBounds(GeneralPath path, out double minY, out double maxY)
    {
        minY = double.MaxValue;
        maxY = double.MinValue;
        bool hasPoints = false;

        foreach (var seg in path.Segments)
        {
            if (seg.Type == GeneralPath.SegmentType.Close)
            {
                continue;
            }

            minY = Math.Min(minY, seg.Y1);
            maxY = Math.Max(maxY, seg.Y1);
            if (seg.Type is GeneralPath.SegmentType.QuadTo or GeneralPath.SegmentType.CurveTo)
            {
                minY = Math.Min(minY, seg.Y2);
                maxY = Math.Max(maxY, seg.Y2);
            }
            if (seg.Type == GeneralPath.SegmentType.CurveTo)
            {
                minY = Math.Min(minY, seg.Y3);
                maxY = Math.Max(maxY, seg.Y3);
            }
            hasPoints = true;
        }

        if (!hasPoints)
        {
            minY = 0;
            maxY = 0;
            return false;
        }

        return true;
    }
}
