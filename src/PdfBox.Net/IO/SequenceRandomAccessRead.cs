/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 * PDFBOX_SOURCE_PATH: io/src/main/java/org/apache/pdfbox/io/SequenceRandomAccessRead.java
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
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PdfBox.Net.IO;

/// <summary>
/// Wrapper class to combine several RandomAccessRead instances so that they can be accessed as one big RandomAccessRead.
/// </summary>
public class SequenceRandomAccessRead : RandomAccessRead
{
    private readonly List<RandomAccessRead> _readerList;
    private readonly long[] _startPositions;
    private readonly long[] _endPositions;
    private readonly int _numberOfReader;
    private int _currentIndex;
    private long _currentPosition;
    private readonly long _totalLength;
    private bool _isClosed;
    private RandomAccessRead? _currentRandomAccessRead;

    /// <summary>
    /// Creates a new SequenceRandomAccessRead combining the given readers.
    /// Empty readers are filtered out before use.
    /// </summary>
    /// <param name="randomAccessReadList">the list of RandomAccessRead instances to combine</param>
    /// <exception cref="ArgumentException">if the list is null or empty</exception>
    public SequenceRandomAccessRead(IList<RandomAccessRead> randomAccessReadList)
    {
        if (randomAccessReadList is null)
        {
            throw new ArgumentException("Missing input parameter");
        }

        if (randomAccessReadList.Count == 0)
        {
            throw new ArgumentException("Empty list");
        }

        _readerList = randomAccessReadList
            .Where(r => r.Length() > 0)
            .ToList();

        _currentRandomAccessRead = _readerList[_currentIndex];
        _numberOfReader = _readerList.Count;
        _startPositions = new long[_numberOfReader];
        _endPositions = new long[_numberOfReader];

        for (int i = 0; i < _numberOfReader; i++)
        {
            _startPositions[i] = _totalLength;
            _totalLength += _readerList[i].Length();
            _endPositions[i] = _totalLength - 1;
        }
    }

    public void Close()
    {
        foreach (RandomAccessRead randomAccessRead in _readerList)
        {
            randomAccessRead.Close();
        }

        _readerList.Clear();
        _currentRandomAccessRead = null;
        _isClosed = true;
    }

    private RandomAccessRead GetCurrentReader()
    {
        if (_currentRandomAccessRead is null)
        {
            throw new IOException("No current reader available");
        }

        if (_currentRandomAccessRead.IsEOF() && _currentIndex < _numberOfReader - 1)
        {
            _currentIndex++;
            _currentRandomAccessRead = _readerList[_currentIndex];
            _currentRandomAccessRead.Seek(0);
        }

        return _currentRandomAccessRead;
    }

    public int Read()
    {
        CheckClosed();
        RandomAccessRead randomAccessRead = GetCurrentReader();
        int value = randomAccessRead.Read();
        if (value > -1)
        {
            _currentPosition++;
        }

        return value;
    }

    public int Read(byte[] b, int offset, int length)
    {
        CheckClosed();
        if (length == 0)
        {
            return 0;
        }

        int maxAvailBytes = Math.Min(((RandomAccessRead)this).Available(), length);
        if (maxAvailBytes == 0)
        {
            return -1;
        }

        RandomAccessRead randomAccessRead = GetCurrentReader();
        int bytesRead = randomAccessRead.Read(b, offset, maxAvailBytes);
        while (bytesRead > -1 && bytesRead < maxAvailBytes)
        {
            randomAccessRead = GetCurrentReader();
            int additional = randomAccessRead.Read(b, offset + bytesRead, maxAvailBytes - bytesRead);
            if (additional <= 0)
            {
                break;
            }

            bytesRead += additional;
        }

        _currentPosition += bytesRead;
        return bytesRead;
    }

    public long GetPosition()
    {
        CheckClosed();
        return _currentPosition;
    }

    public void Seek(long position)
    {
        CheckClosed();
        if (position < 0)
        {
            throw new IOException($"Invalid position {position}");
        }

        // it is allowed to jump beyond the end of the file
        // jump to the end of the reader
        if (position >= _totalLength)
        {
            _currentIndex = _numberOfReader - 1;
            _currentPosition = _totalLength;
        }
        else
        {
            // search forward/backwards if the new position is after/before the current position
            int increment = position < _currentPosition ? -1 : 1;
            for (int i = _currentIndex; i < _numberOfReader && i >= 0; i += increment)
            {
                if (position >= _startPositions[i] && position <= _endPositions[i])
                {
                    _currentIndex = i;
                    break;
                }
            }

            _currentPosition = position;
        }

        _currentRandomAccessRead = _readerList[_currentIndex];
        _currentRandomAccessRead.Seek(_currentPosition - _startPositions[_currentIndex]);
    }

    public long Length()
    {
        CheckClosed();
        return _totalLength;
    }

    public bool IsClosed()
    {
        return _isClosed;
    }

    /// <summary>
    /// Ensure that the SequenceRandomAccessRead is not closed.
    /// </summary>
    /// <exception cref="IOException">If the SequenceRandomAccessRead is already closed.</exception>
    private void CheckClosed()
    {
        if (_isClosed)
        {
            throw new IOException("SequenceRandomAccessRead already closed");
        }
    }

    public bool IsEOF()
    {
        CheckClosed();
        return _currentPosition >= _totalLength;
    }

    public RandomAccessReadView CreateView(long startPosition, long streamLength)
    {
        throw new NotSupportedException($"{GetType().Name}.CreateView isn't supported.");
    }
}
