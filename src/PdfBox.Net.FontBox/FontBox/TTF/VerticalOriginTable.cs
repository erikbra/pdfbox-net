/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/VerticalOriginTable.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: mechanical
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

public class VerticalOriginTable() : TTFTable(TAG)
{
    public const string TAG = "VORG";

    private float version;
    private int defaultVertOriginY;
    private Dictionary<int, int> origins = [];

    internal override void Read(TrueTypeFont ttf, TTFDataStream data)
    {
        version = data.Read32Fixed();
        defaultVertOriginY = data.ReadSignedShort();
        int numVertOriginYMetrics = data.ReadUnsignedShort();
        origins = new Dictionary<int, int>(numVertOriginYMetrics);
        for (int i = 0; i < numVertOriginYMetrics; ++i)
        {
            int g = data.ReadUnsignedShort();
            int y = data.ReadSignedShort();
            origins[g] = y;
        }

        initialized = true;
    }

    public float GetVersion() => version;

    public int GetOriginY(int gid)
    {
        return origins.TryGetValue(gid, out int y) ? y : defaultVertOriginY;
    }
}
