/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: io/src/main/java/org/apache/pdfbox/io/ScratchFile.java
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

namespace PdfBox.Net.IO;

/// <summary>
/// Implements a memory page handling mechanism as base for creating (multiple)
/// <see cref="RandomAccess"/> buffers each having their own set of pages (implemented by
/// <see cref="ScratchFileBuffer"/>). A buffer is created by calling <see cref="CreateBuffer"/>.
///
/// <para>Pages can be stored in main memory or in a temporary file. A mixed mode
/// is supported, storing a certain amount of pages in memory and only the
/// additional ones in temporary file (defined by maximum main memory to be used).</para>
///
/// <para>Pages can be marked as 'free' in order to re-use them. For in-memory pages
/// this will release the used memory, while for pages in temporary file this
/// simply marks the area as free to re-use.</para>
///
/// <para>If a temporary file was created (done with the first page to be stored
/// in temporary file) it is deleted when <see cref="Close"/> is called.</para>
///
/// <para>This base class for providing pages is thread safe (the buffer implementations are not).</para>
/// </summary>
public class ScratchFile : RandomAccessStreamCache
{
    /// <summary>Number of pages by which the scratch file is enlarged to reduce I/O operations.</summary>
    private const int EnlargePageCount = 16;

    /// <summary>
    /// In case of unrestricted main memory usage this is the initial number of pages
    /// that <see cref="_inMemoryPages"/> is set up for.
    /// </summary>
    private const int InitUnrestrictedMainMemPageCount = 100000;

    /// <summary>The fixed size in bytes of each page.</summary>
    internal const int PageSize = 4096;

    private readonly object _ioLock = new();
    private readonly object _freePageLock = new();

    private readonly DirectoryInfo? _scratchFileDirectory;

    /// <summary>The scratch file; only accessed under synchronization of <see cref="_ioLock"/>.</summary>
    private FileInfo? _scratchFileInfo;

    /// <summary>Random access stream to the scratch file; only accessed under synchronization of <see cref="_ioLock"/>.</summary>
    private FileStream? _raf;

    private volatile int _pageCount;

    /// <summary>Bit array tracking which page indices are free (true = free).</summary>
    private int[] _freePageBits = Array.Empty<int>();

    private int _freeBitsCapacity;

    /// <summary>
    /// Holds pointers to in-memory page content; will be initialized once in the case of restricted
    /// main memory, otherwise it is enlarged as needed and first initialized to a size of
    /// <see cref="InitUnrestrictedMainMemPageCount"/>.
    /// </summary>
    private volatile byte[][]? _inMemoryPages;

    private readonly int _inMemoryMaxPageCount;
    private readonly int _maxPageCount;
    private readonly bool _useScratchFile;
    private readonly bool _maxMainMemoryIsRestricted;

    private readonly List<ScratchFileBuffer> _buffers = new();

    private volatile bool _isClosed;

    /// <summary>
    /// Initializes page handler. If a <paramref name="scratchFileDirectory"/> is supplied,
    /// the scratch file will be created in that directory.
    ///
    /// <para>All pages will be stored in the scratch file.</para>
    /// </summary>
    /// <param name="scratchFileDirectory">
    /// The directory in which to create the scratch file, or <c>null</c> to create it in the
    /// default temporary directory.
    /// </param>
    public ScratchFile(DirectoryInfo? scratchFileDirectory)
        : this(MemoryUsageSetting.SetupTempFileOnly()
            .SetTempDir(scratchFileDirectory ?? new DirectoryInfo(Path.GetTempPath())))
    {
    }

    /// <summary>
    /// Initializes page handler using the provided memory usage settings.
    ///
    /// <para>Depending on the size of allowed memory usage, a number of pages
    /// (memorySize/<see cref="PageSize"/>) will be stored in-memory and only additional
    /// pages will be written to/read from the scratch file.</para>
    /// </summary>
    /// <param name="memUsageSetting">Controls how memory/temporary files are used for buffering.</param>
    public ScratchFile(MemoryUsageSetting memUsageSetting)
    {
        _maxMainMemoryIsRestricted = !memUsageSetting.UseMainMemory() || memUsageSetting.IsMainMemoryRestricted();
        _useScratchFile = _maxMainMemoryIsRestricted && memUsageSetting.UseTempFile();
        _scratchFileDirectory = _useScratchFile ? memUsageSetting.GetTempDir() : null;

        if (_scratchFileDirectory != null && !_scratchFileDirectory.Exists)
        {
            throw new IOException("Scratch file directory does not exist: " + _scratchFileDirectory.FullName);
        }

        _maxPageCount = memUsageSetting.IsStorageRestricted()
            ? (int)Math.Min(int.MaxValue, memUsageSetting.GetMaxStorageBytes() / PageSize)
            : int.MaxValue;

        _inMemoryMaxPageCount = memUsageSetting.UseMainMemory()
            ? (memUsageSetting.IsMainMemoryRestricted()
                ? (int)Math.Min(int.MaxValue, memUsageSetting.GetMaxMainMemoryBytes() / PageSize)
                : int.MaxValue)
            : 0;
    }

    /// <summary>
    /// Returns an instance configured to use only unrestricted main memory for buffering
    /// (equivalent to <c>new ScratchFile(MemoryUsageSetting.SetupMainMemoryOnly())</c>).
    /// </summary>
    public static ScratchFile GetMainMemoryOnlyInstance()
    {
        return new ScratchFile(MemoryUsageSetting.SetupMainMemoryOnly());
    }

    /// <summary>
    /// Returns an instance configured to use only main memory with the defined maximum.
    /// </summary>
    /// <param name="maxMainMemoryBytes">
    /// Maximum number of main-memory bytes to use; <c>-1</c> for no restriction.
    /// </param>
    public static ScratchFile GetMainMemoryOnlyInstance(long maxMainMemoryBytes)
    {
        return new ScratchFile(MemoryUsageSetting.SetupMainMemoryOnly(maxMainMemoryBytes));
    }

    private void InitPages()
    {
        // Called only within _freePageLock.
        if (_inMemoryPages == null)
        {
            int initSize = _maxMainMemoryIsRestricted
                ? _inMemoryMaxPageCount
                : InitUnrestrictedMainMemPageCount;

            _inMemoryPages = new byte[initSize][];
            SetFreeBits(0, initSize);
        }
    }

    // ---- bit-array helpers ----

    private void SetFreeBits(int from, int toExclusive)
    {
        EnsureBitCapacity(toExclusive);
        for (int i = from; i < toExclusive; i++)
        {
            _freePageBits[i >> 5] |= 1 << (i & 31);
        }
    }

    private void ClearFreeBit(int idx)
    {
        EnsureBitCapacity(idx + 1);
        _freePageBits[idx >> 5] &= ~(1 << (idx & 31));
    }

    private bool GetFreeBit(int idx)
    {
        int word = idx >> 5;
        return word < _freePageBits.Length && (_freePageBits[word] & (1 << (idx & 31))) != 0;
    }

    private int NextSetBit(int fromIdx)
    {
        for (int word = fromIdx >> 5; word < _freePageBits.Length; word++)
        {
            int bits = _freePageBits[word];
            if (bits == 0)
            {
                continue;
            }

            int startBit = word == (fromIdx >> 5) ? fromIdx & 31 : 0;
            // mask off bits below startBit
            bits &= unchecked((int)(0xFFFFFFFFu << startBit));
            if (bits != 0)
            {
                return (word << 5) | TrailingZeroCount(bits);
            }
        }

        return -1;
    }

    private static int TrailingZeroCount(int x)
    {
        if (x == 0) return 32;
        int n = 0;
        if ((x & 0x0000FFFF) == 0) { n += 16; x >>= 16; }
        if ((x & 0x000000FF) == 0) { n += 8; x >>= 8; }
        if ((x & 0x0000000F) == 0) { n += 4; x >>= 4; }
        if ((x & 0x00000003) == 0) { n += 2; x >>= 2; }
        if ((x & 0x00000001) == 0) { n++; }
        return n;
    }

    private void EnsureBitCapacity(int minBits)
    {
        int needed = (minBits + 31) >> 5;
        if (needed > _freePageBits.Length)
        {
            int newLen = Math.Max(needed, _freePageBits.Length * 2);
            Array.Resize(ref _freePageBits, newLen);
            _freeBitsCapacity = newLen << 5;
        }
    }

    // ---- page allocation ----

    /// <summary>
    /// Returns the index of a new free page, either from the free page pool
    /// or by enlarging the scratch file (may be created on first use).
    /// </summary>
    internal int GetNewPage()
    {
        lock (_freePageLock)
        {
            InitPages();
            int idx = NextSetBit(0);

            if (idx < 0)
            {
                Enlarge();
                idx = NextSetBit(0);
                if (idx < 0)
                {
                    throw new IOException("Maximum allowed scratch file memory exceeded.");
                }
            }

            ClearFreeBit(idx);

            if (idx >= _pageCount)
            {
                _pageCount = idx + 1;
            }

            return idx;
        }
    }

    /// <summary>
    /// Enlarges the scratch file (or in-memory page array) to provide new free pages.
    /// Called only from within <see cref="_freePageLock"/>.
    /// </summary>
    private void Enlarge()
    {
        lock (_ioLock)
        {
            CheckClosed();

            if (_pageCount >= _maxPageCount)
            {
                return;
            }

            if (_useScratchFile)
            {
                if (_raf == null)
                {
                    string tempPath;
                    if (_scratchFileDirectory == null)
                    {
                        tempPath = Path.GetTempFileName();
                    }
                    else
                    {
                        tempPath = Path.Combine(_scratchFileDirectory.FullName,
                            "PDFBox" + Path.GetRandomFileName() + ".tmp");
                    }

                    _scratchFileInfo = new FileInfo(tempPath);
                    _raf = new FileStream(tempPath, FileMode.Create, FileAccess.ReadWrite,
                        FileShare.None, 4096, FileOptions.RandomAccess);
                }

                long fileLen = _raf.Length;
                long expectedFileLen = ((long)_pageCount - _inMemoryMaxPageCount) * PageSize;

                if (expectedFileLen != fileLen)
                {
                    throw new IOException(
                        $"Expected scratch file size of {expectedFileLen} but found {fileLen}");
                }

                int newPageCount = _pageCount + EnlargePageCount;
                if (newPageCount > _pageCount) // overflow guard
                {
                    _raf.SetLength(((long)newPageCount - _inMemoryMaxPageCount) * PageSize);
                    SetFreeBits(_pageCount, newPageCount);
                }
            }
            else if (!_maxMainMemoryIsRestricted)
            {
                // increase in-memory page array
                int oldSize = _inMemoryPages!.Length;
                int newSize = (int)Math.Min((long)oldSize * 2, int.MaxValue);
                if (newSize > oldSize)
                {
                    byte[][] newPages = new byte[newSize][];
                    Array.Copy(_inMemoryPages, newPages, oldSize);
                    _inMemoryPages = newPages;
                    SetFreeBits(oldSize, newSize);
                }
            }
        }
    }

    /// <summary>Returns the fixed page size in bytes.</summary>
    internal int GetPageSize() => PageSize;

    /// <summary>Reads the page with the specified index.</summary>
    internal byte[] ReadPage(int pageIdx)
    {
        if (pageIdx < 0 || pageIdx >= _pageCount)
        {
            CheckClosed();
            throw new IOException($"Page index out of range: {pageIdx}. Max value: {_pageCount - 1}");
        }

        if (pageIdx < _inMemoryMaxPageCount)
        {
            byte[]? page = _inMemoryPages![pageIdx];
            if (page == null)
            {
                CheckClosed();
                throw new IOException(
                    $"Requested page with index {pageIdx} was not written before.");
            }

            return page;
        }

        lock (_ioLock)
        {
            if (_raf == null)
            {
                CheckClosed();
                throw new IOException(
                    $"Missing scratch file to read page with index {pageIdx} from.");
            }

            byte[] page = new byte[PageSize];
            _raf.Seek(((long)pageIdx - _inMemoryMaxPageCount) * PageSize, SeekOrigin.Begin);
            int totalRead = 0;
            while (totalRead < PageSize)
            {
                int n = _raf.Read(page, totalRead, PageSize - totalRead);
                if (n <= 0) break;
                totalRead += n;
            }

            return page;
        }
    }

    /// <summary>
    /// Writes updated page. Page is kept in-memory if <paramref name="pageIdx"/> &lt; <see cref="_inMemoryMaxPageCount"/>,
    /// otherwise it is written to the scratch file.
    /// </summary>
    internal void WritePage(int pageIdx, byte[] page)
    {
        if (pageIdx < 0 || pageIdx >= _pageCount)
        {
            CheckClosed();
            throw new IOException($"Page index out of range: {pageIdx}. Max value: {_pageCount - 1}");
        }

        if (page.Length != PageSize)
        {
            throw new IOException(
                $"Wrong page size to write: {page.Length}. Expected: {PageSize}");
        }

        if (pageIdx < _inMemoryMaxPageCount)
        {
            if (_maxMainMemoryIsRestricted)
            {
                _inMemoryPages![pageIdx] = page;
            }
            else
            {
                lock (_ioLock)
                {
                    _inMemoryPages![pageIdx] = page;
                }
            }

            CheckClosed();
        }
        else
        {
            lock (_ioLock)
            {
                CheckClosed();
                _raf!.Seek(((long)pageIdx - _inMemoryMaxPageCount) * PageSize, SeekOrigin.Begin);
                _raf.Write(page, 0, PageSize);
            }
        }
    }

    /// <summary>
    /// Checks if this page handler has already been closed.
    /// </summary>
    internal void CheckClosed()
    {
        if (_isClosed)
        {
            throw new IOException("Scratch file already closed");
        }
    }

    /// <summary>Creates a new buffer using this page handler.</summary>
    public RandomAccess CreateBuffer()
    {
        var newBuffer = new ScratchFileBuffer(this);
        lock (_buffers)
        {
            _buffers.Add(newBuffer);
        }

        return newBuffer;
    }

    internal void RemoveBuffer(ScratchFileBuffer buffer)
    {
        lock (_buffers)
        {
            _buffers.Remove(buffer);
        }
    }

    /// <summary>
    /// Allows a buffer that is cleared/closed to release its pages for re-use.
    /// </summary>
    internal void MarkPagesAsFree(int[] pageIndexes, int off, int count)
    {
        lock (_freePageLock)
        {
            for (int aIdx = off; aIdx < count; aIdx++)
            {
                int pageIdx = pageIndexes[aIdx];
                if (pageIdx >= 0 && pageIdx < _pageCount && !GetFreeBit(pageIdx))
                {
                    SetFreeBits(pageIdx, pageIdx + 1);
                    if (pageIdx < _inMemoryMaxPageCount)
                    {
                        _inMemoryPages![pageIdx] = null!;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Closes and deletes the temporary file. No further interaction with
    /// the scratch file or associated buffers can happen after this method is called.
    /// It also releases in-memory pages.
    /// </summary>
    public void Close()
    {
        IOException? ioexc = null;

        lock (_ioLock)
        {
            if (_isClosed)
            {
                return;
            }

            _isClosed = true;

            List<ScratchFileBuffer> buffersSnapshot;
            lock (_buffers)
            {
                buffersSnapshot = new List<ScratchFileBuffer>(_buffers);
                _buffers.Clear();
            }

            foreach (ScratchFileBuffer buffer in buffersSnapshot)
            {
                if (!buffer.IsClosed())
                {
                    try
                    {
                        buffer.Close(false);
                    }
                    catch
                    {
                        // ignore close exceptions
                    }
                }
            }

            if (_raf != null)
            {
                try
                {
                    _raf.Close();
                }
                catch (IOException ioe)
                {
                    ioexc = ioe;
                }
            }

            if (_scratchFileInfo != null)
            {
                try
                {
                    _scratchFileInfo.Refresh();
                    if (_scratchFileInfo.Exists)
                    {
                        File.Delete(_scratchFileInfo.FullName);
                    }
                }
                catch (IOException ioe) when (ioexc == null)
                {
                    ioexc = ioe;
                }
            }
        }

        lock (_freePageLock)
        {
            _freePageBits = Array.Empty<int>();
            _pageCount = 0;
        }

        if (ioexc != null)
        {
            throw ioexc;
        }
    }
}
