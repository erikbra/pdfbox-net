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
using PdfBox.Net.FontBox.CMap;
using PdfBox.Net.FontBox.Util;
using PdfBox.Net.IO;
using PdfBox.Net.PDModel.Font.Encoding;
using PdfBox.Net.Util;

namespace PdfBox.Net.PDModel.Font;

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
