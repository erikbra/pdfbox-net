/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Integration tests verifying that PDModel.Font surfaces are wired to real FontBox implementations.
 *
 * PORT_MODE: adapted
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

using PdfBox.Net.FontBox;
using PdfBox.Net.FontBox.TTF;
using PdfBox.Net.FontBox.Util;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.Util;
using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.Tests;

/// <summary>
/// Integration tests confirming that the PDModel.Font surfaces are backed by the real FontBox
/// types (FontBox.TTF.TrueTypeFont, FontBox.FontBoxFont, FontBox.Util.BoundingBox) rather than
/// the former hand-rolled stubs.
/// </summary>
public class FontStubsReplacementTest
{
    // ── Minimal concrete helpers ───────────────────────────────────────────────

    /// <summary>Minimal concrete FontBoxFont for testing.</summary>
    private sealed class TestFontBoxFont : FontBoxFont
    {
        private readonly string _name;

        public TestFontBoxFont(string name) => _name = name;

        public string GetName() => _name;
        public BoundingBox GetFontBBox() => new();
        public IList<float> GetFontMatrix() => [0.001f, 0f, 0f, 0.001f, 0f, 0f];
        public GeneralPath GetPath(string name) => new();
        public float GetWidth(string name) => 500f;
        public bool HasGlyph(string name) => true;
    }

    /// <summary>Minimal concrete PDTrueTypeFont for testing.</summary>
    private sealed class TestPDTrueTypeFont : PDTrueTypeFont
    {
        private readonly FontBoxFont _fbFont;

        public TestPDTrueTypeFont(FontBoxFont fbFont) => _fbFont = fbFont;

        public override FontBoxFont GetFontBoxFont() => _fbFont;
        public override bool IsStandard14() => false;
        public override bool HasGlyph(int code) => false;
        public override GeneralPath GetNormalizedPath(int code) => new();
    }

    /// <summary>Minimal concrete PDCIDFontType2 for testing.</summary>
    private sealed class TestPDCIDFontType2 : PDCIDFontType2
    {
        public override string GetName() => "TestCIDFont";
    }

    /// <summary>Minimal concrete PDSimpleFont for testing.</summary>
    private sealed class TestPDSimpleFont : PDSimpleFont
    {
        private readonly FontBoxFont _fbFont;

        public TestPDSimpleFont(FontBoxFont fbFont) => _fbFont = fbFont;

        public override FontBoxFont GetFontBoxFont() => _fbFont;
        public override bool IsStandard14() => false;
        public override bool HasGlyph(int code) => false;
        public override GeneralPath GetNormalizedPath(int code) => new();
    }

    // ── TrueTypeFont replacement ───────────────────────────────────────────────

    [Fact]
    public void PDTrueTypeFont_GetTrueTypeFont_ReturnsFontBoxTTFTrueTypeFont()
    {
        TrueTypeFont fbFont = new();
        var pdFont = new TestPDTrueTypeFont(fbFont);

        TrueTypeFont ttf = pdFont.GetTrueTypeFont();

        // Verify the returned type is the real FontBox TTF implementation
        Assert.IsType<TrueTypeFont>(ttf);
        Assert.Same(fbFont, ttf);
    }

    [Fact]
    public void PDTrueTypeFont_GetTrueTypeFont_DefaultReturnsUnitsPerEm1000()
    {
        var fbFont = new TestFontBoxFont("TestTTF");
        var pdFont = new TestPDTrueTypeFont(fbFont);

        TrueTypeFont ttf = pdFont.GetTrueTypeFont();

        // Default TrueTypeFont with no header table falls back to 1000
        Assert.Equal(1000, ttf.GetUnitsPerEm());
    }

    [Fact]
    public void PDTrueTypeFont_GetTrueTypeFont_ParsedFontHasCorrectUnitsPerEm()
    {
        byte[] bytes = FontBoxTestFixtures.CreateMinimalTrueType();
        TTFParser parser = new();
        TrueTypeFont ttf = parser.Parse(bytes);

        // A parsed font should surface the real UnitsPerEm from the head table
        Assert.Equal(1000, ttf.GetUnitsPerEm());
    }

    [Fact]
    public void PDCIDFontType2_GetTrueTypeFont_ReturnsFontBoxTTFTrueTypeFont()
    {
        var cidFont = new TestPDCIDFontType2();

        TrueTypeFont ttf = cidFont.GetTrueTypeFont();

        Assert.IsType<TrueTypeFont>(ttf);
        Assert.Equal(1000, ttf.GetUnitsPerEm());
    }

    // ── FontBoxFont replacement ────────────────────────────────────────────────

    [Fact]
    public void PDSimpleFont_GetFontBoxFont_ReturnsFontBoxFontBoxFontInterface()
    {
        var fbFont = new TestFontBoxFont("MyFont");
        var pdFont = new TestPDSimpleFont(fbFont);

        FontBoxFont result = pdFont.GetFontBoxFont();

        Assert.IsAssignableFrom<FontBoxFont>(result);
        Assert.Equal("MyFont", result.GetName());
    }

    [Fact]
    public void PDSimpleFont_GetFontBoxFont_GetName_ReturnsExpectedFontName()
    {
        var fbFont = new TestFontBoxFont("HelveticaNeue");
        var pdFont = new TestPDSimpleFont(fbFont);

        Assert.Equal("HelveticaNeue", pdFont.GetName());
    }

    // ── BoundingBox replacement ────────────────────────────────────────────────

    [Fact]
    public void PDFont_GetBoundingBox_ReturnsFontBoxUtilBoundingBox()
    {
        var fbFont = new TestFontBoxFont("TestFont");
        var pdFont = new TestPDSimpleFont(fbFont);

        BoundingBox bbox = pdFont.GetBoundingBox();

        Assert.IsAssignableFrom<BoundingBox>(bbox);
    }

    [Fact]
    public void PDSimpleFont_GetFontMatrix_UsesFontBoxValues()
    {
        var pdFont = new TestPDSimpleFont(new TestFontBoxFont("f"));

        Matrix fontMatrix = pdFont.GetFontMatrix();

        Assert.Equal(0.001f, fontMatrix.GetScaleX());
        Assert.Equal(0.001f, fontMatrix.GetScaleY());
    }

    [Fact]
    public void PDFont_GetBoundingBox_DefaultReturnsAllZeroBox()
    {
        var pdFont = new TestPDSimpleFont(new TestFontBoxFont("f"));

        BoundingBox bbox = pdFont.GetBoundingBox();

        Assert.Equal(0f, bbox.GetLowerLeftX());
        Assert.Equal(0f, bbox.GetLowerLeftY());
        Assert.Equal(0f, bbox.GetUpperRightX());
        Assert.Equal(0f, bbox.GetUpperRightY());
    }

    [Fact]
    public void BoundingBox_SettersAndGetters_RoundTrip()
    {
        var bbox = new BoundingBox(10f, 20f, 110f, 120f);

        Assert.Equal(10f, bbox.GetLowerLeftX());
        Assert.Equal(20f, bbox.GetLowerLeftY());
        Assert.Equal(110f, bbox.GetUpperRightX());
        Assert.Equal(120f, bbox.GetUpperRightY());
        Assert.Equal(100f, bbox.GetWidth());
        Assert.Equal(100f, bbox.GetHeight());
    }
}
