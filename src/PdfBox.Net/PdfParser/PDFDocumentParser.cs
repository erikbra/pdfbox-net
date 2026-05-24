/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdfparser/PDFParser.java
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

namespace PdfBox.Net.PdfParser;

public sealed class PDFDocumentParser
{
    private const int DefaultTrailByteCount = 2048;
    private static readonly byte[] PdfHeaderBytes = Encoding.ASCII.GetBytes("%PDF-");
    private static readonly byte[] EofMarkerBytes = Encoding.ASCII.GetBytes("%%EOF");
    private static readonly byte[] StartXrefBytes = Encoding.ASCII.GetBytes("startxref");

    private readonly byte[] _data;

    public PDFDocumentParser(Stream input)
    {
        ArgumentNullException.ThrowIfNull(input);
        using MemoryStream copy = new();
        input.CopyTo(copy);
        _data = copy.ToArray();
    }

    public ParserBootstrapState ParseDocumentStart()
    {
        return ParseDocumentStart(_data);
    }

    internal static ParserBootstrapState ParseDocumentStart(byte[] data)
    {
        float headerVersion = ParseHeaderVersion(data);
        (long startXrefOffset, long startXrefKeywordOffset) = FindStartXref(data);
        return new ParserBootstrapState(headerVersion, startXrefOffset, startXrefKeywordOffset);
    }

    private static float ParseHeaderVersion(byte[] data)
    {
        int start = IndexOf(data, PdfHeaderBytes, 0, data.Length);
        if (start < 0)
        {
            throw new IOException("Error: Header doesn't contain versioninfo");
        }

        int cursor = start + PdfHeaderBytes.Length;
        int tokenStart = cursor;
        while (cursor < data.Length && data[cursor] is >= (byte)'0' and <= (byte)'9')
        {
            cursor++;
        }

        if (cursor == tokenStart || cursor >= data.Length || data[cursor] != '.')
        {
            throw new IOException("Error: Header doesn't contain versioninfo");
        }

        cursor++;
        int fractionStart = cursor;
        while (cursor < data.Length && data[cursor] is >= (byte)'0' and <= (byte)'9')
        {
            cursor++;
        }

        if (cursor == fractionStart)
        {
            throw new IOException("Error: Header doesn't contain versioninfo");
        }

        string versionToken = Encoding.ASCII.GetString(data, tokenStart, cursor - tokenStart);
        if (!float.TryParse(versionToken, NumberStyles.Float, CultureInfo.InvariantCulture, out float version))
        {
            throw new IOException("Error: Header doesn't contain versioninfo");
        }

        return version;
    }

    private static (long startXrefOffset, long startXrefKeywordOffset) FindStartXref(byte[] data)
    {
        int trailingByteCount = Math.Min(data.Length, DefaultTrailByteCount);
        int trailingStart = data.Length - trailingByteCount;

        int eofInTrailing = LastIndexOf(data, EofMarkerBytes, trailingStart, data.Length);
        int searchEndExclusive = eofInTrailing >= 0 ? eofInTrailing : data.Length;

        int startXrefIndex = LastIndexOf(data, StartXrefBytes, trailingStart, searchEndExclusive);
        if (startXrefIndex < 0)
        {
            throw new IOException("startxref marker not found.");
        }

        int cursor = startXrefIndex + StartXrefBytes.Length;
        while (cursor < data.Length && IsWhiteSpace(data[cursor]))
        {
            cursor++;
        }

        int offsetStart = cursor;
        while (cursor < data.Length && data[cursor] is >= (byte)'0' and <= (byte)'9')
        {
            cursor++;
        }

        if (cursor == offsetStart)
        {
            throw new IOException("startxref marker does not contain an offset.");
        }

        string token = Encoding.ASCII.GetString(data, offsetStart, cursor - offsetStart);
        if (!long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out long startXrefOffset))
        {
            throw new IOException("Invalid startxref value.");
        }

        return (startXrefOffset, startXrefIndex);
    }

    private static int IndexOf(byte[] source, byte[] pattern, int start, int endExclusive)
    {
        int last = endExclusive - pattern.Length;
        for (int i = start; i <= last; i++)
        {
            bool matched = true;
            for (int j = 0; j < pattern.Length; j++)
            {
                if (source[i + j] != pattern[j])
                {
                    matched = false;
                    break;
                }
            }

            if (matched)
            {
                return i;
            }
        }

        return -1;
    }

    private static int LastIndexOf(byte[] source, byte[] pattern, int startInclusive, int endExclusive)
    {
        for (int i = endExclusive - pattern.Length; i >= startInclusive; i--)
        {
            bool matched = true;
            for (int j = 0; j < pattern.Length; j++)
            {
                if (source[i + j] != pattern[j])
                {
                    matched = false;
                    break;
                }
            }

            if (matched)
            {
                return i;
            }
        }

        return -1;
    }

    private static bool IsWhiteSpace(int value)
    {
        return value is 0 or 9 or 10 or 12 or 13 or 32;
    }
}

public sealed record ParserBootstrapState(float HeaderVersion, long StartXrefOffset, long StartXrefKeywordOffset);
