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

/// <summary>
/// This class represents a converter for a mapping into a Type 1 sequence.
/// </summary>
public class Type1CharStringParser
{
    private const int CallsubrOpcode = 10;
    private const int TwoByteOpcode = 12;
    private const int CallOtherSubrOpcode = 16;
    private const int PopOpcode = 17;

    private readonly string _fontName;
    private string _currentGlyph = string.Empty;

    public Type1CharStringParser(string fontName)
    {
        _fontName = fontName;
    }

    /// <summary>
    /// The given byte array will be parsed and converted to a Type 1 sequence.
    /// </summary>
    public List<object> Parse(byte[] bytes, IList<byte[]> subrs, string glyphName)
    {
        _currentGlyph = glyphName;
        return Parse(bytes, subrs, []);
    }

    private List<object> Parse(byte[] bytes, IList<byte[]> subrs, List<object> sequence)
    {
        DataInput input = new DataInputByteArray(bytes);
        while (input.HasRemaining())
        {
            int b0 = input.ReadUnsignedByte();
            if (b0 == CallsubrOpcode)
            {
                ProcessCallSubr(subrs, sequence);
            }
            else if (b0 == TwoByteOpcode && input.PeekUnsignedByte(0) == CallOtherSubrOpcode)
            {
                ProcessCallOtherSubr(input, sequence);
            }
            else if (b0 >= 0 && b0 <= 31)
            {
                sequence.Add(ReadCommand(input, b0));
            }
            else if (b0 >= 32 && b0 <= 255)
            {
                sequence.Add(ReadNumber(input, b0));
            }
            else
            {
                throw new ArgumentException($"Unexpected byte value {b0}");
            }
        }

        return sequence;
    }

    private void ProcessCallSubr(IList<byte[]> subrs, List<object> sequence)
    {
        object obj = sequence[sequence.Count - 1];
        sequence.RemoveAt(sequence.Count - 1);
        if (obj is not int operand)
        {
            // warn: parameter is not an integer - skip
            return;
        }

        if (operand >= 0 && operand < subrs.Count)
        {
            byte[] subrBytes = subrs[operand];
            Parse(subrBytes, subrs, sequence);
            if (sequence.Count > 0
                && sequence[sequence.Count - 1] is CharStringCommand lastCmd
                && lastCmd.GetType1KeyWord() == Type1KeyWord.RET)
            {
                sequence.RemoveAt(sequence.Count - 1);
            }
        }
        else
        {
            // warn: CALLSUBR operand out of range - remove trailing integer params
            while (sequence.Count > 0 && sequence[sequence.Count - 1] is int)
            {
                sequence.RemoveAt(sequence.Count - 1);
            }
        }
    }

    private void ProcessCallOtherSubr(DataInput input, List<object> sequence)
    {
        input.ReadByte(); // consume the second byte (CallOtherSubrOpcode)

        int othersubrNum = (int)RemoveInteger(sequence);
        int numArgs = (int)RemoveInteger(sequence);

        Stack<int> results = new();
        switch (othersubrNum)
        {
            case 0:
                results.Push(RemoveInteger(sequence));
                results.Push(RemoveInteger(sequence));
                sequence.RemoveAt(sequence.Count - 1);
                sequence.Add(0);
                sequence.Add(CharStringCommand.CALLOTHERSUBR);
                break;
            case 1:
                sequence.Add(1);
                sequence.Add(CharStringCommand.CALLOTHERSUBR);
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

        while (input.HasRemaining()
            && input.PeekUnsignedByte(0) == TwoByteOpcode
            && input.PeekUnsignedByte(1) == PopOpcode)
        {
            input.ReadByte();
            input.ReadByte();
            sequence.Add(results.Pop());
        }
    }

    private static int RemoveInteger(List<object> sequence)
    {
        object item = sequence[sequence.Count - 1];
        sequence.RemoveAt(sequence.Count - 1);
        if (item is int i)
        {
            return i;
        }

        CharStringCommand command = (CharStringCommand)item;
        if (command.GetType1KeyWord() == Type1KeyWord.DIV)
        {
            int a = RemoveInteger(sequence);
            int b = RemoveInteger(sequence);
            return b / a;
        }

        throw new IOException($"Unexpected char string command: {command.GetType1KeyWord()}");
    }

    private static CharStringCommand ReadCommand(DataInput input, int b0)
    {
        if (b0 == 12)
        {
            int b1 = input.ReadUnsignedByte();
            return CharStringCommandExtensions.GetInstance(b0, b1);
        }

        return CharStringCommandExtensions.GetInstance(b0);
    }

    private static int ReadNumber(DataInput input, int b0)
    {
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
            return input.ReadInt();
        }

        throw new ArgumentException($"Unexpected number byte {b0}");
    }
}
