/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/TTFTable.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: adapted
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

namespace PdfBox.Net.FontBox.TTF;

/// <summary>
/// A table in a true type font.
/// </summary>
public class TTFTable
{
    public TTFTable()
    {
    }

    public TTFTable(string tag)
    {
        Tag = tag;
    }

    public uint Checksum { get; internal set; }

    public uint Offset { get; internal set; }

    public uint Length { get; internal set; }

    public string Tag { get; internal set; } = string.Empty;

    protected bool initialized;

    /// <summary>
    /// Indicates if the table is already initialized.
    /// </summary>
    public bool Initialized
    {
        get => initialized;
        protected set => initialized = value;
    }

    public uint GetCheckSum() => Checksum;
    internal void SetCheckSum(uint checkSumValue) => Checksum = checkSumValue;
    public uint GetLength() => Length;
    internal void SetLength(uint lengthValue) => Length = lengthValue;
    public uint GetOffset() => Offset;
    internal void SetOffset(uint offsetValue) => Offset = offsetValue;
    public string GetTag() => Tag;
    internal void SetTag(string tagValue) => Tag = tagValue;
    public bool GetInitialized() => Initialized;

    /// <summary>
    /// This will read the required data from the stream.
    /// </summary>
    internal virtual void Read(TrueTypeFont ttf, TTFDataStream data)
    {
    }

    /// <summary>
    /// This will read required headers from the stream into outHeaders.
    /// </summary>
    internal virtual void ReadHeaders(TrueTypeFont ttf, TTFDataStream data, FontHeaders outHeaders)
    {
    }

    internal void Load(TrueTypeFont ttf, TTFDataStream data)
    {
        long currentPosition = data.GetCurrentPosition();
        data.Seek(Offset);
        Read(ttf, data);
        data.Seek(currentPosition);
    }
}
