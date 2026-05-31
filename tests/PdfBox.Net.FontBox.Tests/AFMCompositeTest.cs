/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: fontbox/src/test/java/org/apache/fontbox/afm/CompositeTest.java
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

namespace PdfBox.Net.FontBox.Tests;

public class AFMCompositeTest
{
    [Fact]
    public void TestComposite()
    {
        Composite composite = new() { Name = "name" };
        Assert.Equal("name", composite.Name);
        Assert.Empty(composite.Parts);

        CompositePart compositePart = new() { Name = "name", DisplacementX = 10, DisplacementY = 20 };
        composite.Parts.Add(compositePart);
        List<CompositePart> parts = composite.Parts;
        Assert.Single(parts);
        Assert.Equal("name", parts[0].Name);

        // C# List<T> is mutable by design; adding further items is allowed
        parts.Add(compositePart);
        Assert.Equal(2, parts.Count);
    }
}
