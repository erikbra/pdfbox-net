/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: io/src/main/java/org/apache/pdfbox/io/ScratchFileBuffer.java
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
/// Implementation of <see cref="RandomAccess"/> as a sequence of multiple fixed-size pages handled
/// by <see cref="ScratchFile"/>.
/// </summary>
internal class ScratchFileBuffer : RandomAccess
{
    private readonly int _pageSize;

    /// <summary>The underlying page handler.</summary>
    private ScratchFile? _pageHandler;

    /// <summary>The number of bytes of content in this buffer.</summary>
    private long _size;

    /// <summary>Index of current page in <see cref="_pageIndexes"/> (the nth page within this buffer).</summary>
    private int _currentPagePositionInPageIndexes;

    /// <summary>The offset of the current page's first byte within the overall buffer.</summary>
    private long _currentPageOffset;

    /// <summary>The current page data.</summary>
    private byte[]? _currentPage;

    /// <summary>The current position (for next read/write) within the current page.</summary>
    private int _positionInPage;

    /// <summary><c>true</c> if the current page was changed by a write method.</summary>
    private bool _currentPageContentChanged;

    /// <summary>Contains an ordered list of page indexes as known by the page handler.</summary>
    private int[] _pageIndexes = new int[16];

    /// <summary>Number of pages held by this buffer.</summary>
    private int _pageCount;

    /// <summary>
    /// Creates a new buffer using pages handled by the provided <see cref="ScratchFile"/>.
    /// </summary>
    internal ScratchFileBuffer(ScratchFile pageHandler)
    {
        pageHandler.CheckClosed();
        _pageHandler = pageHandler;
        _pageSize = pageHandler.GetPageSize();
        AddPage();
    }

    private void CheckClosed()
    {
        if (_pageHandler == null)
        {
            throw new IOException("Buffer already closed");
        }

        _pageHandler.CheckClosed();
    }

    /// <summary>
    /// Adds a new page and positions all pointers to the start of that new page.
    /// </summary>
    private void AddPage()
    {
        if (_pageCount + 1 >= _pageIndexes.Length)
        {
            int newSize = _pageIndexes.Length * 2;
            if (newSize < _pageIndexes.Length)
            {
                if (_pageIndexes.Length == int.MaxValue)
                {
                    throw new IOException("Maximum buffer size reached.");
                }

                newSize = int.MaxValue;
            }

            int[] newPageIndexes = new int[newSize];
            Array.Copy(_pageIndexes, newPageIndexes, _pageCount);
            _pageIndexes = newPageIndexes;
        }

        int newPageIdx = _pageHandler!.GetNewPage();

        _pageIndexes[_pageCount] = newPageIdx;
        _currentPagePositionInPageIndexes = _pageCount;
        _currentPageOffset = (long)_pageCount * _pageSize;
        _pageCount++;
        _currentPage = new byte[_pageSize];
        _positionInPage = 0;
    }

    public long Length()
    {
        CheckClosed();
        return _size;
    }

    /// <summary>
    /// Ensures the current page has at least one byte left
    /// (<see cref="_positionInPage"/> &lt; <see cref="_pageSize"/>).
    /// </summary>
    /// <param name="addNewPageIfNeeded">
    /// If <c>true</c>, a new page may be added when we are at the end of the last buffer page.
    /// </param>
    /// <returns>
    /// <c>true</c> if there are still bytes available within the current page;
    /// <c>false</c> if we are at the end of the last page and adding a new page is not allowed.
    /// </returns>
    private bool EnsureAvailableBytesInPage(bool addNewPageIfNeeded)
    {
        if (_positionInPage >= _pageSize)
        {
            if (_currentPageContentChanged)
            {
                _pageHandler!.WritePage(_pageIndexes[_currentPagePositionInPageIndexes], _currentPage!);
                _currentPageContentChanged = false;
            }

            if (_currentPagePositionInPageIndexes + 1 < _pageCount)
            {
                // already have more pages (due to a backward seek)
                _currentPage = _pageHandler!.ReadPage(
                    _pageIndexes[++_currentPagePositionInPageIndexes]);
                _currentPageOffset = (long)_currentPagePositionInPageIndexes * _pageSize;
                _positionInPage = 0;
            }
            else if (addNewPageIfNeeded)
            {
                AddPage();
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    public void Write(int b)
    {
        CheckClosed();
        EnsureAvailableBytesInPage(true);

        _currentPage![_positionInPage++] = (byte)b;
        _currentPageContentChanged = true;

        if (_currentPageOffset + _positionInPage > _size)
        {
            _size = _currentPageOffset + _positionInPage;
        }
    }

    public void Write(byte[] b) => Write(b, 0, b.Length);

    public void Write(byte[] b, int offset, int length)
    {
        CheckClosed();

        int remain = length;
        int bOff = offset;

        while (remain > 0)
        {
            EnsureAvailableBytesInPage(true);

            int bytesToWrite = Math.Min(remain, _pageSize - _positionInPage);

            Array.Copy(b, bOff, _currentPage!, _positionInPage, bytesToWrite);

            _positionInPage += bytesToWrite;
            _currentPageContentChanged = true;

            bOff += bytesToWrite;
            remain -= bytesToWrite;
        }

        if (_currentPageOffset + _positionInPage > _size)
        {
            _size = _currentPageOffset + _positionInPage;
        }
    }

    public void Clear()
    {
        CheckClosed();

        // keep only the first page, discard all others
        _pageHandler!.MarkPagesAsFree(_pageIndexes, 1, _pageCount - 1);
        _pageCount = 1;

        if (_currentPagePositionInPageIndexes > 0)
        {
            _currentPage = _pageHandler.ReadPage(_pageIndexes[0]);
            _currentPagePositionInPageIndexes = 0;
            _currentPageOffset = 0;
        }

        _positionInPage = 0;
        _size = 0;
        _currentPageContentChanged = false;
    }

    public long GetPosition()
    {
        CheckClosed();
        return _currentPageOffset + _positionInPage;
    }

    public void Seek(long seekToPosition)
    {
        CheckClosed();

        if (seekToPosition > _size)
        {
            throw new EndOfStreamException();
        }

        if (seekToPosition < 0)
        {
            throw new IOException($"Negative seek offset: {seekToPosition}");
        }

        if (seekToPosition >= _currentPageOffset &&
            seekToPosition <= _currentPageOffset + _pageSize)
        {
            // within the same page
            _positionInPage = (int)(seekToPosition - _currentPageOffset);
        }
        else
        {
            if (_currentPageContentChanged)
            {
                _pageHandler!.WritePage(
                    _pageIndexes[_currentPagePositionInPageIndexes], _currentPage!);
                _currentPageContentChanged = false;
            }

            int newPagePosition = (int)(seekToPosition / _pageSize);

            // PDFBOX-4756: Prevent seeking a non-yet-existent page
            if (seekToPosition % _pageSize == 0 && seekToPosition == _size)
            {
                newPagePosition--;
            }

            _currentPage = _pageHandler!.ReadPage(_pageIndexes[newPagePosition]);
            _currentPagePositionInPageIndexes = newPagePosition;
            _currentPageOffset = (long)_currentPagePositionInPageIndexes * _pageSize;
            _positionInPage = (int)(seekToPosition - _currentPageOffset);
        }
    }

    public bool IsClosed() => _pageHandler == null;

    public bool IsEOF()
    {
        CheckClosed();
        return _currentPageOffset + _positionInPage >= _size;
    }

    public int Read()
    {
        CheckClosed();

        if (_currentPageOffset + _positionInPage >= _size)
        {
            return -1;
        }

        if (!EnsureAvailableBytesInPage(false))
        {
            throw new IOException("Unexpectedly no bytes available for read in buffer.");
        }

        return _currentPage![_positionInPage++] & 0xff;
    }

    public int Read(byte[] b, int off, int len)
    {
        CheckClosed();

        if (_currentPageOffset + _positionInPage >= _size)
        {
            return -1;
        }

        int remain = (int)Math.Min(len, _size - (_currentPageOffset + _positionInPage));

        int totalBytesRead = 0;
        int bOff = off;

        while (remain > 0)
        {
            if (!EnsureAvailableBytesInPage(false))
            {
                throw new IOException("Unexpectedly no bytes available for read in buffer.");
            }

            int readBytes = Math.Min(remain, _pageSize - _positionInPage);

            Array.Copy(_currentPage!, _positionInPage, b, bOff, readBytes);

            _positionInPage += readBytes;
            totalBytesRead += readBytes;
            bOff += readBytes;
            remain -= readBytes;
        }

        return totalBytesRead;
    }

    public void Close() => Close(true);

    /// <summary>
    /// Releases all resources and optionally removes this buffer from <see cref="ScratchFile"/>.
    /// </summary>
    internal void Close(bool removeBuffer)
    {
        if (_pageHandler != null)
        {
            _pageHandler.MarkPagesAsFree(_pageIndexes, 0, _pageCount);
            if (removeBuffer)
            {
                _pageHandler.RemoveBuffer(this);
            }

            _pageHandler = null;
            _currentPage = null;
            _currentPageOffset = 0;
            _currentPagePositionInPageIndexes = -1;
            _positionInPage = 0;
            _size = 0;
        }
    }

    public RandomAccessReadView CreateView(long startPosition, long streamLength)
    {
        throw new IOException($"{GetType().Name}.CreateView isn't supported.");
    }
}
