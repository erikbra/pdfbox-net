/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Added focused xUnit coverage for the C# port of Apache PDFBox RandomAccessOutputStream behavior.
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

using System;
using System.IO;
using PdfBox.Net.IO;
using Xunit;

namespace PdfBox.Net.Tests;

/// <summary>
/// Unittest for <see cref="RandomAccessOutputStream"/>.
/// </summary>
public class RandomAccessOutputStreamTest
{
    [Fact]
    public void TestWriteMethods()
    {
        var randomAccessWrite = new RandomAccessReadWriteBuffer();
        try
        {
            var randomAccessOutputStream = new RandomAccessOutputStream(randomAccessWrite);
            randomAccessOutputStream.WriteByte(1);
            randomAccessOutputStream.Write([2, 3, 4], 0, 3);
            randomAccessOutputStream.Write([5, 6, 7, 8], 1, 2);

            randomAccessWrite.Seek(0);
            byte[] bytes = new byte[6];
            Assert.Equal(6, randomAccessWrite.Read(bytes, 0, bytes.Length));
            Assert.Equal([1, 2, 3, 4, 6, 7], bytes);
        }
        finally
        {
            randomAccessWrite.Close();
        }
    }

    [Fact]
    public void TestUnsupportedOperations()
    {
        var randomAccessWrite = new RandomAccessReadWriteBuffer();
        try
        {
            var randomAccessOutputStream = new RandomAccessOutputStream(randomAccessWrite);
            Assert.False(randomAccessOutputStream.CanRead);
            Assert.False(randomAccessOutputStream.CanSeek);
            Assert.True(randomAccessOutputStream.CanWrite);
            Assert.Throws<NotSupportedException>(() => randomAccessOutputStream.Read([], 0, 0));
            Assert.Throws<NotSupportedException>(() => randomAccessOutputStream.Seek(0, SeekOrigin.Begin));
            Assert.Throws<NotSupportedException>(() => randomAccessOutputStream.SetLength(0));
        }
        finally
        {
            randomAccessWrite.Close();
        }
    }
}
