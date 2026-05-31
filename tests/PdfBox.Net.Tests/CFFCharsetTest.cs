/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/test/java/org/apache/fontbox/cff/CFFCharsetTest.java
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

namespace PdfBox.Net.Tests;

public class CFFCharsetTest
{
    [Fact]
    public void TestEmbeddedCharset()
    {
        // true -> CFFCharsetCID
        EmbeddedCharset embeddedCharsetCID = new(true);
        Assert.True(embeddedCharsetCID.IsCIDFont());
        embeddedCharsetCID.AddCID(10, 20);
        // test existing mapping
        Assert.Equal(10, embeddedCharsetCID.GetGIDForCID(20));
        Assert.Equal(20, embeddedCharsetCID.GetCIDForGID(10));
        // test not existing mapping
        Assert.Equal(0, embeddedCharsetCID.GetGIDForCID(99));
        Assert.Equal(0, embeddedCharsetCID.GetCIDForGID(99));
        // test not allowed method calls
        Assert.Throws<InvalidOperationException>(() => embeddedCharsetCID.GetSIDForGID(0));
        Assert.Throws<InvalidOperationException>(() => embeddedCharsetCID.GetGIDForSID(0));
        Assert.Throws<InvalidOperationException>(() => embeddedCharsetCID.AddSID(0, 0, "test"));
        Assert.Throws<InvalidOperationException>(() => embeddedCharsetCID.GetSID("test"));
        Assert.Throws<InvalidOperationException>(() => embeddedCharsetCID.GetNameForGID(0));

        // false -> CFFCharsetType1
        EmbeddedCharset embeddedCharsetType1 = new(false);
        Assert.False(embeddedCharsetType1.IsCIDFont());
        embeddedCharsetType1.AddSID(10, 20, "test");
        // test existing mapping
        Assert.Equal(20, embeddedCharsetType1.GetSID("test"));
        Assert.Equal(10, embeddedCharsetType1.GetGIDForSID(20));
        Assert.Equal(20, embeddedCharsetType1.GetSIDForGID(10));
        // test not existing mapping
        Assert.Equal(0, embeddedCharsetType1.GetGIDForSID(99));
        Assert.Equal(0, embeddedCharsetType1.GetSIDForGID(99));
        // test not allowed method calls
        Assert.Throws<InvalidOperationException>(() => embeddedCharsetType1.GetCIDForGID(0));
        Assert.Throws<InvalidOperationException>(() => embeddedCharsetType1.GetGIDForCID(0));
        Assert.Throws<InvalidOperationException>(() => embeddedCharsetType1.AddCID(0, 0));
    }

    [Fact]
    public void TestCFFCharsetCID()
    {
        CFFCharsetCID cffCharsetCID = new();
        Assert.True(cffCharsetCID.IsCIDFont());
        cffCharsetCID.AddCID(10, 20);
        // test existing mapping
        Assert.Equal(10, cffCharsetCID.GetGIDForCID(20));
        Assert.Equal(20, cffCharsetCID.GetCIDForGID(10));
        // test not existing mapping
        Assert.Equal(0, cffCharsetCID.GetGIDForCID(99));
        Assert.Equal(0, cffCharsetCID.GetCIDForGID(99));
        // test not allowed method calls
        Assert.Throws<InvalidOperationException>(() => cffCharsetCID.GetSIDForGID(0));
        Assert.Throws<InvalidOperationException>(() => cffCharsetCID.GetGIDForSID(0));
        Assert.Throws<InvalidOperationException>(() => cffCharsetCID.AddSID(0, 0, "test"));
        Assert.Throws<InvalidOperationException>(() => cffCharsetCID.GetSID("test"));
        Assert.Throws<InvalidOperationException>(() => cffCharsetCID.GetNameForGID(0));
    }

    [Fact]
    public void TestCFFCharsetType1()
    {
        CFFCharsetType1 cffCharsetType1 = new();
        Assert.False(cffCharsetType1.IsCIDFont());
        cffCharsetType1.AddSID(10, 20, "test");
        // test existing mapping
        Assert.Equal(20, cffCharsetType1.GetSID("test"));
        Assert.Equal(10, cffCharsetType1.GetGIDForSID(20));
        Assert.Equal(20, cffCharsetType1.GetSIDForGID(10));
        // test not existing mapping
        Assert.Equal(0, cffCharsetType1.GetGIDForSID(99));
        Assert.Equal(0, cffCharsetType1.GetSIDForGID(99));
        // test not allowed method calls
        Assert.Throws<InvalidOperationException>(() => cffCharsetType1.GetCIDForGID(0));
        Assert.Throws<InvalidOperationException>(() => cffCharsetType1.GetGIDForCID(0));
        Assert.Throws<InvalidOperationException>(() => cffCharsetType1.AddCID(0, 0));
    }

    [Fact]
    public void TestCFFExpertCharset()
    {
        CFFExpertCharset cffExpertCharset = CFFExpertCharset.GetInstance();
        // check .notdef mapping
        Assert.Equal(0, cffExpertCharset.GetSIDForGID(0));
        Assert.Equal(0, cffExpertCharset.GetSID(".notdef"));
        Assert.Equal(".notdef", cffExpertCharset.GetNameForGID(0));
        // check some randomly chosen mappings
        Assert.Equal(253, cffExpertCharset.GetSIDForGID(32));
        Assert.Equal(253, cffExpertCharset.GetSID("asuperior"));
        Assert.Equal("asuperior", cffExpertCharset.GetNameForGID(32));

        Assert.Equal(240, cffExpertCharset.GetSIDForGID(17));
        Assert.Equal(240, cffExpertCharset.GetSID("oneoldstyle"));
        Assert.Equal("oneoldstyle", cffExpertCharset.GetNameForGID(17));

        Assert.Equal(347, cffExpertCharset.GetSIDForGID(134));
        Assert.Equal(347, cffExpertCharset.GetSID("Agravesmall"));
        Assert.Equal("Agravesmall", cffExpertCharset.GetNameForGID(134));
    }

    [Fact]
    public void TestCFFExpertSubsetCharset()
    {
        CFFExpertSubsetCharset cffExpertSubsetCharset = CFFExpertSubsetCharset.GetInstance();
        // check .notdef mapping
        Assert.Equal(0, cffExpertSubsetCharset.GetSIDForGID(0));
        Assert.Equal(0, cffExpertSubsetCharset.GetSID(".notdef"));
        Assert.Equal(".notdef", cffExpertSubsetCharset.GetNameForGID(0));
        // check some randomly chosen mappings
        Assert.Equal(246, cffExpertSubsetCharset.GetSIDForGID(19));
        Assert.Equal(246, cffExpertSubsetCharset.GetSID("sevenoldstyle"));
        Assert.Equal("sevenoldstyle", cffExpertSubsetCharset.GetNameForGID(19));

        Assert.Equal(324, cffExpertSubsetCharset.GetSIDForGID(61));
        Assert.Equal(324, cffExpertSubsetCharset.GetSID("onethird"));
        Assert.Equal("onethird", cffExpertSubsetCharset.GetNameForGID(61));

        Assert.Equal(345, cffExpertSubsetCharset.GetSIDForGID(85));
        Assert.Equal(345, cffExpertSubsetCharset.GetSID("periodinferior"));
        Assert.Equal("periodinferior", cffExpertSubsetCharset.GetNameForGID(85));
    }

    [Fact]
    public void TestCFFISOAdobeCharset()
    {
        CFFISOAdobeCharset cffISOAdobeCharset = CFFISOAdobeCharset.GetInstance();
        // check .notdef mapping
        Assert.Equal(0, cffISOAdobeCharset.GetSIDForGID(0));
        Assert.Equal(0, cffISOAdobeCharset.GetSID(".notdef"));
        Assert.Equal(".notdef", cffISOAdobeCharset.GetNameForGID(0));

        // check some randomly chosen mappings
        Assert.Equal(32, cffISOAdobeCharset.GetSIDForGID(32));
        Assert.Equal(32, cffISOAdobeCharset.GetSID("question"));
        Assert.Equal("question", cffISOAdobeCharset.GetNameForGID(32));

        Assert.Equal(76, cffISOAdobeCharset.GetSIDForGID(76));
        Assert.Equal(76, cffISOAdobeCharset.GetSID("k"));
        Assert.Equal("k", cffISOAdobeCharset.GetNameForGID(76));

        Assert.Equal(218, cffISOAdobeCharset.GetSIDForGID(218));
        Assert.Equal(218, cffISOAdobeCharset.GetSID("odieresis"));
        Assert.Equal("odieresis", cffISOAdobeCharset.GetNameForGID(218));
    }
}
