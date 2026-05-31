/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/test/java/org/apache/fontbox/cff/CFFEncodingTest.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 */

/*
 * Copyright 2017 The Apache Software Foundation.
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

using PdfBox.Net.FontBox.CFF;

namespace PdfBox.Net.FontBox.Tests;

public class CFFEncodingTest
{
    [Fact]
    public void TestCFFExpertEncoding()
    {
        CFFExpertEncoding cffExpertEncoding = CFFExpertEncoding.GetInstance();
        // check some randomly chosen mappings
        Assert.Equal(".notdef", cffExpertEncoding.GetName(0));
        Assert.Equal("space", cffExpertEncoding.GetName(32));
        Assert.Equal("Psmall", cffExpertEncoding.GetName(112));
        Assert.Equal("Ucircumflexsmall", cffExpertEncoding.GetName(251));
        Assert.Equal(32, cffExpertEncoding.GetCode("space"));
        Assert.Equal(112, cffExpertEncoding.GetCode("Psmall"));
        Assert.Equal(251, cffExpertEncoding.GetCode("Ucircumflexsmall"));
    }

    [Fact]
    public void TestCFFStandardEncoding()
    {
        CFFStandardEncoding cffStandardEncoding = CFFStandardEncoding.INSTANCE;
        // check some randomly chosen mappings
        Assert.Equal(".notdef", cffStandardEncoding.GetName(0));
        Assert.Equal("space", cffStandardEncoding.GetName(32));
        Assert.Equal("p", cffStandardEncoding.GetName(112));
        Assert.Equal("germandbls", cffStandardEncoding.GetName(251));
        Assert.Equal(32, cffStandardEncoding.GetCode("space"));
        Assert.Equal(112, cffStandardEncoding.GetCode("p"));
        Assert.Equal(251, cffStandardEncoding.GetCode("germandbls"));
    }
}
