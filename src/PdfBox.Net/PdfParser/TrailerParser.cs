/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdfparser/TrailerParser.java
 * PDFBOX_SOURCE_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc
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

using PdfBox.Net.COS;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using PdfBox.Net.Logging;

namespace PdfBox.Net.PdfParser;

/// <summary>Reads the trailing EOF and startxref information of a PDF.</summary>
/// <remarks>The Stream/byte-array parser adapter resolves xref sections in PDFParser.</remarks>
public class TrailerParser
{
    private static ILogger<TrailerParser> LOG => PdfBoxLogging.CreateLogger<TrailerParser>();
    private const int DefaultTrailByteCount = 2048;
    private static readonly byte[] EofMarkerBytes = Encoding.ASCII.GetBytes("%%EOF");
    private static readonly byte[] StartXrefBytes = Encoding.ASCII.GetBytes("startxref");

    private readonly PDFParser _parser;
    private readonly COSDocument _document;

    /// <summary>Creates a parser for the trailer of the supplied document and source.</summary>
    public TrailerParser(COSDocument cosDocument, COSParser cosParser)
        : this(cosDocument, new PDFParser(cosParser.CopySourceBytes(), cosDocument))
    {
    }

    internal TrailerParser(COSDocument document, PDFParser parser)
    {
        _document = document;
        _parser = parser;
    }

    /// <summary>Whether the trailer was rebuilt by brute-force recovery.</summary>
    /// <remarks>The existing eager .NET parser reports malformed xrefs rather than rebuilding them.</remarks>
    public bool TrailerWasRebuild() => false;

    /// <summary>Returns the live table populated while reading the trailer.</summary>
    public Dictionary<COSObjectKey, long> GetXrefTable() => _document.GetXrefTable();

    /// <summary>Reads the trailer dictionary and populates the document xref table.</summary>
    protected COSDictionary RetrieveTrailer() => _parser.ReadTrailerValues();

    internal COSDictionary RetrieveTrailerValues() => RetrieveTrailer();

    internal static (long startXrefOffset, long startXrefKeywordOffset) FindStartXref(byte[] data)
    {
        int trailingByteCount = Math.Min(data.Length, DefaultTrailByteCount);
        int trailingStart = data.Length - trailingByteCount;

        int eofInTrailing = LastIndexOf(data, EofMarkerBytes, trailingStart, data.Length);
        if (eofInTrailing < 0)
        {
            LOG.LogDebug("Missing end of file marker '{EndOfFileMarker}'", Encoding.ASCII.GetString(EofMarkerBytes));
        }
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
