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
using Microsoft.Extensions.Logging;
using PdfBox.Net.Logging;

namespace PdfBox.Net.PdfParser;

public sealed class PDFDocumentParser
{
    private static ILogger<PDFDocumentParser> LOG => PdfBoxLogging.CreateLogger<PDFDocumentParser>();

    private static readonly byte[] PdfHeaderBytes = Encoding.ASCII.GetBytes("%PDF-");

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
        (long startXrefOffset, long startXrefKeywordOffset) = TrailerParser.FindStartXref(data);
        return new ParserBootstrapState(headerVersion, startXrefOffset, startXrefKeywordOffset);
    }

    private static float ParseHeaderVersion(byte[] data)
    {
        int start = IndexOf(data, PdfHeaderBytes, 0, data.Length);
        if (start < 0)
        {
            if (TryParseDamagedHeaderVersion(data, out float damagedHeaderVersion))
            {
                return damagedHeaderVersion;
            }

            throw MissingVersionInfo();
        }

        int cursor = start + PdfHeaderBytes.Length;
        int tokenStart = cursor;
        while (cursor < data.Length && data[cursor] is >= (byte)'0' and <= (byte)'9')
        {
            cursor++;
        }

        if (cursor == tokenStart || cursor >= data.Length || data[cursor] != '.')
        {
            throw MissingVersionInfo();
        }

        cursor++;
        int fractionStart = cursor;
        while (cursor < data.Length && data[cursor] is >= (byte)'0' and <= (byte)'9')
        {
            cursor++;
        }

        if (cursor == fractionStart)
        {
            throw MissingVersionInfo();
        }

        string versionToken = Encoding.ASCII.GetString(data, tokenStart, cursor - tokenStart);
        if (!float.TryParse(versionToken, NumberStyles.Float, CultureInfo.InvariantCulture, out float version))
        {
            throw MissingVersionInfo();
        }

        return version;
    }

    private static bool TryParseDamagedHeaderVersion(byte[] data, out float version)
    {
        version = 0;
        int lineEnd = 0;
        while (lineEnd < data.Length && data[lineEnd] is not (byte)'\r' and not (byte)'\n')
        {
            lineEnd++;
        }

        if (lineEnd < 7 || data[0] != (byte)'%' || data[1] != (byte)'P')
        {
            return false;
        }

        int marker = IndexOf(data, Encoding.ASCII.GetBytes("F-"), 0, lineEnd);
        if (marker < 0 || marker + 4 >= lineEnd)
        {
            return false;
        }

        int tokenStart = marker + 2;
        int cursor = tokenStart;
        while (cursor < lineEnd && data[cursor] is >= (byte)'0' and <= (byte)'9')
        {
            cursor++;
        }

        if (cursor == tokenStart || cursor >= lineEnd || data[cursor] != '.')
        {
            return false;
        }

        cursor++;
        int fractionStart = cursor;
        while (cursor < lineEnd && data[cursor] is >= (byte)'0' and <= (byte)'9')
        {
            cursor++;
        }

        if (cursor == fractionStart)
        {
            return false;
        }

        string versionToken = Encoding.ASCII.GetString(data, tokenStart, cursor - tokenStart);
        return float.TryParse(versionToken, NumberStyles.Float, CultureInfo.InvariantCulture, out version);
    }

    private static IOException MissingVersionInfo()
    {
        LOG.LogWarning("Error: Header doesn't contain versioninfo");
        return new IOException("Error: Header doesn't contain versioninfo");
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


}

public sealed record ParserBootstrapState(float HeaderVersion, long StartXrefOffset, long StartXrefKeywordOffset);
