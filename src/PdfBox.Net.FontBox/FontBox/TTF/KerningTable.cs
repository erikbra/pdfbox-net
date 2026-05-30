/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/KerningTable.java
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

namespace PdfBox.Net.FontBox.TTF;

public class KerningTable() : TTFTable(TAG)
{
    public const string TAG = "kern";

    private KerningSubtable[]? subtables;

    internal override void Read(TrueTypeFont ttf, TTFDataStream data)
    {
        int version = data.ReadUnsignedShort();
        if (version != 0)
        {
            version = (version << 16) | data.ReadUnsignedShort();
        }

        int numSubtables = version switch
        {
            0 => data.ReadUnsignedShort(),
            1 => (int)data.ReadUnsignedInt(),
            _ => 0
        };

        if (numSubtables > 0)
        {
            subtables = new KerningSubtable[numSubtables];
            for (int i = 0; i < numSubtables; ++i)
            {
                KerningSubtable subtable = new();
                subtable.Read(data, version);
                subtables[i] = subtable;
            }
        }

        initialized = true;
    }

    public KerningSubtable? GetHorizontalKerningSubtable() => GetHorizontalKerningSubtable(false);

    public KerningSubtable? GetHorizontalKerningSubtable(bool cross)
    {
        if (subtables != null)
        {
            foreach (KerningSubtable subtable in subtables)
            {
                if (subtable.IsHorizontalKerning(cross))
                {
                    return subtable;
                }
            }
        }

        return null;
    }
}
