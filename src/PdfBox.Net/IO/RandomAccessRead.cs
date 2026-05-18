/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

// This file was converted mechanically to C# using AI.
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
    /// <summary>
    /// Read a single byte of data.
    /// </summary>
    /// <returns>The byte of data that is being read.</returns>
    int Read();

    /// <summary>
    /// Read a buffer of data.
    /// </summary>
    /// <param name="b">The buffer to write the data to.</param>
    /// <returns>The number of bytes that were actually read.</returns>
    int Read(byte[] b)
    {
        return Read(b, 0, b.Length);
    }

    /// <summary>
    /// Read a buffer of data.
    /// </summary>
    /// <param name="b">The buffer to write the data to.</param>
    /// <param name="offset">Offset into the buffer to start writing.</param>
    /// <param name="length">The amount of data to attempt to read.</param>
    /// <returns>The number of bytes that were actually read.</returns>
    int Read(byte[] b, int offset, int length);

    /// <summary>
    /// Returns offset of next byte to be returned by a read method.
    /// </summary>
    /// <returns>
    /// offset of next byte which will be returned with next <see cref="Read()"/> (if no more bytes are left it
    /// returns a value &gt;= length of source)
    /// </returns>
    long GetPosition();

    /// <summary>
    /// Seek to a position in the data.
    /// </summary>
    /// <param name="position">The position to seek to.</param>
    void Seek(long position);

    /// <summary>
    /// The total number of bytes that are available.
    /// </summary>
    /// <returns>The number of bytes available.</returns>
    long Length();

    /// <summary>
    /// Returns true if this source has been closed.
    /// </summary>
    /// <returns>true if the source has been closed</returns>
    bool IsClosed();

    /// <summary>
    /// This will peek at the next byte.
    /// </summary>
    /// <returns>The next byte on the stream, leaving it as available to read.</returns>
    int Peek()
    {
        int result = Read();
        if (result != -1)
        {
            Rewind(1);
        }

        return result;
    }

    /// <summary>
    /// Seek backwards the given number of bytes.
    /// </summary>
    /// <param name="bytes">the number of bytes to be seeked backwards</param>
    void Rewind(int bytes)
    {
        Seek(GetPosition() - bytes);
    }

    /// <summary>
    /// A simple test to see if we are at the end of the data.
    /// </summary>
    /// <returns>true if we are at the end of the data.</returns>
    bool IsEOF();

    /// <summary>
    /// Returns an estimate of the number of bytes that can be read.
    /// </summary>
    /// <returns>the number of bytes that can be read</returns>
    int Available()
    {
        return (int)Math.Min(Length() - GetPosition(), int.MaxValue);
    }

    /// <summary>
    /// Skips a given number of bytes.
    /// </summary>
    /// <param name="length">the number of bytes to be skipped</param>
    void Skip(int length)
    {
        Seek(GetPosition() + length);
    }

    /// <summary>
    /// Creates a random access read view starting at the given position with the given length.
    /// </summary>
    /// <param name="startPosition">start position within the underlying random access read</param>
    /// <param name="streamLength">stream length</param>
    /// <returns>the random access read view</returns>
    RandomAccessReadView CreateView(long startPosition, long streamLength);

    /// <summary>
    /// Same as <see cref="Read(byte[])"/> but will loop until exactly length bytes are read or
    /// it will throw an exception.
    /// </summary>
    /// <param name="b">The buffer to write the data to.</param>
    void ReadFully(byte[] b)
    {
        ReadFully(b, 0, b.Length);
    }

    /// <summary>
    /// Same as <see cref="Read(byte[], int, int)"/> but will loop until exactly length bytes are read or
    /// it will throw an exception.
    /// </summary>
    /// <param name="b">The buffer to write the data to.</param>
    /// <param name="offset">Offset into the buffer to start writing.</param>
    /// <param name="length">The exact amount of data to attempt to read.</param>
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
