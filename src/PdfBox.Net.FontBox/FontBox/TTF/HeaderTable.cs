/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache FontBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/HeaderTable.java
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

public sealed class HeaderTable() : TTFTable("head")
{
    public ushort UnitsPerEm { get; private set; }

    public short XMin { get; private set; }

    public short YMin { get; private set; }

    public short XMax { get; private set; }

    public short YMax { get; private set; }

    internal override void Read(TTFDataStream dataStream)
    {
        _ = dataStream.Read32Fixed();
        _ = dataStream.Read32Fixed();
        _ = dataStream.ReadUnsignedInt();
        _ = dataStream.ReadUnsignedInt();
        _ = dataStream.ReadUnsignedShort();
        UnitsPerEm = dataStream.ReadUnsignedShort();
        _ = ReadLongDateTime(dataStream);
        _ = ReadLongDateTime(dataStream);
        XMin = dataStream.ReadSignedShort();
        YMin = dataStream.ReadSignedShort();
        XMax = dataStream.ReadSignedShort();
        YMax = dataStream.ReadSignedShort();
        _ = dataStream.ReadUnsignedShort();
        _ = dataStream.ReadUnsignedShort();
        _ = dataStream.ReadSignedShort();
        _ = dataStream.ReadSignedShort();
        _ = dataStream.ReadSignedShort();
    }

    private static long ReadLongDateTime(TTFDataStream dataStream)
    {
        uint high = dataStream.ReadUnsignedInt();
        uint low = dataStream.ReadUnsignedInt();
        return unchecked(((long)high << 32) | low);
    }
}
