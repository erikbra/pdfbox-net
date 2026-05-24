/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cmap/CMapParser.java
 * PDFBOX_SOURCE_COMMIT: 746cf4e103f4c5ef3897edd3715088ca43beee42
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 746cf4e103f4c5ef3897edd3715088ca43beee42
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

using System.Globalization;
using System.Text;
using PdfBox.Net.IO;

namespace PdfBox.Net.FontBox.CMap;

/// <summary>
/// Parses a CMap stream.
/// </summary>
public class CMapParser
{
    private const string MarkEndOfDictionary = ">>";
    private const string MarkEndOfArray = "]";

    private readonly byte[] _tokenParserByteBuffer = new byte[512];
    private bool _strictMode;

    public CMapParser()
    {
    }

    public CMapParser(bool strictMode)
    {
        _strictMode = strictMode;
    }

    public CMap ParsePredefined(string name)
    {
        using RandomAccessRead randomAccessRead = GetExternalCMap(name);
        _strictMode = false;
        return Parse(randomAccessRead);
    }

    public CMap Parse(RandomAccessRead randomAccessRead)
    {
        CMap result = new();
        object? previousToken = null;
        object? token = ParseNextToken(randomAccessRead);

        while (token is not null)
        {
            if (token is Operator op)
            {
                if (op.Op == "endcmap")
                {
                    break;
                }

                if (op.Op == "usecmap" && previousToken is LiteralName useCmapName)
                {
                    ParseUsecmap(useCmapName, result);
                }
                else if (previousToken is int or double)
                {
                    int countValue = Convert.ToInt32(previousToken, CultureInfo.InvariantCulture);
                    if (op.Op == "begincodespacerange")
                    {
                        ParseBegincodespacerange(countValue, randomAccessRead, result);
                    }
                    else if (op.Op == "beginbfchar")
                    {
                        ParseBeginbfchar(countValue, randomAccessRead, result);
                    }
                    else if (op.Op == "beginbfrange")
                    {
                        ParseBeginbfrange(countValue, randomAccessRead, result);
                    }
                    else if (op.Op == "begincidchar")
                    {
                        ParseBegincidchar(countValue, randomAccessRead, result);
                    }
                    else if (op.Op == "begincidrange" && previousToken is int)
                    {
                        ParseBegincidrange(countValue, randomAccessRead, result);
                    }
                }
            }
            else if (token is LiteralName literal)
            {
                ParseLiteralName(literal, randomAccessRead, result);
            }

            previousToken = token;
            token = ParseNextToken(randomAccessRead);
        }

        return result;
    }

    private void ParseUsecmap(LiteralName useCmapName, CMap result)
    {
        using RandomAccessRead randomAccessRead = GetExternalCMap(useCmapName.Name);
        CMap useCMap = Parse(randomAccessRead);
        result.UseCmap(useCMap);
    }

    private void ParseLiteralName(LiteralName literal, RandomAccessRead randomAccessRead, CMap result)
    {
        switch (literal.Name)
        {
            case "WMode":
            {
                object? next = ParseNextToken(randomAccessRead);
                if (next is int wMode)
                {
                    result.WMode = wMode;
                }

                break;
            }
            case "CMapName":
            {
                object? next = ParseNextToken(randomAccessRead);
                if (next is LiteralName name)
                {
                    result.Name = name.Name;
                }

                break;
            }
            case "CMapVersion":
            {
                object? next = ParseNextToken(randomAccessRead);
                if (next is int or double)
                {
                    result.Version = Convert.ToString(next, CultureInfo.InvariantCulture);
                }
                else if (next is string version)
                {
                    result.Version = version;
                }

                break;
            }
            case "CMapType":
            {
                object? next = ParseNextToken(randomAccessRead);
                if (next is int type)
                {
                    result.Type = type;
                }

                break;
            }
            case "Registry":
            {
                object? next = ParseNextToken(randomAccessRead);
                if (next is string registry)
                {
                    result.Registry = registry;
                }

                break;
            }
            case "Ordering":
            {
                object? next = ParseNextToken(randomAccessRead);
                if (next is string ordering)
                {
                    result.Ordering = ordering;
                }

                break;
            }
            case "Supplement":
            {
                object? next = ParseNextToken(randomAccessRead);
                if (next is int supplement)
                {
                    result.Supplement = supplement;
                }

                break;
            }
            default:
                break;
        }
    }

    private static void CheckExpectedOperator(Operator op, string expectedOperatorName, string rangeName)
    {
        if (op.Op != expectedOperatorName)
        {
            throw new IOException($"Error : ~{rangeName} contains an unexpected operator : {op.Op}");
        }
    }

    private void ParseBegincodespacerange(int cosCount, RandomAccessRead randomAccessRead, CMap result)
    {
        for (int j = 0; j < cosCount; j++)
        {
            object? nextToken = ParseNextToken(randomAccessRead);
            if (nextToken is Operator op)
            {
                CheckExpectedOperator(op, "endcodespacerange", "codespacerange");
                break;
            }

            if (nextToken is not byte[] startRange)
            {
                throw new IOException("start range missing");
            }

            byte[] endRange = ParseByteArray(randomAccessRead);
            try
            {
                result.AddCodespaceRange(new CodespaceRange(startRange, endRange));
            }
            catch (ArgumentException ex)
            {
                throw new IOException(ex.Message, ex);
            }
        }
    }

    private void ParseBeginbfchar(int cosCount, RandomAccessRead randomAccessRead, CMap result)
    {
        for (int j = 0; j < cosCount; j++)
        {
            object? nextToken = ParseNextToken(randomAccessRead);
            if (nextToken is Operator op)
            {
                CheckExpectedOperator(op, "endbfchar", "bfchar");
                break;
            }

            if (nextToken is not byte[] inputCode)
            {
                throw new IOException("input code missing");
            }

            nextToken = ParseNextToken(randomAccessRead);
            if (nextToken is byte[] bytes)
            {
                string value = CreateStringFromBytes(bytes);
                result.AddCharMapping(inputCode, value);
            }
            else if (nextToken is LiteralName literalName)
            {
                result.AddCharMapping(inputCode, literalName.Name);
            }
            else
            {
                throw new IOException($"Error parsing CMap beginbfchar, expected{{COSString or COSName}} and not {nextToken}");
            }
        }
    }

    private void ParseBegincidrange(int numberOfLines, RandomAccessRead randomAccessRead, CMap result)
    {
        for (int n = 0; n < numberOfLines; n++)
        {
            object? nextToken = ParseNextToken(randomAccessRead);
            if (nextToken is Operator op)
            {
                CheckExpectedOperator(op, "endcidrange", "cidrange");
                break;
            }

            if (nextToken is not byte[] startCode)
            {
                throw new IOException("start code missing");
            }

            byte[] endCode = ParseByteArray(randomAccessRead);
            int mappedCode = ParseInteger(randomAccessRead);

            if (startCode.Length == endCode.Length)
            {
                if (startCode.SequenceEqual(endCode))
                {
                    result.AddCIDMapping(startCode, mappedCode);
                }
                else
                {
                    result.AddCIDRange(startCode, endCode, mappedCode);
                }
            }
            else
            {
                throw new IOException("Error : ~cidrange values must not have different byte lengths");
            }
        }
    }

    private void ParseBegincidchar(int cosCount, RandomAccessRead randomAccessRead, CMap result)
    {
        for (int j = 0; j < cosCount; j++)
        {
            object? nextToken = ParseNextToken(randomAccessRead);
            if (nextToken is Operator op)
            {
                CheckExpectedOperator(op, "endcidchar", "cidchar");
                break;
            }

            if (nextToken is not byte[] inputCode)
            {
                throw new IOException("input code missing");
            }

            int mappedCid = ParseInteger(randomAccessRead);
            result.AddCIDMapping(inputCode, mappedCid);
        }
    }

    private void ParseBeginbfrange(int cosCount, RandomAccessRead randomAccessRead, CMap result)
    {
        for (int j = 0; j < cosCount; j++)
        {
            object? nextToken = ParseNextToken(randomAccessRead);
            if (nextToken is Operator op)
            {
                CheckExpectedOperator(op, "endbfrange", "bfrange");
                break;
            }

            if (nextToken is not byte[] startCode)
            {
                throw new IOException("start code missing");
            }

            nextToken = ParseNextToken(randomAccessRead);
            if (nextToken is Operator op2)
            {
                CheckExpectedOperator(op2, "endbfrange", "bfrange");
                break;
            }

            if (nextToken is not byte[] endCode)
            {
                throw new IOException("end code missing");
            }

            int start = CMap.ToInt(startCode);
            int end = CMap.ToInt(endCode);
            if (end < start)
            {
                break;
            }

            nextToken = ParseNextToken(randomAccessRead);
            if (nextToken is List<object> array)
            {
                List<byte[]> byteArray = array.OfType<byte[]>().ToList();
                if (byteArray.Count != 0 && byteArray.Count >= end - start)
                {
                    AddMappingFrombfrange(result, startCode, byteArray);
                }
            }
            else if (nextToken is byte[] tokenBytes)
            {
                if (tokenBytes.Length > 0)
                {
                    if (tokenBytes.Length == 2 && start == 0 && end == 0xffff && tokenBytes[0] == 0 && tokenBytes[1] == 0)
                    {
                        for (int i = 0; i < 256; i++)
                        {
                            startCode[0] = (byte)i;
                            startCode[1] = 0;
                            tokenBytes[0] = (byte)i;
                            tokenBytes[1] = 0;
                            AddMappingFrombfrange(result, startCode, 256, tokenBytes);
                        }
                    }
                    else
                    {
                        AddMappingFrombfrange(result, startCode, end - start + 1, tokenBytes);
                    }
                }
            }
        }
    }

    private static void AddMappingFrombfrange(CMap cmap, byte[] startCode, List<byte[]> tokenBytesList)
    {
        foreach (byte[] tokenBytes in tokenBytesList)
        {
            string value = CreateStringFromBytes(tokenBytes);
            cmap.AddCharMapping(startCode, value);
            Increment(startCode, startCode.Length - 1, useStrictMode: false);
        }
    }

    private void AddMappingFrombfrange(CMap cmap, byte[] startCode, int values, byte[] tokenBytes)
    {
        for (int i = 0; i < values; i++)
        {
            string value = CreateStringFromBytes(tokenBytes);
            cmap.AddCharMapping(startCode, value);
            if (!Increment(tokenBytes, tokenBytes.Length - 1, _strictMode))
            {
                break;
            }

            Increment(startCode, startCode.Length - 1, useStrictMode: false);
        }
    }

    private RandomAccessRead GetExternalCMap(string name)
    {
        Stream? stream = GetType().Assembly.GetManifestResourceStream(name)
            ?? GetType().Assembly.GetManifestResourceStream($"{typeof(CMapParser).Namespace}.{name}");

        if (stream is null)
        {
            throw new IOException($"Error: Could not find referenced cmap stream {name}");
        }

        return RandomAccessReadBuffer.CreateBufferFromStream(stream);
    }

    private object? ParseNextToken(RandomAccessRead randomAccessRead)
    {
        int nextByte = randomAccessRead.Read();

        while (nextByte is 0x09 or 0x20 or 0x0D or 0x0A)
        {
            nextByte = randomAccessRead.Read();
        }

        return nextByte switch
        {
            '%' => ReadLine(randomAccessRead, nextByte),
            '(' => ReadString(randomAccessRead),
            '>' => randomAccessRead.Read() == '>'
                ? MarkEndOfDictionary
                : throw new IOException("Error: expected the end of a dictionary."),
            ']' => MarkEndOfArray,
            '[' => ReadArray(randomAccessRead),
            '<' => ReadDictionary(randomAccessRead),
            '/' => ReadLiteralName(randomAccessRead),
            -1 => null,
            >= '0' and <= '9' => ReadNumber(randomAccessRead, nextByte),
            _ => ReadOperator(randomAccessRead, nextByte),
        };
    }

    private int ParseInteger(RandomAccessRead randomAccessRead)
    {
        object? nextToken = ParseNextToken(randomAccessRead);
        if (nextToken is null)
        {
            throw new IOException("expected integer value is missing");
        }

        if (nextToken is int intValue)
        {
            return intValue;
        }

        throw new IOException("invalid type for next token");
    }

    private byte[] ParseByteArray(RandomAccessRead randomAccessRead)
    {
        object? nextToken = ParseNextToken(randomAccessRead);
        if (nextToken is null)
        {
            throw new IOException("expected byte[] value is missing");
        }

        if (nextToken is byte[] bytes)
        {
            return bytes;
        }

        throw new IOException("invalid type for next token");
    }

    private List<object> ReadArray(RandomAccessRead randomAccessRead)
    {
        List<object> list = [];
        object? nextToken = ParseNextToken(randomAccessRead);
        while (nextToken is not null && !MarkEndOfArray.Equals(nextToken))
        {
            list.Add(nextToken);
            nextToken = ParseNextToken(randomAccessRead);
        }

        return list;
    }

    private static string ReadString(RandomAccessRead randomAccessRead)
    {
        StringBuilder buffer = new();
        int stringByte = randomAccessRead.Read();
        while (stringByte != -1 && stringByte != ')')
        {
            buffer.Append((char)stringByte);
            stringByte = randomAccessRead.Read();
        }

        return buffer.ToString();
    }

    private static string ReadLine(RandomAccessRead randomAccessRead, int firstByte)
    {
        StringBuilder buffer = new();
        buffer.Append((char)firstByte);
        ReadUntilEndOfLine(randomAccessRead, buffer);
        return buffer.ToString();
    }

    private static LiteralName ReadLiteralName(RandomAccessRead randomAccessRead)
    {
        StringBuilder buffer = new();
        int stringByte = randomAccessRead.Read();

        while (!IsWhitespaceOrEOF(stringByte) && !IsDelimiter(stringByte))
        {
            buffer.Append((char)stringByte);
            stringByte = randomAccessRead.Read();
        }

        if (IsDelimiter(stringByte))
        {
            randomAccessRead.Rewind(1);
        }

        return new LiteralName(buffer.ToString());
    }

    private static Operator ReadOperator(RandomAccessRead randomAccessRead, int firstByte)
    {
        int nextByte = firstByte;
        StringBuilder buffer = new();
        buffer.Append((char)nextByte);
        nextByte = randomAccessRead.Read();

        while (!IsWhitespaceOrEOF(nextByte) && !IsDelimiter(nextByte) && !char.IsDigit((char)nextByte))
        {
            buffer.Append((char)nextByte);
            nextByte = randomAccessRead.Read();
        }

        if (IsDelimiter(nextByte) || char.IsDigit((char)nextByte))
        {
            randomAccessRead.Rewind(1);
        }

        return new Operator(buffer.ToString());
    }

    private static object ReadNumber(RandomAccessRead randomAccessRead, int firstByte)
    {
        int nextByte = firstByte;
        StringBuilder buffer = new();
        buffer.Append((char)nextByte);
        nextByte = randomAccessRead.Read();

        while (!IsWhitespaceOrEOF(nextByte) && (char.IsDigit((char)nextByte) || nextByte == '.'))
        {
            buffer.Append((char)nextByte);
            nextByte = randomAccessRead.Read();
        }

        if (nextByte != -1)
        {
            randomAccessRead.Rewind(1);
        }

        string value = buffer.ToString();
        if (value.Contains('.', StringComparison.Ordinal))
        {
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double doubleValue))
            {
                return doubleValue;
            }
        }
        else
        {
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue))
            {
                return intValue;
            }
        }

        throw new IOException($"Invalid number '{value}'");
    }

    private object ReadDictionary(RandomAccessRead randomAccessRead)
    {
        int theNextByte = randomAccessRead.Read();
        if (theNextByte == '<')
        {
            Dictionary<string, object?> result = [];
            object? key = ParseNextToken(randomAccessRead);
            while (key is LiteralName literalName && MarkEndOfDictionary != literalName.Name)
            {
                object? value = ParseNextToken(randomAccessRead);
                result[literalName.Name] = value;
                key = ParseNextToken(randomAccessRead);
            }

            return result;
        }

        int multipler = 16;
        int bufferIndex = -1;
        while (theNextByte != -1 && theNextByte != '>')
        {
            if (IsWhitespaceOrEOF(theNextByte))
            {
                theNextByte = randomAccessRead.Read();
                continue;
            }

            int intValue;
            if (theNextByte is >= '0' and <= '9')
            {
                intValue = theNextByte - '0';
            }
            else if (theNextByte is >= 'A' and <= 'F')
            {
                intValue = 10 + theNextByte - 'A';
            }
            else if (theNextByte is >= 'a' and <= 'f')
            {
                intValue = 10 + theNextByte - 'a';
            }
            else
            {
                throw new IOException($"Error: expected hex character and not {(char)theNextByte}:{theNextByte}");
            }

            intValue *= multipler;
            if (multipler == 16)
            {
                bufferIndex++;
                if (bufferIndex >= _tokenParserByteBuffer.Length)
                {
                    throw new IOException($"cmap token is larger than buffer size {_tokenParserByteBuffer.Length}");
                }

                _tokenParserByteBuffer[bufferIndex] = 0;
                multipler = 1;
            }
            else
            {
                multipler = 16;
            }

            _tokenParserByteBuffer[bufferIndex] += (byte)intValue;
            theNextByte = randomAccessRead.Read();
        }

        byte[] finalResult = new byte[bufferIndex + 1];
        Array.Copy(_tokenParserByteBuffer, 0, finalResult, 0, bufferIndex + 1);
        return finalResult;
    }

    private static void ReadUntilEndOfLine(RandomAccessRead randomAccessRead, StringBuilder buffer)
    {
        int nextByte = randomAccessRead.Read();
        while (nextByte != -1 && nextByte != 0x0D && nextByte != 0x0A)
        {
            buffer.Append((char)nextByte);
            nextByte = randomAccessRead.Read();
        }
    }

    private static bool IsWhitespaceOrEOF(int aByte)
    {
        return aByte is -1 or 0x20 or 0x0D or 0x0A;
    }

    private static bool IsDelimiter(int aByte)
    {
        return aByte is '(' or ')' or '<' or '>' or '[' or ']' or '{' or '}' or '/' or '%';
    }

    private static bool Increment(byte[] data, int position, bool useStrictMode)
    {
        if (position > 0 && (data[position] & 0xFF) == 255)
        {
            if (useStrictMode)
            {
                return false;
            }

            data[position] = 0;
            Increment(data, position - 1, useStrictMode);
        }
        else
        {
            data[position] = (byte)(data[position] + 1);
        }

        return true;
    }

    private static string CreateStringFromBytes(byte[] bytes)
    {
        if (bytes.Length <= 2)
        {
            return CMapStrings.GetMapping(bytes) ?? string.Empty;
        }

        return System.Text.Encoding.BigEndianUnicode.GetString(bytes);
    }

    private sealed class LiteralName(string name)
    {
        public string Name { get; } = name;
    }

    private sealed class Operator(string op)
    {
        public string Op { get; } = op;
    }
}
