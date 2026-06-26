/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cff/CFFCIDFont.java
 * PDFBOX_SOURCE_COMMIT: 96856ee9704c928f3c8dbcccedd1a51cb914ef12
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 96856ee9704c928f3c8dbcccedd1a51cb914ef12
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

using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.FontBox.CFF;

public sealed class CFFCIDFont : CFFFont
{
    private readonly Dictionary<int, CIDKeyedType2CharString> _charStringCache = new();
    private readonly List<Dictionary<string, object>> _fontDictionaries = [];
    private readonly List<Dictionary<string, object>> _privateDictionaries = [];
    private FDSelect? _fdSelect;

    public string Registry { get; internal set; } = string.Empty;
    public string Ordering { get; internal set; } = string.Empty;
    public int Supplement { get; internal set; }
    public IReadOnlyList<Dictionary<string, object>> GetFontDicts() => _fontDictionaries;
    public IReadOnlyList<Dictionary<string, object>> GetPrivDicts() => _privateDictionaries;
    public FDSelect? GetFDSelect() => _fdSelect;

    internal void SetFontDicts(List<Dictionary<string, object>> fontDicts)
    {
        _fontDictionaries.Clear();
        _fontDictionaries.AddRange(fontDicts);
    }

    internal void SetPrivDicts(List<Dictionary<string, object>> privDicts)
    {
        _privateDictionaries.Clear();
        _privateDictionaries.AddRange(privDicts);
    }

    internal void SetFDSelect(FDSelect fdSelect) => _fdSelect = fdSelect;

    public override Type2CharString GetType2CharString(int cidOrGid)
    {
        if (_charStringCache.TryGetValue(cidOrGid, out CIDKeyedType2CharString? cached))
        {
            return cached;
        }

        int gid = GetCharset().GetGIDForCID(cidOrGid);
        IList<byte[]> charStrings = GetCharStringBytes();
        byte[] bytes = gid >= 0 && gid < charStrings.Count ? charStrings[gid] : charStrings[0];
        List<object> sequence = new Type2CharStringParser(GetName()).Parse(bytes, GetGlobalSubrIndex().ToArray(), GetLocalSubrIndex(gid));
        CIDKeyedType2CharString charString = new(
            GetName(),
            cidOrGid,
            gid,
            sequence,
            GetDefaultWidthX(gid),
            GetNominalWidthX(gid));
        _charStringCache[cidOrGid] = charString;
        return charString;
    }

    public override GeneralPath GetPath(string name) => GetType2CharString(SelectorToCID(name)).GetPath();
    public override float GetWidth(string name) => GetType2CharString(SelectorToCID(name)).GetWidth();
    public override bool HasGlyph(string name) => SelectorToCID(name) != 0;

    private static int SelectorToCID(string selector)
    {
        if (!selector.StartsWith("\\", StringComparison.Ordinal))
        {
            throw new ArgumentException("Invalid selector", nameof(selector));
        }

        return int.Parse(selector.AsSpan(1), System.Globalization.CultureInfo.InvariantCulture);
    }

    private byte[][]? GetLocalSubrIndex(int gid)
    {
        int fdIndex = _fdSelect?.GetFDIndex(gid) ?? -1;
        if (fdIndex < 0 || fdIndex >= _privateDictionaries.Count)
        {
            return null;
        }

        return _privateDictionaries[fdIndex].TryGetValue("Subrs", out object? value) && value is byte[][] subrs
            ? subrs
            : null;
    }

    private int GetDefaultWidthX(int gid) => GetPrivateNumber(gid, "defaultWidthX", 1000);

    private int GetNominalWidthX(int gid) => GetPrivateNumber(gid, "nominalWidthX", 0);

    private int GetPrivateNumber(int gid, string name, int defaultValue)
    {
        int fdIndex = _fdSelect?.GetFDIndex(gid) ?? -1;
        if (fdIndex < 0 || fdIndex >= _privateDictionaries.Count)
        {
            return defaultValue;
        }

        return _privateDictionaries[fdIndex].TryGetValue(name, out object? value) && TryNumber(value, out int number)
            ? number
            : defaultValue;
    }

    private static bool TryNumber(object value, out int number)
    {
        switch (value)
        {
            case int intValue:
                number = intValue;
                return true;
            case float floatValue:
                number = (int)floatValue;
                return true;
            case double doubleValue:
                number = (int)doubleValue;
                return true;
            default:
                number = 0;
                return false;
        }
    }
}
