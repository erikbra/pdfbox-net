/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Added focused xUnit coverage for the C# port of Apache PDFBox RandomAccessStreamCache behavior.
 * No direct equivalent test file exists in the upstream Apache PDFBox source.
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
using Xunit;
using RandomAccess = PdfBox.Net.IO.RandomAccess;

namespace PdfBox.Net.Tests;

/// <summary>
/// Unittest for <see cref="RandomAccessStreamCache"/>.
/// </summary>
public class RandomAccessStreamCacheTest
{
    /// <summary>
    /// An in-memory implementation of <see cref="RandomAccessStreamCache"/> backed by
    /// <see cref="RandomAccessReadWriteBuffer"/> for use in tests.
    /// </summary>
    private sealed class InMemoryStreamCache : RandomAccessStreamCache
    {
        public RandomAccess CreateBuffer()
        {
            return new RandomAccessReadWriteBuffer();
        }

        public void Close()
        {
        }
    }

    [Fact]
    public void TestCreateBuffer()
    {
        var cache = new InMemoryStreamCache();
        var buffer = cache.CreateBuffer();
        try
        {
            Assert.NotNull(buffer);
            Assert.False(buffer.IsClosed());
        }
        finally
        {
            ((RandomAccessRead)buffer).Close();
        }
    }

    [Fact]
    public void TestCreateBufferWriteAndRead()
    {
        var cache = new InMemoryStreamCache();
        var buffer = cache.CreateBuffer();
        try
        {
            buffer.Write([1, 2, 3, 4, 5]);
            Assert.Equal(5, buffer.Length());
            buffer.Seek(0);
            Assert.Equal(1, buffer.Read());
            Assert.Equal(2, buffer.Read());
            Assert.Equal(3, buffer.Read());
        }
        finally
        {
            ((RandomAccessRead)buffer).Close();
        }
    }

    [Fact]
    public void TestStreamCacheCreateFunction()
    {
        RandomAccessStreamCache.StreamCacheCreateFunction createFunction =
            () => new InMemoryStreamCache();

        var cache = createFunction();
        Assert.NotNull(cache);

        var buffer = cache.CreateBuffer();
        try
        {
            Assert.False(buffer.IsClosed());
            buffer.Write([10, 20, 30]);
            Assert.Equal(3, buffer.Length());
        }
        finally
        {
            ((RandomAccessRead)buffer).Close();
            cache.Close();
        }
    }

    [Fact]
    public void TestMultipleBuffers()
    {
        var cache = new InMemoryStreamCache();
        var buffer1 = cache.CreateBuffer();
        var buffer2 = cache.CreateBuffer();
        try
        {
            buffer1.Write([1, 2, 3]);
            buffer2.Write([4, 5, 6, 7]);
            Assert.Equal(3, buffer1.Length());
            Assert.Equal(4, buffer2.Length());
        }
        finally
        {
            ((RandomAccessRead)buffer1).Close();
            ((RandomAccessRead)buffer2).Close();
            cache.Close();
        }
    }
}
