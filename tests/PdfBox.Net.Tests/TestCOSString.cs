/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Converted from Apache PDFBox test source with focused xUnit adaptations.
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/cos/TestCOSString.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using System.Text;
using PdfBox.Net.COS;
using Xunit;

namespace PdfBox.Net.Tests;

public class TestCOSString
{
    private const string EscCharString = "( test#some) escaped< \\chars>!~1239857 ";
    private const string EscCharStringPdfFormat = "\\( test#some\\) escaped< \\\\chars>!~1239857 ";

    [Fact]
    public void TestSetForceHexLiteralForm()
    {
        string inputString = "Test with a text and a few numbers 1, 2 and 3";
        string pdfHex = "<" + CreateHex(inputString) + ">";
        COSString cosStr = new(inputString, true);
        WritePDFTests(pdfHex, cosStr);

        COSString escStr = new(EscCharString);
        WritePDFTests("(" + EscCharStringPdfFormat + ")", escStr);
        COSString escStrHex = new(EscCharString, true);
        WritePDFTests("<" + CreateHex(EscCharString) + ">", escStrHex);
    }

    [Fact]
    public void TestFromHex()
    {
        string expected = "Quick and simple test";
        string hexForm = CreateHex(expected);
        COSString test1 = COSString.ParseHex(hexForm);
        WritePDFTests("(" + expected + ")", test1);
        COSString test2 = COSString.ParseHex(CreateHex(EscCharString));
        WritePDFTests("(" + EscCharStringPdfFormat + ")", test2);
        Assert.Throws<IOException>(() => COSString.ParseHex(hexForm + "xx"));
    }

    [Fact]
    public void TestGetHex()
    {
        string expected = "Test subject for testing getHex";
        COSString test1 = new(expected);
        string hexForm = CreateHex(expected);
        Assert.Equal(hexForm, test1.ToHexString());
        COSString escCS = new(EscCharString);
        Assert.Equal(CreateHex(EscCharString), escCS.ToHexString());
    }

    [Fact]
    public void TestGetString()
    {
        string testStr = "Test subject for getString()";
        COSString test1 = new(testStr);
        Assert.Equal(testStr, test1.GetString());

        COSString hexStr = COSString.ParseHex(CreateHex(testStr));
        Assert.Equal(testStr, hexStr.GetString());

        COSString escapedString = new(EscCharString);
        Assert.Equal(EscCharString, escapedString.GetString());

        testStr = "Line1\nLine2\nLine3\n";
        COSString lineFeedString = new(testStr);
        Assert.Equal(testStr, lineFeedString.GetString());
    }

    [Fact]
    public void TestGetBytes()
    {
        COSString str = new(EscCharString);
        Assert.Equal(Encoding.Latin1.GetBytes(EscCharString), str.GetBytes());
    }

    [Fact]
    public void TestWritePDF()
    {
        COSString testSubj = new(EscCharString);
        WritePDFTests("(" + EscCharStringPdfFormat + ")", testSubj);
        string textString = "This is just an arbitrary piece of text for testing";
        COSString testSubj2 = new(textString);
        WritePDFTests("(" + textString + ")", testSubj2);
    }

    [Fact]
    public void TestUnicode()
    {
        string theString = "\u4e16";
        COSString str = new(theString);
        Assert.Equal(theString, str.GetString());

        string textAscii = "This is some regular text. It should all be expressible in ASCII";
        string text8Bit = "En français où les choses sont accentués. En español, así";
        string textHighBits = "をクリックしてく";

        COSString stringAscii = new(textAscii);
        Assert.Equal(textAscii, stringAscii.GetString());
        COSString string8Bit = new(text8Bit);
        Assert.Equal(text8Bit, string8Bit.GetString());
        COSString stringHighBits = new(textHighBits);
        Assert.Equal(textHighBits, stringHighBits.GetString());
    }

    [Fact]
    public void TestEquals()
    {
        for (int i = 0; i < 10; i++)
        {
            COSString x1 = new("Test");
            Assert.Equal(x1, x1);

            COSString y1 = new("Test");
            Assert.Equal(x1, y1);
            Assert.Equal(y1, x1);
            COSString x2 = new("Test", true);
            Assert.NotEqual(x1, x2);
            Assert.NotEqual(x2, x1);

            COSString z1 = new("Test");
            Assert.Equal(x1, y1);
            Assert.Equal(y1, z1);
            Assert.Equal(x1, z1);
            Assert.NotEqual(y1, x2);
            Assert.NotEqual(x1, x2);
        }
    }

    [Fact]
    public void TestHashCode()
    {
        COSString str1 = new("Test1");
        COSString str2 = new("Test2");
        Assert.NotEqual(str1.GetHashCode(), str2.GetHashCode());
        COSString str3 = new("Test1");
        Assert.Equal(str1.GetHashCode(), str3.GetHashCode());
        COSString str3Hex = new("Test1", true);
        Assert.NotEqual(str1.GetHashCode(), str3Hex.GetHashCode());
    }

    [Fact]
    public void TestCompareFromHexString()
    {
        COSString test1 = COSString.ParseHex("000000FF000000");
        COSString test2 = COSString.ParseHex("000000FF00FFFF");
        Assert.Equal(test1, test1);
        Assert.Equal(test2, test2);
        Assert.NotEqual(test1.ToHexString(), test2.ToHexString());
        Assert.NotEqual(test1.GetBytes(), test2.GetBytes());
        Assert.NotEqual(test1, test2);
        Assert.NotEqual(test2, test1);
        Assert.NotEqual(test1.GetString(), test2.GetString());
    }

    [Fact]
    public void TestEmptyStringWithBOM()
    {
        Assert.True(COSString.ParseHex("FEFF").GetString().Length == 0);
        Assert.True(COSString.ParseHex("FFFE").GetString().Length == 0);
    }

    private static void WritePDFTests(string expected, COSString testSubj)
    {
        using MemoryStream outStream = new();
        testSubj.WritePDF(outStream);
        Assert.Equal(expected, Encoding.Latin1.GetString(outStream.ToArray()));
    }

    private static string CreateHex(string str)
    {
        StringBuilder sb = new();
        foreach (char c in str)
        {
            sb.Append(((int)c).ToString("X"));
        }

        return sb.ToString();
    }
}
