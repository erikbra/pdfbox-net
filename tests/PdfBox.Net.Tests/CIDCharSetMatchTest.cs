/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/pdmodel/font/CIDCharSetMatchTest.java
 * PDFBOX_SOURCE_COMMIT: 046747da99a870902217efabf1c41297de157059
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 046747da99a870902217efabf1c41297de157059
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
using PdfBox.Net.COS;
using PdfBox.Net.FontBox;
using PdfBox.Net.PDModel.Font;

namespace PdfBox.Net.Tests;

public class CIDCharSetMatchTest
{
    [Theory]
    [InlineData("CNS1", 0, true)]
    [InlineData("Japan1", 1 << 20, false)]
    [InlineData("Identity", 1 << 20, true)]
    [InlineData("Identity", 1 << 18, false)]
    [InlineData(null, 1 << 20, true)]
    [InlineData(null, 0, false)]
    public void TestCharSetMatch(string? candidateOrdering, int codePages, bool expected)
    {
        FontMapperImpl mapper = new(new TestProvider());
        TestInfo info = new("TestFont", candidateOrdering, codePages);

        Assert.Equal(expected, mapper.IsCharSetMatch(SystemInfo("CNS1"), info));
    }

    [Theory]
    [InlineData("NotoSansCJKsc-Regular", "Japan1", false)]
    [InlineData("NotoSansCJKtc-Regular", "Japan1", false)]
    [InlineData("NotoSansHK-Regular", "Japan1", false)]
    [InlineData("NotoSansJP-Regular", "CNS1", false)]
    [InlineData("NotoSansCJKjp-Regular", "GB1", false)]
    [InlineData("NotoSansKR-Regular", "GB1", false)]
    [InlineData("NotoSansKR-Regular", "Japan1", false)]
    [InlineData("NotoSansKR-Regular", "Korea1", true)]
    [InlineData("NotoSansTC-Regular", "CNS1", true)]
    [InlineData("MalgunGothic-Semilight", "CNS1", false)]
    [InlineData("MalgunGothic-Semilight", "Korea1", true)]
    public void MisleadingCodePageBitsAreMasked(string name, string requestedOrdering, bool expected)
    {
        const int cjkCodePages = (1 << 17) | (1 << 18) | (1 << 19) | (1 << 20) | (1 << 21);
        FontMapperImpl mapper = new(new TestProvider());

        Assert.Equal(expected, mapper.IsCharSetMatch(SystemInfo(requestedOrdering), new TestInfo(name, "Identity", cjkCodePages)));
    }

    [Fact]
    public void MissingDescriptorWeightPrefersRegularCandidate()
    {
        TestInfo bold = new("Bold", "Identity", 1 << 20, weight: 700);
        TestInfo regular = new("Regular", "Identity", 1 << 20, weight: 400);
        FontMapperImpl mapper = new(new TestProvider(bold, regular));

        Assert.Same(regular, mapper.GetFontMatches(new PDFontDescriptor(), SystemInfo("CNS1"))[0]);
    }

    [Fact]
    public void ExplicitDescriptorWeightSelectsMatchingCandidate()
    {
        TestInfo regular = new("Regular", "Identity", 1 << 20, weight: 400);
        TestInfo bold = new("Bold", "Identity", 1 << 20, weight: 700);
        FontMapperImpl mapper = new(new TestProvider(regular, bold));
        PDFontDescriptor descriptor = new();
        descriptor.GetCOSObject().SetFloat(COSName.GetPDFName("FontWeight"), 700);

        Assert.Same(bold, mapper.GetFontMatches(descriptor, SystemInfo("CNS1"))[0]);
    }

    [Fact]
    public void DroidSansFallbackIsOnlyRankedWhenExplicitlyRequested()
    {
        TestInfo droid = new("DroidSansFallback", "Identity", 1 << 20, weight: 400);
        TestInfo regular = new("Regular", "Identity", 1 << 20, weight: 400);
        FontMapperImpl mapper = new(new TestProvider(droid, regular));
        PDFontDescriptor descriptor = new();
        Assert.DoesNotContain(droid, mapper.GetFontMatches(descriptor, SystemInfo("CNS1")));

        descriptor.GetCOSObject().SetName(COSName.GetPDFName("FontName"), "DroidSansFallback");
        Assert.Contains(droid, mapper.GetFontMatches(descriptor, SystemInfo("CNS1")));
    }

    [Fact]
    public void BarcodePanoseCandidateIsNotChosenForOrdinaryText()
    {
        TestInfo barcode = new("Code128", "Identity", 1 << 20, panose: new byte[10]);
        FontMapperImpl mapper = new(new TestProvider(barcode));
        PDFontDescriptor descriptor = new();
        COSDictionary style = new();
        style.SetItem(COSName.GetPDFName("Panose"), new COSString(new byte[12]));
        descriptor.GetCOSObject().SetItem(COSName.GetPDFName("Style"), style);
        Assert.Empty(mapper.GetFontMatches(descriptor, SystemInfo("CNS1")));

        descriptor.GetCOSObject().SetName(COSName.GetPDFName("FontName"), "Code128");
        Assert.Single(mapper.GetFontMatches(descriptor, SystemInfo("CNS1")));
    }

    private static PDCIDSystemInfo SystemInfo(string ordering)
    {
        COSDictionary dictionary = new();
        dictionary.SetString(COSName.GetPDFName("Registry"), "Adobe");
        dictionary.SetString(COSName.GetPDFName("Ordering"), ordering);
        return new PDCIDSystemInfo(dictionary);
    }

    private sealed class TestProvider(params FontInfo[] fonts) : FontProvider
    {
        public override IReadOnlyList<FontInfo> GetFontInfo() => fonts;
        public override string ToDebugString() => "test font metadata";
    }

    private sealed class TestInfo(string name, string? ordering, int codePages, int weight = 0, byte[]? panose = null) : FontInfo
    {
        public override string GetPostScriptName() => name;
        public override FontFormat GetFormat() => FontFormat.OTF;
        public override PDCIDSystemInfo? GetCIDSystemInfo() => ordering is null ? null : SystemInfo(ordering);
        public override FontBoxFont GetFont() => throw new InvalidOperationException("Candidate filtering must not load font outlines.");
        public override int GetFamilyClass() => 0;
        public override int GetWeightClass() => weight;
        public override int GetCodePageRange1() => codePages;
        public override int GetCodePageRange2() => 0;
        public override int GetMacStyle() => 0;
        public override PDPanoseClassification? GetPanose() => panose is null ? null : new PDPanoseClassification(panose);
    }
}
