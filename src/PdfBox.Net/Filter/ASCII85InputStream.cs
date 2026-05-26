/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/filter/ASCII85InputStream.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

namespace PdfBox.Net.Filter;

public sealed class ASCII85InputStream : Stream
{
    private const byte Terminator = (byte)'~';
    private const byte Offset = (byte)'!';
    private const byte Newline = (byte)'\n';
    private const byte Return = (byte)'\r';
    private const byte Space = (byte)' ';
    private const byte PaddingU = (byte)'u';
    private const byte Z = (byte)'z';

    private readonly Stream _input;
    private int _index;
    private int _count;
    private bool _eof;
    private byte[]? _ascii = new byte[5];
    private byte[]? _decoded = new byte[4];

    public ASCII85InputStream(Stream input)
    {
        _input = input ?? throw new ArgumentNullException(nameof(input));
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

    public override int ReadByte()
    {
        if (_index >= _count)
        {
            if (_eof)
            {
                return -1;
            }

            _index = 0;
            byte z;
            do
            {
                int next = _input.ReadByte();
                if (next == -1)
                {
                    _eof = true;
                    return -1;
                }

                z = (byte)next;
            } while (z == Newline || z == Return || z == Space);

            if (z == Terminator)
            {
                _eof = true;
                _ascii = null;
                _decoded = null;
                _count = 0;
                return -1;
            }

            if (z == Z)
            {
                _decoded![0] = _decoded[1] = _decoded[2] = _decoded[3] = 0;
                _count = 4;
            }
            else
            {
                _ascii![0] = z;
                int k;
                for (k = 1; k < 5; ++k)
                {
                    do
                    {
                        int next = _input.ReadByte();
                        if (next == -1)
                        {
                            _eof = true;
                            return -1;
                        }

                        z = (byte)next;
                    } while (z == Newline || z == Return || z == Space);

                    _ascii[k] = z;
                    if (z == Terminator)
                    {
                        _ascii[k] = PaddingU;
                        break;
                    }
                }

                _count = k - 1;
                if (_count == 0)
                {
                    _eof = true;
                    _ascii = null;
                    _decoded = null;
                    return -1;
                }

                if (k < 5)
                {
                    for (++k; k < 5; ++k)
                    {
                        _ascii[k] = PaddingU;
                    }

                    _eof = true;
                }

                long value = 0;
                for (k = 0; k < 5; ++k)
                {
                    int adjusted = _ascii[k] - Offset;
                    if (adjusted < 0 || adjusted > 93)
                    {
                        _count = 0;
                        _eof = true;
                        _ascii = null;
                        _decoded = null;
                        throw new IOException("Invalid data in Ascii85 stream");
                    }

                    value = (value * 85L) + adjusted;
                }

                for (k = 3; k >= 0; --k)
                {
                    _decoded![k] = (byte)(value & 0xFF);
                    value >>= 8;
                }
            }
        }

        return _decoded![_index++] & 0xFF;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (offset + count > buffer.Length)
        {
            throw new ArgumentException("Offset and count exceed buffer length.");
        }

        if (_eof && _index >= _count)
        {
            return 0;
        }

        int read = 0;
        for (; read < count; read++)
        {
            if (_index < _count)
            {
                buffer[offset + read] = _decoded![_index++];
                continue;
            }

            int next = ReadByte();
            if (next == -1)
            {
                break;
            }

            buffer[offset + read] = (byte)next;
        }

        return read;
    }

    public override void Flush()
    {
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        _ascii = null;
        _decoded = null;
        _eof = true;
        if (disposing)
        {
            _input.Dispose();
        }

        base.Dispose(disposing);
    }
}
