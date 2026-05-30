/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/OTFParser.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
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

using PdfBox.Net.IO;

namespace PdfBox.Net.FontBox.TTF;

/// <summary>
/// OpenType font file parser.
/// </summary>
public sealed class OTFParser : TTFParser
{
    public OTFParser()
    {
    }

    public OTFParser(bool isEmbedded) : base(isEmbedded)
    {
    }

    public override TrueTypeFont Parse(RandomAccessRead randomAccessRead)
    {
        return (OpenTypeFont)base.Parse(randomAccessRead);
    }

    internal override TrueTypeFont Parse(TTFDataStream raf)
    {
        return (OpenTypeFont)base.Parse(raf);
    }

    internal override TrueTypeFont NewFont(TTFDataStream raf)
    {
        return new OpenTypeFont(raf);
    }

    protected override TTFTable ReadTable(string tag)
    {
        return tag switch
        {
            "BASE" or "GDEF" or "GPOS" or GlyphSubstitutionTable.TAG or OTLTable.TAG => new OTLTable { Tag = tag },
            CFFTable.TAG => new CFFTable(),
            _ => base.ReadTable(tag),
        };
    }

    protected override bool AllowCFF() => true;
}
