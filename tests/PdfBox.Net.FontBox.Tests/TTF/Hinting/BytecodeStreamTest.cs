/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/test/java/org/apache/fontbox/ttf/BytecodeStreamTest.java
 * PDFBOX_SOURCE_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: 5206dee5bfe0f435609f6506e3fccccbc46a95cc
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
using System.Numerics;
using System.Text;
using System.Globalization;
using PdfBox.Net.Util.Geometry;
using Microsoft.Extensions.Logging;
using PdfBox.Net.Logging;

using PdfBox.Net.FontBox.TTF;
using ExecutionContext = PdfBox.Net.FontBox.TTF.ExecutionContext;

namespace PdfBox.Net.FontBox.Tests.TTF.Hinting;

/// <summary>
/// Tests for the BytecodeStream cursor, including bounds checking.
/// </summary>
public class BytecodeStreamTest
{
    [Fact]
    public void TestSequentialReads()
    {
        BytecodeStream s = new BytecodeStream(new byte[] { (byte) 0xB0, 0x05, (byte) 0xFF, 0x01 });
        Assert.True(s.HasNext());
        Equal(0xB0, s.NextByte());
        Equal(5, s.NextByte());
        // 0xFF01 as a signed word is negative
        Equal(unchecked((short) 0xFF01), s.NextWord());
        Assert.False(s.HasNext());
    }

    [Fact]
    public void TestByteReadPastEndThrows()
    {
        BytecodeStream s = new BytecodeStream(new byte[] { 0x01 });
        Equal(1, s.NextByte());
        Assert.Throws<HintingException>( () => s.NextByte());
    }

    [Fact]
    public void TestWordReadPastEndThrows()
    {
        // only one byte, but a word needs two
        BytecodeStream s = new BytecodeStream(new byte[] { 0x01 });
        Assert.Throws<HintingException>( () => s.NextWord());
    }

    [Fact]
    public void TestSeekOutOfRangeThrows()
    {
        BytecodeStream s = new BytecodeStream(new byte[] { 0x01, 0x02 });
        s.Seek(2); // end position is valid
        Assert.False(s.HasNext());
        Assert.Throws<HintingException>( () => s.Seek(3));
        Assert.Throws<HintingException>( () => s.Seek(-1));
    }

    [Fact]
    public void TestInstructionStartTracking()
    {
        BytecodeStream s = new BytecodeStream(new byte[] { 0x10, 0x11, 0x12 });
        s.NextByte();
        s.MarkInstructionStart();
        Equal(1, s.InstructionStart());
        Equal(0x11, s.NextByte());
    }
    private static void Equal<T>(T expected, T actual, string? message = null)
    {
        Assert.Equal(expected, actual);
    }

    private static void AssertTimeoutPreemptively(TimeSpan timeout, Action action)
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();
        action();
        Assert.True(timer.Elapsed < timeout, $"Execution exceeded {timeout}.");
    }

}
