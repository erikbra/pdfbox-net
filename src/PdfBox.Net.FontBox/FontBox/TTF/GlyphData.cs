/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache FontBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/GlyphData.java
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

using PdfBox.Net.FontBox.Util;

namespace PdfBox.Net.FontBox.TTF;

public sealed class GlyphData
{
    public short XMinimum { get; private set; }

    public short YMinimum { get; private set; }

    public short XMaximum { get; private set; }

    public short YMaximum { get; private set; }

    public BoundingBox BoundingBox { get; private set; } = new();

    public short NumberOfContours { get; private set; }

    public GlyphDescription Description { get; private set; } = new GlyfSimpleDescript();

    internal void InitData(GlyphTable glyphTable, TTFDataStream dataStream, int leftSideBearing, int level)
    {
        NumberOfContours = dataStream.ReadSignedShort();
        XMinimum = dataStream.ReadSignedShort();
        YMinimum = dataStream.ReadSignedShort();
        XMaximum = dataStream.ReadSignedShort();
        YMaximum = dataStream.ReadSignedShort();
        BoundingBox = new BoundingBox(XMinimum, YMinimum, XMaximum, YMaximum);

        if (NumberOfContours >= 0)
        {
            short x0 = (short)(leftSideBearing - XMinimum);
            Description = new GlyfSimpleDescript(NumberOfContours, dataStream, x0);
        }
        else
        {
            Description = new GlyfCompositeDescript(dataStream, glyphTable, level + 1);
        }
    }

    internal void InitEmptyData()
    {
        Description = new GlyfSimpleDescript();
        BoundingBox = new BoundingBox();
    }
}
