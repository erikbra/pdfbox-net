/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDSimpleFont.java
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
using PdfBox.Net.FontBox.Util;
using PdfBox.Net.PDModel.Font.Encoding;
using PdfBox.Net.Util;
using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.PDModel.Font;

public abstract partial class PDSimpleFont : PDVectorFont
{
    private static readonly COSName BaseFontKey = COSName.GetPDFName("BaseFont");
    private static readonly COSName FontDescriptorKey = COSName.GetPDFName("FontDescriptor");

    private readonly Encoding.Encoding _encoding;
    private readonly GlyphList _glyphList;

    protected Encoding.Encoding FontEncoding => _encoding;

    protected PDSimpleFont(COSDictionary fontDictionary)
        : this(fontDictionary, null)
    {
    }

    protected PDSimpleFont(COSDictionary fontDictionary, Encoding.Encoding? encoding)
        : base(fontDictionary)
    {
        _encoding = encoding ?? DictionaryEncoding.ResolveEncoding(fontDictionary);
        _glyphList = Standard14Fonts.GetMappedFontName(fontDictionary.GetNameAsString(BaseFontKey)) == "ZapfDingbats"
            ? GlyphList.GetZapfDingbats()
            : GlyphList.GetAdobeGlyphList();
    }

    public override string GetName()
    {
        string name = base.GetName();
        return name != "Unknown" ? name : GetFontBoxFont()?.GetName() ?? "Unknown";
    }

    public override Matrix GetFontMatrix()
    {
        FontBoxFont? fbFont = GetFontBoxFont();
        if (fbFont != null)
        {
            IList<float> values = fbFont.GetFontMatrix();
            if (values.Count >= 6)
            {
                return new Matrix(values[0], values[1], values[2], values[3], values[4], values[5]);
            }
        }

        return base.GetFontMatrix();
    }

    public override BoundingBox GetBoundingBox()
    {
        if (IsStandard14() && Standard14Fonts.GetAFM(GetName())?.FontBBox is { } afmBBox
            && (afmBBox.GetWidth() != 0 || afmBBox.GetHeight() != 0))
        {
            return afmBBox;
        }

        FontBoxFont? fbFont = GetFontBoxFont();
        if (fbFont != null)
        {
            BoundingBox bbox = fbFont.GetFontBBox();
            if (bbox.GetWidth() != 0 || bbox.GetHeight() != 0)
            {
                return bbox;
            }
        }

        return base.GetBoundingBox();
    }

    public override float GetWidth(int code)
    {
        if (TryGetExplicitWidth(code, out float explicitWidth))
        {
            return explicitWidth;
        }

        string glyphName = _encoding.GetName(code);
        if (IsStandard14())
        {
            return GetStandard14Width(glyphName);
        }

        if (glyphName == ".notdef")
        {
            return 0;
        }

        try
        {
            return GetFontBoxFont()?.GetWidth(glyphName) ?? 0f;
        }
        catch
        {
            return 0;
        }
    }

    public override float GetStringWidth(string text)
    {
        byte[] encoded = Encode(text);
        float width = 0f;
        foreach (byte code in encoded)
        {
            width += GetWidth(code);
        }

        return width;
    }

    protected override byte[] Encode(int unicode)
    {
        string glyphName = _glyphList.CodePointToName(unicode);
        int? code = _encoding.GetCode(glyphName);
        if (glyphName == ".notdef" || code is null || code < byte.MinValue || code > byte.MaxValue)
        {
            throw new ArgumentException(
                $"U+{unicode:X4} ('{glyphName}') is not available in font {GetName()}, encoding: {_encoding.GetEncodingName()}.",
                nameof(unicode));
        }

        return [(byte)code.Value];
    }

    public override float GetWidthFromFont(int code)
    {
        string glyphName = _encoding.GetName(code);
        if (glyphName == ".notdef")
        {
            return 0f;
        }

        return GetFontBoxFont()?.GetWidth(glyphName) ?? 0f;
    }

    private float GetStandard14Width(string glyphName)
    {
        if (glyphName == ".notdef")
        {
            return 250f;
        }

        string afmName = glyphName switch
        {
            "nbspace" => "space",
            "sfthyphen" => "hyphen",
            _ => glyphName,
        };

        return Standard14Fonts.GetAFM(GetName())?.GetCharacterWidth(afmName) ?? 0f;
    }

    protected override string? ToUnicodeFallback(int code, GlyphList glyphList)
    {
        string glyphName = _encoding.GetName(code);
        if (glyphName == ".notdef")
        {
            return null;
        }

        GlyphList unicodeGlyphList = ReferenceEquals(_glyphList, GlyphList.GetAdobeGlyphList())
            ? glyphList
            : _glyphList;
        return unicodeGlyphList.ToUnicode(glyphName);
    }

    public override bool HasGlyph(int code)
    {
        string glyphName = _encoding.GetName(code);
        return glyphName != ".notdef" && (GetFontBoxFont()?.HasGlyph(glyphName) ?? false);
    }

    public virtual bool HasGlyph(string name)
    {
        return !string.IsNullOrEmpty(name) && name != ".notdef" && (GetFontBoxFont()?.HasGlyph(name) ?? false);
    }

    public string CodeToName(int code) => _encoding.GetName(code);

    public override GeneralPath GetNormalizedPath(int code)
    {
        string glyphName = _encoding.GetName(code);
        if (glyphName == ".notdef")
        {
            return new GeneralPath();
        }

        return GetFontBoxFont()?.GetPath(glyphName) ?? new GeneralPath();
    }

    public override GeneralPath GetPath(int code) => GetNormalizedPath(code);

    public virtual GeneralPath GetPath(string name)
    {
        return string.IsNullOrEmpty(name) || name == ".notdef"
            ? new GeneralPath()
            : GetFontBoxFont()?.GetPath(name) ?? new GeneralPath();
    }

    public abstract FontBoxFont? GetFontBoxFont();
    public abstract override bool IsStandard14();

    protected static bool? GetSymbolicFlag(COSDictionary fontDictionary)
    {
        return fontDictionary.GetCOSDictionary(FontDescriptorKey) is COSDictionary descriptorDictionary
            ? new PDFontDescriptor(descriptorDictionary).IsSymbolic()
            : null;
    }
}
