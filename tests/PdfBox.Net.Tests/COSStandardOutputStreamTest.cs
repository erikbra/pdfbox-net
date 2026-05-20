/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Added focused xUnit coverage for the C# port of Apache PDFBox COSStandardOutputStream behavior.
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
using PdfBox.Net.PdfWriter;
using Xunit;

namespace PdfBox.Net.Tests;

/// <summary>
/// Unittest for <see cref="COSStandardOutputStream"/>.
/// </summary>
public class COSStandardOutputStreamTest
{
    [Fact]
    public void TestPositionAndEolHandling()
    {
        using var memoryStream = new MemoryStream();
        var output = new COSStandardOutputStream(memoryStream);

        Assert.Equal(0, output.GetPos());
        Assert.False(output.IsOnNewLine());

        output.WriteByte((byte)'A');
        output.WriteCRLF();
        output.WriteEOL();
        output.WriteEOL();
        output.WriteLF();

        Assert.Equal(5, output.GetPos());
        Assert.False(output.IsOnNewLine());
        Assert.Equal([(byte)'A', (byte)'\r', (byte)'\n', (byte)'\n', (byte)'\n'], memoryStream.ToArray());
    }

    [Fact]
    public void TestInitialPositionConstructorAndWriteResetNewlineState()
    {
        using var memoryStream = new MemoryStream();
        var output = new COSStandardOutputStream(memoryStream, 9);

        output.SetOnNewLine(true);
        output.WriteByte((byte)'B');

        Assert.Equal(10, output.GetPos());
        Assert.False(output.IsOnNewLine());
        Assert.Equal([(byte)'B'], memoryStream.ToArray());
    }

    [Fact]
    public void TestUnsupportedReadSeekLengthOperations()
    {
        using var memoryStream = new MemoryStream();
        var output = new COSStandardOutputStream(memoryStream);

        Assert.False(output.CanRead);
        Assert.False(output.CanSeek);
        Assert.True(output.CanWrite);
        Assert.Throws<NotSupportedException>(() => _ = output.Length);
        Assert.Throws<NotSupportedException>(() => output.Read([], 0, 0));
        Assert.Throws<NotSupportedException>(() => output.Seek(0, SeekOrigin.Begin));
        Assert.Throws<NotSupportedException>(() => output.SetLength(0));
    }
}
