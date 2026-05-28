/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fixup/processor/AcroFormDefaultsProcessor.java
 * PDFBOX_SOURCE_COMMIT: 8c3cc02c967e80a02dcbd787af4d6393161d7bc8
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: 8c3cc02c967e80a02dcbd787af4d6393161d7bc8
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
using PdfBox.Net.PDModel.Interactive.Form;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.PDModel.Fixup.Processor;

public sealed class AcroFormDefaultsProcessor : AbstractProcessor
{
    private static readonly COSName FontKey = COSName.GetPDFName("Font");
    private static readonly COSName HelvKey = COSName.GetPDFName("Helv");
    private static readonly COSName ZaDbKey = COSName.GetPDFName("ZaDb");
    private const string AdobeDefaultAppearanceString = "/Helv 0 Tf 0 g ";

    public AcroFormDefaultsProcessor(PDDocument document)
        : base(document)
    {
    }

    public override void Process()
    {
        PDAcroForm? acroForm = document.GetDocumentCatalog().GetAcroForm(null);
        if (acroForm != null)
        {
            VerifyOrCreateDefaults(acroForm);
        }
    }

    private static void VerifyOrCreateDefaults(PDAcroForm acroForm)
    {
        COSDictionary acroFormDictionary = (COSDictionary)acroForm.GetCOSObject();
        if (string.IsNullOrEmpty(acroForm.GetDefaultAppearance()))
        {
            acroForm.SetDefaultAppearance(AdobeDefaultAppearanceString);
            acroFormDictionary.SetNeedToBeUpdated(true);
        }

        PDResources? defaultResources = acroForm.GetDefaultResources();
        if (defaultResources is null)
        {
            defaultResources = new PDResources();
            acroForm.SetDefaultResources(defaultResources);
            acroFormDictionary.SetNeedToBeUpdated(true);
        }

        COSDictionary resourcesDictionary = defaultResources.GetCOSObject();
        COSDictionary fontDictionary = resourcesDictionary.GetCOSDictionary(FontKey) ?? new COSDictionary();
        if (!resourcesDictionary.ContainsKey(FontKey))
        {
            resourcesDictionary.SetItem(FontKey, fontDictionary);
        }

        EnsureFont(fontDictionary, HelvKey, "Helvetica", resourcesDictionary);
        EnsureFont(fontDictionary, ZaDbKey, "ZapfDingbats", resourcesDictionary);
    }

    private static void EnsureFont(COSDictionary fontDictionary, COSName resourceName, string baseFontName, COSDictionary resourcesDictionary)
    {
        if (fontDictionary.ContainsKey(resourceName))
        {
            return;
        }

        COSDictionary font = new();
        font.SetName(COSName.TYPE, "Font");
        font.SetName(COSName.SUBTYPE, "Type1");
        font.SetName(COSName.GetPDFName("BaseFont"), baseFontName);
        fontDictionary.SetItem(resourceName, font);
        resourcesDictionary.SetNeedToBeUpdated(true);
        fontDictionary.SetNeedToBeUpdated(true);
    }
}
