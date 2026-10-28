/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/ExecutionContext.java
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
/// All mutable state for a single run of the interpreter, bundled into one object so opcode handlers
/// share a uniform execute(ExecutionContext) signature and new state can be added without
/// touching every handler. It holds the operand stack, storage area, scaled control values, the two
/// point zones, the GraphicsState, the current BytecodeStream, and the ppem the program
/// is running at.
/// &lt;p&gt;
/// Not all of that state is per-run. The storage area and the twilight zone are owned by the
/// TrueTypeInterpreter and handed to every context at one size, because a font may compute values
/// into them in prep and read them back from each glyph program - FreeType keeps both on the
/// TT_Size for the same reason.
/// 
/// @author Apache PDFBox
/// </summary>
internal class ExecutionContext
{
    private TrueTypeInterpreter interpreter;
    private GraphicsState graphicsState;

    private int[] stack;
    private int stackPointer;

    // owned by the interpreter and shared by every context at one size, so prep can seed them
    private int[] storage;
    private Zone twilightZone;

    private int[] controlValues;
    private Zone glyphZone;

    private int ppem;
    private int pointSize;
    private int unitsPerEm;

    private BytecodeStream stream;
    private int callDepth;
    private bool returnFromFunction;

    // Execution budget. TrueType can only loop through a backward jump or a LOOPCALL, so bounding
    // those two bounds the whole program - without them a four-byte glyph program can spin forever.
    // Both counters are per-context, and one context is one top-level program run, so they need no
    // reset. After FreeType's neg_jump_counter / loopcall_counter in TT_RunIns.
    private long negativeJumpCounter;
    private long loopCallCounter;
    private int executionBudget = -1;

    // v40 "backward compatibility" (grayscale subpixel) state. When set, point moves in the x
    // direction are suppressed so stems are not grid-fit and darkened under antialiasing, and y moves
    // are frozen once IUP has run on both axes. Only enabled for the glyph program, never fpgm/prep.
    private bool backwardCompatibility;
    private CodeRange codeRange = CodeRange.GLYPH;
    private bool iupxCalled;
    private bool iupyCalled;
    private bool composite;

    /// <summary>
/// @param interpreter the owning interpreter (for function calls)
/// @param graphicsState the graphics state this run starts from
/// @param maxStackElements operand stack capacity
/// @param storage the interpreter's storage area, shared across the runs at one size
/// @param controlValues the scaled control values (F26Dot6), or null
/// @param twilightZone the interpreter's twilight zone, shared across the runs at one size
/// </summary>
    public ExecutionContext(TrueTypeInterpreter interpreter, GraphicsState graphicsState,
            int maxStackElements, int[] storage, int[] controlValues, Zone twilightZone)
    {
        this.interpreter = interpreter;
        this.graphicsState = graphicsState;
        this.stack = new int[Math.Max(maxStackElements, 1)];
        this.storage = storage;
        this.controlValues = controlValues != null ? controlValues : new int[0];
        this.twilightZone = twilightZone;
    }

    /// <summary>
/// @return the owning interpreter
/// </summary>
    public TrueTypeInterpreter GetInterpreter()
    {
        return interpreter;
    }

    /// <summary>
/// @return the graphics state
/// </summary>
    public GraphicsState GetGraphicsState()
    {
        return graphicsState;
    }

    // --- operand stack ---------------------------------------------------

    /// <summary>
/// Pushes a value onto the operand stack.
/// 
/// @param value the value to push
/// @throws HintingException on stack overflow
/// </summary>
    public void Push(int value)
    {
        if (stackPointer >= stack.Length)
        {
            throw new HintingException("interpreter stack overflow at " + stackPointer);
        }
        stack[stackPointer++] = value;
    }

    /// <summary>
/// Pops a value from the operand stack.
/// 
/// @return the popped value
/// @throws HintingException on stack underflow
/// </summary>
    public int Pop()
    {
        if (stackPointer <= 0)
        {
            throw new HintingException("interpreter stack underflow");
        }
        return stack[--stackPointer];
    }

    /// <summary>
/// Returns the value n positions below the top without removing it (peek(0) is the
/// top of stack).
/// 
/// @param n depth below the top
/// @return the value at that depth
/// @throws HintingException if the depth is output of range
/// </summary>
    public int Peek(int n)
    {
        int index = stackPointer - 1 - n;
        if (index < 0 || index >= stackPointer)
        {
            throw new HintingException("interpreter stack peek output of range: " + n);
        }
        return stack[index];
    }

    /// <summary>
/// @return the current stack depth
/// </summary>
    public int GetStackDepth()
    {
        return stackPointer;
    }

    /// <summary>
/// Empties the operand stack.
/// </summary>
    public void ClearStack()
    {
        stackPointer = 0;
    }

    // --- execution budget -------------------------------------------------

    /// <summary>
/// The maximum number of backward jumps, and separately of LOOPCALL iterations, this run may
/// make before it is abandoned. Sized the way FreeType's TT_RunIns sizes its counters, so a
/// legitimately loop-heavy program still completes while a crafted one cannot run forever: from the
/// glyph's point count and the control value count for a glyph program, and from the control value
/// count alone for fpgm/prep, which typically loop over every CVT entry. Computed on first use,
/// because the glyph zone is attached after the context is built.
/// 
/// @return the per-run budget
/// </summary>
    public int GetExecutionBudget()
    {
        if (executionBudget < 0)
        {
            int points = glyphZone != null ? glyphZone.GetPointCount() : 0;
            executionBudget = points > 0
                    ? Math.Max(50, 10 * points) + Math.Max(50, controlValues.Length / 10)
                    : 300 + 22 * controlValues.Length;
        }
        return executionBudget;
    }

    /// <summary>
/// Records one backward jump, failing the run once #getExecutionBudget() is exhausted.
/// 
/// @throws HintingException if too many backward jumps have been made
/// </summary>
    public void CountNegativeJump()
    {
        if (++negativeJumpCounter > GetExecutionBudget())
        {
            throw new HintingException(
                    "too many backward jumps, limit is " + GetExecutionBudget());
        }
    }

    /// <summary>
/// Adds count iterations to the LOOPCALL budget, failing before the loop is entered
/// rather than partway through it. The budget is cumulative across the run, so a program cannot slip
/// past it by issuing many small loops.
/// 
/// @param count the number of iterations about to be run, always positive
/// @throws HintingException if the budget is exhausted
/// </summary>
    public void CountLoopCalls(int count)
    {
        loopCallCounter += count;
        if (loopCallCounter > GetExecutionBudget())
        {
            throw new HintingException("LOOPCALL runs too long, limit is " + GetExecutionBudget()
                    + " iterations, asked for " + loopCallCounter);
        }
    }

    // --- storage and control values --------------------------------------

    /// <summary>
/// @return the storage area, shared with every other run at this size
/// </summary>
    public int[] GetStorage()
    {
        return storage;
    }

    /// <summary>
/// @return the scaled control values in F26Dot6
/// </summary>
    public int[] GetControlValues()
    {
        return controlValues;
    }

    // --- zones -----------------------------------------------------------

    /// <summary>
/// @return the twilight zone (zone 0), shared with every other run at this size
/// </summary>
    public Zone GetTwilightZone()
    {
        return twilightZone;
    }

    /// <summary>
/// @return the glyph zone (zone 1), or null if no glyph is loaded
/// </summary>
    public Zone GetGlyphZone()
    {
        return glyphZone;
    }

    /// <summary>
/// @param zone the glyph zone (zone 1)
/// </summary>
    public void SetGlyphZone(Zone zone)
    {
        this.glyphZone = zone;
    }

    /// <summary>
/// Resolves a zone pointer (0 = twilight, 1 = glyph) to its Zone.
/// 
/// @param zonePointer the zone pointer value
/// @return the corresponding zone
/// @throws HintingException if the pointer is invalid or the glyph zone is unset
/// </summary>
    public Zone GetZone(int zonePointer)
    {
        if (zonePointer == 0)
        {
            return twilightZone;
        }
        if (zonePointer == 1)
        {
            if (glyphZone == null)
            {
                throw new HintingException("glyph zone referenced but not loaded");
            }
            return glyphZone;
        }
        throw new HintingException("invalid zone pointer: " + zonePointer);
    }

    // --- sizing ----------------------------------------------------------

    /// <summary>
/// @return the active pixels-per-em
/// </summary>
    public int GetPpem()
    {
        return ppem;
    }

    /// <summary>
/// @param value the active pixels-per-em
/// </summary>
    public void SetPpem(int value)
    {
        this.ppem = value;
    }

    /// <summary>
/// @return the point size
/// </summary>
    public int GetPointSize()
    {
        return pointSize;
    }

    /// <summary>
/// @param value the point size
/// </summary>
    public void SetPointSize(int value)
    {
        this.pointSize = value;
    }

    /// <summary>
/// @return the font's unitsPerEm
/// </summary>
    public int GetUnitsPerEm()
    {
        return unitsPerEm;
    }

    /// <summary>
/// @param value the font's unitsPerEm
/// </summary>
    public void SetUnitsPerEm(int value)
    {
        this.unitsPerEm = value;
    }

    // --- projection / freedom vector math --------------------------------

    /// <summary>
/// Dot product of two F2Dot14 vectors (or a coordinate against an F2Dot14 vector), returning the
/// result shifted back down by 14 bits with rounding. This is the projection primitive: projecting
/// an F26Dot6 coordinate onto an F2Dot14 unit vector yields an F26Dot6 distance.
/// 
/// @param ax first vector x
/// @param ay first vector y
/// @param bx second vector x (F2Dot14)
/// @param by second vector y (F2Dot14)
/// @return the rounded dot product
/// </summary>
    public static int Dot14(int ax, int ay, int bx, int by)
    {
        long product = (long) ax * bx + (long) ay * by;
        return (int) ((product + 0x2000) >> 14);
    }

    /// <summary>
/// Projects a coordinate onto the projection vector.
/// 
/// @param x the x coordinate in F26Dot6
/// @param y the y coordinate in F26Dot6
/// @return the projected distance in F26Dot6
/// </summary>
    public int Project(int x, int y)
    {
        UnitVector pv = graphicsState.GetProjectionVector();
        return Dot14(x, y, pv.GetX(), pv.GetY());
    }

    /// <summary>
/// Projects a coordinate onto the dual projection vector (used to measure original, unhinted
/// positions).
/// 
/// @param x the x coordinate in F26Dot6
/// @param y the y coordinate in F26Dot6
/// @return the projected distance in F26Dot6
/// </summary>
    public int DualProject(int x, int y)
    {
        UnitVector dv = graphicsState.GetDualProjectionVector();
        return Dot14(x, y, dv.GetX(), dv.GetY());
    }

    /// <summary>
/// Returns the current (hinted) projected distance from point p0 in zone0 to point
/// p1 in zone1, measured along the projection vector.
/// </summary>
    public int ProjectedDistance(Zone zone1, int p1, Zone zone0, int p0)
    {
        CheckPoint(zone1, p1);
        CheckPoint(zone0, p0);
        return Project(zone1.GetCurrentX()[p1] - zone0.GetCurrentX()[p0],
                zone1.GetCurrentY()[p1] - zone0.GetCurrentY()[p0]);
    }

    /// <summary>
/// Returns the original (unhinted) projected distance from point p0 in zone0 to
/// point p1 in zone1, measured along the dual projection vector.
/// </summary>
    public int DualProjectedDistance(Zone zone1, int p1, Zone zone0, int p0)
    {
        CheckPoint(zone1, p1);
        CheckPoint(zone0, p0);
        return DualProject(zone1.GetOriginalX()[p1] - zone0.GetOriginalX()[p0],
                zone1.GetOriginalY()[p1] - zone0.GetOriginalY()[p0]);
    }

    /// <summary>
/// Validates a point number taken from the bytecode (an operand, or a reference point a program
/// set) against the zone it indexes. Out-of-range point numbers are malformed font data, not a
/// bug here; FreeType checks each one (BOUNDS) and so do we, so such a glyph falls back to
/// its raw outline rather than failing with an array index error.
/// 
/// @param zone the zone the point is in
/// @param point the point number
/// @return point
/// @throws HintingException if the zone has no such point
/// </summary>
    public static int CheckPoint(Zone zone, int point)
    {
        if (point < 0 || point >= zone.GetPointCount())
        {
            throw new HintingException("point " + point + " output of range, zone has "
                    + zone.GetPointCount() + " points");
        }
        return point;
    }

    /// <summary>
/// Moves a point by the given projected distance along the freedom vector, touching the axes the
/// freedom vector acts on. The displacement is distance * freedom / (freedom . projection)
/// (see #freedomDotProjection()), which reduces to distance when both vectors are
/// the same axis. After FreeType 2.13's Direct_Move, the version the golden data comes from.
/// 
/// @param zone the zone holding the point
/// @param point the point index
/// @param distance the projected distance to move, in F26Dot6
/// </summary>
    public void MovePoint(Zone zone, int point, int distance)
    {
        CheckPoint(zone, point);
        UnitVector fv = graphicsState.GetFreedomVector();
        int fDotP = FreedomDotProjection();
        if (fv.GetX() != 0)
        {
            // backward-compatibility (v40 grayscale): never grid-fit in the x direction, so horizontal
            // stems keep their natural sub-pixel position and are not darkened by antialiasing
            if (!backwardCompatibility)
            {
                zone.GetCurrentX()[point] += Fixed.MulDiv(distance, fv.GetX(), fDotP);
            }
            zone.GetTouchedX()[point] = true;
        }
        if (fv.GetY() != 0)
        {
            // y moves are allowed until IUP has run on both axes; afterwards the glyph is frozen
            if (!(backwardCompatibility && iupxCalled && iupyCalled))
            {
                zone.GetCurrentY()[point] += Fixed.MulDiv(distance, fv.GetY(), fDotP);
            }
            zone.GetTouchedY()[point] = true;
        }
    }

    /// <summary>
/// Moves a point's &lt;em&gt;original&lt;/em&gt; position by the given projected distance along the freedom
/// vector, as #movePoint does for the current position. Nothing is touched and the
/// backward-compatibility rules do not apply. Used to place twilight points (FreeType's
/// func_move_orig).
/// 
/// @param zone the zone holding the point
/// @param point the point index
/// @param distance the projected distance to move, in F26Dot6
/// </summary>
    public void MoveOriginal(Zone zone, int point, int distance)
    {
        CheckPoint(zone, point);
        UnitVector fv = graphicsState.GetFreedomVector();
        int fDotP = FreedomDotProjection();
        if (fv.GetX() != 0)
        {
            zone.GetOriginalX()[point] += Fixed.MulDiv(distance, fv.GetX(), fDotP);
        }
        if (fv.GetY() != 0)
        {
            zone.GetOriginalY()[point] += Fixed.MulDiv(distance, fv.GetY(), fDotP);
        }
    }

    /// <summary>
/// The dot product of the freedom and projection vectors in F2Dot14, as FreeType 2.13's
/// Compute_Funcs derives it: exactly the projection component when the freedom vector is an axis,
/// otherwise truncated; below 0x400 in magnitude (nearly perpendicular) it is replaced by 1.0 to
/// avoid huge moves.
/// </summary>
    private int FreedomDotProjection()
    {
        UnitVector fv = graphicsState.GetFreedomVector();
        UnitVector pv = graphicsState.GetProjectionVector();
        int fDotP;
        if (fv.GetX() == Fixed.ONE_F2DOT14)
        {
            fDotP = pv.GetX();
        }
        else if (fv.GetY() == Fixed.ONE_F2DOT14)
        {
            fDotP = pv.GetY();
        }
        else
        {
            fDotP = (int) (((long) pv.GetX() * fv.GetX() + (long) pv.GetY() * fv.GetY()) >> 14);
        }
        return Math.Abs(fDotP) < 0x400 ? Fixed.ONE_F2DOT14 : fDotP;
    }

    /// <summary>
/// @return whether v40 backward-compatibility (grayscale subpixel) movement rules are active
/// </summary>
    public bool IsBackwardCompatibility()
    {
        return backwardCompatibility;
    }

    /// <summary>
/// @param value whether to apply v40 backward-compatibility movement rules (glyph program only)
/// </summary>
    public void SetBackwardCompatibility(bool value)
    {
        this.backwardCompatibility = value;
    }

    /// <summary>
/// @return which top-level program this context runs
/// </summary>
    public CodeRange GetCodeRange()
    {
        return codeRange;
    }

    /// <summary>
/// @param value which top-level program this context runs
/// </summary>
    public void SetCodeRange(CodeRange value)
    {
        this.codeRange = value;
    }

    /// <summary>
/// Marks IUP[x] as having run; resets each program run.
/// </summary>
    public void SetIupxCalled()
    {
        this.iupxCalled = true;
    }

    /// <summary>
/// Marks IUP[y] as having run; resets each program run.
/// </summary>
    public void SetIupyCalled()
    {
        this.iupyCalled = true;
    }

    /// <summary>
/// @return whether IUP has run on both axes (the glyph is frozen for backward compatibility)
/// </summary>
    public bool IsIupDone()
    {
        return iupxCalled && iupyCalled;
    }

    /// <summary>
/// @return whether the running program belongs to a composite glyph
/// </summary>
    public bool IsComposite()
    {
        return composite;
    }

    /// <summary>
/// @param value whether the running program belongs to a composite glyph
/// </summary>
    public void SetComposite(bool value)
    {
        this.composite = value;
    }

    // --- execution cursor and call state ---------------------------------

    /// <summary>
/// @return the current bytecode stream
/// </summary>
    public BytecodeStream GetStream()
    {
        return stream;
    }

    /// <summary>
/// @param stream the current bytecode stream
/// </summary>
    public void SetStream(BytecodeStream stream)
    {
        this.stream = stream;
    }

    /// <summary>
/// @return the current call nesting depth
/// </summary>
    public int GetCallDepth()
    {
        return callDepth;
    }

    /// <summary>
/// Increments the call nesting depth.
/// </summary>
    public void EnterCall()
    {
        callDepth++;
    }

    /// <summary>
/// Decrements the call nesting depth.
/// </summary>
    public void LeaveCall()
    {
        callDepth--;
    }

    /// <summary>
/// @return true if an ENDF asked the current function body to return
/// </summary>
    public bool IsReturnFromFunction()
    {
        return returnFromFunction;
    }

    /// <summary>
/// @param value whether the current function body should return
/// </summary>
    public void SetReturnFromFunction(bool value)
    {
        this.returnFromFunction = value;
    }

    /// <summary>
/// The top-level program a context runs, after FreeType's code ranges. Some instructions behave
/// differently depending on it: INSTCTRL, for one, only takes effect in prep.
/// </summary>
    public enum CodeRange
    {
        /// <summary>
/// the font program, fpgm
/// </summary>
        FONT,
        /// <summary>
/// the control value program, prep
/// </summary>
        CONTROL_VALUE,
        /// <summary>
/// a glyph program
/// </summary>
        GLYPH
    }
}
