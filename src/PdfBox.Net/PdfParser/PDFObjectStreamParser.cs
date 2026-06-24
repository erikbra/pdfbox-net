/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdfparser/PDFObjectStreamParser.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
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

using System.Globalization;
using System.Text;
using PdfBox.Net.COS;

namespace PdfBox.Net.PdfParser;

public sealed class PDFObjectStreamParser
{
    private readonly COSStream _objectStream;
    private readonly Dictionary<COSObjectKey, COSObject> _objectPool;

    public PDFObjectStreamParser(COSStream objectStream, Dictionary<COSObjectKey, COSObject> objectPool)
    {
        _objectStream = objectStream ?? throw new ArgumentNullException(nameof(objectStream));
        _objectPool = objectPool ?? throw new ArgumentNullException(nameof(objectPool));
    }

    public IReadOnlyList<(COSObjectKey Key, COSBase Value)> Parse()
    {
        int objectCount = _objectStream.GetInt(COSName.GetPDFName("N"), -1);
        int firstObjectOffset = _objectStream.GetInt(COSName.GetPDFName("First"), -1);
        if (objectCount < 0 || firstObjectOffset < 0)
        {
            throw new IOException("Object stream is missing required /N or /First entries.");
        }

        byte[] decoded = ReadDecodedObjectStream();
        SyntaxReader reader = new(decoded, 0);
        List<(long objectNumber, int objectOffset)> indexEntries = [];
        for (int i = 0; i < objectCount; i++)
        {
            string objectNumberToken = reader.ReadToken();
            string objectOffsetToken = reader.ReadToken();
            if (!long.TryParse(objectNumberToken, NumberStyles.Integer, CultureInfo.InvariantCulture, out long objectNumber) ||
                !int.TryParse(objectOffsetToken, NumberStyles.Integer, CultureInfo.InvariantCulture, out int objectOffset))
            {
                throw new IOException("Malformed object stream index table.");
            }

            indexEntries.Add((objectNumber, objectOffset));
        }

        List<(COSObjectKey Key, COSBase Value)> parsedObjects = [];
        foreach ((long objectNumber, int objectOffset) in indexEntries)
        {
            int absoluteObjectOffset = checked(firstObjectOffset + objectOffset);
            if (absoluteObjectOffset < 0 || absoluteObjectOffset >= decoded.Length)
            {
                throw new IOException("Object stream object offset is outside decoded stream bounds.");
            }

            SyntaxReader objectReader = new(decoded, absoluteObjectOffset);
            COSBase value = ParseObject(objectReader);
            COSObjectKey key = new(objectNumber, 0);
            SetObjectKeyIfStable(value, key);
            parsedObjects.Add((key, value));
        }

        return parsedObjects;
    }

    private byte[] ReadDecodedObjectStream()
    {
        using Stream input = _objectStream.CreateInputStream();
        using MemoryStream output = new();
        input.CopyTo(output);
        return output.ToArray();
    }

    private COSBase ParseObject(SyntaxReader reader)
    {
        reader.SkipSpacesAndComments();
        int first = reader.ReadByte();
        if (first == -1)
        {
            throw new EndOfStreamException("Unexpected EOF while parsing object stream entry.");
        }

        switch ((char)first)
        {
            case '<':
            {
                int next = reader.ReadByte();
                if (next == '<')
                {
                    return ParseDictionary(reader);
                }

                reader.UnreadByte(next);
                return ParseHexString(reader);
            }
            case '[':
                return ParseArray(reader);
            case '/':
                return ParseName(reader);
            case '(':
                return ParseLiteralString(reader);
            default:
                return ParseAtomicToken(reader, first);
        }
    }

    private COSBase ParseDictionary(SyntaxReader reader)
    {
        COSDictionary dictionary = new();
        while (true)
        {
            reader.SkipSpacesAndComments();
            int b = reader.ReadByte();
            if (b == '>')
            {
                if (reader.ReadByte() != '>')
                {
                    throw new IOException("Malformed dictionary close marker.");
                }

                break;
            }

            reader.UnreadByte(b);
            COSBase keyObject = ParseObject(reader);
            if (keyObject is not COSName key)
            {
                throw new IOException("Dictionary key must be a COS name.");
            }

            COSBase valueObject = ParseObject(reader);
            dictionary.SetItem(key, valueObject);
        }

        return dictionary;
    }

    private COSBase ParseArray(SyntaxReader reader)
    {
        COSArray array = new();
        while (true)
        {
            reader.SkipSpacesAndComments();
            int b = reader.ReadByte();
            if (b == ']')
            {
                break;
            }

            if (b == -1)
            {
                throw new EndOfStreamException("Unexpected EOF while parsing array.");
            }

            reader.UnreadByte(b);
            array.Add(ParseObject(reader));
        }

        return array;
    }

    private COSBase ParseName(SyntaxReader reader)
    {
        StringBuilder builder = new();
        while (true)
        {
            int b = reader.ReadByte();
            if (b == -1 || IsWhiteSpace(b) || IsDelimiter(b))
            {
                reader.UnreadByte(b);
                break;
            }

            if (b == '#')
            {
                int high = reader.ReadByte();
                int low = reader.ReadByte();
                if (high == -1 || low == -1)
                {
                    throw new EndOfStreamException("Unexpected EOF in name escape.");
                }

                int decoded = ParseHex(high) * 16 + ParseHex(low);
                if (decoded < 0)
                {
                    throw new IOException("Invalid hex escape in name token.");
                }

                builder.Append((char)decoded);
                continue;
            }

            builder.Append((char)b);
        }

        return COSName.GetPDFName(builder.ToString());
    }

    private COSBase ParseLiteralString(SyntaxReader reader)
    {
        List<byte> bytes = [];
        int nesting = 1;
        while (nesting > 0)
        {
            int b = reader.ReadByte();
            if (b == -1)
            {
                throw new EndOfStreamException("Unexpected EOF in literal string.");
            }

            if (b == '\\')
            {
                int escaped = reader.ReadByte();
                if (escaped == -1)
                {
                    throw new EndOfStreamException("Unexpected EOF in string escape.");
                }

                if (escaped is >= '0' and <= '7')
                {
                    int octal = escaped - '0';
                    for (int i = 0; i < 2; i++)
                    {
                        int next = reader.PeekByte();
                        if (next is >= '0' and <= '7')
                        {
                            octal = (octal * 8) + (reader.ReadByte() - '0');
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
                    case 'n': bytes.Add((byte)'\n'); break;
                    case 'r': bytes.Add((byte)'\r'); break;
                    case 't': bytes.Add((byte)'\t'); break;
                    case 'b': bytes.Add((byte)'\b'); break;
                    case 'f': bytes.Add((byte)'\f'); break;
                    case '\r':
                        if (reader.PeekByte() == '\n')
                        {
                            _ = reader.ReadByte();
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

    private COSBase ParseHexString(SyntaxReader reader)
    {
        StringBuilder hex = new();
        while (true)
        {
            int b = reader.ReadByte();
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

    private COSBase ParseAtomicToken(SyntaxReader reader, int first)
    {
        string token = reader.ReadTokenStartingWith(first);
        if (token.Length == 0)
        {
            throw new IOException("Unexpected empty token.");
        }

        return token switch
        {
            "true" => COSBoolean.TRUE,
            "false" => COSBoolean.FALSE,
            "null" => COSNull.NULL,
            _ => ParseNumberOrReference(reader, token)
        };
    }

    private COSBase ParseNumberOrReference(SyntaxReader reader, string numberToken)
    {
        if (!long.TryParse(numberToken, NumberStyles.Integer, CultureInfo.InvariantCulture, out long objectNumber) || objectNumber < 0)
        {
            return COSNumber.Get(numberToken);
        }

        int rollbackPosition = reader.Position;
        reader.SkipSpacesAndComments();
        if (reader.TryReadIntegerToken(out int generation) && generation >= 0)
        {
            reader.SkipSpacesAndComments();
            if (reader.TryReadKeyword("R"))
            {
                COSObjectKey key = new(objectNumber, generation);
                if (_objectPool.TryGetValue(key, out COSObject? existing))
                {
                    return existing;
                }

                COSObject created = new(key);
                _objectPool[key] = created;
                return created;
            }
        }

        reader.Position = rollbackPosition;
        return COSNumber.Get(numberToken);
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

    private static void SetObjectKeyIfStable(COSBase value, COSObjectKey key)
    {
        if (value is COSDictionary or COSArray or COSStream or COSString or COSFloat)
        {
            value.SetKey(key);
        }
    }

    private sealed class SyntaxReader(byte[] data, int position)
    {
        private const int NoLookAhead = -2;
        private int _lookAhead = NoLookAhead;

        public int Position
        {
            get => position;
            set
            {
                position = value;
                _lookAhead = NoLookAhead;
            }
        }

        public int PeekByte()
        {
            if (_lookAhead == NoLookAhead)
            {
                _lookAhead = position >= data.Length ? -1 : data[position];
            }

            return _lookAhead;
        }

        public int ReadByte()
        {
            if (_lookAhead != NoLookAhead)
            {
                int value = _lookAhead;
                _lookAhead = NoLookAhead;
                if (value != -1)
                {
                    position++;
                }

                return value;
            }

            if (position >= data.Length)
            {
                return -1;
            }

            return data[position++];
        }

        public void UnreadByte(int value)
        {
            if (value == -1)
            {
                return;
            }

            if (_lookAhead != NoLookAhead)
            {
                throw new InvalidOperationException("Reader lookahead buffer already occupied.");
            }

            _lookAhead = value;
            position--;
        }

        public void SkipSpacesAndComments()
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

        public string ReadToken()
        {
            SkipSpacesAndComments();
            int b = ReadByte();
            if (b == -1)
            {
                return string.Empty;
            }

            return ReadTokenStartingWith(b);
        }

        public string ReadTokenStartingWith(int first)
        {
            StringBuilder token = new();
            token.Append((char)first);
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

        public bool TryReadKeyword(string expected)
        {
            int rollback = Position;
            string token = ReadToken();
            if (token.Equals(expected, StringComparison.Ordinal))
            {
                return true;
            }

            Position = rollback;
            return false;
        }

        public bool TryReadIntegerToken(out int value)
        {
            int rollback = Position;
            string token = ReadToken();
            if (int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }

            Position = rollback;
            return false;
        }
    }
}
