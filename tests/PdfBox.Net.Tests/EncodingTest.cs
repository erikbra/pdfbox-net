/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/test/java/org/apache/fontbox/encoding/EncodingTest.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
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

using PdfBox.Net.FontBox.Encoding;

namespace PdfBox.Net.Tests;

public class EncodingTest
{
    [Fact]
    public void TestStandardEncoding()
    {
        StandardEncoding standardEncoding = StandardEncoding.INSTANCE;

        Assert.Equal(".notdef", standardEncoding.GetName(0));
        Assert.Equal("space", standardEncoding.GetName(32));
        Assert.Equal("p", standardEncoding.GetName(112));
        Assert.Equal("guilsinglleft", standardEncoding.GetName(172));
        Assert.Equal(32, standardEncoding.GetCode("space"));
        Assert.Equal(112, standardEncoding.GetCode("p"));
        Assert.Equal(172, standardEncoding.GetCode("guilsinglleft"));
    }

    [Fact]
    public void TestMacRomanEncoding()
    {
        MacRomanEncoding macRomanEncoding = MacRomanEncoding.INSTANCE;

        Assert.Equal(".notdef", macRomanEncoding.GetName(0));
        Assert.Equal("space", macRomanEncoding.GetName(32));
        Assert.Equal("p", macRomanEncoding.GetName(112));
        Assert.Equal("germandbls", macRomanEncoding.GetName(167));
        Assert.Equal(32, macRomanEncoding.GetCode("space"));
        Assert.Equal(112, macRomanEncoding.GetCode("p"));
        Assert.Equal(167, macRomanEncoding.GetCode("germandbls"));
    }

    [Fact]
    public void TestBuiltInEncodingFallbacks()
    {
        BuiltInEncoding builtInEncoding = new(new Dictionary<int, string>
        {
            [65] = "A",
            [66] = "B"
        });

        Assert.Equal("A", builtInEncoding.GetName(65));
        Assert.Equal(66, builtInEncoding.GetCode("B"));
        Assert.Equal(".notdef", builtInEncoding.GetName(999));
        Assert.Null(builtInEncoding.GetCode("missing"));
        Assert.Throws<NotSupportedException>(() => builtInEncoding.GetCodeToNameMap().Add(67, "C"));
    }
}
