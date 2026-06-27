/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: io/src/main/java/org/apache/pdfbox/io/RandomAccessInputStream.java
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
/// A stream which reads from a <see cref="RandomAccessRead"/>.
/// </summary>
public class RandomAccessInputStream : Stream
{
    private readonly RandomAccessRead _input;
    private long _position;

    /// <summary>
    /// Creates a new RandomAccessInputStream with a position of zero.
    /// The stream maintains its own position independent of the RandomAccessRead.
    /// </summary>
    /// <param name="randomAccessRead">The RandomAccessRead to read from.</param>
    public RandomAccessInputStream(RandomAccessRead randomAccessRead)
    {
        _input = randomAccessRead;
        _position = 0;
    }

    private void RestorePosition()
    {
        _input.Seek(_position);
    }

    public int Available()
    {
        return (int)Math.Max(0, Math.Min(_input.Length() - _position, int.MaxValue));
    }

    public int Read()
    {
        return ReadByte();
    }

    public long Skip(long n)
    {
        if (n <= 0)
        {
            return 0;
        }

        RestorePosition();
        _input.Seek(_position + n);
        _position += n;
        return n;
    }

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length => _input.Length();

    public override long Position
    {
        get => _position;
        set => _position = value;
    }

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        RestorePosition();
        if (_input.IsEOF())
        {
            return 0;
        }

        int n = _input.Read(buffer, offset, count);
        if (n > 0)
        {
            _position += n;
        }

        return n > -1 ? n : 0;
    }

    public override int ReadByte()
    {
        RestorePosition();
        if (_input.IsEOF())
        {
            return -1;
        }

        int b = _input.Read();
        if (b != -1)
        {
            _position++;
        }

        return b;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        _position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => _position + offset,
            SeekOrigin.End => _input.Length() + offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, null)
        };
        return _position;
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }
}
