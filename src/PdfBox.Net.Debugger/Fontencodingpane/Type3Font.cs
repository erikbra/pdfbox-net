/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/fontencodingpane/Type3Font.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
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

using PdfBox.Net.FontBox.Util;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Font.Encoding;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.Debugger.Fontencodingpane;

/// <summary>
/// Glyph-encoding data model for Type 3 fonts.
/// Adapted from Apache PDFBox Type3Font (Khyrul Bashar, Tilman Hausherr).
/// Note: glyph rendering (BufferedImage) is replaced with a placeholder string token.
/// </summary>
public sealed class Type3Font : FontPane
{
    public const string NoGlyph = "No glyph";

    /// <summary>
    /// Table columns: [0] Code (int), [1] Glyph name (string),
    /// [2] Unicode (string), [3] Glyph token (string).
    /// </summary>
    public object[][] TableData { get; }

    public string[] ColumnNames { get; } = ["Code", "Glyph Name", "Unicode Character", "Glyph"];

    public Dictionary<string, string> Attributes { get; }

    public int TotalAvailableGlyph { get; private set; }

    public Type3Font(PDType3Font font, PDResources resources)
    {
        var fontBBox = CalcBBox(font);
        var glyphList = GlyphList.GetAdobeGlyphList();
        TableData = BuildTable(font, fontBBox, glyphList);

        string? name = font.GetName();
        if (name == null && font.GetFontDescriptor()?.GetFontName() is string descName)
        {
            name = descName;
        }

        Attributes = new Dictionary<string, string>
        {
            ["Font"] = name ?? string.Empty,
            ["Glyphs"] = TotalAvailableGlyph.ToString(),
        };
    }

    private static PDRectangle CalcBBox(PDType3Font font)
    {
        double minX = 0, maxX = 0, minY = 0, maxY = 0;
        for (int code = 0; code <= 255; code++)
        {
            PDType3CharProc? charProc = font.GetCharProc(code);
            if (charProc == null)
            {
                continue;
            }

            PDRectangle? glyphBBox = charProc.GetGlyphBBox();
            if (glyphBBox == null)
            {
                continue;
            }

            minX = Math.Min(minX, glyphBBox.GetLowerLeftX());
            maxX = Math.Max(maxX, glyphBBox.GetUpperRightX());
            minY = Math.Min(minY, glyphBBox.GetLowerLeftY());
            maxY = Math.Max(maxY, glyphBBox.GetUpperRightY());
        }

        var bbox = new PDRectangle((float)minX, (float)minY,
                                   (float)(maxX - minX), (float)(maxY - minY));
        if (bbox.GetWidth() <= 0 || bbox.GetHeight() <= 0)
        {
            BoundingBox fallback = font.GetBoundingBox();
            bbox = new PDRectangle(fallback.GetLowerLeftX(), fallback.GetLowerLeftY(),
                                   fallback.GetWidth(), fallback.GetHeight());
        }

        return bbox;
    }

    private object[][] BuildTable(PDType3Font font, PDRectangle fontBBox, GlyphList glyphList)
    {
        bool isEmpty = fontBBox.GetWidth() <= 0 || fontBBox.GetHeight() <= 0;
        var table = new object[256][];
        for (int code = 0; code <= 255; code++)
        {
            if (font.HasGlyph(code) || font.ToUnicode(code, glyphList) != null)
            {
                // GetCharProc can return the glyph name for display.
                PDType3CharProc? charProc = font.GetCharProc(code);
                string glyphName = $"code {code}";
                string? unicode = font.ToUnicode(code, glyphList);
                string glyphToken = (charProc != null && !isEmpty) ? $"[glyph {code}]" : NoGlyph;
                table[code] = [code, glyphName, unicode ?? NoGlyph, glyphToken];
                TotalAvailableGlyph++;
            }
            else
            {
                table[code] = [code, NoGlyph, NoGlyph, NoGlyph];
            }
        }

        return table;
    }
}
