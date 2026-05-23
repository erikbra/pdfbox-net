/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache FontBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/TTFParser.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: trunk
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

using PdfBox.Net.IO;

namespace PdfBox.Net.FontBox.TTF;

public class TTFParser(bool allowOpenType = false)
{
    private const uint SfntTrueType = 0x00010000;
    private const uint SfntTrue = 0x74727565;
    private const uint SfntType1 = 0x74797031;
    private const uint SfntOtto = 0x4F54544F;

    private readonly bool _allowOpenType = allowOpenType;

    public virtual TrueTypeFont Parse(RandomAccessRead randomAccessRead)
    {
        long length = randomAccessRead.Length();
        if (length > int.MaxValue)
        {
            throw new IOException("TTF input is too large");
        }

        long position = randomAccessRead.GetPosition();
        randomAccessRead.Seek(0);
        byte[] bytes = new byte[length];
        randomAccessRead.ReadFully(bytes);
        randomAccessRead.Seek(position);
        return Parse(bytes);
    }

    public virtual TrueTypeFont Parse(byte[] bytes)
    {
        MemoryTTFDataStream dataStream = new(bytes);
        uint sfntVersion = dataStream.ReadUnsignedInt();
        if (!IsSupportedVersion(sfntVersion))
        {
            throw new IOException($"Unsupported sfnt version 0x{sfntVersion:X8}");
        }

        TrueTypeFont font = sfntVersion == SfntOtto ? new OpenTypeFont() : new TrueTypeFont();
        font.SfntVersion = sfntVersion;
        font.NumberOfTables = dataStream.ReadUnsignedShort();
        _ = dataStream.ReadUnsignedShort();
        _ = dataStream.ReadUnsignedShort();
        _ = dataStream.ReadUnsignedShort();

        List<TTFTable> tables = [];
        for (int i = 0; i < font.NumberOfTables; i++)
        {
            string tag = dataStream.ReadTag();
            TTFTable table = CreateTable(tag);
            table.Checksum = dataStream.ReadUnsignedInt();
            table.Offset = dataStream.ReadUnsignedInt();
            table.Length = dataStream.ReadUnsignedInt();
            ValidateTableRange(dataStream.Length, table);
            font.AddTable(table);
            tables.Add(table);
        }

        foreach (TTFTable table in tables.OrderBy(GetLoadPriority).ThenBy(t => t.Offset))
        {
            table.Load(font, dataStream);
        }

        return font;
    }

    protected virtual TTFTable CreateTable(string tag)
    {
        return tag switch
        {
            "head" => new HeaderTable(),
            "maxp" => new MaximumProfileTable(),
            "name" => new NamingTable(),
            "cmap" => new CmapTable(),
            "hhea" => new HorizontalHeaderTable(),
            "hmtx" => new HorizontalMetricsTable(),
            "loca" => new IndexToLocationTable(),
            "glyf" => new GlyphTable(),
            "post" => new PostScriptTable(),
            _ => new TTFTable(tag),
        };
    }

    private static int GetLoadPriority(TTFTable table)
    {
        return table.Tag switch
        {
            "head" => 0,
            "maxp" => 1,
            "hhea" => 2,
            "hmtx" => 3,
            "loca" => 4,
            "cmap" => 5,
            "glyf" => 6,
            "post" => 7,
            "name" => 8,
            _ => 100,
        };
    }

    private bool IsSupportedVersion(uint sfntVersion)
    {
        return sfntVersion == SfntTrueType ||
               sfntVersion == SfntTrue ||
               sfntVersion == SfntType1 ||
               (_allowOpenType && sfntVersion == SfntOtto);
    }

    private static void ValidateTableRange(long streamLength, TTFTable table)
    {
        long end = (long)table.Offset + table.Length;
        if (table.Offset > streamLength || end > streamLength || end < table.Offset)
        {
            throw new IOException($"Invalid table range for {table.Tag}");
        }
    }
}
