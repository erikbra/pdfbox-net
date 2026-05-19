/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: io/src/main/java/org/apache/pdfbox/io/RandomAccessReadBufferedFile.java
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace PdfBox.Net.IO;

/// <summary>
/// Provides random access to portions of a file combined with buffered reading of content.
/// The start of the next bytes to read can be set via the <see cref="Seek"/> method.
///
/// <para>The file is accessed via a <see cref="FileStream"/> and read in fixed-size page chunks
/// that are stored in an LRU page cache.</para>
/// </summary>
public class RandomAccessReadBufferedFile : RandomAccessRead
{
    private const int PageSizeShift = 12;
    private const int PageSize = 1 << PageSizeShift; // 4096
    private const long PageOffsetMask = unchecked((long)(0xFFFFFFFFFFFF_F000L)); // -1L << PageSizeShift
    private const int MaxCachedPages = 1000;

    // map holding all copies created for CreateView (keyed by thread id)
    private readonly ConcurrentDictionary<long, RandomAccessReadBufferedFile> _rafCopies = new();

    // LRU page cache
    private readonly LruPageCache _pageCache = new(MaxCachedPages);

    private long _curPageOffset = -1;
    private byte[]? _curPage;
    private int _offsetWithinPage;

    private readonly FileStream _fileStream;
    private readonly string _filePath;
    private readonly long _fileLength;
    private long _fileOffset;
    private bool _isClosed;

    /// <summary>
    /// Creates a random access buffered file instance for the file at the given path.
    /// </summary>
    public RandomAccessReadBufferedFile(string filePath)
    {
        _filePath = filePath;
        _fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read,
            FileShare.Read, 4096, FileOptions.RandomAccess);
        _fileLength = _fileStream.Length;
        Seek(0);
    }

    /// <summary>
    /// Creates a random access buffered file instance for the given file.
    /// </summary>
    public RandomAccessReadBufferedFile(FileInfo file) : this(file.FullName)
    {
    }

    public long GetPosition()
    {
        CheckClosed();
        return _fileOffset;
    }

    /// <summary>
    /// Seeks to a new position. If the new position is outside of the current page the new page
    /// is either taken from the cache or read from the file and added to the cache.
    /// </summary>
    public void Seek(long position)
    {
        CheckClosed();
        if (position < 0)
        {
            throw new IOException($"Invalid position {position}");
        }

        long newPageOffset = position & PageOffsetMask;
        if (newPageOffset != _curPageOffset)
        {
            byte[]? newPage = _pageCache.Get(newPageOffset);
            if (newPage == null)
            {
                _fileStream.Seek(newPageOffset, SeekOrigin.Begin);
                newPage = ReadPage();
                _pageCache.Put(newPageOffset, newPage);
            }

            _curPageOffset = newPageOffset;
            _curPage = newPage;
        }

        _fileOffset = Math.Min(position, _fileLength);
        _offsetWithinPage = (int)(_fileOffset - _curPageOffset);
    }

    /// <summary>
    /// Reads a page's worth of data from the current file position.
    /// </summary>
    private byte[] ReadPage()
    {
        byte[] page = _pageCache.TakeRecycledBuffer() ?? new byte[PageSize];

        int totalRead = 0;
        while (totalRead < PageSize)
        {
            int n = _fileStream.Read(page, totalRead, PageSize - totalRead);
            if (n <= 0)
            {
                break;
            }

            totalRead += n;
        }

        return page;
    }

    public int Read()
    {
        CheckClosed();
        if (_fileOffset >= _fileLength)
        {
            return -1;
        }

        if (_offsetWithinPage == PageSize)
        {
            Seek(_fileOffset);
        }

        _fileOffset++;
        return _curPage![_offsetWithinPage++] & 0xff;
    }

    public int Read(byte[] b, int off, int len)
    {
        CheckClosed();
        if (_fileOffset >= _fileLength)
        {
            return -1;
        }

        if (_offsetWithinPage == PageSize)
        {
            Seek(_fileOffset);
        }

        int commonLen = Math.Min(PageSize - _offsetWithinPage, len);
        if (_fileLength - _fileOffset < PageSize)
        {
            commonLen = Math.Min(commonLen, (int)(_fileLength - _fileOffset));
        }

        Array.Copy(_curPage!, _offsetWithinPage, b, off, commonLen);

        _offsetWithinPage += commonLen;
        _fileOffset += commonLen;

        return commonLen;
    }

    public long Length()
    {
        CheckClosed();
        return _fileLength;
    }

    public void Close()
    {
        if (!IsClosed())
        {
            foreach (RandomAccessReadBufferedFile copy in _rafCopies.Values)
            {
                try
                {
                    copy.Close();
                }
                catch
                {
                    // ignore
                }
            }

            _rafCopies.Clear();
            _fileStream.Close();
            _pageCache.Clear();
            _isClosed = true;
        }
    }

    public bool IsClosed() => _isClosed;

    private void CheckClosed()
    {
        if (_isClosed)
        {
            throw new IOException($"{GetType().Name} already closed");
        }
    }

    public bool IsEOF()
    {
        CheckClosed();
        return ((RandomAccessRead)this).Peek() == -1;
    }

    public RandomAccessReadView CreateView(long startPosition, long streamLength)
    {
        CheckClosed();
        long currentThreadId = Environment.CurrentManagedThreadId;
        if (!_rafCopies.TryGetValue(currentThreadId, out RandomAccessReadBufferedFile? copy)
            || copy.IsClosed())
        {
            copy = new RandomAccessReadBufferedFile(_filePath);
            _rafCopies[currentThreadId] = copy;
        }

        return new RandomAccessReadView(copy, startPosition, streamLength);
    }

    // ---- LRU page cache ----

    private sealed class LruPageCache(int capacity)
    {
        private readonly int _capacity = capacity;
        private readonly Dictionary<long, LinkedListNode<(long key, byte[] page)>> _map = new();
        private readonly LinkedList<(long key, byte[] page)> _list = new();
        private byte[]? _recycled;

        public byte[]? Get(long key)
        {
            if (!_map.TryGetValue(key, out LinkedListNode<(long, byte[])>? node))
            {
                return null;
            }

            _list.Remove(node);
            _list.AddLast(node);
            return node.Value.Item2;
        }

        public void Put(long key, byte[] page)
        {
            if (_map.TryGetValue(key, out LinkedListNode<(long, byte[])>? existing))
            {
                _list.Remove(existing);
                _map.Remove(key);
            }

            var node = _list.AddLast((key, page));
            _map[key] = node;

            if (_list.Count > _capacity)
            {
                LinkedListNode<(long k, byte[] p)> oldest = _list.First!;
                _recycled = oldest.Value.p; // recycle the buffer
                _map.Remove(oldest.Value.k);
                _list.RemoveFirst();
            }
        }

        public byte[]? TakeRecycledBuffer()
        {
            byte[]? r = _recycled;
            _recycled = null;
            return r;
        }

        public void Clear()
        {
            _map.Clear();
            _list.Clear();
            _recycled = null;
        }
    }
}
