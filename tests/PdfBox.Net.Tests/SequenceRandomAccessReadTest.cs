/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 * PDFBOX_SOURCE_PATH: io/src/test/java/org/apache/pdfbox/io/SequenceRandomAccessReadTest.java
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
using System.Collections.Generic;
using System.IO;
using System.Text;
using PdfBox.Net.IO;
using Xunit;

namespace PdfBox.Net.Tests;

/// <summary>
/// Unittest for <see cref="SequenceRandomAccessRead"/>.
/// </summary>
public class SequenceRandomAccessReadTest
{
    [Fact]
    public void TestCreateAndRead()
    {
        string input1 = "This is a test string number 1";
        var randomAccessReadBuffer1 = new RandomAccessReadBuffer(Encoding.ASCII.GetBytes(input1));
        string input2 = "This is a test string number 2";
        var randomAccessReadBuffer2 = new RandomAccessReadBuffer(Encoding.ASCII.GetBytes(input2));
        var inputList = new List<RandomAccessRead> { randomAccessReadBuffer1, randomAccessReadBuffer2 };

        var sequenceRandomAccessRead = new SequenceRandomAccessRead(inputList);
        try
        {
            Assert.Throws<NotSupportedException>(() => sequenceRandomAccessRead.CreateView(0, 10));

            int overallLength = input1.Length + input2.Length;
            Assert.Equal(overallLength, sequenceRandomAccessRead.Length());

            byte[] bytesRead = new byte[overallLength];
            Assert.Equal(overallLength, ((RandomAccessRead)sequenceRandomAccessRead).Read(bytesRead));
            Assert.Equal(input1 + input2, Encoding.ASCII.GetString(bytesRead));
        }
        finally
        {
            sequenceRandomAccessRead.Close();
        }

        // test missing parameter
        Assert.Throws<ArgumentException>(() => new SequenceRandomAccessRead(null!));

        // test empty list
        var emptyList = new List<RandomAccessRead>();
        Assert.Throws<ArgumentException>(() => new SequenceRandomAccessRead(emptyList));

        // test closed readers - should throw because readers are already closed
        Assert.Throws<IOException>(() => new SequenceRandomAccessRead(inputList));
    }

    [Fact]
    public void TestSeekPeekAndRewind()
    {
        string input1 = "01234567890123456789";
        var randomAccessReadBuffer1 = new RandomAccessReadBuffer(Encoding.ASCII.GetBytes(input1));
        string input2 = "abcdefghijklmnopqrst";
        var randomAccessReadBuffer2 = new RandomAccessReadBuffer(Encoding.ASCII.GetBytes(input2));
        var inputList = new List<RandomAccessRead> { randomAccessReadBuffer1, randomAccessReadBuffer2 };

        var sequenceRandomAccessRead = new SequenceRandomAccessRead(inputList);
        try
        {
            // test seek, rewind and peek in the first part of the sequence
            sequenceRandomAccessRead.Seek(4);
            Assert.Equal(4, sequenceRandomAccessRead.GetPosition());
            Assert.Equal('4', sequenceRandomAccessRead.Read());
            Assert.Equal(5, sequenceRandomAccessRead.GetPosition());
            ((RandomAccessRead)sequenceRandomAccessRead).Rewind(1);
            Assert.Equal(4, sequenceRandomAccessRead.GetPosition());
            Assert.Equal('4', sequenceRandomAccessRead.Read());
            Assert.Equal('5', ((RandomAccessRead)sequenceRandomAccessRead).Peek());
            Assert.Equal(5, sequenceRandomAccessRead.GetPosition());
            Assert.Equal('5', sequenceRandomAccessRead.Read());
            Assert.Equal(6, sequenceRandomAccessRead.GetPosition());

            // test seek, rewind and peek in the second part of the sequence
            sequenceRandomAccessRead.Seek(24);
            Assert.Equal(24, sequenceRandomAccessRead.GetPosition());
            Assert.Equal('e', sequenceRandomAccessRead.Read());
            ((RandomAccessRead)sequenceRandomAccessRead).Rewind(1);
            Assert.Equal('e', sequenceRandomAccessRead.Read());
            Assert.Equal('f', ((RandomAccessRead)sequenceRandomAccessRead).Peek());
            Assert.Equal('f', sequenceRandomAccessRead.Read());

            Assert.Throws<IOException>(() => sequenceRandomAccessRead.Seek(-1));
        }
        finally
        {
            sequenceRandomAccessRead.Close();
        }
    }

    [Fact]
    public void TestBorderCases()
    {
        string input1 = "01234567890123456789";
        var randomAccessReadBuffer1 = new RandomAccessReadBuffer(Encoding.ASCII.GetBytes(input1));
        string input2 = "abcdefghijklmnopqrst";
        var randomAccessReadBuffer2 = new RandomAccessReadBuffer(Encoding.ASCII.GetBytes(input2));
        var inputList = new List<RandomAccessRead> { randomAccessReadBuffer1, randomAccessReadBuffer2 };

        var sequenceRandomAccessRead = new SequenceRandomAccessRead(inputList);
        try
        {
            // jump to the last byte of the first part of the sequence
            sequenceRandomAccessRead.Seek(19);
            Assert.Equal('9', sequenceRandomAccessRead.Read());
            ((RandomAccessRead)sequenceRandomAccessRead).Rewind(1);
            Assert.Equal('9', sequenceRandomAccessRead.Read());
            Assert.Equal('a', ((RandomAccessRead)sequenceRandomAccessRead).Peek());
            Assert.Equal('a', sequenceRandomAccessRead.Read());

            // jump back to the first sequence
            sequenceRandomAccessRead.Seek(17);
            byte[] bytesRead = new byte[6];
            Assert.Equal(6, ((RandomAccessRead)sequenceRandomAccessRead).Read(bytesRead));
            Assert.Equal("789abc", Encoding.ASCII.GetString(bytesRead));
            Assert.Equal(23, sequenceRandomAccessRead.GetPosition());

            // rewind back to the first sequence
            ((RandomAccessRead)sequenceRandomAccessRead).Rewind(6);
            Assert.Equal(17, sequenceRandomAccessRead.GetPosition());
            bytesRead = new byte[6];
            Assert.Equal(6, ((RandomAccessRead)sequenceRandomAccessRead).Read(bytesRead));
            Assert.Equal("789abc", Encoding.ASCII.GetString(bytesRead));

            // jump to the start of the sequence
            sequenceRandomAccessRead.Seek(0);
            bytesRead = new byte[6];
            Assert.Equal(6, ((RandomAccessRead)sequenceRandomAccessRead).Read(bytesRead));
            Assert.Equal("012345", Encoding.ASCII.GetString(bytesRead));
        }
        finally
        {
            sequenceRandomAccessRead.Close();
        }
    }

    [Fact]
    public void TestEOF()
    {
        string input1 = "01234567890123456789";
        var randomAccessReadBuffer1 = new RandomAccessReadBuffer(Encoding.ASCII.GetBytes(input1));
        string input2 = "abcdefghijklmnopqrst";
        var randomAccessReadBuffer2 = new RandomAccessReadBuffer(Encoding.ASCII.GetBytes(input2));
        var inputList = new List<RandomAccessRead> { randomAccessReadBuffer1, randomAccessReadBuffer2 };

        var sequenceRandomAccessRead = new SequenceRandomAccessRead(inputList);

        int overallLength = input1.Length + input2.Length;

        sequenceRandomAccessRead.Seek(overallLength - 1);
        Assert.False(sequenceRandomAccessRead.IsEOF());
        Assert.Equal('t', ((RandomAccessRead)sequenceRandomAccessRead).Peek());
        Assert.False(sequenceRandomAccessRead.IsEOF());
        Assert.Equal('t', sequenceRandomAccessRead.Read());
        Assert.True(sequenceRandomAccessRead.IsEOF());
        Assert.Equal(-1, sequenceRandomAccessRead.Read());
        Assert.Equal(-1, sequenceRandomAccessRead.Read(new byte[1], 0, 1));

        // rewind
        ((RandomAccessRead)sequenceRandomAccessRead).Rewind(5);
        Assert.False(sequenceRandomAccessRead.IsEOF());
        byte[] bytesRead = new byte[5];
        Assert.Equal(5, ((RandomAccessRead)sequenceRandomAccessRead).Read(bytesRead));
        Assert.Equal("pqrst", Encoding.ASCII.GetString(bytesRead));
        Assert.True(sequenceRandomAccessRead.IsEOF());

        // seek to a position beyond the end of the input
        sequenceRandomAccessRead.Seek(overallLength + 10);
        Assert.True(sequenceRandomAccessRead.IsEOF());
        Assert.Equal(overallLength, sequenceRandomAccessRead.GetPosition());

        Assert.False(sequenceRandomAccessRead.IsClosed());
        sequenceRandomAccessRead.Close();
        Assert.True(sequenceRandomAccessRead.IsClosed());

        // closing a SequenceRandomAccessRead twice shouldn't be a problem
        sequenceRandomAccessRead.Close();

        Assert.Throws<IOException>(() => sequenceRandomAccessRead.Read());
    }

    [Fact]
    public void TestEmptyStream()
    {
        string input1 = "01234567890123456789";
        var randomAccessReadBuffer1 = new RandomAccessReadBuffer(Encoding.ASCII.GetBytes(input1));
        string input2 = "abcdefghijklmnopqrst";
        var randomAccessReadBuffer2 = new RandomAccessReadBuffer(Encoding.ASCII.GetBytes(input2));
        var emptyBuffer = new RandomAccessReadBuffer(Encoding.ASCII.GetBytes(""));

        var inputList = new List<RandomAccessRead> { randomAccessReadBuffer1, emptyBuffer, randomAccessReadBuffer2 };

        var sequenceRandomAccessRead = new SequenceRandomAccessRead(inputList);
        try
        {
            // check length - empty buffer is filtered out
            Assert.Equal(input1.Length + input2.Length, sequenceRandomAccessRead.Length());

            // read from both parts of the sequence
            byte[] bytesRead = new byte[10];
            sequenceRandomAccessRead.Seek(15);
            Assert.Equal(10, ((RandomAccessRead)sequenceRandomAccessRead).Read(bytesRead));
            Assert.Equal("56789abcde", Encoding.ASCII.GetString(bytesRead));

            // rewind and read again
            ((RandomAccessRead)sequenceRandomAccessRead).Rewind(15);
            bytesRead = new byte[5];
            Assert.Equal(5, ((RandomAccessRead)sequenceRandomAccessRead).Read(bytesRead));
            Assert.Equal("01234", Encoding.ASCII.GetString(bytesRead));

            // check EOF when reading
            bytesRead = new byte[5];
            sequenceRandomAccessRead.Seek(38);
            Assert.Equal(2, ((RandomAccessRead)sequenceRandomAccessRead).Read(bytesRead));
            Assert.Equal("st", Encoding.ASCII.GetString(bytesRead, 0, 2));

            // check EOF after seek
            sequenceRandomAccessRead.Seek(40);
            Assert.True(sequenceRandomAccessRead.IsEOF());
        }
        finally
        {
            sequenceRandomAccessRead.Close();
        }
    }

    [Fact]
    public void TestLargeBuffers()
    {
        var r1 = new RandomAccessReadBuffer(new byte[2448]);
        var r2 = new RandomAccessReadBuffer(new byte[2412]);
        var r3 = new RandomAccessReadBuffer(new byte[2417]);
        var r4 = new RandomAccessReadBuffer(new byte[2433]);
        var r5 = new RandomAccessReadBuffer(new byte[2432]);
        var r6 = new RandomAccessReadBuffer(new byte[2416]);
        var r7 = new RandomAccessReadBuffer(new byte[2417]);
        var r8 = new RandomAccessReadBuffer(new byte[2266]);

        var readerList = new List<RandomAccessRead> { r1, r2, r3, r4, r5, r6, r7, r8 };
        var sequenceRandomAccessRead = new SequenceRandomAccessRead(readerList);
        try
        {
            int expectedLength = 2448 + 2412 + 2417 + 2433 + 2432 + 2416 + 2417 + 2266;
            Assert.Equal(expectedLength, sequenceRandomAccessRead.Length());

            byte[] result = new byte[expectedLength];
            int bytesRead = ((RandomAccessRead)sequenceRandomAccessRead).Read(result);
            Assert.Equal(expectedLength, bytesRead);
            Assert.Equal(sequenceRandomAccessRead.Length(), bytesRead);
        }
        finally
        {
            sequenceRandomAccessRead.Close();
        }
    }
}
