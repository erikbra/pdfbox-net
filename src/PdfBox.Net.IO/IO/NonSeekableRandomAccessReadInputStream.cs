/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: io/src/main/java/org/apache/pdfbox/io/NonSeekableRandomAccessReadInputStream.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

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

using System;
using System.IO;

namespace PdfBox.Net.IO;

/// <summary>
/// An implementation of the <see cref="RandomAccessRead"/> interface using a <see cref="Stream"/> as source.
/// It is optimized for a minimal memory footprint by using a small buffer to read from the stream.
/// </summary>
public class NonSeekableRandomAccessReadInputStream : RandomAccessRead
{
    // current position within the stream
    protected long Position = 0;
    // current pointer for the current chunk
    protected int CurrentBufferPointer = 0;
    // current size of the stream
    protected long Size = 0;

    // the source input stream
    private readonly Stream _stream;

    // buffer size
    private const int BufferSize = 4096;
    // we are using 3 different buffers for navigation
    private const int Current = 0;
    private const int Last = 1;
    private const int Next = 2;

    // array holding all buffers
    private readonly byte[][] _buffers = [new byte[BufferSize], new byte[BufferSize], new byte[BufferSize]];
    // array holding the number of bytes of all buffers
    private readonly int[] _bufferBytes = [-1, -1, -1];

    private bool _isClosed;
    private bool _isEOF;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public NonSeekableRandomAccessReadInputStream(Stream inputStream)
    {
        _stream = inputStream;
    }

    public void Close()
    {
        _stream.Close();
        _isClosed = true;
    }

    public void Seek(long position)
    {
        throw new IOException($"{GetType().Name}.Seek isn't supported.");
    }

    public void Skip(int length)
    {
        byte[] skipBuffer = new byte[Math.Min(length, BufferSize)];
        int remaining = length;
        while (remaining > 0)
        {
            int bytesRead = Read(skipBuffer, 0, Math.Min(remaining, skipBuffer.Length));
            if (bytesRead == -1)
            {
                break;
            }

            remaining -= bytesRead;
        }
    }

    public long GetPosition()
    {
        CheckClosed();
        return Position;
    }

    public int Read()
    {
        CheckClosed();
        if (IsEOF())
        {
            return -1;
        }

        if (CurrentBufferPointer >= _bufferBytes[Current] && !Fetch())
        {
            _isEOF = true;
            return -1;
        }

        Position++;
        return _buffers[Current][CurrentBufferPointer++] & 0xFF;
    }

    public int Read(byte[] b, int offset, int length)
    {
        CheckClosed();
        if (b == null)
        {
            throw new ArgumentNullException(nameof(b), "buffer is null");
        }

        if (offset < 0 || length < 0 || offset + length > b.Length)
        {
            throw new IndexOutOfRangeException($"buffer length={b.Length} offset={offset} length={length}");
        }

        if (length == 0)
        {
            return 0;
        }

        if (IsEOF())
        {
            return -1;
        }

        int numberOfBytesRead = 0;
        while (numberOfBytesRead < length)
        {
            int available = _bufferBytes[Current] - CurrentBufferPointer;
            if (available > 0)
            {
                int bytesToCopy = Math.Min(length - numberOfBytesRead, available);
                Array.Copy(_buffers[Current], CurrentBufferPointer, b, numberOfBytesRead + offset, bytesToCopy);
                CurrentBufferPointer += bytesToCopy;
                Position += bytesToCopy;
                numberOfBytesRead += bytesToCopy;
            }
            else if (!Fetch())
            {
                _isEOF = true;
                break;
            }
        }

        return numberOfBytesRead > 0 ? numberOfBytesRead : -1;
    }

    public void ReadFully(byte[] b, int offset, int length)
    {
        CheckClosed();
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

    private void SwitchBuffers(int firstBuffer, int secondBuffer)
    {
        (_buffers[firstBuffer], _buffers[secondBuffer]) = (_buffers[secondBuffer], _buffers[firstBuffer]);
        (_bufferBytes[firstBuffer], _bufferBytes[secondBuffer]) = (_bufferBytes[secondBuffer], _bufferBytes[firstBuffer]);
    }

    private bool Fetch()
    {
        CheckClosed();
        CurrentBufferPointer = 0;
        if (_bufferBytes[Next] > -1)
        {
            SwitchBuffers(Current, Last);
            SwitchBuffers(Current, Next);
            _bufferBytes[Next] = -1;
            return true;
        }

        try
        {
            if (_bufferBytes[Last] == BufferSize && _bufferBytes[Current] > 0 && _bufferBytes[Current] < BufferSize)
            {
                Array.Copy(_buffers[Last], _bufferBytes[Current], _buffers[Last], 0, BufferSize - _bufferBytes[Current]);
                Array.Copy(_buffers[Current], 0, _buffers[Last], BufferSize - _bufferBytes[Current], _bufferBytes[Current]);
                _bufferBytes[Last] = BufferSize;
            }
            else
            {
                SwitchBuffers(Current, Last);
            }

            _bufferBytes[Current] = _stream.Read(_buffers[Current], 0, _buffers[Current].Length);
            if (_bufferBytes[Current] <= 0)
            {
                _bufferBytes[Current] = -1;
                return false;
            }

            Size += _bufferBytes[Current];
        }
        catch
        {
            _isEOF = true;
            throw;
        }

        return true;
    }

    public int Available()
    {
        CheckClosed();
        int buffered = Math.Max(0, _bufferBytes[Current] - CurrentBufferPointer);
        return buffered + (int)Math.Min(_stream.CanSeek ? (_stream.Length - _stream.Position) : 0, int.MaxValue);
    }

    public long Length()
    {
        CheckClosed();
        return Size + (_stream.CanSeek ? (_stream.Length - _stream.Position) : 0);
    }

    public void Rewind(int bytes)
    {
        if (CurrentBufferPointer >= bytes)
        {
            CurrentBufferPointer -= bytes;
            Position -= bytes;
            _isEOF = false;
        }
        else if (_bufferBytes[Last] > 0 && (bytes - CurrentBufferPointer) <= _bufferBytes[Last])
        {
            int remainingBytesToRewind = bytes - CurrentBufferPointer;
            SwitchBuffers(Current, Next);
            SwitchBuffers(Current, Last);
            _bufferBytes[Last] = -1;
            CurrentBufferPointer = _bufferBytes[Current] - remainingBytesToRewind;
            Position -= bytes;
            _isEOF = false;
        }
        else
        {
            throw new IOException("not enough bytes available to perform the rewind operation");
        }
    }

    protected void CheckClosed()
    {
        if (_isClosed)
        {
            throw new IOException($"{GetType().Name} already closed");
        }
    }

    public bool IsClosed()
    {
        return _isClosed;
    }

    public bool IsEOF()
    {
        CheckClosed();
        return _isEOF;
    }

    public RandomAccessReadView CreateView(long startPosition, long streamLength)
    {
        throw new IOException($"{GetType().Name}.CreateView isn't supported.");
    }
}
