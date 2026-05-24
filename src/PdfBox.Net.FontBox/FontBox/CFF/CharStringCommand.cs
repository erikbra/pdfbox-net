/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cff/CharStringCommand.java
 * PDFBOX_SOURCE_COMMIT: a088a1ed6367338f1d7bc9b9fab643e5e469d7ae
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: a088a1ed6367338f1d7bc9b9fab643e5e469d7ae
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

namespace PdfBox.Net.FontBox.CFF;

/// <summary>
/// This class represents a CharStringCommand.
/// </summary>
public enum CharStringCommand
{
    HSTEM = 1,
    VSTEM = 3,
    VMOVETO = 4,
    RLINETO = 5,
    HLINETO = 6,
    VLINETO = 7,
    RRCURVETO = 8,
    CLOSEPATH = 9,
    CALLSUBR = 10,
    RET = 11,
    ESCAPE = 12,
    HSBW = 13,
    ENDCHAR = 14,
    HSTEMHM = 18,
    HINTMASK = 19,
    CNTRMASK = 20,
    RMOVETO = 21,
    HMOVETO = 22,
    VSTEMHM = 23,
    RCURVELINE = 24,
    RLINECURVE = 25,
    VVCURVETO = 26,
    HHCURVETO = 27,
    SHORTINT = 28,
    CALLGSUBR = 29,
    VHCURVETO = 30,
    HVCURVETO = 31,
    DOTSECTION = 192,
    VSTEM3 = 193,
    HSTEM3 = 194,
    AND = 195,
    OR = 196,
    NOT = 197,
    SEAC = 198,
    SBW = 199,
    ABS = 201,
    ADD = 202,
    SUB = 203,
    DIV = 204,
    NEG = 206,
    EQ = 207,
    CALLOTHERSUBR = 208,
    POP = 209,
    DROP = 210,
    PUT = 212,
    GET = 213,
    IFELSE = 214,
    RANDOM = 215,
    MUL = 216,
    SQRT = 218,
    DUP = 219,
    EXCH = 220,
    INDEX = 221,
    ROLL = 222,
    SETCURRENTPOINT = 225,
    HFLEX = 226,
    FLEX = 227,
    HFLEX1 = 228,
    FLEX1 = 229,
    UNKNOWN = 99,
}

public static class CharStringCommandExtensions
{
    public static int GetValue(this CharStringCommand command) => (int)command;

    public static CharStringCommand GetInstance(int b0)
    {
        foreach (CharStringCommand cmd in Enum.GetValues<CharStringCommand>())
        {
            if ((int)cmd == b0)
            {
                return cmd;
            }
        }

        return CharStringCommand.UNKNOWN;
    }

    public static CharStringCommand GetInstance(int b0, int b1) => GetInstance((b0 << 4) + b1);

    public static CharStringCommand GetInstance(int[] values)
    {
        return values.Length switch
        {
            1 => GetInstance(values[0]),
            2 => GetInstance(values[0], values[1]),
            _ => CharStringCommand.UNKNOWN,
        };
    }

    public static Type1KeyWord? GetType1KeyWord(this CharStringCommand command)
    {
        return command switch
        {
            CharStringCommand.HSTEM => Type1KeyWord.HSTEM,
            CharStringCommand.VSTEM => Type1KeyWord.VSTEM,
            CharStringCommand.VMOVETO => Type1KeyWord.VMOVETO,
            CharStringCommand.RLINETO => Type1KeyWord.RLINETO,
            CharStringCommand.HLINETO => Type1KeyWord.HLINETO,
            CharStringCommand.VLINETO => Type1KeyWord.VLINETO,
            CharStringCommand.RRCURVETO => Type1KeyWord.RRCURVETO,
            CharStringCommand.CLOSEPATH => Type1KeyWord.CLOSEPATH,
            CharStringCommand.CALLSUBR => Type1KeyWord.CALLSUBR,
            CharStringCommand.RET => Type1KeyWord.RET,
            CharStringCommand.ESCAPE => Type1KeyWord.ESCAPE,
            CharStringCommand.HSBW => Type1KeyWord.HSBW,
            CharStringCommand.ENDCHAR => Type1KeyWord.ENDCHAR,
            CharStringCommand.RMOVETO => Type1KeyWord.RMOVETO,
            CharStringCommand.HMOVETO => Type1KeyWord.HMOVETO,
            CharStringCommand.VHCURVETO => Type1KeyWord.VHCURVETO,
            CharStringCommand.HVCURVETO => Type1KeyWord.HVCURVETO,
            CharStringCommand.DOTSECTION => Type1KeyWord.DOTSECTION,
            CharStringCommand.VSTEM3 => Type1KeyWord.VSTEM3,
            CharStringCommand.HSTEM3 => Type1KeyWord.HSTEM3,
            CharStringCommand.SEAC => Type1KeyWord.SEAC,
            CharStringCommand.SBW => Type1KeyWord.SBW,
            CharStringCommand.DIV => Type1KeyWord.DIV,
            CharStringCommand.CALLOTHERSUBR => Type1KeyWord.CALLOTHERSUBR,
            CharStringCommand.POP => Type1KeyWord.POP,
            CharStringCommand.SETCURRENTPOINT => Type1KeyWord.SETCURRENTPOINT,
            _ => null,
        };
    }

    public static Type2KeyWord? GetType2KeyWord(this CharStringCommand command)
    {
        return command switch
        {
            CharStringCommand.HSTEM => Type2KeyWord.HSTEM,
            CharStringCommand.VSTEM => Type2KeyWord.VSTEM,
            CharStringCommand.VMOVETO => Type2KeyWord.VMOVETO,
            CharStringCommand.RLINETO => Type2KeyWord.RLINETO,
            CharStringCommand.HLINETO => Type2KeyWord.HLINETO,
            CharStringCommand.VLINETO => Type2KeyWord.VLINETO,
            CharStringCommand.RRCURVETO => Type2KeyWord.RRCURVETO,
            CharStringCommand.CALLSUBR => Type2KeyWord.CALLSUBR,
            CharStringCommand.RET => Type2KeyWord.RET,
            CharStringCommand.ESCAPE => Type2KeyWord.ESCAPE,
            CharStringCommand.ENDCHAR => Type2KeyWord.ENDCHAR,
            CharStringCommand.HSTEMHM => Type2KeyWord.HSTEMHM,
            CharStringCommand.HINTMASK => Type2KeyWord.HINTMASK,
            CharStringCommand.CNTRMASK => Type2KeyWord.CNTRMASK,
            CharStringCommand.RMOVETO => Type2KeyWord.RMOVETO,
            CharStringCommand.HMOVETO => Type2KeyWord.HMOVETO,
            CharStringCommand.VSTEMHM => Type2KeyWord.VSTEMHM,
            CharStringCommand.RCURVELINE => Type2KeyWord.RCURVELINE,
            CharStringCommand.RLINECURVE => Type2KeyWord.RLINECURVE,
            CharStringCommand.VVCURVETO => Type2KeyWord.VVCURVETO,
            CharStringCommand.HHCURVETO => Type2KeyWord.HHCURVETO,
            CharStringCommand.SHORTINT => Type2KeyWord.SHORTINT,
            CharStringCommand.CALLGSUBR => Type2KeyWord.CALLGSUBR,
            CharStringCommand.VHCURVETO => Type2KeyWord.VHCURVETO,
            CharStringCommand.HVCURVETO => Type2KeyWord.HVCURVETO,
            CharStringCommand.AND => Type2KeyWord.AND,
            CharStringCommand.OR => Type2KeyWord.OR,
            CharStringCommand.NOT => Type2KeyWord.NOT,
            CharStringCommand.ABS => Type2KeyWord.ABS,
            CharStringCommand.ADD => Type2KeyWord.ADD,
            CharStringCommand.SUB => Type2KeyWord.SUB,
            CharStringCommand.DIV => Type2KeyWord.DIV,
            CharStringCommand.NEG => Type2KeyWord.NEG,
            CharStringCommand.EQ => Type2KeyWord.EQ,
            CharStringCommand.DROP => Type2KeyWord.DROP,
            CharStringCommand.PUT => Type2KeyWord.PUT,
            CharStringCommand.GET => Type2KeyWord.GET,
            CharStringCommand.IFELSE => Type2KeyWord.IFELSE,
            CharStringCommand.RANDOM => Type2KeyWord.RANDOM,
            CharStringCommand.MUL => Type2KeyWord.MUL,
            CharStringCommand.SQRT => Type2KeyWord.SQRT,
            CharStringCommand.DUP => Type2KeyWord.DUP,
            CharStringCommand.EXCH => Type2KeyWord.EXCH,
            CharStringCommand.INDEX => Type2KeyWord.INDEX,
            CharStringCommand.ROLL => Type2KeyWord.ROLL,
            CharStringCommand.HFLEX => Type2KeyWord.HFLEX,
            CharStringCommand.FLEX => Type2KeyWord.FLEX,
            CharStringCommand.HFLEX1 => Type2KeyWord.HFLEX1,
            CharStringCommand.FLEX1 => Type2KeyWord.FLEX1,
            _ => null,
        };
    }
}

/// <summary>
/// Enum of all valid Type 1 key words.
/// </summary>
public enum Type1KeyWord
{
    HSTEM,
    VSTEM,
    VMOVETO,
    RLINETO,
    HLINETO,
    VLINETO,
    RRCURVETO,
    CLOSEPATH,
    CALLSUBR,
    RET,
    ESCAPE,
    HSBW,
    ENDCHAR,
    RMOVETO,
    HMOVETO,
    VHCURVETO,
    HVCURVETO,
    DOTSECTION,
    VSTEM3,
    HSTEM3,
    SEAC,
    SBW,
    DIV,
    CALLOTHERSUBR,
    POP,
    SETCURRENTPOINT,
}

/// <summary>
/// Enum of all valid Type 2 key words.
/// </summary>
public enum Type2KeyWord
{
    HSTEM,
    VSTEM,
    VMOVETO,
    RLINETO,
    HLINETO,
    VLINETO,
    RRCURVETO,
    CALLSUBR,
    RET,
    ESCAPE,
    ENDCHAR,
    HSTEMHM,
    HINTMASK,
    CNTRMASK,
    RMOVETO,
    HMOVETO,
    VSTEMHM,
    RCURVELINE,
    RLINECURVE,
    VVCURVETO,
    HHCURVETO,
    SHORTINT,
    CALLGSUBR,
    VHCURVETO,
    HVCURVETO,
    AND,
    OR,
    NOT,
    ABS,
    ADD,
    SUB,
    DIV,
    NEG,
    EQ,
    DROP,
    PUT,
    GET,
    IFELSE,
    RANDOM,
    MUL,
    SQRT,
    DUP,
    EXCH,
    INDEX,
    ROLL,
    HFLEX,
    FLEX,
    HFLEX1,
    FLEX1,
}
