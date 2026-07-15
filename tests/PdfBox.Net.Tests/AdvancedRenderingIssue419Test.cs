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
using PdfBox.Net.PDModel.Graphics;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.PDModel.Graphics.Patterns;
using PdfBox.Net.PDModel.Graphics.Shading;
using PdfBox.Net.PDModel.Graphics.State;
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
    public void RenderImage_AnnotationAppearance_UsesAppearanceColorSpaceResources()
    {
        using PDDocument document = CreateDocument(string.Empty);
        PDPage page = document.GetPage(0);

        PDResources appearanceResources = new();
        COSName colorSpaceName = appearanceResources.Add(PDDeviceRGB.Instance, "Cs");

        PDAppearanceStream appearance = new(new COSStream());
        appearance.SetBBox(new PDRectangle(0, 0, 20, 20));
        appearance.SetResources(appearanceResources);
        WriteStream(appearance.GetCOSObject()!, $"/{colorSpaceName.GetName()} cs\n1 0.82 0.39 sc\n0 0 20 20 re\nf\n");

        PDAppearanceDictionary appearanceDictionary = new();
        appearanceDictionary.SetNormalAppearance(appearance);

        PDAnnotationSquare annotation = new();
        annotation.SetRectangle(new PDRectangle(150, 450, 20, 20));
        annotation.SetAppearance(appearanceDictionary);
        page.SetAnnotations([annotation]);

        using BufferedImage image = new PDFRenderer(document).RenderImage(0, 1f, ImageType.RGB);

        Assert.True(CountYellowPixels(image, 150, 322, 20, 20) > 300);
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
    public void RenderImage_AnnotationAppearance_ClipsToAppearanceBBox()
    {
        using PDDocument document = CreateDocument(string.Empty);
        PDPage page = document.GetPage(0);

        PDAppearanceStream appearance = new(new COSStream());
        appearance.SetBBox(new PDRectangle(0, 0, 10, 10));
        WriteStream(appearance.GetCOSObject()!, "1 0 0 rg\n0 0 20 20 re\nf\n");

        PDAppearanceDictionary appearanceDictionary = new();
        appearanceDictionary.SetNormalAppearance(appearance);

        PDAnnotationSquare annotation = new();
        annotation.SetRectangle(new PDRectangle(120, 420, 20, 20));
        annotation.SetAppearance(appearanceDictionary);
        page.SetAnnotations([annotation]);

        using BufferedImage image = new PDFRenderer(document).RenderImage(0, 1f, ImageType.RGB);

        Assert.True(CountNonWhitePixels(image, 120, 352, 20, 20) > 300);
        Assert.Equal(0, CountNonWhitePixels(image, 140, 332, 20, 40));
        Assert.Equal(0, CountNonWhitePixels(image, 120, 332, 40, 20));
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
    public void RenderImage_VectorAlphaSoftMask_AppliesGroupAlphaAndBounds()
    {
        PDResources resources = new();
        PDSoftMask softMask = CreateVectorSoftMask(
            "Alpha",
            new PDRectangle(100, 300, 100, 40),
            "0 0 0 rg\n100 300 50 40 re\nf\n");
        PDExtendedGraphicsState extGState = new();
        extGState.SetSoftMask(softMask);
        resources.Put(COSName.GetPDFName("GsMask"), extGState);

        using PDDocument document = CreateDocument(
            "/GsMask gs\n1 0 0 rg\n100 300 100 40 re\nf\n",
            resources);
        using BufferedImage image = new PDFRenderer(document).RenderImage(0, 1f, ImageType.RGB);

        AssertRedDominant(image.GetRgb(120, 472), "inside the opaque half of the alpha mask");
        AssertWhitePixel(image.GetRgb(180, 472), "inside the transparent half of the alpha mask");
        AssertWhitePixel(image.GetRgb(90, 472), "outside the soft-mask bounds");
    }

    [Fact]
    public void RenderImage_VectorLuminositySoftMask_AppliesBackdropAndTransferFunction()
    {
        COSDictionary transfer = new();
        transfer.SetInt(COSName.GetPDFName("FunctionType"), 2);
        transfer.SetItem(COSName.GetPDFName("Domain"), COSArray.Of(0f, 1f));
        transfer.SetItem(COSName.GetPDFName("C0"), COSArray.Of(1f));
        transfer.SetItem(COSName.GetPDFName("C1"), COSArray.Of(0f));
        transfer.SetFloat(COSName.GetPDFName("N"), 1f);

        PDResources resources = new();
        PDSoftMask softMask = CreateVectorSoftMask(
            "Luminosity",
            new PDRectangle(100, 300, 100, 40),
            "1 1 1 rg\n100 300 50 40 re\nf\n",
            COSArray.Of(0f, 0f, 0f),
            transfer);
        PDExtendedGraphicsState extGState = new();
        extGState.SetSoftMask(softMask);
        resources.Put(COSName.GetPDFName("GsMask"), extGState);

        using PDDocument document = CreateDocument(
            "/GsMask gs\n0 0 1 rg\n100 300 100 40 re\nf\n",
            resources);
        using BufferedImage image = new PDFRenderer(document).RenderImage(0, 1f, ImageType.RGB);

        AssertWhitePixel(image.GetRgb(120, 472), "where white luminosity is inverted to transparent");
        AssertBlueDominant(image.GetRgb(180, 472), "where the black backdrop is inverted to opaque");
    }

    [Fact]
    public void RenderImage_TransparencyGroup_ClipsContentToTransformedBounds()
    {
        PDTransparencyGroup group = CreateTransparencyGroup(
            new PDRectangle(0, 0, 40, 30),
            "0 0 0 rg\n-100 -100 300 300 re\nf\n");
        group.SetMatrix(new Matrix(1, 0, 0, 1, 120, 320));
        PDResources resources = new();
        resources.Put(COSName.GetPDFName("Group"), group);

        using PDDocument document = CreateDocument("/Group Do\n", resources);
        using BufferedImage image = new PDFRenderer(document).RenderImage(0, 1f, ImageType.RGB);

        Assert.True(CountNonWhitePixels(image, 120, 442, 40, 30) > 1000);
        Assert.Equal(0, CountNonWhitePixels(image, 110, 442, 10, 30));
        Assert.Equal(0, CountNonWhitePixels(image, 160, 442, 10, 30));
    }

    [Fact]
    public void RenderImage_KnockoutTransparencyGroup_ReplacesEarlierGroupObjectInOverlap()
    {
        PDTransparencyGroup group = CreateOverlappingTransparencyGroup(knockout: true);
        group.SetMatrix(new Matrix(1, 0, 0, 1, 100, 300));
        PDResources resources = new();
        resources.Put(COSName.GetPDFName("Group"), group);

        using PDDocument document = CreateDocument("/Group Do\n", resources);
        using BufferedImage image = new PDFRenderer(document).RenderImage(0, 1f, ImageType.RGB);

        (int red, int green, int blue) = GetRgb(image.GetRgb(130, 472));
        Assert.InRange(red, 115, 140);
        Assert.InRange(green, 115, 140);
        Assert.InRange(blue, 240, 255);
    }

    [Fact]
    public void RenderImage_NonKnockoutTransparencyGroup_CompositesOverEarlierGroupObject()
    {
        PDTransparencyGroup group = CreateOverlappingTransparencyGroup(knockout: false);
        group.SetMatrix(new Matrix(1, 0, 0, 1, 100, 300));
        PDResources resources = new();
        resources.Put(COSName.GetPDFName("Group"), group);

        using PDDocument document = CreateDocument("/Group Do\n", resources);
        using BufferedImage image = new PDFRenderer(document).RenderImage(0, 1f, ImageType.RGB);

        (int red, int green, int blue) = GetRgb(image.GetRgb(130, 472));
        Assert.InRange(red, 115, 140);
        Assert.InRange(green, 50, 75);
        Assert.InRange(blue, 175, 205);
    }

    [Fact]
    public void RenderImage_NonIsolatedBlendGroup_UsesPageBackdrop()
    {
        PDTransparencyGroup group = CreateNonIsolatedMultiplyGroup(
            new PDRectangle(0, 0, 40, 40),
            "1 0 0 rg\n0 0 40 40 re\nf\n");
        group.SetMatrix(new Matrix(1, 0, 0, 1, 100, 300));
        PDResources resources = new();
        resources.Put(COSName.GetPDFName("Group"), group);

        using PDDocument document = CreateDocument(
            "0 0 1 rg\n100 300 40 40 re\nf\n/Group Do\n",
            resources);
        using BufferedImage image = new PDFRenderer(document).RenderImage(0, 1f, ImageType.RGB);

        (int red, int green, int blue) = GetRgb(image.GetRgb(120, 472));
        Assert.True(red < 20 && green < 20 && blue < 20,
            $"Expected red multiplied with the blue page backdrop to be black, got RGB({red},{green},{blue}).");
    }

    [Fact]
    public void RenderImage_LuminositySoftMask_NestedNonIsolatedBlendGroupUsesMaskBackdrop()
    {
        PDTransparencyGroup nestedGroup = CreateNonIsolatedMultiplyGroup(
            new PDRectangle(100, 300, 50, 40),
            "1 0 0 rg\n100 300 50 40 re\nf\n");
        PDResources maskResources = new();
        maskResources.Put(COSName.GetPDFName("Nested"), nestedGroup);

        PDTransparencyGroup maskGroup = CreateTransparencyGroup(
            new PDRectangle(100, 300, 100, 40),
            "0 0 1 rg\n100 300 100 40 re\nf\n/Nested Do\n");
        maskGroup.SetResources(maskResources);
        maskGroup.GetGroup()!.GetCOSObject().SetItem(COSName.CS, PDDeviceRGB.Instance.GetCOSObject());

        COSDictionary softMaskDictionary = new();
        softMaskDictionary.SetItem(COSName.GetPDFName("S"), COSName.GetPDFName("Luminosity"));
        softMaskDictionary.SetItem(COSName.GetPDFName("G"), maskGroup.GetCOSObject());
        PDExtendedGraphicsState extGState = new();
        extGState.SetSoftMask(new PDSoftMask(softMaskDictionary));
        PDResources pageResources = new();
        pageResources.Put(COSName.GetPDFName("GsMask"), extGState);

        using PDDocument document = CreateDocument(
            "/GsMask gs\n1 0 0 rg\n100 300 100 40 re\nf\n",
            pageResources);
        using BufferedImage image = new PDFRenderer(document).RenderImage(0, 1f, ImageType.RGB);

        AssertWhitePixel(image.GetRgb(120, 472),
            "where the nested multiply group produces a black luminosity mask");
    }

    [Fact]
    public void RenderImage_DeviceCmykKnockoutGroup_BlendsChildAgainstOriginalBackdrop()
    {
        using PDDocument withEarlierObject = CreateDeviceCmykKnockoutDocument(includeEarlierObject: true);
        using PDDocument withoutEarlierObject = CreateDeviceCmykKnockoutDocument(includeEarlierObject: false);
        using BufferedImage actual = new PDFRenderer(withEarlierObject).RenderImage(0, 1f, ImageType.RGB);
        using BufferedImage expected = new PDFRenderer(withoutEarlierObject).RenderImage(0, 1f, ImageType.RGB);

        (int actualRed, int actualGreen, int actualBlue) = GetRgb(actual.GetRgb(130, 472));
        (int expectedRed, int expectedGreen, int expectedBlue) = GetRgb(expected.GetRgb(130, 472));
        Assert.InRange(Math.Abs(actualRed - expectedRed), 0, 2);
        Assert.InRange(Math.Abs(actualGreen - expectedGreen), 0, 2);
        Assert.InRange(Math.Abs(actualBlue - expectedBlue), 0, 2);
    }

    [Fact]
    public void RenderImage_ZeroOpacityKnockoutChild_ClearsEarlierObjectUsingGeometryShape()
    {
        PDResources groupResources = new();
        PDExtendedGraphicsState halfAlpha = new();
        halfAlpha.SetNonStrokingAlphaConstant(0.5f);
        groupResources.Put(COSName.GetPDFName("Half"), halfAlpha);
        PDExtendedGraphicsState zeroAlpha = new();
        zeroAlpha.SetNonStrokingAlphaConstant(0f);
        groupResources.Put(COSName.GetPDFName("Zero"), zeroAlpha);

        PDTransparencyGroup group = CreateTransparencyGroup(
            new PDRectangle(0, 0, 60, 40),
            "/Half gs\n1 0 0 rg\n0 0 40 40 re\nf\n/Zero gs\n0 0 1 rg\n20 0 40 40 re\nf\n");
        group.SetResources(groupResources);
        group.GetGroup()!.GetCOSObject().SetBoolean(COSName.GetPDFName("I"), true);
        group.GetGroup()!.GetCOSObject().SetBoolean(COSName.K, true);
        group.SetMatrix(new Matrix(1, 0, 0, 1, 100, 300));
        PDResources pageResources = new();
        pageResources.Put(COSName.GetPDFName("Group"), group);

        using PDDocument document = CreateDocument("/Group Do\n", pageResources);
        using BufferedImage image = new PDFRenderer(document).RenderImage(0, 1f, ImageType.RGB);

        AssertRedDominant(image.GetRgb(110, 472), "outside the zero-opacity child's shape");
        AssertWhitePixel(image.GetRgb(130, 472), "inside the zero-opacity child's knockout shape");
    }

    [Fact]
    public void RenderImage_KnockoutImageShape_PreservesIntrinsicSoftMaskAlpha()
    {
        using PDDocument document = new();
        PDPage page = new();
        document.AddPage(page);

        PDImageXObject image = LosslessFactory.CreateFromRawData(
            document,
            [0, 0, 0, 0, 0, 0],
            2,
            1,
            8,
            3);
        PDImageXObject softMask = LosslessFactory.CreateFromRawData(document, [255, 0], 2, 1, 8, 1);
        image.GetCOSObject()!.SetItem(COSName.SMASK, softMask.GetCOSObject());

        PDResources groupResources = new();
        groupResources.Put(COSName.GetPDFName("Image"), image);
        PDTransparencyGroup group = new(new PDStream(document));
        group.SetBBox(new PDRectangle(0, 0, 60, 40));
        group.SetResources(groupResources);
        PDTransparencyGroupAttributes attributes = new();
        attributes.GetCOSObject().SetBoolean(COSName.GetPDFName("I"), true);
        attributes.GetCOSObject().SetBoolean(COSName.K, true);
        group.SetGroup(attributes);
        group.SetMatrix(new Matrix(1, 0, 0, 1, 100, 300));
        WriteStream(
            (COSStream)group.GetCOSObject()!,
            "1 0 0 rg\n0 0 60 40 re\nf\nq\n40 0 0 40 20 0 cm\n/Image Do\nQ\n");

        PDResources pageResources = new();
        pageResources.Put(COSName.GetPDFName("Group"), group);
        page.SetResources(pageResources);
        WriteStream((COSDictionary)page.GetCOSObject(), "/Group Do\n");

        using BufferedImage rendered = new PDFRenderer(document).RenderImage(0, 1f, ImageType.RGB);

        AssertRedDominant(rendered.GetRgb(110, 472), "before the image");
        (int opaqueRed, int opaqueGreen, int opaqueBlue) = GetRgb(rendered.GetRgb(130, 472));
        Assert.True(opaqueRed < 20 && opaqueGreen < 20 && opaqueBlue < 20,
            $"Expected the opaque image pixel to replace red, got RGB({opaqueRed},{opaqueGreen},{opaqueBlue}).");
        AssertRedDominant(rendered.GetRgb(150, 472), "under the transparent image pixel");
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
    public void RenderImage_PatternColorSpace_RepeatsColoredTilingCell()
    {
        PDResources resources = new();
        PDTilingPattern pattern = new();
        pattern.SetPaintType(PDTilingPattern.PAINT_COLORED);
        pattern.SetTilingType(PDTilingPattern.TILING_CONSTANT_SPACING);
        pattern.SetBBox(new PDRectangle(0, 0, 8, 8));
        pattern.SetXStep(8);
        pattern.SetYStep(8);
        WriteStream((COSStream)pattern.GetCOSObject(), "0 0 0 rg\n0 0 4 8 re\nf\n");
        COSName patternName = resources.Add(pattern);

        using PDDocument document = CreateDocument($"/Pattern cs\n/{patternName.GetName()} scn\n100 300 32 16 re\nf\n", resources);

        using BufferedImage image = new PDFRenderer(document).RenderImage(0, 1f, ImageType.RGB);

        int nonWhitePixels = CountNonWhitePixels(image, 100, 476, 32, 16);
        Assert.InRange(nonWhitePixels, 120, 420);
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

    [Theory]
    [InlineData("JBIG2Image.pdf")]
    [InlineData("JPXTestCMYK.pdf")]
    [InlineData("png_demo.pdf")]
    [InlineData("ccitt4-cib-test.pdf")]
    public void RenderImage_ImageFilterFixtures_DoesNotThrowWhenFixtureIsAvailable(string fixtureName)
    {
        string? fixturePath = FindApacheImageIoFixture(fixtureName);
        if (fixturePath is null)
        {
            return;
        }

        using PDDocument document = Loader.LoadPDF(fixturePath);
        using BufferedImage image = new PDFRenderer(document).RenderImageWithDPI(0, 36, ImageType.RGB);

        Assert.True(image.Width > 0);
        Assert.True(image.Height > 0);
    }

    [Theory]
    [InlineData("JPXTestCMYK.pdf")]
    [InlineData("JPXTestGrey.pdf")]
    [InlineData("JPXTestRGB.pdf")]
    public void RenderImage_JpxImageFilterFixtures_DrawVisiblePixelsWhenFixtureIsAvailable(string fixtureName)
    {
        string? fixturePath = FindApacheImageIoFixture(fixtureName);
        if (fixturePath is null)
        {
            return;
        }

        using PDDocument document = Loader.LoadPDF(fixturePath);
        using BufferedImage image = new PDFRenderer(document).RenderImageWithDPI(0, 36, ImageType.RGB);

        Assert.True(CountNonWhitePixels(image, 0, 0, image.Width, image.Height) > 100);
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

    private static PDSoftMask CreateVectorSoftMask(
        string subtype,
        PDRectangle bbox,
        string contentStream,
        COSArray? backdropColor = null,
        COSBase? transferFunction = null)
    {
        PDTransparencyGroup group = CreateTransparencyGroup(bbox, contentStream);
        group.GetGroup()!.GetCOSObject().SetItem(COSName.CS, PDDeviceRGB.Instance.GetCOSObject());

        COSDictionary dictionary = new();
        dictionary.SetItem(COSName.GetPDFName("S"), COSName.GetPDFName(subtype));
        dictionary.SetItem(COSName.GetPDFName("G"), group.GetCOSObject());
        dictionary.SetItem(COSName.GetPDFName("BC"), backdropColor);
        dictionary.SetItem(COSName.GetPDFName("TR"), transferFunction);
        return new PDSoftMask(dictionary);
    }

    private static PDTransparencyGroup CreateTransparencyGroup(PDRectangle bbox, string contentStream)
    {
        COSStream stream = new();
        PDTransparencyGroup group = new(stream);
        group.SetBBox(bbox);
        group.SetGroup(new PDTransparencyGroupAttributes());
        WriteStream(stream, contentStream);
        return group;
    }

    private static PDTransparencyGroup CreateOverlappingTransparencyGroup(bool knockout)
    {
        PDResources resources = new();
        PDExtendedGraphicsState halfAlpha = new();
        halfAlpha.SetNonStrokingAlphaConstant(0.5f);
        resources.Put(COSName.GetPDFName("Half"), halfAlpha);

        PDTransparencyGroup group = CreateTransparencyGroup(
            new PDRectangle(0, 0, 60, 40),
            "/Half gs\n1 0 0 rg\n0 0 40 40 re\nf\n0 0 1 rg\n20 0 40 40 re\nf\n");
        group.SetResources(resources);
        group.GetGroup()!.GetCOSObject().SetBoolean(COSName.GetPDFName("I"), true);
        group.GetGroup()!.GetCOSObject().SetBoolean(COSName.K, knockout);
        return group;
    }

    private static PDTransparencyGroup CreateNonIsolatedMultiplyGroup(
        PDRectangle bbox,
        string contentStream)
    {
        PDResources resources = new();
        PDExtendedGraphicsState multiply = new();
        multiply.SetBlendMode(BlendMode.MULTIPLY);
        resources.Put(COSName.GetPDFName("Multiply"), multiply);

        PDTransparencyGroup group = CreateTransparencyGroup(
            bbox,
            "/Multiply gs\n" + contentStream);
        group.SetResources(resources);
        group.GetGroup()!.GetCOSObject().SetBoolean(COSName.GetPDFName("I"), false);
        return group;
    }

    private static PDDocument CreateDeviceCmykKnockoutDocument(bool includeEarlierObject)
    {
        PDResources groupResources = new();
        PDExtendedGraphicsState halfNormal = new();
        halfNormal.SetNonStrokingAlphaConstant(0.5f);
        groupResources.Put(COSName.GetPDFName("HalfNormal"), halfNormal);
        PDExtendedGraphicsState halfMultiply = new();
        halfMultiply.SetNonStrokingAlphaConstant(0.5f);
        halfMultiply.SetBlendMode(BlendMode.MULTIPLY);
        groupResources.Put(COSName.GetPDFName("HalfMultiply"), halfMultiply);

        string firstObject = includeEarlierObject
            ? "/HalfNormal gs\n0 1 1 0 k\n0 0 40 40 re\nf\n"
            : string.Empty;
        PDTransparencyGroup group = CreateTransparencyGroup(
            new PDRectangle(0, 0, 60, 40),
            firstObject + "/HalfMultiply gs\n1 1 0 0 k\n20 0 40 40 re\nf\n");
        group.SetResources(groupResources);
        group.GetGroup()!.GetCOSObject().SetBoolean(COSName.GetPDFName("I"), false);
        group.GetGroup()!.GetCOSObject().SetBoolean(COSName.K, true);
        group.GetGroup()!.GetCOSObject().SetItem(COSName.CS, PDDeviceCMYK.Instance.GetCOSObject());
        group.SetMatrix(new Matrix(1, 0, 0, 1, 100, 300));

        PDResources pageResources = new();
        pageResources.Put(COSName.GetPDFName("Group"), group);
        return CreateDocument("0.8 g\n100 300 60 40 re\nf\n/Group Do\n", pageResources);
    }

    private static (int Red, int Green, int Blue) GetRgb(int argb)
    {
        return ((argb >> 16) & 0xFF, (argb >> 8) & 0xFF, argb & 0xFF);
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

    private static int CountYellowPixels(BufferedImage image, int x, int y, int width, int height)
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
                if (r > 220 && g > 160 && g < 240 && b > 60 && b < 150)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static void AssertWhitePixel(int argb, string location)
    {
        int red = (argb >> 16) & 0xFF;
        int green = (argb >> 8) & 0xFF;
        int blue = argb & 0xFF;
        Assert.True(red > 245 && green > 245 && blue > 245,
            $"Expected white {location}, got RGB({red},{green},{blue}).");
    }

    private static void AssertRedDominant(int argb, string location)
    {
        int red = (argb >> 16) & 0xFF;
        int green = (argb >> 8) & 0xFF;
        int blue = argb & 0xFF;
        Assert.True(red > 200 && red > green * 2 && red > blue * 2,
            $"Expected red {location}, got RGB({red},{green},{blue}).");
    }

    private static void AssertBlueDominant(int argb, string location)
    {
        int red = (argb >> 16) & 0xFF;
        int green = (argb >> 8) & 0xFF;
        int blue = argb & 0xFF;
        Assert.True(blue > 200 && blue > red * 2 && blue > green * 2,
            $"Expected blue {location}, got RGB({red},{green},{blue}).");
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

    private static string? FindApacheImageIoFixture(string fixtureName)
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
            "tools",
            "src",
            "test",
            "resources",
            "input",
            "ImageIOUtil",
            fixtureName));

        return File.Exists(candidate) ? candidate : null;
    }
}
