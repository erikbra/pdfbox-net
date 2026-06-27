/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted from Apache PDFBox Java source with AI assistance.
 *
 * PDFBOX_SOURCE_PATH: pdfbox/src/main/java/org/apache/pdfbox/pdmodel/interactive/annotation/PDAnnotationWidget.java
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
using PdfBox.Net.PDModel.Interactive.Action;
using PdfBox.Net.PDModel.Interactive.Form;

namespace PdfBox.Net.PDModel.Interactive.Annotation;

public sealed partial class PDAnnotationWidget : PDAnnotation
{
    public const string SUB_TYPE = "Widget";

    public PDAnnotationWidget()
    {
        GetCOSDictionary().SetName(COSName.SUBTYPE, SUB_TYPE);
    }

    public PDAnnotationWidget(COSDictionary dict)
        : base(dict)
    {
    }

    public string GetHighlightingMode()
    {
        return GetCOSDictionary().GetNameAsString(COSName.H, PDAnnotationLink.HIGHLIGHT_MODE_INVERT);
    }

    public void SetHighlightingMode(string? highlightingMode)
    {
        if (highlightingMode is null or
            PDAnnotationLink.HIGHLIGHT_MODE_NONE or
            PDAnnotationLink.HIGHLIGHT_MODE_INVERT or
            PDAnnotationLink.HIGHLIGHT_MODE_OUTLINE or
            PDAnnotationLink.HIGHLIGHT_MODE_PUSH or
            "T")
        {
            GetCOSDictionary().SetName(COSName.H, highlightingMode);
            return;
        }

        throw new ArgumentException("Valid values for highlighting mode are 'N', 'I', 'O', 'P' or 'T'.", nameof(highlightingMode));
    }

    public PDAction? GetAction()
    {
        COSDictionary? action = GetCOSDictionary().GetCOSDictionary(COSName.A);
        return action != null ? PDActionFactory.CreateAction(action) : null;
    }

    public void SetAction(PDAction? action)
    {
        GetCOSDictionary().SetItem(COSName.A, action);
    }

    public void SetParent(PDField? field)
    {
        GetCOSDictionary().SetItem(COSName.PARENT, field?.GetCOSObject());
    }

    public PDAnnotationAdditionalActions? GetActions()
    {
        COSDictionary? actions = GetCOSDictionary().GetCOSDictionary(COSName.AA);
        return actions != null ? new PDAnnotationAdditionalActions(actions) : null;
    }

    public void SetActions(PDAnnotationAdditionalActions? actions)
    {
        GetCOSDictionary().SetItem(COSName.AA, actions);
    }

    public PDAppearanceCharacteristicsDictionary? GetAppearanceCharacteristics()
    {
        return GetCOSDictionary().GetCOSDictionary(COSName.GetPDFName("MK")) is COSDictionary dictionary
            ? new PDAppearanceCharacteristicsDictionary(dictionary)
            : null;
    }

    public void SetAppearanceCharacteristics(PDAppearanceCharacteristicsDictionary? characteristics)
    {
        GetCOSDictionary().SetItem(COSName.GetPDFName("MK"), characteristics);
    }

    public PDBorderStyleDictionary? GetBorderStyle()
    {
        return GetCOSDictionary().GetCOSDictionary(COSName.BS) is COSDictionary dictionary
            ? new PDBorderStyleDictionary(dictionary)
            : null;
    }

    public void SetBorderStyle(PDBorderStyleDictionary? borderStyle)
    {
        GetCOSDictionary().SetItem(COSName.BS, borderStyle);
    }
}
