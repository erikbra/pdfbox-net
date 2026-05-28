/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/digitalsignature/COSFilterInputStream.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

using System.Collections;

namespace PdfBox.Net.PDModel.Interactive.DigitalSignature;

public sealed class COSFilterInputStream : Stream
{
    private readonly Stream _inner;
    private readonly (int Begin, int End)[] _ranges;
    private int _rangeIndex = -1;
    private long _position;

    public COSFilterInputStream(Stream input, int[] byteRange)
    {
        _inner = input ?? throw new ArgumentNullException(nameof(input));
        if (byteRange is null || byteRange.Length == 0 || byteRange.Length % 2 != 0)
        {
            _ranges = [];
            return;
        }

        _ranges = Enumerable.Range(0, byteRange.Length / 2)
            .Select(i => (byteRange[i * 2], byteRange[i * 2] + byteRange[i * 2 + 1]))
            .ToArray();
    }

    public COSFilterInputStream(byte[] input, int[] byteRange)
        : this(new MemoryStream(input ?? []), byteRange)
    {
    }

    public byte[] ToByteArray()
    {
        using MemoryStream output = new();
        CopyTo(output);
        return output.ToArray();
    }

    private long Remaining => _ranges[_rangeIndex].End - _position;

    private bool MoveNextRange()
    {
        if (_rangeIndex + 1 >= _ranges.Length)
        {
            return false;
        }

        _rangeIndex++;
        while (_position < _ranges[_rangeIndex].Begin)
        {
            long skipped = _inner.Seek(_ranges[_rangeIndex].Begin - _position, SeekOrigin.Current);
            if (skipped == 0)
            {
                throw new IOException("Unable to advance to byte range start.");
            }

            _position += skipped;
        }

        return true;
    }

    public override int ReadByte()
    {
        if ((_rangeIndex == -1 || Remaining <= 0) && !MoveNextRange())
        {
            return -1;
        }

        int result = _inner.ReadByte();
        if (result >= 0)
        {
            _position++;
        }
        return result;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if ((_rangeIndex == -1 || Remaining <= 0) && !MoveNextRange())
        {
            return 0;
        }

        int read = _inner.Read(buffer, offset, (int)Math.Min(count, Remaining));
        _position += read;
        return read;
    }

    public override bool CanRead => _inner.CanRead;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => _position; set => throw new NotSupportedException(); }
    public override void Flush() { }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _inner.Dispose();
        }

        base.Dispose(disposing);
    }
}
