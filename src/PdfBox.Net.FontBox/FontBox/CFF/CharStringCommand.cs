/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cff/CharStringCommand.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: trunk
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

public sealed class CharStringCommand
{
    public enum Type1KeyWord
    {
        HSTEM, VSTEM, VMOVETO, RLINETO, HLINETO, VLINETO, RRCURVETO, CLOSEPATH,
        CALLSUBR, RET, ESCAPE, HSBW, ENDCHAR, RMOVETO, HMOVETO, VHCURVETO, HVCURVETO,
        DOTSECTION, VSTEM3, HSTEM3, SEAC, SBW, DIV, CALLOTHERSUBR, POP, SETCURRENTPOINT
    }

    public enum Type2KeyWord
    {
        HSTEM, VSTEM, VMOVETO, RLINETO, HLINETO, VLINETO, RRCURVETO, CALLSUBR, RET, ESCAPE,
        ENDCHAR, HSTEMHM, HINTMASK, CNTRMASK, RMOVETO, HMOVETO, VSTEMHM, RCURVELINE,
        RLINECURVE, VVCURVETO, HHCURVETO, SHORTINT, CALLGSUBR, VHCURVETO, HVCURVETO,
        AND, OR, NOT, ABS, ADD, SUB, DIV, NEG, EQ, DROP, PUT, GET, IFELSE, RANDOM, MUL,
        SQRT, DUP, EXCH, INDEX, ROLL, HFLEX, FLEX, HFLEX1, FLEX1
    }

    public static readonly CharStringCommand UNKNOWN = new(null, null, 99);

    private static readonly List<CharStringCommand> All =
    [
        new(Type1KeyWord.HSTEM, Type2KeyWord.HSTEM, 1),
        new(Type1KeyWord.VSTEM, Type2KeyWord.VSTEM, 3),
        new(Type1KeyWord.VMOVETO, Type2KeyWord.VMOVETO, 4),
        new(Type1KeyWord.RLINETO, Type2KeyWord.RLINETO, 5),
        new(Type1KeyWord.HLINETO, Type2KeyWord.HLINETO, 6),
        new(Type1KeyWord.VLINETO, Type2KeyWord.VLINETO, 7),
        new(Type1KeyWord.RRCURVETO, Type2KeyWord.RRCURVETO, 8),
        new(Type1KeyWord.CLOSEPATH, null, 9),
        new(Type1KeyWord.CALLSUBR, Type2KeyWord.CALLSUBR, 10),
        new(Type1KeyWord.RET, Type2KeyWord.RET, 11),
        new(Type1KeyWord.ESCAPE, Type2KeyWord.ESCAPE, 12),
        new(Type1KeyWord.HSBW, null, 13),
        new(Type1KeyWord.ENDCHAR, Type2KeyWord.ENDCHAR, 14),
        new(null, Type2KeyWord.HSTEMHM, 18),
        new(null, Type2KeyWord.HINTMASK, 19),
        new(null, Type2KeyWord.CNTRMASK, 20),
        new(Type1KeyWord.RMOVETO, Type2KeyWord.RMOVETO, 21),
        new(Type1KeyWord.HMOVETO, Type2KeyWord.HMOVETO, 22),
        new(null, Type2KeyWord.VSTEMHM, 23),
        new(null, Type2KeyWord.RCURVELINE, 24),
        new(null, Type2KeyWord.RLINECURVE, 25),
        new(null, Type2KeyWord.VVCURVETO, 26),
        new(null, Type2KeyWord.HHCURVETO, 27),
        new(null, Type2KeyWord.SHORTINT, 28),
        new(null, Type2KeyWord.CALLGSUBR, 29),
        new(Type1KeyWord.VHCURVETO, Type2KeyWord.VHCURVETO, 30),
        new(Type1KeyWord.HVCURVETO, Type2KeyWord.HVCURVETO, 31),
        new(Type1KeyWord.DOTSECTION, null, 192),
        new(Type1KeyWord.VSTEM3, null, 193),
        new(Type1KeyWord.HSTEM3, null, 194),
        new(null, Type2KeyWord.AND, 195),
        new(null, Type2KeyWord.OR, 196),
        new(null, Type2KeyWord.NOT, 197),
        new(Type1KeyWord.SEAC, null, 198),
        new(Type1KeyWord.SBW, null, 199),
        new(null, Type2KeyWord.ABS, 201),
        new(null, Type2KeyWord.ADD, 202),
        new(null, Type2KeyWord.SUB, 203),
        new(Type1KeyWord.DIV, Type2KeyWord.DIV, 204),
        new(null, Type2KeyWord.NEG, 206),
        new(null, Type2KeyWord.EQ, 207),
        new(Type1KeyWord.CALLOTHERSUBR, null, 208),
        new(Type1KeyWord.POP, null, 209),
        new(null, Type2KeyWord.DROP, 210),
        new(null, Type2KeyWord.PUT, 212),
        new(null, Type2KeyWord.GET, 213),
        new(null, Type2KeyWord.IFELSE, 214),
        new(null, Type2KeyWord.RANDOM, 215),
        new(null, Type2KeyWord.MUL, 216),
        new(null, Type2KeyWord.SQRT, 218),
        new(null, Type2KeyWord.DUP, 219),
        new(null, Type2KeyWord.EXCH, 220),
        new(null, Type2KeyWord.INDEX, 221),
        new(null, Type2KeyWord.ROLL, 222),
        new(Type1KeyWord.SETCURRENTPOINT, null, 225),
        new(null, Type2KeyWord.HFLEX, 226),
        new(null, Type2KeyWord.FLEX, 227),
        new(null, Type2KeyWord.HFLEX1, 228),
        new(null, Type2KeyWord.FLEX1, 229),
    ];

    private static readonly CharStringCommand[] CommandsByValue;

    static CharStringCommand()
    {
        int max = All.Max(static command => command.Value);
        CommandsByValue = new CharStringCommand[max + 1];
        foreach (CharStringCommand command in All)
        {
            CommandsByValue[command.Value] = command;
        }
    }

    private CharStringCommand(Type1KeyWord? type1KeyWord, Type2KeyWord? type2KeyWord, int value)
    {
        Type1 = type1KeyWord;
        Type2 = type2KeyWord;
        Value = value;
        _stringValue = value == 99 ? "unknown command|" : $"{(type2KeyWord?.ToString() ?? type1KeyWord?.ToString() ?? "UNKNOWN")}|";
    }

    private readonly string _stringValue;

    public Type1KeyWord? Type1 { get; }
    public Type2KeyWord? Type2 { get; }
    public int Value { get; }

    public static CharStringCommand GetInstance(int b0)
    {
        if (b0 >= 0 && b0 < CommandsByValue.Length && CommandsByValue[b0] is not null)
        {
            return CommandsByValue[b0];
        }

        return UNKNOWN;
    }

    public static CharStringCommand GetInstance(int b0, int b1)
    {
        return GetInstance((b0 << 4) + b1);
    }

    public static CharStringCommand GetInstance(int[] values)
    {
        return values.Length switch
        {
            1 => GetInstance(values[0]),
            2 => GetInstance(values[0], values[1]),
            _ => UNKNOWN,
        };
    }

    public override string ToString() => _stringValue;
}
