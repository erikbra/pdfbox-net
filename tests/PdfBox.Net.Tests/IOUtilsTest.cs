/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: io/src/test/java/org/apache/pdfbox/io/TestIOUtils.java
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
using System.IO;
using PdfBox.Net.IO;
using Xunit;

namespace PdfBox.Net.Tests;

/// <summary>
/// Unit tests for <see cref="IOUtils"/>.
/// </summary>
public class IOUtilsTest
{
    [Fact]
    public void TestPopulateBuffer()
    {
        byte[] data = "Hello World!"u8.ToArray();
        byte[] buffer = new byte[data.Length];
        long count = IOUtils.PopulateBuffer(new MemoryStream(data), buffer);
        Assert.Equal(12, count);

        buffer = new byte[data.Length - 2];
        using var smallInput = new MemoryStream(data);
        count = IOUtils.PopulateBuffer(smallInput, buffer);
        Assert.Equal(10, count);
        byte[] leftOver = new byte[smallInput.Length - smallInput.Position];
        _ = smallInput.Read(leftOver, 0, leftOver.Length);
        Assert.Equal(2, leftOver.Length);
    }

    [Fact]
    public void TestToByteArray()
    {
        byte[] data = "Test Data"u8.ToArray();
        byte[] result = IOUtils.ToByteArray(new MemoryStream(data));
        Assert.Equal(data, result);
    }

    [Fact]
    public void TestCopy()
    {
        byte[] data = "Copy Test Content"u8.ToArray();
        using Stream input = new MemoryStream(data);
        using var output = new MemoryStream();

        long copied = IOUtils.Copy(input, output);

        Assert.Equal(data.Length, copied);
        Assert.Equal(data, output.ToArray());
    }

    [Fact]
    public void TestCloseQuietlySuppressesException()
    {
        var failingCloseable = new FailingDisposable();
        IOUtils.CloseQuietly(failingCloseable);
    }

    [Fact]
    public void TestCloseAndLogExceptionCloseThrows()
    {
        IOException closeException = new("Close error");
        var failingCloseable = new ThrowingIoDisposable(closeException);

        IOException? result = IOUtils.CloseAndLogException(failingCloseable, null, "testResource", null);

        Assert.Same(closeException, result);
    }

    [Fact]
    public void TestCreateMemoryOnlyStreamCache()
    {
        RandomAccessStreamCache.StreamCacheCreateFunction function = IOUtils.CreateMemoryOnlyStreamCache();
        Assert.NotNull(function);
        using RandomAccessStreamCache cache = function();
        Assert.IsType<RandomAccessStreamCacheImpl>(cache);
    }

    [Fact]
    public void TestCreateTempFileOnlyStreamCache()
    {
        RandomAccessStreamCache.StreamCacheCreateFunction function = IOUtils.CreateTempFileOnlyStreamCache();
        Assert.NotNull(function);
        using RandomAccessStreamCache cache = function();
        Assert.IsType<ScratchFile>(cache);
    }

    [Fact]
    public void TestCreateProtectedTempDirAndFile()
    {
        DirectoryInfo tempDir = IOUtils.CreateProtectedTempDir();
        try
        {
            Assert.True(tempDir.Exists);
            Assert.StartsWith("pdfbox-", tempDir.Name, StringComparison.Ordinal);

            FileInfo tempFile = IOUtils.CreateProtectedTempFile(tempDir, "test", ".tmp");
            try
            {
                Assert.True(tempFile.Exists);
                Assert.Equal(tempDir.FullName, tempFile.DirectoryName);
                Assert.StartsWith("test", tempFile.Name, StringComparison.Ordinal);
                Assert.EndsWith(".tmp", tempFile.Name, StringComparison.Ordinal);
            }
            finally
            {
                if (tempFile.Exists)
                {
                    tempFile.Delete();
                }
            }
        }
        finally
        {
            if (tempDir.Exists)
            {
                tempDir.Delete();
            }
        }
    }

    private sealed class FailingDisposable : IDisposable
    {
        public void Dispose() => throw new InvalidOperationException("boom");
    }

    private sealed class ThrowingIoDisposable(IOException exception) : IDisposable
    {
        public void Dispose() => throw exception;
    }
}
