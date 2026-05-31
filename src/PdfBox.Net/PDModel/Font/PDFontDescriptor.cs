/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDFontDescriptor.java
 * PDFBOX_SOURCE_COMMIT: 977c0fe181d112446967becaf143764e8d9dea8a
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 977c0fe181d112446967becaf143764e8d9dea8a
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
using PdfBox.Net.FontBox.Util;
using PdfBox.Net.PDModel.Common;

namespace PdfBox.Net.PDModel.Font;

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
