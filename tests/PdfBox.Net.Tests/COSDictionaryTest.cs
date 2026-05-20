/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Converted from Apache PDFBox test source with focused xUnit adaptations.
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/cos/COSDictionaryTest.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;
using Xunit;

namespace PdfBox.Net.Tests;

public class COSDictionaryTest
{
    [Fact]
    public void TestCOSDictionaryNotEqualsCOSStream()
    {
        COSDictionary cosDictionary = new();
        COSStream cosStream = new();
        COSName key = COSName.GetPDFName("BE");
        cosDictionary.SetItem(key, key);
        cosDictionary.SetInt(COSName.LENGTH, 0);
        cosStream.SetItem(key, key);
        Assert.NotEqual(cosDictionary, cosStream);
        Assert.NotEqual(cosStream, cosDictionary);
    }
}
