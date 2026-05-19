/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: io/src/main/java/org/apache/pdfbox/io/RandomAccessReadMemoryMappedFile.java
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
using System.IO.MemoryMappedFiles;

namespace PdfBox.Net.IO;

/// <summary>
/// An implementation of the <see cref="RandomAccessRead"/> interface backed by a memory-mapped
/// file. The whole file is mapped to memory and the maximum file size is limited to
/// <see cref="int.MaxValue"/> bytes.
/// </summary>
public class RandomAccessReadMemoryMappedFile : RandomAccessRead
{
    // the memory-mapped file (only set on the owning instance, not on view copies)
    private readonly MemoryMappedFile? _mappedFile;

    // the view accessor used for reading (null for empty files, null after Close)
    private MemoryMappedViewAccessor? _accessor;

    // current read position
    private long _position;

    // size of the whole file
    private readonly long _size;

    // whether this instance owns the underlying MemoryMappedFile (false for view copies)
    private readonly bool _ownsMapping;

    // explicit closed flag (needed to distinguish "empty file" from "closed")
    private bool _isClosed;

    /// <summary>
    /// Creates a random access memory-mapped file instance for the file at the given path.
    /// </summary>
    /// <param name="filePath">Path of the file to be read.</param>
    public RandomAccessReadMemoryMappedFile(string filePath)
    {
        long fileSize = new FileInfo(filePath).Length;
        if (fileSize > int.MaxValue)
        {
            throw new IOException(
                $"{GetType().Name} doesn't yet support files bigger than {int.MaxValue}");
        }

        _size = fileSize;
        if (_size > 0)
        {
            _mappedFile = MemoryMappedFile.CreateFromFile(
                filePath,
                FileMode.Open,
                mapName: null,
                capacity: 0,
                MemoryMappedFileAccess.Read);
            _accessor = _mappedFile.CreateViewAccessor(0, _size, MemoryMappedFileAccess.Read);
        }

        _ownsMapping = true;
    }

    /// <summary>
    /// Creates a random access memory-mapped file instance for the given file.
    /// </summary>
    public RandomAccessReadMemoryMappedFile(FileInfo file) : this(file.FullName)
    {
    }

    /// <summary>Private copy constructor used by <see cref="CreateView"/>.</summary>
    private RandomAccessReadMemoryMappedFile(RandomAccessReadMemoryMappedFile parent)
    {
        _size = parent._size;
        _mappedFile = parent._mappedFile;
        if (_mappedFile != null)
        {
            _accessor = _mappedFile.CreateViewAccessor(0, _size, MemoryMappedFileAccess.Read);
        }

        _ownsMapping = false;
    }

    public void Close()
    {
        if (IsClosed())
        {
            return;
        }

        _isClosed = true;

        _accessor?.Dispose();
        _accessor = null;

        if (_ownsMapping)
        {
            _mappedFile?.Dispose();
        }
    }

    public void Seek(long position)
    {
        CheckClosed();
        if (position < 0)
        {
            throw new IOException($"Invalid position {position}");
        }

        // it is allowed to jump beyond the end of the file
        _position = Math.Min(position, _size);
    }

    public long GetPosition()
    {
        CheckClosed();
        return _position;
    }

    public int Read()
    {
        CheckClosed();
        if (_position >= _size)
        {
            return -1;
        }

        return _accessor!.ReadByte(_position++) & 0xff;
    }

    public int Read(byte[] b, int offset, int length)
    {
        CheckClosed();
        if (_position >= _size)
        {
            return -1;
        }

        int remainingBytes = (int)(_size - _position);
        int bytesToRead = Math.Min(remainingBytes, length);
        _accessor!.ReadArray(_position, b, offset, bytesToRead);
        _position += bytesToRead;
        return bytesToRead;
    }

    public long Length()
    {
        CheckClosed();
        return _size;
    }

    private void CheckClosed()
    {
        if (IsClosed())
        {
            throw new IOException($"{GetType().Name} already closed");
        }
    }

    public bool IsClosed() => _isClosed;

    public bool IsEOF()
    {
        CheckClosed();
        return _position >= _size;
    }

    public RandomAccessReadView CreateView(long startPosition, long streamLength)
    {
        return new RandomAccessReadView(
            new RandomAccessReadMemoryMappedFile(this),
            startPosition,
            streamLength,
            closeInput: true);
    }
}
