/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/test/java/org/apache/fontbox/cff/DataInputRandomAccessTest.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 */

/*
 * Copyright 2017 The Apache Software Foundation.
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

using PdfBox.Net.FontBox.CFF;
using PdfBox.Net.IO;

namespace PdfBox.Net.Tests;

public class CFFDataInputRandomAccessTest
{
    [Fact]
    public void TestReadBytes()
    {
        byte[] data = [0, 0xFF, 2, 0xFD, 4, 0xFB, 6, 0xF9, 8, 0xF7];
        DataInput dataInput = new DataInputRandomAccessRead(new RandomAccessReadBuffer(data));
        Assert.ThrowsAny<IOException>(() => dataInput.ReadBytes(20));
        Assert.Equal([0], dataInput.ReadBytes(1));
        Assert.Equal([0xFF, 2, 0xFD], dataInput.ReadBytes(3));
        dataInput.SetPosition(6);
        Assert.Equal([6, 0xF9, 8], dataInput.ReadBytes(3));
        Assert.Throws<IOException>(() => dataInput.ReadBytes(-1));
        Assert.ThrowsAny<IOException>(() => dataInput.ReadBytes(5));
    }

    [Fact]
    public void TestReadByte()
    {
        byte[] data = [0, 0xFF, 2, 0xFD, 4, 0xFB, 6, 0xF9, 8, 0xF7];
        DataInput dataInput = new DataInputRandomAccessRead(new RandomAccessReadBuffer(data));
        Assert.Equal(0, dataInput.ReadByte());
        Assert.Equal(unchecked((byte)-1), dataInput.ReadByte());
        dataInput.SetPosition(6);
        Assert.Equal(6, dataInput.ReadByte());
        Assert.Equal(unchecked((byte)-7), dataInput.ReadByte());
        dataInput.SetPosition(dataInput.Length() - 1);
        Assert.Equal(unchecked((byte)-9), dataInput.ReadByte());
        Assert.Throws<IOException>(() => dataInput.ReadByte());
    }

    [Fact]
    public void TestReadUnsignedByte()
    {
        byte[] data = [0, 0xFF, 2, 0xFD, 4, 0xFB, 6, 0xF9, 8, 0xF7];
        DataInput dataInput = new DataInputRandomAccessRead(new RandomAccessReadBuffer(data));
        Assert.Equal(0, dataInput.ReadUnsignedByte());
        Assert.Equal(255, dataInput.ReadUnsignedByte());
        dataInput.SetPosition(6);
        Assert.Equal(6, dataInput.ReadUnsignedByte());
        Assert.Equal(249, dataInput.ReadUnsignedByte());
        dataInput.SetPosition(dataInput.Length() - 1);
        Assert.Equal(247, dataInput.ReadUnsignedByte());
        Assert.Throws<IOException>(() => dataInput.ReadUnsignedByte());
    }

    [Fact]
    public void TestBasics()
    {
        byte[] data = [0, 0xFF, 2, 0xFD, 4, 0xFB, 6, 0xF9, 8, 0xF7];
        DataInput dataInput = new DataInputRandomAccessRead(new RandomAccessReadBuffer(data));
        Assert.Equal(10, dataInput.Length());
        Assert.True(dataInput.HasRemaining());
        Assert.Throws<IOException>(() => dataInput.SetPosition(-1));
        int length = dataInput.Length();
        Assert.Throws<IOException>(() => dataInput.SetPosition(length));
    }

    [Fact]
    public void TestPeek()
    {
        byte[] data = [0, 0xFF, 2, 0xFD, 4, 0xFB, 6, 0xF9, 8, 0xF7];
        DataInput dataInput = new DataInputRandomAccessRead(new RandomAccessReadBuffer(data));
        Assert.Equal(0, dataInput.PeekUnsignedByte(0));
        Assert.Equal(251, dataInput.PeekUnsignedByte(5));
        Assert.Throws<IOException>(() => dataInput.PeekUnsignedByte(-1));
        int length = dataInput.Length();
        Assert.Throws<IOException>(() => dataInput.PeekUnsignedByte(length));
    }

    [Fact]
    public void TestReadShort()
    {
        byte[] data = [0x00, 0x0F, 0xAA, 0, 0xFE, 0xFF];
        DataInput dataInput = new DataInputRandomAccessRead(new RandomAccessReadBuffer(data));
        Assert.Equal(unchecked((short)0x000F), dataInput.ReadShort());
        Assert.Equal(unchecked((short)0xAA00), dataInput.ReadShort());
        Assert.Equal(unchecked((short)0xFEFF), dataInput.ReadShort());
        Assert.Throws<IOException>(() => dataInput.ReadShort());
    }

    [Fact]
    public void TestReadUnsignedShort()
    {
        byte[] data = [0x00, 0x0F, 0xAA, 0, 0xFE, 0xFF];
        DataInput dataInput = new DataInputRandomAccessRead(new RandomAccessReadBuffer(data));
        Assert.Equal(0x000F, dataInput.ReadUnsignedShort());
        Assert.Equal(0xAA00, dataInput.ReadUnsignedShort());
        Assert.Equal(0xFEFF, dataInput.ReadUnsignedShort());
        Assert.Throws<IOException>(() => dataInput.ReadUnsignedShort());

        byte[] data2 = [0x00];
        DataInput dataInput2 = new DataInputRandomAccessRead(new RandomAccessReadBuffer(data2));
        Assert.Throws<IOException>(() => dataInput2.ReadUnsignedShort());
    }

    [Fact]
    public void TestReadInt()
    {
        byte[] data = [0x00, 0x0F, 0xAA, 0, 0xFE, 0xFF, 0x30, 0x50];
        DataInput dataInput = new DataInputRandomAccessRead(new RandomAccessReadBuffer(data));
        Assert.Equal(0x000FAA00, dataInput.ReadInt());
        Assert.Equal(unchecked((int)0xFEFF3050), dataInput.ReadInt());
        Assert.Throws<IOException>(() => dataInput.ReadInt());

        byte[] data2 = [0x00, 0x0F, 0xAA];
        DataInput dataInput2 = new DataInputRandomAccessRead(new RandomAccessReadBuffer(data2));
        Assert.Throws<IOException>(() => dataInput2.ReadInt());
    }
}
