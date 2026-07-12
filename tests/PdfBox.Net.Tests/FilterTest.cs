/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Focused filter tests for PdfBox.Net parity work.
 */

using System.IO.Compression;
using System.Text;
using ImageMagick;
using PdfBox.Net.COS;
using PdfBox.Net.Filter;
using PdfBox.Net.PDModel.Graphics.Color;
using FilterBase = PdfBox.Net.Filter.Filter;

namespace PdfBox.Net.Tests;

public class FilterTest
{
    [Fact]
    public void FlateFilterRoundTrip()
    {
        FlateFilter filter = new();
        byte[] source = Encoding.ASCII.GetBytes("q 1 0 0 1 10 20 cm BT /F1 12 Tf (Hello Flate) Tj ET Q\n");

        byte[] encoded = Encode(filter, source);
        byte[] decoded = Decode(filter, encoded, new COSDictionary());

        Assert.Equal(source, decoded);
    }

    [Fact]
    public void FlateFilterDecodesRealCompressedContentFixture()
    {
        byte[] source = Encoding.ASCII.GetBytes("BT /F1 10 Tf 72 720 Td (Fixture stream) Tj ET\n");
        byte[] compressedFixture = CompressWithZlib(source);

        FlateFilter filter = new();
        byte[] decoded = Decode(filter, compressedFixture, new COSDictionary());

        Assert.Equal(source, decoded);
    }

    [Fact]
    public void FlateFilterHonorsPdfBoxDeflateLevelSetting()
    {
        byte[] source = Encoding.ASCII.GetBytes(string.Concat(Enumerable.Repeat(
            "BT /F1 10 Tf 72 720 Td (deflate level fixture) Tj ET\n",
            64)));
        string? previousLevel = Environment.GetEnvironmentVariable(FilterBase.SyspropDeflateLevel);

        try
        {
            Environment.SetEnvironmentVariable(FilterBase.SyspropDeflateLevel, "5");

            byte[] encoded = Encode(new FlateFilter(), source);

            Assert.Equal(CompressWithZlib(source, 5), encoded);
            Assert.NotEqual(CompressWithZlib(source, 1), encoded);
            Assert.Equal(source, Decode(new FlateFilter(), encoded, new COSDictionary()));
        }
        finally
        {
            Environment.SetEnvironmentVariable(FilterBase.SyspropDeflateLevel, previousLevel);
        }
    }

    [Fact]
    public void ASCIIHexRoundTrip()
    {
        ASCIIHexFilter filter = new();
        byte[] source = [0x00, 0x11, 0x22, 0x33, 0x44, 0xFE, 0xFF];

        byte[] encoded = Encode(filter, source);
        byte[] decoded = Decode(filter, encoded, new COSDictionary());

        Assert.Equal(source, decoded);
    }

    [Fact]
    public void ASCII85RoundTrip()
    {
        ASCII85Filter filter = new();
        byte[] source = Encoding.ASCII.GetBytes("ASCII85 needs stable tuple handling and z-shorthand support.");

        byte[] encoded = Encode(filter, source);
        byte[] decoded = Decode(filter, encoded, new COSDictionary());

        Assert.Equal(source, decoded);
    }

    [Fact]
    public void ASCII85StreamRoundTrip()
    {
        byte[] source = Encoding.ASCII.GetBytes("ASCII85 stream helper parity.");
        byte[] encoded;
        using (MemoryStream encodedSink = new())
        {
            using (ASCII85OutputStream output = new(encodedSink))
            {
                output.Write(source, 0, source.Length);
            }

            encoded = encodedSink.ToArray();
        }

        using MemoryStream encodedSource = new(encoded);
        using ASCII85InputStream input = new(encodedSource);
        using MemoryStream decoded = new();
        input.CopyTo(decoded);

        Assert.Equal(source, decoded.ToArray());
    }

    [Fact]
    public void RunLengthRoundTrip()
    {
        RunLengthDecodeFilter filter = new();
        byte[] source = [1, 1, 1, 1, 2, 3, 4, 5, 9, 9, 9, 9, 9, 8, 7, 6, 5, 4, 4, 4, 4];

        byte[] encoded = Encode(filter, source);
        byte[] decoded = Decode(filter, encoded, new COSDictionary());

        Assert.Equal(source, decoded);
    }

    [Fact]
    public void CCITTFaxGroup4RoundTripWithBlackIsOne()
    {
        CCITTFaxDecodeFilter filter = new();
        byte[] source =
        [
            0b1111_0000,
            0b0011_1100,
            0b0000_1111,
            0b1010_1010
        ];

        COSDictionary encodeParameters = new();
        encodeParameters.SetInt(COSName.COLUMNS, 8);
        encodeParameters.SetInt(COSName.ROWS, 4);

        byte[] encoded = Encode(filter, source, encodeParameters);
        Assert.Equal(
            [0x26, 0xAE, 0x18, 0x70, 0xC3, 0x26, 0xA8, 0x22, 0x87, 0x04, 0x12, 0x80, 0x08, 0x00, 0x80],
            encoded);

        byte[] decoded = Decode(filter, encoded, CreateCcittDecodeParameters(8, 4, blackIsOne: true));

        Assert.Equal(source, decoded);
    }

    [Fact]
    public void CCITTFaxDecodeInvertsWhenBlackIsZero()
    {
        CCITTFaxDecodeFilter filter = new();
        byte[] source = [0b1111_0000];
        byte[] expectedInverted = [0b0000_1111];

        COSDictionary encodeParameters = new();
        encodeParameters.SetInt(COSName.COLUMNS, 8);
        encodeParameters.SetInt(COSName.ROWS, 1);

        byte[] encoded = Encode(filter, source, encodeParameters);
        byte[] decoded = Decode(filter, encoded, CreateCcittDecodeParameters(8, 1, blackIsOne: false));

        Assert.Equal(expectedInverted, decoded);
    }

    [Fact]
    public void CCITTFaxDecodeRejectsInvalidDimensions()
    {
        CCITTFaxDecodeFilter filter = new();
        COSDictionary parameters = CreateCcittDecodeParameters(0, 1, blackIsOne: true);

        IOException ex = Assert.Throws<IOException>(() => Decode(filter, [0], parameters));

        Assert.Contains("Invalid CCITT image dimensions", ex.Message);
    }

    [Fact]
    public void DctFilterDecodesRgbJpegFixture()
    {
        byte[] encoded = File.ReadAllBytes(Path.Combine("Fixtures", "Images", "test-2x1-rgb.jpg"));

        byte[] decoded = Decode(new DCTFilter(), encoded, new COSDictionary());

        Assert.Equal([121, 121, 121, 130, 130, 130], decoded);
    }

    [Fact]
    public void DctFilterDecodesGrayscaleJpegFixture()
    {
        byte[] encoded = File.ReadAllBytes(Path.Combine("Fixtures", "Images", "test-2x1-gray.jpg"));

        byte[] decoded = Decode(new DCTFilter(), encoded, new COSDictionary());

        Assert.Equal([121, 130], decoded);
    }

    [Fact]
    public void DctFilterDecodesCmykJpegFixture()
    {
        byte[] encoded = File.ReadAllBytes(Path.Combine("Fixtures", "Images", "jpegcmyk.jpg"));

        byte[] decoded = Decode(new DCTFilter(), encoded, new COSDictionary());

        Assert.Equal(343 * 287 * 4, decoded.Length);
        Assert.Equal([255, 255, 255, 255], decoded[..4]);
    }

    [Fact]
    public void DctFilterReportsMissingProviderForCmykJpeg()
    {
        byte[] encoded = File.ReadAllBytes(Path.Combine("Fixtures", "Images", "jpegcmyk.jpg"));

        NotSupportedException ex = Assert.Throws<NotSupportedException>(() =>
            Decode(new DCTFilter(MissingJpegRasterDecoder.Instance), encoded, new COSDictionary()));

        Assert.Contains("PdfBox.Net.ImageMagick", ex.Message);
    }

    [Fact]
    public void DctFilterEncodeReportsUseJpegFactory()
    {
        NotSupportedException ex = Assert.Throws<NotSupportedException>(() => Encode(new DCTFilter(), [0, 1, 2]));

        Assert.Contains("JPEGFactory", ex.Message);
    }

    [Fact]
    public void JpxFilterDecodesRgbJp2Fixture()
    {
        byte[] encoded = EncodeJp2(CreatePpm(2, 1, [255, 0, 0, 0, 255, 0]), MagickFormat.Ppm);
        JPXFilter filter = new();
        COSDictionary parameters = new();
        parameters.SetItem(COSName.COLORSPACE, COSName.GetPDFName("DeviceRGB"));
        parameters.SetInt(COSName.BITS_PER_COMPONENT, 1);
        parameters.SetInt(COSName.WIDTH, 99);
        parameters.SetInt(COSName.HEIGHT, 99);
        parameters.SetItem(COSName.DECODE, new COSArray());

        DecodeResult result = DecodeWithResult(filter, encoded, parameters, out byte[] decoded);

        Assert.Equal(6, decoded.Length);
        AssertColorNear([255, 0, 0], decoded[..3], tolerance: 8);
        AssertColorNear([0, 255, 0], decoded[3..6], tolerance: 8);
        Assert.Equal(8, result.GetParameters().GetInt(COSName.BITS_PER_COMPONENT));
        Assert.Equal(2, result.GetParameters().GetInt(COSName.WIDTH));
        Assert.Equal(1, result.GetParameters().GetInt(COSName.HEIGHT));
        Assert.Null(result.GetParameters().GetDictionaryObject(COSName.DECODE));
        Assert.Null(result.GetJPXColorSpace());
    }

    [Fact]
    public void JpxFilterDecodesGrayscaleJp2AndEmbeddedColorSpace()
    {
        byte[] encoded = EncodeJp2(CreatePgm(2, 1, [32, 224]), MagickFormat.Pgm);

        DecodeResult result = DecodeWithResult(new JPXFilter(), encoded, new COSDictionary(), out byte[] decoded);

        Assert.Equal(2, decoded.Length);
        Assert.InRange(decoded[0], 24, 40);
        Assert.InRange(decoded[1], 216, 232);
        PDJPXColorSpace colorSpace = Assert.IsType<PDJPXColorSpace>(result.GetJPXColorSpace());
        Assert.Equal(1, colorSpace.GetNumberOfComponents());
        Assert.Equal([0f, 1f], colorSpace.GetDefaultDecode(8));
    }

    [Fact]
    public void JpxFilterHonorsSourceRegionAndSubsampling()
    {
        byte[] encoded = EncodeJp2(CreatePpm(3, 2,
        [
            10, 20, 30, 40, 50, 60, 70, 80, 90,
            100, 110, 120, 130, 140, 150, 160, 170, 180
        ]), MagickFormat.Ppm);
        DecodeOptions options = new(0, 0, 3, 2);
        options.SetSubsamplingX(2);
        options.SetSubsamplingY(1);

        DecodeResult result = DecodeWithResult(new JPXFilter(), encoded, new COSDictionary(), options, out byte[] decoded);

        Assert.Equal(12, decoded.Length);
        Assert.True(options.IsFilterSubsampled());
        Assert.Equal(3, result.GetParameters().GetInt(COSName.WIDTH));
        Assert.Equal(2, result.GetParameters().GetInt(COSName.HEIGHT));
    }

    [Fact]
    public void JpxFilterReportsUnsupportedPayloadClearly()
    {
        IOException ex = Assert.Throws<IOException>(() => Decode(new JPXFilter(), [1, 2, 3], new COSDictionary()));

        Assert.Contains("JPEG 2000 (JPX)", ex.Message);
    }

    [Fact]
    public void JpxFilterReportsMissingProviderClearly()
    {
        NotSupportedException ex = Assert.Throws<NotSupportedException>(() =>
            Decode(new JPXFilter(MissingJpxRasterDecoder.Instance), [1, 2, 3], new COSDictionary()));

        Assert.Contains("PdfBox.Net.ImageMagick", ex.Message);
    }

    [Fact]
    public void Jbig2FilterReportsInvalidPayloadClearly()
    {
        IOException ex = Assert.Throws<IOException>(() => Decode(new JBIG2Filter(), [1, 2, 3], new COSDictionary()));

        Assert.Contains("Could not read JBIG2 image", ex.Message);
    }

    [Fact]
    public void Jbig2FilterMissingDecoderAdapterReportsClearly()
    {
        IOException ex = Assert.Throws<IOException>(() => Decode(new JBIG2Filter(MissingJbig2RasterDecoder.Instance), [1, 2, 3], new COSDictionary()));

        Assert.Contains("JBIG2 decoder is not installed", ex.Message);
    }

    [Fact]
    public void Jbig2FilterPassesGlobalsAndDecodeOptionsToDecoder()
    {
        RecordingJbig2Decoder decoder = new([0x80]);
        JBIG2Filter filter = new(decoder);
        COSStream globals = new();
        using (Stream globalOutput = globals.CreateOutputStream())
        {
            globalOutput.Write([0xAA, 0xBB]);
        }

        COSDictionary decodeParms = new();
        decodeParms.SetItem(COSName.JBIG2_GLOBALS, globals);
        COSDictionary parameters = new();
        parameters.SetItem(COSName.FILTER, COSName.JBIG2_DECODE);
        parameters.SetItem(COSName.DECODE_PARMS, decodeParms);
        parameters.SetInt(COSName.BITS_PER_COMPONENT, 2);
        DecodeOptions options = new(new DecodeRegion(1, 2, 3, 4));
        options.SetSubsamplingX(2);
        options.SetSubsamplingY(3);
        options.SetSubsamplingOffsetX(1);
        options.SetSubsamplingOffsetY(2);

        DecodeResult result = DecodeWithResult(filter, [0x10, 0x20, 0x30], parameters, options, out byte[] decoded);

        Assert.Equal([0x80], decoded);
        Assert.Equal([0x10, 0x20, 0x30], decoder.Encoded);
        Assert.Equal([0xAA, 0xBB], decoder.Globals);
        Assert.Equal(options.GetSourceRegion(), decoder.Options.SourceRegion);
        Assert.Equal(2, decoder.Options.SubsamplingX);
        Assert.Equal(3, decoder.Options.SubsamplingY);
        Assert.Equal(1, decoder.Options.SubsamplingOffsetX);
        Assert.Equal(2, decoder.Options.SubsamplingOffsetY);
        Assert.Equal(2, decoder.Options.BitsPerComponent);
        Assert.True(options.IsFilterSubsampled());
        Assert.Same(parameters, result.GetParameters());
    }

    [Fact]
    public void LzwRoundTrip()
    {
        LZWFilter filter = new();
        byte[] source = Encoding.ASCII.GetBytes("TOBEORNOTTOBEORTOBEORNOT - LZW regression fixture");

        byte[] encoded = Encode(filter, source);
        byte[] decoded = Decode(filter, encoded, new COSDictionary());

        Assert.Equal(source, decoded);
    }

    [Fact]
    public void FlateDecoderStreamDecodesZlibFixture()
    {
        byte[] source = Encoding.ASCII.GetBytes("Flate decoder stream fixture");
        byte[] compressedFixture = CompressWithZlib(source);
        using MemoryStream compressed = new(compressedFixture);
        using FlateFilterDecoderStream stream = new(compressed);
        using MemoryStream decoded = new();
        stream.CopyTo(decoded);
        Assert.Equal(source, decoded.ToArray());
    }

    [Fact]
    public void FilterFactoryResolvesStandardFilters()
    {
        FilterFactory factory = FilterFactory.Instance;

        Assert.IsType<FlateFilter>(factory.GetFilter(COSName.FLATE_DECODE));
        Assert.IsType<FlateFilter>(factory.GetFilter(COSName.FLATE_DECODE_ABBREVIATION));
        Assert.IsType<ASCIIHexFilter>(factory.GetFilter(COSName.ASCII_HEX_DECODE));
        Assert.IsType<ASCIIHexFilter>(factory.GetFilter(COSName.ASCII_HEX_DECODE_ABBREVIATION));
        Assert.IsType<ASCII85Filter>(factory.GetFilter(COSName.ASCII85_DECODE));
        Assert.IsType<ASCII85Filter>(factory.GetFilter(COSName.ASCII85_DECODE_ABBREVIATION));
        Assert.IsType<RunLengthDecodeFilter>(factory.GetFilter(COSName.RUN_LENGTH_DECODE));
        Assert.IsType<LZWFilter>(factory.GetFilter(COSName.LZW_DECODE));
        Assert.IsType<DCTFilter>(factory.GetFilter(COSName.DCT_DECODE));
        Assert.IsType<CCITTFaxDecodeFilter>(factory.GetFilter(COSName.CCITTFAX_DECODE));
        Assert.IsType<JPXFilter>(factory.GetFilter(COSName.JPX_DECODE));
        Assert.IsType<JBIG2Filter>(factory.GetFilter(COSName.JBIG2_DECODE));
        Assert.IsType<CryptFilter>(factory.GetFilter(COSName.CRYPT));
    }

    [Fact]
    public void PlaceholderFilters_ThrowNotSupported()
    {
        Assert.Throws<NotSupportedException>(() => Encode(new JBIG2Filter(), [1, 2, 3]));
    }

    [Fact]
    public void CryptFilter_IdentityRoundTrips_AndUnknownFilterThrows()
    {
        CryptFilter filter = new();
        byte[] source = Encoding.ASCII.GetBytes("crypt identity fixture");

        COSDictionary identity = new();
        identity.SetItem(COSName.NAME, COSName.IDENTITY);
        byte[] encoded = Encode(filter, source);
        byte[] decoded = Decode(filter, encoded, identity);
        Assert.Equal(source, decoded);

        COSDictionary unsupported = new();
        unsupported.SetItem(COSName.NAME, COSName.GetPDFName("AESV2"));
        Assert.Throws<NotSupportedException>(() => Decode(filter, source, unsupported));
    }

    private static byte[] Encode(FilterBase filter, byte[] source)
    {
        return Encode(filter, source, new COSDictionary());
    }

    private static byte[] Encode(FilterBase filter, byte[] source, COSDictionary parameters)
    {
        using MemoryStream input = new(source);
        using MemoryStream output = new();
        filter.Encode(input, output, parameters, 0);
        return output.ToArray();
    }

    private static byte[] Decode(FilterBase filter, byte[] source, COSDictionary parameters)
    {
        using MemoryStream input = new(source);
        using MemoryStream output = new();
        filter.Decode(input, output, parameters, 0, DecodeOptions.DEFAULT);
        return output.ToArray();
    }

    private static DecodeResult DecodeWithResult(FilterBase filter, byte[] source, COSDictionary parameters, out byte[] decoded)
    {
        return DecodeWithResult(filter, source, parameters, DecodeOptions.DEFAULT, out decoded);
    }

    private static DecodeResult DecodeWithResult(FilterBase filter, byte[] source, COSDictionary parameters, DecodeOptions options, out byte[] decoded)
    {
        using MemoryStream input = new(source);
        using MemoryStream output = new();
        DecodeResult result = filter.Decode(input, output, parameters, 0, options);
        decoded = output.ToArray();
        return result;
    }

    private static COSDictionary CreateCcittDecodeParameters(int columns, int rows, bool blackIsOne)
    {
        COSDictionary decodeParms = new();
        decodeParms.SetInt(COSName.COLUMNS, columns);
        decodeParms.SetInt(COSName.ROWS, rows);
        decodeParms.SetInt(COSName.K, -1);
        decodeParms.SetBoolean(COSName.BLACK_IS_1, blackIsOne);

        COSDictionary parameters = new();
        parameters.SetItem(COSName.FILTER, COSName.CCITTFAX_DECODE);
        parameters.SetItem(COSName.DECODE_PARMS, decodeParms);
        parameters.SetInt(COSName.HEIGHT, rows);
        return parameters;
    }

    private static byte[] CompressWithZlib(byte[] source)
    {
        return CompressWithZlib(source, 9);
    }

    private static byte[] CompressWithZlib(byte[] source, int compressionLevel)
    {
        using MemoryStream encoded = new();
        ZLibCompressionOptions options = new()
        {
            CompressionLevel = compressionLevel
        };
        using (ZLibStream zlib = new(encoded, options, leaveOpen: true))
        {
            zlib.Write(source, 0, source.Length);
        }

        return encoded.ToArray();
    }

    private static byte[] EncodeJp2(byte[] imageBytes, MagickFormat sourceFormat)
    {
        using MagickImage image = new(imageBytes, sourceFormat);
        image.Quality = 100;
        using MemoryStream encoded = new();
        image.Write(encoded, MagickFormat.Jp2);
        return encoded.ToArray();
    }

    private static byte[] CreatePpm(int width, int height, byte[] rgb)
    {
        byte[] header = Encoding.ASCII.GetBytes($"P6\n{width} {height}\n255\n");
        return [.. header, .. rgb];
    }

    private static byte[] CreatePgm(int width, int height, byte[] gray)
    {
        byte[] header = Encoding.ASCII.GetBytes($"P5\n{width} {height}\n255\n");
        return [.. header, .. gray];
    }

    private static void AssertColorNear(byte[] expected, byte[] actual, int tolerance)
    {
        Assert.Equal(expected.Length, actual.Length);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.InRange(actual[i], Math.Max(0, expected[i] - tolerance), Math.Min(255, expected[i] + tolerance));
        }
    }

    private sealed class RecordingJbig2Decoder(byte[] decoded) : IJbig2RasterDecoder
    {
        public byte[] Encoded { get; private set; } = [];
        public byte[]? Globals { get; private set; }
        public Jbig2DecodeOptions Options { get; private set; } = new(null, 1, 1, 0, 0, 1);

        public byte[] Decode(byte[] encoded, byte[]? globals, Jbig2DecodeOptions options)
        {
            Encoded = encoded;
            Globals = globals;
            Options = options;
            return decoded;
        }
    }
}
