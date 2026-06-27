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
public sealed partial class HeaderTable : TTFTable
{
    public const string TAG = "head";
    public const int MAC_STYLE_BOLD = 1;
    public const int MAC_STYLE_ITALIC = 2;

    public HeaderTable() : base(TAG)
    {
    }

    private float _version;
private float _fontRevision;
private uint _checkSumAdjustment;
private uint _magicNumber;
private int _flags;
private int _unitsPerEm;
private DateTimeOffset _created;
private DateTimeOffset _modified;
private short _xMin;
private short _yMin;
private short _xMax;
private short _yMax;
private int _macStyle;
private int _lowestRecPPEM;
private short _fontDirectionHint;
private short _indexToLocFormat;
private short _glyphDataFormat;
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

    public uint GetCheckSumAdjustment() => _checkSumAdjustment;
    public void SetCheckSumAdjustment(uint value) => _checkSumAdjustment = value;
    public DateTimeOffset GetCreated() => _created;
    public void SetCreated(DateTimeOffset value) => _created = value;
    public int GetFlags() => _flags;
    public void SetFlags(int value) => _flags = value;
    public short GetFontDirectionHint() => _fontDirectionHint;
    public void SetFontDirectionHint(short value) => _fontDirectionHint = value;
    public float GetFontRevision() => _fontRevision;
    public void SetFontRevision(float value) => _fontRevision = value;
    public short GetGlyphDataFormat() => _glyphDataFormat;
    public void SetGlyphDataFormat(short value) => _glyphDataFormat = value;
    public short GetIndexToLocFormat() => _indexToLocFormat;
    public void SetIndexToLocFormat(short value) => _indexToLocFormat = value;
    public int GetLowestRecPPEM() => _lowestRecPPEM;
    public void SetLowestRecPPEM(int value) => _lowestRecPPEM = value;
    public int GetMacStyle() => _macStyle;
    public void SetMacStyle(int value) => _macStyle = value;
    public uint GetMagicNumber() => _magicNumber;
    public void SetMagicNumber(uint value) => _magicNumber = value;
    public DateTimeOffset GetModified() => _modified;
    public void SetModified(DateTimeOffset value) => _modified = value;
    public int GetUnitsPerEm() => _unitsPerEm;
    public void SetUnitsPerEm(int value) => _unitsPerEm = value;
    public float GetVersion() => _version;
    public void SetVersion(float value) => _version = value;
    public short GetXMax() => _xMax;
    public void SetXMax(short value) => _xMax = value;
    public short GetXMin() => _xMin;
    public void SetXMin(short value) => _xMin = value;
    public short GetYMax() => _yMax;
    public void SetYMax(short value) => _yMax = value;
    public short GetYMin() => _yMin;
    public void SetYMin(short value) => _yMin = value;
}
