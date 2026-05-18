// PDFBOX_SOURCE_PATH: io/src/test/java/org/apache/pdfbox/io/RandomAccessReadViewTest.java
// PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
// PORT_MODE: mechanical
// PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db

using System;
using System.IO;
using PdfBox.Net.IO;
using Xunit;

namespace PdfBox.Net.Tests;

/// <summary>
/// Unittest for <see cref="RandomAccessReadView"/>.
/// </summary>
public class RandomAccessReadViewTest
{
    [Fact]
    public void TestPositionSkip()
    {
        byte[] values = CreateValues();
        var randomAccessSource = new ArrayBackedRandomAccessRead(values);
        var randomAccessReadView = new RandomAccessReadView(randomAccessSource, 10, 20);

        Assert.Equal(0, randomAccessReadView.GetPosition());
        Assert.Equal(10, ((RandomAccessRead)randomAccessReadView).Peek());
        ((RandomAccessRead)randomAccessReadView).Skip(5);
        Assert.Equal(5, randomAccessReadView.GetPosition());
        Assert.Equal(15, ((RandomAccessRead)randomAccessReadView).Peek());

        randomAccessReadView.Close();
        randomAccessSource.Close();
    }

    [Fact]
    public void TestPositionRead()
    {
        byte[] values = CreateValues();
        var randomAccessSource = new ArrayBackedRandomAccessRead(values);
        var randomAccessReadView = new RandomAccessReadView(randomAccessSource, 10, 20);

        Assert.Equal(0, randomAccessReadView.GetPosition());
        Assert.Equal(10, randomAccessReadView.Read());
        Assert.Equal(11, randomAccessReadView.Read());
        Assert.Equal(12, randomAccessReadView.Read());
        Assert.Equal(3, randomAccessReadView.GetPosition());

        Assert.False(randomAccessReadView.IsClosed());
        randomAccessReadView.Close();
        Assert.True(randomAccessReadView.IsClosed());
        randomAccessReadView.Close();

        randomAccessSource.Close();
    }

    [Fact]
    public void TestSeekEOF()
    {
        byte[] values = CreateValues();
        var randomAccessSource = new ArrayBackedRandomAccessRead(values);
        var randomAccessReadView = new RandomAccessReadView(randomAccessSource, 10, 20);

        randomAccessReadView.Seek(3);
        Assert.Equal(3, randomAccessReadView.GetPosition());
        Assert.Throws<IOException>(() => randomAccessReadView.Seek(-1));
        Assert.False(randomAccessReadView.IsEOF());
        randomAccessReadView.Seek(20);
        Assert.True(randomAccessReadView.IsEOF());
        Assert.Equal(-1, randomAccessReadView.Read());
        Assert.Equal(-1, randomAccessReadView.Read(new byte[1], 0, 1));
        randomAccessReadView.Close();

        randomAccessSource.Close();

        Assert.Throws<IOException>(() => randomAccessReadView.Read());
    }

    [Fact]
    public void TestPositionReadBytes()
    {
        byte[] values = CreateValues();
        var randomAccessSource = new ArrayBackedRandomAccessRead(values);
        var randomAccessReadView = new RandomAccessReadView(randomAccessSource, 10, 20);

        Assert.Equal(0, randomAccessReadView.GetPosition());
        byte[] buffer = new byte[4];
        ((RandomAccessRead)randomAccessReadView).Read(buffer);
        Assert.Equal(10, buffer[0]);
        Assert.Equal(13, buffer[3]);
        Assert.Equal(4, randomAccessReadView.GetPosition());

        randomAccessReadView.Read(buffer, 1, 2);
        Assert.Equal(10, buffer[0]);
        Assert.Equal(14, buffer[1]);
        Assert.Equal(15, buffer[2]);
        Assert.Equal(13, buffer[3]);
        Assert.Equal(6, randomAccessReadView.GetPosition());

        randomAccessReadView.Close();
        randomAccessSource.Close();
    }

    [Fact]
    public void TestPositionPeek()
    {
        byte[] values = CreateValues();
        var randomAccessSource = new ArrayBackedRandomAccessRead(values);
        var randomAccessReadView = new RandomAccessReadView(randomAccessSource, 10, 20);

        Assert.Equal(0, randomAccessReadView.GetPosition());
        ((RandomAccessRead)randomAccessReadView).Skip(6);
        Assert.Equal(6, randomAccessReadView.GetPosition());

        Assert.Equal(16, ((RandomAccessRead)randomAccessReadView).Peek());
        Assert.Equal(6, randomAccessReadView.GetPosition());

        randomAccessReadView.Close();
        randomAccessSource.Close();
    }

    [Fact]
    public void TestPositionUnreadBytes()
    {
        byte[] values = CreateValues();
        var randomAccessSource = new ArrayBackedRandomAccessRead(values);
        var randomAccessReadView = new RandomAccessReadView(randomAccessSource, 10, 20);

        Assert.Equal(0, randomAccessReadView.GetPosition());
        randomAccessReadView.Read();
        randomAccessReadView.Read();
        byte[] readBytes = new byte[6];
        Assert.Equal(readBytes.Length, ((RandomAccessRead)randomAccessReadView).Read(readBytes));
        Assert.Equal(8, randomAccessReadView.GetPosition());
        randomAccessReadView.Rewind(readBytes.Length);
        Assert.Equal(2, randomAccessReadView.GetPosition());
        Assert.Equal(12, randomAccessReadView.Read());
        Assert.Equal(3, randomAccessReadView.GetPosition());
        randomAccessReadView.Read(readBytes, 2, 4);
        Assert.Equal(12, readBytes[0]);
        Assert.Equal(13, readBytes[2]);
        Assert.Equal(16, readBytes[5]);
        Assert.Equal(7, randomAccessReadView.GetPosition());
        randomAccessReadView.Rewind(4);
        Assert.Equal(3, randomAccessReadView.GetPosition());

        randomAccessReadView.Close();
        randomAccessSource.Close();
    }

    [Fact]
    public void TestCreateView()
    {
        byte[] values = CreateValues();
        var randomAccessSource = new ArrayBackedRandomAccessRead(values);
        var randomAccessReadView = new RandomAccessReadView(randomAccessSource, 10, 20);

        Assert.Throws<IOException>(() => randomAccessReadView.CreateView(0, 20));

        randomAccessReadView.Close();
        randomAccessSource.Close();
    }

    private static byte[] CreateValues()
    {
        return
        [
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
            10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
            20
        ];
    }

    private sealed class ArrayBackedRandomAccessRead(byte[] source) : RandomAccessRead
    {
        private readonly byte[] _source = source;
        private long _position;
        private bool _closed;

        public int Read()
        {
            CheckClosed();
            if (IsEOF())
            {
                return -1;
            }

            return _source[_position++];
        }

        public int Read(byte[] b, int offset, int length)
        {
            CheckClosed();
            if (IsEOF())
            {
                return -1;
            }

            int toRead = (int)Math.Min(length, _source.Length - _position);
            Array.Copy(_source, _position, b, offset, toRead);
            _position += toRead;
            return toRead;
        }

        public long GetPosition()
        {
            CheckClosed();
            return _position;
        }

        public void Seek(long position)
        {
            CheckClosed();
            if (position < 0)
            {
                throw new IOException($"Invalid position {position}");
            }

            _position = Math.Min(position, _source.Length);
        }

        public long Length()
        {
            CheckClosed();
            return _source.Length;
        }

        public bool IsClosed()
        {
            return _closed;
        }

        public bool IsEOF()
        {
            CheckClosed();
            return _position >= _source.Length;
        }

        public RandomAccessReadView CreateView(long startPosition, long streamLength)
        {
            CheckClosed();
            return new RandomAccessReadView(this, startPosition, streamLength);
        }

        public void Close()
        {
            _closed = true;
        }

        private void CheckClosed()
        {
            if (_closed)
            {
                throw new IOException("ArrayBackedRandomAccessRead already closed");
            }
        }
    }
}
