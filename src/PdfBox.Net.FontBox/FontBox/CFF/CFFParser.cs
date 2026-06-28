/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cff/CFFParser.java
 * PDFBOX_SOURCE_COMMIT: 746cf4e103f4c5ef3897edd3715088ca43beee42
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 746cf4e103f4c5ef3897edd3715088ca43beee42
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
using PdfBox.Net.FontBox.TTF;
using PdfBox.Net.IO;

namespace PdfBox.Net.FontBox.CFF;

public sealed class CFFParser
{
    private const string TagOtto = "OTTO";
    private const string TagTtcf = "ttcf";
    private static readonly byte[] TtfOnlyTag = [0x00, 0x01, 0x00, 0x00];
    private static readonly HashSet<string> DeltaKeys = ["BlueValues", "OtherBlues", "FamilyBlues", "FamilyOtherBlues", "StemSnapH", "StemSnapV"];
    private string? _debugFontName;

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
        Header header = ReadHeader(input);
        input.Seek(header.HeaderSize);

        string[] names = ReadIndexStrings(input);
        if (names.Length == 0)
        {
            throw new IOException("Name index missing in CFF font");
        }

        byte[][] topDicts = ReadIndexData(input);
        if (topDicts.Length == 0)
        {
            throw new IOException("Top DICT INDEX missing in CFF font");
        }

        string[] strings = ReadIndexStrings(input);
        byte[][] globalSubrs = ReadIndexData(input);

        List<CFFFont> fonts = [];
        for (int i = 0; i < names.Length; i++)
        {
            Dict topDict = ReadDict(topDicts[i], strings);
            CFFFont font = ParseFont(names[i], cffBytes, topDict, globalSubrs, strings);
            fonts.Add(font);
        }

        return fonts;
    }

    public override string ToString()
    {
        return $"{nameof(CFFParser)}[{_debugFontName ?? "null"}]";
    }

    private CFFFont ParseFont(string name, byte[] cffBytes, Dict topDict, byte[][] globalSubrs, string[] strings)
    {
        bool isCid = topDict.TryGetArray("ROS", out List<float>? ros);
        CFFFont font = isCid ? new CFFCIDFont() : new CFFType1Font();
        _debugFontName = name;
        font.SetName(name);
        font.SetData(cffBytes);
        font.SetGlobalSubrIndex(globalSubrs);
        ApplyTopDict(font, topDict);

        if (!topDict.TryGetInt("CharStrings", out int charStringsOffset))
        {
            throw new IOException("CharStrings is missing or empty");
        }

        byte[][] charStrings = ReadIndexData(new Reader(cffBytes, charStringsOffset));
        font.SetCharStrings(charStrings);
        font.SetCharset(ReadCharset(cffBytes, topDict, strings, charStrings.Length, isCid));

        if (font is CFFType1Font type1)
        {
            type1.SetEncoding(ReadEncoding(cffBytes, topDict, strings, type1.GetCharset()));
            ReadType1Private(cffBytes, topDict, strings, type1);
            type1.BuildNameToGidMap();
        }
        else if (font is CFFCIDFont cid && ros is not null)
        {
            cid.Registry = ResolveString((int)ros[0], strings);
            cid.Ordering = ResolveString((int)ros[1], strings);
            cid.Supplement = (int)ros[2];
            ParseCIDFontDicts(cffBytes, topDict, strings, charStrings.Length, cid);
        }

        return font;
    }

    private static void ParseCIDFontDicts(byte[] cffBytes, Dict topDict, string[] strings, int nGlyphs, CFFCIDFont cid)
    {
        if (!topDict.TryGetInt("FDArray", out int fdArrayOffset))
        {
            throw new IOException("FDArray is missing for a CIDKeyed Font.");
        }

        byte[][] fdIndex = ReadIndexData(new Reader(cffBytes, fdArrayOffset));
        if (fdIndex.Length == 0)
        {
            throw new IOException("Font dict index is missing for a CIDKeyed Font.");
        }

        List<Dictionary<string, object>> fontDicts = [];
        List<Dictionary<string, object>> privateDicts = [];
        bool privateDictPopulated = false;

        foreach (byte[] fdBytes in fdIndex)
        {
            Dict fontDict = ReadDict(fdBytes, strings);
            Dictionary<string, object> fontDictMap = new(StringComparer.Ordinal)
            {
                ["FontName"] = fontDict.GetStringOrNull("FontName", strings) ?? string.Empty,
                ["FontType"] = fontDict.GetNumberOrDefault("FontType", 0f),
                ["FontBBox"] = fontDict.GetArrayOrEmpty("FontBBox"),
                ["FontMatrix"] = fontDict.GetArrayOrEmpty("FontMatrix")
            };
            fontDicts.Add(fontDictMap);

            if (!fontDict.TryGetArray("Private", out List<float>? privateEntry) || privateEntry is null || privateEntry.Count < 2)
            {
                privateDicts.Add(new Dictionary<string, object>(StringComparer.Ordinal));
                continue;
            }

            int privateSize = (int)privateEntry[0];
            int privateOffset = (int)privateEntry[1];
            Dict privateDict = ReadDict(cffBytes.AsSpan(privateOffset, privateSize).ToArray(), strings, isPrivateDict: true);
            Dictionary<string, object> parsedPrivate = ReadPrivateDict(privateDict);
            privateDictPopulated = true;

            if (privateDict.TryGetInt("Subrs", out int subrsOffset))
            {
                parsedPrivate["Subrs"] = ReadIndexData(new Reader(cffBytes, privateOffset + subrsOffset));
            }

            privateDicts.Add(parsedPrivate);
        }

        if (!privateDictPopulated)
        {
            throw new IOException("Font DICT invalid without \"Private\" entry");
        }

        if (!topDict.TryGetInt("FDSelect", out int fdSelectOffset))
        {
            throw new IOException("FDSelect is missing or empty");
        }

        FDSelect fdSelect = ReadFDSelect(new Reader(cffBytes, fdSelectOffset), nGlyphs);
        cid.SetFontDicts(fontDicts);
        cid.SetPrivDicts(privateDicts);
        cid.SetFDSelect(fdSelect);
    }

    private static Dictionary<string, object> ReadPrivateDict(Dict dict)
    {
        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["BlueValues"] = dict.GetArrayOrEmpty("BlueValues"),
            ["OtherBlues"] = dict.GetArrayOrEmpty("OtherBlues"),
            ["FamilyBlues"] = dict.GetArrayOrEmpty("FamilyBlues"),
            ["FamilyOtherBlues"] = dict.GetArrayOrEmpty("FamilyOtherBlues"),
            ["BlueScale"] = dict.GetNumberOrDefault("BlueScale", 0.039625f),
            ["BlueShift"] = dict.GetNumberOrDefault("BlueShift", 7),
            ["BlueFuzz"] = dict.GetNumberOrDefault("BlueFuzz", 1),
            ["StdHW"] = dict.GetFirstOrDefault("StdHW") ?? 0f,
            ["StdVW"] = dict.GetFirstOrDefault("StdVW") ?? 0f,
            ["StemSnapH"] = dict.GetArrayOrEmpty("StemSnapH"),
            ["StemSnapV"] = dict.GetArrayOrEmpty("StemSnapV"),
            ["ForceBold"] = dict.GetNumberOrDefault("ForceBold", 0f),
            ["LanguageGroup"] = dict.GetNumberOrDefault("LanguageGroup", 0f),
            ["ExpansionFactor"] = dict.GetNumberOrDefault("ExpansionFactor", 0.06f),
            ["initialRandomSeed"] = dict.GetNumberOrDefault("initialRandomSeed", 0f),
            ["defaultWidthX"] = dict.GetNumberOrDefault("defaultWidthX", 0f),
            ["nominalWidthX"] = dict.GetNumberOrDefault("nominalWidthX", 0f)
        };
    }

    private static void ApplyTopDict(CFFFont font, Dict topDict)
    {
        foreach ((string key, object value) in topDict.Values)
        {
            font.AddValueToTopDict(key, value);
        }
    }

    private static void ReadType1Private(byte[] cffBytes, Dict topDict, string[] strings, CFFType1Font font)
    {
        if (!topDict.TryGetArray("Private", out List<float>? privateData) || privateData is null || privateData.Count < 2)
        {
            throw new IOException($"Private dictionary entry missing for font {font.GetName()}");
        }

        int size = (int)privateData[0];
        int offset = (int)privateData[1];
        Dict privateDict = ReadDict(cffBytes.AsSpan(offset, size).ToArray(), strings, isPrivateDict: true);
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
            return isCid ? new EmptyCharsetCID(nGlyphs) : CFFISOAdobeCharset.INSTANCE;
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

        return ReadEmbeddedCharset(new Reader(cffBytes, charsetOffset), strings, nGlyphs, isCid);
    }

    private static CFFCharset ReadEmbeddedCharset(Reader input, string[] strings, int nGlyphs, bool isCid)
    {
        int format = input.ReadCard8();
        return format switch
        {
            0 => ReadFormat0Charset(input, strings, nGlyphs, isCid),
            1 => ReadFormat1Charset(input, strings, nGlyphs, isCid),
            2 => ReadFormat2Charset(input, strings, nGlyphs, isCid),
            _ => throw new IOException($"Incorrect charset format {format}")
        };
    }

    private static CFFCharset ReadFormat0Charset(Reader input, string[] strings, int nGlyphs, bool isCid)
    {
        EmbeddedCharset charset = new(isCid);
        if (isCid)
        {
            charset.AddCID(0, 0);
            for (int gid = 1; gid < nGlyphs; gid++)
            {
                charset.AddCID(gid, input.ReadCard16());
            }
        }
        else
        {
            charset.AddSID(0, 0, ".notdef");
            for (int gid = 1; gid < nGlyphs; gid++)
            {
                int sid = input.ReadCard16();
                charset.AddSID(gid, sid, ResolveString(sid, strings));
            }
        }

        return charset;
    }

    private static CFFCharset ReadFormat1Charset(Reader input, string[] strings, int nGlyphs, bool isCid)
    {
        EmbeddedCharset charset = new(isCid);
        if (isCid)
        {
            charset.AddCID(0, 0);
            int gid = 1;
            while (gid < nGlyphs)
            {
                int first = input.ReadCard16();
                int left = input.ReadCard8();
                for (int j = 0; j <= left && gid < nGlyphs; j++)
                {
                    charset.AddCID(gid++, first + j);
                }
            }
        }
        else
        {
            charset.AddSID(0, 0, ".notdef");
            int gid = 1;
            while (gid < nGlyphs)
            {
                int first = input.ReadCard16();
                int left = input.ReadCard8();
                for (int j = 0; j <= left && gid < nGlyphs; j++)
                {
                    int sid = first + j;
                    charset.AddSID(gid++, sid, ResolveString(sid, strings));
                }
            }
        }

        return charset;
    }

    private static CFFCharset ReadFormat2Charset(Reader input, string[] strings, int nGlyphs, bool isCid)
    {
        EmbeddedCharset charset = new(isCid);
        if (isCid)
        {
            charset.AddCID(0, 0);
            int gid = 1;
            while (gid < nGlyphs)
            {
                int first = input.ReadCard16();
                int left = input.ReadCard16();
                for (int j = 0; j <= left && gid < nGlyphs; j++)
                {
                    charset.AddCID(gid++, first + j);
                }
            }
        }
        else
        {
            charset.AddSID(0, 0, ".notdef");
            int gid = 1;
            while (gid < nGlyphs)
            {
                int first = input.ReadCard16();
                int left = input.ReadCard16();
                for (int j = 0; j <= left && gid < nGlyphs; j++)
                {
                    int sid = first + j;
                    charset.AddSID(gid++, sid, ResolveString(sid, strings));
                }
            }
        }

        return charset;
    }

    private static CFFEncoding ReadEncoding(byte[] cffBytes, Dict topDict, string[] strings, CFFCharset charset)
    {
        if (!topDict.TryGetInt("Encoding", out int encodingId))
        {
            return CFFStandardEncoding.INSTANCE;
        }

        return encodingId switch
        {
            0 => CFFStandardEncoding.INSTANCE,
            1 => CFFExpertEncoding.INSTANCE,
            _ => ReadEmbeddedEncoding(new Reader(cffBytes, encodingId), strings, charset)
        };
    }

    private static CFFEncoding ReadEmbeddedEncoding(Reader input, string[] strings, CFFCharset charset)
    {
        int format = input.ReadCard8();
        int baseFormat = format & 0x7F;
        CFFEncoding encoding = baseFormat switch
        {
            0 => ReadFormat0Encoding(input, strings, charset),
            1 => ReadFormat1Encoding(input, strings, charset),
            _ => throw new IOException($"Invalid encoding base format {baseFormat}")
        };

        if ((format & 0x80) != 0)
        {
            ReadEncodingSupplement(input, encoding, strings);
        }

        return encoding;
    }

    private static CFFEncoding ReadFormat0Encoding(Reader input, string[] strings, CFFCharset charset)
    {
        int nCodes = input.ReadCard8();
        Format0Encoding encoding = new(nCodes);
        encoding.Add(0, 0, ".notdef");
        for (int gid = 1; gid <= nCodes; gid++)
        {
            int code = input.ReadCard8();
            int sid = charset.GetSIDForGID(gid);
            encoding.Add(code, sid, ResolveString(sid, strings));
        }

        return encoding;
    }

    private static CFFEncoding ReadFormat1Encoding(Reader input, string[] strings, CFFCharset charset)
    {
        int nRanges = input.ReadCard8();
        Format1Encoding encoding = new(nRanges);
        encoding.Add(0, 0, ".notdef");
        int gid = 1;
        for (int i = 0; i < nRanges; i++)
        {
            int first = input.ReadCard8();
            int left = input.ReadCard8();
            for (int j = 0; j <= left; j++)
            {
                int sid = charset.GetSIDForGID(gid++);
                encoding.Add(first + j, sid, ResolveString(sid, strings));
            }
        }

        return encoding;
    }

    private static void ReadEncodingSupplement(Reader input, CFFEncoding encoding, string[] strings)
    {
        int nSups = input.ReadCard8();
        for (int i = 0; i < nSups; i++)
        {
            int code = input.ReadCard8();
            int sid = input.ReadCard16();
            encoding.Add(code, sid, ResolveString(sid, strings));
        }
    }

    private static FDSelect ReadFDSelect(Reader input, int nGlyphs)
    {
        int format = input.ReadCard8();
        return format switch
        {
            0 => ReadFormat0FDSelect(input, nGlyphs),
            3 => ReadFormat3FDSelect(input),
            _ => throw new IOException($"Unsupported FDSelect format {format}")
        };
    }

    private static FDSelect ReadFormat0FDSelect(Reader input, int nGlyphs)
    {
        int[] fds = new int[nGlyphs];
        for (int i = 0; i < nGlyphs; i++)
        {
            fds[i] = input.ReadCard8();
        }

        return new Format0FDSelect(fds);
    }

    private static FDSelect ReadFormat3FDSelect(Reader input)
    {
        int nRanges = input.ReadCard16();
        List<Range3> ranges = new(nRanges);
        for (int i = 0; i < nRanges; i++)
        {
            ranges.Add(new Range3(input.ReadCard16(), input.ReadCard8()));
        }

        int sentinel = input.ReadCard16();
        return new Format3FDSelect(ranges, sentinel);
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
        if (bytes.Length >= 4 && System.Text.Encoding.ASCII.GetString(bytes, 0, 4) == TagTtcf)
        {
            throw new IOException("True Type Collection fonts are not supported.");
        }

        if (bytes.Length >= 4 && bytes.AsSpan(0, 4).SequenceEqual(TtfOnlyTag))
        {
            throw new IOException("OpenType fonts containing a true type font are not supported.");
        }

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
        int major = input.ReadCard8();
        int minor = input.ReadCard8();
        int headerSize = input.ReadCard8();
        int offSize = input.ReadCard8();
        if (offSize is < 1 or > 4)
        {
            throw new IOException($"Illegal offSize value {offSize} in CFF font");
        }

        return new Header(major, minor, headerSize, offSize);
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
        if (offSize is < 1 or > 4)
        {
            throw new IOException($"Illegal offSize value {offSize} in CFF font");
        }

        int[] offsets = new int[count + 1];
        for (int i = 0; i <= count; i++)
        {
            offsets[i] = input.ReadOffset(offSize);
            if (offsets[i] > input.Length)
            {
                throw new IOException($"Illegal offset value {offsets[i]} in CFF font");
            }
        }

        int dataStart = input.Position;
        byte[][] result = new byte[count][];
        for (int i = 0; i < count; i++)
        {
            int start = dataStart + offsets[i] - 1;
            int end = dataStart + offsets[i + 1] - 1;
            if (end < start)
            {
                throw new IOException($"Negative index data length at entry {i}");
            }

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
                string? op = ReadOperator(b0, input);
                if (op is null)
                {
                    throw new IOException($"Unsupported CFF operator {b0}");
                }

                dict.Add(op, DeltaKeys.Contains(op) ? ToDelta(operands) : new List<float>(operands));
                operands.Clear();
            }
            else if (b0 == 28 || b0 == 29 || b0 == 30 || (b0 >= 32 && b0 <= 254))
            {
                operands.Add(ReadNumber(b0, input));
            }
            else
            {
                throw new IOException($"Invalid DICT data b0 byte: {b0}");
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

    private static string? ReadOperator(int b0, Reader input)
    {
        return b0 == 12 ? CFFOperator.GetOperator(b0, input.ReadCard8()) : CFFOperator.GetOperator(b0);
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
            _ => throw new IOException($"Unsupported CFF operand byte {b0}")
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

        public float? GetFirstOrDefault(string key)
        {
            return TryGetArray(key, out List<float>? values) && values is not null && values.Count > 0 ? values[0] : null;
        }

        public float GetNumberOrDefault(string key, float defaultValue)
        {
            return GetFirstOrDefault(key) ?? defaultValue;
        }

        public List<float>? GetArrayOrDefault(string key, List<float>? defaultValue)
        {
            return TryGetArray(key, out List<float>? values) && values is not null ? values : defaultValue;
        }

        public List<float> GetArrayOrEmpty(string key)
        {
            return TryGetArray(key, out List<float>? values) && values is not null ? values : [];
        }

        public string? GetStringOrNull(string key, string[] strings)
        {
            if (TryGetArray(key, out List<float>? values) && values is not null && values.Count > 0)
            {
                return ResolveString((int)values[0], strings);
            }

            return null;
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
        public int Length => _data.Length;
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
            StringBuilder sb = new();
            bool done = false;
            bool exponentMissing = false;
            bool hasExponent = false;
            while (!done)
            {
                int b = ReadCard8();
                int[] nibbles = [b >> 4, b & 0x0F];
                foreach (int nibble in nibbles)
                {
                    switch (nibble)
                    {
                        case >= 0x0 and <= 0x9:
                            sb.Append((char)('0' + nibble));
                            exponentMissing = false;
                            break;
                        case 0xA:
                            sb.Append('.');
                            break;
                        case 0xB:
                            if (!hasExponent)
                            {
                                sb.Append('E');
                                exponentMissing = true;
                                hasExponent = true;
                            }
                            break;
                        case 0xC:
                            if (!hasExponent)
                            {
                                sb.Append("E-");
                                exponentMissing = true;
                                hasExponent = true;
                            }
                            break;
                        case 0xD:
                            break;
                        case 0xE:
                            sb.Append('-');
                            break;
                        case 0xF:
                            done = true;
                            break;
                    }

                    if (done)
                    {
                        break;
                    }
                }
            }

            if (exponentMissing)
            {
                sb.Append('0');
            }

            if (sb.Length == 0)
            {
                return 0f;
            }

            return float.Parse(sb.ToString(), CultureInfo.InvariantCulture);
        }
    }

    private sealed class Format0Encoding : CFFEncoding
    {
        public int NumberOfCodes { get; }
        public Format0Encoding(int numberOfCodes) => NumberOfCodes = numberOfCodes;
    }

    private sealed class Format1Encoding : CFFEncoding
    {
        public int NumberOfRanges { get; }
        public Format1Encoding(int numberOfRanges) => NumberOfRanges = numberOfRanges;
    }

    private sealed class EmptyCharsetCID : CFFCharsetCID
    {
        public EmptyCharsetCID(int numCharStrings)
        {
            AddCID(0, 0);
            for (int i = 1; i < numCharStrings; i++)
            {
                AddCID(i, i);
            }
        }
    }

    private readonly record struct Range3(int First, int FD);

    private sealed class Format0FDSelect : FDSelect
    {
        private readonly int[] _fds;

        public Format0FDSelect(int[] fds)
        {
            _fds = fds;
        }

        public int GetFDIndex(int gid)
        {
            return gid >= 0 && gid < _fds.Length ? _fds[gid] : 0;
        }
    }

    private sealed class Format3FDSelect : FDSelect
    {
        private readonly List<Range3> _ranges;
        private readonly int _sentinel;

        public Format3FDSelect(List<Range3> ranges, int sentinel)
        {
            _ranges = ranges;
            _sentinel = sentinel;
        }

        public int GetFDIndex(int gid)
        {
            for (int i = 0; i < _ranges.Count; i++)
            {
                if (_ranges[i].First <= gid)
                {
                    if (i + 1 < _ranges.Count)
                    {
                        if (_ranges[i + 1].First > gid)
                        {
                            return _ranges[i].FD;
                        }
                    }
                    else
                    {
                        return _sentinel > gid ? _ranges[i].FD : -1;
                    }
                }
            }

            return 0;
        }
    }
}
