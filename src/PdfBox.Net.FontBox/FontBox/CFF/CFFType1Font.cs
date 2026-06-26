/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cff/CFFType1Font.java
 * PDFBOX_SOURCE_COMMIT: b38089ae1f9915bdde9c87c81962af31d843ff18
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: b38089ae1f9915bdde9c87c81962af31d843ff18
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

using System.Collections.Concurrent;
using PdfBox.Net.Util.Geometry;
using FontEncoding = PdfBox.Net.FontBox.Encoding.Encoding;

namespace PdfBox.Net.FontBox.CFF;

public sealed class CFFType1Font : CFFFont, EncodedFont
{
    private readonly ConcurrentDictionary<int, Type2CharString> _cache = new();
    private readonly Dictionary<string, object> _privateDict = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _nameToGid = new(StringComparer.Ordinal);
    private CFFEncoding _encoding = CFFStandardEncoding.INSTANCE;
    private byte[][] _localSubrs = [];

    internal void SetEncoding(CFFEncoding encoding) => _encoding = encoding;
    internal void SetPrivateDict(Dictionary<string, object> privateDict)
    {
        _privateDict.Clear();
        foreach ((string key, object value) in privateDict)
        {
            _privateDict[key] = value;
        }
    }
    internal Dictionary<string, object> GetMutablePrivateDict() => _privateDict;
    internal void SetLocalSubrs(byte[][] localSubrs) => _localSubrs = localSubrs;

    internal void BuildNameToGidMap()
    {
        _nameToGid.Clear();
        for (int gid = 0; gid < GetNumCharStrings(); gid++)
        {
            _nameToGid[GetCharset().GetNameForGID(gid)] = gid;
        }
    }

    public FontEncoding GetEncoding() => _encoding;
    public IDictionary<string, object> GetPrivateDict() => _privateDict;
    public IList<byte[]> GetLocalSubrIndex() => _localSubrs;

    public override Type2CharString GetType2CharString(int gid)
    {
        return _cache.GetOrAdd(gid, value =>
        {
            IList<byte[]> charStrings = GetCharStringBytes();
            byte[] bytes = value >= 0 && value < charStrings.Count ? charStrings[value] : charStrings[0];
            List<object> sequence = new Type2CharStringParser(GetName()).Parse(bytes, GetGlobalSubrIndex().ToArray(), _localSubrs);
            return new Type2CharString(GetName(), GetCharset().GetNameForGID(value), value, sequence, GetDefaultWidthX(), GetNominalWidthX());
        });
    }

    public Type2CharString GetType1CharString(string name)
    {
        return GetType2CharString(NameToGID(name));
    }

    public int NameToGID(string name)
    {
        return _nameToGid.TryGetValue(name, out int gid) ? gid : 0;
    }

    public override GeneralPath GetPath(string name)
    {
        return _nameToGid.TryGetValue(name, out int gid) ? GetType2CharString(gid).GetPath() : new GeneralPath();
    }

    public override float GetWidth(string name)
    {
        return _nameToGid.TryGetValue(name, out int gid) ? GetType2CharString(gid).GetWidth() : 0;
    }

    public override bool HasGlyph(string name)
    {
        return _nameToGid.ContainsKey(name);
    }

    private int GetDefaultWidthX() => GetNumberProperty("defaultWidthX", 1000);

    private int GetNominalWidthX() => GetNumberProperty("nominalWidthX", 0);

    private int GetNumberProperty(string name, int defaultValue)
    {
        if (topDict.TryGetValue(name, out object? topValue) && TryNumber(topValue, out int topNumber))
        {
            return topNumber;
        }

        return _privateDict.TryGetValue(name, out object? privateValue) && TryNumber(privateValue, out int privateNumber)
            ? privateNumber
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
