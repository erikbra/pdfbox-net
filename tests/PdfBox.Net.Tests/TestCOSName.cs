/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Converted from Apache PDFBox test source with focused xUnit adaptations.
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/cos/TestCOSName.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using System.Text;
using PdfBox.Net.COS;
using Xunit;

namespace PdfBox.Net.Tests;

public class TestCOSName
{
    [Fact]
    public void PDFBox4076()
    {
        string special = "中国你好!";
        COSDictionary dictionary = new();
        COSName key = COSName.GetPDFName(special);
        dictionary.SetString(key, special);

        Assert.True(dictionary.ContainsKey(special));
        Assert.Equal(special, dictionary.GetString(special));
    }

    [Fact]
    public void PDFBox6178()
    {
        COSName name = COSName.GetPDFName([(byte)'m', 0xE4, (byte)'n', (byte)'n', (byte)'l', (byte)'i', (byte)'c', (byte)'h']);
        using MemoryStream ms = new();
        name.WritePDF(ms);
        string writtenKey = Encoding.UTF8.GetString(ms.ToArray());
        Assert.Equal("/m#E4nnlich", writtenKey);
    }

    [Fact]
    public void NameWithASCII_NUL()
    {
        COSName name = COSName.GetPDFName([(byte)'m', 0x00, (byte)'n', (byte)'n', (byte)'l', (byte)'i', (byte)'c', (byte)'h']);
        using MemoryStream ms = new();
        name.WritePDF(ms);
        string writtenKey = Encoding.UTF8.GetString(ms.ToArray());
        Assert.Equal("/m#00nnlich", writtenKey);
    }
}
