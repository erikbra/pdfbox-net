/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cff/DataInputRandomAccessRead.java
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

using PdfBox.Net.IO;

namespace PdfBox.Net.FontBox.CFF;

public sealed class DataInputRandomAccessRead(RandomAccessRead randomAccessRead) : DataInput
{
    private readonly RandomAccessRead _randomAccessRead = randomAccessRead;

    public bool HasRemaining() => _randomAccessRead.Available() > 0;

    public int GetPosition() => (int)_randomAccessRead.GetPosition();

    public void SetPosition(int position)
    {
        if (position < 0)
        {
            throw new IOException("position is negative");
        }

        if (position >= _randomAccessRead.Length())
        {
            throw new IOException($"New position is out of range {position} >= {_randomAccessRead.Length()}");
        }

        _randomAccessRead.Seek(position);
    }

    public byte ReadByte()
    {
        if (!HasRemaining())
        {
            throw new IOException("End of buffer reached!");
        }

        return (byte)_randomAccessRead.Read();
    }

    public int ReadUnsignedByte()
    {
        if (!HasRemaining())
        {
            throw new IOException("End of buffer reached!");
        }

        return _randomAccessRead.Read();
    }

    public int PeekUnsignedByte(int offset)
    {
        if (offset < 0)
        {
            throw new IOException("offset is negative");
        }

        if (offset == 0)
        {
            return _randomAccessRead.Peek();
        }

        long currentPosition = _randomAccessRead.GetPosition();
        if (currentPosition + offset >= _randomAccessRead.Length())
        {
            throw new IOException($"Offset position is out of range {currentPosition + offset} >= {_randomAccessRead.Length()}");
        }

        _randomAccessRead.Seek(currentPosition + offset);
        int value = _randomAccessRead.Read();
        _randomAccessRead.Seek(currentPosition);
        return value;
    }

    public byte[] ReadBytes(int length)
    {
        if (length < 0)
        {
            throw new IOException("length is negative");
        }

        byte[] bytes = new byte[length];
        _randomAccessRead.ReadFully(bytes);
        return bytes;
    }

    public int Length() => (int)_randomAccessRead.Length();
}
