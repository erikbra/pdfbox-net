/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Converted from Apache PDFBox test source with focused xUnit adaptations.
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/cos/TestCOSStream.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using System.Text;
using PdfBox.Net.COS;
using PdfBox.Net.Filter;
using FilterBase = PdfBox.Net.Filter.Filter;
using Xunit;

namespace PdfBox.Net.Tests;

public class TestCOSStream
{
    [Fact]
    public void TestUncompressedStreamEncode()
    {
        byte[] testString = Encoding.ASCII.GetBytes("This is a test string to be used as input for TestCOSStream");
        COSStream stream = CreateStream(testString, null);
        ValidateEncoded(stream, testString);
    }

    [Fact]
    public void TestUncompressedStreamDecode()
    {
        byte[] testString = Encoding.ASCII.GetBytes("This is a test string to be used as input for TestCOSStream");
        COSStream stream = CreateStream(testString, null);
        ValidateDecoded(stream, testString);
    }

    [Fact]
    public void TestCompressedStream1Encode()
    {
        byte[] testString = Encoding.ASCII.GetBytes("This is a test string to be used as input for TestCOSStream");
        byte[] testStringEncoded = EncodeData(testString, COSName.FLATE_DECODE);
        COSStream stream = CreateStream(testString, COSName.FLATE_DECODE);
        ValidateEncoded(stream, testStringEncoded);
    }

    [Fact]
    public void TestCompressedStream1Decode()
    {
        byte[] testString = Encoding.ASCII.GetBytes("This is a test string to be used as input for TestCOSStream");
        byte[] testStringEncoded = EncodeData(testString, COSName.FLATE_DECODE);
        COSStream stream = new();

        using (Stream output = stream.CreateRawOutputStream())
        {
            output.Write(testStringEncoded, 0, testStringEncoded.Length);
        }

        stream.SetItem(COSName.FILTER, COSName.FLATE_DECODE);
        ValidateDecoded(stream, testString);
    }

    [Fact]
    public void TestCompressedStream2Encode()
    {
        byte[] testString = Encoding.ASCII.GetBytes("This is a test string to be used as input for TestCOSStream");
        byte[] testStringEncoded = EncodeData(testString, COSName.FLATE_DECODE);
        testStringEncoded = EncodeData(testStringEncoded, COSName.ASCII85_DECODE);

        COSArray filters = new();
        filters.Add(COSName.ASCII85_DECODE);
        filters.Add(COSName.FLATE_DECODE);

        COSStream stream = CreateStream(testString, filters);
        ValidateEncoded(stream, testStringEncoded);
    }

    [Fact]
    public void TestCompressedStream2Decode()
    {
        byte[] testString = Encoding.ASCII.GetBytes("This is a test string to be used as input for TestCOSStream");
        byte[] testStringEncoded = EncodeData(testString, COSName.FLATE_DECODE);
        testStringEncoded = EncodeData(testStringEncoded, COSName.ASCII85_DECODE);
        COSStream stream = new();

        COSArray filters = new();
        filters.Add(COSName.ASCII85_DECODE);
        filters.Add(COSName.FLATE_DECODE);
        stream.SetItem(COSName.FILTER, filters);

        using (Stream output = stream.CreateRawOutputStream())
        {
            output.Write(testStringEncoded, 0, testStringEncoded.Length);
        }

        ValidateDecoded(stream, testString);
    }

    [Fact]
    public void TestCompressedStreamDoubleClose()
    {
        byte[] testString = Encoding.ASCII.GetBytes("This is a test string to be used as input for TestCOSStream");
        byte[] testStringEncoded = EncodeData(testString, COSName.FLATE_DECODE);
        COSStream stream = new();
        Stream output = stream.CreateOutputStream(COSName.FLATE_DECODE);
        output.Write(testString, 0, testString.Length);
        output.Close();
        output.Close();
        ValidateEncoded(stream, testStringEncoded);
    }

    [Fact]
    public void TestHasStreamData()
    {
        using COSStream stream = new();
        Assert.False(stream.HasData());
        Assert.Throws<IOException>(() => stream.CreateInputStream());

        byte[] testString = Encoding.ASCII.GetBytes("This is a test string to be used as input for TestCOSStream");
        using (Stream output = stream.CreateOutputStream())
        {
            output.Write(testString, 0, testString.Length);
        }

        Assert.True(stream.HasData());
    }

    private static byte[] EncodeData(byte[] original, COSName filter)
    {
        FilterBase encodingFilter = FilterFactory.Instance.GetFilter(filter);
        using MemoryStream encoded = new();
        using MemoryStream input = new(original, false);
        encodingFilter.Encode(input, encoded, new COSDictionary(), 0);
        return encoded.ToArray();
    }

    private static COSStream CreateStream(byte[] testString, COSBase? filters)
    {
        COSStream stream = new();
        using (Stream output = stream.CreateOutputStream(filters))
        {
            output.Write(testString, 0, testString.Length);
        }

        return stream;
    }

    private static void ValidateEncoded(COSStream stream, byte[] expected)
    {
        using (stream)
        using (Stream input = stream.CreateRawInputStream())
        using (MemoryStream captured = new())
        {
            input.CopyTo(captured);
            Assert.True(expected.AsSpan().SequenceEqual(captured.ToArray()));
        }
    }

    private static void ValidateDecoded(COSStream stream, byte[] expected)
    {
        using (stream)
        using (Stream input = stream.CreateInputStream())
        using (MemoryStream captured = new())
        {
            input.CopyTo(captured);
            Assert.True(expected.AsSpan().SequenceEqual(captured.ToArray()));
        }
    }
}
