/*
 * Copyright (c) 2026 Erik A. Brandstadmoen.
 *
 * Port-local helper for writing glyph layout content stream operands.
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.PDModel;

internal static class GlyphLayoutContentStreamSupport
{
    internal static COSString ToGlyphCodeString(IEnumerable<int> glyphCodes)
    {
        List<byte> bytes = [];
        foreach (int glyphCode in glyphCodes)
        {
            bytes.Add((byte)((glyphCode >> 8) & 0xFF));
            bytes.Add((byte)(glyphCode & 0xFF));
        }

        return new COSString(bytes.ToArray(), forceHex: true);
    }

    internal static COSArray ToGlyphsAndPositionsArray(GlyphsAndPositions glyphsAndPositions)
    {
        ArgumentNullException.ThrowIfNull(glyphsAndPositions);

        COSArray array = new();
        foreach (object item in glyphsAndPositions.ToArray())
        {
            switch (item)
            {
                case GlyphsAndPositions.GlyphSubList glyphSubList:
                    array.Add(ToGlyphCodeString(glyphSubList));
                    break;
                case float position:
                    array.Add(new COSFloat(position));
                    break;
                default:
                    throw new ArgumentException(
                        $"Argument must consist of GlyphSubList and float entries, not {item?.GetType().FullName ?? "null"}.",
                        nameof(glyphsAndPositions));
            }
        }

        return array;
    }
}
