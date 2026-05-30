/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/type1/Token.java
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

namespace PdfBox.Net.FontBox.Type1;

public sealed class Token
{
    public enum Kind
    {
        None,
        String,
        Name,
        Literal,
        Real,
        Integer,
        StartArray,
        EndArray,
        StartProc,
        EndProc,
        StartDict,
        EndDict,
        CharString,
    }

    public static readonly Kind STRING = Kind.String;
    public static readonly Kind NAME = Kind.Name;
    public static readonly Kind LITERAL = Kind.Literal;
    public static readonly Kind REAL = Kind.Real;
    public static readonly Kind INTEGER = Kind.Integer;
    public static readonly Kind START_ARRAY = Kind.StartArray;
    public static readonly Kind END_ARRAY = Kind.EndArray;
    public static readonly Kind START_PROC = Kind.StartProc;
    public static readonly Kind END_PROC = Kind.EndProc;
    public static readonly Kind START_DICT = Kind.StartDict;
    public static readonly Kind END_DICT = Kind.EndDict;
    public static readonly Kind CHARSTRING = Kind.CharString;

    public Token(string text, Kind kind)
    {
        Text = text;
        TokenKind = kind;
    }

    public Token(char character, Kind kind)
        : this(character.ToString(), kind)
    {
    }

    public Token(byte[] data, Kind kind)
    {
        Data = data;
        TokenKind = kind;
    }

    public string? Text { get; }

    public byte[]? Data { get; }

    public Kind TokenKind { get; }

    public string GetText() => Text ?? string.Empty;

    public Kind GetKind() => TokenKind;

    public int IntValue() => (int)float.Parse(GetText(), CultureInfo.InvariantCulture);

    public float FloatValue() => float.Parse(GetText(), CultureInfo.InvariantCulture);

    public bool BooleanValue() => GetText() == "true";

    public byte[] GetData() => Data ?? Array.Empty<byte>();

    public override string ToString()
    {
        return TokenKind == Kind.CharString
            ? $"Token[kind=CHARSTRING, data={GetData().Length} bytes]"
            : $"Token[kind={TokenKind}, text={Text}]";
    }
}
