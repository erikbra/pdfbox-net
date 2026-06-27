/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: fontbox/src/main/java/org/apache/fontbox/ttf/OS2WindowsMetricsTable.java
 */

using System.IO;

namespace PdfBox.Net.FontBox.TTF;

public partial class OS2WindowsMetricsTable
{
    public string AchVendId
    {
        get => GetAchVendId();
        set => SetAchVendId(value);
    }

    public short AverageCharWidth
    {
        get => GetAverageCharWidth();
        set => SetAverageCharWidth(value);
    }

    public long CodePageRange1
    {
        get => GetCodePageRange1();
        set => SetCodePageRange1(value);
    }

    public long CodePageRange2
    {
        get => GetCodePageRange2();
        set => SetCodePageRange2(value);
    }

    public int FamilyClass
    {
        get => GetFamilyClass();
        set => SetFamilyClass(value);
    }

    public int FirstCharIndex
    {
        get => GetFirstCharIndex();
        set => SetFirstCharIndex(value);
    }

    public int FsSelection
    {
        get => GetFsSelection();
        set => SetFsSelection(value);
    }

    public short FsType
    {
        get => GetFsType();
        set => SetFsType(value);
    }

    public int LastCharIndex
    {
        get => GetLastCharIndex();
        set => SetLastCharIndex(value);
    }

    public byte[] Panose
    {
        get => GetPanose();
        set => SetPanose(value);
    }

    public short StrikeoutPosition
    {
        get => GetStrikeoutPosition();
        set => SetStrikeoutPosition(value);
    }

    public short StrikeoutSize
    {
        get => GetStrikeoutSize();
        set => SetStrikeoutSize(value);
    }

    public short SubscriptXOffset
    {
        get => GetSubscriptXOffset();
        set => SetSubscriptXOffset(value);
    }

    public short SubscriptXSize
    {
        get => GetSubscriptXSize();
        set => SetSubscriptXSize(value);
    }

    public short SubscriptYOffset
    {
        get => GetSubscriptYOffset();
        set => SetSubscriptYOffset(value);
    }

    public short SubscriptYSize
    {
        get => GetSubscriptYSize();
        set => SetSubscriptYSize(value);
    }

    public short SuperscriptXOffset
    {
        get => GetSuperscriptXOffset();
        set => SetSuperscriptXOffset(value);
    }

    public short SuperscriptXSize
    {
        get => GetSuperscriptXSize();
        set => SetSuperscriptXSize(value);
    }

    public short SuperscriptYOffset
    {
        get => GetSuperscriptYOffset();
        set => SetSuperscriptYOffset(value);
    }

    public short SuperscriptYSize
    {
        get => GetSuperscriptYSize();
        set => SetSuperscriptYSize(value);
    }

    public int TypoAscender
    {
        get => GetTypoAscender();
        set => SetTypoAscender(value);
    }

    public int TypoDescender
    {
        get => GetTypoDescender();
        set => SetTypoDescender(value);
    }

    public int TypoLineGap
    {
        get => GetTypoLineGap();
        set => SetTypoLineGap(value);
    }

    public long UnicodeRange1
    {
        get => GetUnicodeRange1();
        set => SetUnicodeRange1(value);
    }

    public long UnicodeRange2
    {
        get => GetUnicodeRange2();
        set => SetUnicodeRange2(value);
    }

    public long UnicodeRange3
    {
        get => GetUnicodeRange3();
        set => SetUnicodeRange3(value);
    }

    public long UnicodeRange4
    {
        get => GetUnicodeRange4();
        set => SetUnicodeRange4(value);
    }

    public int Version
    {
        get => GetVersion();
        set => SetVersion(value);
    }

    public int WeightClass
    {
        get => GetWeightClass();
        set => SetWeightClass(value);
    }

    public int WidthClass
    {
        get => GetWidthClass();
        set => SetWidthClass(value);
    }

    public int WinAscent
    {
        get => GetWinAscent();
        set => SetWinAscent(value);
    }

    public int WinDescent
    {
        get => GetWinDescent();
        set => SetWinDescent(value);
    }
}
