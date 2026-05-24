/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted PANOSE wrapper for font descriptor classification bytes.
 *
 * PORT_MODE: adapted
 */

namespace PdfBox.Net.PDModel.Font;

public sealed class PDPanose
{
    public const int PanoseLength = 10;

    private readonly byte[] _bytes;

    public PDPanose(byte[] bytes)
    {
        _bytes = bytes is { Length: PanoseLength } ? (byte[])bytes.Clone() : throw new ArgumentException($"PANOSE requires exactly {PanoseLength} bytes.", nameof(bytes));
    }

    public byte FamilyKind => _bytes[0];
    public byte SerifStyle => _bytes[1];
    public byte Weight => _bytes[2];
    public byte Proportion => _bytes[3];
    public byte Contrast => _bytes[4];
    public byte StrokeVariation => _bytes[5];
    public byte ArmStyle => _bytes[6];
    public byte Letterform => _bytes[7];
    public byte Midline => _bytes[8];
    public byte XHeight => _bytes[9];

    public byte[] GetBytes() => (byte[])_bytes.Clone();
}
