/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fixup/processor/AcroFormDefaultsProcessor.java
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

using PdfBox.Net.COS;
using PdfBox.Net.PDModel.Font;
using PdfBox.Net.PDModel.Interactive.Form;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.PDModel.Fixup.Processor;

public class AcroFormDefaultsProcessor : AbstractProcessor
{
    private static readonly COSName FontKey = COSName.GetPDFName("Font");
    private static readonly COSName HelvKey = COSName.GetPDFName("Helv");
    private static readonly COSName ZaDbKey = COSName.GetPDFName("ZaDb");
    private static readonly COSName BaseFontKey = COSName.GetPDFName("BaseFont");

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
        const string adobeDefaultAppearanceString = "/Helv 0 Tf 0 g ";

        if (acroForm.GetDefaultAppearance().Length == 0)
        {
            acroForm.SetDefaultAppearance(adobeDefaultAppearanceString);
            ((COSDictionary)acroForm.GetCOSObject()).SetNeedToBeUpdated(true);
        }

        PDResources? defaultResources = acroForm.GetDefaultResources();
        if (defaultResources == null)
        {
            defaultResources = new PDResources();
            acroForm.SetDefaultResources(defaultResources);
            ((COSDictionary)acroForm.GetCOSObject()).SetNeedToBeUpdated(true);
        }

        COSDictionary fontDict = defaultResources.GetCOSObject().GetCOSDictionary(FontKey) ?? new COSDictionary();
        if (!defaultResources.GetCOSObject().ContainsKey(FontKey))
        {
            defaultResources.GetCOSObject().SetItem(FontKey, fontDict);
        }

        if (!fontDict.ContainsKey(HelvKey))
        {
            defaultResources.Put(HelvKey, CreateType1Standard14Font("Helvetica"));
            defaultResources.GetCOSObject().SetNeedToBeUpdated(true);
            fontDict.SetNeedToBeUpdated(true);
        }

        if (!fontDict.ContainsKey(ZaDbKey))
        {
            defaultResources.Put(ZaDbKey, CreateType1Standard14Font("ZapfDingbats"));
            defaultResources.GetCOSObject().SetNeedToBeUpdated(true);
            fontDict.SetNeedToBeUpdated(true);
        }
    }

    private static PDFont CreateType1Standard14Font(string baseFontName)
    {
        COSDictionary dictionary = new();
        dictionary.SetName(COSName.TYPE, "Font");
        dictionary.SetName(COSName.SUBTYPE, "Type1");
        dictionary.SetName(BaseFontKey, baseFontName);
        return PDFontFactory.CreateFont(dictionary);
    }
}
