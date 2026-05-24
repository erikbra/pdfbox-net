/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/OpenTypeFont.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: trunk
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

using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.FontBox.TTF;

/// <summary>
/// An OpenType (OTF/TTF) font.
/// </summary>
public sealed class OpenTypeFont : TrueTypeFont
{
    private bool _hasPostScriptTag;

    public OpenTypeFont() : base()
    {
    }

    internal OpenTypeFont(TTFDataStream fontData) : base(fontData)
    {
    }

    internal override void SetVersion(float versionValue)
    {
        _hasPostScriptTag = BitConverter.SingleToInt32Bits(versionValue) == 0x469EA8A9;
        base.SetVersion(versionValue);
    }

    public CFFTable GetCFF()
    {
        if (!_hasPostScriptTag)
        {
            throw new NotSupportedException("TTF fonts do not have a CFF table");
        }

        return (CFFTable)GetTable(CFFTable.TAG)!;
    }

    public override GlyphTable? GetGlyph()
    {
        if (_hasPostScriptTag)
        {
            throw new NotSupportedException("OTF fonts do not have a glyf table");
        }

        return base.GetGlyph();
    }

    public override GeneralPath GetPath(string name)
    {
        if (_hasPostScriptTag && IsSupportedOTF())
        {
            return GetCFF().GetFont()?.GetPath(name) ?? new GeneralPath();
        }

        return base.GetPath(name);
    }

    public bool IsPostScript()
    {
        return _hasPostScriptTag || tables.ContainsKey(CFFTable.TAG) || tables.ContainsKey("CFF2");
    }

    public bool IsSupportedOTF()
    {
        return !(_hasPostScriptTag && !tables.ContainsKey(CFFTable.TAG) && tables.ContainsKey("CFF2"));
    }

    public bool HasLayoutTables()
    {
        return tables.ContainsKey("BASE") || tables.ContainsKey("GDEF") || tables.ContainsKey("GPOS") ||
               tables.ContainsKey(GlyphSubstitutionTable.TAG) || tables.ContainsKey(OTLTable.TAG);
    }
}
