using PdfBox.Net.Filter;

namespace PdfBox.Net.Tests;

public class DecodeRegionTest
{
    [Fact]
    public void IntersectReturnsOverlappingRegion()
    {
        DecodeRegion region = new(2, 3, 8, 10);

        DecodeRegion intersection = region.Intersect(new DecodeRegion(5, 7, 10, 10));

        Assert.Equal(new DecodeRegion(5, 7, 5, 6), intersection);
        Assert.Equal(5, intersection.Left);
        Assert.Equal(7, intersection.Top);
        Assert.Equal(10, intersection.Right);
        Assert.Equal(13, intersection.Bottom);
        Assert.False(intersection.IsEmpty);
    }

    [Fact]
    public void IntersectReturnsEmptyWhenRegionsDoNotOverlap()
    {
        DecodeRegion region = new(0, 0, 10, 10);

        DecodeRegion intersection = region.Intersect(new DecodeRegion(10, 10, 5, 5));

        Assert.Equal(DecodeRegion.Empty, intersection);
        Assert.True(intersection.IsEmpty);
    }
}
