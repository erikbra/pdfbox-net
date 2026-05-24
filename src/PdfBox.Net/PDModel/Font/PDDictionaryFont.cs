/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted implementation for minimal font wrapper backed by a PDF resource dictionary entry.
 *
 * PORT_MODE: adapted
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
    private PDDictionaryFont(COSDictionary dict)
        : base(dict)
    {
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

    /// <inheritdoc/>
    public override float GetSpaceWidth() => 20f;

    /// <inheritdoc/>
    public override string? ToUnicode(int code, GlyphList glyphList)
    {
        if (code < 0 || code > byte.MaxValue)
        {
            return null;
        }

        return System.Text.Encoding.Latin1.GetString([(byte)code]);
    }
}
