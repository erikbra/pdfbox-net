/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cff/DataInputByteArray.java
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

namespace PdfBox.Net.FontBox.CFF;

public sealed class DataInputByteArray(byte[] buffer) : DataInput
{
    private readonly byte[] _buffer = buffer;
    private int _position;

    public bool HasRemaining() => _position < _buffer.Length;

    public int GetPosition() => _position;

    public void SetPosition(int position)
    {
        if (position < 0)
        {
            throw new IOException("position is negative");
        }

        if (position >= _buffer.Length)
        {
            throw new IOException($"New position is out of range {position} >= {_buffer.Length}");
        }

        _position = position;
    }

    public byte ReadByte()
    {
        if (!HasRemaining())
        {
            throw new IOException("End of buffer reached");
        }

        return _buffer[_position++];
    }

    public int ReadUnsignedByte()
    {
        if (!HasRemaining())
        {
            throw new IOException("End of buffer reached");
        }

        return _buffer[_position++] & 0xFF;
    }

    public int PeekUnsignedByte(int offset)
    {
        if (offset < 0)
        {
            throw new IOException("offset is negative");
        }

        int index = _position + offset;
        if (index >= _buffer.Length)
        {
            throw new IOException($"Offset position is out of range {index} >= {_buffer.Length}");
        }

        return _buffer[index] & 0xFF;
    }

    public byte[] ReadBytes(int length)
    {
        if (length < 0)
        {
            throw new IOException("length is negative");
        }

        if (_buffer.Length - _position < length)
        {
            throw new IOException("Premature end of buffer reached");
        }

        byte[] bytes = new byte[length];
        Array.Copy(_buffer, _position, bytes, 0, length);
        _position += length;
        return bytes;
    }

    public int Length() => _buffer.Length;
}
