using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Graphics.Image;

namespace PdfBox.Net.Tests;

public sealed class JPEGFactoryStreamingTest
{
    [Fact]
    public void CreateFromStreamCopiesForwardOnlyInputAndLeavesItOpen()
    {
        byte[] jpeg = File.ReadAllBytes(Fixture("test-2x1-rgb.jpg"));
        using ChunkedStream input = new(jpeg);
        using PDDocument document = new();
        PDImageXObject image = JPEGFactory.CreateFromStream(document, input);

        Assert.Equal(2, image.GetWidth());
        Assert.Equal(1, image.GetHeight());
        Assert.Equal("DeviceRGB", image.GetColorSpace().GetName());
        Assert.Equal(jpeg.Length, input.BytesRead);
        Assert.True(input.CanRead);
        using Stream stored = image.GetCOSObject()!.CreateRawInputStream();
        using MemoryStream bytes = new();
        stored.CopyTo(bytes);
        Assert.Equal(jpeg, bytes.ToArray());
    }

    [Fact]
    public void CreateFromByteArrayPreservesRawDataAndPixelValues()
    {
        byte[] jpeg = File.ReadAllBytes(Fixture("test-2x1-gray.jpg"));
        using PDDocument document = new();
        PDImageXObject fromBytes = JPEGFactory.CreateFromByteArray(document, jpeg);
        using MemoryStream input = new(jpeg);
        PDImageXObject fromStream = JPEGFactory.CreateFromStream(document, input);
        Assert.Equal(fromStream.GetWidth(), fromBytes.GetWidth());
        Assert.Equal(fromStream.GetHeight(), fromBytes.GetHeight());
        Assert.Equal(SampledImageReader.GetRGBImage(fromStream), SampledImageReader.GetRGBImage(fromBytes));
        using Stream stored = fromBytes.GetCOSObject()!.CreateRawInputStream();
        using MemoryStream bytes = new();
        stored.CopyTo(bytes);
        Assert.Equal(jpeg, bytes.ToArray());
    }

    [Theory]
    [InlineData(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0, 0 })]
    [InlineData(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0, 1 })]
    [InlineData(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0, 5, 1 })]
    [InlineData(new byte[] { 0xFF, 0xD8, 0xFF, 0xC0, 0, 8, 8 })]
    public void CreateFromStreamRejectsMalformedOrTruncatedSegments(byte[] jpeg)
    {
        using PDDocument document = new();
        using ChunkedStream input = new(jpeg);
        Assert.ThrowsAny<IOException>(() => JPEGFactory.CreateFromStream(document, input));
        Assert.True(input.CanRead);
    }

    [Fact]
    public void CcittFactoryLeavesCallerInputOpen()
    {
        using FileStream input = File.OpenRead(Fixture("ccittg4.tif"));
        using PDDocument document = new();
        PDImageXObject image = CCITTFactory.CreateFromStream(document, input);
        Assert.True(image.GetWidth() > 0);
        Assert.True(input.CanRead);
    }

    private static string Fixture(string name) => Path.Combine(AppContext.BaseDirectory, "Fixtures", "Images", name);

    private sealed class ChunkedStream(byte[] bytes) : Stream
    {
        private bool _disposed;
        public int BytesRead { get; private set; }
        public override bool CanRead => !_disposed;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override int Read(byte[] buffer, int offset, int count)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            int length = Math.Min(3, Math.Min(count, bytes.Length - BytesRead));
            bytes.AsSpan(BytesRead, length).CopyTo(buffer.AsSpan(offset, length));
            BytesRead += length;
            return length;
        }
        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        protected override void Dispose(bool disposing)
        {
            _disposed = true;
            base.Dispose(disposing);
        }
    }
}
