using System.Globalization;
using System.Text;

namespace PdfBox.Net.Tests;

internal static class FontBoxTestFixtures
{
    public static byte[] CreateMinimalType1Pfb()
    {
        string segment1Text = string.Join('\n',
        [
            "%!FontType1-1.0: TestFont 1.0",
            "10 dict begin",
            "/FontName /TestFont def",
            "/FontInfo 5 dict dup begin",
            "/version (1.0) readonly def",
            "/Notice (Test notice) readonly def",
            "/FullName (Test Font) readonly def",
            "/FamilyName (Test Family) readonly def",
            "/Weight (Regular) readonly def",
            "end readonly def",
            "/isFixedPitch false def",
            "/ItalicAngle 0 def",
            "/UnderlinePosition -100 def",
            "/UnderlineThickness 50 def",
            "/Encoding 256 array",
            "dup 0 /.notdef put",
            "dup 65 /A put",
            "readonly def",
            "/FontBBox [0 0 500 700] readonly def",
            "/FontMatrix [0.001 0 0 0.001 0 0] readonly def",
            "currentdict end",
            "currentfile eexec",
        ]) + "\n";

        byte[] notdef = EncryptType1CharString([14]);
        byte[] glyphA = EncryptType1CharString([139, 14]);
        byte[] subr0 = EncryptType1CharString([11]);

        using MemoryStream clear = new();
        WriteAscii(clear, "/Private 8 dict dup begin\n");
        WriteAscii(clear, "/lenIV 4 def\n");
        WriteAscii(clear, "/BlueValues [0 10] def\n");
        WriteAscii(clear, "/ForceBold false def\n");
        WriteAscii(clear, "/Subrs 1 array\n");
        WriteAscii(clear, $"dup 0 {subr0.Length} RD ");
        clear.Write(subr0);
        WriteAscii(clear, " NP\n");
        WriteAscii(clear, "/CharStrings 2 dict dup begin\n");
        WriteAscii(clear, $"/.notdef {notdef.Length} RD ");
        clear.Write(notdef);
        WriteAscii(clear, " ND\n");
        WriteAscii(clear, $"/A {glyphA.Length} RD ");
        clear.Write(glyphA);
        WriteAscii(clear, " ND\n");
        WriteAscii(clear, "end\nend\ncleartomark\n");

        byte[] segment1 = Encoding.ASCII.GetBytes(segment1Text);
        byte[] segment2 = EncryptEexec(clear.ToArray());
        return BuildPfb(segment1, segment2);
    }

    public static byte[] CreateMinimalOpenTypeCff()
    {
        byte[] cff = CreateMinimalCff();
        int offset = 12 + 16;
        int paddedLength = (cff.Length + 3) & ~3;
        using MemoryStream stream = new();
        WriteAscii(stream, "OTTO");
        WriteUInt16(stream, 1);
        WriteUInt16(stream, 16);
        WriteUInt16(stream, 16);
        WriteUInt16(stream, 0);
        WriteAscii(stream, "CFF ");
        WriteUInt32(stream, 0);
        WriteUInt32(stream, (uint)offset);
        WriteUInt32(stream, (uint)cff.Length);
        stream.Write(cff);
        while (stream.Length < offset + paddedLength)
        {
            stream.WriteByte(0);
        }

        return stream.ToArray();
    }

    public static byte[] CreateMinimalCMap()
    {
        string cmap = string.Join('\n',
        [
            "/CIDInit /ProcSet findresource begin",
            "12 dict begin",
            "begincmap",
            "/CMapName /Test-CMap def",
            "/Registry (Adobe) def",
            "/Ordering (Identity) def",
            "/Supplement 0 def",
            "1 begincodespacerange",
            "<00> <FF>",
            "endcodespacerange",
            "1 beginbfchar",
            "<20> <0020>",
            "endbfchar",
            "1 begincidrange",
            "<30> <32> 100",
            "endcidrange",
            "1 begincidchar",
            "<40> 200",
            "endcidchar",
            "endcmap",
            "CMapName currentdict /CMap defineresource pop",
            "end",
            "end",
        ]) + "\n";

        return Encoding.ASCII.GetBytes(cmap);
    }

    public static byte[] CreateMinimalTrueType()
    {
        byte[] head = CreateHeadTable();
        byte[] maxp = CreateMaxpTable();
        byte[] name = CreateNameTable("MiniTTF");

        const int tableCount = 3;
        int directorySize = 12 + tableCount * 16;
        int headOffset = directorySize;
        int maxpOffset = headOffset + Align4(head.Length);
        int nameOffset = maxpOffset + Align4(maxp.Length);

        using MemoryStream stream = new();
        WriteUInt32(stream, 0x00010000);
        WriteUInt16(stream, (ushort)tableCount);
        WriteUInt16(stream, 32);
        WriteUInt16(stream, 1);
        WriteUInt16(stream, 16);

        WriteAscii(stream, "head");
        WriteUInt32(stream, 0);
        WriteUInt32(stream, (uint)headOffset);
        WriteUInt32(stream, (uint)head.Length);

        WriteAscii(stream, "maxp");
        WriteUInt32(stream, 0);
        WriteUInt32(stream, (uint)maxpOffset);
        WriteUInt32(stream, (uint)maxp.Length);

        WriteAscii(stream, "name");
        WriteUInt32(stream, 0);
        WriteUInt32(stream, (uint)nameOffset);
        WriteUInt32(stream, (uint)name.Length);

        stream.Write(head);
        WritePadding(stream, head.Length);
        stream.Write(maxp);
        WritePadding(stream, maxp.Length);
        stream.Write(name);
        WritePadding(stream, name.Length);
        return stream.ToArray();
    }

    private static byte[] CreateMinimalCff()
    {
        byte[] nameIndex = BuildIndex([Encoding.ASCII.GetBytes("MiniCFF")]);
        byte[] stringIndex = BuildIndex([]);
        byte[] globalSubrIndex = BuildIndex([]);
        byte[] privateDict = BuildDict(
            EncodeInteger(0),
            EncodeInteger(10),
            [6]);
        byte[] charStringsIndex = BuildIndex([[14], [14]]);
        byte[] topDict = [];
        while (true)
        {
            byte[] topDictIndex = BuildIndex([topDict]);
            int prefix = 4 + nameIndex.Length + topDictIndex.Length + stringIndex.Length + globalSubrIndex.Length;
            int charStringsOffset = prefix;
            int privateOffset = prefix + charStringsIndex.Length;
            byte[] nextTopDict = BuildDict(
                EncodeInteger(0), EncodeInteger(0), EncodeInteger(500), EncodeInteger(700), [5],
                EncodeInteger(charStringsOffset), [17],
                EncodeInteger(privateDict.Length), EncodeInteger(privateOffset), [18]);
            if (nextTopDict.SequenceEqual(topDict))
            {
                topDict = nextTopDict;
                break;
            }

            topDict = nextTopDict;
        }

        byte[] topDictIndexFinal = BuildIndex([topDict]);
        using MemoryStream stream = new();
        stream.Write([1, 0, 4, 1]);
        stream.Write(nameIndex);
        stream.Write(topDictIndexFinal);
        stream.Write(stringIndex);
        stream.Write(globalSubrIndex);
        stream.Write(charStringsIndex);
        stream.Write(privateDict);
        return stream.ToArray();
    }

    private static byte[] BuildDict(params byte[][] parts)
    {
        using MemoryStream stream = new();
        foreach (byte[] part in parts)
        {
            stream.Write(part);
        }

        return stream.ToArray();
    }

    private static byte[] BuildIndex(byte[][] objects)
    {
        using MemoryStream stream = new();
        WriteUInt16(stream, (ushort)objects.Length);
        if (objects.Length == 0)
        {
            return stream.ToArray();
        }

        stream.WriteByte(1);
        int offset = 1;
        stream.WriteByte(1);
        foreach (byte[] obj in objects)
        {
            offset += obj.Length;
            stream.WriteByte((byte)offset);
        }

        foreach (byte[] obj in objects)
        {
            stream.Write(obj);
        }

        return stream.ToArray();
    }

    private static byte[] EncodeInteger(int value)
    {
        if (value >= -107 && value <= 107)
        {
            return [(byte)(value + 139)];
        }

        if (value >= 108 && value <= 1131)
        {
            int adjusted = value - 108;
            return [(byte)(247 + adjusted / 256), (byte)(adjusted % 256)];
        }

        if (value >= -1131 && value <= -108)
        {
            int adjusted = -value - 108;
            return [(byte)(251 + adjusted / 256), (byte)(adjusted % 256)];
        }

        return [28, (byte)(value >> 8), (byte)value];
    }

    private static byte[] BuildPfb(byte[] segment1, byte[] segment2)
    {
        using MemoryStream stream = new();
        WritePfbRecord(stream, 0x01, segment1);
        WritePfbRecord(stream, 0x02, segment2);
        stream.WriteByte(0x80);
        stream.WriteByte(0x03);
        return stream.ToArray();
    }

    private static void WritePfbRecord(Stream stream, byte type, byte[] data)
    {
        stream.WriteByte(0x80);
        stream.WriteByte(type);
        WriteInt32LittleEndian(stream, data.Length);
        stream.Write(data);
    }

    private static byte[] EncryptEexec(byte[] clear)
    {
        return Encrypt(clear, 55665, 4);
    }

    private static byte[] EncryptType1CharString(byte[] clear)
    {
        return Encrypt(clear, 4330, 4);
    }

    private static byte[] Encrypt(byte[] clear, int seed, int discard)
    {
        byte[] plain = new byte[discard + clear.Length];
        Array.Copy(clear, 0, plain, discard, clear.Length);
        byte[] cipher = new byte[plain.Length];
        int r = seed;
        for (int i = 0; i < plain.Length; i++)
        {
            int plainByte = plain[i] & 0xFF;
            int cipherByte = plainByte ^ (r >> 8);
            cipher[i] = (byte)cipherByte;
            r = ((cipherByte + r) * 52845 + 22719) & 0xFFFF;
        }

        return cipher;
    }

    private static void WriteAscii(Stream stream, string value)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(value);
        stream.Write(bytes);
    }

    private static void WriteUInt16(Stream stream, ushort value)
    {
        stream.WriteByte((byte)(value >> 8));
        stream.WriteByte((byte)value);
    }

    private static void WriteUInt32(Stream stream, uint value)
    {
        stream.WriteByte((byte)(value >> 24));
        stream.WriteByte((byte)(value >> 16));
        stream.WriteByte((byte)(value >> 8));
        stream.WriteByte((byte)value);
    }

    private static void WriteInt32LittleEndian(Stream stream, int value)
    {
        stream.WriteByte((byte)value);
        stream.WriteByte((byte)(value >> 8));
        stream.WriteByte((byte)(value >> 16));
        stream.WriteByte((byte)(value >> 24));
    }

    private static byte[] CreateHeadTable()
    {
        using MemoryStream stream = new();
        WriteUInt32(stream, 0x00010000);
        WriteUInt32(stream, 0x00010000);
        WriteUInt32(stream, 0);
        WriteUInt32(stream, 0x5F0F3CF5);
        WriteUInt16(stream, 0);
        WriteUInt16(stream, 1000);
        WriteUInt32(stream, 0);
        WriteUInt32(stream, 0);
        WriteUInt32(stream, 0);
        WriteUInt32(stream, 0);
        WriteInt16(stream, 0);
        WriteInt16(stream, 0);
        WriteInt16(stream, 500);
        WriteInt16(stream, 700);
        WriteUInt16(stream, 0);
        WriteUInt16(stream, 8);
        WriteInt16(stream, 2);
        WriteInt16(stream, 0);
        WriteInt16(stream, 0);
        return stream.ToArray();
    }

    private static byte[] CreateMaxpTable()
    {
        using MemoryStream stream = new();
        WriteUInt32(stream, 0x00010000);
        WriteUInt16(stream, 2);
        return stream.ToArray();
    }

    private static byte[] CreateNameTable(string fullName)
    {
        byte[] value = Encoding.BigEndianUnicode.GetBytes(fullName);
        using MemoryStream stream = new();
        WriteUInt16(stream, 0);
        WriteUInt16(stream, 1);
        WriteUInt16(stream, 18);
        WriteUInt16(stream, 3);
        WriteUInt16(stream, 1);
        WriteUInt16(stream, 0x0409);
        WriteUInt16(stream, 4);
        WriteUInt16(stream, (ushort)value.Length);
        WriteUInt16(stream, 0);
        stream.Write(value);
        return stream.ToArray();
    }

    private static void WriteInt16(Stream stream, short value)
    {
        stream.WriteByte((byte)(value >> 8));
        stream.WriteByte((byte)value);
    }

    private static int Align4(int length)
    {
        return (length + 3) & ~3;
    }

    private static void WritePadding(Stream stream, int length)
    {
        int aligned = Align4(length);
        for (int i = length; i < aligned; i++)
        {
            stream.WriteByte(0);
        }
    }
}
