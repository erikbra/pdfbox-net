/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox-layout-awt/src/main/java/org/apache/pdfbox/glyphlayout/awt/GlyphLayoutFontLoaderAwt.java
 * PDFBOX_SOURCE_COMMIT: 56575fd583792844b6bd182d67739d26568b1d01
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 56575fd583792844b6bd182d67739d26568b1d01
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

using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.Rendering;

namespace PdfBox.Net.GlyphLayout.Awt;

/// <summary>
/// Loads the <see cref="PDType0Font"/> and AWT-style <see cref="Font"/> for
/// <see cref="GlyphLayoutProcessorAwt"/>.
/// </summary>
/// <remarks>
/// The .NET port currently keeps this as a Java-shaped registration layer. The
/// AWT <see cref="Font"/> proxy does not yet expose a full shaping engine.
/// </remarks>
public class GlyphLayoutFontLoaderAwt
{
    private readonly Dictionary<PDType0Font, Font> _awtFontMap = new();
    private readonly Dictionary<PDType0Font, FontOptions> _fontOptionsMap = new();

    public PDType0Font LoadFont(PDDocument pdDocument, Stream inputStream)
    {
        return LoadFont(pdDocument, inputStream, true, null);
    }

    public PDType0Font LoadFont(PDDocument pdDocument, Stream inputStream, bool embedSubset)
    {
        return LoadFont(pdDocument, inputStream, embedSubset, null);
    }

    public PDType0Font LoadFont(PDDocument pdDocument, Stream inputStream, FontOptions? fontOptions)
    {
        return LoadFont(pdDocument, inputStream, true, fontOptions);
    }

    public PDType0Font LoadFont(PDDocument pdDocument, Stream inputStream, bool embedSubset, FontOptions? fontOptions)
    {
        ArgumentNullException.ThrowIfNull(inputStream);

        using MemoryStream buffer = new();
        inputStream.CopyTo(buffer);
        byte[] fontBytes = buffer.ToArray();

        using MemoryStream pdFontInput = new(fontBytes, writable: false);
        PDType0Font pdType0Font = PDType0Font.Load(pdDocument, pdFontInput, embedSubset);

        using MemoryStream awtFontInput = new(fontBytes, writable: false);
        LoadAwtFont(pdType0Font, awtFontInput, fontOptions);
        return pdType0Font;
    }

    protected void LoadAwtFont(PDType0Font pdType0Font, Stream inputStream, FontOptions? fontOptions)
    {
        ArgumentNullException.ThrowIfNull(pdType0Font);
        ArgumentNullException.ThrowIfNull(inputStream);

        if (!_awtFontMap.ContainsKey(pdType0Font))
        {
            _awtFontMap[pdType0Font] = new Font();
            _fontOptionsMap[pdType0Font] = fontOptions ?? new FontOptions();
        }
    }

    public bool SupportsFont(PDFont font)
    {
        return font is PDType0Font type0Font && _awtFontMap.ContainsKey(type0Font);
    }

    public Font? GetAwtFont(PDType0Font font)
    {
        return _awtFontMap.GetValueOrDefault(font);
    }

    public FontOptions? GetFontOptions(PDType0Font font)
    {
        return _fontOptionsMap.GetValueOrDefault(font);
    }

    /// <summary>
    /// Specify options for an AWT font.
    /// </summary>
    public class FontOptions
    {
        public bool Kerning { get; private set; }
        public bool Ligatures { get; private set; }

        public FontOptions SetKerningOn()
        {
            Kerning = true;
            return this;
        }

        public FontOptions SetLigaturesOn()
        {
            Ligatures = true;
            return this;
        }
    }
}
