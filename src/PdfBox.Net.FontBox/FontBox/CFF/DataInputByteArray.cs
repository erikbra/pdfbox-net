/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/cff/DataInputByteArray.java
 * PDFBOX_SOURCE_COMMIT: a038be4895b922f2f91a1bbdedce40a3b082fd8c
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: a038be4895b922f2f91a1bbdedce40a3b082fd8c
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

/// <summary>
/// This class implements the DataInput interface using a byte buffer as source.
/// </summary>
public class DataInputByteArray : DataInput
{
    private readonly byte[] _inputBuffer;
    private int _bufferPosition;

    public DataInputByteArray(byte[] buffer)
    {
        _inputBuffer = buffer;
        _bufferPosition = 0;
    }

    public bool HasRemaining() => _bufferPosition < _inputBuffer.Length;

    public int GetPosition() => _bufferPosition;

    public void SetPosition(int position)
    {
        if (position < 0)
        {
            throw new IOException("position is negative");
        }

        if (position >= _inputBuffer.Length)
        {
            throw new IOException($"New position is out of range {position} >= {_inputBuffer.Length}");
        }

        _bufferPosition = position;
    }

    public byte ReadByte()
    {
        if (!HasRemaining())
        {
            throw new IOException("End of buffer reached");
        }

        return _inputBuffer[_bufferPosition++];
    }

    public int ReadUnsignedByte()
    {
        if (!HasRemaining())
        {
            throw new IOException("End of buffer reached");
        }

        return _inputBuffer[_bufferPosition++] & 0xff;
    }

    public int PeekUnsignedByte(int offset)
    {
        if (offset < 0)
        {
            throw new IOException("offset is negative");
        }

        if (_bufferPosition + offset >= _inputBuffer.Length)
        {
            throw new IOException($"Offset position is out of range {_bufferPosition + offset} >= {_inputBuffer.Length}");
        }

        return _inputBuffer[_bufferPosition + offset] & 0xff;
    }

    public byte[] ReadBytes(int length)
    {
        if (length < 0)
        {
            throw new IOException("length is negative");
        }

        if (_inputBuffer.Length - _bufferPosition < length)
        {
            throw new IOException("Premature end of buffer reached");
        }

        byte[] bytes = new byte[length];
        Array.Copy(_inputBuffer, _bufferPosition, bytes, 0, length);
        _bufferPosition += length;
        return bytes;
    }

    public int Length() => _inputBuffer.Length;
}
