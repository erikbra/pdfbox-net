/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Focused filter tests for PdfBox.Net parity work.
 */

using System.IO.Compression;
using System.Text;
using PdfBox.Net.COS;
using PdfBox.Net.Filter;
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
    public void DctFilterEncodeReportsUseJpegFactory()
    {
        NotSupportedException ex = Assert.Throws<NotSupportedException>(() => Encode(new DCTFilter(), [0, 1, 2]));

        Assert.Contains("JPEGFactory", ex.Message);
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
        COSDictionary parameters = new();
        byte[] payload = [1, 2, 3];

        Assert.Throws<NotSupportedException>(() => Decode(new JPXFilter(), payload, parameters));
        Assert.Throws<NotSupportedException>(() => Decode(new JBIG2Filter(), payload, parameters));
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
        using MemoryStream encoded = new();
        using (ZLibStream zlib = new(encoded, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            zlib.Write(source, 0, source.Length);
        }

        return encoded.ToArray();
    }
}
