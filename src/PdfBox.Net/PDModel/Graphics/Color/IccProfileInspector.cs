/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 */

namespace PdfBox.Net.PDModel.Graphics.Color;

internal static class IccProfileInspector
{
    private const int HeaderLength = 128;

    internal static bool TryGetProfileComponents(byte[] profileData, out int components)
    {
        components = 0;
        if (!TryGetDeclaredLength(profileData, out int declaredLength))
        {
            return false;
        }

        ReadOnlySpan<byte> signature = profileData.AsSpan(16, 4);
        if (signature.SequenceEqual("GRAY"u8)) components = 1;
        else if (signature.SequenceEqual("RGB "u8)) components = 3;
        else if (signature.SequenceEqual("CMYK"u8)) components = 4;
        return components != 0;
    }

    internal static bool IsSrgb(byte[] profileData)
    {
        if (!TryGetDeclaredLength(profileData, out int declaredLength) ||
            !profileData.AsSpan(16, 4).SequenceEqual("RGB "u8) ||
            declaredLength < HeaderLength + 4)
        {
            return false;
        }

        uint tagCountValue = ReadUInt32BigEndian(profileData, HeaderLength);
        int availableTagCount = (declaredLength - HeaderLength - 4) / 12;
        int tagCount = (int)Math.Min(tagCountValue, (uint)availableTagCount);
        for (int index = 0; index < tagCount; index++)
        {
            int tagRecordOffset = HeaderLength + 4 + (index * 12);
            if (!profileData.AsSpan(tagRecordOffset, 4).SequenceEqual("desc"u8))
            {
                continue;
            }

            uint offsetValue = ReadUInt32BigEndian(profileData, tagRecordOffset + 4);
            uint sizeValue = ReadUInt32BigEndian(profileData, tagRecordOffset + 8);
            if (offsetValue > int.MaxValue || sizeValue > int.MaxValue)
            {
                continue;
            }

            int offset = (int)offsetValue;
            int size = (int)sizeValue;
            if (offset < 0 || size < 12 || offset > declaredLength - size)
            {
                continue;
            }

            string description = ReadProfileDescription(profileData.AsSpan(offset, size));
            if (IsSrgbDescription(description))
            {
                return true;
            }
        }

        return false;
    }

    private static string ReadProfileDescription(ReadOnlySpan<byte> tagData)
    {
        if (tagData[..4].SequenceEqual("desc"u8))
        {
            uint lengthValue = ReadUInt32BigEndian(tagData, 8);
            int availableLength = tagData.Length - 12;
            int length = (int)Math.Min(lengthValue, (uint)availableLength);
            if (length > 0 && tagData[12 + length - 1] == 0)
            {
                length--;
            }

            return System.Text.Encoding.ASCII.GetString(tagData.Slice(12, length));
        }

        if (tagData[..4].SequenceEqual("mluc"u8) && tagData.Length >= 28)
        {
            uint recordCountValue = ReadUInt32BigEndian(tagData, 8);
            uint recordSizeValue = ReadUInt32BigEndian(tagData, 12);
            if (recordSizeValue < 12 || recordSizeValue > int.MaxValue)
            {
                return string.Empty;
            }

            int recordSize = (int)recordSizeValue;
            int availableRecordCount = (tagData.Length - 16) / recordSize;
            int recordCount = (int)Math.Min(recordCountValue, (uint)availableRecordCount);
            for (int index = 0; index < recordCount; index++)
            {
                int recordOffset = 16 + (index * recordSize);
                uint lengthValue = ReadUInt32BigEndian(tagData, recordOffset + 4);
                uint offsetValue = ReadUInt32BigEndian(tagData, recordOffset + 8);
                if (lengthValue > int.MaxValue || offsetValue > int.MaxValue)
                {
                    continue;
                }

                int length = (int)lengthValue;
                int offset = (int)offsetValue;
                if ((length & 1) == 0 && length > 0 && offset >= 0 && offset <= tagData.Length - length)
                {
                    return System.Text.Encoding.BigEndianUnicode.GetString(tagData.Slice(offset, length));
                }
            }
        }

        return string.Empty;
    }

    private static bool IsSrgbDescription(string description)
    {
        string normalized = description.Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
        return normalized.Contains("SRGB", StringComparison.Ordinal) ||
               normalized.Contains("IEC61966-2.1", StringComparison.Ordinal) ||
               normalized.Contains("IEC61966-2-1", StringComparison.Ordinal);
    }

    private static bool TryGetDeclaredLength(byte[] profileData, out int declaredLength)
    {
        declaredLength = 0;
        if (profileData.Length < HeaderLength ||
            profileData[36] != (byte)'a' || profileData[37] != (byte)'c' ||
            profileData[38] != (byte)'s' || profileData[39] != (byte)'p')
        {
            return false;
        }

        uint declaredLengthValue = ReadUInt32BigEndian(profileData, 0);
        if (declaredLengthValue < HeaderLength ||
            declaredLengthValue > int.MaxValue ||
            declaredLengthValue > profileData.Length)
        {
            return false;
        }

        declaredLength = (int)declaredLengthValue;
        return true;
    }

    private static uint ReadUInt32BigEndian(ReadOnlySpan<byte> data, int offset)
    {
        return ((uint)data[offset] << 24) |
               ((uint)data[offset + 1] << 16) |
               ((uint)data[offset + 2] << 8) |
               data[offset + 3];
    }
}
