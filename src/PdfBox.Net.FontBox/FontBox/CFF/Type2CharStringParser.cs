/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cff/Type2CharStringParser.java
 * PDFBOX_SOURCE_COMMIT: 1517eedc66d2a4a4d2f4097b779cb0422edbcdc8
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 1517eedc66d2a4a4d2f4097b779cb0422edbcdc8
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
/// This class represents a converter for a mapping into a Type 2 sequence.
/// </summary>
public class Type2CharStringParser
{
    private static readonly int CallsubrOpcode = (int)CharStringCommand.CALLSUBR;
    private static readonly int CallGSubrOpcode = (int)CharStringCommand.CALLGSUBR;
    private static readonly int HintmaskOpcode = (int)CharStringCommand.HINTMASK;
    private static readonly int CntrmaskOpcode = (int)CharStringCommand.CNTRMASK;

    private readonly string _fontName;

    public Type2CharStringParser(string fontName)
    {
        _fontName = fontName;
    }

    /// <summary>
    /// The given byte array will be parsed and converted to a Type 2 sequence.
    /// </summary>
    public List<object> Parse(byte[] bytes, byte[][]? globalSubrIndex, byte[][]? localSubrIndex)
    {
        GlyphData glyphData = new();
        ParseSequence(bytes, globalSubrIndex, localSubrIndex, glyphData);
        return glyphData.Sequence;
    }

    private void ParseSequence(byte[] bytes, byte[][]? globalSubrIndex, byte[][]? localSubrIndex, GlyphData glyphData)
    {
        DataInput input = new DataInputByteArray(bytes);

        while (input.HasRemaining())
        {
            int b0 = input.ReadUnsignedByte();
            if (b0 == CallsubrOpcode)
            {
                ProcessCallSubr(globalSubrIndex, localSubrIndex, glyphData);
            }
            else if (b0 == CallGSubrOpcode)
            {
                ProcessCallGSubr(globalSubrIndex, localSubrIndex, glyphData);
            }
            else if (b0 == HintmaskOpcode || b0 == CntrmaskOpcode)
            {
                glyphData.VstemCount += CountNumbers(glyphData.Sequence) / 2;
                int maskLength = GetMaskLength(glyphData.HstemCount, glyphData.VstemCount);
                for (int i = 0; i < maskLength; i++)
                {
                    input.ReadUnsignedByte();
                }

                glyphData.Sequence.Add(CharStringCommandExtensions.GetInstance(b0));
            }
            else if ((b0 >= 0 && b0 <= 18) || (b0 >= 21 && b0 <= 27) || (b0 >= 29 && b0 <= 31))
            {
                glyphData.Sequence.Add(ReadCommand(b0, input, glyphData));
            }
            else if (b0 == 28 || (b0 >= 32 && b0 <= 255))
            {
                glyphData.Sequence.Add(ReadNumber(b0, input));
            }
            else
            {
                throw new ArgumentException($"Unexpected byte value {b0}");
            }
        }
    }

    private byte[]? GetSubrBytes(byte[][] subrIndex, GlyphData glyphData)
    {
        int last = glyphData.Sequence.Count - 1;
        int operand = (int)(double)glyphData.Sequence[last];
        glyphData.Sequence.RemoveAt(last);
        int subrNumber = CalculateSubrNumber(operand, subrIndex.Length);
        return subrNumber < subrIndex.Length ? subrIndex[subrNumber] : null;
    }

    private void ProcessCallSubr(byte[][]? globalSubrIndex, byte[][]? localSubrIndex, GlyphData glyphData)
    {
        if (localSubrIndex is { Length: > 0 })
        {
            byte[]? subrBytes = GetSubrBytes(localSubrIndex, glyphData);
            if (subrBytes is not null)
            {
                ProcessSubr(globalSubrIndex, localSubrIndex, subrBytes, glyphData);
            }
        }
    }

    private void ProcessCallGSubr(byte[][]? globalSubrIndex, byte[][]? localSubrIndex, GlyphData glyphData)
    {
        if (globalSubrIndex is { Length: > 0 })
        {
            byte[]? subrBytes = GetSubrBytes(globalSubrIndex, glyphData);
            if (subrBytes is not null)
            {
                ProcessSubr(globalSubrIndex, localSubrIndex, subrBytes, glyphData);
            }
        }
    }

    private void ProcessSubr(byte[][]? globalSubrIndex, byte[][]? localSubrIndex, byte[] subrBytes, GlyphData glyphData)
    {
        ParseSequence(subrBytes, globalSubrIndex, localSubrIndex, glyphData);
        if (glyphData.Sequence.Count > 0
            && glyphData.Sequence[glyphData.Sequence.Count - 1] is CharStringCommand lastCmd
            && lastCmd.GetType2KeyWord() == Type2KeyWord.RET)
        {
            glyphData.Sequence.RemoveAt(glyphData.Sequence.Count - 1);
        }
    }

    private static int CalculateSubrNumber(int operand, int subrIndexLength)
    {
        if (subrIndexLength < 1240)
        {
            return 107 + operand;
        }

        if (subrIndexLength < 33900)
        {
            return 1131 + operand;
        }

        return 32768 + operand;
    }

    private static CharStringCommand ReadCommand(int b0, DataInput input, GlyphData glyphData)
    {
        switch (b0)
        {
            case 1:
            case 18:
                glyphData.HstemCount += CountNumbers(glyphData.Sequence) / 2;
                return CharStringCommandExtensions.GetInstance(b0);
            case 3:
            case 23:
                glyphData.VstemCount += CountNumbers(glyphData.Sequence) / 2;
                return CharStringCommandExtensions.GetInstance(b0);
            case 12:
                return CharStringCommandExtensions.GetInstance(b0, input.ReadUnsignedByte());
            default:
                return CharStringCommandExtensions.GetInstance(b0);
        }
    }

    private static double ReadNumber(int b0, DataInput input)
    {
        if (b0 == 28)
        {
            return (double)input.ReadShort();
        }

        if (b0 >= 32 && b0 <= 246)
        {
            return b0 - 139;
        }

        if (b0 >= 247 && b0 <= 250)
        {
            int b1 = input.ReadUnsignedByte();
            return (b0 - 247) * 256 + b1 + 108;
        }

        if (b0 >= 251 && b0 <= 254)
        {
            int b1 = input.ReadUnsignedByte();
            return -(b0 - 251) * 256 - b1 - 108;
        }

        if (b0 == 255)
        {
            short value = input.ReadShort();
            double fraction = input.ReadUnsignedShort() / 65535d;
            return value + fraction;
        }

        throw new ArgumentException($"Unexpected number byte {b0}");
    }

    private static int GetMaskLength(int hstemCount, int vstemCount)
    {
        int hintCount = hstemCount + vstemCount;
        int length = hintCount / 8;
        if (hintCount % 8 > 0)
        {
            length++;
        }

        return length;
    }

    private static int CountNumbers(List<object> sequence)
    {
        int count = 0;
        for (int i = sequence.Count - 1; i >= 0; i--)
        {
            if (sequence[i] is not double)
            {
                return count;
            }

            count++;
        }

        return count;
    }

    public override string ToString() => _fontName;

    private sealed class GlyphData
    {
        public List<object> Sequence { get; } = [];
        public int HstemCount { get; set; }
        public int VstemCount { get; set; }
    }
}
