/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Converted from Apache PDFBox test source with focused xUnit adaptations.
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/cos/PDFDocEncodingTest.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.Tests;

public class PDFDocEncodingTest
{
    private static readonly List<string> Deviations =
    [
        "\u02D8", "\u02C7", "\u02C6", "\u02D9", "\u02DD", "\u02DB", "\u02DA", "\u02DC",
        "\u2022", "\u2020", "\u2021", "\u2026", "\u2014", "\u2013", "\u0192", "\u2044",
        "\u2039", "\u203A", "\u2212", "\u2030", "\u201E", "\u201C", "\u201D", "\u2018",
        "\u2019", "\u201A", "\u2122", "\uFB01", "\uFB02", "\u0141", "\u0152", "\u0160",
        "\u0178", "\u017D", "\u0131", "\u0142", "\u0153", "\u0161", "\u017E", "\u20AC"
    ];

    [Fact]
    public void TestDeviations()
    {
        foreach (string deviation in Deviations)
        {
            COSString cosString = new(deviation);
            Assert.Equal(deviation, cosString.GetString());
        }
    }

    [Fact]
    public void TestPDFBox3864()
    {
        for (int i = 0; i < 256; i++)
        {
            string hex = $"FEFF{i:X4}";
            COSString cs1 = COSString.ParseHex(hex);
            COSString cs2 = new(cs1.GetString());
            Assert.Equal(cs1, cs2);
        }
    }
}
