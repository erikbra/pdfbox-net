/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/pfb/PfbParser.java
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

using System.Text;

namespace PdfBox.Net.FontBox.Pfb;

public sealed class PfbParser
{
    private const int PfbHeaderLength = 18;
    private const int StartMarker = 0x80;
    private const int AsciiMarker = 0x01;
    private const int BinaryMarker = 0x02;
    private const int EofMarker = 0x03;

    private byte[] _pfbData = [];
    private readonly int[] _lengths = new int[3];

    public PfbParser(string filename)
        : this(File.ReadAllBytes(filename))
    {
    }

    public PfbParser(Stream input)
        : this(ReadAllBytes(input))
    {
    }

    public PfbParser(byte[] bytes)
    {
        ParsePfb(bytes);
    }

    public int[] GetLengths() => (int[])_lengths.Clone();

    public byte[] GetPfbdata() => _pfbData;

    public Stream GetInputStream() => new MemoryStream(_pfbData, writable: false);

    public int Size() => _pfbData.Length;

    public byte[] GetSegment1() => _pfbData[.._lengths[0]];

    public byte[] GetSegment2() => _pfbData[_lengths[0]..(_lengths[0] + _lengths[1])];

    private static byte[] ReadAllBytes(Stream input)
    {
        using MemoryStream buffer = new();
        input.CopyTo(buffer);
        return buffer.ToArray();
    }

    private void ParsePfb(byte[] pfb)
    {
        if (pfb.Length < PfbHeaderLength)
        {
            throw new IOException("PFB header missing");
        }

        List<int> types = [];
        List<byte[]> segments = [];
        using MemoryStream stream = new(pfb, writable: false);
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
            if (size < 0)
            {
                throw new IOException($"record size {size} is negative");
            }

            if (size > pfb.Length)
            {
                throw new IOException($"record size {size} would be larger than the input");
            }

            byte[] segment = new byte[size];
            int read = stream.Read(segment, 0, size);
            if (read != size)
            {
                throw new EndOfStreamException("EOF while reading PFB font");
            }

            total += size;
            types.Add(recordType);
            segments.Add(segment);
        }

        if (total > pfb.Length)
        {
            throw new IOException($"total record size {total} would be larger than the input");
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
