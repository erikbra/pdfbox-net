/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/GraphicsState.java
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
/// The TrueType interpreter graphics state: projection/freedom vectors, reference and zone pointers,
/// round state, and the various cut-ins and distances. Two operations carry the correctness burden:
/// &lt;ul&gt;
///   &lt;li&gt;#copy() performs a &lt;em&gt;deep&lt;/em&gt; copy of the UnitVector fields, so mutating a
///       per-glyph clone never contaminates the saved post-prep template.&lt;/li&gt;
///   &lt;li&gt;#resetForGlyph() resets only the fields the TrueType spec re-initializes at the start
///       of each glyph program (the vectors, reference points, zone pointers and loop counter), while
///       leaving the values established by prep (round state, cut-ins, delta base/shift, etc.)
///       intact.&lt;/li&gt;
/// &lt;/ul&gt;
/// 
/// @author Apache PDFBox
/// </summary>
internal class GraphicsState
{
    /// <summary>
/// Round to grid - the default round state.
/// </summary>
    public const int ROUND_TO_GRID = 0;
    /// <summary>
/// Round to half grid.
/// </summary>
    public const int ROUND_TO_HALF_GRID = 1;
    /// <summary>
/// Round to double grid.
/// </summary>
    public const int ROUND_TO_DOUBLE_GRID = 2;
    /// <summary>
/// Round down to grid.
/// </summary>
    public const int ROUND_DOWN_TO_GRID = 3;
    /// <summary>
/// Round up to grid.
/// </summary>
    public const int ROUND_UP_TO_GRID = 4;
    /// <summary>
/// Rounding off.
/// </summary>
    public const int ROUND_OFF = 5;
    /// <summary>
/// Super round (set by SROUND).
/// </summary>
    public const int ROUND_SUPER = 6;
    /// <summary>
/// Super round 45 degrees (set by S45ROUND).
/// </summary>
    public const int ROUND_SUPER_45 = 7;

    private UnitVector projectionVector;
    private UnitVector freedomVector;
    private UnitVector dualProjectionVector;

    private int rp0;
    private int rp1;
    private int rp2;

    private int zp0;
    private int zp1;
    private int zp2;

    private int loop;
    private int roundState;
    // derived rounding parameters (all F26Dot6), configured from the round state
    private int roundPeriod;
    private int roundPhase;
    private int roundThreshold;
    private bool roundOff;

    private int minimumDistance;
    private int controlValueCutIn;
    private int singleWidthCutIn;
    private int singleWidthValue;
    private int deltaBase;
    private int deltaShift;
    private bool autoFlip;
    private int scanControl;
    private int scanType;
    private int instructControl;

    /// <summary>
/// Creates a graphics state with the spec-mandated default values. This is the state before the
/// font's fpgm/prep programs run.
/// </summary>
    public GraphicsState()
    {
        projectionVector = UnitVector.XAxis();
        freedomVector = UnitVector.XAxis();
        dualProjectionVector = UnitVector.XAxis();
        rp0 = 0;
        rp1 = 0;
        rp2 = 0;
        zp0 = 1;
        zp1 = 1;
        zp2 = 1;
        loop = 1;
        SetRoundState(ROUND_TO_GRID);
        minimumDistance = Fixed.ONE;          // 1 pixel
        controlValueCutIn = 17 * Fixed.ONE / 16; // 17/16 pixel = 68
        singleWidthCutIn = 0;
        singleWidthValue = 0;
        deltaBase = 9;
        deltaShift = 3;
        autoFlip = true;
        scanControl = 0;
        scanType = 0;
        instructControl = 0;
    }

    private GraphicsState(GraphicsState src)
    {
        projectionVector = src.projectionVector.Copy();
        freedomVector = src.freedomVector.Copy();
        dualProjectionVector = src.dualProjectionVector.Copy();
        rp0 = src.rp0;
        rp1 = src.rp1;
        rp2 = src.rp2;
        zp0 = src.zp0;
        zp1 = src.zp1;
        zp2 = src.zp2;
        loop = src.loop;
        roundState = src.roundState;
        roundPeriod = src.roundPeriod;
        roundPhase = src.roundPhase;
        roundThreshold = src.roundThreshold;
        roundOff = src.roundOff;
        minimumDistance = src.minimumDistance;
        controlValueCutIn = src.controlValueCutIn;
        singleWidthCutIn = src.singleWidthCutIn;
        singleWidthValue = src.singleWidthValue;
        deltaBase = src.deltaBase;
        deltaShift = src.deltaShift;
        autoFlip = src.autoFlip;
        scanControl = src.scanControl;
        scanType = src.scanType;
        instructControl = src.instructControl;
    }

    /// <summary>
/// Returns an independent deep copy, with the UnitVector fields cloned rather than shared.
/// 
/// @return a deep copy of this graphics state
/// </summary>
    public GraphicsState Copy()
    {
        return new GraphicsState(this);
    }

    /// <summary>
/// Resets the per-glyph graphics state fields to their defaults, as the TrueType spec requires at
/// the start of each glyph's instruction stream. The projection, freedom and dual-projection
/// vectors return to the x axis; the reference points reset to 0; the zone pointers reset to the
/// glyph zone (1); the loop counter resets to 1. Fields configured by prep (round state,
/// cut-ins, minimum distance, delta base/shift, single width, auto-flip, scan control) are left
/// untouched.
/// </summary>
    public void ResetForGlyph()
    {
        projectionVector.Set(Fixed.ONE_F2DOT14, 0);
        freedomVector.Set(Fixed.ONE_F2DOT14, 0);
        dualProjectionVector.Set(Fixed.ONE_F2DOT14, 0);
        rp0 = 0;
        rp1 = 0;
        rp2 = 0;
        zp0 = 1;
        zp1 = 1;
        zp2 = 1;
        loop = 1;
    }

    /// <summary>
/// @return the projection vector
/// </summary>
    public UnitVector GetProjectionVector()
    {
        return projectionVector;
    }

    /// <summary>
/// @return the freedom vector
/// </summary>
    public UnitVector GetFreedomVector()
    {
        return freedomVector;
    }

    /// <summary>
/// @return the dual projection vector
/// </summary>
    public UnitVector GetDualProjectionVector()
    {
        return dualProjectionVector;
    }

    /// <summary>
/// @return reference point 0
/// </summary>
    public int GetRp0()
    {
        return rp0;
    }

    /// <summary>
/// @param value reference point 0
/// </summary>
    public void SetRp0(int value)
    {
        rp0 = value;
    }

    /// <summary>
/// @return reference point 1
/// </summary>
    public int GetRp1()
    {
        return rp1;
    }

    /// <summary>
/// @param value reference point 1
/// </summary>
    public void SetRp1(int value)
    {
        rp1 = value;
    }

    /// <summary>
/// @return reference point 2
/// </summary>
    public int GetRp2()
    {
        return rp2;
    }

    /// <summary>
/// @param value reference point 2
/// </summary>
    public void SetRp2(int value)
    {
        rp2 = value;
    }

    /// <summary>
/// @return zone pointer 0
/// </summary>
    public int GetZp0()
    {
        return zp0;
    }

    /// <summary>
/// @param value zone pointer 0
/// </summary>
    public void SetZp0(int value)
    {
        zp0 = value;
    }

    /// <summary>
/// @return zone pointer 1
/// </summary>
    public int GetZp1()
    {
        return zp1;
    }

    /// <summary>
/// @param value zone pointer 1
/// </summary>
    public void SetZp1(int value)
    {
        zp1 = value;
    }

    /// <summary>
/// @return zone pointer 2
/// </summary>
    public int GetZp2()
    {
        return zp2;
    }

    /// <summary>
/// @param value zone pointer 2
/// </summary>
    public void SetZp2(int value)
    {
        zp2 = value;
    }

    /// <summary>
/// @return the loop counter
/// </summary>
    public int GetLoop()
    {
        return loop;
    }

    /// <summary>
/// @param value the loop counter
/// </summary>
    public void SetLoop(int value)
    {
        loop = value;
    }

    /// <summary>
/// @return the round state
/// </summary>
    public int GetRoundState()
    {
        return roundState;
    }

    /// <summary>
/// Sets the round state and derives the period/phase/threshold the #round(int) engine uses.
/// The simple states are expressed as special cases of the super-round parameters. SROUND
/// and S45ROUND call #setSuperRound(int, int) instead.
/// 
/// @param value one of the ROUND_* constants
/// </summary>
    public void SetRoundState(int value)
    {
        roundState = value;
        roundOff = false;
        switch (value)
        {
            case ROUND_TO_GRID:
                roundPeriod = Fixed.ONE;
                roundPhase = 0;
                roundThreshold = Fixed.HALF;
                break;
            case ROUND_TO_HALF_GRID:
                roundPeriod = Fixed.ONE;
                roundPhase = Fixed.HALF;
                roundThreshold = Fixed.HALF;
                break;
            case ROUND_TO_DOUBLE_GRID:
                roundPeriod = Fixed.HALF;
                roundPhase = 0;
                roundThreshold = Fixed.HALF / 2;
                break;
            case ROUND_DOWN_TO_GRID:
                roundPeriod = Fixed.ONE;
                roundPhase = 0;
                roundThreshold = 0;
                break;
            case ROUND_UP_TO_GRID:
                roundPeriod = Fixed.ONE;
                roundPhase = 0;
                roundThreshold = Fixed.ONE - 1;
                break;
            case ROUND_OFF:
                roundOff = true;
                break;
            default:
                // ROUND_SUPER / ROUND_SUPER_45 are configured by setSuperRound
                break;
        }
    }

    /// <summary>
/// SROUND/S45ROUND period selector (bits 7-6) to the period in half grid periods.
/// </summary>
    private static readonly int[] PERIOD_HALVES = [ 1, 2, 4, 2 ];

    /// <summary>
/// Configures super-round parameters for SROUND/S45ROUND from the selector byte,
/// per the TrueType specification. The period, phase and threshold are derived in F2Dot14 and
/// then converted to F26Dot6, exactly as FreeType's SetSuperRound does; deriving them in
/// F26Dot6 directly is off by one for the S45ROUND "period - 1" threshold (44 instead of 45).
/// 
/// @param gridPeriod the base grid period in F2Dot14 (0x4000 = one pixel for SROUND;
/// 0x2D41 = sqrt(2)/2, the diagonal, for S45ROUND)
/// @param selector the operand byte controlling period, phase and threshold
/// </summary>
    public void SetSuperRound(int gridPeriod, int selector)
    {
        // bits 7-6: the period as a multiple of the grid, in halves - 1/2, 1, 2, and the reserved
        // value 3 taken as 1 (as FreeType does); bits 5-4: the phase in quarters of the period
        int period = gridPeriod * PERIOD_HALVES[(selector >> 6) & 3] / 2;
        int phase = period * ((selector >> 4) & 3) / 4;
        int n = selector & 0x0F;
        int threshold = n == 0 ? period - 1 : (n - 4) * period / 8;

        // F2Dot14 -> F26Dot6
        roundPeriod = Math.Max(1, period >> 8);
        roundPhase = phase >> 8;
        roundThreshold = threshold >> 8;
        roundState = ROUND_SUPER;
        roundOff = false;
    }

    /// <summary>
/// Rounds a distance according to the current round state. Engine compensation (the black/white/
/// grey distance bias FreeType applies) is treated as zero, which is correct for an anti-aliased
/// Java2D target.
/// 
/// @param distance the distance in F26Dot6
/// @return the rounded distance in F26Dot6
/// </summary>
    public int Round(int distance)
    {
        if (roundOff)
        {
            return distance;
        }
        int val;
        if (distance >= 0)
        {
            val = JavaFloorDiv(distance - roundPhase + roundThreshold, roundPeriod) * roundPeriod;
            if (val < 0)
            {
                val = 0;
            }
            val += roundPhase;
        }
        else
        {
            val = -(JavaFloorDiv(roundThreshold - roundPhase - distance, roundPeriod) * roundPeriod);
            if (val > 0)
            {
                val = 0;
            }
            val -= roundPhase;
        }
        return val;
    }

    /// <summary>
/// @return the minimum distance in F26Dot6
/// </summary>
    public int GetMinimumDistance()
    {
        return minimumDistance;
    }

    /// <summary>
/// @param value the minimum distance in F26Dot6
/// </summary>
    public void SetMinimumDistance(int value)
    {
        minimumDistance = value;
    }

    /// <summary>
/// @return the control value cut-in in F26Dot6
/// </summary>
    public int GetControlValueCutIn()
    {
        return controlValueCutIn;
    }

    /// <summary>
/// @param value the control value cut-in in F26Dot6
/// </summary>
    public void SetControlValueCutIn(int value)
    {
        controlValueCutIn = value;
    }

    /// <summary>
/// @return the single width cut-in in F26Dot6
/// </summary>
    public int GetSingleWidthCutIn()
    {
        return singleWidthCutIn;
    }

    /// <summary>
/// @param value the single width cut-in in F26Dot6
/// </summary>
    public void SetSingleWidthCutIn(int value)
    {
        singleWidthCutIn = value;
    }

    /// <summary>
/// @return the single width value in F26Dot6
/// </summary>
    public int GetSingleWidthValue()
    {
        return singleWidthValue;
    }

    /// <summary>
/// @param value the single width value in F26Dot6
/// </summary>
    public void SetSingleWidthValue(int value)
    {
        singleWidthValue = value;
    }

    /// <summary>
/// @return the delta base
/// </summary>
    public int GetDeltaBase()
    {
        return deltaBase;
    }

    /// <summary>
/// @param value the delta base
/// </summary>
    public void SetDeltaBase(int value)
    {
        deltaBase = value;
    }

    /// <summary>
/// @return the delta shift
/// </summary>
    public int GetDeltaShift()
    {
        return deltaShift;
    }

    /// <summary>
/// @param value the delta shift
/// </summary>
    public void SetDeltaShift(int value)
    {
        deltaShift = value;
    }

    /// <summary>
/// @return whether auto-flip is enabled
/// </summary>
    public bool IsAutoFlip()
    {
        return autoFlip;
    }

    /// <summary>
/// @param value whether auto-flip is enabled
/// </summary>
    public void SetAutoFlip(bool value)
    {
        autoFlip = value;
    }

    /// <summary>
/// @return the scan control flags
/// </summary>
    public int GetScanControl()
    {
        return scanControl;
    }

    /// <summary>
/// @param value the scan control flags
/// </summary>
    public void SetScanControl(int value)
    {
        scanControl = value;
    }

    /// <summary>
/// @return the scan type
/// </summary>
    public int GetScanType()
    {
        return scanType;
    }

    /// <summary>
/// @param value the scan type
/// </summary>
    public void SetScanType(int value)
    {
        scanType = value;
    }

    /// <summary>
/// @return the instruction control flags
/// </summary>
    public int GetInstructControl()
    {
        return instructControl;
    }

    /// <summary>
/// @param value the instruction control flags
/// </summary>
    public void SetInstructControl(int value)
    {
        instructControl = value;
    }
    private static int JavaFloorDiv(int value, int divisor)
    {
        int quotient = value / divisor;
        return value % divisor < 0 ? quotient - 1 : quotient;
    }

}
