/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/type1/Type1Lexer.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
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
using System.Text.RegularExpressions;

namespace PdfBox.Net.FontBox.Type1;

public sealed class Type1Lexer
{
    private static readonly Regex IntegerPattern = new(@"^[+-]?\d+$", RegexOptions.Compiled);
    private static readonly Regex RadixPattern = new(@"^([+-]?)(\d+)#([0-9A-Za-z]+)$", RegexOptions.Compiled);
    private static readonly Regex RealPattern = new(@"^[+-]?(?:\d+\.\d*|\d*\.\d+|\d+[eE][+-]?\d+|\d+\.\d*[eE][+-]?\d+|\d*\.\d+[eE][+-]?\d+|\d+e[+-]?\d+|\d+E[+-]?\d+)$", RegexOptions.Compiled);

    private readonly byte[] _input;
    private int _position;
    private Token? _ahead;
    private Token? _previous;

    public Type1Lexer(byte[] input)
    {
        _input = input;
    }

    public Token? NextToken()
    {
        if (_ahead is not null)
        {
            Token token = _ahead;
            _ahead = null;
            _previous = token;
            return token;
        }

        Token? next = ReadToken(_previous);
        _previous = next;
        return next;
    }

    public Token? PeekToken()
    {
        _ahead ??= ReadToken(_previous);
        return _ahead;
    }

    public bool PeekKind(Token.Kind kind)
    {
        return PeekToken()?.GetKind() == kind;
    }

    private Token? ReadToken(Token? previousToken)
    {
        while (_position < _input.Length)
        {
            char c = (char)_input[_position++];
            switch (c)
            {
                case ' ':
                case '\t':
                case '\n':
                case '\r':
                case '\f':
                case '\0':
                    continue;
                case '%':
                    ReadComment();
                    continue;
                case '[':
                    return new Token(c, Token.START_ARRAY);
                case ']':
                    return new Token(c, Token.END_ARRAY);
                case '{':
                    return new Token(c, Token.START_PROC);
                case '}':
                    return new Token(c, Token.END_PROC);
                case '<':
                    if (_position < _input.Length && _input[_position] == '>')
                    {
                        _position++;
                        return new Token("<>", Token.NAME);
                    }

                    if (_position < _input.Length && _input[_position] == '<')
                    {
                        _position++;
                        return new Token("<<", Token.START_DICT);
                    }

                    return new Token(ReadRegular(c), Token.NAME);
                case '>':
                    if (_position < _input.Length && _input[_position] == '>')
                    {
                        _position++;
                        return new Token(">>", Token.END_DICT);
                    }

                    return new Token(ReadRegular(c), Token.NAME);
                case '(':
                    return ReadString();
                case '/':
                {
                    int namePosition = _position;
                    string literal = ReadRegular('\0');
                    if (literal.Length == 0)
                    {
                        throw new DamagedFontException($"Could not read token at position {namePosition}");
                    }

                    return new Token(literal, Token.LITERAL);
                }
                default:
                {
                    string regular = ReadRegular(c);
                    if (regular.Length == 0)
                    {
                        continue;
                    }

                    if (TryReadNumber(regular, out Token? number))
                    {
                        return number;
                    }

                    if (previousToken?.GetKind() == Token.INTEGER && (regular == "RD" || regular == "-|"))
                    {
                        return ReadCharString(previousToken.IntValue());
                    }

                    return new Token(regular, Token.NAME);
                }
            }
        }

        return null;
    }

    private string ReadRegular(char first)
    {
        StringBuilder builder = new();
        if (first != '\0')
        {
            builder.Append(first);
        }

        while (_position < _input.Length)
        {
            char c = (char)_input[_position];
            if (char.IsWhiteSpace(c) || c is '[' or ']' or '{' or '}' or '(' or ')' or '<' or '>' or '/' or '%')
            {
                break;
            }

            _position++;
            builder.Append(c);
        }

        return builder.ToString();
    }

    private void ReadComment()
    {
        while (_position < _input.Length)
        {
            char c = (char)_input[_position++];
            if (c == '\n' || c == '\r')
            {
                break;
            }
        }
    }

    private Token ReadString()
    {
        StringBuilder builder = new();
        int nesting = 1;

        while (_position < _input.Length)
        {
            char c = (char)_input[_position++];
            if (c == '\\')
            {
                if (_position >= _input.Length)
                {
                    break;
                }

                char escaped = (char)_input[_position++];
                switch (escaped)
                {
                    case 'n': builder.Append('\n'); break;
                    case 'r': builder.Append('\n'); break;
                    case 't': builder.Append('\t'); break;
                    case 'b': builder.Append('\b'); break;
                    case 'f': builder.Append('\f'); break;
                    case '\\': builder.Append('\\'); break;
                    case '(':
                    case ')': builder.Append(escaped); break;
                    case '\n':
                    case '\r':
                        break;
                    default:
                        if (escaped is >= '0' and <= '7')
                        {
                            StringBuilder octal = new();
                            octal.Append(escaped);
                            for (int i = 0; i < 2 && _position < _input.Length; i++)
                            {
                                char octalChar = (char)_input[_position];
                                if (octalChar is < '0' or > '7')
                                {
                                    break;
                                }

                                _position++;
                                octal.Append(octalChar);
                            }

                            builder.Append((char)Convert.ToInt32(octal.ToString(), 8));
                        }
                        else
                        {
                            builder.Append(escaped);
                        }
                        break;
                }

                continue;
            }

            if (c == '(')
            {
                nesting++;
                builder.Append(c);
                continue;
            }

            if (c == ')')
            {
                nesting--;
                if (nesting == 0)
                {
                    return new Token(builder.ToString(), Token.STRING);
                }

                builder.Append(c);
                continue;
            }

            builder.Append(c);
        }

        throw new DamagedFontException("Unterminated string literal");
    }

    private Token ReadCharString(int length)
    {
        if (_position < _input.Length && char.IsWhiteSpace((char)_input[_position]))
        {
            _position++;
        }

        if (length < 0 || _position + length > _input.Length)
        {
            throw new IOException($"String length {length} is larger than input");
        }

        byte[] data = new byte[length];
        Array.Copy(_input, _position, data, 0, length);
        _position += length;
        return new Token(data, Token.CHARSTRING);
    }

    private static bool TryReadNumber(string text, out Token? token)
    {
        Match radix = RadixPattern.Match(text);
        if (radix.Success)
        {
            int sign = radix.Groups[1].Value == "-" ? -1 : 1;
            int @base = int.Parse(radix.Groups[2].Value, CultureInfo.InvariantCulture);
            int value = Convert.ToInt32(radix.Groups[3].Value, @base) * sign;
            token = new Token(value.ToString(CultureInfo.InvariantCulture), Token.INTEGER);
            return true;
        }

        if (IntegerPattern.IsMatch(text))
        {
            token = new Token(text, Token.INTEGER);
            return true;
        }

        if (RealPattern.IsMatch(text))
        {
            token = new Token(text, Token.REAL);
            return true;
        }

        token = null;
        return false;
    }
}
