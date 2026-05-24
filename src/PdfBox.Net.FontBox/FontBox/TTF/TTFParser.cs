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

using System.IO;
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
        MemoryTTFDataStream data = new(bytes);
        uint sfntVersion = data.ReadUnsignedInt();
        if (!IsSupportedVersion(sfntVersion))
        {
            throw new IOException($"Unsupported sfnt version 0x{sfntVersion:X8}");
        }

        TrueTypeFont font = sfntVersion == SfntOtto ? new OpenTypeFont() : new TrueTypeFont();
        font.SfntVersion = sfntVersion;
        font.NumberOfTables = data.ReadUnsignedShort();
        _ = data.ReadUnsignedShort();
        _ = data.ReadUnsignedShort();
        _ = data.ReadUnsignedShort();

        List<TTFTable> tables = [];
        for (int i = 0; i < font.NumberOfTables; i++)
        {
            string tag = data.ReadTag();
            TTFTable table = CreateTable(tag);
            table.Checksum = data.ReadUnsignedInt();
            table.Offset = data.ReadUnsignedInt();
            table.Length = data.ReadUnsignedInt();
            ValidateTableRange(data, table);
            tables.Add(table);
            font.AddTable(table);
        }

        string[] order = ["head", "maxp", "hhea", "name", "hmtx", "loca", "cmap", "post", "glyf"];
        HashSet<string> loaded = new(StringComparer.Ordinal);
        foreach (string tag in order)
        {
            if (font.GetTable(tag) is TTFTable table)
            {
                table.Load(font, data);
                loaded.Add(tag);
            }
        }

        foreach (TTFTable table in tables)
        {
            if (!loaded.Contains(table.Tag))
            {
                table.Load(font, data);
            }
        }

        return font;
    }

    protected virtual TTFTable CreateTable(string tag) => tag switch
    {
        "head" => new HeaderTable(),
        "maxp" => new MaximumProfileTable(),
        "name" => new NamingTable(),
        "hhea" => new HorizontalHeaderTable(),
        "hmtx" => new HorizontalMetricsTable(),
        "loca" => new IndexToLocationTable(),
        "cmap" => new CmapTable(),
        "post" => new PostScriptTable(),
        "glyf" => new GlyphTable(),
        _ => new TTFTable(tag),
    };

    protected bool IsSupportedVersion(uint version)
    {
        return version == SfntTrueType ||
               version == SfntTrue ||
               version == SfntType1 ||
               (_allowOpenType && version == SfntOtto);
    }

    protected static void ValidateTableRange(TTFDataStream data, TTFTable table)
    {
        long end = (long)table.Offset + table.Length;
        if (table.Offset > data.Length || end > data.Length || end < table.Offset)
        {
            throw new IOException($"Invalid table range for {table.Tag}");
        }
    }
}
