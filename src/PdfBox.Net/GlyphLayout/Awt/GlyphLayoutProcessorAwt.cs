/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox-layout-awt/src/main/java/org/apache/pdfbox/glyphlayout/awt/GlyphLayoutProcessorAwt.java
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

using System.Text;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.Rendering;

namespace PdfBox.Net.GlyphLayout.Awt;

/// <summary>
/// Processor for glyph layout.
/// </summary>
/// <remarks>
/// Java PDFBox delegates this implementation to <c>java.awt.Font</c>. The .NET
/// port keeps the same registration and content-stream API but currently emits
/// Unicode scalar values as Type 0 glyph codes. This is a conservative fallback
/// for Identity-H embedded fonts and a clear replacement point for a future
/// HarfBuzz/Skia text shaping backend.
/// </remarks>
public class GlyphLayoutProcessorAwt : GlyphLayoutProcessorInterface
{
    private readonly GlyphLayoutFontLoaderAwt _glyphLayoutFontLoaderAwt;

    public GlyphLayoutProcessorAwt()
    {
        _glyphLayoutFontLoaderAwt = new GlyphLayoutFontLoaderAwt();
    }

    public static void CheckMissingGlyphs(string text, Font awtFont)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(awtFont);
    }

    public bool SupportsFont(PDFont font)
    {
        return _glyphLayoutFontLoaderAwt.SupportsFont(font);
    }

    public PDType0Font LoadFont(PDDocument pdDocument, Stream inputStream)
    {
        return _glyphLayoutFontLoaderAwt.LoadFont(pdDocument, inputStream);
    }

    public PDType0Font LoadFont(PDDocument pdDocument, Stream inputStream, bool embedSubset)
    {
        return _glyphLayoutFontLoaderAwt.LoadFont(pdDocument, inputStream, embedSubset);
    }

    public PDType0Font LoadFont(
        PDDocument pdDocument,
        Stream inputStream,
        GlyphLayoutFontLoaderAwt.FontOptions? fontOptions)
    {
        return _glyphLayoutFontLoaderAwt.LoadFont(pdDocument, inputStream, fontOptions);
    }

    public PDType0Font LoadFont(
        PDDocument pdDocument,
        Stream inputStream,
        bool embedSubset,
        GlyphLayoutFontLoaderAwt.FontOptions? fontOptions)
    {
        return _glyphLayoutFontLoaderAwt.LoadFont(pdDocument, inputStream, embedSubset, fontOptions);
    }

    protected virtual int[] ComputeGlyphCodes(PDType0Font font, float fontSize, string text, int bidiLevel)
    {
        ArgumentNullException.ThrowIfNull(font);
        ArgumentNullException.ThrowIfNull(text);

        List<int> glyphCodes = [];
        foreach (Rune rune in text.EnumerateRunes())
        {
            glyphCodes.Add(rune.Value);
        }

        return glyphCodes.ToArray();
    }

    public void ShowText(ContentStreamForGlyphLayoutInterface contentStream, PDType0Font font, float fontSize, string text)
    {
        ShowTextUni(contentStream, font, fontSize, text, bidiLevel: 0);
    }

    protected virtual void ShowTextUni(
        ContentStreamForGlyphLayoutInterface contentStream,
        PDType0Font font,
        float fontSize,
        string text,
        int bidiLevel)
    {
        ArgumentNullException.ThrowIfNull(contentStream);
        int[] glyphCodes = ComputeGlyphCodes(font, fontSize, text ?? string.Empty, bidiLevel);
        contentStream.ShowGlyphCodes(glyphCodes);
    }
}
