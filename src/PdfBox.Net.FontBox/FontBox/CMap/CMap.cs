/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cmap/CMap.java
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

using System.IO;

namespace PdfBox.Net.FontBox.CMap;

/// <summary>
/// This class represents a CMap file.
/// </summary>
public class CMap
{
    private const string Space = " ";

    private int _wMode;
    private string? _cmapName;
    private string? _cmapVersion;
    private int _cmapType = -1;

    private string? _registry;
    private string? _ordering;
    private int _supplement;

    private int _minCodeLength = 4;
    private int _maxCodeLength;

    private int _minCidLength = 4;
    private int _maxCidLength;

    private readonly List<CodespaceRange> _codespaceRanges = [];

    private readonly Dictionary<int, string> _charToUnicodeOneByte = [];
    private readonly Dictionary<int, string> _charToUnicodeTwoBytes = [];
    private readonly Dictionary<int, string> _charToUnicodeMoreBytes = [];

    private readonly Dictionary<int, Dictionary<int, int>> _codeToCid = [];
    private readonly List<CIDRange> _codeToCidRanges = [];

    private readonly Dictionary<string, byte[]> _unicodeToByteCodes = [];

    private int _spaceMapping = -1;

    internal CMap()
    {
    }

    public bool HasCIDMappings() => _codeToCid.Count != 0 || _codeToCidRanges.Count != 0;

    public bool HasUnicodeMappings() => _charToUnicodeOneByte.Count != 0
        || _charToUnicodeTwoBytes.Count != 0
        || _charToUnicodeMoreBytes.Count != 0;

    public string? ToUnicode(int code)
    {
        string? unicode = code < 256 ? ToUnicode(code, 1) : null;
        if (unicode is null)
        {
            if (code <= 0xFFFF)
            {
                return ToUnicode(code, 2);
            }

            if (code <= 0xFFFFFF)
            {
                return ToUnicode(code, 3);
            }

            return ToUnicode(code, 4);
        }

        return unicode;
    }

    public string? ToUnicode(int code, int length)
    {
        if (length == 1)
        {
            return _charToUnicodeOneByte.GetValueOrDefault(code);
        }

        if (length == 2)
        {
            return _charToUnicodeTwoBytes.GetValueOrDefault(code);
        }

        return _charToUnicodeMoreBytes.GetValueOrDefault(code);
    }

    public string? ToUnicode(byte[] code) => ToUnicode(ToInt(code), code.Length);

    public int ReadCode(Stream input)
    {
        byte[] bytes = new byte[_maxCodeLength];

        int bytesRead = 0;
        while (bytesRead < _minCodeLength)
        {
            int read = input.Read(bytes, bytesRead, _minCodeLength - bytesRead);
            if (read <= 0)
            {
                break;
            }

            bytesRead += read;
        }

        long resetPosition = input.CanSeek ? input.Position : -1;

        for (int i = _minCodeLength - 1; i < _maxCodeLength; i++)
        {
            int byteCount = i + 1;
            if (_codespaceRanges.Any(r => r.IsFullMatch(bytes, byteCount)))
            {
                return ToInt(bytes, byteCount);
            }

            if (byteCount < _maxCodeLength)
            {
                int next = input.ReadByte();
                if (next == -1)
                {
                    break;
                }

                bytes[byteCount] = (byte)next;
            }
        }

        if (input.CanSeek && resetPosition >= 0)
        {
            input.Position = resetPosition;
        }
        else
        {
            Console.Error.WriteLine($"mark() and reset() not supported, {_maxCodeLength - 1} bytes have been skipped");
        }

        return ToInt(bytes, _minCodeLength);
    }

    public static int ToInt(byte[] data)
    {
        return ToInt(data, data.Length);
    }

    internal static int ToInt(byte[] data, int dataLen)
    {
        int code = 0;
        for (int i = 0; i < dataLen; ++i)
        {
            code <<= 8;
            code |= data[i] & 0xFF;
        }

        return code;
    }

    public int ToCID(byte[] code)
    {
        if (!HasCIDMappings() || code.Length < _minCidLength || code.Length > _maxCidLength)
        {
            return 0;
        }

        int? cid = null;
        if (_codeToCid.TryGetValue(code.Length, out Dictionary<int, int>? codeToCidMap))
        {
            if (codeToCidMap.TryGetValue(ToInt(code), out int directCid))
            {
                cid = directCid;
            }
        }

        return cid ?? ToCIDFromRanges(code);
    }

    public int ToCID(int code)
    {
        if (!HasCIDMappings())
        {
            return 0;
        }

        int cid = 0;
        int length = _minCidLength;
        while (cid == 0 && length <= _maxCidLength)
        {
            cid = ToCID(code, length++);
        }

        return cid;
    }

    public int ToCID(int code, int length)
    {
        if (!HasCIDMappings() || length < _minCidLength || length > _maxCidLength)
        {
            return 0;
        }

        int? cid = null;
        if (_codeToCid.TryGetValue(length, out Dictionary<int, int>? codeToCidMap))
        {
            if (codeToCidMap.TryGetValue(code, out int directCid))
            {
                cid = directCid;
            }
        }

        return cid ?? ToCIDFromRanges(code, length);
    }

    private int ToCIDFromRanges(int code, int length)
    {
        foreach (CIDRange range in _codeToCidRanges)
        {
            int ch = range.Map(code, length);
            if (ch != -1)
            {
                return ch;
            }
        }

        return 0;
    }

    private int ToCIDFromRanges(byte[] code)
    {
        foreach (CIDRange range in _codeToCidRanges)
        {
            int ch = range.Map(code);
            if (ch != -1)
            {
                return ch;
            }
        }

        return 0;
    }

    internal void AddCharMapping(byte[] codes, string unicode)
    {
        switch (codes.Length)
        {
            case 1:
                _charToUnicodeOneByte[CMapStrings.GetIndexValue(codes) ?? ToInt(codes)] = unicode;
                _unicodeToByteCodes[unicode] = CMapStrings.GetByteValue(codes) ?? [.. codes];
                break;
            case 2:
                _charToUnicodeTwoBytes[CMapStrings.GetIndexValue(codes) ?? ToInt(codes)] = unicode;
                _unicodeToByteCodes[unicode] = CMapStrings.GetByteValue(codes) ?? [.. codes];
                break;
            case 3:
            case 4:
                _charToUnicodeMoreBytes[ToInt(codes)] = unicode;
                _unicodeToByteCodes[unicode] = [.. codes];
                break;
            default:
                Console.Error.WriteLine($"Mappings with more than 4 bytes (here: {codes.Length}) aren't supported yet");
                break;
        }

        if (Space.Equals(unicode, StringComparison.Ordinal))
        {
            _spaceMapping = ToInt(codes);
        }
    }

    public byte[]? GetCodesFromUnicode(string unicode) => _unicodeToByteCodes.GetValueOrDefault(unicode);

    internal void AddCIDMapping(byte[] code, int cid)
    {
        if (!_codeToCid.TryGetValue(code.Length, out Dictionary<int, int>? codeToCidMap))
        {
            codeToCidMap = [];
            _codeToCid[code.Length] = codeToCidMap;
            _minCidLength = Math.Min(_minCidLength, code.Length);
            _maxCidLength = Math.Max(_maxCidLength, code.Length);
        }

        codeToCidMap[ToInt(code)] = cid;
    }

    internal void AddCIDRange(byte[] from, byte[] to, int cid)
    {
        AddCIDRange(_codeToCidRanges, ToInt(from), ToInt(to), cid, from.Length);
    }

    private void AddCIDRange(List<CIDRange> cidRanges, int from, int to, int cid, int length)
    {
        CIDRange? lastRange = cidRanges.Count > 0 ? cidRanges[^1] : null;
        if (lastRange is null || !lastRange.Extend(from, to, cid, length))
        {
            cidRanges.Add(new CIDRange(from, to, cid, length));
            _minCidLength = Math.Min(_minCidLength, length);
            _maxCidLength = Math.Max(_maxCidLength, length);
        }
    }

    internal void AddCodespaceRange(CodespaceRange range)
    {
        _codespaceRanges.Add(range);
        _maxCodeLength = Math.Max(_maxCodeLength, range.CodeLength);
        _minCodeLength = Math.Min(_minCodeLength, range.CodeLength);
    }

    internal void UseCmap(CMap cmap)
    {
        foreach (CodespaceRange range in cmap._codespaceRanges)
        {
            AddCodespaceRange(range);
        }

        foreach ((int key, string value) in cmap._charToUnicodeOneByte)
        {
            _charToUnicodeOneByte[key] = value;
            _unicodeToByteCodes[value] = [(byte)(key % 0xFF)];
        }

        foreach ((int key, string value) in cmap._charToUnicodeTwoBytes)
        {
            _charToUnicodeTwoBytes[key] = value;
            _unicodeToByteCodes[value] = [(byte)((key >>> 8) & 0xFF), (byte)(key & 0xFF)];
        }

        foreach ((int key, string value) in cmap._charToUnicodeMoreBytes)
        {
            _charToUnicodeMoreBytes[key] = value;
            byte[] bar = key <= 0xFFFFFF
                ? [(byte)((key >>> 16) & 0xFF), (byte)((key >>> 8) & 0xFF), (byte)(key & 0xFF)]
                : [(byte)((key >>> 24) & 0xFF), (byte)((key >>> 16) & 0xFF), (byte)((key >>> 8) & 0xFF), (byte)(key & 0xFF)];
            _unicodeToByteCodes[value] = bar;
        }

        foreach ((int key, Dictionary<int, int> value) in cmap._codeToCid)
        {
            if (!_codeToCid.TryGetValue(key, out Dictionary<int, int>? existing))
            {
                _codeToCid[key] = new Dictionary<int, int>(value);
            }
            else
            {
                foreach ((int mapCode, int mapCid) in value)
                {
                    existing[mapCode] = mapCid;
                }
            }
        }

        _codeToCidRanges.AddRange(cmap._codeToCidRanges);
        _maxCodeLength = Math.Max(_maxCodeLength, cmap._maxCodeLength);
        _minCodeLength = Math.Min(_minCodeLength, cmap._minCodeLength);
        _maxCidLength = Math.Max(_maxCidLength, cmap._maxCidLength);
        _minCidLength = Math.Min(_minCidLength, cmap._minCidLength);
    }

    public int WMode
    {
        get => _wMode;
        set => _wMode = value;
    }

    public string? Name
    {
        get => _cmapName;
        set => _cmapName = value;
    }

    public string? Version
    {
        get => _cmapVersion;
        set => _cmapVersion = value;
    }

    public int Type
    {
        get => _cmapType;
        set => _cmapType = value;
    }

    public string? Registry
    {
        get => _registry;
        set => _registry = value;
    }

    public string? Ordering
    {
        get => _ordering;
        set => _ordering = value;
    }

    public int Supplement
    {
        get => _supplement;
        set => _supplement = value;
    }

    public int SpaceMapping => _spaceMapping;

    public override string? ToString() => _cmapName;
}
