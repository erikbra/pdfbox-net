using PdfBox.Net.FontBox.Encoding;
using PdfBox.Net.FontBox.Pfb;
using PdfBox.Net.FontBox.Type1;

namespace PdfBox.Net.FontBox.Tests;

public class PfbAndType1FontTest
{
    [Fact]
    public void TestMinimalPfbFontParses()
    {
        Type1Font font = Type1Font.CreateWithPFB(FontBoxTestFixtures.CreateMinimalType1Pfb());
        Assert.Equal("1.0", font.GetVersion());
        Assert.Equal("TestFont", font.GetFontName());
        Assert.Equal("Test Font", font.GetFullName());
        Assert.Equal("Test Family", font.GetFamilyName());
        Assert.Equal("Test notice", font.GetNotice());
        Assert.False(font.IsFixedPitch());
        Assert.False(font.IsForceBold());
        Assert.Equal(0, font.GetItalicAngle());
        Assert.Equal("Regular", font.GetWeight());
        Assert.IsType<BuiltInEncoding>(font.GetEncoding());
        Assert.Equal("A", font.GetEncoding().GetName(65));
        Assert.Equal(2, font.GetCharStringsDict().Count);
        Assert.True(font.HasGlyph("A"));
        Assert.NotNull(font.GetPath("A"));
        Assert.Single(font.GetSubrsArray());
        Assert.Equal(500, font.GetFontBBox().GetUpperRightX());
    }

    [Fact]
    public void TestNegativeRecordSize()
    {
        byte[] crashInput =
        [
            0x80, 0x01,
            0x01, 0x00, 0x00, 0xFF,
            0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF,
            0x27, 0x05, 0xF8, 0xFF,
            0xD2, 0x40,
        ];
        Assert.Throws<IOException>(() => new PfbParser(crashInput));
    }

    [Fact]
    public void TestEmpty()
    {
        IOException exception = Assert.Throws<IOException>(() => Type1Font.CreateWithPFB([]));
        Assert.Equal("Start marker missing", exception.Message);
    }

    [Fact]
    public void ParseStream_ReadsShortChunksAndStopsAtEofMarkerWithoutClosingInput()
    {
        byte[] fontBytes = FontBoxTestFixtures.CreateMinimalType1Pfb();
        using ChunkedInput input = new([.. fontBytes, 0x55, 0x66]);

        PfbParser parser = new(input);

        Assert.Equal(new PfbParser(fontBytes).GetPfbdata(), parser.GetPfbdata());
        Assert.Equal(fontBytes.Length, input.BytesRead);
        Assert.False(input.Closed);
        Assert.Equal(0x55, input.ReadByte());
    }

    [Fact]
    public void ParseStream_RejectsHugeTruncatedRecordWithoutRequestingHugeBuffer()
    {
        using ChunkedInput input = new([0x80, 0x01, 0xff, 0xff, 0xff, 0x7f, 0x42]);
        EndOfStreamException exception = Assert.Throws<EndOfStreamException>(() => new PfbParser(input));
        Assert.Equal("EOF while reading PFB font", exception.Message);
        Assert.InRange(input.LargestRead, 1, 8192);
        Assert.False(input.Closed);
    }

    [Fact]
    public void ParseStream_RejectsOneShortSegment()
    {
        IOException exception = Assert.Throws<IOException>(() => new PfbParser([0x80, 0x01, 3, 0, 0, 0, 1, 2, 3]));
        Assert.Equal("PFB header missing", exception.Message);
    }

    private sealed class ChunkedInput(byte[] bytes) : Stream
    {
        private readonly MemoryStream _input = new(bytes, writable: false);
        public int BytesRead { get; private set; }
        public int LargestRead { get; private set; }
        public bool Closed { get; private set; }
        public override bool CanRead => !Closed;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override int Read(byte[] buffer, int offset, int count)
        {
            LargestRead = Math.Max(LargestRead, count);
            int read = _input.Read(buffer, offset, Math.Min(count, 3));
            BytesRead += read;
            return read;
        }
        public override int ReadByte()
        {
            int value = _input.ReadByte();
            if (value >= 0) BytesRead++;
            return value;
        }
        protected override void Dispose(bool disposing)
        {
            Closed = true;
            if (disposing) _input.Dispose();
            base.Dispose(disposing);
        }
        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
