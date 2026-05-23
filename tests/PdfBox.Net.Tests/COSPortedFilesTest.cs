using PdfBox.Net.COS;
using PdfBox.Net.Filter;
using PdfBox.Net.IO;
using RandomAccessBuffer = PdfBox.Net.IO.RandomAccess;
using System.Drawing;

namespace PdfBox.Net.Tests;

public class COSPortedFilesTest
{
    [Fact]
    public void TestCOSInputStreamCreateWithoutFilters()
    {
        byte[] expected = [1, 2, 3, 4];
        using MemoryStream encoded = new(expected, writable: false);
        using COSInputStream input = COSInputStream.Create([], new COSDictionary(), encoded);
        using MemoryStream output = new();

        input.CopyTo(output);

        Assert.Equal(expected, output.ToArray());
        DecodeResult first = input.GetDecodeResult();
        DecodeResult second = input.GetDecodeResult();
        Assert.NotSame(first, second);
        Assert.Equal(0, first.GetParameters().Size());
    }

    [Fact]
    public void TestCOSOutputStreamWithoutFiltersWritesDirectly()
    {
        using MemoryStream destination = new();
        using COSOutputStream output = new([], new COSDictionary(), destination, new TestStreamCache());

        output.Write([10, 11, 12], 0, 3);
        output.Close();

        Assert.Equal([10, 11, 12], destination.ToArray());
    }

    [Fact]
    public void TestUnmodifiableCOSDictionaryThrowsOnMutation()
    {
        UnmodifiableCOSDictionary dictionary = new(new COSDictionary());

        Assert.Throws<InvalidOperationException>(() => dictionary.SetInt(COSName.LENGTH, 1));
        Assert.Throws<InvalidOperationException>(() => dictionary.RemoveItem(COSName.LENGTH));
        Assert.Throws<InvalidOperationException>(() => dictionary.Clear());
    }

    [Fact]
    public void TestDecodeOptionsDefaultIsImmutable()
    {
        DecodeOptions options = DecodeOptions.DEFAULT;
        Assert.True(options.IsFilterSubsampled());
        Assert.Throws<InvalidOperationException>(() => options.SetSubsamplingX(2));
        Assert.Throws<InvalidOperationException>(() => options.SetSourceRegion(new Rectangle(0, 0, 1, 1)));
    }

    [Fact]
    public void TestDecodeOptionsSubsamplingConstructorSetsBothAxes()
    {
        DecodeOptions options = new(3);

        Assert.Equal(3, options.GetSubsamplingX());
        Assert.Equal(3, options.GetSubsamplingY());
        Assert.False(options.IsFilterSubsampled());
    }

    private sealed class TestStreamCache : RandomAccessStreamCache
    {
        public RandomAccessBuffer CreateBuffer()
        {
            return new RandomAccessReadWriteBuffer();
        }

        public void Close()
        {
        }

        public void Dispose()
        {
            Close();
        }
    }
}
