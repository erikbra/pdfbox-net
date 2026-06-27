/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/OS2WindowsMetricsTable.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: mechanical
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

using System.IO;

namespace PdfBox.Net.FontBox.TTF;

public partial class OS2WindowsMetricsTable() : TTFTable(TAG)
{
    public const int WEIGHT_CLASS_THIN = 100;
    public const int WEIGHT_CLASS_ULTRA_LIGHT = 200;
    public const int WEIGHT_CLASS_LIGHT = 300;
    public const int WEIGHT_CLASS_NORMAL = 400;
    public const int WEIGHT_CLASS_MEDIUM = 500;
    public const int WEIGHT_CLASS_SEMI_BOLD = 600;
    public const int WEIGHT_CLASS_BOLD = 700;
    public const int WEIGHT_CLASS_EXTRA_BOLD = 800;
    public const int WEIGHT_CLASS_BLACK = 900;

    public const int WIDTH_CLASS_ULTRA_CONDENSED = 1;
    public const int WIDTH_CLASS_EXTRA_CONDENSED = 2;
    public const int WIDTH_CLASS_CONDENSED = 3;
    public const int WIDTH_CLASS_SEMI_CONDENSED = 4;
    public const int WIDTH_CLASS_MEDIUM = 5;
    public const int WIDTH_CLASS_SEMI_EXPANDED = 6;
    public const int WIDTH_CLASS_EXPANDED = 7;
    public const int WIDTH_CLASS_EXTRA_EXPANDED = 8;
    public const int WIDTH_CLASS_ULTRA_EXPANDED = 9;

    public const int FAMILY_CLASS_NO_CLASSIFICATION = 0;
    public const int FAMILY_CLASS_OLDSTYLE_SERIFS = 1;
    public const int FAMILY_CLASS_TRANSITIONAL_SERIFS = 2;
    public const int FAMILY_CLASS_MODERN_SERIFS = 3;
    public const int FAMILY_CLASS_CLAREDON_SERIFS = 4;
    public const int FAMILY_CLASS_SLAB_SERIFS = 5;
    public const int FAMILY_CLASS_FREEFORM_SERIFS = 7;
    public const int FAMILY_CLASS_SANS_SERIF = 8;
    public const int FAMILY_CLASS_ORNAMENTALS = 9;
    public const int FAMILY_CLASS_SCRIPTS = 10;
    public const int FAMILY_CLASS_SYMBOLIC = 12;

    public const short FSTYPE_RESTRICTED = 0x0002;
    public const short FSTYPE_PREVIEW_AND_PRINT = 0x0004;
    public const short FSTYPE_EDITIBLE = 0x0008;
    public const short FSTYPE_NO_SUBSETTING = 0x0100;
    public const short FSTYPE_BITMAP_ONLY = 0x0200;

    public const string TAG = "OS/2";

    private int _version;
private short _averageCharWidth;
private int _weightClass;
private int _widthClass;
private short _fsType;
private short _subscriptXSize;
private short _subscriptYSize;
private short _subscriptXOffset;
private short _subscriptYOffset;
private short _superscriptXSize;
private short _superscriptYSize;
private short _superscriptXOffset;
private short _superscriptYOffset;
private short _strikeoutSize;
private short _strikeoutPosition;
private int _familyClass;
private byte[] _panose = new byte[10];

    private long _unicodeRange1;
private long _unicodeRange2;
private long _unicodeRange3;
private long _unicodeRange4;
private string _achVendId = "XXXX";

    private int _fsSelection;
private int _firstCharIndex;
private int _lastCharIndex;
private int _typoAscender;
private int _typoDescender;
private int _typoLineGap;
private int _winAscent;
private int _winDescent;
private long _codePageRange1;
private long _codePageRange2;
public int SxHeight { get; set; }
    public int SCapHeight { get; set; }
    public int UsDefaultChar { get; set; }
    public int UsBreakChar { get; set; }
    public int UsMaxContext { get; set; }

    public string GetAchVendId() => _achVendId;
    public void SetAchVendId(string achVendIdValue) => _achVendId = achVendIdValue;
    public short GetAverageCharWidth() => _averageCharWidth;
    public void SetAverageCharWidth(short averageCharWidthValue) => _averageCharWidth = averageCharWidthValue;
    public long GetCodePageRange1() => _codePageRange1;
    public void SetCodePageRange1(long codePageRange1Value) => _codePageRange1 = codePageRange1Value;
    public long GetCodePageRange2() => _codePageRange2;
    public void SetCodePageRange2(long codePageRange2Value) => _codePageRange2 = codePageRange2Value;
    public int GetFamilyClass() => _familyClass;
    public void SetFamilyClass(int familyClassValue) => _familyClass = familyClassValue;
    public int GetFirstCharIndex() => _firstCharIndex;
    public void SetFirstCharIndex(int firstCharIndexValue) => _firstCharIndex = firstCharIndexValue;
    public int GetFsSelection() => _fsSelection;
    public void SetFsSelection(int fsSelectionValue) => _fsSelection = fsSelectionValue;
    public short GetFsType() => _fsType;
    public void SetFsType(short fsTypeValue) => _fsType = fsTypeValue;
    public int GetLastCharIndex() => _lastCharIndex;
    public void SetLastCharIndex(int lastCharIndexValue) => _lastCharIndex = lastCharIndexValue;
    public byte[] GetPanose() => _panose;
    public void SetPanose(byte[] panoseValue) => _panose = panoseValue;
    public short GetStrikeoutPosition() => _strikeoutPosition;
    public void SetStrikeoutPosition(short strikeoutPositionValue) => _strikeoutPosition = strikeoutPositionValue;
    public short GetStrikeoutSize() => _strikeoutSize;
    public void SetStrikeoutSize(short strikeoutSizeValue) => _strikeoutSize = strikeoutSizeValue;
    public short GetSubscriptXOffset() => _subscriptXOffset;
    public void SetSubscriptXOffset(short subscriptXOffsetValue) => _subscriptXOffset = subscriptXOffsetValue;
    public short GetSubscriptXSize() => _subscriptXSize;
    public void SetSubscriptXSize(short subscriptXSizeValue) => _subscriptXSize = subscriptXSizeValue;
    public short GetSubscriptYOffset() => _subscriptYOffset;
    public void SetSubscriptYOffset(short subscriptYOffsetValue) => _subscriptYOffset = subscriptYOffsetValue;
    public short GetSubscriptYSize() => _subscriptYSize;
    public void SetSubscriptYSize(short subscriptYSizeValue) => _subscriptYSize = subscriptYSizeValue;
    public short GetSuperscriptXOffset() => _superscriptXOffset;
    public void SetSuperscriptXOffset(short superscriptXOffsetValue) => _superscriptXOffset = superscriptXOffsetValue;
    public short GetSuperscriptXSize() => _superscriptXSize;
    public void SetSuperscriptXSize(short superscriptXSizeValue) => _superscriptXSize = superscriptXSizeValue;
    public short GetSuperscriptYOffset() => _superscriptYOffset;
    public void SetSuperscriptYOffset(short superscriptYOffsetValue) => _superscriptYOffset = superscriptYOffsetValue;
    public short GetSuperscriptYSize() => _superscriptYSize;
    public void SetSuperscriptYSize(short superscriptYSizeValue) => _superscriptYSize = superscriptYSizeValue;
    public int GetTypoLineGap() => _typoLineGap;
    public void SetTypoLineGap(int typeLineGapValue) => _typoLineGap = typeLineGapValue;
    public int GetTypoAscender() => _typoAscender;
    public void SetTypoAscender(int typoAscenderValue) => _typoAscender = typoAscenderValue;
    public int GetTypoDescender() => _typoDescender;
    public void SetTypoDescender(int typoDescenderValue) => _typoDescender = typoDescenderValue;
    public long GetUnicodeRange1() => _unicodeRange1;
    public void SetUnicodeRange1(long unicodeRange1Value) => _unicodeRange1 = unicodeRange1Value;
    public long GetUnicodeRange2() => _unicodeRange2;
    public void SetUnicodeRange2(long unicodeRange2Value) => _unicodeRange2 = unicodeRange2Value;
    public long GetUnicodeRange3() => _unicodeRange3;
    public void SetUnicodeRange3(long unicodeRange3Value) => _unicodeRange3 = unicodeRange3Value;
    public long GetUnicodeRange4() => _unicodeRange4;
    public void SetUnicodeRange4(long unicodeRange4Value) => _unicodeRange4 = unicodeRange4Value;
    public int GetVersion() => _version;
    public void SetVersion(int versionValue) => _version = versionValue;
    public int GetWeightClass() => _weightClass;
    public void SetWeightClass(int weightClassValue) => _weightClass = weightClassValue;
    public int GetWidthClass() => _widthClass;
    public void SetWidthClass(int widthClassValue) => _widthClass = widthClassValue;
    public int GetWinAscent() => _winAscent;
    public void SetWinAscent(int winAscentValue) => _winAscent = winAscentValue;
    public int GetWinDescent() => _winDescent;
    public void SetWinDescent(int winDescentValue) => _winDescent = winDescentValue;
    public int GetHeight() => SxHeight;
    public int GetCapHeight() => SCapHeight;
    public int GetDefaultChar() => UsDefaultChar;
    public int GetBreakChar() => UsBreakChar;
    public int GetMaxContext() => UsMaxContext;

    internal override void Read(TrueTypeFont ttf, TTFDataStream data)
    {
        Version = data.ReadUnsignedShort();
        AverageCharWidth = data.ReadSignedShort();
        WeightClass = data.ReadUnsignedShort();
        WidthClass = data.ReadUnsignedShort();
        FsType = data.ReadSignedShort();
        SubscriptXSize = data.ReadSignedShort();
        SubscriptYSize = data.ReadSignedShort();
        SubscriptXOffset = data.ReadSignedShort();
        SubscriptYOffset = data.ReadSignedShort();
        SuperscriptXSize = data.ReadSignedShort();
        SuperscriptYSize = data.ReadSignedShort();
        SuperscriptXOffset = data.ReadSignedShort();
        SuperscriptYOffset = data.ReadSignedShort();
        StrikeoutSize = data.ReadSignedShort();
        StrikeoutPosition = data.ReadSignedShort();
        FamilyClass = data.ReadSignedShort();
        Panose = data.Read(10);
        UnicodeRange1 = data.ReadUnsignedInt();
        UnicodeRange2 = data.ReadUnsignedInt();
        UnicodeRange3 = data.ReadUnsignedInt();
        UnicodeRange4 = data.ReadUnsignedInt();
        AchVendId = data.ReadString(4);
        FsSelection = data.ReadUnsignedShort();
        FirstCharIndex = data.ReadUnsignedShort();
        LastCharIndex = data.ReadUnsignedShort();
        try
        {
            TypoAscender = data.ReadSignedShort();
            TypoDescender = data.ReadSignedShort();
            TypoLineGap = data.ReadSignedShort();
            WinAscent = data.ReadUnsignedShort();
            WinDescent = data.ReadUnsignedShort();
        }
        catch (EndOfStreamException)
        {
            initialized = true;
            return;
        }

        if (Version >= 1)
        {
            try
            {
                CodePageRange1 = data.ReadUnsignedInt();
                CodePageRange2 = data.ReadUnsignedInt();
            }
            catch (EndOfStreamException)
            {
                Version = 0;
                initialized = true;
                return;
            }
        }

        if (Version >= 2)
        {
            try
            {
                SxHeight = data.ReadSignedShort();
                SCapHeight = data.ReadSignedShort();
                UsDefaultChar = data.ReadUnsignedShort();
                UsBreakChar = data.ReadUnsignedShort();
                UsMaxContext = data.ReadUnsignedShort();
            }
            catch (EndOfStreamException)
            {
                Version = 1;
                initialized = true;
                return;
            }
        }

        initialized = true;
    }
}
