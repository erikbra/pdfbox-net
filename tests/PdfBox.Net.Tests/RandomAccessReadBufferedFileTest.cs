/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: io/src/test/java/org/apache/pdfbox/io/RandomAccessReadBufferedFileTest.java
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
using System.Text;
using PdfBox.Net.IO;
using Xunit;

namespace PdfBox.Net.Tests;

/// <summary>
/// Unittest for <see cref="RandomAccessReadBufferedFile"/>.
/// </summary>
public class RandomAccessReadBufferedFileTest : IDisposable
{
    // "0123456789" repeated 13 times = 130 characters (matching upstream resource file)
    private static readonly byte[] FileContents =
        Encoding.ASCII.GetBytes(string.Concat(
            "0123456789012345678901234567890123456789012345678901234567890123456789",
            "01234567890123456789012345678901234567890123456789012345678901234567"));

    private readonly string _file1Path;
    private readonly string _emptyFilePath;

    public RandomAccessReadBufferedFileTest()
    {
        _file1Path = Path.GetTempFileName();
        File.WriteAllBytes(_file1Path, FileContents);

        _emptyFilePath = Path.GetTempFileName();
        File.WriteAllBytes(_emptyFilePath, Array.Empty<byte>());
    }

    public void Dispose()
    {
        TryDelete(_file1Path);
        TryDelete(_emptyFilePath);
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); } catch { /* ignore */ }
    }

    [Fact]
    public void TestPositionSkip()
    {
        using RandomAccessRead randomAccessSource = new RandomAccessReadBufferedFile(_file1Path);
        Assert.Equal(0, randomAccessSource.GetPosition());
        ((RandomAccessRead)randomAccessSource).Skip(5);
        Assert.Equal((int)'5', randomAccessSource.Read());
        Assert.Equal(6, randomAccessSource.GetPosition());
    }

    [Fact]
    public void TestPositionRead()
    {
        RandomAccessRead randomAccessSource = new RandomAccessReadBufferedFile(_file1Path);

        Assert.Equal(0, randomAccessSource.GetPosition());
        Assert.Equal((int)'0', randomAccessSource.Read());
        Assert.Equal((int)'1', randomAccessSource.Read());
        Assert.Equal((int)'2', randomAccessSource.Read());
        Assert.Equal(3, randomAccessSource.GetPosition());

        Assert.False(randomAccessSource.IsClosed());
        randomAccessSource.Close();
        Assert.True(randomAccessSource.IsClosed());
    }

    [Fact]
    public void TestSeekEOF()
    {
        RandomAccessRead randomAccessSource = new RandomAccessReadBufferedFile(_file1Path);

        randomAccessSource.Seek(3);
        Assert.Equal(3, randomAccessSource.GetPosition());

        Assert.Throws<IOException>(() => randomAccessSource.Seek(-1));

        Assert.False(randomAccessSource.IsEOF());
        randomAccessSource.Seek(randomAccessSource.Length());
        Assert.True(randomAccessSource.IsEOF());
        Assert.Equal(-1, randomAccessSource.Read());
        Assert.Equal(-1, randomAccessSource.Read(new byte[1], 0, 1));

        randomAccessSource.Close();
        Assert.Throws<IOException>(() => randomAccessSource.Read());
    }

    [Fact]
    public void TestPositionReadBytes()
    {
        using RandomAccessRead randomAccessSource = new RandomAccessReadBufferedFile(_file1Path);
        Assert.Equal(0, randomAccessSource.GetPosition());
        byte[] buffer = new byte[4];
        ((RandomAccessRead)randomAccessSource).Read(buffer);
        Assert.Equal((byte)'0', buffer[0]);
        Assert.Equal((byte)'3', buffer[3]);
        Assert.Equal(4, randomAccessSource.GetPosition());

        randomAccessSource.Read(buffer, 1, 2);
        Assert.Equal((byte)'0', buffer[0]);
        Assert.Equal((byte)'4', buffer[1]);
        Assert.Equal((byte)'5', buffer[2]);
        Assert.Equal((byte)'3', buffer[3]);
        Assert.Equal(6, randomAccessSource.GetPosition());
    }

    [Fact]
    public void TestPositionPeek()
    {
        using RandomAccessRead randomAccessSource = new RandomAccessReadBufferedFile(_file1Path);
        Assert.Equal(0, randomAccessSource.GetPosition());
        ((RandomAccessRead)randomAccessSource).Skip(6);
        Assert.Equal(6, randomAccessSource.GetPosition());

        Assert.Equal((int)'6', ((RandomAccessRead)randomAccessSource).Peek());
        Assert.Equal(6, randomAccessSource.GetPosition());
    }

    [Fact]
    public void TestFileInfoConstructor()
    {
        using RandomAccessRead randomAccessSource =
            new RandomAccessReadBufferedFile(new FileInfo(_file1Path));
        Assert.Equal(FileContents.Length, randomAccessSource.Length());
    }

    [Fact]
    public void TestPositionUnreadBytes()
    {
        using RandomAccessRead randomAccessSource = new RandomAccessReadBufferedFile(_file1Path);
        Assert.Equal(0, randomAccessSource.GetPosition());
        randomAccessSource.Read();
        randomAccessSource.Read();
        byte[] readBytes = new byte[6];
        Assert.Equal(readBytes.Length, ((RandomAccessRead)randomAccessSource).Read(readBytes));
        Assert.Equal(8, randomAccessSource.GetPosition());
        ((RandomAccessRead)randomAccessSource).Rewind(readBytes.Length);
        Assert.Equal(2, randomAccessSource.GetPosition());
        Assert.Equal((int)'2', randomAccessSource.Read());
        Assert.Equal(3, randomAccessSource.GetPosition());
        randomAccessSource.Read(readBytes, 2, 4);
        Assert.Equal(7, randomAccessSource.GetPosition());
        ((RandomAccessRead)randomAccessSource).Rewind(4);
        Assert.Equal(3, randomAccessSource.GetPosition());
    }

    [Fact]
    public void TestEmptyBuffer()
    {
        using RandomAccessRead randomAccessSource =
            new RandomAccessReadBufferedFile(_emptyFilePath);
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

    [Fact]
    public void TestView()
    {
        using RandomAccessRead randomAccessSource = new RandomAccessReadBufferedFile(_file1Path);
        using RandomAccessReadView view = randomAccessSource.CreateView(3, 10);
        Assert.Equal(0, view.GetPosition());
        Assert.Equal((int)'3', view.Read());
        Assert.Equal((int)'4', view.Read());
        Assert.Equal((int)'5', view.Read());
        Assert.Equal(3, view.GetPosition());
    }

    [Fact]
    public void TestLength()
    {
        using RandomAccessRead randomAccessSource = new RandomAccessReadBufferedFile(_file1Path);
        Assert.Equal(FileContents.Length, randomAccessSource.Length());
    }

    [Fact]
    public void TestReadFullyAcrossPageBoundary()
    {
        // Write data larger than one page (4096 bytes) to exercise page boundary reading.
        string bigFilePath = Path.GetTempFileName();
        try
        {
            byte[] data = new byte[8000];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(i % 251);
            }

            File.WriteAllBytes(bigFilePath, data);

            using RandomAccessRead reader = new RandomAccessReadBufferedFile(bigFilePath);
            Assert.Equal(8000, reader.Length());

            byte[] buf = new byte[8000];
            ((RandomAccessRead)reader).ReadFully(buf);

            for (int i = 0; i < data.Length; i++)
            {
                Assert.Equal(data[i], buf[i]);
            }
        }
        finally
        {
            TryDelete(bigFilePath);
        }
    }
}
