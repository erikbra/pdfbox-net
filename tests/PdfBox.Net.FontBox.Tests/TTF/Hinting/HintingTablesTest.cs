/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/test/java/org/apache/fontbox/ttf/HintingTablesTest.java
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
using PdfBox.Net.IO;
using ExecutionContext = PdfBox.Net.FontBox.TTF.ExecutionContext;

namespace PdfBox.Net.FontBox.Tests.TTF.Hinting;

/// <summary>
/// Tests parsing of the TrueType hinting tables ('cvt ', 'fpgm', 'prep', 'gasp').
///
/// LiberationSans-Regular was built as a hand-hinted, metric-compatible Arial replacement, so it
/// carries a full bytecode hinting program and all four tables. The expected values below were taken
/// from its on-disk table directory.
/// </summary>
public class HintingTablesTest : HintingTestSupport
{


    [Fact]
    public void TestControlValueTable()
    {
        using TrueTypeFont font = Parse("/ttf/LiberationSans-Regular.ttf");
        ControlValueTable? cvt = font.GetControlValues();
        NotNull(cvt);
        // 648-byte table / 2 bytes per FWord
        Equal(324, cvt.GetValueCount());
        Equal(324, cvt.GetValues().Length);
        // CVT entries are signed FWords (raw font units, not yet scaled to ppem)
        Equal(1484, cvt.GetValues()[0]);
    }

    [Fact]
    public void TestFontProgramTable()
    {
        using TrueTypeFont font = Parse("/ttf/LiberationSans-Regular.ttf");
        FontProgramTable? fpgm = font.GetFontProgram();
        NotNull(fpgm);
        Equal(1972, fpgm.GetProgram().Length);
    }

    [Fact]
    public void TestControlValueProgramTable()
    {
        using TrueTypeFont font = Parse("/ttf/LiberationSans-Regular.ttf");
        ControlValueProgramTable? prep = font.GetControlValueProgram();
        NotNull(prep);
        Equal(835, prep.GetProgram().Length);
    }

    [Fact]
    public void TestGaspTable()
    {
        using TrueTypeFont font = Parse("/ttf/LiberationSans-Regular.ttf");
        GaspTable? gasp = font.GetGasp();
        NotNull(gasp);
        Equal(0, gasp.GetVersion());

        // three ranges: (<=8: DOGRAY), (<=17: GRIDFIT), (<=65535: GRIDFIT|DOGRAY)
        Equal(new int[] { 8, 17, 65535 }, gasp.GetRangeMaxPPEM());
        Equal(new int[] { GaspTable.GASP_DOGRAY, GaspTable.GASP_GRIDFIT,
                GaspTable.GASP_GRIDFIT | GaspTable.GASP_DOGRAY }, gasp.GetRangeFlags());

        // ppem -> flags lookup, including range boundaries
        Equal(GaspTable.GASP_DOGRAY, gasp.GetFlags(8));
        Equal(GaspTable.GASP_GRIDFIT, gasp.GetFlags(9));
        Equal(GaspTable.GASP_GRIDFIT, gasp.GetFlags(17));
        Equal(GaspTable.GASP_GRIDFIT | GaspTable.GASP_DOGRAY, gasp.GetFlags(18));
        Equal(GaspTable.GASP_GRIDFIT | GaspTable.GASP_DOGRAY, gasp.GetFlags(2000));

        // grid-fitting is off at 8 ppem and below, on above it
        Assert.False(gasp.IsGridFit(8));
        Assert.True(gasp.IsGridFit(9));
        Assert.True(gasp.IsGridFit(16));
    }

    [Fact]
    public void TestAbsentTablesReturnNull()
    {
        // None of the bundled fonts lacks all four tables, but the absent-table path is covered:
        // JosefinSans-Italic has no 'cvt '/'fpgm' (but does have 'prep'/'gasp')...
        using TrueTypeFont josefin = Parse("/ttf/JosefinSans-Italic.ttf");
        IsNull(josefin.GetControlValues());
        IsNull(josefin.GetFontProgram());
        NotNull(josefin.GetControlValueProgram());
        NotNull(josefin.GetGasp());

        // ...and Lohit-Tamil has no 'gasp'. Absent accessors must return null, not throw.
        using TrueTypeFont lohit = Parse("/ttf/Lohit-Tamil.ttf");
        IsNull(lohit.GetGasp());
        NotNull(lohit.GetControlValues());
        NotNull(lohit.GetFontProgram());
        NotNull(lohit.GetControlValueProgram());
    }
}
