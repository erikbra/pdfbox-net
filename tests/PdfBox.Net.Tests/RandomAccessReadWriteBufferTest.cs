/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: io/src/test/java/org/apache/pdfbox/io/RandomAccessReadWriteBufferTest.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

/*
 * Copyright 2014 The Apache Software Foundation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
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
using Xunit;

namespace PdfBox.Net.Tests;

/// <summary>
/// Unittest for <see cref="RandomAccessReadWriteBuffer"/>.
/// </summary>
public class RandomAccessReadWriteBufferTest
{
    private const int NumIterations = 3;

    [Fact]
    public void TestClose()
    {
        var randomAccessReadWrite = new RandomAccessReadWriteBuffer();
        try
        {
            randomAccessReadWrite.Write([1, 2, 3, 4]);
            Assert.False(randomAccessReadWrite.IsClosed());
            randomAccessReadWrite.Close();
            Assert.True(randomAccessReadWrite.IsClosed());
        }
        finally
        {
            if (!randomAccessReadWrite.IsClosed())
            {
                randomAccessReadWrite.Close();
            }
        }
    }

    [Fact]
    public void TestClear()
    {
        var randomAccessReadWrite = new RandomAccessReadWriteBuffer(4);
        try
        {
            randomAccessReadWrite.Write([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
            Assert.Equal(10, randomAccessReadWrite.Length());
            Assert.Equal(10, randomAccessReadWrite.GetPosition());
            randomAccessReadWrite.Clear();
            Assert.False(randomAccessReadWrite.IsClosed());
            Assert.Equal(0, randomAccessReadWrite.Length());
            Assert.Equal(0, randomAccessReadWrite.GetPosition());
        }
        finally
        {
            randomAccessReadWrite.Close();
        }
    }

    [Fact]
    public void TestLengthWriteByte()
    {
        var randomAccessReadWrite = new RandomAccessReadWriteBuffer();
        try
        {
            Assert.Equal(0, randomAccessReadWrite.Length());
            randomAccessReadWrite.Write(1);
            randomAccessReadWrite.Write(2);
            randomAccessReadWrite.Write(3);
            Assert.Equal(3, randomAccessReadWrite.Length());
        }
        finally
        {
            randomAccessReadWrite.Close();
        }
    }

    [Fact]
    public void TestLengthWriteBytes()
    {
        var randomAccessReadWrite = new RandomAccessReadWriteBuffer();
        try
        {
            Assert.Equal(0, randomAccessReadWrite.Length());
            randomAccessReadWrite.Write([1, 2, 3, 4, 5, 6, 7]);
            Assert.Equal(7, randomAccessReadWrite.Length());
            randomAccessReadWrite.Write([8, 9, 10, 11]);
            Assert.Equal(11, randomAccessReadWrite.Length());
        }
        finally
        {
            randomAccessReadWrite.Close();
        }
    }

    [Fact]
    public void TestPaging()
    {
        var randomAccessReadWrite = new RandomAccessReadWriteBuffer(5);
        try
        {
            Assert.Equal(0, randomAccessReadWrite.Length());
            randomAccessReadWrite.Write([1, 2, 3, 4, 5, 6, 7]);
            Assert.Equal(7, randomAccessReadWrite.Length());
            randomAccessReadWrite.Write([8, 9, 10, 11]);
            Assert.Equal(11, randomAccessReadWrite.Length());
        }
        finally
        {
            randomAccessReadWrite.Close();
        }
    }

    [Fact]
    public void TestRandomAccessRead()
    {
        var randomAccessReadWrite = new RandomAccessReadWriteBuffer();
        try
        {
            randomAccessReadWrite.Write([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11]);
            Assert.Equal(11, randomAccessReadWrite.Length());
            randomAccessReadWrite.Seek(0);
            Assert.Equal(11, randomAccessReadWrite.Length());
            byte[] bytesRead = new byte[11];
            Assert.Equal(11, ((RandomAccessRead)randomAccessReadWrite).Read(bytesRead));
            Assert.Equal(1, bytesRead[0]);
            Assert.Equal(7, bytesRead[6]);
            Assert.Equal(8, bytesRead[7]);
            Assert.Equal(11, bytesRead[10]);
        }
        finally
        {
            randomAccessReadWrite.Close();
        }
    }

    [Fact]
    public void TestEOFBugInSeek()
    {
        var randomAccessReadWrite = new RandomAccessReadWriteBuffer();
        try
        {
            byte[] bytes = new byte[RandomAccessReadBuffer.DEFAULT_CHUNK_SIZE_4KB];
            for (int i = 0; i < NumIterations; i++)
            {
                long position0 = randomAccessReadWrite.GetPosition();
                randomAccessReadWrite.Write(bytes);
                long position1 = randomAccessReadWrite.GetPosition();
                Assert.Equal(RandomAccessReadBuffer.DEFAULT_CHUNK_SIZE_4KB, position1 - position0);
                randomAccessReadWrite.Write(bytes);
                long position2 = randomAccessReadWrite.GetPosition();
                Assert.Equal(RandomAccessReadBuffer.DEFAULT_CHUNK_SIZE_4KB, position2 - position1);
                randomAccessReadWrite.Seek(0);
                randomAccessReadWrite.Seek(i * 2 * RandomAccessReadBuffer.DEFAULT_CHUNK_SIZE_4KB);
            }
        }
        finally
        {
            randomAccessReadWrite.Close();
        }
    }

    [Fact]
    public void TestBufferLength()
    {
        var randomAccessReadWrite = new RandomAccessReadWriteBuffer();
        try
        {
            byte[] bytes = new byte[RandomAccessReadBuffer.DEFAULT_CHUNK_SIZE_4KB];
            randomAccessReadWrite.Write(bytes);
            Assert.Equal(RandomAccessReadBuffer.DEFAULT_CHUNK_SIZE_4KB, randomAccessReadWrite.Length());
        }
        finally
        {
            randomAccessReadWrite.Close();
        }
    }

    [Fact]
    public void TestBufferSeek()
    {
        var randomAccessReadWrite = new RandomAccessReadWriteBuffer();
        try
        {
            byte[] bytes = new byte[RandomAccessReadBuffer.DEFAULT_CHUNK_SIZE_4KB];
            randomAccessReadWrite.Write(bytes);
            Assert.Throws<IOException>(() => randomAccessReadWrite.Seek(-1));
        }
        finally
        {
            randomAccessReadWrite.Close();
        }
    }

    [Fact]
    public void TestBufferEOF()
    {
        var randomAccessReadWrite = new RandomAccessReadWriteBuffer();
        try
        {
            byte[] bytes = new byte[RandomAccessReadBuffer.DEFAULT_CHUNK_SIZE_4KB];
            randomAccessReadWrite.Write(bytes);
            randomAccessReadWrite.Seek(0);
            Assert.False(randomAccessReadWrite.IsEOF());
            randomAccessReadWrite.Seek(RandomAccessReadBuffer.DEFAULT_CHUNK_SIZE_4KB);
            Assert.True(randomAccessReadWrite.IsEOF());
        }
        finally
        {
            randomAccessReadWrite.Close();
        }
    }

    [Fact]
    public void TestAlreadyClose()
    {
        var randomAccessReadWrite = new RandomAccessReadWriteBuffer();
        try
        {
            byte[] bytes = new byte[RandomAccessReadBuffer.DEFAULT_CHUNK_SIZE_4KB];
            randomAccessReadWrite.Write(bytes);
            randomAccessReadWrite.Close();
            Assert.Throws<IOException>(() => randomAccessReadWrite.Seek(0));
        }
        finally
        {
            if (!randomAccessReadWrite.IsClosed())
            {
                randomAccessReadWrite.Close();
            }
        }
    }
}
