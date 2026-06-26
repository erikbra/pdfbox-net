/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/test/java/org/apache/fontbox/afm/FontMetricsTest.java
 * PDFBOX_SOURCE_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
 * PORT_MODE: mechanical
 * PORT_LAST_SYNC_COMMIT: eeb5d611e0cea8beac3d7025a4dbccbef51d5caf
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

using PdfBox.Net.FontBox.AFM;
using PdfBox.Net.FontBox.Util;

namespace PdfBox.Net.FontBox.Tests;

public class AFMFontMetricsTest
{
    [Fact]
    public void TestFontMetricsNames()
    {
        FontMetrics fontMetrics = new()
        {
            FontName = "fontName",
            FamilyName = "familyName",
            FullName = "fullName",
            FontVersion = "fontVersion",
            Notice = "notice",
        };

        Assert.Equal("fontName", fontMetrics.FontName);
        Assert.Equal("familyName", fontMetrics.FamilyName);
        Assert.Equal("fullName", fontMetrics.FullName);
        Assert.Equal("fontVersion", fontMetrics.FontVersion);
        Assert.Equal("notice", fontMetrics.Notice);

        Assert.Empty(fontMetrics.Comments);
        fontMetrics.AddComment("comment");
        Assert.Single(fontMetrics.Comments);
    }

    [Fact]
    public void TestFontMetricsSimpleValues()
    {
        FontMetrics fontMetrics = new()
        {
            AfmVersion = 4.3f,
            Weight = "weight",
            EncodingScheme = "encodingScheme",
            MappingScheme = 0,
            EscChar = 0,
            CharacterSet = "characterSet",
            Characters = 10,
            IsBaseFont = true,
            IsFixedV = true,
            CapHeight = 10f,
            XHeight = 20f,
            Ascender = 30f,
            Descender = 40f,
            StdHW = 50f,
            StdVW = 60f,
            UnderlinePosition = 70f,
            UnderlineThickness = 80f,
            ItalicAngle = 90f,
            IsFixedPitch = true,
        };

        Assert.Equal(4.3f, fontMetrics.AfmVersion);
        Assert.Equal("weight", fontMetrics.Weight);
        Assert.Equal("encodingScheme", fontMetrics.EncodingScheme);
        Assert.Equal(0, fontMetrics.MappingScheme);
        Assert.Equal(0, fontMetrics.EscChar);
        Assert.Equal("characterSet", fontMetrics.CharacterSet);
        Assert.Equal(10, fontMetrics.Characters);
        Assert.True(fontMetrics.IsBaseFont);
        Assert.True(fontMetrics.IsFixedV);
        Assert.Equal(10f, fontMetrics.CapHeight);
        Assert.Equal(20f, fontMetrics.XHeight);
        Assert.Equal(30f, fontMetrics.Ascender);
        Assert.Equal(40f, fontMetrics.Descender);
        Assert.Equal(50f, fontMetrics.StdHW);
        Assert.Equal(60f, fontMetrics.StdVW);
        Assert.Equal(70f, fontMetrics.UnderlinePosition);
        Assert.Equal(80f, fontMetrics.UnderlineThickness);
        Assert.Equal(90f, fontMetrics.ItalicAngle);
        Assert.True(fontMetrics.IsFixedPitch);

        fontMetrics.StandardHorizontalWidth = 51f;
        fontMetrics.StandardVerticalWidth = 61f;
        fontMetrics.FixedPitch = false;

        Assert.Equal(51f, fontMetrics.StdHW);
        Assert.Equal(61f, fontMetrics.StdVW);
        Assert.False(fontMetrics.IsFixedPitch);
    }

    [Fact]
    public void TestFontMetricsComplexValues()
    {
        FontMetrics fontMetrics = new()
        {
            FontBBox = new BoundingBox(10, 20, 30, 40),
            VVector = [10f, 20f],
            CharWidth = [30f, 40f],
        };

        Assert.Equal(10, fontMetrics.FontBBox!.GetLowerLeftX());
        Assert.Equal(20, fontMetrics.FontBBox.GetLowerLeftY());
        Assert.Equal(30, fontMetrics.FontBBox.GetUpperRightX());
        Assert.Equal(40, fontMetrics.FontBBox.GetUpperRightY());
        Assert.Equal(10f, fontMetrics.VVector![0]);
        Assert.Equal(20f, fontMetrics.VVector[1]);
        Assert.Equal(30f, fontMetrics.CharWidth[0]);
        Assert.Equal(40f, fontMetrics.CharWidth[1]);
    }

    [Fact]
    public void TestMetricSets()
    {
        FontMetrics fontMetrics = new();
        fontMetrics.MetricSets = 1;
        Assert.Equal(1, fontMetrics.MetricSets);

        // any value < 0 should throw an ArgumentException
        Assert.Throws<ArgumentException>(() => fontMetrics.MetricSets = -1);

        // any value > 2 should throw an ArgumentException
        Assert.Throws<ArgumentException>(() => fontMetrics.MetricSets = 3);
    }

    [Fact]
    public void TestCharMetrics()
    {
        FontMetrics fontMetrics = new();
        Assert.Empty(fontMetrics.CharMetrics);

        CharMetric charMetric = new();
        fontMetrics.AddCharMetric(charMetric);
        Assert.Single(fontMetrics.CharMetrics);
    }

    [Fact]
    public void TestComposites()
    {
        FontMetrics fontMetrics = new();
        Assert.Empty(fontMetrics.Composites);

        Composite composite = new() { Name = "name" };
        fontMetrics.AddComposite(composite);
        Assert.Single(fontMetrics.Composites);
    }

    [Fact]
    public void TestKernAliases()
    {
        FontMetrics fontMetrics = new();

        TrackKern trackKern = new();
        fontMetrics.AddTrackKern(trackKern);
        Assert.Same(trackKern, Assert.Single(fontMetrics.GetTrackKern()));

        KernPair kernPair = new();
        fontMetrics.AddKernPair(kernPair);
        Assert.Same(kernPair, Assert.Single(fontMetrics.KernPairs));

        KernPair kernPair0 = new();
        fontMetrics.AddKernPair0(kernPair0);
        Assert.Same(kernPair0, Assert.Single(fontMetrics.KernPairs0));

        KernPair kernPair1 = new();
        fontMetrics.AddKernPair1(kernPair1);
        Assert.Same(kernPair1, Assert.Single(fontMetrics.KernPairs1));
    }

    [Fact]
    public void TestKernData()
    {
        FontMetrics fontMetrics = new();

        // KernPairs
        Assert.Empty(fontMetrics.KernPairs);
        KernPair kernPair = new() { FirstGlyph = "first", SecondGlyph = "second", DeltaX = 10f, DeltaY = 20f };
        fontMetrics.KernPairs.Add(kernPair);
        Assert.Single(fontMetrics.KernPairs);

        // KernPairs0
        Assert.Empty(fontMetrics.KernPairs0);
        fontMetrics.KernPairs0.Add(kernPair);
        Assert.Single(fontMetrics.KernPairs0);

        // KernPairs1
        Assert.Empty(fontMetrics.KernPairs1);
        fontMetrics.KernPairs1.Add(kernPair);
        Assert.Single(fontMetrics.KernPairs1);

        // TrackKerns
        Assert.Empty(fontMetrics.TrackKerns);
        TrackKern trackKern = new() { Degree = 0, MinPtSize = 1f, MinKern = 1f, MaxPtSize = 10f, MaxKern = 10f };
        fontMetrics.TrackKerns.Add(trackKern);
        Assert.Single(fontMetrics.TrackKerns);
    }

    [Fact]
    public void TestCharMetricDimensions()
    {
        FontMetrics fontMetrics = new();
        Assert.Equal(0f, fontMetrics.GetAverageCharacterWidth());

        fontMetrics.CharMetrics.Add(new() { Name = "ten",    Wx = 10f, Wy = 20f });
        fontMetrics.CharMetrics.Add(new() { Name = "twenty", Wx = 20f, Wy = 40f });
        fontMetrics.CharMetrics.Add(new() { Name = "thirty", Wx = 30f, Wy = 60f });
        fontMetrics.CharMetrics.Add(new() { Name = "forty",  Wx = 40f, Wy = 80f });

        Assert.Equal(10f, fontMetrics.GetCharacterWidth("ten"));
        Assert.Equal(30f, fontMetrics.GetCharacterWidth("thirty"));
        Assert.Equal(0f,  fontMetrics.GetCharacterWidth("unknown"));

        Assert.Equal(40f, fontMetrics.GetCharacterHeight("twenty"));
        Assert.Equal(80f, fontMetrics.GetCharacterHeight("forty"));
        Assert.Equal(0f,  fontMetrics.GetCharacterHeight("unknown"));

        Assert.Equal(25f, fontMetrics.GetAverageCharacterWidth());
    }
}
