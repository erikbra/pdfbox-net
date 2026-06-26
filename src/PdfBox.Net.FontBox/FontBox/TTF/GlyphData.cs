/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/GlyphData.java
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

using PdfBox.Net.FontBox.Util;
using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.FontBox.TTF;

public class GlyphData
{
    public short XMin { get; private set; }
    public short YMin { get; private set; }
    public short XMax { get; private set; }
    public short YMax { get; private set; }
    public short XMinimum => XMin;
    public short YMinimum => YMin;
    public short XMaximum => XMax;
    public short YMaximum => YMax;
    public short NumberOfContours { get; private set; }
    public BoundingBox BoundingBox { get; private set; } = new();

    private GlyfDescript? _glyphDescription;

    public GlyphDescription? Description => _glyphDescription;

    public GeneralPath GetPath()
    {
        return _glyphDescription == null ? new GeneralPath() : new GlyphRenderer(_glyphDescription).GetPath();
    }

    internal void InitData(GlyphTable glyphTable, TTFDataStream data, int leftSideBearing, int level)
    {
        NumberOfContours = data.ReadSignedShort();
        XMin = data.ReadSignedShort();
        YMin = data.ReadSignedShort();
        XMax = data.ReadSignedShort();
        YMax = data.ReadSignedShort();
        BoundingBox = new BoundingBox(XMin, YMin, XMax, YMax);

        if (NumberOfContours >= 0)
        {
            _glyphDescription = new GlyfSimpleDescript(NumberOfContours, data, (short)(leftSideBearing - XMin));
        }
        else
        {
            _glyphDescription = new GlyfCompositeDescript(data, glyphTable, level + 1);
        }
    }

    internal void InitEmptyData()
    {
        _glyphDescription = new GlyfSimpleDescript();
        BoundingBox = new BoundingBox();
    }
}
