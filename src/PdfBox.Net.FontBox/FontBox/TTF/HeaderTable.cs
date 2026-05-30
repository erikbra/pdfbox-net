/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/HeaderTable.java
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

namespace PdfBox.Net.FontBox.TTF;

/// <summary>
/// This 'head'-table is a required table in a TrueType font.
/// </summary>
public sealed class HeaderTable : TTFTable
{
    public const string TAG = "head";
    public const int MAC_STYLE_BOLD = 1;
    public const int MAC_STYLE_ITALIC = 2;

    public HeaderTable() : base(TAG)
    {
    }

    public float Version { get; set; }
    public float FontRevision { get; set; }
    public uint CheckSumAdjustment { get; set; }
    public uint MagicNumber { get; set; }
    public int Flags { get; set; }
    public int UnitsPerEm { get; set; }
    public DateTimeOffset Created { get; set; }
    public DateTimeOffset Modified { get; set; }
    public short XMin { get; set; }
    public short YMin { get; set; }
    public short XMax { get; set; }
    public short YMax { get; set; }
    public int MacStyle { get; set; }
    public int LowestRecPPEM { get; set; }
    public short FontDirectionHint { get; set; }
    public short IndexToLocFormat { get; set; }
    public short GlyphDataFormat { get; set; }

    internal override void ReadHeaders(TrueTypeFont ttf, TTFDataStream data, FontHeaders outHeaders)
    {
        data.Seek(data.GetCurrentPosition() + 44);
        MacStyle = data.ReadUnsignedShort();
        outHeaders.SetHeaderMacStyle(MacStyle);
    }

    internal override void Read(TrueTypeFont ttf, TTFDataStream data)
    {
        Version = data.Read32Fixed();
        FontRevision = data.Read32Fixed();
        CheckSumAdjustment = data.ReadUnsignedInt();
        MagicNumber = data.ReadUnsignedInt();
        Flags = data.ReadUnsignedShort();
        UnitsPerEm = data.ReadUnsignedShort();
        Created = data.ReadInternationalDate();
        Modified = data.ReadInternationalDate();
        XMin = data.ReadSignedShort();
        YMin = data.ReadSignedShort();
        XMax = data.ReadSignedShort();
        YMax = data.ReadSignedShort();
        MacStyle = data.ReadUnsignedShort();
        LowestRecPPEM = data.ReadUnsignedShort();
        FontDirectionHint = data.ReadSignedShort();
        IndexToLocFormat = data.ReadSignedShort();
        GlyphDataFormat = data.ReadSignedShort();
        initialized = true;
    }

    public uint GetCheckSumAdjustment() => CheckSumAdjustment;
    public void SetCheckSumAdjustment(uint value) => CheckSumAdjustment = value;
    public DateTimeOffset GetCreated() => Created;
    public void SetCreated(DateTimeOffset value) => Created = value;
    public int GetFlags() => Flags;
    public void SetFlags(int value) => Flags = value;
    public short GetFontDirectionHint() => FontDirectionHint;
    public void SetFontDirectionHint(short value) => FontDirectionHint = value;
    public float GetFontRevision() => FontRevision;
    public void SetFontRevision(float value) => FontRevision = value;
    public short GetGlyphDataFormat() => GlyphDataFormat;
    public void SetGlyphDataFormat(short value) => GlyphDataFormat = value;
    public short GetIndexToLocFormat() => IndexToLocFormat;
    public void SetIndexToLocFormat(short value) => IndexToLocFormat = value;
    public int GetLowestRecPPEM() => LowestRecPPEM;
    public void SetLowestRecPPEM(int value) => LowestRecPPEM = value;
    public int GetMacStyle() => MacStyle;
    public void SetMacStyle(int value) => MacStyle = value;
    public uint GetMagicNumber() => MagicNumber;
    public void SetMagicNumber(uint value) => MagicNumber = value;
    public DateTimeOffset GetModified() => Modified;
    public void SetModified(DateTimeOffset value) => Modified = value;
    public int GetUnitsPerEm() => UnitsPerEm;
    public void SetUnitsPerEm(int value) => UnitsPerEm = value;
    public float GetVersion() => Version;
    public void SetVersion(float value) => Version = value;
    public short GetXMax() => XMax;
    public void SetXMax(short value) => XMax = value;
    public short GetXMin() => XMin;
    public void SetXMin(short value) => XMin = value;
    public short GetYMax() => YMax;
    public void SetYMax(short value) => YMax = value;
    public short GetYMin() => YMin;
    public void SetYMin(short value) => YMin = value;
}
