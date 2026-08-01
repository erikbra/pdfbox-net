/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/rendering/GlyphCache.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
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

using System.IO;
using Microsoft.Extensions.Logging;
using PdfBox.Net.Logging;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.Rendering;

/// <summary>
/// A simple glyph outline cache.
/// </summary>
internal sealed class GlyphCache
{
    private static ILogger<GlyphCache> LOG => PdfBoxLogging.CreateLogger<GlyphCache>();

    private readonly PDVectorFont _font;
    private readonly Dictionary<int, GeneralPath> _cache = [];

    internal GlyphCache(PDVectorFont font)
    {
        _font = font ?? throw new ArgumentNullException(nameof(font));
    }

    public GeneralPath GetPathForCharacterCode(int code)
    {
        if (_cache.TryGetValue(code, out GeneralPath? path))
        {
            return path;
        }

        try
        {
            if (!_font.HasGlyph(code))
            {
                if (_font is PDType0Font type0Font)
                {
                    int cid = type0Font.CodeToCID(code);
                    LOG.LogWarning("No glyph for code {Code} (CID {Cid:x4}) in font {FontName}",
                        code, cid, _font.GetName());
                }
                else if (_font is PDSimpleFont simpleFont)
                {
                    LOG.LogWarning(
                        "No glyph for code {Code} in {FontType} {FontName} (embedded or system font used: {FontBoxFontName})",
                        code, _font.GetType().Name, _font.GetName(), simpleFont.GetFontBoxFont()?.GetName() ?? "null");
                    if (code == 10 && simpleFont.IsStandard14())
                    {
                        path = new GeneralPath();
                        _cache[code] = path;
                        return path;
                    }
                }
                else
                {
                    LOG.LogWarning("No glyph for code {Code} in font {FontName}", code, _font.GetName());
                }
            }

            path = _font.GetNormalizedPath(code);
            _cache[code] = path;
            return path;
        }
        catch (IOException exception)
        {
            LOG.LogError(exception, "Glyph rendering failed for code {Code} in font {FontName}",
                code, _font.GetName());
            return new GeneralPath();
        }
    }
}
