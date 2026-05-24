/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/GlyphRenderer.java
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

using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.FontBox.TTF;

internal class GlyphRenderer(GlyphDescription glyphDescription)
{
    public GeneralPath GetPath()
    {
        Point[] points = Describe(glyphDescription);
        return CalculatePath(points);
    }

    private static Point[] Describe(GlyphDescription gd)
    {
        int endPtIndex = 0;
        int endPtOfContourIndex = -1;
        Point[] points = new Point[gd.GetPointCount()];
        for (int i = 0; i < points.Length; i++)
        {
            if (endPtOfContourIndex == -1)
            {
                endPtOfContourIndex = gd.GetEndPtOfContours(endPtIndex);
            }

            bool endPt = endPtOfContourIndex == i;
            if (endPt)
            {
                endPtIndex++;
                endPtOfContourIndex = -1;
            }

            points[i] = new Point(gd.GetXCoordinate(i), gd.GetYCoordinate(i),
                (gd.GetFlags(i) & GlyfDescript.OnCurve) != 0, endPt);
        }

        return points;
    }

    private static GeneralPath CalculatePath(Point[] points)
    {
        GeneralPath path = new();
        int start = 0;
        for (int p = 0, len = points.Length; p < len; ++p)
        {
            if (points[p].EndOfContour)
            {
                Point firstPoint = points[start];
                Point lastPoint = points[p];
                List<Point> contour = new((p - start) + 3);
                for (int q = start; q <= p; ++q)
                {
                    contour.Add(points[q]);
                }

                if (points[start].OnCurve)
                {
                    contour.Add(firstPoint);
                }
                else if (points[p].OnCurve)
                {
                    contour.Insert(0, lastPoint);
                }
                else
                {
                    Point pmid = MidValue(firstPoint, lastPoint);
                    contour.Insert(0, pmid);
                    contour.Add(pmid);
                }

                MoveTo(path, contour[0]);
                for (int j = 1, clen = contour.Count; j < clen; j++)
                {
                    Point pnow = contour[j];
                    if (pnow.OnCurve)
                    {
                        LineTo(path, pnow);
                    }
                    else if (contour[j + 1].OnCurve)
                    {
                        QuadTo(path, pnow, contour[j + 1]);
                        ++j;
                    }
                    else
                    {
                        QuadTo(path, pnow, MidValue(pnow, contour[j + 1]));
                    }
                }

                path.ClosePath();
                start = p + 1;
            }
        }

        return path;
    }

    private static void MoveTo(GeneralPath path, Point point)
    {
        path.MoveTo(point.X, point.Y);
    }

    private static void LineTo(GeneralPath path, Point point)
    {
        path.LineTo(point.X, point.Y);
    }

    private static void QuadTo(GeneralPath path, Point ctrlPoint, Point point)
    {
        path.QuadTo(ctrlPoint.X, ctrlPoint.Y, point.X, point.Y);
    }

    private static int MidValue(int a, int b)
    {
        return a + (b - a) / 2;
    }

    private static Point MidValue(Point point1, Point point2)
    {
        return new Point(MidValue(point1.X, point2.X), MidValue(point1.Y, point2.Y));
    }

    private sealed class Point(int x, int y, bool onCurve, bool endOfContour)
    {
        public int X { get; } = x;
        public int Y { get; } = y;
        public bool OnCurve { get; } = onCurve;
        public bool EndOfContour { get; } = endOfContour;

        public Point(int x, int y) : this(x, y, true, false)
        {
        }
    }
}
