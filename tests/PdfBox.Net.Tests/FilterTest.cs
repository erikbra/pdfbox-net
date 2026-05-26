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

    private static byte[] Encode(FilterBase filter, byte[] source)
    {
        using MemoryStream input = new(source);
        using MemoryStream output = new();
        filter.Encode(input, output, new COSDictionary(), 0);
        return output.ToArray();
    }

    private static byte[] Decode(FilterBase filter, byte[] source, COSDictionary parameters)
    {
        using MemoryStream input = new(source);
        using MemoryStream output = new();
        filter.Decode(input, output, parameters, 0, DecodeOptions.DEFAULT);
        return output.ToArray();
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
