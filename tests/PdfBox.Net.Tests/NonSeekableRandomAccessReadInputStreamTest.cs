/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: io/src/test/java/org/apache/pdfbox/io/NonSeekableRandomAccessReadInputStreamTest.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 */

/*
 * Copyright 2020 The Apache Software Foundation.
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

using System;
using System.IO;
using PdfBox.Net.IO;
using Xunit;

namespace PdfBox.Net.Tests;

/// <summary>
/// Unittest for <see cref="NonSeekableRandomAccessReadInputStream"/>.
/// </summary>
public class NonSeekableRandomAccessReadInputStreamTest
{
    [Fact]
    public void TestPositionSkip()
    {
        byte[] inputValues = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        using var bais = new MemoryStream(inputValues);
        using var randomAccessSource = new NonSeekableRandomAccessReadInputStream(bais);

        Assert.Equal(0, randomAccessSource.GetPosition());
        randomAccessSource.Skip(5);
        Assert.Equal(5, randomAccessSource.Read());
        Assert.Equal(6, randomAccessSource.GetPosition());
    }

    [Fact]
    public void TestSeekEOF()
    {
        byte[] inputValues = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        using var bais = new MemoryStream(inputValues);
        using var randomAccessSource = new NonSeekableRandomAccessReadInputStream(bais);

        Assert.Throws<IOException>(() => randomAccessSource.Seek(3));
    }

    [Fact]
    public void TestPositionUnreadBytes()
    {
        byte[] inputValues = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        using var bais = new MemoryStream(inputValues);
        using var randomAccessSource = new NonSeekableRandomAccessReadInputStream(bais);

        Assert.Equal(0, randomAccessSource.GetPosition());
        randomAccessSource.Read();
        randomAccessSource.Read();
        byte[] readBytes = new byte[6];
        Assert.Equal(readBytes.Length, randomAccessSource.Read(readBytes, 0, readBytes.Length));
        Assert.Equal(8, randomAccessSource.GetPosition());
        randomAccessSource.Rewind(readBytes.Length);
        Assert.Equal(2, randomAccessSource.GetPosition());
        Assert.Equal(2, randomAccessSource.Read());

        Assert.Equal(3, randomAccessSource.GetPosition());
        randomAccessSource.Read(readBytes, 2, 4);
        Assert.Equal(7, randomAccessSource.GetPosition());
        randomAccessSource.Rewind(4);
        Assert.Equal(3, randomAccessSource.GetPosition());
    }

    [Fact]
    public void TestRewindAcrossBuffers()
    {
        byte[] ba = new byte[4096 + 5];
        int rewSize = 7;
        byte testVal = 123;
        ba[ba.Length - rewSize] = testVal;
        using var bais = new MemoryStream(ba);
        using RandomAccessRead rar = new NonSeekableRandomAccessReadInputStream(bais);

        int len = rar.Read(new byte[ba.Length - rewSize]);
        Assert.Equal(ba.Length - rewSize, len);
        len = rar.Read(new byte[rewSize]);
        Assert.Equal(rewSize, len);
        int by = rar.Read();
        Assert.Equal(-1, by);
        Assert.True(rar.IsEOF());
        rar.Rewind(len);
        by = rar.Read();
        Assert.Equal(testVal, by);
    }

    [Fact]
    public void TestReadBytesParameterValidation()
    {
        byte[] inputValues = [0, 1, 2, 3, 4];
        using var bais = new MemoryStream(inputValues);
        using var rar = new NonSeekableRandomAccessReadInputStream(bais);

        Assert.Throws<ArgumentNullException>(() => rar.Read(null!, 0, 1));

        byte[] buf = new byte[4];
        Assert.Throws<IndexOutOfRangeException>(() => rar.Read(buf, -1, 2));
        Assert.Throws<IndexOutOfRangeException>(() => rar.Read(buf, 0, -1));
        Assert.Throws<IndexOutOfRangeException>(() => rar.Read(buf, 2, 4));

        Assert.Equal(0, rar.Read(buf, 0, 0));
        Assert.Equal(0, rar.GetPosition());
    }

    [Fact]
    public void TestReadFullyEOF()
    {
        byte[] inputValues = [0, 1, 2];
        using var bais = new MemoryStream(inputValues);
        using var rar = new NonSeekableRandomAccessReadInputStream(bais);

        Assert.Throws<EndOfStreamException>(() => rar.ReadFully(new byte[10], 0, 10));
    }

    [Fact]
    public void TestSkipPastEOF()
    {
        byte[] inputValues = [0, 1, 2, 3, 4];
        using var bais = new MemoryStream(inputValues);
        using var rar = new NonSeekableRandomAccessReadInputStream(bais);

        rar.Skip(100);
        Assert.Equal(5, rar.GetPosition());
        Assert.True(rar.IsEOF());
    }

    [Fact]
    public void TestPDFBOX5161()
    {
        using RandomAccessRead rar = new NonSeekableRandomAccessReadInputStream(new MemoryStream(new byte[4099]));
        byte[] buf = new byte[4096];
        int bytesRead = rar.Read(buf);
        Assert.Equal(4096, bytesRead);
        bytesRead = rar.Read(buf, 0, 3);
        Assert.Equal(3, bytesRead);
    }
}
