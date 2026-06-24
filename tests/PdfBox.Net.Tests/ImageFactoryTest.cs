using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Graphics.Image;
using SkiaSharp;

namespace PdfBox.Net.Tests;

public class ImageFactoryTest
{
    private static string ImageFixture(string name) =>
        Path.Combine("Fixtures", "Images", name);

    // ─── JPEGFactory ───────────────────────────────────────────────────────────

    [Fact]
    public void JPEGFactory_CreateFromStream_GrayscaleReturnsCorrectDimensions()
    {
        using PDDocument doc = new();
        using FileStream fs = File.OpenRead(ImageFixture("test-2x1-gray.jpg"));
        PDImageXObject img = JPEGFactory.CreateFromStream(doc, fs);

        Assert.Equal(2, img.GetWidth());
        Assert.Equal(1, img.GetHeight());
        Assert.Equal(8, img.GetBitsPerComponent());
    }

    [Fact]
    public void JPEGFactory_CreateFromStream_GrayscaleHasDCTDecodeFilter()
    {
        using PDDocument doc = new();
        using FileStream fs = File.OpenRead(ImageFixture("test-2x1-gray.jpg"));
        PDImageXObject img = JPEGFactory.CreateFromStream(doc, fs);

        COSStream? cosStream = img.GetStream()?.GetCOSObject();
        Assert.NotNull(cosStream);

        COSBase? filter = cosStream.GetItem(COSName.FILTER);
        Assert.Equal(COSName.DCT_DECODE, filter);
    }

    [Fact]
    public void JPEGFactory_CreateFromStream_GrayscaleHasDeviceGrayColorSpace()
    {
        using PDDocument doc = new();
        using FileStream fs = File.OpenRead(ImageFixture("test-2x1-gray.jpg"));
        PDImageXObject img = JPEGFactory.CreateFromStream(doc, fs);

        string colorSpaceName = img.GetColorSpace().GetName();
        Assert.Equal("DeviceGray", colorSpaceName);
    }

    [Fact]
    public void JPEGFactory_CreateFromStream_RgbReturnsCorrectColorSpace()
    {
        using PDDocument doc = new();
        using FileStream fs = File.OpenRead(ImageFixture("test-2x1-rgb.jpg"));
        PDImageXObject img = JPEGFactory.CreateFromStream(doc, fs);

        Assert.Equal(2, img.GetWidth());
        Assert.Equal(1, img.GetHeight());
        Assert.Equal("DeviceRGB", img.GetColorSpace().GetName());
    }

    [Fact]
    public void JPEGFactory_CreateFromFile_ReturnsCorrectDimensions()
    {
        using PDDocument doc = new();
        PDImageXObject img = JPEGFactory.CreateFromFile(doc, ImageFixture("test-2x1-gray.jpg"));

        Assert.Equal(2, img.GetWidth());
        Assert.Equal(1, img.GetHeight());
    }

    [Fact]
    public void JPEGFactory_CreateFromStream_StreamDataRoundtrips()
    {
        using PDDocument doc = new();
        byte[] originalBytes = File.ReadAllBytes(ImageFixture("test-2x1-gray.jpg"));
        using MemoryStream ms = new(originalBytes);
        PDImageXObject img = JPEGFactory.CreateFromStream(doc, ms);

        // The raw stream data should be the original JPEG bytes (no re-encoding).
        byte[] stored = img.GetStream()!.GetCOSObject().CreateRawInputStream().ToByteArray();
        Assert.Equal(originalBytes, stored);
    }

    // ─── LosslessFactory ───────────────────────────────────────────────────────

    [Fact]
    public void LosslessFactory_CreateFromImage_ReturnsCorrectDimensions()
    {
        using PDDocument doc = new();
        using SKBitmap bmp = new SKBitmap(4, 3);

        PDImageXObject img = LosslessFactory.CreateFromImage(doc, bmp);

        Assert.Equal(4, img.GetWidth());
        Assert.Equal(3, img.GetHeight());
    }

    [Fact]
    public void LosslessFactory_CreateFromImage_HasFlateDecodeFilter()
    {
        using PDDocument doc = new();
        using SKBitmap bmp = new SKBitmap(2, 2);

        PDImageXObject img = LosslessFactory.CreateFromImage(doc, bmp);

        COSStream? cosStream = img.GetStream()?.GetCOSObject();
        Assert.NotNull(cosStream);
        COSBase? filter = cosStream.GetItem(COSName.FILTER);
        Assert.Equal(COSName.FLATE_DECODE, filter);
    }

    [Fact]
    public void LosslessFactory_CreateFromImage_HasDeviceRGBColorSpace()
    {
        using PDDocument doc = new();
        using SKBitmap bmp = new SKBitmap(1, 1);

        PDImageXObject img = LosslessFactory.CreateFromImage(doc, bmp);

        Assert.Equal("DeviceRGB", img.GetColorSpace().GetName());
        Assert.Equal(8, img.GetBitsPerComponent());
    }

    [Fact]
    public void LosslessFactory_CreateFromImage_PixelDataRoundtrips()
    {
        using PDDocument doc = new();
        using SKBitmap bmp = new SKBitmap(2, 1);
        bmp.SetPixel(0, 0, new SKColor(255, 0, 0));    // red
        bmp.SetPixel(1, 0, new SKColor(0, 255, 0));    // green

        PDImageXObject img = LosslessFactory.CreateFromImage(doc, bmp);

        // GetImageData decodes through FlateDecode
        byte[] decoded = img.GetImageData();
        // Expect: R=255 G=0 B=0  R=0 G=255 B=0
        Assert.Equal(new byte[] { 255, 0, 0, 0, 255, 0 }, decoded);
    }

    [Fact]
    public void LosslessFactory_CreateFromRawData_RgbDataRoundtrips()
    {
        using PDDocument doc = new();
        byte[] rgb = [255, 0, 0, 0, 255, 0];

        PDImageXObject img = LosslessFactory.CreateFromRawData(doc, rgb, 2, 1, 8, 3);

        Assert.Equal(2, img.GetWidth());
        Assert.Equal(1, img.GetHeight());
        Assert.Equal("DeviceRGB", img.GetColorSpace().GetName());
        Assert.Equal(rgb, img.GetImageData());
    }

    [Fact]
    public void CustomFactory_CreateFromRaw_DelegatesToLosslessRawData()
    {
        using PDDocument doc = new();
        byte[] gray = [0, 255];

        PDImageXObject img = CustomFactory.CreateFromRaw(doc, gray, 2, 1, 8, 1);

        Assert.Equal("DeviceGray", img.GetColorSpace().GetName());
        Assert.Equal(gray, img.GetImageData());
    }

    [Fact]
    public void PNGConverter_Convert_ReturnsLosslessImage()
    {
        using PDDocument doc = new();
        using FileStream png = File.OpenRead(ImageFixture("test-2x1.png"));

        PDImageXObject? img = PNGConverter.Convert(doc, png);

        Assert.NotNull(img);
        Assert.Equal(2, img.GetWidth());
        Assert.Equal(1, img.GetHeight());
        Assert.Equal(COSName.FLATE_DECODE, img.GetStream()?.GetCOSObject().GetItem(COSName.FILTER));
    }

    [Fact]
    public void PNGConverter_Convert_ReturnsNullForInvalidPng()
    {
        using PDDocument doc = new();
        using MemoryStream invalid = new([1, 2, 3, 4]);

        Assert.Null(PNGConverter.Convert(doc, invalid));
    }

    [Fact]
    public void SampledImageReader_GetRGBImage_ConvertsCmykSamples()
    {
        using PDDocument doc = new();
        byte[] cmyk = [0, 255, 255, 0];
        PDImageXObject img = LosslessFactory.CreateFromRawData(doc, cmyk, 1, 1, 8, 4);

        byte[] rgb = SampledImageReader.GetRGBImage(img);

        Assert.Equal([255, 0, 0], rgb);
    }

    [Fact]
    public void SampledImageReader_GetRGBImage_UnpacksTwoBitGraySamples()
    {
        using PDDocument doc = new();
        PDImageXObject img = LosslessFactory.CreateFromRawData(doc, [0x1B], 4, 1, 2, 1);

        byte[] rgb = SampledImageReader.GetRGBImage(img);

        Assert.Equal(
            [
                0, 0, 0,
                85, 85, 85,
                170, 170, 170,
                255, 255, 255
            ],
            rgb);
    }

    [Fact]
    public void SampledImageReader_GetRGBImage_UnpacksFourBitGraySamples()
    {
        using PDDocument doc = new();
        PDImageXObject img = LosslessFactory.CreateFromRawData(doc, [0x0F], 2, 1, 4, 1);

        byte[] rgb = SampledImageReader.GetRGBImage(img);

        Assert.Equal([0, 0, 0, 255, 255, 255], rgb);
    }

    // ─── CCITTFactory ─────────────────────────────────────────────────────────

    [Fact]
    public void CCITTFactory_CreateFromFile_CreatesCcittImage()
    {
        using PDDocument doc = new();

        PDImageXObject img = CCITTFactory.CreateFromFile(doc, ImageFixture("ccittg4.tif"));

        Assert.Equal(344, img.GetWidth());
        Assert.Equal(287, img.GetHeight());
        Assert.Equal(1, img.GetBitsPerComponent());
        Assert.Equal("DeviceGray", img.GetColorSpace().GetName());
        Assert.Equal(COSName.CCITTFAX_DECODE, img.GetStream()?.GetCOSObject().GetItem(COSName.FILTER));
        Assert.Equal(((344 + 7) / 8) * 287, img.GetImageData().Length);
    }

    // ─── PDImageXObject.CreateFromFile ─────────────────────────────────────────

    [Fact]
    public void CreateFromFile_Jpeg_DispatchesToJPEGFactory()
    {
        using PDDocument doc = new();
        PDImageXObject img = PDImageXObject.CreateFromFile(
            ImageFixture("test-2x1-gray.jpg"), doc);

        Assert.Equal(2, img.GetWidth());
        Assert.Equal(1, img.GetHeight());
        // JPEG images use DCTDecode
        COSBase? filter = img.GetStream()?.GetCOSObject().GetItem(COSName.FILTER);
        Assert.Equal(COSName.DCT_DECODE, filter);
    }

    [Fact]
    public void CreateFromFile_Png_DispatchesToLosslessFactory()
    {
        using PDDocument doc = new();
        PDImageXObject img = PDImageXObject.CreateFromFile(
            ImageFixture("test-2x1.png"), doc);

        Assert.Equal(2, img.GetWidth());
        Assert.Equal(1, img.GetHeight());
        // PNG images use FlateDecode
        COSBase? filter = img.GetStream()?.GetCOSObject().GetItem(COSName.FILTER);
        Assert.Equal(COSName.FLATE_DECODE, filter);
    }

    [Fact]
    public void CreateFromFile_Tiff_DispatchesToCCITTFactory()
    {
        using PDDocument doc = new();

        PDImageXObject img = PDImageXObject.CreateFromFile(ImageFixture("ccittg4.tif"), doc);

        Assert.Equal(COSName.CCITTFAX_DECODE, img.GetStream()?.GetCOSObject().GetItem(COSName.FILTER));
    }

    [Fact]
    public void CreateFromFile_UnsupportedExtension_ThrowsIOException()
    {
        using PDDocument doc = new();
        Assert.Throws<IOException>(() =>
            PDImageXObject.CreateFromFile("image.webp", doc));
    }
}

internal static class StreamExtensions
{
    internal static byte[] ToByteArray(this Stream stream)
    {
        using MemoryStream ms = new();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}
