/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/pdmodel/font/IdentityCJKFixture.java
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
using PdfBox.Net.COS;
using PdfBox.Net.FontBox.CFF;
using PdfBox.Net.FontBox.TTF;
using PdfBox.Net.PDModel.Font;

namespace PdfBox.Net.Tests;

/// <summary>
/// Installs an Adobe-Identity-0 CID-keyed font as substitute for every non-embedded CID font,
/// standing in for an installed Noto CJK / Source Han font, and restores the previous mapper on
/// disposal. FontMappers is process-wide; test classes using this must use the isolated collection.
/// The SIL OFL 1.1 substitute retains space and U+4E2D with original GIDs; other glyphs are empty.
/// </summary>
internal sealed class IdentityCJKFixture : IDisposable
{
    internal const int ZHONG = 0x4E2D;
    internal const int ZHONG_GID = 9544;
    private readonly FontMapper _original;
    internal OpenTypeFont Substitute { get; }

    internal IdentityCJKFixture()
    {
        Substitute = (OpenTypeFont)new OTFParser().Parse(File.ReadAllBytes(Path.Combine(
            AppContext.BaseDirectory, "Fixtures", "Fonts", "NotoSansCJKsc-Regular-subset.otf")));
        if (((CFFCIDFont)Substitute.GetCFF().GetFont()!).Ordering != "Identity")
        {
            throw new InvalidOperationException("fixture is not Adobe-Identity-0");
        }
        _original = FontMappers.Instance;
        FontMappers.Instance = new FixtureMapper(_original, Substitute);
    }

    public void Dispose()
    {
        FontMappers.Instance = _original;
        Substitute.Dispose();
    }

    /// <summary>A non-embedded Adobe-GB1 Type0 font dictionary.</summary>
    internal static COSDictionary Type0(string baseFont, string subtype, string encoding)
    {
        COSDictionary ros = new();
        ros.SetString(COSName.GetPDFName("Registry"), "Adobe");
        ros.SetString(COSName.GetPDFName("Ordering"), "GB1");
        ros.SetInt(COSName.GetPDFName("Supplement"), 2);
        COSDictionary descriptor = new();
        descriptor.SetName(COSName.TYPE, "FontDescriptor");
        descriptor.SetName(COSName.GetPDFName("FontName"), baseFont);
        descriptor.SetInt(COSName.GetPDFName("Flags"), 6);
        COSArray bbox = new();
        foreach (int value in new[] { 0, -141, 996, 855 }) bbox.Add(COSInteger.Get(value));
        descriptor.SetItem(COSName.GetPDFName("FontBBox"), bbox);
        descriptor.SetInt(COSName.GetPDFName("ItalicAngle"), 0);
        descriptor.SetInt(COSName.GetPDFName("Ascent"), 859);
        descriptor.SetInt(COSName.GetPDFName("Descent"), -141);
        descriptor.SetInt(COSName.GetPDFName("CapHeight"), 859);
        descriptor.SetInt(COSName.GetPDFName("StemV"), 80);
        COSDictionary cid = new();
        cid.SetName(COSName.TYPE, "Font");
        cid.SetName(COSName.SUBTYPE, subtype);
        cid.SetName(COSName.GetPDFName("BaseFont"), baseFont);
        cid.SetItem(COSName.GetPDFName("CIDSystemInfo"), ros);
        cid.SetItem(COSName.GetPDFName("FontDescriptor"), descriptor);
        cid.SetInt(COSName.GetPDFName("DW"), 1000);
        COSArray descendants = new();
        descendants.Add(cid);
        COSDictionary type0 = new();
        type0.SetName(COSName.TYPE, "Font");
        type0.SetName(COSName.SUBTYPE, "Type0");
        type0.SetName(COSName.GetPDFName("BaseFont"), baseFont);
        type0.SetName(COSName.GetPDFName("Encoding"), encoding);
        type0.SetItem(COSName.GetPDFName("DescendantFonts"), descendants);
        return type0;
    }

    private sealed class FixtureMapper(FontMapper original, OpenTypeFont substitute) : FontMapper
    {
        public string? FindFontFile(string postScriptName) => original.FindFontFile(postScriptName);
        public CIDFontMapping? GetCIDFont(string baseFont, PDFontDescriptor? fontDescriptor, PDCIDSystemInfo? cidSystemInfo) =>
            new(substitute, null, true);
    }
}

[CollectionDefinition("Identity CJK substitute", DisableParallelization = true)]
public sealed class IdentityCJKCollection { }
