/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cff/CFFParser.java
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

using PdfBox.Net.IO;
using PdfBox.Net.FontBox.TTF;

namespace PdfBox.Net.FontBox.CFF;

public sealed class CFFParser
{
    private const string TagOtto = "OTTO";

    public void ParseFirstSubFontROS(RandomAccessRead randomAccessRead, FontHeaders outHeaders)
    {
        List<CFFFont> fonts = Parse(randomAccessRead);
        if (fonts.Count == 0 || fonts[0] is not CFFCIDFont cid)
        {
            return;
        }

        outHeaders.SetOtfROS(cid.Registry, cid.Ordering, cid.Supplement);
    }

    public List<CFFFont> Parse(RandomAccessRead randomAccessRead)
    {
        randomAccessRead.Seek(0);
        byte[] bytes = new byte[randomAccessRead.Length()];
        randomAccessRead.ReadFully(bytes);
        return Parse(bytes);
    }

    public List<CFFFont> Parse(byte[] bytes)
    {
        byte[] cffBytes = ExtractCffTableIfWrapped(bytes);
        Reader input = new(cffBytes);
        input.Seek(ReadHeader(input).HeaderSize);

        string[] names = ReadIndexStrings(input);
        byte[][] topDicts = ReadIndexData(input);
        string[] strings = ReadIndexStrings(input);
        byte[][] globalSubrs = ReadIndexData(input);

        List<CFFFont> fonts = [];
        for (int i = 0; i < names.Length; i++)
        {
            Dict topDict = ReadDict(topDicts[i], strings);
            CFFFont font = CreateFont(names[i], cffBytes, topDict, globalSubrs, strings, input);
            fonts.Add(font);
        }

        return fonts;
    }

    private static CFFFont CreateFont(string name, byte[] cffBytes, Dict topDict, byte[][] globalSubrs, string[] strings, Reader input)
    {
        bool isCid = topDict.TryGetArray("ROS", out List<float>? ros);
        CFFFont font = isCid ? new CFFCIDFont() : new CFFType1Font();
        font.SetName(name);
        font.SetData(cffBytes);
        font.SetGlobalSubrIndex(globalSubrs);
        ApplyTopDict(font, topDict);

        int charStringsOffset = topDict.GetInt("CharStrings");
        byte[][] charStrings = ReadIndexData(new Reader(cffBytes, charStringsOffset));
        font.SetCharStrings(charStrings);

        if (font is CFFType1Font type1)
        {
            CFFCharset charset = ReadCharset(cffBytes, topDict, strings, charStrings.Length, isCid: false);
            font.SetCharset(charset);
            type1.SetEncoding(ReadEncoding(cffBytes, topDict));
            ReadPrivate(cffBytes, topDict, strings, type1);
            type1.BuildNameToGidMap();
        }
        else if (font is CFFCIDFont cid && ros is not null)
        {
            font.SetCharset(ReadCharset(cffBytes, topDict, strings, charStrings.Length, isCid: true));
            cid.Registry = ResolveString((int)ros[0], strings);
            cid.Ordering = ResolveString((int)ros[1], strings);
            cid.Supplement = (int)ros[2];
        }

        return font;
    }

    private static void ApplyTopDict(CFFFont font, Dict topDict)
    {
        foreach ((string key, object value) in topDict.Values)
        {
            font.AddValueToTopDict(key, value);
        }
    }

    private static void ReadPrivate(byte[] cffBytes, Dict topDict, string[] strings, CFFType1Font font)
    {
        if (!topDict.TryGetArray("Private", out List<float>? privateData) || privateData is null || privateData.Count < 2)
        {
            return;
        }

        int size = (int)privateData[0];
        int offset = (int)privateData[1];
        Dict privateDict = ReadDict(cffBytes[offset..(offset + size)], strings, isPrivateDict: true);
        Dictionary<string, object> target = font.GetMutablePrivateDict();
        foreach ((string key, object value) in privateDict.Values)
        {
            target[key] = value;
        }

        if (privateDict.TryGetInt("Subrs", out int subrsOffset))
        {
            font.SetLocalSubrs(ReadIndexData(new Reader(cffBytes, offset + subrsOffset)));
        }
    }

    private static CFFCharset ReadCharset(byte[] cffBytes, Dict topDict, string[] strings, int nGlyphs, bool isCid)
    {
        if (!topDict.TryGetInt("charset", out int charsetOffset))
        {
            charsetOffset = 0;
        }

        if (!isCid && charsetOffset == 0)
        {
            return CFFISOAdobeCharset.INSTANCE;
        }

        if (!isCid && charsetOffset == 1)
        {
            return CFFExpertCharset.INSTANCE;
        }

        if (!isCid && charsetOffset == 2)
        {
            return CFFExpertSubsetCharset.INSTANCE;
        }

        EmbeddedCharset embedded = new(isCid);
        if (charsetOffset == 0 && isCid)
        {
            for (int gid = 0; gid < nGlyphs; gid++)
            {
                embedded.AddCID(gid, gid);
            }

            return embedded;
        }

        Reader input = new(cffBytes, charsetOffset);
        int format = input.ReadCard8();

        int gidIndex = 0;
        if (!isCid)
        {
            embedded.AddSID(gidIndex++, 0, ".notdef");
        }

        if (format == 0)
        {
            while (gidIndex < nGlyphs)
            {
                int sidOrCid = input.ReadCard16();
                if (isCid)
                {
                    embedded.AddCID(gidIndex++, sidOrCid);
                }
                else
                {
                    embedded.AddSID(gidIndex++, sidOrCid, ResolveString(sidOrCid, strings));
                }
            }
        }
        else if (format == 1)
        {
            while (gidIndex < nGlyphs)
            {
                int first = input.ReadCard16();
                int nLeft = input.ReadCard8();
                for (int i = 0; i <= nLeft && gidIndex < nGlyphs; i++)
                {
                    int sidOrCid = first + i;
                    if (isCid)
                    {
                        embedded.AddCID(gidIndex++, sidOrCid);
                    }
                    else
                    {
                        embedded.AddSID(gidIndex++, sidOrCid, ResolveString(sidOrCid, strings));
                    }
                }
            }
        }
        else if (format == 2)
        {
            while (gidIndex < nGlyphs)
            {
                int first = input.ReadCard16();
                int nLeft = input.ReadCard16();
                for (int i = 0; i <= nLeft && gidIndex < nGlyphs; i++)
                {
                    int sidOrCid = first + i;
                    if (isCid)
                    {
                        embedded.AddCID(gidIndex++, sidOrCid);
                    }
                    else
                    {
                        embedded.AddSID(gidIndex++, sidOrCid, ResolveString(sidOrCid, strings));
                    }
                }
            }
        }
        else
        {
            throw new IOException($"Unsupported CFF charset format {format}");
        }

        return embedded;
    }

    private static CFFEncoding ReadEncoding(byte[] cffBytes, Dict topDict)
    {
        if (!topDict.TryGetInt("Encoding", out int encodingOffset) || encodingOffset == 0)
        {
            return CFFStandardEncoding.INSTANCE;
        }

        if (encodingOffset == 1)
        {
            return CFFExpertEncoding.INSTANCE;
        }

        Reader input = new(cffBytes, encodingOffset);
        int format = input.ReadCard8();
        bool hasSupplement = (format & 0x80) != 0;
        format &= 0x7F;
        EmbeddedEncoding encoding = new();
        if (format == 0)
        {
            int nCodes = input.ReadCard8();
            for (int gid = 1; gid <= nCodes; gid++)
            {
                int code = input.ReadCard8();
                encoding.Add(code, gid, CFFStandardString.GetName(gid));
            }
        }
        else if (format == 1)
        {
            int nRanges = input.ReadCard8();
            int gid = 1;
            for (int i = 0; i < nRanges; i++)
            {
                int first = input.ReadCard8();
                int nLeft = input.ReadCard8();
                for (int j = 0; j <= nLeft; j++)
                {
                    encoding.Add(first + j, gid, CFFStandardString.GetName(gid));
                    gid++;
                }
            }
        }
        else
        {
            throw new IOException($"Unsupported CFF encoding format {format}");
        }

        if (hasSupplement)
        {
            int nSups = input.ReadCard8();
            for (int i = 0; i < nSups; i++)
            {
                int code = input.ReadCard8();
                int sid = input.ReadCard16();
                encoding.Add(code, sid, ResolveString(sid, []));
            }
        }

        return encoding;
    }

    private static string ResolveString(int sid, string[] customStrings)
    {
        int customIndex = sid - 391;
        if (customIndex >= 0 && customIndex < customStrings.Length)
        {
            return customStrings[customIndex];
        }

        return CFFStandardString.GetName(sid);
    }

    private static byte[] ExtractCffTableIfWrapped(byte[] bytes)
    {
        if (bytes.Length < 4 || System.Text.Encoding.ASCII.GetString(bytes, 0, 4) != TagOtto)
        {
            return bytes;
        }

        Reader input = new(bytes);
        input.Skip(4);
        int numTables = input.ReadCard16();
        input.Skip(6);
        for (int i = 0; i < numTables; i++)
        {
            string tag = input.ReadTag();
            input.Skip(4);
            int offset = input.ReadOffset32();
            int length = input.ReadOffset32();
            if (tag == "CFF ")
            {
                return bytes[offset..(offset + length)];
            }
        }

        throw new IOException("CFF table not found in OpenType font");
    }

    private static Header ReadHeader(Reader input)
    {
        return new Header(input.ReadCard8(), input.ReadCard8(), input.ReadCard8(), input.ReadCard8());
    }

    private static string[] ReadIndexStrings(Reader input)
    {
        return ReadIndexData(input).Select(System.Text.Encoding.ASCII.GetString).ToArray();
    }

    private static byte[][] ReadIndexData(Reader input)
    {
        int count = input.ReadCard16();
        if (count == 0)
        {
            return [];
        }

        int offSize = input.ReadCard8();
        int[] offsets = new int[count + 1];
        for (int i = 0; i <= count; i++)
        {
            offsets[i] = input.ReadOffset(offSize);
        }

        int dataStart = input.Position;
        byte[][] result = new byte[count][];
        for (int i = 0; i < count; i++)
        {
            int start = dataStart + offsets[i] - 1;
            int end = dataStart + offsets[i + 1] - 1;
            result[i] = input.Slice(start, end - start);
        }

        input.Seek(dataStart + offsets[count] - 1);
        return result;
    }

    private static Dict ReadDict(byte[] data, string[] strings, bool isPrivateDict = false)
    {
        Reader input = new(data);
        Dict dict = new();
        List<float> operands = [];
        while (!input.IsEof)
        {
            int b0 = input.ReadCard8();
            if (b0 <= 21)
            {
                string op = ReadOperator(b0, input, isPrivateDict);
                dict.Add(op, op is "BlueValues" ? ToDelta(operands) : new List<float>(operands));
                operands.Clear();
            }
            else
            {
                operands.Add(ReadNumber(b0, input));
            }
        }

        dict.ResolveStrings(strings);
        return dict;
    }

    private static List<float> ToDelta(List<float> values)
    {
        List<float> delta = [];
        float current = 0;
        foreach (float value in values)
        {
            current += value;
            delta.Add(current);
        }

        return delta;
    }

    private static string ReadOperator(int b0, Reader input, bool isPrivateDict)
    {
        string? op = b0 == 12
            ? CFFOperator.GetOperator(b0, input.ReadCard8())
            : CFFOperator.GetOperator(b0);
        return op ?? throw new IOException($"Unsupported CFF operator {b0}");
    }

    private sealed class EmbeddedEncoding : CFFEncoding
    {
    }

    private static float ReadNumber(int b0, Reader input)
    {
        return b0 switch
        {
            >= 32 and <= 246 => b0 - 139,
            >= 247 and <= 250 => ((b0 - 247) * 256) + input.ReadCard8() + 108,
            >= 251 and <= 254 => -((b0 - 251) * 256) - input.ReadCard8() - 108,
            28 => input.ReadInt16(),
            29 => input.ReadInt32(),
            30 => input.ReadReal(),
            _ => throw new IOException($"Unsupported CFF operand byte {b0}"),
        };
    }

    private readonly record struct Header(int Major, int Minor, int HeaderSize, int OffSize);

    private sealed class Dict
    {
        public Dictionary<string, object> Values { get; } = new(StringComparer.Ordinal);

        public void Add(string key, List<float> values)
        {
            Values[key] = values;
        }

        public bool TryGetArray(string key, out List<float>? values)
        {
            if (Values.TryGetValue(key, out object? value) && value is List<float> list)
            {
                values = list;
                return true;
            }

            values = null;
            return false;
        }

        public int GetInt(string key)
        {
            return TryGetInt(key, out int value) ? value : 0;
        }

        public bool TryGetInt(string key, out int value)
        {
            if (TryGetArray(key, out List<float>? values) && values is not null && values.Count > 0)
            {
                value = (int)values[0];
                return true;
            }

            value = 0;
            return false;
        }

        public void ResolveStrings(string[] strings)
        {
            if (TryGetArray("ROS", out List<float>? ros) && ros is not null && ros.Count >= 3)
            {
                Values["ROS"] = new List<float>(ros);
            }
        }
    }

    private sealed class Reader
    {
        private readonly byte[] _data;
        public int Position { get; private set; }
        public bool IsEof => Position >= _data.Length;

        public Reader(byte[] data, int position = 0)
        {
            _data = data;
            Position = position;
        }

        public void Seek(int position) => Position = position;
        public void Skip(int count) => Position += count;
        public byte[] Slice(int start, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(_data, start, result, 0, length);
            return result;
        }

        public int ReadCard8() => _data[Position++];
        public int ReadCard16() => (ReadCard8() << 8) | ReadCard8();
        public int ReadOffset(int size)
        {
            int value = 0;
            for (int i = 0; i < size; i++)
            {
                value = (value << 8) | ReadCard8();
            }

            return value;
        }

        public int ReadOffset32() => (ReadCard8() << 24) | (ReadCard8() << 16) | (ReadCard8() << 8) | ReadCard8();
        public short ReadInt16() => (short)ReadCard16();
        public int ReadInt32() => ReadOffset32();
        public string ReadTag() => System.Text.Encoding.ASCII.GetString(_data, Position, 4) is string tag ? (Position += 4, tag).tag : string.Empty;
        public float ReadReal()
        {
            List<char> chars = [];
            bool end = false;
            while (!end)
            {
                int b = ReadCard8();
                foreach (int nibble in new[] { b >> 4, b & 0x0F })
                {
                    switch (nibble)
                    {
                        case 0xA: chars.Add('.'); break;
                        case 0xB: chars.Add('E'); break;
                        case 0xC: chars.Add('E'); chars.Add('-'); break;
                        case 0xE: chars.Add('-'); break;
                        case 0xF: end = true; break;
                        default: chars.Add((char)('0' + nibble)); break;
                    }

                    if (end)
                    {
                        break;
                    }
                }
            }

            return float.Parse(new string([.. chars]), System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
