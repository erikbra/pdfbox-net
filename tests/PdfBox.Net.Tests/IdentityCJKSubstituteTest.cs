/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/pdmodel/font/IdentityCJKSubstituteTest.java
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
using System.Buffers.Binary;
using PdfBox.Net.COS;
using PdfBox.Net.FontBox.CFF;
using PdfBox.Net.FontBox.TTF;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.Tests;

/// <summary>
/// Non-embedded Adobe-GB1 CID fonts substituted by an Adobe-Identity-0 CID-keyed font (Noto CJK,
/// Source Han). The PDF's CIDs are not glyph indices in such a substitute, so glyphs must be
/// resolved via Unicode: PDFBOX-6249 for CIDFontType0, PDFBOX-6293 for CIDFontType2.
/// </summary>
[Collection("Identity CJK substitute")]
public sealed class IdentityCJKSubstituteTest
{
    [Theory]
    [InlineData("SimSun", "CIDFontType2")]
    [InlineData("AdobeSongStd-Light", "CIDFontType0")]
    public void ResolvesGlyphViaUnicode(string baseFont, string subtype)
    {
        using IdentityCJKFixture fixture = new();
        PDType0Font font = (PDType0Font)PDFontFactory.CreateFont(
            IdentityCJKFixture.Type0(baseFont, subtype, "UniGB-UCS2-H"));
        Assert.Equal(IdentityCJKFixture.ZHONG_GID, font.CodeToGID(IdentityCJKFixture.ZHONG));
        Assert.True(font.HasGlyph(IdentityCJKFixture.ZHONG));
        GeneralPath path = font.GetPath(IdentityCJKFixture.ZHONG);
        Assert.NotEmpty(path.Segments);
        GeneralPath expected = ((CFFCIDFont)fixture.Substitute.GetCFF().GetFont()!)
            .GetType2CharString(IdentityCJKFixture.ZHONG_GID).GetPath();
        Assert.Equal(Bounds(expected), Bounds(path));
        Assert.True(Bounds(path).MaxX > Bounds(path).MinX);
    }
    [Fact]
    public void CffOutlinesScaleByUnitsPerEmWithoutMutatingCachedPath()
    {
        byte[] bytes = FontBoxTestFixtures.CreateMinimalOpenTypeCff(cidKeyed: true);
        // The first table-directory entry is head; unitsPerEm is at head + 18.
        int headOffset = (int)BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(20, 4));
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(headOffset + 18, 2), 2000);
        using OpenTypeFont fontProgram = (OpenTypeFont)new OTFParser().Parse(bytes);
        Assert.Equal(2000, fontProgram.GetUnitsPerEm());
        GeneralPath raw = fontProgram.GetCFF().GetFont()!.GetType2CharString(1).GetPath();
        Assert.Equal(100, Bounds(raw).MaxX);
        PDCIDFontType2 descendant = new(new COSDictionary(), fontProgram);
        COSDictionary dictionary = new();
        dictionary.SetName(COSName.GetPDFName("Encoding"), "Identity-H");
        PDType0Font font = new(dictionary, descendant);
        Assert.True(font.HasGlyph(1));
        Assert.True(descendant.HasGlyph(1));
        Assert.Equal(50, Bounds(font.GetNormalizedPath(1)).MaxX);
        Assert.Equal(50, Bounds(descendant.GetNormalizedPath(1)).MaxX);
        Assert.Equal(100, Bounds(raw).MaxX);
    }

    // java.awt.geom.Path2D bounds include the control points. GeneralPath's port exposes
    // segments directly; compute those same bounds without requiring a rendering backend.
    private static (float MinX, float MinY, float MaxX, float MaxY) Bounds(GeneralPath path)
    {
        List<(float X, float Y)> points = new();
        foreach (GeneralPath.Segment segment in path.Segments)
        {
            if (segment.Type == GeneralPath.SegmentType.Close) continue;
            points.Add((segment.X1, segment.Y1));
            if (segment.Type is GeneralPath.SegmentType.QuadTo or GeneralPath.SegmentType.CurveTo)
                points.Add((segment.X2, segment.Y2));
            if (segment.Type == GeneralPath.SegmentType.CurveTo)
                points.Add((segment.X3, segment.Y3));
        }
        return (points.Min(p => p.X), points.Min(p => p.Y), points.Max(p => p.X), points.Max(p => p.Y));
    }
}
