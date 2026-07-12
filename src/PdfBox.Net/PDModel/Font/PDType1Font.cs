/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDType1Font.java
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
using PdfBox.Net.FontBox.Pfb;
using PdfBox.Net.FontBox.TTF;
using PdfBox.Net.FontBox.Type1;
using PdfBox.Net.PDModel.Font.Encoding;
using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.PDModel.Font;

public partial class PDType1Font : PDSimpleFont
{
    public enum FontName
    {
        TIMES_ROMAN,
        TIMES_BOLD,
        TIMES_ITALIC,
        TIMES_BOLD_ITALIC,
        HELVETICA,
        HELVETICA_BOLD,
        HELVETICA_OBLIQUE,
        HELVETICA_BOLD_OBLIQUE,
        COURIER,
        COURIER_BOLD,
        COURIER_OBLIQUE,
        COURIER_BOLD_OBLIQUE,
        SYMBOL,
        ZAPF_DINGBATS,
    }

    private static readonly COSName FontDescriptorKey = COSName.GetPDFName("FontDescriptor");
    private static readonly COSName FontFileKey = COSName.GetPDFName("FontFile");
    private static readonly COSName BaseFontKey = COSName.GetPDFName("BaseFont");
    private static readonly COSName FirstCharKey = COSName.GetPDFName("FirstChar");
    private static readonly COSName LastCharKey = COSName.GetPDFName("LastChar");
    private static readonly COSName WidthsKey = COSName.GetPDFName("Widths");
    private static readonly COSName TypeKey = COSName.GetPDFName("Type");
    private static readonly COSName FontBBoxKey = COSName.GetPDFName("FontBBox");
    private static readonly COSName FlagsKey = COSName.GetPDFName("Flags");
    private static readonly COSName AscentKey = COSName.GetPDFName("Ascent");
    private static readonly COSName DescentKey = COSName.GetPDFName("Descent");
    private static readonly COSName CapHeightKey = COSName.GetPDFName("CapHeight");
    private static readonly COSName ItalicAngleKey = COSName.GetPDFName("ItalicAngle");
    private static readonly COSName StemVKey = COSName.GetPDFName("StemV");
    private static readonly COSName FontNameKey = COSName.GetPDFName("FontName");
    private static readonly COSName FontFamilyKey = COSName.GetPDFName("FontFamily");

    private readonly Type1Font? _type1Font;
    private readonly FontBoxFont? _fontBoxFont;
    private readonly bool _isStandard14;

    public PDType1Font(FontName baseFont)
        : this(CreateStandard14Dictionary(ToStandard14Name(baseFont)))
    {
    }

    public PDType1Font(COSDictionary dictionary, Type1Font? type1Font = null, FontBoxFont? fontBoxFont = null)
        : base(dictionary, ResolveType1Encoding(dictionary, type1Font))
    {
        _type1Font = type1Font;
        _isStandard14 = Standard14Fonts.IsStandard14Font(GetName());
        _fontBoxFont = fontBoxFont ?? type1Font as FontBoxFont ?? TryLoadMappedFont(dictionary.GetNameAsString(BaseFontKey));
    }

    public PDType1Font(PDDocument document, Stream pfbStream)
        : this(CreateEmbeddedType1FontData(document, pfbStream))
    {
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

        return new PDType1Font(dictionary, font, fontBoxFont);
    }

    public override FontBoxFont? GetFontBoxFont() => _fontBoxFont;
    public override bool IsStandard14() => _isStandard14;
    public override bool IsEmbedded() => _type1Font is not null;
    public override bool IsDamaged() => false;
    public string GetBaseFont() => GetName();
    public Type1Font? GetType1Font() => _type1Font;
    public override GeneralPath GetPath(int code) => base.GetPath(code);
    public override GeneralPath GetPath(string name) => base.GetPath(name);

    private PDType1Font(EmbeddedType1FontData embeddedFontData)
        : this(embeddedFontData.Dictionary, embeddedFontData.Type1Font, embeddedFontData.Type1Font)
    {
    }

    private static COSDictionary CreateStandard14Dictionary(string baseFontName)
    {
        COSDictionary dictionary = new();
        dictionary.SetItem(COSName.TYPE, COSName.GetPDFName("Font"));
        dictionary.SetName(COSName.SUBTYPE, "Type1");
        dictionary.SetName(BaseFontKey, baseFontName);
        if (baseFontName is not "Symbol" and not "ZapfDingbats")
        {
            dictionary.SetItem(COSName.GetPDFName("Encoding"), COSName.GetPDFName("WinAnsiEncoding"));
        }
        return dictionary;
    }

    private static string ToStandard14Name(FontName baseFont) =>
        baseFont switch
        {
            FontName.TIMES_ROMAN => "Times-Roman",
            FontName.TIMES_BOLD => "Times-Bold",
            FontName.TIMES_ITALIC => "Times-Italic",
            FontName.TIMES_BOLD_ITALIC => "Times-BoldItalic",
            FontName.HELVETICA => "Helvetica",
            FontName.HELVETICA_BOLD => "Helvetica-Bold",
            FontName.HELVETICA_OBLIQUE => "Helvetica-Oblique",
            FontName.HELVETICA_BOLD_OBLIQUE => "Helvetica-BoldOblique",
            FontName.COURIER => "Courier",
            FontName.COURIER_BOLD => "Courier-Bold",
            FontName.COURIER_OBLIQUE => "Courier-Oblique",
            FontName.COURIER_BOLD_OBLIQUE => "Courier-BoldOblique",
            FontName.SYMBOL => "Symbol",
            FontName.ZAPF_DINGBATS => "ZapfDingbats",
            _ => throw new ArgumentOutOfRangeException(nameof(baseFont), baseFont, "Unsupported standard 14 font."),
        };

    private static EmbeddedType1FontData CreateEmbeddedType1FontData(PDDocument document, Stream pfbStream)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(pfbStream);

        using MemoryStream buffer = new();
        pfbStream.CopyTo(buffer);
        byte[] pfbBytes = buffer.ToArray();
        PfbParser pfbParser = new(pfbBytes);
        Type1Font type1Font = Type1Font.CreateWithPFB(pfbBytes);

        COSDictionary dictionary = new();
        dictionary.SetItem(TypeKey, COSName.GetPDFName("Font"));
        dictionary.SetName(COSName.SUBTYPE, "Type1");
        dictionary.SetName(BaseFontKey, type1Font.GetName());

        COSDictionary fontDescriptor = BuildEmbeddedType1FontDescriptor(type1Font);
        COSStream fontFile = document.GetDocument().CreateCOSStream();
        using (Stream output = fontFile.CreateOutputStream(COSName.FLATE_DECODE))
        {
            byte[] fontBytes = pfbParser.GetPfbdata();
            output.Write(fontBytes, 0, fontBytes.Length);
        }

        int[] lengths = pfbParser.GetLengths();
        for (int i = 0; i < lengths.Length; i++)
        {
            fontFile.SetInt(COSName.GetPDFName($"Length{i + 1}"), lengths[i]);
        }

        fontDescriptor.SetItem(FontFileKey, fontFile);
        dictionary.SetItem(FontDescriptorKey, fontDescriptor);

        Type1Encoding fontEncoding = new(type1Font);
        COSArray widths = new();
        for (int code = 0; code <= 255; code++)
        {
            float width;
            try
            {
                width = type1Font.GetWidth(fontEncoding.GetName(code));
            }
            catch (IOException)
            {
                width = 0f;
            }

            widths.Add(COSInteger.Get((long)MathF.Round(width)));
        }

        dictionary.SetInt(FirstCharKey, 0);
        dictionary.SetInt(LastCharKey, 255);
        dictionary.SetItem(WidthsKey, widths);

        return new EmbeddedType1FontData(dictionary, type1Font);
    }

    private static COSDictionary BuildEmbeddedType1FontDescriptor(Type1Font type1Font)
    {
        COSDictionary descriptor = new();
        descriptor.SetItem(TypeKey, COSName.GetPDFName("FontDescriptor"));
        descriptor.SetName(FontNameKey, type1Font.GetName());
        if (!string.IsNullOrWhiteSpace(type1Font.GetFamilyName()))
        {
            descriptor.SetString(FontFamilyKey, type1Font.GetFamilyName());
        }

        int flags = 0;
        if (type1Font.IsFixedPitch())
        {
            flags |= 1;
        }

        bool isSymbolic = type1Font.GetEncoding() is PdfBox.Net.FontBox.Encoding.BuiltInEncoding;
        flags |= isSymbolic ? 4 : 32;
        if (Math.Abs(type1Font.GetItalicAngle()) > float.Epsilon)
        {
            flags |= 64;
        }

        descriptor.SetInt(FlagsKey, flags);

        var bbox = type1Font.GetFontBBox();
        descriptor.SetItem(FontBBoxKey, COSArray.Of(
            bbox.GetLowerLeftX(),
            bbox.GetLowerLeftY(),
            bbox.GetUpperRightX(),
            bbox.GetUpperRightY()));

        descriptor.SetFloat(ItalicAngleKey, type1Font.GetItalicAngle());
        descriptor.SetFloat(AscentKey, bbox.GetUpperRightY());
        descriptor.SetFloat(DescentKey, bbox.GetLowerLeftY());

        IList<float> blueValues = type1Font.GetBlueValues();
        float capHeight = blueValues.Count > 2 ? blueValues[2] : bbox.GetUpperRightY();
        descriptor.SetFloat(CapHeightKey, capHeight);
        descriptor.SetFloat(StemVKey, 0f);
        return descriptor;
    }

    private static Encoding.Encoding ResolveType1Encoding(COSDictionary dictionary, Type1Font? type1Font)
    {
        if (dictionary.GetDictionaryObject(COSName.GetPDFName("Encoding")) is COSDictionary encodingDictionary)
        {
            Encoding.Encoding? builtIn = type1Font != null ? new Type1Encoding(type1Font) : null;
            bool symbolic = GetSymbolicFlag(dictionary) ?? false;
            return new DictionaryEncoding(encodingDictionary, !symbolic, builtIn);
        }

        if (dictionary.GetDictionaryObject(COSName.GetPDFName("Encoding")) is not null)
        {
            return DictionaryEncoding.ResolveEncoding(dictionary);
        }

        if (type1Font != null)
        {
            return new Type1Encoding(type1Font);
        }

        if (Standard14Fonts.GetAFM(dictionary.GetNameAsString(BaseFontKey)) is { } afm)
        {
            return new Type1Encoding(afm);
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

            if (extension.Equals(".ttc", StringComparison.OrdinalIgnoreCase))
            {
                using TrueTypeCollection collection = new(fontPath);
                return collection.GetFontByName(mappedName)
                       ?? (fontName is not null ? collection.GetFontByName(fontName) : null)
                       ?? collection.GetFontByName(Path.GetFileNameWithoutExtension(fontPath));
            }
        }
        catch
        {
            // Keep dictionary-driven fallback behavior.
        }

        return null;
    }

    private sealed record EmbeddedType1FontData(COSDictionary Dictionary, Type1Font Type1Font);
}
