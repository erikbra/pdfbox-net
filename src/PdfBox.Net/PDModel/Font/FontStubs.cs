/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDFont.java
 * PDFBOX_SOURCE_COMMIT: b07158974a4dbbcebf0e33d3797b9f0655cc62d9
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: b07158974a4dbbcebf0e33d3797b9f0655cc62d9
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
using PdfBox.Net.FontBox;
using PdfBox.Net.FontBox.CMap;
using PdfBox.Net.FontBox.TTF;
using PdfBox.Net.FontBox.Type1;
using PdfBox.Net.FontBox.Util;
using PdfBox.Net.IO;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Font.Encoding;
using PdfBox.Net.Util;
using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.PDModel.Font
{
    public partial interface PDFontLike
    {
        string GetName();
    }

    public abstract class PDFont : PDFontLike
    {
        private static readonly COSName BaseFontKey = COSName.GetPDFName("BaseFont");
        private static readonly COSName FirstCharKey = COSName.GetPDFName("FirstChar");
        private static readonly COSName LastCharKey = COSName.GetPDFName("LastChar");
        private static readonly COSName WidthsKey = COSName.GetPDFName("Widths");
        private static readonly COSName ToUnicodeKey = COSName.GetPDFName("ToUnicode");
        private static readonly COSName FontMatrixKey = COSName.GetPDFName("FontMatrix");
        private static readonly COSName FontDescriptorKey = COSName.GetPDFName("FontDescriptor");

        protected readonly COSDictionary FontDictionary;

        private readonly float[]? _widths;
        private readonly int _firstChar;
        private readonly int _lastChar;
        private readonly CMap? _toUnicodeCMap;

        private Matrix? _fontMatrix;
        private PDFontDescriptor? _fontDescriptor;

        protected PDFont(COSDictionary fontDictionary)
        {
            FontDictionary = fontDictionary ?? throw new ArgumentNullException(nameof(fontDictionary));
            _firstChar = FontDictionary.GetInt(FirstCharKey, -1);
            _lastChar = FontDictionary.GetInt(LastCharKey, -1);
            _widths = ReadWidths(FontDictionary.GetCOSArray(WidthsKey));
            _toUnicodeCMap = ReadToUnicodeMap(FontDictionary.GetDictionaryObject(ToUnicodeKey));
        }

        public virtual string GetName()
        {
            COSBase? baseFontEntry = FontDictionary.GetDictionaryObject(BaseFontKey);
            return baseFontEntry switch
            {
                COSName cosName => cosName.GetName(),
                COSString cosString => cosString.GetString(),
                _ => "Unknown",
            };
        }

        public virtual bool IsVertical() => false;

        public virtual float GetWidth(int code)
        {
            if (_widths != null && _firstChar >= 0 && code >= _firstChar && code <= _lastChar)
            {
                int index = code - _firstChar;
                if ((uint)index < (uint)_widths.Length)
                {
                    return _widths[index];
                }
            }

            return GetFontDescriptor()?.GetMissingWidth() ?? 0f;
        }

        public virtual string? ToUnicode(int code, GlyphList glyphList)
        {
            if (_toUnicodeCMap != null)
            {
                string? mapped = _toUnicodeCMap.ToUnicode(code);
                if (!string.IsNullOrEmpty(mapped))
                {
                    return mapped;
                }
            }

            return ToUnicodeFallback(code, glyphList);
        }

        public virtual float GetSpaceWidth()
        {
            float width = GetWidth(32);
            if (width > 0)
            {
                return width;
            }

            width = GetAverageFontWidth();
            return width > 0 ? width * 0.5f : 250f;
        }

        public virtual float GetAverageFontWidth()
        {
            if (_widths is { Length: > 0 })
            {
                float sum = 0;
                int count = 0;
                for (int i = 0; i < _widths.Length; i++)
                {
                    float value = _widths[i];
                    if (value > 0)
                    {
                        sum += value;
                        count++;
                    }
                }

                if (count > 0)
                {
                    return sum / count;
                }
            }

            float missing = GetFontDescriptor()?.GetMissingWidth() ?? 0f;
            return missing > 0 ? missing : 500f;
        }

        public virtual Matrix GetFontMatrix()
        {
            if (_fontMatrix != null)
            {
                return _fontMatrix;
            }

            COSArray? matrixArray = FontDictionary.GetCOSArray(FontMatrixKey);
            if (matrixArray != null && matrixArray.Size() >= 6)
            {
                _fontMatrix = new Matrix(
                    matrixArray.GetObject(0) is COSNumber m0 ? m0.FloatValue() : 0.001f,
                    matrixArray.GetObject(1) is COSNumber m1 ? m1.FloatValue() : 0f,
                    matrixArray.GetObject(2) is COSNumber m2 ? m2.FloatValue() : 0f,
                    matrixArray.GetObject(3) is COSNumber m3 ? m3.FloatValue() : 0.001f,
                    matrixArray.GetObject(4) is COSNumber m4 ? m4.FloatValue() : 0f,
                    matrixArray.GetObject(5) is COSNumber m5 ? m5.FloatValue() : 0f);
            }
            else
            {
                _fontMatrix = new Matrix(0.001f, 0, 0, 0.001f, 0, 0);
            }

            return _fontMatrix;
        }

        public virtual COSDictionary GetCOSObject() => FontDictionary;

        public virtual BoundingBox GetBoundingBox() => GetFontDescriptor()?.GetFontBoundingBox() ?? new BoundingBox();

        public virtual PDFontDescriptor? GetFontDescriptor()
        {
            if (_fontDescriptor != null)
            {
                return _fontDescriptor;
            }

            if (FontDictionary.GetDictionaryObject(FontDescriptorKey) is COSDictionary descriptor)
            {
                _fontDescriptor = new PDFontDescriptor(descriptor);
            }

            return _fontDescriptor;
        }

        protected virtual string? ToUnicodeFallback(int code, GlyphList glyphList)
        {
            return null;
        }

        protected float[]? GetExplicitWidths()
        {
            return _widths;
        }

        private static float[]? ReadWidths(COSArray? widths)
        {
            if (widths is null || widths.Size() == 0)
            {
                return null;
            }

            float[] result = new float[widths.Size()];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = widths.GetObject(i) is COSNumber n ? n.FloatValue() : 0f;
            }

            return result;
        }

        private static CMap? ReadToUnicodeMap(COSBase? toUnicode)
        {
            try
            {
                if (toUnicode is COSStream stream)
                {
                    using Stream input = stream.CreateInputStream();
                    using MemoryStream buffer = new();
                    input.CopyTo(buffer);
                    RandomAccessRead randomAccess = new RandomAccessReadBuffer(buffer.ToArray());
                    return new CMapParser().Parse(randomAccess);
                }
            }
            catch
            {
                // Preserve non-throwing font access behavior for malformed or unsupported ToUnicode maps.
            }

            return null;
        }
    }

    public abstract partial class PDVectorFont : PDFont
    {
        protected PDVectorFont(COSDictionary fontDictionary)
            : base(fontDictionary)
        {
        }

        public abstract bool HasGlyph(int code);
        public abstract GeneralPath GetNormalizedPath(int code);
    }

    public abstract partial class PDSimpleFont : PDVectorFont
    {
        private readonly PdfBox.Net.PDModel.Font.Encoding.Encoding _encoding;

        protected PdfBox.Net.PDModel.Font.Encoding.Encoding FontEncoding => _encoding;

        protected PDSimpleFont(COSDictionary fontDictionary)
            : this(fontDictionary, null)
        {
        }

        protected PDSimpleFont(COSDictionary fontDictionary, PdfBox.Net.PDModel.Font.Encoding.Encoding? encoding)
            : base(fontDictionary)
        {
            _encoding = encoding ?? DictionaryEncoding.ResolveEncoding(fontDictionary);
        }

        public override string GetName()
        {
            string name = base.GetName();
            return name != "Unknown" ? name : GetFontBoxFont().GetName();
        }

        public override Matrix GetFontMatrix()
        {
            IList<float> values = GetFontBoxFont().GetFontMatrix();
            if (values.Count >= 6)
            {
                return new Matrix(values[0], values[1], values[2], values[3], values[4], values[5]);
            }

            return base.GetFontMatrix();
        }

        public override BoundingBox GetBoundingBox()
        {
            BoundingBox bbox = GetFontBoxFont().GetFontBBox();
            return bbox.GetWidth() == 0 && bbox.GetHeight() == 0 ? base.GetBoundingBox() : bbox;
        }

        public override float GetWidth(int code)
        {
            float explicitWidth = base.GetWidth(code);
            if (explicitWidth > 0)
            {
                return explicitWidth;
            }

            string glyphName = _encoding.GetName(code);
            if (glyphName == ".notdef")
            {
                return 0;
            }

            try
            {
                return GetFontBoxFont().GetWidth(glyphName);
            }
            catch
            {
                return 0;
            }
        }

        protected override string? ToUnicodeFallback(int code, GlyphList glyphList)
        {
            string glyphName = _encoding.GetName(code);
            if (glyphName == ".notdef")
            {
                return null;
            }

            return glyphList.ToUnicode(glyphName);
        }

        public override bool HasGlyph(int code)
        {
            string glyphName = _encoding.GetName(code);
            return glyphName != ".notdef" && GetFontBoxFont().HasGlyph(glyphName);
        }

        public override GeneralPath GetNormalizedPath(int code)
        {
            string glyphName = _encoding.GetName(code);
            if (glyphName == ".notdef")
            {
                return new GeneralPath();
            }

            return GetFontBoxFont().GetPath(glyphName);
        }

        public abstract FontBoxFont GetFontBoxFont();
        public abstract bool IsStandard14();
    }

    public partial class PDType1Font : PDSimpleFont
    {
        private static readonly COSName FontDescriptorKey = COSName.GetPDFName("FontDescriptor");
        private static readonly COSName FontFileKey = COSName.GetPDFName("FontFile");
        private static readonly COSName BaseFontKey = COSName.GetPDFName("BaseFont");

        private readonly Type1Font? _type1Font;
        private readonly FontBoxFont _fontBoxFont;
        private readonly bool _isStandard14;

        public PDType1Font(COSDictionary dictionary, Type1Font? type1Font = null, FontBoxFont? fontBoxFont = null)
            : base(dictionary, ResolveType1Encoding(dictionary, type1Font))
        {
            _type1Font = type1Font;
            _isStandard14 = Standard14Fonts.IsStandard14Font(GetName());
            _fontBoxFont = fontBoxFont ?? type1Font as FontBoxFont ?? new Standard14PlaceholderFont(GetName());
        }

        internal static PDType1Font Load(COSDictionary dictionary)
        {
            Type1Font? font = null;
            FontBoxFont? fontBoxFont = null;
            try
            {
                if (dictionary.GetDictionaryObject(FontDescriptorKey) is COSDictionary descriptor &&
                    descriptor.GetDictionaryObject(FontFileKey) is COSStream fontFile)
                {
                    using Stream stream = fontFile.CreateInputStream();
                    using MemoryStream buffer = new();
                    stream.CopyTo(buffer);
                    byte[] bytes = buffer.ToArray();
                    font = TryCreateType1Font(bytes, fontFile);
                    fontBoxFont = font;
                }
            }
            catch
            {
                // Keep dictionary-driven fallback behavior.
            }

            if (fontBoxFont == null)
            {
                fontBoxFont = TryLoadMappedFont(dictionary.GetNameAsString(BaseFontKey));
            }

            return new PDType1Font(dictionary, font, fontBoxFont);
        }

        public override FontBoxFont GetFontBoxFont() => _fontBoxFont;
        public override bool IsStandard14() => _isStandard14;

        private static PdfBox.Net.PDModel.Font.Encoding.Encoding ResolveType1Encoding(COSDictionary dictionary, Type1Font? type1Font)
        {
            if (dictionary.GetDictionaryObject(COSName.GetPDFName("Encoding")) is not null)
            {
                return DictionaryEncoding.ResolveEncoding(dictionary);
            }

            if (type1Font != null)
            {
                return new Type1Encoding(type1Font);
            }

            return DictionaryEncoding.ResolveStandard14FallbackEncoding(dictionary.GetNameAsString(BaseFontKey));
        }

        private static Type1Font? TryCreateType1Font(byte[] bytes, COSStream fontFile)
        {
            if (bytes.Length == 0)
            {
                return null;
            }

            if (bytes[0] == 0x80)
            {
                return Type1Font.CreateWithPFB(bytes);
            }

            int length1 = fontFile.GetInt(COSName.GetPDFName("Length1"), -1);
            int length2 = fontFile.GetInt(COSName.GetPDFName("Length2"), -1);
            if (length1 > 0 && length2 > 0 && length1 + length2 <= bytes.Length)
            {
                byte[] segment1 = bytes[..length1];
                byte[] segment2 = bytes[length1..(length1 + length2)];
                return Type1Font.CreateWithSegments(segment1, segment2);
            }

            return null;
        }

        private static FontBoxFont? TryLoadMappedFont(string? fontName)
        {
            string mappedName = Standard14Fonts.GetMappedFontName(fontName) ?? fontName ?? string.Empty;
            if (string.IsNullOrWhiteSpace(mappedName))
            {
                return null;
            }

            string? fontPath = FontMappers.Instance.FindFontFile(mappedName);
            if (string.IsNullOrWhiteSpace(fontPath) || !File.Exists(fontPath))
            {
                return null;
            }

            try
            {
                string extension = Path.GetExtension(fontPath);
                if (extension.Equals(".pfb", StringComparison.OrdinalIgnoreCase))
                {
                    byte[] bytes = File.ReadAllBytes(fontPath);
                    return Type1Font.CreateWithPFB(bytes);
                }

                if (extension.Equals(".ttf", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".otf", StringComparison.OrdinalIgnoreCase))
                {
                    byte[] bytes = File.ReadAllBytes(fontPath);
                    return new TTFParser().Parse(bytes);
                }
            }
            catch
            {
                // Keep dictionary-driven fallback behavior.
            }

            return null;
        }

        private sealed class Standard14PlaceholderFont(string fontName) : FontBoxFont
        {
            public string GetName() => fontName;
            public BoundingBox GetFontBBox() => new BoundingBox(0, -250, 1000, 900);
            public IList<float> GetFontMatrix() => [0.001f, 0, 0, 0.001f, 0, 0];
            public GeneralPath GetPath(string name) => new GeneralPath();
            public float GetWidth(string name) => name == "space" ? 20f : 40f;
            public bool HasGlyph(string name) => true;
        }
    }

    public partial class PDTrueTypeFont : PDSimpleFont
    {
        private static readonly COSName FontDescriptorKey = COSName.GetPDFName("FontDescriptor");
        private static readonly COSName FontFile2Key = COSName.GetPDFName("FontFile2");

        private readonly TrueTypeFont _trueTypeFont;
        private readonly CmapLookup? _unicodeCmap;

        public PDTrueTypeFont(COSDictionary dictionary, TrueTypeFont trueTypeFont)
            : base(dictionary)
        {
            _trueTypeFont = trueTypeFont ?? throw new ArgumentNullException(nameof(trueTypeFont));
            _unicodeCmap = _trueTypeFont.GetUnicodeCmapLookup(false);
        }

        public PDTrueTypeFont(TrueTypeFont trueTypeFont)
            : this(new COSDictionary(), trueTypeFont)
        {
        }

        internal static PDTrueTypeFont? Load(COSDictionary dictionary)
        {
            try
            {
                if (dictionary.GetDictionaryObject(FontDescriptorKey) is COSDictionary descriptor &&
                    descriptor.GetDictionaryObject(FontFile2Key) is COSStream fontFile)
                {
                    using Stream stream = fontFile.CreateInputStream();
                    TrueTypeFont ttf = new TTFParser(isEmbedded: true).ParseEmbedded(stream);
                    return new PDTrueTypeFont(dictionary, ttf);
                }
            }
            catch
            {
                // Keep non-throwing font factory behavior.
            }

            return null;
        }

        public override FontBoxFont GetFontBoxFont() => _trueTypeFont;
        public override bool IsStandard14() => false;

        public TrueTypeFont GetTrueTypeFont() => _trueTypeFont;

        protected override string? ToUnicodeFallback(int code, GlyphList glyphList)
        {
            if (_unicodeCmap != null && _unicodeCmap.GetGlyphId(code) != 0)
            {
                return char.ConvertFromUtf32(code);
            }

            return base.ToUnicodeFallback(code, glyphList);
        }
    }

    public partial class PDType0Font : PDVectorFont
    {
        private static readonly COSName EncodingKey = COSName.GetPDFName("Encoding");
        private static readonly COSName CidSystemInfoKey = COSName.GetPDFName("CIDSystemInfo");

        private readonly PDCIDFont? _descendantFont;
        private readonly CMap? _cMap;
        private readonly CMap? _cMapUcs2;
        private readonly bool _isCMapPredefined;
        private readonly bool _isDescendantCjk;

        public PDType0Font(COSDictionary dictionary, PDCIDFont? descendantFont)
            : base(dictionary)
        {
            _descendantFont = descendantFont;
            (_cMap, _isCMapPredefined) = ReadEncodingCMap(dictionary);
            _isDescendantCjk = IsDescendantCjk(descendantFont);
            _cMapUcs2 = ReadUcs2CMap(dictionary, _cMap, _isCMapPredefined, _isDescendantCjk);
        }

        internal static PDType0Font Load(COSDictionary dictionary)
        {
            PDCIDFont? descendant = null;
            COSArray? descendants = dictionary.GetCOSArray(COSName.GetPDFName("DescendantFonts"));
            if (descendants != null && descendants.Size() > 0 && descendants.GetObject(0) is COSDictionary descendantDict)
            {
                descendant = PDFontFactory.CreateDescendantFont(descendantDict);
            }

            return new PDType0Font(dictionary, descendant);
        }

        public int CodeToCID(int code)
        {
            if (_cMap != null)
            {
                return _cMap.ToCID(code);
            }

            return _descendantFont?.CodeToCID(code) ?? code;
        }
        public PDCIDFont? GetDescendantFont() => _descendantFont;
        public CMap? GetCMap() => _cMap;
        public CMap? GetCMapUCS2() => _cMapUcs2;

        public override bool IsVertical() => _descendantFont?.IsVertical() ?? base.IsVertical();
        public override float GetWidth(int code) => _descendantFont?.GetWidth(CodeToCID(code)) ?? base.GetWidth(code);
        public override float GetAverageFontWidth() => _descendantFont?.GetAverageFontWidth() ?? base.GetAverageFontWidth();
        public override float GetSpaceWidth() => _descendantFont?.GetSpaceWidth() ?? base.GetSpaceWidth();
        public override PDFontDescriptor? GetFontDescriptor() => base.GetFontDescriptor() ?? _descendantFont?.GetFontDescriptor();
        public override string? ToUnicode(int code, GlyphList glyphList)
        {
            string? unicode = base.ToUnicode(code, glyphList);
            if (!string.IsNullOrEmpty(unicode))
            {
                return unicode;
            }

            if ((_isCMapPredefined || _isDescendantCjk) && _cMapUcs2 != null)
            {
                int cid = CodeToCID(code);
                string? mapped = _cMapUcs2.ToUnicode(cid);
                if (!string.IsNullOrEmpty(mapped))
                {
                    return mapped;
                }
            }

            return _descendantFont?.ToUnicode(CodeToCID(code), glyphList);
        }

        public override BoundingBox GetBoundingBox()
        {
            BoundingBox bbox = base.GetBoundingBox();
            return bbox.GetWidth() == 0 && bbox.GetHeight() == 0
                ? _descendantFont?.GetBoundingBox() ?? bbox
                : bbox;
        }

        public override bool HasGlyph(int code)
        {
            if (_descendantFont is PDCIDFontType2 type2)
            {
                int gid = type2.CodeToGID(CodeToCID(code));
                return type2.GetTrueTypeFont().GetGlyph()?.GetGlyph(gid) != null;
            }

            return false;
        }

        public override GeneralPath GetNormalizedPath(int code)
        {
            if (_descendantFont is PDCIDFontType2 type2)
            {
                int gid = type2.CodeToGID(CodeToCID(code));
                return type2.GetTrueTypeFont().GetGlyph()?.GetGlyph(gid)?.GetPath() ?? new GeneralPath();
            }

            return new GeneralPath();
        }

        private static (CMap? CMap, bool IsPredefined) ReadEncodingCMap(COSDictionary dictionary)
        {
            COSBase? encoding = dictionary.GetDictionaryObject(EncodingKey);
            if (encoding is COSName encodingName)
            {
                CMap? predefined = TryParsePredefinedCMap(encodingName.GetName());
                if (predefined == null &&
                    (string.Equals(encodingName.GetName(), "Identity-H", StringComparison.Ordinal) ||
                     string.Equals(encodingName.GetName(), "Identity-V", StringComparison.Ordinal)))
                {
                    predefined = CreateIdentityCMap();
                }

                return (predefined, predefined != null);
            }

            if (encoding is not null)
            {
                return (ReadCMap(encoding), false);
            }

            return (null, false);
        }

        private static CMap? ReadUcs2CMap(COSDictionary dictionary, CMap? cMap, bool isCMapPredefined, bool isDescendantCjk)
        {
            string? baseName = null;
            if (isDescendantCjk && dictionary.GetCOSArray(COSName.GetPDFName("DescendantFonts"))?.GetObject(0) is COSDictionary descendantDict)
            {
                if (descendantDict.GetDictionaryObject(CidSystemInfoKey) is COSDictionary cidSystemInfoDict)
                {
                    PDCIDSystemInfo cidSystemInfo = new(cidSystemInfoDict);
                    baseName = $"{cidSystemInfo.Registry}-{cidSystemInfo.Ordering}-{cidSystemInfo.Supplement}";
                }
            }
            else if (isCMapPredefined && dictionary.GetDictionaryObject(EncodingKey) is COSName encodingName)
            {
                baseName = encodingName.GetName();
            }
            else if (isDescendantCjk && cMap != null && !string.IsNullOrWhiteSpace(cMap.Name))
            {
                baseName = cMap.Name;
            }

            if (string.IsNullOrWhiteSpace(baseName))
            {
                return null;
            }

            CMap? predefined = TryParsePredefinedCMap(baseName);
            if (predefined is null || string.IsNullOrWhiteSpace(predefined.Registry) || string.IsNullOrWhiteSpace(predefined.Ordering))
            {
                return null;
            }

            string ucs2Name = $"{predefined.Registry}-{predefined.Ordering}-UCS2";
            return TryParsePredefinedCMap(ucs2Name);
        }

        private static bool IsDescendantCjk(PDCIDFont? descendantFont)
        {
            PDCIDSystemInfo? ros = descendantFont?.GetCIDSystemInfo();
            if (ros == null || !string.Equals(ros.Registry, "Adobe", StringComparison.Ordinal))
            {
                return false;
            }

            return string.Equals(ros.Ordering, "GB1", StringComparison.Ordinal) ||
                   string.Equals(ros.Ordering, "CNS1", StringComparison.Ordinal) ||
                   string.Equals(ros.Ordering, "Japan1", StringComparison.Ordinal) ||
                   string.Equals(ros.Ordering, "Korea1", StringComparison.Ordinal);
        }

        private static CMap? TryParsePredefinedCMap(string name)
        {
            try
            {
                return new CMapParser().ParsePredefined(name);
            }
            catch
            {
                return null;
            }
        }

        private static CMap CreateIdentityCMap()
        {
            const string identityMap = """
/CIDInit /ProcSet findresource begin
12 dict begin
begincmap
/CIDSystemInfo << /Registry (Adobe) /Ordering (Identity) /Supplement 0 >> def
/CMapName /Identity-H def
/CMapType 1 def
1 begincodespacerange
<0000> <FFFF>
endcodespacerange
1 begincidrange
<0000> <FFFF> 0
endcidrange
endcmap
CMapName currentdict /CMap defineresource pop
end
end
""";

            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(identityMap);
            using RandomAccessRead randomAccess = new RandomAccessReadBuffer(bytes);
            return new CMapParser().Parse(randomAccess);
        }

        private static CMap? ReadCMap(COSBase encoding)
        {
            if (encoding is not COSStream stream)
            {
                return null;
            }

            try
            {
                using Stream input = stream.CreateInputStream();
                using MemoryStream buffer = new();
                input.CopyTo(buffer);
                using RandomAccessRead randomAccess = new RandomAccessReadBuffer(buffer.ToArray());
                return new CMapParser().Parse(randomAccess);
            }
            catch
            {
                return null;
            }
        }
    }

    public abstract partial class PDCIDFont : PDFont
    {
        private static readonly COSName WidthsKey = COSName.GetPDFName("Widths");
        private static readonly COSName WKey = COSName.GetPDFName("W");
        private static readonly COSName DWKey = COSName.GetPDFName("DW");
        private static readonly COSName CidSystemInfoKey = COSName.GetPDFName("CIDSystemInfo");

        private readonly Dictionary<int, float> _widthsByCid = [];
        protected readonly float DefaultWidth;

        protected PDCIDFont(COSDictionary fontDictionary)
            : base(fontDictionary)
        {
            DefaultWidth = fontDictionary.GetFloat(DWKey, 1000f);
            ReadCIDWidths(fontDictionary.GetCOSArray(WKey), _widthsByCid);
        }

        public virtual int CodeToCID(int code) => code;
        public virtual PDCIDSystemInfo? GetCIDSystemInfo()
        {
            return FontDictionary.GetDictionaryObject(CidSystemInfoKey) is COSDictionary dict
                ? new PDCIDSystemInfo(dict)
                : null;
        }

        /// <summary>Returns true when the CID widths dictionary (W array) contains an explicit entry for the given CID.</summary>
        protected bool HasExplicitCidWidth(int cid) => _widthsByCid.ContainsKey(cid);

        public override float GetWidth(int code)
        {
            if (_widthsByCid.TryGetValue(code, out float width))
            {
                return width;
            }

            if (FontDictionary.GetCOSArray(WidthsKey) != null)
            {
                float baseWidth = base.GetWidth(code);
                if (baseWidth > 0)
                {
                    return baseWidth;
                }
            }

            return DefaultWidth;
        }

        private static void ReadCIDWidths(COSArray? widths, Dictionary<int, float> widthsByCid)
        {
            if (widths == null)
            {
                return;
            }

            int index = 0;
            while (index < widths.Size())
            {
                if (widths.GetObject(index) is not COSNumber startNumber)
                {
                    index++;
                    continue;
                }

                int startCid = startNumber.IntValue();
                index++;
                if (index >= widths.Size())
                {
                    break;
                }

                if (widths.GetObject(index) is COSArray rangeWidths)
                {
                    for (int offset = 0; offset < rangeWidths.Size(); offset++)
                    {
                        if (rangeWidths.GetObject(offset) is COSNumber widthNumber)
                        {
                            widthsByCid[startCid + offset] = widthNumber.FloatValue();
                        }
                    }

                    index++;
                    continue;
                }

                if (widths.GetObject(index) is COSNumber endNumber &&
                    index + 1 < widths.Size() &&
                    widths.GetObject(index + 1) is COSNumber widthNumberForRange)
                {
                    int endCid = endNumber.IntValue();
                    float width = widthNumberForRange.FloatValue();
                    for (int cid = startCid; cid <= endCid; cid++)
                    {
                        widthsByCid[cid] = width;
                    }

                    index += 2;
                    continue;
                }

                index++;
            }
        }
    }

    public partial class PDCIDFontType0 : PDCIDFont
    {
        public PDCIDFontType0(COSDictionary dictionary)
            : base(dictionary)
        {
        }
    }

    public partial class PDCIDFontType2 : PDCIDFont
    {
        private static readonly COSName FontDescriptorKey = COSName.GetPDFName("FontDescriptor");
        private static readonly COSName FontFile2Key = COSName.GetPDFName("FontFile2");
        private static readonly COSName CIDToGIDMapKey = COSName.GetPDFName("CIDToGIDMap");

        private readonly TrueTypeFont _trueTypeFont;
        private readonly CmapLookup? _unicodeCmap;
        private readonly int[]? _cidToGid;

        public PDCIDFontType2(COSDictionary dictionary, TrueTypeFont? trueTypeFont = null)
            : base(dictionary)
        {
            _trueTypeFont = trueTypeFont ?? new TrueTypeFont();
            _unicodeCmap = _trueTypeFont.GetUnicodeCmapLookup(false);
            _cidToGid = ReadCidToGidMap(dictionary);
        }

        internal static PDCIDFontType2 Load(COSDictionary dictionary)
        {
            TrueTypeFont? ttf = null;
            try
            {
                if (dictionary.GetDictionaryObject(FontDescriptorKey) is COSDictionary descriptor &&
                    descriptor.GetDictionaryObject(FontFile2Key) is COSStream fontFile)
                {
                    using Stream stream = fontFile.CreateInputStream();
                    ttf = new TTFParser(isEmbedded: true).ParseEmbedded(stream);
                }
            }
            catch
            {
                // Keep non-throwing CID font construction behavior.
            }

            return new PDCIDFontType2(dictionary, ttf);
        }

        public override bool IsVertical()
        {
            return _trueTypeFont.GetVerticalHeader() != null && _trueTypeFont.GetVerticalMetrics() != null;
        }

        public TrueTypeFont GetTrueTypeFont() => _trueTypeFont;

        /// <summary>
        /// Maps a CID to a glyph ID (GID) using the CIDToGIDMap entry.
        /// When the CIDToGIDMap is "Identity" or absent, returns the CID as the GID.
        /// </summary>
        public int CodeToGID(int cid)
        {
            if (_cidToGid != null)
            {
                return (cid >= 0 && cid < _cidToGid.Length) ? _cidToGid[cid] : 0;
            }

            return cid;
        }

        /// <summary>
        /// Returns the ToUnicode string for the given character code via the TTF unicode cmap
        /// when no explicit ToUnicode CMap is present in the font dictionary.
        /// </summary>
        protected override string? ToUnicodeFallback(int code, GlyphList glyphList)
        {
            if (_unicodeCmap != null)
            {
                List<int>? charCodes = _unicodeCmap.GetCharCodes(CodeToGID(code));
                if (charCodes != null && charCodes.Count > 0)
                {
                    return char.ConvertFromUtf32(charCodes[0]);
                }
            }

            return base.ToUnicodeFallback(code, glyphList);
        }

        /// <summary>
        /// Returns the width for the given character code. Explicit W/DW dictionary entries take
        /// precedence; when absent, falls back to the TTF advance width scaled to PDF text units.
        /// </summary>
        public override float GetWidth(int code)
        {
            if (HasExplicitCidWidth(code))
            {
                return base.GetWidth(code);
            }

            // Fall back to TTF advance width scaled to 1000-unit PDF space.
            int gid = CodeToGID(code);
            int unitsPerEm = _trueTypeFont.GetUnitsPerEm();
            return _trueTypeFont.GetAdvanceWidth(gid) * 1000f / unitsPerEm;
        }

        private static int[]? ReadCidToGidMap(COSDictionary dictionary)
        {
            COSBase? entry = dictionary.GetDictionaryObject(CIDToGIDMapKey);
            if (entry is COSName name && string.Equals(name.GetName(), "Identity", StringComparison.Ordinal))
            {
                // Identity: CID == GID, no explicit mapping needed.
                return null;
            }

            if (entry is COSStream stream)
            {
                try
                {
                    using Stream input = stream.CreateInputStream();
                    using MemoryStream buffer = new();
                    input.CopyTo(buffer);
                    byte[] bytes = buffer.ToArray();
                    int count = bytes.Length / 2;
                    int[] map = new int[count];
                    for (int i = 0; i < count; i++)
                    {
                        map[i] = (bytes[i * 2] << 8) | bytes[i * 2 + 1];
                    }

                    return map;
                }
                catch
                {
                    // Fall back to identity mapping.
                }
            }

            return null;
        }
    }

    public partial class PDFontDescriptor
    {
        private const int FlagFixedPitch = 1;
        private const int FlagSerif = 2;
        private const int FlagSymbolic = 4;
        private const int FlagScript = 8;
        private const int FlagNonSymbolic = 32;
        private const int FlagItalic = 64;
        private const int FlagAllCap = 65536;
        private const int FlagSmallCap = 131072;
        private const int FlagForceBold = 262144;

        private static readonly COSName FontNameKey = COSName.GetPDFName("FontName");
        private static readonly COSName FontFamilyKey = COSName.GetPDFName("FontFamily");
        private static readonly COSName FontWeightKey = COSName.GetPDFName("FontWeight");
        private static readonly COSName FontStretchKey = COSName.GetPDFName("FontStretch");
        private static readonly COSName CapHeightKey = COSName.GetPDFName("CapHeight");
        private static readonly COSName AscentKey = COSName.GetPDFName("Ascent");
        private static readonly COSName DescentKey = COSName.GetPDFName("Descent");
        private static readonly COSName LeadingKey = COSName.GetPDFName("Leading");
        private static readonly COSName StemVKey = COSName.GetPDFName("StemV");
        private static readonly COSName StemHKey = COSName.GetPDFName("StemH");
        private static readonly COSName MissingWidthKey = COSName.GetPDFName("MissingWidth");
        private static readonly COSName AverageWidthKey = COSName.GetPDFName("AvgWidth");
        private static readonly COSName MaxWidthKey = COSName.GetPDFName("MaxWidth");
        private static readonly COSName FontBBoxKey = COSName.GetPDFName("FontBBox");
        private static readonly COSName XHeightKey = COSName.GetPDFName("XHeight");
        private static readonly COSName ItalicAngleKey = COSName.GetPDFName("ItalicAngle");
        private static readonly COSName FlagsKey = COSName.GetPDFName("Flags");
        private static readonly COSName CharSetKey = COSName.GetPDFName("CharSet");
        private static readonly COSName StyleKey = COSName.GetPDFName("Style");
        private static readonly COSName PanoseKey = COSName.GetPDFName("Panose");

        private readonly COSDictionary _dictionary;
        private float _xHeight = float.NegativeInfinity;
        private float _capHeight = float.NegativeInfinity;
        private int _flags = -1;

        public PDFontDescriptor()
            : this(new COSDictionary())
        {
        }

        public PDFontDescriptor(COSDictionary dictionary)
        {
            _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
        }

        public COSDictionary GetCOSObject() => _dictionary;

        public string? GetFontName() => _dictionary.GetNameAsString(FontNameKey);
        public string? GetFontFamily() => _dictionary.GetString(FontFamilyKey);
        public float GetFontWeight() => _dictionary.GetFloat(FontWeightKey, 0f);
        public string? GetFontStretch() => _dictionary.GetNameAsString(FontStretchKey);
        public int GetFlags() => _flags == -1 ? _flags = _dictionary.GetInt(FlagsKey, 0) : _flags;
        public float GetAscent() => _dictionary.GetFloat(AscentKey, 0f);
        public float GetDescent() => _dictionary.GetFloat(DescentKey, 0f);
        public float GetLeading() => _dictionary.GetFloat(LeadingKey, 0f);
        public float GetStemV() => _dictionary.GetFloat(StemVKey, 0f);
        public float GetStemH() => _dictionary.GetFloat(StemHKey, 0f);
        public float GetMissingWidth() => _dictionary.GetFloat(MissingWidthKey, 0f);
        public float GetAverageWidth() => _dictionary.GetFloat(AverageWidthKey, 0f);
        public float GetMaxWidth() => _dictionary.GetFloat(MaxWidthKey, 0f);
        public float GetItalicAngle() => _dictionary.GetFloat(ItalicAngleKey, 0f);
        public string? GetCharSet() => _dictionary.GetString(CharSetKey);

        public float GetCapHeight()
        {
            if (float.IsNegativeInfinity(_capHeight))
            {
                _capHeight = Math.Abs(_dictionary.GetFloat(CapHeightKey, 0f));
            }

            return _capHeight;
        }

        public float GetXHeight()
        {
            if (float.IsNegativeInfinity(_xHeight))
            {
                _xHeight = Math.Abs(_dictionary.GetFloat(XHeightKey, 0f));
            }

            return _xHeight;
        }

        public bool HasWidths() => _dictionary.ContainsKey(AverageWidthKey) || _dictionary.ContainsKey(MissingWidthKey);
        public bool HasMissingWidth() => _dictionary.ContainsKey(MissingWidthKey);
        public bool IsFixedPitch() => IsFlagBitOn(FlagFixedPitch);
        public bool IsSerif() => IsFlagBitOn(FlagSerif);
        public bool IsSymbolic() => IsFlagBitOn(FlagSymbolic);
        public bool IsScript() => IsFlagBitOn(FlagScript);
        public bool IsNonSymbolic() => IsFlagBitOn(FlagNonSymbolic);
        public bool IsItalic() => IsFlagBitOn(FlagItalic);
        public bool IsAllCap() => IsFlagBitOn(FlagAllCap);
        public bool IsSmallCap() => IsFlagBitOn(FlagSmallCap);
        public bool IsForceBold() => IsFlagBitOn(FlagForceBold);
        public PDRectangle? GetFontBoundingBoxRectangle()
        {
            COSArray? bbox = _dictionary.GetCOSArray(FontBBoxKey);
            return bbox == null ? null : new PDRectangle(bbox);
        }

        public BoundingBox GetFontBoundingBox()
        {
            PDRectangle? bbox = GetFontBoundingBoxRectangle();
            if (bbox == null)
            {
                return new BoundingBox();
            }

            return new BoundingBox(bbox.GetLowerLeftX(), bbox.GetLowerLeftY(), bbox.GetUpperRightX(), bbox.GetUpperRightY());
        }

        public PDPanose? GetPanose()
        {
            COSDictionary? style = _dictionary.GetCOSDictionary(StyleKey);
            if (style?.GetDictionaryObject(PanoseKey) is COSString panose)
            {
                byte[] bytes = panose.GetBytes();
                if (bytes.Length >= PDPanose.PanoseLength)
                {
                    return new PDPanose(bytes);
                }
            }

            return null;
        }

        private bool IsFlagBitOn(int bit)
        {
            return (GetFlags() & bit) != 0;
        }
    }
}
