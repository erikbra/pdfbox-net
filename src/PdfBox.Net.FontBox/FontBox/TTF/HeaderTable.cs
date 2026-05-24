/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache FontBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/HeaderTable.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: mechanical
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

namespace PdfBox.Net.FontBox.TTF;

public sealed class HeaderTable() : TTFTable("head")
{
    public const int MacStyleBold = 1;
    public const int MacStyleItalic = 2;

    public float Version { get; set; }

    public float FontRevision { get; set; }

    public long CheckSumAdjustment { get; set; }

    public long MagicNumber { get; set; }

    public int Flags { get; set; }

    public int UnitsPerEm { get; set; }

    public short XMin { get; set; }

    public short YMin { get; set; }

    public short XMax { get; set; }

    public short YMax { get; set; }

    public int MacStyle { get; set; }

    public int LowestRecPPEM { get; set; }

    public short FontDirectionHint { get; set; }

    public short IndexToLocFormat { get; set; }

    public short GlyphDataFormat { get; set; }

    internal override void Read(TrueTypeFont font, TTFDataStream data)
    {
        Version = data.Read32Fixed();
        FontRevision = data.Read32Fixed();
        CheckSumAdjustment = data.ReadUnsignedInt();
        MagicNumber = data.ReadUnsignedInt();
        Flags = data.ReadUnsignedShort();
        UnitsPerEm = data.ReadUnsignedShort();
        _ = data.ReadLong();
        _ = data.ReadLong();
        XMin = data.ReadSignedShort();
        YMin = data.ReadSignedShort();
        XMax = data.ReadSignedShort();
        YMax = data.ReadSignedShort();
        MacStyle = data.ReadUnsignedShort();
        LowestRecPPEM = data.ReadUnsignedShort();
        FontDirectionHint = data.ReadSignedShort();
        IndexToLocFormat = data.ReadSignedShort();
        GlyphDataFormat = data.ReadSignedShort();
        Initialized = true;
    }
}
