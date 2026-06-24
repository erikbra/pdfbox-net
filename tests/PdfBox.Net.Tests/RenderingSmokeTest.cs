/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: adapted
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
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Graphics;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.PDModel.Graphics.State;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Rendering;
using SkiaSharp;
using System.Text;

namespace PdfBox.Net.Tests;

/// <summary>
/// Smoke tests that verify <see cref="PDFRenderer.RenderImage"/> produces real pixel output.
/// These tests exercise the SkiaSharp rendering backend introduced in issue #33.
/// </summary>
public class RenderingSmokeTest
{
    // ── Helper ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a minimal PDF document with a single page whose content stream
    /// is the supplied PDF operator text. The page is Letter-size (612×792 pt).
    /// </summary>
    private static PDDocument CreateDocument(string contentStream, PDModel.Resources.PDResources? resources = null)
    {
        var document = new PDDocument();
        var page = new PDPage(); // Letter: 612×792
        document.AddPage(page);

        var pageDict = (COSDictionary)page.GetCOSObject();
        if (resources is null)
        {
            pageDict.SetItem(COSName.RESOURCES, new COSDictionary());
        }
        else
        {
            page.SetResources(resources);
        }

        var stream = new COSStream();
        using (Stream output = stream.CreateOutputStream())
        {
            byte[] bytes = Encoding.Latin1.GetBytes(contentStream);
            output.Write(bytes, 0, bytes.Length);
        }

        pageDict.SetItem(COSName.CONTENTS, stream);
        return document;
    }

    // ── RenderImage returns a correctly-sized bitmap ──────────────────────────

    [Fact]
    public void RenderImage_EmptyPage_ReturnsCorrectDimensions()
    {
        // Letter at scale=1 → 612×792 pixels
        using var document = CreateDocument(string.Empty);
        var renderer = new PDFRenderer(document);

        using BufferedImage image = renderer.RenderImage(0, 1f);

        Assert.Equal(612, image.Width);
        Assert.Equal(792, image.Height);
    }

    [Fact]
    public void RenderImage_ScaleTwo_ReturnsTwiceTheDimensions()
    {
        using var document = CreateDocument(string.Empty);
        var renderer = new PDFRenderer(document);

        using BufferedImage image = renderer.RenderImage(0, 2f);

        Assert.Equal(1224, image.Width);
        Assert.Equal(1584, image.Height);
    }

    [Fact]
    public void RenderImageWithDPI_144DPI_ReturnsTwiceBaseDimensions()
    {
        // 144 DPI / 72 DPI = scale 2
        using var document = CreateDocument(string.Empty);
        var renderer = new PDFRenderer(document);

        using BufferedImage image = renderer.RenderImageWithDPI(0, 144f);

        Assert.Equal(1224, image.Width);
        Assert.Equal(1584, image.Height);
    }

    // ── Empty page is white / transparent ────────────────────────────────────

    [Fact]
    public void RenderImage_EmptyPageRgb_IsWhite()
    {
        using var document = CreateDocument(string.Empty);
        var renderer = new PDFRenderer(document);

        using BufferedImage image = renderer.RenderImage(0, 1f, ImageType.RGB);

        // Centre pixel should be white (background fill).
        int argb = image.GetRgb(306, 396);
        int r = (argb >> 16) & 0xFF;
        int g = (argb >> 8) & 0xFF;
        int b = argb & 0xFF;
        Assert.Equal(255, r);
        Assert.Equal(255, g);
        Assert.Equal(255, b);
    }

    [Fact]
    public void RenderImage_BlendModeResource_RendersThroughArgbBackingImage()
    {
        var resources = new PDResources();
        PDExtendedGraphicsState graphicsState = new();
        graphicsState.SetBlendMode(BlendMode.MULTIPLY);
        resources.Put(COSName.GetPDFName("GS1"), graphicsState);
        using var document = CreateDocument(string.Empty, resources);
        var renderer = new PDFRenderer(document);

        using BufferedImage image = renderer.RenderImage(0, 1f, ImageType.RGB);
        BufferedImage? backingImage = renderer.GetPageImage();

        Assert.Equal(BufferedImage.TYPE_INT_RGB, image.Type);
        Assert.NotNull(backingImage);
        Assert.Equal(BufferedImage.TYPE_INT_ARGB, backingImage!.Type);
        backingImage.Dispose();
    }

    // ── Filled rectangle produces non-white pixels ────────────────────────────

    /// <summary>
    /// PDF operator sequence:
    ///   0.8 0.2 0.2 rg    set non-stroking colour (red-ish, RGB)
    ///   100 300 200 100 re fill that rectangle
    ///   f                  fill
    ///
    /// The rectangle is in PDF user space: lower-left (100, 300), 200 wide, 100 tall.
    /// At scale=1 it maps to canvas pixels approximately:
    ///   x: 100..300
    ///   y: 792-400=392 .. 792-300=492  (Y is flipped)
    /// Centre of rectangle in canvas coords ≈ (200, 442).
    /// </summary>
    [Fact]
    public void RenderImage_FilledRectangle_PixelInsideRectIsNotWhite()
    {
        const string cs = "0.8 0.2 0.2 rg\n100 300 200 100 re\nf\n";
        using var document = CreateDocument(cs);
        var renderer = new PDFRenderer(document);

        using BufferedImage image = renderer.RenderImage(0, 1f, ImageType.RGB);

        // Pixel at the centre of the filled rectangle (canvas coords: ~200, 442)
        int argb = image.GetRgb(200, 442);
        int r = (argb >> 16) & 0xFF;
        int g = (argb >> 8) & 0xFF;
        int b = argb & 0xFF;

        // Should be reddish, definitely NOT all-white.
        Assert.False(r == 255 && g == 255 && b == 255,
            $"Expected non-white pixel inside filled rectangle but got RGB({r},{g},{b})");

        // Red channel should dominate.
        Assert.True(r > g, $"Expected R > G but got R={r}, G={g}");
        Assert.True(r > b, $"Expected R > B but got R={r}, B={b}");
    }

    [Fact]
    public void RenderImage_FilledRectangle_PixelOutsideRectIsWhite()
    {
        const string cs = "0.8 0.2 0.2 rg\n100 300 200 100 re\nf\n";
        using var document = CreateDocument(cs);
        var renderer = new PDFRenderer(document);

        using BufferedImage image = renderer.RenderImage(0, 1f, ImageType.RGB);

        // Pixel well outside the rectangle (near top-left, canvas coords: 10, 10)
        int argb = image.GetRgb(10, 10);
        int r = (argb >> 16) & 0xFF;
        int g = (argb >> 8) & 0xFF;
        int b = argb & 0xFF;

        Assert.Equal(255, r);
        Assert.Equal(255, g);
        Assert.Equal(255, b);
    }

    // ── Stroked rectangle produces non-white pixels on its border ─────────────

    [Fact]
    public void RenderImage_StrokedRectangle_PixelOnBorderIsNotWhite()
    {
        // Thick (10pt) black stroke for easy detection.
        const string cs = "0 0 0 RG\n10 w\n50 600 100 50 re\nS\n";
        using var document = CreateDocument(cs);
        var renderer = new PDFRenderer(document);

        using BufferedImage image = renderer.RenderImage(0, 1f, ImageType.RGB);

        // Bottom edge of the rectangle in PDF space is y=600.
        // In canvas coords: y = 792 - 600 = 192. With 10pt stroke half-width = 5,
        // the stroke spans canvas y ≈ 187..197. Check y=192.
        // x centre of the bottom edge ≈ 50 + 50 = 100.
        int argb = image.GetRgb(100, 192);
        int r = (argb >> 16) & 0xFF;
        int g = (argb >> 8) & 0xFF;
        int b = argb & 0xFF;

        Assert.False(r == 255 && g == 255 && b == 255,
            $"Expected non-white pixel on stroked border but got RGB({r},{g},{b})");
    }

    // ── Multiple filled regions ────────────────────────────────────────────────

    [Fact]
    public void RenderImage_TwoFilledRects_BothRegionsAreColored()
    {
        // Blue rectangle near top-left; green rectangle near bottom-right.
        const string cs =
            "0.2 0.2 0.8 rg\n50 650 120 80 re\nf\n" +
            "0.2 0.8 0.2 rg\n400 50 120 80 re\nf\n";
        using var document = CreateDocument(cs);
        var renderer = new PDFRenderer(document);

        using BufferedImage image = renderer.RenderImage(0, 1f, ImageType.RGB);

        // Blue rectangle centre: PDF (110, 690) → canvas (110, 792-690=102)
        int blueArgb = image.GetRgb(110, 102);
        int br = (blueArgb >> 16) & 0xFF;
        int bg = (blueArgb >> 8) & 0xFF;
        int bb = blueArgb & 0xFF;
        Assert.True(bb > br, $"Expected blue channel > red in blue rectangle, got R={br} B={bb}");

        // Green rectangle centre: PDF (460, 90) → canvas (460, 792-90=702)
        int greenArgb = image.GetRgb(460, 702);
        int gr = (greenArgb >> 16) & 0xFF;
        int gg = (greenArgb >> 8) & 0xFF;
        int gb = greenArgb & 0xFF;
        Assert.True(gg > gr, $"Expected green channel > red in green rectangle, got G={gg} R={gr}");
    }

    [Fact]
    public void RenderImage_TextContent_DrawsVisibleGlyphPixels()
    {
        var resources = new PDModel.Resources.PDResources();
        resources.Put(COSName.GetPDFName("F1"), new PDType1Font(PDType1Font.FontName.HELVETICA_BOLD));
        using var document = CreateDocument("BT\n0 0 0 rg\n/F1 48 Tf\n100 500 Td\n(Hi) Tj\nET\n", resources);

        var renderer = new PDFRenderer(document);
        using BufferedImage image = renderer.RenderImage(0, 1f, ImageType.RGB);

        int textRegionPixels = CountNonWhitePixels(image, 80, 230, 180, 120);
        int pagePixels = CountNonWhitePixels(image, 0, 0, image.Width, image.Height);
        Assert.True(textRegionPixels > 40,
            $"Expected visible text pixels in region but saw {textRegionPixels}; full page non-white pixels: {pagePixels}");
    }

    [Fact]
    public void RenderImage_PdfBox5002Standard14Fallback_DrawsVisibleText()
    {
        byte[] pdf = Convert.FromBase64String(PdfBox5002FixtureBase64);
        using PDDocument document = Loader.LoadPDF(pdf);

        using BufferedImage image = new PDFRenderer(document).RenderImageWithDPI(0, 36f, ImageType.RGB);

        (bool nearBlank, int nonBackground, int dominant) = MeasureNearBlank(image);
        Assert.False(nearBlank,
            $"Expected visible text glyph pixels but render was near blank; nonBackground={nonBackground}, dominant={dominant}.");
    }

    [Theory]
    [InlineData("rotation.pdf")]
    [InlineData("with_outline.pdf")]
    public void RenderImage_RuntimePlaceholderFixtures_DrawVisibleFirstPage(string fixtureName)
    {
        string fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", fixtureName);
        using PDDocument document = Loader.LoadPDF(fixturePath);

        using BufferedImage image = new PDFRenderer(document).RenderImageWithDPI(0, 36f, ImageType.RGB);

        (bool nearBlank, int nonBackground, int dominant) = MeasureNearBlank(image);
        Assert.False(nearBlank,
            $"Expected visible text glyph pixels for {fixtureName} but render was near blank; nonBackground={nonBackground}, dominant={dominant}.");
    }

    [Fact]
    public void RenderImage_ImageXObject_DrawsDecodedImagePixels()
    {
        using var document = new PDDocument();
        using var source = new SKBitmap(2, 2);
        source.SetPixel(0, 0, SKColors.Red);
        source.SetPixel(1, 0, SKColors.Green);
        source.SetPixel(0, 1, SKColors.Blue);
        source.SetPixel(1, 1, SKColors.White);
        PDImageXObject imageXObject = LosslessFactory.CreateFromImage(document, source);

        var resources = new PDModel.Resources.PDResources();
        resources.Put(COSName.GetPDFName("Im1"), imageXObject);
        using PDDocument pageDocument = CreateDocument("q\n80 0 0 80 100 300 cm\n/Im1 Do\nQ\n", resources);

        var renderer = new PDFRenderer(pageDocument);
        using BufferedImage image = renderer.RenderImage(0, 1f, ImageType.RGB);

        int redArgb = image.GetRgb(120, 422);
        int redR = (redArgb >> 16) & 0xFF;
        int redG = (redArgb >> 8) & 0xFF;
        int redB = redArgb & 0xFF;
        int pagePixels = CountNonWhitePixels(image, 0, 0, image.Width, image.Height);
        Assert.True(redR > redG && redR > redB,
            $"Expected red-dominant image pixel but got RGB({redR},{redG},{redB}); full page non-white pixels: {pagePixels}");

        int blueArgb = image.GetRgb(120, 462);
        int blueR = (blueArgb >> 16) & 0xFF;
        int blueG = (blueArgb >> 8) & 0xFF;
        int blueB = blueArgb & 0xFF;
        Assert.True(blueB > blueR && blueB > blueG, $"Expected blue-dominant image pixel but got RGB({blueR},{blueG},{blueB})");
    }

    private static int CountNonWhitePixels(BufferedImage image, int x, int y, int width, int height)
    {
        int count = 0;
        for (int yy = y; yy < y + height; yy++)
        {
            for (int xx = x; xx < x + width; xx++)
            {
                int argb = image.GetRgb(xx, yy);
                int r = (argb >> 16) & 0xFF;
                int g = (argb >> 8) & 0xFF;
                int b = argb & 0xFF;
                if (r != 255 || g != 255 || b != 255)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static (bool NearBlank, int NonBackground, int Dominant) MeasureNearBlank(BufferedImage image)
    {
        int total = image.Width * image.Height;
        int background = image.GetRgb(0, 0);
        var histogram = new Dictionary<int, int>();
        int nonBackground = 0;
        int transparent = 0;
        int dominant = 0;

        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                int argb = image.GetRgb(x, y);
                int alpha = (argb >> 24) & 0xFF;
                if (alpha < 8)
                {
                    transparent++;
                }

                if (ColorDistance(argb, background) > 8)
                {
                    nonBackground++;
                }

                histogram.TryGetValue(argb, out int count);
                count++;
                histogram[argb] = count;
                dominant = Math.Max(dominant, count);
            }
        }

        bool nearBlank = transparent == total ||
                         nonBackground <= Math.Max(10, total / 1000) ||
                         dominant >= (int)Math.Ceiling(total * 0.995);
        return (nearBlank, nonBackground, dominant);
    }

    private static int ColorDistance(int left, int right)
    {
        int lr = (left >> 16) & 0xFF;
        int lg = (left >> 8) & 0xFF;
        int lb = left & 0xFF;
        int rr = (right >> 16) & 0xFF;
        int rg = (right >> 8) & 0xFF;
        int rb = right & 0xFF;
        return Math.Abs(lr - rr) + Math.Abs(lg - rg) + Math.Abs(lb - rb);
    }

    // Apache PDFBox test resource input/PDFBOX-5002.pdf, kept inline to avoid a binary test fixture.
    private const string PdfBox5002FixtureBase64 =
        "JVBERi0xLjQKJfbk/N8KMSAwIG9iago8PAovVHlwZSAvQ2F0YWxvZwovVmVyc2lvbiAvMS40Ci9QYWdlcyAyIDAg" +
        "Ugo+PgplbmRvYmoKMiAwIG9iago8PAovVHlwZSAvUGFnZXMKL0tpZHMgWzMgMCBSXQovQ291bnQgMQo+PgplbmRv" +
        "YmoKMyAwIG9iago8PAovVHlwZSAvUGFnZQovTWVkaWFCb3ggWzAuMCAwLjAgNTk1LjI3NTYzIDg0MS44ODk4XQov" +
        "UGFyZW50IDIgMCBSCi9Db250ZW50cyA0IDAgUgovUmVzb3VyY2VzIDUgMCBSCj4+CmVuZG9iago0IDAgb2JqCjw8" +
        "Ci9MZW5ndGggMTQ4Ci9GaWx0ZXIgL0ZsYXRlRGVjb2RlCj4+CnN0cmVhbQ0KeJxVzssOgjAQheH9PMVZ6kZaIoJb" +
        "4mVv+gKIgw4pxdh6eXyRpEnNbL/5Z7KDRq5gOsp2/JKWT8carSeF3/iWakOFQpXrVVVtyxLmQgsjwTLEocFZruhG" +
        "F5YwPe0NZVNPz720sJ4TRZLw48AI/Al4S7jF7Uj1JqEN/NBYy4+/Q5GqMqGjw/C0Qe7Te1Yc+6i/Uq07yg0KZW5k" +
        "c3RyZWFtCmVuZG9iago1IDAgb2JqCjw8Ci9Gb250IDYgMCBSCj4+CmVuZG9iago2IDAgb2JqCjw8Ci9GMSA3IDAg" +
        "Ugo+PgplbmRvYmoKNyAwIG9iago8PAovVHlwZSAvRm9udAovU3VidHlwZSAvVHlwZTEKL0Jhc2VGb250IC9IZWx2" +
        "ZXRpY2EtQm9sZAovRW5jb2RpbmcgL1dpbkFuc2lFbmNvZGluZwo+PgplbmRvYmoKeHJlZgowIDgKMDAwMDAwMDAw" +
        "MCA2NTUzNSBmDQowMDAwMDAwMDE1IDAwMDAwIG4NCjAwMDAwMDAwNzggMDAwMDAgbg0KMDAwMDAwMDEzNSAwMDAw" +
        "MCBuDQowMDAwMDAwMjU0IDAwMDAwIG4NCjAwMDAwMDA0NzYgMDAwMDAgbg0KMDAwMDAwMDUwOSAwMDAwMCBuDQow" +
        "MDAwMDAwNTQwIDAwMDAwIG4NCnRyYWlsZXIKPDwKL1Jvb3QgMSAwIFIKL0lEIFs8RURFM0U3NThBNDkxMzJBNDVD" +
        "OTRFRkZFNkVBQTY3RjQ+IDxFREUzRTc1OEE0OTEzMkE0NUM5NEVGRkU2RUFBNjdGND5dCi9TaXplIDgKPj4Kc3Rh" +
        "cnR4cmVmCjY0MgolJUVPRgo=";
}
