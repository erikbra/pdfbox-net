/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Converted from Apache PDFBox test source with focused xUnit adaptations.
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/cos/TestCOSArray.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

/*
 * Copyright 2018 The Apache Software Foundation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 */

using PdfBox.Net.COS;
using Xunit;

namespace PdfBox.Net.Tests;

public class TestCOSArray
{
    [Fact]
    public void TestCreate()
    {
        COSArray cosArray = new();
        Assert.Equal(0, cosArray.Size());
        Assert.Throws<NullReferenceException>(() => _ = new COSArray((IEnumerable<COSObjectable?>)null!));

        cosArray = COSArray.OfCOSNames([COSName.A.GetName(), COSName.B.GetName(), COSName.C.GetName()]);
        Assert.Equal(3, cosArray.Size());
        Assert.Equal(COSName.A, cosArray.Get(0));
        Assert.Equal(COSName.B, cosArray.Get(1));
        Assert.Equal(COSName.C, cosArray.Get(2));
    }

    [Fact]
    public void TestConvertString2COSNameAndBack()
    {
        COSArray cosArray = COSArray.OfCOSNames([COSName.A.GetName(), COSName.B.GetName(), COSName.C.GetName()]);
        Assert.Equal(3, cosArray.Size());
        Assert.Equal(COSName.A, cosArray.Get(0));
        Assert.Equal(COSName.B, cosArray.Get(1));
        Assert.Equal(COSName.C, cosArray.Get(2));

        List<string> cosNameStringList = cosArray.ToCOSNameStringList();
        Assert.Equal(3, cosNameStringList.Count);
        Assert.Equal(COSName.A.GetName(), cosNameStringList[0]);
        Assert.Equal(COSName.B.GetName(), cosNameStringList[1]);
        Assert.Equal(COSName.C.GetName(), cosNameStringList[2]);
    }

    [Fact]
    public void TestConvertString2COSStringAndBack()
    {
        COSArray cosArray = COSArray.OfCOSStrings(["A", "B", "C"]);
        Assert.Equal(3, cosArray.Size());
        Assert.Equal("A", cosArray.GetString(0));
        Assert.Equal("B", cosArray.GetString(1));
        Assert.Equal("C", cosArray.GetString(2));

        List<string> cosStringStringList = cosArray.ToCOSStringStringList();
        Assert.Equal(3, cosStringStringList.Count);
        Assert.Equal("A", cosStringStringList[0]);
        Assert.Equal("B", cosStringStringList[1]);
        Assert.Equal("C", cosStringStringList[2]);
    }

    [Fact]
    public void TestConvertInteger2COSStringAndBack()
    {
        COSArray cosArray = COSArray.OfCOSIntegers([1, 2, 3]);
        Assert.Equal(3, cosArray.Size());
        Assert.Equal(1, cosArray.GetInt(0));
        Assert.Equal(2, cosArray.GetInt(1));
        Assert.Equal(3, cosArray.GetInt(2));

        List<int?> cosNumberIntegerList = cosArray.ToCOSNumberIntegerList();
        Assert.Equal(3, cosNumberIntegerList.Count);
        Assert.Equal(1, cosNumberIntegerList[0]);
        Assert.Equal(2, cosNumberIntegerList[1]);
        Assert.Equal(3, cosNumberIntegerList[2]);

        cosArray = new COSArray([COSInteger.Get(1), null, COSInteger.Get(3)]);
        Assert.Equal(3, cosArray.Size());
        Assert.Equal(1, cosArray.GetInt(0));
        Assert.Null(cosArray.Get(1));
        Assert.Equal(3, cosArray.GetInt(2));
        cosNumberIntegerList = cosArray.ToCOSNumberIntegerList();
        Assert.Equal(3, cosNumberIntegerList.Count);
        Assert.Equal(1, cosNumberIntegerList[0]);
        Assert.Null(cosNumberIntegerList[1]);
        Assert.Equal(3, cosNumberIntegerList[2]);
    }

    [Fact]
    public void TestConvertFloat2COSStringAndBack()
    {
        float[] floatArrayStart = [1f, 0.1f, 0.02f];
        COSArray cosArray = new();
        cosArray.SetFloatArray(floatArrayStart);

        Assert.Equal(3, cosArray.Size());
        Assert.Equal(COSFloat.ONE, cosArray.Get(0));
        Assert.Equal(new COSFloat(0.1f), cosArray.Get(1));
        Assert.Equal(new COSFloat(0.02f), cosArray.Get(2));

        List<float?> cosNumberFloatList = cosArray.ToCOSNumberFloatList();
        Assert.Equal(3, cosNumberFloatList.Count);
        Assert.Equal(1f, cosNumberFloatList[0]);
        Assert.Equal(0.1f, cosNumberFloatList[1]);
        Assert.Equal(0.02f, cosNumberFloatList[2]);

        float[] floatArrayEnd = cosArray.ToFloatArray();
        Assert.Equal(floatArrayStart, floatArrayEnd);

        cosArray = new COSArray([COSFloat.ONE, null, new COSFloat(0.02f)]);
        Assert.Equal(3, cosArray.Size());
        Assert.Equal(COSFloat.ONE, cosArray.Get(0));
        Assert.Null(cosArray.Get(1));
        Assert.Equal(new COSFloat(0.02f), cosArray.Get(2));

        cosNumberFloatList = cosArray.ToCOSNumberFloatList();
        Assert.Equal(3, cosNumberFloatList.Count);
        Assert.Equal(1f, cosNumberFloatList[0]);
        Assert.Null(cosNumberFloatList[1]);
        Assert.Equal(0.02f, cosNumberFloatList[2]);

        floatArrayEnd = cosArray.ToFloatArray();
        Assert.Equal([1f, 0f, 0.02f], floatArrayEnd);
    }

    [Fact]
    public void TestGetSetName()
    {
        COSArray cosArray = new();
        cosArray.GrowToSize(3);
        cosArray.SetName(0, "A");
        cosArray.SetName(1, "B");
        cosArray.SetName(2, "C");
        Assert.Equal(3, cosArray.Size());
        Assert.Equal("A", cosArray.GetName(0));
        Assert.Equal("B", cosArray.GetName(1));
        Assert.Equal("C", cosArray.GetName(2));
        Assert.Equal("NULL", cosArray.GetName(3, "NULL"));
        Assert.Equal(0, cosArray.IndexOf(COSName.A));
        Assert.Equal(1, cosArray.IndexOf(COSName.B));
        Assert.Equal(2, cosArray.IndexOf(COSName.C));
        Assert.Equal(-1, cosArray.IndexOf(COSName.D));
        cosArray.SetName(1, "D");
        Assert.Equal(3, cosArray.Size());
        Assert.Equal("D", cosArray.GetName(1));
    }

    [Fact]
    public void TestGetSetInt()
    {
        COSArray cosArray = new();
        cosArray.GrowToSize(3);
        cosArray.SetInt(0, 0);
        cosArray.SetInt(1, 1);
        cosArray.SetInt(2, 2);
        Assert.Equal(3, cosArray.Size());
        Assert.Equal(0, cosArray.GetInt(0));
        Assert.Equal(1, cosArray.GetInt(1));
        Assert.Equal(2, cosArray.GetInt(2));
        Assert.Equal(0, cosArray.GetInt(3, 0));
        Assert.Equal(0, cosArray.IndexOf(COSInteger.Get(0)));
        Assert.Equal(1, cosArray.IndexOf(COSInteger.Get(1)));
        Assert.Equal(2, cosArray.IndexOf(COSInteger.Get(2)));
        Assert.Equal(-1, cosArray.IndexOf(COSInteger.Get(3)));
        cosArray.SetInt(1, 3);
        Assert.Equal(3, cosArray.Size());
        Assert.Equal(3, cosArray.GetInt(1));
    }

    [Fact]
    public void TestGetSetString()
    {
        COSArray cosArray = new();
        cosArray.GrowToSize(3);
        cosArray.SetString(0, "Test1");
        cosArray.SetString(1, "Test2");
        cosArray.SetString(2, "Test3");
        Assert.Equal(3, cosArray.Size());
        Assert.Equal("Test1", cosArray.GetString(0));
        Assert.Equal("Test2", cosArray.GetString(1));
        Assert.Equal("Test3", cosArray.GetString(2));
        Assert.Equal("NULL", cosArray.GetString(3, "NULL"));
        Assert.Equal(0, cosArray.IndexOf(new COSString("Test1")));
        Assert.Equal(1, cosArray.IndexOf(new COSString("Test2")));
        Assert.Equal(2, cosArray.IndexOf(new COSString("Test3")));
        Assert.Equal(-1, cosArray.IndexOf(new COSString("Test4")));
        cosArray.SetString(1, "Test4");
        Assert.Equal(3, cosArray.Size());
        Assert.Equal("Test4", cosArray.GetString(1));
    }

    [Fact]
    public void TestRemove()
    {
        COSArray cosArray = COSArray.OfCOSIntegers([1, 2, 3, 4, 5, 6]);
        cosArray.Clear();
        Assert.Equal(0, cosArray.Size());

        cosArray = COSArray.OfCOSIntegers([1, 2, 3, 4, 5, 6]);
        Assert.Equal(COSInteger.Get(3), cosArray.Remove(2));
        Assert.Equal(5, cosArray.Size());
        Assert.Equal(1, cosArray.GetInt(0));
        Assert.Equal(4, cosArray.GetInt(2));

        Assert.True(cosArray.RemoveObject(COSInteger.Get(5)));
        Assert.Equal(4, cosArray.Size());
        Assert.Equal(1, cosArray.GetInt(0));
        Assert.Equal(4, cosArray.GetInt(2));
        Assert.Equal(6, cosArray.GetInt(3));

        cosArray = COSArray.OfCOSIntegers([1, 2, 3, 4, 5, 6]);
        cosArray.RemoveAll([COSInteger.Get(3), COSInteger.Get(4)]);
        Assert.Equal(4, cosArray.Size());
        Assert.Equal(2, cosArray.GetInt(1));
        Assert.Equal(5, cosArray.GetInt(2));

        cosArray = COSArray.OfCOSIntegers([1, 2, 3, 4, 5, 6]);
        cosArray.RetainAll([COSInteger.Get(3), COSInteger.Get(4)]);
        Assert.Equal(2, cosArray.Size());
        Assert.Equal(3, cosArray.GetInt(0));
        Assert.Equal(4, cosArray.GetInt(1));
    }

    [Fact]
    public void TestGrowToSize()
    {
        COSArray cosArray = new();
        Assert.Equal(0, cosArray.Size());
        cosArray.GrowToSize(2);
        Assert.Equal(2, cosArray.Size());
        cosArray.GrowToSize(2, COSInteger.Get(0));
        Assert.Equal(2, cosArray.Size());
        cosArray.GrowToSize(4, COSInteger.Get(1));
        Assert.Equal(4, cosArray.Size());
        List<int?> cosNumberIntegerList = cosArray.ToCOSNumberIntegerList();
        Assert.Equal(4, cosNumberIntegerList.Count);
        Assert.Null(cosNumberIntegerList[0]);
        Assert.Equal(1, cosNumberIntegerList[2]);
        Assert.Equal(1, cosNumberIntegerList[3]);
    }

    [Fact]
    public void TestToList()
    {
        COSArray cosArray = COSArray.OfCOSIntegers([0, 1, 2, 3, 4, 5]);
        List<COSBase?> list = cosArray.ToList();
        Assert.Equal(6, list.Count);
        Assert.Equal(COSInteger.Get(0), list[0]);
        Assert.Equal(COSInteger.Get(5), list[5]);
    }
}
