/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/test/java/org/apache/fontbox/afm/CharMetricTest.java
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

namespace PdfBox.Net.Tests;

public class AFMCharMetricTest
{
    [Fact]
    public void TestCharMetricSimpleValues()
    {
        CharMetric charMetric = new()
        {
            CharacterCode = 0,
            Name = "name",
            Wx = 10f,
            W0x = 20f,
            W1x = 30f,
            Wy = 40f,
            W0y = 50f,
            W1y = 60f,
        };

        Assert.Equal(0, charMetric.CharacterCode);
        Assert.Equal("name", charMetric.Name);
        Assert.Equal(10f, charMetric.Wx);
        Assert.Equal(20f, charMetric.W0x);
        Assert.Equal(30f, charMetric.W1x);
        Assert.Equal(40f, charMetric.Wy);
        Assert.Equal(50f, charMetric.W0y);
        Assert.Equal(60f, charMetric.W1y);
    }

    [Fact]
    public void TestCharMetricComplexValues()
    {
        CharMetric charMetric = new()
        {
            BoundingBox = new BoundingBox(10, 20, 30, 40),
        };

        Assert.Equal(10, charMetric.BoundingBox!.GetLowerLeftX());
        Assert.Equal(20, charMetric.BoundingBox.GetLowerLeftY());
        Assert.Equal(30, charMetric.BoundingBox.GetUpperRightX());
        Assert.Equal(40, charMetric.BoundingBox.GetUpperRightY());

        Assert.Empty(charMetric.Ligatures);
        Ligature ligature = new() { Successor = "successor", LigatureValue = "ligature" };
        charMetric.Ligatures.Add(ligature);
        List<Ligature> ligatures = charMetric.Ligatures;
        Assert.Single(ligatures);
        Assert.Equal("successor", ligatures[0].Successor);

        // Ligatures list is mutable by design in C# (direct List<T> property)
    }
}
