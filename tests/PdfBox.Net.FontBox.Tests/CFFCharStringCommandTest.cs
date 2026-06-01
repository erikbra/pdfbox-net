/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/test/java/org/apache/fontbox/cff/CharStringCommandTest.java
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

using PdfBox.Net.FontBox.CFF;

namespace PdfBox.Net.FontBox.Tests;

public class CFFCharStringCommandTest
{
    [Fact]
    public void TestValue()
    {
        Assert.Equal(1, CharStringCommand.HSTEM.GetValue());
        Assert.Equal(12, CharStringCommand.ESCAPE.GetValue());
        Assert.Equal((12 << 4) + 0, CharStringCommand.DOTSECTION.GetValue());
        Assert.Equal((12 << 4) + 3, CharStringCommand.AND.GetValue());
        Assert.Equal(13, CharStringCommand.HSBW.GetValue());
    }

    [Fact]
    public void TestCharStringCommand()
    {
        CharStringCommand charStringCommand1 = CharStringCommandExtensions.GetInstance(1);
        Assert.Equal(Type1KeyWord.HSTEM, charStringCommand1.GetType1KeyWord());
        Assert.Equal(Type2KeyWord.HSTEM, charStringCommand1.GetType2KeyWord());

        CharStringCommand charStringCommand12_0 = CharStringCommandExtensions.GetInstance(12, 0);
        Assert.Equal(Type1KeyWord.DOTSECTION, charStringCommand12_0.GetType1KeyWord());
        Assert.Null(charStringCommand12_0.GetType2KeyWord());

        int[] values12_3 = [12, 3];
        CharStringCommand charStringCommand12_3 = CharStringCommandExtensions.GetInstance(values12_3);
        Assert.Null(charStringCommand12_3.GetType1KeyWord());
        Assert.Equal(Type2KeyWord.AND, charStringCommand12_3.GetType2KeyWord());
    }

    [Fact]
    public void TestUnknownCharStringCommand()
    {
        CharStringCommand charStringCommandUnknown = CharStringCommandExtensions.GetInstance(99);
        Assert.Equal(CharStringCommand.UNKNOWN, charStringCommandUnknown);
    }
}
