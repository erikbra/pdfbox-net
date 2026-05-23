/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache FontBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/IndexToLocationTable.java
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

public sealed class IndexToLocationTable() : TTFTable("loca")
{
    public long[] Offsets { get; private set; } = [];

    internal override void Read(TrueTypeFont font, TTFDataStream dataStream)
    {
        HeaderTable? header = font.GetHeader();
        if (header is null)
        {
            throw new IOException("Could not get head table");
        }

        int numGlyphs = font.GetNumberOfGlyphs();
        Offsets = new long[numGlyphs + 1];
        for (int i = 0; i < Offsets.Length; i++)
        {
            Offsets[i] = header.IndexToLocFormat == 0
                ? dataStream.ReadUnsignedShort() * 2L
                : dataStream.ReadUnsignedInt();
        }

        if (numGlyphs == 1 && Offsets[0] == 0 && Offsets[1] == 0)
        {
            throw new IOException("The font has no glyphs");
        }
    }
}
