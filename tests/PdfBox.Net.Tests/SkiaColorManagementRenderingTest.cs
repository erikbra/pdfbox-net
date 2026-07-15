/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PORT_MODE: native-test
 */

using ImageMagick;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.Rendering;

namespace PdfBox.Net.Tests;

public class SkiaColorManagementRenderingTest
{
    [Fact]
    public void RenderImage_OutputIntentMakesDeviceCmykVectorAndImageConverge()
    {
        const float cyan = 0.2f;
        const float magenta = 0.6f;
        const float yellow = 0.35f;
        const float black = 0.1f;

        using PDDocument document = new();
        using MemoryStream profile = new(ColorProfiles.CoatedFOGRA39.ToByteArray());
        document.GetDocumentCatalog().AddOutputIntent(new PDOutputIntent(document, profile));

        PDPage page = new(new PDRectangle(20, 10));
        document.AddPage(page);
        PDImageXObject image = LosslessFactory.CreateFromRawData(
            document,
            [
                ToSample(cyan),
                ToSample(magenta),
                ToSample(yellow),
                ToSample(black),
            ],
            1,
            1,
            8,
            4);

        using (PDPageContentStream content = new(document, page))
        {
            content.SetNonStrokingColor(cyan, magenta, yellow, black);
            content.AddRect(0, 0, 10, 10);
            content.Fill();
            content.DrawImage(image, 10, 0, 10, 10);
        }

        PDColorManagementContext context = PDColorManagementContext.Create(document)!;
        PDColorSpace managedCmyk = context.ResolveDeviceColorSpace(PDDeviceCMYK.Instance);
        int expected = ToPackedRgb(managedCmyk.ToRGB([cyan, magenta, yellow, black]));
        int naive = ToPackedRgb(PDDeviceCMYK.Instance.ToRGB([cyan, magenta, yellow, black]));

        using BufferedImage rendered = new PDFRenderer(document).RenderImage(0, 1f, ImageType.RGB);
        int vectorPixel = rendered.GetRgb(5, 5) & 0xFFFFFF;
        int imagePixel = rendered.GetRgb(15, 5) & 0xFFFFFF;

        Assert.NotEqual(naive, vectorPixel);
        AssertRgbClose(expected, vectorPixel, 1);
        AssertRgbClose(vectorPixel, imagePixel, 1);
    }

    [Fact]
    public void RenderImage_OutputIntentMakesIndexedIccVectorAndImageConverge()
    {
        using PDDocument document = new();
        using MemoryStream outputProfile = new(ColorProfiles.CoatedFOGRA39.ToByteArray());
        document.GetDocumentCatalog().AddOutputIntent(new PDOutputIntent(document, outputProfile));

        PDICCBased adobeRgb = CreateIccColorSpace(ColorProfiles.AdobeRGB1998.ToByteArray(), 3);
        PDIndexed indexed = PDIndexed.Create(adobeRgb, 0, [114, 247, 13]);
        PDImageXObject image = LosslessFactory.CreateFromRawData(document, [0], 1, 1, 8, 1);
        image.GetCOSObject()!.SetItem(COSName.COLORSPACE, indexed.GetCOSObject());

        PDPage page = new(new PDRectangle(20, 10));
        document.AddPage(page);
        using (PDPageContentStream content = new(document, page))
        {
            content.SetNonStrokingColor(new PDColor([0f], indexed));
            content.AddRect(0, 0, 10, 10);
            content.Fill();
            content.DrawImage(image, 10, 0, 10, 10);
        }

        PDColorManagementContext context = PDColorManagementContext.Create(document)!;
        int direct = ToPackedRgb(indexed.ToRGB([0]));
        int expected = ToPackedRgb(context.ResolveColorSpace(indexed).ToRGB([0]));
        using BufferedImage rendered = new PDFRenderer(document).RenderImage(0, 1f, ImageType.RGB);
        int vectorPixel = rendered.GetRgb(5, 5) & 0xFFFFFF;
        int imagePixel = rendered.GetRgb(15, 5) & 0xFFFFFF;

        Assert.NotEqual(direct, imagePixel);
        AssertRgbClose(vectorPixel, imagePixel, 1);
        AssertRgbClose(expected, imagePixel, 1);
    }

    private static byte ToSample(float component) =>
        (byte)MathF.Round(component * byte.MaxValue);

    private static PDICCBased CreateIccColorSpace(byte[] profile, int components)
    {
        var profileStream = new COSStream();
        profileStream.SetInt(COSName.GetPDFName("N"), components);
        using (Stream output = profileStream.CreateOutputStream())
        {
            output.Write(profile);
        }

        var array = new COSArray();
        array.Add(COSName.GetPDFName("ICCBased"));
        array.Add(profileStream);
        return PDICCBased.Create(array, null);
    }

    private static int ToPackedRgb(float[] rgb) =>
        ((int)MathF.Round(rgb[0] * byte.MaxValue) << 16) |
        ((int)MathF.Round(rgb[1] * byte.MaxValue) << 8) |
        (int)MathF.Round(rgb[2] * byte.MaxValue);

    private static void AssertRgbClose(int expected, int actual, int tolerance)
    {
        Assert.InRange((actual >> 16) & 0xFF, ((expected >> 16) & 0xFF) - tolerance, ((expected >> 16) & 0xFF) + tolerance);
        Assert.InRange((actual >> 8) & 0xFF, ((expected >> 8) & 0xFF) - tolerance, ((expected >> 8) & 0xFF) + tolerance);
        Assert.InRange(actual & 0xFF, (expected & 0xFF) - tolerance, (expected & 0xFF) + tolerance);
    }
}
