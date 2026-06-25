/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/contentstream/PDFGraphicsStreamEngine.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
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

using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.Rendering;
using PdfBox.Net.Util;

namespace PdfBox.Net.ContentStream;

public abstract class PDFGraphicsStreamEngine : PDFStreamEngine
{
    protected PDFGraphicsStreamEngine(PDPage page)
    {
        Page = page;
    }

    protected PDPage Page { get; }

    public override Matrix GetInitialMatrix()
    {
        return base.GetInitialMatrix();
    }

    public override void AppendRectangle(float x, float y, float width, float height)
    {
        AppendRectangle(
            new Point2D(x, y),
            new Point2D(x + width, y),
            new Point2D(x + width, y + height),
            new Point2D(x, y + height));
    }

    public virtual void AppendRectangle(Point2D p0, Point2D p1, Point2D p2, Point2D p3)
    {
        MoveTo((float)p0.X, (float)p0.Y);
        LineTo((float)p1.X, (float)p1.Y);
        LineTo((float)p2.X, (float)p2.Y);
        LineTo((float)p3.X, (float)p3.Y);
        ClosePath();
    }

    public virtual void DrawImage(PDImage pdImage)
    {
    }

    public void Run(PDPage page)
    {
        ProcessPage(page);
    }
}
