/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: io/src/test/java/org/apache/pdfbox/io/RandomAccessInputStreamTest.java
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
/// Unittest for <see cref="RandomAccessInputStream"/>.
/// </summary>
public class RandomAccessInputStreamTest
{
    [Fact]
    public void TestPositionSkip()
    {
        byte[] inputValues = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var randomAccessRead = new RandomAccessReadBuffer(new MemoryStream(inputValues));
        try
        {
            var randomAccessInputStream = new RandomAccessInputStream(randomAccessRead);
            Assert.Equal(11, randomAccessInputStream.Available());
            randomAccessInputStream.Skip(5);
            Assert.Equal(5, randomAccessInputStream.ReadByte());
            Assert.Equal(5, randomAccessInputStream.Available());
            Assert.Equal(0, randomAccessInputStream.Skip(-10));
        }
        finally
        {
            randomAccessRead.Close();
        }
    }

    [Fact]
    public void TestPositionRead()
    {
        byte[] inputValues = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var randomAccessRead = new RandomAccessReadBuffer(new MemoryStream(inputValues));
        try
        {
            var randomAccessInputStream = new RandomAccessInputStream(randomAccessRead);
            Assert.Equal(11, randomAccessInputStream.Available());
            Assert.Equal(0, randomAccessInputStream.ReadByte());
            Assert.Equal(1, randomAccessInputStream.ReadByte());
            Assert.Equal(2, randomAccessInputStream.ReadByte());
            Assert.Equal(8, randomAccessInputStream.Available());
        }
        finally
        {
            randomAccessRead.Close();
        }
    }

    [Fact]
    public void TestSeekEOF()
    {
        byte[] inputValues = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var randomAccessRead = new RandomAccessReadBuffer(new MemoryStream(inputValues));
        try
        {
            var randomAccessInputStream = new RandomAccessInputStream(randomAccessRead);
            Assert.Equal(12, randomAccessInputStream.Skip(inputValues.Length + 1));

            Assert.Equal(0, randomAccessInputStream.Available());
            Assert.Equal(-1, randomAccessInputStream.ReadByte());
            Assert.Equal(0, randomAccessInputStream.Read(new byte[1], 0, 1));
        }
        finally
        {
            randomAccessRead.Close();
        }
    }

    [Fact]
    public void TestPositionReadBytes()
    {
        byte[] inputValues = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var randomAccessRead = new RandomAccessReadBuffer(new MemoryStream(inputValues));
        try
        {
            var randomAccessInputStream = new RandomAccessInputStream(randomAccessRead);
            Assert.Equal(11, randomAccessInputStream.Available());
            byte[] buffer = new byte[4];
            Assert.Equal(4, randomAccessInputStream.Read(buffer, 0, buffer.Length));
            Assert.Equal(0, buffer[0]);
            Assert.Equal(3, buffer[3]);
            Assert.Equal(7, randomAccessInputStream.Available());

            Assert.Equal(2, randomAccessInputStream.Read(buffer, 1, 2));
            Assert.Equal(0, buffer[0]);
            Assert.Equal(4, buffer[1]);
            Assert.Equal(5, buffer[2]);
            Assert.Equal(3, buffer[3]);
            Assert.Equal(5, randomAccessInputStream.Available());
        }
        finally
        {
            randomAccessRead.Close();
        }
    }

    [Fact]
    public void TestIndependentPosition()
    {
        byte[] inputValues = { 10, 20, 30, 40, 50 };
        var randomAccessRead = new RandomAccessReadBuffer(new MemoryStream(inputValues));
        try
        {
            var randomAccessInputStream = new RandomAccessInputStream(randomAccessRead);
            Assert.Equal(10, randomAccessInputStream.ReadByte());
            randomAccessRead.Seek(4);
            Assert.Equal(20, randomAccessInputStream.ReadByte());
        }
        finally
        {
            randomAccessRead.Close();
        }
    }

    [Fact]
    public void TestEmptyBuffer()
    {
        var randomAccessRead = new RandomAccessReadBuffer([]);
        try
        {
            var randomAccessInputStream = new RandomAccessInputStream(randomAccessRead);
            Assert.Equal(-1, randomAccessInputStream.ReadByte());
            Assert.Equal(0, randomAccessInputStream.Read(new byte[6], 0, 6));
            Assert.Equal(0, randomAccessInputStream.Available());
        }
        finally
        {
            randomAccessRead.Close();
        }
    }
}
