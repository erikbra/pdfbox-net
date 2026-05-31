/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/test/java/org/apache/fontbox/cmap/TestCodespaceRange.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
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

using PdfBox.Net.FontBox.CMap;

namespace PdfBox.Net.Tests;

/// <summary>
/// This will test the CodeSpaceRange implementation.
/// </summary>
public class CMapCodespaceRangeTest
{
    /// <summary>
    /// Check whether the code length calculation works.
    /// </summary>
    [Fact]
    public void TestCodeLength()
    {
        byte[] startBytes1 = [0x00];
        byte[] endBytes1 = [0x20];
        CodespaceRange range1 = new(startBytes1, endBytes1);
        Assert.Equal(1, range1.CodeLength);

        byte[] startBytes2 = [0x00, 0x00];
        byte[] endBytes2 = [0x01, 0x20];
        CodespaceRange range2 = new(startBytes2, endBytes2);
        Assert.Equal(2, range2.CodeLength);
    }

    /// <summary>
    /// Check whether the constructor checks the length of the start and end bytes.
    /// </summary>
    [Fact]
    public void TestConstructor()
    {
        // PDFBOX-4923 "1 begincodespacerange <00> <ffff> endcodespacerange" case is accepted
        byte[] startBytes1 = [0x00];
        byte[] endBytes2 = [0xFF, 0xFF];
        _ = new CodespaceRange(startBytes1, endBytes2); // should not throw

        // other cases of different lengths are not accepted
        byte[] startBytes3 = [0x01];
        byte[] endBytes4 = [0x01, 0x20];
        Assert.Throws<ArgumentException>(() => new CodespaceRange(startBytes3, endBytes4));
    }

    [Fact]
    public void TestMatches()
    {
        byte[] startBytes1 = [0x00];
        byte[] endBytes1 = [0xA0];
        CodespaceRange range1 = new(startBytes1, endBytes1);
        // check start and end value
        Assert.True(range1.Matches([0x00]));
        Assert.True(range1.Matches([0xA0]));
        // check any value within range
        Assert.True(range1.Matches([0x10]));
        // check first value out of range
        Assert.False(range1.Matches([0xA1]));
        // check any value out of range
        Assert.False(range1.Matches([0xD0]));
        // check any value with a different code length
        Assert.False(range1.Matches([0x00, 0x10]));

        byte[] startBytes2 = [0x81, 0x40];
        byte[] endBytes2 = [0x9F, 0xFC];
        CodespaceRange range2 = new(startBytes2, endBytes2);
        // check lower start and end value
        Assert.True(range2.Matches([0x81, 0x40]));
        Assert.True(range2.Matches([0x81, 0xFC]));
        // check higher start and end value
        Assert.True(range2.Matches([0x81, 0x40]));
        Assert.True(range2.Matches([0x9F, 0x40]));
        // check any value within lower range
        Assert.True(range2.Matches([0x81, 0x65]));
        // check any value within higher range
        Assert.True(range2.Matches([0x90, 0x40]));
        // check first value out of lower range
        Assert.False(range2.Matches([0x81, 0xFD]));
        // check first value out of higher range
        Assert.False(range2.Matches([0xA0, 0x40]));
        // check any value out of lower range
        Assert.False(range2.Matches([0x81, 0x20]));
        // check any value out of higher range
        Assert.False(range2.Matches([0x10, 0x40]));
        // check value between start and end but not within the rectangular range
        Assert.False(range2.Matches([0x82, 0x20]));
        // check any value with a different code length
        Assert.False(range2.Matches([0x00]));
    }
}
