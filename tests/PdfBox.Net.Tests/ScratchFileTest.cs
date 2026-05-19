/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Added focused xUnit coverage for the C# port of Apache PDFBox ScratchFile and
 * ScratchFileBuffer behavior. Upstream ScratchFileTest.java does not exist at the
 * reference commit; tests are authored to match the patterns and behaviors described
 * in the upstream Java source and applied consistently with the existing test suite.
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
using PdfBox.Net.IO;
using Xunit;
using RandomAccess = PdfBox.Net.IO.RandomAccess;

namespace PdfBox.Net.Tests;

/// <summary>
/// Unittest for <see cref="ScratchFile"/> and <see cref="ScratchFileBuffer"/>.
/// </summary>
public class ScratchFileTest
{
    // ---- main-memory-only mode ----

    [Fact]
    public void TestMainMemoryOnlyCreateBuffer()
    {
        using ScratchFile scratch = ScratchFile.GetMainMemoryOnlyInstance();
        using RandomAccess buffer = scratch.CreateBuffer();

        Assert.False(buffer.IsClosed());
        buffer.Write([1, 2, 3]);
        buffer.Seek(0);
        Assert.Equal(1, buffer.Read());
        Assert.Equal(2, buffer.Read());
        Assert.Equal(3, buffer.Read());
    }

    [Fact]
    public void TestMainMemoryOnlyLength()
    {
        using ScratchFile scratch = ScratchFile.GetMainMemoryOnlyInstance();
        using RandomAccess buffer = scratch.CreateBuffer();

        buffer.Write([10, 20, 30, 40, 50]);
        buffer.Seek(0);
        Assert.Equal(5, buffer.Length());
    }

    [Fact]
    public void TestMainMemoryOnlySeekAndRead()
    {
        byte[] data = new byte[200];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (byte)(i % 251);
        }

        using ScratchFile scratch = ScratchFile.GetMainMemoryOnlyInstance();
        using RandomAccess buffer = scratch.CreateBuffer();

        buffer.Write(data);

        for (int i = 0; i < data.Length; i++)
        {
            buffer.Seek(i);
            Assert.Equal(data[i], buffer.Read());
        }
    }

    [Fact]
    public void TestMainMemoryOnlyClear()
    {
        using ScratchFile scratch = ScratchFile.GetMainMemoryOnlyInstance();
        using RandomAccess buffer = scratch.CreateBuffer();

        buffer.Write([1, 2, 3, 4, 5]);
        Assert.Equal(5, buffer.Length());

        buffer.Clear();
        Assert.Equal(0, buffer.Length());
        Assert.True(buffer.IsEOF());
    }

    [Fact]
    public void TestMainMemoryOnlyIsEOF()
    {
        using ScratchFile scratch = ScratchFile.GetMainMemoryOnlyInstance();
        using RandomAccess buffer = scratch.CreateBuffer();

        Assert.True(buffer.IsEOF());
        buffer.Write([42]);
        buffer.Seek(0);
        Assert.False(buffer.IsEOF());
        buffer.Read();
        Assert.True(buffer.IsEOF());
    }

    [Fact]
    public void TestMainMemoryOnlyGetPosition()
    {
        using ScratchFile scratch = ScratchFile.GetMainMemoryOnlyInstance();
        using RandomAccess buffer = scratch.CreateBuffer();

        Assert.Equal(0, buffer.GetPosition());
        buffer.Write([1, 2, 3]);
        Assert.Equal(3, buffer.GetPosition());
        buffer.Seek(1);
        Assert.Equal(1, buffer.GetPosition());
    }

    [Fact]
    public void TestMainMemoryOnlyReadArray()
    {
        using ScratchFile scratch = ScratchFile.GetMainMemoryOnlyInstance();
        using RandomAccess buffer = scratch.CreateBuffer();

        buffer.Write([0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);
        buffer.Seek(2);

        byte[] result = new byte[4];
        int read = buffer.Read(result, 0, 4);
        Assert.Equal(4, read);
        Assert.Equal(2, result[0]);
        Assert.Equal(3, result[1]);
        Assert.Equal(4, result[2]);
        Assert.Equal(5, result[3]);
    }

    [Fact]
    public void TestMainMemoryOnlyReadAtEOF()
    {
        using ScratchFile scratch = ScratchFile.GetMainMemoryOnlyInstance();
        using RandomAccess buffer = scratch.CreateBuffer();

        buffer.Write([1]);
        buffer.Seek(1);

        Assert.Equal(-1, buffer.Read());
        Assert.Equal(-1, buffer.Read(new byte[4], 0, 4));
    }

    [Fact]
    public void TestMultipleBuffers()
    {
        using ScratchFile scratch = ScratchFile.GetMainMemoryOnlyInstance();
        using RandomAccess buf1 = scratch.CreateBuffer();
        using RandomAccess buf2 = scratch.CreateBuffer();

        buf1.Write([1, 2, 3]);
        buf2.Write([4, 5, 6]);

        buf1.Seek(0);
        buf2.Seek(0);

        Assert.Equal(1, buf1.Read());
        Assert.Equal(4, buf2.Read());
    }

    [Fact]
    public void TestBufferClosedAfterScratchFileClosed()
    {
        ScratchFile scratch = ScratchFile.GetMainMemoryOnlyInstance();
        RandomAccess buffer = scratch.CreateBuffer();

        Assert.False(buffer.IsClosed());
        scratch.Close();
        Assert.True(buffer.IsClosed());
    }

    [Fact]
    public void TestCreateViewThrows()
    {
        using ScratchFile scratch = ScratchFile.GetMainMemoryOnlyInstance();
        using RandomAccess buffer = scratch.CreateBuffer();
        buffer.Write([1, 2, 3]);

        Assert.Throws<IOException>(() => buffer.CreateView(0, 3));
    }

    // ---- multi-page writes (exercises page boundary transitions) ----

    [Fact]
    public void TestLargeWriteSpanningMultiplePages()
    {
        // Write more than one 4096-byte page worth of data
        int dataSize = 5000;
        byte[] data = new byte[dataSize];
        for (int i = 0; i < dataSize; i++)
        {
            data[i] = (byte)(i % 251);
        }

        using ScratchFile scratch = ScratchFile.GetMainMemoryOnlyInstance();
        using RandomAccess buffer = scratch.CreateBuffer();

        buffer.Write(data);
        Assert.Equal(dataSize, buffer.Length());

        buffer.Seek(0);
        byte[] readBack = new byte[dataSize];
        ((RandomAccess)buffer).ReadFully(readBack);

        Assert.Equal(data, readBack);
    }

    [Fact]
    public void TestSeekAcrossPageBoundary()
    {
        int dataSize = 8192; // exactly two pages
        byte[] data = new byte[dataSize];
        for (int i = 0; i < dataSize; i++)
        {
            data[i] = (byte)(i % 251);
        }

        using ScratchFile scratch = ScratchFile.GetMainMemoryOnlyInstance();
        using RandomAccess buffer = scratch.CreateBuffer();

        buffer.Write(data);

        // Seek to middle of second page and read
        buffer.Seek(5000);
        Assert.Equal(data[5000], buffer.Read());

        // Seek backwards to first page
        buffer.Seek(100);
        Assert.Equal(data[100], buffer.Read());
    }

    // ---- temp-file mode ----

    [Fact]
    public void TestTempFileModeCreateBuffer()
    {
        using ScratchFile scratch = new ScratchFile(MemoryUsageSetting.SetupTempFileOnly());
        using RandomAccess buffer = scratch.CreateBuffer();

        buffer.Write([10, 20, 30]);
        buffer.Seek(0);
        Assert.Equal(10, buffer.Read());
        Assert.Equal(20, buffer.Read());
        Assert.Equal(30, buffer.Read());
    }

    [Fact]
    public void TestTempFileModeWriteReadAcrossPages()
    {
        int dataSize = 5000;
        byte[] data = new byte[dataSize];
        for (int i = 0; i < dataSize; i++)
        {
            data[i] = (byte)(i % 251);
        }

        using ScratchFile scratch = new ScratchFile(MemoryUsageSetting.SetupTempFileOnly());
        using RandomAccess buffer = scratch.CreateBuffer();

        buffer.Write(data);
        Assert.Equal(dataSize, buffer.Length());

        buffer.Seek(0);
        byte[] readBack = new byte[dataSize];
        ((RandomAccess)buffer).ReadFully(readBack);

        Assert.Equal(data, readBack);
    }

    // ---- MemoryUsageSetting integration ----

    [Fact]
    public void TestMemoryUsageSettingStreamCacheMainMemory()
    {
        MemoryUsageSetting setting = MemoryUsageSetting.SetupMainMemoryOnly();
        using RandomAccessStreamCache cache = setting.StreamCache();
        using RandomAccess buffer = cache.CreateBuffer();

        buffer.Write([7, 8, 9]);
        buffer.Seek(0);
        Assert.Equal(7, buffer.Read());
    }

    [Fact]
    public void TestMemoryUsageSettingStreamCacheTempFile()
    {
        MemoryUsageSetting setting = MemoryUsageSetting.SetupTempFileOnly();
        using RandomAccessStreamCache cache = setting.StreamCache();
        Assert.IsType<ScratchFile>(cache);

        using RandomAccess buffer = cache.CreateBuffer();
        buffer.Write([1, 2, 3]);
        buffer.Seek(0);
        Assert.Equal(1, buffer.Read());
        Assert.Equal(2, buffer.Read());
        Assert.Equal(3, buffer.Read());
    }

    [Fact]
    public void TestMemoryUsageSettingStreamCacheMixed()
    {
        MemoryUsageSetting setting = MemoryUsageSetting.SetupMixed(4096);
        using RandomAccessStreamCache cache = setting.StreamCache();
        Assert.IsType<ScratchFile>(cache);

        using RandomAccess buffer = cache.CreateBuffer();
        buffer.Write([42]);
        buffer.Seek(0);
        Assert.Equal(42, buffer.Read());
    }
}
