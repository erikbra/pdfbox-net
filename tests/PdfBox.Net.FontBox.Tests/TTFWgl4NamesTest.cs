/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/test/java/org/apache/fontbox/ttf/WGL4NamesTest.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 */

/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements. See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using PdfBox.Net.FontBox.TTF;

namespace PdfBox.Net.FontBox.Tests;

public class TTFWgl4NamesTest
{
    [Fact]
    public void TestAllNames()
    {
        string[] allNames = WGL4Names.GetAllNames();
        Assert.NotNull(allNames);
        Assert.Equal(WGL4Names.NumberOfMacGlyphs, allNames.Length);
    }

    [Fact]
    public void TestGetGlyphName()
    {
        Assert.Equal(".notdef", WGL4Names.GetGlyphName(0));
        Assert.Equal("equal", WGL4Names.GetGlyphName(32));
        Assert.Equal("h", WGL4Names.GetGlyphName(75));
        Assert.Equal("Aacute", WGL4Names.GetGlyphName(201));
        Assert.Equal("Ocircumflex", WGL4Names.GetGlyphName(209));
        Assert.Equal("ccaron", WGL4Names.GetGlyphName(256));
        Assert.Null(WGL4Names.GetGlyphName(WGL4Names.NumberOfMacGlyphs + 1));
        Assert.Null(WGL4Names.GetGlyphName(-1));
    }

    [Fact]
    public void TestGlyphIndices()
    {
        Assert.Equal(0, WGL4Names.GetGlyphIndex(".notdef"));
        Assert.Equal(32, WGL4Names.GetGlyphIndex("equal"));
        Assert.Equal(75, WGL4Names.GetGlyphIndex("h"));
        Assert.Equal(201, WGL4Names.GetGlyphIndex("Aacute"));
        Assert.Equal(209, WGL4Names.GetGlyphIndex("Ocircumflex"));
        Assert.Equal(256, WGL4Names.GetGlyphIndex("ccaron"));
        Assert.Null(WGL4Names.GetGlyphIndex("INVALID"));
    }
}
