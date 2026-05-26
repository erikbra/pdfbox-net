/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Focused xUnit coverage for the C# port of Apache PDFBox file type detection behavior.
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

using PdfBox.Net.Util.FileTypeDetector;

namespace PdfBox.Net.Tests;

public class FileTypeDetectorTest
{
    [Fact]
    public void DetectsPngMagicNumberFromByteArray()
    {
        byte[] bytes = [(byte)0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52];

        Assert.Equal(FileType.PNG, FileTypeDetector.DetectFileType(bytes));
    }

    [Fact]
    public void DetectFileTypeResetsSeekableStreamPosition()
    {
        byte[] bytes = [0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x01, 0x02, 0x03];
        using MemoryStream stream = new(bytes);

        FileType type = FileTypeDetector.DetectFileType(stream);

        Assert.Equal(FileType.GIF, type);
        Assert.Equal(0, stream.Position);
    }

    [Fact]
    public void DetectsUnknownWhenMagicNumberDoesNotMatch()
    {
        byte[] bytes = [0x01, 0x02, 0x03, 0x04];

        Assert.Equal(FileType.UNKNOWN, FileTypeDetector.DetectFileType(bytes));
    }
}
