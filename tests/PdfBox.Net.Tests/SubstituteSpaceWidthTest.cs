/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/pdmodel/font/SubstituteSpaceWidthTest.java
 * PDFBOX_SOURCE_COMMIT: a9ae066a942c122c9dded9bfa1b08dfae4078ba4
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: a9ae066a942c122c9dded9bfa1b08dfae4078ba4
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
using System.Text;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Font;

namespace PdfBox.Net.Tests;

/// <summary>
/// PDFBOX-6292: The space width of a non-embedded Identity-H CIDFontType2 without ToUnicode must
/// come from the PDF, not from whichever substitute is installed.
/// </summary>
[Collection("Identity CJK substitute")]
public sealed class SubstituteSpaceWidthTest
{
    [Fact]
    public void SpaceWidthIndependentOfSubstitute()
    {
        using IdentityCJKFixture fixture = new();
        PDType0Font font = (PDType0Font)PDFontFactory.CreateFont(SimSunIdentityH());
        PDCIDFontType2 cidFont = (PDCIDFontType2)font.GetDescendantFont()!;
        int substituteSpaceGid = cidFont.GetTrueTypeFont().GetUnicodeCmapLookup()!.GetGlyphId(' ');
        // The bundled fixture makes the upstream test deterministic on every host.
        Assert.InRange(substituteSpaceGid, 1, 31);
        Assert.Equal(500, font.GetStringWidth(" "));
        Assert.Equal(1000, font.GetWidth(32));
        Assert.Equal(font.GetWidth(32), font.GetSpaceWidth());
    }

    [Fact]
    public void ExplicitToUnicodeSpaceMappingStillSelectsPdfWidth()
    {
        using IdentityCJKFixture fixture = new();
        COSDictionary dictionary = SimSunIdentityH();
        COSStream toUnicode = new();
        using (Stream output = toUnicode.CreateOutputStream())
        {
            output.Write(Encoding.ASCII.GetBytes("""
                /CIDInit /ProcSet findresource begin 12 dict begin begincmap
                /CIDSystemInfo << /Registry (Adobe) /Ordering (UCS) /Supplement 0 >> def
                /CMapName /SpaceMap def /CMapType 2 def
                1 begincodespacerange <0000> <FFFF> endcodespacerange
                1 beginbfchar <0007> <0020> endbfchar
                endcmap CMapName currentdict /CMap defineresource pop end end
                """));
        }
        dictionary.SetItem(COSName.GetPDFName("ToUnicode"), toUnicode);
        PDType0Font font = (PDType0Font)PDFontFactory.CreateFont(dictionary);
        Assert.Equal(500, font.GetSpaceWidth());
        Assert.Equal(1000, font.GetWidth(32));
    }

    private static COSDictionary SimSunIdentityH()
    {
        COSDictionary type0 = IdentityCJKFixture.Type0("SimSun", "CIDFontType2", "Identity-H");
        COSDictionary cid = (COSDictionary)type0.GetCOSArray(COSName.GetPDFName("DescendantFonts"))!.GetObject(0)!;
        COSArray widths = new();
        for (int i = 0; i < 32; i++) widths.Add(COSInteger.Get(500));
        COSArray w = new();
        w.Add(COSInteger.Get(0));
        w.Add(widths);
        cid.SetItem(COSName.GetPDFName("W"), w);
        return type0;
    }
}
