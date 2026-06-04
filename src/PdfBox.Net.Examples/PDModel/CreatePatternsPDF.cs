/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: examples/src/main/java/org/apache/pdfbox/examples/pdmodel/CreatePatternsPDF.java
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

using PdfBox.Net.COS;
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

            using (PDPageContentStream pcs = new PDPageContentStream(doc, page))
            {
                // Colored pattern, i.e. the pattern content stream will set its own color(s)
                PDColorSpace patternCS1 = new PDColoredTilingPattern(page.GetResources());

                PDTilingPattern tilingPattern1 = new PDTilingPattern();
                tilingPattern1.SetBBox(new PDRectangle(0, 0, 10, 10));
                tilingPattern1.SetPaintType(PDTilingPattern.PAINT_COLORED);
                tilingPattern1.SetTilingType(PDTilingPattern.TILING_CONSTANT_SPACING);
                tilingPattern1.SetXStep(10);
                tilingPattern1.SetYStep(10);

                COSName patternName1 = page.GetResources()!.Add(tilingPattern1);
                using (PDPatternContentStream cs1 = new PDPatternContentStream(tilingPattern1))
                {
                    // Set color, draw diagonal line + 2 more diagonals so that corners look good
                    cs1.SetStrokingColor(1, 0, 0);
                    cs1.MoveTo(0, 0);
                    cs1.LineTo(10, 10);
                    cs1.MoveTo(-1, 9);
                    cs1.LineTo(1, 11);
                    cs1.MoveTo(9, -1);
                    cs1.LineTo(11, 1);
                    cs1.Stroke();
                }

                pcs.SetNonStrokingColorWithPattern(patternCS1, patternName1);
                pcs.AddRect(50, 500, 200, 200);
                pcs.Fill();

                // Uncolored pattern - the color is passed later
                PDTilingPattern tilingPattern2 = new PDTilingPattern();
                tilingPattern2.SetBBox(new PDRectangle(0, 0, 10, 10));
                tilingPattern2.SetPaintType(PDTilingPattern.PAINT_UNCOLORED);
                tilingPattern2.SetTilingType(PDTilingPattern.TILING_NO_DISTORTION);
                tilingPattern2.SetXStep(10);
                tilingPattern2.SetYStep(10);

                COSName patternName2 = page.GetResources()!.Add(tilingPattern2);
                using (PDPatternContentStream cs2 = new PDPatternContentStream(tilingPattern2))
                {
                    // draw a cross
                    cs2.MoveTo(0, 5);
                    cs2.LineTo(10, 5);
                    cs2.MoveTo(5, 0);
                    cs2.LineTo(5, 10);
                    cs2.Stroke();
                }

                // Uncolored pattern colorspace needs to know the colorspace
                // for the color values that will be passed when painting the fill
                PDColorSpace patternCS2 = new PDUncoloredTilingPattern(page.GetResources(), PDDeviceRGB.Instance);
                PDColor patternColor2green = new PDColor(
                    new float[] { 0, 1, 0 },
                    patternName2,
                    patternCS2);

                pcs.SetNonStrokingColor(patternColor2green);
                pcs.AddRect(300, 500, 100, 100);
                pcs.Fill();

                // same pattern again but with different color + different pattern start position
                PDColor patternColor2blue = new PDColor(
                    new float[] { 0, 0, 1 },
                    patternName2,
                    patternCS2);
                pcs.SetNonStrokingColor(patternColor2blue);
                pcs.AddRect(455, 505, 100, 100);
                pcs.Fill();
            }

            string outputPath = args.Length > 0 ? args[0] : "patterns.pdf";
            doc.Save(outputPath);
        }
    }
}
