/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 *
 * Added fixture-driven coverage for full PDF document loading pipeline
 * (xref table, filtered streams, and xref stream variants).
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

using PdfBox.Net.PDModel;

namespace PdfBox.Net.Tests;

public class FullPdfDocumentLoadingTest
{
    [Theory]
    [InlineData("classic-xref-fixture.pdf", 1, "Classic Fixture", "pdfbox-net")]
    [InlineData("flate-content-fixture.pdf", 1, "Classic Fixture", "pdfbox-net")]
    [InlineData("xref-stream-fixture.pdf", 1, "XRef Stream Fixture", "pdfbox-net")]
    public void FullLoaderReadsRealPdfFixtures(string fixtureName, int expectedPages, string expectedTitle, string expectedAuthor)
    {
        string fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", fixtureName);

        using PDDocument document = PDDocument.Load(fixturePath);

        Assert.Equal(expectedPages, document.GetNumberOfPages());
        Assert.Equal(expectedTitle, document.GetDocumentInformation().GetTitle());
        Assert.Equal(expectedAuthor, document.GetDocumentInformation().GetAuthor());
        Assert.Equal(expectedPages, document.GetDocumentCatalog().GetPages().GetCount());
    }
}
