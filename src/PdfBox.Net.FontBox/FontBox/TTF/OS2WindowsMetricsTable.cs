/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/OS2WindowsMetricsTable.java
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

using System.IO;

namespace PdfBox.Net.FontBox.TTF;

public class OS2WindowsMetricsTable() : TTFTable(TAG)
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

    public int Version { get; set; }
    public short AverageCharWidth { get; set; }
    public int WeightClass { get; set; }
    public int WidthClass { get; set; }
    public short FsType { get; set; }
    public short SubscriptXSize { get; set; }
    public short SubscriptYSize { get; set; }
    public short SubscriptXOffset { get; set; }
    public short SubscriptYOffset { get; set; }
    public short SuperscriptXSize { get; set; }
    public short SuperscriptYSize { get; set; }
    public short SuperscriptXOffset { get; set; }
    public short SuperscriptYOffset { get; set; }
    public short StrikeoutSize { get; set; }
    public short StrikeoutPosition { get; set; }
    public int FamilyClass { get; set; }
    public byte[] Panose { get; set; } = new byte[10];
    public long UnicodeRange1 { get; set; }
    public long UnicodeRange2 { get; set; }
    public long UnicodeRange3 { get; set; }
    public long UnicodeRange4 { get; set; }
    public string AchVendId { get; set; } = "XXXX";
    public int FsSelection { get; set; }
    public int FirstCharIndex { get; set; }
    public int LastCharIndex { get; set; }
    public int TypoAscender { get; set; }
    public int TypoDescender { get; set; }
    public int TypoLineGap { get; set; }
    public int WinAscent { get; set; }
    public int WinDescent { get; set; }
    public long CodePageRange1 { get; set; }
    public long CodePageRange2 { get; set; }
    public int SxHeight { get; set; }
    public int SCapHeight { get; set; }
    public int UsDefaultChar { get; set; }
    public int UsBreakChar { get; set; }
    public int UsMaxContext { get; set; }

    public string GetAchVendId() => AchVendId;
    public void SetAchVendId(string achVendIdValue) => AchVendId = achVendIdValue;
    public short GetAverageCharWidth() => AverageCharWidth;
    public void SetAverageCharWidth(short averageCharWidthValue) => AverageCharWidth = averageCharWidthValue;
    public long GetCodePageRange1() => CodePageRange1;
    public void SetCodePageRange1(long codePageRange1Value) => CodePageRange1 = codePageRange1Value;
    public long GetCodePageRange2() => CodePageRange2;
    public void SetCodePageRange2(long codePageRange2Value) => CodePageRange2 = codePageRange2Value;
    public int GetFamilyClass() => FamilyClass;
    public void SetFamilyClass(int familyClassValue) => FamilyClass = familyClassValue;
    public int GetFirstCharIndex() => FirstCharIndex;
    public void SetFirstCharIndex(int firstCharIndexValue) => FirstCharIndex = firstCharIndexValue;
    public int GetFsSelection() => FsSelection;
    public void SetFsSelection(int fsSelectionValue) => FsSelection = fsSelectionValue;
    public short GetFsType() => FsType;
    public void SetFsType(short fsTypeValue) => FsType = fsTypeValue;
    public int GetLastCharIndex() => LastCharIndex;
    public void SetLastCharIndex(int lastCharIndexValue) => LastCharIndex = lastCharIndexValue;
    public byte[] GetPanose() => Panose;
    public void SetPanose(byte[] panoseValue) => Panose = panoseValue;
    public short GetStrikeoutPosition() => StrikeoutPosition;
    public void SetStrikeoutPosition(short strikeoutPositionValue) => StrikeoutPosition = strikeoutPositionValue;
    public short GetStrikeoutSize() => StrikeoutSize;
    public void SetStrikeoutSize(short strikeoutSizeValue) => StrikeoutSize = strikeoutSizeValue;
    public short GetSubscriptXOffset() => SubscriptXOffset;
    public void SetSubscriptXOffset(short subscriptXOffsetValue) => SubscriptXOffset = subscriptXOffsetValue;
    public short GetSubscriptXSize() => SubscriptXSize;
    public void SetSubscriptXSize(short subscriptXSizeValue) => SubscriptXSize = subscriptXSizeValue;
    public short GetSubscriptYOffset() => SubscriptYOffset;
    public void SetSubscriptYOffset(short subscriptYOffsetValue) => SubscriptYOffset = subscriptYOffsetValue;
    public short GetSubscriptYSize() => SubscriptYSize;
    public void SetSubscriptYSize(short subscriptYSizeValue) => SubscriptYSize = subscriptYSizeValue;
    public short GetSuperscriptXOffset() => SuperscriptXOffset;
    public void SetSuperscriptXOffset(short superscriptXOffsetValue) => SuperscriptXOffset = superscriptXOffsetValue;
    public short GetSuperscriptXSize() => SuperscriptXSize;
    public void SetSuperscriptXSize(short superscriptXSizeValue) => SuperscriptXSize = superscriptXSizeValue;
    public short GetSuperscriptYOffset() => SuperscriptYOffset;
    public void SetSuperscriptYOffset(short superscriptYOffsetValue) => SuperscriptYOffset = superscriptYOffsetValue;
    public short GetSuperscriptYSize() => SuperscriptYSize;
    public void SetSuperscriptYSize(short superscriptYSizeValue) => SuperscriptYSize = superscriptYSizeValue;
    public int GetTypoLineGap() => TypoLineGap;
    public void SetTypoLineGap(int typeLineGapValue) => TypoLineGap = typeLineGapValue;
    public int GetTypoAscender() => TypoAscender;
    public void SetTypoAscender(int typoAscenderValue) => TypoAscender = typoAscenderValue;
    public int GetTypoDescender() => TypoDescender;
    public void SetTypoDescender(int typoDescenderValue) => TypoDescender = typoDescenderValue;
    public long GetUnicodeRange1() => UnicodeRange1;
    public void SetUnicodeRange1(long unicodeRange1Value) => UnicodeRange1 = unicodeRange1Value;
    public long GetUnicodeRange2() => UnicodeRange2;
    public void SetUnicodeRange2(long unicodeRange2Value) => UnicodeRange2 = unicodeRange2Value;
    public long GetUnicodeRange3() => UnicodeRange3;
    public void SetUnicodeRange3(long unicodeRange3Value) => UnicodeRange3 = unicodeRange3Value;
    public long GetUnicodeRange4() => UnicodeRange4;
    public void SetUnicodeRange4(long unicodeRange4Value) => UnicodeRange4 = unicodeRange4Value;
    public int GetVersion() => Version;
    public void SetVersion(int versionValue) => Version = versionValue;
    public int GetWeightClass() => WeightClass;
    public void SetWeightClass(int weightClassValue) => WeightClass = weightClassValue;
    public int GetWidthClass() => WidthClass;
    public void SetWidthClass(int widthClassValue) => WidthClass = widthClassValue;
    public int GetWinAscent() => WinAscent;
    public void SetWinAscent(int winAscentValue) => WinAscent = winAscentValue;
    public int GetWinDescent() => WinDescent;
    public void SetWinDescent(int winDescentValue) => WinDescent = winDescentValue;
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
