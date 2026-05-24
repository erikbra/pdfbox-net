/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/RandomAccessReadUnbufferedDataStream.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: mechanical
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

using PdfBox.Net.IO;

namespace PdfBox.Net.FontBox.TTF;

/// <summary>
/// In contrast to <see cref="RandomAccessReadDataStream"/>,
/// this class doesn't pre-load <see cref="RandomAccessRead"/> into a byte array;
/// it works with <see cref="RandomAccessRead"/> directly.
/// Performance: it is much faster if most of the buffer is skipped, and slower if whole buffer is read.
/// </summary>
internal class RandomAccessReadUnbufferedDataStream : TTFDataStream
{
    private readonly long _length;
    private readonly RandomAccessRead _randomAccessRead;

    internal RandomAccessReadUnbufferedDataStream(RandomAccessRead randomAccessRead)
    {
        _length = randomAccessRead.Length();
        _randomAccessRead = randomAccessRead;
    }

    public override long GetCurrentPosition()
    {
        return _randomAccessRead.GetPosition();
    }

    /// <summary>
    /// Close the underlying resources.
    /// </summary>
    public override void Close()
    {
        _randomAccessRead.Close();
    }

    public override int Read()
    {
        return _randomAccessRead.Read();
    }

    public override long ReadLong()
    {
        return ((long)ReadInt() << 32) | (ReadInt() & 0xFFFFFFFFL);
    }

    private int ReadInt()
    {
        int b1 = Read();
        int b2 = Read();
        int b3 = Read();
        int b4 = Read();
        return (b1 << 24) | (b2 << 16) | (b3 << 8) | b4;
    }

    public override void Seek(long pos)
    {
        _randomAccessRead.Seek(pos);
    }

    public override int Read(byte[] b, int off, int len)
    {
        return _randomAccessRead.Read(b, off, len);
    }

    /// <summary>
    /// Lifetime of returned Stream is bound by this lifetime; it won't close the underlying RandomAccessRead.
    /// </summary>
    public override Stream GetOriginalData()
    {
        return new RandomAccessReadNonClosingStream(_randomAccessRead.CreateView(0, _length));
    }

    public override long GetOriginalDataSize()
    {
        return _length;
    }

    public override RandomAccessRead? CreateSubView(long length)
    {
        try
        {
            return _randomAccessRead.CreateView(_randomAccessRead.GetPosition(), length);
        }
        catch (IOException)
        {
            return null;
        }
    }

    /// <summary>
    /// A stream that reads from a <see cref="RandomAccessReadView"/> without closing it on Dispose.
    /// </summary>
    private sealed class RandomAccessReadNonClosingStream : Stream
    {
        private readonly RandomAccessReadView _randomAccessRead;

        internal RandomAccessReadNonClosingStream(RandomAccessReadView randomAccessRead)
        {
            _randomAccessRead = randomAccessRead;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _randomAccessRead.Length();
        public override long Position
        {
            get => _randomAccessRead.GetPosition();
            set => _randomAccessRead.Seek(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _randomAccessRead.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            return _randomAccessRead.Read();
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override void Flush() { }

        protected override void Dispose(bool disposing)
        {
            // WARNING: do NOT close _randomAccessRead here — its lifetime is controlled externally
            base.Dispose(disposing);
        }
    }
}
