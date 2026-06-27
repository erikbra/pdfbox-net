/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * .NET facade properties for Java-style accessor methods.
 *
 * PORT_MODE: native-adapter
 * PORT_ADAPTER_FOR: fontbox/src/main/java/org/apache/fontbox/ttf/HeaderTable.java
 */

namespace PdfBox.Net.FontBox.TTF;

public sealed partial class HeaderTable
{
    public uint CheckSumAdjustment
    {
        get => GetCheckSumAdjustment();
        set => SetCheckSumAdjustment(value);
    }

    public DateTimeOffset Created
    {
        get => GetCreated();
        set => SetCreated(value);
    }

    public int Flags
    {
        get => GetFlags();
        set => SetFlags(value);
    }

    public short FontDirectionHint
    {
        get => GetFontDirectionHint();
        set => SetFontDirectionHint(value);
    }

    public float FontRevision
    {
        get => GetFontRevision();
        set => SetFontRevision(value);
    }

    public short GlyphDataFormat
    {
        get => GetGlyphDataFormat();
        set => SetGlyphDataFormat(value);
    }

    public short IndexToLocFormat
    {
        get => GetIndexToLocFormat();
        set => SetIndexToLocFormat(value);
    }

    public int LowestRecPPEM
    {
        get => GetLowestRecPPEM();
        set => SetLowestRecPPEM(value);
    }

    public int MacStyle
    {
        get => GetMacStyle();
        set => SetMacStyle(value);
    }

    public uint MagicNumber
    {
        get => GetMagicNumber();
        set => SetMagicNumber(value);
    }

    public DateTimeOffset Modified
    {
        get => GetModified();
        set => SetModified(value);
    }

    public int UnitsPerEm
    {
        get => GetUnitsPerEm();
        set => SetUnitsPerEm(value);
    }

    public float Version
    {
        get => GetVersion();
        set => SetVersion(value);
    }

    public short XMax
    {
        get => GetXMax();
        set => SetXMax(value);
    }

    public short XMin
    {
        get => GetXMin();
        set => SetXMin(value);
    }

    public short YMax
    {
        get => GetYMax();
        set => SetYMax(value);
    }

    public short YMin
    {
        get => GetYMin();
        set => SetYMin(value);
    }
}
