/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/type1/Type1Font.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
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
using PdfBox.Net.FontBox.CFF;
using PdfBox.Net.FontBox.Pfb;
using PdfBox.Net.FontBox.Util;
using PdfBox.Net.Util.Geometry;
using FontEncoding = PdfBox.Net.FontBox.Encoding.Encoding;
using PdfBuiltInEncoding = PdfBox.Net.FontBox.Encoding.BuiltInEncoding;
using PdfStandardEncoding = PdfBox.Net.FontBox.Encoding.StandardEncoding;

namespace PdfBox.Net.FontBox.Type1;

public sealed class Type1Font : FontBoxFont, EncodedFont, Type1CharStringReader
{
    private readonly ConcurrentDictionary<string, Type1CharString> _charStringCache = new(StringComparer.Ordinal);
    private readonly List<byte[]> _subrs = [];
    private readonly Dictionary<string, byte[]> _charStringsDict = new(StringComparer.Ordinal);
    private readonly Dictionary<string, object> _properties = new(StringComparer.Ordinal);

    public static Type1Font CreateWithPFB(Stream pfbStream)
    {
        return CreateWithPFB(ReadAllBytes(pfbStream));
    }

    public static Type1Font CreateWithPFB(byte[] pfbBytes)
    {
        PfbParser pfb = new(pfbBytes);
        Type1Font font = CreateWithSegments(pfb.GetSegment1(), pfb.GetSegment2());
        font._asciiSegment = pfb.GetSegment1();
        font._binarySegment = pfb.GetSegment2();
        return font;
    }

    public static Type1Font CreateWithSegments(byte[] segment1, byte[] segment2)
    {
        return new Type1Parser().Parse(segment1, segment2);
    }

    private byte[] _asciiSegment = [];
    private byte[] _binarySegment = [];
    private FontEncoding _encoding = PdfStandardEncoding.INSTANCE;

    internal void SetEncoding(FontEncoding encoding) => _encoding = encoding;
    internal void AddSubr(int index, byte[] bytes)
    {
        while (_subrs.Count <= index)
        {
            _subrs.Add(Array.Empty<byte>());
        }

        _subrs[index] = bytes;
    }

    internal void AddCharString(string name, byte[] bytes) => _charStringsDict[name] = bytes;
    internal void SetProperty(string key, object value) => _properties[key] = value;
    internal void SetAsciiSegment(byte[] bytes) => _asciiSegment = bytes;
    internal void SetBinarySegment(byte[] bytes) => _binarySegment = bytes;
    internal int GetIntPropertyForParser(string key, int defaultValue = 0) => GetIntProperty(key, defaultValue);

    public IList<byte[]> GetSubrsArray() => _subrs;
    public IDictionary<string, byte[]> GetCharStringsDict() => _charStringsDict;
    public string GetName() => GetFontName();

    public GeneralPath GetPath(string name)
    {
        return GetType1CharString(name).GetPath();
    }

    public float GetWidth(string name)
    {
        return GetType1CharString(name).GetWidth();
    }

    public bool HasGlyph(string name)
    {
        return _charStringsDict.ContainsKey(name);
    }

    public Type1CharString GetType1CharString(string name)
    {
        if (!_charStringsDict.TryGetValue(name, out byte[]? bytes))
        {
            throw new IOException($"Type 1 glyph '{name}' was not found.");
        }

        return _charStringCache.GetOrAdd(name, key => new Type1CharString(GetFontName(), key, bytes));
    }

    public string GetFontName() => GetStringProperty("FontName") ?? string.Empty;
    public FontEncoding GetEncoding() => _encoding;
    public int GetPaintType() => GetIntProperty("PaintType");
    public int GetFontType() => GetIntProperty("FontType");
    public IList<float> GetFontMatrix() => GetFloatListProperty("FontMatrix");
    public BoundingBox GetFontBBox() => new(GetFloatListProperty("FontBBox"));
    public int GetUniqueID() => GetIntProperty("UniqueID");
    public float GetStrokeWidth() => GetFloatProperty("StrokeWidth");
    public string GetFontID() => GetStringProperty("FontID") ?? string.Empty;
    public string GetVersion() => GetStringProperty("version") ?? string.Empty;
    public string GetNotice() => GetStringProperty("Notice") ?? string.Empty;
    public string GetFullName() => GetStringProperty("FullName") ?? string.Empty;
    public string GetFamilyName() => GetStringProperty("FamilyName") ?? string.Empty;
    public string GetWeight() => GetStringProperty("Weight") ?? string.Empty;
    public float GetItalicAngle() => GetFloatProperty("ItalicAngle");
    public bool IsFixedPitch() => GetBoolProperty("isFixedPitch");
    public float GetUnderlinePosition() => GetFloatProperty("UnderlinePosition");
    public float GetUnderlineThickness() => GetFloatProperty("UnderlineThickness");
    public IList<float> GetBlueValues() => GetFloatListProperty("BlueValues");
    public IList<float> GetOtherBlues() => GetFloatListProperty("OtherBlues");
    public IList<float> GetFamilyBlues() => GetFloatListProperty("FamilyBlues");
    public IList<float> GetFamilyOtherBlues() => GetFloatListProperty("FamilyOtherBlues");
    public float GetBlueScale() => GetFloatProperty("BlueScale", 0.039625f);
    public int GetBlueShift() => GetIntProperty("BlueShift", 7);
    public int GetBlueFuzz() => GetIntProperty("BlueFuzz", 1);
    public IList<float> GetStdHW() => GetFloatListProperty("StdHW");
    public IList<float> GetStdVW() => GetFloatListProperty("StdVW");
    public IList<float> GetStemSnapH() => GetFloatListProperty("StemSnapH");
    public IList<float> GetStemSnapV() => GetFloatListProperty("StemSnapV");
    public bool IsForceBold() => GetBoolProperty("ForceBold");
    public int GetLanguageGroup() => GetIntProperty("LanguageGroup");
    public byte[] GetASCIISegment() => _asciiSegment;
    public byte[] GetBinarySegment() => _binarySegment;

    private static byte[] ReadAllBytes(Stream stream)
    {
        using MemoryStream buffer = new();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }

    private string? GetStringProperty(string key)
    {
        return _properties.TryGetValue(key, out object? value) ? value as string : null;
    }

    private int GetIntProperty(string key, int defaultValue = 0)
    {
        return _properties.TryGetValue(key, out object? value) && value is int intValue ? intValue : defaultValue;
    }

    private float GetFloatProperty(string key, float defaultValue = 0)
    {
        return _properties.TryGetValue(key, out object? value) && value is float floatValue ? floatValue : defaultValue;
    }

    private bool GetBoolProperty(string key)
    {
        return _properties.TryGetValue(key, out object? value) && value is bool boolValue && boolValue;
    }

    private IList<float> GetFloatListProperty(string key)
    {
        return _properties.TryGetValue(key, out object? value) && value is List<float> floats ? floats : [];
    }

    public override string ToString() => $"Type1Font[fontName={GetFontName()}]";
}
