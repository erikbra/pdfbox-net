/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache FontBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/TTFDataStream.java
 * PDFBOX_SOURCE_COMMIT: trunk
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: trunk
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

using System.IO;
using System.Text;

namespace PdfBox.Net.FontBox.TTF;

public abstract class TTFDataStream
{
    public abstract long Position { get; }

    public abstract long Length { get; }

    public abstract void Seek(long position);

    public abstract int Read();

    public abstract int Read(byte[] buffer, int offset, int count);

    public long GetCurrentPosition() => Position;

    public long GetOriginalDataSize() => Length;

    public byte[] ReadBytes(int count)
    {
        byte[] buffer = new byte[count];
        int totalRead = 0;
        while (totalRead < count)
        {
            int bytesRead = Read(buffer, totalRead, count - totalRead);
            if (bytesRead <= 0)
            {
                throw new EndOfStreamException("Unexpected end of TTF data stream");
            }

            totalRead += bytesRead;
        }

        return buffer;
    }

    public int ReadUnsignedByte()
    {
        int value = Read();
        if (value < 0)
        {
            throw new EndOfStreamException("Unexpected end of TTF data stream");
        }

        return value;
    }

    public int ReadSignedByte()
    {
        int value = ReadUnsignedByte();
        return value > 127 ? value - 256 : value;
    }

    public ushort ReadUnsignedShort()
    {
        int high = ReadUnsignedByte();
        int low = ReadUnsignedByte();
        return (ushort)((high << 8) | low);
    }

    public short ReadSignedShort() => unchecked((short)ReadUnsignedShort());

    public uint ReadUnsignedInt()
    {
        int b1 = ReadUnsignedByte();
        int b2 = ReadUnsignedByte();
        int b3 = ReadUnsignedByte();
        int b4 = ReadUnsignedByte();
        return (uint)((b1 << 24) | (b2 << 16) | (b3 << 8) | b4);
    }

    public long ReadLong()
    {
        int high = unchecked((int)ReadUnsignedInt());
        int low = unchecked((int)ReadUnsignedInt());
        return ((long)high << 32) | (long)(uint)low;
    }

    public float Read32Fixed()
    {
        float retval = ReadSignedShort();
        retval += ReadUnsignedShort() / 65536f;
        return retval;
    }

    public string ReadTag() => System.Text.Encoding.ASCII.GetString(ReadBytes(4));

    public string ReadString(int length) => System.Text.Encoding.Latin1.GetString(ReadBytes(length));

    public int[] ReadUnsignedByteArray(int length)
    {
        int[] values = new int[length];
        for (int i = 0; i < length; i++)
        {
            values[i] = ReadUnsignedByte();
        }

        return values;
    }

    public int[] ReadUnsignedShortArray(int length)
    {
        int[] values = new int[length];
        for (int i = 0; i < length; i++)
        {
            values[i] = ReadUnsignedShort();
        }

        return values;
    }
}
