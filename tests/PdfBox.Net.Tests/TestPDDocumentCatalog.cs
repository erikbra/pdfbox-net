/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/test/java/org/apache/pdfbox/pdmodel/TestPDDocumentCatalog.java
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

using System.Text;
using PdfBox.Net.COS;
using PdfBox.Net.PDModel;
using PdfBox.Net.PDModel.Common;
using PdfBox.Net.PDModel.Graphics.Color;
using PdfBox.Net.PDModel.Graphics.OptionalContent;
using PdfBox.Net.PDModel.Interactive.PageNavigation;

namespace PdfBox.Net.Tests;

public class TestPDDocumentCatalog
{
    [Fact]
    public void PageLayoutRoundtrip()
    {
        using PDDocument document = new();
        PDDocumentCatalog catalog = document.GetDocumentCatalog();

        Assert.Equal(PageLayout.SinglePage, catalog.GetPageLayout());

        catalog.SetPageLayout(PageLayout.SinglePage);
        Assert.Equal(PageLayout.SinglePage, catalog.GetPageLayout());

        catalog.SetPageLayout(PageLayout.TwoColumnLeft);
        Assert.Equal(PageLayout.TwoColumnLeft, catalog.GetPageLayout());

        // Clearing
        catalog.SetPageLayout(null);
        Assert.Equal(PageLayout.SinglePage, catalog.GetPageLayout());
    }

    [Fact]
    public void PageModeRoundtrip()
    {
        using PDDocument document = new();
        PDDocumentCatalog catalog = document.GetDocumentCatalog();

        Assert.Equal(PageMode.UseNone, catalog.GetPageMode());

        catalog.SetPageMode(PageMode.FullScreen);
        Assert.Equal(PageMode.FullScreen, catalog.GetPageMode());

        catalog.SetPageMode(PageMode.UseOutlines);
        Assert.Equal(PageMode.UseOutlines, catalog.GetPageMode());

        catalog.SetPageMode(null);
        Assert.Equal(PageMode.UseNone, catalog.GetPageMode());
    }

    [Fact]
    public void LanguageRoundtrip()
    {
        using PDDocument document = new();
        PDDocumentCatalog catalog = document.GetDocumentCatalog();

        Assert.Null(catalog.GetLanguage());

        catalog.SetLanguage("en-US");
        Assert.Equal("en-US", catalog.GetLanguage());

        catalog.SetLanguage(null);
        Assert.Null(catalog.GetLanguage());
    }

    [Fact]
    public void InvalidPageLayoutFallsBackToSinglePage()
    {
        using PDDocument document = new();
        PDDocumentCatalog catalog = document.GetDocumentCatalog();
        ((COSDictionary)catalog.GetCOSObject()).SetName(COSName.PAGE_LAYOUT, "NotALayout");
        Assert.Equal(PageLayout.SinglePage, catalog.GetPageLayout());
    }

    [Fact]
    public void InvalidPageModeFallsBackToUseNone()
    {
        using PDDocument document = new();
        PDDocumentCatalog catalog = document.GetDocumentCatalog();
        ((COSDictionary)catalog.GetCOSObject()).SetName(COSName.PAGE_MODE, "NotAMode");
        Assert.Equal(PageMode.UseNone, catalog.GetPageMode());
    }

    [Fact]
    public void AllPageLayoutValuesRoundtrip()
    {
        foreach (PageLayout layout in Enum.GetValues<PageLayout>())
        {
            using PDDocument document = new();
            document.GetDocumentCatalog().SetPageLayout(layout);
            Assert.Equal(layout, document.GetDocumentCatalog().GetPageLayout());
        }
    }

    [Fact]
    public void AllPageModeValuesRoundtrip()
    {
        foreach (PageMode mode in Enum.GetValues<PageMode>())
        {
            using PDDocument document = new();
            document.GetDocumentCatalog().SetPageMode(mode);
            Assert.Equal(mode, document.GetDocumentCatalog().GetPageMode());
        }
    }

    [Fact]
    public void HandleBooleanInOpenAction()
    {
        using PDDocument document = new();

        ((COSDictionary)document.GetDocumentCatalog().GetCOSObject()).SetBoolean(COSName.OPEN_ACTION, false);

        Assert.Null(document.GetDocumentCatalog().GetOpenAction());
    }

    [Fact]
    public void ThreadsAllowNullAndEmpty()
    {
        using PDDocument document = new();
        PDDocumentCatalog catalog = document.GetDocumentCatalog();

        Assert.Empty(catalog.GetThreads());

        catalog.SetThreads([]);
        Assert.Empty(catalog.GetThreads());

        catalog.SetThreads(null);
        Assert.Empty(catalog.GetThreads());
    }

    [Fact]
    public void OutputIntentsRoundtrip()
    {
        using PDDocument document = new();
        PDDocumentCatalog catalog = document.GetDocumentCatalog();

        Assert.Empty(catalog.GetOutputIntents());

        using MemoryStream colorProfile = new(Encoding.ASCII.GetBytes("dummy-icc-profile"));
        PDOutputIntent outputIntent = new(document, colorProfile);
        outputIntent.SetInfo("sRGB IEC61966-2.1");
        outputIntent.SetOutputCondition("sRGB IEC61966-2.1");
        outputIntent.SetOutputConditionIdentifier("sRGB IEC61966-2.1");
        outputIntent.SetRegistryName("http://www.color.org");

        catalog.AddOutputIntent(outputIntent);

        IList<PDOutputIntent> outputIntents = catalog.GetOutputIntents();
        Assert.Single(outputIntents);
        Assert.Equal("sRGB IEC61966-2.1", outputIntents[0].GetInfo());

        catalog.SetOutputIntents(outputIntents);
        Assert.Single(catalog.GetOutputIntents());
    }

    [Fact]
    public void MetadataRoundtrip()
    {
        using PDDocument document = new();
        PDDocumentCatalog catalog = document.GetDocumentCatalog();
        Assert.Null(catalog.GetMetadata());

        PDMetadata metadata = new(document);
        byte[] payload = Encoding.UTF8.GetBytes("<x:xmpmeta>catalog</x:xmpmeta>");
        metadata.ImportXMPMetadata(payload);

        catalog.SetMetadata(metadata);

        using Stream exported = catalog.GetMetadata()!.ExportXMPMetadata();
        using MemoryStream copy = new();
        exported.CopyTo(copy);
        Assert.Equal(payload, copy.ToArray());
    }

    [Fact]
    public void GetDestsWrapsCatalogDestsDictionary()
    {
        using PDDocument document = new();
        PDDocumentCatalog catalog = document.GetDocumentCatalog();
        COSDictionary dests = new();
        ((COSDictionary)catalog.GetCOSObject()).SetItem(COSName.DESTS, dests);

        Assert.NotNull(catalog.GetDests());
        Assert.Same(dests, catalog.GetDests()!.GetCOSObject());
    }

    [Fact]
    public void SettingOptionalContentPropertiesBumpsDocumentVersion()
    {
        using PDDocument document = new();
        document.GetDocumentCatalog().SetOCProperties(new PDOptionalContentProperties());

        Assert.Equal(1.5f, document.GetVersion());
        Assert.Equal("1.5", document.GetDocumentCatalog().GetVersion());
    }
}
