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

// Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
// Mechanically converted from Apache PDFBox Java source with AI assistance.
// PDFBOX_SOURCE_PATH: io/src/main/java/org/apache/pdfbox/io/RandomAccessReadBuffer.java
// PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
// PORT_MODE: mechanical
// PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace PdfBox.Net.IO;

/// <summary>
/// An implementation of the RandomAccessRead interface to store data in memory. The data will be stored in chunks
/// organized in a list.
/// </summary>
public class RandomAccessReadBuffer : RandomAccessRead
{
    // default chunk size is 4kb
    public const int DEFAULT_CHUNK_SIZE_4KB = 1 << 12;

    // use the default chunk size
    protected int _chunkSize = DEFAULT_CHUNK_SIZE_4KB;

    // list containing all chunks
    private readonly List<byte[]> _bufferList;

    // current chunk
    protected byte[]? _currentBuffer;

    // current pointer to the whole buffer
    protected long _pointer;

    // current pointer for the current chunk
    protected int _currentBufferPointer;

    // size of the whole buffer
    protected long _size;

    // current chunk list index
    private int _bufferListIndex;

    // maximum chunk list index
    private int _bufferListMaxIndex;

    // map holding all copies of the current buffer
    private readonly ConcurrentDictionary<int, RandomAccessReadBuffer> _rarbCopies = new();

    /// <summary>
    /// Default constructor.
    /// </summary>
    protected RandomAccessReadBuffer()
        : this(DEFAULT_CHUNK_SIZE_4KB)
    {
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    protected RandomAccessReadBuffer(int definedChunkSize)
    {
        // starting with one chunk
        _chunkSize = definedChunkSize;
        _currentBuffer = new byte[_chunkSize];
        _bufferList = [_currentBuffer];
    }

    /// <summary>
    /// Create a random access buffer using the given byte array.
    /// </summary>
    /// <param name="input">the byte array to be read</param>
    public RandomAccessReadBuffer(byte[] input)
    {
        // this is a special case. Wrap the given byte array to a single buffer.
        _chunkSize = input.Length;
        _size = _chunkSize;
        _currentBuffer = input;
        _bufferList = [_currentBuffer];
    }

    /// <summary>
    /// Create a random access buffer using a byte array segment. The segment count is used as the readable length.
    /// </summary>
    /// <param name="input">the byte array segment to be read</param>
    public RandomAccessReadBuffer(ArraySegment<byte> input)
        : this(CopySegment(input))
    {
    }

    /// <summary>
    /// Create a random access read buffer of the given input stream by copying the data to the memory.
    /// </summary>
    /// <param name="input">the input stream to be read</param>
    public RandomAccessReadBuffer(Stream input)
        : this()
    {
        int bytesRead;
        int remainingBytes = _chunkSize;
        int offset = 0;
        byte[] eofCheck = new byte[1];

        while (remainingBytes > 0 &&
               (bytesRead = input.Read(_currentBuffer!, offset, remainingBytes)) > 0)
        {
            remainingBytes -= bytesRead;
            offset += bytesRead;
            _size += bytesRead;

            if (remainingBytes == 0 && input.Read(eofCheck, 0, 1) > 0)
            {
                ExpandBuffer();
                _currentBuffer![_currentBufferPointer++] = eofCheck[0];
                offset = 1;
                remainingBytes = _chunkSize - 1;
                _size++;
            }
        }

        Seek(0);
    }

    private static byte[] CopySegment(ArraySegment<byte> input)
    {
        if (input.Count == 0)
        {
            return Array.Empty<byte>();
        }

        byte[] copy = new byte[input.Count];
        Array.Copy(input.Array!, input.Offset, copy, 0, input.Count);
        return copy;
    }

    private RandomAccessReadBuffer(RandomAccessReadBuffer parent)
    {
        _chunkSize = parent._chunkSize;
        _size = parent._size;
        _bufferListMaxIndex = parent._bufferListMaxIndex;
        _bufferList = new List<byte[]>(parent._bufferList.Count);

        foreach (byte[] buffer in parent._bufferList)
        {
            byte[] newBuffer = new byte[buffer.Length];
            Array.Copy(buffer, newBuffer, buffer.Length);
            _bufferList.Add(newBuffer);
        }

        _currentBuffer = _bufferList[0];
    }

    public void Close()
    {
        if (!IsClosed())
        {
            foreach (RandomAccessReadBuffer readBufferCopy in _rarbCopies.Values)
            {
                try
                {
                    readBufferCopy.Close();
                }
                catch
                {
                    // ignore close exceptions
                }
            }

            _rarbCopies.Clear();
            _currentBuffer = null;
            _bufferList.Clear();
        }
    }

    public void Seek(long position)
    {
        CheckClosed();
        if (position < 0)
        {
            throw new IOException($"Invalid position {position}");
        }

        if (position < _size)
        {
            _pointer = position;
            // calculate the chunk list index
            _bufferListIndex = _chunkSize > 0 ? (int)(_pointer / _chunkSize) : 0;
            _currentBufferPointer = _chunkSize > 0 ? (int)(_pointer % _chunkSize) : 0;
            _currentBuffer = _bufferList[_bufferListIndex];
        }
        else
        {
            // it is allowed to jump beyond the end of the file
            // jump to the end of the buffer
            _pointer = _size;
            _bufferListIndex = _bufferListMaxIndex;
            _currentBuffer = _bufferList[_bufferListIndex];
            _currentBufferPointer = _chunkSize > 0 ? (int)(_size % _chunkSize) : 0;
        }
    }

    public long GetPosition()
    {
        CheckClosed();
        return _pointer;
    }

    public int Read()
    {
        CheckClosed();
        if (_pointer >= _size)
        {
            return -1;
        }

        if (_currentBufferPointer >= _chunkSize)
        {
            if (_bufferListIndex >= _bufferListMaxIndex)
            {
                return -1;
            }

            _currentBuffer = _bufferList[++_bufferListIndex];
            _currentBufferPointer = 0;
        }

        _pointer++;
        return _currentBuffer![_currentBufferPointer++] & 0xff;
    }

    public int Read(byte[] b, int offset, int length)
    {
        CheckClosed();
        int bytesRead = ReadRemainingBytes(b, offset, length);

        if (bytesRead == -1)
        {
            if (((RandomAccessRead)this).Available() > 0)
            {
                bytesRead = 0;
            }
            else
            {
                return -1;
            }
        }

        while (bytesRead < length && ((RandomAccessRead)this).Available() > 0)
        {
            if (_currentBufferPointer == _chunkSize)
            {
                NextBuffer();
            }

            bytesRead += ReadRemainingBytes(b, offset + bytesRead, length - bytesRead);
        }

        return bytesRead;
    }

    private int ReadRemainingBytes(byte[] b, int offset, int length)
    {
        if (_pointer >= _size)
        {
            return -1;
        }

        int maxLength = (int)Math.Min(length, _size - _pointer);
        int remainingBytes = _chunkSize - _currentBufferPointer;

        // no more bytes left
        if (remainingBytes == 0)
        {
            return -1;
        }

        if (maxLength >= remainingBytes)
        {
            // copy the remaining bytes from the current buffer
            Array.Copy(_currentBuffer!, _currentBufferPointer, b, offset, remainingBytes);
            // end of file reached
            _currentBufferPointer += remainingBytes;
            _pointer += remainingBytes;
            return remainingBytes;
        }

        // copy the remaining bytes from the whole buffer
        Array.Copy(_currentBuffer!, _currentBufferPointer, b, offset, maxLength);
        // end of file reached
        _currentBufferPointer += maxLength;
        _pointer += maxLength;
        return maxLength;
    }

    public long Length()
    {
        CheckClosed();
        return _size;
    }

    /// <summary>
    /// create a new buffer chunk and adjust all pointers and indices.
    /// </summary>
    protected void ExpandBuffer()
    {
        if (_bufferListMaxIndex > _bufferListIndex)
        {
            // there is already an existing chunk
            NextBuffer();
        }
        else
        {
            // create a new chunk and add it to the buffer
            _currentBuffer = new byte[_chunkSize];
            _bufferList.Add(_currentBuffer);
            _currentBufferPointer = 0;
            _bufferListMaxIndex++;
            _bufferListIndex++;
        }
    }

    /// <summary>
    /// switch to the next buffer chunk and reset the buffer pointer.
    /// </summary>
    private void NextBuffer()
    {
        if (_bufferListIndex == _bufferListMaxIndex)
        {
            throw new IOException("No more chunks available, end of buffer reached");
        }

        _currentBufferPointer = 0;
        _currentBuffer = _bufferList[++_bufferListIndex];
    }

    /// <summary>
    /// Ensure that the RandomAccessBuffer is not closed.
    /// </summary>
    protected void CheckClosed()
    {
        if (_currentBuffer is null)
        {
            // consider that the rab is closed if there is no current buffer
            throw new IOException("RandomAccessBuffer already closed");
        }
    }

    public bool IsClosed()
    {
        return _currentBuffer is null;
    }

    public bool IsEOF()
    {
        CheckClosed();
        return _pointer >= _size;
    }

    public RandomAccessReadView CreateView(long startPosition, long streamLength)
    {
        int currentThreadId = Environment.CurrentManagedThreadId;
        if (!_rarbCopies.TryGetValue(currentThreadId, out RandomAccessReadBuffer? randomAccessReadBuffer)
            || randomAccessReadBuffer.IsClosed())
        {
            randomAccessReadBuffer = new RandomAccessReadBuffer(this);
            _rarbCopies[currentThreadId] = randomAccessReadBuffer;
        }

        return new RandomAccessReadView(randomAccessReadBuffer, startPosition, streamLength);
    }

    /// <summary>
    /// Create a new RandomAccessReadBuffer using the given Stream. The data is copied to memory and the
    /// Stream is closed.
    /// </summary>
    /// <param name="inputStream">the Stream holding the data to be copied</param>
    /// <returns>the RandomAccessReadBuffer holding the data of the Stream</returns>
    public static RandomAccessReadBuffer CreateBufferFromStream(Stream inputStream)
    {
        RandomAccessReadBuffer? randomAccessRead = null;
        using (inputStream)
        {
            randomAccessRead = new RandomAccessReadBuffer(inputStream);
        }

        return randomAccessRead;
    }

    /// <summary>
    /// Reset to position 0 and remove all buffers but the first one.
    /// </summary>
    protected void ResetBuffers()
    {
        _size = 0;
        _pointer = 0;
        _currentBuffer = _bufferList[0];
        _currentBufferPointer = 0;
        _bufferListIndex = 0;
        _bufferListMaxIndex = 0;
        _bufferList.Clear();
        _bufferList.Add(_currentBuffer);
    }
}
