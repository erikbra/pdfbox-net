/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Native regression coverage for issue #419 advanced rendering paths.
 *
 * PORT_MODE: native-test
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

using System.Text;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.Patterns;
using PdfBox.Net.PDModel.Graphics.Shading;
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Rendering;
using PdfBox.Net.Util;

namespace PdfBox.Net.Tests;

public class AdvancedRenderingIssue419Test
{
    [Fact]
    public void RenderImage_FormXObject_DrawsNestedContent()
    {
        PDResources resources = new();
        PDFormXObject form = CreateForm("0 0 1 rg\n0 0 40 40 re\nf\n", new PDRectangle(0, 0, 40, 40));
        COSName formName = COSName.GetPDFName("Fm0");
        resources.Put(formName, form);

        using PDDocument document = CreateDocument("q\n1 0 0 1 100 500 cm\n/Fm0 Do\nQ\n", resources);
        using BufferedImage image = new PDFRenderer(document).RenderImage(0, 1f, ImageType.RGB);

        Assert.True(CountNonWhitePixels(image, 100, 252, 40, 40) > 20);
    }

    [Fact]
    public void RenderImage_AnnotationAppearance_DrawsAtAnnotationRectangle()
    {
        using PDDocument document = CreateDocument(string.Empty);
        PDPage page = document.GetPage(0);

        PDAppearanceStream appearance = new(new COSStream());
        appearance.SetBBox(new PDRectangle(0, 0, 20, 20));
        WriteStream(appearance.GetCOSObject()!, "1 0 0 rg\n0 0 20 20 re\nf\n");

        PDAppearanceDictionary appearanceDictionary = new();
        appearanceDictionary.SetNormalAppearance(appearance);

        PDAnnotationSquare annotation = new();
        annotation.SetRectangle(new PDRectangle(150, 450, 20, 20));
        annotation.SetAppearance(appearanceDictionary);
        page.SetAnnotations([annotation]);

        using BufferedImage image = new PDFRenderer(document).RenderImage(0, 1f, ImageType.RGB);

        Assert.True(CountNonWhitePixels(image, 150, 322, 20, 20) > 20);
    }

    [Fact]
    public void RenderImage_AnnotationAppearance_DoesNotInheritPageContentCtm()
    {
        using PDDocument document = CreateDocument("0.24 0 0 -0.24 7.2 654.72 cm\n0 0 1 1 re\nf\n");
        PDPage page = document.GetPage(0);

        PDAppearanceStream appearance = new(new COSStream());
        appearance.SetBBox(new PDRectangle(0, 0, 20, 20));
        WriteStream(appearance.GetCOSObject()!, "1 0 0 rg\n0 0 20 20 re\nf\n");

        PDAppearanceDictionary appearanceDictionary = new();
        appearanceDictionary.SetNormalAppearance(appearance);

        PDAnnotationSquare annotation = new();
        annotation.SetRectangle(new PDRectangle(150, 450, 20, 20));
        annotation.SetAppearance(appearanceDictionary);
        page.SetAnnotations([annotation]);

        using BufferedImage image = new PDFRenderer(document).RenderImage(0, 1f, ImageType.RGB);

        Assert.True(CountNonWhitePixels(image, 150, 322, 20, 20) > 20);
    }

    [Fact]
    public void RenderImage_AnnotationAppearance_UsesTransformedAppearanceBBox()
    {
        using PDDocument document = CreateDocument(string.Empty);
        PDPage page = document.GetPage(0);

        PDAppearanceStream appearance = new(new COSStream());
        appearance.SetBBox(new PDRectangle(0, 0, 10, 10));
        appearance.SetMatrix(new Matrix(0.25f, 0, 0, 0.25f, 0, 0));
        WriteStream(appearance.GetCOSObject()!, "1 0 0 rg\n0 0 10 10 re\nf\n");

        PDAppearanceDictionary appearanceDictionary = new();
        appearanceDictionary.SetNormalAppearance(appearance);

        PDAnnotationSquare annotation = new();
        annotation.SetRectangle(new PDRectangle(120, 420, 40, 40));
        annotation.SetAppearance(appearanceDictionary);
        page.SetAnnotations([annotation]);

        using BufferedImage image = new PDFRenderer(document).RenderImage(0, 1f, ImageType.RGB);

        Assert.True(CountNonWhitePixels(image, 120, 332, 40, 40) > 600);
    }

    [Fact]
    public void SoftMask_CreateContext_AppliesMaskAlpha()
    {
        BufferedImage mask = new(2, 1, BufferedImage.TYPE_INT_ARGB);
        WritableRaster maskRaster = mask.GetRaster();
        maskRaster.SetPixel(0, 0, [0, 0, 0, 255]);
        maskRaster.SetPixel(1, 0, [255, 255, 255, 255]);

        SoftMask softMask = new(Color.Black, mask, new Rectangle2D(0, 0, 2, 1), null, null);
        using PaintContext context = softMask.CreateContext(
            new ColorModel(),
            new Rectangle(0, 0, 2, 1),
            new Rectangle2D(0, 0, 2, 1),
            new AffineTransform(),
            new RenderingHints());

        Raster raster = context.GetRaster(0, 0, 2, 1);
        int[] pixel = new int[4];

        raster.GetPixel(0, 0, pixel);
        Assert.Equal(0, pixel[3]);

        raster.GetPixel(1, 0, pixel);
        Assert.Equal(255, pixel[3]);
    }

    [Fact]
    public void TilingPaint_CreateContext_RendersPatternCell()
    {
        using PDDocument document = CreateDocument(string.Empty);
        PDFRenderer renderer = new(document);
        PageDrawer drawer = new(new PageDrawerParameters(
            renderer,
            document.GetPage(0),
            false,
            RenderDestination.VIEW,
            new RenderingHints(),
            0.5f));

        PDTilingPattern pattern = new();
        pattern.SetPaintType(PDTilingPattern.PAINT_COLORED);
        pattern.SetTilingType(PDTilingPattern.TILING_CONSTANT_SPACING);
        pattern.SetBBox(new PDRectangle(0, 0, 8, 8));
        pattern.SetXStep(8);
        pattern.SetYStep(8);
        WriteStream((COSStream)pattern.GetCOSObject(), "1 0 0 rg\n0 0 8 8 re\nf\n");

        TilingPaint tilingPaint = new(drawer, pattern, new AffineTransform());
        using PaintContext context = tilingPaint.CreateContext(
            new ColorModel(),
            new Rectangle(0, 0, 8, 8),
            new Rectangle2D(0, 0, 8, 8),
            new AffineTransform(),
            new RenderingHints());

        Raster raster = context.GetRaster(0, 0, 8, 8);

        Assert.True(CountVisiblePixels(raster, 8, 8) > 20);
    }

    [Fact]
    public void RenderImage_ShadingFill_RendersWithoutThrowing()
    {
        PDResources resources = new();
        PDShadingType2 shading = new(new COSDictionary());
        shading.SetShadingType(PDShading.SHADING_TYPE2);
        shading.SetColorSpace(PDDeviceRGB.Instance);
        shading.SetBackground(COSArray.Of(0f, 0.5f, 0f));
        shading.SetBBox(new PDRectangle(100, 300, 80, 60));
        COSName shadingName = resources.Add(shading, "Sh");

        using PDDocument document = CreateDocument($"/{shadingName.GetName()} sh\n", resources);

        using BufferedImage image = new PDFRenderer(document).RenderImage(0, 1f, ImageType.RGB);

        Assert.True(CountNonWhitePixels(image, 100, 432, 80, 60) > 20);
    }

    [Fact]
    public void RenderImage_PatternColorSpace_DoesNotThrowOnPatternColor()
    {
        PDResources resources = new();
        PDTilingPattern pattern = new();
        pattern.SetPaintType(PDTilingPattern.PAINT_COLORED);
        pattern.SetTilingType(PDTilingPattern.TILING_CONSTANT_SPACING);
        pattern.SetBBox(new PDRectangle(0, 0, 8, 8));
        pattern.SetXStep(8);
        pattern.SetYStep(8);
        WriteStream((COSStream)pattern.GetCOSObject(), "0 0 0 rg\n0 0 8 8 re\nf\n");
        COSName patternName = resources.Add(pattern);

        using PDDocument document = CreateDocument($"/Pattern cs\n/{patternName.GetName()} scn\n100 300 50 50 re\nf\n", resources);

        using BufferedImage image = new PDFRenderer(document).RenderImage(0, 1f, ImageType.RGB);

        Assert.True(CountNonWhitePixels(image, 100, 442, 50, 50) > 20);
    }

    [Fact]
    public void RenderImage_ApacheSurveyPdf_DoesNotThrowWhenFixtureIsAvailable()
    {
        string? surveyPath = FindApacheSurveyFixture();
        if (surveyPath is null)
        {
            return;
        }

        using PDDocument document = Loader.LoadPDF(surveyPath);
        using BufferedImage image = new PDFRenderer(document).RenderImage(0, 0.25f, ImageType.RGB);

        Assert.True(image.Width > 0);
        Assert.True(image.Height > 0);
    }

    private static PDDocument CreateDocument(string contentStream, PDResources? resources = null)
    {
        PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);

        if (resources is not null)
        {
            page.SetResources(resources);
        }

        WriteStream((COSDictionary)page.GetCOSObject(), contentStream);
        return document;
    }

    private static PDFormXObject CreateForm(string contentStream, PDRectangle bbox)
    {
        COSStream stream = new();
        PDFormXObject form = new(stream);
        form.SetBBox(bbox);
        WriteStream(stream, contentStream);
        return form;
    }

    private static void WriteStream(COSDictionary dictionary, string contentStream)
    {
        COSStream stream = new();
        WriteStream(stream, contentStream);
        dictionary.SetItem(COSName.CONTENTS, stream);
    }

    private static void WriteStream(COSStream stream, string contentStream)
    {
        using Stream output = stream.CreateOutputStream();
        byte[] bytes = Encoding.Latin1.GetBytes(contentStream);
        output.Write(bytes, 0, bytes.Length);
    }

    private static int CountNonWhitePixels(BufferedImage image, int x, int y, int width, int height)
    {
        int count = 0;
        for (int py = y; py < y + height; py++)
        {
            for (int px = x; px < x + width; px++)
            {
                int argb = image.GetRgb(px, py);
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

    private static int CountVisiblePixels(Raster raster, int width, int height)
    {
        int count = 0;
        int[] pixel = new int[4];
        for (int py = 0; py < height; py++)
        {
            for (int px = 0; px < width; px++)
            {
                raster.GetPixel(px, py, pixel);
                if (pixel[3] > 0)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static string? FindApacheSurveyFixture()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }

        if (directory is null)
        {
            return null;
        }

        string candidate = Path.GetFullPath(Path.Combine(
            directory.FullName,
            "..",
            "..",
            "apache",
            "pdfbox",
            "pdfbox",
            "src",
            "test",
            "resources",
            "input",
            "rendering",
            "survey.pdf"));

        return File.Exists(candidate) ? candidate : null;
    }
}
