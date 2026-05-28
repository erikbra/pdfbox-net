/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fixup/processor/AcroFormOrphanWidgetsProcessor.java
 * PDFBOX_SOURCE_COMMIT: daabc241bd8d6b729caaee0c2070043b3fe5f8dc
 * PORT_MODE: adapted
 * PORT_LAST_SYNC_COMMIT: daabc241bd8d6b729caaee0c2070043b3fe5f8dc
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
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.Form;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.PDModel.Fixup.Processor;

public sealed class AcroFormOrphanWidgetsProcessor : AbstractProcessor
{
    private static readonly COSName FontKey = COSName.GetPDFName("Font");

    public AcroFormOrphanWidgetsProcessor(PDDocument document)
        : base(document)
    {
    }

    public override void Process()
    {
        PDAcroForm? acroForm = document.GetDocumentCatalog().GetAcroForm(null);
        if (acroForm != null)
        {
            ResolveFieldsFromWidgets(acroForm);
        }
    }

    private void ResolveFieldsFromWidgets(PDAcroForm acroForm)
    {
        PDResources? resources = acroForm.GetDefaultResources();
        if (resources is null)
        {
            return;
        }

        List<PDField> fields = [];
        HashSet<COSDictionary> seenRoots = [];
        foreach (PDPage page in document.GetPages())
        {
            HandleAnnotations(acroForm, resources, fields, page.GetAnnotations(), seenRoots);
        }

        acroForm.SetFields(fields);
    }

    private static void HandleAnnotations(PDAcroForm acroForm, PDResources acroFormResources, List<PDField> fields, IList<PDAnnotation> annotations, ISet<COSDictionary> seenRoots)
    {
        foreach (PDAnnotation annotation in annotations)
        {
            if (annotation is not PDAnnotationWidget widget)
            {
                continue;
            }

            AddFontsFromWidget(acroFormResources, widget);

            COSDictionary? fieldDictionary = ResolveRootFieldDictionary(widget.GetCOSDictionary());
            if (fieldDictionary is null || !seenRoots.Add(fieldDictionary))
            {
                continue;
            }

            fields.Add(PDField.FromDictionary(acroForm, fieldDictionary));
        }
    }

    private static COSDictionary? ResolveRootFieldDictionary(COSDictionary widgetDictionary)
    {
        COSDictionary current = widgetDictionary;
        while (current.GetCOSDictionary(COSName.PARENT) is COSDictionary parent)
        {
            current = parent;
        }

        return current;
    }

    private static void AddFontsFromWidget(PDResources acroFormResources, PDAnnotationWidget widget)
    {
        PDResources? widgetResources = widget.GetNormalAppearanceStream()?.GetResources();
        COSDictionary? widgetFontDictionary = widgetResources?.GetCOSObject().GetCOSDictionary(FontKey);
        if (widgetFontDictionary is null)
        {
            return;
        }

        COSDictionary acroFormResourcesDictionary = acroFormResources.GetCOSObject();
        COSDictionary acroFormFontDictionary = acroFormResourcesDictionary.GetCOSDictionary(FontKey) ?? new COSDictionary();
        if (!acroFormResourcesDictionary.ContainsKey(FontKey))
        {
            acroFormResourcesDictionary.SetItem(FontKey, acroFormFontDictionary);
        }

        foreach (COSName fontName in widgetFontDictionary.KeySet())
        {
            if (fontName.GetName().StartsWith("+", StringComparison.Ordinal) || acroFormFontDictionary.ContainsKey(fontName))
            {
                continue;
            }

            if (widgetFontDictionary.GetDictionaryObject(fontName) is COSBase font)
            {
                acroFormFontDictionary.SetItem(fontName, font);
                acroFormResourcesDictionary.SetNeedToBeUpdated(true);
                acroFormFontDictionary.SetNeedToBeUpdated(true);
            }
        }
    }
}
