/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache FontBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/NamingTable.java
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

namespace PdfBox.Net.FontBox.TTF;

public sealed class NamingTable() : TTFTable("name")
{
    private readonly List<NameRecord> _nameRecords = [];

    public IReadOnlyList<NameRecord> NameRecords => _nameRecords;

    internal override void Read(TrueTypeFont font, TTFDataStream dataStream)
    {
        long tableStart = dataStream.Position;
        _ = dataStream.ReadUnsignedShort();
        ushort count = dataStream.ReadUnsignedShort();
        ushort stringOffset = dataStream.ReadUnsignedShort();

        _nameRecords.Clear();
        for (int i = 0; i < count; i++)
        {
            _nameRecords.Add(new NameRecord
            {
                PlatformId = dataStream.ReadUnsignedShort(),
                PlatformEncodingId = dataStream.ReadUnsignedShort(),
                LanguageId = dataStream.ReadUnsignedShort(),
                NameId = dataStream.ReadUnsignedShort(),
                StringLength = dataStream.ReadUnsignedShort(),
                StringOffset = dataStream.ReadUnsignedShort(),
            });
        }

        foreach (NameRecord record in _nameRecords)
        {
            dataStream.Seek(tableStart + stringOffset + record.StringOffset);
            byte[] bytes = dataStream.ReadBytes(record.StringLength);
            record.Value = DecodeName(record, bytes);
        }
    }

    public string? GetEnglishName(ushort nameId)
    {
        NameRecord? windows = _nameRecords.FirstOrDefault(n =>
            n.NameId == nameId &&
            n.PlatformId == 3 &&
            n.LanguageId == 0x0409 &&
            !string.IsNullOrEmpty(n.Value));
        if (windows is not null)
        {
            return windows.Value;
        }

        NameRecord? unicode = _nameRecords.FirstOrDefault(n =>
            n.NameId == nameId &&
            n.PlatformId == 0 &&
            !string.IsNullOrEmpty(n.Value));
        if (unicode is not null)
        {
            return unicode.Value;
        }

        return _nameRecords.FirstOrDefault(n => n.NameId == nameId && !string.IsNullOrEmpty(n.Value))?.Value;
    }

    private static string DecodeName(NameRecord record, byte[] bytes)
    {
        try
        {
            if (record.PlatformId == 0 || record.PlatformId == 3)
            {
                return System.Text.Encoding.BigEndianUnicode.GetString(bytes);
            }

            return System.Text.Encoding.ASCII.GetString(bytes);
        }
        catch
        {
            return string.Empty;
        }
    }
}
