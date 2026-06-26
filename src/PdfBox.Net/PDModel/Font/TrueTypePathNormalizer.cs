/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/font/PDCIDFontType2.java
 * PDFBOX_SOURCE_COMMIT: 853e0761ff9db37ee8ed1e63fe4823d8afea21e4
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 853e0761ff9db37ee8ed1e63fe4823d8afea21e4
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

using PdfBox.Net.FontBox.TTF;
using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.PDModel.Font;

internal static class TrueTypePathNormalizer
{
    internal static GeneralPath GetNormalizedPath(TrueTypeFont trueTypeFont, int gid, bool drawGidZero)
    {
        ArgumentNullException.ThrowIfNull(trueTypeFont);

        if (gid == 0 && !drawGidZero)
        {
            return new GeneralPath();
        }

        GeneralPath path = trueTypeFont.GetGlyph()?.GetGlyph(gid)?.GetPath() ?? new GeneralPath();
        int unitsPerEm = Math.Max(1, trueTypeFont.GetUnitsPerEm());
        return unitsPerEm == 1000 ? path : Scale(path, 1000f / unitsPerEm);
    }

    private static GeneralPath Scale(GeneralPath path, float scale)
    {
        var scaled = new GeneralPath();
        foreach (GeneralPath.Segment segment in path.Segments)
        {
            switch (segment.Type)
            {
                case GeneralPath.SegmentType.MoveTo:
                    scaled.MoveTo(segment.X1 * scale, segment.Y1 * scale);
                    break;
                case GeneralPath.SegmentType.LineTo:
                    scaled.LineTo(segment.X1 * scale, segment.Y1 * scale);
                    break;
                case GeneralPath.SegmentType.QuadTo:
                    scaled.QuadTo(
                        segment.X1 * scale,
                        segment.Y1 * scale,
                        segment.X2 * scale,
                        segment.Y2 * scale);
                    break;
                case GeneralPath.SegmentType.CurveTo:
                    scaled.CurveTo(
                        segment.X1 * scale,
                        segment.Y1 * scale,
                        segment.X2 * scale,
                        segment.Y2 * scale,
                        segment.X3 * scale,
                        segment.Y3 * scale);
                    break;
                case GeneralPath.SegmentType.Close:
                    scaled.ClosePath();
                    break;
            }
        }

        return scaled;
    }
}
