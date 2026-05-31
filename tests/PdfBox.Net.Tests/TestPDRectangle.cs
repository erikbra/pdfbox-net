/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/pdmodel/common/PDRectangleTest.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted-minimal
 * PORT_LAST_SYNC_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
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
using PdfBox.Net.FontBox.Util;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.Util;
using PdfBox.Net.Util.Geometry;

namespace PdfBox.Net.Tests;

public class TestPDRectangle
{
    [Fact]
    public void DefaultConstructorIsZero()
    {
        PDRectangle rect = new();
        Assert.Equal(0f, rect.GetLowerLeftX());
        Assert.Equal(0f, rect.GetLowerLeftY());
        Assert.Equal(0f, rect.GetUpperRightX());
        Assert.Equal(0f, rect.GetUpperRightY());
        Assert.Equal(0f, rect.GetWidth());
        Assert.Equal(0f, rect.GetHeight());
    }

    [Fact]
    public void WidthHeightConstructor()
    {
        PDRectangle rect = new(200f, 300f);
        Assert.Equal(0f, rect.GetLowerLeftX());
        Assert.Equal(0f, rect.GetLowerLeftY());
        Assert.Equal(200f, rect.GetUpperRightX());
        Assert.Equal(300f, rect.GetUpperRightY());
        Assert.Equal(200f, rect.GetWidth());
        Assert.Equal(300f, rect.GetHeight());
    }

    [Fact]
    public void XYWidthHeightConstructor()
    {
        PDRectangle rect = new(10f, 20f, 100f, 200f);
        Assert.Equal(10f, rect.GetLowerLeftX());
        Assert.Equal(20f, rect.GetLowerLeftY());
        Assert.Equal(110f, rect.GetUpperRightX());
        Assert.Equal(220f, rect.GetUpperRightY());
        Assert.Equal(100f, rect.GetWidth());
        Assert.Equal(200f, rect.GetHeight());
    }

    [Fact]
    public void COSArrayConstructorNormalizesCoordinates()
    {
        // Feed array in upper-left/lower-right order to verify normalization
        COSArray arr = COSArray.Of(100f, 200f, 50f, 150f);
        PDRectangle rect = new(arr);
        Assert.Equal(50f, rect.GetLowerLeftX());
        Assert.Equal(150f, rect.GetLowerLeftY());
        Assert.Equal(100f, rect.GetUpperRightX());
        Assert.Equal(200f, rect.GetUpperRightY());
    }

    [Fact]
    public void ContainsPointInside()
    {
        PDRectangle rect = new(0f, 0f, 100f, 100f);
        Assert.True(rect.Contains(50f, 50f));
    }

    [Fact]
    public void ContainsPointOutside()
    {
        PDRectangle rect = new(0f, 0f, 100f, 100f);
        Assert.False(rect.Contains(150f, 50f));
        Assert.False(rect.Contains(50f, 150f));
    }

    [Fact]
    public void ContainsPointOnBoundary()
    {
        PDRectangle rect = new(0f, 0f, 100f, 100f);
        Assert.True(rect.Contains(0f, 0f));
        Assert.True(rect.Contains(100f, 100f));
    }

    [Fact]
    public void SettersUpdateValues()
    {
        PDRectangle rect = new();
        rect.SetLowerLeftX(5f);
        rect.SetLowerLeftY(10f);
        rect.SetUpperRightX(200f);
        rect.SetUpperRightY(300f);
        Assert.Equal(5f, rect.GetLowerLeftX());
        Assert.Equal(10f, rect.GetLowerLeftY());
        Assert.Equal(200f, rect.GetUpperRightX());
        Assert.Equal(300f, rect.GetUpperRightY());
        Assert.Equal(195f, rect.GetWidth());
        Assert.Equal(290f, rect.GetHeight());
    }

    [Fact]
    public void CreateRetranslatedRectangle()
    {
        PDRectangle rect = new(100f, 100f, 300f, 200f);
        PDRectangle translated = rect.CreateRetranslatedRectangle();
        Assert.Equal(0f, translated.GetLowerLeftX());
        Assert.Equal(0f, translated.GetLowerLeftY());
        Assert.Equal(rect.GetWidth(), translated.GetUpperRightX());
        Assert.Equal(rect.GetHeight(), translated.GetUpperRightY());
    }


    [Fact]
    public void ImmutableRectangleRejectsMutation()
    {
        PDImmutableRectangle rect = new(50f, 75f);

        Assert.Equal(50f, rect.GetWidth());
        Assert.Equal(75f, rect.GetHeight());
        Assert.Throws<NotSupportedException>(() => rect.SetUpperRightX(100f));
    }

    [Fact]
    public void LetterConstantImmutable()
    {
        Assert.Throws<NotSupportedException>(() => PDRectangle.LETTER.SetLowerLeftX(0f));
    }

    [Fact]
    public void StaticConstantsHaveCorrectDimensions()
    {
        // A4: 210mm x 297mm = approximately 595.28 x 841.89 pt
        Assert.Equal(595f, PDRectangle.A4.GetWidth(), 1f);
        Assert.Equal(842f, PDRectangle.A4.GetHeight(), 1f);

        // Letter: 8.5" x 11" = 612 x 792 pt
        Assert.Equal(612f, PDRectangle.LETTER.GetWidth(), 1f);
        Assert.Equal(792f, PDRectangle.LETTER.GetHeight(), 1f);
    }

    [Fact]
    public void GetCOSArrayReturnsArray()
    {
        PDRectangle rect = new(10f, 20f, 100f, 200f);
        COSArray arr = rect.GetCOSArray();
        Assert.NotNull(arr);
        Assert.Equal(4, arr.Size());
    }

    [Fact]
    public void BoundingBoxConstructorCopiesCoordinates()
    {
        BoundingBox box = new(10f, 20f, 30f, 40f);

        PDRectangle rect = new(box);

        Assert.Equal(10f, rect.GetLowerLeftX());
        Assert.Equal(20f, rect.GetLowerLeftY());
        Assert.Equal(30f, rect.GetUpperRightX());
        Assert.Equal(40f, rect.GetUpperRightY());
    }

    [Fact]
    public void ToGeneralPathBuildsClosedRectanglePath()
    {
        PDRectangle rect = new(10f, 20f, 30f, 40f);

        GeneralPath path = rect.ToGeneralPath();

        Assert.Collection(
            path.Segments,
            segment => Assert.Equal(new GeneralPath.Segment(GeneralPath.SegmentType.MoveTo, 10f, 20f, 0f, 0f), segment),
            segment => Assert.Equal(new GeneralPath.Segment(GeneralPath.SegmentType.LineTo, 40f, 20f, 0f, 0f), segment),
            segment => Assert.Equal(new GeneralPath.Segment(GeneralPath.SegmentType.LineTo, 40f, 60f, 0f, 0f), segment),
            segment => Assert.Equal(new GeneralPath.Segment(GeneralPath.SegmentType.LineTo, 10f, 60f, 0f, 0f), segment),
            segment => Assert.Equal(new GeneralPath.Segment(GeneralPath.SegmentType.Close, 0f, 0f, 0f, 0f), segment));
    }

    [Fact]
    public void TransformAppliesMatrixToCorners()
    {
        PDRectangle rect = new(10f, 20f, 30f, 40f);
        Matrix matrix = new(2f, 0f, 0f, 3f, 5f, 7f);

        GeneralPath path = rect.Transform(matrix);

        Assert.Collection(
            path.Segments,
            segment => Assert.Equal(new GeneralPath.Segment(GeneralPath.SegmentType.MoveTo, 25f, 67f, 0f, 0f), segment),
            segment => Assert.Equal(new GeneralPath.Segment(GeneralPath.SegmentType.LineTo, 85f, 67f, 0f, 0f), segment),
            segment => Assert.Equal(new GeneralPath.Segment(GeneralPath.SegmentType.LineTo, 85f, 187f, 0f, 0f), segment),
            segment => Assert.Equal(new GeneralPath.Segment(GeneralPath.SegmentType.LineTo, 25f, 187f, 0f, 0f), segment),
            segment => Assert.Equal(new GeneralPath.Segment(GeneralPath.SegmentType.Close, 0f, 0f, 0f, 0f), segment));
    }
}
