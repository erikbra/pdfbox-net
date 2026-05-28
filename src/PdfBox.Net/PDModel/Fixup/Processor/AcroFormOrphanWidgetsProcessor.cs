/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/fixup/processor/AcroFormOrphanWidgetsProcessor.java
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
using PdfBox.Net.PDModel.Interactive.Annotation;
using PdfBox.Net.PDModel.Interactive.Form;
using PdfBox.Net.PDModel.Resources;

namespace PdfBox.Net.PDModel.Fixup.Processor;

public class AcroFormOrphanWidgetsProcessor : AbstractProcessor
{
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
        if (resources == null)
        {
            return;
        }

        List<PDField> fields = [];
        Dictionary<string, PDField> nonTerminalFieldsMap = new(StringComparer.Ordinal);

        foreach (PDPage page in document.GetPages())
        {
            HandleAnnotations(acroForm, resources, fields, page.GetAnnotations(), nonTerminalFieldsMap);
        }

        acroForm.SetFields(fields);

        foreach (PDField field in acroForm.GetFieldTree())
        {
            if (field is PDVariableText variableText)
            {
                EnsureFontResources(resources, variableText);
            }
        }
    }

    private static void HandleAnnotations(PDAcroForm acroForm, PDResources acroFormResources,
        List<PDField> fields, IList<PDAnnotation> annotations, Dictionary<string, PDField> nonTerminalFieldsMap)
    {
        foreach (PDAnnotation annotation in annotations)
        {
            if (annotation is not PDAnnotationWidget)
            {
                continue;
            }

            AddFontFromWidget(acroFormResources, annotation);

            COSDictionary? parent = annotation.GetCOSDictionary().GetCOSDictionary(COSName.PARENT);
            if (parent != null)
            {
                PDField? resolvedField = ResolveNonRootField(acroForm, parent, nonTerminalFieldsMap);
                if (resolvedField != null)
                {
                    fields.Add(resolvedField);
                }
            }
            else
            {
                fields.Add(PDFieldFactory.CreateField(acroForm, annotation.GetCOSDictionary(), null));
            }
        }
    }

    private static void AddFontFromWidget(PDResources acroFormResources, PDAnnotation annotation)
    {
        PDAppearanceStream? normalAppearanceStream = annotation.GetNormalAppearanceStream();
        if (normalAppearanceStream == null)
        {
            return;
        }

        PDResources? widgetResources = normalAppearanceStream.GetResources();
        if (widgetResources == null)
        {
            return;
        }

        foreach (COSName fontName in widgetResources.GetFontNames())
        {
            if (fontName.GetName().StartsWith("+", StringComparison.Ordinal))
            {
                continue;
            }

            if (acroFormResources.GetFont(fontName) == null)
            {
                PDFont? widgetFont = widgetResources.GetFont(fontName);
                if (widgetFont != null)
                {
                    acroFormResources.Put(fontName, widgetFont);
                }
            }
        }
    }

    private static PDField? ResolveNonRootField(PDAcroForm acroForm, COSDictionary parent,
        Dictionary<string, PDField> nonTerminalFieldsMap)
    {
        COSDictionary? rootParent = parent;
        while (rootParent != null && rootParent.ContainsKey(COSName.PARENT))
        {
            rootParent = rootParent.GetCOSDictionary(COSName.PARENT);
        }

        if (rootParent == null)
        {
            return null;
        }

        string key = rootParent.GetString(COSName.T, string.Empty) ?? string.Empty;
        if (!nonTerminalFieldsMap.ContainsKey(key))
        {
            PDField field = PDFieldFactory.CreateField(acroForm, rootParent, null);
            nonTerminalFieldsMap[field.GetFullyQualifiedName() ?? key] = field;
            return field;
        }

        return null;
    }

    private static void EnsureFontResources(PDResources defaultResources, PDVariableText field)
    {
        string? daString = field.GetDefaultAppearance();
        if (string.IsNullOrWhiteSpace(daString) || !daString.StartsWith("/", StringComparison.Ordinal))
        {
            return;
        }

        int separatorIndex = daString.IndexOf(' ');
        if (separatorIndex <= 1)
        {
            return;
        }

        COSName fontName = COSName.GetPDFName(daString.Substring(1, separatorIndex - 1));
        if (defaultResources.GetFont(fontName) != null)
        {
            return;
        }

        PDFont? widgetFont = TryGetWidgetFontResource(field, fontName);
        if (widgetFont != null)
        {
            defaultResources.Put(fontName, widgetFont);
        }
    }

    private static PDFont? TryGetWidgetFontResource(PDVariableText field, COSName fontName)
    {
        foreach (PDAnnotationWidget widget in field.GetWidgets())
        {
            PDResources? widgetResources = widget.GetNormalAppearanceStream()?.GetResources();
            PDFont? widgetFont = widgetResources?.GetFont(fontName);
            if (widgetFont != null)
            {
                return widgetFont;
            }
        }

        return null;
    }
}
