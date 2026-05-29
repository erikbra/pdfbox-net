/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: benchmark/src/main/java/org/apache/pdfbox/benchmark/Rendering.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
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
using ImageIOUtil = PdfBox.Net.Tools.ImageIO.ImageIOUtil;

namespace PdfBox.Net.Benchmark;

public static class Rendering
{
    public const string AltonaTestSuite = "target/pdfs/eci_altona-test-suite-v2_technical2_x4.pdf";
    public const string GhentCmykX4 = "target/pdfs/Ghent_PDF_Output_Suite_V50_Full/Categories/1-CMYK/Test pages/Ghent_PDF-Output-Test-V50_CMYK_X4.pdf";
    public const string Pdf32000_2008 = "target/pdfs/PDF32000_2008.pdf";
    public const string RenderOutputDir = "target/renditions";

    static Rendering()
    {
        Directory.CreateDirectory(RenderOutputDir);
    }

    public static void RenderGhentCMYKNoOutput() => RenderNoOutput(GhentCmykX4, 600f);

    public static void RenderGhentCMYK() => RenderToPngFiles(GhentCmykX4, 600f, "ghent");

    public static void RenderAltonaNoOutput() => RenderNoOutput(AltonaTestSuite, 600f);

    public static void RenderAltona() => RenderToPngFiles(AltonaTestSuite, 600f, "altona");

    public static void RenderPDFSpecNoOutput() => RenderNoOutput(Pdf32000_2008, 150f);

    public static void RenderPDFSpec() => RenderToPngFiles(Pdf32000_2008, 150f, "pdf32000_2008");

    public static void RenderNoOutput(string filePath, float dpi)
    {
        using PDDocument document = Loader.LoadPDF(filePath);
        var renderer = new PdfBox.Net.Rendering.PDFRenderer(document);
        int pageCount = document.GetNumberOfPages();
        for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
        {
            using var image = renderer.RenderImageWithDPI(pageIndex, dpi);
        }
    }

    public static void RenderToPngFiles(string filePath, float dpi, string outputPrefix)
    {
        using PDDocument document = Loader.LoadPDF(filePath);
        var renderer = new PdfBox.Net.Rendering.PDFRenderer(document);
        int pageCount = document.GetNumberOfPages();
        Directory.CreateDirectory(RenderOutputDir);
        for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
        {
            using var image = renderer.RenderImageWithDPI(pageIndex, dpi);
            string outputPath = Path.Combine(RenderOutputDir, $"{outputPrefix}-{pageIndex}.png");
            ImageIOUtil.WriteImage(image, outputPath, (int)MathF.Round(dpi));
        }
    }
}
