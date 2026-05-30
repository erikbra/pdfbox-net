/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/TTFSubsetter.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
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
using System.Linq;
using System.Text;
using TextEncoding = System.Text.Encoding;

namespace PdfBox.Net.FontBox.TTF;

/// <summary>
/// Subsetter for TrueType (TTF) fonts.
/// </summary>
public sealed class TTFSubsetter
{
    private static readonly byte[] PadBuf = [0, 0, 0, 0];
    private static readonly TextEncoding BigEndianUnicode = new UnicodeEncoding(bigEndian: true, byteOrderMark: false);

    private readonly TrueTypeFont ttf;
    private readonly CmapLookup unicodeCmap;
    private readonly SortedDictionary<int, int> uniToGID;
    private readonly IList<string>? keepTables;
    private readonly SortedSet<int> glyphIds;
    private readonly HashSet<int> invisibleGlyphIds;
    private string? prefix;
    private bool hasAddedCompoundReferences;

    public TTFSubsetter(TrueTypeFont ttf) : this(ttf, null)
    {
    }

    public TTFSubsetter(TrueTypeFont ttf, IList<string>? tables)
    {
        this.ttf = ttf;
        keepTables = tables;
        uniToGID = new SortedDictionary<int, int>();
        glyphIds = new SortedSet<int>();
        invisibleGlyphIds = new HashSet<int>();
        unicodeCmap = ttf.GetUnicodeCmapLookup() ?? throw new IOException($"The TrueType font {ttf.GetName()} does not contain a usable Unicode cmap");
        glyphIds.Add(0);
    }

    public void SetPrefix(string prefix)
    {
        this.prefix = prefix;
    }

    public void Add(int unicode)
    {
        int gid = unicodeCmap.GetGlyphId(unicode);
        if (gid != 0)
        {
            uniToGID[unicode] = gid;
            glyphIds.Add(gid);
        }
    }

    public void AddAll(ISet<int> unicodeSet)
    {
        foreach (int unicode in unicodeSet)
        {
            Add(unicode);
        }
    }

    public void ForceInvisible(int unicode)
    {
        int gid = unicodeCmap.GetGlyphId(unicode);
        if (gid != 0)
        {
            invisibleGlyphIds.Add(gid);
        }
    }

    public Dictionary<int, int> GetGIDMap()
    {
        AddCompoundReferences();

        Dictionary<int, int> newToOld = new();
        int newGid = 0;
        foreach (int oldGid in glyphIds)
        {
            newToOld[newGid] = oldGid;
            newGid++;
        }

        return newToOld;
    }

    private long WriteFileHeader(Stream output, int nTables)
    {
        WriteUint32(output, 0x00010000);
        WriteUint16(output, nTables);

        int mask = HighestOneBit(nTables);
        int searchRange = mask * 16;
        WriteUint16(output, searchRange);

        int entrySelector = Log2(mask);
        WriteUint16(output, entrySelector);

        int last = 16 * nTables - searchRange;
        WriteUint16(output, last);

        return 0x00010000L + ToUInt32(nTables, searchRange) + ToUInt32(entrySelector, last);
    }

    private long WriteTableHeader(Stream output, string tag, long offset, byte[] bytes)
    {
        long checksum = 0;
        for (int nup = 0, n = bytes.Length; nup < n; nup++)
        {
            checksum += (bytes[nup] & 0xffL) << (24 - nup % 4 * 8);
        }

        checksum &= 0xffffffffL;
        byte[] tagBytes = TextEncoding.ASCII.GetBytes(tag);
        output.Write(tagBytes, 0, 4);
        WriteUint32(output, checksum);
        WriteUint32(output, offset);
        WriteUint32(output, bytes.Length);
        return ToUInt32(tagBytes) + checksum + checksum + offset + bytes.Length;
    }

    private static void WriteTableBody(Stream output, byte[] bytes)
    {
        int n = bytes.Length;
        output.Write(bytes, 0, n);
        if (n % 4 != 0)
        {
            output.Write(PadBuf, 0, 4 - n % 4);
        }
    }

    private byte[] BuildHeadTable()
    {
        using MemoryStream ms = new(54);
        HeaderTable h = ttf.GetHeader() ?? throw new IOException("Could not get head table");
        WriteFixed(ms, h.GetVersion());
        WriteFixed(ms, h.GetFontRevision());
        WriteUint32(ms, 0);
        WriteUint32(ms, h.GetMagicNumber());
        WriteUint16(ms, h.GetFlags());
        WriteUint16(ms, h.GetUnitsPerEm());
        WriteLongDateTime(ms, h.GetCreated());
        WriteLongDateTime(ms, h.GetModified());
        WriteSInt16(ms, h.GetXMin());
        WriteSInt16(ms, h.GetYMin());
        WriteSInt16(ms, h.GetXMax());
        WriteSInt16(ms, h.GetYMax());
        WriteUint16(ms, h.GetMacStyle());
        WriteUint16(ms, h.GetLowestRecPPEM());
        WriteSInt16(ms, h.GetFontDirectionHint());
        WriteSInt16(ms, 1);
        WriteSInt16(ms, h.GetGlyphDataFormat());
        return ms.ToArray();
    }

    private byte[] BuildHheaTable()
    {
        using MemoryStream ms = new(36);
        HorizontalHeaderTable h = ttf.GetHorizontalHeader() ?? throw new IOException("Could not get hhea table");
        WriteFixed(ms, h.Version);
        WriteSInt16(ms, h.Ascender);
        WriteSInt16(ms, h.Descender);
        WriteSInt16(ms, h.LineGap);
        WriteUint16(ms, h.AdvanceWidthMax);
        WriteSInt16(ms, h.MinLeftSideBearing);
        WriteSInt16(ms, h.MinRightSideBearing);
        WriteSInt16(ms, h.XMaxExtent);
        WriteSInt16(ms, h.CaretSlopeRise);
        WriteSInt16(ms, h.CaretSlopeRun);
        WriteSInt16(ms, h.Reserved1);
        WriteSInt16(ms, h.Reserved2);
        WriteSInt16(ms, h.Reserved3);
        WriteSInt16(ms, h.Reserved4);
        WriteSInt16(ms, h.Reserved5);
        WriteSInt16(ms, h.MetricDataFormat);

        int hmetrics = h.NumberOfHMetrics > 0 ? glyphIds.GetViewBetween(0, h.NumberOfHMetrics - 1).Count : 0;
        if (glyphIds.Max >= h.NumberOfHMetrics && !glyphIds.Contains(h.NumberOfHMetrics - 1))
        {
            hmetrics++;
        }

        WriteUint16(ms, hmetrics);
        return ms.ToArray();
    }

    private static bool ShouldCopyNameRecord(NameRecord nr)
    {
        return nr.GetPlatformId() == NameRecord.PLATFORM_WINDOWS
            && nr.GetPlatformEncodingId() == NameRecord.ENCODING_WINDOWS_UNICODE_BMP
            && nr.GetLanguageId() == NameRecord.LANGUAGE_WINDOWS_EN_US
            && nr.GetNameId() >= 0
            && nr.GetNameId() < 7;
    }

    private byte[]? BuildNameTable()
    {
        NamingTable? name = ttf.GetNaming();
        if (name == null || keepTables != null && !keepTables.Contains(NamingTable.TAG))
        {
            return null;
        }

        using MemoryStream ms = new(512);
        List<NameRecord> nameRecords = name.GetNameRecords();
        int numRecords = nameRecords.Count(ShouldCopyNameRecord);
        WriteUint16(ms, 0);
        WriteUint16(ms, numRecords);
        WriteUint16(ms, 2 * 3 + 2 * 6 * numRecords);

        if (numRecords == 0)
        {
            return null;
        }

        byte[][] names = new byte[numRecords][];
        int j = 0;
        foreach (NameRecord nameRecord in nameRecords)
        {
            if (!ShouldCopyNameRecord(nameRecord))
            {
                continue;
            }

            int platform = nameRecord.GetPlatformId();
            int encoding = nameRecord.GetPlatformEncodingId();
            TextEncoding charset = TextEncoding.Latin1;
            if (platform == CmapTable.PlatformWindows && encoding == CmapTable.EncodingWinUnicodeBmp)
            {
                charset = BigEndianUnicode;
            }
            else if (platform == 2)
            {
                if (encoding == 0)
                {
                    charset = TextEncoding.ASCII;
                }
                else if (encoding == 1)
                {
                    charset = BigEndianUnicode;
                }
            }

            string value = nameRecord.GetString() ?? string.Empty;
            if (nameRecord.GetNameId() == NameRecord.NAME_POSTSCRIPT_NAME && prefix != null)
            {
                value = prefix + value;
            }

            names[j] = charset.GetBytes(value);
            j++;
        }

        int offset = 0;
        j = 0;
        foreach (NameRecord nr in nameRecords)
        {
            if (!ShouldCopyNameRecord(nr))
            {
                continue;
            }

            WriteUint16(ms, nr.GetPlatformId());
            WriteUint16(ms, nr.GetPlatformEncodingId());
            WriteUint16(ms, nr.GetLanguageId());
            WriteUint16(ms, nr.GetNameId());
            WriteUint16(ms, names[j].Length);
            WriteUint16(ms, offset);
            offset += names[j].Length;
            j++;
        }

        for (int i = 0; i < numRecords; i++)
        {
            ms.Write(names[i], 0, names[i].Length);
        }

        return ms.ToArray();
    }

    private byte[] BuildMaxpTable()
    {
        using MemoryStream ms = new(32);
        MaximumProfileTable p = ttf.GetMaximumProfile() ?? throw new IOException("Could not get maxp table");
        WriteFixed(ms, p.GetVersion());
        WriteUint16(ms, glyphIds.Count);
        if (p.GetVersion() >= 1.0f)
        {
            WriteUint16(ms, p.GetMaxPoints());
            WriteUint16(ms, p.GetMaxContours());
            WriteUint16(ms, p.GetMaxCompositePoints());
            WriteUint16(ms, p.GetMaxCompositeContours());
            WriteUint16(ms, p.GetMaxZones());
            WriteUint16(ms, p.GetMaxTwilightPoints());
            WriteUint16(ms, p.GetMaxStorage());
            WriteUint16(ms, p.GetMaxFunctionDefs());
            WriteUint16(ms, p.GetMaxInstructionDefs());
            WriteUint16(ms, p.GetMaxStackElements());
            WriteUint16(ms, p.GetMaxSizeOfInstructions());
            WriteUint16(ms, p.GetMaxComponentElements());
            WriteUint16(ms, p.GetMaxComponentDepth());
        }

        return ms.ToArray();
    }

    private byte[]? BuildOS2Table()
    {
        OS2WindowsMetricsTable? os2 = ttf.GetOS2Windows();
        if (os2 == null || uniToGID.Count == 0 || keepTables != null && !keepTables.Contains(OS2WindowsMetricsTable.TAG))
        {
            return null;
        }

        using MemoryStream ms = new(78);
        WriteUint16(ms, os2.GetVersion());
        WriteSInt16(ms, os2.GetAverageCharWidth());
        WriteUint16(ms, os2.GetWeightClass());
        WriteUint16(ms, os2.GetWidthClass());
        WriteSInt16(ms, os2.GetFsType());
        WriteSInt16(ms, os2.GetSubscriptXSize());
        WriteSInt16(ms, os2.GetSubscriptYSize());
        WriteSInt16(ms, os2.GetSubscriptXOffset());
        WriteSInt16(ms, os2.GetSubscriptYOffset());
        WriteSInt16(ms, os2.GetSuperscriptXSize());
        WriteSInt16(ms, os2.GetSuperscriptYSize());
        WriteSInt16(ms, os2.GetSuperscriptXOffset());
        WriteSInt16(ms, os2.GetSuperscriptYOffset());
        WriteSInt16(ms, os2.GetStrikeoutSize());
        WriteSInt16(ms, (short)os2.GetStrikeoutPosition());
        WriteSInt16(ms, (short)os2.GetFamilyClass());
        byte[] panose = os2.GetPanose();
        ms.Write(panose, 0, panose.Length);
        WriteUint32(ms, 0);
        WriteUint32(ms, 0);
        WriteUint32(ms, 0);
        WriteUint32(ms, 0);

        byte[] achVendId = new byte[4];
        byte[] vendIdBytes = TextEncoding.ASCII.GetBytes(os2.GetAchVendId() ?? string.Empty);
        Array.Copy(vendIdBytes, 0, achVendId, 0, Math.Min(achVendId.Length, vendIdBytes.Length));
        ms.Write(achVendId, 0, achVendId.Length);

        WriteUint16(ms, os2.GetFsSelection());
        WriteUint16(ms, uniToGID.Keys.First());
        WriteUint16(ms, uniToGID.Keys.Last());
        WriteUint16(ms, os2.GetTypoAscender());
        WriteUint16(ms, os2.GetTypoDescender());
        WriteUint16(ms, os2.GetTypoLineGap());
        WriteUint16(ms, os2.GetWinAscent());
        WriteUint16(ms, os2.GetWinDescent());
        return ms.ToArray();
    }

    private static byte[] BuildLocaTable(long[] newOffsets)
    {
        using MemoryStream ms = new(newOffsets.Length * 4);
        foreach (long offset in newOffsets)
        {
            WriteUint32(ms, offset);
        }

        return ms.ToArray();
    }

    private void AddCompoundReferences()
    {
        if (hasAddedCompoundReferences)
        {
            return;
        }

        hasAddedCompoundReferences = true;
        bool hasNested;
        GlyphTable g = ttf.GetGlyph() ?? throw new IOException("Could not get glyf table");
        long[] offsets = ttf.GetIndexToLocation()?.GetOffsets() ?? throw new IOException("Could not get loca table");
        do
        {
            SortedSet<int>? glyphIdsToAdd = null;
            using Stream input = ttf.GetOriginalData();
            SkipBytes(input, g.GetOffset());

            long lastOff = 0L;
            foreach (int gid in glyphIds)
            {
                long offset = offsets[gid];
                long length = offsets[gid + 1] - offset;
                SkipBytes(input, offset - lastOff);
                byte[] buf = new byte[(int)length];
                ReadFully(input, buf, 0, buf.Length);

                if (buf.Length >= 2 && buf[0] == 0xff && buf[1] == 0xff)
                {
                    int off = 2 * 5;
                    int flags;
                    do
                    {
                        flags = (buf[off] & 0xff) << 8 | buf[off + 1] & 0xff;
                        off += 2;
                        int ogid = (buf[off] & 0xff) << 8 | buf[off + 1] & 0xff;
                        if (!glyphIds.Contains(ogid))
                        {
                            glyphIdsToAdd ??= new SortedSet<int>();
                            glyphIdsToAdd.Add(ogid);
                        }

                        off += 2;
                        if ((flags & 1 << 0) != 0)
                        {
                            off += 2 * 2;
                        }
                        else
                        {
                            off += 2;
                        }

                        if ((flags & 1 << 7) != 0)
                        {
                            off += 2 * 4;
                        }
                        else if ((flags & 1 << 6) != 0)
                        {
                            off += 2 * 2;
                        }
                        else if ((flags & 1 << 3) != 0)
                        {
                            off += 2;
                        }
                    }
                    while ((flags & 1 << 5) != 0);
                }

                lastOff = offsets[gid + 1];
            }

            hasNested = glyphIdsToAdd != null;
            if (hasNested)
            {
                glyphIds.UnionWith(glyphIdsToAdd!);
            }
        }
        while (hasNested);
    }

    private byte[] BuildGlyfTable(long[] newOffsets)
    {
        using MemoryStream ms = new(512);
        GlyphTable g = ttf.GetGlyph() ?? throw new IOException("Could not get glyf table");
        long[] offsets = ttf.GetIndexToLocation()?.GetOffsets() ?? throw new IOException("Could not get loca table");
        using Stream input = ttf.GetOriginalData();
        SkipBytes(input, g.GetOffset());

        long lastOff = 0;
        long newOffset = 0;
        int newGid = 0;
        foreach (int gid in glyphIds)
        {
            long offset = offsets[gid];
            long length = offsets[gid + 1] - offset;
            newOffsets[newGid++] = newOffset;
            SkipBytes(input, offset - lastOff);

            if (invisibleGlyphIds.Contains(gid))
            {
                lastOff = offset;
                continue;
            }

            byte[] buf = new byte[(int)length];
            ReadFully(input, buf, 0, buf.Length);
            if (buf.Length >= 2 && buf[0] == 0xff && buf[1] == 0xff)
            {
                int off = 2 * 5;
                int flags;
                do
                {
                    flags = (buf[off] & 0xff) << 8 | buf[off + 1] & 0xff;
                    off += 2;
                    int componentGid = (buf[off] & 0xff) << 8 | buf[off + 1] & 0xff;
                    if (!glyphIds.Contains(componentGid))
                    {
                        throw new IOException($"Internal error: componentGid {componentGid} not in glyphIds set");
                    }

                    int newComponentGid = GetNewGlyphId(componentGid);
                    buf[off] = (byte)(newComponentGid >> 8);
                    buf[off + 1] = (byte)newComponentGid;
                    off += 2;

                    if ((flags & 1 << 0) != 0)
                    {
                        off += 2 * 2;
                    }
                    else
                    {
                        off += 2;
                    }

                    if ((flags & 1 << 7) != 0)
                    {
                        off += 2 * 4;
                    }
                    else if ((flags & 1 << 6) != 0)
                    {
                        off += 2 * 2;
                    }
                    else if ((flags & 1 << 3) != 0)
                    {
                        off += 2;
                    }
                }
                while ((flags & 1 << 5) != 0);

                if ((flags & 0x0100) == 0x0100)
                {
                    int numInstr = (buf[off] & 0xff) << 8 | buf[off + 1] & 0xff;
                    off += 2;
                    off += numInstr;
                }

                ms.Write(buf, 0, off);
                newOffset += off;
            }
            else if (buf.Length > 0)
            {
                ms.Write(buf, 0, buf.Length);
                newOffset += buf.Length;
            }

            if (newOffset % 4 != 0)
            {
                int len = 4 - (int)(newOffset % 4);
                ms.Write(PadBuf, 0, len);
                newOffset += len;
            }

            lastOff = offset + length;
        }

        newOffsets[newGid] = newOffset;
        return ms.ToArray();
    }

    private int GetNewGlyphId(int oldGid)
    {
        return glyphIds.Count(gid => gid < oldGid);
    }

    private byte[]? BuildCmapTable()
    {
        if (ttf.GetCmap() == null || uniToGID.Count == 0 || keepTables != null && !keepTables.Contains(CmapTable.Tag))
        {
            return null;
        }

        using MemoryStream ms = new(64);
        WriteUint16(ms, 0);
        WriteUint16(ms, 1);
        WriteUint16(ms, CmapTable.PlatformWindows);
        WriteUint16(ms, CmapTable.EncodingWinUnicodeBmp);
        WriteUint32(ms, 12);

        List<KeyValuePair<int, int>> mappings = [.. uniToGID];
        KeyValuePair<int, int> lastChar = mappings[0];
        KeyValuePair<int, int> prevChar = lastChar;
        int lastGid = GetNewGlyphId(lastChar.Value);

        int[] startCode = new int[uniToGID.Count + 1];
        int[] endCode = new int[startCode.Length];
        int[] idDelta = new int[startCode.Length];
        int segCount = 0;
        for (int i = 1; i < mappings.Count; i++)
        {
            KeyValuePair<int, int> curChar2Gid = mappings[i];
            int curGid = GetNewGlyphId(curChar2Gid.Value);
            if (curChar2Gid.Key > 0xFFFF)
            {
                throw new NotSupportedException("non-BMP Unicode character");
            }

            if (curChar2Gid.Key != prevChar.Key + 1 || curGid - lastGid != curChar2Gid.Key - lastChar.Key)
            {
                if (lastGid != 0)
                {
                    startCode[segCount] = lastChar.Key;
                    endCode[segCount] = prevChar.Key;
                    idDelta[segCount] = lastGid - lastChar.Key;
                    segCount++;
                }
                else if (lastChar.Key != prevChar.Key)
                {
                    startCode[segCount] = lastChar.Key + 1;
                    endCode[segCount] = prevChar.Key;
                    idDelta[segCount] = lastGid - lastChar.Key;
                    segCount++;
                }

                lastGid = curGid;
                lastChar = curChar2Gid;
            }

            prevChar = curChar2Gid;
        }

        startCode[segCount] = lastChar.Key;
        endCode[segCount] = prevChar.Key;
        idDelta[segCount] = lastGid - lastChar.Key;
        segCount++;

        startCode[segCount] = 0xffff;
        endCode[segCount] = 0xffff;
        idDelta[segCount] = 1;
        segCount++;

        int searchRange = 2 * HighestOneBit(segCount);
        WriteUint16(ms, 4);
        WriteUint16(ms, 8 * 2 + segCount * 4 * 2);
        WriteUint16(ms, 0);
        WriteUint16(ms, segCount * 2);
        WriteUint16(ms, searchRange);
        WriteUint16(ms, Log2(searchRange / 2));
        WriteUint16(ms, 2 * segCount - searchRange);

        for (int i = 0; i < segCount; i++)
        {
            WriteUint16(ms, endCode[i]);
        }

        WriteUint16(ms, 0);

        for (int i = 0; i < segCount; i++)
        {
            WriteUint16(ms, startCode[i]);
        }

        for (int i = 0; i < segCount; i++)
        {
            WriteUint16(ms, idDelta[i]);
        }

        for (int i = 0; i < segCount; i++)
        {
            WriteUint16(ms, 0);
        }

        return ms.ToArray();
    }

    private byte[]? BuildPostTable()
    {
        PostScriptTable? post = ttf.GetPostScript();
        if (post == null || post.GlyphNames == null || keepTables != null && !keepTables.Contains(PostScriptTable.TAG))
        {
            return null;
        }

        using MemoryStream ms = new(64);
        WriteFixed(ms, 2.0);
        WriteFixed(ms, post.ItalicAngle);
        WriteSInt16(ms, post.UnderlinePosition);
        WriteSInt16(ms, post.UnderlineThickness);
        WriteUint32(ms, post.IsFixedPitch);
        WriteUint32(ms, post.MinMemType42);
        WriteUint32(ms, post.MaxMemType42);
        WriteUint32(ms, post.MinMemType1);
        WriteUint32(ms, post.MaxMemType1);
        WriteUint16(ms, glyphIds.Count);

        Dictionary<string, int> names = new(StringComparer.Ordinal);
        foreach (int gid in glyphIds)
        {
            string name = post.GetName(gid) ?? string.Empty;
            int? macId = WGL4Names.GetGlyphIndex(name);
            if (macId != null)
            {
                WriteUint16(ms, macId.Value);
            }
            else
            {
                if (!names.TryGetValue(name, out int ordinal))
                {
                    ordinal = names.Count;
                    names[name] = ordinal;
                }

                WriteUint16(ms, 258 + ordinal);
            }
        }

        foreach (string name in names.Keys)
        {
            byte[] buf = TextEncoding.ASCII.GetBytes(name);
            WriteUint8(ms, buf.Length);
            ms.Write(buf, 0, buf.Length);
        }

        return ms.ToArray();
    }

    private byte[] BuildHmtxTable()
    {
        using MemoryStream ms = new();
        HorizontalHeaderTable h = ttf.GetHorizontalHeader() ?? throw new IOException("Could not get hhea table");
        HorizontalMetricsTable hm = ttf.GetHorizontalMetrics() ?? throw new IOException("Could not get hmtx table");
        using Stream input = ttf.GetOriginalData();

        int lastgid = h.NumberOfHMetrics - 1;
        bool needLastGidWidth = glyphIds.Max > lastgid && !glyphIds.Contains(lastgid);
        SkipBytes(input, hm.GetOffset());

        long lastOffset = 0;
        foreach (int gid in glyphIds)
        {
            long offset;
            if (gid <= lastgid)
            {
                if (invisibleGlyphIds.Contains(gid))
                {
                    ms.Write(PadBuf, 0, 4);
                }
                else
                {
                    offset = gid * 4L;
                    lastOffset = CopyBytes(input, ms, offset, lastOffset, 4);
                }
            }
            else
            {
                if (needLastGidWidth)
                {
                    needLastGidWidth = false;
                    offset = lastgid * 4L;
                    lastOffset = CopyBytes(input, ms, offset, lastOffset, 2);
                }

                offset = h.NumberOfHMetrics * 4L + (gid - h.NumberOfHMetrics) * 2L;
                lastOffset = CopyBytes(input, ms, offset, lastOffset, 2);
            }
        }

        return ms.ToArray();
    }

    private static long CopyBytes(Stream input, Stream output, long newOffset, long lastOffset, int count)
    {
        long nskip = newOffset - lastOffset;
        if (SkipBytes(input, nskip) != nskip)
        {
            throw new EndOfStreamException("Unexpected EOF exception parsing glyphId of hmtx table.");
        }

        byte[] buf = new byte[count];
        if (ReadFully(input, buf, 0, count) != count)
        {
            throw new EndOfStreamException("Unexpected EOF exception parsing glyphId of hmtx table.");
        }

        output.Write(buf, 0, count);
        return newOffset + count;
    }

    public void WriteToStream(Stream os)
    {
        AddCompoundReferences();
        using Stream output = os;
        long[] newLoca = new long[glyphIds.Count + 1];

        byte[] head = BuildHeadTable();
        byte[] hhea = BuildHheaTable();
        byte[] maxp = BuildMaxpTable();
        byte[]? name = BuildNameTable();
        byte[]? os2 = BuildOS2Table();
        byte[] glyf = BuildGlyfTable(newLoca);
        byte[] loca = BuildLocaTable(newLoca);
        byte[]? cmap = BuildCmapTable();
        byte[] hmtx = BuildHmtxTable();
        byte[]? post = BuildPostTable();

        SortedDictionary<string, byte[]> tables = new(StringComparer.Ordinal);
        if (os2 != null)
        {
            tables[OS2WindowsMetricsTable.TAG] = os2;
        }

        if (cmap != null)
        {
            tables[CmapTable.Tag] = cmap;
        }

        tables[GlyphTable.TAG] = glyf;
        tables[HeaderTable.TAG] = head;
        tables[HorizontalHeaderTable.TAG] = hhea;
        tables[HorizontalMetricsTable.TAG] = hmtx;
        tables[IndexToLocationTable.TAG] = loca;
        tables[MaximumProfileTable.TAG] = maxp;
        if (name != null)
        {
            tables[NamingTable.TAG] = name;
        }

        if (post != null)
        {
            tables[PostScriptTable.TAG] = post;
        }

        foreach (KeyValuePair<string, TTFTable> entry in ttf.GetTableMap())
        {
            string tag = entry.Key;
            TTFTable table = entry.Value;
            if (!tables.ContainsKey(tag) && (keepTables == null || keepTables.Contains(tag)))
            {
                tables[tag] = ttf.GetTableBytes(table);
            }
        }

        long checksum = WriteFileHeader(output, tables.Count);
        long offset = 12L + 16L * tables.Count;
        foreach (KeyValuePair<string, byte[]> entry in tables)
        {
            checksum += WriteTableHeader(output, entry.Key, offset, entry.Value);
            offset += (entry.Value.Length + 3L) / 4 * 4;
        }

        checksum = 0xB1B0AFBAL - (checksum & 0xffffffffL);
        head[8] = (byte)(checksum >> 24);
        head[9] = (byte)(checksum >> 16);
        head[10] = (byte)(checksum >> 8);
        head[11] = (byte)checksum;
        foreach (byte[] bytes in tables.Values)
        {
            WriteTableBody(output, bytes);
        }
    }

    private static void WriteFixed(Stream output, double value)
    {
        double integerPart = Math.Floor(value);
        double fractionalPart = (value - integerPart) * 65536.0;
        WriteUint16(output, (int)integerPart);
        WriteUint16(output, (int)fractionalPart);
    }

    private static void WriteUint32(Stream output, long value)
    {
        output.WriteByte((byte)(value >> 24));
        output.WriteByte((byte)((value >> 16) & 0xff));
        output.WriteByte((byte)((value >> 8) & 0xff));
        output.WriteByte((byte)(value & 0xff));
    }

    private static void WriteUint16(Stream output, int value)
    {
        output.WriteByte((byte)((value >> 8) & 0xff));
        output.WriteByte((byte)(value & 0xff));
    }

    private static void WriteSInt16(Stream output, int value)
    {
        WriteUint16(output, value & 0xffff);
    }

    private static void WriteUint8(Stream output, int value)
    {
        output.WriteByte((byte)value);
    }

    private static void WriteLongDateTime(Stream output, DateTimeOffset calendar)
    {
        DateTimeOffset epoch1904 = new(1904, 1, 1, 0, 0, 0, TimeSpan.Zero);
        long secondsSince1904 = (long)(calendar - epoch1904).TotalSeconds;
        output.WriteByte((byte)(secondsSince1904 >> 56));
        output.WriteByte((byte)(secondsSince1904 >> 48));
        output.WriteByte((byte)(secondsSince1904 >> 40));
        output.WriteByte((byte)(secondsSince1904 >> 32));
        output.WriteByte((byte)(secondsSince1904 >> 24));
        output.WriteByte((byte)(secondsSince1904 >> 16));
        output.WriteByte((byte)(secondsSince1904 >> 8));
        output.WriteByte((byte)(secondsSince1904 & 0xff));
    }

    private static long ToUInt32(int high, int low)
    {
        return ((high & 0xffffL) << 16) | (low & 0xffffL);
    }

    private static long ToUInt32(byte[] bytes)
    {
        return ((bytes[0] & 0xffL) << 24)
             | ((bytes[1] & 0xffL) << 16)
             | ((bytes[2] & 0xffL) << 8)
             | (bytes[3] & 0xffL);
    }

    private static int Log2(int num)
    {
        return (int)Math.Floor(Math.Log2(num));
    }

    private static int HighestOneBit(int num)
    {
        if (num <= 0)
        {
            return 0;
        }

        return 1 << Log2(num);
    }

    private static long SkipBytes(Stream input, long count)
    {
        if (count <= 0)
        {
            return 0;
        }

        if (input.CanSeek)
        {
            input.Seek(count, SeekOrigin.Current);
            return count;
        }

        byte[] buffer = new byte[8192];
        long remaining = count;
        while (remaining > 0)
        {
            int read = input.Read(buffer, 0, (int)Math.Min(buffer.Length, remaining));
            if (read <= 0)
            {
                break;
            }

            remaining -= read;
        }

        return count - remaining;
    }

    private static int ReadFully(Stream input, byte[] buffer, int offset, int count)
    {
        int totalRead = 0;
        while (totalRead < count)
        {
            int read = input.Read(buffer, offset + totalRead, count - totalRead);
            if (read <= 0)
            {
                break;
            }

            totalRead += read;
        }

        return totalRead;
    }

    public void AddGlyphIds(ISet<int> allGlyphIds)
    {
        glyphIds.UnionWith(allGlyphIds);
    }
}
