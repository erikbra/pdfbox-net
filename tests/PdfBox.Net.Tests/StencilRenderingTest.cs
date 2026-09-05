using System.Text;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.Form;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.PDModel.Graphics.Patterns;
using PdfBox.Net.PDModel.Graphics.State;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Rendering;
using PdfBox.Net.Util;

namespace PdfBox.Net.Tests;

public sealed class StencilRenderingTest
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void StencilUsesCurrentColorAndDecodeForInlineAndXObject(bool inline, bool inverse)
    {
        using PDDocument document = CreateDocument();
        PDPage page = document.GetPage(0);
        byte data = inverse ? (byte)0x80 : (byte)0x40;
        string decode = inverse ? "1 0" : "0 1";
        if (inline)
        {
            using Stream stream = CreatePageOutput(page);
            stream.Write(Encoding.ASCII.GetBytes(
                $"1 0 0 rg\n40 0 0 20 20 40 cm\nBI /W 2 /H 1 /BPC 1 /IM true /D [{decode}] ID "));
            stream.WriteByte(data);
            stream.Write(Encoding.ASCII.GetBytes("\nEI\n"));
        }
        else
        {
            PDImageXObject stencil = CreateStencil(document, 2, 1, [data]);
            stencil.GetCOSObject()!.SetItem(COSName.DECODE, inverse ? COSArray.Of(1, 0) : COSArray.Of(0, 1));
            page.GetResources()!.Put(COSName.GetPDFName("Mask"), stencil);
            WritePage(page, "1 0 0 rg\n40 0 0 20 20 40 cm\n/Mask Do\n");
        }

        using BufferedImage rendered = new PDFRenderer(document).RenderImage(0);
        Assert.Equal(0xFF0000, rendered.GetRgb(30, 50) & 0xFFFFFF);
        Assert.Equal(0xFFFFFF, rendered.GetRgb(50, 50) & 0xFFFFFF);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TransparentStencilLeavesNoSubpixelPaint(bool interpolate)
    {
        using PDDocument document = CreateDocument();
        PDPage page = document.GetPage(0);
        PDImageXObject stencil = CreateStencil(document, 1, 1, [0xFF]);
        stencil.GetCOSObject()!.SetBoolean(COSName.INTERPOLATE, interpolate);
        page.GetResources()!.Put(COSName.GetPDFName("Mask"), stencil);
        WritePage(page, "1 0 0 rg\n1 0 0 1 20 40 cm\n/Mask Do\n");

        using BufferedImage rendered = new PDFRenderer(document).RenderImage(0, 0.5f);
        for (int y = 0; y < rendered.Height; y++)
        {
            for (int x = 0; x < rendered.Width; x++)
            {
                Assert.Equal(0xFFFFFF, rendered.GetRgb(x, y) & 0xFFFFFF);
            }
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MixedStencilLeavesNoPaintAlongTransparentOuterBorder(bool interpolate)
    {
        using PDDocument document = CreateDocument();
        PDPage page = document.GetPage(0);
        // The central sample paints; its surrounding eight samples are transparent.
        PDImageXObject stencil = CreateStencil(document, 3, 3, [0xE0, 0xA0, 0xE0]);
        stencil.GetCOSObject()!.SetBoolean(COSName.INTERPOLATE, interpolate);
        page.GetResources()!.Put(COSName.GetPDFName("Mask"), stencil);
        WritePage(page, "1 0 0 rg\n14 0 0 14 20.25 40.25 cm\n/Mask Do\n");

        using BufferedImage rendered = new PDFRenderer(document).RenderImage(0, 0.5f);
        int center = rendered.GetRgb(13, 26);
        Assert.Equal(255, (center >> 16) & 0xFF);
        Assert.InRange((center >> 8) & 0xFF, 0, 127);
        Assert.InRange(center & 0xFF, 0, 127);
        for (int y = 21; y <= 31; y++)
        {
            for (int x = 9; x <= 18; x++)
            {
                // These pixels are outside the central sample's interpolation footprint.
                if (x is <= 10 or >= 17 || y is <= 22 or >= 29)
                {
                    Assert.Equal(0xFFFFFF, rendered.GetRgb(x, y) & 0xFFFFFF);
                }
            }
        }
    }

    [Theory]
    [InlineData(1f, false, false)]
    [InlineData(2f, false, false)]
    [InlineData(1f, true, false)]
    [InlineData(2f, true, false)]
    [InlineData(1f, false, true)]
    [InlineData(2f, false, true)]
    [InlineData(1f, true, true)]
    [InlineData(2f, true, true)]
    public void PatternStencilPreservesGapsAndSoftMaskAtDeviceScale(float scale, bool softMask, bool reflected)
    {
        using PDDocument document = CreateDocument();
        PDPage page = document.GetPage(0);
        PDResources resources = page.GetResources()!;
        PDTilingPattern pattern = new();
        pattern.SetPaintType(PDTilingPattern.PAINT_COLORED);
        pattern.SetTilingType(PDTilingPattern.TILING_CONSTANT_SPACING);
        pattern.SetBBox(new PDRectangle(0, 0, 16, 16));
        pattern.SetXStep(16);
        pattern.SetYStep(16);
        if (reflected)
        {
            pattern.SetMatrix(new Matrix(1, 0, 0, -1, 0, 100).CreateAffineTransform());
        }
        WriteStream((COSStream)pattern.GetCOSObject(), "1 0 0 rg\n0 0 8 8 re\nf\n");
        COSName patternName = resources.Add(pattern);
        resources.Put(COSName.GetPDFName("Mask"), CreateStencil(document, 1, 1, [0]));

        if (softMask)
        {
            PDTransparencyGroup group = new(new COSStream());
            group.SetBBox(new PDRectangle(0, 0, 100, 100));
            group.SetGroup(new PDTransparencyGroupAttributes());
            group.GetGroup()!.GetCOSObject().SetItem(COSName.CS, PDDeviceRGB.Instance.GetCOSObject());
            WriteStream((COSStream)group.GetCOSObject()!, "0 0 0 rg\n0 0 48 100 re\nf\n");
            COSDictionary mask = new();
            mask.SetItem(COSName.S, COSName.GetPDFName("Alpha"));
            mask.SetItem(COSName.GetPDFName("G"), group.GetCOSObject());
            PDExtendedGraphicsState state = new();
            state.SetSoftMask(new PDSoftMask(mask));
            resources.Put(COSName.GetPDFName("SoftMask"), state);
        }

        WritePage(page,
            (softMask ? "/SoftMask gs\n" : "") +
            $"/Pattern cs\n/{patternName.GetName()} scn\nq\n64 0 0 32 16 32 cm\n/Mask Do\nQ\n");
        using BufferedImage rendered = new PDFRenderer(document).RenderImage(0, scale);
        Assert.Equal(0xFF0000, Pixel(rendered, 20, 50, scale));
        Assert.Equal(0xFFFFFF, Pixel(rendered, 28, 50, scale));
        Assert.Equal(softMask ? 0xFFFFFF : 0xFF0000, Pixel(rendered, 52, 50, scale));
    }

    private static int Pixel(BufferedImage image, int x, int y, float scale) =>
        image.GetRgb((int)(x * scale), (int)(y * scale)) & 0xFFFFFF;

    [Fact]
    public void GraphicsStateCloneRetainsColoredAndUncoloredPatterns()
    {
        PDGraphicsState state = new();
        PDColor stroke = new([0.25f, 0.5f, 0.75f], COSName.GetPDFName("Stroke"),
            new PDPattern(new PDResources(), PDDeviceRGB.Instance));
        PDColor fill = new(COSName.GetPDFName("Fill"), new PDPattern(new PDResources()));
        state.SetStrokingColor(stroke);
        state.SetNonStrokingColor(fill);

        PDGraphicsState clone = state.Clone();
        Assert.Equal(stroke.GetPatternName(), clone.GetStrokingColor().GetPatternName());
        Assert.Equal(stroke.GetComponents(), clone.GetStrokingColor().GetComponents());
        Assert.Equal(fill.GetPatternName(), clone.GetNonStrokingColor().GetPatternName());
        clone.SetNonStrokingColor(PDDeviceGray.Instance.GetInitialColor());
        Assert.Equal(fill.GetPatternName(), state.GetNonStrokingColor().GetPatternName());
    }

    private static PDDocument CreateDocument()
    {
        PDDocument document = new();
        PDPage page = new(new PDRectangle(100, 100));
        page.SetResources(new PDResources());
        document.AddPage(page);
        return document;
    }

    private static PDImageXObject CreateStencil(PDDocument document, int width, int height, byte[] data)
    {
        PDStream stream = new(document);
        COSStream dictionary = stream.GetCOSObject();
        dictionary.SetInt(COSName.WIDTH, width);
        dictionary.SetInt(COSName.HEIGHT, height);
        dictionary.SetInt(COSName.BITS_PER_COMPONENT, 1);
        dictionary.SetBoolean(COSName.IMAGE_MASK, true);
        using (Stream output = dictionary.CreateRawOutputStream())
        {
            output.Write(data);
        }
        return new PDImageXObject(stream, null);
    }

    private static void WritePage(PDPage page, string content)
    {
        using Stream output = CreatePageOutput(page);
        output.Write(Encoding.ASCII.GetBytes(content));
    }

    private static Stream CreatePageOutput(PDPage page)
    {
        COSStream stream = new();
        ((COSDictionary)page.GetCOSObject()).SetItem(COSName.CONTENTS, stream);
        return stream.CreateOutputStream();
    }

    private static void WriteStream(COSStream stream, string content)
    {
        using Stream output = stream.CreateOutputStream();
        output.Write(Encoding.ASCII.GetBytes(content));
    }
}
