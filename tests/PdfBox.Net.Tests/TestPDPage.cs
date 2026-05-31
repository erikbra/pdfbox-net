/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Mechanically converted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/pdmodel/TestPDPage.java
 * PDFBOX_SOURCE_COMMIT: ccd281cfecedcc0ad39709bece5e67b19a54e8db
 * PORT_MODE: adapted
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

using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Interactive.Form;
using PdfBox.Net.PDModel.Interactive.Annotation;

namespace PdfBox.Net.Tests;

public class TestPDPage
{
    [Fact]
    public void DefaultPageHasLetterMediaBox()
    {
        PDPage page = new();
        PDRectangle mediaBox = page.GetMediaBox();
        Assert.NotNull(mediaBox);
        // U.S. Letter: 8.5" x 11" at 72 dpi = 612 x 792
        Assert.Equal(612f, mediaBox.GetWidth(), 1f);
        Assert.Equal(792f, mediaBox.GetHeight(), 1f);
    }

    [Fact]
    public void PageWithCustomMediaBox()
    {
        PDRectangle a4 = PDRectangle.A4;
        PDPage page = new(a4);
        PDRectangle mediaBox = page.GetMediaBox();
        Assert.NotNull(mediaBox);
        Assert.Equal(a4.GetWidth(), mediaBox.GetWidth(), 1f);
        Assert.Equal(a4.GetHeight(), mediaBox.GetHeight(), 1f);
    }

    [Fact]
    public void SetAndGetMediaBox()
    {
        PDPage page = new();
        PDRectangle customMediaBox = new(0, 0, 500, 700);
        page.SetMediaBox(customMediaBox);
        PDRectangle result = page.GetMediaBox();
        Assert.Equal(500f, result.GetWidth(), 0.01f);
        Assert.Equal(700f, result.GetHeight(), 0.01f);
    }

    [Fact]
    public void CropBoxDefaultsToMediaBox()
    {
        PDPage page = new();
        PDRectangle cropBox = page.GetCropBox();
        PDRectangle mediaBox = page.GetMediaBox();
        Assert.Equal(mediaBox.GetWidth(), cropBox.GetWidth(), 0.01f);
        Assert.Equal(mediaBox.GetHeight(), cropBox.GetHeight(), 0.01f);
    }

    [Fact]
    public void SetAndGetCropBox()
    {
        PDPage page = new();
        PDRectangle cropBox = new(10, 10, 400, 600);
        page.SetCropBox(cropBox);
        PDRectangle result = page.GetCropBox();
        // Clipped to media box: lower-left should be max(0,10)=10, upper-right capped to media
        Assert.Equal(10f, result.GetLowerLeftX(), 0.01f);
        Assert.Equal(10f, result.GetLowerLeftY(), 0.01f);
    }

    [Fact]
    public void BleedBoxDefaultsToCropBox()
    {
        PDPage page = new();
        PDRectangle bleedBox = page.GetBleedBox();
        PDRectangle cropBox = page.GetCropBox();
        Assert.Equal(cropBox.GetWidth(), bleedBox.GetWidth(), 0.01f);
        Assert.Equal(cropBox.GetHeight(), bleedBox.GetHeight(), 0.01f);
    }

    [Fact]
    public void TrimBoxDefaultsToCropBox()
    {
        PDPage page = new();
        PDRectangle trimBox = page.GetTrimBox();
        PDRectangle cropBox = page.GetCropBox();
        Assert.Equal(cropBox.GetWidth(), trimBox.GetWidth(), 0.01f);
        Assert.Equal(cropBox.GetHeight(), trimBox.GetHeight(), 0.01f);
    }

    [Fact]
    public void ArtBoxDefaultsToCropBox()
    {
        PDPage page = new();
        PDRectangle artBox = page.GetArtBox();
        PDRectangle cropBox = page.GetCropBox();
        Assert.Equal(cropBox.GetWidth(), artBox.GetWidth(), 0.01f);
        Assert.Equal(cropBox.GetHeight(), artBox.GetHeight(), 0.01f);
    }

    [Fact]
    public void RotationDefaultsToZero()
    {
        PDPage page = new();
        Assert.Equal(0, page.GetRotation());
    }

    [Fact]
    public void SetAndGetRotation()
    {
        PDPage page = new();
        page.SetRotation(90);
        Assert.Equal(90, page.GetRotation());

        page.SetRotation(180);
        Assert.Equal(180, page.GetRotation());

        page.SetRotation(270);
        Assert.Equal(270, page.GetRotation());
    }

    [Fact]
    public void RotationNormalized()
    {
        PDPage page = new();
        page.SetRotation(450);
        Assert.Equal(90, page.GetRotation());

        page.SetRotation(-90);
        Assert.Equal(270, page.GetRotation());
    }

    [Fact]
    public void StructParentsRoundtrip()
    {
        PDPage page = new();
        page.SetStructParents(42);
        Assert.Equal(42, page.GetStructParents());
    }

    [Fact]
    public void HasContentsReturnsFalseForNewPage()
    {
        PDPage page = new();
        Assert.False(page.HasContents());
    }

    [Fact]
    public void NullMediaBoxRemovesIt()
    {
        PDPage page = new();
        page.SetMediaBox(null);
        // Should fall back to LETTER default when re-read
        Assert.NotNull(page.GetMediaBox());
    }

    [Fact]
    public void AddingPageAfterCreatingAnnotationDoesNotBreakSave()
    {
        using PDDocument document = new();
        PDPage page = new(PDRectangle.A4);

        PDAcroForm acroForm = new(document);
        document.GetDocumentCatalog().SetAcroForm(acroForm);

        PDTextField textField = new(acroForm);
        textField.SetPartialName("testField");
        PDAnnotationWidget widget = new();
        textField.SetWidgets([widget]);
        widget.SetRectangle(new PDRectangle(100, 700, 200, 20));
        widget.SetPage(page);
        page.GetAnnotations().Add(widget);
        acroForm.GetFields().Add(textField);

        document.AddPage(page);

        using MemoryStream output = new();
        document.Save(output);

        Assert.NotEmpty(output.ToArray());
    }

    [Fact]
    public void NullThreadBeads()
    {
        PDPage page = new();

        Assert.Empty(page.GetThreadBeads());

        page.SetThreadBeads([]);
        Assert.Empty(page.GetThreadBeads());

        page.SetThreadBeads(null);
        Assert.Empty(page.GetThreadBeads());
    }
}
