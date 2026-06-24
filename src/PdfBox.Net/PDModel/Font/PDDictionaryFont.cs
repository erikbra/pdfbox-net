/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted implementation for dictionary-backed PDF font behavior.
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
using PdfBox.Net.PDModel.Font.Encoding;

namespace PdfBox.Net.PDModel.Font;

/// <summary>
/// A minimal PDFont implementation backed directly by a PDF font dictionary.
/// Used when a font is resolved from page resources but the full font file (Type 1, TrueType, etc.)
/// is not loaded. All metric operations return safe defaults so that text extraction
/// can proceed without font-file I/O.
/// </summary>
public sealed class PDDictionaryFont : PDFont
{
    private readonly Encoding.Encoding _encoding;

    private PDDictionaryFont(COSDictionary dict)
        : base(dict)
    {
        _encoding = DictionaryEncoding.ResolveEncoding(dict);
    }

    /// <summary>
    /// Constructs a <see cref="PDDictionaryFont"/> from the given font dictionary.
    /// </summary>
    public static PDDictionaryFont Create(COSDictionary dict) =>
        new PDDictionaryFont(dict ?? throw new ArgumentNullException(nameof(dict)));

    /// <inheritdoc/>
    public override float GetWidth(int code)
    {
        float width = base.GetWidth(code);
        if (width > 0)
        {
            return width;
        }

        return code == 0x20 ? 20f : 40f;
    }

    protected override string? ToUnicodeFallback(int code, GlyphList glyphList)
    {
        if (code < 0 || code > byte.MaxValue)
        {
            return null;
        }

        string glyphName = _encoding.GetName(code);
        if (glyphName != ".notdef")
        {
            string? mapped = glyphList.ToUnicode(glyphName);
            if (mapped is not null)
            {
                return mapped;
            }
        }

        return System.Text.Encoding.Latin1.GetString([(byte)code]);
    }
}
