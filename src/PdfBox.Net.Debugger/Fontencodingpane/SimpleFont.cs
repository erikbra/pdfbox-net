/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/fontencodingpane/SimpleFont.java
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

using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Font.Encoding;
using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.Debugger.Fontencodingpane;

/// <summary>
/// Glyph-encoding data model for non-Type-3 simple fonts (PDType1Font, PDTrueTypeFont, etc.).
/// Adapted from Apache PDFBox SimpleFont (Khyrul Bashar, Apache Software Foundation).
/// Note: glyph names are not exposed by the C# public API; the Name column is omitted.
/// </summary>
public sealed class SimpleFont : FontPane
{
    public const string NoGlyph = "None";

    /// <summary>
    /// Table columns: [0] Code (int), [1] Unicode (string), [2] Glyph (<see cref="GeneralPath"/>).
    /// </summary>
    public object[][] TableData { get; }

    public string[] ColumnNames { get; } = ["Code", "Unicode Character", "Glyph"];

    public Dictionary<string, string> Attributes { get; }

    public int TotalAvailableGlyph { get; private set; }

    public SimpleFont(PDSimpleFont font)
    {
        var glyphList = GlyphList.GetAdobeGlyphList();
        TableData = BuildTable(font, glyphList);

        Attributes = new Dictionary<string, string>
        {
            ["Font"] = font.GetName(),
            ["Glyphs"] = TotalAvailableGlyph.ToString(),
            ["Standard 14"] = font.IsStandard14().ToString(),
        };
    }

    private object[][] BuildTable(PDSimpleFont font, GlyphList glyphList)
    {
        var table = new object[256][];
        for (int code = 0; code <= 255; code++)
        {
            string? unicode = font.ToUnicode(code, glyphList);
            if (font.HasGlyph(code))
            {
                GeneralPath path;
                try
                {
                    path = font.GetNormalizedPath(code);
                }
                catch
                {
                    path = new GeneralPath();
                }

                table[code] = [code, unicode ?? NoGlyph, path];
                TotalAvailableGlyph++;
            }
            else
            {
                table[code] = [code, NoGlyph, new GeneralPath()];
            }
        }

        return table;
    }
}
