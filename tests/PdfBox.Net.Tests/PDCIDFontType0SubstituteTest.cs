/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/pdmodel/font/PDCIDFontType0SubstituteTest.java
 * PDFBOX_SOURCE_COMMIT: 046747da99a870902217efabf1c41297de157059
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 046747da99a870902217efabf1c41297de157059
 */
/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements. See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using PdfBox.Net.COS;
using PdfBox.Net.FontBox.TTF;
using PdfBox.Net.PDModel.Font;

namespace PdfBox.Net.Tests;

public class PDCIDFontType0SubstituteTest
{
    [Fact]
    public void DifferentCharacterCollection_ResolvesOutlineAndWidthViaUnicode()
    {
        using OpenTypeFont substitute = Assert.IsType<OpenTypeFont>(
            new OTFParser().Parse(FontBoxTestFixtures.CreateMinimalOpenTypeCff(cidKeyed: true)));
        COSDictionary descendantDictionary = new();
        descendantDictionary.SetName(COSName.GetPDFName("BaseFont"), "LegacyCnsFont");
        COSDictionary ros = new();
        ros.SetString(COSName.GetPDFName("Registry"), "Adobe");
        ros.SetString(COSName.GetPDFName("Ordering"), "CNS1");
        ros.SetInt(COSName.GetPDFName("Supplement"), 0);
        descendantDictionary.SetItem(COSName.GetPDFName("CIDSystemInfo"), ros);
        PDCIDFontType0 descendant = new(descendantDictionary, null, new SubstituteMapper(substitute));

        COSDictionary parentDictionary = new();
        parentDictionary.SetItem(COSName.GetPDFName("Encoding"), COSName.GetPDFName("Identity-H"));
        parentDictionary.SetItem(COSName.GetPDFName("ToUnicode"), COSName.GetPDFName("Identity-H"));
        PDType0Font parent = new(parentDictionary, descendant);

        Assert.False(parent.IsEmbedded());
        Assert.Equal(65, parent.CodeToCID(65));
        Assert.Equal(1, parent.CodeToGID(65));
        Assert.True(parent.HasGlyph(65));
        Assert.NotEmpty(parent.GetPath(65).Segments);
        Assert.Equal(500, parent.GetWidthFromFont(65), 3);
        Assert.False(parent.HasGlyph(66));
        Assert.Equal(0, parent.CodeToGID(66));
        Assert.Empty(parent.GetPath(66).Segments);
    }

    [Fact]
    public void PredefinedCnsEncodingWithoutToUnicodeResolvesSubstituteGlyph()
    {
        using OpenTypeFont substitute = Assert.IsType<OpenTypeFont>(
            new OTFParser().Parse(FontBoxTestFixtures.CreateMinimalOpenTypeCff(cidKeyed: true)));
        COSDictionary descendantDictionary = new();
        descendantDictionary.SetName(COSName.GetPDFName("BaseFont"), "LegacyCnsFont");
        COSDictionary ros = new();
        ros.SetString(COSName.GetPDFName("Registry"), "Adobe");
        ros.SetString(COSName.GetPDFName("Ordering"), "CNS1");
        ros.SetInt(COSName.GetPDFName("Supplement"), 0);
        descendantDictionary.SetItem(COSName.GetPDFName("CIDSystemInfo"), ros);
        PDCIDFontType0 descendant = new(descendantDictionary, null, new SubstituteMapper(substitute));
        COSDictionary parentDictionary = new();
        parentDictionary.SetItem(COSName.GetPDFName("Encoding"), COSName.GetPDFName("UniCNS-UTF16-H"));
        COSArray descendants = new();
        descendants.Add(descendantDictionary);
        parentDictionary.SetItem(COSName.GetPDFName("DescendantFonts"), descendants);
        PDType0Font parent = new(parentDictionary, descendant);

        Assert.NotNull(parent.GetCMap());
        Assert.NotNull(parent.GetCMapUCS2());
        Assert.Equal(34, parent.CodeToCID(65));
        Assert.Equal("A", parent.ToUnicode(65));
        Assert.Equal(1, parent.CodeToGID(65));
        Assert.True(parent.HasGlyph(65));
        Assert.NotEmpty(parent.GetPath(65).Segments);
        Assert.Equal(500, parent.GetWidthFromFont(65), 3);
        Assert.Equal("中", parent.ToUnicode(0x4E2D));
    }

    private sealed class SubstituteMapper(OpenTypeFont font) : FontMapper
    {
        public string? FindFontFile(string postScriptName) => null;
        public CIDFontMapping? GetCIDFont(string baseFont, PDFontDescriptor? fontDescriptor, PDCIDSystemInfo? cidSystemInfo) =>
            new(font, null, true);
    }
}
