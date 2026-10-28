/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/Zone.java
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
/// A set of points the interpreter can manipulate - either zone 0 (the twilight zone, holding phantom
/// reference points) or zone 1 (the glyph's own outline points plus its appended phantom points). Each
/// point has its scaled-but-unhinted "original" position and a "current" position the bytecode moves,
/// both in F26Dot6, plus per-axis touch flags used by interpolation.
/// 
/// @author Apache PDFBox
/// </summary>
internal class Zone
{
    private int[] currentX;
    private int[] currentY;
    private int[] originalX;
    private int[] originalY;
    private int[] unscaledX;
    private int[] unscaledY;
    private bool[] touchedX;
    private bool[] touchedY;
    private bool[] onCurve;
    private int[] contourEnds;

    /// <summary>
/// Allocates a zone of the given size.
/// 
/// @param pointCount number of points (including any phantom points)
/// @param contourCount number of contours (0 for the twilight zone)
/// </summary>
    public Zone(int pointCount, int contourCount)
    {
        currentX = new int[pointCount];
        currentY = new int[pointCount];
        originalX = new int[pointCount];
        originalY = new int[pointCount];
        unscaledX = new int[pointCount];
        unscaledY = new int[pointCount];
        touchedX = new bool[pointCount];
        touchedY = new bool[pointCount];
        onCurve = new bool[pointCount];
        contourEnds = new int[contourCount];
    }

    /// <summary>
/// Zeroes every coordinate and flag. The twilight zone outlives a single program run - it is owned by
/// the interpreter so values prep puts there survive into each glyph program - so it is reset
/// rather than reallocated when the size changes, as FreeType does in tt_size_run_prep.
/// </summary>
    public void Reset()
    {
        Array.Fill(currentX, 0);
        Array.Fill(currentY, 0);
        Array.Fill(originalX, 0);
        Array.Fill(originalY, 0);
        Array.Fill(unscaledX, 0);
        Array.Fill(unscaledY, 0);
        Array.Fill(touchedX, false);
        Array.Fill(touchedY, false);
        Array.Fill(onCurve, false);
        Array.Fill(contourEnds, 0);
    }

    /// <summary>
/// @return the number of points in this zone
/// </summary>
    public int GetPointCount()
    {
        return currentX.Length;
    }

    /// <summary>
/// @return the current (hinted) x coordinates in F26Dot6
/// </summary>
    public int[] GetCurrentX()
    {
        return currentX;
    }

    /// <summary>
/// @return the current (hinted) y coordinates in F26Dot6
/// </summary>
    public int[] GetCurrentY()
    {
        return currentY;
    }

    /// <summary>
/// @return the original (scaled, unhinted) x coordinates in F26Dot6
/// </summary>
    public int[] GetOriginalX()
    {
        return originalX;
    }

    /// <summary>
/// @return the original (scaled, unhinted) y coordinates in F26Dot6
/// </summary>
    public int[] GetOriginalY()
    {
        return originalY;
    }

    /// <summary>
/// @return the original &lt;em&gt;unscaled&lt;/em&gt; x coordinates in font units. Interpolation and relative
/// measurements use these for the ratio, matching FreeType's orus, because the unrounded
/// font-unit values preserve precision the scaled F26Dot6 originals would lose.
/// </summary>
    public int[] GetUnscaledX()
    {
        return unscaledX;
    }

    /// <summary>
/// @return the original unscaled y coordinates in font units
/// </summary>
    public int[] GetUnscaledY()
    {
        return unscaledY;
    }

    /// <summary>
/// @return per-point touch flags for the x axis
/// </summary>
    public bool[] GetTouchedX()
    {
        return touchedX;
    }

    /// <summary>
/// @return per-point touch flags for the y axis
/// </summary>
    public bool[] GetTouchedY()
    {
        return touchedY;
    }

    /// <summary>
/// @return per-point on-curve flags
/// </summary>
    public bool[] GetOnCurve()
    {
        return onCurve;
    }

    /// <summary>
/// @return the index of the last point of each contour
/// </summary>
    public int[] GetContourEnds()
    {
        return contourEnds;
    }
}
