/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDType3Font.java
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
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Font.Encoding;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Util;
using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.PDModel.Font;

public sealed class PDType3Font : PDSimpleFont
{
    private static readonly COSName NameKey = COSName.NAME;
    private static readonly COSName CharProcsKey = COSName.GetPDFName("CharProcs");
    private static readonly COSName FontBBoxKey = COSName.GetPDFName("FontBBox");
    private static readonly COSName FirstCharKey = COSName.GetPDFName("FirstChar");
    private static readonly COSName LastCharKey = COSName.GetPDFName("LastChar");
    private static readonly COSName ResourcesKey = COSName.RESOURCES;
    private static readonly COSName FontMatrixKey = COSName.GetPDFName("FontMatrix");

    private readonly PDResources? _resources;
    private COSDictionary? _charProcs;
    private BoundingBox? _fontBBox;
    private Matrix? _fontMatrix;

    public PDType3Font(COSDictionary dictionary)
        : base(dictionary, ResolveEncoding(dictionary))
    {
        if (dictionary.GetCOSDictionary(ResourcesKey) is COSDictionary resourcesDictionary)
        {
            _resources = new PDResources(resourcesDictionary);
        }
    }

    public override string GetName() => FontDictionary.GetNameAsString(NameKey) ?? base.GetName();

    public override Vector GetDisplacement(int code)
    {
        return GetFontMatrix().Transform(GetWidth(code), 0);
    }

    public override float GetWidth(int code)
    {
        float[]? widths = GetExplicitWidths();
        int firstChar = FontDictionary.GetInt(FirstCharKey, -1);
        int lastChar = FontDictionary.GetInt(LastCharKey, -1);
        if (widths != null && code >= firstChar && code <= lastChar)
        {
            int index = code - firstChar;
            if ((uint)index < (uint)widths.Length)
            {
                return widths[index];
            }
        }

        float missingWidth = GetFontDescriptor()?.GetMissingWidth() ?? 0f;
        if (missingWidth > 0)
        {
            return missingWidth;
        }

        PDType3CharProc? charProc = GetCharProc(code);
        if (charProc == null || charProc.GetCOSObject().GetLength() == 0)
        {
            return 0f;
        }

        try
        {
            return charProc.GetWidth();
        }
        catch
        {
            return 0f;
        }
    }

    public override Matrix GetFontMatrix()
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

    public override BoundingBox GetBoundingBox()
    {
        if (_fontBBox != null)
        {
            return _fontBBox;
        }

        PDRectangle? rect = GetFontBBox();
        if (rect != null && (rect.GetWidth() != 0 || rect.GetHeight() != 0))
        {
            _fontBBox = new BoundingBox(rect.GetLowerLeftX(), rect.GetLowerLeftY(), rect.GetUpperRightX(), rect.GetUpperRightY());
            return _fontBBox;
        }

        bool hasBounds = false;
        float minX = 0f;
        float minY = 0f;
        float maxX = 0f;
        float maxY = 0f;
        COSDictionary? charProcs = GetCharProcs();
        if (charProcs != null)
        {
            foreach (COSName name in charProcs.KeySet())
            {
                if (charProcs.GetDictionaryObject(name) is not COSStream glyphStream)
                {
                    continue;
                }

                PDRectangle? glyphBox = new PDType3CharProc(this, glyphStream).GetGlyphBBox();
                if (glyphBox == null)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    minX = glyphBox.GetLowerLeftX();
                    minY = glyphBox.GetLowerLeftY();
                    maxX = glyphBox.GetUpperRightX();
                    maxY = glyphBox.GetUpperRightY();
                    hasBounds = true;
                }
                else
                {
                    minX = Math.Min(minX, glyphBox.GetLowerLeftX());
                    minY = Math.Min(minY, glyphBox.GetLowerLeftY());
                    maxX = Math.Max(maxX, glyphBox.GetUpperRightX());
                    maxY = Math.Max(maxY, glyphBox.GetUpperRightY());
                }
            }
        }

        _fontBBox = hasBounds ? new BoundingBox(minX, minY, maxX, maxY) : new BoundingBox();
        return _fontBBox;
    }

    public override bool HasGlyph(int code)
    {
        string glyphName = FontEncoding.GetName(code);
        return glyphName != ".notdef" && GetCharProcs()?.GetDictionaryObject(COSName.GetPDFName(glyphName)) is COSStream;
    }

    public override GeneralPath GetNormalizedPath(int code) => new();
    public override GeneralPath GetPath(int code) => GetNormalizedPath(code);
    public override GeneralPath GetPath(string name) => new();

    public override FontBoxFont? GetFontBoxFont() => throw new NotSupportedException("Type 3 fonts do not use FontBox fonts.");

    public override bool IsStandard14() => false;
    public override bool IsEmbedded() => true;
    public override bool IsDamaged() => false;

    public override float GetWidthFromFont(int code)
    {
        return GetWidth(code);
    }

    public PDResources? GetResources() => _resources;

    public PDRectangle? GetFontBBox()
    {
        COSArray? box = FontDictionary.GetCOSArray(FontBBoxKey);
        return box == null ? null : new PDRectangle(box);
    }

    public COSDictionary? GetCharProcs()
    {
        _charProcs ??= FontDictionary.GetCOSDictionary(CharProcsKey);
        return _charProcs;
    }

    public PDType3CharProc? GetCharProc(int code)
    {
        string glyphName = FontEncoding.GetName(code);
        if (glyphName == ".notdef")
        {
            return null;
        }

        return GetCharProcs()?.GetDictionaryObject(COSName.GetPDFName(glyphName)) is COSStream stream
            ? new PDType3CharProc(this, stream)
            : null;
    }

    private static PdfBox.Net.PDModel.Font.Encoding.Encoding ResolveEncoding(COSDictionary dictionary)
    {
        return dictionary.GetDictionaryObject(COSName.GetPDFName("Encoding")) is null
            ? new PdfBox.Net.PDModel.Font.Encoding.Encoding()
            : DictionaryEncoding.ResolveEncoding(dictionary);
    }
}
