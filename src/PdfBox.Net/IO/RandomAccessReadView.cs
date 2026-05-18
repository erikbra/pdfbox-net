/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 * PDFBOX_SOURCE_PATH: io/src/main/java/org/apache/pdfbox/io/RandomAccessReadView.java
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
/// This class provides a view of a part of a random access read. It clips the section starting
/// at the given start position with the given length into a new random access read.
/// </summary>
public class RandomAccessReadView(RandomAccessRead randomAccessRead, long startPosition, long streamLength, bool closeInput = false)
    : RandomAccessRead
{
    // the underlying random access read
    private RandomAccessRead? _randomAccessRead = randomAccessRead;
    // the start position within the underlying source
    private readonly long _startPosition = startPosition;
    // stream length
    private readonly long _streamLength = streamLength;
    // close input
    private readonly bool _closeInput = closeInput;
    // current position within the view
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

    /// <summary>
    /// Restore the current position within the underlying random access read.
    /// </summary>
    private void RestorePosition()
    {
        _randomAccessRead!.Seek(_startPosition + _currentPosition);
    }

    /// <summary>
    /// Ensure that the view isn't closed.
    /// </summary>
    /// <exception cref="IOException">If RandomAccessReadView already closed.</exception>
    private void CheckClosed()
    {
        if (IsClosed())
        {
            // consider that the rab is closed if there is no current buffer
            throw new IOException("RandomAccessReadView already closed");
        }
    }

    public RandomAccessReadView CreateView(long startPosition, long streamLength)
    {
        throw new IOException($"{GetType().Name}.createView isn't supported.");
    }
}
