/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/TrueTypeInterpreter.java
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
/// The TrueType bytecode interpreter: the VM driver and the 256-entry opcode dispatch table.
/// &lt;p&gt;
/// It carries the execution engine - the BytecodeStream cursor driven dispatch loop, function
/// definition and calling with a depth cap, branching, the push family, and the
/// stack/arithmetic/logical/storage and point-moving opcodes - plus the size lifecycle: fpgm
/// executed once, prep executed per ppem change with the result saved as the per-glyph
/// template. An opcode with no handler and no IDEF binding throws HintingException,
/// which the caller catches per glyph and falls back to the raw outline.
/// 
/// @author Apache PDFBox
/// </summary>
internal class TrueTypeInterpreter
{
    /// <summary>
/// Maximum CALL/LOOPCALL nesting depth, matching FreeType.
/// </summary>
    internal const int MAX_CALL_DEPTH = 64;

    /// <summary>
/// A single opcode handler.
/// </summary>
    
    private delegate void OpHandler(ExecutionContext ctx);

    // opcodes referenced by the engine itself (control flow / push)
    private const int NPUSHB = 0x40;
    private const int NPUSHW = 0x41;
    private const int PUSHB_BASE = 0xB0;
    private const int PUSHW_BASE = 0xB8;
    private const int ELSE = 0x1B;
    private const int IF = 0x58;
    private const int EIF = 0x59;
    private const int FDEF = 0x2C;
    private const int ENDF = 0x2D;
    private const int IDEF = 0x89;

    private OpHandler[] dispatch = new OpHandler[256];
    private Dictionary<int, FunctionDef> functions = new();
    private Dictionary<int, FunctionDef> instructionDefs = new();

    private int maxStackElements;
    private int unitsPerEm;

    // The storage area and twilight zone belong to the size, not to one program run: a font may seed
    // them in prep and read them back from every glyph program. FreeType keeps both on the TT_Size and
    // clears them in tt_size_run_prep, which is what setPpem does below.
    private int[] storage;
    private Zone twilightZone;

    private byte[]? fontProgram;
    private byte[]? controlValueProgram;
    private int[] rawControlValues = new int[0];
    private int[] scaledControlValues = new int[0];

    private int ppem;
    private int pointSize;
    private GraphicsState? savedState;
    private ExecutionTracer? tracer;

    /// <summary>
/// @param maxStackElements operand stack capacity (from maxp)
/// @param maxStorage storage area size (from maxp)
/// @param maxTwilightPoints twilight zone size (from maxp)
/// @param unitsPerEm the font's unitsPerEm (from head)
/// </summary>
    public TrueTypeInterpreter(int maxStackElements, int maxStorage, int maxTwilightPoints,
            int unitsPerEm)
    {
        this.maxStackElements = maxStackElements;
        this.unitsPerEm = unitsPerEm;
        this.storage = new int[Math.Max(maxStorage, 0)];
        this.twilightZone = new Zone(Math.Max(maxTwilightPoints, 0), 0);
        BuildDispatch();
    }

    // --- configuration ---------------------------------------------------

    /// <summary>
/// @param program the raw fpgm bytecode, or null
/// </summary>
    public void SetFontProgram(byte[]? program)
    {
        this.fontProgram = program;
    }

    /// <summary>
/// @param program the raw prep bytecode, or null
/// </summary>
    public void SetControlValueProgram(byte[]? program)
    {
        this.controlValueProgram = program;
    }

    /// <summary>
/// @param values the raw control values in font units, or null
/// </summary>
    public void SetControlValues(int[]? values)
    {
        this.rawControlValues = values != null ? values : new int[0];
    }

    /// <summary>
/// Attaches (or clears with null) an execution tracer that emits one FreeType-comparable
/// line per executed instruction. Used by the trace-diff tooling to localize divergence; off in
/// normal operation.
/// 
/// @param tracer the tracer, or null to disable tracing
/// </summary>
    public void SetTracer(ExecutionTracer? tracer)
    {
        this.tracer = tracer;
    }

    // --- lifecycle -------------------------------------------------------

    /// <summary>
/// Runs the font program (fpgm) once, populating the function table. Safe to call when
/// there is no font program.
/// </summary>
    public void PrepareFontProgram()
    {
        functions.Clear();
        instructionDefs.Clear();
        if (fontProgram == null || fontProgram.Length == 0)
        {
            return;
        }
        ExecutionContext ctx = NewContext(new GraphicsState());
        ctx.SetCodeRange(ExecutionContext.CodeRange.FONT);
        Run(ctx, new BytecodeStream(fontProgram));
    }

    /// <summary>
/// Establishes a new ppem: scales the control values, clears the storage area and twilight zone, runs
/// the control value program (prep) from a default graphics state, and saves the resulting
/// state as the per-glyph template. Whatever prep leaves in storage and the twilight zone
/// stays there for every glyph hinted at this size, which is why they are cleared here and not per
/// glyph - after FreeType's tt_size_run_prep. Anything the font program wrote to storage is
/// discarded, again as FreeType does: fpgm is only meant to define functions.
/// 
/// @param ppemValue the pixels-per-em to render at
/// @param pointSizeValue the point size
/// </summary>
    public void SetPpem(int ppemValue, int pointSizeValue)
    {
        this.ppem = ppemValue;
        this.pointSize = pointSizeValue;
        ScaleControlValues();
        Array.Fill(storage, 0);
        twilightZone.Reset();

        GraphicsState gs = new GraphicsState();
        if (controlValueProgram != null && controlValueProgram.Length > 0)
        {
            ExecutionContext ctx = NewContext(gs);
            ctx.SetCodeRange(ExecutionContext.CodeRange.CONTROL_VALUE);
            Run(ctx, new BytecodeStream(controlValueProgram));
        }
        savedState = gs;
    }

    private void ScaleControlValues()
    {
        scaledControlValues = new int[rawControlValues.Length];
        for (int i = 0; i < rawControlValues.Length; i++)
        {
            scaledControlValues[i] = Fixed.Scale(rawControlValues[i], ppem, unitsPerEm);
        }
    }

    /// <summary>
/// Builds a fresh execution context wired to this interpreter's sizes, scaled control values and the
/// current ppem. The stack and per-run counters are new; the storage area and twilight zone are the
/// interpreter's own, so values prep left there are visible to the glyph programs.
/// 
/// @param gs the graphics state the context starts from
/// @return a new execution context
/// </summary>
    public ExecutionContext NewContext(GraphicsState gs)
    {
        ExecutionContext ctx = new ExecutionContext(this, gs, maxStackElements, storage,
                scaledControlValues, twilightZone);
        ctx.SetUnitsPerEm(unitsPerEm);
        ctx.SetPpem(ppem);
        ctx.SetPointSize(pointSize);
        return ctx;
    }

    /// <summary>
/// Test/utility entry point: runs a standalone bytecode program from the saved (post-prep)
/// state, or a default state if no size has been set, and returns the resulting context so callers
/// can inspect the stack and state.
/// 
/// @param program the bytecode to run
/// @param ppemValue the ppem to run at
/// @return the execution context after the program completes
/// </summary>
    public ExecutionContext ExecuteProgram(byte[] program, int ppemValue)
    {
        this.ppem = ppemValue;
        GraphicsState gs = savedState != null ? savedState.Copy() : new GraphicsState();
        ExecutionContext ctx = NewContext(gs);
        Run(ctx, new BytecodeStream(program));
        return ctx;
    }

    /// <summary>
/// @return the saved post-prep graphics state, or null if no size has been set
/// </summary>
    public GraphicsState GetSavedState()
    {
        return savedState;
    }

    /// <summary>
/// @return the function table populated by fpgm
/// </summary>
    public Dictionary<int, FunctionDef> GetFunctions()
    {
        return functions;
    }

    // --- execution engine ------------------------------------------------

    /// <summary>
/// Runs a bytecode stream to completion (or until an ENDF returns from a function body),
/// dispatching each opcode through the table.
/// 
/// @param ctx the execution context
/// @param s the stream to run
/// </summary>
    public void Run(ExecutionContext ctx, BytecodeStream s)
    {
        BytecodeStream previous = ctx.GetStream();
        ctx.SetStream(s);
        try
        {
            while (s.HasNext() && !ctx.IsReturnFromFunction())
            {
                s.MarkInstructionStart();
                int opcode = s.NextByte();
                if (tracer != null)
                {
                    tracer.Trace(s.InstructionStart(), opcode, ctx);
                }
                dispatch[opcode](ctx);
            }
        }
        finally
        {
            ctx.SetStream(previous);
        }
    }

    /// <summary>
/// Calls the function with the given number, running its body until the matching ENDF.
/// 
/// @param ctx the execution context
/// @param functionNumber the function to call
/// @throws HintingException if the function is undefined or the call depth is exceeded
/// </summary>
    public void CallFunction(ExecutionContext ctx, int functionNumber)
    {
        FunctionDef def = functions.GetValueOrDefault(functionNumber);
        if (def == null)
        {
            throw new HintingException("call to undefined function " + functionNumber);
        }
        CallBody(ctx, def);
    }

    /// <summary>
/// Runs a function/instruction body from its entry point until the matching ENDF.
/// </summary>
    private void CallBody(ExecutionContext ctx, FunctionDef def)
    {
        if (ctx.GetCallDepth() >= MAX_CALL_DEPTH)
        {
            throw new HintingException("maximum call depth " + MAX_CALL_DEPTH + " exceeded");
        }
        ctx.EnterCall();
        try
        {
            BytecodeStream body = new BytecodeStream(def.GetProgram());
            body.SetEnd(def.GetEnd());
            body.Seek(def.GetEntryPoint());
            Run(ctx, body);
            ctx.SetReturnFromFunction(false);
        }
        finally
        {
            ctx.LeaveCall();
        }
    }

    private void DefineFunction(ExecutionContext ctx)
    {
        int functionNumber = ctx.Pop();
        BytecodeStream s = ctx.GetStream();
        int entry = s.Position();
        int end = SkipFunctionBody(s);
        functions[functionNumber] = new FunctionDef(s.GetCode(), entry, end);
    }

    /// <summary>
/// IDEF: binds the opcode on top of the stack to the following instructions (until ENDF).
/// </summary>
    private void DefineInstruction(ExecutionContext ctx)
    {
        int opcode = ctx.Pop();
        BytecodeStream s = ctx.GetStream();
        int entry = s.Position();
        int end = SkipFunctionBody(s);
        instructionDefs[opcode & 0xFF] = new FunctionDef(s.GetCode(), entry, end);
    }

    /// <summary>
/// Skips to the ENDF closing a function/instruction body, leaving the stream after it.
/// A nested FDEF/IDEF is an error, as in FreeType (Nested_DEFS).
/// 
/// @return the position of the ENDF
/// </summary>
    private int SkipFunctionBody(BytecodeStream s)
    {
        while (s.HasNext())
        {
            int opcode = s.NextByte();
            if (opcode == ENDF)
            {
                return s.Position() - 1;
            }
            if (opcode == FDEF || opcode == IDEF)
            {
                throw new HintingException("nested function definition");
            }
            SkipPushOperands(opcode, s);
        }
        throw new HintingException("FDEF without matching ENDF");
    }

    /// <summary>
/// On a false IF, skips forward to the matching ELSE or EIF, accounting for
/// nested IF blocks and for the inline operands of push instructions. Leaves the stream
/// positioned just after the terminator.
/// </summary>
    private void SkipToElseOrEif(BytecodeStream s)
    {
        int depth = 0;
        while (s.HasNext())
        {
            int opcode = s.NextByte();
            if (opcode == IF)
            {
                depth++;
            }
            else if (opcode == EIF)
            {
                if (depth == 0)
                {
                    return;
                }
                depth--;
            }
            else if (opcode == ELSE && depth == 0)
            {
                return;
            }
            else
            {
                SkipPushOperands(opcode, s);
            }
        }
        throw new HintingException("IF without matching EIF");
    }

    /// <summary>
/// After a true IF branch reaches its ELSE, skips the else-branch to the matching
/// EIF.
/// </summary>
    private void SkipToEif(BytecodeStream s)
    {
        int depth = 0;
        while (s.HasNext())
        {
            int opcode = s.NextByte();
            if (opcode == IF)
            {
                depth++;
            }
            else if (opcode == EIF)
            {
                if (depth == 0)
                {
                    return;
                }
                depth--;
            }
            else
            {
                SkipPushOperands(opcode, s);
            }
        }
        throw new HintingException("ELSE without matching EIF");
    }

    /// <summary>
/// Advances the stream past the inline operands of a push opcode; a no-op for other opcodes.
/// </summary>
    private void SkipPushOperands(int opcode, BytecodeStream s)
    {
        if (opcode == NPUSHB)
        {
            s.Skip(s.NextByte());
        }
        else if (opcode == NPUSHW)
        {
            s.Skip(2 * s.NextByte());
        }
        else if (opcode >= PUSHB_BASE && opcode <= PUSHB_BASE + 7)
        {
            s.Skip(opcode - PUSHB_BASE + 1);
        }
        else if (opcode >= PUSHW_BASE && opcode <= PUSHW_BASE + 7)
        {
            s.Skip(2 * (opcode - PUSHW_BASE + 1));
        }
    }

    // --- dispatch table --------------------------------------------------

    private void BuildDispatch()
    {
        for (int i = 0; i < dispatch.Length; i++)
        {
            int opcode = i;
            dispatch[i] = ctx =>
            {
                // an opcode with no built-in handler may have been given one by IDEF
                FunctionDef def = instructionDefs.GetValueOrDefault(opcode);
                if (def != null)
                {
                    CallBody(ctx, def);
                    return;
                }
                throw new HintingException(
                        string.Format("unsupported TrueType opcode 0x%02X", opcode));
            };
        }

        InstallPushOps();
        InstallStackOps();
        InstallArithmeticOps();
        InstallLogicalOps();
        InstallFlowOps();
        InstallStateOps();
        InstallStorageAndCvtOps();
        InstallMiscOps();
        InstallVectorOps();
        InstallRoundOps();
        InstallPointOps();
        InstallInterpolationOps();
        InstallMeasureOps();
        InstallDeltaOps();
        InstallFlipOps();
    }

    private void InstallPushOps()
    {
        dispatch[NPUSHB] = ctx =>
        {
            int n = ctx.GetStream().NextByte();
            for (int i = 0; i < n; i++)
            {
                ctx.Push(ctx.GetStream().NextByte());
            }
        };
        dispatch[NPUSHW] = ctx =>
        {
            int n = ctx.GetStream().NextByte();
            for (int i = 0; i < n; i++)
            {
                ctx.Push(ctx.GetStream().NextWord());
            }
        };
        for (int k = 0; k < 8; k++)
        {
            int count = k + 1;
            dispatch[PUSHB_BASE + k] = ctx =>
            {
                for (int i = 0; i < count; i++)
                {
                    ctx.Push(ctx.GetStream().NextByte());
                }
            };
            dispatch[PUSHW_BASE + k] = ctx =>
            {
                for (int i = 0; i < count; i++)
                {
                    ctx.Push(ctx.GetStream().NextWord());
                }
            };
        }
    }

    private void InstallStackOps()
    {
        dispatch[0x20] = ctx => ctx.Push(ctx.Peek(0));                 // DUP
        dispatch[0x21] = ctx => { ctx.Pop(); };                       // POP
        dispatch[0x22] = ctx => ctx.ClearStack();                // CLEAR
        dispatch[0x23] = ctx =>                                       // SWAP
        {
            int a = ctx.Pop();
            int b = ctx.Pop();
            ctx.Push(a);
            ctx.Push(b);
        };
        dispatch[0x24] = ctx => ctx.Push(ctx.GetStackDepth());        // DEPTH
        // CINDEX/MINDEX: k counts from the top, 1-based. An output-of-range k is ignored (the index
        // operand has been consumed and the stack is otherwise left alone), as FreeType does outside
        // pedantic mode, rather than aborting the glyph.
        dispatch[0x25] = ctx =>                                       // CINDEX
        {
            int k = ctx.Pop();
            if (k > 0 && k <= ctx.GetStackDepth())
            {
                ctx.Push(ctx.Peek(k - 1));
            }
        };
        dispatch[0x26] = ctx =>                                       // MINDEX
        {
            int k = ctx.Pop();
            if (k <= 0 || k > ctx.GetStackDepth())
            {
                return;
            }
            // move the k-th element to the top; the k-1 elements above it keep their order
            int[] above = new int[k - 1];
            for (int i = k - 2; i >= 0; i--)
            {
                above[i] = ctx.Pop();
            }
            int moved = ctx.Pop();
            for (int i = 0; i < above.Length; i++)
            {
                ctx.Push(above[i]);
            }
            ctx.Push(moved);
        };
        dispatch[0x8A] = ctx =>                                       // ROLL
        {
            int c = ctx.Pop();
            int b = ctx.Pop();
            int a = ctx.Pop();
            ctx.Push(b);
            ctx.Push(c);
            ctx.Push(a);
        };
    }

    private void InstallArithmeticOps()
    {
        dispatch[0x60] = ctx => Binary(ctx, (a, b) => a + b);            // ADD
        dispatch[0x61] = ctx => Binary(ctx, (a, b) => a - b);            // SUB
        dispatch[0x62] = ctx => Binary(ctx, Fixed.Div);                 // DIV
        dispatch[0x63] = ctx => Binary(ctx, Fixed.Mul);                 // MUL
        dispatch[0x64] = ctx => ctx.Push(Math.Abs(ctx.Pop()));           // ABS
        dispatch[0x65] = ctx => ctx.Push(-ctx.Pop());                    // NEG
        dispatch[0x66] = ctx => ctx.Push(Fixed.Floor(ctx.Pop()));        // FLOOR
        dispatch[0x67] = ctx => ctx.Push(Fixed.Ceil(ctx.Pop()));         // CEILING
        dispatch[0x8B] = ctx => Binary(ctx, Math.Max);                  // MAX
        dispatch[0x8C] = ctx => Binary(ctx, Math.Min);                  // MIN
    }

    private void InstallLogicalOps()
    {
        dispatch[0x50] = ctx => Binary(ctx, (a, b) => Bool(a < b));      // LT
        dispatch[0x51] = ctx => Binary(ctx, (a, b) => Bool(a <= b));     // LTEQ
        dispatch[0x52] = ctx => Binary(ctx, (a, b) => Bool(a > b));      // GT
        dispatch[0x53] = ctx => Binary(ctx, (a, b) => Bool(a >= b));     // GTEQ
        dispatch[0x54] = ctx => Binary(ctx, (a, b) => Bool(a == b));     // EQ
        dispatch[0x55] = ctx => Binary(ctx, (a, b) => Bool(a != b));     // NEQ
        // ODD/EVEN round with the *current round state* (not plain round-to-grid) and then test the
        // parity of the resulting pixel, as the spec says and FreeType does (Ins_ODD / Ins_EVEN)
        dispatch[0x56] = ctx => ctx.Push(Bool((ctx.GetGraphicsState().Round(ctx.Pop()) & 127) == 64)); // ODD
        dispatch[0x57] = ctx => ctx.Push(Bool((ctx.GetGraphicsState().Round(ctx.Pop()) & 127) == 0));  // EVEN
        dispatch[0x5A] = ctx => Binary(ctx, (a, b) => Bool(a != 0 && b != 0)); // AND
        dispatch[0x5B] = ctx => Binary(ctx, (a, b) => Bool(a != 0 || b != 0)); // OR
        dispatch[0x5C] = ctx => ctx.Push(Bool(ctx.Pop() == 0));          // NOT
    }

    private void InstallFlowOps()
    {
        dispatch[IF] = ctx =>
        {
            if (ctx.Pop() == 0)
            {
                SkipToElseOrEif(ctx.GetStream());
            }
        };
        dispatch[ELSE] = ctx => SkipToEif(ctx.GetStream());
        dispatch[EIF] = ctx => { /* no-op terminator */ };
        dispatch[0x1C] = ctx => Jump(ctx, ctx.Pop());                    // JMPR
        dispatch[0x78] = ctx =>                                          // JROT
        {
            int e = ctx.Pop();
            int offset = ctx.Pop();
            if (e != 0)
            {
                Jump(ctx, offset);
            }
        };
        dispatch[0x79] = ctx =>                                          // JROF
        {
            int e = ctx.Pop();
            int offset = ctx.Pop();
            if (e == 0)
            {
                Jump(ctx, offset);
            }
        };
        dispatch[FDEF] = this.DefineFunction;
        dispatch[ENDF] = ctx =>
        {
            // ENDF outside a function body is an error in FreeType (ENDF_In_Exec_Stream); an FDEF
            // that was skipped correctly never lets its ENDF execute at the top level
            if (ctx.GetCallDepth() <= 0)
            {
                throw new HintingException("ENDF outside a function body");
            }
            ctx.SetReturnFromFunction(true);
        };
        dispatch[IDEF] = this.DefineInstruction;
        dispatch[0x2B] = ctx => CallFunction(ctx, ctx.Pop());            // CALL
        dispatch[0x2A] = ctx =>                                          // LOOPCALL
        {
            int functionNumber = ctx.Pop();
            int count = ctx.Pop();
            // the spec calls the count unsigned; FreeType runs nothing at all when it is not positive
            if (count <= 0)
            {
                return;
            }
            // charge the whole loop up front, so an absurd count fails before a single iteration runs
            ctx.CountLoopCalls(count);
            for (int i = 0; i < count; i++)
            {
                CallFunction(ctx, functionNumber);
            }
        };
    }

    /// <summary>
/// Jumps to offset bytes from the start of the current instruction. A backward jump is the
/// only way TrueType bytecode can loop other than LOOPCALL, so those are counted against the
/// run's budget and the program is abandoned once it exceeds it.
/// 
/// @param ctx the execution context
/// @param offset the jump offset, relative to the current instruction
/// @throws HintingException if the stream position is output of range, or too many backward jumps
/// </summary>
    private static void Jump(ExecutionContext ctx, int offset)
    {
        if (offset < 0)
        {
            ctx.CountNegativeJump();
        }
        BytecodeStream s = ctx.GetStream();
        s.Seek(s.InstructionStart() + offset);
    }

    private void InstallStateOps()
    {
        dispatch[0x17] = ctx => ctx.GetGraphicsState().SetLoop(ctx.Pop());            // SLOOP
        dispatch[0x10] = ctx => ctx.GetGraphicsState().SetRp0(ctx.Pop());            // SRP0
        dispatch[0x11] = ctx => ctx.GetGraphicsState().SetRp1(ctx.Pop());            // SRP1
        dispatch[0x12] = ctx => ctx.GetGraphicsState().SetRp2(ctx.Pop());            // SRP2
        dispatch[0x1A] = ctx => ctx.GetGraphicsState().SetMinimumDistance(ctx.Pop()); // SMD
        dispatch[0x5E] = ctx => ctx.GetGraphicsState().SetDeltaBase(ctx.Pop());      // SDB
        dispatch[0x5F] = ctx => ctx.GetGraphicsState().SetDeltaShift(ctx.Pop());     // SDS
        dispatch[0x1D] = ctx => ctx.GetGraphicsState().SetControlValueCutIn(ctx.Pop()); // SCVTCI
        dispatch[0x1E] = ctx => ctx.GetGraphicsState().SetSingleWidthCutIn(ctx.Pop()); // SSWCI
        dispatch[0x1F] = ctx => ctx.GetGraphicsState().SetSingleWidthValue(ctx.Pop()); // SSW

        dispatch[0x18] = RoundState(GraphicsState.ROUND_TO_GRID);        // RTG
        dispatch[0x19] = RoundState(GraphicsState.ROUND_TO_HALF_GRID);   // RTHG
        dispatch[0x3D] = RoundState(GraphicsState.ROUND_TO_DOUBLE_GRID); // RTDG
        dispatch[0x7C] = RoundState(GraphicsState.ROUND_UP_TO_GRID);     // RUTG
        dispatch[0x7D] = RoundState(GraphicsState.ROUND_DOWN_TO_GRID);   // RDTG
        dispatch[0x7A] = RoundState(GraphicsState.ROUND_OFF);            // ROFF
        // grid period in F2Dot14: one pixel for SROUND, the 45-degree diagonal sqrt(2)/2 for S45ROUND
        dispatch[0x76] = ctx => ctx.GetGraphicsState().SetSuperRound(0x4000, ctx.Pop()); // SROUND
        dispatch[0x77] = ctx => ctx.GetGraphicsState().SetSuperRound(0x2D41, ctx.Pop()); // S45ROUND
        dispatch[0x13] = ctx => ctx.GetGraphicsState().SetZp0(ctx.Pop());            // SZP0
        dispatch[0x14] = ctx => ctx.GetGraphicsState().SetZp1(ctx.Pop());            // SZP1
        dispatch[0x15] = ctx => ctx.GetGraphicsState().SetZp2(ctx.Pop());            // SZP2
        dispatch[0x16] = ctx =>                                                      // SZPS
        {
            int zone = ctx.Pop();
            GraphicsState gs = ctx.GetGraphicsState();
            gs.SetZp0(zone);
            gs.SetZp1(zone);
            gs.SetZp2(zone);
        };
    }

    private void InstallStorageAndCvtOps()
    {
        dispatch[0x43] = ctx =>                                          // RS
        {
            int index = ctx.Pop();
            ctx.Push(Read(ctx.GetStorage(), index, "storage"));
        };
        dispatch[0x42] = ctx =>                                          // WS
        {
            int value = ctx.Pop();
            int index = ctx.Pop();
            Write(ctx.GetStorage(), index, value, "storage");
        };
        dispatch[0x45] = ctx =>                                          // RCVT
        {
            int index = ctx.Pop();
            ctx.Push(Read(ctx.GetControlValues(), index, "cvt"));
        };
        dispatch[0x44] = ctx =>                                          // WCVTP
        {
            int value = ctx.Pop();
            int index = ctx.Pop();
            Write(ctx.GetControlValues(), index, value, "cvt");
        };
        dispatch[0x70] = ctx =>                                          // WCVTF
        {
            int value = ctx.Pop();
            int index = ctx.Pop();
            Write(ctx.GetControlValues(), index,
                    Fixed.Scale(value, ctx.GetPpem(), ctx.GetUnitsPerEm()), "cvt");
        };
    }

    private void InstallMiscOps()
    {
        dispatch[0x4B] = ctx => ctx.Push(ctx.GetPpem());                 // MPPEM
        dispatch[0x4C] = ctx => ctx.Push(ctx.GetPointSize());            // MPS
        dispatch[0x4F] = ctx => { ctx.Pop(); };                          // DEBUG (pops, no-op)
        dispatch[0x7E] = ctx => { ctx.Pop(); };                          // SANGW (deprecated, pops)
        dispatch[0x7F] = ctx => { /* AA - deprecated no-op */ };         // AA
        dispatch[0x88] = ctx =>                                          // GETINFO
        {
            int selector = ctx.Pop();
            int result = 0;
            if ((selector & 0x0001) != 0)
            {
                // rasterizer version 40: FreeType's "minimal" subpixel interpreter, which we mirror
                // for grayscale antialiased rendering (lighter stems than the classic v35)
                result |= 40;
            }
            // we always render grayscale-subpixel ("lean"), non-LCD: report the subpixel bits a v40
            // grayscale rasterizer returns so fonts take their lighter ClearType-aware code paths.
            // (the grayscale bit 12 is intentionally not set: FreeType clears exc->grayscale in lean mode)
            if ((selector & 0x0040) != 0)
            {
                result |= 1 << 13; // subpixel hinting active
            }
            if ((selector & 0x0400) != 0)
            {
                result |= 1 << 17; // ClearType hinting active
            }
            if ((selector & 0x0800) != 0)
            {
                result |= 1 << 18; // subpixel positioned
            }
            if ((selector & 0x1000) != 0)
            {
                result |= 1 << 19; // grayscale ClearType
            }
            ctx.Push(result);
        };
    }

    // --- vector setters --------------------------------------------------

    private void InstallVectorOps()
    {
        dispatch[0x00] = ctx => SetProjAndFreedomAxis(ctx, false); // SVTCA[0] y
        dispatch[0x01] = ctx => SetProjAndFreedomAxis(ctx, true);  // SVTCA[1] x
        dispatch[0x02] = ctx => SetProjectionAxis(ctx, false);     // SPVTCA[0] y
        dispatch[0x03] = ctx => SetProjectionAxis(ctx, true);      // SPVTCA[1] x
        dispatch[0x04] = ctx => SetFreedomAxis(ctx, false);        // SFVTCA[0] y
        dispatch[0x05] = ctx => SetFreedomAxis(ctx, true);         // SFVTCA[1] x
        dispatch[0x06] = ctx => SetProjectionToLine(ctx, false);   // SPVTL[0] parallel
        dispatch[0x07] = ctx => SetProjectionToLine(ctx, true);    // SPVTL[1] perpendicular
        dispatch[0x08] = ctx => SetFreedomToLine(ctx, false);      // SFVTL[0] parallel
        dispatch[0x09] = ctx => SetFreedomToLine(ctx, true);       // SFVTL[1] perpendicular
        dispatch[0x86] = ctx => SetDualProjectionToLine(ctx, false); // SDPVTL[0]
        dispatch[0x87] = ctx => SetDualProjectionToLine(ctx, true);  // SDPVTL[1]
        dispatch[0x0E] = ctx =>                                    // SFVTPV
        {
            UnitVector pv = ctx.GetGraphicsState().GetProjectionVector();
            ctx.GetGraphicsState().GetFreedomVector().Set(pv.GetX(), pv.GetY());
        };
        // SPVFS/SFVFS: the low 16 bits, sign-extended, normalized; (0,0) leaves the vector unchanged
        // (FreeType's Ins_SPVFS/Ins_SFVFS)
        dispatch[0x0A] = ctx =>                                    // SPVFS
        {
            int y = (short) ctx.Pop();
            int x = (short) ctx.Pop();
            if (x != 0 || y != 0)
            {
                UnitVector v = UnitVector.Normalize(x, y);
                ctx.GetGraphicsState().GetProjectionVector().Set(v.GetX(), v.GetY());
                ctx.GetGraphicsState().GetDualProjectionVector().Set(v.GetX(), v.GetY());
            }
        };
        dispatch[0x0B] = ctx =>                                    // SFVFS
        {
            int y = (short) ctx.Pop();
            int x = (short) ctx.Pop();
            if (x != 0 || y != 0)
            {
                UnitVector v = UnitVector.Normalize(x, y);
                ctx.GetGraphicsState().GetFreedomVector().Set(v.GetX(), v.GetY());
            }
        };
        dispatch[0x0C] = ctx =>                                    // GPV
        {
            UnitVector pv = ctx.GetGraphicsState().GetProjectionVector();
            ctx.Push(pv.GetX());
            ctx.Push(pv.GetY());
        };
        dispatch[0x0D] = ctx =>                                    // GFV
        {
            UnitVector fv = ctx.GetGraphicsState().GetFreedomVector();
            ctx.Push(fv.GetX());
            ctx.Push(fv.GetY());
        };
    }

    private static void SetProjAndFreedomAxis(ExecutionContext ctx, bool xAxis)
    {
        SetProjectionAxis(ctx, xAxis);
        SetFreedomAxis(ctx, xAxis);
    }

    private static void SetProjectionAxis(ExecutionContext ctx, bool xAxis)
    {
        int x = xAxis ? Fixed.ONE_F2DOT14 : 0;
        int y = xAxis ? 0 : Fixed.ONE_F2DOT14;
        ctx.GetGraphicsState().GetProjectionVector().Set(x, y);
        ctx.GetGraphicsState().GetDualProjectionVector().Set(x, y);
    }

    private static void SetFreedomAxis(ExecutionContext ctx, bool xAxis)
    {
        int x = xAxis ? Fixed.ONE_F2DOT14 : 0;
        int y = xAxis ? 0 : Fixed.ONE_F2DOT14;
        ctx.GetGraphicsState().GetFreedomVector().Set(x, y);
    }

    private void SetProjectionToLine(ExecutionContext ctx, bool perpendicular)
    {
        // FreeType's Ins_SPVTL: the dual vector follows the projection vector; only SDPVTL derives
        // it from the original outline
        UnitVector v = LineVector(ctx, perpendicular, false)[0];
        ctx.GetGraphicsState().GetProjectionVector().Set(v.GetX(), v.GetY());
        ctx.GetGraphicsState().GetDualProjectionVector().Set(v.GetX(), v.GetY());
    }

    private void SetFreedomToLine(ExecutionContext ctx, bool perpendicular)
    {
        UnitVector v = LineVector(ctx, perpendicular, false)[0];
        ctx.GetGraphicsState().GetFreedomVector().Set(v.GetX(), v.GetY());
    }

    private void SetDualProjectionToLine(ExecutionContext ctx, bool perpendicular)
    {
        UnitVector[] v = LineVector(ctx, perpendicular, true);
        ctx.GetGraphicsState().GetProjectionVector().Set(v[0].GetX(), v[0].GetY());
        ctx.GetGraphicsState().GetDualProjectionVector().Set(v[1].GetX(), v[1].GetY());
    }

    /// <summary>
/// Pops two point numbers and returns the unit vector along (or perpendicular to) the line between
/// them, as FreeType's Ins_SxVTL/Ins_SDPVTL do: the top of the stack is a point in zp2, the one
/// below it a point in zp1, and the line runs from the zp2 point to the zp1 point. A zero-length
/// line gives the x axis, ignoring the perpendicular flag.
/// 
/// @return {current-based vector, original-based vector (only when withOriginal, else
///         null)}
/// </summary>
    private UnitVector[] LineVector(ExecutionContext ctx, bool perpendicular,
            bool withOriginal)
    {
        GraphicsState gs = ctx.GetGraphicsState();
        int p2 = ctx.Pop();
        int p1 = ctx.Pop();
        Zone to = ctx.GetZone(gs.GetZp1());
        Zone from = ctx.GetZone(gs.GetZp2());
        ExecutionContext.CheckPoint(to, p1);
        ExecutionContext.CheckPoint(from, p2);
        UnitVector original = null;
        bool perp = perpendicular;
        if (withOriginal)
        {
            int dx = to.GetOriginalX()[p1] - from.GetOriginalX()[p2];
            int dy = to.GetOriginalY()[p1] - from.GetOriginalY()[p2];
            // FreeType clears the perpendicular flag on a zero-length original line and does not
            // restore it for the current line that follows
            perp &= dx != 0 || dy != 0;
            original = LineUnitVector(dx, dy, perp);
        }
        UnitVector current = LineUnitVector(to.GetCurrentX()[p1] - from.GetCurrentX()[p2],
                to.GetCurrentY()[p1] - from.GetCurrentY()[p2], perp);
        return new UnitVector[] { current, original };
    }

    private static UnitVector LineUnitVector(int dx, int dy, bool perpendicular)
    {
        if (dx == 0 && dy == 0)
        {
            return UnitVector.XAxis();
        }
        // rotate before normalizing, as FreeType does, so rounding matches for negative components
        return perpendicular ? UnitVector.Normalize(-dy, dx) : UnitVector.Normalize(dx, dy);
    }

    // --- rounding opcodes ------------------------------------------------

    private void InstallRoundOps()
    {
        for (int k = 0; k < 4; k++)
        {
            dispatch[0x68 + k] = ctx => ctx.Push(ctx.GetGraphicsState().Round(ctx.Pop())); // ROUND[ab]
            dispatch[0x6C + k] = ctx => ctx.Push(ctx.Pop());                                // NROUND[ab]
        }
    }

    // --- point movement --------------------------------------------------

    private void InstallPointOps()
    {
        dispatch[0x0F] = this.DoIsect;              // ISECT
        dispatch[0x2E] = ctx => DoMDAP(ctx, false);  // MDAP[0] no round
        dispatch[0x2F] = ctx => DoMDAP(ctx, true);   // MDAP[1] round
        dispatch[0x3E] = ctx => DoMIAP(ctx, false);  // MIAP[0] no round
        dispatch[0x3F] = ctx => DoMIAP(ctx, true);   // MIAP[1] round + cut-in
        dispatch[0x3A] = ctx => DoMSIRP(ctx, false); // MSIRP[0]
        dispatch[0x3B] = ctx => DoMSIRP(ctx, true);  // MSIRP[1] set rp0
        dispatch[0x3C] = this.DoAlignRp;            // ALIGNRP
        dispatch[0x27] = this.DoAlignPts;           // ALIGNPTS
        dispatch[0x29] = this.DoUtp;                // UTP
        dispatch[0x38] = this.DoShpix;              // SHPIX
        dispatch[0x32] = ctx => DoShp(ctx, false);   // SHP[0] rp2/zp1
        dispatch[0x33] = ctx => DoShp(ctx, true);    // SHP[1] rp1/zp0
        dispatch[0x34] = ctx => DoShc(ctx, false);   // SHC[0]
        dispatch[0x35] = ctx => DoShc(ctx, true);    // SHC[1]
        dispatch[0x36] = ctx => DoShz(ctx, false);   // SHZ[0]
        dispatch[0x37] = ctx => DoShz(ctx, true);    // SHZ[1]
        for (int op = 0xC0; op <= 0xDF; op++)
        {
            int code = op;
            dispatch[op] = ctx => DoMDRP(ctx, code); // MDRP[abcde]
        }
        for (int op = 0xE0; op <= 0xFF; op++)
        {
            int code = op;
            dispatch[op] = ctx => DoMIRP(ctx, code); // MIRP[abcde]
        }
    }

    /// <summary>
/// ISECT: moves a point to where line A (through a0 and a1, in zp1) crosses line B (through b0 and
/// b1, in zp0). With u = a1 - a0 and v = b1 - b0 the crossing is a0 + t u, where
/// t = ((b0 - a0) x v) / (u x v), solved exactly in integers. When the lines are within about 3
/// degrees of parallel (|u x v| at most 1/19 of |u . v|, the cut-off FreeType uses) the crossing
/// is too unstable to use, and the point goes to the average of the four line points instead.
/// </summary>
    private void DoIsect(ExecutionContext ctx)
    {
        GraphicsState gs = ctx.GetGraphicsState();
        int b1 = ctx.Pop();
        int b0 = ctx.Pop();
        int a1 = ctx.Pop();
        int a0 = ctx.Pop();
        int point = ctx.Pop();
        Zone lineA = ctx.GetZone(gs.GetZp1());
        Zone lineB = ctx.GetZone(gs.GetZp0());
        Zone target = ctx.GetZone(gs.GetZp2());
        ExecutionContext.CheckPoint(lineA, a0);
        ExecutionContext.CheckPoint(lineA, a1);
        ExecutionContext.CheckPoint(lineB, b0);
        ExecutionContext.CheckPoint(lineB, b1);
        ExecutionContext.CheckPoint(target, point);

        long startX = lineA.GetCurrentX()[a0];
        long startY = lineA.GetCurrentY()[a0];
        long ux = lineA.GetCurrentX()[a1] - startX;
        long uy = lineA.GetCurrentY()[a1] - startY;
        long otherX = lineB.GetCurrentX()[b0];
        long otherY = lineB.GetCurrentY()[b0];
        long vx = lineB.GetCurrentX()[b1] - otherX;
        long vy = lineB.GetCurrentY()[b1] - otherY;

        long cross = ux * vy - uy * vx;
        long dot = ux * vx + uy * vy;
        int x;
        int y;
        if (Math.Abs(cross) * 19 > Math.Abs(dot))
        {
            long along = (otherX - startX) * vy - (otherY - startY) * vx;
            x = (int) (startX + RoundedQuotient(along, ux, cross));
            y = (int) (startY + RoundedQuotient(along, uy, cross));
        }
        else
        {
            x = (int) ((startX + lineA.GetCurrentX()[a1] + otherX + lineB.GetCurrentX()[b1]) / 4);
            y = (int) ((startY + lineA.GetCurrentY()[a1] + otherY + lineB.GetCurrentY()[b1]) / 4);
        }
        target.GetCurrentX()[point] = x;
        target.GetCurrentY()[point] = y;
        target.GetTouchedX()[point] = true;
        target.GetTouchedY()[point] = true;
    }

    /// <summary>
/// a * b / c rounded to the nearest integer, halves away from zero, without overflow.
/// </summary>
    private static long RoundedQuotient(long a, long b, long c)
    {
        BigInteger product = new BigInteger(a) * new BigInteger(b);
        BigInteger divisor = new BigInteger(c);
        BigInteger q = BigInteger.DivRem(BigInteger.Abs(product), BigInteger.Abs(divisor), out BigInteger remainder);
        if ((remainder << 1) >= BigInteger.Abs(divisor))
        {
            q += BigInteger.One;
        }
        // Java BigInteger.longValue keeps the low 64 bits on overflow.
        BigInteger signed = product.Sign * divisor.Sign < 0 ? -q : q;
        return unchecked((long)(ulong)(signed & ulong.MaxValue));
    }

    private void DoMDAP(ExecutionContext ctx, bool round)
    {
        GraphicsState gs = ctx.GetGraphicsState();
        int point = ctx.Pop();
        Zone zone = ctx.GetZone(gs.GetZp0());
        ExecutionContext.CheckPoint(zone, point);
        int cur = ctx.Project(zone.GetCurrentX()[point], zone.GetCurrentY()[point]);
        int distance = round ? gs.Round(cur) : cur;
        ctx.MovePoint(zone, point, distance - cur);
        gs.SetRp0(point);
        gs.SetRp1(point);
    }

    private void DoMIAP(ExecutionContext ctx, bool round)
    {
        GraphicsState gs = ctx.GetGraphicsState();
        int cvtIndex = ctx.Pop();
        int point = ctx.Pop();
        Zone zone = ctx.GetZone(gs.GetZp0());
        ExecutionContext.CheckPoint(zone, point);
        int[] cvt = ctx.GetControlValues();
        int value = cvtIndex >= 0 && cvtIndex < cvt.Length ? cvt[cvtIndex] : 0;

        if (gs.GetZp0() == 0)
        {
            // twilight point: establish its position from the control value along the freedom
            // vector (FreeType's Ins_MIAP)
            UnitVector fv = gs.GetFreedomVector();
            zone.GetOriginalX()[point] = Fixed.Mul14(value, fv.GetX());
            zone.GetOriginalY()[point] = Fixed.Mul14(value, fv.GetY());
            zone.GetCurrentX()[point] = zone.GetOriginalX()[point];
            zone.GetCurrentY()[point] = zone.GetOriginalY()[point];
        }
        int cur = ctx.Project(zone.GetCurrentX()[point], zone.GetCurrentY()[point]);
        if (round)
        {
            if (Math.Abs(value - cur) > gs.GetControlValueCutIn())
            {
                value = cur;
            }
            value = gs.Round(value);
        }
        ctx.MovePoint(zone, point, value - cur);
        gs.SetRp0(point);
        gs.SetRp1(point);
    }

    private void DoMSIRP(ExecutionContext ctx, bool setRp0)
    {
        GraphicsState gs = ctx.GetGraphicsState();
        int distance = ctx.Pop();
        int point = ctx.Pop();
        Zone zp1 = ctx.GetZone(gs.GetZp1());
        Zone zp0 = ctx.GetZone(gs.GetZp0());
        int rp0 = gs.GetRp0();
        ExecutionContext.CheckPoint(zp1, point);
        ExecutionContext.CheckPoint(zp0, rp0);
        if (gs.GetZp1() == 0)
        {
            // twilight point: place it at rp0's original position moved by the distance, as the MS
            // rasterizer does (undocumented; FreeType's Ins_MSIRP)
            zp1.GetOriginalX()[point] = zp0.GetOriginalX()[rp0];
            zp1.GetOriginalY()[point] = zp0.GetOriginalY()[rp0];
            ctx.MoveOriginal(zp1, point, distance);
            zp1.GetCurrentX()[point] = zp1.GetOriginalX()[point];
            zp1.GetCurrentY()[point] = zp1.GetOriginalY()[point];
        }
        int curDist = ctx.ProjectedDistance(zp1, point, zp0, rp0);
        ctx.MovePoint(zp1, point, distance - curDist);
        gs.SetRp1(rp0);
        gs.SetRp2(point);
        if (setRp0)
        {
            gs.SetRp0(point);
        }
    }

    private void DoAlignRp(ExecutionContext ctx)
    {
        GraphicsState gs = ctx.GetGraphicsState();
        Zone zp1 = ctx.GetZone(gs.GetZp1());
        Zone zp0 = ctx.GetZone(gs.GetZp0());
        int rp0 = gs.GetRp0();
        ForEachLoopPoint(ctx, point =>
        {
            int dist = ctx.ProjectedDistance(zp1, point, zp0, rp0);
            ctx.MovePoint(zp1, point, -dist);
        });
    }

    /// <summary>
/// ALIGNPTS, after FreeType's Ins_ALIGNPTS: the point below the top of the stack is in zp1, the top
/// one in zp0, and each moves by half their projected distance (truncated), towards the other.
/// </summary>
    private void DoAlignPts(ExecutionContext ctx)
    {
        GraphicsState gs = ctx.GetGraphicsState();
        int p2 = ctx.Pop();
        int p1 = ctx.Pop();
        Zone zp1 = ctx.GetZone(gs.GetZp1());
        Zone zp0 = ctx.GetZone(gs.GetZp0());
        int distance = ctx.ProjectedDistance(zp0, p2, zp1, p1) / 2;
        ctx.MovePoint(zp1, p1, distance);
        ctx.MovePoint(zp0, p2, -distance);
    }

    private void DoUtp(ExecutionContext ctx)
    {
        GraphicsState gs = ctx.GetGraphicsState();
        Zone zone = ctx.GetZone(gs.GetZp0());
        UnitVector fv = gs.GetFreedomVector();
        int point = ExecutionContext.CheckPoint(zone, ctx.Pop());
        if (fv.GetX() != 0)
        {
            zone.GetTouchedX()[point] = false;
        }
        if (fv.GetY() != 0)
        {
            zone.GetTouchedY()[point] = false;
        }
    }

    /// <summary>
/// SHPIX moves points by a raw pixel amount along the freedom vector. Under backward
/// compatibility (v40 grayscale) FreeType treats a glyph-zone SHPIX like DELTAP - it only moves
/// points already touched in y, and never after IUP - because an unconditional SHPIX pushes
/// untouched points off their interpolated positions; twilight-zone points are always moved.
/// </summary>
    private void DoShpix(ExecutionContext ctx)
    {
        GraphicsState gs = ctx.GetGraphicsState();
        int amount = ctx.Pop();
        Zone zp2 = ctx.GetZone(gs.GetZp2());
        bool twilight = zp2 == ctx.GetTwilightZone();
        ForEachLoopPoint(ctx, point =>
        {
            if (twilight || DeltaPointAllowed(ctx, zp2, point))
            {
                ctx.MovePoint(zp2, point, amount);
            }
        });
    }

    private void DoShp(ExecutionContext ctx, bool useRp1)
    {
        GraphicsState gs = ctx.GetGraphicsState();
        int reference = ReferencePoint(gs, useRp1);
        Zone refZone = ReferenceZone(ctx, useRp1);
        int shift = ReferenceShift(ctx, refZone, reference);
        Zone zp2 = ctx.GetZone(gs.GetZp2());
        ForEachLoopPoint(ctx, point =>
        {
            if (!(refZone == zp2 && point == reference))
            {
                ctx.MovePoint(zp2, point, shift);
            }
        });
    }

    private void DoShc(ExecutionContext ctx, bool useRp1)
    {
        GraphicsState gs = ctx.GetGraphicsState();
        int reference = ReferencePoint(gs, useRp1);
        Zone refZone = ReferenceZone(ctx, useRp1);
        int shift = ReferenceShift(ctx, refZone, reference);
        int contour = ctx.Pop();
        Zone zp2 = ctx.GetZone(gs.GetZp2());
        int[] ends = zp2.GetContourEnds();
        // the twilight zone counts as a single contour of all its points (FreeType's Ins_SHC)
        bool twilight = zp2 == ctx.GetTwilightZone();
        if (contour < 0 || contour >= (twilight ? 1 : ends.Length))
        {
            return;
        }
        int start = contour == 0 ? 0 : ends[contour - 1] + 1;
        int end = twilight ? zp2.GetPointCount() - 1 : ends[contour];
        for (int i = start; i <= end; i++)
        {
            // FreeType's SHC does not move the reference point itself (it has already moved)
            if (!(refZone == zp2 && i == reference))
            {
                ctx.MovePoint(zp2, i, shift);
            }
        }
    }

    private void DoShz(ExecutionContext ctx, bool useRp1)
    {
        GraphicsState gs = ctx.GetGraphicsState();
        int reference = ReferencePoint(gs, useRp1);
        Zone refZone = ReferenceZone(ctx, useRp1);
        int shift = ReferenceShift(ctx, refZone, reference);
        int zoneNumber = ctx.Pop();
        Zone zone = ctx.GetZone(zoneNumber);
        for (int i = 0; i < zone.GetPointCount(); i++)
        {
            if (!(refZone == zone && i == reference))
            {
                ctx.MovePoint(zone, i, shift);
            }
        }
    }

    private static int ReferencePoint(GraphicsState gs, bool useRp1)
    {
        return useRp1 ? gs.GetRp1() : gs.GetRp2();
    }

    private static Zone ReferenceZone(ExecutionContext ctx, bool useRp1)
    {
        GraphicsState gs = ctx.GetGraphicsState();
        return ctx.GetZone(useRp1 ? gs.GetZp0() : gs.GetZp1());
    }

    /// <summary>
/// The projected distance the reference point (rp1 in zp0, or rp2 in zp1) has been moved.
/// </summary>
    private static int ReferenceShift(ExecutionContext ctx, Zone refZone, int reference)
    {
        ExecutionContext.CheckPoint(refZone, reference);
        return ctx.Project(refZone.GetCurrentX()[reference] - refZone.GetOriginalX()[reference],
                refZone.GetCurrentY()[reference] - refZone.GetOriginalY()[reference]);
    }

    private void DoMDRP(ExecutionContext ctx, int op)
    {
        GraphicsState gs = ctx.GetGraphicsState();
        int flags = op & 0x1F;
        bool setRp0 = (flags & 0x10) != 0;
        bool useMin = (flags & 0x08) != 0;
        bool round = (flags & 0x04) != 0;
        int point = ctx.Pop();
        Zone zp1 = ctx.GetZone(gs.GetZp1());
        Zone zp0 = ctx.GetZone(gs.GetZp0());
        int rp0 = gs.GetRp0();
        ExecutionContext.CheckPoint(zp1, point);
        ExecutionContext.CheckPoint(zp0, rp0);

        int orgDist;
        if (gs.GetZp0() == 0 || gs.GetZp1() == 0)
        {
            // twilight points have no font-unit source: measure their scaled originals
            orgDist = ctx.DualProjectedDistance(zp1, point, zp0, rp0);
        }
        else
        {
            // measure in font units and scale the distance, as FreeType's Ins_MDRP does (orus):
            // projecting the individually rounded scaled originals can land a half-pixel distance on
            // the other side of the rounding threshold and move the point by a whole pixel
            int dist = ctx.DualProject(zp1.GetUnscaledX()[point] - zp0.GetUnscaledX()[rp0],
                    zp1.GetUnscaledY()[point] - zp0.GetUnscaledY()[rp0]);
            // a composite's unscaled coordinates are already device positions (scale 1)
            orgDist = ctx.IsComposite() ? dist : Fixed.Scale(dist, ctx.GetPpem(), ctx.GetUnitsPerEm());
        }
        orgDist = ApplySingleWidth(gs, orgDist);
        int distance = round ? gs.Round(orgDist) : orgDist;
        distance = ApplyMinimumDistance(gs, useMin, orgDist, distance);

        int curDist = ctx.ProjectedDistance(zp1, point, zp0, rp0);
        ctx.MovePoint(zp1, point, distance - curDist);
        gs.SetRp1(rp0);
        gs.SetRp2(point);
        if (setRp0)
        {
            gs.SetRp0(point);
        }
    }

    private void DoMIRP(ExecutionContext ctx, int op)
    {
        GraphicsState gs = ctx.GetGraphicsState();
        int flags = op & 0x1F;
        bool setRp0 = (flags & 0x10) != 0;
        bool useMin = (flags & 0x08) != 0;
        bool round = (flags & 0x04) != 0;
        // the CVT entry number is on top of the stack, the point number below it
        int cvtIndex = ctx.Pop();
        int point = ctx.Pop();
        int[] cvt = ctx.GetControlValues();
        int cvtValue = cvtIndex >= 0 && cvtIndex < cvt.Length ? cvt[cvtIndex] : 0;
        cvtValue = ApplySingleWidth(gs, cvtValue);

        Zone zp1 = ctx.GetZone(gs.GetZp1());
        Zone zp0 = ctx.GetZone(gs.GetZp0());
        int rp0 = gs.GetRp0();
        ExecutionContext.CheckPoint(zp1, point);
        ExecutionContext.CheckPoint(zp0, rp0);
        if (gs.GetZp1() == 0)
        {
            // twilight point: place it the control value away from rp0's original position along the
            // freedom vector, as the MS rasterizer does (undocumented; FreeType's Ins_MIRP)
            UnitVector fv = gs.GetFreedomVector();
            zp1.GetOriginalX()[point] = zp0.GetOriginalX()[rp0] + Fixed.Mul14(cvtValue, fv.GetX());
            zp1.GetOriginalY()[point] = zp0.GetOriginalY()[rp0] + Fixed.Mul14(cvtValue, fv.GetY());
            zp1.GetCurrentX()[point] = zp1.GetOriginalX()[point];
            zp1.GetCurrentY()[point] = zp1.GetOriginalY()[point];
        }
        int orgDist = ctx.DualProjectedDistance(zp1, point, zp0, rp0);

        // auto-flip the control value to match the sign of the original distance
        if (gs.IsAutoFlip() && (orgDist ^ cvtValue) < 0)
        {
            cvtValue = -cvtValue;
        }
        int distance;
        if (round)
        {
            // the control value cut-in only applies when both points are in the same zone
            if (gs.GetZp0() == gs.GetZp1()
                    && Math.Abs(cvtValue - orgDist) > gs.GetControlValueCutIn())
            {
                cvtValue = orgDist;
            }
            distance = gs.Round(cvtValue);
        }
        else
        {
            distance = cvtValue;
        }
        distance = ApplyMinimumDistance(gs, useMin, orgDist, distance);

        int curDist = ctx.ProjectedDistance(zp1, point, zp0, rp0);
        ctx.MovePoint(zp1, point, distance - curDist);
        gs.SetRp1(rp0);
        gs.SetRp2(point);
        if (setRp0)
        {
            gs.SetRp0(point);
        }
    }

    private static int ApplySingleWidth(GraphicsState gs, int distance)
    {
        if (Math.Abs(distance - gs.GetSingleWidthValue()) < gs.GetSingleWidthCutIn())
        {
            return distance >= 0 ? gs.GetSingleWidthValue() : -gs.GetSingleWidthValue();
        }
        return distance;
    }

    private static int ApplyMinimumDistance(GraphicsState gs, bool useMin, int orgDist,
            int distance)
    {
        if (!useMin)
        {
            return distance;
        }
        int md = gs.GetMinimumDistance();
        if (orgDist >= 0)
        {
            return distance < md ? md : distance;
        }
        return distance > -md ? -md : distance;
    }

    // --- interpolation ---------------------------------------------------

    private void InstallInterpolationOps()
    {
        dispatch[0x30] = ctx => DoIup(ctx, false); // IUP[0] y
        dispatch[0x31] = ctx => DoIup(ctx, true);  // IUP[1] x
        dispatch[0x39] = this.DoIp;               // IP
    }

    private void DoIup(ExecutionContext ctx, bool xAxis)
    {
        // record that IUP ran on this axis; under backward-compatibility, once both axes are done the
        // glyph is frozen against further y moves (see ExecutionContext.movePoint) and further IUPs
        // do nothing (FreeType's Ins_IUP)
        if (ctx.IsBackwardCompatibility() && ctx.IsIupDone())
        {
            return;
        }
        if (xAxis)
        {
            ctx.SetIupxCalled();
        }
        else
        {
            ctx.SetIupyCalled();
        }
        // IUP always operates on the glyph zone, directly on the x or y coordinate
        Zone zone = ctx.GetZone(1);
        int[] cur = xAxis ? zone.GetCurrentX() : zone.GetCurrentY();
        int[] org = xAxis ? zone.GetOriginalX() : zone.GetOriginalY();
        int[] orus = xAxis ? zone.GetUnscaledX() : zone.GetUnscaledY();
        bool[] touched = xAxis ? zone.GetTouchedX() : zone.GetTouchedY();
        int[] ends = zone.GetContourEnds();
        int start = 0;
        foreach (int end in ends)
        {
            InterpolateContour(cur, org, orus, touched, start, end);
            start = end + 1;
        }
    }

    /// <summary>
/// Interpolates the untouched points of one contour, after FreeType's Ins_IUP: a single touched
/// point shifts the whole contour by its movement; otherwise each run of untouched points between two
/// touched ones is shifted by the nearer one's movement when it lies outside their original span,
/// and interpolated between them when inside - by the unscaled (font-unit) coordinates.
/// </summary>
    private static void InterpolateContour(int[] cur, int[] org, int[] orus, bool[] touched,
            int start, int end)
    {
        if (end < start)
        {
            return;
        }
        int firstTouched = -1;
        int touchedCount = 0;
        for (int i = start; i <= end; i++)
        {
            if (touched[i])
            {
                if (firstTouched < 0)
                {
                    firstTouched = i;
                }
                touchedCount++;
            }
        }
        if (touchedCount == 0)
        {
            return;
        }
        if (touchedCount == 1)
        {
            int delta = cur[firstTouched] - org[firstTouched];
            if (delta != 0)
            {
                for (int i = start; i <= end; i++)
                {
                    if (i != firstTouched)
                    {
                        cur[i] += delta;
                    }
                }
            }
            return;
        }
        // walk the contour cyclically, interpolating the untouched run between each touched pair
        int t1 = firstTouched;
        int seen = 0;
        for (int step = 1; step <= end - start + 1 && seen < touchedCount; step++)
        {
            int i = start + (firstTouched - start + step) % (end - start + 1);
            if (touched[i])
            {
                InterpolateRun(cur, org, orus, t1, i, start, end);
                t1 = i;
                seen++;
            }
        }
    }

    /// <summary>
/// Places the untouched points that lie between two touched points, walking the contour forwards
/// from from to to, as the TrueType specification describes for IUP: a point whose
/// original coordinate lies between the two touched points' originals keeps its relative position
/// between them, measured on the unscaled (font unit) outline for precision; a point outside that
/// range moves by the same amount as the touched point on its side. The ratio of the hinted to the
/// font-unit span is held in 16.16 fixed point, the precision FreeType interpolates at, so results
/// agree with it to the 1/64 px.
/// </summary>
    private static void InterpolateRun(int[] cur, int[] org, int[] orus, int from, int to,
            int start, int end)
    {
        // the touched point lower in the outline, and the one higher; on a tie, the first one
        bool forward = orus[from] <= orus[to];
        int lower = forward ? from : to;
        int upper = forward ? to : from;
        int fontUnitSpan = orus[upper] - orus[lower];
        int ratio = fontUnitSpan == 0 ? 0 : Fixed.DivFix(cur[upper] - cur[lower], fontUnitSpan);
        for (int i = Following(from, start, end); i != to; i = Following(i, start, end))
        {
            if (org[i] <= org[lower])
            {
                cur[i] = org[i] + cur[lower] - org[lower];
            }
            else if (org[i] >= org[upper])
            {
                cur[i] = org[i] + cur[upper] - org[upper];
            }
            else if (fontUnitSpan == 0)
            {
                cur[i] = cur[lower];
            }
            else
            {
                cur[i] = cur[lower] + Fixed.MulFix(orus[i] - orus[lower], ratio);
            }
        }
    }

    /// <summary>
/// The point after i on a closed contour running from start to end.
/// </summary>
    private static int Following(int i, int start, int end)
    {
        return i == end ? start : i + 1;
    }

    private void DoIp(ExecutionContext ctx)
    {
        GraphicsState gs = ctx.GetGraphicsState();
        Zone z0 = ctx.GetZone(gs.GetZp0());
        Zone z1 = ctx.GetZone(gs.GetZp1());
        Zone z2 = ctx.GetZone(gs.GetZp2());
        int rp1 = ExecutionContext.CheckPoint(z0, gs.GetRp1());
        int rp2 = ExecutionContext.CheckPoint(z1, gs.GetRp2());
        // Measure the original positions in unscaled font units (FreeType's orus) so the interpolation
        // ratio keeps full precision; the scaled F26Dot6 originals round each coordinate and can shift
        // an interpolated point by a unit, which a later rounding opcode then amplifies to a whole pixel.
        // Exception: twilight-zone points have no font-unit source, so their unscaled coordinates are
        // (0,0); using them would collapse every original distance to zero. When any zone here is the
        // twilight zone, FreeType measures the scaled originals instead, so we do the same.
        bool twilight = gs.GetZp0() == 0 || gs.GetZp1() == 0 || gs.GetZp2() == 0;
        int curRp1 = ctx.Project(z0.GetCurrentX()[rp1], z0.GetCurrentY()[rp1]);
        int orgRp1 = twilight ? ctx.DualProject(z0.GetOriginalX()[rp1], z0.GetOriginalY()[rp1])
                : ctx.DualProject(z0.GetUnscaledX()[rp1], z0.GetUnscaledY()[rp1]);
        int curRp2 = ctx.Project(z1.GetCurrentX()[rp2], z1.GetCurrentY()[rp2]);
        int orgRp2 = twilight ? ctx.DualProject(z1.GetOriginalX()[rp2], z1.GetOriginalY()[rp2])
                : ctx.DualProject(z1.GetUnscaledX()[rp2], z1.GetUnscaledY()[rp2]);
        int orgRange = orgRp2 - orgRp1;
        int curRange = curRp2 - curRp1;
        ForEachLoopPoint(ctx, point =>
        {
            ExecutionContext.CheckPoint(z2, point);
            int orgP = twilight ? ctx.DualProject(z2.GetOriginalX()[point], z2.GetOriginalY()[point])
                    : ctx.DualProject(z2.GetUnscaledX()[point], z2.GetUnscaledY()[point]);
            int curP = ctx.Project(z2.GetCurrentX()[point], z2.GetCurrentY()[point]);
            int newP;
            if (orgRange == 0)
            {
                newP = curRp1 + (orgP - orgRp1);
            }
            else
            {
                newP = curRp1 + Fixed.MulDiv(orgP - orgRp1, curRange, orgRange);
            }
            ctx.MovePoint(z2, point, newP - curP);
        });
    }

    // --- measurement -----------------------------------------------------

    private void InstallMeasureOps()
    {
        dispatch[0x46] = ctx => DoGc(ctx, false); // GC[0] current
        dispatch[0x47] = ctx => DoGc(ctx, true);  // GC[1] original
        dispatch[0x48] = this.DoScfs;            // SCFS
        dispatch[0x49] = ctx => DoMd(ctx, false); // MD[0] grid-fitted
        dispatch[0x4A] = ctx => DoMd(ctx, true);  // MD[1] original
    }

    private void DoGc(ExecutionContext ctx, bool original)
    {
        GraphicsState gs = ctx.GetGraphicsState();
        Zone zone = ctx.GetZone(gs.GetZp2());
        int point = ExecutionContext.CheckPoint(zone, ctx.Pop());
        if (original)
        {
            ctx.Push(ctx.DualProject(zone.GetOriginalX()[point], zone.GetOriginalY()[point]));
        }
        else
        {
            ctx.Push(ctx.Project(zone.GetCurrentX()[point], zone.GetCurrentY()[point]));
        }
    }

    private void DoScfs(ExecutionContext ctx)
    {
        GraphicsState gs = ctx.GetGraphicsState();
        int value = ctx.Pop();
        int point = ctx.Pop();
        Zone zone = ctx.GetZone(gs.GetZp2());
        ExecutionContext.CheckPoint(zone, point);
        int cur = ctx.Project(zone.GetCurrentX()[point], zone.GetCurrentY()[point]);
        ctx.MovePoint(zone, point, value - cur);
    }

    private void DoMd(ExecutionContext ctx, bool original)
    {
        GraphicsState gs = ctx.GetGraphicsState();
        int p2 = ctx.Pop();
        int p1 = ctx.Pop();
        Zone zp0 = ctx.GetZone(gs.GetZp0());
        Zone zp1 = ctx.GetZone(gs.GetZp1());
        // FreeType measures project(zp0[p1] - zp1[p2]); p1 is the deeper operand, p2 the top
        if (original)
        {
            ctx.Push(ctx.DualProjectedDistance(zp0, p1, zp1, p2));
        }
        else
        {
            ctx.Push(ctx.ProjectedDistance(zp0, p1, zp1, p2));
        }
    }

    // --- delta exceptions ------------------------------------------------

    private void InstallDeltaOps()
    {
        dispatch[0x5D] = ctx => DoDeltaP(ctx, 0); // DELTAP1
        dispatch[0x71] = ctx => DoDeltaP(ctx, 1); // DELTAP2
        dispatch[0x72] = ctx => DoDeltaP(ctx, 2); // DELTAP3
        dispatch[0x73] = ctx => DoDeltaC(ctx, 0); // DELTAC1
        dispatch[0x74] = ctx => DoDeltaC(ctx, 1); // DELTAC2
        dispatch[0x75] = ctx => DoDeltaC(ctx, 2); // DELTAC3
    }

    private void DoDeltaP(ExecutionContext ctx, int band)
    {
        GraphicsState gs = ctx.GetGraphicsState();
        Zone zone = ctx.GetZone(gs.GetZp0());
        int n = ctx.Pop();
        for (int i = 0; i < n; i++)
        {
            int point = ctx.Pop();
            int arg = ctx.Pop();
            if (DeltaTargetPpem(gs, arg, band) == ctx.GetPpem()
                    && DeltaPointAllowed(ctx, zone, point))
            {
                ctx.MovePoint(zone, point, DecodeDelta(arg & 0x0F, gs.GetDeltaShift()));
            }
        }
    }

    /// <summary>
/// Backward-compatibility (v40 grayscale) gate shared by DELTAP and SHPIX: once IUP has run the
/// move is dropped, and before IUP it is applied only to points already touched in y (or, for
/// composites, when the freedom vector has a y component). Outside backward-compatibility mode
/// the move always applies. This keeps DELTAP/SHPIX from nudging untouched points off their
/// interpolated grayscale positions (FreeType Ins_DELTAP / Ins_SHPIX).
/// </summary>
    private static bool DeltaPointAllowed(ExecutionContext ctx, Zone zone, int point)
    {
        if (!ctx.IsBackwardCompatibility())
        {
            return true;
        }
        if (ctx.IsIupDone())
        {
            return false;
        }
        bool touchedY = point >= 0 && point < zone.GetTouchedY().Length
                && zone.GetTouchedY()[point];
        return touchedY
                || (ctx.IsComposite() && ctx.GetGraphicsState().GetFreedomVector().GetY() != 0);
    }

    private void DoDeltaC(ExecutionContext ctx, int band)
    {
        GraphicsState gs = ctx.GetGraphicsState();
        int[] cvt = ctx.GetControlValues();
        int n = ctx.Pop();
        for (int i = 0; i < n; i++)
        {
            int cvtIndex = ctx.Pop();
            int arg = ctx.Pop();
            if (DeltaTargetPpem(gs, arg, band) == ctx.GetPpem() && cvtIndex >= 0
                    && cvtIndex < cvt.Length)
            {
                cvt[cvtIndex] += DecodeDelta(arg & 0x0F, gs.GetDeltaShift());
            }
        }
    }

    private static int DeltaTargetPpem(GraphicsState gs, int arg, int band)
    {
        return ((arg >> 4) & 0x0F) + gs.GetDeltaBase() + band * 16;
    }

    private static int DecodeDelta(int steps, int deltaShift)
    {
        int relative = steps < 8 ? steps - 8 : steps - 7; // 0..15 -> -8..-1, 1..8
        int unit = Fixed.ONE >> deltaShift;               // 1 / 2^deltaShift of a pixel
        return relative * unit;
    }

    // --- flip and scan-conversion ----------------------------------------

    private void InstallFlipOps()
    {
        dispatch[0x4D] = ctx => ctx.GetGraphicsState().SetAutoFlip(true);  // FLIPON
        dispatch[0x4E] = ctx => ctx.GetGraphicsState().SetAutoFlip(false); // FLIPOFF
        dispatch[0x80] = ctx =>                                            // FLIPPT
        {
            Zone glyph = ctx.GetZone(1);
            bool[] onCurve = glyph.GetOnCurve();
            ForEachLoopPoint(ctx, point =>
                    onCurve[ExecutionContext.CheckPoint(glyph, point)] = !onCurve[point]);
        };
        dispatch[0x81] = ctx => FlipRange(ctx, true);                      // FLIPRGON
        dispatch[0x82] = ctx => FlipRange(ctx, false);                     // FLIPRGOFF
        dispatch[0x85] = ctx => ctx.GetGraphicsState().SetScanControl(ctx.Pop()); // SCANCTRL
        dispatch[0x8D] = ctx => ctx.GetGraphicsState().SetScanType(ctx.Pop());    // SCANTYPE
        dispatch[0x8E] = TrueTypeInterpreter.DoInstctrl;                  // INSTCTRL
    }

    /// <summary>
/// INSTCTRL, after FreeType's Ins_INSTCTRL. The selector is an index 1..3 naming one flag bit, and
/// the value must be 0 or that bit. In prep it sets or clears the bit in the graphics
/// state, which GlyphHinter reads before each glyph: bit 1 turns hinting off, bit 2 makes
/// glyph programs start from the default graphics state, bit 4 waives v40 backward compatibility.
/// A glyph program may only use selector 3, to waive (or restore) backward compatibility for itself
/// - native-ClearType fonts do that per glyph. Anything else is ignored.
/// </summary>
    private static void DoInstctrl(ExecutionContext ctx)
    {
        int selector = ctx.Pop();
        int value = ctx.Pop();
        if (selector < 1 || selector > 3)
        {
            return;
        }
        int flag = 1 << (selector - 1);
        if (value != 0 && value != flag)
        {
            return;
        }
        if (ctx.GetCodeRange() == ExecutionContext.CodeRange.CONTROL_VALUE)
        {
            GraphicsState gs = ctx.GetGraphicsState();
            gs.SetInstructControl(gs.GetInstructControl() & ~flag | value);
        }
        else if (ctx.GetCodeRange() == ExecutionContext.CodeRange.GLYPH && selector == 3)
        {
            ctx.SetBackwardCompatibility(value == 0);
        }
    }

    private static void FlipRange(ExecutionContext ctx, bool onCurve)
    {
        bool[] flags = ctx.GetZone(1).GetOnCurve();
        int high = ctx.Pop();
        int low = ctx.Pop();
        for (int i = low; i <= high && i < flags.Length; i++)
        {
            if (i >= 0)
            {
                flags[i] = onCurve;
            }
        }
    }

    // --- loop helper -----------------------------------------------------

    
    private delegate void PointConsumer(int point);

    /// <summary>
/// Processes the graphics-state loop count of points, popping one per iteration, then resets the
/// loop counter to 1.
/// </summary>
    private static void ForEachLoopPoint(ExecutionContext ctx, PointConsumer consumer)
    {
        int loop = ctx.GetGraphicsState().GetLoop();
        for (int i = 0; i < loop; i++)
        {
            consumer(ctx.Pop());
        }
        ctx.GetGraphicsState().SetLoop(1);
    }

    // --- handler helpers -------------------------------------------------

    
    private delegate int IntBinaryOp(int a, int b);

    private static void Binary(ExecutionContext ctx, IntBinaryOp op)
    {
        int b = ctx.Pop();
        int a = ctx.Pop();
        ctx.Push(op(a, b));
    }

    private static int Bool(bool value)
    {
        return value ? 1 : 0;
    }

    private static OpHandler RoundState(int state)
    {
        return ctx => ctx.GetGraphicsState().SetRoundState(state);
    }

    private static int Read(int[] array, int index, string name)
    {
        if (index < 0 || index >= array.Length)
        {
            throw new HintingException(name + " index output of range: " + index);
        }
        return array[index];
    }

    private static void Write(int[] array, int index, int value, string name)
    {
        if (index < 0 || index >= array.Length)
        {
            throw new HintingException(name + " index output of range: " + index);
        }
        array[index] = value;
    }
}
