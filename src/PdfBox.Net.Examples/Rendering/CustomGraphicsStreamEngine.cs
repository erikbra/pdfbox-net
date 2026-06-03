/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/rendering/CustomGraphicsStreamEngine.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
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

using PdfBox.Net;
using PdfBox.Net.ContentStream;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.Rendering;

namespace PdfBox.Net.Examples.Rendering;

public class CustomGraphicsStreamEngine : PDFGraphicsStreamEngine
{
    public CustomGraphicsStreamEngine(PDPage page) : base(page)
    {
    }

    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("usage: CustomGraphicsStreamEngine <input-pdf>");
            return;
        }

        using (PDDocument document = Loader.LoadPDF(args[0]))
        {
            PDPage page = document.GetPage(0);
            CustomGraphicsStreamEngine engine = new CustomGraphicsStreamEngine(page);
            engine.Run();
        }
    }

    public void Run()
    {
        ProcessPage(Page);
    }

    public override void AppendRectangle(Point2D p0, Point2D p1, Point2D p2, Point2D p3)
    {
        Console.WriteLine($"appendRectangle {p0.X:F2} {p0.Y:F2}, {p1.X:F2} {p1.Y:F2}, {p2.X:F2} {p2.Y:F2}, {p3.X:F2} {p3.Y:F2}");
        base.AppendRectangle(p0, p1, p2, p3);
    }

    public override void DrawImage(PDImage pdImage)
    {
        Console.WriteLine("drawImage");
    }

    public override void Clip(int windingRule)
    {
        Console.WriteLine("clip");
        base.Clip(windingRule);
    }

    public override void MoveTo(float x, float y)
    {
        Console.WriteLine($"moveTo {x:F2} {y:F2}");
        base.MoveTo(x, y);
    }

    public override void LineTo(float x, float y)
    {
        Console.WriteLine($"lineTo {x:F2} {y:F2}");
        base.LineTo(x, y);
    }

    public override void CurveTo(float x1, float y1, float x2, float y2, float x3, float y3)
    {
        Console.WriteLine($"curveTo {x1:F2} {y1:F2}, {x2:F2} {y2:F2}, {x3:F2} {y3:F2}");
        base.CurveTo(x1, y1, x2, y2, x3, y3);
    }

    public override Point2D? GetCurrentPoint()
    {
        return base.GetCurrentPoint();
    }

    public override void ClosePath()
    {
        Console.WriteLine("closePath");
        base.ClosePath();
    }

    public override void EndPath()
    {
        Console.WriteLine("endPath");
        base.EndPath();
    }

    public override void StrokePath()
    {
        Console.WriteLine("strokePath");
        base.StrokePath();
    }

    public override void FillPath(int windingRule)
    {
        Console.WriteLine("fillPath");
        base.FillPath(windingRule);
    }

    public override void FillAndStrokePath(int windingRule)
    {
        Console.WriteLine("fillAndStrokePath");
        base.FillAndStrokePath(windingRule);
    }

    public override void ShadingFill(COSName shadingName)
    {
        Console.WriteLine($"shadingFill {shadingName}");
    }
}
