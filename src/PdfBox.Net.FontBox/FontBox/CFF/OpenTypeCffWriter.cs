using System.Buffers.Binary;
using System.Text;
using PdfBox.Net.FontBox.Util;

namespace PdfBox.Net.FontBox.CFF;

/// <summary>
/// Wraps a PDF Type 1C CFF program in the minimal OpenType tables required by web browsers.
/// </summary>
/// <remarks>
/// PDF stores Type 1C data as a raw CFF stream. Browsers require the same CFF bytes to be
/// packaged in an OpenType container with metrics and a Unicode cmap.
/// </remarks>
public static class OpenTypeCffWriter
{
    private const uint CheckSumMagic = 0xB1B0AFBA;
    private const ushort UnitsPerEm = 1000;

    /// <summary>
    /// Creates an OpenType CFF program for a simple PDF Type 1C font.
    /// </summary>
    /// <param name="font">The parsed raw CFF font.</param>
    /// <param name="rawCff">The original raw CFF program.</param>
    /// <param name="familyName">The PDF font name used as the generated face identity.</param>
    /// <param name="unicodeToGlyphId">Single-code-point Unicode mappings to CFF glyph IDs.</param>
    /// <param name="fontWeight">The CSS/OpenType weight class.</param>
    /// <param name="italic">Whether the font is italic or oblique.</param>
    /// <param name="program">The generated OpenType program when conversion succeeds.</param>
    /// <param name="failureReason">The conversion limitation when conversion fails.</param>
    /// <returns><see langword="true"/> when a browser-loadable OpenType program was created.</returns>
    public static bool TryCreate(
        CFFType1Font font,
        ReadOnlySpan<byte> rawCff,
        string familyName,
        IReadOnlyDictionary<int, int> unicodeToGlyphId,
        int fontWeight,
        bool italic,
        out byte[] program,
        out string? failureReason)
    {
        ArgumentNullException.ThrowIfNull(font);
        ArgumentException.ThrowIfNullOrWhiteSpace(familyName);
        ArgumentNullException.ThrowIfNull(unicodeToGlyphId);

        program = [];
        failureReason = null;
        if (rawCff.Length < 4 || rawCff[0] != 1)
        {
            failureReason = "The FontFile3 stream is not a supported CFF version 1 program.";
            return false;
        }

        int glyphCount = font.GetNumCharStrings();
        if (glyphCount < 1 || glyphCount > ushort.MaxValue)
        {
            failureReason = "The CFF glyph count is outside the OpenType range.";
            return false;
        }

        SortedDictionary<int, int> cmap = new(unicodeToGlyphId
            .Where(static mapping => mapping.Key is > 0 and <= 0x10FFFF && mapping.Value is > 0 and <= ushort.MaxValue)
            .Where(mapping => mapping.Value < glyphCount)
            .GroupBy(static mapping => mapping.Key)
            .ToDictionary(static group => group.Key, static group => group.First().Value));
        if (cmap.Count == 0)
        {
            failureReason = "The PDF font has no single-code-point Unicode mappings for a browser cmap.";
            return false;
        }

        int[] advances = GetAdvances(font, glyphCount);
        BoundingBox bounds = font.GetFontBBox();
        int weight = Math.Clamp(fontWeight, 100, 900);
        Dictionary<string, byte[]> tables = new(StringComparer.Ordinal)
        {
            ["CFF "] = rawCff.ToArray(),
            ["OS/2"] = BuildOs2Table(bounds, advances, cmap, weight, italic),
            ["cmap"] = BuildCmapTable(cmap),
            ["head"] = BuildHeadTable(bounds, weight, italic),
            ["hhea"] = BuildHheaTable(bounds, advances),
            ["hmtx"] = BuildHmtxTable(advances),
            ["maxp"] = BuildMaxpTable(glyphCount),
            ["name"] = BuildNameTable(familyName, weight, italic),
            ["post"] = BuildPostTable(italic)
        };

        program = BuildSfnt(tables);
        return true;
    }

    private static int[] GetAdvances(CFFType1Font font, int glyphCount)
    {
        int[] advances = new int[glyphCount];
        for (int glyphId = 0; glyphId < glyphCount; glyphId++)
        {
            try
            {
                advances[glyphId] = Math.Clamp((int)MathF.Round(font.GetType2CharString(glyphId).GetWidth()), 0, ushort.MaxValue);
            }
            catch (IOException)
            {
                advances[glyphId] = UnitsPerEm;
            }
        }

        return advances;
    }

    private static byte[] BuildSfnt(IReadOnlyDictionary<string, byte[]> tables)
    {
        KeyValuePair<string, byte[]>[] orderedTables = tables.OrderBy(static pair => pair.Key, StringComparer.Ordinal).ToArray();
        int tableCount = orderedTables.Length;
        int directoryLength = 12 + tableCount * 16;
        int offset = directoryLength;
        List<TableRecord> records = new(tableCount);
        foreach ((string tag, byte[] tableData) in orderedTables)
        {
            records.Add(new TableRecord(tag, offset, tableData));
            offset += AlignedLength(tableData.Length);
        }

        using MemoryStream output = new(offset);
        WriteTag(output, "OTTO");
        WriteUInt16(output, (ushort)tableCount);
        int largestPowerOfTwo = LargestPowerOfTwo(tableCount);
        WriteUInt16(output, (ushort)(largestPowerOfTwo * 16));
        WriteUInt16(output, (ushort)Log2(largestPowerOfTwo));
        WriteUInt16(output, (ushort)(tableCount * 16 - largestPowerOfTwo * 16));

        foreach (TableRecord record in records)
        {
            WriteTag(output, record.Tag);
            WriteUInt32(output, Checksum(record.Data));
            WriteUInt32(output, (uint)record.Offset);
            WriteUInt32(output, (uint)record.Data.Length);
        }

        foreach (TableRecord record in records)
        {
            output.Write(record.Data);
            WritePadding(output, record.Data.Length);
        }

        byte[] data = output.ToArray();
        TableRecord head = records.Single(static record => record.Tag == "head");
        uint adjustment = unchecked(CheckSumMagic - Checksum(data));
        BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan(head.Offset + 8, sizeof(uint)), adjustment);
        return data;
    }

    private static byte[] BuildHeadTable(BoundingBox bounds, int weight, bool italic)
    {
        using MemoryStream output = new(54);
        WriteUInt32(output, 0x00010000);
        WriteUInt32(output, 0x00010000);
        WriteUInt32(output, 0);
        WriteUInt32(output, 0x5F0F3CF5);
        WriteUInt16(output, 0x000B);
        WriteUInt16(output, UnitsPerEm);
        WriteUInt64(output, 0);
        WriteUInt64(output, 0);
        WriteInt16(output, ToInt16(bounds.GetLowerLeftX()));
        WriteInt16(output, ToInt16(bounds.GetLowerLeftY()));
        WriteInt16(output, ToInt16(bounds.GetUpperRightX()));
        WriteInt16(output, ToInt16(bounds.GetUpperRightY()));
        WriteUInt16(output, (ushort)((weight >= 600 ? 1 : 0) | (italic ? 2 : 0)));
        WriteUInt16(output, 8);
        WriteInt16(output, 2);
        WriteInt16(output, 0);
        WriteInt16(output, 0);
        return output.ToArray();
    }

    private static byte[] BuildHheaTable(BoundingBox bounds, IReadOnlyList<int> advances)
    {
        int ascender = Math.Max(0, (int)ToInt16(bounds.GetUpperRightY()));
        int descender = Math.Min(0, (int)ToInt16(bounds.GetLowerLeftY()));
        int advanceWidthMax = advances.Count == 0 ? UnitsPerEm : advances.Max();
        using MemoryStream output = new(36);
        WriteUInt32(output, 0x00010000);
        WriteInt16(output, (short)ascender);
        WriteInt16(output, (short)descender);
        WriteInt16(output, 0);
        WriteUInt16(output, (ushort)advanceWidthMax);
        WriteInt16(output, 0);
        WriteInt16(output, 0);
        WriteInt16(output, ToInt16(bounds.GetUpperRightX()));
        WriteInt16(output, 1);
        WriteInt16(output, 0);
        WriteInt16(output, 0);
        WriteInt16(output, 0);
        WriteInt16(output, 0);
        WriteInt16(output, 0);
        WriteInt16(output, 0);
        WriteInt16(output, 0);
        WriteUInt16(output, (ushort)advances.Count);
        return output.ToArray();
    }

    private static byte[] BuildHmtxTable(IReadOnlyList<int> advances)
    {
        using MemoryStream output = new(advances.Count * 4);
        foreach (int advance in advances)
        {
            WriteUInt16(output, (ushort)Math.Clamp(advance, 0, ushort.MaxValue));
            WriteInt16(output, 0);
        }

        return output.ToArray();
    }

    private static byte[] BuildMaxpTable(int glyphCount)
    {
        using MemoryStream output = new(6);
        WriteUInt32(output, 0x00005000);
        WriteUInt16(output, (ushort)glyphCount);
        return output.ToArray();
    }

    private static byte[] BuildCmapTable(IReadOnlyDictionary<int, int> cmap)
    {
        using MemoryStream format12 = new();
        WriteUInt16(format12, 12);
        WriteUInt16(format12, 0);
        WriteUInt32(format12, (uint)(16 + cmap.Count * 12));
        WriteUInt32(format12, 0);
        WriteUInt32(format12, (uint)cmap.Count);
        foreach ((int codePoint, int glyphId) in cmap)
        {
            WriteUInt32(format12, (uint)codePoint);
            WriteUInt32(format12, (uint)codePoint);
            WriteUInt32(format12, (uint)glyphId);
        }

        using MemoryStream output = new();
        WriteUInt16(output, 0);
        WriteUInt16(output, 1);
        WriteUInt16(output, 3);
        WriteUInt16(output, 10);
        WriteUInt32(output, 12);
        output.Write(format12.ToArray());
        return output.ToArray();
    }

    private static byte[] BuildNameTable(string familyName, int weight, bool italic)
    {
        string family = RemoveSubsetPrefix(familyName);
        string style = italic ? "Italic" : weight >= 600 ? "Bold" : "Regular";
        string fullName = style == "Regular" ? family : family + " " + style;
        string postScriptName = new(family.Where(static character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_').ToArray());
        if (string.IsNullOrWhiteSpace(postScriptName))
        {
            postScriptName = "PdfBoxEmbeddedCff";
        }

        NameEntry[] entries =
        [
            new(1, family),
            new(2, style),
            new(4, fullName),
            new(6, postScriptName)
        ];
        byte[][] encoded = entries.Select(static entry => System.Text.Encoding.BigEndianUnicode.GetBytes(entry.Value)).ToArray();
        ushort stringOffset = (ushort)(6 + entries.Length * 12);
        using MemoryStream output = new();
        WriteUInt16(output, 0);
        WriteUInt16(output, (ushort)entries.Length);
        WriteUInt16(output, stringOffset);
        int offset = 0;
        for (int index = 0; index < entries.Length; index++)
        {
            WriteUInt16(output, 3);
            WriteUInt16(output, 1);
            WriteUInt16(output, 0x0409);
            WriteUInt16(output, (ushort)entries[index].NameId);
            WriteUInt16(output, (ushort)encoded[index].Length);
            WriteUInt16(output, (ushort)offset);
            offset += encoded[index].Length;
        }

        foreach (byte[] bytes in encoded)
        {
            output.Write(bytes);
        }

        return output.ToArray();
    }

    private static byte[] BuildOs2Table(
        BoundingBox bounds,
        IReadOnlyList<int> advances,
        IReadOnlyDictionary<int, int> cmap,
        int weight,
        bool italic)
    {
        int ascender = Math.Max(0, (int)ToInt16(bounds.GetUpperRightY()));
        int descender = Math.Min(0, (int)ToInt16(bounds.GetLowerLeftY()));
        int averageWidth = advances.Count == 0 ? UnitsPerEm : (int)Math.Round(advances.Average());
        int firstCharacter = cmap.Keys.Min();
        int lastCharacter = cmap.Keys.Max();
        using MemoryStream output = new(78);
        WriteUInt16(output, 0);
        WriteInt16(output, ToInt16(averageWidth));
        WriteUInt16(output, (ushort)weight);
        WriteUInt16(output, 5);
        WriteUInt16(output, 0);
        for (int index = 0; index < 8; index++)
        {
            WriteInt16(output, 0);
        }

        for (int index = 0; index < 3; index++)
        {
            WriteInt16(output, 0);
        }
        output.Write(new byte[10]);
        WriteUInt32(output, 0);
        WriteUInt32(output, 0);
        WriteUInt32(output, 0);
        WriteUInt32(output, 0);
        WriteTag(output, "PDFB");
        WriteUInt16(output, italic ? (ushort)1 : (ushort)0);
        WriteUInt16(output, (ushort)Math.Min(firstCharacter, ushort.MaxValue));
        WriteUInt16(output, (ushort)Math.Min(lastCharacter, ushort.MaxValue));
        WriteInt16(output, (short)ascender);
        WriteInt16(output, (short)descender);
        WriteInt16(output, 0);
        WriteUInt16(output, (ushort)ascender);
        WriteUInt16(output, (ushort)Math.Abs(descender));
        return output.ToArray();
    }

    private static byte[] BuildPostTable(bool italic)
    {
        using MemoryStream output = new(32);
        WriteUInt32(output, 0x00030000);
        WriteUInt32(output, italic ? 0xFFF40000 : 0);
        WriteInt16(output, -100);
        WriteInt16(output, 50);
        WriteUInt32(output, 0);
        WriteUInt32(output, 0);
        WriteUInt32(output, 0);
        WriteUInt32(output, 0);
        WriteUInt32(output, 0);
        return output.ToArray();
    }

    private static string RemoveSubsetPrefix(string name)
    {
        int plusIndex = name.IndexOf('+');
        return plusIndex is > 0 && plusIndex <= 6 && name[..plusIndex].All(static character => character is >= 'A' and <= 'Z')
            ? name[(plusIndex + 1)..]
            : name;
    }

    private static int AlignedLength(int length) => (length + 3) & ~3;

    private static int LargestPowerOfTwo(int value)
    {
        int result = 1;
        while (result * 2 <= value)
        {
            result *= 2;
        }

        return result;
    }

    private static int Log2(int value)
    {
        int result = 0;
        while (value > 1)
        {
            value >>= 1;
            result++;
        }

        return result;
    }

    private static uint Checksum(ReadOnlySpan<byte> data)
    {
        uint checksum = 0;
        for (int offset = 0; offset < data.Length; offset += 4)
        {
            uint value = 0;
            for (int byteIndex = 0; byteIndex < 4 && offset + byteIndex < data.Length; byteIndex++)
            {
                value |= (uint)data[offset + byteIndex] << (24 - byteIndex * 8);
            }

            checksum = unchecked(checksum + value);
        }

        return checksum;
    }

    private static short ToInt16(float value) => (short)Math.Clamp(MathF.Round(value), short.MinValue, short.MaxValue);

    private static void WritePadding(Stream output, int length)
    {
        int padding = AlignedLength(length) - length;
        if (padding > 0)
        {
            output.Write(new byte[padding]);
        }
    }

    private static void WriteTag(Stream output, string tag)
    {
        if (tag.Length != 4 || tag.Any(static character => character > 0x7F))
        {
            throw new ArgumentException("OpenType tags must contain four ASCII characters.", nameof(tag));
        }

        output.Write(System.Text.Encoding.ASCII.GetBytes(tag));
    }

    private static void WriteUInt16(Stream output, ushort value)
    {
        output.WriteByte((byte)(value >> 8));
        output.WriteByte((byte)value);
    }

    private static void WriteInt16(Stream output, short value) => WriteUInt16(output, unchecked((ushort)value));

    private static void WriteUInt32(Stream output, uint value)
    {
        output.WriteByte((byte)(value >> 24));
        output.WriteByte((byte)(value >> 16));
        output.WriteByte((byte)(value >> 8));
        output.WriteByte((byte)value);
    }

    private static void WriteUInt64(Stream output, ulong value)
    {
        WriteUInt32(output, (uint)(value >> 32));
        WriteUInt32(output, (uint)value);
    }

    private readonly record struct TableRecord(string Tag, int Offset, byte[] Data);

    private readonly record struct NameEntry(int NameId, string Value);
}
