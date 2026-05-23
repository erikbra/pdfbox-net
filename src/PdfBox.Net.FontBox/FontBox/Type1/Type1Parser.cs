/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/type1/Type1Parser.java
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

using System.Globalization;
using System.Text;
using PdfBuiltInEncoding = PdfBox.Net.FontBox.Encoding.BuiltInEncoding;
using PdfStandardEncoding = PdfBox.Net.FontBox.Encoding.StandardEncoding;

namespace PdfBox.Net.FontBox.Type1;

public sealed class Type1Parser
{
    private const ushort Type1Key = 4330;
    private const ushort EexecKey = 55665;

    public Type1Font Parse(byte[] segment1, byte[] segment2)
    {
        Type1Font font = new();
        font.SetAsciiSegment(segment1);
        font.SetBinarySegment(segment2);
        ParseAscii(font, segment1);
        ParseBinary(font, segment2);
        return font;
    }

    private static void ParseAscii(Type1Font font, byte[] bytes)
    {
        Type1Lexer lexer = new(bytes);
        while (lexer.NextToken() is Token token)
        {
            if (token.GetKind() != Token.LITERAL)
            {
                continue;
            }

            string key = token.GetText();
            if (key == "FontInfo")
            {
                ParseFontInfo(font, lexer);
            }
            else if (key == "Encoding")
            {
                ParseEncoding(font, lexer);
            }
            else
            {
                List<Token> value = ReadValue(lexer);
                ApplySimpleValue(font, key, value);
            }
        }
    }

    private static void ParseFontInfo(Type1Font font, Type1Lexer lexer)
    {
        Token? token;
        while ((token = lexer.NextToken()) is not null)
        {
            if (token.GetKind() == Token.NAME && token.GetText() == "begin")
            {
                break;
            }
        }

        while ((token = lexer.NextToken()) is not null)
        {
            if (token.GetKind() == Token.NAME && token.GetText() == "end")
            {
                break;
            }

            if (token.GetKind() != Token.LITERAL)
            {
                continue;
            }

            string key = token.GetText();
            List<Token> value = ReadValue(lexer);
            ApplySimpleValue(font, key, value);
        }
    }

    private static void ParseEncoding(Type1Font font, Type1Lexer lexer)
    {
        Token? first = lexer.NextToken();
        if (first is null)
        {
            return;
        }

        if (first.GetKind() == Token.NAME && first.GetText() == "StandardEncoding")
        {
            font.SetEncoding(PdfStandardEncoding.INSTANCE);
            ConsumeDefinitionRemainder(lexer);
            return;
        }

        Dictionary<int, string> map = [];
        Token? token;
        while ((token = lexer.NextToken()) is not null)
        {
            if (token.GetKind() == Token.NAME && token.GetText() == "dup")
            {
                Token code = Require(lexer.NextToken(), Token.INTEGER);
                Token name = Require(lexer.NextToken(), Token.LITERAL);
                map[code.IntValue()] = name.GetText();
                Token? put = lexer.NextToken();
                if (put is null)
                {
                    break;
                }
            }
            else if (token.GetKind() == Token.NAME && token.GetText() == "def")
            {
                break;
            }
        }

        font.SetEncoding(new PdfBuiltInEncoding(map));
    }

    private static List<Token> ReadValue(Type1Lexer lexer)
    {
        List<Token> tokens = [];
        int depth = 0;
        while (lexer.NextToken() is Token token)
        {
            if (token.GetKind() == Token.NAME && depth == 0 && (token.GetText() == "def" || token.GetText() == "readonly" || token.GetText() == "noaccess" || token.GetText() == "ND" || token.GetText() == "|-" || token.GetText() == "NP"))
            {
                if (token.GetText() == "readonly" || token.GetText() == "noaccess")
                {
                    continue;
                }

                break;
            }

            if (token.GetKind() is Token.Kind.StartArray or Token.Kind.StartProc or Token.Kind.StartDict)
            {
                depth++;
            }
            else if (token.GetKind() is Token.Kind.EndArray or Token.Kind.EndProc or Token.Kind.EndDict)
            {
                depth--;
                if (depth < 0)
                {
                    break;
                }
            }

            tokens.Add(token);
        }

        return tokens;
    }

    private static void ApplySimpleValue(Type1Font font, string key, List<Token> value)
    {
        if (value.Count == 0)
        {
            return;
        }

        if (value.Count == 1)
        {
            Token token = value[0];
            switch (token.GetKind())
            {
                case Token.Kind.String:
                case Token.Kind.Name:
                case Token.Kind.Literal:
                    font.SetProperty(key, token.GetText());
                    return;
                case Token.Kind.Integer:
                    font.SetProperty(key, token.IntValue());
                    return;
                case Token.Kind.Real:
                    font.SetProperty(key, token.FloatValue());
                    return;
            }
        }

        List<float> numbers = [];
        foreach (Token token in value)
        {
            if (token.GetKind() == Token.INTEGER)
            {
                numbers.Add(token.IntValue());
            }
            else if (token.GetKind() == Token.REAL)
            {
                numbers.Add(token.FloatValue());
            }
        }

        if (numbers.Count > 0)
        {
            font.SetProperty(key, numbers);
        }
    }

    private static void ConsumeDefinitionRemainder(Type1Lexer lexer)
    {
        while (lexer.PeekToken() is Token token && token.GetKind() == Token.NAME && (token.GetText() == "readonly" || token.GetText() == "noaccess"))
        {
            lexer.NextToken();
        }

        if (lexer.PeekToken() is Token def && def.GetKind() == Token.NAME && def.GetText() == "def")
        {
            lexer.NextToken();
        }
    }

    private static void ParseBinary(Type1Font font, byte[] bytes)
    {
        byte[] cipher = IsBinary(bytes) ? bytes : HexToBinary(bytes);
        byte[] decrypted = Decrypt(cipher, EexecKey, 4);
        Type1Lexer lexer = new(decrypted);
        while (lexer.NextToken() is Token token)
        {
            if (token.GetKind() != Token.LITERAL)
            {
                continue;
            }

            string key = token.GetText();
            if (key == "Subrs")
            {
                ParseSubrs(font, lexer);
            }
            else if (key == "CharStrings")
            {
                ParseCharStrings(font, lexer);
            }
            else
            {
                List<Token> value = ReadValue(lexer);
                ApplySimpleValue(font, key, value);
            }
        }
    }

    private static void ParseSubrs(Type1Font font, Type1Lexer lexer)
    {
        _ = lexer.NextToken(); // count
        _ = lexer.NextToken(); // array
        while (lexer.PeekToken() is Token token)
        {
            if (token.GetKind() == Token.NAME && token.GetText() == "dup")
            {
                lexer.NextToken();
                int index = Require(lexer.NextToken(), Token.INTEGER).IntValue();
                _ = Require(lexer.NextToken(), Token.INTEGER);
                Token data = Require(lexer.NextToken(), Token.CHARSTRING);
                font.AddSubr(index, Decrypt(data.GetData(), Type1Key, font.GetIntPropertyForParser("lenIV", 4)));
                continue;
            }

            if (token.GetKind() == Token.LITERAL || (token.GetKind() == Token.NAME && token.GetText() == "end"))
            {
                break;
            }

            lexer.NextToken();
        }
    }

    private static void ParseCharStrings(Type1Font font, Type1Lexer lexer)
    {
        _ = lexer.NextToken(); // count
        _ = lexer.NextToken(); // dict
        while (lexer.NextToken() is Token token)
        {
            if (token.GetKind() == Token.NAME && token.GetText() == "begin")
            {
                break;
            }
        }

        while (lexer.PeekToken() is Token token)
        {
            if (token.GetKind() == Token.NAME && token.GetText() == "end")
            {
                lexer.NextToken();
                break;
            }

            Token name = Require(lexer.NextToken(), Token.LITERAL);
            _ = Require(lexer.NextToken(), Token.INTEGER);
            Token data = Require(lexer.NextToken(), Token.CHARSTRING);
            font.AddCharString(name.GetText(), Decrypt(data.GetData(), Type1Key, font.GetIntPropertyForParser("lenIV", 4)));
            ConsumeCharStringTerminator(lexer);
        }
    }

    private static void ConsumeCharStringTerminator(Type1Lexer lexer)
    {
        while (lexer.PeekToken() is Token token)
        {
            if (token.GetKind() != Token.NAME)
            {
                break;
            }

            string name = token.GetText();
            if (name is "ND" or "def" or "noaccess" or "readonly" or "|-" or "NP")
            {
                lexer.NextToken();
                continue;
            }

            break;
        }
    }

    private static Token Require(Token? token, Token.Kind kind)
    {
        if (token is null || token.GetKind() != kind)
        {
            throw new IOException($"Expected token {kind} but got {token}");
        }

        return token;
    }

    private static byte[] Decrypt(byte[] cipherBytes, ushort seed, int discard)
    {
        byte[] plain = new byte[Math.Max(0, cipherBytes.Length - discard)];
        int r = seed;
        int c1 = 52845;
        int c2 = 22719;

        for (int i = 0; i < cipherBytes.Length; i++)
        {
            int cipher = cipherBytes[i] & 0xFF;
            int plainByte = cipher ^ (r >> 8);
            r = ((cipher + r) * c1 + c2) & 0xFFFF;
            if (i >= discard)
            {
                plain[i - discard] = (byte)plainByte;
            }
        }

        return plain;
    }

    private static bool IsBinary(byte[] bytes)
    {
        int max = Math.Min(4, bytes.Length);
        for (int i = 0; i < max; i++)
        {
            if (!IsHex(bytes[i]))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsHex(byte b)
    {
        return (b is >= (byte)'0' and <= (byte)'9') ||
               (b is >= (byte)'A' and <= (byte)'F') ||
               (b is >= (byte)'a' and <= (byte)'f') ||
               char.IsWhiteSpace((char)b);
    }

    private static byte[] HexToBinary(byte[] bytes)
    {
        StringBuilder hex = new(bytes.Length);
        foreach (byte b in bytes)
        {
            if (IsHex(b) && !char.IsWhiteSpace((char)b))
            {
                hex.Append((char)b);
            }
        }

        if ((hex.Length & 1) == 1)
        {
            hex.Append('0');
        }

        byte[] data = new byte[hex.Length / 2];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = byte.Parse(hex.ToString(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        return data;
    }
}
