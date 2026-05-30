/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/main/java/org/apache/fontbox/ttf/TTFDataStream.java
 * PDFBOX_SOURCE_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 7e9effef313cb0ff091e741d7d4aa58c3b1ecdbf
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
using TextEncoding = System.Text.Encoding;
using PdfBox.Net.IO;

namespace PdfBox.Net.FontBox.TTF;

/// <summary>
/// An abstract class to read a data stream.
/// </summary>
public abstract class TTFDataStream : IDisposable
{
    private static readonly DateTimeOffset TtfEpoch = new(1904, 1, 1, 0, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Read a 16.16 fixed value, where the first 16 bits are the decimal and the last 16 bits are the fraction.
    /// </summary>
    public float Read32Fixed()
    {
        float retval = ReadSignedShort();
        retval += ReadUnsignedShort() / 65536f;
        return retval;
    }

    /// <summary>
    /// Read a fixed length ascii string.
    /// </summary>
    public string ReadString(int length)
    {
        return ReadString(length, TextEncoding.Latin1);
    }

    /// <summary>
    /// Read a fixed length string.
    /// </summary>
    public string ReadString(int length, TextEncoding charset)
    {
        return charset.GetString(Read(length));
    }

    /// <summary>
    /// Read an unsigned byte.
    /// </summary>
    public abstract int Read();

    /// <summary>
    /// Read an unsigned byte.
    /// </summary>
    public abstract long ReadLong();

    /// <summary>
    /// Read a signed byte.
    /// </summary>
    public int ReadSignedByte()
    {
        int signedByte = Read();
        return signedByte <= 127 ? signedByte : signedByte - 256;
    }

    /// <summary>
    /// Read an unsigned byte. Similar to <see cref="Read()"/>, but throws an exception if EOF is unexpectedly reached.
    /// </summary>
    public int ReadUnsignedByte()
    {
        int unsignedByte = Read();
        if (unsignedByte == -1)
        {
            throw new EndOfStreamException("premature EOF");
        }

        return unsignedByte;
    }

    /// <summary>
    /// Read an unsigned integer.
    /// </summary>
    public uint ReadUnsignedInt()
    {
        long byte1 = Read();
        long byte2 = Read();
        long byte3 = Read();
        long byte4 = Read();
        if (byte4 < 0)
        {
            throw new EndOfStreamException($"EOF at {GetCurrentPosition()}, b1: {byte1}, b2: {byte2}, b3: {byte3}, b4: {byte4}");
        }

        return (uint)((byte1 << 24) + (byte2 << 16) + (byte3 << 8) + byte4);
    }

    /// <summary>
    /// Read an unsigned short.
    /// </summary>
    public ushort ReadUnsignedShort()
    {
        int b1 = Read();
        int b2 = Read();
        if ((b1 | b2) < 0)
        {
            throw new EndOfStreamException($"EOF at {GetCurrentPosition()}, b1: {b1}, b2: {b2}");
        }

        return (ushort)((b1 << 8) + b2);
    }

    /// <summary>
    /// Read an unsigned byte array.
    /// </summary>
    public int[] ReadUnsignedByteArray(int length)
    {
        int[] array = new int[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = Read();
        }

        return array;
    }

    /// <summary>
    /// Read an unsigned short array.
    /// </summary>
    public int[] ReadUnsignedShortArray(int length)
    {
        int[] array = new int[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = ReadUnsignedShort();
        }

        return array;
    }

    /// <summary>
    /// Read a signed short.
    /// </summary>
    public short ReadSignedShort()
    {
        return unchecked((short)ReadUnsignedShort());
    }

    /// <summary>
    /// Read an eight byte international date.
    /// </summary>
    public DateTimeOffset ReadInternationalDate()
    {
        long secondsSince1904 = ReadLong();
        return TtfEpoch.AddSeconds(secondsSince1904);
    }

    /// <summary>
    /// Reads a tag, an array of four uint8s used to identify a script, language system, feature, or baseline.
    /// </summary>
    public string ReadTag()
    {
        return TextEncoding.ASCII.GetString(Read(4));
    }

    /// <summary>
    /// Seek into the datasource.
    /// </summary>
    public abstract void Seek(long pos);

    /// <summary>
    /// Read a specific number of bytes from the stream.
    /// </summary>
    public byte[] Read(int numberOfBytes)
    {
        byte[] data = new byte[numberOfBytes];
        int amountRead;
        int totalAmountRead = 0;
        while (totalAmountRead < numberOfBytes && (amountRead = Read(data, totalAmountRead, numberOfBytes - totalAmountRead)) != -1)
        {
            totalAmountRead += amountRead;
        }

        if (totalAmountRead == numberOfBytes)
        {
            return data;
        }

        throw new IOException("Unexpected end of TTF stream reached");
    }

    public byte[] ReadBytes(int numberOfBytes) => Read(numberOfBytes);

    /// <summary>
    /// See <see cref="Stream.Read(byte[], int, int)"/>.
    /// </summary>
    public abstract int Read(byte[] b, int off, int len);

    /// <summary>
    /// Creates a view from current position to <c>pos + length</c>.
    /// </summary>
    public virtual RandomAccessRead? CreateSubView(long length)
    {
        return null;
    }

    /// <summary>
    /// Get the current position in the stream.
    /// </summary>
    public abstract long GetCurrentPosition();

    /// <summary>
    /// This will get the original data file that was used for this stream.
    /// </summary>
    public abstract Stream GetOriginalData();

    /// <summary>
    /// This will get the original data size that was used for this stream.
    /// </summary>
    public abstract long GetOriginalDataSize();

    public abstract void Close();

    public void Dispose() => Close();
}
