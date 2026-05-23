/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdfparser/PDFStreamParser.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
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

using System.Text;
using PdfBox.Net.ContentStream.Operator;
using PdfBox.Net.COS;

namespace PdfBox.Net.PdfParser;

/// <summary>
/// Parses low-level content stream tokens into COS operands and operators.
/// </summary>
public sealed class PDFStreamParser
{
    private const int NoLookAhead = -2;
    private readonly Stream _input;
    private int _lookAhead = NoLookAhead;

    public PDFStreamParser(Stream input)
    {
        _input = input ?? throw new ArgumentNullException(nameof(input));
    }

    public static IList<object> Parse(Stream input)
    {
        return new PDFStreamParser(input).ParseTokens();
    }

    public IList<object> ParseTokens()
    {
        List<object> tokens = new();
        while (true)
        {
            SkipSpacesAndComments();
            if (PeekByte() == -1)
            {
                break;
            }

            tokens.Add(ReadTokenObject());
        }

        return tokens;
    }

    private object ReadTokenObject()
    {
        int first = PeekByte();
        return first switch
        {
            '<' => ReadDictionaryOrHexString(),
            '[' => ReadArray(),
            '/' => ReadName(),
            '(' => ReadLiteralString(),
            _ => ReadAtomicOrOperatorToken()
        };
    }

    private COSBase ReadDictionaryOrHexString()
    {
        _ = ReadByte();
        if (PeekByte() == '<')
        {
            _ = ReadByte();
            return ReadDictionary();
        }

        return ReadHexString();
    }

    private object ReadAtomicOrOperatorToken()
    {
        string token = ReadToken();
        if (token.Length == 0)
        {
            throw new IOException("Unexpected empty token.");
        }

        if (token.Equals(OperatorName.BEGIN_INLINE_IMAGE, StringComparison.Ordinal))
        {
            return ReadInlineImageOperator();
        }

        return token switch
        {
            "true" => COSBoolean.TRUE,
            "false" => COSBoolean.FALSE,
            "null" => COSNull.NULL,
            _ => TryReadNumberOrOperator(token)
        };
    }

    private object TryReadNumberOrOperator(string token)
    {
        try
        {
            return COSNumber.Get(token);
        }
        catch (IOException)
        {
            return Operator.GetOperator(token);
        }
    }

    private Operator ReadInlineImageOperator()
    {
        COSDictionary parameters = new();
        while (true)
        {
            SkipSpacesAndComments();
            if (PeekByte() == -1)
            {
                throw new IOException("Unexpected EOF while parsing inline image dictionary.");
            }

            object keyOrDataMarker = ReadTokenObject();
            if (keyOrDataMarker is Operator marker &&
                marker.GetName().Equals(OperatorName.BEGIN_INLINE_IMAGE_DATA, StringComparison.Ordinal))
            {
                break;
            }

            if (keyOrDataMarker is not COSName key)
            {
                throw new IOException($"Invalid inline image key token '{keyOrDataMarker}'.");
            }

            SkipSpacesAndComments();
            parameters.SetItem(key, ReadTokenObject() as COSBase ?? throw new IOException("Inline image dictionary value must be COSBase."));
        }

        ConsumeInlineImageDataSeparator();
        byte[] imageData = ReadInlineImageData();

        Operator op = Operator.GetOperator(OperatorName.BEGIN_INLINE_IMAGE);
        op.SetImageParameters(parameters);
        op.SetImageData(imageData);
        return op;
    }

    private void ConsumeInlineImageDataSeparator()
    {
        int separator = ReadByte();
        if (separator == '\r' && PeekByte() == '\n')
        {
            _ = ReadByte();
            return;
        }

        if (separator != -1 && !IsWhiteSpace(separator))
        {
            UnreadByte(separator);
        }
    }

    private byte[] ReadInlineImageData()
    {
        if (!_input.CanSeek)
        {
            using MemoryStream remainder = new();
            int b;
            while ((b = ReadByte()) != -1)
            {
                remainder.WriteByte((byte)b);
            }

            byte[] bytes = remainder.ToArray();
            if (!TryFindInlineImageEnd(bytes, out int nonSeekDataLength, out _))
            {
                throw new IOException("Missing inline image end marker (EI).");
            }

            return bytes[..nonSeekDataLength];
        }

        long start = _input.Position;
        long count = _input.Length - start;
        byte[] remainderBytes = new byte[count];
        _ = _input.Read(remainderBytes, 0, remainderBytes.Length);

        if (!TryFindInlineImageEnd(remainderBytes, out int dataLength, out int consumedLength))
        {
            throw new IOException("Missing inline image end marker (EI).");
        }

        _input.Position = start + dataLength + consumedLength;
        _lookAhead = NoLookAhead;

        return remainderBytes[..dataLength];
    }

    private static bool TryFindInlineImageEnd(byte[] bytes, out int dataLength, out int consumedLength)
    {
        for (int i = 0; i <= bytes.Length - 4; i++)
        {
            bool isEnd = (bytes[i] is (byte)'\n' or (byte)'\r') &&
                         bytes[i + 1] == (byte)'E' &&
                         bytes[i + 2] == (byte)'I' &&
                         (bytes[i + 3] is (byte)'\n' or (byte)'\r' or (byte)' ');
            if (isEnd)
            {
                dataLength = i;
                consumedLength = 4;
                return true;
            }
        }

        dataLength = 0;
        consumedLength = 0;
        return false;
    }

    private COSBase ReadDictionary()
    {
        COSDictionary dict = new();
        while (true)
        {
            SkipSpacesAndComments();
            int b = ReadByte();
            if (b == '>')
            {
                if (ReadByte() != '>')
                {
                    throw new IOException("Malformed dictionary close marker.");
                }

                break;
            }

            UnreadByte(b);
            COSBase keyToken = ReadTokenObject() as COSBase ?? throw new IOException("Dictionary key must be COSBase.");
            if (keyToken is not COSName key)
            {
                throw new IOException("Dictionary key must be a name.");
            }

            SkipSpacesAndComments();
            COSBase value = ReadTokenObject() as COSBase ?? throw new IOException("Dictionary value must be COSBase.");
            dict.SetItem(key, value);
        }

        return dict;
    }

    private COSBase ReadArray()
    {
        _ = ReadByte();
        COSArray array = new();
        while (true)
        {
            SkipSpacesAndComments();
            int b = ReadByte();
            if (b == ']')
            {
                break;
            }

            if (b == -1)
            {
                throw new EndOfStreamException("Unexpected EOF inside array.");
            }

            UnreadByte(b);
            array.Add(ReadTokenObject() as COSBase ?? throw new IOException("Array item must be COSBase."));
        }

        return array;
    }

    private COSBase ReadName()
    {
        _ = ReadByte();
        StringBuilder name = new();
        while (true)
        {
            int b = ReadByte();
            if (b == -1 || IsWhiteSpace(b) || IsDelimiter(b))
            {
                UnreadByte(b);
                break;
            }

            if (b == '#')
            {
                int high = ReadByte();
                int low = ReadByte();
                if (high == -1 || low == -1)
                {
                    throw new EndOfStreamException("Unexpected EOF in name escape.");
                }

                int value = ParseHex(high) * 16 + ParseHex(low);
                if (value < 0)
                {
                    throw new IOException("Invalid hex escape in name token.");
                }

                name.Append((char)value);
                continue;
            }

            name.Append((char)b);
        }

        return COSName.GetPDFName(name.ToString());
    }

    private COSBase ReadLiteralString()
    {
        _ = ReadByte();
        List<byte> bytes = new(capacity: 64);
        int nesting = 1;
        while (nesting > 0)
        {
            int b = ReadByte();
            if (b == -1)
            {
                throw new EndOfStreamException("Unexpected EOF in literal string.");
            }

            if (b == '\\')
            {
                int escaped = ReadByte();
                if (escaped == -1)
                {
                    throw new EndOfStreamException("Unexpected EOF in string escape.");
                }

                if (escaped is >= '0' and <= '7')
                {
                    int octal = escaped - '0';
                    for (int i = 0; i < 2; i++)
                    {
                        int next = PeekByte();
                        if (next is >= '0' and <= '7')
                        {
                            octal = (octal * 8) + (ReadByte() - '0');
                        }
                        else
                        {
                            break;
                        }
                    }

                    bytes.Add((byte)octal);
                    continue;
                }

                switch ((char)escaped)
                {
                    case 'n':
                        bytes.Add((byte)'\n');
                        break;
                    case 'r':
                        bytes.Add((byte)'\r');
                        break;
                    case 't':
                        bytes.Add((byte)'\t');
                        break;
                    case 'b':
                        bytes.Add((byte)'\b');
                        break;
                    case 'f':
                        bytes.Add((byte)'\f');
                        break;
                    case '\r':
                        if (PeekByte() == '\n')
                        {
                            _ = ReadByte();
                        }

                        break;
                    case '\n':
                        break;
                    default:
                        bytes.Add((byte)escaped);
                        break;
                }

                continue;
            }

            if (b == '(')
            {
                nesting++;
            }
            else if (b == ')')
            {
                nesting--;
                if (nesting == 0)
                {
                    break;
                }
            }

            bytes.Add((byte)b);
        }

        return new COSString([.. bytes]);
    }

    private COSBase ReadHexString()
    {
        StringBuilder hex = new();
        while (true)
        {
            int b = ReadByte();
            if (b == -1)
            {
                throw new EndOfStreamException("Unexpected EOF in hex string.");
            }

            if (b == '>')
            {
                break;
            }

            if (!IsWhiteSpace(b))
            {
                hex.Append((char)b);
            }
        }

        return COSString.ParseHex(hex.ToString());
    }

    private string ReadToken()
    {
        StringBuilder token = new();
        while (true)
        {
            int b = ReadByte();
            if (b == -1 || IsWhiteSpace(b) || IsDelimiter(b))
            {
                UnreadByte(b);
                break;
            }

            token.Append((char)b);
        }

        return token.ToString();
    }

    private void SkipSpacesAndComments()
    {
        while (true)
        {
            int b = ReadByte();
            if (b == -1)
            {
                return;
            }

            if (IsWhiteSpace(b))
            {
                continue;
            }

            if (b == '%')
            {
                while (true)
                {
                    int c = ReadByte();
                    if (c is -1 or '\n' or '\r')
                    {
                        break;
                    }
                }

                continue;
            }

            UnreadByte(b);
            return;
        }
    }

    private int PeekByte()
    {
        if (_lookAhead == NoLookAhead)
        {
            _lookAhead = _input.ReadByte();
        }

        return _lookAhead;
    }

    private int ReadByte()
    {
        if (_lookAhead != NoLookAhead)
        {
            int b = _lookAhead;
            _lookAhead = NoLookAhead;
            return b;
        }

        return _input.ReadByte();
    }

    private void UnreadByte(int value)
    {
        if (value == -1)
        {
            return;
        }

        if (_lookAhead != NoLookAhead)
        {
            throw new InvalidOperationException("Parser lookahead buffer already occupied.");
        }

        _lookAhead = value;
    }

    private static bool IsWhiteSpace(int value)
    {
        return value is 0 or 9 or 10 or 12 or 13 or 32;
    }

    private static bool IsDelimiter(int value)
    {
        return value is '(' or ')' or '<' or '>' or '[' or ']' or '{' or '}' or '/' or '%';
    }

    private static int ParseHex(int value)
    {
        return value switch
        {
            >= '0' and <= '9' => value - '0',
            >= 'A' and <= 'F' => value - 'A' + 10,
            >= 'a' and <= 'f' => value - 'a' + 10,
            _ => -1
        };
    }
}
