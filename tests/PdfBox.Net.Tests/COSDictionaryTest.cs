/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Converted from Apache PDFBox test source with focused xUnit adaptations.
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/cos/COSDictionaryTest.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PdfWriter;
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

    [Fact]
    public void JavaCompatibilityOverloadsDelegateToDictionaryValues()
    {
        COSDictionary dictionary = new();
        COSName first = COSName.GetPDFName("First");
        COSName second = COSName.GetPDFName("Second");
        COSName flag = COSName.GetPDFName("Flag");
        COSName date = COSName.GetPDFName("Date");

        dictionary.SetInt(first, 7);
        dictionary.SetLong(second, 12);
        dictionary.SetBoolean(flag, true);
        DateTimeOffset fallbackDate = new(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);

        Assert.Equal(7, dictionary.GetInt(first));
        Assert.Equal(7, dictionary.GetInt("First"));
        Assert.Equal(7, dictionary.GetInt(COSName.GetPDFName("Missing"), first, -1));
        Assert.Equal(12, dictionary.GetLong(second));
        Assert.Equal(12, dictionary.GetLong("Second"));
        Assert.Equal(-1f, dictionary.GetFloat(COSName.GetPDFName("Missing")));
        Assert.True(dictionary.GetBoolean(COSName.GetPDFName("Missing"), flag, false));
        Assert.Equal(fallbackDate, dictionary.GetDate(date, fallbackDate));
        Assert.Same(dictionary.GetItem(first), dictionary.GetItem(COSName.GetPDFName("Missing"), first));

        List<COSName> keys = [];
        dictionary.ForEach((key, _) => keys.Add(key));
        Assert.Contains(first, keys);
        Assert.Contains(second, keys);
        Assert.Contains(flag, keys);
    }

    [Fact]
    public void EntrySetAndSerializationPreserveInsertionOrder()
    {
        COSDictionary dictionary = new();
        COSName second = COSName.GetPDFName("Second");
        COSName first = COSName.GetPDFName("First");
        COSName third = COSName.GetPDFName("Third");

        dictionary.SetInt(second, 2);
        dictionary.SetInt(first, 1);
        dictionary.SetInt(third, 3);
        dictionary.SetInt(first, 11);

        Assert.Equal(
            ["Second", "First", "Third"],
            dictionary.EntrySet().Select(entry => entry.Key.GetName()).ToArray());

        string serialized = COSWriter.SerializeToString(dictionary);
        Assert.True(serialized.IndexOf("/Second", StringComparison.Ordinal) < serialized.IndexOf("/First", StringComparison.Ordinal));
        Assert.True(serialized.IndexOf("/First", StringComparison.Ordinal) < serialized.IndexOf("/Third", StringComparison.Ordinal));
    }

    [Fact]
    public void ResetImportedObjectKeysClearsNestedKeysButSkipsParentLinks()
    {
        COSDictionary root = new();
        COSDictionary child = new();
        COSDictionary parent = new();
        COSArray array = new();
        COSDictionary arrayChild = new();

        root.SetKey(new COSObjectKey(1, 0));
        child.SetKey(new COSObjectKey(2, 0));
        parent.SetKey(new COSObjectKey(3, 0));
        arrayChild.SetKey(new COSObjectKey(4, 0));
        array.SetKey(new COSObjectKey(5, 0));

        array.Add(arrayChild);
        root.SetItem(COSName.GetPDFName("Child"), child);
        root.SetItem(COSName.PARENT, parent);
        root.SetItem(COSName.GetPDFName("Array"), array);

        root.ResetImportedObjectKeys();

        Assert.Null(root.GetKey());
        Assert.Null(child.GetKey());
        Assert.Null(array.GetKey());
        Assert.Null(arrayChild.GetKey());
        Assert.NotNull(parent.GetKey());
    }

}
