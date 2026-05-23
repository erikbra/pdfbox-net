/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cff/Type2CharStringParser.java
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

public sealed class Type2CharStringParser(string fontName)
{
    private const int CallSubr = 10;
    private const int CallGSubr = 29;
    private const int HintMask = 19;
    private const int CntrMask = 20;

    private readonly string _fontName = fontName;

    public List<object> Parse(byte[] bytes, byte[][] globalSubrIndex, byte[][] localSubrIndex)
    {
        GlyphData glyphData = new();
        ParseSequence(bytes, globalSubrIndex, localSubrIndex, glyphData);
        return glyphData.Sequence;
    }

    private void ParseSequence(byte[] bytes, byte[][] globalSubrIndex, byte[][] localSubrIndex, GlyphData glyphData)
    {
        DataInput input = new DataInputByteArray(bytes);
        while (input.HasRemaining())
        {
            int b0 = input.ReadUnsignedByte();
            if (b0 == CallSubr)
            {
                ProcessCallSubr(globalSubrIndex, localSubrIndex, glyphData);
            }
            else if (b0 == CallGSubr)
            {
                ProcessCallGSubr(globalSubrIndex, localSubrIndex, glyphData);
            }
            else if (b0 is HintMask or CntrMask)
            {
                glyphData.VstemCount += CountNumbers(glyphData.Sequence) / 2;
                int maskLength = GetMaskLength(glyphData.HstemCount, glyphData.VstemCount);
                for (int i = 0; i < maskLength; i++)
                {
                    input.ReadUnsignedByte();
                }

                glyphData.Sequence.Add(CharStringCommand.GetInstance(b0));
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
                throw new IOException("Invalid Type2 charstring byte");
            }
        }
    }

    private static byte[]? GetSubrBytes(byte[][] subrIndex, GlyphData glyphData)
    {
        int operand = glyphData.Sequence[^1] is int intOperand ? intOperand : 0;
        glyphData.Sequence.RemoveAt(glyphData.Sequence.Count - 1);
        int subrNumber = CalculateSubrNumber(operand, subrIndex.Length);
        return subrNumber < subrIndex.Length ? subrIndex[subrNumber] : null;
    }

    private void ProcessCallSubr(byte[][] globalSubrIndex, byte[][] localSubrIndex, GlyphData glyphData)
    {
        if (localSubrIndex.Length == 0)
        {
            return;
        }

        byte[]? subrBytes = GetSubrBytes(localSubrIndex, glyphData);
        ProcessSubr(globalSubrIndex, localSubrIndex, subrBytes, glyphData);
    }

    private void ProcessCallGSubr(byte[][] globalSubrIndex, byte[][] localSubrIndex, GlyphData glyphData)
    {
        if (globalSubrIndex.Length == 0)
        {
            return;
        }

        byte[]? subrBytes = GetSubrBytes(globalSubrIndex, glyphData);
        ProcessSubr(globalSubrIndex, localSubrIndex, subrBytes, glyphData);
    }

    private void ProcessSubr(byte[][] globalSubrIndex, byte[][] localSubrIndex, byte[]? subrBytes, GlyphData glyphData)
    {
        if (subrBytes is null)
        {
            return;
        }

        ParseSequence(subrBytes, globalSubrIndex, localSubrIndex, glyphData);
        if (glyphData.Sequence.Count > 0 && glyphData.Sequence[^1] is CharStringCommand command && command.Type2 == CharStringCommand.Type2KeyWord.RET)
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
                return CharStringCommand.GetInstance(b0);
            case 3:
            case 23:
                glyphData.VstemCount += CountNumbers(glyphData.Sequence) / 2;
                return CharStringCommand.GetInstance(b0);
            case 12:
                return CharStringCommand.GetInstance(b0, input.ReadUnsignedByte());
            default:
                return CharStringCommand.GetInstance(b0);
        }
    }

    private static object ReadNumber(int b0, DataInput input)
    {
        if (b0 == 28)
        {
            return (int)input.ReadShort();
        }

        if (b0 >= 32 && b0 <= 246)
        {
            return b0 - 139;
        }

        if (b0 >= 247 && b0 <= 250)
        {
            int b1 = input.ReadUnsignedByte();
            return ((b0 - 247) * 256) + b1 + 108;
        }

        if (b0 >= 251 && b0 <= 254)
        {
            int b1 = input.ReadUnsignedByte();
            return -((b0 - 251) * 256) - b1 - 108;
        }

        if (b0 == 255)
        {
            short value = input.ReadShort();
            double fraction = input.ReadUnsignedShort() / 65535d;
            return value + fraction;
        }

        throw new IOException("Invalid Type2 charstring number encoding");
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
            if (sequence[i] is not int and not double)
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
