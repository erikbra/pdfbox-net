/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Adapted low-level parser bridge for chunk-2 token/object flow.
 * Based on Apache PDFBox parser token semantics with the minimal scope
 * required for COS object roundtrip parsing in this repository stage.
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
using PdfBox.Net.COS;

namespace PdfBox.Net.PdfParser;

public sealed class COSParser
{
    private const int NoLookAhead = -2;
    private readonly Stream _input;
    private int _lookAhead = NoLookAhead;

    public COSParser(Stream input)
    {
        _input = input ?? throw new ArgumentNullException(nameof(input));
    }

    public static COSBase Parse(Stream input)
    {
        return new COSParser(input).ParseObject();
    }

    public static COSBase Parse(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Parse(new MemoryStream(Encoding.Latin1.GetBytes(input)));
    }

    public COSBase ParseObject()
    {
        SkipSpacesAndComments();
        COSBase value = ReadObject();
        SkipSpacesAndComments();
        if (PeekByte() != -1)
        {
            throw new IOException("Unexpected trailing content after COS object.");
        }

        return value;
    }

    private COSBase ReadObject()
    {
        int first = ReadByte();
        if (first == -1)
        {
            throw new EndOfStreamException("Unexpected EOF while reading COS object.");
        }

        switch ((char)first)
        {
            case '<':
            {
                int next = ReadByte();
                if (next == '<')
                {
                    return ReadDictionary();
                }

                UnreadByte(next);
                return ReadHexString();
            }

            case '[':
                return ReadArray();
            case '/':
                return ReadName();
            case '(':
                return ReadLiteralString();
            default:
                UnreadByte(first);
                return ReadAtomicToken();
        }
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
            COSBase keyToken = ReadObject();
            if (keyToken is not COSName key)
            {
                throw new IOException("Dictionary key must be a name.");
            }

            SkipSpacesAndComments();
            COSBase value = ReadObject();
            dict.SetItem(key, value);
        }

        return dict;
    }

    private COSBase ReadArray()
    {
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
            array.Add(ReadObject());
        }

        return array;
    }

    private COSBase ReadName()
    {
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
        List<byte> bytes = [];
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
                            ReadByte();
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

    private COSBase ReadAtomicToken()
    {
        string token = ReadToken();
        if (token.Length == 0)
        {
            throw new IOException("Unexpected empty token.");
        }

        return token switch
        {
            "true" => COSBoolean.TRUE,
            "false" => COSBoolean.FALSE,
            "null" => COSNull.NULL,
            _ => COSNumber.Get(token)
        };
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
