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

using PdfBox.Net.COS;
using PdfBox.Net.FontBox.Util;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Font.Encoding;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Rendering;
using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.Debugger.Fontencodingpane;

/// <summary>
/// Glyph-encoding data model for Type 3 fonts.
/// Adapted from Apache PDFBox Type3Font (Khyrul Bashar, Tilman Hausherr).
/// </summary>
public sealed class Type3Font : FontPane
{
    public const string NoGlyph = "No glyph";
    public const string GlyphPreviewUnavailable = "[glyph preview unavailable]";

    /// <summary>
    /// Table columns: [0] Code (int), [1] Glyph name (string),
    /// [2] Unicode (string), [3] Glyph preview (<see cref="BufferedImage"/> or string token).
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
            ["Encoding"] = font.GetEncoding().GetEncodingName(),
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
        var renderedGlyphs = new Dictionary<string, BufferedImage>(StringComparer.Ordinal);
        var encoding = font.GetEncoding();
        IDictionary<int, string> codeToName = encoding.GetCodeToNameMap();

        for (int code = 0; code <= 255; code++)
        {
            string? unicode = font.ToUnicode(code, glyphList);
            if (codeToName.ContainsKey(code) || unicode != null)
            {
                string glyphName = encoding.GetName(code);
                object glyph = NoGlyph;
                if (!isEmpty)
                {
                    if (!renderedGlyphs.TryGetValue(glyphName, out BufferedImage? image))
                    {
                        image = RenderType3Glyph(font, fontBBox, code);
                        if (image != null)
                        {
                            renderedGlyphs[glyphName] = image;
                        }
                    }

                    glyph = image is null ? GlyphPreviewUnavailable : image;
                }

                table[code] = [code, glyphName, unicode ?? NoGlyph, glyph];
                TotalAvailableGlyph++;
            }
            else
            {
                table[code] = [code, NoGlyph, NoGlyph, NoGlyph];
            }
        }

        return table;
    }

    // Kind of an overkill to create a PDF for one glyph, but there is no better
    // backend-neutral way to reuse the existing PDFBox rendering pipeline.
    private static BufferedImage? RenderType3Glyph(PDType3Font font, PDRectangle fontBBox, int code)
    {
        if (!RenderingBackend.IsRegistered)
        {
            return null;
        }

        using PDDocument doc = new();
        int scale = 1;
        float minDimension = MathF.Min(MathF.Abs(fontBBox.GetWidth()), MathF.Abs(fontBBox.GetHeight()));
        if (minDimension > 0 && (fontBBox.GetWidth() < 72 || fontBBox.GetHeight() < 72))
        {
            scale = Math.Max(1, (int)(72 / minDimension));
        }

        PDPage page = new(new PDRectangle(fontBBox.GetWidth() * scale, fontBBox.GetHeight() * scale));
        PDResources pageResources = font.GetResources() ?? new PDResources();
        pageResources.Put(COSName.GetPDFName("F0"), font);
        page.SetResources(pageResources);
        doc.AddPage(page);

        float scalingFactorX = font.GetFontMatrix().GetScalingFactorX();
        float scalingFactorY = font.GetFontMatrix().GetScalingFactorY();
        float translateX = scalingFactorX > 0 ? -fontBBox.GetLowerLeftX() : fontBBox.GetUpperRightX();
        float translateY = scalingFactorY > 0 ? -fontBBox.GetLowerLeftY() : fontBBox.GetUpperRightY();
        float minScale = MathF.Min(MathF.Abs(scalingFactorX), MathF.Abs(scalingFactorY));
        float fontSize = minScale > 0 ? scale / minScale : scale;

        string content = FormattableString.Invariant($"""
            q
            1 0 0 1 {translateX * scale} {translateY * scale} cm
            BT
            /F0 {fontSize} Tf
            <{code:X2}> Tj
            ET
            Q
            """);

        COSStream stream = new();
        using (Stream output = stream.CreateOutputStream())
        {
            byte[] bytes = System.Text.Encoding.Latin1.GetBytes(content);
            output.Write(bytes, 0, bytes.Length);
        }

        ((COSDictionary)page.GetCOSObject()).SetItem(COSName.CONTENTS, stream);
        return new PDFRenderer(doc).RenderImage(0);
    }
}
