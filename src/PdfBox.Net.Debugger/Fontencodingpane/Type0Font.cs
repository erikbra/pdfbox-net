/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: debugger/src/main/java/org/apache/pdfbox/debugger/fontencodingpane/Type0Font.java
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
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Font.Encoding;

namespace PdfBox.Net.Debugger.Fontencodingpane;

/// <summary>
/// CID-to-GID data model for Type 0 fonts (descendant PDCIDFontType2).
/// Adapted from Apache PDFBox Type0Font (Khyrul Bashar).
/// </summary>
public sealed class Type0Font : FontPane
{
    public const string NoGlyph = "No glyph";

    private static readonly COSName CidToGidMapKey = COSName.GetPDFName("CIDToGIDMap");

    /// <summary>
    /// Table columns when a CIDToGIDMap stream is present:
    /// [0] CID (int), [1] GID (int), [2] Unicode (string).
    /// Otherwise:
    /// [0] Code (int), [1] CID (int), [2] GID (int), [3] Unicode (string).
    /// </summary>
    public object[][]? TableData { get; }

    public string[] ColumnNames { get; } = [];

    public Dictionary<string, string> Attributes { get; }

    public int TotalAvailableGlyph { get; private set; }

    public Type0Font(PDCIDFont descendantFont, PDType0Font parentFont)
    {
        var glyphList = GlyphList.GetAdobeGlyphList();
        Attributes = new Dictionary<string, string>
        {
            ["Font"] = descendantFont.GetName(),
        };

        object[][]? cidToGid = ReadCIDToGIDMap(descendantFont, parentFont, glyphList);
        if (cidToGid != null)
        {
            TableData = cidToGid;
            ColumnNames = ["CID", "GID", "Unicode Character"];
            Attributes["CIDs"] = cidToGid.Length.ToString();
        }
        else
        {
            TableData = ReadMap(descendantFont, parentFont, glyphList);
            ColumnNames = ["Code", "CID", "GID", "Unicode Character"];
            if (TableData != null)
            {
                Attributes["CIDs"] = TableData.Length.ToString();
                Attributes["Glyphs"] = TotalAvailableGlyph.ToString();
            }
        }
    }

    private object[][]? ReadCIDToGIDMap(PDCIDFont font, PDType0Font parentFont, GlyphList glyphList)
    {
        COSDictionary dict = font.GetCOSObject();
        COSStream? stream = dict.GetCOSStream(CidToGidMapKey);
        if (stream == null)
        {
            return null;
        }

        byte[] mapBytes;
        using (var input = stream.CreateInputStream())
        using (var ms = new System.IO.MemoryStream())
        {
            input.CopyTo(ms);
            mapBytes = ms.ToArray();
        }

        int count = mapBytes.Length / 2;
        var table = new object[count][];
        for (int i = 0; i < count; i++)
        {
            int gid = ((mapBytes[i * 2] & 0xFF) << 8) | (mapBytes[i * 2 + 1] & 0xFF);
            string? unicode = gid != 0 ? parentFont.ToUnicode(i, glyphList) : null;
            if (gid != 0)
            {
                TotalAvailableGlyph++;
            }

            table[i] = [i, gid, unicode ?? NoGlyph];
        }

        return table;
    }

    private object[][]? ReadMap(PDCIDFont descendantFont, PDType0Font parentFont, GlyphList glyphList)
    {
        // Only PDCIDFontType2 supports CodeToGID; other subtypes fall back to CID == GID.
        var t2 = descendantFont as PDCIDFontType2;

        // Count codes that map to a non-zero GID.
        int codes = 0;
        for (int code = 0; code < 65535; code++)
        {
            int cid = descendantFont.CodeToCID(code);
            int gid = t2 != null ? t2.CodeToGID(cid) : cid;
            if (gid != 0)
            {
                codes++;
            }
        }

        if (codes == 0)
        {
            return [];
        }

        var tab = new object[codes][];
        int index = 0;
        for (int code = 0; code < 65535 && index < codes; code++)
        {
            int cid = descendantFont.CodeToCID(code);
            int gid = t2 != null ? t2.CodeToGID(cid) : cid;
            if (gid == 0)
            {
                continue;
            }

            string? unicode = parentFont.ToUnicode(code, glyphList);
            TotalAvailableGlyph++;
            tab[index++] = [code, cid, gid, unicode ?? NoGlyph];
        }

        return tab;
    }
}
