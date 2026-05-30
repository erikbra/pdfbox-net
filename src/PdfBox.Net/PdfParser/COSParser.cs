/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Adapted low-level parser bridge for chunk-2 token/object flow.
 * Based on Apache PDFBox parser token semantics with the minimal scope
 * required for COS object roundtrip parsing in this repository stage.
 * Also incorporates the BaseParser utility primitives (SkipLinebreak,
 * IsEndOfName, ReadExpectedChar, ReadExpectedString, IsEOF, IsEOL, IsLF,
 * IsCR, IsSpace, IsDigit, ReadInt, ReadLong, ReadStringNumber) to satisfy
 * the BaseParser.java source-to-target mapping for this adapted port.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdfparser/COSParser.java
 * PDFBOX_SOURCE_COMMIT: a71c5679d69bc3fd3ab15e248b69441ee91dca6c
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: a71c5679d69bc3fd3ab15e248b69441ee91dca6c
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

    // ASCII character constants (from BaseParser.java)
    private const byte AsciiNull = 0;
    private const byte AsciiTab = 9;
    private const byte AsciiLf = 10;
    private const byte AsciiCr = 13;
    private const byte AsciiSpace = 32;
    private const byte AsciiZero = 48;
    private const byte AsciiNine = 57;

    private static readonly int MaxLengthLong = long.MaxValue.ToString().Length;

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
        return value is AsciiNull or AsciiTab or AsciiLf or 12 or AsciiCr or AsciiSpace;
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

    // ── BaseParser utility primitives ─────────────────────────────────────────
    // The following methods are ported from BaseParser.java to complete the
    // source-to-target mapping for the pdfbox/src/main/java/…/pdfparser/BaseParser.java
    // upstream path in this adapted port.

    /// <summary>
    /// Determine if a character terminates a PDF name.
    /// </summary>
    /// <param name="ch">The character.</param>
    /// <returns><c>true</c> if the character terminates a PDF name.</returns>
    public static bool IsEndOfName(int ch)
    {
        return ch is AsciiSpace or AsciiCr or AsciiLf or AsciiTab
            or '>' or '<' or '[' or '/' or ']' or ')' or '(' or AsciiNull or '\f' or '%' or -1;
    }

    /// <summary>
    /// Tells if the next byte to be read is an end of line byte (LF or CR).
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <returns><c>true</c> if the byte is 0x0A or 0x0D.</returns>
    public static bool IsEOL(int c) => IsLF(c) || IsCR(c);

    /// <summary>
    /// Tells if the given byte is a line feed (0x0A).
    /// </summary>
    public static bool IsLF(int c) => c == AsciiLf;

    /// <summary>
    /// Tells if the given byte is a carriage return (0x0D).
    /// </summary>
    public static bool IsCR(int c) => c == AsciiCr;

    /// <summary>
    /// Tells if the given byte is a space character (0x20).
    /// </summary>
    public static bool IsSpace(int c) => c == AsciiSpace;

    /// <summary>
    /// Tells if the given byte is a decimal digit.
    /// </summary>
    public static bool IsDigit(int c) => c >= AsciiZero && c <= AsciiNine;

    /// <summary>
    /// Tells whether the parser is at end-of-stream.
    /// </summary>
    public bool IsEOF() => PeekByte() == -1;

    /// <summary>
    /// Skip one line break (CR, LF, or CRLF). Returns <c>true</c> if a line break was consumed.
    /// </summary>
    public bool SkipLinebreak()
    {
        int c = ReadByte();
        if (IsCR(c))
        {
            if (!IsLF(PeekByte()))
            {
                return true;
            }

            ReadByte(); // consume the LF of a CRLF pair
            return true;
        }

        if (IsLF(c))
        {
            return true;
        }

        UnreadByte(c);
        return false;
    }

    /// <summary>
    /// Skip the upcoming CRLF or LF which are supposed to follow a stream keyword.
    /// Trailing spaces are removed as well (from BaseParser.skipWhiteSpaces).
    /// </summary>
    public void SkipWhiteSpaces()
    {
        int whitespace = ReadByte();
        while (IsSpace(whitespace))
        {
            whitespace = ReadByte();
        }

        if (!SkipLinebreakValue(whitespace))
        {
            UnreadByte(whitespace);
        }
    }

    private bool SkipLinebreakValue(int linebreak)
    {
        if (IsCR(linebreak))
        {
            int next = ReadByte();
            if (!IsLF(next))
            {
                UnreadByte(next);
            }

            return true;
        }

        return IsLF(linebreak);
    }

    /// <summary>
    /// Read one char and throw an <see cref="IOException"/> if it is not the expected value.
    /// </summary>
    /// <param name="ec">The expected character.</param>
    public void ReadExpectedChar(char ec)
    {
        int c = ReadByte();
        if (c != ec)
        {
            throw new IOException(
                $"expected='{ec}' actual='{(char)c}' at current stream position");
        }
    }

    /// <summary>
    /// Reads the given pattern from the stream, optionally skipping surrounding whitespace.
    /// </summary>
    /// <param name="expectedString">The pattern to be matched.</param>
    /// <param name="skipSpaces">If <c>true</c>, spaces before and after the pattern are skipped.</param>
    public void ReadExpectedString(char[] expectedString, bool skipSpaces)
    {
        if (skipSpaces)
        {
            SkipSpacesAndComments();
        }

        foreach (char c in expectedString)
        {
            int read = ReadByte();
            if (read != c)
            {
                throw new IOException(
                    $"Expected string '{new string(expectedString)}' but missed at character '{c}'");
            }
        }

        if (skipSpaces)
        {
            SkipSpacesAndComments();
        }
    }

    /// <summary>
    /// Reads a digit-only sequence used by <see cref="ReadInt"/> and <see cref="ReadLong"/>.
    /// </summary>
    public StringBuilder ReadStringNumber()
    {
        StringBuilder buffer = new();
        int lastByte;
        while (IsDigit(lastByte = ReadByte()))
        {
            buffer.Append((char)lastByte);
            if (buffer.Length > MaxLengthLong)
            {
                throw new IOException($"Number '{buffer}' is getting too long, stop reading.");
            }
        }

        if (lastByte != -1)
        {
            UnreadByte(lastByte);
        }

        return buffer;
    }

    /// <summary>
    /// Reads an integer from the stream, skipping leading spaces.
    /// </summary>
    public int ReadInt()
    {
        SkipSpacesAndComments();
        StringBuilder intBuffer = ReadStringNumber();
        if (!int.TryParse(intBuffer.ToString(), out int result))
        {
            throw new IOException($"Error: Expected an integer type, instead got '{intBuffer}'");
        }

        return result;
    }

    /// <summary>
    /// Reads a long from the stream, skipping leading spaces.
    /// </summary>
    public long ReadLong()
    {
        SkipSpacesAndComments();
        StringBuilder longBuffer = ReadStringNumber();
        if (!long.TryParse(longBuffer.ToString(), out long result))
        {
            throw new IOException($"Error: Expected a long type, instead got '{longBuffer}'");
        }

        return result;
    }
}
