// PDFBOX_SOURCE_PATH: io/src/main/java/org/apache/pdfbox/io/RandomAccessRead.java
// PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
// PORT_MODE: mechanical
// PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db

using System;
using System.IO;

namespace PdfBox.Net.IO;

/// <summary>
/// An interface allowing random access read operations.
/// </summary>
public interface RandomAccessRead
{
    int Read();

    int Read(byte[] b)
    {
        return Read(b, 0, b.Length);
    }

    int Read(byte[] b, int offset, int length);

    long GetPosition();

    void Seek(long position);

    long Length();

    bool IsClosed();

    int Peek()
    {
        int result = Read();
        if (result != -1)
        {
            Rewind(1);
        }

        return result;
    }

    void Rewind(int bytes)
    {
        Seek(GetPosition() - bytes);
    }

    bool IsEOF();

    int Available()
    {
        return (int)Math.Min(Length() - GetPosition(), int.MaxValue);
    }

    void Skip(int length)
    {
        Seek(GetPosition() + length);
    }

    RandomAccessReadView CreateView(long startPosition, long streamLength);

    void ReadFully(byte[] b)
    {
        ReadFully(b, 0, b.Length);
    }

    void ReadFully(byte[] b, int offset, int length)
    {
        if (Length() - GetPosition() < length)
        {
            throw new EndOfStreamException("Premature end of buffer reached");
        }

        int bytesReadTotal = 0;
        while (bytesReadTotal < length)
        {
            int bytesReadNow = Read(b, offset + bytesReadTotal, length - bytesReadTotal);
            if (bytesReadNow <= 0)
            {
                throw new EndOfStreamException("EOF, should have been detected earlier");
            }

            bytesReadTotal += bytesReadNow;
        }
    }

    void Close();
}
