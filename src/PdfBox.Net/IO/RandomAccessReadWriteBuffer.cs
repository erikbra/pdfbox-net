/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 * PDFBOX_SOURCE_PATH: io/src/main/java/org/apache/pdfbox/io/RandomAccessReadWriteBuffer.java
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

namespace PdfBox.Net.IO;

/// <summary>
/// An implementation of the RandomAccess interface to store data in memory. The data will be stored in chunks organized
/// in an ArrayList. The data can be read after writing.
/// </summary>
public class RandomAccessReadWriteBuffer : RandomAccessReadBuffer, RandomAccess
{
    /// <summary>
    /// Default constructor.
    /// </summary>
    public RandomAccessReadWriteBuffer()
    {
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public RandomAccessReadWriteBuffer(int definedChunkSize)
        : base(definedChunkSize)
    {
    }

    public void Clear()
    {
        CheckClosed();
        ResetBuffers();
    }

    public void Write(int b)
    {
        CheckClosed();

        if (_chunkSize - _currentBufferPointer <= 0)
        {
            ExpandBuffer();
        }

        _currentBuffer![_currentBufferPointer++] = (byte)b;
        _pointer++;
        if (_pointer > _size)
        {
            _size = _pointer;
        }
    }

    public void Write(byte[] b)
    {
        Write(b, 0, b.Length);
    }

    public void Write(byte[] b, int offset, int length)
    {
        CheckClosed();

        int remaining = length;
        int bufferOffset = offset;

        while (remaining > 0)
        {
            int bytesToWrite = Math.Min(remaining, _chunkSize - _currentBufferPointer);
            if (bytesToWrite <= 0)
            {
                ExpandBuffer();
                bytesToWrite = Math.Min(remaining, _chunkSize - _currentBufferPointer);
            }

            if (bytesToWrite > 0)
            {
                Array.Copy(b, bufferOffset, _currentBuffer!, _currentBufferPointer, bytesToWrite);
                _currentBufferPointer += bytesToWrite;
                _pointer += bytesToWrite;
            }

            bufferOffset += bytesToWrite;
            remaining -= bytesToWrite;
        }

        if (_pointer > _size)
        {
            _size = _pointer;
        }
    }
}
