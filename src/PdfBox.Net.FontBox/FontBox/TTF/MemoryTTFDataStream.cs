/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/RandomAccessReadDataStream.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
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

using System.IO;
using PdfBox.Net.IO;

namespace PdfBox.Net.FontBox.TTF;

/// <summary>
/// An implementation of the TTFDataStream using buffered in-memory data.
/// </summary>
public sealed class MemoryTTFDataStream : TTFDataStream
{
    private readonly long _length;
    private readonly byte[] _data;
    private int _currentPosition;

    public MemoryTTFDataStream(byte[] data)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
        _length = data.Length;
    }

    public MemoryTTFDataStream(Stream inputStream)
    {
        ArgumentNullException.ThrowIfNull(inputStream);
        using MemoryStream memory = new();
        inputStream.CopyTo(memory);
        _data = memory.ToArray();
        _length = _data.Length;
    }

    public MemoryTTFDataStream(RandomAccessRead randomAccessRead)
    {
        ArgumentNullException.ThrowIfNull(randomAccessRead);
        _length = randomAccessRead.Length();
        if (_length > int.MaxValue - 8)
        {
            throw new IOException($"Stream is too long, size: {_length}");
        }

        _data = new byte[(int)_length];
        int remainingBytes = _data.Length;
        int amountRead;
        while ((amountRead = randomAccessRead.Read(_data, _data.Length - remainingBytes, remainingBytes)) > 0)
        {
            remainingBytes -= amountRead;
        }
    }

    public override long GetCurrentPosition()
    {
        return _currentPosition;
    }

    public override void Close()
    {
        // nothing to do
    }

    public override int Read()
    {
        if (_currentPosition >= _length)
        {
            return -1;
        }

        return _data[_currentPosition++] & 0xff;
    }

    public override long ReadLong()
    {
        return ((long)ReadInt() << 32) + (ReadInt() & 0xFFFFFFFFL);
    }

    private int ReadInt()
    {
        int b1 = Read();
        int b2 = Read();
        int b3 = Read();
        int b4 = Read();
        return (b1 << 24) + (b2 << 16) + (b3 << 8) + b4;
    }

    public override void Seek(long pos)
    {
        if (pos < 0)
        {
            throw new IOException($"Invalid position {pos}");
        }

        _currentPosition = pos < _length ? (int)pos : (int)_length;
    }

    public override int Read(byte[] b, int off, int len)
    {
        if (_currentPosition >= _length)
        {
            return -1;
        }

        int remainingBytes = (int)(_length - _currentPosition);
        int bytesToRead = Math.Min(remainingBytes, len);
        Array.Copy(_data, _currentPosition, b, off, bytesToRead);
        _currentPosition += bytesToRead;
        return bytesToRead;
    }

    public override RandomAccessRead? CreateSubView(long length)
    {
        try
        {
            RandomAccessReadBuffer buffer = new(_data);
            return buffer.CreateView(_currentPosition, length);
        }
        catch
        {
            return null;
        }
    }

    public override Stream GetOriginalData()
    {
        return new MemoryStream(_data, writable: false);
    }

    public override long GetOriginalDataSize()
    {
        return _length;
    }
}
