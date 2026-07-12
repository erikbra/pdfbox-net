/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 */

namespace PdfBox.Net.PDModel.Graphics.Color;

internal static class IccProfileInspector
{
    internal static bool TryGetProfileComponents(byte[] profileData, out int components)
    {
        components = 0;
        if (profileData.Length < 128 ||
            profileData[36] != (byte)'a' || profileData[37] != (byte)'c' ||
            profileData[38] != (byte)'s' || profileData[39] != (byte)'p')
        {
            return false;
        }

        uint declaredLength = ((uint)profileData[0] << 24) |
                              ((uint)profileData[1] << 16) |
                              ((uint)profileData[2] << 8) |
                              profileData[3];
        if (declaredLength < 128 || declaredLength > profileData.Length)
        {
            return false;
        }

        ReadOnlySpan<byte> signature = profileData.AsSpan(16, 4);
        if (signature.SequenceEqual("GRAY"u8)) components = 1;
        else if (signature.SequenceEqual("RGB "u8)) components = 3;
        else if (signature.SequenceEqual("CMYK"u8)) components = 4;
        return components != 0;
    }
}
