/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cff/CFFFont.java
 * PDFBOX_SOURCE_COMMIT: f23622e3b60d1601123aab943219e4d38b255eb4
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: f23622e3b60d1601123aab943219e4d38b255eb4
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

using PdfBox.Net.FontBox.Util;

namespace PdfBox.Net.FontBox.CFF;

public abstract class CFFFont : FontBoxFont
{
    private string _fontName = string.Empty;
    private CFFCharset _charset = new CFFCharsetType1();
    protected readonly Dictionary<string, object> topDict = new(StringComparer.Ordinal);
    protected byte[][] charStrings = [];
    protected byte[][] globalSubrIndex = [];
    private byte[] _data = [];

    public string GetName() => _fontName;
    internal void SetName(string name) => _fontName = name;

    public void AddValueToTopDict(string name, object value)
    {
        topDict[name] = value;
    }

    public IDictionary<string, object> GetTopDict() => topDict;

    public IList<float> GetFontMatrix()
    {
        return topDict.TryGetValue("FontMatrix", out object? value) && value is List<float> floats
            ? floats
            : [0.001f, 0, 0, 0.001f, 0, 0];
    }

    public BoundingBox GetFontBBox()
    {
        return topDict.TryGetValue("FontBBox", out object? value) && value is List<float> floats && floats.Count >= 4
            ? new BoundingBox(floats)
            : new BoundingBox();
    }

    public CFFCharset GetCharset() => _charset;
    internal void SetCharset(CFFCharset charset) => _charset = charset;
    public IList<byte[]> GetCharStringBytes() => charStrings;
    internal void SetCharStrings(byte[][] value) => charStrings = value;
    internal void SetData(byte[] data) => _data = data;
    public byte[] GetData() => _data;
    public int GetNumCharStrings() => charStrings.Length;
    internal void SetGlobalSubrIndex(byte[][] value) => globalSubrIndex = value;
    public IList<byte[]> GetGlobalSubrIndex() => globalSubrIndex;
    public abstract Type2CharString GetType2CharString(int cidOrGid);
    public abstract PdfBox.Net.Util.Geometry.GeneralPath GetPath(string name);
    public abstract float GetWidth(string name);
    public abstract bool HasGlyph(string name);
}
