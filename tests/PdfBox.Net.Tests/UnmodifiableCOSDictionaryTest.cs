/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Converted from Apache PDFBox test source with focused xUnit adaptations.
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/cos/UnmodifiableCOSDictionaryTest.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.Tests;

public class UnmodifiableCOSDictionaryTest
{
    [Fact]
    public void TestUnmodifiableCOSDictionary()
    {
        COSDictionary unmodifiableCOSDictionary = new COSDictionary().AsUnmodifiableDictionary();

        Assert.Throws<InvalidOperationException>(() => unmodifiableCOSDictionary.Clear());
        Assert.Throws<InvalidOperationException>(() => unmodifiableCOSDictionary.RemoveItem(COSName.A));
        Assert.Throws<InvalidOperationException>(() => unmodifiableCOSDictionary.AddAll(new COSDictionary()));
        Assert.Throws<InvalidOperationException>(() => unmodifiableCOSDictionary.SetFlag(COSName.A, 0, true));
        Assert.Throws<InvalidOperationException>(() => unmodifiableCOSDictionary.SetNeedToBeUpdated(true));
    }

    [Fact]
    public void TestSetItem()
    {
        COSDictionary unmodifiableCOSDictionary = new COSDictionary().AsUnmodifiableDictionary();
        Assert.Throws<InvalidOperationException>(() => unmodifiableCOSDictionary.SetItem(COSName.A, COSName.A));
        Assert.Throws<InvalidOperationException>(() => unmodifiableCOSDictionary.SetItem("A", COSName.A));
    }

    [Fact]
    public void TestSetBoolean()
    {
        COSDictionary unmodifiableCOSDictionary = new COSDictionary().AsUnmodifiableDictionary();
        Assert.Throws<InvalidOperationException>(() => unmodifiableCOSDictionary.SetBoolean(COSName.A, true));
        Assert.Throws<InvalidOperationException>(() => unmodifiableCOSDictionary.SetBoolean("A", true));
    }

    [Fact]
    public void TestSetName()
    {
        COSDictionary unmodifiableCOSDictionary = new COSDictionary().AsUnmodifiableDictionary();
        Assert.Throws<InvalidOperationException>(() => unmodifiableCOSDictionary.SetName(COSName.A, "A"));
        Assert.Throws<InvalidOperationException>(() => unmodifiableCOSDictionary.SetName("A", "A"));
    }

    [Fact]
    public void TestSetDate()
    {
        COSDictionary unmodifiableCOSDictionary = new COSDictionary().AsUnmodifiableDictionary();
        DateTimeOffset date = DateTimeOffset.UtcNow;
        Assert.Throws<InvalidOperationException>(() => unmodifiableCOSDictionary.SetDate(COSName.A, date));
        Assert.Throws<InvalidOperationException>(() => unmodifiableCOSDictionary.SetDate("A", date));
    }

    [Fact]
    public void TestSetEmbeddedDate()
    {
        COSDictionary unmodifiableCOSDictionary = new COSDictionary().AsUnmodifiableDictionary();
        COSName paramsKey = COSName.GetPDFName("Params");
        DateTimeOffset date = DateTimeOffset.UtcNow;
        Assert.Throws<InvalidOperationException>(() => unmodifiableCOSDictionary.SetEmbeddedDate(paramsKey, COSName.A, date));
    }

    [Fact]
    public void TestSetString()
    {
        COSDictionary unmodifiableCOSDictionary = new COSDictionary().AsUnmodifiableDictionary();
        Assert.Throws<InvalidOperationException>(() => unmodifiableCOSDictionary.SetString(COSName.A, "A"));
        Assert.Throws<InvalidOperationException>(() => unmodifiableCOSDictionary.SetString("A", "A"));
    }

    [Fact]
    public void TestSetEmbeddedString()
    {
        COSDictionary unmodifiableCOSDictionary = new COSDictionary().AsUnmodifiableDictionary();
        COSName paramsKey = COSName.GetPDFName("Params");
        Assert.Throws<InvalidOperationException>(() => unmodifiableCOSDictionary.SetEmbeddedString(paramsKey, COSName.A, "A"));
    }

    [Fact]
    public void TestSetInt()
    {
        COSDictionary unmodifiableCOSDictionary = new COSDictionary().AsUnmodifiableDictionary();
        Assert.Throws<InvalidOperationException>(() => unmodifiableCOSDictionary.SetInt(COSName.A, 0));
        Assert.Throws<InvalidOperationException>(() => unmodifiableCOSDictionary.SetInt("A", 0));
    }

    [Fact]
    public void TestSetEmbeddedInt()
    {
        COSDictionary unmodifiableCOSDictionary = new COSDictionary().AsUnmodifiableDictionary();
        COSName paramsKey = COSName.GetPDFName("Params");
        Assert.Throws<InvalidOperationException>(() => unmodifiableCOSDictionary.SetEmbeddedInt(paramsKey, COSName.A, 0));
    }

    [Fact]
    public void TestSetLong()
    {
        COSDictionary unmodifiableCOSDictionary = new COSDictionary().AsUnmodifiableDictionary();
        Assert.Throws<InvalidOperationException>(() => unmodifiableCOSDictionary.SetLong(COSName.A, 0));
        Assert.Throws<InvalidOperationException>(() => unmodifiableCOSDictionary.SetLong("A", 0));
    }

    [Fact]
    public void TestSetFloat()
    {
        COSDictionary unmodifiableCOSDictionary = new COSDictionary().AsUnmodifiableDictionary();
        Assert.Throws<InvalidOperationException>(() => unmodifiableCOSDictionary.SetFloat(COSName.A, 0));
        Assert.Throws<InvalidOperationException>(() => unmodifiableCOSDictionary.SetFloat("A", 0));
    }
}
