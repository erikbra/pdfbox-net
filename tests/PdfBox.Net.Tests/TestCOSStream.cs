/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Converted from Apache PDFBox test source with focused xUnit adaptations.
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/cos/TestCOSStream.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using System.Text;
using PdfBox.Net.COS;
using Xunit;

namespace PdfBox.Net.Tests;

public class TestCOSStream
{
    [Fact]
    public void TestUncompressedStreamEncode()
    {
        byte[] testString = Encoding.ASCII.GetBytes("This is a test string to be used as input for TestCOSStream");
        COSStream stream = CreateStream(testString);
        ValidateEncoded(stream, testString);
    }

    [Fact]
    public void TestUncompressedStreamDecode()
    {
        byte[] testString = Encoding.ASCII.GetBytes("This is a test string to be used as input for TestCOSStream");
        COSStream stream = CreateStream(testString);
        ValidateDecoded(stream, testString);
    }

    [Fact]
    public void TestUncompressedStreamDoubleClose()
    {
        byte[] testString = Encoding.ASCII.GetBytes("This is a test string to be used as input for TestCOSStream");
        COSStream stream = new();
        Stream output = stream.CreateOutputStream();
        output.Write(testString, 0, testString.Length);
        output.Close();
        output.Close();
        ValidateEncoded(stream, testString);
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

    private static COSStream CreateStream(byte[] testString)
    {
        COSStream stream = new();
        using (Stream output = stream.CreateOutputStream())
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
            Assert.Equal(expected, captured.ToArray());
        }
    }

    private static void ValidateDecoded(COSStream stream, byte[] expected)
    {
        using (stream)
        using (Stream input = stream.CreateInputStream())
        using (MemoryStream captured = new())
        {
            input.CopyTo(captured);
            Assert.Equal(expected, captured.ToArray());
        }
    }
}
