/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: io/src/test/java/org/apache/pdfbox/io/RandomAccessReadBufferTest.java
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
/// Unittest for <see cref="RandomAccessReadBuffer"/>.
/// </summary>
public class RandomAccessReadBufferTest
{
    [Fact]
    public void TestPositionSkip()
    {
        byte[] inputValues = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var randomAccessSource = new RandomAccessReadBuffer(new MemoryStream(inputValues));
        try
        {
            Assert.Equal(0, randomAccessSource.GetPosition());
            ((RandomAccessRead)randomAccessSource).Skip(5);
            Assert.Equal(5, randomAccessSource.Read());
            Assert.Equal(6, randomAccessSource.GetPosition());
        }
        finally
        {
            randomAccessSource.Close();
        }
    }

    [Fact]
    public void TestPositionRead()
    {
        byte[] inputValues = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var randomAccessSource = new RandomAccessReadBuffer(new MemoryStream(inputValues));

        Assert.Equal(0, randomAccessSource.GetPosition());
        Assert.Equal(0, randomAccessSource.Read());
        Assert.Equal(1, randomAccessSource.Read());
        Assert.Equal(2, randomAccessSource.Read());
        Assert.Equal(3, randomAccessSource.GetPosition());

        Assert.False(randomAccessSource.IsClosed());
        randomAccessSource.Close();
        Assert.True(randomAccessSource.IsClosed());
    }

    [Fact]
    public void TestSeekEOF()
    {
        byte[] inputValues = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var randomAccessSource = new RandomAccessReadBuffer(new MemoryStream(inputValues));

        randomAccessSource.Seek(3);
        Assert.Equal(3, randomAccessSource.GetPosition());

        Assert.Throws<IOException>(() => randomAccessSource.Seek(-1));

        Assert.False(randomAccessSource.IsEOF());
        randomAccessSource.Seek(20);
        Assert.True(randomAccessSource.IsEOF());
        Assert.Equal(-1, randomAccessSource.Read());
        Assert.Equal(-1, randomAccessSource.Read(new byte[1], 0, 1));

        randomAccessSource.Close();
        Assert.Throws<IOException>(() => randomAccessSource.Read());
    }

    [Fact]
    public void TestPositionReadBytes()
    {
        byte[] inputValues = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var randomAccessSource = new RandomAccessReadBuffer(new MemoryStream(inputValues));
        try
        {
            Assert.Equal(0, randomAccessSource.GetPosition());
            byte[] buffer = new byte[4];
            ((RandomAccessRead)randomAccessSource).Read(buffer);
            Assert.Equal(0, buffer[0]);
            Assert.Equal(3, buffer[3]);
            Assert.Equal(4, randomAccessSource.GetPosition());

            randomAccessSource.Read(buffer, 1, 2);
            Assert.Equal(0, buffer[0]);
            Assert.Equal(4, buffer[1]);
            Assert.Equal(5, buffer[2]);
            Assert.Equal(3, buffer[3]);
            Assert.Equal(6, randomAccessSource.GetPosition());
        }
        finally
        {
            randomAccessSource.Close();
        }
    }

    [Fact]
    public void TestPositionPeek()
    {
        byte[] inputValues = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var randomAccessSource = new RandomAccessReadBuffer(new MemoryStream(inputValues));
        try
        {
            Assert.Equal(0, randomAccessSource.GetPosition());
            ((RandomAccessRead)randomAccessSource).Skip(6);
            Assert.Equal(6, randomAccessSource.GetPosition());

            Assert.Equal(6, ((RandomAccessRead)randomAccessSource).Peek());
            Assert.Equal(6, randomAccessSource.GetPosition());
        }
        finally
        {
            randomAccessSource.Close();
        }
    }

    [Fact]
    public void TestPositionUnreadBytes()
    {
        byte[] inputValues = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var randomAccessSource = new RandomAccessReadBuffer(new MemoryStream(inputValues));
        try
        {
            Assert.Equal(0, randomAccessSource.GetPosition());
            randomAccessSource.Read();
            randomAccessSource.Read();
            byte[] readBytes = new byte[6];
            Assert.Equal(readBytes.Length, ((RandomAccessRead)randomAccessSource).Read(readBytes));
            Assert.Equal(8, randomAccessSource.GetPosition());
            ((RandomAccessRead)randomAccessSource).Rewind(readBytes.Length);
            Assert.Equal(2, randomAccessSource.GetPosition());
            Assert.Equal(2, randomAccessSource.Read());
            Assert.Equal(3, randomAccessSource.GetPosition());
            randomAccessSource.Read(readBytes, 2, 4);
            Assert.Equal(7, randomAccessSource.GetPosition());
            ((RandomAccessRead)randomAccessSource).Rewind(4);
            Assert.Equal(3, randomAccessSource.GetPosition());
        }
        finally
        {
            randomAccessSource.Close();
        }
    }

    [Fact]
    public void TestEmptyBuffer()
    {
        var randomAccessSource = new RandomAccessReadBuffer(Array.Empty<byte>());
        try
        {
            Assert.Equal(-1, randomAccessSource.Read());
            Assert.Equal(-1, ((RandomAccessRead)randomAccessSource).Peek());
            byte[] readBytes = new byte[6];
            Assert.Equal(-1, ((RandomAccessRead)randomAccessSource).Read(readBytes));
            randomAccessSource.Seek(0);
            Assert.Equal(0, randomAccessSource.GetPosition());
            randomAccessSource.Seek(6);
            Assert.Equal(0, randomAccessSource.GetPosition());
            Assert.True(randomAccessSource.IsEOF());
            Assert.Throws<IOException>(() => ((RandomAccessRead)randomAccessSource).Rewind(3));
        }
        finally
        {
            randomAccessSource.Close();
        }
    }

    [Fact]
    public void TestView()
    {
        byte[] inputValues = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var randomAccessSource = new RandomAccessReadBuffer(new MemoryStream(inputValues));
        RandomAccessReadView? view = null;
        try
        {
            view = randomAccessSource.CreateView(3, 5);
            Assert.Equal(0, view.GetPosition());
            Assert.Equal(3, view.Read());
            Assert.Equal(4, view.Read());
            Assert.Equal(5, view.Read());
            Assert.Equal(3, view.GetPosition());
        }
        finally
        {
            view?.Close();
            randomAccessSource.Close();
        }
    }

    [Fact]
    public void TestPDFBOX5158()
    {
        string path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, new byte[4096]);
            Assert.Equal(4096, new FileInfo(path).Length);

            using FileStream isStream = File.OpenRead(path);
            var randomAccessRead = new RandomAccessReadBuffer(isStream);
            try
            {
                Assert.Equal(0, randomAccessRead.Read());
            }
            finally
            {
                randomAccessRead.Close();
            }
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void TestPDFBOX5161()
    {
        var randomAccessRead = new RandomAccessReadBuffer(new MemoryStream(new byte[4099]));
        try
        {
            byte[] buf = new byte[4096];
            int bytesRead = ((RandomAccessRead)randomAccessRead).Read(buf);
            Assert.Equal(4096, bytesRead);
            bytesRead = randomAccessRead.Read(buf, 0, 3);
            Assert.Equal(3, bytesRead);
        }
        finally
        {
            randomAccessRead.Close();
        }
    }

    [Fact]
    public void TestPDFBOX5111()
    {
        byte[] fixtureBytes = new byte[34060];
        for (int i = 0; i < fixtureBytes.Length; i++)
        {
            fixtureBytes[i] = (byte)(i % 251);
        }

        var randomAccessRead = new RandomAccessReadBuffer(new MemoryStream(fixtureBytes));
        try
        {
            Assert.Equal(34060, randomAccessRead.Length());
            randomAccessRead.Seek(34059);
            Assert.Equal(fixtureBytes[34059], randomAccessRead.Read());
        }
        finally
        {
            randomAccessRead.Close();
        }
    }

    [Fact]
    public void TestPDFBOX5764()
    {
        int bufferSize = 4096;
        int limit = 2048;
        byte[] buffer = new byte[bufferSize];
        var randomAccessRead = new RandomAccessReadBuffer(new ArraySegment<byte>(buffer, 0, limit));
        try
        {
            byte[] buf = new byte[bufferSize];
            int bytesRead = ((RandomAccessRead)randomAccessRead).Read(buf);
            Assert.Equal(limit, bytesRead);
        }
        finally
        {
            randomAccessRead.Close();
        }
    }
}
