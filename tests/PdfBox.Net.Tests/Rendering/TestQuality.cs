/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/rendering/TestQuality.java
 * PDFBOX_SOURCE_COMMIT: 046747da99a870902217efabf1c41297de157059
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 046747da99a870902217efabf1c41297de157059
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
using PdfBox.Net.Rendering;

namespace PdfBox.Net.Tests.Rendering;

public sealed class TestQuality
{
    /// <summary>PDFBOX-6077: gaps between pattern tiles remain transparent.</summary>
    [Fact]
    public void TestPDFBox6077()
    {
        using PDDocument document = Loader.LoadPDF(Fixture("PDFBOX-6077-example.pdf"));
        using BufferedImage rendered = new PDFRenderer(document).RenderImageWithDPI(0, 100);
        Assert.Equal(unchecked((int)0xFFFFFFFF), rendered.GetRgb(280, 23));
        int imagePixel = rendered.GetRgb(400, 585);
        int red = (imagePixel >> 16) & 0xFF;
        int green = (imagePixel >> 8) & 0xFF;
        int blue = imagePixel & 0xFF;
        Assert.True(red > 150 && green > 150 && green > blue + 30 && red > blue + 20,
            $"Expected the green image payload, got {imagePixel:X8}.");
    }

    /// <summary>PDFBOX-6077: the soft-masked stencil pattern stays visible at page-device coordinates.</summary>
    [Fact]
    public void TestPDFBox5842()
    {
        using PDDocument document = Loader.LoadPDF(Fixture("PDFBOX-5842-reduced.pdf"));
        using BufferedImage rendered = new PDFRenderer(document).RenderImageWithDPI(0, 100);
        Assert.NotEqual(unchecked((int)0xFFFFFFFF), rendered.GetRgb(267, 1329));
        // The Java reference paints this marker red. Merely asserting non-white
        // would also accept the old renderer's incorrect opaque black stencil.
        int marker = rendered.GetRgb(270, 1335);
        int red = (marker >> 16) & 0xFF;
        int green = (marker >> 8) & 0xFF;
        int blue = marker & 0xFF;
        Assert.True(red > 180 && red > green + 100 && blue > green,
            $"Expected the red map marker, got {marker:X8}.");
    }

    /// <summary>PDFBOX-5403: a tile-boundary seam must not wash out the pattern-filled text.</summary>
    [Fact]
    public void TestPDFBox5403()
    {
        using PDDocument document = Loader.LoadPDF(Fixture("PDFBOX-5403-bad-rendering.pdf"));
        using BufferedImage rendered = new PDFRenderer(document).RenderImageWithDPI(2, 100);
        int rgb = rendered.GetRgb(159, 115);
        Assert.True(((rgb >> 16) & 0xFF) < 100, $"Expected a dark text pixel, got {rgb:X8}.");
    }

    private static string Fixture(string name) =>
        Path.Combine(AppContext.BaseDirectory, "Fixtures", "Rendering", "UpstreamJira", name);
}
