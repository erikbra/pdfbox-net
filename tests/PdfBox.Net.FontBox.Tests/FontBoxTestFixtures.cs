using System.Globalization;
using System.Text;

namespace PdfBox.Net.FontBox.Tests;

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

        byte[] segment1 = System.Text.Encoding.ASCII.GetBytes(segment1Text);
        byte[] segment2 = EncryptEexec(clear.ToArray());
        return BuildPfb(segment1, segment2);
    }

    public static byte[] CreateMinimalOpenTypeCff()
    {
        return CreateMinimalOpenTypeCff("CFF ");
    }

    public static byte[] CreateMinimalOpenTypeCff2()
    {
        return CreateMinimalOpenTypeCff("CFF2");
    }

    private static byte[] CreateMinimalOpenTypeCff(string cffTableTag)
    {
        byte[] cff = CreateMinimalType1Cff();
        byte[] head = CreateHeadTable();
        byte[] maxp = CreateMaxpTable();
        byte[] name = CreateNameTable("MiniCFF");
        byte[] hhea = CreateHheaTable(numHMetrics: 2);
        byte[] hmtx = CreateHmtxTable(numGlyphs: 2);
        byte[] post = CreatePostTable();
        byte[] cmap = CreateCmapTable();

        byte[][] tables = [head, hhea, maxp, post, cmap, hmtx, name, cff];
        string[] tags = ["head", "hhea", "maxp", "post", "cmap", "hmtx", "name", cffTableTag];

        int tableCount = tables.Length;
        int directorySize = 12 + tableCount * 16;

        int[] offsets = new int[tableCount];
        int currentOffset = directorySize;
        for (int i = 0; i < tableCount; i++)
        {
            offsets[i] = currentOffset;
            currentOffset += Align4(tables[i].Length);
        }

        int searchRangeExp = (int)Math.Log2(tableCount);
        ushort searchRange2 = (ushort)(1 << searchRangeExp);

        using MemoryStream stream = new();
        WriteAscii(stream, "OTTO");
        WriteUInt16(stream, (ushort)tableCount);
        WriteUInt16(stream, (ushort)(searchRange2 * 16));
        WriteUInt16(stream, (ushort)searchRangeExp);
        WriteUInt16(stream, (ushort)(tableCount * 16 - searchRange2 * 16));

        for (int i = 0; i < tableCount; i++)
        {
            WriteAscii(stream, tags[i]);
            WriteUInt32(stream, 0);
            WriteUInt32(stream, (uint)offsets[i]);
            WriteUInt32(stream, (uint)tables[i].Length);
        }

        for (int i = 0; i < tableCount; i++)
        {
            stream.Write(tables[i]);
            WritePadding(stream, tables[i].Length);
        }

        return stream.ToArray();
    }

    public static byte[] CreateMinimalCffWithExpertCharsetEncoding()
    {
        return CreateMinimalType1Cff(useExpertCharsetEncoding: true);
    }

    public static byte[] CreateMinimalCidCff()
    {
        byte[] nameIndex = BuildIndex([System.Text.Encoding.ASCII.GetBytes("MiniCID")]);
        byte[] stringIndex = BuildIndex([System.Text.Encoding.ASCII.GetBytes("Adobe"), System.Text.Encoding.ASCII.GetBytes("Identity")]);
        byte[] globalSubrIndex = BuildIndex([]);
        byte[] charStringsIndex = BuildIndex([[14], [14]]);
        byte[] charsetData = [0, 0, 42]; // format 0, gid 1 -> cid 42
        byte[] fdSelectData = [0, 0, 0]; // format 0, two gids mapped to FD 0
        byte[] privateDict = BuildDict(
            EncodeInteger(500), [20],
            EncodeInteger(0), [21]);

        byte[] topDict = [];
        byte[] fdDict = [];
        while (true)
        {
            byte[] topDictIndex = BuildIndex([topDict]);
            byte[] fdArrayIndex = BuildIndex([fdDict]);
            int prefix = 4 + nameIndex.Length + topDictIndex.Length + stringIndex.Length + globalSubrIndex.Length;
            int charStringsOffset = prefix;
            int charsetOffset = charStringsOffset + charStringsIndex.Length;
            int fdSelectOffset = charsetOffset + charsetData.Length;
            int fdArrayOffset = fdSelectOffset + fdSelectData.Length;
            int privateOffset = fdArrayOffset + fdArrayIndex.Length;

            byte[] nextFdDict = BuildDict(
                EncodeInteger(privateDict.Length), EncodeInteger(privateOffset), [18]);
            byte[] nextFdArrayIndex = BuildIndex([nextFdDict]);
            privateOffset = fdArrayOffset + nextFdArrayIndex.Length;
            nextFdDict = BuildDict(
                EncodeInteger(privateDict.Length), EncodeInteger(privateOffset), [18]);

            byte[] nextTopDict = BuildDict(
                EncodeInteger(391), EncodeInteger(392), EncodeInteger(0), [12, 30], // ROS
                EncodeInteger(charsetOffset), [15],
                EncodeInteger(charStringsOffset), [17],
                EncodeInteger(fdArrayOffset), [12, 36],
                EncodeInteger(fdSelectOffset), [12, 37]);

            if (nextTopDict.SequenceEqual(topDict) && nextFdDict.SequenceEqual(fdDict))
            {
                topDict = nextTopDict;
                fdDict = nextFdDict;
                break;
            }

            topDict = nextTopDict;
            fdDict = nextFdDict;
        }

        byte[] topDictIndexFinal = BuildIndex([topDict]);
        byte[] fdArrayIndexFinal = BuildIndex([fdDict]);

        using MemoryStream stream = new();
        stream.Write([1, 0, 4, 1]);
        stream.Write(nameIndex);
        stream.Write(topDictIndexFinal);
        stream.Write(stringIndex);
        stream.Write(globalSubrIndex);
        stream.Write(charStringsIndex);
        stream.Write(charsetData);
        stream.Write(fdSelectData);
        stream.Write(fdArrayIndexFinal);
        stream.Write(privateDict);
        return stream.ToArray();
    }

    public static byte[] CreateMinimalTrueType()
    {
        return CreateMinimalTrueTypeWithUpm(1000);
    }

    /// <summary>Creates a minimal TrueType with a custom unitsPerEm value and a fixed advance width of 500 design units.</summary>
    public static byte[] CreateMinimalTrueTypeWithUpm(int unitsPerEm)
    {
        byte[] head = CreateHeadTable(unitsPerEm);
        byte[] maxp = CreateMaxpTable();
        byte[] name = CreateNameTable("MiniTTF");
        byte[] hhea = CreateHheaTable(numHMetrics: 2);
        byte[] hmtx = CreateHmtxTable(numGlyphs: 2);
        byte[] post = CreatePostTable();
        byte[] cmap = CreateCmapTable();
        // 2 glyphs: GID 0 = empty, GID 1 = single contour box
        byte[] glyf = CreateGlyfTable();
        byte[] loca = CreateLocaTable(glyfLength: glyf.Length);

        byte[][] tables = [head, hhea, maxp, post, cmap, loca, glyf, hmtx, name];
        string[] tags   = ["head", "hhea", "maxp", "post", "cmap", "loca", "glyf", "hmtx", "name"];

        int tableCount = tables.Length;
        int directorySize = 12 + tableCount * 16;

        // calculate offsets
        int[] offsets = new int[tableCount];
        int currentOffset = directorySize;
        for (int i = 0; i < tableCount; i++)
        {
            offsets[i] = currentOffset;
            currentOffset += Align4(tables[i].Length);
        }

        using MemoryStream stream = new();
        WriteUInt32(stream, 0x00010000);
        WriteUInt16(stream, (ushort)tableCount);
        WriteUInt16(stream, 128);  // searchRange  = 8 * 16
        WriteUInt16(stream, 3);    // entrySelector = log2(8)
        WriteUInt16(stream, (ushort)(tableCount * 16 - 128)); // rangeShift

        for (int i = 0; i < tableCount; i++)
        {
            WriteAscii(stream, tags[i]);
            WriteUInt32(stream, 0);
            WriteUInt32(stream, (uint)offsets[i]);
            WriteUInt32(stream, (uint)tables[i].Length);
        }

        for (int i = 0; i < tableCount; i++)
        {
            stream.Write(tables[i]);
            WritePadding(stream, tables[i].Length);
        }

        return stream.ToArray();
    }

    private static byte[] CreateHheaTable(int numHMetrics)
    {
        using MemoryStream stream = new();
        WriteUInt32(stream, 0x00010000); // version 1.0
        WriteInt16(stream, 800);  // ascender
        WriteInt16(stream, -200); // descender
        WriteInt16(stream, 0);    // lineGap
        WriteUInt16(stream, 500); // advanceWidthMax
        WriteInt16(stream, 0);    // minLeftSideBearing
        WriteInt16(stream, 0);    // minRightSideBearing
        WriteInt16(stream, 500);  // xMaxExtent
        WriteInt16(stream, 1);    // caretSlopeRise
        WriteInt16(stream, 0);    // caretSlopeRun
        WriteInt16(stream, 0);    // caretOffset (reserved1)
        WriteInt16(stream, 0);    // reserved2
        WriteInt16(stream, 0);    // reserved3
        WriteInt16(stream, 0);    // reserved4
        WriteInt16(stream, 0);    // reserved5
        WriteInt16(stream, 0);    // metricDataFormat
        WriteUInt16(stream, (ushort)numHMetrics);
        return stream.ToArray();
    }

    private static byte[] CreateHmtxTable(int numGlyphs)
    {
        using MemoryStream stream = new();
        for (int i = 0; i < numGlyphs; i++)
        {
            WriteUInt16(stream, 500); // advanceWidth
            WriteInt16(stream, 0);    // lsb
        }
        return stream.ToArray();
    }

    private static byte[] CreatePostTable()
    {
        using MemoryStream stream = new();
        WriteUInt32(stream, 0x00030000); // version 3.0 (no glyph names)
        WriteUInt32(stream, 0);          // italicAngle
        WriteInt16(stream, -100);        // underlinePosition
        WriteInt16(stream, 50);          // underlineThickness
        WriteUInt32(stream, 0);          // isFixedPitch
        WriteUInt32(stream, 0);          // minMemType42
        WriteUInt32(stream, 0);          // maxMemType42
        WriteUInt32(stream, 0);          // minMemType1
        WriteUInt32(stream, 0);          // maxMemType1
        return stream.ToArray();
    }

    private static byte[] CreateCmapTable()
    {
        // Format 4 cmap with a single segment mapping U+0041 ('A') → GID 1
        // Format 4 header: segCount=2 (1 real + 1 terminator)
        ushort segCount = 2;
        ushort segCountX2 = (ushort)(segCount * 2);
        ushort searchRange = 4;
        ushort entrySelector = 1;
        ushort rangeShift = 0;

        using MemoryStream fmt4 = new();
        WriteUInt16(fmt4, 4);          // format
        WriteUInt16(fmt4, 0);          // length placeholder
        WriteUInt16(fmt4, 0);          // language
        WriteUInt16(fmt4, segCountX2); // segCountX2
        WriteUInt16(fmt4, searchRange);
        WriteUInt16(fmt4, entrySelector);
        WriteUInt16(fmt4, rangeShift);
        // endCount[0]=0x0041, endCount[1]=0xFFFF
        WriteUInt16(fmt4, 0x0041);
        WriteUInt16(fmt4, 0xFFFF);
        WriteUInt16(fmt4, 0);          // reservedPad
        // startCount[0]=0x0041, startCount[1]=0xFFFF
        WriteUInt16(fmt4, 0x0041);
        WriteUInt16(fmt4, 0xFFFF);
        // idDelta[0]=1-0x41=0xFFC0(-64), idDelta[1]=1
        WriteInt16(fmt4, unchecked((short)(1 - 0x0041)));
        WriteInt16(fmt4, 1);
        // idRangeOffset[0]=0, idRangeOffset[1]=0
        WriteUInt16(fmt4, 0);
        WriteUInt16(fmt4, 0);

        byte[] fmt4Bytes = fmt4.ToArray();
        // fix length field
        fmt4Bytes[2] = (byte)(fmt4Bytes.Length >> 8);
        fmt4Bytes[3] = (byte)(fmt4Bytes.Length & 0xff);

        using MemoryStream stream = new();
        WriteUInt16(stream, 0);       // version
        WriteUInt16(stream, 1);       // numTables
        // encoding record: platform=3(Windows), encodingID=1(Unicode BMP), offset=4+8=12
        WriteUInt16(stream, 3);
        WriteUInt16(stream, 1);
        WriteUInt32(stream, 12);      // offset to format 4
        stream.Write(fmt4Bytes);
        return stream.ToArray();
    }

    private static byte[] CreateGlyfTable()
    {
        // GID 0: empty (no contours) - 10 bytes glyph header with 0 contours
        using MemoryStream stream = new();
        WriteInt16(stream, 0);    // numberOfContours = 0 (empty glyph)
        WriteInt16(stream, 0);    // xMin
        WriteInt16(stream, 0);    // yMin
        WriteInt16(stream, 0);    // xMax
        WriteInt16(stream, 0);    // yMax
        // GID 1: simple glyph with 1 contour, 4 points (box)
        WriteInt16(stream, 1);    // numberOfContours = 1
        WriteInt16(stream, 0);    // xMin
        WriteInt16(stream, 0);    // yMin
        WriteInt16(stream, 500);  // xMax
        WriteInt16(stream, 700);  // yMax
        WriteUInt16(stream, 3);   // endPtsOfContours[0] = 3 (4 points: 0,1,2,3)
        WriteUInt16(stream, 0);   // instructionLength = 0
        // flags: 4 points, all on-curve (flag=1)
        stream.WriteByte(1); stream.WriteByte(1); stream.WriteByte(1); stream.WriteByte(1);
        // xCoordinates (relative): 0, 500, 0, -500
        WriteInt16(stream, 0); WriteInt16(stream, 500); WriteInt16(stream, 0); WriteInt16(stream, unchecked((short)-500));
        // yCoordinates (relative): 0, 0, 700, 0
        WriteInt16(stream, 0); WriteInt16(stream, 0); WriteInt16(stream, 700); WriteInt16(stream, 0);
        return stream.ToArray();
    }

    private static byte[] CreateLocaTable(int glyfLength)
    {
        // long format (4 bytes per offset), 3 offsets for 2 glyphs (GID 0..1) + sentinel
        // GID 0 starts at 0, GID 1 starts at 10 (0 = empty = 10 bytes), sentinel at glyfLength
        using MemoryStream stream = new();
        WriteUInt32(stream, 0);                 // GID 0 offset
        WriteUInt32(stream, 10);                // GID 1 offset (empty glyph = 10 bytes)
        WriteUInt32(stream, (uint)glyfLength);  // sentinel
        return stream.ToArray();
    }

    private static byte[] CreateMinimalType1Cff(bool useExpertCharsetEncoding = false)
    {
        byte[] nameIndex = BuildIndex([System.Text.Encoding.ASCII.GetBytes("MiniCFF")]);
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
                useExpertCharsetEncoding ? EncodeInteger(1) : [],
                useExpertCharsetEncoding ? [15] : [],
                useExpertCharsetEncoding ? EncodeInteger(1) : [],
                useExpertCharsetEncoding ? [16] : [],
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
        byte[] bytes = System.Text.Encoding.ASCII.GetBytes(value);
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

    private static byte[] CreateHeadTable(int unitsPerEm = 1000)
    {
        using MemoryStream stream = new();
        WriteUInt32(stream, 0x00010000);
        WriteUInt32(stream, 0x00010000);
        WriteUInt32(stream, 0);
        WriteUInt32(stream, 0x5F0F3CF5);
        WriteUInt16(stream, 0);
        WriteUInt16(stream, (ushort)unitsPerEm);
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
        WriteInt16(stream, 1);
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
        byte[] value = System.Text.Encoding.BigEndianUnicode.GetBytes(fullName);
        using MemoryStream stream = new();
        WriteUInt16(stream, 0);
        WriteUInt16(stream, 1);
        WriteUInt16(stream, 18);
        WriteUInt16(stream, 3);
        WriteUInt16(stream, 1);
        WriteUInt16(stream, 0x0409);
        WriteUInt16(stream, 6);    // nameId = 6 (PostScript name)
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
