/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/test/java/org/apache/fontbox/ttf/TrueTypeInterpreterTest.java
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

using PdfBox.Net.FontBox.TTF;
using ExecutionContext = PdfBox.Net.FontBox.TTF.ExecutionContext;

namespace PdfBox.Net.FontBox.Tests.TTF.Hinting;

/// <summary>
/// Program-level tests: hand-assembled bytecode fed through the dispatch loop with no font.
/// These exercise the engine - dispatch, the push family, branching, function definition and calling,
/// and the BytecodeStream bounds checks - independently of any glyph.
/// </summary>
public class TrueTypeInterpreterTest
{
    // opcodes used to assemble test programs
    private static readonly byte PUSHB1 = (byte) 0xB0; // PUSHB[0] - push one byte
    private static readonly byte PUSHB2 = (byte) 0xB1; // PUSHB[1] - push two bytes
    private static readonly byte NPUSHW = (byte) 0x41;
    private static readonly byte ADD = 0x60;
    private static readonly byte SUB = 0x61;
    private static readonly byte MUL = 0x63;
    private static readonly byte DUP = 0x20;
    private static readonly byte SWAP = 0x23;
    private static readonly byte DEPTH = 0x24;
    private static readonly byte ROLL = (byte) 0x8A;
    private static readonly byte CINDEX = 0x25;
    private static readonly byte MINDEX = 0x26;
    private static readonly byte NPUSHB = 0x40;
    private static readonly byte JROF = 0x79;
    private static readonly byte POP = 0x21;
    private static readonly byte GT = 0x52;
    private static readonly byte ODD = 0x56;
    private static readonly byte EVEN = 0x57;
    private static readonly byte AND = 0x5A;
    private static readonly byte OR = 0x5B;
    private static readonly byte NOT = 0x5C;
    private static readonly byte RTHG = 0x19;
    private static readonly byte RDTG = 0x7D;
    private static readonly byte ROFF = 0x7A;
    private static readonly byte IF = 0x58;
    private static readonly byte ELSE = 0x1B;
    private static readonly byte EIF = 0x59;
    private static readonly byte PUSHW1 = (byte) 0xB8; // PUSHW[0] - push one signed word
    private static readonly byte JMPR = 0x1C;
    private static readonly byte JROT = 0x78;
    private static readonly byte FDEF = 0x2C;
    private static readonly byte ENDF = 0x2D;
    private static readonly byte CALL = 0x2B;
    private static readonly byte LOOPCALL = 0x2A;
    private static readonly byte MPPEM = 0x4B;
    private static readonly byte WS = 0x42;
    private static readonly byte RS = 0x43;
    private static readonly byte SZPS = 0x16;
    private static readonly byte SCFS = 0x48;
    private static readonly byte GC = 0x46;
    private static readonly byte INSTCTRL = (byte) 0x8E;

    private static TrueTypeInterpreter Interpreter()
    {
        return new TrueTypeInterpreter(256, 16, 0, 2048);
    }

    private static int RunTop(byte[] program)
    {
        ExecutionContext ctx = Interpreter().ExecuteProgram(program, 16);
        return ctx.Peek(0);
    }

    /// <summary>
    /// Runs the program and returns the whole stack, bottom to top.
    /// </summary>
    private static int[] RunStack(byte[] program)
    {
        ExecutionContext ctx = Interpreter().ExecuteProgram(program, 16);
        int[] stack = new int[ctx.GetStackDepth()];
        for (int i = 0; i < stack.Length; i++)
        {
            stack[i] = ctx.Peek(stack.Length - 1 - i);
        }
        return stack;
    }

    [Fact]
    public void TestPushAndAdd()
    {
        // PUSHB[1] 2 3 ; ADD  ->  5
        Equal(5, RunTop(new byte[] { PUSHB2, 2, 3, ADD }));
    }

    [Fact]
    public void TestNpushwSigned()
    {
        // NPUSHW 1 0xFFFF ; -> -1 on the stack
        Equal(-1, RunTop(new byte[] { NPUSHW, 1, (byte) 0xFF, (byte) 0xFF }));
    }

    [Fact]
    public void TestArithmetic()
    {
        // 10 - 3 == 7
        Equal(7, RunTop(new byte[] { PUSHB2, 10, 3, SUB }));
        // 64(=1.0) * 192(=3.0) == 192(=3.0) ... use F26Dot6: PUSHB 64, then need words; use small ints
        // 2.0 * 3.0 in F26Dot6: push 128 and 192 via NPUSHW
        Equal(Fixed.FromInt(6),
                RunTop(new byte[] { NPUSHW, 2, 0, (byte) 128, 0, (byte) 192, MUL }));
    }

    [Fact]
    public void TestStackOps()
    {
        // DUP: push 7, dup, add -> 14
        Equal(14, RunTop(new byte[] { PUSHB1, 7, DUP, ADD }));
        // SWAP then SUB: push 3,10 swap -> 10,3 ; SUB pops b=3,a=10 -> 7
        Equal(7, RunTop(new byte[] { PUSHB2, 3, 10, SWAP, SUB }));
        // DEPTH after pushing three values -> 3
        Equal(3, RunTop(new byte[] { PUSHB2, 1, 2, PUSHB1, 9, DEPTH }));
        // ROLL: 1 2 3 -> 2 3 1, top is 1
        Equal(1, RunTop(new byte[] { PUSHB2, 1, 2, PUSHB1, 3, ROLL }));
    }

    /// <summary>
    /// ROLL moves the third element to the top (FreeType's Ins_ROLL: [a b c] -&gt; [b c a] with c on top
    /// before and a on top after). Checks all three resulting positions, not just the top.
    /// </summary>
    [Fact]
    public void TestRollFullPermutation()
    {
        // after ROLL the stack is b c a; pop a (top) with a SUB against 0 pushed... simpler: use
        // arithmetic that only gives the expected value for the expected order:
        // [1 2 3] ROLL -> [2 3 1]; SUB -> 3 - 1 = 2; SUB -> 2 - 2 = 0
        Equal(0, RunTop(new byte[] { PUSHB2, 1, 2, PUSHB1, 3, ROLL, SUB, SUB }));
        // [5 7 9] ROLL -> [7 9 5]; SUB -> 9 - 5 = 4; SUB -> 7 - 4 = 3
        Equal(3, RunTop(new byte[] { PUSHB2, 5, 7, PUSHB1, 9, ROLL, SUB, SUB }));
    }

    /// <summary>
    /// ODD/EVEN round the operand with the &lt;em&gt;current round state&lt;/em&gt; before testing the pixel's
    /// parity (spec, and FreeType's Ins_ODD/Ins_EVEN), so the same value can be odd under one round
    /// state and even under another. AND/OR/NOT are C truthiness on F26Dot6 values.
    /// </summary>
    [Fact]
    public void TestOddEvenUseRoundState()
    {
        // 1.4 px (90) rounds to 1 px under round-to-grid: odd
        Equal(1, RunTop(new byte[] { PUSHB1, 90, ODD }));
        Equal(0, RunTop(new byte[] { PUSHB1, 90, EVEN }));
        // 1.6 px (102) rounds to 2 px: even
        Equal(0, RunTop(new byte[] { PUSHB1, 102, ODD }));
        Equal(1, RunTop(new byte[] { PUSHB1, 102, EVEN }));
        // round-down-to-grid: 1.6 px -> 1 px: odd
        Equal(1, RunTop(new byte[] { RDTG, PUSHB1, 102, ODD }));
        // round-to-half-grid: 1.4 px -> 1.5 px, which is neither an odd nor an even pixel
        Equal(0, RunTop(new byte[] { RTHG, PUSHB1, 90, ODD }));
        Equal(0, RunTop(new byte[] { RTHG, PUSHB1, 90, EVEN }));
        // round off: the value is tested as-is; 64 is odd, 128 even, 90 neither
        Equal(1, RunTop(new byte[] { ROFF, PUSHB1, 64, ODD }));
        Equal(1, RunTop(new byte[] { ROFF, PUSHB1, (byte) 128, EVEN }));
        Equal(0, RunTop(new byte[] { ROFF, PUSHB1, 90, ODD }));
        Equal(0, RunTop(new byte[] { ROFF, PUSHB1, 90, EVEN }));
    }

    [Fact]
    public void TestAndOrNot()
    {
        Equal(1, RunTop(new byte[] { PUSHB2, 3, 64, AND }));
        Equal(0, RunTop(new byte[] { PUSHB2, 3, 0, AND }));
        Equal(1, RunTop(new byte[] { PUSHB2, 0, 5, OR }));
        Equal(0, RunTop(new byte[] { PUSHB2, 0, 0, OR }));
        Equal(1, RunTop(new byte[] { PUSHB1, 0, NOT }));
        Equal(0, RunTop(new byte[] { PUSHB1, 9, NOT }));
    }

    [Fact]
    public void TestIfElseTrueBranch()
    {
        // push 1 (true) ; IF push 10 ELSE push 20 EIF -> 10
        Equal(10, RunTop(new byte[] { PUSHB1, 1, IF, PUSHB1, 10, ELSE, PUSHB1, 20, EIF }));
    }

    [Fact]
    public void TestIfElseFalseBranch()
    {
        // push 0 (false) ; IF push 10 ELSE push 20 EIF -> 20
        Equal(20, RunTop(new byte[] { PUSHB1, 0, IF, PUSHB1, 10, ELSE, PUSHB1, 20, EIF }));
    }

    [Fact]
    public void TestNestedIf()
    {
        // outer true, inner (5>3) true -> 99
        // PUSHB 1 ; IF [ PUSHB 5 3 ; GT ; IF PUSHB 99 ELSE PUSHB 1 EIF ] ELSE PUSHB 7 EIF
        byte[] program = {
                PUSHB1, 1, IF,
                    PUSHB2, 5, 3, GT, IF,
                        PUSHB1, 99,
                    ELSE,
                        PUSHB1, 1,
                    EIF,
                ELSE,
                    PUSHB1, 7,
                EIF };
        Equal(99, RunTop(program));
    }

    [Fact]
    public void TestJmpr()
    {
        // PUSHB 3 ; JMPR (jump +3 from the JMPR opcode) skips a push, lands on PUSHB 42
        // layout: [0]PUSHB1 [1]3 [2]JMPR [3]PUSHB1 [4]7(skipped) [5]PUSHB1 [6]42
        ExecutionContext ctx = Interpreter().ExecuteProgram(
                new byte[] { PUSHB1, 3, JMPR, PUSHB1, 7, PUSHB1, 42 }, 16);
        Equal(42, ctx.Peek(0));
        Equal(1, ctx.GetStackDepth()); // the skipped push never ran
    }

    [Fact]
    public void TestFunctionDefAndCall()
    {
        // define function 5 = "double the top" (DUP ADD); call it on 21 -> 42
        TrueTypeInterpreter interp = Interpreter();
        interp.SetFontProgram(new byte[] { PUSHB1, 5, FDEF, DUP, ADD, ENDF });
        interp.PrepareFontProgram();
        Equal(1, interp.GetFunctions().Count);

        ExecutionContext ctx = interp.ExecuteProgram(new byte[] { PUSHB1, 21, PUSHB1, 5, CALL }, 16);
        Equal(42, ctx.Peek(0));
    }

    [Fact]
    public void TestLoopCall()
    {
        // function 1 = "add 1"; LOOPCALL it 3 times starting from 0 -> 3
        TrueTypeInterpreter interp = Interpreter();
        interp.SetFontProgram(new byte[] { PUSHB1, 1, FDEF, PUSHB1, 1, ADD, ENDF });
        interp.PrepareFontProgram();

        // stack: value=0, count=3, fn=1 ; LOOPCALL pops fn then count
        ExecutionContext ctx = interp.ExecuteProgram(
                new byte[] { PUSHB1, 0, PUSHB2, 3, 1, LOOPCALL }, 16);
        Equal(3, ctx.Peek(0));
    }

    [Fact]
    public void TestCallDepthLimitTrips()
    {
        // function 0 calls itself unconditionally -> must trip the depth cap, not StackOverflowError
        TrueTypeInterpreter interp = Interpreter();
        interp.SetFontProgram(new byte[] { PUSHB1, 0, FDEF, PUSHB1, 0, CALL, ENDF });
        interp.PrepareFontProgram();

        HintingException ex = Assert.Throws<HintingException>(
                () => interp.ExecuteProgram(new byte[] { PUSHB1, 0, CALL }, 16));
        Equal(true, ex.Message.Contains("call depth"));
    }

    [Fact]
    public void TestIdefDefinesOpcode()
    {
        // IDEF binds reserved opcode 0x83 to "push 42"; invoking 0x83 then runs that body.
        // PUSHB[0] 0x83 ; IDEF ; PUSHB[0] 42 ; ENDF ; <0x83>
        ExecutionContext ctx = Interpreter().ExecuteProgram(
                new byte[] { PUSHB1, (byte) 0x83, (byte) 0x89, PUSHB1, 42, ENDF, (byte) 0x83 }, 16);
        Equal(42, ctx.Peek(0));
    }

    [Fact]
    public void TestUndefinedFunctionThrows()
    {
        Assert.Throws<HintingException>(
                () => Interpreter().ExecuteProgram(new byte[] { PUSHB1, 9, CALL }, 16));
    }

    [Fact]
    public void TestMppemReflectsPpem()
    {
        ExecutionContext ctx = Interpreter().ExecuteProgram(new byte[] { MPPEM }, 19);
        Equal(19, ctx.Peek(0));
    }

    [Fact]
    public void TestUnsupportedOpcodeThrows()
    {
        // 0x28 is a reserved/unused opcode; unimplemented opcodes must throw, not no-op
        Assert.Throws<HintingException>(
                () => Interpreter().ExecuteProgram(new byte[] { 0x28 }, 16));
    }

    [Fact]
    public void TestControlValueScalingThroughPrep()
    {
        // raw cvt [2048] at 16 ppem, unitsPerEm 2048 -> scaled to 16px (1024 in F26Dot6)
        TrueTypeInterpreter interp = Interpreter();
        interp.SetControlValues(new int[] { 2048 });
        interp.SetPpem(16, 16);
        // RCVT 0 -> the scaled value
        ExecutionContext ctx = interp.ExecuteProgram(new byte[] { PUSHB1, 0, 0x45 }, 16);
        Equal(Fixed.FromInt(16), ctx.Peek(0));
    }

    /// <summary>
    /// A backward jump is one of only two ways TrueType bytecode can loop, and this four-byte program -
    /// PUSHW -3 ; JMPR, which jumps back onto its own push - used to spin forever. It must now
    /// hit the execution budget and throw, so GlyphHinter falls back to the raw outline.
    /// </summary>
    [Fact]
    public void TestBackwardJumpIsBounded()
    {
        AssertTimeoutPreemptively(TimeSpan.FromSeconds(5), () =>
                Assert.Throws<HintingException>(
                        () => Interpreter().ExecuteProgram(
                                new byte[] { PUSHW1, (byte) 0xFF, (byte) 0xFD, JMPR }, 16)));
    }

    /// <summary>
    /// A backward-jump loop that stays inside the budget must still run to completion - the bound exists
    /// to stop runaway programs, not legitimately loop-heavy ones.
    /// </summary>
    [Fact]
    public void TestBackwardJumpWithinBudgetCompletes()
    {
        // counter = 60; loop { counter -= 1; if (counter != 0) jump back } -> 59 backward jumps
        // [0]PUSHW1 60 [3]PUSHB1 1 [5]SUB [6]DUP [7]PUSHW1 -8 [10]SWAP [11]JROT
        byte[] program =
        {
            PUSHW1, 0, 60,
            PUSHB1, 1, SUB, DUP, PUSHW1, (byte) 0xFF, (byte) 0xF8, SWAP, JROT
        };
        ExecutionContext ctx = AssertTimeoutPreemptively(TimeSpan.FromSeconds(5),
                () => Interpreter().ExecuteProgram(program, 16));
        Equal(0, ctx.Peek(0));
        Equal(1, ctx.GetStackDepth());
    }

    /// <summary>
    /// FreeType's TT_RunIns gives a program without glyph points (fpgm/prep) 300 + 22 * cvtSize
    /// LOOPCALL iterations; the glyph formula would allow only 100 here. Keyboard.ttf's prep
    /// loops 240 times, and failing it disabled hinting for the whole font.
    /// </summary>
    [Fact]
    public void TestPrepLoopCallBudgetIsSizedFromTheControlValues()
    {
        TrueTypeInterpreter interp = Interpreter();
        interp.SetFontProgram(new byte[] { PUSHB1, 1, FDEF, PUSHB1, 1, ADD, ENDF });
        interp.PrepareFontProgram();
        interp.SetControlValueProgram(new byte[] { PUSHB1, 0, PUSHB1, (byte) 240, PUSHB1, 1, LOOPCALL,
                PUSHB1, 7, SWAP, WS });
        interp.SetPpem(16, 16);
        Equal(240, interp.ExecuteProgram(new byte[] { PUSHB1, 7, RS }, 16).Peek(0));
    }

    /// <summary>
    /// LOOPCALL takes its iteration count off the stack, so a crafted font can ask for billions.
    /// The whole loop is charged against the budget up front, so an absurd count fails before a single
    /// iteration runs.
    /// </summary>
    [Fact]
    public void TestLoopCallCountIsBounded()
    {
        TrueTypeInterpreter interp = Interpreter();
        interp.SetFontProgram(new byte[] { PUSHB1, 1, FDEF, PUSHB1, 1, ADD, ENDF });
        interp.PrepareFontProgram();

        // stack: value=0, count=32767, fn=1
        AssertTimeoutPreemptively(TimeSpan.FromSeconds(5), () =>
                Assert.Throws<HintingException>(
                        () => interp.ExecuteProgram(
                                new byte[] { PUSHB1, 0, PUSHW1, 0x7F, (byte) 0xFF, PUSHB1, 1,
                                        LOOPCALL }, 16)));
    }

    /// <summary>
    /// The spec calls the LOOPCALL count unsigned; FreeType runs nothing at all when it is not
    /// positive, so a negative count must be a no-op rather than an error or an underflowing loop.
    /// </summary>
    [Fact]
    public void TestNonPositiveLoopCallCountRunsNothing()
    {
        TrueTypeInterpreter interp = Interpreter();
        interp.SetFontProgram(new byte[] { PUSHB1, 1, FDEF, PUSHB1, 1, ADD, ENDF });
        interp.PrepareFontProgram();

        // stack: value=7, count=-1, fn=1 -> the function never runs, 7 is left untouched
        ExecutionContext ctx = interp.ExecuteProgram(
                new byte[] { PUSHB1, 7, PUSHW1, (byte) 0xFF, (byte) 0xFF, PUSHB1, 1, LOOPCALL }, 16);
        Equal(7, ctx.Peek(0));
        Equal(1, ctx.GetStackDepth());
    }

    /// <summary>
    /// A font may compute values into the storage area in prep and read them back from every
    /// glyph program, so storage belongs to the size and not to one program run. Building a fresh
    /// storage array per run made RS read zeros - silently wrong outlines rather than a failure.
    /// </summary>
    [Fact]
    public void TestStoragePersistsFromPrepIntoGlyphProgram()
    {
        TrueTypeInterpreter interp = Interpreter();
        interp.SetControlValueProgram(new byte[] { PUSHB2, 5, 42, WS }); // storage[5] = 42
        interp.SetPpem(16, 16);

        ExecutionContext ctx = interp.ExecuteProgram(new byte[] { PUSHB1, 5, RS }, 16);
        Equal(42, ctx.Peek(0));
    }

    /// <summary>
    /// INSTCTRL in prep sets or clears one flag bit, leaving the others (FreeType's Ins_INSTCTRL):
    /// selector k names bit 1 &lt;&lt; (k - 1), and the value must be 0 or that bit. Out-of-range selectors
    /// and mismatched values are ignored.
    /// </summary>
    [Fact]
    public void TestInstctrlInPrepSetsAndClearsSingleBits()
    {
        Equal(1, InstructControlAfterPrep(1, 1));
        Equal(3, InstructControlAfterPrep(1, 1, 2, 2));
        Equal(6, InstructControlAfterPrep(1, 1, 2, 2, 4, 3, 0, 1));
        Equal(0, InstructControlAfterPrep(1, 4));       // selector output of range
        Equal(0, InstructControlAfterPrep(1, 2));       // value is not the selector's bit
        Equal(0, InstructControlAfterPrep(1, 0));       // selector output of range
    }

    /// <summary>
    /// Outside prep only selector 3 has an effect, and only on the running glyph program.
    /// </summary>
    [Fact]
    public void TestInstctrlOutsidePrepLeavesFlagsAlone()
    {
        TrueTypeInterpreter interp = Interpreter();
        interp.SetPpem(16, 16);
        ExecutionContext ctx = interp.ExecuteProgram(new byte[] { PUSHB2, 1, 1, INSTCTRL }, 16);
        Equal(0, ctx.GetGraphicsState().GetInstructControl());

        ctx = interp.NewContext(interp.GetSavedState().Copy());
        ctx.SetBackwardCompatibility(true);
        interp.Run(ctx, new BytecodeStream(new byte[] { PUSHB2, 4, 3, INSTCTRL }));
        Assert.False(ctx.IsBackwardCompatibility());
        interp.Run(ctx, new BytecodeStream(new byte[] { PUSHB2, 0, 3, INSTCTRL }));
        Assert.True(ctx.IsBackwardCompatibility());
        Equal(0, ctx.GetGraphicsState().GetInstructControl());
    }

    /// <summary>
    /// Runs INSTCTRL once per (value, selector) pair in prep and returns the saved flags.
    /// </summary>
    private static int InstructControlAfterPrep(params int[] valueSelectorPairs)
    {
        byte[] prep = new byte[valueSelectorPairs.Length / 2 * 4];
        for (int i = 0, j = 0; i < valueSelectorPairs.Length; i += 2)
        {
            prep[j++] = PUSHB2;
            prep[j++] = (byte) valueSelectorPairs[i];
            prep[j++] = (byte) valueSelectorPairs[i + 1];
            prep[j++] = INSTCTRL;
        }
        TrueTypeInterpreter interp = Interpreter();
        interp.SetControlValueProgram(prep);
        interp.SetPpem(16, 16);
        return interp.GetSavedState().GetInstructControl();
    }

    /// <summary>
    /// The twilight zone belongs to the size for the same reason: prep seeds points there.
    /// </summary>
    [Fact]
    public void TestTwilightPointsPersistFromPrep()
    {
        TrueTypeInterpreter interp = new TrueTypeInterpreter(256, 16, 4, 2048);
        // prep: aim every zone pointer at the twilight zone, then set point 1's x to 128
        interp.SetControlValueProgram(new byte[] { PUSHB1, 0, SZPS, PUSHB2, 1, (byte) 128, SCFS });
        interp.SetPpem(16, 16);

        ExecutionContext ctx = interp.ExecuteProgram(new byte[] { PUSHB1, 0, SZPS, PUSHB1, 1, GC }, 16);
        Equal(128, ctx.Peek(0));
    }

    /// <summary>
    /// A new size starts clean, as FreeType clears both in tt_size_run_prep.
    /// </summary>
    [Fact]
    public void TestStorageIsClearedOnPpemChange()
    {
        TrueTypeInterpreter interp = Interpreter();
        interp.SetPpem(16, 16);
        interp.ExecuteProgram(new byte[] { PUSHB2, 5, 42, WS }, 16);
        Equal(42, interp.ExecuteProgram(new byte[] { PUSHB1, 5, RS }, 16).Peek(0));

        interp.SetPpem(24, 24);
        Equal(0, interp.ExecuteProgram(new byte[] { PUSHB1, 5, RS }, 24).Peek(0));
    }
    /// <summary>
    /// MINDEX moves the k-th element (1 = top) to the top and closes the gap, leaving everything
    /// else in order (FreeType's Ins_MINDEX). Checked for k = 1 (no-op), 2 (= SWAP), 3 (= ROLL) and a
    /// deeper k, on the whole stack rather than just the top.
    /// </summary>
    [Fact]
    public void TestMindex()
    {
        // stack bottom->top: 10 20 30 40 50
        byte[] five = { NPUSHB, 5, 10, 20, 30, 40, 50 };
        Equal(new int[] { 10, 20, 30, 40, 50 }, RunStack(Concat(five, PUSHB1, 1, MINDEX)));
        Equal(new int[] { 10, 20, 30, 50, 40 }, RunStack(Concat(five, PUSHB1, 2, MINDEX)));
        Equal(new int[] { 10, 20, 40, 50, 30 }, RunStack(Concat(five, PUSHB1, 3, MINDEX)));
        Equal(new int[] { 20, 30, 40, 50, 10 }, RunStack(Concat(five, PUSHB1, 5, MINDEX)));
        // k = 2 is SWAP and k = 3 is ROLL
        Equal(RunStack(Concat(five, SWAP)), RunStack(Concat(five, PUSHB1, 2, MINDEX)));
        Equal(RunStack(Concat(five, ROLL)), RunStack(Concat(five, PUSHB1, 3, MINDEX)));
        // output of range: k is consumed, the rest of the stack is untouched (FreeType, non-pedantic)
        Equal(new int[] { 10, 20, 30, 40, 50 }, RunStack(Concat(five, PUSHB1, 6, MINDEX)));
        Equal(new int[] { 10, 20, 30, 40, 50 }, RunStack(Concat(five, PUSHB1, 0, MINDEX)));
    }

    [Fact]
    public void TestCindex()
    {
        byte[] three = { PUSHB2, 10, 20, PUSHB1, 30 };
        Equal(new int[] { 10, 20, 30, 30 }, RunStack(Concat(three, PUSHB1, 1, CINDEX)));
        Equal(new int[] { 10, 20, 30, 10 }, RunStack(Concat(three, PUSHB1, 3, CINDEX)));
        Equal(new int[] { 10, 20, 30 }, RunStack(Concat(three, PUSHB1, 4, CINDEX)));
    }

    /// <summary>
    /// Jump offsets are relative to the jump opcode itself (offset 1 is the next instruction). A
    /// forward JMPR skips code; a backward JROT builds a countdown loop; JROF jumps only on false.
    /// </summary>
    [Fact]
    public void TestJumpsAreRelativeToTheJumpOpcode()
    {
        // JMPR +4 skips the "PUSHB1 99 POP" that follows... layout:
        //  0: PUSHB1 4   2: JMPR   3: PUSHB1 99   5: POP   6: PUSHB1 7
        // JMPR at 2, offset 4 -> 6, so 99 is never pushed
        Equal(new int[] { 7 }, RunStack(new byte[] { PUSHB1, 4, JMPR, PUSHB1, 99, POP, PUSHB1, 7 }));

        // countdown: push 3; loop: push 1, SUB, DUP, push -6 (offset), SWAP, JROT -> loops while != 0
        //  0: PUSHB1 3
        //  2: PUSHB1 1   4: SUB   5: DUP   6: PUSHW1 FF FA (-6)   9: SWAP   10: JROT  (10 - 6 = 4? no: 10-8=2)
        // JROT at 10 must land on 2: offset -8
        Equal(new int[] { 0 }, RunStack(new byte[] { PUSHB1, 3,
                PUSHB1, 1, SUB, DUP, PUSHW1, (byte) 0xFF, (byte) 0xF8, SWAP, JROT }));

        // JROF: jumps when the condition is false
        //  0: PUSHB2 4 0   3: JROF   4: PUSHB1 99   6: POP   7: PUSHB1 8
        Equal(new int[] { 8 }, RunStack(new byte[] { PUSHB2, 4, 0, JROF, PUSHB1, 99, POP, PUSHB1, 8 }));
        //  with a true condition JROF falls through
        Equal(new int[] { 99, 8 }, RunStack(new byte[] { PUSHB2, 4, 1, JROF, PUSHB1, 99, PUSHB1, 8 }));
    }

    /// <summary>
    /// Skipping a false IF branch must respect nesting and must not mistake push &lt;em&gt;data&lt;/em&gt; for
    /// opcodes: the skipped branch here contains PUSHB operands equal to the IF, ELSE and EIF opcode
    /// bytes, and a nested IF/ELSE/EIF.
    /// </summary>
    [Fact]
    public void TestNestedIfElseWithOpcodeBytesAsPushData()
    {
        byte[] program = {
            PUSHB1, 0, IF,                       // false: skip to ELSE
                PUSHB1, EIF,                     //   data byte 0x59 must not end the skip
                PUSHB1, 1, IF, PUSHB1, ELSE, ELSE, PUSHB1, IF, EIF, // nested, with data 0x1B / 0x58
                PUSHB1, 99,
            ELSE,
                PUSHB1, 1, IF, PUSHB1, 5, ELSE, PUSHB1, 6, EIF,     // nested true: 5
                PUSHB1, 0, IF, PUSHB1, 7, ELSE, PUSHB1, 8, EIF,     // nested false: 8
            EIF,
            PUSHB1, 9 };
        Equal(new int[] { 5, 8, 9 }, RunStack(program));

        // and a true outer branch skips the else-branch, which itself contains nested IFs and data
        byte[] program2 = {
            PUSHB1, 1, IF,
                PUSHB1, 1,
            ELSE,
                PUSHB1, EIF, PUSHB1, 0, IF, PUSHB1, IF, ELSE, PUSHB1, ELSE, EIF, PUSHB1, 99,
            EIF,
            PUSHB1, 2 };
        Equal(new int[] { 1, 2 }, RunStack(program2));
    }

    /// <summary>
    /// Function bodies: an IF/ELSE inside a body, a body containing push data equal to ENDF, a
    /// function calling another function (return flag must not leak to the caller), and a recursive
    /// function terminated by its own IF.
    /// </summary>
    [Fact]
    public void TestFunctionBodiesNestedCallsAndReturns()
    {
        // fn 0: doubles the top (DUP ADD), but contains an IF whose skipped branch holds ENDF as data
        // fn 1: calls fn 0 twice, then pushes 100 - the push after the inner calls proves the inner
        //       ENDF did not return from fn 1
        byte[] program = {
            PUSHB1, 0, FDEF,
                PUSHB1, 0, IF, PUSHB1, ENDF, PUSHB1, ENDF, ELSE, DUP, ADD, EIF,
            ENDF,
            PUSHB1, 1, FDEF,
                PUSHB1, 0, CALL, PUSHB1, 0, CALL, PUSHB1, 100,
            ENDF,
            PUSHB1, 3, PUSHB1, 1, CALL };
        Equal(new int[] { 12, 100 }, RunStack(program));

        // fn 2: recursive countdown: if top != 0 { push 1, SUB, call 2 } ; leaves 0
        byte[] recursive = {
            PUSHB1, 2, FDEF,
                DUP, IF, PUSHB1, 1, SUB, PUSHB1, 2, CALL, EIF,
            ENDF,
            PUSHB1, 5, PUSHB1, 2, CALL, PUSHB1, 42 };
        Equal(new int[] { 0, 42 }, RunStack(recursive));
    }

    [Fact]
    public void TestEndfOutsideFunctionIsAnError()
    {
        Assert.Throws<HintingException>( () => RunStack(new byte[] { PUSHB1, 1, ENDF }));
    }

    private static byte[] Concat(byte[] head, params int[] tail)
    {
        byte[] output = new byte[head.Length + tail.Length];
        Array.Copy(head, 0, output, 0, head.Length);
        for (int i = 0; i < tail.Length; i++)
        {
            output[head.Length + i] = (byte) tail[i];
        }
        return output;
    }
    /// <summary>
    /// A jump that leaves a function body is an error (FreeType: IP &gt; Def-&gt;end in JMPR), and
    /// a body never runs past its own ENDF even if control flow tries to.
    /// </summary>
    [Fact]
    public void TestJumpOutOfFunctionBodyIsAnError()
    {
        // fn 0 body: PUSHB1 20 JMPR -> would land far past ENDF
        byte[] program = { PUSHB1, 0, FDEF, PUSHB1, 20, JMPR, ENDF, PUSHB1, 0, CALL, PUSHB1, 1 };
        Assert.Throws<HintingException>( () => RunStack(program));
        // a jump to the ENDF itself is fine (offset 3 from JMPR at 5 lands on ENDF at 8)
        byte[] ok = { PUSHB1, 0, FDEF, PUSHB1, 3, JMPR, PUSHB1, 99, ENDF, PUSHB1, 0, CALL, PUSHB1, 1 };
        Equal(new int[] { 1 }, RunStack(ok));
    }

    /// <summary>
    /// GETINFO reports a v40 rasterizer rendering grayscale ClearType, as FreeType 2.13.2 does for the
    /// grayscale target: version 40, subpixel hinting (bit 13), ClearType (17), subpixel positioned
    /// (18) and grayscale ClearType (19), but not plain grayscale (bit 12, cleared in "lean" mode).
    /// </summary>
    [Fact]
    public void TestGetInfoReportsAGrayscaleV40Rasterizer()
    {
        // selector: version (1), grayscale (32), subpixel (64), ClearType (1024), positioned (2048),
        // grayscale ClearType (4096)
        int selector = 1 | 32 | 64 | 1024 | 2048 | 4096;
        int info = RunTop(new byte[] { PUSHW1, (byte) (selector >> 8), (byte) selector, (byte) 0x88 });
        Equal(40 | 1 << 13 | 1 << 17 | 1 << 18 | 1 << 19, info);
    }

    /// <summary>
    /// Unterminated blocks are malformed bytecode: a HintingException, so the glyph falls back.
    /// </summary>
    [Fact]
    public void TestUnterminatedBlocksAreHintingExceptions()
    {
        // false IF skipping to an ELSE/EIF that never comes
        Assert.Throws<HintingException>(
                () => Interpreter().ExecuteProgram(new byte[] { PUSHB1, 0, IF, PUSHB1, 1 }, 16));
        // true IF reaching an ELSE that skips to an EIF that never comes
        Assert.Throws<HintingException>(
                () => Interpreter().ExecuteProgram(new byte[] { PUSHB1, 1, IF, ELSE, PUSHB1, 1 }, 16));
        // FDEF without ENDF
        Assert.Throws<HintingException>(
                () => Interpreter().ExecuteProgram(new byte[] { PUSHB1, 0, FDEF, PUSHB1, 1 }, 16));
    }

    /// <summary>
    /// Storage and CVT indices output of range are HintingExceptions, not array index errors.
    /// </summary>
    [Fact]
    public void TestStorageAndControlValueIndicesAreBounded()
    {
        // the interpreter has 16 storage slots and no control values
        Assert.Throws<HintingException>(
                () => Interpreter().ExecuteProgram(new byte[] { PUSHB1, 99, RS }, 16));
        Assert.Throws<HintingException>(
                () => Interpreter().ExecuteProgram(new byte[] { PUSHB2, 99, 1, WS }, 16));
        Assert.Throws<HintingException>(
                () => Interpreter().ExecuteProgram(new byte[] { PUSHB2, 99, 1, 0x44 }, 16)); // WCVTP
    }

    /// <summary>
    /// A push's operands must not run past the ENDF of the function body it executes in. Scanning a
    /// body skips push operands, so this can only happen through a jump into the middle of another
    /// push's operands. Here the body is PUSHB[0] 2, JMPR, PUSHB[0] 0xB1, ENDF: the jump lands
    /// on the operand byte 0xB1, which executes as PUSHB[1] and would read the ENDF and the caller's
    /// next byte as its two operands.
    /// </summary>
    [Fact]
    public void TestFunctionBodyCannotReadOperandsPastEndf()
    {
        byte[] program = {
            PUSHB1, 0, FDEF,
            PUSHB1, 2, JMPR, PUSHB1, (byte) 0xB1, ENDF,
            PUSHB1, 0, CALL };
        Assert.Throws<HintingException>( () => Interpreter().ExecuteProgram(program, 16));
    }

    [Fact]
    public void TestNestedFunctionDefinitionIsAnError()
    {
        byte[] program = { PUSHB1, 0, FDEF, PUSHB1, 1, FDEF, ENDF, ENDF };
        Assert.Throws<HintingException>( () => RunStack(program));
    }
    [Fact]
    public void AbsPreservesJavaSignedMinimumOverflow()
    {
        var interp = Interpreter();
        var ctx = interp.NewContext(new GraphicsState());
        ctx.Push(int.MinValue);
        interp.Run(ctx, new BytecodeStream(new byte[] { 0x64 }));
        Assert.Equal(int.MinValue, ctx.Pop());
        Assert.Equal(long.MinValue, Fixed.Abs(long.MinValue));
    }

    private static void Equal<T>(T expected, T actual, string? message = null)
    {
        Assert.Equal(expected, actual);
    }

    private static void AssertTimeoutPreemptively(TimeSpan timeout, Action action)
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();
        action();
        Assert.True(timer.Elapsed < timeout, $"Execution exceeded {timeout}.");
    }
    private static T AssertTimeoutPreemptively<T>(TimeSpan timeout, Func<T> action)
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();
        T value = action();
        Assert.True(timer.Elapsed < timeout, $"Execution exceeded {timeout}.");
        return value;
    }

}
