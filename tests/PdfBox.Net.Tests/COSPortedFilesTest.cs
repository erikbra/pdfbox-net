using PdfBox.Net.COS;
using PdfBox.Net.Filter;
using PdfBox.Net.IO;
using RandomAccessBuffer = PdfBox.Net.IO.RandomAccess;

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
        Assert.Same(DecodeResult.CreateDefault(), input.GetDecodeResult());
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
