/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/rendering/TestRendering.java
 * PDFBOX_SOURCE_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: aba442860ed4f9f99f9e52e78e34bb23570c2390
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
using PdfBox.Net.PDModel;
using PdfBox.Net.Rendering;
using PdfBox.Net.Text;
using PdfBox.Net.Util;
using System.Text;

namespace PdfBox.Net.Tests;

/// <summary>
/// Tests for the rendering and text packages.
/// Covers enums, data classes, and API surface of the ported types.
/// </summary>
public class RenderingTextTest
{
    // ── ImageType ────────────────────────────────────────────────────────────

    [Fact]
    public void ImageType_AllValuesExist()
    {
        // All five enum values defined in the Java source must be present
        Assert.Equal(5, Enum.GetValues<ImageType>().Length);
        _ = ImageType.BINARY;
        _ = ImageType.GRAY;
        _ = ImageType.RGB;
        _ = ImageType.ARGB;
        _ = ImageType.BGR;
    }

    [Fact]
    public void ImageType_ToPixelFormat_MatchesJavaBufferedImageConstants()
    {
        // Java BufferedImage constants used in the upstream Java source
        Assert.Equal(12, ImageType.BINARY.ToPixelFormat()); // TYPE_BYTE_BINARY
        Assert.Equal(10, ImageType.GRAY.ToPixelFormat());   // TYPE_BYTE_GRAY
        Assert.Equal(1, ImageType.RGB.ToPixelFormat());     // TYPE_INT_RGB
        Assert.Equal(2, ImageType.ARGB.ToPixelFormat());    // TYPE_INT_ARGB
        Assert.Equal(5, ImageType.BGR.ToPixelFormat());     // TYPE_3BYTE_BGR
    }

    // ── RenderDestination ────────────────────────────────────────────────────

    [Fact]
    public void RenderDestination_AllValuesExist()
    {
        Assert.Equal(3, Enum.GetValues<RenderDestination>().Length);
        _ = RenderDestination.EXPORT;
        _ = RenderDestination.VIEW;
        _ = RenderDestination.PRINT;
    }

    // ── RenderingHints ───────────────────────────────────────────────────────

    [Fact]
    public void RenderingHints_DefaultConstructor_IsEmpty()
    {
        var hints = new RenderingHints();
        Assert.Empty(hints);
    }

    [Fact]
    public void RenderingHints_CanAddAndRetrieveEntries()
    {
        var hints = new RenderingHints();
        hints["key"] = "value";
        Assert.Equal("value", hints["key"]);
    }

    // ── Matrix ───────────────────────────────────────────────────────────────

    [Fact]
    public void Matrix_DefaultConstructor_IsIdentity()
    {
        var m = new Matrix();
        Assert.Equal(1f, m.GetScaleX());
        Assert.Equal(1f, m.GetScaleY());
        Assert.Equal(0f, m.GetShearX());
        Assert.Equal(0f, m.GetShearY());
        Assert.Equal(0f, m.GetTranslateX());
        Assert.Equal(0f, m.GetTranslateY());
    }

    [Fact]
    public void Matrix_ParameterizedConstructor_SetsElements()
    {
        var m = new Matrix(2f, 0f, 0f, 3f, 10f, 20f);
        Assert.Equal(2f, m.GetScaleX());
        Assert.Equal(3f, m.GetScaleY());
        Assert.Equal(10f, m.GetTranslateX());
        Assert.Equal(20f, m.GetTranslateY());
    }

    [Fact]
    public void Matrix_ScalingFactor_IdentityMatrix()
    {
        var m = new Matrix();
        Assert.Equal(1f, m.GetScalingFactorX());
        Assert.Equal(1f, m.GetScalingFactorY());
    }

    [Fact]
    public void Matrix_GetTranslateInstance_SetsTranslation()
    {
        var m = Matrix.GetTranslateInstance(5f, -3f);
        Assert.Equal(5f, m.GetTranslateX());
        Assert.Equal(-3f, m.GetTranslateY());
        Assert.Equal(1f, m.GetScaleX());
        Assert.Equal(1f, m.GetScaleY());
    }

    [Fact]
    public void Matrix_Transform_AppliesTranslation()
    {
        var m = Matrix.GetTranslateInstance(10f, 20f);
        var v = m.Transform(0f, 0f);
        Assert.Equal(10f, v.GetX());
        Assert.Equal(20f, v.GetY());
    }

    // ── TextPosition ─────────────────────────────────────────────────────────

    private static TextPosition CreateSampleTextPosition(
        string unicode = "A",
        float x = 10f, float y = 100f,
        int rotation = 0,
        float pageWidth = 612f, float pageHeight = 792f)
    {
        // Build a simple identity-like text matrix placed at (x, y)
        // For rotation=0: x comes from translateX, y from translateY
        var textMatrix = new Matrix(1f, 0f, 0f, 1f, x, pageHeight - y);
        return new TextPosition(
            pageRotation: rotation,
            pageWidth: pageWidth,
            pageHeight: pageHeight,
            textMatrix: textMatrix,
            endX: x + 10f,
            endY: pageHeight - y,
            maxHeight: 12f,
            individualWidth: 10f,
            spaceWidth: 4f,
            unicode: unicode,
            codes: [65],
            font: null!,
            fontSize: 12f,
            fontSizeInPt: 12);
    }

    [Fact]
    public void TextPosition_GetUnicode_ReturnsUnicode()
    {
        var tp = CreateSampleTextPosition("Hello");
        Assert.Equal("Hello", tp.GetUnicode());
    }

    [Fact]
    public void TextPosition_GetCharacterCodes_ReturnsCodes()
    {
        var tp = CreateSampleTextPosition();
        Assert.Equal([65], tp.GetCharacterCodes());
    }

    [Fact]
    public void TextPosition_GetRotation_ReturnsRotation()
    {
        var tp = CreateSampleTextPosition(rotation: 90);
        Assert.Equal(90, tp.GetRotation());
    }

    [Fact]
    public void TextPosition_GetWidth_SumOfIndividualWidths()
    {
        var tp = CreateSampleTextPosition();
        // individualWidth was set to 10f
        Assert.Equal(10f, tp.GetWidth());
    }

    [Fact]
    public void TextPosition_GetHeight_ReturnsMaxHeight()
    {
        var tp = CreateSampleTextPosition();
        Assert.Equal(12f, tp.GetHeight());
    }

    [Fact]
    public void TextPosition_GetWidthOfSpace_ReturnsSpaceWidth()
    {
        var tp = CreateSampleTextPosition();
        Assert.Equal(4f, tp.GetWidthOfSpace());
    }

    [Fact]
    public void TextPosition_GetFontSize_ReturnsFontSize()
    {
        var tp = CreateSampleTextPosition();
        Assert.Equal(12f, tp.GetFontSize());
    }

    [Fact]
    public void TextPosition_GetFontSizeInPt_ReturnsFontSizeInPt()
    {
        var tp = CreateSampleTextPosition();
        Assert.Equal(12, tp.GetFontSizeInPt());
    }

    [Fact]
    public void TextPosition_ToString_ReturnsUnicode()
    {
        var tp = CreateSampleTextPosition("X");
        Assert.Equal("X", tp.ToString());
    }

    [Fact]
    public void TextPosition_GetPageDimensions_ReturnConstructorValues()
    {
        var tp = CreateSampleTextPosition(pageWidth: 595f, pageHeight: 842f);
        Assert.Equal(595f, tp.GetPageWidth());
        Assert.Equal(842f, tp.GetPageHeight());
    }

    // ── TextPositionComparator ────────────────────────────────────────────────

    [Fact]
    public void TextPositionComparator_SameObject_ReturnsZero()
    {
        var tp = CreateSampleTextPosition();
        var cmp = new TextPositionComparator();
        Assert.Equal(0, cmp.Compare(tp, tp));
    }

    [Fact]
    public void TextPositionComparator_NullFirst_IsLessThanNonNull()
    {
        var tp = CreateSampleTextPosition();
        var cmp = new TextPositionComparator();
        Assert.True(cmp.Compare(null, tp) < 0);
        Assert.True(cmp.Compare(tp, null) > 0);
    }

    [Fact]
    public void TextPositionComparator_SameLine_SortsLeftToRight()
    {
        // Two positions on the same line; the one with smaller X comes first
        var tp1 = CreateSampleTextPosition("A", x: 10f, y: 100f);
        var tp2 = CreateSampleTextPosition("B", x: 50f, y: 100f);
        var cmp = new TextPositionComparator();
        Assert.True(cmp.Compare(tp1, tp2) < 0);
        Assert.True(cmp.Compare(tp2, tp1) > 0);
    }

    [Fact]
    public void TextPositionComparator_DifferentLines_SortsTopToBottom()
    {
        // pos1 is above pos2 (smaller Y in screen coords = higher up)
        var tp1 = CreateSampleTextPosition("A", x: 10f, y: 50f);
        var tp2 = CreateSampleTextPosition("B", x: 10f, y: 200f);
        var cmp = new TextPositionComparator();
        Assert.True(cmp.Compare(tp1, tp2) < 0);
    }

    // ── PDFTextStripper API surface ───────────────────────────────────────────

    [Fact]
    public void PDFTextStripper_DefaultWordSeparatorIsSpace()
    {
        var stripper = new PDFTextStripper();
        Assert.Equal(" ", stripper.GetWordSeparator());
    }

    [Fact]
    public void PDFTextStripper_SetWordSeparator_RoundTrips()
    {
        var stripper = new PDFTextStripper();
        stripper.SetWordSeparator("\t");
        Assert.Equal("\t", stripper.GetWordSeparator());
    }

    [Fact]
    public void PDFTextStripper_DefaultStartPage_IsOne()
    {
        var stripper = new PDFTextStripper();
        Assert.Equal(1, stripper.GetStartPage());
    }

    [Fact]
    public void PDFTextStripper_DefaultEndPage_IsMaxInt()
    {
        var stripper = new PDFTextStripper();
        Assert.Equal(int.MaxValue, stripper.GetEndPage());
    }

    [Fact]
    public void PDFTextStripper_SetStartPage_RoundTrips()
    {
        var stripper = new PDFTextStripper();
        stripper.SetStartPage(3);
        Assert.Equal(3, stripper.GetStartPage());
    }

    [Fact]
    public void PDFTextStripper_SetEndPage_RoundTrips()
    {
        var stripper = new PDFTextStripper();
        stripper.SetEndPage(10);
        Assert.Equal(10, stripper.GetEndPage());
    }

    [Fact]
    public void PDFTextStripper_SortByPosition_DefaultsFalse()
    {
        var stripper = new PDFTextStripper();
        Assert.False(stripper.IsSortByPosition());
    }

    [Fact]
    public void PDFTextStripper_SetSortByPosition_RoundTrips()
    {
        var stripper = new PDFTextStripper();
        stripper.SetSortByPosition(true);
        Assert.True(stripper.IsSortByPosition());
    }

    [Fact]
    public void PDFTextStripper_SuppressDuplicateOverlappingText_DefaultsTrue()
    {
        var stripper = new PDFTextStripper();
        Assert.True(stripper.IsSuppressDuplicateOverlappingText());
    }

    [Fact]
    public void PDFTextStripper_ShouldSeparateByBeads_DefaultsTrue()
    {
        var stripper = new PDFTextStripper();
        Assert.True(stripper.IsShouldSeparateByBeads());
    }

    [Fact]
    public void PDFTextStripper_SetLineSeparator_RoundTrips()
    {
        var stripper = new PDFTextStripper();
        stripper.SetLineSeparator("\r\n");
        Assert.Equal("\r\n", stripper.GetLineSeparator());
    }

    [Fact]
    public void PDFTextStripper_SetParagraphStart_RoundTrips()
    {
        var stripper = new PDFTextStripper();
        stripper.SetParagraphStart(">> ");
        Assert.Equal(">> ", stripper.GetParagraphStart());
    }

    [Fact]
    public void PDFTextStripper_SetParagraphEnd_RoundTrips()
    {
        var stripper = new PDFTextStripper();
        stripper.SetParagraphEnd(" <<");
        Assert.Equal(" <<", stripper.GetParagraphEnd());
    }

    // ── PDFTextStripperByArea API surface ─────────────────────────────────────

    [Fact]
    public void PDFTextStripperByArea_AddAndGetRegions()
    {
        var stripper = new PDFTextStripperByArea();
        var rect = new PdfBox.Net.Rendering.Rectangle2D(0, 0, 100, 50);
        stripper.AddRegion("r1", rect);
        var regions = stripper.GetRegions();
        Assert.Single(regions);
        Assert.Equal("r1", regions[0]);
    }

    [Fact]
    public void PDFTextStripperByArea_RemoveRegion_RemovesIt()
    {
        var stripper = new PDFTextStripperByArea();
        var rect = new PdfBox.Net.Rendering.Rectangle2D(0, 0, 100, 50);
        stripper.AddRegion("r1", rect);
        stripper.RemoveRegion("r1");
        Assert.Empty(stripper.GetRegions());
    }

    [Fact]
    public void PDFTextStripperByArea_RemoveNonExistentRegion_DoesNotThrow()
    {
        var stripper = new PDFTextStripperByArea();
        // Removing a region that was never added should not throw
        stripper.RemoveRegion("nonExistent");
        Assert.Empty(stripper.GetRegions());
    }

    [Fact]
    public void PDFTextStripperByArea_ShouldSeparateByBeads_IgnoredAlwaysFalse()
    {
        var stripper = new PDFTextStripperByArea();
        // The Java source override is a no-op; beads always disabled for by-area
        Assert.False(stripper.IsShouldSeparateByBeads());
        stripper.SetShouldSeparateByBeads(true); // no-op
        Assert.False(stripper.IsShouldSeparateByBeads());
    }

    // ── PDFMarkedContentExtractor API surface ─────────────────────────────────

    [Fact]
    public void PDFMarkedContentExtractor_SuppressDuplicateDefault_IsTrue()
    {
        var extractor = new PDFMarkedContentExtractor();
        Assert.True(extractor.IsSuppressDuplicateOverlappingText());
    }

    [Fact]
    public void PDFMarkedContentExtractor_SetSuppressDuplicate_RoundTrips()
    {
        var extractor = new PDFMarkedContentExtractor();
        extractor.SetSuppressDuplicateOverlappingText(false);
        Assert.False(extractor.IsSuppressDuplicateOverlappingText());
    }

    [Fact]
    public void PDFMarkedContentExtractor_GetMarkedContents_InitiallyEmpty()
    {
        var extractor = new PDFMarkedContentExtractor();
        Assert.Empty(extractor.GetMarkedContents());
    }

    // ── Functional baseline extraction tests ──────────────────────────────────

    [Fact]
    public void PDFTextStripper_GetText_ExtractsWordsAndLines()
    {
        using var document = CreateSimpleTextFixtureDocument("""
            BT
            /F1 12 Tf
            50 700 Td
            (AB CD) Tj
            0 -20 Td
            (EFGH) Tj
            ET
            """);

        var stripper = new PDFTextStripper();
        string extracted = stripper.GetText(document);

        Assert.Equal($"AB CD{Environment.NewLine}EFGH{Environment.NewLine}", extracted);
    }

    [Fact]
    public void PDFMarkedContentExtractor_ProcessPage_CapturesMarkedContentText()
    {
        using var document = CreateSimpleTextFixtureDocument("""
            /P BMC
            BT
            /F1 12 Tf
            50 700 Td
            (QWERTY) Tj
            ET
            EMC
            /Span BMC
            BT
            /F1 12 Tf
            50 680 Td
            (ASDFGH) Tj
            ET
            EMC
            """);

        var extractor = new PDFMarkedContentExtractor();
        extractor.ProcessPage(document.GetPage(0));
        var markedContents = extractor.GetMarkedContents();

        Assert.Equal(2, markedContents.Count);
        Assert.Equal("P", markedContents[0].Tag.GetName());
        Assert.Equal("Span", markedContents[1].Tag.GetName());
        Assert.Equal("QWERTY", string.Concat(markedContents[0].GetTexts().Select(tp => tp.GetUnicode())));
        Assert.Equal("ASDFGH", string.Concat(markedContents[1].GetTexts().Select(tp => tp.GetUnicode())));
    }

    [Fact]
    public void PDFTextStripperByArea_ExtractRegions_ExtractsTextByRegion()
    {
        using var document = CreateSimpleTextFixtureDocument("""
            BT
            /F1 12 Tf
            50 700 Td
            (QWERTY) Tj
            0 -60 Td
            (ZXCVBN) Tj
            ET
            """);

        var stripper = new PDFTextStripperByArea();
        stripper.AddRegion("top", new PdfBox.Net.Rendering.Rectangle2D(40, 80, 250, 40));
        stripper.AddRegion("bottom", new PdfBox.Net.Rendering.Rectangle2D(40, 140, 250, 40));

        stripper.ExtractRegions(document.GetPage(0));

        string top = stripper.GetTextForRegion("top");
        string bottom = stripper.GetTextForRegion("bottom");

        Assert.Contains("QWERTY", top);
        Assert.DoesNotContain("ZXCVBN", top);
        Assert.Contains("ZXCVBN", bottom);
        Assert.DoesNotContain("QWERTY", bottom);
    }

    private static PDDocument CreateSimpleTextFixtureDocument(string contentStream)
    {
        var document = new PDDocument();
        var page = new PDPage();
        document.AddPage(page);

        var pageDict = (COSDictionary)page.GetCOSObject();
        pageDict.SetItem(COSName.RESOURCES, CreateDefaultResourcesDictionary());

        var stream = new COSStream();
        using (Stream output = stream.CreateOutputStream())
        {
            byte[] bytes = Encoding.Latin1.GetBytes(contentStream);
            output.Write(bytes, 0, bytes.Length);
        }

        pageDict.SetItem(COSName.CONTENTS, stream);
        return document;
    }

    private static COSDictionary CreateDefaultResourcesDictionary()
    {
        var fontDictionary = new COSDictionary();
        fontDictionary.SetItem(COSName.TYPE, COSName.GetPDFName("Font"));
        fontDictionary.SetItem(COSName.GetPDFName("Subtype"), COSName.GetPDFName("Type1"));
        fontDictionary.SetItem(COSName.GetPDFName("BaseFont"), COSName.GetPDFName("Helvetica"));

        var fonts = new COSDictionary();
        fonts.SetItem(COSName.GetPDFName("F1"), fontDictionary);

        var resources = new COSDictionary();
        resources.SetItem(COSName.GetPDFName("Font"), fonts);
        return resources;
    }
}
