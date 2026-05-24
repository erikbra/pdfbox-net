/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Integration tests for the real GlyphList implementation backed by the Adobe Glyph List resource.
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

using PdfBox.Net.PDModel.Font.Encoding;

namespace PdfBox.Net.Tests;

/// <summary>
/// Integration tests verifying that the real GlyphList class maps glyph names to Unicode
/// strings correctly via the embedded Adobe Glyph List resource.
/// </summary>
public class GlyphListTest
{
    // ── Adobe Glyph List (AGL) lookups ────────────────────────────────────────

    [Fact]
    public void GetAdobeGlyphList_IsNotNull()
    {
        GlyphList agl = GlyphList.GetAdobeGlyphList();
        Assert.NotNull(agl);
    }

    [Theory]
    [InlineData("A", "A")]         // U+0041
    [InlineData("B", "B")]         // U+0042
    [InlineData("Z", "Z")]         // U+005A
    [InlineData("a", "a")]         // U+0061
    [InlineData("z", "z")]         // U+007A
    [InlineData("zero", "0")]      // U+0030
    [InlineData("one", "1")]       // U+0031
    [InlineData("nine", "9")]      // U+0039
    [InlineData("space", " ")]     // U+0020
    [InlineData("period", ".")]    // U+002E
    [InlineData("comma", ",")]     // U+002C
    [InlineData("hyphen", "-")]    // U+002D
    [InlineData("colon", ":")]     // U+003A
    [InlineData("semicolon", ";")] // U+003B
    [InlineData("slash", "/")]     // U+002F
    [InlineData("parenleft", "(")] // U+0028
    [InlineData("parenright",")")] // U+0029
    [InlineData("plus", "+")]      // U+002B
    [InlineData("equal", "=")]     // U+003D
    [InlineData("at", "@")]        // U+0040
    [InlineData("numbersign", "#")]// U+0023
    [InlineData("dollar", "$")]    // U+0024
    [InlineData("percent", "%")]   // U+0025
    [InlineData("ampersand", "&")] // U+0026
    [InlineData("quotedbl", "\"")] // U+0022
    [InlineData("quotesingle","'")] // U+0027
    [InlineData("asterisk", "*")]  // U+002A
    [InlineData("less", "<")]      // U+003C
    [InlineData("greater", ">")]   // U+003E
    [InlineData("question", "?")] // U+003F
    [InlineData("bracketleft", "[")] // U+005B
    [InlineData("bracketright","]")] // U+005D
    [InlineData("backslash", "\\")] // U+005C
    [InlineData("underscore", "_")] // U+005F
    public void ToUnicode_CommonGlyphNames_ReturnsExpectedCharacter(string glyphName, string expected)
    {
        GlyphList agl = GlyphList.GetAdobeGlyphList();
        Assert.Equal(expected, agl.ToUnicode(glyphName));
    }

    [Theory]
    [InlineData("Aacute", "\u00C1")]    // Latin capital A with acute
    [InlineData("aacute", "\u00E1")]    // Latin small a with acute
    [InlineData("Agrave", "\u00C0")]    // Latin capital A with grave
    [InlineData("agrave", "\u00E0")]    // Latin small a with grave
    [InlineData("Adieresis", "\u00C4")] // Latin capital A with diaeresis
    [InlineData("adieresis", "\u00E4")] // Latin small a with diaeresis
    [InlineData("AE", "\u00C6")]        // Latin capital AE ligature
    [InlineData("ae", "\u00E6")]        // Latin small ae ligature
    [InlineData("Ntilde", "\u00D1")]    // Latin capital N with tilde
    [InlineData("ntilde", "\u00F1")]    // Latin small n with tilde
    [InlineData("Oslash", "\u00D8")]    // Latin capital O with stroke
    [InlineData("oslash", "\u00F8")]    // Latin small o with stroke
    [InlineData("OE", "\u0152")]        // Latin capital OE ligature
    [InlineData("oe", "\u0153")]        // Latin small oe ligature
    [InlineData("germandbls", "\u00DF")] // German sharp S (ß)
    [InlineData("copyright", "\u00A9")] // Copyright sign
    [InlineData("registered", "\u00AE")] // Registered sign
    [InlineData("trademark", "\u2122")] // Trade mark sign
    [InlineData("bullet", "\u2022")]    // Bullet
    [InlineData("ellipsis", "\u2026")]  // Horizontal ellipsis
    [InlineData("endash", "\u2013")]    // En dash
    [InlineData("emdash", "\u2014")]    // Em dash
    [InlineData("quoteleft", "\u2018")] // Left single quotation mark
    [InlineData("quoteright", "\u2019")] // Right single quotation mark
    [InlineData("quotedblleft", "\u201C")] // Left double quotation mark
    [InlineData("quotedblright", "\u201D")] // Right double quotation mark
    public void ToUnicode_AccentedAndSpecialGlyphs_ReturnsExpectedCharacter(string glyphName, string expected)
    {
        GlyphList agl = GlyphList.GetAdobeGlyphList();
        Assert.Equal(expected, agl.ToUnicode(glyphName));
    }

    [Fact]
    public void ToUnicode_UnknownGlyphName_ReturnsNull()
    {
        GlyphList agl = GlyphList.GetAdobeGlyphList();
        Assert.Null(agl.ToUnicode("xyzUnknownGlyph12345"));
    }

    [Fact]
    public void ToUnicode_NullInput_ReturnsNull()
    {
        GlyphList agl = GlyphList.GetAdobeGlyphList();
        Assert.Null(agl.ToUnicode(null));
    }

    [Fact]
    public void ToUnicode_EmptyString_ReturnsNull()
    {
        GlyphList agl = GlyphList.GetAdobeGlyphList();
        Assert.Null(agl.ToUnicode(string.Empty));
    }

    // ── "uni" name convention ──────────────────────────────────────────────────

    [Theory]
    [InlineData("uni0041", "A")]       // A
    [InlineData("uni0061", "a")]       // a
    [InlineData("uni0030", "0")]       // zero
    [InlineData("uni00C0", "\u00C0")]  // À
    [InlineData("uni20AC", "\u20AC")]  // €
    [InlineData("uni00410042", "AB")]  // two codepoints
    public void ToUnicode_UniNameConvention_ReturnsDecodedCharacter(string glyphName, string expected)
    {
        GlyphList agl = GlyphList.GetAdobeGlyphList();
        Assert.Equal(expected, agl.ToUnicode(glyphName));
    }

    [Theory]
    [InlineData("u0041", "A")]       // A via short "u" convention
    [InlineData("u20AC", "\u20AC")]  // € via short "u" convention
    [InlineData("u1F600", "\U0001F600")] // Emoji (supplementary plane)
    public void ToUnicode_UNameConvention_ReturnsDecodedCharacter(string glyphName, string expected)
    {
        GlyphList agl = GlyphList.GetAdobeGlyphList();
        Assert.Equal(expected, agl.ToUnicode(glyphName));
    }

    // ── Constructor with extension stream ─────────────────────────────────────

    [Fact]
    public void Constructor_WithNullStream_CopiesBaseList()
    {
        GlyphList base_ = GlyphList.GetAdobeGlyphList();
        GlyphList extended = new(base_, null);

        // All base mappings should be preserved
        Assert.Equal(base_.ToUnicode("A"), extended.ToUnicode("A"));
        Assert.Equal(base_.ToUnicode("space"), extended.ToUnicode("space"));
    }

    [Fact]
    public void Constructor_WithStream_AddsCustomMappings()
    {
        const string customEntry = "mycustomglyph;1F600\n";
        using MemoryStream stream = new(System.Text.Encoding.ASCII.GetBytes(customEntry));
        GlyphList extended = new(GlyphList.GetAdobeGlyphList(), stream);

        // Custom glyph should resolve
        string? result = extended.ToUnicode("mycustomglyph");
        Assert.Equal("\U0001F600", result);

        // AGL glyphs still resolve
        Assert.Equal("A", extended.ToUnicode("A"));
    }

    [Fact]
    public void Constructor_WithStream_StreamCommentsAreIgnored()
    {
        const string customEntry = "# this is a comment\nmycustom2;0042\n# another comment\n";
        using MemoryStream stream = new(System.Text.Encoding.ASCII.GetBytes(customEntry));
        GlyphList extended = new(GlyphList.GetAdobeGlyphList(), stream);

        Assert.Equal("B", extended.ToUnicode("mycustom2"));
    }

    // ── Default (empty) constructor ────────────────────────────────────────────

    [Fact]
    public void DefaultConstructor_EmptyList_ReturnsNullForAnyGlyph()
    {
        GlyphList empty = new();
        Assert.Null(empty.ToUnicode("A"));
        Assert.Null(empty.ToUnicode("space"));
    }

    [Fact]
    public void DefaultConstructor_UniNameConvention_StillWorks()
    {
        // Even an empty GlyphList should resolve "uni" names
        GlyphList empty = new();
        Assert.Equal("A", empty.ToUnicode("uni0041"));
    }

    // ── Thread-safety: singleton is stable ────────────────────────────────────

    [Fact]
    public void GetAdobeGlyphList_ReturnsSameInstance()
    {
        GlyphList first = GlyphList.GetAdobeGlyphList();
        GlyphList second = GlyphList.GetAdobeGlyphList();
        Assert.Same(first, second);
    }
}
