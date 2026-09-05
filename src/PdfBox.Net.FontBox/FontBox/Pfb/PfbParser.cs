/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/pfb/PfbParser.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 046747da99a870902217efabf1c41297de157059
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

using Microsoft.Extensions.Logging;
using PdfBox.Net.Logging;

namespace PdfBox.Net.FontBox.Pfb;

public sealed class PfbParser
{
    private static ILogger<PfbParser> LOG => PdfBoxLogging.CreateLogger<PfbParser>();

    private const int PfbHeaderLength = 18;
    private const int StartMarker = 0x80;
    private const int AsciiMarker = 0x01;
    private const int BinaryMarker = 0x02;
    private const int EofMarker = 0x03;

    private byte[] _pfbData = [];
    private readonly int[] _lengths = new int[3];

    public PfbParser(string filename)
    {
        using Stream input = File.OpenRead(filename);
        ParsePfb(input);
    }

    public PfbParser(Stream input)
    {
        ParsePfb(input);
    }

    public PfbParser(byte[] bytes)
    {
        using MemoryStream input = new(bytes, writable: false);
        ParsePfb(input);
    }

    public int[] GetLengths() => (int[])_lengths.Clone();

    public byte[] GetPfbdata() => _pfbData;

    public Stream GetInputStream() => new MemoryStream(_pfbData, writable: false);

    public int Size() => _pfbData.Length;

    public byte[] GetSegment1() => _pfbData[.._lengths[0]];

    public byte[] GetSegment2() => _pfbData[_lengths[0]..(_lengths[0] + _lengths[1])];

    private void ParsePfb(Stream stream)
    {
        List<int> types = [];
        List<byte[]> segments = [];
        long total = 0;

        while (true)
        {
            int start = stream.ReadByte();
            if (start == -1 && total > 0)
            {
                break;
            }

            if (start != StartMarker)
            {
                throw new IOException("Start marker missing");
            }

            int recordType = stream.ReadByte();
            if (recordType == EofMarker)
            {
                break;
            }

            if (recordType != AsciiMarker && recordType != BinaryMarker)
            {
                throw new IOException($"Incorrect record type: {recordType}");
            }

            int size = ReadLittleEndianInt(stream);
            LOG.LogDebug("Record type: {RecordType}, segment size: {SegmentSize}", recordType, size);
            if (size < 0)
            {
                throw new IOException($"record size {size} is negative");
            }

            // PDFBOX-6044: grow the buffer only as bytes actually arrive, so a bogus/huge
            // size cannot force an allocation larger than what the stream really holds.
            byte[] segment = ReadSegment(stream, size);
            if (segment.Length != size)
            {
                throw new EndOfStreamException("EOF while reading PFB font");
            }

            total += size;
            types.Add(recordType);
            segments.Add(segment);
        }

        if (total < PfbHeaderLength)
        {
            throw new IOException("PFB header missing");
        }

        _pfbData = new byte[total];
        byte[]? clearToMark = null;
        int dst = 0;

        for (int i = 0; i < types.Count; i++)
        {
            if (types[i] != AsciiMarker)
            {
                continue;
            }

            byte[] segment = segments[i];
            if (i == types.Count - 1 && segment.Length < 600 && System.Text.Encoding.ASCII.GetString(segment).Contains("cleartomark", StringComparison.Ordinal))
            {
                clearToMark = segment;
                continue;
            }

            Array.Copy(segment, 0, _pfbData, dst, segment.Length);
            dst += segment.Length;
        }

        _lengths[0] = dst;

        for (int i = 0; i < types.Count; i++)
        {
            if (types[i] != BinaryMarker)
            {
                continue;
            }

            byte[] segment = segments[i];
            Array.Copy(segment, 0, _pfbData, dst, segment.Length);
            dst += segment.Length;
        }

        _lengths[1] = dst - _lengths[0];

        if (clearToMark is not null)
        {
            Array.Copy(clearToMark, 0, _pfbData, dst, clearToMark.Length);
            _lengths[2] = clearToMark.Length;
        }
    }

    private static byte[] ReadSegment(Stream stream, int size)
    {
        using MemoryStream segment = new();
        byte[] buffer = new byte[Math.Min(size, 8192)];
        int remaining = size;
        while (remaining > 0)
        {
            int read = stream.Read(buffer, 0, Math.Min(remaining, buffer.Length));
            if (read == 0)
            {
                break;
            }

            segment.Write(buffer, 0, read);
            remaining -= read;
        }

        return segment.ToArray();
    }

    private static int ReadLittleEndianInt(Stream stream)
    {
        int b0 = stream.ReadByte();
        int b1 = stream.ReadByte();
        int b2 = stream.ReadByte();
        int b3 = stream.ReadByte();
        if (b3 < 0)
        {
            throw new EndOfStreamException("EOF while reading PFB font header");
        }

        return b0 | (b1 << 8) | (b2 << 16) | (b3 << 24);
    }
}
