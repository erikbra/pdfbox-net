/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/CreatePatternsPDF.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: adapted
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

using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.Patterns;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.Examples.PDModel;

/// <summary>
/// This is an example of how to create a page that uses patterns to paint areas.
/// </summary>
public static class CreatePatternsPDF
{
    public static void Main(string[] args)
    {
        using (PDDocument doc = new PDDocument())
        {
            PDPage page = new PDPage(PDRectangle.A4);
            doc.AddPage(page);
            page.SetResources(new PDResources());

            PDColorSpace patternCS1 = new PDPattern(null, PDDeviceRGB.Instance);

            PDTilingPattern tilingPattern1 = new PDTilingPattern();
            tilingPattern1.SetBBox(new PDRectangle(0, 0, 10, 10));
            tilingPattern1.SetPaintType(PDTilingPattern.PAINT_COLORED);
            tilingPattern1.SetTilingType(PDTilingPattern.TILING_CONSTANT_SPACING);
            tilingPattern1.SetXStep(10);
            tilingPattern1.SetYStep(10);

            PDTilingPattern tilingPattern2 = new PDTilingPattern();
            tilingPattern2.SetBBox(new PDRectangle(0, 0, 10, 10));
            tilingPattern2.SetPaintType(PDTilingPattern.PAINT_UNCOLORED);
            tilingPattern2.SetTilingType(PDTilingPattern.TILING_NO_DISTORTION);
            tilingPattern2.SetXStep(10);
            tilingPattern2.SetYStep(10);

            // NOTE: PDResources.Add(PDTilingPattern) and PDPageContentStream drawing operators
            // (SetNonStrokingColor, AddRect, Fill) are not yet implemented in this .NET port.
            throw new NotSupportedException(
                "PDResources.Add(PDTilingPattern) and PDPageContentStream pattern drawing " +
                "operators are not yet implemented in this .NET port.");
        }
    }
}
