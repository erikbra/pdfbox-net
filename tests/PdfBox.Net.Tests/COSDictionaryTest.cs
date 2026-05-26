/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Converted from Apache PDFBox test source with focused xUnit adaptations.
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/cos/COSDictionaryTest.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
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


    [Fact]
    public void COSDictionaryMapSynchronizesDictionaryMutations()
    {
        COSDictionary dictionary = new();
        Dictionary<string, PDRectangle> actuals = new(StringComparer.Ordinal)
        {
            ["MediaBox"] = new PDRectangle(100f, 200f)
        };
        COSDictionaryMap<string, PDRectangle> map = new(actuals, dictionary);

        map["CropBox"] = new PDRectangle(10f, 20f);

        Assert.True(dictionary.ContainsKey(COSName.GetPDFName("CropBox")));
        Assert.Equal(2, map.Count);

        map.Remove("MediaBox");
        Assert.False(map.ContainsKey("MediaBox"));
    }

    [Fact]
    public void COSDictionaryMapConvertsBasicTypes()
    {
        COSDictionary dictionary = new();
        dictionary.SetString("Title", "hello");
        dictionary.SetInt("Count", 7);
        dictionary.SetName(COSName.TYPE, "Catalog");
        dictionary.SetItem(COSName.GetPDFName("Flag"), COSBoolean.TRUE);

        COSDictionaryMap<string, object>? converted = COSDictionaryMap<string, object>.ConvertBasicTypesToMap(dictionary);

        Assert.NotNull(converted);
        Assert.Equal("hello", converted!["Title"]);
        Assert.Equal(7, converted["Count"]);
        Assert.Equal("Catalog", converted["Type"]);
        Assert.Equal(true, converted["Flag"]);
    }

}
