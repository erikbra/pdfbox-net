/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache FontBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/RandomAccessReadDataStream.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: trunk
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

namespace PdfBox.Net.FontBox.TTF;

public sealed class MemoryTTFDataStream : TTFDataStream
{
    private readonly byte[] _data;
    private int _position;

    public MemoryTTFDataStream(byte[] data)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
    }

    public MemoryTTFDataStream(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        byte[] bytes = new byte[stream.Length];
        stream.ReadExactly(bytes, 0, bytes.Length);
        _data = bytes;
    }

    public override long Position => _position;

    public override long Length => _data.Length;

    public override void Seek(long position)
    {
        if (position < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        _position = position > Length ? _data.Length : (int)position;
    }

    public override int Read()
    {
        if (_position >= _data.Length)
        {
            return -1;
        }

        return _data[_position++];
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (_position >= _data.Length)
        {
            return -1;
        }

        int bytesToCopy = Math.Min(count, _data.Length - _position);
        Buffer.BlockCopy(_data, _position, buffer, offset, bytesToCopy);
        _position += bytesToCopy;
        return bytesToCopy;
    }
}
