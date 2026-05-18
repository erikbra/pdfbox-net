// PDFBOX_SOURCE_PATH: io/src/main/java/org/apache/pdfbox/io/RandomAccessReadView.java
// PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
// PORT_MODE: mechanical
// PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db

using System;
using System.IO;

namespace PdfBox.Net.IO;

/// <summary>
/// This class provides a view of a part of a random access read. It clips the section starting
/// at the given start position with the given length into a new random access read.
/// </summary>
public class RandomAccessReadView(RandomAccessRead randomAccessRead, long startPosition, long streamLength, bool closeInput = false)
    : RandomAccessRead
{
    private RandomAccessRead? _randomAccessRead = randomAccessRead;
    private readonly long _startPosition = startPosition;
    private readonly long _streamLength = streamLength;
    private readonly bool _closeInput = closeInput;
    private long _currentPosition;

    public long GetPosition()
    {
        CheckClosed();
        return _currentPosition;
    }

    public void Seek(long newOffset)
    {
        CheckClosed();
        if (newOffset < 0)
        {
            throw new IOException($"Invalid position {newOffset}");
        }

        _randomAccessRead!.Seek(_startPosition + Math.Min(newOffset, _streamLength));
        _currentPosition = newOffset;
    }

    public int Read()
    {
        if (IsEOF())
        {
            return -1;
        }

        RestorePosition();
        int readValue = _randomAccessRead!.Read();
        if (readValue > -1)
        {
            _currentPosition++;
        }

        return readValue;
    }

    public int Read(byte[] b, int off, int len)
    {
        if (IsEOF())
        {
            return -1;
        }

        RestorePosition();
        int readBytes = _randomAccessRead!.Read(b, off, Math.Min(len, ((RandomAccessRead)this).Available()));
        _currentPosition += readBytes;
        return readBytes;
    }

    public long Length()
    {
        CheckClosed();
        return _streamLength;
    }

    public void Close()
    {
        if (_closeInput && _randomAccessRead is not null)
        {
            _randomAccessRead.Close();
        }

        _randomAccessRead = null;
    }

    public bool IsClosed()
    {
        return _randomAccessRead is null || _randomAccessRead.IsClosed();
    }

    public void Rewind(int bytes)
    {
        CheckClosed();
        RestorePosition();
        _randomAccessRead!.Rewind(bytes);
        _currentPosition -= bytes;
    }

    public bool IsEOF()
    {
        CheckClosed();
        return _currentPosition >= _streamLength;
    }

    public RandomAccessReadView CreateView(long startPosition, long streamLength)
    {
        throw new IOException($"{GetType().Name}.createView isn't supported.");
    }

    private void RestorePosition()
    {
        _randomAccessRead!.Seek(_startPosition + _currentPosition);
    }

    private void CheckClosed()
    {
        if (IsClosed())
        {
            throw new IOException("RandomAccessReadView already closed");
        }
    }
}
