/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/HorizontalHeaderTable.java
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

public sealed class HorizontalHeaderTable() : TTFTable(TAG)
{
    public const string TAG = "hhea";
    public float Version { get; set; }
    public short Ascender { get; set; }
    public short Descender { get; set; }
    public short LineGap { get; set; }
    public int AdvanceWidthMax { get; set; }
    public short MinLeftSideBearing { get; set; }
    public short MinRightSideBearing { get; set; }
    public short XMaxExtent { get; set; }
    public short CaretSlopeRise { get; set; }
    public short CaretSlopeRun { get; set; }
    public short Reserved1 { get; set; }
    public short Reserved2 { get; set; }
    public short Reserved3 { get; set; }
    public short Reserved4 { get; set; }
    public short Reserved5 { get; set; }
    public short MetricDataFormat { get; set; }
    public int NumberOfHMetrics { get; set; }

    internal override void Read(TrueTypeFont font, TTFDataStream data)
    {
        Version = data.Read32Fixed();
        Ascender = data.ReadSignedShort();
        Descender = data.ReadSignedShort();
        LineGap = data.ReadSignedShort();
        AdvanceWidthMax = data.ReadUnsignedShort();
        MinLeftSideBearing = data.ReadSignedShort();
        MinRightSideBearing = data.ReadSignedShort();
        XMaxExtent = data.ReadSignedShort();
        CaretSlopeRise = data.ReadSignedShort();
        CaretSlopeRun = data.ReadSignedShort();
        Reserved1 = data.ReadSignedShort();
        Reserved2 = data.ReadSignedShort();
        Reserved3 = data.ReadSignedShort();
        Reserved4 = data.ReadSignedShort();
        Reserved5 = data.ReadSignedShort();
        MetricDataFormat = data.ReadSignedShort();
        NumberOfHMetrics = data.ReadUnsignedShort();
        Initialized = true;
    }
}
