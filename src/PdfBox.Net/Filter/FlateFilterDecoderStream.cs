/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/filter/FlateFilterDecoderStream.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using System.IO.Compression;

namespace PdfBox.Net.Filter;

public sealed class FlateFilterDecoderStream : Stream
{
    private readonly Stream _input;
    private readonly DeflateStream _inflater;

    public FlateFilterDecoderStream(Stream input)
    {
        _input = input ?? throw new ArgumentNullException(nameof(input));

        // PDF flate streams normally include a zlib wrapper header.
        _input.ReadByte();
        _input.ReadByte();
        _inflater = new DeflateStream(_input, CompressionMode.Decompress, leaveOpen: true);
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return _inflater.Read(buffer, offset, count);
    }

    public override int Read(Span<byte> buffer)
    {
        return _inflater.Read(buffer);
    }

    public override int ReadByte()
    {
        Span<byte> one = stackalloc byte[1];
        return _inflater.Read(one) == 0 ? -1 : one[0];
    }

    public override void Flush()
    {
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _inflater.Dispose();
            _input.Dispose();
        }

        base.Dispose(disposing);
    }
}
