/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cff/Type1CharStringParser.java
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

public sealed class Type1CharStringParser(string fontName)
{
    private const int CallSubr = 10;
    private const int TwoByte = 12;
    private const int CallOtherSubr = 16;
    private const int Pop = 17;

    private readonly string _fontName = fontName;
    private string _currentGlyph = string.Empty;

    public List<object> Parse(byte[] bytes, List<byte[]> subrs, string glyphName)
    {
        _currentGlyph = glyphName;
        return Parse(bytes, subrs, []);
    }

    private List<object> Parse(byte[] bytes, List<byte[]> subrs, List<object> sequence)
    {
        DataInput input = new DataInputByteArray(bytes);
        while (input.HasRemaining())
        {
            int b0 = input.ReadUnsignedByte();
            if (b0 == CallSubr)
            {
                ProcessCallSubr(subrs, sequence);
            }
            else if (b0 == TwoByte && input.PeekUnsignedByte(0) == CallOtherSubr)
            {
                ProcessCallOtherSubr(input, sequence);
            }
            else if (b0 <= 31)
            {
                sequence.Add(ReadCommand(input, b0));
            }
            else
            {
                sequence.Add(ReadNumber(input, b0));
            }
        }

        return sequence;
    }

    private void ProcessCallSubr(List<byte[]> subrs, List<object> sequence)
    {
        if (sequence.Count == 0)
        {
            return;
        }

        object operandObject = sequence[^1];
        sequence.RemoveAt(sequence.Count - 1);
        if (operandObject is not int operand)
        {
            return;
        }

        if (operand >= 0 && operand < subrs.Count)
        {
            byte[] subrBytes = subrs[operand];
            Parse(subrBytes, subrs, sequence);
            if (sequence.Count > 0 && sequence[^1] is CharStringCommand command && command.Type1 == CharStringCommand.Type1KeyWord.RET)
            {
                sequence.RemoveAt(sequence.Count - 1);
            }
        }
        else
        {
            while (sequence.Count > 0 && sequence[^1] is int)
            {
                sequence.RemoveAt(sequence.Count - 1);
            }
        }
    }

    private void ProcessCallOtherSubr(DataInput input, List<object> sequence)
    {
        input.ReadByte();

        int otherSubrNum = RemoveInteger(sequence);
        int numArgs = RemoveInteger(sequence);
        Stack<int> results = [];

        switch (otherSubrNum)
        {
            case 0:
                results.Push(RemoveInteger(sequence));
                results.Push(RemoveInteger(sequence));
                if (sequence.Count > 0)
                {
                    sequence.RemoveAt(sequence.Count - 1);
                }

                sequence.Add(0);
                sequence.Add(CharStringCommand.GetInstance(12, 16));
                break;
            case 1:
                sequence.Add(1);
                sequence.Add(CharStringCommand.GetInstance(12, 16));
                break;
            case 3:
                results.Push(RemoveInteger(sequence));
                break;
            default:
                for (int i = 0; i < numArgs; i++)
                {
                    results.Push(RemoveInteger(sequence));
                }

                break;
        }

        while (input.HasRemaining() && input.PeekUnsignedByte(0) == TwoByte && input.PeekUnsignedByte(1) == Pop)
        {
            input.ReadByte();
            input.ReadByte();
            sequence.Add(results.Count > 0 ? results.Pop() : 0);
        }
    }

    private int RemoveInteger(List<object> sequence)
    {
        if (sequence.Count == 0)
        {
            throw new IOException("Missing integer operand in Type1 charstring sequence");
        }

        object item = sequence[^1];
        sequence.RemoveAt(sequence.Count - 1);
        if (item is int intValue)
        {
            return intValue;
        }

        if (item is CharStringCommand command && command.Type1 == CharStringCommand.Type1KeyWord.DIV)
        {
            int a = RemoveInteger(sequence);
            int b = RemoveInteger(sequence);
            return b / a;
        }

        throw new IOException($"Unexpected char string command in glyph {_currentGlyph} of font {_fontName}");
    }

    private static CharStringCommand ReadCommand(DataInput input, int b0)
    {
        if (b0 == 12)
        {
            int b1 = input.ReadUnsignedByte();
            return CharStringCommand.GetInstance(b0, b1);
        }

        return CharStringCommand.GetInstance(b0);
    }

    private static int ReadNumber(DataInput input, int b0)
    {
        return b0 switch
        {
            >= 32 and <= 246 => b0 - 139,
            >= 247 and <= 250 => ((b0 - 247) * 256) + input.ReadUnsignedByte() + 108,
            >= 251 and <= 254 => -((b0 - 251) * 256) - input.ReadUnsignedByte() - 108,
            255 => input.ReadInt(),
            _ => throw new IOException("Invalid Type1 charstring number encoding"),
        };
    }
}
