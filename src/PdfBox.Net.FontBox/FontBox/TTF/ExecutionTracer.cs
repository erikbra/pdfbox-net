/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/ExecutionTracer.java
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
/// A first-class, toggleable execution tracer for the interpreter. When attached to a
/// TrueTypeInterpreter it emits one line per executed instruction, just before the instruction
/// runs, in a format deliberately close to FreeType's FT2_DEBUG=ttinterp output:
///
///
/// &lt;pc&gt;  &lt;MNEMONIC&gt;  # &lt;top of stack, deepest..top&gt;
///
///
/// The program counter and the operand stack are what matter for diffing: feeding the same glyph
/// through this tracer and through FreeType and aligning the two traces by instruction index pinpoints
/// the first instruction where control flow or an operand value diverges. See
/// src/test/resources/ttf/hinting/trace_diff.py for the diff tool.
/// </summary>
/// <remarks>
/// Author: Apache PDFBox
/// </remarks>
internal class ExecutionTracer
{
    private static readonly string[] MNEMONICS = BuildMnemonics();

    /// <summary>
    /// Number of top-of-stack operands to print, matching FreeType's trace window.
    /// </summary>
    private const int STACK_WINDOW = 8;

    private TextWriter output;
    private int tracePoint;

    /// <param name="output">
    /// where to write trace lines
    /// </param>
    public ExecutionTracer(TextWriter output) : this(output, -1)
    {
    }

    /// <param name="output">
    /// where to write trace lines
    /// </param>
    /// <param name="tracePoint">
    /// a glyph-zone point index whose current coordinate is appended to each line
    /// (for localizing silent point-position divergence), or -1 to omit
    /// </param>
    public ExecutionTracer(TextWriter output, int tracePoint)
    {
        this.output = output;
        this.tracePoint = tracePoint;
    }

    /// <param name="opcode">
    /// an opcode value 0-255
    /// </param>
    /// <returns>
    /// the mnemonic for that opcode
    /// </returns>
    public static string Mnemonic(int opcode)
    {
        return MNEMONICS[opcode & 0xFF];
    }

    /// <summary>
    /// Emits a trace line for the instruction about to execute.
    /// </summary>
    /// <param name="pc">
    /// the program-counter position of the instruction
    /// </param>
    /// <param name="opcode">
    /// the opcode about to run
    /// </param>
    /// <param name="ctx">
    /// the execution context (its stack is sampled)
    /// </param>
    internal void Trace(int pc, int opcode, ExecutionContext ctx)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(string.Format(CultureInfo.InvariantCulture, "{0:D6}  {1,-11}", pc, MNEMONICS[opcode & 0xFF]));
        if (tracePoint >= 0)
        {
            Zone zone = ctx.GetGlyphZone();
            if (zone != null && tracePoint < zone.GetPointCount())
            {
                sb.Append(string.Format(CultureInfo.InvariantCulture, " P{0}=({1},{2})", tracePoint,
                        zone.GetCurrentX()[tracePoint], zone.GetCurrentY()[tracePoint]));
            }
        }
        sb.Append(" #");
        // top of stack first, matching FreeType's ttinterp window
        int window = Math.Min(STACK_WINDOW, ctx.GetStackDepth());
        for (int k = 0; k < window; k++)
        {
            sb.Append(' ').Append(ctx.Peek(k));
        }
        output.WriteLine(sb);
    }

    private static string[] BuildMnemonics()
    {
        string[] m = new string[256];
        for (int i = 0; i < 256; i++)
        {
            m[i] = string.Format(CultureInfo.InvariantCulture, "INS_{0:X2}", i);
        }
        // axis-variant vector setters: [y] for the even (0) code, [x] for the odd (1) code
        Put(m, 0x00, "SVTCA[y]");
        Put(m, 0x01, "SVTCA[x]");
        Put(m, 0x02, "SPVTCA[y]");
        Put(m, 0x03, "SPVTCA[x]");
        Put(m, 0x04, "SFVTCA[y]");
        Put(m, 0x05, "SFVTCA[x]");
        Put(m, 0x06, "SPVTL[||]");
        Put(m, 0x07, "SPVTL[+]");
        Put(m, 0x08, "SFVTL[||]");
        Put(m, 0x09, "SFVTL[+]");
        Put(m, 0x0A, "SPVFS");
        Put(m, 0x0B, "SFVFS");
        Put(m, 0x0C, "GPV");
        Put(m, 0x0D, "GFV");
        Put(m, 0x0E, "SFVTPV");
        Put(m, 0x0F, "ISECT");
        Put(m, 0x10, "SRP0");
        Put(m, 0x11, "SRP1");
        Put(m, 0x12, "SRP2");
        Put(m, 0x13, "SZP0");
        Put(m, 0x14, "SZP1");
        Put(m, 0x15, "SZP2");
        Put(m, 0x16, "SZPS");
        Put(m, 0x17, "SLOOP");
        Put(m, 0x18, "RTG");
        Put(m, 0x19, "RTHG");
        Put(m, 0x1A, "SMD");
        Put(m, 0x1B, "ELSE");
        Put(m, 0x1C, "JMPR");
        Put(m, 0x1D, "SCVTCI");
        Put(m, 0x1E, "SSWCI");
        Put(m, 0x1F, "SSW");
        Put(m, 0x20, "DUP");
        Put(m, 0x21, "POP");
        Put(m, 0x22, "CLEAR");
        Put(m, 0x23, "SWAP");
        Put(m, 0x24, "DEPTH");
        Put(m, 0x25, "CINDEX");
        Put(m, 0x26, "MINDEX");
        Put(m, 0x27, "ALIGNPTS");
        Put(m, 0x29, "UTP");
        Put(m, 0x2A, "LOOPCALL");
        Put(m, 0x2B, "CALL");
        Put(m, 0x2C, "FDEF");
        Put(m, 0x2D, "ENDF");
        Put(m, 0x2E, "MDAP[nr]");
        Put(m, 0x2F, "MDAP[rnd]");
        Put(m, 0x30, "IUP[y]");
        Put(m, 0x31, "IUP[x]");
        Put(m, 0x32, "SHP[rp2]");
        Put(m, 0x33, "SHP[rp1]");
        Put(m, 0x34, "SHC[rp2]");
        Put(m, 0x35, "SHC[rp1]");
        Put(m, 0x36, "SHZ[rp2]");
        Put(m, 0x37, "SHZ[rp1]");
        Put(m, 0x38, "SHPIX");
        Put(m, 0x39, "IP");
        Put(m, 0x3A, "MSIRP[nr]");
        Put(m, 0x3B, "MSIRP[rp0]");
        Put(m, 0x3C, "ALIGNRP");
        Put(m, 0x3D, "RTDG");
        Put(m, 0x3E, "MIAP[nr]");
        Put(m, 0x3F, "MIAP[rnd]");
        Put(m, 0x40, "NPUSHB");
        Put(m, 0x41, "NPUSHW");
        Put(m, 0x42, "WS");
        Put(m, 0x43, "RS");
        Put(m, 0x44, "WCVTP");
        Put(m, 0x45, "RCVT");
        Put(m, 0x46, "GC[cur]");
        Put(m, 0x47, "GC[org]");
        Put(m, 0x48, "SCFS");
        Put(m, 0x49, "MD[grid]");
        Put(m, 0x4A, "MD[org]");
        Put(m, 0x4B, "MPPEM");
        Put(m, 0x4C, "MPS");
        Put(m, 0x4D, "FLIPON");
        Put(m, 0x4E, "FLIPOFF");
        Put(m, 0x4F, "DEBUG");
        Put(m, 0x50, "LT");
        Put(m, 0x51, "LTEQ");
        Put(m, 0x52, "GT");
        Put(m, 0x53, "GTEQ");
        Put(m, 0x54, "EQ");
        Put(m, 0x55, "NEQ");
        Put(m, 0x56, "ODD");
        Put(m, 0x57, "EVEN");
        Put(m, 0x58, "IF");
        Put(m, 0x59, "EIF");
        Put(m, 0x5A, "AND");
        Put(m, 0x5B, "OR");
        Put(m, 0x5C, "NOT");
        Put(m, 0x5D, "DELTAP1");
        Put(m, 0x5E, "SDB");
        Put(m, 0x5F, "SDS");
        Put(m, 0x60, "ADD");
        Put(m, 0x61, "SUB");
        Put(m, 0x62, "DIV");
        Put(m, 0x63, "MUL");
        Put(m, 0x64, "ABS");
        Put(m, 0x65, "NEG");
        Put(m, 0x66, "FLOOR");
        Put(m, 0x67, "CEILING");
        Put(m, 0x70, "WCVTF");
        Put(m, 0x71, "DELTAP2");
        Put(m, 0x72, "DELTAP3");
        Put(m, 0x73, "DELTAC1");
        Put(m, 0x74, "DELTAC2");
        Put(m, 0x75, "DELTAC3");
        Put(m, 0x76, "SROUND");
        Put(m, 0x77, "S45ROUND");
        Put(m, 0x78, "JROT");
        Put(m, 0x79, "JROF");
        Put(m, 0x7A, "ROFF");
        Put(m, 0x7C, "RUTG");
        Put(m, 0x7D, "RDTG");
        Put(m, 0x7E, "SANGW");
        Put(m, 0x7F, "AA");
        Put(m, 0x80, "FLIPPT");
        Put(m, 0x81, "FLIPRGON");
        Put(m, 0x82, "FLIPRGOFF");
        Put(m, 0x85, "SCANCTRL");
        Put(m, 0x86, "SDPVTL[||]");
        Put(m, 0x87, "SDPVTL[+]");
        Put(m, 0x88, "GETINFO");
        Put(m, 0x89, "IDEF");
        Put(m, 0x8A, "ROLL");
        Put(m, 0x8B, "MAX");
        Put(m, 0x8C, "MIN");
        Put(m, 0x8D, "SCANTYPE");
        Put(m, 0x8E, "INSTCTRL");
        for (int k = 0; k < 4; k++)
        {
            Put(m, 0x68 + k, "ROUND[" + k + "]");
            Put(m, 0x6C + k, "NROUND[" + k + "]");
        }
        for (int k = 0; k < 8; k++)
        {
            Put(m, 0xB0 + k, "PUSHB[" + (k + 1) + "]");
            Put(m, 0xB8 + k, "PUSHW[" + (k + 1) + "]");
        }
        for (int op = 0xC0; op <= 0xDF; op++)
        {
            Put(m, op, "MDRP[" + (op & 0x1F).ToString("x", CultureInfo.InvariantCulture) + "]");
        }
        for (int op = 0xE0; op <= 0xFF; op++)
        {
            Put(m, op, "MIRP[" + (op & 0x1F).ToString("x", CultureInfo.InvariantCulture) + "]");
        }
        return m;
    }

    private static void Put(string[] m, int opcode, string name)
    {
        m[opcode] = name;
    }
}
