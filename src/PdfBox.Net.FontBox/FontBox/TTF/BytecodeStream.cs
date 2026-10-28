/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/BytecodeStream.java
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
/// A bounds-checked cursor over a TrueType bytecode program (fpgm, prep, a glyph instruction stream, or
/// a function body). It owns the program counter so individual opcode handlers never index the array by
/// hand - this removes a whole class of off-by-one and overrun bugs and is trivially testable on its
/// own. Reads past the end throw HintingException so the per-glyph fallback can catch cleanly.
/// 
/// @author Apache PDFBox
/// </summary>
internal class BytecodeStream
{
    private byte[] code;
    private int ip;
    private int instructionStart;
    // exclusive upper bound for the program counter; a function body ends at its ENDF
    private int limit;

    /// <summary>
/// @param code the bytecode program; not copied
/// </summary>
    public BytecodeStream(byte[] code)
    {
        this.code = code != null ? code : new byte[0];
        this.limit = this.code.Length;
    }

    /// <summary>
/// @return the underlying bytecode array (not copied); used to record function entry points
/// </summary>
    public byte[] GetCode()
    {
        return code;
    }

    /// <summary>
/// @return true if there is at least one more byte to read
/// </summary>
    public bool HasNext()
    {
        return ip < limit;
    }

    /// <summary>
/// Restricts execution to [0, end]: the stream is exhausted after the byte at end
/// (the ENDF of a function body) and a seek past it is an error, as a jump output of a
/// function body is in FreeType.
/// 
/// @param end the position of the last byte that may be executed
/// </summary>
    public void SetEnd(int end)
    {
        this.limit = Math.Min(code.Length, end + 1);
    }

    /// <summary>
/// @return the current program-counter position
/// </summary>
    public int Position()
    {
        return ip;
    }

    /// <summary>
/// Records the current position as the start of the instruction about to be read. Relative jumps
/// (JMPR/JROT/JROF) are measured from here.
/// </summary>
    public void MarkInstructionStart()
    {
        instructionStart = ip;
    }

    /// <summary>
/// @return the position recorded by the most recent #markInstructionStart()
/// </summary>
    public int InstructionStart()
    {
        return instructionStart;
    }

    /// <summary>
/// Moves the program counter to an absolute position.
/// 
/// @param position the new position, within [0, length]
/// @throws HintingException if the position is output of range
/// </summary>
    public void Seek(int position)
    {
        if (position < 0 || position > limit)
        {
            throw new HintingException(
                    "bytecode seek output of range: " + position + " of " + limit);
        }
        ip = position;
    }

    /// <summary>
/// Advances the program counter by a relative amount (may be negative).
/// 
/// @param delta the number of bytes to skip
/// @throws HintingException if the result is output of range
/// </summary>
    public void Skip(int delta)
    {
        Seek(ip + delta);
    }

    /// <summary>
/// Reads the next byte as an unsigned 0-255 value (an opcode, or push operand). Like
/// #seek(int), reads honour a function body's end, so a push reached by a jump into the
/// middle of another push's operands cannot take bytes past the body's ENDF.
/// 
/// @return the next unsigned byte
/// @throws HintingException if the stream is exhausted
/// </summary>
    public int NextByte()
    {
        if (ip >= limit)
        {
            throw new HintingException("bytecode read past end at " + ip);
        }
        return code[ip++] & 0xFF;
    }

    /// <summary>
/// Reads the next two bytes as a signed big-endian 16-bit word.
/// 
/// @return the next signed word
/// @throws HintingException if fewer than two bytes remain
/// </summary>
    public int NextWord()
    {
        if (ip + 1 >= limit)
        {
            throw new HintingException("bytecode word read past end at " + ip);
        }
        int hi = code[ip++] & 0xFF;
        int lo = code[ip++] & 0xFF;
        return (short) ((hi << 8) | lo);
    }
}
