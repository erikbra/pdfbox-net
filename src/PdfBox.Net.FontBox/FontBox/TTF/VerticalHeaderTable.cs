/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/VerticalHeaderTable.java
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

public class VerticalHeaderTable() : TTFTable(TAG)
{
    public const string TAG = "vhea";

    public float Version { get; private set; }
    public short Ascender { get; private set; }
    public short Descender { get; private set; }
    public short LineGap { get; private set; }
    public int AdvanceHeightMax { get; private set; }
    public short MinTopSideBearing { get; private set; }
    public short MinBottomSideBearing { get; private set; }
    public short YMaxExtent { get; private set; }
    public short CaretSlopeRise { get; private set; }
    public short CaretSlopeRun { get; private set; }
    public short CaretOffset { get; private set; }
    public short Reserved1 { get; private set; }
    public short Reserved2 { get; private set; }
    public short Reserved3 { get; private set; }
    public short Reserved4 { get; private set; }
    public short MetricDataFormat { get; private set; }
    public int NumberOfVMetrics { get; private set; }

    internal override void Read(TrueTypeFont ttf, TTFDataStream data)
    {
        Version = data.Read32Fixed();
        Ascender = data.ReadSignedShort();
        Descender = data.ReadSignedShort();
        LineGap = data.ReadSignedShort();
        AdvanceHeightMax = data.ReadUnsignedShort();
        MinTopSideBearing = data.ReadSignedShort();
        MinBottomSideBearing = data.ReadSignedShort();
        YMaxExtent = data.ReadSignedShort();
        CaretSlopeRise = data.ReadSignedShort();
        CaretSlopeRun = data.ReadSignedShort();
        CaretOffset = data.ReadSignedShort();
        Reserved1 = data.ReadSignedShort();
        Reserved2 = data.ReadSignedShort();
        Reserved3 = data.ReadSignedShort();
        Reserved4 = data.ReadSignedShort();
        MetricDataFormat = data.ReadSignedShort();
        NumberOfVMetrics = data.ReadUnsignedShort();
        initialized = true;
    }

    public int GetAdvanceHeightMax() => AdvanceHeightMax;
    public short GetAscender() => Ascender;
    public short GetCaretSlopeRise() => CaretSlopeRise;
    public short GetCaretSlopeRun() => CaretSlopeRun;
    public short GetCaretOffset() => CaretOffset;
    public short GetDescender() => Descender;
    public short GetLineGap() => LineGap;
    public short GetMetricDataFormat() => MetricDataFormat;
    public short GetMinTopSideBearing() => MinTopSideBearing;
    public short GetMinBottomSideBearing() => MinBottomSideBearing;
    public int GetNumberOfVMetrics() => NumberOfVMetrics;
    public short GetReserved1() => Reserved1;
    public short GetReserved2() => Reserved2;
    public short GetReserved3() => Reserved3;
    public short GetReserved4() => Reserved4;
    public float GetVersion() => Version;
    public short GetYMaxExtent() => YMaxExtent;
}
