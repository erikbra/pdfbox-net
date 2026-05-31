/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/test/java/org/apache/fontbox/cmap/CIDRangeTest.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
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

using PdfBox.Net.FontBox.CMap;

namespace PdfBox.Net.FontBox.Tests;

public class CMapCIDRangeTest
{
    [Fact]
    public void TestCIDRangeOneByte()
    {
        CIDRange cidRange = new(0, 20, 65, 1);
        Assert.Equal(1, cidRange.CodeLength);

        Assert.Equal(65, cidRange.Map([0]));
        Assert.Equal(75, cidRange.Map([10]));
        // out of range
        Assert.Equal(-1, cidRange.Map([30]));
        // wrong code length
        Assert.Equal(-1, cidRange.Map([0, 10]));

        Assert.Equal(65, cidRange.Map(0, 1));
        Assert.Equal(75, cidRange.Map(10, 1));
        // out of range
        Assert.Equal(-1, cidRange.Map(30, 1));
        // wrong code length
        Assert.Equal(-1, cidRange.Map(10, 2));

        Assert.Equal(0, cidRange.Unmap(65));
        Assert.Equal(10, cidRange.Unmap(75));
        // out of range
        Assert.Equal(-1, cidRange.Unmap(100));
    }

    [Fact]
    public void TestCIDRangeTwoByte()
    {
        CIDRange cidRange = new(256, 280, 65, 2);
        Assert.Equal(2, cidRange.CodeLength);

        Assert.Equal(65, cidRange.Map([1, 0]));
        Assert.Equal(75, cidRange.Map([1, 10]));
        // out of range
        Assert.Equal(-1, cidRange.Map([1, 30]));
        // wrong code length
        Assert.Equal(-1, cidRange.Map([10]));

        Assert.Equal(65, cidRange.Map(256, 2));
        Assert.Equal(75, cidRange.Map(266, 2));
        // out of range
        Assert.Equal(-1, cidRange.Map(290, 2));
        // wrong code length
        Assert.Equal(-1, cidRange.Map(256, 1));

        Assert.Equal(256, cidRange.Unmap(65));
        Assert.Equal(266, cidRange.Unmap(75));
        // out of range
        Assert.Equal(-1, cidRange.Unmap(100));
    }
}
