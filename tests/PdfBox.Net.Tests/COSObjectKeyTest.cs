/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Converted from Apache PDFBox test source with focused xUnit adaptations.
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/cos/COSObjectKeyTest.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using PdfBox.Net.COS;

namespace PdfBox.Net.Tests;

public class COSObjectKeyTest
{
    [Fact]
    public void TestInputValues()
    {
        Assert.Throws<ArgumentException>(() => _ = new COSObjectKey(-1L, 0));
        Assert.Throws<ArgumentException>(() => _ = new COSObjectKey(1L, -1));
    }

    [Fact]
    public void CompareToInputNotNullOutputZero()
    {
        COSObjectKey objectUnderTest = new(1L, 0);
        COSObjectKey other = new(1L, 0);

        int retval = objectUnderTest.CompareTo(other);

        Assert.Equal(0, retval);
    }

    [Fact]
    public void CompareToInputNotNullOutputNotNull()
    {
        COSObjectKey objectUnderTest = new(1L, 0);
        COSObjectKey other = new(9_999_999L, 0);

        int retvalNegative = objectUnderTest.CompareTo(other);
        int retvalPositive = other.CompareTo(objectUnderTest);

        Assert.Equal(-1, retvalNegative);
        Assert.Equal(1, retvalPositive);
    }

    [Fact]
    public void TestEquals()
    {
        Assert.Equal(new COSObjectKey(100, 0), new COSObjectKey(100, 0));
        Assert.NotEqual(new COSObjectKey(100, 0), new COSObjectKey(101, 0));
    }

    [Fact]
    public void TestInternalRepresentation()
    {
        COSObjectKey key = new(100, 0);
        Assert.Equal(100, key.GetNumber());
        Assert.Equal(0, key.GetGeneration());

        key = new COSObjectKey(200, 4);
        Assert.Equal(200, key.GetNumber());
        Assert.Equal(4, key.GetGeneration());

        key = new COSObjectKey(200000, 0);
        Assert.Equal(200000, key.GetNumber());
        Assert.Equal(0, key.GetGeneration());

        key = new COSObjectKey(87654321, 123);
        Assert.Equal(87654321, key.GetNumber());
        Assert.Equal(123, key.GetGeneration());
    }

    [Fact]
    public void TestSortingOrder()
    {
        COSObjectKey key40 = new(4, 0);
        COSObjectKey key41 = new(4, 1);
        COSObjectKey key50 = new(5, 0);

        Assert.Equal(0, key40.CompareTo(key40));
        Assert.Equal(0, key41.CompareTo(key41));
        Assert.Equal(-1, key40.CompareTo(key41));
        Assert.Equal(-1, key40.CompareTo(key50));
        Assert.Equal(-1, key41.CompareTo(key50));
    }

    [Fact]
    public void CheckHashCode()
    {
        Assert.Equal(new COSObjectKey(100, 0).GetHashCode(), new COSObjectKey(100, 0).GetHashCode());
        Assert.NotEqual(new COSObjectKey(100, 0).GetHashCode(), new COSObjectKey(200, 0).GetHashCode());
        Assert.NotEqual(new COSObjectKey(100, 0).GetHashCode(), new COSObjectKey(99, 1).GetHashCode());
    }
}
