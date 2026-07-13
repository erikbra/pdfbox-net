using System.Buffers.Binary;
using ImageMagick;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.Image;
using PdfBox.Net.PDModel.Resources;
using PdfBox.Net.Rendering;

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
        using BufferedImage bmp = new(4, 3, BufferedImage.TYPE_INT_ARGB);

        PDImageXObject img = LosslessFactory.CreateFromImage(doc, bmp);

        Assert.Equal(4, img.GetWidth());
        Assert.Equal(3, img.GetHeight());
    }

    [Fact]
    public void LosslessFactory_CreateFromImage_HasFlateDecodeFilter()
    {
        using PDDocument doc = new();
        using BufferedImage bmp = new(2, 2, BufferedImage.TYPE_INT_ARGB);

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
        using BufferedImage bmp = new(1, 1, BufferedImage.TYPE_INT_ARGB);

        PDImageXObject img = LosslessFactory.CreateFromImage(doc, bmp);

        Assert.Equal("DeviceRGB", img.GetColorSpace().GetName());
        Assert.Equal(8, img.GetBitsPerComponent());
    }

    [Fact]
    public void LosslessFactory_CreateFromImage_PixelDataRoundtrips()
    {
        using PDDocument doc = new();
        using BufferedImage bmp = new(2, 1, BufferedImage.TYPE_INT_ARGB);
        bmp.SetRgb(0, 0, unchecked((int)0xFFFF0000));    // red
        bmp.SetRgb(1, 0, unchecked((int)0xFF00FF00));    // green

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
    public void PdfImageExporter_ExportPng_ReturnsBrowserSafePng()
    {
        using PDDocument doc = new();
        byte[] rgb = [255, 0, 0, 0, 255, 0];
        PDImageXObject img = LosslessFactory.CreateFromRawData(doc, rgb, 2, 1, 8, 3);

        PdfImageExportResult result = PdfImageExporter.ExportPng(img);

        Assert.Equal("image/png", result.ContentType);
        Assert.Equal("png", result.FileExtension);
        Assert.True(result.Data.Length >= 24);
        Assert.Equal(0x89504E47u, BinaryPrimitives.ReadUInt32BigEndian(result.Data.AsSpan(0, 4)));
        Assert.Equal(2, BinaryPrimitives.ReadInt32BigEndian(result.Data.AsSpan(16, 4)));
        Assert.Equal(1, BinaryPrimitives.ReadInt32BigEndian(result.Data.AsSpan(20, 4)));
    }

    [Fact]
    public void PdfImageExporter_ExportForBrowser_PreservesDeviceRgbJpegBytes()
    {
        using PDDocument document = new();
        byte[] original = File.ReadAllBytes(ImageFixture("test-2x1-rgb.jpg"));
        using MemoryStream input = new(original);
        PDImageXObject image = JPEGFactory.CreateFromStream(document, input);

        PdfImageExportResult result = PdfImageExporter.ExportForBrowser(image);

        Assert.Equal("image/jpeg", result.ContentType);
        Assert.Equal("jpg", result.FileExtension);
        Assert.Equal(original, result.Data);
    }

    [Fact]
    public void PdfImageExporter_ExportForBrowser_PreservesDemonstrablySrgbIccJpeg()
    {
        using PDDocument document = new();
        byte[] original = File.ReadAllBytes(ImageFixture("test-2x1-rgb.jpg"));
        using MemoryStream input = new(original);
        PDImageXObject image = JPEGFactory.CreateFromStream(document, input);
        image.GetCOSObject()!.SetItem(
            COSName.COLORSPACE,
            CreateIccColorSpace(ColorProfiles.SRGB.ToByteArray(), 3).GetCOSObject());

        PdfImageExportResult result = PdfImageExporter.ExportForBrowser(image);

        Assert.Equal("image/jpeg", result.ContentType);
        Assert.Equal(original, result.Data);
    }

    [Fact]
    public void PdfImageExporter_ExportForBrowser_ConvertsNonSrgbIccJpegToPng()
    {
        using PDDocument document = new();
        using FileStream input = File.OpenRead(ImageFixture("test-2x1-rgb.jpg"));
        PDImageXObject image = JPEGFactory.CreateFromStream(document, input);
        image.GetCOSObject()!.SetItem(
            COSName.COLORSPACE,
            CreateIccColorSpace(ColorProfiles.AdobeRGB1998.ToByteArray(), 3).GetCOSObject());

        PdfImageExportResult result = PdfImageExporter.ExportForBrowser(image);

        AssertPng(result);
    }

    [Fact]
    public void PdfImageExporter_ExportForBrowser_ConvertsJpegWithDecodeArrayToPng()
    {
        using PDDocument document = new();
        using FileStream input = File.OpenRead(ImageFixture("test-2x1-rgb.jpg"));
        PDImageXObject image = JPEGFactory.CreateFromStream(document, input);
        image.GetCOSObject()!.SetItem(COSName.DECODE, COSArray.Of(1f, 0f, 1f, 0f, 1f, 0f));

        PdfImageExportResult result = PdfImageExporter.ExportForBrowser(image);

        AssertPng(result);
    }

    [Fact]
    public void PdfImageExporter_ExportForBrowser_ConvertsJpegWithDctDecodeParametersToPng()
    {
        using PDDocument document = new();
        using FileStream input = File.OpenRead(ImageFixture("test-2x1-rgb.jpg"));
        PDImageXObject image = JPEGFactory.CreateFromStream(document, input);
        COSDictionary decodeParameters = new();
        decodeParameters.SetInt(COSName.GetPDFName("ColorTransform"), 1);
        image.GetCOSObject()!.SetItem(COSName.DECODE_PARMS, decodeParameters);

        PdfImageExportResult result = PdfImageExporter.ExportForBrowser(image);

        AssertPng(result);
    }

    [Fact]
    public void PdfImageExporter_ExportForBrowser_ConvertsNonThreeComponentJpegToPng()
    {
        using PDDocument document = new();
        using FileStream input = File.OpenRead(ImageFixture("test-2x1-gray.jpg"));
        PDImageXObject image = JPEGFactory.CreateFromStream(document, input);

        PdfImageExportResult result = PdfImageExporter.ExportForBrowser(image);

        AssertPng(result);
    }

    [Fact]
    public void PdfImageExporter_ExportForBrowser_ConvertsJpegWithColorKeyMaskToPng()
    {
        using PDDocument document = new();
        using FileStream input = File.OpenRead(ImageFixture("test-2x1-rgb.jpg"));
        PDImageXObject image = JPEGFactory.CreateFromStream(document, input);
        image.GetCOSObject()!.SetItem(COSName.GetPDFName("Mask"), COSArray.Of(0, 0, 0, 0, 0, 0));

        PdfImageExportResult result = PdfImageExporter.ExportForBrowser(image);

        AssertPng(result);
    }

    [Fact]
    public void PdfImageExporter_ExportForBrowser_ConvertsDeviceRgbJpegWhenOutputIntentRequiresTransform()
    {
        using PDDocument document = new();
        using FileStream input = File.OpenRead(ImageFixture("test-2x1-rgb.jpg"));
        PDImageXObject image = JPEGFactory.CreateFromStream(document, input);
        using MemoryStream profile = new(ColorProfiles.AdobeRGB1998.ToByteArray());
        document.GetDocumentCatalog().AddOutputIntent(new PDOutputIntent(document, profile));
        PDColorManagementContext context = PDColorManagementContext.Create(document)!;

        PdfImageExportResult result = PdfImageExporter.ExportForBrowser(image, context);

        AssertPng(result);
    }

    [Fact]
    public void PdfImageExporter_OutputIntentDeviceCmyk_UsesSingleBatchTransform()
    {
        using PDDocument document = new();
        using MemoryStream profile = new(ColorProfiles.CoatedFOGRA39.ToByteArray());
        document.GetDocumentCatalog().AddOutputIntent(new PDOutputIntent(document, profile));
        PDColorManagementContext context = PDColorManagementContext.Create(document)!;
        byte[] samples = new byte[64 * 4];
        for (int pixel = 0; pixel < 64; pixel++)
        {
            samples[(pixel * 4)] = (byte)(pixel * 4);
            samples[(pixel * 4) + 1] = 160;
            samples[(pixel * 4) + 2] = 40;
            samples[(pixel * 4) + 3] = 20;
        }

        PDImageXObject image = LosslessFactory.CreateFromRawData(document, samples, 64, 1, 8, 4);

        PdfImageExportResult result = PdfImageExporter.ExportPng(image, context);

        using BufferedImage exported = RenderingBackend.Current.ImageCodec.Decode(result.Data)
            ?? throw new InvalidOperationException("The exported PNG could not be decoded.");
        int pixelRgb = exported.GetRgb(0, 0) & 0xFFFFFF;
        float[] naive = PDDeviceCMYK.Instance.ToRGB([0f, 160f / 255f, 40f / 255f, 20f / 255f]);
        int naiveRgb = ((int)MathF.Round(naive[0] * 255f) << 16) |
                       ((int)MathF.Round(naive[1] * 255f) << 8) |
                       (int)MathF.Round(naive[2] * 255f);
        Assert.NotEqual(naiveRgb, pixelRgb);
        Assert.Equal(1, context.GetColorTransformOperationCount());
    }

    [Fact]
    public void PdfImageExporter_ExportPng_PreservesXObjectSoftMaskAlpha()
    {
        using PDDocument doc = new();
        PDImageXObject image = LosslessFactory.CreateFromRawData(doc, [255, 0, 0, 255, 0, 0], 2, 1, 8, 3);
        PDImageXObject softMask = LosslessFactory.CreateFromRawData(doc, [255, 0], 2, 1, 8, 1);
        image.GetCOSObject()!.SetItem(COSName.SMASK, softMask.GetCOSObject());

        PdfImageExportResult result = PdfImageExporter.ExportForBrowser(image);

        using BufferedImage exported = RenderingBackend.Current.ImageCodec.Decode(result.Data)
            ?? throw new InvalidOperationException("The exported PNG could not be decoded.");
        Assert.Equal(0xFF, (exported.GetRgb(0, 0) >> 24) & 0xFF);
        Assert.Equal(0x00, (exported.GetRgb(1, 0) >> 24) & 0xFF);
    }

    [Fact]
    public void PdfImageExporter_ExportPng_ExportsInlineImage()
    {
        COSDictionary parameters = new();
        parameters.SetInt(COSName.GetPDFName("W"), 1);
        parameters.SetInt(COSName.H, 1);
        parameters.SetInt(COSName.GetPDFName("BPC"), 8);
        parameters.SetItem(COSName.CS, COSName.GetPDFName("RGB"));
        PDInlineImage image = new(parameters, [255, 0, 0], new PDResources());

        PdfImageExportResult result = PdfImageExporter.ExportPng(image);

        Assert.Equal("image/png", result.ContentType);
        Assert.Equal(1, BinaryPrimitives.ReadInt32BigEndian(result.Data.AsSpan(16, 4)));
        Assert.Equal(1, BinaryPrimitives.ReadInt32BigEndian(result.Data.AsSpan(20, 4)));
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
    public void SampledImageReader_GetRGBImage_TransformsIccRasterAsSingleBatch()
    {
        byte[] samples = new byte[64 * 4];
        for (int pixel = 0; pixel < 64; pixel++)
        {
            samples[(pixel * 4)] = (byte)(pixel * 4);
            samples[(pixel * 4) + 1] = 160;
            samples[(pixel * 4) + 2] = 40;
            samples[(pixel * 4) + 3] = 20;
        }

        PDICCBased colorSpace = CreateIccColorSpace(ColorProfiles.CoatedFOGRA39.ToByteArray(), 4);

        byte[] rgb = SampledImageReader.GetRGBImage(64, 1, 8, colorSpace, samples, null);

        Assert.Equal(64 * 3, rgb.Length);
        Assert.Equal(1, colorSpace.GetColorTransformOperationCount());
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

    [Fact]
    public void SampledImageReader_GetRGBImage_UnpacksSixteenBitGraySamples()
    {
        using PDDocument doc = new();
        PDImageXObject img = LosslessFactory.CreateFromRawData(
            doc,
            [0x00, 0x00, 0x80, 0x00, 0xFF, 0xFF],
            3,
            1,
            16,
            1);

        byte[] rgb = SampledImageReader.GetRGBImage(img);

        Assert.Equal([0, 0, 0, 128, 128, 128, 255, 255, 255], rgb);
    }

    // ─── CCITTFactory ─────────────────────────────────────────────────────────

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
        return Assert.IsType<PDICCBased>(PDColorSpace.Create(array));
    }

    private static void AssertPng(PdfImageExportResult result)
    {
        Assert.Equal("image/png", result.ContentType);
        Assert.Equal("png", result.FileExtension);
        Assert.True(result.Data.Length >= 24);
        Assert.Equal(0x89504E47u, BinaryPrimitives.ReadUInt32BigEndian(result.Data.AsSpan(0, 4)));
    }

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
